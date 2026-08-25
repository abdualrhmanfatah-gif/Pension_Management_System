using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Financial;

/// <summary>
/// الفصول المالية (الأرباع السنوية)
/// </summary>
public class Quarter : ISoftDeletable
{
    [Key]
    public int QuarterID { get; set; }

    [Required]
    public int FiscalYearID { get; set; }

    [ForeignKey(nameof(FiscalYearID))]
    public virtual FiscalYear FiscalYear { get; set; } = null!;

    [Range(1, 4)]
    public int QuarterNumber { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsClosed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation
    public virtual ICollection<FiscalPeriod> FiscalPeriods { get; set; } = new List<FiscalPeriod>();
}
