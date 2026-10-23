using PensionMS.Infrastructure.Services.Settlements;
using System.Linq.Expressions;
using Moq;
using OfficeOpenXml;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Settlements;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Tests.Services;

/// <summary>
/// اختبارات سلامة مالية لكشف التسوية: بوابة الاعتماد، منع الحذف بعد الاعتماد،
/// واشتقاق الإجماليات من البنود الفعلية بدل += / -=.
/// </summary>
public class SettlementBatchFinancialSafetyTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISettlementRepository> _settlements = new();
    private readonly Mock<IJournalEntryService> _journalEntry = new();
    private readonly Mock<IImmutableAuditService> _audit = new();
    private readonly List<SettlementsBatch> _batches = new();
    private readonly List<SettlementItem> _items = new();
    private readonly List<SettlementDecision> _decisions = new();

    public SettlementBatchFinancialSafetyTests()
    {
        Query(u => u.SettlementsBatches, _batches);
        // يحاكي Global Query Filter على ISoftDeletable (PensionDbContext.cs:286-296):
        // البنود المنطقية المحذوفة لا تظهر في الاستعلامات.
        Query(u => u.SettlementItems, () => _items.Where(i => !i.IsDeleted));
        Query(u => u.SettlementDecisions, _decisions);

        var batchesRepo = _unitOfWork.Object.SettlementsBatches;
        _unitOfWork.Setup(u => u.SettlementsBatches.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => _batches.FirstOrDefault(b => b.BatchID == id));
        _unitOfWork.Setup(u => u.SettlementsBatches.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _batches.FirstOrDefault(b => b.BatchNumber == id));

        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _unitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.HasActiveTransaction).Returns(false);

        _journalEntry.Setup(j => j.GetPeriodIdForDateAsync(It.IsAny<DateTime>())).ReturnsAsync(1);
        _journalEntry.Setup(j => j.CreateIntegrationEntryAsync(It.IsAny<FinancialOperationRequestDto>(), It.IsAny<int>()))
            .ReturnsAsync((FinancialOperationRequestDto r, int _) =>
                new FinancialOperationResultDto { IsSuccess = true, EntryNumber = "JE-1", TotalDebit = r.Amount, TotalCredit = r.Amount });
    }

    // ── البوابة: لا استيراد بعد الاعتماد ──

    [Theory]
    [InlineData(BatchStatusConstants.Approved)]
    [InlineData(BatchStatusConstants.LegacyApproved)]
    [InlineData(BatchStatusConstants.Completed)]
    public async Task Import_OnFinanciallyLockedBatch_IsRejected(string status)
    {
        var batch = Batch(50, status, totalAmount: 750000m, totalCount: 5);

        var result = await Service().ImportExcelAsync(50, Excel("قرار-1"), 7);

        Assert.False(result.IsSuccess);
        Assert.Contains("لا يمكن استيراد بيانات إضافية", result.Message);
        Assert.Equal(750000m, batch.TotalAmount);
        Assert.Equal(5, batch.TotalCount);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        Assert.Empty(_items);
        _journalEntry.Verify(j => j.CreateIntegrationEntryAsync(It.IsAny<FinancialOperationRequestDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Import_OnNewBatch_IsAllowed()
    {
        Batch(51, BatchStatusConstants.New, 0m, 0);

        // لا تحقق من صحة الملف هنا — الفشل يأتي بعد البوابة، وهذا يثبت أن البوابة لم ترفض.
        var result = await Service().ImportExcelAsync(51, Excel("قرار-2"), 7);

        Assert.DoesNotContain("لا يمكن استيراد بيانات إضافية", result.Message ?? "");
    }

    // ── الاعتماد ──

    [Fact]
    public async Task Approve_OnNewBatch_IsRejected_BecauseNothingImported()
    {
        Batch(52, BatchStatusConstants.New, 0m, 0);

        var result = await Service().ApproveBatchAsync(52, 7);

        Assert.False(result.IsSuccess);
        _journalEntry.Verify(j => j.CreateIntegrationEntryAsync(It.IsAny<FinancialOperationRequestDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Approve_WithOpenFailures_IsRejected()
    {
        var batch = Batch(53, BatchStatusConstants.Imported, 300000m, 2);
        batch.FailureReportJson = "[]";

        var result = await Service().ApproveBatchAsync(53, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal(BatchStatusConstants.Imported, batch.Status);
        _journalEntry.Verify(j => j.CreateIntegrationEntryAsync(It.IsAny<FinancialOperationRequestDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Approve_OnCleanImportedBatch_Succeeds_AndPostsAmount()
    {
        var batch = Batch(54, BatchStatusConstants.Imported, 300000m, 2);
        batch.FailureReportJson = null;
        batch.UpdatedAt = DateTime.UtcNow;

        var result = await Service().ApproveBatchAsync(54, 7);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(BatchStatusConstants.Approved, batch.Status);
        _journalEntry.Verify(j => j.CreateIntegrationEntryAsync(
            It.Is<FinancialOperationRequestDto>(r => r.Amount == 300000m && r.SourceReferenceID == 54), 7), Times.Once);
    }

    // ── الحذف ممنوع بعد الاعتماد ──

    [Theory]
    [InlineData(BatchStatusConstants.Approved)]
    [InlineData(BatchStatusConstants.LegacyApproved)]
    [InlineData(BatchStatusConstants.Completed)]
    public async Task Delete_OnApprovedBatch_IsRejected(string status)
    {
        var batch = Batch(60, status, 750000m, 5);
        Item(900, batch, processedRankId: 2, salary: 750000m);

        var result = await Service().DeleteBatchAsync(60, 7);

        Assert.False(result.IsSuccess);
        Assert.Contains("لا يمكن حذف كشف معتمد", result.Message);
        Assert.False(batch.IsDeleted);
        Assert.False(_items[0].IsDeleted);
    }

    [Fact]
    public async Task Delete_OnImportedBatch_RecalculatesTotalsFromItems()
    {
        var batch = Batch(61, BatchStatusConstants.Imported, 900000m, 2);
        var kept = Item(901, batch, processedRankId: 2, salary: 400000m);
        var removed = Item(902, batch, processedRankId: 3, salary: 500000m);

        var result = await Service().DeleteBatchAsync(61, 7);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(batch.IsDeleted);
        Assert.True(kept.IsDeleted);
        Assert.True(removed.IsDeleted);
        // TotalAmount لا ينقص في حذف الكشف — الكشف محذوف ككل.
        Assert.Equal(900000m, batch.TotalAmount);
        // كل الوسائط تُمرَّر صراحة: شجرة التعبير لا تقبل وسائط اختيارية مُهمَلة.
        _audit.Verify(a => a.LogAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDecision_OnBatchApprovedByLink_IsRejected()
    {
        var batch = Batch(70, BatchStatusConstants.Approved, 120000m, 1);
        var decision = new SettlementDecision { DecisionID = 500, DecisionNumber = "ق-9", CreatedAt = DateTime.UtcNow };
        _decisions.Add(decision);
        var item = Item(910, batch, processedRankId: 1, salary: 120000m);
        item.DecisionID = decision.DecisionID;
        item.Decision = decision;
        decision.SettlementItems.Add(item);

        var result = await Service().DeleteDecisionAsync(decision.DecisionID, 7);

        Assert.False(result.IsSuccess);
        Assert.Contains("لا يمكن حذف قرار مرتبط بكشف معتمد", result.Message);
        Assert.False(decision.IsDeleted);
        Assert.False(item.IsDeleted);
    }

    [Fact]
    public async Task DeleteDecision_OnEditableBatch_SoftDeletesItems_AndRecalculatesBatchTotals()
    {
        var batch = Batch(71, BatchStatusConstants.Imported, 900000m, 2);
        var decision = new SettlementDecision { DecisionID = 501, DecisionNumber = "ق-10", CreatedAt = DateTime.UtcNow };
        _decisions.Add(decision);

        var kept = Item(920, batch, processedRankId: 2, salary: 400000m);
        var removed = Item(921, batch, processedRankId: 3, salary: 500000m);
        removed.DecisionID = decision.DecisionID;
        removed.Decision = decision;
        decision.SettlementItems.Add(removed);

        var result = await Service().DeleteDecisionAsync(decision.DecisionID, 7);

        Assert.True(result.IsSuccess, result.Message);
        Assert.True(decision.IsDeleted);
        Assert.True(removed.IsDeleted);
        Assert.False(kept.IsDeleted);
        // اشتقاق من البنود غير المحذوفة: 1 بند، 400000 — لا 900000 القديم.
        Assert.Equal(1, batch.TotalCount);
        Assert.Equal(400000m, batch.TotalAmount);
    }

    // ── Helpers ──

    private SettlementService Service() => SettlementServiceFactory.Create(
        _unitOfWork, settlements: _settlements, journalEntry: _journalEntry, audit: _audit);

    private SettlementsBatch Batch(int id, string status, decimal totalAmount, int totalCount)
    {
        var batch = new SettlementsBatch
        {
            BatchID = id,
            BatchNumber = $"BATCH-{id}",
            Status = status,
            TotalAmount = totalAmount,
            TotalCount = totalCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _batches.Add(batch);
        return batch;
    }

    private SettlementItem Item(int id, SettlementsBatch batch, int processedRankId, decimal salary)
    {
        var item = new SettlementItem
        {
            SettlementItemID = id,
            BatchID = batch.BatchID,
            Batch = batch,
            MilitaryNumber = $"M-{id}",
            FullName = $"اسم {id}",
            CurrentRankID = 1,
            ProcessedRankID = processedRankId,
            ProcessedRank = new Rank { RankID = processedRankId, BaseSalary = salary },
            Amount = salary,
            CreatedAt = DateTime.UtcNow
        };
        batch.SettlementItems.Add(item);
        _items.Add(item);
        return item;
    }

    /// <summary>ملف xlsx صالح بسطر بيانات واحد — يسمح للاختبار بتجاوز فحص الملف.</summary>
    private static ImportSettlementExcelDto Excel(string decisionNumber)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("كشف");
        ws.Cells[1, 1].Value = 1;
        ws.Cells[1, 2].Value = "4000585";
        ws.Cells[1, 3].Value = "رتبة حالية";
        ws.Cells[1, 4].Value = "اسم";
        ws.Cells[1, 5].Value = "رتبة معالجة";
        ws.Cells[2, 1].Value = 2;
        ws.Cells[2, 2].Value = "4000586";
        ws.Cells[2, 3].Value = "رتبة حالية";
        ws.Cells[2, 4].Value = "اسم ثاني";
        ws.Cells[2, 5].Value = "رتبة معالجة";

        return new ImportSettlementExcelDto
        {
            File = new MemoryRemoteFile(package.GetAsByteArray(), "settlements.xlsx"),
            DecisionNumber = decisionNumber
        };
    }

    private sealed class MemoryRemoteFile : IRemoteFile
    {
        private readonly byte[] _bytes;
        public MemoryRemoteFile(byte[] bytes, string fileName) { _bytes = bytes; FileName = fileName; }
        public string FileName { get; }
        public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public long Length => _bytes.LongLength;
        public Stream OpenReadStream() => new MemoryStream(_bytes, writable: false);
    }

    private Mock<IRepository<T>> Query<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, IEnumerable<T> data) where T : class
    {
        var repo = new Mock<IRepository<T>>();
        repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(data));
        _unitOfWork.Setup(property).Returns(repo.Object);
        return repo;
    }

    private Mock<IRepository<T>> Query<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, Func<IEnumerable<T>> data) where T : class
    {
        var repo = new Mock<IRepository<T>>();
        repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(data()));
        _unitOfWork.Setup(property).Returns(repo.Object);
        return repo;
    }
}