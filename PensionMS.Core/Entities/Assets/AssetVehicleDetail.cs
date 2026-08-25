namespace PensionMS.Core.Entities.Assets;

public class AssetVehicleDetail
{
    public int AssetID { get; set; }
    public virtual FixedAsset Asset { get; set; } = null!;
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
