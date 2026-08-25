using System.ComponentModel.DataAnnotations;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// مرفقات الإشعارات
/// </summary>
public class NotificationAttachment : ISoftDeletable
{
    [Key]
    public int AttachmentID { get; set; }

    public long NotificationID { get; set; }
    public virtual Notification Notification { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
}