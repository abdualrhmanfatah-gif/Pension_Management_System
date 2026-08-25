namespace PensionMS.Core.Entities;

/// <summary>
/// الأدوار
/// </summary>
public class Role
{
    public int RoleID { get; set; }

    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<TablePermission> TablePermissions { get; set; } = new List<TablePermission>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
}
