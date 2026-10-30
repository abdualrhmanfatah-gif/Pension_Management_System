using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Ministry;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Infrastructure.Services.Pension.MinistryApproval;

namespace PensionMS.Tests.Services;

public class MinistryApprovalManualItemTests
{
    [Theory]
    [InlineData(BatchStatusConstants.Completed, true)]
    [InlineData(BatchStatusConstants.Draft, false)]
    [InlineData(BatchStatusConstants.Approved, false)]
    [InlineData(BatchStatusConstants.Imported, false)]
    public async Task AddManualItem_AllowsAnyBatchStatus(string status, bool firstItemProcessed)
    {
        var (service, batch, items) = CreateService(
            new List<MinistryApprovalItem> { Item(1, processed: firstItemProcessed) },
            Batch(status, totalCount: 1, processedCount: firstItemProcessed ? 1 : 0));

        var result = await service.AddManualItemAsync(1, AddDto("100", "اسم جديد"), 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, items.Count);
        Assert.Equal(2, batch.TotalCount);
        Assert.Equal(firstItemProcessed ? 1 : 0, batch.ProcessedCount);
        var expectedStatus = status == BatchStatusConstants.Completed
            ? BatchStatusConstants.Processing
            : status;
        Assert.Equal(expectedStatus, batch.Status);
    }

    [Fact]
    public async Task AddManualItem_ToNewBatch_RecountsTotalAndAppendsSerial()
    {
        var (service, batch, items) = CreateService(
            new List<MinistryApprovalItem> { Item(1), Item(2) },
            Batch(BatchStatusConstants.New, totalCount: 2, processedCount: 0));

        var result = await service.AddManualItemAsync(1, AddDto(" 200 ", "اسم جديد"), 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, items.Count);
        Assert.Equal(3, batch.TotalCount);
        Assert.Equal(3, items[^1].SerialNumber);
        Assert.Equal("200", items[^1].MilitaryNumber);
        Assert.False(items[^1].IsProcessed);
        Assert.Equal(BatchStatusConstants.New, batch.Status);
    }

    [Fact]
    public async Task AddManualItem_RejectsDuplicateMilitaryNumberWithinBatch()
    {
        var (service, _, items) = CreateService(
            new List<MinistryApprovalItem> { Item(1, military: "1") },
            Batch(BatchStatusConstants.New, totalCount: 1, processedCount: 0));

        var result = await service.AddManualItemAsync(1, AddDto("1 ", "اسم آخر"), 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("الرقم العسكري موجود مسبقاً في هذا الكشف", result.Message);
        Assert.Single(items);
    }

    [Fact]
    public async Task UpdateManualItem_RejectsProcessedItem()
    {
        var (service, _, items) = CreateService(
            new List<MinistryApprovalItem> { Item(1, processed: true) },
            Batch(BatchStatusConstants.Completed, totalCount: 1, processedCount: 1));

        var result = await service.UpdateManualItemAsync(1, UpdateDto("9", "اسم معدّل"));

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن تعديل قيد تمت معالجته", result.Message);
        Assert.Equal("1", items[0].MilitaryNumber);
        Assert.Equal("اسم مطابق", items[0].FullName);
    }

    [Fact]
    public async Task UpdateManualItem_AppliesChanges_AndRejectsDuplicateNumber()
    {
        var (service, _, items) = CreateService(
            new List<MinistryApprovalItem> { Item(1), Item(2) },
            Batch(BatchStatusConstants.Processing, totalCount: 2, processedCount: 0));

        var duplicate = await service.UpdateManualItemAsync(1, UpdateDto("2", "اسم مكرر"));
        Assert.False(duplicate.IsSuccess);
        Assert.Equal("الرقم العسكري موجود مسبقاً في هذا الكشف", duplicate.Message);

        var updated = await service.UpdateManualItemAsync(1, UpdateDto(" 77 ", "اسم معدّل"));
        Assert.True(updated.IsSuccess);
        Assert.Equal("77", items[0].MilitaryNumber);
        Assert.Equal("اسم معدّل", items[0].FullName);
    }

    [Fact]
    public async Task DeleteManualItem_SoftDeletesAndRecomputesBatchCounters()
    {
        var items = new List<MinistryApprovalItem> { Item(1, processed: true), Item(2) };
        var (service, batch, _) = CreateService(
            items,
            Batch(BatchStatusConstants.Processing, totalCount: 2, processedCount: 1));

        var result = await service.DeleteManualItemAsync(2, 5);

        Assert.True(result.IsSuccess);
        Assert.True(items[1].IsDeleted);
        Assert.Equal(5, items[1].DeletedByUserID);
        Assert.Equal(1, batch.TotalCount);
        Assert.Equal(1, batch.ProcessedCount);
        Assert.Equal(BatchStatusConstants.Completed, batch.Status);
    }

    [Fact]
    public async Task DeleteManualItem_RejectsProcessedItem()
    {
        var (service, _, items) = CreateService(
            new List<MinistryApprovalItem> { Item(1, processed: true) },
            Batch(BatchStatusConstants.Completed, totalCount: 1, processedCount: 1));

        var result = await service.DeleteManualItemAsync(1, 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن حذف قيد تمت معالجته", result.Message);
        Assert.False(items[0].IsDeleted);
    }

    private static AddManualItemDto AddDto(string militaryNumber, string fullName) =>
        new() { MilitaryNumber = militaryNumber, FullName = fullName };

    private static UpdateManualItemDto UpdateDto(string militaryNumber, string fullName) =>
        new() { MilitaryNumber = militaryNumber, FullName = fullName };

    private static MinistryApprovalItem Item(int id, bool processed = false, string? military = null) => new()
    {
        ApprovalItemID = id,
        BatchID = 1,
        SerialNumber = id,
        MilitaryNumber = military ?? id.ToString(),
        FullName = "اسم مطابق",
        IsProcessed = processed,
        CreatedAt = DateTime.UtcNow.AddMinutes(-id)
    };

    private static MinistryApprovalBatch Batch(string status, int totalCount, int processedCount) => new()
    {
        BatchID = 1,
        BatchNumber = "ك-1",
        BatchDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        ReceivedDate = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
        Status = status,
        TotalCount = totalCount,
        ProcessedCount = processedCount
    };

    private static (MinistryApprovalService Service, MinistryApprovalBatch Batch, List<MinistryApprovalItem> Items)
        CreateService(List<MinistryApprovalItem> items, MinistryApprovalBatch batch)
    {
        var itemRepository = new Mock<IRepository<MinistryApprovalItem>>();
        itemRepository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<MinistryApprovalItem>(items));
        itemRepository.Setup(r => r.AddAsync(It.IsAny<MinistryApprovalItem>()))
            .Callback<MinistryApprovalItem>(i => items.Add(i))
            .Returns(Task.CompletedTask);

        var batchRepository = new Mock<IRepository<MinistryApprovalBatch>>();
        batchRepository.Setup(r => r.Query())
            .Returns(() => new TestAsyncEnumerable<MinistryApprovalBatch>(new[] { batch }));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.MinistryApprovalItems).Returns(itemRepository.Object);
        unitOfWork.Setup(u => u.MinistryApprovalBatches).Returns(batchRepository.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var notifications = new Mock<INotificationService>();
        notifications.Setup(n => n.NotifyRoleAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()))
            .ReturnsAsync(ApiResponse<bool>.Success(true));

        var service = new MinistryApprovalService(
            unitOfWork.Object,
            notifications.Object,
            new ExcelReportExporter(new ExcelReportTemplate()),
            new PdfReportExporter());

        return (service, batch, items);
    }
}
