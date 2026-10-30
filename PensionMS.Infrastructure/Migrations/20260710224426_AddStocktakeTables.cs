using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStocktakeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetStocktakes",
                columns: table => new
                {
                    AssetStocktakeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearID = table.Column<int>(type: "int", nullable: false),
                    StocktakeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    VerifiedByUserID = table.Column<int>(type: "int", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetStocktakes", x => x.AssetStocktakeID);
                    table.ForeignKey(
                        name: "FK_AssetStocktakes_FiscalYears_FiscalYearID",
                        column: x => x.FiscalYearID,
                        principalTable: "FiscalYears",
                        principalColumn: "FiscalYearID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetStocktakes_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetStocktakes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssetStocktakes_Users_VerifiedByUserID",
                        column: x => x.VerifiedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimpleStocktakes",
                columns: table => new
                {
                    SimpleStocktakeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearID = table.Column<int>(type: "int", nullable: false),
                    StocktakeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    CompletedByUserID = table.Column<int>(type: "int", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    ItemsWithVariance = table.Column<int>(type: "int", nullable: false),
                    TotalVarianceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JournalEntryID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleStocktakes", x => x.SimpleStocktakeID);
                    table.ForeignKey(
                        name: "FK_SimpleStocktakes_FiscalYears_FiscalYearID",
                        column: x => x.FiscalYearID,
                        principalTable: "FiscalYears",
                        principalColumn: "FiscalYearID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimpleStocktakes_JournalEntries_JournalEntryID",
                        column: x => x.JournalEntryID,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimpleStocktakes_Users_CompletedByUserID",
                        column: x => x.CompletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimpleStocktakes_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimpleStocktakes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AssetStocktakeLines",
                columns: table => new
                {
                    StocktakeLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetStocktakeID = table.Column<int>(type: "int", nullable: false),
                    FixedAssetID = table.Column<int>(type: "int", nullable: false),
                    AssetNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetBookValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetStocktakeLines", x => x.StocktakeLineID);
                    table.ForeignKey(
                        name: "FK_AssetStocktakeLines_AssetStocktakes_AssetStocktakeID",
                        column: x => x.AssetStocktakeID,
                        principalTable: "AssetStocktakes",
                        principalColumn: "AssetStocktakeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetStocktakeLines_FixedAssets_FixedAssetID",
                        column: x => x.FixedAssetID,
                        principalTable: "FixedAssets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimpleStocktakeLines",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimpleStocktakeID = table.Column<int>(type: "int", nullable: false),
                    InventoryItemID = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemQuantity = table.Column<int>(type: "int", nullable: false),
                    PhysicalQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleStocktakeLines", x => x.LineID);
                    table.ForeignKey(
                        name: "FK_SimpleStocktakeLines_InventoryItems_InventoryItemID",
                        column: x => x.InventoryItemID,
                        principalTable: "InventoryItems",
                        principalColumn: "InventoryItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimpleStocktakeLines_SimpleStocktakes_SimpleStocktakeID",
                        column: x => x.SimpleStocktakeID,
                        principalTable: "SimpleStocktakes",
                        principalColumn: "SimpleStocktakeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 413, DateTimeKind.Utc).AddTicks(6285));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 413, DateTimeKind.Utc).AddTicks(6295));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 413, DateTimeKind.Utc).AddTicks(6297));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 413, DateTimeKind.Utc).AddTicks(6299));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 413, DateTimeKind.Utc).AddTicks(6301));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 432, DateTimeKind.Utc).AddTicks(7780));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 432, DateTimeKind.Utc).AddTicks(7786));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 432, DateTimeKind.Utc).AddTicks(7788));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 432, DateTimeKind.Utc).AddTicks(7789));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 10, 22, 44, 23, 432, DateTimeKind.Utc).AddTicks(7790));

            migrationBuilder.CreateIndex(
                name: "IX_AssetStocktakeLines_AssetStocktakeID",
                table: "AssetStocktakeLines",
                column: "AssetStocktakeID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetStocktakeLines_FixedAssetID",
                table: "AssetStocktakeLines",
                column: "FixedAssetID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetStocktakes_CreatedByUserID",
                table: "AssetStocktakes",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetStocktakes_DeletedByUserID",
                table: "AssetStocktakes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetStocktakes_FiscalYearID",
                table: "AssetStocktakes",
                column: "FiscalYearID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetStocktakes_VerifiedByUserID",
                table: "AssetStocktakes",
                column: "VerifiedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleStocktakeLines_InventoryItemID",
                table: "SimpleStocktakeLines",
                column: "InventoryItemID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleStocktakeLines_SimpleStocktakeID",
                table: "SimpleStocktakeLines",
                column: "SimpleStocktakeID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleStocktakes_CompletedByUserID",
                table: "SimpleStocktakes",
                column: "CompletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleStocktakes_CreatedByUserID",
                table: "SimpleStocktakes",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleStocktakes_DeletedByUserID",
                table: "SimpleStocktakes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleStocktakes_FiscalYearID",
                table: "SimpleStocktakes",
                column: "FiscalYearID");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleStocktakes_JournalEntryID",
                table: "SimpleStocktakes",
                column: "JournalEntryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetStocktakeLines");

            migrationBuilder.DropTable(
                name: "SimpleStocktakeLines");

            migrationBuilder.DropTable(
                name: "AssetStocktakes");

            migrationBuilder.DropTable(
                name: "SimpleStocktakes");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5235));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5242));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5244));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5263));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 814, DateTimeKind.Utc).AddTicks(5264));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9553));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9563));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9565));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9566));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 9, 19, 27, 20, 834, DateTimeKind.Utc).AddTicks(9567));
        }
    }
}
