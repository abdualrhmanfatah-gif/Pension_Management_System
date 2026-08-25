namespace PensionMS.Core.Entities;

/// <summary>
/// قائمة انتظار المزامنة
/// </summary>
public class SyncQueue
{
    public long SyncID { get; set; }

    public string DeviceID { get; set; } = string.Empty;
    public virtual DeviceRegistration DeviceRegistration { get; set; } = null!;

    public int UserID { get; set; }
    public virtual User User { get; set; } = null!;

    public int TableID { get; set; }
    public virtual SystemTable SystemTable { get; set; } = null!;

    public int RecordID { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime LocalTimestamp { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Status { get; set; } = string.Empty;
    public DateTime? SyncedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
