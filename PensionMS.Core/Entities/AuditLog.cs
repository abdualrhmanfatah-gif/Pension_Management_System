using System.ComponentModel.DataAnnotations.Schema;

namespace PensionMS.Core.Entities;

/// <summary>
/// سجل العمليات
/// </summary>
public class AuditLog
{
    public long LogID { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بالعملية
    /// </summary>
    public int? UserID { get; set; }
    public virtual User? User { get; set; }

    /// <summary>
    /// معرف الجدول في SystemTables
    /// </summary>
    public int? TableID { get; set; }
    [ForeignKey("TableID")]
    public virtual SystemTable? SystemTable { get; set; }

    /// <summary>
    /// نوع العملية: Create / Update / Delete / ... إلخ
    /// </summary>
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// المعرف الرقمي للسجل المتأثر (المفتاح الأساسي)
    /// </summary>
    public int? RecordID { get; set; }

    /// <summary>
    /// مفتاح إضافي للسجل (إن لزم) — يمكن استخدامه للتوافق أو للربط مع أنظمة أخرى
    /// </summary>
    public int? EntityKey { get; set; }

    /// <summary>
    /// اسم عرضي واضح للكيان المتأثر (مثال: \"إبن: أحمد محمد\"، \"دفتر رقم 123\")
    /// </summary>
    public string? EntityDisplayName { get; set; }

    /// <summary>
    /// القيم القديمة (JSON) على مستوى الحقول
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// القيم الجديدة (JSON) على مستوى الحقول
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// عنوان الـ IP للجهاز الذي نفذ العملية
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// معلومات المتصفح/العميل
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// معرّف تجميعي لربط عدة عمليات ضمن نفس الطلب أو نفس العملية التجارية
    /// </summary>
    public Guid? CorrelationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
