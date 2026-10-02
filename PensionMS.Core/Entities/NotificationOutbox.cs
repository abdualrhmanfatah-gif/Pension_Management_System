namespace PensionMS.Core.Entities;

public enum OutboxChannel : byte
{
    SignalR = 0
}

public enum OutboxStatus : byte
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}

public class NotificationOutbox
{
    public long OutboxId { get; set; }
    public long NotificationId { get; set; }
    public virtual Notification Notification { get; set; } = null!;

    public OutboxChannel Channel { get; set; } = OutboxChannel.SignalR;
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
    public byte RetryCount { get; set; }
    public byte MaxRetries { get; set; } = 3;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAttemptAt { get; set; }
    public string? ErrorMessage { get; set; }
}
