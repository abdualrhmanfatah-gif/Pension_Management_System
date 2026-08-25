using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryCustodyController : ControllerBase
{
    private readonly IInventoryCustodyService _custodyService;

    public InventoryCustodyController(IInventoryCustodyService custodyService)
    {
        _custodyService = custodyService;
    }

    [HttpGet]
    [Authorize(Policy = "Inventory.Custody.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _custodyService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("active")]
    [Authorize(Policy = "Inventory.Custody.View")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _custodyService.GetActiveAsync();
        return result.ToActionResult();
    }

    [HttpGet("by-employee/{employeeId:int}")]
    [Authorize(Policy = "Inventory.Custody.View")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        var result = await _custodyService.GetByEmployeeAsync(employeeId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Inventory.Custody.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _custodyService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Inventory.Custody.Create")]
    public async Task<IActionResult> Create([FromBody] CreateCustodyDto dto)
    {
        var result = await _custodyService.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPut("{id:int}/return")]
    [Authorize(Policy = "Inventory.Custody.Return")]
    public async Task<IActionResult> Return(int id, [FromBody] UpdateCustodyDto dto)
    {
        var result = await _custodyService.ReturnCustodyAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _custodyService.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}
