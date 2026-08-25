using PensionMS.Core.Entities.Assets;

namespace PensionMS.Core.DTOs.Assets;

// ═══════════════════════════════════════
// FixedAssetType DTOs
// ═══════════════════════════════════════

public class AssetTypeDto
{
    public int AssetTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int AssetCount { get; set; }
}

// ═══════════════════════════════════════
// FixedAssetCategory DTOs
// ═══════════════════════════════════════

public class AssetCategoryDto
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public string? Description { get; set; }
    public int AssetTypeID { get; set; }
    public string? AssetTypeName { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal SalvagePercent { get; set; }
    public int? AssetAccountID { get; set; }
    public string? AssetAccountName { get; set; }
    public string? AssetAccountCode { get; set; }
    public int? AccumDeprAccountID { get; set; }
    public string? AccumDeprAccountName { get; set; }
    public int? DeprExpenseAccountID { get; set; }
    public string? DeprExpenseAccountName { get; set; }
    public bool IsActive { get; set; }
    public int AssetCount { get; set; }
}

public class CreateAssetCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public string? Description { get; set; }
    public int AssetTypeID { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal SalvagePercent { get; set; } = 5m;
    public int? AssetAccountID { get; set; }
    public int? AccumDeprAccountID { get; set; }
    public int? DeprExpenseAccountID { get; set; }
}

public class UpdateAssetCategoryDto
{
    public string? CategoryName { get; set; }
    public string? CategoryCode { get; set; }
    public string? Description { get; set; }
    public int? AssetTypeID { get; set; }
    public decimal? DepreciationRate { get; set; }
    public decimal? SalvagePercent { get; set; }
    public int? AssetAccountID { get; set; }
    public int? AccumDeprAccountID { get; set; }
    public int? DeprExpenseAccountID { get; set; }
    public bool? IsActive { get; set; }
}

// ═══════════════════════════════════════
// FixedAsset DTOs
// ═══════════════════════════════════════

public class FixedAssetDto
{
    public int AssetID { get; set; }
    public string AssetNumber { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public int AssetTypeID { get; set; }
    public string? AssetTypeName { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime InServiceDate { get; set; }
    public decimal AcquiredCost { get; set; }
    public decimal SalvageValue { get; set; }
    public string CurrencyCode { get; set; } = "YER";
    public int Quantity { get; set; } = 1;
    public decimal DepreciationRate { get; set; }
    public decimal UnitPrice { get; set; }
    public AssetValuationMethod ValuationMethod { get; set; }
    public string? ValuationNotes { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    public DateTime? LastDepreciationDate { get; set; }
    public string? Location { get; set; }
    public int? CustodianEmployeeID { get; set; }
    public string? CustodianEmployeeName { get; set; }
    public int AssetAccountID { get; set; }
    public string? AssetAccountName { get; set; }
    public string? AssetAccountCode { get; set; }
    public int AccumDeprAccountID { get; set; }
    public int DeprExpenseAccountID { get; set; }
    public AssetStatus Status { get; set; }
    public DateTime? DisposalDate { get; set; }
    public string? DisposalReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public AssetITDetailDto? ITDetail { get; set; }
    public AssetVehicleDetailDto? VehicleDetail { get; set; }
    public AssetBuildingDetailDto? BuildingDetail { get; set; }
    public AssetLandDetailDto? LandDetail { get; set; }
    public AssetFurnitureDetailDto? FurnitureDetail { get; set; }
}

public class CreateFixedAssetDto
{
    public string AssetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryID { get; set; }
    public int AssetTypeID { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime InServiceDate { get; set; }
    public decimal AcquiredCost { get; set; }
    public decimal SalvageValue { get; set; }
    public string CurrencyCode { get; set; } = "YER";
    public int Quantity { get; set; } = 1;
    public decimal DepreciationRate { get; set; }
    public decimal UnitPrice { get; set; }
    public AssetValuationMethod ValuationMethod { get; set; } = AssetValuationMethod.PurchasePrice;
    public string? ValuationNotes { get; set; }
    public string? Location { get; set; }
    public int? CustodianEmployeeID { get; set; }
    public int AssetAccountID { get; set; }
    public int AccumDeprAccountID { get; set; }
    public int DeprExpenseAccountID { get; set; }
    /// <summary>تخطي القيد المحاسبي — يُستخدم عند إضافة الأصل من صفحة الأرصدة الافتتاحية</summary>
    public bool SkipJournalEntry { get; set; }
    /// <summary>الإهلاك المتراكم الأولي — يُستخدم عند إضافة الأصل من صفحة الأرصدة الافتتاحية</summary>
    public decimal InitialAccumulatedDepreciation { get; set; } = 0;
    public AssetITDetailDto? ITDetail { get; set; }
    public AssetVehicleDetailDto? VehicleDetail { get; set; }
    public AssetBuildingDetailDto? BuildingDetail { get; set; }
    public AssetLandDetailDto? LandDetail { get; set; }
    public AssetFurnitureDetailDto? FurnitureDetail { get; set; }
}

public class UpdateFixedAssetDto
{
    public string? AssetName { get; set; }
    public string? Description { get; set; }
    public int? CategoryID { get; set; }
    public int? AssetTypeID { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? InServiceDate { get; set; }
    public decimal? AcquiredCost { get; set; }
    public decimal? SalvageValue { get; set; }
    public int? Quantity { get; set; }
    public decimal? DepreciationRate { get; set; }
    public decimal? UnitPrice { get; set; }
    public AssetValuationMethod? ValuationMethod { get; set; }
    public string? ValuationNotes { get; set; }
    public string? Location { get; set; }
    public int? CustodianEmployeeID { get; set; }
    public int? AssetAccountID { get; set; }
    public int? AccumDeprAccountID { get; set; }
    public int? DeprExpenseAccountID { get; set; }
    public AssetStatus? Status { get; set; }
    public string? DisposalReason { get; set; }
    public AssetITDetailDto? ITDetail { get; set; }
    public AssetVehicleDetailDto? VehicleDetail { get; set; }
    public AssetBuildingDetailDto? BuildingDetail { get; set; }
    public AssetLandDetailDto? LandDetail { get; set; }
    public AssetFurnitureDetailDto? FurnitureDetail { get; set; }
}

// ═══════════════════════════════════════
// Detail DTOs
// ═══════════════════════════════════════

public class AssetITDetailDto
{
    public string? Model { get; set; }
    public string? Manufacturer { get; set; }
    public string? ProcessorType { get; set; }
    public string? ProcessorSpeed { get; set; }
    public string? RamSize { get; set; }
    public string? HardDiskType { get; set; }
    public string? HardDiskSize { get; set; }
    public string? GpuModel { get; set; }
    public string? ScreenSize { get; set; }
    public string? ScreenResolution { get; set; }
    public string? OperatingSystem { get; set; }
    public bool HasWebcam { get; set; }
    public bool HasWifi { get; set; }
    public bool HasBluetooth { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string? Notes { get; set; }
}

public class AssetVehicleDetailDto
{
    public string? PlateNumber { get; set; }
    public string? PlateLetter1 { get; set; }
    public string? PlateLetter2 { get; set; }
    public string? PlateLetter3 { get; set; }
    public string? EngineNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public string? TransmissionType { get; set; }
    public string? FuelType { get; set; }
    public int? CylinderCount { get; set; }
    public string? EngineCapacity { get; set; }
    public int? HorsePower { get; set; }
    public int WheelCount { get; set; } = 4;
    public string? TireSize { get; set; }
    public string? Color { get; set; }
    public int? ModelYear { get; set; }
    public DateTime? RegistrationExpiry { get; set; }
    public DateTime? InsuranceExpiry { get; set; }
    public int Mileage { get; set; }
    public bool HasGps { get; set; }
    public string? Notes { get; set; }
}

public class AssetBuildingDetailDto
{
    public decimal? AreaSqm { get; set; }
    public decimal? BuiltAreaSqm { get; set; }
    public int FloorCount { get; set; } = 1;
    public string? OwnershipDocNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Notes { get; set; }
}

public class AssetLandDetailDto
{
    public string? PlotNumber { get; set; }
    public decimal? LandAreaSqm { get; set; }
    public string? OwnershipDocNumber { get; set; }
    public DateTime? OwnershipDocDate { get; set; }
    public string? Notes { get; set; }
}

public class AssetFurnitureDetailDto
{
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? Notes { get; set; }
}

// ═══════════════════════════════════════
// DepreciationSchedule DTOs
// ═══════════════════════════════════════

public class DepreciationScheduleDto
{
    public int ScheduleID { get; set; }
    public int AssetID { get; set; }
    public string? AssetNumber { get; set; }
    public string? AssetName { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
