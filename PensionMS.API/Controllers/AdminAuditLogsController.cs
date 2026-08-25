using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

using PensionMS.API.Extensions;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize]
public class AdminAuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AdminAuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    /// <summary>
    /// جلب سجلات التدقيق مع فلترة وترقيم
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? action = null,
        [FromQuery] int? userId = null,
        [FromQuery] int? tableId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, action, userId, tableId, from, to, search);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب سجلات التدقيق المقاومة للتلاعب (Immutable)
    /// </summary>
    [HttpGet("immutable")]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetImmutable(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? actorId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _service.GetImmutableAsync(page, pageSize, action, entityType, actorId, from, to);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب قائمة الأحداث المتاحة (للفلتر)
    /// </summary>
    [HttpGet("actions")]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetDistinctActions()
    {
        var result = await _service.GetDistinctActionsAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب قائمة الجداول (للفلتر)
    /// </summary>
    [HttpGet("tables")]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetTables()
    {
        var result = await _service.GetTablesAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب سجلات التدقيق لسجل محدد في جدول محدد
    /// </summary>
    [HttpGet("by-record/{tableId:int}/{recordId:int}")]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetByRecord(int tableId, int recordId)
    {
        var result = await _service.GetByRecordAsync(tableId, recordId);
        return result.ToActionResult();
    }
}



