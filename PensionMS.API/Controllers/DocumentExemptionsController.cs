using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.DocumentExemptions;
using UpdateDocumentExemptionDto = PensionMS.Core.DTOs.DocumentExemptions.UpdateDocumentExemptionDto;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/document-exemptions")]
[Authorize]
public class DocumentExemptionsController : ControllerBase
{
    private readonly IDocumentExemptionService _service;

    public DocumentExemptionsController(IDocumentExemptionService service)
    {
        _service = service;
    }

    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "DocumentExemption.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileAsync(fileId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "DocumentExemption.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "DocumentExemption.Add")]
    public async Task<IActionResult> Create([FromBody] CreateDocumentExemptionDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DocumentExemption.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentExemptionDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DocumentExemption.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }
}


