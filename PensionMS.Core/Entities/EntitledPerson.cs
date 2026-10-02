using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// المستحقون (الورثة)
/// </summary>
public class EntitledPerson : ISoftDeletable
{
    public int EntitledID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int RelationshipID { get; set; }
    public virtual Relationship Relationship { get; set; } = null!;

    public int? SocialStatusID { get; set; }
    public virtual SocialStatus? SocialStatus { get; set; }

    public int? HealthStatusID { get; set; }
    public virtual HealthStatus? HealthStatus { get; set; }



    // ─── البيانات الشخصية ────────────────────────────────────────
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? PersonalPhotoPath { get; set; }

    // ─── حالة الاستحقاق ──────────────────────────────────────────
    public bool IsActive { get; set; } = true;
    public DateTime? StopDate { get; set; }
    public string? StopReason { get; set; }

    /// <summary>
    /// كود سبب الإيقاف المُعرَّف — يُستخدم في آلة الحالة:
    /// AGE_LIMIT | DAUGHTER_MARRIED | WIDOW_REMARRIED | DECEASED | ADMIN_DECISION
    /// </summary>
    public string? StopCode { get; set; }

    // ─── الحصص ─────────────────────────────────────────────
    /// <summary>
    /// النسبة المئوية الافتراضية لهذا المستحق (0-100)
    /// تُستخدم كقيمة افتراضية عند تعيينه كمستلم معاش
    /// </summary>
    public decimal? SharePercent { get; set; }

    // ─── تسجيل الوفاة ────────────────────────────────────────────
    public bool IsDeceased { get; set; }
    public DateOnly? DeceasedDate { get; set; }

    // ─── استثناء الإعاقة/المرض النفسي (للابن فقط) ───────────────
    /// <summary>
    /// true = معاق أو مريض نفسي → يُعفى من شرط عمر 24 سنة
    /// </summary>
    public bool HasDisabilityException { get; set; }

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

    public virtual ICollection<Agent> Agents { get; set; } = new List<Agent>();
    public virtual ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();
    public virtual ICollection<GuardianEntitledPerson> GuardianEntitledPersons { get; set; } = new List<GuardianEntitledPerson>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<SalaryRecipient> SalaryRecipients { get; set; } = new List<SalaryRecipient>();
}
