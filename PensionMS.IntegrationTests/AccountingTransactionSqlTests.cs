using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Financial;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Accounting;

namespace PensionMS.IntegrationTests;

public class AccountingTransactionSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task VoucherPostAndCancel_KeepJournalAndBankMovementAtomic()
    {
        int voucherId, userId;
        await using (var db = database.OpenContext())
        {
            db.Add(new JournalEntryType { Code = "REC", TypeName = "سند قبض", TypeNameEn = "Receipt" });
            var user = new User { Username = "voucher-test", FullName = "اختبار", PasswordHash = "test" };
            var debit = new ChartOfAccount { AccountCode = "TST1", AccountName = "اختبار مدين", AccountNature = "Debit", IsPostable = true };
            var credit = new ChartOfAccount { AccountCode = "TST2", AccountName = "اختبار دائن", AccountNature = "Credit", IsPostable = true };
            var bank = new Bank { BankName = "اختبار", Account = debit };
            var fiscal = new FiscalYear { YearName = "اختبار", StartDate = DateTime.UtcNow.AddMonths(-6), EndDate = DateTime.UtcNow.AddMonths(6) };
            db.Add(new FiscalPeriod { FiscalYear = fiscal, PeriodName = "اختبار", StartDate = fiscal.StartDate, EndDate = fiscal.EndDate });
            var voucher = new Voucher { VoucherNumber = "test-voucher", VoucherType = VoucherType.Receipt,
                CreatedByUser = user, DebitAccount = debit, CreditAccount = credit, Bank = bank,
                Amount = 100, Description = "اختبار", VoucherDate = DateTime.UtcNow };
            db.Add(voucher);
            await db.SaveChangesAsync();
            (voucherId, userId) = (voucher.VoucherID, user.UserID);
        }
        await using (var db = database.OpenContext(new FailAfterSecondSave()))
        using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new VoucherService(uow, new JournalEntryService(uow)).PostVoucherAsync(voucherId, userId));
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            Assert.Equal(VoucherStatus.Draft, (await db.Set<Voucher>().SingleAsync(v => v.VoucherID == voucherId)).Status);
            Assert.Empty(await db.Set<JournalEntry>().ToListAsync());
            var result = await new VoucherService(uow, new JournalEntryService(uow)).PostVoucherAsync(voucherId, userId);
            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(VoucherStatus.Posted, result.Data!.Status);
            var bankMovement = Assert.Single(await db.Set<BankTransaction>().ToListAsync());
            Assert.True(bankMovement.LineID > 0);
            Assert.Equal(100, bankMovement.Amount);
        }
        await using (var db = database.OpenContext(new FailAfterSecondSave()))
        using (var uow = new UnitOfWork(db))
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new VoucherService(uow, new JournalEntryService(uow)).CancelVoucherAsync(voucherId, userId));
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            Assert.Equal(VoucherStatus.Posted, (await db.Set<Voucher>().SingleAsync(v => v.VoucherID == voucherId)).Status);
            Assert.Single(await db.Set<JournalEntry>().ToListAsync());
            var result = await new VoucherService(uow, new JournalEntryService(uow)).CancelVoucherAsync(voucherId, userId);
            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(VoucherStatus.Cancelled, result.Data!.Status);
            Assert.Equal(2, await db.Set<JournalEntry>().CountAsync());
            var movements = await db.Set<BankTransaction>().OrderBy(t => t.TransactionID).ToListAsync();
            Assert.Equal(2, movements.Count);
            Assert.Equal("Withdrawal", movements[1].TransactionType);
            Assert.Equal(100, movements[1].Amount);
            Assert.True(movements[1].LineID > 0);
        }
    }

    private sealed class FailAfterSecondSave : SaveChangesInterceptor
    {
        private int _calls;
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
            CancellationToken cancellationToken = default) => ++_calls == 2
                ? throw new InvalidOperationException("فشل اختباري") : new ValueTask<int>(result);
    }
}
