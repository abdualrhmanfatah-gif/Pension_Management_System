using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartOfAccounts",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountNature = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccountLevel = table.Column<int>(type: "int", nullable: false),
                    ParentAccountID = table.Column<int>(type: "int", nullable: true),
                    BookNumber = table.Column<int>(type: "int", nullable: false),
                    IsPostable = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_ChartOfAccounts_ParentAccountID",
                        column: x => x.ParentAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CostCenters",
                columns: table => new
                {
                    CostCenterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentCostCenterID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenters", x => x.CostCenterID);
                    table.ForeignKey(
                        name: "FK_CostCenters_CostCenters_ParentCostCenterID",
                        column: x => x.ParentCostCenterID,
                        principalTable: "CostCenters",
                        principalColumn: "CostCenterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostCenters_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FiscalPeriods",
                columns: table => new
                {
                    PeriodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearID = table.Column<int>(type: "int", nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ClosedByUserID = table.Column<int>(type: "int", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalPeriods", x => x.PeriodID);
                    table.ForeignKey(
                        name: "FK_FiscalPeriods_FiscalYears_FiscalYearID",
                        column: x => x.FiscalYearID,
                        principalTable: "FiscalYears",
                        principalColumn: "FiscalYearID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FiscalPeriods_Users_ClosedByUserID",
                        column: x => x.ClosedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FiscalPeriods_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AccountBalances",
                columns: table => new
                {
                    BalanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    PeriodID = table.Column<int>(type: "int", nullable: false),
                    OpeningDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PeriodDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PeriodCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalances", x => x.BalanceID);
                    table.ForeignKey(
                        name: "FK_AccountBalances_ChartOfAccounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountBalances_FiscalPeriods_PeriodID",
                        column: x => x.PeriodID,
                        principalTable: "FiscalPeriods",
                        principalColumn: "PeriodID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountBalances_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    JournalEntryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodID = table.Column<int>(type: "int", nullable: false),
                    SourceModule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceReferenceID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PostedByUserID = table.Column<int>(type: "int", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversedEntryID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.JournalEntryID);
                    table.ForeignKey(
                        name: "FK_JournalEntries_FiscalPeriods_PeriodID",
                        column: x => x.PeriodID,
                        principalTable: "FiscalPeriods",
                        principalColumn: "PeriodID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_JournalEntries_ReversedEntryID",
                        column: x => x.ReversedEntryID,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Users_PostedByUserID",
                        column: x => x.PostedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryID = table.Column<int>(type: "int", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    CostCenterID = table.Column<int>(type: "int", nullable: true),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_ChartOfAccounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_CostCenters_CostCenterID",
                        column: x => x.CostCenterID,
                        principalTable: "CostCenters",
                        principalColumn: "CostCenterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryID",
                        column: x => x.JournalEntryID,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_Users_DeletedByUserID",
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
                value: new DateTime(2026, 7, 3, 17, 7, 59, 907, DateTimeKind.Utc).AddTicks(7353));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 907, DateTimeKind.Utc).AddTicks(7388));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 907, DateTimeKind.Utc).AddTicks(7390));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 907, DateTimeKind.Utc).AddTicks(7394));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 907, DateTimeKind.Utc).AddTicks(7395));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 935, DateTimeKind.Utc).AddTicks(5241));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 935, DateTimeKind.Utc).AddTicks(5263));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 935, DateTimeKind.Utc).AddTicks(5264));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 935, DateTimeKind.Utc).AddTicks(5266));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 17, 7, 59, 935, DateTimeKind.Utc).AddTicks(5267));

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_AccountID_PeriodID",
                table: "AccountBalances",
                columns: new[] { "AccountID", "PeriodID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_DeletedByUserID",
                table: "AccountBalances",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_PeriodID",
                table: "AccountBalances",
                column: "PeriodID");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_AccountCode",
                table: "ChartOfAccounts",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_DeletedByUserID",
                table: "ChartOfAccounts",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_ParentAccountID",
                table: "ChartOfAccounts",
                column: "ParentAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_Code",
                table: "CostCenters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_DeletedByUserID",
                table: "CostCenters",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_ParentCostCenterID",
                table: "CostCenters",
                column: "ParentCostCenterID");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_ClosedByUserID",
                table: "FiscalPeriods",
                column: "ClosedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_DeletedByUserID",
                table: "FiscalPeriods",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_FiscalYearID_PeriodNumber",
                table: "FiscalPeriods",
                columns: new[] { "FiscalYearID", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_DeletedByUserID",
                table: "JournalEntries",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PeriodID_Status",
                table: "JournalEntries",
                columns: new[] { "PeriodID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PostedByUserID",
                table: "JournalEntries",
                column: "PostedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversedEntryID",
                table: "JournalEntries",
                column: "ReversedEntryID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_SourceModule_SourceReferenceID",
                table: "JournalEntries",
                columns: new[] { "SourceModule", "SourceReferenceID" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountID",
                table: "JournalEntryLines",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_CostCenterID",
                table: "JournalEntryLines",
                column: "CostCenterID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_DeletedByUserID",
                table: "JournalEntryLines",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryID",
                table: "JournalEntryLines",
                column: "JournalEntryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBalances");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "ChartOfAccounts");

            migrationBuilder.DropTable(
                name: "CostCenters");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "FiscalPeriods");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 425, DateTimeKind.Utc).AddTicks(5604));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 425, DateTimeKind.Utc).AddTicks(5637));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 425, DateTimeKind.Utc).AddTicks(5638));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 425, DateTimeKind.Utc).AddTicks(5653));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 425, DateTimeKind.Utc).AddTicks(5654));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 452, DateTimeKind.Utc).AddTicks(5253));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 452, DateTimeKind.Utc).AddTicks(5279));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 452, DateTimeKind.Utc).AddTicks(5280));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 452, DateTimeKind.Utc).AddTicks(5282));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 1, 21, 3, 14, 452, DateTimeKind.Utc).AddTicks(5283));
        }
    }
}
