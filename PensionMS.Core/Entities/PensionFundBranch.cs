namespace PensionMS.Core.Entities;

/// <summary>
/// فروع صندوق التقاعد — يدعم هيكل شجري (إقليم ← فرع)
/// </summary>
public class PensionFundBranch
{
    public int PensionBranchID { get; set; }

    /// <summary>
    /// الفرع الأب (للهيكل الشجري) — null يعني فرع جذري أو إقليم
    /// </summary>
    public int? ParentBranchId { get; set; }
    public virtual PensionFundBranch? ParentBranch { get; set; }

    public string BranchName { get; set; } = string.Empty;
    public string? Governorate { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PensionFundBranch> ChildBranches { get; set; } = new List<PensionFundBranch>();
    public virtual ICollection<RetirementFile> RetirementFiles { get; set; } = new List<RetirementFile>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
}

