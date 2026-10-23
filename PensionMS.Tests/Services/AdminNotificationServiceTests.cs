using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Notifications;
using PensionMS.Infrastructure.Services.Notifications;

namespace PensionMS.Tests.Services;

/// اختبارات انحدار لـ N13: الحذف الجماعي يجب أن يرفض الطلب الفارغ
/// بدل حذف كامل الجدول. الاختبارات الحمراء تُغلق عند NF1.
public class AdminNotificationServiceTests
{
    private readonly List<Notification> _notifications = [];
    private readonly Mock<IRepository<Notification>> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IImmutableAuditService> _audit = new();
    private readonly AdminNotificationService _service;

    public AdminNotificationServiceTests()
    {
        _repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<Notification>(_notifications));
        _repo.Setup(r => r.Delete(It.IsAny<Notification>()))
            .Callback<Notification>(n => _notifications.Remove(n));
        _unitOfWork.SetupGet(u => u.Notifications).Returns(_repo.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _service = new AdminNotificationService(
            _unitOfWork.Object,
            Mock.Of<INotificationService>(),
            Mock.Of<IHubContext<NotificationHub, INotificationClient>>(),
            Mock.Of<IFileStorageService>(),
            _audit.Object);
    }

    private void Seed(int count)
    {
        for (var i = 1; i <= count; i++)
            _notifications.Add(new Notification { NotificationID = i, UserID = 1, Title = $"إشعار {i}" });
    }

    [Fact]
    public async Task BulkDelete_EmptyDto_IsRejectedAndDeletesNothing()
    {
        Seed(3);

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto());

        Assert.False(result.IsSuccess, "الطلب الفارغ يجب أن يُرفض");
        Assert.Equal(3, _notifications.Count);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task BulkDelete_OnlyReadFalseWithoutIdsOrFilters_IsRejected()
    {
        Seed(2);

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto { OnlyRead = false });

        Assert.False(result.IsSuccess, "OnlyRead=false وحده ليس اختيارًا");
        Assert.Equal(2, _notifications.Count);
    }

    [Fact]
    public async Task BulkDelete_EmptyIdsList_IsRejected()
    {
        Seed(2);

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto { Ids = [] });

        Assert.False(result.IsSuccess, "قائمة IDs فارغة ليست اختيارًا");
        Assert.Equal(2, _notifications.Count);
    }

    [Fact]
    public async Task BulkDelete_DeleteAllWithoutOtherFilters_DeletesEverything()
    {
        Seed(2);

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto { DeleteAll = true });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data);
        Assert.Empty(_notifications);
    }

    [Fact]
    public async Task BulkDelete_ExplicitIds_DeletesOnlyThose()
    {
        Seed(3);

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto { Ids = [1, 3] });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data);
        Assert.Equal([2], _notifications.Select(n => n.NotificationID));
    }

    [Fact]
    public async Task BulkDelete_CategoryFilter_DeletesOnlyMatching()
    {
        _notifications.Add(new Notification { NotificationID = 1, Category = "FINANCIAL" });
        _notifications.Add(new Notification { NotificationID = 2, Category = "WORKFLOW" });
        _notifications.Add(new Notification { NotificationID = 3, Category = "FINANCIAL" });

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto { Category = "FINANCIAL" });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data);
        Assert.Equal([2], _notifications.Select(n => n.NotificationID));
    }

    [Fact]
    public async Task BulkDelete_DeleteAllWithExplicitIds_IsRejected()
    {
        Seed(2);

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto { DeleteAll = true, Ids = [1] });

        Assert.False(result.IsSuccess, "الاختيارات المتعارضة تُرفض لا تُفسَّر بصمت");
        Assert.Equal(2, _notifications.Count);
    }

    [Fact]
    public async Task BulkDelete_DeleteAllWithCategory_IntersectionNotExpansion()
    {
        _notifications.Add(new Notification { NotificationID = 1, Category = "FINANCIAL" });
        _notifications.Add(new Notification { NotificationID = 2, Category = "WORKFLOW" });

        var result = await _service.BulkDeleteAsync(new BulkDeleteDto { DeleteAll = true, Category = "FINANCIAL" });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data);
        Assert.Equal([2], _notifications.Select(n => n.NotificationID));
    }

    [Fact]
    public async Task BulkDelete_Successful_DeletesAreAudited()
    {
        Seed(2);

        await _service.BulkDeleteAsync(new BulkDeleteDto { DeleteAll = true }, actorId: 42);

        _audit.Verify(a => a.LogAsync(
            42,
            It.IsAny<string>(),
            "BulkDelete",
            nameof(Notification),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task BulkDelete_Rejected_DoesNotAudit()
    {
        Seed(1);

        await _service.BulkDeleteAsync(new BulkDeleteDto(), actorId: 42);

        _audit.Verify(a => a.LogAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(),
            It.IsAny<string?>(), It.IsAny<Guid?>()), Times.Never);
    }
}
