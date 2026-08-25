using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// كشوفات اعتماد القوة
/// </summary>
public class MinistryApprovalBatch : ISoftDeletable
{
    public int BatchID { get; set; }

    public int ImportedByUserID { get; set; }
    public virtual User ImportedByUser { get; set; } = null!;

    public string? BatchNumber { get; set; }
    public DateTime BatchDate { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int TotalCount { get; set; }
    public int ProcessedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<MinistryApprovalItem> MinistryApprovalItems { get; set; } = new List<MinistryApprovalItem>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
