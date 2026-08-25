using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetItems",
                columns: table => new
                {
                    BudgetItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearID = table.Column<int>(type: "int", nullable: false),
                    QuarterID = table.Column<int>(type: "int", nullable: true),
                    PeriodID = table.Column<int>(type: "int", nullable: true),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetItems", x => x.BudgetItemID);
                    table.ForeignKey(
                        name: "FK_BudgetItems_ChartOfAccounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetItems_FiscalPeriods_PeriodID",
                        column: x => x.PeriodID,
                        principalTable: "FiscalPeriods",
                        principalColumn: "PeriodID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetItems_FiscalYears_FiscalYearID",
                        column: x => x.FiscalYearID,
                        principalTable: "FiscalYears",
                        principalColumn: "FiscalYearID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetItems_Quarters_QuarterID",
                        column: x => x.QuarterID,
                        principalTable: "Quarters",
                        principalColumn: "QuarterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetItems_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5235));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5242));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5244));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5263));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5264));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9553));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9563));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9565));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9566));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9567));

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_AccountID",
                table: "BudgetItems",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_DeletedByUserID",
                table: "BudgetItems",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_FiscalYearID",
                table: "BudgetItems",
                column: "FiscalYearID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_PeriodID",
                table: "BudgetItems",
                column: "PeriodID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_QuarterID",
                table: "BudgetItems",
                column: "QuarterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetItems");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 320, DateTimeKind.Utc).AddTicks(2161));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 320, DateTimeKind.Utc).AddTicks(2168));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 320, DateTimeKind.Utc).AddTicks(2170));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 320, DateTimeKind.Utc).AddTicks(2171));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 320, DateTimeKind.Utc).AddTicks(2172));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 346, DateTimeKind.Utc).AddTicks(4341));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 346, DateTimeKind.Utc).AddTicks(4349));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 346, DateTimeKind.Utc).AddTicks(4350));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 346, DateTimeKind.Utc).AddTicks(4352));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 22, 45, 8, 346, DateTimeKind.Utc).AddTicks(4353));
        }
    }
}
