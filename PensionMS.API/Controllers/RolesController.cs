using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Users;
using PensionMS.Core.Interfaces;

using PensionMS.API.Extensions;
namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public RolesController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// الحصول على جميع الأدوار
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _permissionService.GetAllRolesAsync();
        return ApiResponse<object>.Success(roles).ToActionResult();
    }

    /// <summary>
    /// الحصول على دور مع صلاحياته
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _permissionService.GetRoleWithPermissionsAsync(id);
        if (role == null)
            return ApiResponse<object>.Fail("الدور غير موجود").ToActionResult();
        return ApiResponse<object>.Success(role).ToActionResult();
    }

    // ═══════════════════════════════════════
    // RBAC PERMISSION MANAGEMENT
    // ═══════════════════════════════════════

    /// <summary>
    /// قائمة كل الصلاحيات الدقيقة (للـ Permission Matrix)
    /// </summary>
    [HttpGet("/api/permissions")]
    [Authorize(Policy = "System.ManageUsers")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        return ApiResponse<object>.Success(permissions).ToActionResult();
    }

    /// <summary>
    /// تعيين صلاحيات دور (استبدال كامل)
    /// </summary>
    [HttpPut("{id:int}/permissions")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateRolePermissionsDto dto)
    {
        var result = await _permissionService.UpdateRolePermissionsAsync(id, dto.PermissionIds);
        if (!result)
            return ApiResponse<object>.Fail("الدور غير موجود").ToActionResult();
        return ApiResponse<object>.Success(null, "تم تحديث صلاحيات الدور بنجاح").ToActionResult();
    }
}


