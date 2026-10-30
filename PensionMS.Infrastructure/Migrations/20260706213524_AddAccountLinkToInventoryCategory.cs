using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLinkToInventoryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountID",
                table: "InventoryCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryCode",
                table: "InventoryCategories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1233));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1242));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1244));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1245));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1246));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2741));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2748));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2751));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2752));

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_AccountID",
                table: "InventoryCategories",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_CategoryCode",
                table: "InventoryCategories",
                column: "CategoryCode",
                unique: true,
                filter: "[CategoryCode] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryCategories_ChartOfAccounts_AccountID",
                table: "InventoryCategories",
                column: "AccountID",
                principalTable: "ChartOfAccounts",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryCategories_ChartOfAccounts_AccountID",
                table: "InventoryCategories");

            migrationBuilder.DropIndex(
                name: "IX_InventoryCategories_AccountID",
                table: "InventoryCategories");

            migrationBuilder.DropIndex(
                name: "IX_InventoryCategories_CategoryCode",
                table: "InventoryCategories");

            migrationBuilder.DropColumn(
                name: "AccountID",
                table: "InventoryCategories");

            migrationBuilder.DropColumn(
                name: "CategoryCode",
                table: "InventoryCategories");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 94, DateTimeKind.Utc).AddTicks(130));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 94, DateTimeKind.Utc).AddTicks(136));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 94, DateTimeKind.Utc).AddTicks(137));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 94, DateTimeKind.Utc).AddTicks(138));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 94, DateTimeKind.Utc).AddTicks(142));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 123, DateTimeKind.Utc).AddTicks(2126));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 123, DateTimeKind.Utc).AddTicks(2132));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 123, DateTimeKind.Utc).AddTicks(2134));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 123, DateTimeKind.Utc).AddTicks(2135));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 20, 48, 123, DateTimeKind.Utc).AddTicks(2137));
        }
    }
}
