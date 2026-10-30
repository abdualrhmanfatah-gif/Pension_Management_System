using Microsoft.EntityFrameworkCore;

namespace PensionMS.Infrastructure.Data.Seeders;

public static class JournalEntryTypeSeeder
{
    public static async Task SeedAsync(PensionDbContext context)
    {
        if (await context.JournalEntryTypes.AnyAsync())
            return;

        var types = new[]
        {
            new { Code = "OPN", TypeName = "أرصدة افتتاحية", TypeNameEn = "Opening Balance", Color = "#7F8C8D", Description = "قيود بداية السنة المالية والأرصدة المرحلة" },
            new { Code = "GEN", TypeName = "قيد عام", TypeNameEn = "General Journal", Color = "#34495E", Description = "القيود اليدوية والتسويات المحاسبية" },
            new { Code = "PUR", TypeName = "مشتريات", TypeNameEn = "Purchases", Color = "#3498DB", Description = "قيود شراء الأصول والمخزون والخدمات" },
            new { Code = "SAL", TypeName = "رواتب وأجور", TypeNameEn = "Payroll", Color = "#2980B9", Description = "قيود رواتب الموظفين والاستحقاقات" },
            new { Code = "PEN", TypeName = "معاشات تقاعدية", TypeNameEn = "Pensions", Color = "#2ECC71", Description = "قيود صرف المعاشات والمستحقات التقاعدية" },
            new { Code = "EXP", TypeName = "مصروفات", TypeNameEn = "Expenses", Color = "#E74C3C", Description = "المصروفات التشغيلية والإدارية" },
            new { Code = "REV", TypeName = "إيرادات", TypeNameEn = "Revenues", Color = "#27AE60", Description = "الإيرادات والاشتراكات والعوائد" },
            new { Code = "REC", TypeName = "سند قبض", TypeNameEn = "Receipt Voucher", Color = "#16A085", Description = "تحصيل النقدية والبنوك" },
            new { Code = "PAY", TypeName = "سند صرف", TypeNameEn = "Payment Voucher", Color = "#8E44AD", Description = "عمليات الدفع والصرف" },
            new { Code = "AST", TypeName = "الأصول الثابتة", TypeNameEn = "Fixed Assets", Color = "#1ABC9C", Description = "شراء وإضافة ونقل واستبعاد الأصول" },
            new { Code = "DEP", TypeName = "إهلاك الأصول", TypeNameEn = "Depreciation", Color = "#E67E22", Description = "قيود الإهلاك الدوري للأصول" },
            new { Code = "INV", TypeName = "المخزون", TypeNameEn = "Inventory", Color = "#16A085", Description = "حركات وتسويات المخزون" },
            new { Code = "ADJ", TypeName = "تسويات محاسبية", TypeNameEn = "Adjustments", Color = "#F39C12", Description = "قيود التسويات والتصحيح" },
            new { Code = "CLS", TypeName = "إقفال السنة المالية", TypeNameEn = "Year Closing", Color = "#9B59B6", Description = "إقفال الحسابات وترحيل النتائج" },
            new { Code = "SYS", TypeName = "قيود آلية", TypeNameEn = "System Generated", Color = "#95A5A6", Description = "قيود ينشئها النظام تلقائياً" },
        };

        foreach (var t in types)
        {
            context.JournalEntryTypes.Add(new Core.Entities.Accounting.JournalEntryType
            {
                TypeName = t.TypeName,
                TypeNameEn = t.TypeNameEn,
                Code = t.Code,
                Color = t.Color,
                Description = t.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
