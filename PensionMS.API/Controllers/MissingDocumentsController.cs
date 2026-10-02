using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/missing-documents")]
[Authorize]
public class MissingDocumentsController : ControllerBase
{
    private readonly IMissingDocumentService _service;

    public MissingDocumentsController(IMissingDocumentService service)
    {
        _service = service;
    }

    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "MissingDocument.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileAsync(fileId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "MissingDocument.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "MissingDocument.Add")]
    public async Task<IActionResult> Create([FromBody] CreateMissingDocumentDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}/fulfill")]
    [Authorize(Policy = "MissingDocument.Edit")]
    public async Task<IActionResult> MarkFulfilled(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.MarkFulfilledAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "MissingDocument.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب الوثائق الناقصة المكتشفة تلقائياً (بناءً على IsRequired)
    /// </summary>
    [HttpGet("auto-detected/by-file/{fileId:int}")]
    [Authorize(Policy = "MissingDocument.View")]
    public async Task<IActionResult> GetAutoDetectedByFile(int fileId)
    {
        var result = await _service.GetAutoDetectedByFileAsync(fileId);
        return result.ToActionResult();
    }
}


