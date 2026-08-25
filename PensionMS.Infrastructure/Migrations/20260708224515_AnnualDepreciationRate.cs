using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AnnualDepreciationRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepreciationMethod",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "UsefulLifeMonths",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "UsefulLifeMonths",
                table: "FixedAssetCategories");

            migrationBuilder.AddColumn<decimal>(
                name: "DepreciationRate",
                table: "FixedAssets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepreciationRate",
                table: "FixedAssetCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepreciationRate",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "DepreciationRate",
                table: "FixedAssetCategories");

            migrationBuilder.AddColumn<int>(
                name: "DepreciationMethod",
                table: "FixedAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsefulLifeMonths",
                table: "FixedAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsefulLifeMonths",
                table: "FixedAssetCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 605, DateTimeKind.Utc).AddTicks(2721));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 605, DateTimeKind.Utc).AddTicks(2725));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 605, DateTimeKind.Utc).AddTicks(2727));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 605, DateTimeKind.Utc).AddTicks(2729));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 605, DateTimeKind.Utc).AddTicks(2731));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 629, DateTimeKind.Utc).AddTicks(6281));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 629, DateTimeKind.Utc).AddTicks(6288));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 629, DateTimeKind.Utc).AddTicks(6289));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 629, DateTimeKind.Utc).AddTicks(6291));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 20, 16, 58, 629, DateTimeKind.Utc).AddTicks(6292));
        }
    }
}
