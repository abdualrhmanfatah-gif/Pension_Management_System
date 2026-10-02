namespace PensionMS.Core.Entities;

/// <summary>
/// صلاحيات الجداول
/// </summary>
public class TablePermission
{
    public int PermissionID { get; set; }

    public int RoleID { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int TableID { get; set; }
    public virtual SystemTable SystemTable { get; set; } = null!;

    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
