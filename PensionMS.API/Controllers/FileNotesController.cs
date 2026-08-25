using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.FileNotes;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/file-notes")]
[Authorize]
public class FileNotesController : ControllerBase
{
    private readonly IFileNoteService _service;

    public FileNotesController(IFileNoteService service)
    {
        _service = service;
    }

    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "FileNote.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "FileNote.Manage")]
    public async Task<IActionResult> Create([FromBody] CreateFileNoteDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "FileNote.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }
}


