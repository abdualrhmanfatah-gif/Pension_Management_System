using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Notifications;
using PensionMS.Infrastructure.Jobs;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Notifications;

namespace PensionMS.IntegrationTests;

public class NotificationOutboxSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task Outbox_StagesWithoutFlushingAndUsesThreeReadsForFiftyMessages()
    {
        var client = new Mock<INotificationClient>();
        client.Setup(c => c.ReceiveNotification(It.IsAny<NotificationDto>())).Returns(Task.CompletedTask);
        client.Setup(c => c.UnreadCountUpdated(It.IsAny<int>())).Returns(Task.CompletedTask);
        var clients = new Mock<IHubClients<INotificationClient>>();
        clients.Setup(c => c.Group(It.IsAny<string>())).Returns(client.Object);
        var hub = new Mock<IHubContext<NotificationHub, INotificationClient>>();
        hub.SetupGet(h => h.Clients).Returns(clients.Object);
        await using (var db = database.OpenContext())
        using (var uow = new UnitOfWork(db))
        {
            var user = new User { Username = "outbox", FullName = "اختبار", PasswordHash = "test" };
            db.Add(user);
            await db.SaveChangesAsync();
            var notifications = new NotificationService(uow, hub.Object, Mock.Of<IFileStorageService>(), PensionMS.Testing.TestPensionAccess.AllowAllPermissions());
            for (var i = 0; i < 50; i++)
                await notifications.EnqueueAsync(new CreateNotificationDto { UserID = user.UserID, Title = "اختبار", Message = i.ToString() });
            Assert.Equal(0, await db.Notifications.CountAsync());
            client.Verify(c => c.ReceiveNotification(It.IsAny<NotificationDto>()), Times.Never);
            await uow.SaveChangesAsync();
            Assert.Equal(50, await db.NotificationOutbox.CountAsync());
            client.Verify(c => c.ReceiveNotification(It.IsAny<NotificationDto>()), Times.Never);
        }
        var counter = new ReadCounter();
        var services = new ServiceCollection();
        services.AddScoped(_ => database.OpenContext(counter));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton(hub.Object);
        await using var provider = services.BuildServiceProvider();
        var processor = new NotificationOutboxProcessor(provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<NotificationOutboxProcessor>.Instance);
        await processor.ProcessPendingItemsAsync(CancellationToken.None);
        Assert.Equal(3, counter.Count);
        client.Verify(c => c.ReceiveNotification(It.IsAny<NotificationDto>()), Times.Exactly(50));
        // The processor sends each message but coalesces the unread count per recipient.
        client.Verify(c => c.UnreadCountUpdated(50), Times.Once);
        await using var verify = database.OpenContext();
        Assert.Equal(50, await verify.NotificationOutbox.CountAsync(o => o.Status == OutboxStatus.Sent));
    }
}
