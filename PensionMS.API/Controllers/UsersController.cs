using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Users;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

<<<<<<< HEAD
    [HttpPut("{id:int}/role-assignments")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> SetRoleAssignments(int id, [FromBody] SetUserRoleAssignmentsDto dto)
        => (await _userService.SetRoleAssignmentsAsync(id, dto, this.GetUserId())).ToActionResult();

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    [HttpGet("debug-perms/{userId:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> DebugPerms(int userId)
    {
        var result = await _userService.GetDebugPermsAsync(userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// الحصول على جميع المستخدمين
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetAllUsersAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على مستخدم بالمعرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return ApiResponse<object>.Fail("المستخدم غير موجود").ToActionResult();
        return ApiResponse<object>.Success(user).ToActionResult();
    }

    /// <summary>
    /// إنشاء مستخدم جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var currentUserId = this.GetUserId();
        var result = await _userService.CreateUserAsync(dto, currentUserId);
        return result.ToActionResult();
    }

    /// <summary>
    /// تعديل مستخدم
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var currentUserId = this.GetUserId();
        var user = await _userService.UpdateUserAsync(id, dto, currentUserId);
        if (user == null)
            return ApiResponse<object>.Fail("المستخدم غير موجود").ToActionResult();
        return ApiResponse<UserDto>.Success(user).ToActionResult();
    }

    /// <summary>
    /// حذف مستخدم (حذف نهائي)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = this.GetUserId();
        var result = await _userService.DeleteUserAsync(id, currentUserId);
        if (!result)
            return ApiResponse<object>.Fail("المستخدم غير موجود").ToActionResult();
        return ApiResponse<object>.Success(null, "تم حذف المستخدم بنجاح").ToActionResult();
    }

    /// <summary>
    /// إعادة تعيين كلمة المرور
    /// </summary>
    [HttpPost("{id:int}/reset-password")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        var currentUserId = this.GetUserId();
        var result = await _userService.ResetPasswordAsync(id, request.NewPassword, currentUserId);
        if (!result)
            return ApiResponse<object>.Fail("المستخدم غير موجود").ToActionResult();
        return ApiResponse<object>.Success(null, "تم إعادة تعيين كلمة المرور بنجاح").ToActionResult();
    }

    // ═══════════════════════════════════════
    // USER-LEVEL PERMISSIONS
    // ═══════════════════════════════════════

    /// <summary>
    /// صلاحيات المستخدم الفردية
    /// </summary>
    [HttpGet("{id:int}/permissions")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> GetUserPermissions(int id)
    {
        var result = await _userService.GetUserPermissionsAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// تعيين صلاحيات فردية لمستخدم (استبدال كامل)
    /// </summary>
    [HttpPut("{id:int}/permissions")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> SetUserPermissions(int id, [FromBody] SetUserPermissionsDto dto)
    {
        var currentUserId = this.GetUserId();
        var result = await _userService.SetUserPermissionsAsync(id, dto, currentUserId);
        return result.ToActionResult();
    }
}


