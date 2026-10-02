using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// العقارات الاستثمارية
/// </summary>
public class InvestmentProperty : ISoftDeletable
{
    [Key]
    public int PropertyID { get; set; }

    public int? ProjectID { get; set; }
    [ForeignKey(nameof(ProjectID))]
    public virtual InvestmentProject? Project { get; set; }

    public int PensionBranchID { get; set; }
    [ForeignKey(nameof(PensionBranchID))]
    public virtual PensionFundBranch PensionBranch { get; set; } = null!;

    public int PropertyTypeID { get; set; }
    [ForeignKey(nameof(PropertyTypeID))]
    public virtual PropertyType PropertyType { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string PropertyName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? LocationCity { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [MaxLength(500)]
    public string? GoogleMapsUrl { get; set; }

    public string? MainPhotoPath { get; set; }
    public string? PhotosJson { get; set; }
    public string? OwnershipDeedPath { get; set; }

    public decimal TotalArea { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public decimal InitialValue { get; set; }

    [MaxLength(100)]
    public string? TitleDeedNumber { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Ready";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<PropertyUnit> Units { get; set; } = new List<PropertyUnit>();
    public virtual ICollection<AssetValuation> Valuations { get; set; } = new List<AssetValuation>();
    public virtual ICollection<DepreciationRecord> DepreciationRecords { get; set; } = new List<DepreciationRecord>();
    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    [InverseProperty(nameof(Document.InvestmentProperty))]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
