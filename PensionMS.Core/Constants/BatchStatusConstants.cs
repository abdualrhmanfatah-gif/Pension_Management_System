namespace PensionMS.Core.Constants;

/// <summary>
/// ثوابت حالات كشوف الصرف — للتحقق من صحة العمليات (استيراد، تحديث عناصر، اعتماد، حذف).
/// </summary>
public static class BatchStatusConstants
{
    /// <summary>مسودة — يمكن التعديل والتوليد والحذف؛ لا يُسمح باستيراد النتائج أو تحديث حالة العناصر.</summary>
    public const string Draft = "مسودة";

    /// <summary>معتمد — يُسمح باستيراد نتائج الحوالات وتحديث حالة العناصر؛ لا يمكن الحذف أو إعادة التوليد.</summary>
    public const string Approved = "معتمد";

    /// <summary>جديد — كشف تم إنشاؤه حديثاً ولم يتم العمل عليه بعد.</summary>
    public const string New = "جديد";

    /// <summary>تم الاستيراد — كشف تم استيراد بياناته من ملف خارجي.</summary>
    public const string Imported = "تم الاستيراد";

    /// <summary>قيد المعالجة — الكشف تحت المعالجة حالياً.</summary>
    public const string Processing = "قيد المعالجة";

    /// <summary>مكتمل — تم الانتهاء من معالجة الكشف بالكامل.</summary>
    public const string Completed = "مكتمل";

    /// <summary>معالج — قيد تم معالجته.</summary>
    public const string Processed = "معالج";

    /// <summary>معلق — قيد بانتظار المعالجة.</summary>
    public const string Pending = "معلق";

    /// <summary>قيمة قديمة محتملة (ترميز خاطئ) لمسودة.</summary>
    public const string LegacyDraft = "ظ…ط³ظˆط¯ط©";

    /// <summary>قيمة قديمة محتملة (ترميز خاطئ) لمعتمد.</summary>
    public const string LegacyApproved = "ظ…ط¹طھظ…ط¯";

    public static bool IsDraft(string? status) =>
        status == Draft || status == LegacyDraft;

    public static bool IsApproved(string? status) =>
        status == Approved || status == LegacyApproved;
}
