using PensionMS.Core.DTOs.Common;

namespace PensionMS.Infrastructure.Services.Reporting;

internal static class ReportExportGuard
{
    public static List<T> RequireComplete<T>(ApiResponse<PagedResult<T>> result)
    {
        if (!result.IsSuccess || result.Data == null)
            throw new InvalidOperationException(result.Message ?? "تعذر إعداد بيانات التصدير");
        if (result.Data.TotalCount > result.Data.Items.Count)
            throw new InvalidOperationException(
                $"عدد النتائج ({result.Data.TotalCount:N0}) يتجاوز حد التصدير ({result.Data.PageSize:N0}). ضيّق نطاق البحث أو التاريخ ثم أعد التصدير.");
        return result.Data.Items;
    }
}
