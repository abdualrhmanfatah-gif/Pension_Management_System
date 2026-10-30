using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 563, DateTimeKind.Utc).AddTicks(918));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 563, DateTimeKind.Utc).AddTicks(931));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 563, DateTimeKind.Utc).AddTicks(934));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 563, DateTimeKind.Utc).AddTicks(936));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 563, DateTimeKind.Utc).AddTicks(938));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "Code", "Description" },
                values: new object[,]
                {
                    { 145, "Report", "Report.Pension.View", "عرض تقارير المعاشات" },
                    { 146, "Report", "Report.Financial.View", "عرض التقارير المالية" },
                    { 147, "Report", "Report.Entitlements.View", "عرض تقارير الاستحقاقات" },
                    { 148, "Report", "Report.Documents.View", "عرض تقارير الوثائق" },
                    { 149, "Report", "Report.Export", "تصدير التقارير" },
                    { 150, "Report", "Report.Admin.View", "عرض التقارير الإدارية" }
                });

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 614, DateTimeKind.Utc).AddTicks(9981));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 614, DateTimeKind.Utc).AddTicks(9991));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 614, DateTimeKind.Utc).AddTicks(9993));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 614, DateTimeKind.Utc).AddTicks(9995));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 1, 33, 5, 614, DateTimeKind.Utc).AddTicks(9996));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 147);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 148);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 149);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 150);

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
        }
    }
}
