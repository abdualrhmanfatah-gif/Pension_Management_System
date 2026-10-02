using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/hr/HrLeaveRequests")]
[Authorize]
public class HrLeaveRequestsController : ControllerBase
{
    private readonly IHrLeaveRequestService _service;

    public HrLeaveRequestsController(IHrLeaveRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? employeeId = null, [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, employeeId, search, status);
        return result.ToActionResult();
    }

    [HttpGet("stats")]
    [Authorize(Policy = "HR.Dashboard.View")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        if (!result.IsSuccess) return result.ToActionResult();
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/process")]
    [Authorize(Policy = "HR.Employee.Delete")] 
    public async Task<IActionResult> Process(int id, [FromBody] ProcessLeaveRequestDto dto)
    {
        dto.ProcessedByUserID = this.GetUserId();
        var result = await _service.ProcessAsync(id, dto);
        return result.ToActionResult();
    }
}


