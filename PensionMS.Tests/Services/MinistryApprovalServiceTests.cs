using PensionMS.Infrastructure.Services.Pension.MinistryApproval;
using Moq;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Ministry;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Tests.Services;

public class MinistryApprovalServiceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GenderFilter_FindsMatchingItemBeyondTheFirstUnfilteredPage(bool batchOnly)
    {
        var items = Enumerable.Range(1, 20).Select(id => Item(id, 1)).ToList();
        items.Add(Item(21, 2));
        var service = CreateService(items);

        var result = await GetItems(service, batchOnly, 1, 20, 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal(21, Assert.Single(result.Data.Items).ApprovalItemID);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GenderFilter_CountsAndPaginatesOnlyMatchingItems(bool batchOnly)
    {
        var items = Enumerable.Range(1, 20).Select(id => Item(id, 1)).ToList();
        items.AddRange(Enumerable.Range(21, 5).Select(id => Item(id, 2)));
        var service = CreateService(items);

        var result = await GetItems(service, batchOnly, 2, 2, 2);

        Assert.Equal(5, result.Data!.TotalCount);
        Assert.Equal(3, result.Data.TotalPages);
        Assert.Equal(new[] { 23, 24 }, result.Data.Items.Select(i => i.ApprovalItemID));
        Assert.All(result.Data.Items, i => Assert.Equal((byte?)2, i.Gender));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GenderFilter_ExcludesUnknownGender_WhileAllIncludesIt(bool batchOnly)
    {
        var service = CreateService(new[] { Item(1, 1), Item(2, 2), Item(3, null) });

        var males = await GetItems(service, batchOnly, 1, 20, 1);
        var females = await GetItems(service, batchOnly, 1, 20, 2);
        var all = await GetItems(service, batchOnly, 1, 20, null);

        Assert.Equal(1, males.Data!.TotalCount);
        Assert.Equal(1, Assert.Single(males.Data.Items).ApprovalItemID);
        Assert.Equal(1, females.Data!.TotalCount);
        Assert.Equal(2, Assert.Single(females.Data.Items).ApprovalItemID);
        Assert.Equal(3, all.Data!.TotalCount);
        Assert.Contains(all.Data.Items, i => i.Gender == null);
    }

    [Fact]
    public async Task BatchGenderFilter_PreservesSearchBatchAndSoftDeleteFilters()
    {
        var otherBatch = Item(3, 2);
        otherBatch.BatchID = 2;
        var deleted = Item(4, 2);
        deleted.IsDeleted = true;
        var nonMatchingName = Item(5, 2);
        nonMatchingName.FullName = "اسم آخر";
        var service = CreateService(new[] { Item(1, 1), Item(2, 2), otherBatch, deleted, nonMatchingName });

        var result = await service.GetBatchItemsAsync(1, 1, 20, "مطابق", 2);

        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal(2, Assert.Single(result.Data.Items).ApprovalItemID);
    }

    [Fact]
    public async Task AllItemsGenderFilter_PreservesSearchProcessedAndSoftDeleteFilters()
    {
        var pending = Item(3, 2);
        pending.IsProcessed = false;
        var deleted = Item(4, 2);
        deleted.IsDeleted = true;
        var nonMatchingName = Item(5, 2);
        nonMatchingName.FullName = "اسم آخر";
        var service = CreateService(new[] { Item(1, 1), Item(2, 2), pending, deleted, nonMatchingName });

        var result = await service.GetAllItemsAsync(1, 1, "مطابق", true, 2);

        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal(2, Assert.Single(result.Data.Items).ApprovalItemID);
    }

    private static Task<ApiResponse<PagedResult<MinistryApprovalItemDto>>> GetItems(
        MinistryApprovalService service, bool batchOnly, int page, int pageSize, byte? gender) =>
        batchOnly
            ? service.GetBatchItemsAsync(1, page, pageSize, null, gender)
            : service.GetAllItemsAsync(page, pageSize, null, null, gender);

    private static MinistryApprovalItem Item(int id, byte? gender) => new()
    {
        ApprovalItemID = id,
        BatchID = 1,
        SerialNumber = id,
        FullName = "اسم مطابق",
        MilitaryNumber = id.ToString(),
        Gender = gender,
        IsProcessed = true,
        CreatedAt = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(-id)
    };

    private static MinistryApprovalService CreateService(IEnumerable<MinistryApprovalItem> items)
    {
        var repository = new Mock<IRepository<MinistryApprovalItem>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<MinistryApprovalItem>(items));
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(u => u.MinistryApprovalItems).Returns(repository.Object);
        return new MinistryApprovalService(unit.Object, Mock.Of<INotificationService>(),
            new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportExporter(
                new PensionMS.Infrastructure.Reporting.Exporters.ExcelReportTemplate()),
            new PensionMS.Infrastructure.Reporting.Exporters.PdfReportExporter());
    }
}
