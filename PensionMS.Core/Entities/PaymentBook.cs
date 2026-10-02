using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// دفاتر صرف المعاش
/// </summary>
public class PaymentBook : ISoftDeletable
{
    public int PaymentBookID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public string BookNumber { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }

    /// <summary>
    /// فعال | منتهي | ملغي | مفقود | تالف
    /// </summary>
    public string Status { get; set; } = "فعال";

    /// <summary>
    /// إلكتروني (بطاقة) | يدوي (دفتر قديم)
    /// </summary>
    public string BookType { get; set; } = "إلكتروني";

    /// <summary>
    /// لون الدفتر اليدوي القديم (أبيض | أخضر | أزرق)
    /// </summary>
    public string? LegacyColor { get; set; }

    /// <summary>
    /// تجديد سنوي | تغيير وكيل | تعيين وكيل | تعديل وصي | تعديل مستحقين | بدل فاقد | بدل تالف | إصدار أول
    /// </summary>
    public string? ChangeReason { get; set; }

    public string? RecipientName { get; set; }

    /// <summary>
    /// متقاعد | وكيل | وصي | مستحق
    /// </summary>
    public string? RecipientType { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// الفرع الذي أصدر دفتر الصرف
    /// </summary>
    public int? IssuingBranchID { get; set; }
    public virtual PensionFundBranch? IssuingBranch { get; set; }

    public int PendingReviewStatus { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<SalaryPaymentItem> SalaryPaymentItems { get; set; } = new List<SalaryPaymentItem>();
}
