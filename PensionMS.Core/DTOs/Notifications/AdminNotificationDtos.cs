namespace PensionMS.Core.DTOs.Notifications;

public class AdminNotificationStatsDto
{
    public int TotalNotifications { get; set; }
    public int UnreadCount { get; set; }
    public int TodayCount { get; set; }
    public int FailedOutboxCount { get; set; }
    public int PendingOutboxCount { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
}

public class AdminSendNotificationDto
{
    public int UserID { get; set; }
    public List<int>? UserIDs { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Category { get; set; }
    public byte Priority { get; set; } = 1;
    public string? ActionUrl { get; set; }
    public string? ActionType { get; set; }
    public string? Payload { get; set; }
    public int? RelatedRecordID { get; set; }
}

public class AdminBroadcastNotificationDto
{
    public string? RoleName { get; set; }
    public int? BranchID { get; set; }
    public List<int>? UserIDs { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Category { get; set; }
    public byte Priority { get; set; } = 1;
    public string? ActionUrl { get; set; }
}

public class NotificationOutboxDto
{
    public long OutboxId { get; set; }
    public long NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public byte RetryCount { get; set; }
    public byte MaxRetries { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AdminNotificationFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? UserID { get; set; }
    public string? Category { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
}

public class BulkDeleteDto
{
    public List<long>? Ids { get; set; }
    public bool DeleteAll { get; set; }
    public string? Category { get; set; }
    public bool? OnlyRead { get; set; }
    public DateTime? OlderThan { get; set; }
}
