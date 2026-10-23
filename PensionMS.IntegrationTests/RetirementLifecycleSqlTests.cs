using PensionMS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Infrastructure.Services.Pension.Entitlements;

namespace PensionMS.IntegrationTests;

public class RetirementLifecycleSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task SharedRetireeEdit_HoldsEveryAffectedFile_PreservesApprovedAmountsAndHistory()
    {
        var seeded = await SeedAsync(FileStatusConstants.Active, secondFile: true);
        await using (var db = database.OpenContext())
        {
            var retiree = await db.Retirees.SingleAsync(r => r.RetireeID == seeded.RetireeID);
            retiree.Phone = "777111222";
            retiree.UpdatedByUserID = seeded.UserID;
            await db.SaveChangesAsync();
        }
        await using (var db = database.OpenContext())
        {
            var files = await db.RetirementFiles.Where(f => f.RetireeID == seeded.RetireeID).ToListAsync();
            Assert.Equal(2, files.Count);
            Assert.All(files, f => Assert.Equal(FileStatusConstants.Pending, f.StatusID));
            Assert.All(files, f => Assert.Equal("777000111", ApprovedPensionSnapshot.Read(f.LastReviewedFileState)!.Retiree.Phone));
            Assert.Equal(2, await db.PensionSuspensions.CountAsync(s => s.RetirementFile.RetireeID == seeded.RetireeID && s.Status == SuspensionStatusConstants.Active));
            Assert.Equal(2, await db.FileReviewItems.CountAsync(i => i.RetirementFile.RetireeID == seeded.RetireeID));
            var retiree = await db.Retirees.FindAsync(seeded.RetireeID);
            retiree!.Phone = "777333444";
            await db.SaveChangesAsync();
            Assert.Equal(2, await db.PensionSuspensions.CountAsync(s => s.RetirementFile.RetireeID == seeded.RetireeID && s.Status == SuspensionStatusConstants.Active));
            Assert.Equal(4, await db.FileReviewItems.CountAsync(i => i.RetirementFile.RetireeID == seeded.RetireeID));
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            var generated = await new PayrollGenerationService(uow).GenerateForBatchAsync(
                new SalaryPaymentBatch { BatchID = 99, BatchMonth = YemenTime.TodayDateTime }, DateTime.UtcNow, seeded.UserID);
            var items = generated.GeneratedItems.Where(i => i.RetireeID == seeded.RetireeID).ToList();
            Assert.All(items, item => Assert.Equal(1200, item.Amount));
            Assert.Equal(2, items.Count);
        }
    }

    [Fact]
    public async Task DraftSubmission_Approval_IndependentSuspension_AndRejectedCorrection()
    {
        var seeded = await SeedAsync(FileStatusConstants.New);
        await using (var db = database.OpenContext())
        {
            var retiree = await db.Retirees.FindAsync(seeded.RetireeID);
            retiree!.Phone = "777222333";
            await db.SaveChangesAsync();
            Assert.Equal(FileStatusConstants.New, (await db.RetirementFiles.FindAsync(seeded.FileID))!.StatusID);
            Assert.False(await db.PensionSuspensions.AnyAsync(s => s.FileID == seeded.FileID));
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            var service = Review(uow);
            Assert.True((await service.SubmitReviewAsync(seeded.FileID, seeded.UserID)).IsSuccess);
            await db.PensionSuspensions.AddAsync(new PensionSuspension
            {
                FileID = seeded.FileID, CreatedByUserID = seeded.UserID, SuspensionSource = "إداري",
                SuspensionReason = "سبب مستقل", Status = SuspensionStatusConstants.Active, SuspensionDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            var approved = await service.ApproveReviewAsync(seeded.FileID, seeded.UserID);
            Assert.True(approved.IsSuccess, approved.Message);
            Assert.Equal(FileStatusConstants.Active, (await db.RetirementFiles.FindAsync(seeded.FileID))!.StatusID);
            Assert.Single(await db.PensionSuspensions.Where(s => s.FileID == seeded.FileID && s.Status == SuspensionStatusConstants.Active).ToListAsync());
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            var retiree = await db.Retirees.FindAsync(seeded.RetireeID);
            retiree!.Phone = "777444555";
            retiree.UpdatedByUserID = seeded.UserID;
            await db.SaveChangesAsync();
            Assert.True((await Review(uow).RejectReviewAsync(seeded.FileID, seeded.UserID, "البيانات غير صحيحة")).IsSuccess);
            Assert.Equal(FileReviewStatus.Rejected, (await db.RetirementFiles.FindAsync(seeded.FileID))!.ReviewStatus);
            Assert.Equal(FileStatusConstants.Pending, (await db.RetirementFiles.FindAsync(seeded.FileID))!.StatusID);
        }
    }

    [Fact]
    public async Task AddedAndDeletedDocument_AlwaysCreatesReviewAndPreservesDeletedRecord()
    {
        var seeded = await SeedAsync(FileStatusConstants.Active);
        int documentId;
        await using (var db = database.OpenContext())
        {
            var document = new Document { FileID = seeded.FileID, OwnerType = DocumentOwnerType.RetirementFile, DocumentTypeID = DocumentTypeConstants.NationalId,
                UploadedByUserID = seeded.UserID, FileName = "إضافة اختبار", FilePath = "test/new.pdf" };
            db.Add(document);
            await db.SaveChangesAsync();
            documentId = document.DocumentID;
            Assert.Equal(FileStatusConstants.Pending, (await db.RetirementFiles.FindAsync(seeded.FileID))!.StatusID);
            document.IsDeleted = true;
            document.DeletedByUserID = seeded.UserID;
            await db.SaveChangesAsync();
        }
        await using (var db = database.OpenContext())
        {
            Assert.Null(await db.Documents.SingleOrDefaultAsync(d => d.DocumentID == documentId));
            Assert.NotNull(await db.Documents.IgnoreQueryFilters().SingleOrDefaultAsync(d => d.DocumentID == documentId));
            Assert.Equal(2, await db.FileReviewItems.CountAsync(i => i.RetirementFileID == seeded.FileID));
            Assert.Single(await db.PensionSuspensions.Where(s => s.FileID == seeded.FileID && s.Status == SuspensionStatusConstants.Active).ToListAsync());
        }
    }

    [Fact]
    public async Task SameEditorCanApproveWholeFileOrItem_AfterRejectionAndResubmission()
    {
        var seeded = await SeedAsync(FileStatusConstants.Active);
        await using var db = database.OpenContext();
        using var uow = new UnitOfWork(db);
        var reviewer = new User { Username = Guid.NewGuid().ToString("N"), FullName = "مراجع مستقل", PasswordHash = "test" };
        db.Users.Add(reviewer);
        var retiree = await db.Retirees.FindAsync(seeded.RetireeID);
        retiree!.Phone = "777555666";
        retiree.UpdatedByUserID = seeded.UserID;
        await db.SaveChangesAsync();

        var scope = new Mock<ITestBranchScope>();
        scope.Setup(s => s.IsHQ()).Returns(true);
        var service = new FileReviewService(uow, TestPensionAccess.From(uow, scope.Object), MissingDocuments(uow));
        Assert.True((await service.RejectReviewAsync(seeded.FileID, reviewer.UserID, "يلزم التأكد من البيانات")).IsSuccess);
        Assert.True((await service.SubmitReviewAsync(seeded.FileID, reviewer.UserID)).IsSuccess);
        var currentItem = await db.FileReviewItems.Where(i => i.RetirementFileID == seeded.FileID)
            .OrderByDescending(i => i.ItemID).FirstAsync();
        var approved = await service.ApproveItemAsync(currentItem.ItemID, seeded.UserID);
        Assert.True(approved.IsSuccess, approved.Message);
        Assert.Equal(FileStatusConstants.Active, (await db.RetirementFiles.FindAsync(seeded.FileID))!.StatusID);
        Assert.False(await db.PensionSuspensions.AnyAsync(s => s.FileID == seeded.FileID && s.Status == SuspensionStatusConstants.Active));
        retiree.Phone = "777555777";
        retiree.UpdatedByUserID = seeded.UserID;
        await db.SaveChangesAsync();
        approved = await service.ApproveReviewAsync(seeded.FileID, seeded.UserID);
        Assert.True(approved.IsSuccess, approved.Message);
    }

    private static FileReviewService Review(IUnitOfWork uow)
    {
        var scope = new Mock<ITestBranchScope>();
        scope.Setup(s => s.IsHQ()).Returns(true);
        return new FileReviewService(uow, TestPensionAccess.From(uow, scope.Object), MissingDocuments(uow));
    }

    private static IMissingDocumentService MissingDocuments(IUnitOfWork uow) =>
        new PensionMS.Infrastructure.Services.Documents.MissingDocumentService(uow,
            new PensionMS.Infrastructure.Services.Pension.Entitlements.EntitlementEngine(uow),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<PensionMS.Infrastructure.Services.Documents.MissingDocumentService>.Instance,
            TestPensionAccess.All(uow));

    [Fact]
    public async Task BatchCreatorCanApproveOwnBatch_AndAccountingEntryRemainsAtApproval()
    {
        var seeded = await SeedAsync(FileStatusConstants.Active);
        await using var db = database.OpenContext();
        using var uow = new UnitOfWork(db);
        var item = new SalaryPaymentItem { FileID = seeded.FileID, RetireeID = seeded.RetireeID, Amount = 1200,
            RecipientID = await db.SalaryRecipients.Where(r => r.FileID == seeded.FileID).Select(r => r.RecipientID).SingleAsync(),
            PaymentBookID = await db.PaymentBooks.Where(b => b.FileID == seeded.FileID).Select(b => b.PaymentBookID).SingleAsync(),
            StatusID = TransactionStatusConstants.PendingSend, CreatedByUserID = seeded.UserID,
            Batch = new SalaryPaymentBatch { ImportedByUserID = seeded.UserID, Status = BatchStatusConstants.Draft,
                TotalCount = 1, TotalAmount = 1200, BatchMonth = YemenTime.TodayDateTime,
                Bank = new Bank { BankName = Guid.NewGuid().ToString("N") } } };
        db.Add(item);
        await db.SaveChangesAsync();
        var generator = new Mock<IPayrollGenerationService>();
        generator.Setup(g => g.GenerateForBatchAsync(It.IsAny<SalaryPaymentBatch>(), It.IsAny<DateTime>(), seeded.UserID))
            .ReturnsAsync(new PayrollGenerationResult { GeneratedItems = [item] });
        var journal = new Mock<IJournalEntryService>();
        journal.Setup(j => j.CreateIntegrationEntryAsync(It.IsAny<FinancialOperationRequestDto>(), seeded.UserID))
            .ReturnsAsync(new FinancialOperationResultDto { IsSuccess = true });
        var service = new SalaryPaymentBatchService(uow, generator.Object, Mock.Of<IWorkflowTaskService>(),
            Mock.Of<INotificationService>(), journal.Object,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SalaryPaymentBatchService>.Instance,
            new Infrastructure.Reporting.Exporters.ExcelReportExporter(new Infrastructure.Reporting.Exporters.ExcelReportTemplate()), TestPensionAccess.All(uow),
            new PensionSuspensionService(uow, Mock.Of<INotificationService>(), TestPensionAccess.All(uow)));

        var result = await service.ApproveAsync(item.BatchID, seeded.UserID);
        Assert.True(result.IsSuccess, result.Message);
        var batch = await db.SalaryPaymentBatches.AsNoTracking().SingleAsync(b => b.BatchID == item.BatchID);
        Assert.Equal(BatchStatusConstants.Approved, batch.Status);
        Assert.Equal(batch.ImportedByUserID, batch.ApprovedByUserID);
        Assert.Equal(TransactionStatusConstants.PendingSend, item.StatusID);
        journal.Verify(j => j.CreateIntegrationEntryAsync(It.Is<FinancialOperationRequestDto>(r =>
            r.SourceReferenceID == batch.BatchID && r.Amount == 1200), seeded.UserID), Times.Once);
    }

    [Fact]
    public async Task FailureAfterBusinessSave_RollsBackDataReviewAndHoldTogether()
    {
        var seeded = await SeedAsync(FileStatusConstants.Active);
        await using (var db = database.OpenContext(new FailAfterBusinessSave()))
        {
            var retiree = await db.Retirees.FindAsync(seeded.RetireeID);
            retiree!.Phone = "777999111";
            retiree.UpdatedByUserID = seeded.UserID;
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        }
        await using var verify = database.OpenContext();
        Assert.Equal("777000111", (await verify.Retirees.FindAsync(seeded.RetireeID))!.Phone);
        Assert.Equal(FileStatusConstants.Active, (await verify.RetirementFiles.FindAsync(seeded.FileID))!.StatusID);
        Assert.False(await verify.PensionSuspensions.AnyAsync(h => h.FileID == seeded.FileID));
        Assert.False(await verify.FileReviewItems.AnyAsync(i => i.RetirementFileID == seeded.FileID));
    }

    [Fact]
    public async Task ExistingUnpaidAmount_BecomesPayableOnlyAfterReviewAndAllIndependentHoldsEnd()
    {
        var seeded = await SeedAsync(FileStatusConstants.Active);
        int paymentId;
        await using (var db = database.OpenContext())
        {
            var item = new SalaryPaymentItem
            {
                FileID = seeded.FileID, RetireeID = seeded.RetireeID, Amount = 1200, CreatedByUserID = seeded.UserID,
                StatusID = TransactionStatusConstants.PendingSend,
                Batch = new SalaryPaymentBatch { ImportedByUserID = seeded.UserID, Status = BatchStatusConstants.Approved,
                    Bank = new Bank { BankName = Guid.NewGuid().ToString("N") }, BatchMonth = YemenTime.TodayDateTime }
            };
            db.Add(item);
            await db.SaveChangesAsync();
            paymentId = item.PaymentItemID;
            var retiree = await db.Retirees.FindAsync(seeded.RetireeID);
            retiree!.Phone = "777555888";
            retiree.UpdatedByUserID = seeded.UserID;
            await db.SaveChangesAsync();
        }
        await using (var db = database.OpenContext())
        {
            using var unit = new UnitOfWork(db);
            var response = await PaymentBatches(unit).UpdateItemStatusAsync(paymentId,
                new UpdateItemStatusDto { StatusID = TransactionStatusConstants.Sent });
            Assert.False(response.IsSuccess);
            Assert.Equal(TransactionStatusConstants.PendingSend, (await db.SalaryPaymentItems.FindAsync(paymentId))!.StatusID);
            (await db.SalaryPaymentItems.FindAsync(paymentId))!.StatusID = TransactionStatusConstants.Sent;
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            db.PensionSuspensions.Add(new PensionSuspension { FileID = seeded.FileID, CreatedByUserID = seeded.UserID,
                Status = SuspensionStatusConstants.Active, SuspensionSource = "إداري", SuspensionDate = DateTime.UtcNow });
            await db.SaveChangesAsync();
            Assert.True((await Review(uow).ApproveReviewAsync(seeded.FileID, seeded.UserID)).IsSuccess);
        }
        await using (var db = database.OpenContext())
        {
            using var unit = new UnitOfWork(db);
            var response = await PaymentBatches(unit).UpdateItemStatusAsync(paymentId,
                new UpdateItemStatusDto { StatusID = TransactionStatusConstants.Sent });
            Assert.False(response.IsSuccess);
            Assert.Equal("يوجد إيقاف معاش نشط", response.Message);
            (await db.SalaryPaymentItems.FindAsync(paymentId))!.StatusID = TransactionStatusConstants.Sent;
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        }
        await using (var db = database.OpenContext())
        {
            var hold = await db.PensionSuspensions.SingleAsync(h => h.FileID == seeded.FileID && h.Status == SuspensionStatusConstants.Active);
            hold.Status = SuspensionStatusConstants.Lifted;
            hold.EndDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
            using var unit = new UnitOfWork(db);
            var response = await PaymentBatches(unit).UpdateItemStatusAsync(paymentId,
                new UpdateItemStatusDto { StatusID = TransactionStatusConstants.Sent });
            Assert.True(response.IsSuccess, response.Message);
            var item = await db.SalaryPaymentItems.FindAsync(paymentId);
            Assert.Equal(TransactionStatusConstants.Sent, item!.StatusID);
            Assert.Equal(1200, item.Amount);
            Assert.Equal(paymentId, item.PaymentItemID);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingDocumentComplianceCache_UsesCallerTransactionForCommitAndRollback(bool commit)
    {
        var seeded = await SeedAsync(FileStatusConstants.New);
        await using (var db = database.OpenContext())
        {
            (await db.RetirementFiles.FindAsync(seeded.FileID))!.MissingDocumentsCount = 999;
            await db.SaveChangesAsync();
        }
        int expected;
        await using (var db = database.OpenContext())
        using (var unit = new UnitOfWork(db))
        {
            await unit.BeginTransactionAsync();
            var missing = MissingDocuments(unit);
            expected = (await missing.GetComplianceSummaryAsync(seeded.FileID)).MissingCount;
            Assert.NotEqual(999, expected);
            await missing.RefreshComplianceCacheAsync(seeded.FileID);
            Assert.True(unit.HasActiveTransaction);
            Assert.Equal(expected, (await db.RetirementFiles.AsNoTracking()
                .SingleAsync(f => f.FileID == seeded.FileID)).MissingDocumentsCount);
            if (commit) await unit.CommitAsync();
            else await unit.RollbackAsync();
        }
        await using var verify = database.OpenContext();
        Assert.Equal(commit ? expected : 999, (await verify.RetirementFiles.FindAsync(seeded.FileID))!.MissingDocumentsCount);
    }

    private sealed class FailAfterBusinessSave : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
            CancellationToken cancellationToken = default) => throw new InvalidOperationException("فشل اختباري بعد الحفظ");
    }

    private static SalaryPaymentBatchService PaymentBatches(IUnitOfWork unit) => new(unit,
        Mock.Of<IPayrollGenerationService>(), Mock.Of<IWorkflowTaskService>(), Mock.Of<INotificationService>(),
        Mock.Of<IJournalEntryService>(), Microsoft.Extensions.Logging.Abstractions.NullLogger<SalaryPaymentBatchService>.Instance,
        new Infrastructure.Reporting.Exporters.ExcelReportExporter(new Infrastructure.Reporting.Exporters.ExcelReportTemplate()),
        TestPensionAccess.All(unit), new PensionSuspensionService(unit, Mock.Of<INotificationService>(), TestPensionAccess.All(unit)));

    [Fact]
    public async Task Rollout_QueuesAllCurrentRecords_Once_AndPreservesEndedIntentAndIndependentHolds()
    {
        var draft = await SeedAsync(FileStatusConstants.New);
        var ended = await SeedAsync(FileStatusConstants.EntitlementEnded);
        var suspended = await SeedAsync(FileStatusConstants.Active);
        await using var db = database.OpenContext();
        if (!await db.FileStatuses.AnyAsync(s => s.StatusID == 3))
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.FileStatuses ON; INSERT dbo.FileStatuses(StatusID,StatusName,CreatedAt) VALUES(3,N'حالة قديمة',SYSUTCDATETIME()); SET IDENTITY_INSERT dbo.FileStatuses OFF;");
        }
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE RetirementFiles SET StatusID=3 WHERE FileID={suspended.FileID}");
        var script = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "scripts", "retirement-lifecycle-rollout.sql"));
        script = script.Replace("DECLARE @Apply bit = 0;", "DECLARE @Apply bit = 1;")
            .Replace("DECLARE @ActorUserID int = NULL;", $"DECLARE @ActorUserID int = {draft.UserID};");
        await RunScriptAsync();
        var current = await db.RetirementFiles.AsNoTracking().ToListAsync();
        Assert.All(current, f => Assert.Equal(FileStatusConstants.Pending, f.StatusID));
        Assert.Equal(FileStatusConstants.EntitlementEnded, current.Single(f => f.FileID == ended.FileID).PreviousStatusID);
        Assert.Equal(2, await db.PensionSuspensions.CountAsync(s => s.FileID == suspended.FileID && s.Status == SuspensionStatusConstants.Active));
        Assert.NotNull(ApprovedPensionSnapshot.Read(current.Single(f => f.FileID == draft.FileID).LastReviewedFileState));
        var itemCount = await db.FileReviewItems.CountAsync();
        var holdCount = await db.PensionSuspensions.CountAsync();
        await RunScriptAsync();
        Assert.Equal(itemCount, await db.FileReviewItems.CountAsync());
        Assert.Equal(holdCount, await db.PensionSuspensions.CountAsync());

        async Task RunScriptAsync()
        {
            await db.Database.OpenConnectionAsync();
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = script;
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task<(int FileID, int RetireeID, int UserID)> SeedAsync(int status, bool secondFile = false)
    {
        await using var db = database.OpenContext();
        var suffix = Guid.NewGuid().ToString("N")[..16];
        var user = new User { Username = suffix, FullName = "اختبار المراجعة", PasswordHash = "test" };
        var branch = new PensionFundBranch { BranchName = suffix };
        var retiree = new Retiree { FullName = "متقاعد اختبار", NationalID = suffix, IsAlive = true, Phone = "777000111",
            ApprovalItem = new MinistryApprovalItem { FullName = "متقاعد اختبار", MilitaryNumber = suffix,
                Batch = new MinistryApprovalBatch { ImportedByUser = user } } };
        var file = new RetirementFile { Retiree = retiree, PensionBranch = branch, FileNumber = suffix,
            StatusID = status, ReviewStatus = status == FileStatusConstants.Active ? FileReviewStatus.Approved : FileReviewStatus.None };
        var files = new List<RetirementFile> { file };
        if (secondFile) files.Add(new RetirementFile { Retiree = retiree, PensionBranch = branch, FileNumber = suffix + "-2",
            StatusID = status, ReviewStatus = FileReviewStatus.Approved });
        var requiredTypes = await db.DocumentTypes.Where(t => t.IsActive && (t.IsRequired ||
            t.DocumentTypeID == DocumentTypeConstants.FileOriginals || t.DocumentTypeID == DocumentTypeConstants.PaymentBookImage)).ToListAsync();
        db.Add(new FinancialEntitlement { Retiree = retiree, Amount = 1200, EffectiveDate = DateTime.UtcNow.AddYears(-1) });
        foreach (var item in files)
        {
            db.AddRange(item,
                new PaymentBook { RetirementFile = item, BookNumber = item.FileNumber, IssueDate = YemenTime.Today.AddYears(-1), ExpiryDate = YemenTime.Today.AddYears(1) },
                new SalaryRecipient { RetirementFile = item, Retiree = retiree, StartDate = DateTime.UtcNow.AddYears(-1) });
            foreach (var type in requiredTypes)
                db.Add(new Document { RetirementFile = item,
                    Retiree = type.DocumentTypeID == DocumentTypeConstants.NationalId ? retiree : null,
                    OwnerType = type.DocumentTypeID == DocumentTypeConstants.NationalId ? DocumentOwnerType.Retiree : DocumentOwnerType.RetirementFile,
                    DocumentTypeID = type.DocumentTypeID, UploadedByUser = user,
                    FileName = "وثيقة اختبار", FilePath = "test/" + type.DocumentTypeID });
        }
        await db.SaveChangesAsync();
        return (file.FileID, retiree.RetireeID, user.UserID);
    }
}
