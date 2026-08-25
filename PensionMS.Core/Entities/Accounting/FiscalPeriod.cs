using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities.Financial;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// الفترة المحاسبية — تقسيم السنة المالية إلى فترات (  ربع سنوية)
/// </summary>
public class FiscalPeriod : ISoftDeletable
{
    [Key]
    public int PeriodID { get; set; }


    public int FiscalYearID { get; set; }

    /// <summary>معرف الفصل المالي (FK → Quarter)</summary>
    public int? QuarterID { get; set; }

    [Required, MaxLength(50)]
    public string PeriodName { get; set; } = string.Empty;

    public int PeriodNumber { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsClosed { get; set; }

    public int? ClosedByUserID { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FiscalYearID))]
    public virtual FiscalYear? FiscalYear { get; set; }

    [ForeignKey(nameof(QuarterID))]
    public virtual Quarter? Quarter { get; set; }

    [ForeignKey(nameof(ClosedByUserID))]
    public virtual User? ClosedByUser { get; set; }
}
