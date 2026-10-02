using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities.Inventory;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Data.Seeders
{
    public static class InventoryItemSeeder
    {
        public static async Task SeedAsync(PensionDbContext context)
        {
            if (!await context.InventoryItems.AnyAsync())
            {
                var now = System.DateTime.UtcNow;

                var items = new[]
                {
                    // ═══ مستلزمات مكتبية ═══
                    new InventoryItem
                    {
                        ItemName = "ورق A4",
                        ItemCode = "SUP-PAP-001",
                        Description = "ورق أبيض A4 للكتابة والطباعة",
                        Unit = "رزمة",
                        Quantity = 50,
                        MinimumQuantity = 20,
                        Condition = ItemCondition.New,
                        Status = ItemStatus.Active,
                        CreatedAt = now
                    },
                    new InventoryItem
                    {
                        ItemName = "حبر طابعة",
                        ItemCode = "SUP-INK-001",
                        Description = "حبر طابعة أسود",
                        Unit = "قطعة",
                        Quantity = 10,
                        MinimumQuantity = 5,
                        Condition = ItemCondition.New,
                        Status = ItemStatus.Active,
                        CreatedAt = now
                    },
                };

                context.InventoryItems.AddRange(items);
                await context.SaveChangesAsync();

                System.Console.WriteLine($"✅ تم إضافة {items.Length} صنف مخزون بنجاح");
            }
        }
    }
}
