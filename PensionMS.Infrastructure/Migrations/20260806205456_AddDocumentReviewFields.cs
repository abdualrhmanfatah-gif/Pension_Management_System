using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserID",
                table: "Documents",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ReviewedByUserID",
                table: "Documents",
                column: "ReviewedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_ReviewedByUserID",
                table: "Documents",
                column: "ReviewedByUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_ReviewedByUserID",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ReviewedByUserID",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserID",
                table: "Documents");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 137, DateTimeKind.Utc).AddTicks(3433));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 137, DateTimeKind.Utc).AddTicks(3439));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 137, DateTimeKind.Utc).AddTicks(3440));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 137, DateTimeKind.Utc).AddTicks(3442));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 137, DateTimeKind.Utc).AddTicks(3443));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 170, DateTimeKind.Utc).AddTicks(6991));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 170, DateTimeKind.Utc).AddTicks(7000));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 170, DateTimeKind.Utc).AddTicks(7001));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 170, DateTimeKind.Utc).AddTicks(7017));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 3, 0, 17, 18, 170, DateTimeKind.Utc).AddTicks(7018));
        }
    }
}
