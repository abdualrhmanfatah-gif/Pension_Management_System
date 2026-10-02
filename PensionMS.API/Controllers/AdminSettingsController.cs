using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Admin;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize]
public class AdminSettingsController : ControllerBase
{
    private readonly IAdminSettingsService _service;

    public AdminSettingsController(IAdminSettingsService service)
    {
        _service = service;
    }

    /// <summary>
    /// جلب جميع الإعدادات
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "System.Settings")]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var result = await _service.GetAllAsync(category);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب الفئات المتاحة
    /// </summary>
    [HttpGet("categories")]
    [Authorize(Policy = "System.Settings")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _service.GetCategoriesAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// تحديث قيمة إعداد
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "System.Settings")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSettingDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// إنشاء إعداد جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSettingDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف إعداد
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.ToActionResult();
    }
}


