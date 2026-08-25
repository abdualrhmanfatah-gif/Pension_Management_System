using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInventoryCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_InventoryCategories_CategoryID",
                table: "InventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryCategories");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_CategoryID",
                table: "InventoryItems");

            // حذف صلاحيات المستخدمين أولاً (قبل حذف الصلاحيات نفسها)
            migrationBuilder.DeleteData(
                table: "UserPermissions",
                keyColumn: "PermissionId",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "UserPermissions",
                keyColumn: "PermissionId",
                keyValue: 129);

            // حذف الأدوار المرتبطة بالصلاحيتين
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "PermissionId",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "PermissionId",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 129);

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "InventoryItems");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 93, DateTimeKind.Utc).AddTicks(4232));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 93, DateTimeKind.Utc).AddTicks(4237));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 93, DateTimeKind.Utc).AddTicks(4239));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 93, DateTimeKind.Utc).AddTicks(4241));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 93, DateTimeKind.Utc).AddTicks(4245));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 118, DateTimeKind.Utc).AddTicks(7500));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 118, DateTimeKind.Utc).AddTicks(7505));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 118, DateTimeKind.Utc).AddTicks(7507));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 118, DateTimeKind.Utc).AddTicks(7508));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 19, 18, 42, 118, DateTimeKind.Utc).AddTicks(7509));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InventoryCategories",
                columns: table => new
                {
                    InventoryCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    CategoryCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCategories", x => x.InventoryCategoryID);
                    table.ForeignKey(
                        name: "FK_InventoryCategories_ChartOfAccounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCategories_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCategories_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 576, DateTimeKind.Utc).AddTicks(5710));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 576, DateTimeKind.Utc).AddTicks(5716));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 576, DateTimeKind.Utc).AddTicks(5717));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 576, DateTimeKind.Utc).AddTicks(5718));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 576, DateTimeKind.Utc).AddTicks(5720));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "Code", "Description" },
                values: new object[,]
                {
                    { 128, "Inventory", "Inventory.Category.View", "عرض تصنيفات المخزون" },
                    { 129, "Inventory", "Inventory.Category.Manage", "إدارة تصنيفات المخزون" }
                });

            // إعادة إدخال RolePermissions و UserPermissions إذا كانت موجودة
            // (سيتم إنشاؤها يدوياً عند الحاجة)

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 598, DateTimeKind.Utc).AddTicks(6101));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 598, DateTimeKind.Utc).AddTicks(6106));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 598, DateTimeKind.Utc).AddTicks(6108));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 598, DateTimeKind.Utc).AddTicks(6123));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 8, 2, 41, 25, 598, DateTimeKind.Utc).AddTicks(6124));

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CategoryID",
                table: "InventoryItems",
                column: "CategoryID");

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

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_CategoryName",
                table: "InventoryCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_CreatedByUserID",
                table: "InventoryCategories",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_UpdatedByUserID",
                table: "InventoryCategories",
                column: "UpdatedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_InventoryCategories_CategoryID",
                table: "InventoryItems",
                column: "CategoryID",
                principalTable: "InventoryCategories",
                principalColumn: "InventoryCategoryID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
