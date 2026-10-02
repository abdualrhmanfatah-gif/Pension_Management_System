using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.Entities.Inventory;

public class SimpleStocktakeLine
{
    [Key]
    public int LineID { get; set; }
    public int SimpleStocktakeID { get; set; }
    public int InventoryItemID { get; set; }

    // البيانات (تُملأ تلقائياً ثم تُعدّل يدوياً)
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int SystemQuantity { get; set; }
    public int PhysicalQuantity { get; set; }
    public decimal UnitCost { get; set; }

    // الحسابات
    public int Variance => PhysicalQuantity - SystemQuantity;
    public decimal VarianceValue => Math.Abs(Variance) * UnitCost;

    // Navigation
    public virtual SimpleStocktake Stocktake { get; set; } = null!;
    public virtual InventoryItem InventoryItem { get; set; } = null!;
}
