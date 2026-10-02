namespace PensionMS.Core.DTOs.Documents;

/// <summary>
/// طلب التحديث النهائي بعد رفع مجموعة وثائق بوضع التأجيل.
/// يعيد حساب النواقص والامتثال والمراجعة مرة واحدة لكل المجموعة.
/// آمن لإعادة التكرار: نفس المدخلات تعطي نفس الحالة النهائية.
/// </summary>
public class RefreshAfterUploadRequest
{
    public List<int> DocumentIds { get; set; } = new();
}

/// <summary>
/// نتيجة التحديث النهائي بعد الرفع.
/// </summary>
public class RefreshAfterUploadResult
{
    public int ProcessedDocuments { get; set; }
    public List<int> SkippedDocumentIds { get; set; } = new();
    public int FulfilledMissingCount { get; set; }
    public int MissingDocumentsCount { get; set; }
    public bool ReviewRequested { get; set; }
}
