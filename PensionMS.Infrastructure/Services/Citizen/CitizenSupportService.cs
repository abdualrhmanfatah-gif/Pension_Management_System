using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Citizen;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Citizen;

public class CitizenSupportService : ICitizenSupportService
{
    private readonly IUnitOfWork _unitOfWork;

    public CitizenSupportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SupportTicketDto>> GetTicketsAsync(int citizenAccountId)
    {
        var tickets = await _unitOfWork.SupportTickets.Query()
            .Where(t => t.CitizenAccountID == citizenAccountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(t => new SupportTicketDto
        {
            TicketID = t.SupportTicketID,
            TicketNumber = t.TicketNumber,
            Subject = t.Subject,
            Category = t.Category,
            Status = t.Status,
            Priority = t.Priority,
            CreatedAt = t.CreatedAt
        });
    }

    public async Task<bool> CreateTicketAsync(int citizenAccountId, CreateSupportTicketDto dto)
    {
        var ticket = new SupportTicket
        {
            CitizenAccountID = citizenAccountId,
            Subject = dto.Subject,
            Category = dto.Category,
            Status = "مفتوح",
            Priority = "عادي",
            TicketNumber = $"T-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..5].ToUpper()}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SupportTickets.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        var message = new TicketMessage
        {
            TicketID = ticket.SupportTicketID,
            Content = dto.Message,
            SenderType = "CITIZEN",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TicketMessages.AddAsync(message);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<TicketMessageDto>> GetTicketMessagesAsync(int ticketId, int citizenAccountId)
    {
        var ticket = await _unitOfWork.SupportTickets.FirstOrDefaultAsync(t => t.SupportTicketID == ticketId && t.CitizenAccountID == citizenAccountId);
        if (ticket == null) return Enumerable.Empty<TicketMessageDto>();

        var messages = await _unitOfWork.TicketMessages.Query()
            .Where(m => m.TicketID == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => new TicketMessageDto
        {
            MessageID = m.TicketMessageID,
            Content = m.Content,
            SenderType = m.SenderType,
            CreatedAt = m.CreatedAt
        });
    }

    public async Task<bool> AddMessageToTicketAsync(int ticketId, int citizenAccountId, string content)
    {
        var ticket = await _unitOfWork.SupportTickets.FirstOrDefaultAsync(t => t.SupportTicketID == ticketId && t.CitizenAccountID == citizenAccountId);
        if (ticket == null) return false;

        var message = new TicketMessage
        {
            TicketID = ticketId,
            Content = content,
            SenderType = "CITIZEN",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TicketMessages.AddAsync(message);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<CitizenNotificationDto>> GetNotificationsAsync(int retireeId)
    {
        var notifications = await _unitOfWork.CitizenNotifications.Query()
            .Where(n => n.RetireeID == retireeId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new CitizenNotificationDto
        {
            NotificationID = n.NotificationID,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int retireeId)
    {
        var notification = await _unitOfWork.CitizenNotifications.FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.RetireeID == retireeId);
        if (notification == null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        _unitOfWork.CitizenNotifications.Update(notification);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<int> GetUnreadNotificationsCountAsync(int retireeId)
    {
        return await _unitOfWork.CitizenNotifications.CountAsync(n => n.RetireeID == retireeId && !n.IsRead);
    }
}
