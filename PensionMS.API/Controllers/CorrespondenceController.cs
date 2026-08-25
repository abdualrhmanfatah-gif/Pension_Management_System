using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.API.Helpers;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Correspondence;
using PensionMS.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CorrespondenceController : ControllerBase
{
    private readonly ICorrespondenceService _service;

    public CorrespondenceController(ICorrespondenceService service)
    {
        _service = service;
    }

    // ─── Correspondences ─────────────────────────────────────

    [HttpGet]
    [Authorize(Policy = "Correspondence.View")]
    public async Task<IActionResult> GetAll(
        [FromQuery] byte? direction = null, 
        [FromQuery] int? typeId = null,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] string? search = null)
    {
        var result = await _service.GetAllAsync(direction, typeId, page, pageSize, search);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Correspondence.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Correspondence.Create")]
    public async Task<IActionResult> Create([FromBody] CreateCorrespondenceDto dto)
    {
        var result = await _service.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Correspondence.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCorrespondenceDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Correspondence.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.ToActionResult();
    }

    // ─── External Parties ────────────────────────────────────

    [HttpGet("external-parties")]
    [Authorize(Policy = "Correspondence.View")]
    public async Task<IActionResult> GetExternalParties()
    {
        var result = await _service.GetExternalPartiesAsync();
        return result.ToActionResult();
    }

    [HttpPost("external-parties")]
    [Authorize(Policy = "Correspondence.Create")]
    public async Task<IActionResult> CreateExternalParty([FromBody] CreateExternalPartyDto dto)
    {
        var result = await _service.CreateExternalPartyAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("external-parties/{id:int}")]
    [Authorize(Policy = "Correspondence.Edit")]
    public async Task<IActionResult> UpdateExternalParty(int id, [FromBody] UpdateExternalPartyDto dto)
    {
        var result = await _service.UpdateExternalPartyAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("external-parties/{id:int}")]
    [Authorize(Policy = "Correspondence.Delete")]
    public async Task<IActionResult> DeleteExternalParty(int id)
    {
        var result = await _service.DeleteExternalPartyAsync(id);
        return result.ToActionResult();
    }

    // ─── Correspondence Types ────────────────────────────────

    [HttpGet("types")]
    [Authorize(Policy = "Correspondence.View")]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _service.GetTypesAsync();
        return result.ToActionResult();
    }

    // ─── Documents ──────────────────────────────────────────

    [HttpGet("{id:int}/documents")]
    [Authorize(Policy = "Correspondence.View")]
    public async Task<IActionResult> GetDocuments(int id)
    {
        var result = await _service.GetDocumentsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/documents")]
    [Authorize(Policy = "Correspondence.Create")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("لم يتم اختيار ملف"));

        var result = await _service.UploadDocumentAsync(id, file.ToRemoteFile(), this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}/documents/{documentId:int}")]
    [Authorize(Policy = "Correspondence.Delete")]
    public async Task<IActionResult> DeleteDocument(int id, int documentId)
    {
        var result = await _service.DeleteDocumentAsync(id, documentId);
        return result.ToActionResult();
    }
}
