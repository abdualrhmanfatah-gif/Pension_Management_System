namespace PensionMS.Core.Entities;

/// <summary>
/// لقطة ملف — تُخزن حالة الملفات في نقطة زمنية محددة
/// تُستخدم لتتبع التغييرات الرئيسية (تغيير حالة، تعديل مالي، ...)
/// </summary>
public class FileSnapshot
{
    public long Id { get; set; }

    public int FileId { get; set; }
    public virtual RetirementFile File { get; set; } = null!;

    /// <summary>
    /// رقم النسخة التسلسلي
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// نوع اللقطة: StatusChange / FinancialEdit / MajorUpdate / Creation
    /// </summary>
    public string SnapshotType { get; set; } = string.Empty;

    /// <summary>
    /// الحالة الكاملة للملف بصيغة JSON
    /// </summary>
    public string FullState { get; set; } = string.Empty;

    /// <summary>
    /// فرق التغيير عن النسخة السابقة بصيغة JSON (اختياري)
    /// </summary>
    public string? JsonDiff { get; set; }

    /// <summary>
    /// معرّف المستخدم الذي أحدث التغيير
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// سبب التغيير (اختياري)
    /// </summary>
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
