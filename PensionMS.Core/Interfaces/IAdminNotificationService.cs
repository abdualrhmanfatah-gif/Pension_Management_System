using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;

namespace PensionMS.Core.Interfaces;

public interface IAdminNotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetAllNotificationsAsync(AdminNotificationFilterDto filter);
    Task<ApiResponse<AdminNotificationStatsDto>> GetStatsAsync();
    Task<ApiResponse<NotificationDto>> SendNotificationAsync(AdminSendNotificationDto dto, int actorId);
    Task<ApiResponse<int>> BroadcastNotificationAsync(AdminBroadcastNotificationDto dto, int actorId);
    Task<ApiResponse<List<NotificationDto>>> SendNotificationWithAttachmentsAsync(
        string title, string message,
        List<int> userIds,
        List<NotificationAttachmentUpload>? attachments,
        string? category = null,
        byte priority = 1,
        string? actionUrl = null,
        int? actorId = null);
    Task<ApiResponse<PagedResult<NotificationOutboxDto>>> GetOutboxAsync(int page = 1, int pageSize = 20, string? status = null);
    Task<ApiResponse<bool>> RetryOutboxAsync(long outboxId);
    Task<ApiResponse<bool>> DeleteNotificationAsync(long notificationId);
    Task<ApiResponse<int>> BulkDeleteAsync(BulkDeleteDto dto);
}
