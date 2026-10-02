using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Core.Entities.Assets;

public class DepreciationSchedule
{
    public int ScheduleID { get; set; }
    public int AssetID { get; set; }
    public virtual FixedAsset Asset { get; set; } = null!;
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
    public int? JournalEntryID { get; set; }
    public virtual JournalEntry? JournalEntry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
