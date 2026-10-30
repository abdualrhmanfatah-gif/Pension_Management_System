using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// مُستلم المعاش
/// </summary>
public class SalaryRecipient : ISoftDeletable
{
    public int RecipientID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int? RetireeID { get; set; }
    public virtual Retiree? Retiree { get; set; }

    public int? AgentID { get; set; }
    public virtual Agent? Agent { get; set; }

    public int? EntitledID { get; set; }
    public virtual EntitledPerson? EntitledPerson { get; set; }

    public int? GuardianID { get; set; }
    public virtual Guardian? Guardian { get; set; }

    public string RecipientType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

<<<<<<< HEAD
    // ─── الحصص ─────────────────────────────────────────────
    /// <summary>
    /// النسبة المئوية المحسوبة (من المبلغ المُدخل أو يدوياً)
    /// تُحسب: (المبلغ المُدخل ÷ إجمالي المعاش) × 100
    /// </summary>
    public decimal? SharePercent { get; set; }

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public PendingReviewStatus PendingReviewStatus { get; set; } = PendingReviewStatus.None;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }

    public virtual ICollection<SalaryPaymentItem> SalaryPaymentItems { get; set; } = new List<SalaryPaymentItem>();
}
