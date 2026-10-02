using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PensionMS.Core.Entities;

public class FileReviewItem
{
    [Key]
    public long ItemID { get; set; }

    public int RetirementFileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public long? AuditLogID { get; set; }
    [ForeignKey("AuditLogID")]
    public virtual AuditLog? AuditLog { get; set; }

    public int? EntityType { get; set; }
    public int? EntityID { get; set; }

    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public string? SectionName { get; set; }
    public string? SectionType { get; set; }
    public string? EntityDisplayName { get; set; }

    /// <summary>
    /// JSON serialized array of field-level diffs: [{fieldName, oldValue, newValue}, ...]
    /// للعرض فقط (قراءة) — لا يستخدم في approve/reject
    /// </summary>
    public string? FieldChangesJson { get; set; }

    public int Status { get; set; } = 0; // 0=Pending, 1=Approved, 2=Rejected

    public int? ReviewedByUserID { get; set; }
    [ForeignKey("ReviewedByUserID")]
    public virtual User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
