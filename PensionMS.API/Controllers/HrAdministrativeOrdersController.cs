using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;

using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/hr/HrAdministrativeOrders")]
[Authorize]
public class HrAdministrativeOrdersController : ControllerBase
{
    private readonly IHrAdministrativeOrderService _service;

    public HrAdministrativeOrdersController(IHrAdministrativeOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? employeeId = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, employeeId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<object>.Fail("الأمر الإداري غير موجود"));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "HR.Employee.Create")]
    public async Task<IActionResult> Create([FromBody] CreateAdministrativeOrderDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.AdministrativeOrderID }, result);
    }

    [HttpPost("{id:int}/issue")]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> Issue(int id)
    {
        var dto = new IssueAdministrativeOrderDto { IssuedByUserID = this.GetUserId() };
        var result = await _service.IssueAsync(id, dto);
        if (result == null) return NotFound(ApiResponse<object>.Fail("الأمر الإداري غير موجود"));
        return Ok(result);
    }
}
