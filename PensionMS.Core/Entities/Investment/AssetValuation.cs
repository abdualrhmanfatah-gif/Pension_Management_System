using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// تقييم الأصول
/// </summary>
public class AssetValuation : ISoftDeletable
{
    [Key]
    public int ValuationID { get; set; }

    public int PropertyID { get; set; }
    [ForeignKey(nameof(PropertyID))]
    public virtual InvestmentProperty Property { get; set; } = null!;

    public DateTime ValuationDate { get; set; }
    public decimal MarketValue { get; set; }

    [MaxLength(200)]
    public string? ValuedBy { get; set; }

    public string? ValuationReportPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new HashSet<Document>();
}
