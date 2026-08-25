using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/hr/HrServiceHistory")]
[Authorize]
public class HrServiceHistoryController : ControllerBase
{
    private readonly IHrServiceHistoryService _service;

    public HrServiceHistoryController(IHrServiceHistoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "HR.ServiceHistory.View")]
    public async Task<IActionResult> GetGlobalHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _service.GetGlobalHistoryAsync(page, pageSize, search);
        return result.ToActionResult();
    }

    [HttpGet("employee/{employeeId:int}")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId)
    {
        var result = await _service.GetByEmployeeIdAsync(employeeId);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateServiceHistoryEventDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _service.CreateEventAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateServiceHistoryEventDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _service.UpdateEventAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _service.DeleteEventAsync(id, userId);
        return result.ToActionResult();
    }
}


