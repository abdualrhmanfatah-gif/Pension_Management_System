namespace PensionMS.Core.Entities;

/// <summary>
/// جداول النظام
/// </summary>
public class SystemTable
{
    public int TableID { get; set; }

    public string TableNameEn { get; set; } = string.Empty;
    public string TableNameAr { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<TablePermission> TablePermissions { get; set; } = new List<TablePermission>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<SyncQueue> SyncQueues { get; set; } = new List<SyncQueue>();
    public virtual ICollection<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
    public virtual ICollection<StatusTransitionLog> StatusTransitionLogs { get; set; } = new List<StatusTransitionLog>();
}
