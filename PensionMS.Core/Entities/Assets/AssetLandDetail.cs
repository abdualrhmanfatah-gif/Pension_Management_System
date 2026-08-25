namespace PensionMS.Core.Entities.Assets;

public class AssetLandDetail
{
    public int AssetID { get; set; }
    public virtual FixedAsset Asset { get; set; } = null!;
    public string? PlotNumber { get; set; }
    public decimal? LandAreaSqm { get; set; }
    public string? OwnershipDocNumber { get; set; }
    public DateTime? OwnershipDocDate { get; set; }
    public string? Notes { get; set; }
}
