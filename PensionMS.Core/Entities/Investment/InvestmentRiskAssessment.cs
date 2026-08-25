using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// تقييم المخاطر الاستثمارية
/// </summary>
public class InvestmentRiskAssessment : ISoftDeletable
{
    [Key]
    public int RiskID { get; set; }

    public int ProjectID { get; set; }
    [ForeignKey(nameof(ProjectID))]
    public virtual InvestmentProject Project { get; set; } = null!;

    [MaxLength(100)]
    public string? RiskType { get; set; } // Market, Credit, Operational, Legal

    [MaxLength(500)]
    public string? RiskDescription { get; set; }

    public int RiskScore { get; set; }

    public string? MitigationPlan { get; set; }

    [MaxLength(200)]
    public string? ResponsiblePerson { get; set; }

    public DateTime AssessedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
