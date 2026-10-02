namespace PensionMS.Core.Entities;

/// <summary>
/// حالات المعاملات
/// </summary>
public class TransactionStatus
{
    public int StatusID { get; set; }

    public string StatusName { get; set; } = string.Empty;
    public string? StatusColor { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<SalaryPaymentItem> SalaryPaymentItems { get; set; } = new List<SalaryPaymentItem>();
}
