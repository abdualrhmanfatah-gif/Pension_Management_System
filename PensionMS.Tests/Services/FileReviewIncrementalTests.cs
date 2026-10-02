using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services;

namespace PensionMS.Tests.Services;

public class FileReviewIncrementalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RequestReview_RespectsTransactionOwnership(bool callerOwnsTransaction)
    {
        var f = new Fixture();
        f.Unit.SetupGet(u => u.HasActiveTransaction).Returns(callerOwnsTransaction);
        await f.Service.RequestReviewAsync(1, 7);
        f.Unit.Verify(u => u.LockFileReviewAsync(1), Times.Once);
        f.Unit.Verify(u => u.BeginTransactionAsync(), callerOwnsTransaction ? Times.Never() : Times.Once());
        f.Unit.Verify(u => u.CommitAsync(), callerOwnsTransaction ? Times.Never() : Times.Once());
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RequestReview_Exception_RollsBackOnlyOwnedTransaction(bool callerOwnsTransaction)
    {
        var f = new Fixture();
        f.Unit.SetupGet(u => u.HasActiveTransaction).Returns(callerOwnsTransaction);
        f.Unit.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new InvalidOperationException("test"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.RequestReviewAsync(1, 7));
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), callerOwnsTransaction ? Times.Never() : Times.Once());
    }

    [Theory]
    [InlineData("request")]
    [InlineData("approve")]
    [InlineData("reject")]
    [InlineData("approve-item")]
    [InlineData("reject-item")]
    [InlineData("resolve")]
    public async Task ReviewMutations_WaitForFileLockBeforeChangingState(string operation)
    {
        var f = new Fixture();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Unit.Setup(u => u.LockFileReviewAsync(1)).Returns(gate.Task);
        Task action = operation switch {
            "request" => f.Service.RequestReviewAsync(1, 7),
            "approve" => f.Service.ApproveReviewAsync(1, 7),
            "reject" => f.Service.RejectReviewAsync(1, 7, "سبب"),
            "approve-item" => f.Service.ApproveItemAsync(10, 7),
            "reject-item" => f.Service.RejectItemAsync(10, 7, "سبب"),
            _ => f.Service.AutoResolveFileReviewAsync(1)
        };
        Assert.False(action.IsCompleted);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        // Abort the gate instead of mutating domain state: this test targets admission ordering.
        gate.SetException(new InvalidOperationException("test lock interrupted"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => action);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task RequestReview_RechecksFileStateAfterLock()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.RetirementFiles.ReloadAsync(f.File)).Callback(() =>
            f.File.StatusID = FileStatusConstants.Closed).Returns(Task.CompletedTask);
        var result = await f.Service.RequestReviewAsync(1, 7);
        Assert.False(result.IsSuccess);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task RequestReview_OverlappingUploads_KeepOneItemPerAudit()
    {
        var f = new Fixture();
        f.Audits.Add(f.Audit(102, 2));
        using var gate = new SemaphoreSlim(1);
        f.Unit.Setup(u => u.LockFileReviewAsync(1)).Returns(() => gate.WaitAsync());
        f.Unit.Setup(u => u.CommitAsync()).Callback(() => gate.Release()).Returns(Task.CompletedTask);
        f.Unit.Setup(u => u.RollbackAsync()).Callback(() => gate.Release()).Returns(Task.CompletedTask);
        var reachedInsert = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseInsert = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = true;
        f.ReviewItems.Setup(r => r.AddAsync(It.IsAny<FileReviewItem>())).Returns<FileReviewItem>(async item => {
            if (first) { first = false; reachedInsert.SetResult(); await releaseInsert.Task; }
            item.ItemID = 1000 + f.Items.Count;
            f.Items.Add(item);
        });

        var upload1 = f.Service.RequestReviewAsync(1, 7);
        await reachedInsert.Task.WaitAsync(TimeSpan.FromSeconds(3));
        var upload2 = new FileReviewService(f.Unit.Object).RequestReviewAsync(1, 8);
        releaseInsert.SetResult();
        await Task.WhenAll(upload1, upload2).WaitAsync(TimeSpan.FromSeconds(3));

        Assert.Single(f.Items.Where(i => i.AuditLogID == 102));
    }

    [Fact]
    public async Task RequestReview_NewCycleThenSecondUpload_DoesNotExpandAuditWindow()
    {
        var f = new Fixture();
        f.File.ReviewStatus = FileReviewStatus.Approved;
        f.File.StatusID = FileStatusConstants.Active;
        var completed = f.File.ReviewCompletedAt;
        // A historical approved item must not move the new cycle's boundary backwards.
        f.Items[0].Status = FileReviewItemStatusConstants.Approved;
        f.Audits[0].CreatedAt = completed!.Value.AddDays(-10);
        f.Audits.Add(f.Audit(99, 3, completed.Value.AddDays(-1)));
        f.Audits.Add(f.Audit(102, 2));

        await f.Service.RequestReviewAsync(1, 7);
        await f.Service.RequestReviewAsync(1, 7);

        Assert.Equal(completed, f.File.ReviewCompletedAt);
        Assert.DoesNotContain(f.Items, i => i.AuditLogID == 99);
        Assert.Single(f.Items.Where(i => i.AuditLogID == 102));
    }

    [Fact]
    public async Task RequestReview_AddsOnlyNewAudit_PreservesExistingItemAndDecision()
    {
        var f = new Fixture();
        var original = f.Items[0];
        var originalCreated = original.CreatedAt;
        var approved = new FileReviewItem { ItemID = 20, RetirementFileID = 1, AuditLogID = 103,
            EntityType = 6, EntityID = 3, Status = FileReviewItemStatusConstants.Approved, ReviewedByUserID = 7 };
        f.Items.Add(approved);
        f.Audits.Add(f.Audit(103, 3));
        f.Audits.Add(f.Audit(102, 2));

        var result = await f.Service.RequestReviewAsync(1, 7);

        Assert.True(result.IsSuccess);
        Assert.Contains(original, f.Items);
        Assert.Equal(originalCreated, original.CreatedAt);
        Assert.Equal(FileReviewItemStatusConstants.Approved, approved.Status);
        Assert.Equal(7, approved.ReviewedByUserID);
        Assert.Single(f.Items.Where(i => i.AuditLogID == 103));
        Assert.Single(f.Items.Where(i => i.AuditLogID == 102));
        f.ReviewItems.Verify(r => r.Delete(It.IsAny<FileReviewItem>()), Times.Never);
        f.ReviewItems.Verify(r => r.AddAsync(It.IsAny<FileReviewItem>()), Times.Once);
        Assert.Equal(PendingReviewStatus.Pending, f.Documents[1].PendingReviewStatus);
        Assert.Equal(PendingReviewStatus.Approved, f.Documents[2].PendingReviewStatus);
    }

    [Fact]
    public async Task RequestReview_RepeatedCall_DoesNotReinsertOrDeleteItems()
    {
        var f = new Fixture();
        f.Audits.Add(f.Audit(102, 2));
        await f.Service.RequestReviewAsync(1, 7);
        var ids = f.Items.Select(i => i.ItemID).ToArray();

        await f.Service.RequestReviewAsync(1, 7);

        Assert.Equal(ids, f.Items.Select(i => i.ItemID).ToArray());
        f.ReviewItems.Verify(r => r.AddAsync(It.IsAny<FileReviewItem>()), Times.Once);
        f.ReviewItems.Verify(r => r.Delete(It.IsAny<FileReviewItem>()), Times.Never);
    }

    [Fact]
    public async Task RequestReview_RepeatedCall_KeepsLastCompletedBoundary()
    {
        var f = new Fixture();
        var completed = f.File.ReviewCompletedAt;
        f.Audits.Add(f.Audit(99, 3, completed!.Value.AddDays(-1)));
        f.Audits.Add(f.Audit(102, 2));

        await f.Service.RequestReviewAsync(1, 7);
        await f.Service.RequestReviewAsync(1, 7);

        Assert.Equal(completed, f.File.ReviewCompletedAt);
        Assert.DoesNotContain(f.Items, i => i.AuditLogID == 99);
    }

    [Fact]
    public async Task RequestReview_MultiplePendingItems_InsertsOnlyDelta_SameFinalSet()
    {
        var f = new Fixture();
        // Seed 2 pending items; full rebuild would yield {101,102,103} after new doc audit 103.
        f.Audits.Add(f.Audit(102, 2));
        f.Audits.Add(f.Audit(103, 3));
        var before101 = f.Items[0];
        var beforeCreated = before101.CreatedAt;

        var result = await f.Service.RequestReviewAsync(1, 7);

        Assert.True(result.IsSuccess);
        f.ReviewItems.Verify(r => r.Delete(It.IsAny<FileReviewItem>()), Times.Never);
        f.ReviewItems.Verify(r => r.AddAsync(It.IsAny<FileReviewItem>()), Times.Exactly(2));
        var pendingIds = f.Items.Where(i => i.Status == FileReviewItemStatusConstants.Pending)
            .Select(i => i.AuditLogID).OrderBy(x => x).ToArray();
        Assert.Equal(new long?[] { 101, 102, 103 }, pendingIds);
        Assert.Same(before101, f.Items.First(i => i.AuditLogID == 101));
        Assert.Equal(beforeCreated, before101.CreatedAt);
    }

    [Fact]
    public async Task RequestReview_UpdateExistingDocument_NoDuplicateWhenCreateExists()
    {
        var f = new Fixture();
        // Update for doc 1 while its Create is in window: full rebuild filters it out.
        f.Audits.Add(f.AuditUpdate(150, 1, "قديم", "جديد"));

        var result = await f.Service.RequestReviewAsync(1, 7);

        Assert.True(result.IsSuccess);
        f.ReviewItems.Verify(r => r.Delete(It.IsAny<FileReviewItem>()), Times.Never);
        f.ReviewItems.Verify(r => r.AddAsync(It.IsAny<FileReviewItem>()), Times.Never);
        Assert.Single(f.Items.Where(i => i.AuditLogID == 101));
        Assert.DoesNotContain(f.Items, i => i.AuditLogID == 150);
    }

    [Fact]
    public async Task RequestReview_ChangedDocumentWithoutCreate_AddsItsOwnItemOnly()
    {
        var f = new Fixture();
        // Doc 9 has no Create in window; its Update with real field change yields exactly one item.
        f.Documents.Add(new Document { DocumentID = 9, FileID = 1, PendingReviewStatus = PendingReviewStatus.Approved });
        f.Audits.Add(f.AuditUpdate(151, 9, "قديم", "جديد"));

        var result = await f.Service.RequestReviewAsync(1, 7);

        Assert.True(result.IsSuccess);
        f.ReviewItems.Verify(r => r.Delete(It.IsAny<FileReviewItem>()), Times.Never);
        f.ReviewItems.Verify(r => r.AddAsync(It.IsAny<FileReviewItem>()), Times.Once);
        var pendingIds = f.Items.Where(i => i.Status == FileReviewItemStatusConstants.Pending)
            .Select(i => i.AuditLogID).OrderBy(x => x).ToArray();
        Assert.Equal(new long?[] { 101, 151 }, pendingIds);
    }

    [Fact]
    public async Task RequestReview_StaleItem_RemovedOnlyIt_NewInserted()
    {
        var f = new Fixture();
        // Pending item 99 predates last completed boundary: full rebuild drops it.
        var stale = new FileReviewItem { ItemID = 30, RetirementFileID = 1, AuditLogID = 99,
            EntityType = 6, EntityID = 3, FieldName = "رفع وثيقة", Status = FileReviewItemStatusConstants.Pending };
        f.Items.Add(stale);
        f.Audits.Add(f.Audit(99, 3, new DateTime(2025, 12, 31)));
        f.Audits.Add(f.Audit(102, 2));

        var result = await f.Service.RequestReviewAsync(1, 7);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(f.Items, i => i.AuditLogID == 99);
        f.ReviewItems.Verify(r => r.Delete(It.Is<FileReviewItem>(i => i.AuditLogID == 99)), Times.Once);
        f.ReviewItems.Verify(r => r.AddAsync(It.IsAny<FileReviewItem>()), Times.Once);
        Assert.Single(f.Items.Where(i => i.AuditLogID == 101));
        Assert.Single(f.Items.Where(i => i.AuditLogID == 102));
    }

    [Fact]
    public async Task RequestReview_AuditPath_NotBypassed_WritesOnlyThroughSaveChanges()
    {
        var f = new Fixture();
        f.Audits.Add(f.Audit(102, 2));

        var result = await f.Service.RequestReviewAsync(1, 7);

        Assert.True(result.IsSuccess);
        // Review items change only via repository + SaveChanges so AuditSaveChangesInterceptor still sees them.
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        var audits = Mock.Get(f.Unit.Object.AuditLogs);
        audits.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
        audits.Verify(r => r.Update(It.IsAny<AuditLog>()), Times.Never);
        audits.Verify(r => r.Delete(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task RequestReview_PreservesLegacyItemsWithoutAuditLink()
    {
        var f = new Fixture();
        var legacy = new FileReviewItem { ItemID = 25, RetirementFileID = 1, FieldName = "بند قديم" };
        f.Items.Add(legacy);
        f.Audits.Add(f.Audit(102, 2));

        await f.Service.RequestReviewAsync(1, 7);

        Assert.Contains(legacy, f.Items);
        Assert.Equal(25, legacy.ItemID);
    }

    private sealed class Fixture
    {
        public readonly Mock<IUnitOfWork> Unit = new();
        public readonly List<FileReviewItem> Items = new();
        public readonly List<AuditLog> Audits = new();
        public readonly List<Document> Documents = new();
        public readonly Mock<IRepository<FileReviewItem>> ReviewItems;
        public readonly RetirementFile File = new() { FileID = 1, StatusID = FileStatusConstants.UnderReview,
            ReviewStatus = FileReviewStatus.PendingReview, CreatedAt = new DateTime(2025, 1, 1),
            ReviewCompletedAt = new DateTime(2026, 1, 1) };
        public FileReviewService Service { get; }

        public Fixture()
        {
            Items.Add(new FileReviewItem { ItemID = 10, RetirementFileID = 1, AuditLogID = 101,
                EntityType = 6, EntityID = 1, FieldName = "رفع وثيقة", CreatedAt = new DateTime(2026, 2, 1) });
            Audits.Add(Audit(101, 1));
            for (var id = 1; id <= 3; id++) Documents.Add(new Document {
                DocumentID = id, FileID = 1, PendingReviewStatus = PendingReviewStatus.Approved });
            ReviewItems = Repo(Items);
            ReviewItems.Setup(r => r.Delete(It.IsAny<FileReviewItem>())).Callback<FileReviewItem>(item => Items.Remove(item));
            ReviewItems.Setup(r => r.AddAsync(It.IsAny<FileReviewItem>())).Callback<FileReviewItem>(item => {
                item.ItemID = 1000 + Items.Count; Items.Add(item);
            }).Returns(Task.CompletedTask);
            Unit.Setup(u => u.RetirementFiles).Returns(Repo(new[] { File }).Object);
            Unit.Setup(u => u.FileReviewItems).Returns(ReviewItems.Object);
            Unit.Setup(u => u.AuditLogs).Returns(Repo(Audits).Object);
            Unit.Setup(u => u.Documents).Returns(Repo(Documents).Object);
            Unit.Setup(u => u.EntitledPersons).Returns(Repo(Array.Empty<EntitledPerson>()).Object);
            Unit.Setup(u => u.Agents).Returns(Repo(Array.Empty<Agent>()).Object);
            Unit.Setup(u => u.Guardians).Returns(Repo(Array.Empty<Guardian>()).Object);
            Unit.Setup(u => u.PaymentBooks).Returns(Repo(Array.Empty<PaymentBook>()).Object);
            Unit.Setup(u => u.SalaryRecipients).Returns(Repo(Array.Empty<SalaryRecipient>()).Object);
            Unit.Setup(u => u.SocialStatuses).Returns(Repo(Array.Empty<SocialStatus>()).Object);
            Unit.Setup(u => u.HealthStatuses).Returns(Repo(Array.Empty<HealthStatus>()).Object);
            Unit.Setup(u => u.FileStatuses).Returns(Repo(Array.Empty<FileStatus>()).Object);
            Unit.Setup(u => u.Ranks).Returns(Repo(Array.Empty<Rank>()).Object);
            Unit.Setup(u => u.RetireeCategories).Returns(Repo(Array.Empty<RetireeCategory>()).Object);
            Unit.Setup(u => u.PensionFundBranches).Returns(Repo(Array.Empty<PensionFundBranch>()).Object);
            Unit.Setup(u => u.Relationships).Returns(Repo(Array.Empty<Relationship>()).Object);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Service = new FileReviewService(Unit.Object);
        }
        public AuditLog Audit(long id, int documentId, DateTime? at = null) => new() {
            LogID = id, TableID = 6, RecordID = documentId, Action = "Create",
            CreatedAt = at ?? new DateTime(2026, 2, 1), NewValues = "{\"FileName\":\"وثيقة\"}" };
        public AuditLog AuditUpdate(long id, int documentId, string oldName, string newName, DateTime? at = null) => new() {
            LogID = id, TableID = 6, RecordID = documentId, Action = "Update",
            CreatedAt = at ?? new DateTime(2026, 2, 1),
            OldValues = "{\"FileName\":\"" + oldName + "\"}",
            NewValues = "{\"FileName\":\"" + newName + "\"}" };
        private static Mock<IRepository<T>> Repo<T>(IEnumerable<T> values) where T : class
        {
            var repo = new Mock<IRepository<T>>();
            repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(values));
            return repo;
        }
    }
}
