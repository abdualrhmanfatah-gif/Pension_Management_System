using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// مركز التكلفة — لتوزيع المصروفات والإيرادات على الوحدات التنظيمية
/// </summary>
public class CostCenter : ISoftDeletable
{
    [Key]
    public int CostCenterID { get; set; }

    /// <summary>رمز مركز التكلفة</summary>
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>اسم مركز التكلفة</summary>
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>معرف مركز التكلفة الأب (للهيكل الهرمي)</summary>
    public int? ParentCostCenterID { get; set; }

    /// <summary>مركز التكلفة نشط</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ParentCostCenterID))]
    public virtual CostCenter? ParentCostCenter { get; set; }

    public virtual ICollection<CostCenter> Children { get; set; } = new List<CostCenter>();
}
