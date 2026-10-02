using System.ComponentModel.DataAnnotations;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// نوع القيد اليومي — جدول مرجعية لتصنيف القيود المحاسبية
/// </summary>
public class JournalEntryType : ISoftDeletable
{
    [Key]
    public int JournalEntryTypeID { get; set; }

    /// <summary>الاسم بالعربية</summary>
    [Required, MaxLength(50)]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>الاسم بالإنجليزية</summary>
    [Required, MaxLength(50)]
    public string TypeNameEn { get; set; } = string.Empty;

    /// <summary>كود مختصر (SAL, EXP, etc.)</summary>
    [MaxLength(20)]
    public string? Code { get; set; }

    /// <summary>لون HEX لتمييز النوع في الواجهة (#FF5733)</summary>
    [MaxLength(7)]
    public string? Color { get; set; }

    /// <summary>وصف مختصر لنوع القيد</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    // Navigation
    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}
