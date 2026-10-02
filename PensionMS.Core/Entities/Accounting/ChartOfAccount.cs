using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// الدليل المحاسبي — شجرة الحسابات وفق النظام المحاسبي الموحد (5 مستويات)
/// </summary>
public class ChartOfAccount : ISoftDeletable
{
    [Key]
    public int AccountID { get; set; }

    /// <summary>رمز الحساب (مثال: 11221)</summary>
    [Required, MaxLength(10)]
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>اسم الحساب بالعربية</summary>
    [Required, MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>طبيعة الحساب: Debit (مدين) / Credit (دائن)</summary>
    [Required, MaxLength(10)]
    public string AccountNature { get; set; } = string.Empty;

    /// <summary>مستوى الحساب: 1 = باب, 2 = عام, 3 = مساعد, 4 = فرعي</summary>
    public int AccountLevel { get; set; }

    /// <summary>معرف الحساب الأب (للعلاقة الهرمية)</summary>
    public int? ParentAccountID { get; set; }

    /// <summary>رقم الكتاب (1=موجودات, 2=مطلوبات, 3=استخدامات, 4=موارد, 5=نظامية)</summary>
    public int BookNumber { get; set; }

    /// <summary>هل يمكن التقيد على هذا الحساب</summary>
    public bool IsPostable { get; set; }

    /// <summary>الحساب نشط</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>تصنيف الحساب المحاسبي</summary>
    public AccountCategory AccountCategory { get; set; } = AccountCategory.Asset;

    /// <summary>وصف الحساب</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ParentAccountID))]
    public virtual ChartOfAccount? ParentAccount { get; set; }

    public virtual ICollection<ChartOfAccount> Children { get; set; } = new List<ChartOfAccount>();
}
