using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetAllocationApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalRemarks",
                table: "BudgetAllocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "BudgetAllocations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "BudgetAllocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserID",
                table: "BudgetAllocations",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 737, DateTimeKind.Utc).AddTicks(6805));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 737, DateTimeKind.Utc).AddTicks(6816));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 737, DateTimeKind.Utc).AddTicks(6818));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 737, DateTimeKind.Utc).AddTicks(6820));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 737, DateTimeKind.Utc).AddTicks(6821));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 765, DateTimeKind.Utc).AddTicks(1842));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 765, DateTimeKind.Utc).AddTicks(1848));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 765, DateTimeKind.Utc).AddTicks(1850));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 765, DateTimeKind.Utc).AddTicks(1851));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 19, 23, 30, 765, DateTimeKind.Utc).AddTicks(1852));

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_ApprovedByUserID",
                table: "BudgetAllocations",
                column: "ApprovedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetAllocations_Users_ApprovedByUserID",
                table: "BudgetAllocations",
                column: "ApprovedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetAllocations_Users_ApprovedByUserID",
                table: "BudgetAllocations");

            migrationBuilder.DropIndex(
                name: "IX_BudgetAllocations_ApprovedByUserID",
                table: "BudgetAllocations");

            migrationBuilder.DropColumn(
                name: "ApprovalRemarks",
                table: "BudgetAllocations");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "BudgetAllocations");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "BudgetAllocations");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserID",
                table: "BudgetAllocations");

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
    }
}
