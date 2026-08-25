using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities.Assets;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Data.Seeders.Assets;

public static class FixedAssetTypeSeeder
{
    public static async Task SeedAsync(PensionDbContext context)
    {
        if (await context.FixedAssetTypes.AnyAsync()) return;

        var now = System.DateTime.UtcNow;

        var types = new[]
        {
            new FixedAssetType { TypeName = "أجهزة وتقنية", TypeCode = "IT", Description = "أجهزة كمبيوتر، طابعات، شاشات، سيرفرات، معدات شبكات", IsActive = true, CreatedAt = now },
            new FixedAssetType { TypeName = "مركبات", TypeCode = "VEH", Description = "سيارات ركوب، سيارات نقل، مركبات خدمة", IsActive = true, CreatedAt = now },
            new FixedAssetType { TypeName = "مباني", TypeCode = "BLD", Description = "مكاتب إدارية، مستودعات، مرافق", IsActive = true, CreatedAt = now },
            new FixedAssetType { TypeName = "أراضي", TypeCode = "LAND", Description = "أراضي، مساحات خارجية", IsActive = true, CreatedAt = now },
            new FixedAssetType { TypeName = "أثاث ومفروشات", TypeCode = "FUR", Description = "مكاتب، كراسي، خزائن، ستائر", IsActive = true, CreatedAt = now },
        };

        context.FixedAssetTypes.AddRange(types);
        await context.SaveChangesAsync();

        System.Console.WriteLine($"✅ تم إضافة {types.Length} نوع أصول بنجاح");
    }
}
