namespace PensionMS.Core.DTOs.Inventory;

public class SimpleStocktakeDto
{
    public int SimpleStocktakeID { get; set; }
    public int FiscalYearID { get; set; }
    public string? YearName { get; set; }
    public DateTime StocktakeDate { get; set; }
    public string Status { get; set; }
    public string? CreatedByName { get; set; }
    public string? CompletedByName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalItems { get; set; }
    public int ItemsWithVariance { get; set; }
    public decimal TotalVarianceValue { get; set; }
    public int? JournalEntryID { get; set; }
    public string? JournalEntryNumber { get; set; }
    public List<SimpleStocktakeLineDto> Lines { get; set; } = new();
}

public class SimpleStocktakeLineDto
{
    public int LineID { get; set; }
    public int InventoryItemID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int SystemQuantity { get; set; }
    public int PhysicalQuantity { get; set; }
    public int Variance => PhysicalQuantity - SystemQuantity;
    public decimal UnitCost { get; set; }
    public decimal VarianceValue => Math.Abs(Variance) * UnitCost;
}

public class CreateSimpleStocktakeDto
{
    public int FiscalYearID { get; set; }
}

public class RecordSimpleStocktakeDto
{
    public Dictionary<int, int> ActualCounts { get; set; } = new(); // InventoryItemID -> PhysicalQuantity
}
