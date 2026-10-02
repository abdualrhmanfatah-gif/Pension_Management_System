namespace PensionMS.Core.Constants;

/// <summary>
/// ثوابت حالات الملفات — مطابقة لبيانات الـ Seed في PensionDbContext
/// </summary>
public static class FileStatusConstants
{
    public const int New = 1;           // جديد
    public const int Active = 2;        // نشط
    public const int Suspended = 3;     // موقوف
    public const int Closed = 4;        // مغلق
    public const int Incomplete = 5;    // ناقص
    public const int Pending = 6;       // معلق
    public const int UnderReview = 7;   // تحت المراجعة
    public const int Rejected = 8;      // مرفوض
}
