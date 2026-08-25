using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// المستخدمون
/// </summary>
public class User : ISoftDeletable
{
    public int UserID { get; set; }

    public int? RoleID { get; set; }
    public virtual Role? Role { get; set; }

    public int? BranchID { get; set; }
    public virtual PensionFundBranch? Branch { get; set; }

    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public int FailedLoginAttempts { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public bool MustChangePassword { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }

    public virtual ICollection<User> CreatedUsers { get; set; } = new List<User>();
    public virtual ICollection<User> DeletedUsers { get; set; } = new List<User>();
    public virtual ICollection<DeviceRegistration> DeviceRegistrations { get; set; } = new List<DeviceRegistration>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<Notification> TriggeredNotifications { get; set; } = new List<Notification>();
    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
    public virtual ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
