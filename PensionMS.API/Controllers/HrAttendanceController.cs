using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/hr/HrAttendance")]
[Authorize]
public class HrAttendanceController : ControllerBase
{
    private readonly IHrAttendanceService _service;

    public HrAttendanceController(IHrAttendanceService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetAttendance([FromQuery] int? employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetAttendanceEventsAsync(employeeId, fromDate, toDate, page, pageSize);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> RecordAttendance([FromBody] CreateAttendanceEventDto dto)
    {
        var result = await _service.RecordAttendanceAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("summary/{employeeId:int}")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetEmployeeSummary(int employeeId, [FromQuery] int year, [FromQuery] int month)
    {
        var result = await _service.GetEmployeeSummaryAsync(employeeId, year, month);
        return result.ToActionResult();
    }

    [HttpGet("daily-stats")]
    [Authorize(Policy = "HR.Dashboard.View")]
    public async Task<IActionResult> GetDailyStats([FromQuery] DateTime? date)
    {
        var statsDate = date ?? DateTime.Today;
        var result = await _service.GetDailyStatsAsync(statsDate);
        return result.ToActionResult();
    }
}


