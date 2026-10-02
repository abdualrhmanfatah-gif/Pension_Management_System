using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Data.Seeders.Accounting;
using PensionMS.Infrastructure.Data.Seeders.Assets;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Data.Seeders
{
    public static class PrimaryDataSeeder
    {
        public static async Task SeedAllAsync(PensionDbContext context, ILogger logger)
        {
            // Step 1: Ensure main branch exists
            await SeedMainBranchAsync(context, logger);

            // Step 2: Create seed users
            await SeedUsersAsync(context, logger);

            // Step 3: Investment document types
            await RunSeederAsync("InvestmentDocumentType", () => InvestmentDocumentTypeSeeder.SeedAsync(context), logger);

            // Step 4: HR document types
            await RunSeederAsync("HrDocumentType", () => HrDocumentTypeSeeder.SeedAsync(context), logger);

            // Step 5: Chart of Accounts (الدليل المحاسبي الموحد)
            await RunSeederAsync("ChartOfAccounts", () => ChartOfAccountsSeeder.SeedAsync(context), logger);

            // Step 6: Fixed Asset Types (أنواع الأصول الثابتة)
            await RunSeederAsync("FixedAssetType", () => FixedAssetTypeSeeder.SeedAsync(context), logger);

            // Step 7: Fixed Asset Categories (تصنيفات الأصول الثابتة) — تعتمد على الحسابات المالية
            await RunSeederAsync("FixedAssetCategory", () => FixedAssetCategorySeeder.SeedAsync(context), logger);

            // Step 8: Inventory items
            await RunSeederAsync("InventoryItem", () => InventoryItemSeeder.SeedAsync(context), logger);

            // Step 9: Opening balances (الأرصدة الافتتاحية) — الأصول ورصيد البنك المركزي
            await RunSeederAsync("OpeningBalance", () => OpeningBalanceSeeder.SeedAsync(context), logger);
        }

        private static async Task SeedMainBranchAsync(PensionDbContext context, ILogger logger)
        {
            try
            {
                var mainBranch = await context.PensionFundBranches.FirstOrDefaultAsync(b => b.PensionBranchID == 1);
                if (mainBranch == null)
                {
                    mainBranch = new PensionFundBranch
                    {
                        BranchName = "الادارة العامة",
                        Governorate = "عدن",
                        IsActive = true
                    };
                    context.PensionFundBranches.Add(mainBranch);
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ تم إنشاء فرع الإدارة العامة");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ تعذر إنشاء فرع الإدارة العامة: {Message}", ex.Message);
            }
        }

        private static async Task SeedUsersAsync(PensionDbContext context, ILogger logger)
        {
            try
            {
                var mainBranch = await context.PensionFundBranches.FirstOrDefaultAsync(b => b.PensionBranchID == 1);
                if (mainBranch == null)
                {
                    logger.LogWarning("⚠️ لا يمكن إنشاء المستخدمين: فرع الإدارة العامة غير موجود");
                    return;
                }

                var seedUsers = new[]
                {
                    new { Username = "admin",           FullName = "مدير النظام",         Email = "admin@pension.ly",           RoleID = 1 },
                    new { Username = "pension.manager", FullName = "مدير إدارة المعاشات", Email = "pension.mgr@pension.ly",     RoleID = 2 },
                    new { Username = "jamal.alqutaibi",    FullName = "جمال فضل عبدالكريم القطيبي",  Email = "jamal@pension.ly",     RoleID = 1 },
                    new { Username = "shamekh.fattah",     FullName = "شامخ عبدالرحمن أحمد فتاح",    Email = "shamekh@pension.ly",   RoleID = 4 },
                    new { Username = "naif.alshajari",     FullName = "نائف أحمد محمد الشاجري",      Email = "naif@pension.ly",      RoleID = 1 },
                    new { Username = "saleh.haidara",      FullName = "صالح علي مقبل حيدرة",         Email = "saleh@pension.ly",     RoleID = 5 },
                    new { Username = "mansour.alyamani",   FullName = "منصور عبدالله اليماني",       Email = "mansour@pension.ly",   RoleID = 2 },
                    new { Username = "kifah.fattah",       FullName = "كفاح عبدالرحمن أحمد فتاح",   Email = "kifah@pension.ly",     RoleID = 1 },
                    new { Username = "khaled.binsaleh",    FullName = "خالد صالح بن صالح",           Email = "khaled@pension.ly",    RoleID = 4 },
                    new { Username = "amna.salem",         FullName = "آمنة علي عياش سالم",          Email = "amna@pension.ly",      RoleID = 6 },
                    new { Username = "rana.yousef",        FullName = "رنا علي هربي يوسف",           Email = "rana@pension.ly",      RoleID = 3 },
                    new { Username = "ahmad.alkhader",     FullName = "أحمد الخضر أحمد",             Email = "ahmad@pension.ly",     RoleID = 6 },
                    new { Username = "maher.habah",        FullName = "ماهر هبه علي هبه",            Email = "maher@pension.ly",     RoleID = 3 },
                    new { Username = "amro.khaled",        FullName = "عمرو خالد صالح",              Email = "amro@pension.ly",      RoleID = 5 },
                    new { Username = "mohammed.abdo",      FullName = "محمد عبده علي",               Email = "mohammed@pension.ly",  RoleID = 3 },
                };

                var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

                foreach (var su in seedUsers)
                {
                    var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == su.Username);
                    if (existingUser == null)
                    {
                        var newUser = new User
                        {
                            FullName = su.FullName,
                            Username = su.Username,
                            Email = su.Email,
                            PasswordHash = defaultPasswordHash,
                            RoleID = su.RoleID,
                            BranchID = mainBranch.PensionBranchID,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.Users.Add(newUser);
                        try
                        {
                            await context.SaveChangesAsync();
                        }
                        catch (DbUpdateException ex)
                        {
                            context.Entry(newUser).State = EntityState.Detached;
                            logger.LogWarning(ex, "⚠️ تخطي المستخدم المكرر: {Username}", su.Username);
                            continue;
                        }

                        context.UserRoleAssignments.Add(new UserRoleAssignment
                        {
                            UserId = newUser.UserID,
                            RoleId = su.RoleID,
                            BranchId = null,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                        await context.SaveChangesAsync();
                        logger.LogInformation("✅ تم إنشاء المستخدم: {Username} (دور {RoleID})", su.Username, su.RoleID);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ تعذر إنشاء المستخدمين: {Message}", ex.Message);
            }
        }

        private static async Task RunSeederAsync(string name, Func<Task> seeder, ILogger logger)
        {
            try
            {
                await seeder();
                logger.LogInformation("✅ {Name} completed", name);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ {Name} failed: {Message}", name, ex.Message);
            }
        }
    }
}
