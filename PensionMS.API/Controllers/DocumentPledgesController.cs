using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.DocumentPledges;
using UpdateDocumentPledgeDto = PensionMS.Core.DTOs.DocumentPledges.UpdateDocumentPledgeDto;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/document-pledges")]
[Authorize]
public class DocumentPledgesController : ControllerBase
{
    private readonly IDocumentPledgeService _service;

    public DocumentPledgesController(IDocumentPledgeService service)
    {
        _service = service;
    }

    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "DocumentPledge.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileAsync(fileId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "DocumentPledge.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "DocumentPledge.Add")]
    public async Task<IActionResult> Create([FromBody] CreateDocumentPledgeDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "DocumentPledge.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentPledgeDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}/fulfill")]
    [Authorize(Policy = "DocumentPledge.Manage")]
    public async Task<IActionResult> MarkFulfilled(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.MarkFulfilledAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DocumentPledge.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }
}


