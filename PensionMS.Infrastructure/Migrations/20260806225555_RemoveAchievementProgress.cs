using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAchievementProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progress",
                table: "AchievementObstacles");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Progress",
                table: "AchievementObstacles",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 392, DateTimeKind.Utc).AddTicks(950));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 392, DateTimeKind.Utc).AddTicks(956));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 392, DateTimeKind.Utc).AddTicks(958));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 392, DateTimeKind.Utc).AddTicks(959));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 392, DateTimeKind.Utc).AddTicks(960));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 414, DateTimeKind.Utc).AddTicks(7960));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 414, DateTimeKind.Utc).AddTicks(7966));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 414, DateTimeKind.Utc).AddTicks(7968));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 414, DateTimeKind.Utc).AddTicks(7969));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 22, 27, 49, 414, DateTimeKind.Utc).AddTicks(7971));
        }
    }
}
