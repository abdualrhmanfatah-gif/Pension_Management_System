namespace PensionMS.Core.DTOs.RetirementFiles;

/// <summary>
/// سجل تغييرات الملف - يجمع بين تغييرات الحالة وسجل العمليات
/// </summary>
public class FileChangeLogDto
{
    /// <summary>
    /// نوع التغيير: StatusChange, Create, Update, Delete
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// وصف التغيير بالعربية
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// اسم الجدول/القسم المتأثر (مثال: المستحقون، دفاتر الصرف)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// معرف السجل داخل الجدول (إن توفر)
    /// </summary>
    public int? EntityKey { get; set; }

    /// <summary>
    /// اسم عرضي واضح للسجل (مثال: \"إبن: أحمد محمد\"، \"دفتر رقم 123\")
    /// </summary>
    public string? EntityDisplayName { get; set; }

    /// <summary>
    /// الحالة القديمة (لتغييرات الحالة فقط)
    /// </summary>
    public string? OldStatusName { get; set; }

    /// <summary>
    /// الحالة الجديدة (لتغييرات الحالة فقط)
    /// </summary>
    public string? NewStatusName { get; set; }

    /// <summary>
    /// السبب (لتغييرات الحالة فقط)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// التغييرات على مستوى الحقول — في حال توفر OldValues/NewValues
    /// </summary>
    public List<FieldChangeDto>? FieldChanges { get; set; }

    /// <summary>
    /// تاريخ التغيير
    /// </summary>
    public DateTime ChangeDate { get; set; }

    /// <summary>
    /// اسم المستخدم الذي أجرى التغيير
    /// </summary>
    public string? ChangedByUserName { get; set; }

    /// <summary>
    /// عنوان IP
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// معلومات المتصفح/الجهاز
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// معرف تجميعي لربط عدة سجلات ضمن نفس العملية (إن توفر)
    /// </summary>
    public Guid? CorrelationId { get; set; }
}

/// <summary>
/// تفاصيل تغيير حقل فردي — للعرض فقط (قراءة)
/// </summary>
public class FieldChangeDetailDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    /// <summary>
    /// يحدد ما إذا كان الحقل يمثل مسار ملف أو صورة (لعرضها في الواجهة)
    /// </summary>
    public bool IsMedia { get; set; }
}

/// <summary>
/// تمثيل تغيير كيان واحد في المراجعة (إضافة/تعديل/حذف entity)
/// </summary>
public class FieldChangeDto
{
    /// <summary>
    /// اسم الحقل — أو وصف التغيير للكيان (مثال: "إضافة مستحق"، "تعديل بيانات الملف")
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// القيمة القديمة — أو اسم الكيان القديم
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// القيمة الجديدة — أو اسم الكيان الجديد
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// معرف بند المراجعة (FileReviewItem) — لاعتماد/رفض تغيير الكيان
    /// </summary>
    public long? ItemId { get; set; }

    /// <summary>
    /// حالة بند المراجعة: 0=Pending, 1=Approved, 2=Rejected
    /// </summary>
    public int? ItemStatus { get; set; }

    /// <summary>
    /// JSON خام لتفاصيل الحقول (للعرض فقط)
    /// </summary>
    public string? FieldChangesJson { get; set; }

    /// <summary>
    /// تفاصيل الحقول المفكوكة (للعرض فقط — بدون أزرار)
    /// </summary>
    public List<FieldChangeDetailDto>? FieldDetails { get; set; }

    /// <summary>
    /// اسم المستخدم الذي قام بالتعديل
    /// </summary>
    public string? ModifiedByUserName { get; set; }
}
