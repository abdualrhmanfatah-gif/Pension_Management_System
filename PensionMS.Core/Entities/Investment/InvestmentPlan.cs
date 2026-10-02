using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities.Financial;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// خطة الاستثمار السنوية
/// </summary>
public class InvestmentPlan : ISoftDeletable
{
    [Key]
    public int PlanID { get; set; }

    public int ProjectID { get; set; }
    [ForeignKey(nameof(ProjectID))]
    public virtual InvestmentProject Project { get; set; } = null!;

    public int FiscalYearID { get; set; }
    [ForeignKey(nameof(FiscalYearID))]
    public virtual FiscalYear FiscalYear { get; set; } = null!;

    public decimal PlannedAmount { get; set; }
    public decimal ActualSpent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
