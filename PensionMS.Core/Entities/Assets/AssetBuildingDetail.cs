namespace PensionMS.Core.Entities.Assets;

public class AssetBuildingDetail
{
    public int AssetID { get; set; }
    public virtual FixedAsset Asset { get; set; } = null!;
    public decimal? AreaSqm { get; set; }
    public decimal? BuiltAreaSqm { get; set; }
    public int FloorCount { get; set; } = 1;
    public string? OwnershipDocNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Notes { get; set; }
}
