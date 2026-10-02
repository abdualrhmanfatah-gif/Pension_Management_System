using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// حركات البنك — لتسجيل الإيداعات والسحوبات والمطابقة
/// </summary>
public class BankTransaction : ISoftDeletable
{
    [Key]
    public int TransactionID { get; set; }

    /// <summary>معرف البنك</summary>
    public int BankID { get; set; }

    /// <summary>معرف القيد اليومي المرتبط</summary>
    public int JournalEntryID { get; set; }

    /// <summary>معرف سطر القيد المرتبط</summary>
    public int LineID { get; set; }

    /// <summary>تاريخ العملية</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>نوع العملية: Deposit/Withdrawal/Transfer/Fee</summary>
    [Required, MaxLength(20)]
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>المبلغ</summary>
    public decimal Amount { get; set; }

    /// <summary>البيان / الوصف</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>رقم المرجع (رقم شيك، رقم تحويل...)</summary>
    [MaxLength(100)]
    public string? ReferenceNo { get; set; }

    /// <summary>هل تمت المطابقة مع كشف البنك؟</summary>
    public bool IsReconciled { get; set; }

    /// <summary>تاريخ المطابقة</summary>
    public DateTime? ReconciledAt { get; set; }

    /// <summary>رقم صف في كشف البنك المستورد</summary>
    [MaxLength(100)]
    public string? BankStatementRef { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BankID))]
    public virtual Bank? Bank { get; set; }

    [ForeignKey(nameof(JournalEntryID))]
    public virtual JournalEntry? JournalEntry { get; set; }

    [ForeignKey(nameof(LineID))]
    public virtual JournalEntryLine? Line { get; set; }
}
