using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/journal-entry-types")]
[Authorize]
public class JournalEntryTypeController : ControllerBase
{
    private readonly IJournalEntryTypeService _service;

    public JournalEntryTypeController(IJournalEntryTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("all")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAllIncludingInactive()
    {
        var result = await _service.GetAllIncludingInactiveAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryTypeDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJournalEntryTypeDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.ToActionResult();
    }
}
