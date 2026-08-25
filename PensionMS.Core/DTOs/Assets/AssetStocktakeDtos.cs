namespace PensionMS.Core.DTOs.Assets;

public class AssetStocktakeDto
{
    public int AssetStocktakeID { get; set; }
    public int FiscalYearID { get; set; }
    public string? YearName { get; set; }
    public DateTime StocktakeDate { get; set; }
    public string Status { get; set; }
    public string? CreatedByName { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Notes { get; set; }
    public AssetStocktakeSummaryDto Summary { get; set; } = new();
    public List<AssetStocktakeLineDto> Lines { get; set; } = new();
}

public class AssetStocktakeLineDto
{
    public int StocktakeLineID { get; set; }
    public int FixedAssetID { get; set; }
    public string AssetNumber { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? Location { get; set; }
    public decimal NetBookValue { get; set; }
    public int SystemQuantity { get; set; } = 1;
    public string? ActualLocation { get; set; }
    public string ActualCondition { get; set; } = "Good";
    public bool IsPresent { get; set; } = true;
    public int RecordedQuantity { get; set; } = 0;
    public string? Notes { get; set; }
}

public class AssetStocktakeSummaryDto
{
    public int TotalAssets { get; set; }
    public int PresentAssets { get; set; }
    public int MissingAssets { get; set; }
    public int AssetsInGoodCondition { get; set; }
    public int AssetsInFairCondition { get; set; }
    public int AssetsInPoorCondition { get; set; }
    public decimal TotalBookValue { get; set; }
    public decimal MissingBookValue { get; set; }
}

public class CreateAssetStocktakeDto
{
    public int FiscalYearID { get; set; }
    public string? Notes { get; set; }
}

public class RecordAssetStocktakeDto
{
    public List<AssetStocktakeLineDto> Lines { get; set; } = new();
}
