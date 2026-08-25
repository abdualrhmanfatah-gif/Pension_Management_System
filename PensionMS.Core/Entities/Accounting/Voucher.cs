using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

public enum VoucherType
{
    Payment = 1,  // سند صرف
    Receipt = 2   // سند قبض
}

public enum VoucherStatus
{
    Draft = 1,      // مسودة
    Posted = 2,     // مرحّل
    Cancelled = 3   // ملغي
}

/// <summary>
/// سندات الصرف والقبض
/// </summary>
public class Voucher : ISoftDeletable
{
    [Key]
    public int VoucherID { get; set; }

    [Required]
    [MaxLength(30)]
    public string VoucherNumber { get; set; } = string.Empty;

    public VoucherType VoucherType { get; set; }

    public VoucherStatus Status { get; set; } = VoucherStatus.Draft;

    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

    // ── الطرف المستفيد / الدافع ──
    [MaxLength(200)]
    public string? PayeeName { get; set; } // المستفيد (صرف)

    [MaxLength(200)]
    public string? PayerName { get; set; } // الدافع (قبض)

    // ── المبلغ والبيان ──
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty; // البيان

    // ── الحسابات المحاسبية ──
    public int? DebitAccountID { get; set; }

    [ForeignKey(nameof(DebitAccountID))]
    public virtual ChartOfAccount? DebitAccount { get; set; }

    public int CreditAccountID { get; set; }

    [ForeignKey(nameof(CreditAccountID))]
    public virtual ChartOfAccount CreditAccount { get; set; } = null!;

    // ── مراجع إضافية ──
    [MaxLength(100)]
    public string? ChequeNumber { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(200)]
    public string? ReferralSource { get; set; } // مصدر التوجيه

    // ── البنك المرتبط ──
    public int? BankID { get; set; }

    [ForeignKey(nameof(BankID))]
    public virtual Bank? Bank { get; set; }

    // ── القيود المرتبط ──
    public int? JournalEntryID { get; set; }

    [MaxLength(30)]
    public string? JournalEntryNumber { get; set; }

    // ── التدقيق ──
    public int CreatedByUserID { get; set; }

    [ForeignKey(nameof(CreatedByUserID))]
    public virtual User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── ISoftDeletable ──
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
}
