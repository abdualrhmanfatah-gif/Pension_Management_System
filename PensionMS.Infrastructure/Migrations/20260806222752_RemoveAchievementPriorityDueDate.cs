using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAchievementPriorityDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "AchievementObstacles");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "AchievementObstacles");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "AchievementObstacles");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "AchievementObstacles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "AchievementObstacles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "AchievementObstacles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 707, DateTimeKind.Utc).AddTicks(8822));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 707, DateTimeKind.Utc).AddTicks(8829));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 707, DateTimeKind.Utc).AddTicks(8831));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 707, DateTimeKind.Utc).AddTicks(8833));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 707, DateTimeKind.Utc).AddTicks(8836));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 728, DateTimeKind.Utc).AddTicks(8771));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 728, DateTimeKind.Utc).AddTicks(8778));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 728, DateTimeKind.Utc).AddTicks(8779));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 728, DateTimeKind.Utc).AddTicks(8780));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 6, 20, 54, 53, 728, DateTimeKind.Utc).AddTicks(8781));
        }
    }
}
