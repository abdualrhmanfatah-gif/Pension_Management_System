namespace PensionMS.Core.DTOs.Users;

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public int? RoleID { get; set; }
    public int? BranchID { get; set; }
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public int? RoleID { get; set; }
    public int? BranchID { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLocked { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class UserDto
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? RoleID { get; set; }
    public string? RoleName { get; set; }
    public int? BranchID { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>أكواد الصلاحيات الفعلية (من UserRoleAssignments → RolePermissions)</summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>تعيينات الأدوار النشطة</summary>
    public List<UserAssignmentDto> Assignments { get; set; } = new();
}

/// <summary>تعيين دور لمستخدم مع نطاق</summary>
public class UserAssignmentDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>تفاصيل الدور مع صلاحياته</summary>
public class RoleDetailDto
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}

/// <summary>صلاحية واحدة</summary>
public class PermissionDto
{
    public int PermissionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
}

/// <summary>تعيين صلاحيات لدور — قائمة أرقام الصلاحيات</summary>
public class UpdateRolePermissionsDto
{
    public List<int> PermissionIds { get; set; } = new();
}
