using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// مشروع استثماري
/// </summary>
public class InvestmentProject : ISoftDeletable
{
    [Key]
    public int ProjectID { get; set; }

    public int PortfolioID { get; set; }
    [ForeignKey(nameof(PortfolioID))]
    public virtual InvestmentPortfolio Portfolio { get; set; } = null!;

    public int PensionBranchID { get; set; }
    [ForeignKey(nameof(PensionBranchID))]
    public virtual PensionFundBranch PensionBranch { get; set; } = null!;

    public int TypeID { get; set; }
    [ForeignKey(nameof(TypeID))]
    public virtual InvestmentType InvestmentType { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? FeasibilityStudyPath { get; set; }
    public string? BoardApprovalDocPath { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Planned";

    public decimal PlannedBudget { get; set; }
    public decimal ExpectedROI { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<InvestmentPlan> Plans { get; set; } = new List<InvestmentPlan>();
    public virtual ICollection<InvestmentProperty> Properties { get; set; } = new List<InvestmentProperty>();
    public virtual ICollection<InvestmentRiskAssessment> RiskAssessments { get; set; } = new List<InvestmentRiskAssessment>();

    [InverseProperty(nameof(Document.InvestmentProject))]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
