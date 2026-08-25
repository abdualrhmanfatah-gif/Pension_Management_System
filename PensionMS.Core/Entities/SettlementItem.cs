using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// تفاصيل التسويات
/// </summary>
public class SettlementItem : ISoftDeletable
{
    public int SettlementItemID { get; set; }

    public int BatchID { get; set; }
    public virtual SettlementsBatch Batch { get; set; } = null!;

    public int? ApprovalItemID { get; set; }
    public virtual MinistryApprovalItem? ApprovalItem { get; set; }

    /// <summary>
    /// الرقم العسكري — يُخزَّن مباشرة للحالات التي لا يوجد فيها بند اعتماد
    /// </summary>
    public string? MilitaryNumber { get; set; }

    /// <summary>
    /// الاسم الكامل — يُخزَّن مباشرة للحالات التي لا يوجد فيها بند اعتماد
    /// </summary>
    public string? FullName { get; set; }

    public int? RetireeID { get; set; }
    public virtual Retiree? Retiree { get; set; }

    public int? DecisionID { get; set; }
    public virtual SettlementDecision? Decision { get; set; }

    public int CurrentRankID { get; set; }
    public virtual Rank CurrentRank { get; set; } = null!;

    public int ProcessedRankID { get; set; }
    public virtual Rank ProcessedRank { get; set; } = null!;

    public decimal Amount { get; set; }
    public decimal? Deductions { get; set; }
    public decimal NetAmount => Amount - (Deductions ?? 0);

    public int SerialNumber { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
