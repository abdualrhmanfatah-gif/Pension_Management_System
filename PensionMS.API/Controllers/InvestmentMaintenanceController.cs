using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/maintenance")]
[Authorize]
public class InvestmentMaintenanceController : ControllerBase
{
    private readonly IInvestmentMaintenanceService _service;

    public InvestmentMaintenanceController(IInvestmentMaintenanceService service)
    {
        _service = service;
    }

    // ----------------------------------------------------
    // Maintenance Requests
    // ----------------------------------------------------

    [HttpGet("properties/{propertyId:int}/requests")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetRequestsByProperty(int propertyId)
    {
        var result = await _service.GetRequestsByPropertyAsync(propertyId);
        return result.ToActionResult();
    }

    [HttpGet("requests/all")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAllRequests()
    {
        var result = await _service.GetAllMaintenanceRequestsAsync();
        return result.ToActionResult();
    }

    [HttpGet("requests/{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetRequestById(int id)
    {
        var result = await _service.GetMaintenanceRequestByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("requests")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateMaintenanceRequestDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateMaintenanceRequestAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("requests/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] UpdateMaintenanceRequestDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateMaintenanceRequestAsync(id, dto, userId);
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Work Orders
    // ----------------------------------------------------

    [HttpGet("requests/{requestId:int}/work-orders")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetWorkOrdersByRequest(int requestId)
    {
        var result = await _service.GetWorkOrdersByRequestAsync(requestId);
        return result.ToActionResult();
    }

    [HttpGet("work-orders/{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetWorkOrderById(int id)
    {
        var result = await _service.GetWorkOrderByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("work-orders")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateWorkOrder([FromBody] CreateWorkOrderDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateWorkOrderAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("work-orders/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> UpdateWorkOrder(int id, [FromBody] UpdateWorkOrderDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateWorkOrderAsync(id, dto, userId);
        return result.ToActionResult();
    }
}


