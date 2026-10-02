using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// قرارات التسوية
/// </summary>
public class SettlementDecision : ISoftDeletable
{
    public int DecisionID { get; set; }

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }
    public string? DecisionType { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
