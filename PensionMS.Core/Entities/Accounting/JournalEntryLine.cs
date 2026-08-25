using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// بند القيد اليومي — يمثل طرفاً واحداً في القيد المزدوج (مدين أو دائن)
/// </summary>
public class JournalEntryLine : ISoftDeletable
{
    [Key]
    public int LineID { get; set; }

    /// <summary>معرف القيد اليومي</summary>
    public int JournalEntryID { get; set; }

    /// <summary>رقم السطر داخل القيد</summary>
    public int LineNumber { get; set; }

    /// <summary>معرف الحساب من الدليل المحاسبي</summary>
    public int AccountID { get; set; }

    /// <summary>معرف مركز التكلفة (اختياري)</summary>
    public int? CostCenterID { get; set; }

    /// <summary>المبلغ المدين</summary>
    public decimal Debit { get; set; }

    /// <summary>المبلغ الدائن</summary>
    public decimal Credit { get; set; }

    /// <summary>وصف البند (اختياري)</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>رقم الشيك (اختياري)</summary>
    [MaxLength(100)]
    public string? CheckNo { get; set; }

    /// <summary>تاريخ استحقاق الشيك (اختياري)</summary>
    public DateTime? CheckDueDate { get; set; }

    // ── المطابقة البنكية ──

    /// <summary>هل تمت المطابقة مع كشف البنك؟</summary>
    public bool IsReconciled { get; set; }

    /// <summary>تاريخ المطابقة</summary>
    public DateTime? ReconciledAt { get; set; }

    /// <summary>معرف المستخدم الذي قام بالمطابقة</summary>
    public int? ReconciledByUserID { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(JournalEntryID))]
    public virtual JournalEntry? JournalEntry { get; set; }

    [ForeignKey(nameof(AccountID))]
    public virtual ChartOfAccount? Account { get; set; }

    [ForeignKey(nameof(CostCenterID))]
    public virtual CostCenter? CostCenter { get; set; }

    [ForeignKey(nameof(ReconciledByUserID))]
    public virtual User? ReconciledByUser { get; set; }
}
