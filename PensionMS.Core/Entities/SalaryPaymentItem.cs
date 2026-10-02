using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// تفاصيل صرف المعاشات
/// </summary>
public class SalaryPaymentItem : ISoftDeletable
{
    public int PaymentItemID { get; set; }

    public int BatchID { get; set; }
    public virtual SalaryPaymentBatch Batch { get; set; } = null!;

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int RetireeID { get; set; }
    public virtual Retiree Retiree { get; set; } = null!;

    public int? StatusID { get; set; }
    public virtual TransactionStatus? Status { get; set; }

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    /// <summary>
    /// دفتر الصرف النشط أثناء إنشاء عنصر الصرف (Snapshot)
    /// </summary>
    public int? PaymentBookID { get; set; }
    public virtual PaymentBook? PaymentBook { get; set; }

    /// <summary>
    /// مستلم المعاش النشط أثناء إنشاء عنصر الصرف (Snapshot)
    /// </summary>
    public int? RecipientID { get; set; }
    public virtual SalaryRecipient? Recipient { get; set; }

    public string? TransactionNumber { get; set; }
    public string? FinancialNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string? FailureReason { get; set; }

    public string? SenderBranch { get; set; }
    public string? ReceiverBranch { get; set; }
    public string? Currency { get; set; }
    public string? Sender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
