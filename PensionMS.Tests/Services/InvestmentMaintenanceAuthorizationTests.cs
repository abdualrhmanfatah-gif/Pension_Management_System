using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PensionMS.Core.Constants;
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

public class InvestmentMaintenanceAuthorizationTests
{
    [Fact]
    public void RequestProjection_TranslatesItsActualScopedSubqueriesWithoutOpeningSqlConnection()
    {
        // Model/translation coverage only: no SQL execution or transaction/isolation proof.
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMS_ModelOnly;Integrated Security=True").Options);
        using var unit = new UnitOfWork(db);
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var queries = TestPensionAccess.Query(unit, identity.Object);
        var service = new InvestmentMaintenanceService(unit, TestPensionAccess.From(unit, identity.Object));
        var projection = typeof(InvestmentMaintenanceService).GetMethod("ProjectRequests",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var requestQuery = queries.Query(unit.MaintenanceRequests).Where(r => !r.IsDeleted);
        var result = (IQueryable<MaintenanceRequestDto>)projection.Invoke(service, [requestQuery, queries])!;
        var sql = result.ToQueryString();
        Assert.Contains("PensionBranchID", sql);
        Assert.Contains("PropertyUnits", sql);
        Assert.Contains("LeaseContracts", sql);
        Assert.Contains("Tenants", sql);
        Assert.Contains("PropertyID", sql);
    }

    [Fact]
    public async Task AllReads_ScopeBeforeProjection_AndKeepSharedPartiesContextual()
    {
        var f = new Fixture();
        Assert.Equal("عقار أ", (await f.Service.GetMaintenanceRequestByIdAsync(1)).Data!.PropertyName);
        var hiddenRequest = await f.Service.GetMaintenanceRequestByIdAsync(2);
        Assert.False(hiddenRequest.IsSuccess);
        Assert.Equal("طلب الصيانة غير موجود", hiddenRequest.Message);
        Assert.Equal(new[] { 3, 1 }, (await f.Service.GetAllMaintenanceRequestsAsync()).Data!.Select(r => r.RequestID));
        Assert.Equal(new[] { 3, 1 }, (await f.Service.GetRequestsByPropertyAsync(0)).Data!.Select(r => r.RequestID));
        Assert.Equal(2, (await f.Service.GetRequestsByPropertyAsync(-1)).Data!.Count);
        Assert.Equal(new[] { 3, 1 }, (await f.Service.GetRequestsByPropertyAsync(1)).Data!.Select(r => r.RequestID));
        Assert.Empty((await f.Service.GetRequestsByPropertyAsync(2)).Data!);
        Assert.Empty((await f.Service.GetRequestsByPropertyAsync(3)).Data!);
        Assert.Equal("مشترك", (await f.Service.GetMaintenanceRequestByIdAsync(3)).Data!.TenantName);
        var hiddenOrder = await f.Service.GetWorkOrderByIdAsync(2);
        Assert.False(hiddenOrder.IsSuccess);
        Assert.Equal("أمر العمل غير موجود", hiddenOrder.Message);
        Assert.Equal(20, (await f.Service.GetWorkOrderByIdAsync(1)).Data!.EstimatedCost);
        Assert.Equal(1, Assert.Single((await f.Service.GetWorkOrdersByRequestAsync(1)).Data!).WorkOrderID);
        Assert.Empty((await f.Service.GetWorkOrdersByRequestAsync(2)).Data!);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData("request-update")]
    [InlineData("order-create")]
    [InlineData("order-update")]
    public async Task ForeignMutations_RejectBeforeTrackedFieldsOrRepositoriesChange(string operation)
    {
        var f = new Fixture();
        var result = operation switch
        {
            "request-update" => (await f.Service.UpdateMaintenanceRequestAsync(2, new() { Status = "Changed" }, 7)).IsSuccess,
            "order-create" => (await f.Service.CreateWorkOrderAsync(new() { RequestID = 2, EstimatedCost = 99 }, 7)).IsSuccess,
            _ => (await f.Service.UpdateWorkOrderAsync(2, Update(), 7)).IsSuccess
        };
        Assert.False(result);
        Assert.Equal(MaintenanceStatusConstants.New, f.Requests[1].Status);
        Assert.Equal(0, f.Orders[1].ActualCost);
        Assert.Equal("old.pdf", f.Orders[1].InvoiceFilePath);
        Assert.Null(f.Orders[1].CompletionDate);
        Assert.Null(f.Orders[1].UpdatedAt);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData(2, 2, 1, false)] // Foreign property.
    [InlineData(1, 2, 1, false)] // Foreign unit.
    [InlineData(1, 3, 1, false)] // Different property in same branch.
    [InlineData(1, 2, 1, true)] // Both branches allowed, still wrong property.
    [InlineData(1, 1, 2, false)] // Foreign-only tenant.
    [InlineData(999, null, null, false)]
    [InlineData(1, 999, null, false)]
    [InlineData(1, null, 999, false)]
    public async Task RequestCreation_ValidatesExactPropertyAndAuthorizedRelatedContext(int property, int? unit, int? tenant, bool multi)
    {
        var f = new Fixture(multi ? [1, 2] : [1]);
        var result = await f.Service.CreateMaintenanceRequestAsync(Create(property, unit, tenant), 7);
        Assert.False(result.IsSuccess);
        Assert.Equal(3, f.Requests.Count);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData("property")]
    [InlineData("unit")]
    [InlineData("tenant")]
    [InlineData("request")]
    [InlineData("order")]
    public async Task DeletedReferences_AndEntities_AreUnavailable(string entity)
    {
        var f = new Fixture();
        bool success;
        switch (entity)
        {
            case "property": f.Properties[0].IsDeleted = true; success = (await f.Service.CreateMaintenanceRequestAsync(Create(), 7)).IsSuccess; break;
            case "unit": f.Units[0].IsDeleted = true; success = (await f.Service.CreateMaintenanceRequestAsync(Create(), 7)).IsSuccess; break;
            case "tenant": f.Tenants[0].IsDeleted = true; success = (await f.Service.CreateMaintenanceRequestAsync(Create(), 7)).IsSuccess; break;
            case "request": f.Requests[0].IsDeleted = true; success = (await f.Service.CreateWorkOrderAsync(new() { RequestID = 1 }, 7)).IsSuccess; break;
            default: f.Orders[0].IsDeleted = true; success = (await f.Service.UpdateWorkOrderAsync(1, Update(), 7)).IsSuccess; break;
        }
        Assert.False(success);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(1, 3)] // Shared tenant.
    [InlineData(1, 4)] // Unlinked ownership unresolved, preserve first linkage.
    public async Task LegitimateOptionalAndSharedReferences_PreserveRequestCreation(int? unit, int? tenant)
    {
        var f = new Fixture();
        Assert.True((await f.Service.CreateMaintenanceRequestAsync(Create(1, unit, tenant), 7)).IsSuccess);
        var created = f.Requests.Last();
        Assert.Equal(unit, created.UnitID);
        Assert.Equal(tenant, created.TenantID);
        Assert.Equal(MaintenanceStatusConstants.New, created.Status);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(999)]
    public async Task LegacyMismatchedReferences_AreRedactedWithoutHidingAuthorizedRequestOrStatusWorkflow(int unit)
    {
        var f = new Fixture();
        f.Requests[0].UnitID = unit;
        f.Requests[0].TenantID = 2;
        var read = (await f.Service.GetMaintenanceRequestByIdAsync(1)).Data!;
        Assert.Null(read.UnitID); Assert.Null(read.UnitNumber);
        Assert.Null(read.TenantID); Assert.Null(read.TenantName);
        var list = (await f.Service.GetAllMaintenanceRequestsAsync()).Data!.Single(r => r.RequestID == 1);
        Assert.Null(list.UnitNumber); Assert.Null(list.TenantName);
        Assert.True((await f.Service.UpdateMaintenanceRequestAsync(1, new() { Status = "Cancelled" }, 7)).IsSuccess);
        Assert.True((await f.Service.UpdateWorkOrderAsync(1, Update(), 7)).IsSuccess);
    }

    [Fact]
    public async Task DeletedOptionalReferences_AreRedacted_AndUnlinkedPartyRemainsContextual()
    {
        var f = new Fixture();
        f.Units[0].IsDeleted = true;
        f.Tenants[0].IsDeleted = true;
        var read = (await f.Service.GetMaintenanceRequestByIdAsync(1)).Data!;
        Assert.Null(read.UnitID); Assert.Null(read.TenantID);
        f.Requests[0].TenantID = 4;
        Assert.Equal("غير مرتبط", (await f.Service.GetMaintenanceRequestByIdAsync(1)).Data!.TenantName);
    }

    [Theory]
    [InlineData("New", "InProgress")]
    [InlineData("Cancelled", "Cancelled")]
    [InlineData("Completed", "Completed")]
    public async Task AuthorizedOrderLifecycle_PreservesExistingStatusCostAndSingleSaveBehavior(string initial, string expected)
    {
        var f = new Fixture();
        f.Requests[0].Status = initial;
        Assert.True((await f.Service.CreateWorkOrderAsync(new() { RequestID = 1, AssignedContractor = "مشترك", EstimatedCost = 75 }, 7)).IsSuccess);
        Assert.Equal(expected, f.Requests[0].Status);
        Assert.Equal(75, f.Orders.Last().EstimatedCost);
        Assert.Equal(0, f.Orders.Last().ActualCost);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Once);
        Assert.True((await f.Service.UpdateWorkOrderAsync(1, Update(), 7)).IsSuccess);
        Assert.Equal(MaintenanceStatusConstants.Completed, f.Requests[0].Status);
        Assert.Equal(30, f.Orders[0].ActualCost);
        Assert.Equal("new.pdf", f.Orders[0].InvoiceFilePath);
        Assert.Equal("cert.pdf", f.Orders[0].CompletionCertPath);
        Assert.Equal(new DateTime(2026, 10, 1), f.Orders[0].CompletionDate);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task WorkOrderParent_MustBeLiveAndScopedBeforeAnyAssignments()
    {
        var f = new Fixture();
        // Simulates a previously read/navigation-valid order whose actual parent source is unavailable.
        f.Requests.RemoveAt(0);
        var unavailableParent = await f.Service.UpdateWorkOrderAsync(1, Update(), 7);
        Assert.False(unavailableParent.IsSuccess);
        Assert.Equal("طلب الصيانة غير موجود", unavailableParent.Message);
        Assert.Equal(0, f.Orders[0].ActualCost);
        Assert.Null(f.Orders[0].UpdatedAt);
        f.AssertNoWrites();
    }

    [Fact]
    public async Task MultiBranchPermission_PreservesAuthorizedForeignBranchWorkflow()
    {
        var f = new Fixture([1, 2]);
        Assert.Equal(3, (await f.Service.GetAllMaintenanceRequestsAsync()).Data!.Count);
        Assert.True((await f.Service.GetWorkOrderByIdAsync(2)).IsSuccess);
        Assert.True((await f.Service.CreateMaintenanceRequestAsync(Create(2, 2, 2), 7)).IsSuccess);
        Assert.True((await f.Service.UpdateMaintenanceRequestAsync(2, new() { Status = "InProgress" }, 7)).IsSuccess);
        Assert.True((await f.Service.CreateWorkOrderAsync(new() { RequestID = 2 }, 7)).IsSuccess);
        Assert.True((await f.Service.UpdateWorkOrderAsync(2, Update(), 7)).IsSuccess);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
    public async Task EveryContractMember_FailsClosedWithoutOperationScope(int operation)
    {
        var f = new Fixture([]);
        await Assert.ThrowsAsync<BranchScopeAccessException>(async () =>
        {
            switch (operation)
            {
                case 0: await f.Service.CreateMaintenanceRequestAsync(Create(), 7); break;
                case 1: await f.Service.UpdateMaintenanceRequestAsync(1, new(), 7); break;
                case 2: await f.Service.GetMaintenanceRequestByIdAsync(1); break;
                case 3: await f.Service.GetRequestsByPropertyAsync(1); break;
                case 4: await f.Service.GetAllMaintenanceRequestsAsync(); break;
                case 5: await f.Service.CreateWorkOrderAsync(new() { RequestID = 1 }, 7); break;
                case 6: await f.Service.UpdateWorkOrderAsync(1, Update(), 7); break;
                case 7: await f.Service.GetWorkOrderByIdAsync(1); break;
                default: await f.Service.GetWorkOrdersByRequestAsync(1); break;
            }
        });
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task SaveFailures_PropagateWithoutInventingTransactionOwnership(bool workOrder)
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new InvalidOperationException("save-failed"));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            if (workOrder) await f.Service.CreateWorkOrderAsync(new() { RequestID = 1 }, 7);
            else await f.Service.CreateMaintenanceRequestAsync(Create(), 7);
        });
        Assert.Equal("save-failed", error.Message);
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    private static CreateMaintenanceRequestDto Create(int property = 1, int? unit = 1, int? tenant = 1) => new()
        { PropertyID = property, UnitID = unit, TenantID = tenant, Description = "مشترك", ReportedDate = new DateTime(2026, 1, 1) };
    private static UpdateWorkOrderDto Update() => new()
        { ActualCost = 30, CompletionDate = new DateTime(2026, 10, 1), InvoiceFilePath = "new.pdf", CompletionCertPath = "cert.pdf" };

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new(MockBehavior.Strict);
        public List<InvestmentProperty> Properties { get; }
        public List<PropertyUnit> Units { get; }
        public List<Tenant> Tenants { get; }
        public List<LeaseContract> Contracts { get; }
        public List<MaintenanceRequest> Requests { get; }
        public List<WorkOrder> Orders { get; }
        public InvestmentMaintenanceService Service { get; }
        private readonly List<Action> _verifyNoWrites = [];
        public Fixture(int[]? branches = null)
        {
            Properties = [new() { PropertyID = 1, PensionBranchID = 1, PropertyName = "عقار أ" },
                new() { PropertyID = 2, PensionBranchID = 2, PropertyName = "عقار ب" },
                new() { PropertyID = 3, PensionBranchID = 1, PropertyName = "عقار آخر أ" }];
            Units = Properties.Select(p => new PropertyUnit { UnitID = p.PropertyID, PropertyID = p.PropertyID, Property = p, UnitNumber = "وحدة " + p.PropertyID }).ToList();
            Tenants = Enumerable.Range(1, 4).Select(id => new Tenant { TenantID = id, FullName = id == 3 ? "مشترك" : id == 4 ? "غير مرتبط" : "مستأجر " + id }).ToList();
            Contracts = Enumerable.Range(1, 4).Select(id => new LeaseContract { ContractID = id, Unit = Units[id % 2 == 1 ? 0 : 1], UnitID = id % 2 == 1 ? 1 : 2,
                TenantID = id <= 2 ? id : 3, Tenant = Tenants[id <= 2 ? id - 1 : 2] }).ToList();
            foreach (var contract in Contracts) contract.Tenant.LeaseContracts.Add(contract);
            Requests = Enumerable.Range(1, 3).Select(id => new MaintenanceRequest { RequestID = id, PropertyID = id == 2 ? 2 : 1,
                Property = Properties[id == 2 ? 1 : 0], UnitID = id == 2 ? 2 : 1, TenantID = id, Description = "طلب " + id,
                ReportedDate = new DateTime(2026, id, 1), Status = MaintenanceStatusConstants.New }).ToList();
            Orders = Requests.Take(2).Select(r => new WorkOrder { WorkOrderID = r.RequestID, RequestID = r.RequestID,
                MaintenanceRequest = r, EstimatedCost = 20, InvoiceFilePath = "old.pdf" }).ToList();
            Bind(u => u.RetirementFiles, new List<RetirementFile>());
            Bind(u => u.Retirees, new List<Retiree>());
            Bind(u => u.InvestmentProperties, Properties); Bind(u => u.PropertyUnits, Units);
            Bind(u => u.Tenants, Tenants); Bind(u => u.LeaseContracts, Contracts);
            Bind(u => u.MaintenanceRequests, Requests); Bind(u => u.WorkOrders, Orders);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Service = new InvestmentMaintenanceService(Unit.Object, new TestPensionAccess(Unit.Object,
                () => PermissionScope.Restricted(branches ?? [1])));
        }
        public void AssertNoWrites()
        {
            Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
            foreach (var verify in _verifyNoWrites) verify();
        }
        private void Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> selector, List<T> rows) where T : class
        {
            var repository = new Mock<IRepository<T>>(MockBehavior.Strict);
            repository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows.Where(r => r is not ISoftDeletable d || !d.IsDeleted)));
            repository.Setup(r => r.AddAsync(It.IsAny<T>())).Callback<T>(row =>
            {
                if (row is MaintenanceRequest request) request.RequestID = Requests.Count + 10;
                if (row is WorkOrder order) order.WorkOrderID = Orders.Count + 10;
                rows.Add(row);
            }).Returns(Task.CompletedTask);
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
