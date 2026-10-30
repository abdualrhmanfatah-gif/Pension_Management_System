using PensionMS.Core.DTOs.Notifications;

namespace PensionMS.Infrastructure.Services;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationDto notification);
    Task UnreadCountUpdated(int count);
}
