using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileViewPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrackingRecordID",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AchievementObstacles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<byte>(type: "tinyint", nullable: false),
                    OwnerUserID = table.Column<int>(type: "int", nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementObstacles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AchievementObstacles_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementObstacles_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementObstacles_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AchievementObstacles_Users_OwnerUserID",
                        column: x => x.OwnerUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementObstacles_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackingComments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingRecordId = table.Column<int>(type: "int", nullable: false),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingComments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrackingComments_AchievementObstacles_RecordID",
                        column: x => x.RecordID,
                        principalTable: "AchievementObstacles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrackingComments_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrackingComments_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TrackingLinks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingRecordId = table.Column<int>(type: "int", nullable: false),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingLinks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrackingLinks_AchievementObstacles_RecordID",
                        column: x => x.RecordID,
                        principalTable: "AchievementObstacles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7367));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7374));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7375));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7376));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 147, DateTimeKind.Utc).AddTicks(7380));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "Code", "Description" },
                values: new object[] { 143, "File", "File.View", "عرض ملفات التقاعد" });

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7121));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7130));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7131));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7133));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 18, 32, 10, 172, DateTimeKind.Utc).AddTicks(7134));

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TrackingRecordID",
                table: "Documents",
                column: "TrackingRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementObstacles_CreatedByUserID",
                table: "AchievementObstacles",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementObstacles_DeletedByUserID",
                table: "AchievementObstacles",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementObstacles_DepartmentID",
                table: "AchievementObstacles",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementObstacles_OwnerUserID",
                table: "AchievementObstacles",
                column: "OwnerUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementObstacles_UpdatedByUserID",
                table: "AchievementObstacles",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingComments_CreatedByUserID",
                table: "TrackingComments",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingComments_DeletedByUserID",
                table: "TrackingComments",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingComments_RecordID",
                table: "TrackingComments",
                column: "RecordID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingLinks_RecordID",
                table: "TrackingLinks",
                column: "RecordID");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AchievementObstacles_TrackingRecordID",
                table: "Documents",
                column: "TrackingRecordID",
                principalTable: "AchievementObstacles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AchievementObstacles_TrackingRecordID",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "TrackingComments");

            migrationBuilder.DropTable(
                name: "TrackingLinks");

            migrationBuilder.DropTable(
                name: "AchievementObstacles");

            migrationBuilder.DropIndex(
                name: "IX_Documents_TrackingRecordID",
                table: "Documents");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 143);

            migrationBuilder.DropColumn(
                name: "TrackingRecordID",
                table: "Documents");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 730, DateTimeKind.Utc).AddTicks(6960));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 730, DateTimeKind.Utc).AddTicks(6967));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 730, DateTimeKind.Utc).AddTicks(6968));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 730, DateTimeKind.Utc).AddTicks(6970));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 730, DateTimeKind.Utc).AddTicks(6971));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 751, DateTimeKind.Utc).AddTicks(9205));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 751, DateTimeKind.Utc).AddTicks(9211));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 751, DateTimeKind.Utc).AddTicks(9212));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 751, DateTimeKind.Utc).AddTicks(9214));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 11, 0, 24, 56, 751, DateTimeKind.Utc).AddTicks(9215));
        }
    }
}
