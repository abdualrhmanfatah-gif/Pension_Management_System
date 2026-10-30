using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class AdminNotificationService : IAdminNotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly IFileStorageService _fileStorage;

    public AdminNotificationService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IHubContext<NotificationHub, INotificationClient> hubContext,
        IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _hubContext = hubContext;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetAllNotificationsAsync(AdminNotificationFilterDto filter)
    {
        var query = _unitOfWork.Notifications.Query()
            .AsNoTracking()
            .Include(n => n.Actor)
            .Include(n => n.User)
            .Include(n => n.Attachments)
            .AsQueryable();

        if (filter.UserID.HasValue)
            query = query.Where(n => n.UserID == filter.UserID.Value);

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(n => n.Category == filter.Category);

        if (filter.IsRead.HasValue)
            query = query.Where(n => n.IsRead == filter.IsRead.Value);

        if (filter.From.HasValue)
            query = query.Where(n => n.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(n => n.CreatedAt <= filter.To.Value);

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(n => n.Title.Contains(filter.Search) || n.Message.Contains(filter.Search));

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<NotificationDto>
        {
            Items = notifications.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PagedResult<NotificationDto>>.Success(result);
    }

    public async Task<ApiResponse<AdminNotificationStatsDto>> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;

        var totalNotifications = await _unitOfWork.Notifications.CountAsync();
        var unreadCount = await _unitOfWork.Notifications.CountAsync(n => !n.IsRead);
        var todayCount = await _unitOfWork.Notifications.CountAsync(n => n.CreatedAt >= todayStart);

        var byCategory = await _unitOfWork.Notifications.Query()
            .GroupBy(n => n.Category ?? "UNKNOWN")
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Category, g => g.Count);

        var failedCount = await _unitOfWork.NotificationOutbox
            .CountAsync(o => o.Status == OutboxStatus.Failed);
        var pendingCount = await _unitOfWork.NotificationOutbox
            .CountAsync(o => o.Status == OutboxStatus.Pending);

        var stats = new AdminNotificationStatsDto
        {
            TotalNotifications = totalNotifications,
            UnreadCount = unreadCount,
            TodayCount = todayCount,
            FailedOutboxCount = failedCount,
            PendingOutboxCount = pendingCount,
            ByCategory = byCategory
        };

        return ApiResponse<AdminNotificationStatsDto>.Success(stats);
    }

    public async Task<ApiResponse<NotificationDto>> SendNotificationAsync(AdminSendNotificationDto dto, int actorId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserID);
        if (user == null)
            return ApiResponse<NotificationDto>.Fail("المستخدم غير موجود");

        var createDto = new CreateNotificationDto
        {
            UserID = dto.UserID,
            UserIDs = dto.UserIDs,
            Title = dto.Title,
            Message = dto.Message,
            Category = dto.Category,
            Priority = dto.Priority,
            ActionUrl = dto.ActionUrl,
            ActionType = dto.ActionType,
            Payload = dto.Payload,
            RelatedRecordID = dto.RelatedRecordID,
            ActorID = actorId
        };

        if (dto.UserIDs != null && dto.UserIDs.Any())
        {
            var result = await _notificationService.SendNotificationToMultipleAsync(
                dto.Title, dto.Message, dto.UserIDs, dto.Category, dto.Priority, dto.ActionUrl, actorId);
            if (result.IsSuccess && result.Data?.Any() == true)
                return ApiResponse<NotificationDto>.Success(result.Data.First());
            return ApiResponse<NotificationDto>.Fail(result.Message ?? "فشل في الإرسال");
        }

        return await _notificationService.SendNotificationAsync(createDto);
    }

    public async Task<ApiResponse<int>> BroadcastNotificationAsync(AdminBroadcastNotificationDto dto, int actorId)
    {
        if (dto.UserIDs != null && dto.UserIDs.Any())
        {
            var result = await _notificationService.SendNotificationToMultipleAsync(
                dto.Title, dto.Message, dto.UserIDs, dto.Category, dto.Priority, dto.ActionUrl, actorId);
            return result.IsSuccess 
                ? ApiResponse<int>.Success(result.Data?.Count ?? 0) 
                : ApiResponse<int>.Fail(result.Message ?? "فشل في الإرسال");
        }

        var createDto = new CreateNotificationDto
        {
            Title = dto.Title,
            Message = dto.Message,
            Category = dto.Category,
            Priority = dto.Priority,
            ActionUrl = dto.ActionUrl,
            ActorID = actorId
        };

        if (!string.IsNullOrEmpty(dto.RoleName))
        {
            await _notificationService.NotifyRoleAsync(dto.RoleName, createDto);
            var count = await _unitOfWork.Users.CountAsync(u => u.Role.RoleName == dto.RoleName && u.IsActive);
            return ApiResponse<int>.Success(count);
        }

        if (dto.BranchID.HasValue)
        {
            await _notificationService.NotifyBranchAsync(dto.BranchID.Value, createDto);
            var count = await _unitOfWork.Users.CountAsync(u => u.BranchID == dto.BranchID && u.IsActive);
            return ApiResponse<int>.Success(count);
        }

        return ApiResponse<int>.Fail("يجب تحديد مستخدمين، دور، أو فرع للإرسال");
    }

    public async Task<ApiResponse<List<NotificationDto>>> SendNotificationWithAttachmentsAsync(
        string title, string message, 
        List<int> userIds, 
        List<NotificationAttachmentUpload>? attachments,
        string? category = null, 
        byte priority = 1,
        string? actionUrl = null,
        int? actorId = null)
    {
        if (userIds == null || !userIds.Any())
            return ApiResponse<List<NotificationDto>>.Fail("يجب تحديد مستخدم واحد على الأقل");

        return await _notificationService.SendNotificationWithAttachmentsAsync(
            title, message, userIds, attachments, category, priority, actionUrl, actorId);
    }

    public async Task<ApiResponse<PagedResult<NotificationOutboxDto>>> GetOutboxAsync(int page = 1, int pageSize = 20, string? status = null)
    {
        var query = _unitOfWork.NotificationOutbox.Query()
            .AsNoTracking()
            .Include(o => o.Notification)
                .ThenInclude(n => n.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OutboxStatus>(status, true, out var statusEnum))
            query = query.Where(o => o.Status == statusEnum);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<NotificationOutboxDto>
        {
            Items = items.Select(o => new NotificationOutboxDto
            {
                OutboxId = o.OutboxId,
                NotificationId = o.NotificationId,
                Title = o.Notification?.Title ?? "",
                UserName = o.Notification?.User?.FullName ?? "",
                Status = o.Status.ToString(),
                RetryCount = o.RetryCount,
                MaxRetries = o.MaxRetries,
                CreatedAt = o.CreatedAt,
                LastAttemptAt = o.LastAttemptAt,
                ErrorMessage = o.ErrorMessage
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<NotificationOutboxDto>>.Success(result);
    }

    public async Task<ApiResponse<bool>> RetryOutboxAsync(long outboxId)
    {
        var outbox = await _unitOfWork.NotificationOutbox.Query()
            .FirstOrDefaultAsync(o => o.OutboxId == outboxId);
        if (outbox == null)
            return ApiResponse<bool>.Fail("عنصر الـ Outbox غير موجود");

        outbox.Status = OutboxStatus.Pending;
        outbox.RetryCount = 0;
        outbox.ErrorMessage = null;
        _unitOfWork.NotificationOutbox.Update(outbox);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> DeleteNotificationAsync(long notificationId)
    {
        var notification = await _unitOfWork.Notifications.Query()
            .FirstOrDefaultAsync(n => n.NotificationID == notificationId);
        if (notification == null)
            return ApiResponse<bool>.Fail("الإشعار غير موجود");

        _unitOfWork.Notifications.Delete(notification);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<int>> BulkDeleteAsync(BulkDeleteDto dto)
    {
        var query = _unitOfWork.Notifications.Query().AsQueryable();

        if (dto.Ids?.Count > 0)
            query = query.Where(n => dto.Ids.Contains(n.NotificationID));

        if (dto.DeleteAll)
            query = query.Where(n => true);

        if (!string.IsNullOrEmpty(dto.Category))
            query = query.Where(n => n.Category == dto.Category);

        if (dto.OnlyRead.HasValue && dto.OnlyRead.Value)
            query = query.Where(n => n.IsRead);

        if (dto.OlderThan.HasValue)
            query = query.Where(n => n.CreatedAt < dto.OlderThan.Value);

        var notifications = await query.ToListAsync();
        if (notifications.Count == 0)
            return ApiResponse<int>.Success(0);

        foreach (var n in notifications)
            _unitOfWork.Notifications.Delete(n);

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<int>.Success(notifications.Count);
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
