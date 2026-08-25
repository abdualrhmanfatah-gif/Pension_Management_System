using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Guardians;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/guardians")]
[Authorize]
public class GuardiansController : ControllerBase
{
    private readonly IGuardianService _service;

    public GuardiansController(IGuardianService service)
    {
        _service = service;
    }

    /// <summary>بحث شامل عن الأوصياء</summary>
    [HttpGet("search")]
    [Authorize(Policy = "Guardian.View")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
    {
        var result = await _service.SearchAsync(q, limit);
        return result.ToActionResult();
    }

    /// <summary>تفاصيل وصي بالمعرف</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "Guardian.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>وصايا مستحق معين</summary>
    [HttpGet("by-entitled/{entitledId:int}")]
    [Authorize(Policy = "Guardian.View")]
    public async Task<IActionResult> GetByEntitled(int entitledId)
    {
        var result = await _service.GetByEntitledIdAsync(entitledId);
        return result.ToActionResult();
    }

    /// <summary>جميع الوصايا لملف معين</summary>
    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "Guardian.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    /// <summary>
    /// تعيين وصي على مستحق قاصر — يُحوّل SalaryRecipient تلقائياً
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Guardian.Add")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Assign([FromForm] AssignGuardianDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.AssignAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    /// <summary>
    /// إضافة قاصر إضافي لوصي موجود
    /// </summary>
    [HttpPut("{id:int}/add-minor")]
    [Authorize(Policy = "Guardian.Edit")]
    public async Task<IActionResult> AddMinor(int id, [FromBody] AddMinorDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.AddMinorAsync(id, dto.EntitledID, dto.StartDate, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// إزالة قاصر من وصي (إنهاء وصاية على قاصر محدد)
    /// </summary>
    [HttpPut("{id:int}/remove-minor/{entitledId:int}")]
    [Authorize(Policy = "Guardian.Edit")]
    public async Task<IActionResult> RemoveMinor(int id, int entitledId, [FromBody] EndGuardianshipDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.EndGuardianshipAsync(id, entitledId, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// تحديث بيانات الوصي
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "Guardian.Edit")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateGuardianDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// إنهاء الوصاية — يُعيد SalaryRecipient للمستحق تلقائياً
    /// </summary>
    [HttpPut("{id:int}/end")]
    [Authorize(Policy = "Guardian.Delete")]
    public async Task<IActionResult> End(int id, [FromBody] EndGuardianshipDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.EndGuardianshipAsync(id, dto, userId);
        return result.ToActionResult();
    }
}


