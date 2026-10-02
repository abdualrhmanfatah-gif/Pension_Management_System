using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// ملاحظات الملفات
/// </summary>
public class FileNote : ISoftDeletable
{
    public int NoteID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public int? ResolvedByUserID { get; set; }
    public virtual User? ResolvedByUser { get; set; }

    public string NoteText { get; set; } = string.Empty;
    public string NoteType { get; set; } = string.Empty;
    public byte Priority { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
