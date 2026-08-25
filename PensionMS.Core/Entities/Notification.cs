namespace PensionMS.Core.Entities;

/// <summary>
/// الإشعارات
/// </summary>
public class Notification
{
    public long NotificationID { get; set; }

    public int UserID { get; set; }
    public virtual User User { get; set; } = null!;

    public int? RelatedTableID { get; set; }
    public virtual SystemTable? RelatedTable { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Category { get; set; } // SECURITY, FINANCIAL, WORKFLOW, SYSTEM
    public byte Priority { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionType { get; set; }
    public string? Payload { get; set; } // JSON data for dynamic content
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public int? RelatedRecordID { get; set; }

    public int? ActorID { get; set; } // The user who triggered the notification
    public virtual User? Actor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public byte[]? RowVersion { get; set; }

    // Attachments
    public virtual ICollection<NotificationAttachment> Attachments { get; set; } = new List<NotificationAttachment>();
}
