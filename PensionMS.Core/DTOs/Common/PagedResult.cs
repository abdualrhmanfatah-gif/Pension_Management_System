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
