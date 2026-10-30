using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Investment;
using PensionMS.Infrastructure.Services.Security;

namespace PensionMS.IntegrationTests;

public class InvestmentLeasingAuthorizationSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task HttpAndSql_ScopeEachOwnershipChain_RejectWritesAndUploads_AndPreserveSharedParties()
    {
        int actorId, roleId, branchB, propertyA, propertyB, tenantA, tenantB, sharedTenant,
            unlinkedTenant, contractA, contractB, sharedA, sharedB, scheduleB, unitA, unitB;
        var suffix = Guid.NewGuid().ToString("N");
        await using (var db = database.OpenContext())
        {
            var a = new PensionFundBranch { BranchName = "تأجير أ " + suffix };
            var b = new PensionFundBranch { BranchName = "تأجير ب " + suffix };
            var role = new Role { RoleName = "تأجير " + suffix };
            foreach (var code in new[] { "Investment.View", "Investment.Manage" })
                role.RolePermissions.Add(new RolePermission { Permission = await db.Permissions.SingleAsync(p => p.Code == code) });
            var actor = new User { Username = "lease-scope-" + suffix, FullName = "مستخدم اختبار",
                PasswordHash = "synthetic-test-only", Role = role, Branch = a };
            var propertyType = new PropertyType { TypeName = "نوع عقار " + suffix };
            var unitType = new UnitType { TypeName = "نوع وحدة " + suffix };
            var pa = new InvestmentProperty { PropertyName = "عقار أ", PensionBranch = a,
                PropertyType = propertyType, TotalArea = 100, InitialValue = 1000 };
            var pb = new InvestmentProperty { PropertyName = "عقار ب", PensionBranch = b,
                PropertyType = propertyType, TotalArea = 100, InitialValue = 1000 };
            PropertyUnit Unit(InvestmentProperty property, string number) => new()
                { Property = property, UnitType = unitType, UnitNumber = number, Area = 10 };
            Tenant Tenant(string name) => new() { FullName = name, NationalIDOrCR = name + suffix };
            var ta = Tenant("أ"); var tb = Tenant("ب"); var shared = Tenant("مشترك"); var unlinked = Tenant("غير مرتبط");
            LeaseContract Contract(PropertyUnit unit, Tenant tenant) => new()
            {
                Unit = unit, Tenant = tenant, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2027, 1, 1),
                TotalContractValue = 1000, Status = LeaseStatusConstants.Active, PaymentFrequency = PaymentFrequencyConstants.Monthly
            };
            var ca = Contract(Unit(pa, "a1"), ta); var cb = Contract(Unit(pb, "b1"), tb);
            var sa = Contract(Unit(pa, "a2"), shared); var sb = Contract(Unit(pb, "b2"), shared);
            var freeA = Unit(pa, "a3"); var freeB = Unit(pb, "b3");
            var paymentB = new PaymentSchedule { LeaseContract = cb, DueDate = new DateTime(2026, 2, 1), ExpectedAmount = 100 };
            db.AddRange(ca, cb, sa, sb, freeA, freeB, unlinked, paymentB,
                new PaymentSchedule { LeaseContract = ca, DueDate = new DateTime(2026, 2, 1), ExpectedAmount = 100 },
                new PaymentSchedule { LeaseContract = sa, DueDate = new DateTime(2026, 2, 1), ExpectedAmount = 100 },
                new PaymentSchedule { LeaseContract = sb, DueDate = new DateTime(2026, 2, 1), ExpectedAmount = 100 },
                new UserRoleAssignment { User = actor, Role = role, Branch = a });
            await db.SaveChangesAsync();
            (actorId, roleId, branchB, propertyA, propertyB, tenantA, tenantB, sharedTenant, unlinkedTenant,
                contractA, contractB, sharedA, sharedB, scheduleB, unitA, unitB) =
                (actor.UserID, role.RoleID, b.PensionBranchID, pa.PropertyID, pb.PropertyID, ta.TenantID, tb.TenantID,
                shared.TenantID, unlinked.TenantID, ca.ContractID, cb.ContractID, sa.ContractID, sb.ContractID,
                paymentB.ScheduleID, freeA.UnitID, freeB.UnitID);
        }
        var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
        await using var app = await InvestmentProjectHttpHost.StartAsync(services =>
        {
            services.AddScoped(_ => database.OpenContext());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
            services.AddScoped<IInvestmentLeasingService, InvestmentLeasingService>();
            services.AddSingleton(storage.Object);
        });
        using var client = InvestmentProjectHttpHost.Client(app, actorId);
        const string root = "/api/investment/leasing";
        var before = await SnapshotAsync();
        foreach (var path in new[] { $"tenants/{tenantB}", $"tenants/{sharedTenant}", $"contracts/{contractB}", $"contracts/{sharedB}" })
        {
            using var response = await client.GetAsync($"{root}/{path}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        var tenants = (await client.GetFromJsonAsync<ApiResponse<List<TenantDto>>>($"{root}/tenants"))!.Data!;
        Assert.Equal(new[] { tenantA, unlinkedTenant }.Order(), tenants.Select(t => t.TenantID).Order());
        Assert.Equal(1, tenants.Single(t => t.TenantID == tenantA).ActiveContractsCount);
        Assert.True((await client.GetFromJsonAsync<ApiResponse<TenantDto>>($"{root}/tenants/{tenantA}"))!.IsSuccess);
        var leases = (await client.GetFromJsonAsync<ApiResponse<List<LeaseContractDto>>>($"{root}/contracts"))!.Data!;
        Assert.Equal(new[] { contractA, sharedA }.Order(), leases.Select(c => c.ContractID).Order());
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<LeaseContractDto>>>($"{root}/properties/{propertyB}/contracts"))!.Data!);
        Assert.Equal(2, (await client.GetFromJsonAsync<ApiResponse<List<LeaseContractDto>>>($"{root}/properties/{propertyA}/contracts"))!.Data!.Count);
        Assert.Equal(propertyA, Assert.Single((await client.GetFromJsonAsync<ApiResponse<List<PropertyDto>>>
            ($"{root}/tenants/{sharedTenant}/properties"))!.Data!).PropertyID);
        Assert.Empty((await client.GetFromJsonAsync<ApiResponse<List<PaymentScheduleDto>>>($"{root}/contracts/{contractB}/schedules"))!.Data!);
        var schedules = (await client.GetFromJsonAsync<ApiResponse<List<PaymentScheduleDto>>>($"{root}/schedules"))!.Data!;
        Assert.Equal(new[] { contractA, sharedA }.Order(), schedules.Select(s => s.ContractID).Order());

        using (var response = await client.PostAsJsonAsync($"{root}/contracts", Lease(unitB, tenantA)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync($"{root}/contracts", Lease(unitA, tenantB)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/contracts/{contractB}", Lease(unitA, tenantA)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/contracts/{contractA}", Lease(unitB, tenantA)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/contracts/{contractA}", Lease(unitA, tenantB)))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"{root}/contracts/{contractB}"))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        foreach (var tenant in new[] { tenantB, sharedTenant })
        {
            using var update = await client.PutAsJsonAsync($"{root}/tenants/{tenant}", new UpdateTenantDto
                { FullName = "تعديل مرفوض", NationalIDOrCR = "rejected-" + suffix });
            Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
            using var delete = await client.DeleteAsync($"{root}/tenants/{tenant}");
            Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        }
        using (var response = await client.PostAsJsonAsync($"{root}/schedules", new CreatePaymentScheduleDto
            { ContractID = contractB, DueDate = new DateTime(2026, 5, 1), ExpectedAmount = 100 }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PutAsJsonAsync($"{root}/schedules/{scheduleB}/status", new UpdatePaymentStatusDto
            { PaidAmount = 10, Status = "Paid" }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var content = InvestmentLeasingAuthorizationHttpTests.ContractFile())
        using (var response = await client.PostAsync($"{root}/contracts/{contractB}/upload-signed-contract", content))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        storage.VerifyNoOtherCalls();
        Assert.Equal(before, await SnapshotAsync());

        // First linkage has no invented tenant branch; schedules retain the existing transaction/rounding workflow.
        int createdId;
        using (var response = await client.PostAsJsonAsync($"{root}/contracts", Lease(unitA, unlinkedTenant, true)))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            createdId = (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
        }
        await using (var db = database.OpenContext())
        {
            var created = await db.Set<PaymentSchedule>().Where(s => s.ContractID == createdId).OrderBy(s => s.DueDate).ToListAsync();
            Assert.Equal(12, created.Count);
            Assert.Equal(1000, created.Sum(s => s.ExpectedAmount));
            Assert.Equal(83.37m, created[^1].ExpectedAmount);
        }
        // Shared parties remain usable within an authorized lease without exposing their other contracts/profile.
        using (var response = await client.PutAsJsonAsync($"{root}/contracts/{createdId}", Lease(unitA, sharedTenant)))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Additional branch permission (without a central role) permits the shared profile and destination unit.
        await using (var db = database.OpenContext())
        {
            db.UserRoleAssignments.Add(new UserRoleAssignment { UserId = actorId, RoleId = roleId, BranchId = branchB });
            await db.SaveChangesAsync();
        }
        Assert.True((await client.GetFromJsonAsync<ApiResponse<TenantDto>>($"{root}/tenants/{sharedTenant}"))!.IsSuccess);
        using (var response = await client.PutAsJsonAsync($"{root}/contracts/{createdId}", Lease(unitB, tenantB)))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using (var db = database.OpenContext())
            Assert.Equal(unitB, (await db.Set<LeaseContract>().SingleAsync(c => c.ContractID == createdId)).UnitID);
        using (var response = await client.DeleteAsync($"{root}/contracts/{createdId}"))
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using (var db = database.OpenContext())
        {
            var deleted = await db.Set<LeaseContract>().IgnoreQueryFilters().SingleAsync(c => c.ContractID == createdId);
            Assert.True(deleted.IsDeleted);
            Assert.Equal(actorId, deleted.DeletedByUserID);
        }

        CreateLeaseDto Lease(int unit, int tenant, bool generate = false) => new()
        {
            UnitID = unit, TenantID = tenant, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2027, 1, 1),
            TotalContractValue = 1000, PaymentFrequency = PaymentFrequencyConstants.Monthly, GenerateSchedules = generate
        };

        async Task<string> SnapshotAsync()
        {
            await using var db = database.OpenContext();
            return JsonSerializer.Serialize(new
            {
                Tenants = await db.Set<Tenant>().IgnoreQueryFilters().OrderBy(t => t.TenantID)
                    .Select(t => new { t.TenantID, t.FullName, t.TenantType, t.NationalIDOrCR, t.Phone, t.RatingScore,
                        t.AccountID, t.CreatedAt, t.UpdatedAt, t.IsDeleted, t.DeletedAt, t.DeletedByUserID, t.RowVersion }).ToListAsync(),
                Contracts = await db.Set<LeaseContract>().IgnoreQueryFilters().OrderBy(c => c.ContractID)
                    .Select(c => new { c.ContractID, c.UnitID, c.TenantID, c.StartDate, c.EndDate, c.TotalContractValue,
                        c.PaymentFrequency, c.RenewalTerms, c.SignedContractPath, c.Status, c.CreatedAt, c.UpdatedAt,
                        c.IsDeleted, c.DeletedAt, c.DeletedByUserID, c.RowVersion }).ToListAsync(),
                Schedules = await db.Set<PaymentSchedule>().IgnoreQueryFilters().OrderBy(s => s.ScheduleID)
                    .Select(s => new { s.ScheduleID, s.ContractID, s.DueDate, s.ExpectedAmount, s.PaidAmount, s.Status,
                        s.CreatedAt, s.UpdatedAt, s.IsDeleted, s.DeletedAt, s.DeletedByUserID, s.RowVersion }).ToListAsync(),
                AuditCount = await db.AuditLogs.CountAsync()
            });
        }
    }
}
