using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// عقود الإيجار
/// </summary>
public class LeaseContract : ISoftDeletable
{
    [Key]
    public int ContractID { get; set; }

    public int UnitID { get; set; }
    [ForeignKey(nameof(UnitID))]
    public virtual PropertyUnit Unit { get; set; } = null!;

    public int TenantID { get; set; }
    [ForeignKey(nameof(TenantID))]
    public virtual Tenant Tenant { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalContractValue { get; set; }

    [MaxLength(50)]
    public string? PaymentFrequency { get; set; } // Monthly, Quarterly, Annual

    public string? RenewalTerms { get; set; }
    public string? SignedContractPath { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();

    [InverseProperty(nameof(Document.LeaseContract))]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
