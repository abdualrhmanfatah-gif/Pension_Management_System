using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisionalExpenseSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BudgetAllocationID",
                table: "OperationalExpenses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClassifiedAt",
                table: "OperationalExpenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassifiedByUserID",
                table: "OperationalExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProvisional",
                table: "OperationalExpenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 79, DateTimeKind.Utc).AddTicks(5672));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 79, DateTimeKind.Utc).AddTicks(5715));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 79, DateTimeKind.Utc).AddTicks(5717));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 79, DateTimeKind.Utc).AddTicks(5719));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 79, DateTimeKind.Utc).AddTicks(5721));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 107, DateTimeKind.Utc).AddTicks(6912));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 107, DateTimeKind.Utc).AddTicks(6936));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 107, DateTimeKind.Utc).AddTicks(6938));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 107, DateTimeKind.Utc).AddTicks(6939));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 29, 22, 17, 48, 107, DateTimeKind.Utc).AddTicks(6940));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassifiedAt",
                table: "OperationalExpenses");

            migrationBuilder.DropColumn(
                name: "ClassifiedByUserID",
                table: "OperationalExpenses");

            migrationBuilder.DropColumn(
                name: "IsProvisional",
                table: "OperationalExpenses");

            migrationBuilder.AlterColumn<int>(
                name: "BudgetAllocationID",
                table: "OperationalExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5057));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5063));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5066));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5068));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5070));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1618));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1624));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1626));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1627));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1628));
        }
    }
}
