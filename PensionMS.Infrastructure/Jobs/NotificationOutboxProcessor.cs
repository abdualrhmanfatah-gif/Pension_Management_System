using PensionMS.Infrastructure.Services.Notifications;
using PensionMS.Core.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Jobs;

public class NotificationOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationOutboxProcessor> _logger;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(30);

    public NotificationOutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationOutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingItemsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing notification outbox");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    internal async Task ProcessPendingItemsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub, INotificationClient>>();

        var pendingItems = await unitOfWork.NotificationOutbox.Query()
            .Where(o => o.Status == OutboxStatus.Pending && o.RetryCount < o.MaxRetries)
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.OutboxId)
            .Take(50)
            .ToListAsync(ct);

        if (pendingItems.Count == 0) return;

        var notificationIds = pendingItems.Select(o => o.NotificationId).ToList();
        var notifications = await unitOfWork.Notifications.Query()
            .AsNoTracking()
            .Where(n => notificationIds.Contains(n.NotificationID))
            .Include(n => n.Actor)
            .Include(n => n.Attachments)
            .ToDictionaryAsync(n => n.NotificationID, ct);
        var userIds = notifications.Values.Select(n => n.UserID).Distinct().ToList();
        var unreadCounts = await unitOfWork.Notifications.Query()
            .Where(n => userIds.Contains(n.UserID) && !n.IsRead)
            .GroupBy(n => n.UserID)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, ct);

        var deliveredUsers = new HashSet<int>();
        foreach (var item in pendingItems)
        {
            if (!notifications.TryGetValue(item.NotificationId, out var notification))
            {
                item.Status = OutboxStatus.Failed;
                item.ErrorMessage = "Notification not found";
                unitOfWork.NotificationOutbox.Update(item);
                continue;
            }

            var dto = MapToDto(notification);
            try
            {
                await hubContext.Clients
                    .Group($"User_{notification.UserID}")
                    .ReceiveNotification(dto);
                item.Status = OutboxStatus.Sent;
                item.LastAttemptAt = DateTime.UtcNow;
                item.ErrorMessage = null;
                deliveredUsers.Add(notification.UserID);
                _logger.LogDebug("Outbox item {OutboxId} sent successfully", item.OutboxId);
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.LastAttemptAt = DateTime.UtcNow;
                item.ErrorMessage = ex.Message;

                if (item.RetryCount >= item.MaxRetries)
                    item.Status = OutboxStatus.Failed;

                _logger.LogWarning(ex, "Failed to send outbox item {OutboxId} (attempt {RetryCount}/{MaxRetries})",
                    item.OutboxId, item.RetryCount, item.MaxRetries);
            }

            unitOfWork.NotificationOutbox.Update(item);
        }

        await unitOfWork.SaveChangesAsync();

        // Persist delivery before best-effort count pushes; a count failure must not resend the message.
        foreach (var userId in deliveredUsers)
        {
            try
            {
                await hubContext.Clients.Group($"User_{userId}")
                    .UnreadCountUpdated(unreadCounts.GetValueOrDefault(userId));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Unread count push failed for user {UserId}; delivery remains sent", userId);
            }
        }
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        NotificationID = n.NotificationID,
        UserID = n.UserID,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        Category = n.Category,
        Priority = n.Priority,
        ActionUrl = n.ActionUrl,
        ActionType = n.ActionType,
        Payload = n.Payload,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
        ActorID = n.ActorID,
        ActorName = n.Actor?.FullName,
        RelatedTableID = n.RelatedTableID,
        RelatedRecordID = n.RelatedRecordID,
        Attachments = n.Attachments?
            .Where(a => !a.IsDeleted)
            .Select(a => new NotificationAttachmentDto
            {
                AttachmentID = a.AttachmentID,
                NotificationID = a.NotificationID,
                FileName = a.FileName,
                FileUrl = $"/{a.FilePath}",
                MimeType = a.MimeType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt
            }).ToList() ?? new List<NotificationAttachmentDto>()
    };
}
