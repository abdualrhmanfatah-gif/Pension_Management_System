namespace PensionMS.Core.Entities;

/// <summary>
/// الأجهزة المسجلة
/// </summary>
public class DeviceRegistration
{
    public string DeviceID { get; set; } = string.Empty;

    public int UserID { get; set; }
    public virtual User User { get; set; } = null!;

    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public string? LastKnownIP { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SyncQueue> SyncQueues { get; set; } = new List<SyncQueue>();
}
