using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Users;

namespace PensionMS.Core.Interfaces;

public interface IUserService
{
    Task<bool> IsActiveAsync(int userId);
    Task<ApiResponse<PagedResult<UserDto>>> GetAllUsersAsync(int page = 1, int pageSize = 20);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto, int createdByUserId);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto, int updatedByUserId);
    Task<bool> DeleteUserAsync(int id, int deletedByUserId);
    Task<bool> ResetPasswordAsync(int id, string newPassword, int resetByUserId);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, int changedByUserId);
    
    // New methods for permissions and debugging
    Task<ApiResponse<object>> GetDebugPermsAsync(int userId);
    Task<ApiResponse<object>> GetUserPermissionsAsync(int id);
    Task<ApiResponse<object>> SetUserPermissionsAsync(int id, SetUserPermissionsDto dto, int currentUserId);
    Task<ApiResponse<object>> SetRoleAssignmentsAsync(int id, SetUserRoleAssignmentsDto dto, int currentUserId);
}
