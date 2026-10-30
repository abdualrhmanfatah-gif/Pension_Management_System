using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Suspensions;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/suspensions")]
[Authorize]
public class PensionSuspensionsController : ControllerBase
{
    private readonly IPensionSuspensionService _service;

    public PensionSuspensionsController(IPensionSuspensionService service)
    {
        _service = service;
    }

    /// <summary>
    /// إيقاف المعاش مع إنشاء تعهد اختياري
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Suspension.Add")]
    public async Task<IActionResult> Create([FromBody] CreateSuspensionDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateWithPledgeAsync(dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// إنهاء إيقاف المعاش وإعادة تفعيل الملف
    /// </summary>
    [HttpPut("{suspensionId:int}/end")]
    [Authorize(Policy = "Suspension.Edit")]
    public async Task<IActionResult> End(int suspensionId, [FromBody] EndSuspensionDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.EndSuspensionAsync(suspensionId, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// قائمة إيقافات ملف معين
    /// </summary>
    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "Suspension.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    /// <summary>
    /// قائمة شاملة بجميع الإيقافات في النظام
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Suspension.View")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null)
    {
        var result = await _service.GetAllSuspensionsAsync(page, pageSize, status, fromDate, toDate, search);
        return result.ToActionResult();
    }
}


