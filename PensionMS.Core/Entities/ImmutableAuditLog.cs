namespace PensionMS.Core.Entities;

/// <summary>
/// سجل تدقيق مقاوم للتلاعب مع تسلسل تجزئة SHA-256
/// كل سجل يحمل تجزئة السجل السابق لضمان سلامة السلسلة
/// </summary>
public class ImmutableAuditLog
{
    public long Id { get; set; }

    /// <summary>
    /// معرّف المستخدم الذي نفّذ العملية
    /// </summary>
    public int ActorId { get; set; }

    /// <summary>
    /// اسم المستخدم وقت التنفيذ (لقطة — لا يتغير مع تحديث المستخدم)
    /// </summary>
    public string ActorName { get; set; } = string.Empty;

    /// <summary>
    /// نوع العملية: Create / Update / Delete / ChangeStatus / Approve / Override
    /// </summary>
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// اسم الكيان المستهدف: RetirementFile, EntitledPerson, ...
    /// </summary>
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// معرّف السجل المستهدف
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// القيم القديمة بصيغة JSON (فارغ عند Create)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// القيم الجديدة بصيغة JSON (فارغ عند Delete)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// معرّف الفرع الذي تمت العملية منه
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// عنوان IP المصدر
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// تجزئة SHA-256 لهذا السجل (تشمل PreviousHash + البيانات)
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// تجزئة السجل السابق — يُشكل سلسلة تجزئة غير قابلة للكسر
    /// أول سجل يحمل "GENESIS"
    /// </summary>
    public string PreviousHash { get; set; } = "GENESIS";

    /// <summary>
    /// معرّف ارتباطي لربط سجلات نفس العملية التجارية
    /// </summary>
    public Guid? CorrelationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
