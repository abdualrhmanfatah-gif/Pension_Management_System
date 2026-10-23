using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Repositories;

namespace PensionMS.IntegrationTests;

public class SettlementQuerySqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task MilitaryMatches_KeepLinksLeadingZeroesAndRanks_AndBatchMetadataUsesRequestedBatch()
    {
        await using var db = database.OpenContext();
        var user = new User { Username = "test-settlement", FullName = "اختبار", PasswordHash = "test" };
        var ministry = new MinistryApprovalBatch { ImportedByUser = user };
        var approval = new MinistryApprovalItem { Batch = ministry, FullName = "اختبار", MilitaryNumber = "0012" };
        var otherApproval = new MinistryApprovalItem { Batch = ministry, FullName = "مطابقة الرقم", MilitaryNumber = "٠٠١٢" };
        var retiree = new Retiree { ApprovalItem = approval, FullName = "اختبار", NationalID = "test-settlement", MilitaryNumber = " ٠٠١٢ " };
        var rank = new Rank { RankName = "رتبة اختبار", BaseSalary = 120 };
        var batchA = new SettlementsBatch { BatchNumber = "test-a", CreatedByUser = user, SourceRowsCount = 10 };
        var batchB = new SettlementsBatch { BatchNumber = "test-b", CreatedByUser = user, SourceRowsCount = 99, SkippedCount = 7 };
        var decision = new SettlementDecision { DecisionNumber = "test-decision", CreatedByUser = user, DecisionDate = new DateTime(2026, 10, 9) };
        SettlementItem Item(SettlementsBatch batch, string? military) => new()
        {
            Batch = batch, Decision = decision, MilitaryNumber = military, FullName = "اختبار",
            CurrentRank = rank, ProcessedRank = rank, CreatedByUser = user
        };
        var direct = Item(batchA, "999"); direct.Retiree = retiree; direct.ApprovalItem = approval;
        var persian = Item(batchA, "۰۰۱۲");
        var approved = Item(batchB, null); approved.ApprovalItem = otherApproval;
        var spaced = Item(batchB, "0\t0 12");
        var deleted = Item(batchA, "0012"); deleted.IsDeleted = true;
        db.AddRange(direct, persian, approved, spaced, deleted, Item(batchA, "12"), Item(batchA, null));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var repository = new SettlementRepository(db);
        var matches = await repository.GetByRetireeIdAsync(retiree.RetireeID);
        Assert.Equal(new[] { direct.SettlementItemID, persian.SettlementItemID, approved.SettlementItemID, spaced.SettlementItemID }.Order(),
            matches.Select(i => i.SettlementItemID).Order());
        Assert.All(matches, i => { Assert.Equal("test-decision", i.DecisionNumber); Assert.Equal("رتبة اختبار", i.ProcessedRankName); });
        var batchDecision = Assert.Single(await repository.GetBatchDecisionsAsync(batchB.BatchID));
        Assert.Equal(batchB.BatchID, batchDecision.BatchID);
        Assert.Equal(99, batchDecision.SourceRowsCount);
        Assert.Equal(7, batchDecision.SkippedCount);
        Assert.Equal(2, batchDecision.ItemCount);
        var allDecisions = await repository.GetAllDecisionsAsync();
        Assert.Equal(batchA.BatchID, Assert.Single(allDecisions).BatchID);
        var page = await repository.GetDecisionsPagedAsync(1, 10, "test-decision");
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(decision.DecisionID, Assert.Single(page.Items).DecisionID);
        Assert.Equal(batchA.BatchID, page.Items[0].BatchID);
        var exportItems = await repository.GetItemsByDecisionIdAsync(decision.DecisionID);
        Assert.All(exportItems, i => { Assert.NotNull(i.CurrentRank); Assert.NotNull(i.ProcessedRank); });
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetDecisionsPagedAsync(0, 20));
    }
}
