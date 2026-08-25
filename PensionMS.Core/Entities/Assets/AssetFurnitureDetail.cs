namespace PensionMS.Core.Entities.Assets;

public class AssetFurnitureDetail
{
    public int AssetID { get; set; }
    public virtual FixedAsset Asset { get; set; } = null!;
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? Notes { get; set; }
}
