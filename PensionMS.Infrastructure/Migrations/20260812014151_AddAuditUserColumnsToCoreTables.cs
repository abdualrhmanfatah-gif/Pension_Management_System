using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditUserColumnsToCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "SettlementItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "SettlementItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "SalaryRecipients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "SalaryRecipients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "RetirementFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "RetirementFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "PensionSuspensions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "PaymentBooks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "PaymentBooks",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 670, DateTimeKind.Utc).AddTicks(8069));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 670, DateTimeKind.Utc).AddTicks(8082));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 670, DateTimeKind.Utc).AddTicks(8085));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 670, DateTimeKind.Utc).AddTicks(8088));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 670, DateTimeKind.Utc).AddTicks(8090));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 718, DateTimeKind.Utc).AddTicks(8568));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 718, DateTimeKind.Utc).AddTicks(8574));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 718, DateTimeKind.Utc).AddTicks(8576));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 718, DateTimeKind.Utc).AddTicks(8579));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 1, 41, 46, 718, DateTimeKind.Utc).AddTicks(8585));

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_CreatedByUserID",
                table: "SettlementItems",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_UpdatedByUserID",
                table: "SettlementItems",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_CreatedByUserID",
                table: "SalaryRecipients",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_UpdatedByUserID",
                table: "SalaryRecipients",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_CreatedByUserID",
                table: "RetirementFiles",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_UpdatedByUserID",
                table: "RetirementFiles",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PensionSuspensions_UpdatedByUserID",
                table: "PensionSuspensions",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBooks_CreatedByUserID",
                table: "PaymentBooks",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBooks_UpdatedByUserID",
                table: "PaymentBooks",
                column: "UpdatedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentBooks_Users_CreatedByUserID",
                table: "PaymentBooks",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentBooks_Users_UpdatedByUserID",
                table: "PaymentBooks",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PensionSuspensions_Users_UpdatedByUserID",
                table: "PensionSuspensions",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RetirementFiles_Users_CreatedByUserID",
                table: "RetirementFiles",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RetirementFiles_Users_UpdatedByUserID",
                table: "RetirementFiles",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryRecipients_Users_CreatedByUserID",
                table: "SalaryRecipients",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryRecipients_Users_UpdatedByUserID",
                table: "SalaryRecipients",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementItems_Users_CreatedByUserID",
                table: "SettlementItems",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementItems_Users_UpdatedByUserID",
                table: "SettlementItems",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentBooks_Users_CreatedByUserID",
                table: "PaymentBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentBooks_Users_UpdatedByUserID",
                table: "PaymentBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_PensionSuspensions_Users_UpdatedByUserID",
                table: "PensionSuspensions");

            migrationBuilder.DropForeignKey(
                name: "FK_RetirementFiles_Users_CreatedByUserID",
                table: "RetirementFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_RetirementFiles_Users_UpdatedByUserID",
                table: "RetirementFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryRecipients_Users_CreatedByUserID",
                table: "SalaryRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryRecipients_Users_UpdatedByUserID",
                table: "SalaryRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementItems_Users_CreatedByUserID",
                table: "SettlementItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementItems_Users_UpdatedByUserID",
                table: "SettlementItems");

            migrationBuilder.DropIndex(
                name: "IX_SettlementItems_CreatedByUserID",
                table: "SettlementItems");

            migrationBuilder.DropIndex(
                name: "IX_SettlementItems_UpdatedByUserID",
                table: "SettlementItems");

            migrationBuilder.DropIndex(
                name: "IX_SalaryRecipients_CreatedByUserID",
                table: "SalaryRecipients");

            migrationBuilder.DropIndex(
                name: "IX_SalaryRecipients_UpdatedByUserID",
                table: "SalaryRecipients");

            migrationBuilder.DropIndex(
                name: "IX_RetirementFiles_CreatedByUserID",
                table: "RetirementFiles");

            migrationBuilder.DropIndex(
                name: "IX_RetirementFiles_UpdatedByUserID",
                table: "RetirementFiles");

            migrationBuilder.DropIndex(
                name: "IX_PensionSuspensions_UpdatedByUserID",
                table: "PensionSuspensions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentBooks_CreatedByUserID",
                table: "PaymentBooks");

            migrationBuilder.DropIndex(
                name: "IX_PaymentBooks_UpdatedByUserID",
                table: "PaymentBooks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "SettlementItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "SettlementItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "SalaryRecipients");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "SalaryRecipients");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "RetirementFiles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "RetirementFiles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "PensionSuspensions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "PaymentBooks");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "PaymentBooks");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 367, DateTimeKind.Utc).AddTicks(3229));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 367, DateTimeKind.Utc).AddTicks(3237));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 367, DateTimeKind.Utc).AddTicks(3239));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 367, DateTimeKind.Utc).AddTicks(3240));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 367, DateTimeKind.Utc).AddTicks(3241));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 388, DateTimeKind.Utc).AddTicks(5502));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 388, DateTimeKind.Utc).AddTicks(5526));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 388, DateTimeKind.Utc).AddTicks(5527));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 388, DateTimeKind.Utc).AddTicks(5529));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 55, 52, 388, DateTimeKind.Utc).AddTicks(5532));
        }
    }
}
