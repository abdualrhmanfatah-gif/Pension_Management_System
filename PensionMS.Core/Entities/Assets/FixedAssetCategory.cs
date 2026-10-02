using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Core.Entities.Assets;

public class FixedAssetCategory
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public string? Description { get; set; }
    public int AssetTypeID { get; set; }
    public virtual FixedAssetType AssetType { get; set; } = null!;
    public decimal DepreciationRate { get; set; }
    public decimal SalvagePercent { get; set; } = 5m;
    public int? AssetAccountID { get; set; }
    public virtual ChartOfAccount? AssetAccount { get; set; }
    public int? AccumDeprAccountID { get; set; }
    public virtual ChartOfAccount? AccumDeprAccount { get; set; }
    public int? DeprExpenseAccountID { get; set; }
    public virtual ChartOfAccount? DeprExpenseAccount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public virtual ICollection<FixedAsset> Assets { get; set; } = new List<FixedAsset>();
}
