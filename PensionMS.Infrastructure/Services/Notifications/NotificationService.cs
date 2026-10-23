using PensionMS.Core.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly IFileStorageService _fileStorage;
    private readonly IScopedPermissionResolver _permissions;

    public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub, INotificationClient> hubContext, IFileStorageService fileStorage,
        IScopedPermissionResolver permissions)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _fileStorage = fileStorage;
        _permissions = permissions;
    }

    public async Task<ApiResponse<NotificationDto>> SendNotificationAsync(CreateNotificationDto dto)
    {
        if (!await CanReceiveResourceAsync(dto, dto.UserID))
            return ApiResponse<NotificationDto>.Fail("المستلم لا يملك صلاحية عرض البيانات المرتبطة بالإشعار");
        var notification = await StageNotificationAsync(dto, dto.UserID);
        await _unitOfWork.SaveChangesAsync();
        if (notification.ActorID > 0)
            notification.Actor = await _unitOfWork.Users.GetByIdAsync(notification.ActorID.Value);
        return ApiResponse<NotificationDto>.Success(MapToDto(notification));
    }

    public async Task EnqueueRoleAsync(string roleName, CreateNotificationDto dto)
    {
        var userIds = await _unitOfWork.Users.Query()
            .Where(u => u.Role.RoleName == roleName && u.IsActive)
            .Select(u => u.UserID).ToListAsync();
        foreach (var userId in userIds)
            if (await CanReceiveResourceAsync(dto, userId)) await StageNotificationAsync(dto, userId);
    }

    public async Task EnqueueAsync(CreateNotificationDto dto)
    {
        if (await CanReceiveResourceAsync(dto, dto.UserID)) await StageNotificationAsync(dto, dto.UserID);
    }

    private async Task<bool> CanReceiveResourceAsync(CreateNotificationDto dto, int userId)
    {
        if (dto.RelatedTableID == SystemTableConstants.RetirementFiles && dto.RelatedRecordID.HasValue)
        {
            var scope = await _permissions.GetAllowedScopeAsync(userId, "File.View.Sensitive");
            if (scope is null || scope.IsDenied) return false;
            var branch = await _unitOfWork.RetirementFiles.Query().AsNoTracking()
                .Where(f => f.FileID == dto.RelatedRecordID.Value && !f.IsDeleted)
                .Select(f => (int?)f.PensionBranchID).FirstOrDefaultAsync();
            return branch.HasValue && scope.Allows(branch.Value);
        }
        if (dto.RelatedTableID == SystemTableConstants.SalaryPaymentBatches && dto.RelatedRecordID.HasValue)
        {
            var scope = await _permissions.GetAllowedScopeAsync(userId, "SalaryBatch.View");
            if (scope is null || scope.IsDenied) return false;
            if (scope.AllBranches) return true;
            var branches = scope.BranchIds.ToArray();
            return await _unitOfWork.SalaryPaymentItems.Query().AnyAsync(i => i.BatchID == dto.RelatedRecordID.Value)
                && !await _unitOfWork.SalaryPaymentItems.Query().AnyAsync(i => i.BatchID == dto.RelatedRecordID.Value &&
                    !branches.Contains(i.RetirementFile.PensionBranchID));
        }
        return true;
    }

    private async Task<Notification> StageNotificationAsync(CreateNotificationDto dto, int userId)
    {
        var notification = new Notification
        {
            UserID = userId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Category = dto.Category,
            Priority = dto.Priority,
            ActionUrl = dto.ActionUrl,
            ActionType = dto.ActionType,
            Payload = dto.Payload,
            ActorID = dto.ActorID,
            RelatedTableID = dto.RelatedTableID,
            RelatedRecordID = dto.RelatedRecordID,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        var outbox = new NotificationOutbox
        {
            Notification = notification,
            Channel = OutboxChannel.SignalR,
            Status = OutboxStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.NotificationOutbox.AddAsync(outbox);
        return notification;
    }

    public async Task<ApiResponse<List<NotificationDto>>> SendNotificationToMultipleAsync(
        string title, string message, 
        List<int> userIds, 
        string? category = null, 
        byte priority = 1,
        string? actionUrl = null,
        int? actorId = null)
    {
        if (userIds == null || !userIds.Any())
            return ApiResponse<List<NotificationDto>>.Fail("يجب تحديد مستخدم واحد على الأقل");

        var notifications = new List<Notification>();
        foreach (var userId in userIds.Distinct())
            notifications.Add(await StageNotificationAsync(new CreateNotificationDto
            {
                Title = title, Message = message, Category = category, Priority = priority,
                ActionUrl = actionUrl, ActorID = actorId
            }, userId));
        await _unitOfWork.SaveChangesAsync();
        var actor = actorId > 0 ? await _unitOfWork.Users.GetByIdAsync(actorId.Value) : null;
        foreach (var notification in notifications) notification.Actor = actor;
        return ApiResponse<List<NotificationDto>>.Success(notifications.Select(MapToDto).ToList());
    }

    public Task<ApiResponse<List<NotificationDto>>> SendNotificationWithAttachmentsAsync(
        string title, string message, List<int> userIds, List<NotificationAttachmentUpload>? attachments,
        string? category = null, byte priority = 1, string? actionUrl = null, int? actorId = null) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => SendNotificationWithAttachmentsCoreAsync(
            title, message, userIds, attachments, category, priority, actionUrl, actorId));

    private async Task<ApiResponse<List<NotificationDto>>> SendNotificationWithAttachmentsCoreAsync(
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

        var results = new List<NotificationDto>();

        foreach (var userId in userIds.Distinct())
        {
            var notification = new Notification
            {
                UserID = userId,
                Title = title,
                Message = message,
                Category = category,
                Priority = priority,
                ActionUrl = actionUrl,
                ActorID = actorId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Handle attachments
            if (attachments != null && attachments.Any())
            {
                foreach (var file in attachments)
                {
                    if (file.Length > 0 && file.Stream != null)
                    {
                        var filePath = await _fileStorage.SaveFileAsync(file.Stream, file.FileName, "notifications");
                        
                        var attachment = new NotificationAttachment
                        {
                            NotificationID = notification.NotificationID,
                            FileName = file.FileName,
                            FilePath = filePath,
                            MimeType = file.ContentType,
                            FileSize = file.Length,
                            UploadedAt = DateTime.UtcNow
                        };
                        notification.Attachments.Add(attachment);
                        await _unitOfWork.NotificationAttachments.AddAsync(attachment);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }

            if (notification.ActorID > 0)
                notification.Actor = await _unitOfWork.Users.GetByIdAsync(notification.ActorID.Value);

            var result = MapToDto(notification);

            // Save outbox entry
            var outbox = new NotificationOutbox
            {
                NotificationId = notification.NotificationID,
                Channel = OutboxChannel.SignalR,
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.NotificationOutbox.AddAsync(outbox);
            await _unitOfWork.SaveChangesAsync();

            results.Add(result);
        }

        return ApiResponse<List<NotificationDto>>.Success(results);
    }

    public async Task<ApiResponse<bool>> SendBatchNotificationAsync(int userId, string title, string message,
        string? category, byte priority, int count, string? actionUrl)
    {
        var existing = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserID == userId && n.Category == category && n.Title == title
                        && !n.IsRead && n.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Message = message;
            existing.Priority = priority;


            await _unitOfWork.NotificationOutbox.AddAsync(new NotificationOutbox { Notification = existing });
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        var createDto = new CreateNotificationDto
        {
            UserID = userId,
            Title = title,
            Message = message,
            Category = category,
            Priority = priority,
            ActionUrl = actionUrl
        };
        await SendNotificationAsync(createDto);
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20, string? category = null)
    {
        var query = _unitOfWork.Notifications.Query()
            .AsNoTracking()
            .Include(n => n.Actor)
            .Include(n => n.Attachments)
            .Where(n => n.UserID == userId);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(n => n.Category == category);

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<NotificationDto>
        {
            Items = notifications.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<NotificationDto>>.Success(result);
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsPagedAsync(int userId, int page = 1, int pageSize = 20, long? cursor = null)
    {
        var query = _unitOfWork.Notifications.Query()
            .AsNoTracking()
            .Include(n => n.Actor)
            .Include(n => n.Attachments)
            .Where(n => n.UserID == userId);

        if (cursor.HasValue)
        {
            var items = await query
                .Where(n => n.NotificationID < cursor.Value)
                .OrderByDescending(n => n.NotificationID)
                .Take(pageSize + 1)
                .ToListAsync();

            bool hasMore = items.Count > pageSize;
            if (hasMore) items = items.Take(pageSize).ToList();

            return ApiResponse<PagedResult<NotificationDto>>.Success(new PagedResult<NotificationDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize,
                NextCursor = items.Count > 0 ? (int)items.Last().NotificationID : null,
                HasMore = hasMore
            });
        }

        var totalCount = await query.CountAsync();
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<NotificationDto>
        {
            Items = notifications.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<NotificationDto>>.Success(result);
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(int userId)
    {
        var count = await _unitOfWork.Notifications.CountAsync(n => n.UserID == userId && !n.IsRead);
        return ApiResponse<int>.Success(count);
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(long notificationId, int userId)
    {
        var notification = await _unitOfWork.Notifications.Query()
            .FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.UserID == userId);

        if (notification == null) return ApiResponse<bool>.Fail("الإشعار غير موجود");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        var unreadCount = await _unitOfWork.Notifications.CountAsync(n => n.UserID == userId && !n.IsRead);
        await PushUnreadCountAsync(userId, unreadCount);

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId)
    {
        var unread = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserID == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            _unitOfWork.Notifications.Update(n);
        }

        await _unitOfWork.SaveChangesAsync();
        await PushUnreadCountAsync(userId, 0);

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> NotifyRoleAsync(string roleName, CreateNotificationDto dto)
    {
        await EnqueueRoleAsync(roleName, dto);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> NotifyBranchAsync(int branchId, CreateNotificationDto dto)
    {
        var users = await _unitOfWork.Users.Query()
            .Where(u => u.BranchID == branchId && u.IsActive)
            .Select(u => u.UserID)
            .ToListAsync();

        foreach (var userId in users)
            if (await CanReceiveResourceAsync(dto, userId)) await StageNotificationAsync(dto, userId);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task PushUnreadCountAsync(int userId, int count)
    {
        if (_unitOfWork.HasActiveTransaction) return;
        try
        {
            await _hubContext.Clients.Group($"User_{userId}").UnreadCountUpdated(count);
        }
        catch
        {
            // Best effort only: the frontend recovers the count through its authoritative query.
        }
    }

    public async Task<ApiResponse<NotificationAttachmentDownloadDto>> DownloadAttachmentAsync(
        long notificationId, int attachmentId, int userId, bool isAdmin = false)
    {
        // فرض تطابق معرّفي الإشعار والمرفق — لا يُفتح مرفق عبر إشعار آخر
        var attachment = await _unitOfWork.NotificationAttachments.Query()
            .Include(a => a.Notification)
            .FirstOrDefaultAsync(a => a.AttachmentID == attachmentId
                && a.NotificationID == notificationId);

        if (attachment == null)
            return ApiResponse<NotificationAttachmentDownloadDto>.Fail("المرفق غير موجود");

        // المرفق المحذوف لا يُرسل — الفلتر العام يخفيه، والفحص الصريح احتياط
        if (attachment.IsDeleted)
            return ApiResponse<NotificationAttachmentDownloadDto>.Fail("المرفق غير متاح");

        // الملكية: المستلم فقط، أو System.Admin عبر السياسة الصريحة في الـController
        if (!isAdmin && attachment.Notification?.UserID != userId)
            return ApiResponse<NotificationAttachmentDownloadDto>.Fail("غير مصرح بتنزيل هذا المرفق");

        var stream = await _fileStorage.GetFileAsync(attachment.FilePath);
        if (stream == null)
            return ApiResponse<NotificationAttachmentDownloadDto>.Fail("ملف المرفق غير موجود على القرص");

        return ApiResponse<NotificationAttachmentDownloadDto>.Success(new NotificationAttachmentDownloadDto
        {
            Content = stream,
            ContentType = string.IsNullOrWhiteSpace(attachment.MimeType)
                ? "application/octet-stream"
                : attachment.MimeType,
            FileName = attachment.FileName
        });
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
