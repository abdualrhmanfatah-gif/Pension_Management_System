using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// القيد اليومي — السجل الرئيسي للحركات المحاسبية (مزدوجة القيد)
/// </summary>
public class JournalEntry : ISoftDeletable
{
    [Key]
    public int JournalEntryID { get; set; }

    /// <summary>رقم القيد (مثال: JE-2026-000001)</summary>
    [Required, MaxLength(50)]
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>تاريخ القيد</summary>
    public DateTime EntryDate { get; set; }

    /// <summary>معرف الفترة المحاسبية</summary>
    public int PeriodID { get; set; }

    /// <summary>معرف نوع القيد</summary>
    [Required]
    public int JournalEntryTypeID { get; set; }

    /// <summary>معرف العملية المصدرة (مثال: TransactionID, BatchID)</summary>
    public int? SourceReferenceID { get; set; }

    /// <summary>البيان / الوصف</summary>
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>إجمالي الطرف المدين</summary>
    public decimal TotalDebit { get; set; }

    /// <summary>إجمالي الطرف الدائن</summary>
    public decimal TotalCredit { get; set; }

    /// <summary>حالة القيد: Draft / Posted / Reversed</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Draft";

    /// <summary>معرف المستخدم الذي قام بالنشر</summary>
    public int? PostedByUserID { get; set; }

    /// <summary>تاريخ النشر</summary>
    public DateTime? PostedAt { get; set; }

    /// <summary>معرف القيد المعكوس (إذا كان هذا القيد عكساً لقيد آخر)</summary>
    public int? ReversedEntryID { get; set; }

    /// <summary>معرف المستخدم الذي أنشأ القيد</summary>
    public int? CreatedByUserID { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(JournalEntryTypeID))]
    public virtual JournalEntryType JournalEntryType { get; set; } = null!;

    [ForeignKey(nameof(PeriodID))]
    public virtual FiscalPeriod? Period { get; set; }

    [ForeignKey(nameof(CreatedByUserID))]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey(nameof(PostedByUserID))]
    public virtual User? PostedByUser { get; set; }

    [ForeignKey(nameof(ReversedEntryID))]
    public virtual JournalEntry? ReversedEntry { get; set; }

    public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    public virtual ICollection<JournalEntry> ReverseEntries { get; set; } = new List<JournalEntry>();
}
