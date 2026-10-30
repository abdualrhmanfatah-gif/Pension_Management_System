using PensionMS.Core.DTOs.Users;

namespace PensionMS.Core.Interfaces;

public interface IPermissionService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleWithPermissionsAsync(int roleId);
    Task<bool> HasPermissionAsync(int roleId, string tableName, string operation);

    /// <summary>قائمة كل الصلاحيات الدقيقة (Permission entity)</summary>
    Task<List<PermissionDto>> GetAllPermissionsAsync();

    /// <summary>تحديث صلاحيات دور (Permission matrix)</summary>
    Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds);
}
