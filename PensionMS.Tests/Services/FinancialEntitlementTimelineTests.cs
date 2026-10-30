using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class FinancialEntitlementTimelineTests
{
    [Fact]
    public async Task ApprovalOnly_UsesSourceMoneyAndDateInsteadOfRequest()
    {
        var f = new Fixture();
        var result = await f.CreateAsync();
        Assert.True(result.IsSuccess, result.Message);
        var row = Assert.Single(f.Rows);
        Assert.Equal(90000m, row.Amount);
        Assert.Equal(new DateTime(2020, 1, 1), row.EffectiveDate);
        Assert.Null(row.EndDate);
        Assert.Contains("كشف الإحالة رقم 11", row.Notes!);
    }

    [Fact]
    public async Task Settlement_ClosesApprovalAndUsesDecisionDateAndProcessedRank()
    {
        var f = new Fixture();
        f.Settlements.Add(f.Settlement());
        var result = await f.CreateAsync();
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(2, f.Rows.Count);
        Assert.Equal(new DateTime(2025, 12, 31), f.Rows[0].EndDate);
        Assert.Equal(120000m, result.Data!.Amount);
        Assert.Equal(new DateTime(2026, 1, 1), result.Data.EffectiveDate);
        Assert.Null(result.Data.EndDate);
        f.Unit.Verify(u => u.LockRetireeEntitlementsAsync(2), Times.Once);
    }

    [Fact]
    public async Task MultipleSettlements_ApplyChronologicallyAndRetryWithoutWrites()
    {
        var f = new Fixture();
        f.Settlements.Add(f.Settlement());
        var older = f.Settlement();
        older.SettlementItemID = 4;
        older.CreatedAt = DateTime.UtcNow.AddDays(1);
        older.Decision!.DecisionDate = new DateTime(2025, 1, 1);
        older.ProcessedRank.BaseSalary = 100000;
        f.Settlements.Add(older);
        var result = await f.CreateAsync();
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(new[] { 90000m, 100000m, 120000m }, f.Rows.Select(e => e.Amount));
        Assert.Equal(new DateTime(2024, 12, 31), f.Rows[0].EndDate);
        Assert.Equal(new DateTime(2025, 12, 31), f.Rows[1].EndDate);
        f.Unit.Invocations.Clear();
        f.Audit.Invocations.Clear();
        var retry = await f.CreateAsync();
        Assert.True(retry.IsSuccess, retry.Message);
        Assert.Equal(result.Data!.EntitlementID, retry.Data!.EntitlementID);
        Assert.Equal(3, f.Rows.Count);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        Assert.Empty(f.Audit.Invocations);
    }

    [Theory]
    [InlineData("deleted")]
    [InlineData("decision-deleted")]
    [InlineData("batch-deleted")]
    [InlineData("foreign")]
    public async Task InvalidOrForeignSettlement_DoesNotReplaceApproval(string reason)
    {
        var f = new Fixture();
        var s = f.Settlement();
        if (reason == "deleted") s.IsDeleted = true;
        if (reason == "decision-deleted") s.Decision!.IsDeleted = true;
        if (reason == "batch-deleted") s.Batch = new SettlementsBatch { IsDeleted = true };
        if (reason == "foreign") { s.RetireeID = 999; s.ApprovalItemID = 11; }
        f.Settlements.Add(s);
        Assert.True((await f.CreateAsync()).IsSuccess);
        Assert.Equal(90000m, Assert.Single(f.Rows).Amount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UnlinkedSettlement_UsesApprovalOrNormalizedMilitary(bool approval)
    {
        var f = new Fixture();
        var s = f.Settlement();
        s.RetireeID = null;
        if (approval) s.ApprovalItemID = 11;
        else { f.Retiree.MilitaryNumber = "٤٠٠٠٥٨٥"; s.MilitaryNumber = "4000585"; }
        f.Settlements.Add(s);
        var result = await f.CreateAsync();
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(120000m, result.Data!.Amount);
    }

    [Theory]
    [InlineData("unprocessed")]
    [InlineData("salary-null")]
    [InlineData("salary-zero")]
    [InlineData("salary-high")]
    [InlineData("date-null")]
    [InlineData("date-default")]
    [InlineData("settlement-date")]
    [InlineData("settlement-before")]
    [InlineData("settlement-same")]
    [InlineData("settlement-salary")]
    [InlineData("duplicate-source")]
    public async Task InvalidSources_FailBeforeAnyWrite(string reason)
    {
        var f = new Fixture();
        var s = f.Settlement();
        f.Settlements.Add(s);
        var approval = f.Retiree.ApprovalItem;
        if (reason == "unprocessed") approval.IsProcessed = false;
        if (reason == "salary-null") approval.Salary = null;
        if (reason == "salary-zero") approval.Salary = 0;
        if (reason == "salary-high") approval.Salary = 10000001;
        if (reason == "date-null") approval.RetirementDate = null;
        if (reason == "date-default") approval.RetirementDate = default(DateOnly);
        if (reason == "settlement-date") s.Decision!.DecisionDate = default;
        if (reason == "settlement-before") s.Decision!.DecisionDate = new DateTime(2019, 1, 1);
        if (reason == "settlement-same") s.Decision!.DecisionDate = new DateTime(2020, 1, 1);
        if (reason == "settlement-salary") s.ProcessedRank.BaseSalary = 0;
        if (reason == "duplicate-source") f.Settlements.Add(s);
        Assert.False((await f.CreateAsync()).IsSuccess);
        Assert.Empty(f.Rows);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData("conflict")]
    [InlineData("duplicate")]
    [InlineData("invalid-period")]
    public async Task InvalidHistory_IsNotRewritten(string reason)
    {
        var f = new Fixture();
        var row = new FinancialEntitlement { RetireeID = 2, Amount = reason == "conflict" ? 1 : 90000,
            EffectiveDate = new DateTime(2020, 1, 1),
            EndDate = reason == "invalid-period" ? new DateTime(2019, 1, 1) : null };
        f.Rows.Add(row);
        if (reason == "duplicate") f.Rows.Add(new FinancialEntitlement
            { RetireeID = 2, Amount = 90000, EffectiveDate = row.EffectiveDate });
        var oldEnd = row.EndDate;
        Assert.False((await f.CreateAsync()).IsSuccess);
        Assert.Equal(oldEnd, row.EndDate);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EndedHistory_IsNotReopenedOrShownAsActiveApproval()
    {
        var f = new Fixture();
        f.Settlements.Add(f.Settlement());
        f.Rows.Add(new FinancialEntitlement { RetireeID = 2, Amount = 90000,
            EffectiveDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2025, 12, 31) });
        f.Rows.Add(new FinancialEntitlement { RetireeID = 2, Amount = 120000,
            EffectiveDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 3, 1) });
        Assert.True((await f.CreateAsync()).IsSuccess);
        Assert.Equal(new DateTime(2026, 3, 1), f.Rows[1].EndDate);
        Assert.Null((await f.Service.GetActiveByRetireeIdAsync(2)).Data);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task NestedFailure_LeavesRollbackToCaller()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Unit.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new IOException("فشل الحفظ"));
        Assert.False((await f.CreateAsync()).IsSuccess);
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileOpening_UsesRetirementAndDecisionDatesAndReturnsOpeningSourceRank(bool settlement)
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2, StatusID = 1,
            FileOpenDate = new DateTime(2026, 10, 22, 22, 30, 0, DateTimeKind.Utc) });
        if (settlement) f.Settlements.Add(f.Settlement());
        var result = await f.Service.CreateForFileAsync(1, 7);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(settlement ? 2 : 1, f.Rows.Count);
        Assert.Equal(new DateTime(2020, 1, 1), f.Rows[0].EffectiveDate);
        if (settlement) Assert.Equal(new DateTime(2025, 12, 31), f.Rows[0].EndDate);
        Assert.Equal(settlement ? new DateTime(2026, 1, 1) : new DateTime(2020, 1, 1), result.Data!.EffectiveDate);
        Assert.Equal(settlement ? 120000m : 90000m, result.Data!.Amount);
        Assert.Equal(settlement ? 3 : 1, result.Data.RankID);
        Assert.Null(f.Rows[^1].EndDate);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task FileOpening_FutureSettlementDoesNotSelectFutureRankForOpening()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2, StatusID = 1,
            FileOpenDate = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc) });
        var future = f.Settlement();
        future.Decision!.DecisionDate = new DateTime(2027, 1, 1);
        f.Settlements.Add(future);
        var result = await f.Service.CreateForFileAsync(1, 7);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(2, f.Rows.Count);
        Assert.Equal(90000m, result.Data!.Amount);
        Assert.Equal(1, result.Data.RankID);
        Assert.Equal(new DateTime(2020, 1, 1), f.Rows[0].EffectiveDate);
        Assert.Equal(new DateTime(2026, 12, 31), f.Rows[0].EndDate);
        Assert.Equal(new DateTime(2027, 1, 1), f.Rows[1].EffectiveDate);
    }

    [Fact]
    public async Task FileOpening_PreservesSourcePeriodsAndDoesNotDuplicateOnRetry()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2, StatusID = 1,
            FileOpenDate = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc) });
        f.Settlements.Add(f.Settlement());
        f.Rows.Add(new FinancialEntitlement { RetireeID = 2, Amount = 90000,
            EffectiveDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2025, 12, 31) });
        f.Rows.Add(new FinancialEntitlement { RetireeID = 2, Amount = 120000, EffectiveDate = new DateTime(2026, 1, 1) });
        var first = await f.Service.CreateForFileAsync(1, 7);
        Assert.True(first.IsSuccess, first.Message);
        Assert.Null(f.Rows[1].EndDate);
        Assert.Equal(2, f.Rows.Count);
        f.Unit.Invocations.Clear();
        var retry = await f.Service.CreateForFileAsync(1, 7);
        Assert.True(retry.IsSuccess, retry.Message);
        Assert.Equal(2, f.Rows.Count);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task FileOpening_UsesYemenDayToSelectRankWithoutShiftingSourceDates()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2, StatusID = 1,
            FileOpenDate = new DateTime(2026, 10, 22, 22, 30, 0, DateTimeKind.Utc) });
        var settlement = f.Settlement();
        settlement.Decision!.DecisionDate = new DateTime(2026, 10, 23);
        f.Settlements.Add(settlement);
        var result = await f.Service.CreateForFileAsync(1, 7);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(3, result.Data!.RankID);
        Assert.Equal(new DateTime(2026, 10, 23), result.Data.EffectiveDate);
        Assert.Equal(new DateTime(2020, 1, 1), f.Rows[0].EffectiveDate);
        Assert.Equal(new DateTime(2026, 10, 22), f.Rows[0].EndDate);
    }

    [Fact]
    public async Task FileOpening_RejectsExistingOpeningDatePeriodWithoutRewritingHistory()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2, StatusID = 1,
            FileOpenDate = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc) });
        f.Settlements.Add(f.Settlement());
        var existing = new FinancialEntitlement { RetireeID = 2, Amount = 120000,
            EffectiveDate = new DateTime(2026, 10, 23) };
        f.Rows.Add(existing);
        var result = await f.Service.CreateForFileAsync(1, 7);
        Assert.False(result.IsSuccess);
        Assert.Same(existing, Assert.Single(f.Rows));
        Assert.Null(existing.EndDate);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DirectCreation_WithExistingFileStillStartsAtRetirement()
    {
        var f = new Fixture();
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2,
            FileOpenDate = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc) });
        f.Settlements.Add(f.Settlement());
        var result = await f.CreateAsync();
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(2, f.Rows.Count);
        Assert.Equal(new DateTime(2020, 1, 1), f.Rows[0].EffectiveDate);
        Assert.Equal(new DateTime(2026, 1, 1), result.Data!.EffectiveDate);
    }

    [Fact]
    public async Task FileOpening_RequiresActiveTransaction()
    {
        var f = new Fixture();
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.CreateForFileAsync(1, 7));
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task FileOpening_RejectsForeignBranchBeforeReadingFinancialData()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2, PensionBranchID = 2, StatusID = 1 });
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(s => s.IsHQ()).Returns(false);
        identity.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var service = new FinancialEntitlementService(f.Unit.Object, Mock.Of<IMissingDocumentService>(),
            Mock.Of<INotificationService>(), f.Audit.Object, NullLogger<FinancialEntitlementService>.Instance,
            TestPensionAccess.From(f.Unit.Object, identity.Object));
        Assert.False((await service.CreateForFileAsync(1, 7)).IsSuccess);
        f.Unit.Verify(u => u.LockRetireeEntitlementsAsync(It.IsAny<int>()), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task FileOpening_RejectsAlreadyActivatedFile()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        f.Files.Add(new RetirementFile { FileID = 1, RetireeID = 2, StatusID = 2 });
        Assert.False((await f.Service.CreateForFileAsync(1, 7)).IsSuccess);
        f.Unit.Verify(u => u.LockRetireeEntitlementsAsync(It.IsAny<int>()), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("enrollment")]
    [InlineData("equal")]
    [InlineData("earlier")]
    [InlineData("opening-missing")]
    [InlineData("opening-before")]
    [InlineData("rank-missing")]
    public async Task FileOpening_CannotBypassOpeningRulesOrSourceRank(string scenario)
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(true);
        var file = new RetirementFile { FileID = 1, RetireeID = 2, StatusID = 1,
            FileOpenDate = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc) };
        f.Files.Add(file);
        if (scenario == "enrollment") f.Retiree.ApprovalItem.EnrollmentDate = null;
        if (scenario == "equal") f.Retiree.ApprovalItem.EnrollmentDate = f.Retiree.ApprovalItem.RetirementDate;
        if (scenario == "earlier") f.Retiree.ApprovalItem.EnrollmentDate = new DateOnly(2021, 1, 1);
        if (scenario == "opening-missing") file.FileOpenDate = default;
        if (scenario == "opening-before") file.FileOpenDate = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        if (scenario == "rank-missing") f.Retiree.ApprovalItem.RankID = 999;
        var result = await f.Service.CreateForFileAsync(1, 7);
        Assert.False(result.IsSuccess);
        Assert.Empty(f.Rows);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Mock<IImmutableAuditService> Audit { get; } = new();
        public List<FinancialEntitlement> Rows { get; } = [];
        public List<SettlementItem> Settlements { get; } = [];
        public List<RetirementFile> Files { get; } = [];
        public Retiree Retiree { get; } = new() { RetireeID = 2, ApprovalItemID = 11,
            ApprovalItem = new MinistryApprovalItem { ApprovalItemID = 11, IsProcessed = true,
                Salary = 90000, RankID = 1, EnrollmentDate = new DateOnly(1980, 1, 1),
                RetirementDate = new DateOnly(2020, 1, 1) } };
        public FinancialEntitlementService Service { get; }
        public Fixture()
        {
            var retirees = Repo(new List<Retiree> { Retiree });
            var financial = Repo(Rows);
            financial.Setup(r => r.AddAsync(It.IsAny<FinancialEntitlement>())).Callback<FinancialEntitlement>(row =>
                { row.EntitlementID = Rows.Count + 1; Rows.Add(row); }).Returns(Task.CompletedTask);
            Unit.Setup(u => u.Retirees).Returns(retirees.Object);
            Unit.Setup(u => u.FinancialEntitlements).Returns(financial.Object);
            Unit.Setup(u => u.SettlementItems).Returns(Repo(Settlements).Object);
            Unit.Setup(u => u.RetirementFiles).Returns(Repo(Files).Object);
            Unit.Setup(u => u.Ranks).Returns(Repo(new List<Rank> { new() { RankID = 1 }, new() { RankID = 3 } }).Object);
            Unit.Setup(u => u.AuditLogs).Returns(Repo(new List<AuditLog>()).Object);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Service = new FinancialEntitlementService(Unit.Object, Mock.Of<IMissingDocumentService>(),
                Mock.Of<INotificationService>(), Audit.Object, NullLogger<FinancialEntitlementService>.Instance,
                TestPensionAccess.All(Unit.Object));
        }
        public Task<Core.DTOs.Common.ApiResponse<FinancialEntitlementDto>> CreateAsync() => Service.CreateAsync(
            new CreateFinancialEntitlementDto { RetireeID = 2, Amount = 1, EffectiveDate = new DateTime(2030, 1, 1) }, 7);
        public SettlementItem Settlement() => new() { SettlementItemID = 3, RetireeID = 2,
            ProcessedRankID = 3, ProcessedRank = new Rank { RankID = 3, BaseSalary = 120000 },
            Decision = new SettlementDecision { DecisionNumber = "42", DecisionDate = new DateTime(2026, 1, 1) } };
        private static Mock<IRepository<T>> Repo<T>(List<T> rows) where T : class
        {
            var result = new Mock<IRepository<T>>();
            result.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows));
            return result;
        }
    }
}
