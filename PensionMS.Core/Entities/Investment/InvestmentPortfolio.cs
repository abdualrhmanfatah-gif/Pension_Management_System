using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// محفظة استثمارية
/// </summary>
public class InvestmentPortfolio : ISoftDeletable
{
    [Key]
    public int PortfolioID { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int ManagerUserID { get; set; }
    [ForeignKey(nameof(ManagerUserID))]
    public virtual User Manager { get; set; } = null!;

    public decimal TotalTargetCapital { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<InvestmentProject> Projects { get; set; } = new List<InvestmentProject>();
}
