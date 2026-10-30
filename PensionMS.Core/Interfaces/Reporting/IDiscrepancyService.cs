using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IDiscrepancyService
{
    /// <summary>
    /// الحصول على تقرير المباينة بين الكشوفات والسجلات والملفات
    /// </summary>
    Task<ApiResponse<PagedResult<RetireeDiscrepancyDto>>> GetDiscrepancyReportAsync(DiscrepancyFilterDto filter);

    /// <summary>
    /// الحصول على ملخص تقارير كشوفات الإحالة إلى التقاعد
    /// </summary>
    Task<ApiResponse<KashfReportSummaryDto>> GetKashfReportSummaryAsync(DiscrepancyFilterDto filter);

    /// <summary>
    /// تصدير تقرير كشوفات الإحالة إلى التقاعد
    /// </summary>
    Task<byte[]> ExportKashfReportAsync(DiscrepancyFilterDto filter, string format);
}
