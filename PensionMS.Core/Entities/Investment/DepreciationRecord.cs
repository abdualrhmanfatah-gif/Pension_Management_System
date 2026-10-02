using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities.Financial;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// سجل الإهلاك
/// </summary>
public class DepreciationRecord : ISoftDeletable
{
    [Key]
    public int DeprecID { get; set; }

    public int PropertyID { get; set; }
    [ForeignKey(nameof(PropertyID))]
    public virtual InvestmentProperty Property { get; set; } = null!;

    public int FiscalYearID { get; set; }
    [ForeignKey(nameof(FiscalYearID))]
    public virtual FiscalYear FiscalYear { get; set; } = null!;

    public decimal DepreciationRate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal NetBookValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
