using PensionMS.Core.DTOs.Notifications;

namespace PensionMS.Core.Interfaces.Notifications;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationDto notification);
    Task UnreadCountUpdated(int count);
}
