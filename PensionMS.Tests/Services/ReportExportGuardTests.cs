using PensionMS.Core.DTOs.Common;
using PensionMS.Infrastructure.Services.Reporting;

namespace PensionMS.Tests.Services;

public class ReportExportGuardTests
{
    [Fact]
    public void TruncatedOrFailedReport_CannotBecomeSuccessfulExport()
    {
        Assert.Throws<InvalidOperationException>(() => ReportExportGuard.RequireComplete(
            ApiResponse<PagedResult<int>>.Success(new PagedResult<int> { Items = [1, 2], TotalCount = 3, PageSize = 2 })));
        Assert.Throws<InvalidOperationException>(() => ReportExportGuard.RequireComplete(ApiResponse<PagedResult<int>>.Fail("فشل")));
        Assert.Equal(new[] { 1, 2 }, ReportExportGuard.RequireComplete(
            ApiResponse<PagedResult<int>>.Success(new PagedResult<int> { Items = [1, 2], TotalCount = 2, PageSize = 2 })));
        var limit = Assert.Throws<InvalidOperationException>(() => ReportExportGuard.RequireComplete(
            ApiResponse<PagedResult<int>>.Success(new PagedResult<int>
            { Items = Enumerable.Range(1, 10_000).ToList(), TotalCount = 10_001, PageSize = 10_000 })));
        Assert.Contains("حد التصدير", limit.Message);
    }
}
