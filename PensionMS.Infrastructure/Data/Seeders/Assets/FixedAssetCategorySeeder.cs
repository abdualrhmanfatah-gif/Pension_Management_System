using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities.Assets;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Data.Seeders.Assets;

public static class FixedAssetCategorySeeder
{
    // ═══ ربط التصنيفات بالحسابات المحاسبية ═══
    // (CategoryCode → (AccountCode للحساب, نوع الحساب: A=Asset, C=AccumDepr, D=DeprExpense))
    private static readonly Dictionary<string, (string Asset, string? Accum, string? Depr)> AccountLinks = new()
    {
        // ═══ أجهزة وتقنية (IT) ═══
        ["IT-PC"]    = ("1158", "2312", "3514"),
        ["IT-PRN"]   = ("1158", "2312", "3514"),
        ["IT-SCR"]   = ("1158", "2312", "3514"),
        ["IT-SRV"]   = ("1158", "2312", "3514"),
        ["IT-NET"]   = ("1158", "2312", "3514"),

        // ═══ مركبات (VEH) ═══
        ["VEH-CAR"]  = ("1141", "2313", "3513"),
        ["VEH-TRK"]  = ("1141", "2313", "3513"),

        // ═══ مباني (BLD) ═══
        ["BLD-OFF"]  = ("1225", "2311", "3511"),
        ["BLD-WH"]   = ("1225", "2311", "3511"),

        // ═══ أراضي (LAND) ═══
        ["LAND-RES"] = ("1111", null,  null),
        ["LAND-COM"] = ("1111", null,  null),

        // ═══ أثاث ومفروشات (FUR) ═══
        ["FUR-DESK"] = ("1152", "2314", "3514"),
        ["FUR-CHR"]  = ("1152", "2314", "3514"),
    };

    public static async Task SeedAsync(PensionDbContext context)
    {
        // جلب جميع الحسابات الموجودة
        var accounts = await context.ChartOfAccounts
            .Where(a => a.AccountCode != null)
            .ToDictionaryAsync(a => a.AccountCode!, a => a.AccountID);

        int? GetAccountId(string code) =>
            accounts.TryGetValue(code, out var id) ? id : null;

        // جلب أنواع الأصول
        var types = await context.FixedAssetTypes
            .ToDictionaryAsync(t => t.TypeCode, t => t.AssetTypeID);

        int GetTypeId(string code) =>
            types.TryGetValue(code, out var id) ? id : types.Values.First();

        var now = System.DateTime.UtcNow;

        // ═══ إنشاء التصنيفات إذا لم تكن موجودة ═══
        var existing = await context.FixedAssetCategories.ToListAsync();
        if (!existing.Any())
        {
            var categories = new[]
            {
                // ═══ أجهزة وتقنية (IT) ═══
                // 36 شهر = 3 سنوات → 33.33%
                new FixedAssetCategory { CategoryName = "أجهزة كمبيوتر", CategoryCode = "IT-PC", AssetTypeID = GetTypeId("IT"), DepreciationRate = 33.33m, SalvagePercent = 10, AssetAccountID = GetAccountId("1158"), AccumDeprAccountID = GetAccountId("2312"), DeprExpenseAccountID = GetAccountId("3514"), IsActive = true, CreatedAt = now },
                // 60 شهر = 5 سنوات → 20%
                new FixedAssetCategory { CategoryName = "طابعات", CategoryCode = "IT-PRN", AssetTypeID = GetTypeId("IT"), DepreciationRate = 20m, SalvagePercent = 5, AssetAccountID = GetAccountId("1158"), AccumDeprAccountID = GetAccountId("2312"), DeprExpenseAccountID = GetAccountId("3514"), IsActive = true, CreatedAt = now },
                new FixedAssetCategory { CategoryName = "شاشات", CategoryCode = "IT-SCR", AssetTypeID = GetTypeId("IT"), DepreciationRate = 33.33m, SalvagePercent = 5, AssetAccountID = GetAccountId("1158"), AccumDeprAccountID = GetAccountId("2312"), DeprExpenseAccountID = GetAccountId("3514"), IsActive = true, CreatedAt = now },
                new FixedAssetCategory { CategoryName = "سيرفرات", CategoryCode = "IT-SRV", AssetTypeID = GetTypeId("IT"), DepreciationRate = 20m, SalvagePercent = 5, AssetAccountID = GetAccountId("1158"), AccumDeprAccountID = GetAccountId("2312"), DeprExpenseAccountID = GetAccountId("3514"), IsActive = true, CreatedAt = now },
                new FixedAssetCategory { CategoryName = "معدات شبكات", CategoryCode = "IT-NET", AssetTypeID = GetTypeId("IT"), DepreciationRate = 20m, SalvagePercent = 5, AssetAccountID = GetAccountId("1158"), AccumDeprAccountID = GetAccountId("2312"), DeprExpenseAccountID = GetAccountId("3514"), IsActive = true, CreatedAt = now },

                // ═══ مركبات (VEH) ═══
                // 60 شهر = 5 سنوات → 20%
                new FixedAssetCategory { CategoryName = "سيارات ركوب", CategoryCode = "VEH-CAR", AssetTypeID = GetTypeId("VEH"), DepreciationRate = 20m, SalvagePercent = 10, AssetAccountID = GetAccountId("1141"), AccumDeprAccountID = GetAccountId("2313"), DeprExpenseAccountID = GetAccountId("3513"), IsActive = true, CreatedAt = now },
                // 84 شهر = 7 سنوات → 14.29%
                new FixedAssetCategory { CategoryName = "سيارات نقل", CategoryCode = "VEH-TRK", AssetTypeID = GetTypeId("VEH"), DepreciationRate = 14.29m, SalvagePercent = 10, AssetAccountID = GetAccountId("1141"), AccumDeprAccountID = GetAccountId("2313"), DeprExpenseAccountID = GetAccountId("3513"), IsActive = true, CreatedAt = now },

                // ═══ مباني (BLD) ═══
                // 240 شهر = 20 سنة → 5%
                new FixedAssetCategory { CategoryName = "مباني إدارية", CategoryCode = "BLD-OFF", AssetTypeID = GetTypeId("BLD"), DepreciationRate = 5m, SalvagePercent = 10, AssetAccountID = GetAccountId("1225"), AccumDeprAccountID = GetAccountId("2311"), DeprExpenseAccountID = GetAccountId("3511"), IsActive = true, CreatedAt = now },
                new FixedAssetCategory { CategoryName = "مستودعات", CategoryCode = "BLD-WH", AssetTypeID = GetTypeId("BLD"), DepreciationRate = 5m, SalvagePercent = 10, AssetAccountID = GetAccountId("1225"), AccumDeprAccountID = GetAccountId("2311"), DeprExpenseAccountID = GetAccountId("3511"), IsActive = true, CreatedAt = now },

                // ═══ أراضي (LAND) — لا تُهلك ═══
                new FixedAssetCategory { CategoryName = "أراضي سكنية", CategoryCode = "LAND-RES", AssetTypeID = GetTypeId("LAND"), DepreciationRate = 0m, SalvagePercent = 0, AssetAccountID = GetAccountId("1111"), AccumDeprAccountID = null, DeprExpenseAccountID = null, IsActive = true, CreatedAt = now },
                new FixedAssetCategory { CategoryName = "أراضي تجارية", CategoryCode = "LAND-COM", AssetTypeID = GetTypeId("LAND"), DepreciationRate = 0m, SalvagePercent = 0, AssetAccountID = GetAccountId("1111"), AccumDeprAccountID = null, DeprExpenseAccountID = null, IsActive = true, CreatedAt = now },

                // ═══ أثاث ومفروشات (FUR) ═══
                // 120 شهر = 10 سنوات → 10%
                new FixedAssetCategory { CategoryName = "مكاتب إدارية", CategoryCode = "FUR-DESK", AssetTypeID = GetTypeId("FUR"), DepreciationRate = 10m, SalvagePercent = 5, AssetAccountID = GetAccountId("1152"), AccumDeprAccountID = GetAccountId("2314"), DeprExpenseAccountID = GetAccountId("3514"), IsActive = true, CreatedAt = now },
                // 60 شهر = 5 سنوات → 20%
                new FixedAssetCategory { CategoryName = "كراسي", CategoryCode = "FUR-CHR", AssetTypeID = GetTypeId("FUR"), DepreciationRate = 20m, SalvagePercent = 5, AssetAccountID = GetAccountId("1152"), AccumDeprAccountID = GetAccountId("2314"), DeprExpenseAccountID = GetAccountId("3514"), IsActive = true, CreatedAt = now },
            };

            context.FixedAssetCategories.AddRange(categories);
            await context.SaveChangesAsync();
            System.Console.WriteLine($"✅ تم إنشاء {categories.Length} تصنيف أصول بنجاح");
            return;
        }

        // ═══ ربط التصنيفات الموجودة بالحسابات ═══
        bool changed = false;

        foreach (var cat in existing)
        {
            if (!AccountLinks.TryGetValue(cat.CategoryCode, out var mapping))
                continue;

            var assetId = GetAccountId(mapping.Asset);
            var accumId = mapping.Accum != null ? GetAccountId(mapping.Accum) : null;
            var deprId  = mapping.Depr  != null ? GetAccountId(mapping.Depr)  : null;

            if (cat.AssetAccountID != assetId)
            {
                cat.AssetAccountID = assetId;
                changed = true;
            }
            if (cat.AccumDeprAccountID != accumId)
            {
                cat.AccumDeprAccountID = accumId;
                changed = true;
            }
            if (cat.DeprExpenseAccountID != deprId)
            {
                cat.DeprExpenseAccountID = deprId;
                changed = true;
            }
        }

        if (changed)
        {
            await context.SaveChangesAsync();
            System.Console.WriteLine("🔄 تم تحديث ربط التصنيفات بالحسابات المحاسبية");
        }
        else
        {
            System.Console.WriteLine($"✅ {existing.Count} تصنيف أصول مربوطة بحساباتها بنجاح");
        }
    }
}
