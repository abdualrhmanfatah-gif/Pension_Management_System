using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// طلبات الصيانة
/// </summary>
public class MaintenanceRequest : ISoftDeletable
{
    [Key]
    public int RequestID { get; set; }

    public int PropertyID { get; set; }
    [ForeignKey(nameof(PropertyID))]
    public virtual InvestmentProperty Property { get; set; } = null!;

    public int? UnitID { get; set; }
    [ForeignKey(nameof(UnitID))]
    public virtual PropertyUnit? Unit { get; set; }

    public int? TenantID { get; set; }
    [ForeignKey(nameof(TenantID))]
    public virtual Tenant? Tenant { get; set; }

    [MaxLength(50)]
    public string? Priority { get; set; } // Low, Medium, High, Urgent

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? DamagePhotosJson { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "New"; // New, InProgress, Completed, Cancelled

    public DateTime ReportedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
