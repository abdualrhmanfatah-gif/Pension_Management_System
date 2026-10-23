using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Settlements;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Settlements;

namespace PensionMS.IntegrationTests;

public class SettlementTransactionSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    private static SettlementService Service(IUnitOfWork uow, PensionDbContext db, IImmutableAuditService? audit = null) =>
        new(uow, new SettlementRepository(db), Mock.Of<IFinancialEntitlementService>(), Mock.Of<INotificationService>(),
            Mock.Of<IJournalEntryService>(), audit ?? Mock.Of<IImmutableAuditService>(),
            new MemoryCache(new MemoryCacheOptions()), Mock.Of<IMissingDocumentService>(),
            new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportExporter(
                new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportTemplate()), PensionMS.Testing.TestPensionAccess.All(uow));

    [Fact]
    public async Task DecisionCreationAndDeletion_RollBackIdentityLinksAndBatchTotalsTogether()
    {
        int batchId, itemId, userId, decisionId;
        await using (var db = database.OpenContext())
        {
            var user = new User { Username = "decision-tx", FullName = "اختبار", PasswordHash = "test" };
            var rank = new Rank { RankName = "اختبار", BaseSalary = 500 };
            var batch = new SettlementsBatch { BatchNumber = "decision-tx", CreatedByUser = user,
                Status = BatchStatusConstants.Imported, TotalCount = 1, TotalAmount = 500 };
            var item = new SettlementItem { Batch = batch, CreatedByUser = user, FullName = "اختبار", CurrentRank = rank, ProcessedRank = rank };
            db.Add(item);
            await db.SaveChangesAsync();
            (batchId, itemId, userId) = (batch.BatchID, item.SettlementItemID, user.UserID);
        }
        var dto = new CreateSettlementDecisionDto { DecisionNumber = "decision-tx", ItemIDs = new List<int> { itemId } };
        await using (var db = database.OpenContext(new FailSecondSave()))
        using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() => Service(uow, db).CreateDecisionAsync(batchId, dto, userId));
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            Assert.Empty(await db.SettlementDecisions.ToListAsync());
            Assert.Null((await db.SettlementItems.FindAsync(itemId))!.DecisionID);
            var created = await Service(uow, db).CreateDecisionAsync(batchId, dto, userId);
            Assert.True(created.IsSuccess, created.Message);
            decisionId = created.Data!.DecisionID;
        }
        var audit = new Mock<IImmutableAuditService>();
        audit.Setup(a => a.LogAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("فشل بعد حذف القرار وتحديث الإجماليات"));
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() => Service(uow, db, audit.Object).DeleteDecisionAsync(decisionId, userId));
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            Assert.False((await db.SettlementDecisions.FindAsync(decisionId))!.IsDeleted);
            Assert.False((await db.SettlementItems.FindAsync(itemId))!.IsDeleted);
            Assert.Equal(500, (await db.SettlementsBatches.FindAsync(batchId))!.TotalAmount);
            Assert.True((await Service(uow, db).DeleteDecisionAsync(decisionId, userId)).IsSuccess);
        }
        await using (var db = database.OpenContext())
        {
            Assert.Empty(await db.SettlementDecisions.ToListAsync());
            Assert.Empty(await db.SettlementItems.ToListAsync());
            var batch = await db.SettlementsBatches.FindAsync(batchId);
            Assert.Equal(0, batch!.TotalCount);
            Assert.Equal(0, batch.TotalAmount);
            Assert.True((await db.SettlementDecisions.IgnoreQueryFilters().SingleAsync())!.IsDeleted);
        }
    }

    private sealed class FailSecondSave : SaveChangesInterceptor
    {
        private int _count;
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
            CancellationToken cancellationToken = default) => ++_count == 2
                ? throw new InvalidOperationException("فشل اختباري") : new(result);
    }
}
