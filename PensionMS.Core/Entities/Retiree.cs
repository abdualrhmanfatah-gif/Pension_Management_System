using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// المتقاعدون
/// </summary>
public class Retiree : ISoftDeletable
{
    public int RetireeID { get; set; }

    public int? SocialStatusID { get; set; }
    public virtual SocialStatus? SocialStatus { get; set; }

    public int? HealthStatusID { get; set; }
    public virtual HealthStatus? HealthStatus { get; set; }

    public int ApprovalItemID { get; set; }
    public virtual MinistryApprovalItem ApprovalItem { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? PersonalPhotoPath { get; set; }
    public string? MilitaryNumber { get; set; }
    public byte Gender { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public string? LinkNumber { get; set; }
    public bool IsAlive { get; set; } = true;
    public DateOnly? BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }

    public PendingReviewStatus PendingReviewStatus { get; set; } = PendingReviewStatus.None;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<RetirementFile> RetirementFiles { get; set; } = new List<RetirementFile>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<SalaryPaymentItem> SalaryPaymentItems { get; set; } = new List<SalaryPaymentItem>();
    public virtual ICollection<SalaryRecipient> SalaryRecipients { get; set; } = new List<SalaryRecipient>();
    public virtual ICollection<FinancialEntitlement> FinancialEntitlements { get; set; } = new List<FinancialEntitlement>();
    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();

    // ─── Item 15: Factory method ────────────────────────────────────────
    // Enforces the business invariant: a Retiree MUST originate from a MinistryApprovalItem.
    // Prevents direct `new Retiree()` usage that could bypass the approval constraint.
    public static Retiree CreateFromApprovalItem(MinistryApprovalItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new Retiree
        {
            ApprovalItemID = item.ApprovalItemID,
            FullName       = item.FullName,
            MilitaryNumber = item.MilitaryNumber,
            Gender         = item.Gender ?? 1, // Default to Male if null
            NationalID     = string.Empty, // Set separately by operator during file creation
            LinkNumber     = item.LinkNumber,
            IsAlive        = true,
            CreatedAt      = DateTime.UtcNow
        };
    }
}
