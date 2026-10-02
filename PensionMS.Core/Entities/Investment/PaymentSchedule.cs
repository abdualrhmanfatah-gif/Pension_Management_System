using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// جدول الدفعات
/// </summary>
public class PaymentSchedule : ISoftDeletable
{
    [Key]
    public int ScheduleID { get; set; }

    public int ContractID { get; set; }
    [ForeignKey(nameof(ContractID))]
    public virtual LeaseContract LeaseContract { get; set; } = null!;

    public DateTime DueDate { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal PaidAmount { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
