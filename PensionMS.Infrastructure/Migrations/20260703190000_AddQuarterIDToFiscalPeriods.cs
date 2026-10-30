using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuarterIDToFiscalPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add QuarterID column (nullable — existing periods get NULL)
            migrationBuilder.AddColumn<int>(
                name: "QuarterID",
                table: "FiscalPeriods",
                type: "int",
                nullable: true);

            // FK → Quarters.QuarterID with SET NULL on delete
            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_QuarterID",
                table: "FiscalPeriods",
                column: "QuarterID");

            migrationBuilder.AddForeignKey(
                name: "FK_FiscalPeriods_Quarters_QuarterID",
                table: "FiscalPeriods",
                column: "QuarterID",
                principalTable: "Quarters",
                principalColumn: "QuarterID",
                onDelete: ReferentialAction.SetNull);
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
