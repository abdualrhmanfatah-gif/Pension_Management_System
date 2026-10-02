using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;

namespace PensionMS.Core.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<NotificationDto>> SendNotificationAsync(CreateNotificationDto dto);
    Task<ApiResponse<List<NotificationDto>>> SendNotificationToMultipleAsync(
        string title, string message, 
        List<int> userIds, 
        string? category = null, 
        byte priority = 1,
        string? actionUrl = null,
        int? actorId = null);
    Task<ApiResponse<List<NotificationDto>>> SendNotificationWithAttachmentsAsync(
        string title, string message, 
        List<int> userIds, 
        List<NotificationAttachmentUpload>? attachments,
        string? category = null, 
        byte priority = 1,
        string? actionUrl = null,
        int? actorId = null);
    Task<ApiResponse<bool>> SendBatchNotificationAsync(int userId, string title, string message,
        string? category, byte priority, int count, string? actionUrl);
    Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20, string? category = null);
    Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsPagedAsync(int userId, int page = 1, int pageSize = 20, long? cursor = null);
    Task<ApiResponse<int>> GetUnreadCountAsync(int userId);
    Task<ApiResponse<bool>> MarkAsReadAsync(long notificationId, int userId);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId);
    Task PushUnreadCountAsync(int userId, int count);

    // إشعارات مجموعات (Broadcasting)
    Task<ApiResponse<bool>> NotifyRoleAsync(string roleName, CreateNotificationDto dto);
    Task<ApiResponse<bool>> NotifyBranchAsync(int branchId, CreateNotificationDto dto);
}

public class NotificationAttachmentUpload
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Stream Stream { get; set; } = null!;
    public long Length { get; set; }
}
