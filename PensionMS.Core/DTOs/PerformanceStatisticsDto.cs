namespace PensionMS.Core.DTOs;

/// <summary>
/// إحصائيات الأداء الشهري
/// </summary>
public class PerformanceStatisticsDto
{
    /// <summary>إجمالي المتقاعدين</summary>
    public int TotalRetirees { get; set; }

    /// <summary>الملفات النشطة</summary>
    public int ActiveFiles { get; set; }

    /// <summary>قيد المراجعة</summary>
    public int UnderReviewFiles { get; set; }

    /// <summary>الملفات الجديدة</summary>
    public int NewFiles { get; set; }

    /// <summary>بيانات الأداء الشهري</summary>
    public List<MonthlyPerformanceItemDto> MonthlyData { get; set; } = new();
}

/// <summary>
/// عنصر أداء شهري
/// </summary>
public class MonthlyPerformanceItemDto
{
    /// <summary>اسم الشهر</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>عدد المتقاعدين</summary>
    public int Retirees { get; set; }

    /// <summary>عدد الملفات</summary>
    public int Files { get; set; }

    /// <summary>الملفات الجديدة</summary>
    public int NewFiles { get; set; }
}
