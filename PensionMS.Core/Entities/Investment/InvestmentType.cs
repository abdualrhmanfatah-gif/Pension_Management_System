using System.ComponentModel.DataAnnotations;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// نوع الاستثمار
/// </summary>
public class InvestmentType : ISoftDeletable
{
    [Key]
    public int TypeID { get; set; }

    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<InvestmentProject> Projects { get; set; } = new List<InvestmentProject>();
}
