using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClosedAtClosedByUserIDToFiscalYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "FiscalYears",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosedByUserID",
                table: "FiscalYears",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_ClosedByUserID",
                table: "FiscalYears",
                column: "ClosedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_FiscalYears_Users_ClosedByUserID",
                table: "FiscalYears",
                column: "ClosedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FiscalYears_Users_ClosedByUserID",
                table: "FiscalYears");

            migrationBuilder.DropIndex(
                name: "IX_FiscalYears_ClosedByUserID",
                table: "FiscalYears");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "FiscalYears");

            migrationBuilder.DropColumn(
                name: "ClosedByUserID",
                table: "FiscalYears");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 821, DateTimeKind.Utc).AddTicks(387));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 821, DateTimeKind.Utc).AddTicks(394));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 821, DateTimeKind.Utc).AddTicks(396));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 821, DateTimeKind.Utc).AddTicks(398));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 821, DateTimeKind.Utc).AddTicks(400));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 850, DateTimeKind.Utc).AddTicks(7685));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 850, DateTimeKind.Utc).AddTicks(7690));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 850, DateTimeKind.Utc).AddTicks(7692));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 850, DateTimeKind.Utc).AddTicks(7693));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 5, 19, 11, 38, 850, DateTimeKind.Utc).AddTicks(7695));
        }
    }
}
