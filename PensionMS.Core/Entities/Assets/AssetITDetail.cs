namespace PensionMS.Core.Entities.Assets;

public class AssetITDetail
{
    public int AssetID { get; set; }
    public virtual FixedAsset Asset { get; set; } = null!;
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
