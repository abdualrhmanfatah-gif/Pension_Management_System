namespace PensionMS.Core.Entities;

/// <summary>
/// حدث خطورة محتمل (Fraud/Risk Detection)
/// يتم توليده آلياً بواسطة RiskScoringService
/// </summary>
public class RiskEvent
{
    public long Id { get; set; }

    /// <summary>
    /// معرّف الملف المرتبط بالحدث
    /// </summary>
    public int FileId { get; set; }
    public virtual RetirementFile File { get; set; } = null!;

    /// <summary>
    /// نوع الحدث
    /// </summary>
    public RiskType RiskType { get; set; }

    /// <summary>
    /// وصف تفصيلي للمخاطرة
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// مستوى الخطورة
    /// </summary>
    public RiskSeverity Severity { get; set; } = RiskSeverity.Low;

    /// <summary>
    /// نتيجة الفحص (درجة المخاطرة من 1 إلى 100)
    /// </summary>
    public int RiskScore { get; set; }

    /// <summary>
    /// حالة الحدث
    /// </summary>
    public RiskStatus Status { get; set; } = RiskStatus.Open;

    /// <summary>
    /// تاريخ وتوقت رفع الحدث
    /// </summary>
    public DateTime RaisedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاريخ حل الحدث
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// معرّف مسؤول الامتثال (Compliance Officer) الذي قام بالحل
    /// </summary>
    public int? ResolvedByUserId { get; set; }

    /// <summary>
    /// تعليق على الحل
    /// </summary>
    public string? ResolutionNotes { get; set; }
}
