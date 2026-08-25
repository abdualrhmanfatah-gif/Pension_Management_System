namespace PensionMS.Core.Entities;

/// <summary>
/// صلاحية دقيقة (مثل File.Create, File.ChangeStatus.Approve)
/// </summary>
public class Permission
{
    public int PermissionId { get; set; }

    /// <summary>
    /// رمز الصلاحية الفريد — مثل "File.Create", "File.ChangeStatus.Approve"
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// وصف الصلاحية بالعربية
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// فئة الصلاحية للتجميع — مثل "File", "Financial", "Export", "System"
    /// </summary>
    public string? Category { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
