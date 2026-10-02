using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// المستأجرون
/// </summary>
public class Tenant : ISoftDeletable
{
    [Key]
    public int TenantID { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TenantType { get; set; } // Individual, Corporate, Merchant

    [Required]
    [MaxLength(50)]
    public string NationalIDOrCR { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>معرف الحساب في الدليل المحاسبي (حساب 411)</summary>
    public int? AccountID { get; set; }

    public decimal RatingScore { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AccountID))]
    public virtual ChartOfAccount? Account { get; set; }

    public virtual ICollection<LeaseContract> LeaseContracts { get; set; } = new List<LeaseContract>();
    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    [InverseProperty(nameof(Document.Tenant))]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
