using PensionMS.Core.DTOs.Common;

namespace PensionMS.Tests.Services;

public class PagedResultGuardsTests
{
    [Fact]
    public void TruncatedOrFailedReport_CannotBecomeSuccessfulExport()
    {
        ApiResponse<PagedResult<int>>? Build(int? totalCount, int pageSize = 2)
            => totalCount == null
                ? ApiResponse<PagedResult<int>>.Fail("فشل")
                : ApiResponse<PagedResult<int>>.Success(new PagedResult<int>
                {
                    Items = Enumerable.Range(1, Math.Min(2, totalCount.Value)).ToList(),
                    TotalCount = totalCount.Value,
                    PageSize = pageSize
                });

        Assert.Throws<InvalidOperationException>(() => Build(totalCount: 3)!.RequireComplete());
        Assert.Throws<InvalidOperationException>(() => Build(null)!.RequireComplete());
        Assert.Equal(new[] { 1, 2 }, Build(totalCount: 2)!.RequireComplete());
        var limit = Assert.Throws<InvalidOperationException>(() => Build(totalCount: 10_001, pageSize: 10_000)!.RequireComplete());
        Assert.Contains("حد التصدير", limit.Message);
    }
}
