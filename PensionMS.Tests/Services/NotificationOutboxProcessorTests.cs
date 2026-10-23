using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Notifications;
using PensionMS.Infrastructure.Jobs;
using PensionMS.Infrastructure.Services.Notifications;

namespace PensionMS.Tests.Services;

/// اختبارات انحدار لـ N11/N12 وضوابط N10.
/// الحمراء تُغلق عند NF4: إخفاق بث العدد لا يعيد إرسال الرسالة الناجحة.
public class NotificationOutboxProcessorTests : IDisposable
{
    private readonly List<Notification> _notifications = [];
    private readonly List<NotificationOutbox> _outbox = [];
    private readonly Mock<IRepository<Notification>> _notificationRepo = new();
    private readonly Mock<IRepository<NotificationOutbox>> _outboxRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<INotificationClient> _client = new();
    private readonly Mock<IHubContext<NotificationHub, INotificationClient>> _hub = new();
    private readonly ServiceProvider _provider;

    public NotificationOutboxProcessorTests()
    {
        _notificationRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<Notification>(_notifications));
        _outboxRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<NotificationOutbox>(_outbox));
        _outboxRepo.Setup(r => r.Update(It.IsAny<NotificationOutbox>()));
        _unitOfWork.SetupGet(u => u.Notifications).Returns(_notificationRepo.Object);
        _unitOfWork.SetupGet(u => u.NotificationOutbox).Returns(_outboxRepo.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _client.Setup(c => c.ReceiveNotification(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);
        _client.Setup(c => c.UnreadCountUpdated(It.IsAny<int>())).Returns(Task.CompletedTask);
        var clients = new Mock<IHubClients<INotificationClient>>();
        clients.Setup(c => c.Group(It.IsAny<string>())).Returns(_client.Object);
        _hub.SetupGet(h => h.Clients).Returns(clients.Object);

        _provider = new ServiceCollection()
            .AddSingleton(_hub.Object)
            .AddScoped<IUnitOfWork>(_ => _unitOfWork.Object)
            .BuildServiceProvider();
    }

    public void Dispose() => _provider.Dispose();

    private void SeedPending()
    {
        _notifications.Add(new Notification { NotificationID = 1, UserID = 99, Title = "اختبار" });
        _outbox.Add(new NotificationOutbox { OutboxId = 1, NotificationId = 1 });
    }

    private NotificationOutboxProcessor CreateProcessor() =>
        new(_provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<NotificationOutboxProcessor>.Instance);

    [Fact]
    public async Task CountPushFailure_DoesNotResendDeliveredMessage()
    {
        SeedPending();
        _client.SetupSequence(c => c.UnreadCountUpdated(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("خطأ اختبار"))
            .Returns(Task.CompletedTask);
        var processor = CreateProcessor();

        await processor.ProcessPendingItemsAsync(CancellationToken.None);
        await processor.ProcessPendingItemsAsync(CancellationToken.None);

        // N11: الرسالة وصلت في الدورة الأولى؛ إخفاق العدّاد لا يعيد إرسالها
        _client.Verify(c => c.ReceiveNotification(It.IsAny<NotificationDto>()), Times.Once);
        Assert.Equal(OutboxStatus.Sent, _outbox[0].Status);
        Assert.True(_outbox[0].RetryCount == 0, "إخفاق العدّاد لا يدخل في دورة إعادة إرسال الرسالة");
    }

    [Fact]
    public async Task SuccessfulBroadcast_MarksSentAndPushesCount()
    {
        SeedPending();

        await CreateProcessor().ProcessPendingItemsAsync(CancellationToken.None);

        Assert.Equal(OutboxStatus.Sent, _outbox[0].Status);
        Assert.NotNull(_outbox[0].LastAttemptAt);
        _client.Verify(c => c.ReceiveNotification(It.IsAny<NotificationDto>()), Times.Once);
        _client.Verify(c => c.UnreadCountUpdated(1), Times.Once);
    }

    [Fact]
    public async Task MissingNotification_MarksFailedWithoutBroadcast()
    {
        _outbox.Add(new NotificationOutbox { OutboxId = 1, NotificationId = 404 });

        await CreateProcessor().ProcessPendingItemsAsync(CancellationToken.None);

        Assert.Equal(OutboxStatus.Failed, _outbox[0].Status);
        Assert.Equal("Notification not found", _outbox[0].ErrorMessage);
        _client.Verify(c => c.ReceiveNotification(It.IsAny<NotificationDto>()), Times.Never);
    }

    [Fact]
    public async Task ExhaustedRetries_MarksFailedAndSkipsBroadcast()
    {
        _notifications.Add(new Notification { NotificationID = 1, UserID = 99, Title = "اختبار" });
        _outbox.Add(new NotificationOutbox
        {
            OutboxId = 1, NotificationId = 1, RetryCount = 3, MaxRetries = 3
        });

        await CreateProcessor().ProcessPendingItemsAsync(CancellationToken.None);

        _client.Verify(c => c.ReceiveNotification(It.IsAny<NotificationDto>()), Times.Never);
        Assert.Equal(OutboxStatus.Pending, _outbox[0].Status);
    }
}
