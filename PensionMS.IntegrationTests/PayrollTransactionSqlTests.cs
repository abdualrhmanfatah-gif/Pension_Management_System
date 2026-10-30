using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Core.Interfaces.Notifications;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Notifications;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;

namespace PensionMS.IntegrationTests;

public class PayrollTransactionSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    private readonly Mock<IHubContext<NotificationHub, INotificationClient>> _hub = new(MockBehavior.Strict);

    private NotificationService Notifications(IUnitOfWork uow) => new(uow, _hub.Object, Mock.Of<IFileStorageService>(), PensionMS.Testing.TestPensionAccess.AllowAllPermissions());

    private SalaryPaymentBatchService Service(IUnitOfWork uow, IPayrollGenerationService? generator = null) =>
        new(uow, generator ?? new PayrollGenerationService(uow), Mock.Of<IWorkflowTaskService>(),
            Notifications(uow), Mock.Of<IJournalEntryService>(),
            NullLogger<SalaryPaymentBatchService>.Instance,
            new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportExporter(
                new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportTemplate()), PensionMS.Testing.TestPensionAccess.All(uow),
            new PensionSuspensionService(uow, Mock.Of<INotificationService>(), PensionMS.Testing.TestPensionAccess.All(uow), Mock.Of<IDocumentPledgeService>()));

    [Fact]
    public async Task Regeneration_ConcurrentRequests_AndFailureAfterSave_PreserveOneCompleteBatch()
    {
        int batchId, actorId, fileId;
        await using (var db = database.OpenContext())
        {
            var role = await db.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Manager")
                ?? new Role { RoleName = "Manager" };
            var actor = new User { Username = "payroll-test", FullName = "اختبار الرواتب", PasswordHash = "test", Role = role };
            var retiree = new Retiree { FullName = "متقاعد اختبار", NationalID = "payroll-test",
                ApprovalItem = new MinistryApprovalItem { FullName = "متقاعد اختبار", MilitaryNumber = "300",
                    Batch = new MinistryApprovalBatch { ImportedByUser = actor } } };
            var file = new RetirementFile { FileNumber = "payroll-test", Retiree = retiree,
                PensionBranch = new PensionFundBranch { BranchName = "فرع الرواتب" }, StatusID = FileStatusConstants.Active };
            db.AddRange(file,
                new PaymentBook { RetirementFile = file, BookNumber = "payroll-test", Status = PaymentBookStatus.Active,
                    IssueDate = YemenTime.Today, ExpiryDate = YemenTime.Today.AddYears(1) },
                new SalaryRecipient { RetirementFile = file, Retiree = retiree, RecipientType = "متقاعد", IsActive = true },
                new FinancialEntitlement { Retiree = retiree, Amount = 120, EffectiveDate = DateTime.UtcNow.AddDays(-10), EndDate = YemenTime.TodayDateTime },
                // A newer expired record must not displace the valid entitlement.
                new FinancialEntitlement { Retiree = retiree, Amount = 999, EffectiveDate = DateTime.UtcNow.AddDays(-2), EndDate = YemenTime.TodayDateTime.AddDays(-1) });
            var batch = new SalaryPaymentBatch { ImportedByUser = actor, Bank = new Bank { BankName = "بنك اختبار" },
                BatchMonth = YemenTime.TodayDateTime, Status = BatchStatusConstants.Draft };
            db.Add(batch);
            await db.SaveChangesAsync();
            file.LastReviewedFileState = (await ApprovedPensionSnapshot.CaptureAsync(db, file.FileID, retiree.RetireeID)).Serialize();
            await db.SaveChangesAsync();
            (batchId, actorId, fileId) = (batch.BatchID, actor.UserID, file.FileID);
        }

        for (var i = 0; i < 2; i++)
        {
            await using var db = database.OpenContext();
            using var uow = new UnitOfWork(db);
            var result = await Service(uow).GenerateItemsAsync(batchId, actorId);
            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(1, result.Data!.GeneratedCount);
            Assert.Equal(120, result.Data.TotalAmount);
        }
        await AssertState(2);

        await using (var firstDb = database.OpenContext())
        await using (var secondDb = database.OpenContext())
        {
            using var first = new UnitOfWork(firstDb);
            using var second = new UnitOfWork(secondDb);
            var blocker = new BlockingGenerator(new PayrollGenerationService(first));
            var firstRequest = Service(first, blocker).GenerateItemsAsync(batchId, actorId);
            await blocker.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var secondRequest = Service(second).GenerateItemsAsync(batchId, actorId);
            try
            {
                Assert.NotSame(secondRequest, await Task.WhenAny(secondRequest, Task.Delay(250)));
            }
            finally { blocker.Release.TrySetResult(); }
            Assert.All(await Task.WhenAll(firstRequest, secondRequest), r => Assert.True(r.IsSuccess, r.Message));
        }
        await AssertState(4);

        // Simulate a failure after SQL changes were flushed but before commit.
        await using (var db = database.OpenContext(new FailAfterSave()))
        using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() => Service(uow).GenerateItemsAsync(batchId, actorId));
        await AssertState(4);

        // A nested caller owns the transaction: the service must not commit it.
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            await uow.BeginTransactionAsync();
            Assert.True((await Service(uow).GenerateItemsAsync(batchId, actorId)).IsSuccess);
            Assert.True(uow.HasActiveTransaction);
            await uow.RollbackAsync();
        }
        await AssertState(4);
        _hub.VerifyNoOtherCalls();

        async Task AssertState(int generations)
        {
            await using var db = database.OpenContext();
            var item = Assert.Single(await db.Set<SalaryPaymentItem>().Where(i => i.BatchID == batchId).ToListAsync());
            Assert.Equal(120, item.Amount);
            Assert.Equal(actorId, item.CreatedByUserID);
            Assert.Equal(fileId, item.FileID);
            Assert.NotNull(item.PaymentBookID);
            Assert.Equal(generations, await db.Set<SalaryPaymentItem>().IgnoreQueryFilters().CountAsync(i => i.BatchID == batchId));
            var batch = await db.Set<SalaryPaymentBatch>().SingleAsync(b => b.BatchID == batchId);
            Assert.Equal(1, batch.TotalCount);
            Assert.Equal(120, batch.TotalAmount);
            Assert.Equal(generations, await db.Set<Notification>().CountAsync(n => n.RelatedRecordID == batchId));
            Assert.Equal(generations, await db.Set<NotificationOutbox>().CountAsync());
            Assert.All(await db.Set<NotificationOutbox>().ToListAsync(), o => Assert.Equal(OutboxStatus.Pending, o.Status));
        }
    }

    private sealed class FailAfterSave : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
            CancellationToken cancellationToken = default) => throw new InvalidOperationException("فشل اختباري بعد الحفظ");
    }

    private sealed class BlockingGenerator(IPayrollGenerationService inner) : IPayrollGenerationService
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<PayrollGenerationResult> GenerateForBatchAsync(SalaryPaymentBatch batch, DateTime runAt, int userId)
        {
            Entered.TrySetResult();
            await Release.Task.WaitAsync(TimeSpan.FromSeconds(15));
            return await inner.GenerateForBatchAsync(batch, runAt, userId);
        }
    }
}
