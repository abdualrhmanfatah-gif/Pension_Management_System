using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Investment;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.IntegrationTests;

public class InvestmentPropertyAuthorizationSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task HttpAndSql_EnforceCurrentAndDestinationOwners_AndRejectedRequestsDoNotChangeRowsOrStorePhotos()
    {
        int actorId, branchA, branchB, propertyA, propertyB, destinationA, projectA, projectB, propertyTypeId,
            unitTypeId, unitA, unitB, valuationB, depreciationB, fiscalId;
        var suffix = Guid.NewGuid().ToString("N");
        await using (var db = database.OpenContext())
        {
            var a = new PensionFundBranch { BranchName = "عقارات أ " + suffix };
            var b = new PensionFundBranch { BranchName = "عقارات ب " + suffix };
            var role = new Role { RoleName = "عقارات " + suffix };
            foreach (var code in new[] { "Investment.View", "Investment.Add", "Investment.Edit", "Investment.Delete", "Investment.Manage" })
                role.RolePermissions.Add(new RolePermission { Permission = await db.Permissions.SingleAsync(p => p.Code == code) });
            var actor = new User { Username = "property-" + suffix, FullName = "مستخدم اختبار", PasswordHash = "synthetic-only",
                Role = role, Branch = a };
            var portfolio = new InvestmentPortfolio { Name = "محفظة اختبار " + suffix, Manager = actor };
            var investmentType = new InvestmentType { TypeName = "نوع اختبار " + suffix };
            var pa = new InvestmentProject { Name = "مشروع أ " + suffix, Portfolio = portfolio, InvestmentType = investmentType,
                PensionBranch = a, StartDate = DateTime.UtcNow };
            var pb = new InvestmentProject { Name = "مشروع سري ب " + suffix, Portfolio = portfolio, InvestmentType = investmentType,
                PensionBranch = b, StartDate = DateTime.UtcNow };
            var propertyType = new PropertyType { TypeName = "نوع عقار " + suffix };
            var unitType = new UnitType { TypeName = "نوع وحدة " + suffix };
            InvestmentProperty Property(string name, PensionFundBranch branch, InvestmentProject? project) => new()
            { PropertyName = name, PensionBranch = branch, Project = project, PropertyType = propertyType,
                TotalArea = 10, InitialValue = 100, AcquisitionDate = DateTime.UtcNow };
            var own = Property("عقار أ", a, pa);
            var foreign = Property("عقار ب", b, pb);
            var destination = Property("وجهة أ", a, null);
            var legacy = Property("رابط قديم", a, pb);
            var ownUnit = new PropertyUnit { Property = own, UnitType = unitType, UnitNumber = "أ", Area = 5 };
            var foreignUnit = new PropertyUnit { Property = foreign, UnitType = unitType, UnitNumber = "ب", Area = 5 };
            var foreignValuation = new AssetValuation { Property = foreign, ValuationDate = DateTime.UtcNow, MarketValue = 900 };
            var fiscal = new FiscalYear { YearName = "اختبار " + suffix,
                StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddMonths(11) };
            var foreignDepreciation = new DepreciationRecord { Property = foreign, FiscalYear = fiscal, NetBookValue = 800 };
            db.AddRange(own, foreign, destination, legacy, ownUnit, foreignUnit, foreignValuation, foreignDepreciation,
                new AssetValuation { Property = own, ValuationDate = DateTime.UtcNow, MarketValue = 200 },
                new UserRoleAssignment { User = actor, Role = role, Branch = a });
            await db.SaveChangesAsync();
            (actorId, branchA, branchB, propertyA, propertyB, destinationA, projectA, projectB, propertyTypeId,
                unitTypeId, unitA, unitB, valuationB, depreciationB, fiscalId) =
                (actor.UserID, a.PensionBranchID, b.PensionBranchID, own.PropertyID, foreign.PropertyID, destination.PropertyID,
                    pa.ProjectID, pb.ProjectID, propertyType.TypeID, unitType.TypeID, ownUnit.UnitID, foreignUnit.UnitID,
                    foreignValuation.ValuationID, foreignDepreciation.DeprecID, fiscal.FiscalYearID);
        }
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        await using var app = await InvestmentPropertyAuthorizationHttpTests.StartAsync(services =>
        {
            services.AddScoped(_ => database.OpenContext());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
            services.AddScoped<IInvestmentPropertyService, InvestmentPropertyService>();
            services.AddSingleton(storage.Object);
        });
        using var client = InvestmentProjectHttpHost.Client(app, actorId);
        const string root = "/api/investment/properties";
        var before = await SnapshotAsync();
        using (var response = await client.GetAsync($"{root}/{propertyB}")) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var properties = (await client.GetFromJsonAsync<ApiResponse<List<PropertyDto>>>(root))!.Data!;
        Assert.DoesNotContain(properties, p => p.PropertyID == propertyB);
        Assert.Contains(properties, p => p.PropertyName == "رابط قديم" && p.ProjectName == null);
        Assert.Equal(200, (await client.GetFromJsonAsync<ApiResponse<PropertyDetailDto>>($"{root}/{propertyA}"))!.Data!.CurrentMarketValue);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<PropertyDto>>>($"{root}/project/{projectB}"))!.Data!);
        Assert.DoesNotContain((await client.GetFromJsonAsync<ApiResponse<List<PropertyUnitDto>>>($"{root}/units/all"))!.Data!, u => u.UnitID == unitB);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<PropertyUnitDto>>>($"{root}/{propertyB}/units"))!.Data!);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<ValuationDto>>>($"{root}/{propertyB}/valuations"))!.Data!);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<DepreciationRecordDto>>>($"{root}/{propertyB}/depreciation"))!.Data!);

        using (var response = await client.PostAsJsonAsync(root, Create(branchB, null))) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using (var response = await client.PostAsJsonAsync(root, Create(branchA, projectB))) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/{propertyB}", Update(branchA, projectA))) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/{propertyA}", Update(branchB, projectA))) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/{propertyA}", Update(branchA, projectB))) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/{propertyB}")) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync($"{root}/units", Unit(propertyB))) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/units/{unitA}", Unit(propertyB))) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/units/{unitB}", Unit(propertyA))) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/units/{unitB}")) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync($"{root}/valuations", new CreateValuationDto { PropertyID = propertyB, MarketValue = 500 }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/valuations/{valuationB}")) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync($"{root}/depreciation", new CreateDepreciationDto { PropertyID = propertyB, FiscalYearID = fiscalId }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/depreciation/{depreciationB}")) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var photo = InvestmentPropertyAuthorizationHttpTests.Photo())
        using (var response = await client.PostAsync($"{root}/{propertyB}/upload-photo", photo)) Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        storage.VerifyNoOtherCalls();
        Assert.Equal(before, await SnapshotAsync());

        int created;
        using (var response = await client.PostAsJsonAsync(root, Create(branchA, projectA)))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            created = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        using (var response = await client.PutAsJsonAsync($"{root}/{created}", Update(branchA, null))) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var response = await client.PostAsJsonAsync($"{root}/units", Unit(created))) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/units/{unitA}", Unit(destinationA))) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/units/{unitA}")) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        int valuation;
        using (var response = await client.PostAsJsonAsync($"{root}/valuations", new CreateValuationDto { PropertyID = created, MarketValue = 500, ValuationDate = DateTime.UtcNow }))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            valuation = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        using (var response = await client.DeleteAsync($"{root}/valuations/{valuation}")) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        int depreciation;
        using (var response = await client.PostAsJsonAsync($"{root}/depreciation", new CreateDepreciationDto { PropertyID = created, FiscalYearID = fiscalId, NetBookValue = 90 }))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            depreciation = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        using (var response = await client.DeleteAsync($"{root}/depreciation/{depreciation}")) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/{created}")) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using (var db = database.OpenContext())
        {
            Assert.True((await db.InvestmentProperties.IgnoreQueryFilters().SingleAsync(p => p.PropertyID == created)).IsDeleted);
            Assert.Equal(destinationA, (await db.PropertyUnits.IgnoreQueryFilters().SingleAsync(u => u.UnitID == unitA)).PropertyID);
            Assert.True((await db.AssetValuations.IgnoreQueryFilters().SingleAsync(v => v.ValuationID == valuation)).IsDeleted);
            Assert.True((await db.DepreciationRecords.IgnoreQueryFilters().SingleAsync(d => d.DeprecID == depreciation)).IsDeleted);
        }

        CreatePropertyDto Create(int branch, int? project) => new() { PropertyName = "عقار جديد " + suffix,
            PensionBranchID = branch, ProjectID = project, PropertyTypeID = propertyTypeId, TotalArea = 10,
            InitialValue = 100, AcquisitionDate = DateTime.UtcNow };
        UpdatePropertyDto Update(int branch, int? project) => new() { PropertyName = "عقار معدل " + suffix,
            PensionBranchID = branch, ProjectID = project, PropertyTypeID = propertyTypeId, TotalArea = 10,
            InitialValue = 100, AcquisitionDate = DateTime.UtcNow };
        UpdateUnitDto Unit(int property) => new() { PropertyID = property, UnitTypeID = unitTypeId, UnitNumber = "وحدة اختبار", Area = 5 };

        async Task<string> SnapshotAsync()
        {
            await using var db = database.OpenContext();
            return JsonSerializer.Serialize(new
            {
                Properties = await db.InvestmentProperties.IgnoreQueryFilters().OrderBy(p => p.PropertyID)
                    .Select(p => new { p.PropertyID, p.PensionBranchID, p.ProjectID, p.PropertyName, p.MainPhotoPath, p.IsDeleted, p.UpdatedAt }).ToListAsync(),
                Units = await db.PropertyUnits.IgnoreQueryFilters().OrderBy(u => u.UnitID)
                    .Select(u => new { u.UnitID, u.PropertyID, u.UnitNumber, u.IsDeleted, u.UpdatedAt }).ToListAsync(),
                Valuations = await db.AssetValuations.IgnoreQueryFilters().OrderBy(v => v.ValuationID)
                    .Select(v => new { v.ValuationID, v.PropertyID, v.MarketValue, v.IsDeleted, v.UpdatedAt }).ToListAsync(),
                Depreciation = await db.DepreciationRecords.IgnoreQueryFilters().OrderBy(d => d.DeprecID)
                    .Select(d => new { d.DeprecID, d.PropertyID, d.NetBookValue, d.IsDeleted, d.UpdatedAt }).ToListAsync(),
                AuditCount = await db.AuditLogs.CountAsync()
            });
        }
    }
}
