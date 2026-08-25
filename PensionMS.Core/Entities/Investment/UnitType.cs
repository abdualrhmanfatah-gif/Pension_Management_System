using System.ComponentModel.DataAnnotations;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// نوع الوحدة (شقة، محل، مكتب، الخ)
/// </summary>
public class UnitType : ISoftDeletable
{
    [Key]
    public int TypeID { get; set; }

    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<PropertyUnit> Units { get; set; } = new List<PropertyUnit>();
}
