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

public class InvestmentLeasingAuthorizationTests
{
    [Fact]
    public async Task ReadsAndCounts_AreScopedIndependentlyOfSharedTenantRelationships()
    {
        var f = new Fixture();
        var tenant = (await f.Service.GetTenantByIdAsync(1)).Data!;
        Assert.Equal(1, tenant.ActiveContractsCount); // Expired contract is not active.
        Assert.False((await f.Service.GetTenantByIdAsync(2)).IsSuccess);
        Assert.False((await f.Service.GetTenantByIdAsync(3)).IsSuccess); // Shared profile uses existing all-branches rule.
        Assert.Equal(new[] { 1, 4 }, (await f.Service.GetAllTenantsAsync()).Data!.Select(t => t.TenantID));
        Assert.Equal(0, (await f.Service.GetTenantByIdAsync(4)).Data!.ActiveContractsCount);
        Assert.Equal(3, Assert.Single((await f.Service.GetPropertiesByTenantAsync(3)).Data!).PropertyID);
        Assert.False((await f.Service.GetPropertiesByTenantAsync(2)).IsSuccess);
        Assert.Empty((await f.Service.GetPropertiesByTenantAsync(4)).Data!);
        Assert.Equal(new[] { 1, 3, 4 }, (await f.Service.GetAllLeasesAsync()).Data!.Select(c => c.ContractID));
        Assert.Equal(4, (await f.Service.GetLeaseContractByIdAsync(4)).Data!.ContractID);
        Assert.Equal("مشترك", (await f.Service.GetLeaseContractByIdAsync(4)).Data!.TenantName);
        Assert.False((await f.Service.GetLeaseContractByIdAsync(5)).IsSuccess);
        Assert.Empty((await f.Service.GetLeasesByPropertyAsync(2)).Data!);
        Assert.Equal(2, (await f.Service.GetLeasesByPropertyAsync(1)).Data!.Count);
        Assert.Equal(new[] { 3, 1 }, (await f.Service.GetAllSchedulesAsync()).Data!.Select(s => s.ScheduleID));
        Assert.Empty((await f.Service.GetSchedulesByContractAsync(5)).Data!);
        Assert.Equal(1, Assert.Single((await f.Service.GetSchedulesByContractAsync(1)).Data!).ScheduleID);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData("update-foreign-tenant")]
    [InlineData("delete-foreign-tenant")]
    [InlineData("update-shared-tenant")]
    [InlineData("delete-shared-tenant")]
    [InlineData("update-contract")]
    [InlineData("delete-contract")]
    [InlineData("file-path")]
    [InlineData("create-payment")]
    [InlineData("update-payment")]
    public async Task ForeignOrSharedMutations_AreRejectedBeforeChangingRows(string operation)
    {
        var f = new Fixture();
        var update = new UpdateTenantDto { FullName = "تغيير ممنوع", NationalIDOrCR = "changed" };
        var success = operation switch
        {
            "update-foreign-tenant" => (await f.Service.UpdateTenantAsync(2, update, 7)).IsSuccess,
            "delete-foreign-tenant" => (await f.Service.DeleteTenantAsync(2, 7)).IsSuccess,
            "update-shared-tenant" => (await f.Service.UpdateTenantAsync(3, update, 7)).IsSuccess,
            "delete-shared-tenant" => (await f.Service.DeleteTenantAsync(3, 7)).IsSuccess,
            "update-contract" => (await f.Service.UpdateLeaseContractAsync(2, f.Update(), 7)).IsSuccess,
            "delete-contract" => (await f.Service.DeleteLeaseContractAsync(2, 7)).IsSuccess,
            "file-path" => (await f.Service.UpdateLeaseContractFilePathAsync(2, "new.pdf", 7)).IsSuccess,
            "create-payment" => (await f.Service.CreatePaymentScheduleAsync(new CreatePaymentScheduleDto { ContractID = 2, ExpectedAmount = 10 }, 7)).IsSuccess,
            _ => (await f.Service.UpdatePaymentStatusAsync(2, new UpdatePaymentStatusDto { PaidAmount = 90, Status = "Paid" }, 7)).IsSuccess
        };
        Assert.False(success);
        Assert.Equal("مستأجر ب", f.Tenants[1].FullName);
        Assert.False(f.Tenants[1].IsDeleted);
        Assert.Equal("مشترك", f.Tenants[2].FullName);
        Assert.False(f.Tenants[2].IsDeleted);
        Assert.Equal(2, f.Contracts[1].UnitID);
        Assert.False(f.Contracts[1].IsDeleted);
        Assert.Null(f.Contracts[1].SignedContractPath);
        Assert.Equal(5, f.Schedules[1].PaidAmount);
        f.AssertNoWrites();
    }

    [Theory]
    [InlineData(false, 7, 1)]
    [InlineData(false, 6, 2)]
    [InlineData(false, 999, 1)]
    [InlineData(true, 7, 1)]
    [InlineData(true, 6, 2)]
    [InlineData(true, 999, 1)]
    public async Task ContractDestination_RequiresAnAuthorizedUnitAndTenantContext(bool update, int unitId, int tenantId)
    {
        var f = new Fixture();
        bool success;
        if (update)
        {
            var dto = f.Update(); dto.UnitID = unitId; dto.TenantID = tenantId;
            success = (await f.Service.UpdateLeaseContractAsync(1, dto, 7)).IsSuccess;
        }
        else
        {
            var dto = f.Create(); dto.UnitID = unitId; dto.TenantID = tenantId;
            success = (await f.Service.CreateLeaseContractAsync(dto, 7)).IsSuccess;
        }
        Assert.False(success);
        Assert.Equal(1, f.Contracts[0].UnitID);
        Assert.Equal(1, f.Contracts[0].TenantID);
        Assert.Equal(5, f.Contracts.Count);
        f.AssertNoWrites();
        if (!update) f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task InitialTenantAndFirstLease_PreserveExistingWorkflowAndScheduleRounding()
    {
        var f = new Fixture();
        Assert.True((await f.Service.CreateTenantAsync(new CreateTenantDto
            { FullName = "مستأجر جديد", NationalIDOrCR = "new", Phone = "test", RatingScore = 3 }, 7)).IsSuccess);
        Assert.Equal("new", f.Tenants.Last().NationalIDOrCR);
        var dto = f.Create(); dto.TenantID = 4; dto.GenerateSchedules = true;
        Assert.True((await f.Service.CreateLeaseContractAsync(dto, 7)).IsSuccess);
        var contract = f.Contracts.Last();
        var generated = f.Schedules.Where(s => s.ContractID == contract.ContractID).ToList();
        Assert.Equal(12, generated.Count);
        Assert.Equal(1000, generated.Sum(s => s.ExpectedAmount));
        Assert.Equal(83.37m, generated.Last().ExpectedAmount);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(3));
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task SharedTenant_CanBeReferencedFromItsAuthorizedLeaseContextWithoutExposingOtherContracts()
    {
        var f = new Fixture();
        var dto = f.Create(); dto.TenantID = 3;
        Assert.True((await f.Service.CreateLeaseContractAsync(dto, 7)).IsSuccess);
        Assert.False((await f.Service.GetLeaseContractByIdAsync(5)).IsSuccess);
        Assert.Empty((await f.Service.GetSchedulesByContractAsync(5)).Data!);
    }

    [Fact]
    public async Task TenantUpdateAndDelete_RequireAllAssociatedBranchesButNotAnImplicitCentralRole()
    {
        var f = new Fixture(branches: [1, 2]);
        Assert.Equal(2, (await f.Service.GetTenantByIdAsync(3)).Data!.ActiveContractsCount);
        Assert.True((await f.Service.UpdateTenantAsync(3, new UpdateTenantDto
            { FullName = "مشترك معدل", NationalIDOrCR = "shared", RatingScore = 4 }, 7)).IsSuccess);
        Assert.Equal("مشترك معدل", f.Tenants[2].FullName);
        Assert.True((await f.Service.DeleteTenantAsync(3, 7)).IsSuccess);
        Assert.True(f.Tenants[2].IsDeleted);
        Assert.Equal(7, f.Tenants[2].DeletedByUserID);
    }

    [Fact]
    public async Task UnlinkedTenant_ProfileAndMutationRemainOperationPermissionBasedPendingOwnershipDecision()
    {
        var f = new Fixture();
        Assert.True((await f.Service.GetTenantByIdAsync(4)).IsSuccess);
        Assert.True((await f.Service.UpdateTenantAsync(4, new UpdateTenantDto
            { FullName = "غير مرتبط معدل", NationalIDOrCR = "unlinked" }, 7)).IsSuccess);
        Assert.True((await f.Service.DeleteTenantAsync(4, 7)).IsSuccess);
        Assert.True(f.Tenants[3].IsDeleted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GlobalTenantIdentityUniqueness_IsPreservedWithoutReturningForeignProfile(bool update)
    {
        var f = new Fixture();
        var success = update
            ? (await f.Service.UpdateTenantAsync(1, new UpdateTenantDto { FullName = "تغيير", NationalIDOrCR = "foreign" }, 7)).IsSuccess
            : (await f.Service.CreateTenantAsync(new CreateTenantDto { FullName = "تغيير", NationalIDOrCR = "foreign" }, 7)).IsSuccess;
        Assert.False(success);
        Assert.Equal("مستأجر أ", f.Tenants[0].FullName);
        f.AssertNoWrites();
    }

    [Fact]
    public async Task AuthorizedContractAndPaymentMutations_PreserveFieldsAndSoftDeletion()
    {
        var f = new Fixture();
        Assert.True((await f.Service.UpdateLeaseContractAsync(1, f.Update(), 7)).IsSuccess);
        Assert.Equal(6, f.Contracts[0].UnitID);
        Assert.True((await f.Service.UpdateLeaseContractFilePathAsync(1, "new.pdf", 7)).IsSuccess);
        Assert.Equal("new.pdf", f.Contracts[0].SignedContractPath);
        Assert.True((await f.Service.CreatePaymentScheduleAsync(new CreatePaymentScheduleDto
            { ContractID = 1, DueDate = new DateTime(2026, 5, 1), ExpectedAmount = 100 }, 7)).IsSuccess);
        Assert.True((await f.Service.UpdatePaymentStatusAsync(1, new UpdatePaymentStatusDto { PaidAmount = 10, Status = "Paid" }, 7)).IsSuccess);
        Assert.Equal(10, f.Schedules[0].PaidAmount);
        Assert.True((await f.Service.DeleteLeaseContractAsync(1, 7)).IsSuccess);
        Assert.True(f.Contracts[0].IsDeleted);
        Assert.Equal(7, f.Contracts[0].DeletedByUserID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(5));
    }

    [Fact]
    public async Task MultiBranchPermission_AllowsMovingContractIntoAnotherAuthorizedBranch()
    {
        var f = new Fixture(branches: [1, 2]);
        var dto = f.Update(); dto.UnitID = 7; dto.TenantID = 2;
        Assert.True((await f.Service.UpdateLeaseContractAsync(1, dto, 7)).IsSuccess);
        Assert.Equal(7, f.Contracts[0].UnitID);
        Assert.Equal(2, f.Contracts[0].TenantID);
    }

    [Fact]
    public async Task InvalidDatesAndNestedTransactions_PreserveExistingFailureAndOwnership()
    {
        var f = new Fixture();
        var dto = f.Create(); dto.EndDate = dto.StartDate;
        Assert.False((await f.Service.CreateLeaseContractAsync(dto, 7)).IsSuccess);
        f.AssertNoWrites();
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);

        var nested = new Fixture { ActiveTransaction = true };
        Assert.True((await nested.Service.CreateLeaseContractAsync(nested.Create(), 7)).IsSuccess);
        Assert.True(nested.ActiveTransaction);
        nested.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        nested.Unit.Verify(u => u.CommitAsync(), Times.Never);
        nested.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task MissingScope_FailsClosedEvenWhenActorIdIsSupplied()
    {
        var f = new Fixture(branches: []);
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.GetAllTenantsAsync());
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.GetAllLeasesAsync());
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.GetAllSchedulesAsync());
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => f.Service.CreateTenantAsync(new CreateTenantDto(), 1));
        f.AssertNoWrites();
    }

    [Fact]
    public void ScopedLeaseAndPaymentQueries_TranslateOnSqlServerWithoutConnecting()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMS_ModelOnly;Integrated Security=True").Options);
        using var uow = new UnitOfWork(db);
        var identity = new Mock<ITestBranchScope>(); identity.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var q = TestPensionAccess.Query(uow, identity.Object);
        foreach (var sql in new[]
        {
            q.Query(uow.LeaseContracts).ToQueryString(), q.Query(uow.PaymentSchedules).ToQueryString(),
            q.Query(uow.Tenants).ToQueryString(), q.Query(uow.PropertyUnits).ToQueryString()
        }) Assert.Contains("PensionBranchID", sql);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new(MockBehavior.Strict);
        public List<Tenant> Tenants { get; }
        public List<PropertyUnit> Units { get; }
        public List<LeaseContract> Contracts { get; }
        public List<PaymentSchedule> Schedules { get; }
        public InvestmentLeasingService Service { get; }
        public bool ActiveTransaction { get; set; }
        private readonly List<Action> _verifyNoWrites = [];

        public Fixture(int[]? branches = null)
        {
            var a = new InvestmentProperty { PropertyID = 1, PensionBranchID = 1, PropertyName = "عقار أ" };
            var b = new InvestmentProperty { PropertyID = 2, PensionBranchID = 2, PropertyName = "عقار ب" };
            var destination = new InvestmentProperty { PropertyID = 3, PensionBranchID = 1, PropertyName = "وجهة أ" };
            Units = Enumerable.Range(1, 7).Select(id => new PropertyUnit { UnitID = id,
                Property = id is 2 or 5 or 7 ? b : id is 4 or 6 ? destination : a,
                PropertyID = id is 2 or 5 or 7 ? 2 : id is 4 or 6 ? 3 : 1, UnitNumber = id.ToString() }).ToList();
            Tenants = [new() { TenantID = 1, FullName = "مستأجر أ", NationalIDOrCR = "own" },
                new() { TenantID = 2, FullName = "مستأجر ب", NationalIDOrCR = "foreign" },
                new() { TenantID = 3, FullName = "مشترك", NationalIDOrCR = "shared" },
                new() { TenantID = 4, FullName = "غير مرتبط", NationalIDOrCR = "unlinked" }];
            Contracts = Enumerable.Range(1, 5).Select(id => new LeaseContract { ContractID = id, UnitID = id, Unit = Units[id - 1],
                TenantID = id is 1 or 3 ? 1 : id == 2 ? 2 : 3,
                Tenant = Tenants[id is 1 or 3 ? 0 : id == 2 ? 1 : 2],
                Status = id == 3 ? "Expired" : LeaseStatusConstants.Active }).ToList();
            foreach (var contract in Contracts) contract.Tenant.LeaseContracts.Add(contract);
            Schedules = Enumerable.Range(1, 4).Select(id => new PaymentSchedule { ScheduleID = id,
                ContractID = id <= 2 ? id : id + 1, LeaseContract = Contracts[id <= 2 ? id - 1 : id],
                ExpectedAmount = id * 10, PaidAmount = id == 2 ? 5 : 0, DueDate = new DateTime(2026, id, 1) }).ToList();
            Bind(u => u.RetirementFiles, new List<RetirementFile>());
            Bind(u => u.Retirees, new List<Retiree>());
            Bind(u => u.Tenants, Tenants);
            Bind(u => u.PropertyUnits, Units);
            Bind(u => u.LeaseContracts, Contracts);
            Bind(u => u.PaymentSchedules, Schedules);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Unit.SetupGet(u => u.HasActiveTransaction).Returns(() => ActiveTransaction);
            Unit.Setup(u => u.BeginTransactionAsync()).Callback(() => ActiveTransaction = true).Returns(Task.CompletedTask);
            Unit.Setup(u => u.CommitAsync()).Callback(() => ActiveTransaction = false).Returns(Task.CompletedTask);
            Unit.Setup(u => u.RollbackAsync()).Callback(() => ActiveTransaction = false).Returns(Task.CompletedTask);
            Service = new InvestmentLeasingService(Unit.Object, new TestPensionAccess(Unit.Object,
                () => PermissionScope.Restricted(branches ?? [1])));
        }

        public CreateLeaseDto Create() => new() { UnitID = 6, TenantID = 1, StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2027, 1, 1), TotalContractValue = 1000, PaymentFrequency = PaymentFrequencyConstants.Monthly };
        public UpdateLeaseDto Update() => new() { UnitID = 6, TenantID = 1, StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2027, 1, 1), TotalContractValue = 1000, PaymentFrequency = PaymentFrequencyConstants.Monthly };

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
                if (row is LeaseContract contract)
                {
                    contract.ContractID = Contracts.Count + 10;
                    contract.Unit = Units.Single(u => u.UnitID == contract.UnitID);
                    contract.Tenant = Tenants.Single(t => t.TenantID == contract.TenantID);
                    contract.Tenant.LeaseContracts.Add(contract);
                }
                if (row is PaymentSchedule payment)
                {
                    payment.ScheduleID = Schedules.Count + 10;
                    payment.LeaseContract = Contracts.Single(c => c.ContractID == payment.ContractID);
                }
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
