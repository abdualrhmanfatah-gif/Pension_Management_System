using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuarterToFiscalPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add nullable QuarterID column — existing rows get NULL (safe)
            migrationBuilder.AddColumn<int>(
                name: "QuarterID",
                table: "FiscalPeriods",
                type: "int",
                nullable: true);

            // 2) Create index for the FK
            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_QuarterID",
                table: "FiscalPeriods",
                column: "QuarterID");

            // 3) Add FK with SET NULL on delete (safe for existing data)
            migrationBuilder.AddForeignKey(
                name: "FK_FiscalPeriods_Quarters_QuarterID",
                table: "FiscalPeriods",
                column: "QuarterID",
                principalTable: "Quarters",
                principalColumn: "QuarterID",
                onDelete: ReferentialAction.SetNull);

            // 4) Auto-populate QuarterID for existing periods based on PeriodNumber
            migrationBuilder.Sql(@"
                UPDATE fp
                SET fp.QuarterID = q.QuarterID
                FROM FiscalPeriods fp
                INNER JOIN Quarters q 
                    ON q.FiscalYearID = fp.FiscalYearID
                    AND q.QuarterNumber = CASE
                        WHEN fp.PeriodNumber BETWEEN 1 AND 3 THEN 1
                        WHEN fp.PeriodNumber BETWEEN 4 AND 6 THEN 2
                        WHEN fp.PeriodNumber BETWEEN 7 AND 9 THEN 3
                        WHEN fp.PeriodNumber BETWEEN 10 AND 12 THEN 4
                    END
                WHERE fp.QuarterID IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiscalPeriods_Quarters_QuarterID",
                table: "FiscalPeriods");

            migrationBuilder.DropIndex(
                name: "IX_FiscalPeriods_QuarterID",
                table: "FiscalPeriods");

            migrationBuilder.DropColumn(
                name: "QuarterID",
                table: "FiscalPeriods");
        }
    }
}
