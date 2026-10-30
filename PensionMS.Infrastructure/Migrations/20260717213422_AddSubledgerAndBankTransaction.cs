using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubledgerAndBankTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountID",
                table: "Tenants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckDueDate",
                table: "JournalEntryLines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckNo",
                table: "JournalEntryLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReconciled",
                table: "JournalEntryLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "JournalEntryLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciledAt",
                table: "JournalEntryLines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReconciledByUserID",
                table: "JournalEntryLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubledgerEntityID",
                table: "JournalEntryLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubledgerEntityType",
                table: "JournalEntryLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountCategory",
                table: "ChartOfAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasSubledger",
                table: "ChartOfAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubledgerType",
                table: "ChartOfAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountID",
                table: "Banks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNo",
                table: "Banks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwiftCode",
                table: "Banks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankID = table.Column<int>(type: "int", nullable: false),
                    JournalEntryID = table.Column<int>(type: "int", nullable: false),
                    LineID = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsReconciled = table.Column<bool>(type: "bit", nullable: false),
                    ReconciledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankStatementRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankTransactions", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_BankTransactions_Banks_BankID",
                        column: x => x.BankID,
                        principalTable: "Banks",
                        principalColumn: "BankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactions_JournalEntries_JournalEntryID",
                        column: x => x.JournalEntryID,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactions_JournalEntryLines_LineID",
                        column: x => x.LineID,
                        principalTable: "JournalEntryLines",
                        principalColumn: "LineID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransactions_Users_DeletedByUserID",
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
                value: new DateTime(2026, 7, 17, 21, 34, 19, 640, DateTimeKind.Utc).AddTicks(7471));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 640, DateTimeKind.Utc).AddTicks(7479));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 640, DateTimeKind.Utc).AddTicks(7481));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 640, DateTimeKind.Utc).AddTicks(7483));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 640, DateTimeKind.Utc).AddTicks(7485));

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "BankID",
                keyValue: 1,
                columns: new[] { "AccountID", "BankAccountNo", "SwiftCode" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 664, DateTimeKind.Utc).AddTicks(1640));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 664, DateTimeKind.Utc).AddTicks(1646));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 664, DateTimeKind.Utc).AddTicks(1648));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 664, DateTimeKind.Utc).AddTicks(1649));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 21, 34, 19, 664, DateTimeKind.Utc).AddTicks(1650));

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_AccountID",
                table: "Tenants",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryID_LineNumber",
                table: "JournalEntryLines",
                columns: new[] { "JournalEntryID", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_ReconciledByUserID",
                table: "JournalEntryLines",
                column: "ReconciledByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_SubledgerEntityType",
                table: "JournalEntryLines",
                column: "SubledgerEntityType");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PeriodID",
                table: "JournalEntries",
                column: "PeriodID");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_AccountCategory",
                table: "ChartOfAccounts",
                column: "AccountCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_AccountID",
                table: "Banks",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_BankID_TransactionDate",
                table: "BankTransactions",
                columns: new[] { "BankID", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_DeletedByUserID",
                table: "BankTransactions",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_IsReconciled",
                table: "BankTransactions",
                column: "IsReconciled");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_JournalEntryID",
                table: "BankTransactions",
                column: "JournalEntryID");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_LineID",
                table: "BankTransactions",
                column: "LineID");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_ChartOfAccounts_AccountID",
                table: "Banks",
                column: "AccountID",
                principalTable: "ChartOfAccounts",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntryLines_Users_ReconciledByUserID",
                table: "JournalEntryLines",
                column: "ReconciledByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_ChartOfAccounts_AccountID",
                table: "Tenants",
                column: "AccountID",
                principalTable: "ChartOfAccounts",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banks_ChartOfAccounts_AccountID",
                table: "Banks");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntryLines_Users_ReconciledByUserID",
                table: "JournalEntryLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_ChartOfAccounts_AccountID",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_AccountID",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_JournalEntryID_LineNumber",
                table: "JournalEntryLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_ReconciledByUserID",
                table: "JournalEntryLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_SubledgerEntityType",
                table: "JournalEntryLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_PeriodID",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_ChartOfAccounts_AccountCategory",
                table: "ChartOfAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Banks_AccountID",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "AccountID",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CheckDueDate",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "CheckNo",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "IsReconciled",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ReconciledAt",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ReconciledByUserID",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "SubledgerEntityID",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "SubledgerEntityType",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "AccountCategory",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "HasSubledger",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "SubledgerType",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "AccountID",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "BankAccountNo",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "SwiftCode",
                table: "Banks");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7367));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7374));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7375));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7376));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7380));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7121));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7130));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7131));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7133));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7134));
        }
    }
}
