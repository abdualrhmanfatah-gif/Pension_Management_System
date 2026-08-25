namespace PensionMS.Core.Entities;

/// <summary>
/// إشعارات المتقاعدين
/// </summary>
public class CitizenNotification
{
    [System.ComponentModel.DataAnnotations.Key]
    public int NotificationID { get; set; }
    
    public int RetireeID { get; set; }
    public virtual Retiree Retiree { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
