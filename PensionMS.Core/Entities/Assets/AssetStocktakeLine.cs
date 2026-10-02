using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.Entities.Assets;

public class AssetStocktakeLine
{
    [Key]
    public int StocktakeLineID { get; set; }
    public int AssetStocktakeID { get; set; }
    public int FixedAssetID { get; set; }

    // البيانات من السجلات (تُملأ تلقائياً)
    public string AssetNumber { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal NetBookValue { get; set; }
    public int SystemQuantity { get; set; } = 1;

    // البيانات الفعلية (تُسجل يدوياً)
    public string? ActualLocation { get; set; }
    public string ActualCondition { get; set; } = ""; // Good, Fair, Poor, Missing
    public bool IsPresent { get; set; } = false;
    public int RecordedQuantity { get; set; } = 0;
    public string? Notes { get; set; }

    // Navigation
    public virtual AssetStocktake Stocktake { get; set; } = null!;
    public virtual FixedAsset FixedAsset { get; set; } = null!;
}
