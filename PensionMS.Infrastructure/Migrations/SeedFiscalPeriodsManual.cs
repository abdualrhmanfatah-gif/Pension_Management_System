using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedFiscalPeriodsManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert FiscalYear 2026 if not exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [FiscalYears] WHERE [FiscalYearID] = 1)
                BEGIN
                    SET IDENTITY_INSERT [FiscalYears] ON;
                    INSERT INTO [FiscalYears] ([FiscalYearID], [YearName], [StartDate], [EndDate], [TotalBudget], [IsClosed], [CreatedAt], [IsDeleted])
                    VALUES (1, '2026', '2026-01-01', '2026-12-31', 1000000000, 0, GETUTCDATE(), 0);
                    SET IDENTITY_INSERT [FiscalYears] OFF;
                END
            ");

            // Insert 12 monthly periods if not exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [FiscalPeriods] WHERE [FiscalYearID] = 1)
                BEGIN
                    SET IDENTITY_INSERT [FiscalPeriods] ON;
                    INSERT INTO [FiscalPeriods] ([PeriodID], [FiscalYearID], [PeriodName], [PeriodNumber], [StartDate], [EndDate], [IsClosed], [IsDeleted], [CreatedAt])
                    VALUES
                        (1,  1, N'يناير 2026',    1,  '2026-01-01', '2026-01-31', 0, 0, GETUTCDATE()),
                        (2,  1, N'فبراير 2026',   2,  '2026-02-01', '2026-02-28', 0, 0, GETUTCDATE()),
                        (3,  1, N'مارس 2026',      3,  '2026-03-01', '2026-03-31', 0, 0, GETUTCDATE()),
                        (4,  1, N'أبريل 2026',     4,  '2026-04-01', '2026-04-30', 0, 0, GETUTCDATE()),
                        (5,  1, N'مايو 2026',      5,  '2026-05-01', '2026-05-31', 0, 0, GETUTCDATE()),
                        (6,  1, N'يونيو 2026',     6,  '2026-06-01', '2026-06-30', 0, 0, GETUTCDATE()),
                        (7,  1, N'يوليو 2026',     7,  '2026-07-01', '2026-07-31', 0, 0, GETUTCDATE()),
                        (8,  1, N'أغسطس 2026',     8,  '2026-08-01', '2026-08-31', 0, 0, GETUTCDATE()),
                        (9,  1, N'سبتمبر 2026',    9,  '2026-09-01', '2026-09-30', 0, 0, GETUTCDATE()),
                        (10, 1, N'أكتوبر 2026',    10, '2026-10-01', '2026-10-31', 0, 0, GETUTCDATE()),
                        (11, 1, N'نوفمبر 2026',    11, '2026-11-01', '2026-11-30', 0, 0, GETUTCDATE()),
                        (12, 1, N'ديسمبر 2026',    12, '2026-12-01', '2026-12-31', 0, 0, GETUTCDATE());
                    SET IDENTITY_INSERT [FiscalPeriods] OFF;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [FiscalPeriods] WHERE [FiscalYearID] = 1");
            migrationBuilder.Sql("DELETE FROM [FiscalYears] WHERE [FiscalYearID] = 1");
        }
    }
}
