namespace PensionMS.Core.Entities.Assets;

public class FixedAssetType
{
    public int AssetTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<FixedAssetCategory> Categories { get; set; } = new List<FixedAssetCategory>();
    public virtual ICollection<FixedAsset> Assets { get; set; } = new List<FixedAsset>();
}
