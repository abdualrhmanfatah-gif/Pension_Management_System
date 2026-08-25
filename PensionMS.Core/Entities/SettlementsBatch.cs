using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// كشف التسويات
/// </summary>
public class SettlementsBatch : ISoftDeletable
{
    public int BatchID { get; set; }

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public string? BatchNumber { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int TotalCount { get; set; }
    public int SourceRowsCount { get; set; } // Number of rows in the source file
    public int SkippedCount { get; set; }    // Number of skipped/failed rows
    public string? FailureReportJson { get; set; } // JSON storage for failed rows details
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();
    public virtual ICollection<FinancialEntitlement> FinancialEntitlements { get; set; } = new List<FinancialEntitlement>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
