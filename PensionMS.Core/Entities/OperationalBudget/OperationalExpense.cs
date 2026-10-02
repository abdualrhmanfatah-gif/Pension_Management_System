using System.ComponentModel.DataAnnotations;
using PensionMS.Core.Interfaces;


namespace PensionMS.Core.Entities.OperationalBudget;

/// <summary>
/// النفقات التشغيلية الفعلية
/// </summary>
public class OperationalExpense : ISoftDeletable
{
    public int ExpenseID { get; set; }

    public int? QuarterNumber { get; set; } // for provisional expenses: which quarter they belong to
    public int? FiscalYearID { get; set; } // for provisional expenses: which fiscal year they belong to

    public DateTime ExpenseDate { get; set; }
    public decimal Amount { get; set; } // Total Amount before tax
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }

    [Required]
    [MaxLength(200)]
    public string ExpenseType { get; set; } = "ميزانية تشغيلية";

    [Required]
    [MaxLength(500)]
    public string RecipientName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RecipientWorkplace { get; set; }

    public int DocumentCount { get; set; }

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? InternalChequeNumber { get; set; }

    [MaxLength(500)]
    public string? DirectiveSource { get; set; } // مصدر التوجيه / جهة الأمر

    [MaxLength(1000)]
    public string? ExpenseStatement { get; set; } // بيان الصرف (مقابل...)

    [MaxLength(100)]
    public string Status { get; set; } = "جديد";

    public bool IsProvisional { get; set; } = false;
    public DateTime? ClassifiedAt { get; set; }
    public int? ClassifiedByUserID { get; set; }

    public string? SignedOrderPath { get; set; } // مسار أمر الصرف الموقع
    public string? SupportingDocsPath { get; set; } // مسار الأوليات المرفقة

    public string? AttachmentPath { get; set; } // القديم، سيظل للتوافقية أو يمكن استبداله

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
