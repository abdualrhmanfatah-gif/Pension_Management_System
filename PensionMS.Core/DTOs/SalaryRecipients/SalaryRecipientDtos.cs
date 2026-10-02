using PensionMS.Core.Interfaces;

namespace PensionMS.Core.DTOs.SalaryRecipients;

public class SalaryRecipientDto
{
    public int RecipientID { get; set; }
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;

    // Polymorphic recipient — exactly one of these will be set
    public int? RetireeID { get; set; }
    public int? AgentID { get; set; }
    public int? EntitledID { get; set; }
    public int? GuardianID { get; set; }

    // Resolved recipient name (for display)
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientType { get; set; } = string.Empty; // "متقاعد" | "وكيل" | "مستحق" | "وصي"
    public string? RecipientPhotoPath { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int PendingReviewStatus { get; set; }
    public decimal? SharePercent { get; set; }
}

public class CreateSalaryRecipientDto
{
    public int FileID { get; set; }

    // Exactly ONE of these must be set (enforced by CHECK constraint)
    public int? RetireeID { get; set; }
    public int? AgentID { get; set; }
    public int? EntitledID { get; set; }
    public int? GuardianID { get; set; }


    public DateTime StartDate { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// صورة شخصية 4×6 — مطلوبة عند إضافة مستحق ليس لديه صورة شخصية
    /// </summary>
    public IRemoteFile? PersonalPhoto { get; set; }

    /// <summary>
    /// صورة الهوية الوطنية — مطلوبة عند إضافة مستحق ليس لديه هوية مرفوعة
    /// </summary>
    public IRemoteFile? NationalIdImage { get; set; }

    // ─── حقول تكميلية للمستحقين (في حال وجود نواقص) ───
    public string? NationalID { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int? SocialStatusID { get; set; }
    public int? HealthStatusID { get; set; }

    /// <summary>
    /// المبلغ المطلوب للمستحق (يتم حساب النسبة منه تلقائياً)
    /// النسبة = (المبلغ ÷ إجمالي المعاش) × 100
    /// </summary>
    public decimal? EntitledAmount { get; set; }

    /// <summary>
    /// النسبة المئوية المحسوبة من المبلغ أو يدوياً
    /// </summary>
    public decimal? SharePercent { get; set; }
}

public class EndSalaryRecipientDto
{
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
}
