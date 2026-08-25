namespace PensionMS.Core.DTOs.Notifications;

public class NotificationAttachmentDto
{
    public int AttachmentID { get; set; }
    public long NotificationID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class NotificationDto
{
    public long NotificationID { get; set; }
    public int UserID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Category { get; set; }
    public byte Priority { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionType { get; set; }
    public string? Payload { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int? ActorID { get; set; }
    public string? ActorName { get; set; }
    
    public int? RelatedTableID { get; set; }
    public int? RelatedRecordID { get; set; }
    
    public List<NotificationAttachmentDto> Attachments { get; set; } = new();
}

public class CreateNotificationDto
{
    public int UserID { get; set; }
    public List<int>? UserIDs { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Category { get; set; }
    public byte Priority { get; set; } = 1;
    public string? ActionUrl { get; set; }
    public string? ActionType { get; set; }
    public string? Payload { get; set; }
    
    public int? ActorID { get; set; }
    public int? RelatedTableID { get; set; }
    public int? RelatedRecordID { get; set; }
}
