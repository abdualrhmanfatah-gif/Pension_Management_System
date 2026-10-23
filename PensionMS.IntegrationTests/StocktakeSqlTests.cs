using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Accounting;
using PensionMS.Infrastructure.Services.Inventory;

namespace PensionMS.IntegrationTests;

public class StocktakeSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task DeficitAdjustment_PostsBalancedJournalAndPreservesOldStateOnFailure_AndListUsesOneQuery()
    {
        int stocktakeId, userId, itemId, fiscalYearId;
        await using (var db = database.OpenContext())
        {
            var user = new User { Username = "stocktake", FullName = "اختبار", PasswordHash = "test" };
            var fiscal = new FiscalYear { YearName = "اختبار", StartDate = DateTime.UtcNow.AddMonths(-6), EndDate = DateTime.UtcNow.AddMonths(6) };
            db.Add(new FiscalPeriod { FiscalYear = fiscal, PeriodName = "اختبار", StartDate = fiscal.StartDate, EndDate = fiscal.EndDate });
            db.Add(new JournalEntryType { Code = "INV", TypeName = "جرد", TypeNameEn = "Inventory" });
            db.AddRange(new ChartOfAccount { AccountCode = "1301", AccountName = "مخزون", IsPostable = true },
                new ChartOfAccount { AccountCode = "399", AccountName = "تسوية", IsPostable = true });
            var item = new InventoryItem { ItemCode = "stocktake", ItemName = "اختبار", Unit = "قطعة", Quantity = 10 };
            var stocktake = new SimpleStocktake { FiscalYear = fiscal, CreatedByUser = user, StocktakeDate = DateTime.UtcNow };
            stocktake.Lines.Add(new SimpleStocktakeLine { InventoryItem = item, ItemCode = item.ItemCode,
                ItemName = item.ItemName, SystemQuantity = 10, PhysicalQuantity = 7, UnitCost = 20 });
            db.Add(stocktake);
            await db.SaveChangesAsync();
            (stocktakeId, userId, itemId) = (stocktake.SimpleStocktakeID, user.UserID, item.InventoryItemID);
            fiscalYearId = fiscal.FiscalYearID;
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            var audit = new Mock<IImmutableAuditService>();
            audit.Setup(a => a.LogAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<Guid?>()))
                .ThrowsAsync(new InvalidOperationException("فشل بعد القيد وتحديث الكمية"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => new SimpleStocktakeService(uow, new JournalEntryService(uow), audit.Object)
                .AdjustStocktakeAsync(stocktakeId, userId));
        }
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            Assert.Equal(10, (await db.Set<InventoryItem>().FindAsync(itemId))!.Quantity);
            Assert.Equal("InProgress", (await db.Set<SimpleStocktake>().FindAsync(stocktakeId))!.Status);
            Assert.Empty(await db.Set<JournalEntry>().ToListAsync());
            var result = await new SimpleStocktakeService(uow, new JournalEntryService(uow), Mock.Of<IImmutableAuditService>())
                .AdjustStocktakeAsync(stocktakeId, userId);
            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(60, result.Data!.TotalDebit);
            Assert.Equal(60, result.Data.TotalCredit);
            Assert.Equal(7, (await db.Set<InventoryItem>().FindAsync(itemId))!.Quantity);
            Assert.Equal(-60, (await db.Set<SimpleStocktake>().FindAsync(stocktakeId))!.TotalVarianceValue);
        }
        var counter = new ReadCounter();
        await using (var db = database.OpenContext(counter))
        using (var uow = new UnitOfWork(db))
        {
            Assert.Single(await new SimpleStocktakeService(uow, new JournalEntryService(uow), Mock.Of<IImmutableAuditService>()).GetAllStocktakesAsync(fiscalYearId));
            Assert.Equal(1, counter.Count);
        }
    }
}

internal sealed class ReadCounter : DbCommandInterceptor
{
    public int Count { get; private set; }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        if (command.CommandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) Count++;
        return new(result);
    }
}
