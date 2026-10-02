using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// كشف صرف المعاشات
/// </summary>
public class SalaryPaymentBatch : ISoftDeletable
{
    public int BatchID { get; set; }

    public int BankID { get; set; }
    public virtual Bank Bank { get; set; } = null!;

    public int ImportedByUserID { get; set; }
    public virtual User ImportedByUser { get; set; } = null!;

    public int? ApprovedByUserID { get; set; }
    public virtual User? ApprovedByUser { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime BatchMonth { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<SalaryPaymentItem> SalaryPaymentItems { get; set; } = new List<SalaryPaymentItem>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
