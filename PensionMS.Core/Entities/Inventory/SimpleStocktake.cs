using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Inventory;

public class SimpleStocktake : ISoftDeletable
{
    public int SimpleStocktakeID { get; set; }
    public int FiscalYearID { get; set; }
    public DateTime StocktakeDate { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress, Completed, Adjusted
    public int CreatedByUserID { get; set; }
    public int? CompletedByUserID { get; set; }
    public DateTime? CompletedAt { get; set; }

    // ملخص الفروق
    public int TotalItems { get; set; }
    public int ItemsWithVariance { get; set; }
    public decimal TotalVarianceValue { get; set; }

    // القيد المحاسبي
    public int? JournalEntryID { get; set; }

    // Navigation
    public virtual FiscalYear FiscalYear { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? CompletedByUser { get; set; }
    public virtual JournalEntry? JournalEntry { get; set; }
    public virtual ICollection<SimpleStocktakeLine> Lines { get; set; } = new List<SimpleStocktakeLine>();

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
}
