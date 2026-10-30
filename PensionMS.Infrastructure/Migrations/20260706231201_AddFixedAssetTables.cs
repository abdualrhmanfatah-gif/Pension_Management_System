using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedAssetTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FixedAssetTypes",
                columns: table => new
                {
                    AssetTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetTypes", x => x.AssetTypeID);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssetTypeID = table.Column<int>(type: "int", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: false),
                    SalvagePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AssetAccountID = table.Column<int>(type: "int", nullable: true),
                    AccumDeprAccountID = table.Column<int>(type: "int", nullable: true),
                    DeprExpenseAccountID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetCategories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_FixedAssetCategories_ChartOfAccounts_AccumDeprAccountID",
                        column: x => x.AccumDeprAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetCategories_ChartOfAccounts_AssetAccountID",
                        column: x => x.AssetAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetCategories_ChartOfAccounts_DeprExpenseAccountID",
                        column: x => x.DeprExpenseAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetCategories_FixedAssetTypes_AssetTypeID",
                        column: x => x.AssetTypeID,
                        principalTable: "FixedAssetTypes",
                        principalColumn: "AssetTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetCategories_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetCategories_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssets",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    AssetTypeID = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcquiredCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalvageValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: false),
                    DepreciationMethod = table.Column<int>(type: "int", nullable: false),
                    AccumulatedDepreciation = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetBookValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastDepreciationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustodianEmployeeID = table.Column<int>(type: "int", nullable: true),
                    AssetAccountID = table.Column<int>(type: "int", nullable: false),
                    AccumDeprAccountID = table.Column<int>(type: "int", nullable: false),
                    DeprExpenseAccountID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisposalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisposalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssets", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_FixedAssets_ChartOfAccounts_AccumDeprAccountID",
                        column: x => x.AccumDeprAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_ChartOfAccounts_AssetAccountID",
                        column: x => x.AssetAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_ChartOfAccounts_DeprExpenseAccountID",
                        column: x => x.DeprExpenseAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_Employees_CustodianEmployeeID",
                        column: x => x.CustodianEmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_FixedAssetCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "FixedAssetCategories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_FixedAssetTypes_AssetTypeID",
                        column: x => x.AssetTypeID,
                        principalTable: "FixedAssetTypes",
                        principalColumn: "AssetTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FixedAssets_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetBuildingDetails",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    AreaSqm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BuiltAreaSqm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    FloorCount = table.Column<int>(type: "int", nullable: false),
                    OwnershipDocNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetBuildingDetails", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_AssetBuildingDetails_FixedAssets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "FixedAssets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetFurnitureDetails",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetFurnitureDetails", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_AssetFurnitureDetails_FixedAssets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "FixedAssets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetITDetails",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProcessorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProcessorSpeed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RamSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HardDiskType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HardDiskSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GpuModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScreenSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScreenResolution = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasWebcam = table.Column<bool>(type: "bit", nullable: false),
                    HasWifi = table.Column<bool>(type: "bit", nullable: false),
                    HasBluetooth = table.Column<bool>(type: "bit", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MacAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WarrantyExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetITDetails", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_AssetITDetails_FixedAssets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "FixedAssets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetLandDetails",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    PlotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LandAreaSqm = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    OwnershipDocNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OwnershipDocDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetLandDetails", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_AssetLandDetails_FixedAssets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "FixedAssets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetVehicleDetails",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlateLetter1 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PlateLetter2 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PlateLetter3 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EngineNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChassisNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransmissionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CylinderCount = table.Column<int>(type: "int", nullable: true),
                    EngineCapacity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HorsePower = table.Column<int>(type: "int", nullable: true),
                    WheelCount = table.Column<int>(type: "int", nullable: false),
                    TireSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModelYear = table.Column<int>(type: "int", nullable: true),
                    RegistrationExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsuranceExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Mileage = table.Column<int>(type: "int", nullable: false),
                    HasGps = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetVehicleDetails", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_AssetVehicleDetails_FixedAssets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "FixedAssets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepreciationSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    PeriodYear = table.Column<int>(type: "int", nullable: false),
                    PeriodMonth = table.Column<int>(type: "int", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepreciationAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AccumulatedDepreciation = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetBookValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JournalEntryID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepreciationSchedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_DepreciationSchedules_FixedAssets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "FixedAssets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepreciationSchedules_JournalEntries_JournalEntryID",
                        column: x => x.JournalEntryID,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1811));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1819));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1821));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1823));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 88, DateTimeKind.Utc).AddTicks(1824));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6303));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6309));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6311));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6312));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 23, 11, 59, 117, DateTimeKind.Utc).AddTicks(6313));

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationSchedules_AssetID_PeriodYear_PeriodMonth",
                table: "DepreciationSchedules",
                columns: new[] { "AssetID", "PeriodYear", "PeriodMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationSchedules_JournalEntryID",
                table: "DepreciationSchedules",
                column: "JournalEntryID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_AccumDeprAccountID",
                table: "FixedAssetCategories",
                column: "AccumDeprAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_AssetAccountID",
                table: "FixedAssetCategories",
                column: "AssetAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_AssetTypeID",
                table: "FixedAssetCategories",
                column: "AssetTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_CategoryCode",
                table: "FixedAssetCategories",
                column: "CategoryCode",
                unique: true,
                filter: "[CategoryCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_CategoryName",
                table: "FixedAssetCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_CreatedByUserID",
                table: "FixedAssetCategories",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_DeprExpenseAccountID",
                table: "FixedAssetCategories",
                column: "DeprExpenseAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_UpdatedByUserID",
                table: "FixedAssetCategories",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_AccumDeprAccountID",
                table: "FixedAssets",
                column: "AccumDeprAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_AssetAccountID",
                table: "FixedAssets",
                column: "AssetAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_AssetNumber",
                table: "FixedAssets",
                column: "AssetNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_AssetTypeID",
                table: "FixedAssets",
                column: "AssetTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CategoryID",
                table: "FixedAssets",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CreatedByUserID",
                table: "FixedAssets",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CustodianEmployeeID",
                table: "FixedAssets",
                column: "CustodianEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_DeletedByUserID",
                table: "FixedAssets",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_DeprExpenseAccountID",
                table: "FixedAssets",
                column: "DeprExpenseAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_SerialNumber",
                table: "FixedAssets",
                column: "SerialNumber",
                filter: "[SerialNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_UpdatedByUserID",
                table: "FixedAssets",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetTypes_TypeCode",
                table: "FixedAssetTypes",
                column: "TypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetTypes_TypeName",
                table: "FixedAssetTypes",
                column: "TypeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetBuildingDetails");

            migrationBuilder.DropTable(
                name: "AssetFurnitureDetails");

            migrationBuilder.DropTable(
                name: "AssetITDetails");

            migrationBuilder.DropTable(
                name: "AssetLandDetails");

            migrationBuilder.DropTable(
                name: "AssetVehicleDetails");

            migrationBuilder.DropTable(
                name: "DepreciationSchedules");

            migrationBuilder.DropTable(
                name: "FixedAssets");

            migrationBuilder.DropTable(
                name: "FixedAssetCategories");

            migrationBuilder.DropTable(
                name: "FixedAssetTypes");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1233));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1242));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1244));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1245));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 372, DateTimeKind.Utc).AddTicks(1246));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2741));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2748));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2751));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 21, 35, 20, 403, DateTimeKind.Utc).AddTicks(2752));
        }
    }
}
