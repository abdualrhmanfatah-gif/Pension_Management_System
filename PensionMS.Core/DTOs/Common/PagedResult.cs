namespace PensionMS.Core.DTOs.Common;

/// <summary>
/// نتيجة مقسمة إلى صفحات — تدعم كلاً من pagination التقليدي والمبني على Cursor
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    /// <summary>آخر ID في الصفحة الحالية — يُستخدم cursor-based pagination</summary>
    public int? NextCursor { get; set; }

    /// <summary>يوجد بيانات بعد هذا المؤشر</summary>
    public bool HasMore { get; set; }
}
<<<<<<< HEAD

/// <summary>
/// حارس قابلية استكمال التصدير — يمنع تصدير ناقص بصمت عند عبور حدود البيانات المقطعة
/// </summary>
public static class PagedResultGuards
{
    public static List<T> RequireComplete<T>(this ApiResponse<PagedResult<T>> result, string? context = null)
    {
        if (!result.IsSuccess || result.Data == null)
            throw new InvalidOperationException(result.Message ?? $"تعذر إعداد بيانات التصدير{(string.IsNullOrEmpty(context) ? "" : $": {context}")}");
        if (result.Data.TotalCount > result.Data.Items.Count)
            throw new InvalidOperationException(
                $"عدد النتائج ({result.Data.TotalCount:N0}) يتجاوز حد التصدير ({result.Data.PageSize:N0}). ضيّق نطاق البحث أو التاريخ ثم أعد التصدير.{(string.IsNullOrEmpty(context) ? "" : $" ({context})")}");
        return result.Data.Items;
    }
}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
