namespace PensionMS.Core.Entities;

/// <summary>
/// جدول وسيط: ربط Many-to-Many بين الأدوار والصلاحيات
/// </summary>
public class RolePermission
{
    public int RolePermissionId { get; set; }

    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int PermissionId { get; set; }
    public virtual Permission Permission { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
