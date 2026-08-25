using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Assets;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Data.Seeders.Accounting;

/// <summary>
/// إدخال بيانات أولية للأرصدة الافتتاحية (الأصول ورصيد البنك المركزي)
/// يُ被执行 عند إقلاع التطبيق عبر PrimaryDataSeeder.
/// </summary>
public static class OpeningBalanceSeeder
{
    // الفترة المحاسبية الافتتاحية (مُنشأة في SeedFiscalPeriodsManual)
    private const int OpeningPeriodId = 1;

    public static async Task SeedAsync(PensionDbContext context)
    {
        // جلب الحسابات المطلوبة من الدليل المحاسبي
        var accounts = await context.ChartOfAccounts
            .Where(a => a.AccountCode != null)
            .ToDictionaryAsync(a => a.AccountCode!, a => a.AccountID);

        int? GetAccountId(string code) =>
            accounts.TryGetValue(code, out var id) ? id : null;

        // ═══ الرصيد الافتتاحي لرصيد البنك المركزي (أصل — مدين) ═══
        var centralBankAssetId = GetAccountId("18211");
        // ═══ الرصيد الافتتاحي للبنك المركزي (التزام — دائن) ═══
        var centralBankLiabilityId = GetAccountId("256");

        // تجهيز الأرصدة الافتتاحية
        var openingBalances = new System.Collections.Generic.List<AccountBalance>();

        if (centralBankAssetId.HasValue)
        {
            openingBalances.Add(new AccountBalance
            {
                AccountID = centralBankAssetId.Value,
                PeriodID = OpeningPeriodId,
                OpeningDebit = 50000000m,   // 50 مليون — رصيد لدى البنك المركزي (أصل)
                OpeningCredit = 0m,
                PeriodDebit = 0m,
                PeriodCredit = 0m,
                ClosingDebit = 50000000m,
                ClosingCredit = 0m,
                IsDeleted = false
            });
        }

        if (centralBankLiabilityId.HasValue)
        {
            openingBalances.Add(new AccountBalance
            {
                AccountID = centralBankLiabilityId.Value,
                PeriodID = OpeningPeriodId,
                OpeningDebit = 0m,
                OpeningCredit = 50000000m,  // التزام لدى البنك المركزي
                PeriodDebit = 0m,
                PeriodCredit = 0m,
                ClosingDebit = 0m,
                ClosingCredit = 50000000m,
                IsDeleted = false
            });
        }

        // إدراج الأرصدة الافتتاحية بشكل متكرر (idempotent)
        foreach (var bal in openingBalances)
        {
            var exists = await context.AccountBalances
                .AnyAsync(b => b.AccountID == bal.AccountID && b.PeriodID == bal.PeriodID);

            if (!exists)
            {
                context.AccountBalances.Add(bal);
            }
        }

        if (openingBalances.Any())
        {
            await context.SaveChangesAsync();
        }

        // ═══ الأصول الثابتة الافتتاحية (اصول) ═══
        await SeedFixedAssetsAsync(context, accounts);
    }

    private static async Task SeedFixedAssetsAsync(
        PensionDbContext context,
        System.Collections.Generic.Dictionary<string, int> accounts)
    {
        // التأكد من وجود تصنيفات الأصول
        var categories = await context.FixedAssetCategories.ToListAsync();
        if (!categories.Any())
            return; // التصنيفات تُنشأ في FixedAssetCategorySeeder

        int? GetAccountId(string code) =>
            accounts.TryGetValue(code, out var id) ? id : null;

        int GetCatId(string code) =>
            categories.First(c => c.CategoryCode == code).CategoryID;

        var now = DateTime.UtcNow;




        await context.SaveChangesAsync();
        Console.WriteLine($"✅ تم إدراج بيانات الأرصدة الافتتاحية والأصول الأولية");
    }
}
