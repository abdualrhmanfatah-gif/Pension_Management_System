using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOperationalBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationalExpenses_BudgetAllocations_BudgetAllocationID",
                table: "OperationalExpenses");

            migrationBuilder.DropTable(
                name: "BudgetAllocations");

            migrationBuilder.DropTable(
                name: "BudgetItems");

            migrationBuilder.DropTable(
                name: "BudgetSections");

            migrationBuilder.DropTable(
                name: "BudgetChapters");

            migrationBuilder.DropIndex(
                name: "IX_OperationalExpenses_BudgetAllocationID",
                table: "OperationalExpenses");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 120);

            migrationBuilder.DropColumn(
                name: "BudgetAllocationID",
                table: "OperationalExpenses");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 618, DateTimeKind.Utc).AddTicks(1636));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 618, DateTimeKind.Utc).AddTicks(1677));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 618, DateTimeKind.Utc).AddTicks(1679));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 618, DateTimeKind.Utc).AddTicks(1680));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 618, DateTimeKind.Utc).AddTicks(1681));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 644, DateTimeKind.Utc).AddTicks(9996));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 645, DateTimeKind.Utc).AddTicks(20));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 645, DateTimeKind.Utc).AddTicks(22));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 645, DateTimeKind.Utc).AddTicks(23));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 22, 18, 7, 645, DateTimeKind.Utc).AddTicks(25));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetAllocationID",
                table: "OperationalExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BudgetChapters",
                columns: table => new
                {
                    ChapterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetChapters", x => x.ChapterID);
                    table.ForeignKey(
                        name: "FK_BudgetChapters_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BudgetSections",
                columns: table => new
                {
                    SectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChapterID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetSections", x => x.SectionID);
                    table.ForeignKey(
                        name: "FK_BudgetSections_BudgetChapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "BudgetChapters",
                        principalColumn: "ChapterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetSections_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BudgetItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_BudgetItems_BudgetSections_SectionID",
                        column: x => x.SectionID,
                        principalTable: "BudgetSections",
                        principalColumn: "SectionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetItems_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BudgetAllocations",
                columns: table => new
                {
                    AllocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovedByUserID = table.Column<int>(type: "int", nullable: true),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    QuarterID = table.Column<int>(type: "int", nullable: false),
                    AmountAllocated = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetAllocations", x => x.AllocationID);
                    table.ForeignKey(
                        name: "FK_BudgetAllocations_BudgetItems_ItemID",
                        column: x => x.ItemID,
                        principalTable: "BudgetItems",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocations_Quarters_QuarterID",
                        column: x => x.QuarterID,
                        principalTable: "Quarters",
                        principalColumn: "QuarterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocations_Users_ApprovedByUserID",
                        column: x => x.ApprovedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocations_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(321));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(357));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(359));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(361));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(362));

            migrationBuilder.InsertData(
                table: "BudgetChapters",
                columns: new[] { "ChapterID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "31", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الأول: المعاشات والأجور", null },
                    { 2, "32", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الثاني: المستلزمات السلعية", null },
                    { 3, "33", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الثالث: المصروفات الخدمية", null },
                    { 4, "34", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الرابع: المصروفات التحويلية والإعانات", null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "Code", "Description" },
                values: new object[,]
                {
                    { 71, "OperationalBudget", "OperationalBudget.View", "عرض الميزانية التشغيلية" },
                    { 72, "OperationalBudget", "OperationalBudget.Manage", "إدارة الميزانية التشغيلية" },
                    { 92, "OperationalBudget", "OperationalBudget.Add", "إضافة ميزانية تشغيلية" },
                    { 93, "OperationalBudget", "OperationalBudget.Edit", "تعديل ميزانية تشغيلية" },
                    { 94, "OperationalBudget", "OperationalBudget.Delete", "حذف ميزانية تشغيلية" },
                    { 115, "OperationalBudget", "OperationalBudget.Reports.View", "عرض تقارير الميزانية التشغيلية" },
                    { 119, "OperationalBudget", "BudgetStructure.View", "عرض هيكل الميزانية التعريفي" },
                    { 120, "OperationalBudget", "BudgetStructure.Manage", "إدارة هيكل الميزانية التعريفي" }
                });

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9323));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9620));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9623));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9625));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9628));

            migrationBuilder.InsertData(
                table: "BudgetSections",
                columns: new[] { "SectionID", "ChapterID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "311", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "معاشات وأجور نقدية", null },
                    { 2, 1, "312", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "البدلات والتعويضات الأخرى", null },
                    { 3, 1, "313", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "المزايا العينية", null },
                    { 4, 1, "314", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "المكافآت", null },
                    { 5, 1, "316", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "التأمينات والتقاعد", null },
                    { 6, 2, "322", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الوقود والزيوت والقوى المحركة والمياه", null },
                    { 7, 2, "323", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "قطع الغيار واللوازم", null },
                    { 8, 2, "325", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "القرطاسية والمطبوعات", null }
                });

            migrationBuilder.InsertData(
                table: "BudgetItems",
                columns: new[] { "ItemID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "SectionID", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "3111", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "معاشات وأجور دائمة", 1, null },
                    { 2, "3112", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "معاشات وأجور تعاقدية", 1, null },
                    { 3, "3121", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تمثيل + مظهر", 2, null },
                    { 4, "3122", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل مناطق حرة (كادر) + الأمن", 2, null },
                    { 5, "31233", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل حضور جلسات", 2, null },
                    { 6, "3124", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تخرج", 2, null },
                    { 7, "3125", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل مسؤولية", 2, null },
                    { 8, "3126", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تعويض السكن", 2, null },
                    { 9, "3127", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تعويض العمل الإضافي", 2, null },
                    { 10, "3128", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل انتقال", 2, null },
                    { 11, "3131", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "تكاليف أغذية العاملين", 3, null },
                    { 12, "3132", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "تكاليف ملابس العاملين", 3, null },
                    { 13, "3133", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "خدمات ثقافية واجتماعية للعاملين", 3, null },
                    { 14, "3134", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "تكاليف علاج العاملين", 3, null },
                    { 15, "3141", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "المكافآت التشجيعية", 4, null },
                    { 16, "3142", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "إكرامية رمضان", 4, null },
                    { 17, "3161", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "حصة الحكومة (قسط التقاعد + مساهمة الموظف)", 5, null },
                    { 18, "3162", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "حصة صندوق التأمين إصابة العمل", 5, null },
                    { 19, "3221", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مواد بترولية", 6, null },
                    { 20, "3222", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "كهرباء", 6, null },
                    { 21, "3223", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مياه", 6, null },
                    { 22, "3224", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "زيوت وشحوم", 6, null },
                    { 23, "3225", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "غاز", 6, null },
                    { 24, "3231", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "قطع غيار الآلات والسيارات", 7, null },
                    { 25, "3232", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "لوازم ومهمات الصيانة", 7, null },
                    { 26, "3233", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "قطع غيار التجهيزات الإدارية", 7, null },
                    { 27, "3251", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "القرطاسية واللوازم المكتبية", 8, null },
                    { 28, "3252", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مستلزمات سلعية متنوعة", 8, null },
                    { 29, "3253", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مطبوعات دورية", 8, null },
                    { 30, "3254", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "شتول وأغراس", 8, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_BudgetAllocationID",
                table: "OperationalExpenses",
                column: "BudgetAllocationID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_ApprovedByUserID",
                table: "BudgetAllocations",
                column: "ApprovedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_DeletedByUserID",
                table: "BudgetAllocations",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_ItemID",
                table: "BudgetAllocations",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_QuarterID_ItemID",
                table: "BudgetAllocations",
                columns: new[] { "QuarterID", "ItemID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetChapters_Code",
                table: "BudgetChapters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetChapters_DeletedByUserID",
                table: "BudgetChapters",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_Code",
                table: "BudgetItems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_DeletedByUserID",
                table: "BudgetItems",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_SectionID",
                table: "BudgetItems",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetSections_ChapterID",
                table: "BudgetSections",
                column: "ChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetSections_Code",
                table: "BudgetSections",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetSections_DeletedByUserID",
                table: "BudgetSections",
                column: "DeletedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationalExpenses_BudgetAllocations_BudgetAllocationID",
                table: "OperationalExpenses",
                column: "BudgetAllocationID",
                principalTable: "BudgetAllocations",
                principalColumn: "AllocationID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
