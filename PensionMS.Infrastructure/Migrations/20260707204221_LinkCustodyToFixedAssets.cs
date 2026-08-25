using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkCustodyToFixedAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryCustodies_InventoryItems_InventoryItemID",
                table: "InventoryCustodies");

            migrationBuilder.RenameColumn(
                name: "InventoryItemID",
                table: "InventoryCustodies",
                newName: "AssetID");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryCustodies_InventoryItemID",
                table: "InventoryCustodies",
                newName: "IX_InventoryCustodies_AssetID");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 838, DateTimeKind.Utc).AddTicks(7092));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 838, DateTimeKind.Utc).AddTicks(7101));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 838, DateTimeKind.Utc).AddTicks(7103));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 838, DateTimeKind.Utc).AddTicks(7104));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 838, DateTimeKind.Utc).AddTicks(7105));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 861, DateTimeKind.Utc).AddTicks(3717));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 861, DateTimeKind.Utc).AddTicks(3729));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 861, DateTimeKind.Utc).AddTicks(3730));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 861, DateTimeKind.Utc).AddTicks(3731));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 7, 20, 42, 18, 861, DateTimeKind.Utc).AddTicks(3732));

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryCustodies_FixedAssets_AssetID",
                table: "InventoryCustodies",
                column: "AssetID",
                principalTable: "FixedAssets",
                principalColumn: "AssetID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryCustodies_FixedAssets_AssetID",
                table: "InventoryCustodies");

            migrationBuilder.RenameColumn(
                name: "AssetID",
                table: "InventoryCustodies",
                newName: "InventoryItemID");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryCustodies_AssetID",
                table: "InventoryCustodies",
                newName: "IX_InventoryCustodies_InventoryItemID");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1811));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1819));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1821));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1823));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1824));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6303));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6309));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6311));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6312));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6313));

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryCustodies_InventoryItems_InventoryItemID",
                table: "InventoryCustodies",
                column: "InventoryItemID",
                principalTable: "InventoryItems",
                principalColumn: "InventoryItemID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
