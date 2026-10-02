namespace PensionMS.Core.Entities;

/// <summary>
/// صلاحية فردية مخصصة لمستخدم — مستقلة عن صلاحيات الدور
/// </summary>
public class UserPermission
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int PermissionId { get; set; }
    public virtual Permission Permission { get; set; } = null!;

    /// <summary>منح (true) أو سحب (false) — override على صلاحيات الدور</summary>
    public bool IsGranted { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? GrantedByUserId { get; set; }
    public virtual User? GrantedByUser { get; set; }
}
