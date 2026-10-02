using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.OperationalBudget;

namespace PensionMS.Core.Entities.Financial;

/// <summary>
/// السنة المالية
/// </summary>
public class FiscalYear : ISoftDeletable
{
    public int FiscalYearID { get; set; }

    [Required]
    [MaxLength(50)]
    public string YearName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalBudget { get; set; }

    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ClosedByUserID { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<Quarter> Quarters { get; set; } = new List<Quarter>();
    public virtual ICollection<FiscalPeriod> FiscalPeriods { get; set; } = new List<FiscalPeriod>();

    [ForeignKey(nameof(ClosedByUserID))]
    public virtual User? ClosedByUser { get; set; }
}
