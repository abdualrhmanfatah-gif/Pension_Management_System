namespace PensionMS.Core.Entities;

/// <summary>
/// تعيين دور لمستخدم مع نطاق فرع اختياري
/// يحل محل User.RoleID الواحد — يدعم أدوار متعددة ونطاقات مختلفة
/// </summary>
public class UserRoleAssignment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// نطاق الفرع — null يعني صلاحية مركزية (كل الفروع)
    /// </summary>
    public int? BranchId { get; set; }
    public virtual PensionFundBranch? Branch { get; set; }

    /// <summary>
    /// نطاق إقليمي — null يعني لا إقليمي
    /// يستخدم للأدوار التي تشرف على عدة فروع
    /// </summary>
    public int? RegionId { get; set; }
    public virtual PensionFundBranch? Region { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
