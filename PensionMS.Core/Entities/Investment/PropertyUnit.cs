using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// وحدات العقار (شقق، مكاتب، محلات)
/// </summary>
public class PropertyUnit : ISoftDeletable
{
    [Key]
    public int UnitID { get; set; }

    public int PropertyID { get; set; }
    [ForeignKey(nameof(PropertyID))]
    public virtual InvestmentProperty Property { get; set; } = null!;

    public int UnitTypeID { get; set; }
    [ForeignKey(nameof(UnitTypeID))]
    public virtual UnitType UnitType { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string UnitNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Floor { get; set; }

    public decimal Area { get; set; }

    public string? MainPhotoPath { get; set; }
    public string? PhotosJson { get; set; }
    public string? LayoutPlanPath { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Available";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<LeaseContract> LeaseContracts { get; set; } = new List<LeaseContract>();
    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    [InverseProperty(nameof(Document.PropertyUnit))]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
