namespace PensionMS.Core.Constants;

/// <summary>
/// ثوابت حالات الملفات — مطابقة لبيانات الـ Seed في PensionDbContext
/// </summary>
public static class FileStatusConstants
{
    public const int New = 1;           // جديد
    public const int Active = 2;        // نشط
    public const int EntitlementEnded = 4; // انتهى استحقاقه
    public const int Pending = 6;       // معلق

    public static bool IsCurrent(int status) => status is New or Active or Pending or EntitlementEnded;
    public static string Name(int status) => status switch
    {
        New => "جديد", Active => "نشط", Pending => "معلق",
        EntitlementEnded => "انتهى استحقاقه", _ => "حالة سابقة — يلزم تحويل الملف للمراجعة"
    };
}

/// <summary>معرّفات تاريخية للقراءة والتحويل فقط؛ ليست حالات ملف قابلة للاختيار.</summary>
public static class LegacyFileStatusConstants
{
    public const int Suspended = 3;
    public const int Incomplete = 5;
    public const int UnderReview = 7;
    public const int Rejected = 8;
}
