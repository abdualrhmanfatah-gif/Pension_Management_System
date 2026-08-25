using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// تفاصيل كشف الإحالة إلى التقاعد (القوة)
/// </summary>
public class MinistryApprovalItem : ISoftDeletable
{
    public int ApprovalItemID { get; set; }

    public int BatchID { get; set; }
    public virtual MinistryApprovalBatch Batch { get; set; } = null!;

    public int? RankID { get; set; }
    public virtual Rank? Rank { get; set; }

    public virtual Retiree? Retiree { get; set; }

    public int? ProcessedByUserID { get; set; }
    public virtual User? ProcessedByUser { get; set; }

    public int SerialNumber { get; set; }
    public string MilitaryNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public byte? Gender { get; set; }
    public string? UnitName { get; set; }
    public DateTime? EnrollmentDate { get; set; }
    public DateTime? RetirementDate { get; set; }
    public decimal? Salary { get; set; }
    public int? CategoryID { get; set; }
    public virtual RetireeCategory? Category { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Notes { get; set; }
    public string? LinkNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();
}
