using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Investment;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class InvestmentPropertyAuthorizationTests
{
    [Fact]
    public async Task Reads_FilterBeforePagingAndHideForeignProjectMetadata()
    {
        var f = new Fixture();
        var own = (await f.Service.GetPropertyByIdAsync(1)).Data!;
        Assert.Equal("مشروع أ", own.ProjectName);
        Assert.Equal(400, own.CurrentMarketValue);
        Assert.Equal(1, Assert.Single(own.Units).UnitID);
        Assert.Equal(2, own.Valuations.Count);
        Assert.False((await f.Service.GetPropertyByIdAsync(2)).IsSuccess);
        Assert.Equal(1, Assert.Single((await f.Service.GetAllPropertiesAsync(1, 1)).Data!).PropertyID);
        var legacy = Assert.Single((await f.Service.GetAllPropertiesAsync(2, 1)).Data!);
        Assert.Equal(3, legacy.PropertyID);
        Assert.Null(legacy.ProjectName);
        Assert.Null((await f.Service.GetPropertyByIdAsync(3)).Data!.ProjectName);
        Assert.Equal(1, Assert.Single((await f.Service.GetPropertiesByProjectAsync(1)).Data!).PropertyID);
        Assert.Empty((await f.Service.GetPropertiesByProjectAsync(2)).Data!);
        Assert.Equal(1, Assert.Single((await f.Service.GetAllUnitsAsync()).Data!).UnitID);
        Assert.Empty((await f.Service.GetUnitsByPropertyAsync(2)).Data!);
        Assert.Equal(2, (await f.Service.GetValuationsByPropertyAsync(1)).Data!.Count);
        Assert.Empty((await f.Service.GetValuationsByPropertyAsync(2)).Data!);
        Assert.Equal(1, Assert.Single((await f.Service.GetDepreciationByPropertyAsync(1)).Data!).DeprecID);
        Assert.Empty((await f.Service.GetDepreciationByPropertyAsync(2)).Data!);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData("update-property")]
    [InlineData("delete-property")]
    [InlineData("photo")]
    [InlineData("create-unit")]
    [InlineData("update-unit")]
    [InlineData("delete-unit")]
    [InlineData("create-valuation")]
    [InlineData("delete-valuation")]
    [InlineData("create-depreciation")]
    [InlineData("delete-depreciation")]
    public async Task ForeignOwners_AreHiddenBeforeAnyMutation(string operation)
    {
        var f = new Fixture();
        var success = operation switch
        {
            "update-property" => (await f.Service.UpdatePropertyAsync(2, f.Update(), 7)).IsSuccess,
            "delete-property" => (await f.Service.DeletePropertyAsync(2, 7)).IsSuccess,
            "photo" => (await f.Service.UpdatePropertyMainPhotoPathAsync(2, "new.png", 7)).IsSuccess,
            "create-unit" => (await f.Service.CreateUnitAsync(new CreateUnitDto { PropertyID = 2 }, 7)).IsSuccess,
            "update-unit" => (await f.Service.UpdateUnitAsync(2, new UpdateUnitDto { PropertyID = 1 }, 7)).IsSuccess,
            "delete-unit" => (await f.Service.DeleteUnitAsync(2, 7)).IsSuccess,
            "create-valuation" => (await f.Service.CreateValuationAsync(new CreateValuationDto { PropertyID = 2 }, 7)).IsSuccess,
            "delete-valuation" => (await f.Service.DeleteValuationAsync(2, 7)).IsSuccess,
            "create-depreciation" => (await f.Service.CreateDepreciationAsync(new CreateDepreciationDto { PropertyID = 2 }, 7)).IsSuccess,
            _ => (await f.Service.DeleteDepreciationAsync(2, 7)).IsSuccess
        };
        Assert.False(success);
        Assert.Equal("عقار ب", f.Properties[1].PropertyName);
        Assert.False(f.Properties[1].IsDeleted);
        Assert.Null(f.Properties[1].MainPhotoPath);
        Assert.Equal(2, f.Units[1].PropertyID);
        Assert.False(f.Units[1].IsDeleted);
        Assert.False(f.Valuations[1].IsDeleted);
        Assert.False(f.Depreciation[1].IsDeleted);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_RejectsForeignBranchIncludingTheExistingUserBranchFallback(bool fallback)
    {
        var f = new Fixture();
        var dto = f.Create();
        dto.PensionBranchID = fallback ? 0 : 2;
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.CreatePropertyAsync(dto, fallback ? 8 : 7));
        Assert.Equal(5, f.Properties.Count);
        f.AssertNoWrites();
    }

    [Fact]
    public async Task Update_RejectsAnUnauthorizedBranchInputWithoutChangingAnyFields()
    {
        var f = new Fixture();
        var dto = f.Update();
        dto.PensionBranchID = 2;
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.UpdatePropertyAsync(1, dto, 7));
        Assert.Equal("عقار أ", f.Properties[0].PropertyName);
        Assert.Equal(1, f.Properties[0].PensionBranchID);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PropertyProjectReference_RequiresAuthorizedDestination(bool update)
    {
        var f = new Fixture();
        bool success;
        if (update)
        {
            var dto = f.Update(); dto.ProjectID = 2;
            success = (await f.Service.UpdatePropertyAsync(1, dto, 7)).IsSuccess;
        }
        else
        {
            var dto = f.Create(); dto.ProjectID = 2;
            success = (await f.Service.CreatePropertyAsync(dto, 7)).IsSuccess;
        }
        Assert.False(success);
        Assert.Equal(1, f.Properties[0].ProjectID);
        Assert.Equal("عقار أ", f.Properties[0].PropertyName);
        f.AssertNoWrites();
    }

    [Fact]
    public async Task MovingAnAuthorizedUnit_ToForeignProperty_IsRejectedBeforeChangingIt()
    {
        var f = new Fixture();
        Assert.False((await f.Service.UpdateUnitAsync(1,
            new UpdateUnitDto { PropertyID = 2, UnitTypeID = 1, UnitNumber = "تغيير ممنوع" }, 7)).IsSuccess);
        Assert.Equal(1, f.Units[0].PropertyID);
        Assert.Equal("وحدة أ", f.Units[0].UnitNumber);
        f.AssertNoWrites();
    }

    [Fact]
    public async Task AuthorizedProperty_CreateUpdatePhotoAndSoftDelete_PreserveExistingBehavior()
    {
        var f = new Fixture();
        var dto = f.Create(); dto.PensionBranchID = 0;
        Assert.True((await f.Service.CreatePropertyAsync(dto, 7)).IsSuccess);
        Assert.Equal(1, f.Properties.Last().PensionBranchID);
        Assert.True((await f.Service.UpdatePropertyAsync(1, f.Update(), 7)).IsSuccess);
        Assert.Equal("عقار معدل", f.Properties[0].PropertyName);
        Assert.True((await f.Service.UpdatePropertyMainPhotoPathAsync(1, "new.png", 7)).IsSuccess);
        Assert.Equal("new.png", f.Properties[0].MainPhotoPath);
        Assert.True((await f.Service.DeletePropertyAsync(1, 7)).IsSuccess);
        Assert.True(f.Properties[0].IsDeleted);
        Assert.Equal(7, f.Properties[0].DeletedByUserID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(4));
    }

    [Fact]
    public async Task AuthorizedUnit_CreateMoveAndDelete_ValidateBothCurrentAndDestinationOwners()
    {
        var f = new Fixture();
        Assert.True((await f.Service.CreateUnitAsync(new CreateUnitDto
            { PropertyID = 1, UnitTypeID = 1, UnitNumber = "وحدة جديدة" }, 7)).IsSuccess);
        Assert.True((await f.Service.UpdateUnitAsync(1, new UpdateUnitDto
            { PropertyID = 5, UnitTypeID = 1, UnitNumber = "وحدة منقولة" }, 7)).IsSuccess);
        Assert.Equal(5, f.Units[0].PropertyID);
        Assert.True((await f.Service.DeleteUnitAsync(1, 7)).IsSuccess);
        Assert.True(f.Units[0].IsDeleted);
        Assert.Equal(7, f.Units[0].DeletedByUserID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(3));
    }

    [Fact]
    public async Task AuthorizedValuationAndDepreciation_CreateAndDelete_KeepSoftDeletionAndAmounts()
    {
        var f = new Fixture();
        Assert.True((await f.Service.CreateValuationAsync(new CreateValuationDto
            { PropertyID = 1, MarketValue = 500 }, 7)).IsSuccess);
        Assert.Equal(500, f.Valuations.Last().MarketValue);
        Assert.True((await f.Service.DeleteValuationAsync(1, 7)).IsSuccess);
        Assert.True(f.Valuations[0].IsDeleted);
        Assert.Equal(7, f.Valuations[0].DeletedByUserID);
        Assert.True((await f.Service.CreateDepreciationAsync(new CreateDepreciationDto
            { PropertyID = 1, FiscalYearID = 1, DepreciationAmount = 50, NetBookValue = 950 }, 7)).IsSuccess);
        Assert.Equal(950, f.Depreciation.Last().NetBookValue);
        Assert.True((await f.Service.DeleteDepreciationAsync(1, 7)).IsSuccess);
        Assert.True(f.Depreciation[0].IsDeleted);
        Assert.Equal(7, f.Depreciation[0].DeletedByUserID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(4));
    }

    [Fact]
    public async Task MultiBranchScope_AllowsAuthorizedCrossEntityLinks_ButDoesNotInventPropertyBranchReassignment()
    {
        var f = new Fixture(branches: [1, 2]);
        var dto = f.Update(); dto.ProjectID = 2; dto.PensionBranchID = 2;
        Assert.True((await f.Service.UpdatePropertyAsync(1, dto, 7)).IsSuccess);
        Assert.Equal(2, f.Properties[0].ProjectID);
        Assert.Equal(1, f.Properties[0].PensionBranchID); // Existing update does not assign this DTO field.
        Assert.True((await f.Service.UpdateUnitAsync(1, new UpdateUnitDto
            { PropertyID = 2, UnitTypeID = 1, UnitNumber = "منقولة" }, 7)).IsSuccess);
        Assert.Equal(2, f.Units[0].PropertyID);
    }

    [Fact]
    public async Task CentralScope_CanSeeBothBranchesAndReferencedProjectNames()
    {
        var f = new Fixture(central: true);
        Assert.Equal(4, (await f.Service.GetAllPropertiesAsync()).Data!.Count);
        Assert.Equal("مشروع ب", (await f.Service.GetPropertyByIdAsync(3)).Data!.ProjectName);
        Assert.Equal(2, (await f.Service.GetAllUnitsAsync()).Data!.Count);
    }

    [Fact]
    public async Task MissingPermission_FailsClosedWithoutWrites()
    {
        var f = new Fixture(branches: []);
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.GetAllPropertiesAsync());
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.CreatePropertyAsync(f.Create(), 7));
        f.AssertNoWrites();
    }

    [Fact]
    public void ScopeQueries_ForPropertiesAndChildren_TranslateOnSqlServerWithoutAConnection()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMS_ModelOnly;Integrated Security=True").Options);
        using var unit = new UnitOfWork(db);
        var scope = new Mock<ITestBranchScope>(); scope.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var queries = TestPensionAccess.Query(unit, scope.Object);
        foreach (var sql in new[]
        {
            queries.Query(unit.InvestmentProperties).OrderBy(p => p.PropertyID).Skip(1).Take(1).ToQueryString(),
            queries.Query(unit.PropertyUnits).ToQueryString(),
            queries.Query(unit.AssetValuations).ToQueryString(),
            queries.Query(unit.DepreciationRecords).ToQueryString(),
            queries.Query(unit.InvestmentProjects).Where(p => new[] { 1, 2 }.Contains(p.ProjectID))
                .Select(p => new { p.ProjectID, p.Name }).ToQueryString()
        }) Assert.Contains("PensionBranchID", sql);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new(MockBehavior.Strict);
        public List<InvestmentProperty> Properties { get; }
        public List<PropertyUnit> Units { get; }
        public List<AssetValuation> Valuations { get; }
        public List<DepreciationRecord> Depreciation { get; }
        public InvestmentPropertyService Service { get; }
        private readonly List<Action> _verifyNoWrites = [];

        public Fixture(bool central = false, int[]? branches = null)
        {
            var projects = new List<InvestmentProject>
            { new() { ProjectID = 1, PensionBranchID = 1, Name = "مشروع أ" }, new() { ProjectID = 2, PensionBranchID = 2, Name = "مشروع ب" } };
            Properties = [new() { PropertyID = 1, PensionBranchID = 1, PropertyName = "عقار أ", ProjectID = 1, Project = projects[0] },
                new() { PropertyID = 2, PensionBranchID = 2, PropertyName = "عقار ب", ProjectID = 2, Project = projects[1] },
                new() { PropertyID = 3, PensionBranchID = 1, PropertyName = "رابط قديم", ProjectID = 2, Project = projects[1] },
                new() { PropertyID = 4, PensionBranchID = 1, PropertyName = "محذوف", IsDeleted = true },
                new() { PropertyID = 5, PensionBranchID = 1, PropertyName = "عقار وجهة" }];
            Units = [new() { UnitID = 1, PropertyID = 1, Property = Properties[0], UnitNumber = "وحدة أ" },
                new() { UnitID = 2, PropertyID = 2, Property = Properties[1], UnitNumber = "وحدة ب" }];
            Valuations = [new() { ValuationID = 1, PropertyID = 1, Property = Properties[0], ValuationDate = new DateTime(2026, 1, 1), MarketValue = 300 },
                new() { ValuationID = 2, PropertyID = 2, Property = Properties[1], MarketValue = 900 },
                new() { ValuationID = 3, PropertyID = 1, Property = Properties[0], ValuationDate = new DateTime(2026, 1, 1), MarketValue = 400 }];
            Depreciation = [new() { DeprecID = 1, PropertyID = 1, Property = Properties[0] },
                new() { DeprecID = 2, PropertyID = 2, Property = Properties[1] }];
            Properties[0].Units = [Units[0]];
            Properties[0].Valuations = [Valuations[0], Valuations[2]];
            Bind(u => u.RetirementFiles, new List<RetirementFile>());
            Bind(u => u.Retirees, new List<Retiree>());
            Bind(u => u.InvestmentProperties, Properties);
            Bind(u => u.PropertyUnits, Units);
            Bind(u => u.AssetValuations, Valuations);
            Bind(u => u.DepreciationRecords, Depreciation);
            Bind(u => u.InvestmentProjects, projects);
            Bind(u => u.Users, new List<User> { new() { UserID = 7, BranchID = 1 }, new() { UserID = 8, BranchID = 2 } }, u => u.UserID);
            Bind(u => u.PropertyTypes, new List<PropertyType> { new() { TypeID = 1 } }, t => t.TypeID);
            Bind(u => u.UnitTypes, new List<UnitType> { new() { TypeID = 1 } }, t => t.TypeID);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Service = new InvestmentPropertyService(Unit.Object, new TestPensionAccess(Unit.Object,
                () => central ? PermissionScope.All : PermissionScope.Restricted(branches ?? [1])));
        }

        public CreatePropertyDto Create() => new() { PropertyName = "عقار جديد", PropertyTypeID = 1, PensionBranchID = 1 };
        public UpdatePropertyDto Update() => new() { PropertyName = "عقار معدل", PropertyTypeID = 1, PensionBranchID = 1 };

        public void AssertNoWrites()
        {
            Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
            foreach (var verify in _verifyNoWrites) verify();
        }

        private void Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> selector, List<T> rows, Func<T, int>? key = null) where T : class
        {
            var repository = new Mock<IRepository<T>>(MockBehavior.Strict);
            repository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows.Where(r => r is not ISoftDeletable d || !d.IsDeleted)));
            if (key != null) repository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => rows.FirstOrDefault(r => key(r) == id));
            repository.Setup(r => r.AddAsync(It.IsAny<T>())).Callback<T>(rows.Add).Returns(Task.CompletedTask);
            repository.Setup(r => r.Update(It.IsAny<T>()));
            Unit.Setup(selector).Returns(repository.Object);
            _verifyNoWrites.Add(() =>
            {
                repository.Verify(r => r.AddAsync(It.IsAny<T>()), Times.Never);
                repository.Verify(r => r.Update(It.IsAny<T>()), Times.Never);
            });
        }
    }
}
