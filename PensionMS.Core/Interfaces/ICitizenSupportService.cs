using PensionMS.Core.DTOs.Citizen;

namespace PensionMS.Core.Interfaces;

public interface ICitizenSupportService
{
    // Support
    Task<IEnumerable<SupportTicketDto>> GetTicketsAsync(int citizenAccountId);
    Task<bool> CreateTicketAsync(int citizenAccountId, CreateSupportTicketDto dto);
    Task<IEnumerable<TicketMessageDto>> GetTicketMessagesAsync(int ticketId, int citizenAccountId);
    Task<bool> AddMessageToTicketAsync(int ticketId, int citizenAccountId, string content);

    // Notifications
    Task<IEnumerable<CitizenNotificationDto>> GetNotificationsAsync(int retireeId);
    Task<bool> MarkNotificationAsReadAsync(int notificationId, int retireeId);
    Task<int> GetUnreadNotificationsCountAsync(int retireeId);
}
