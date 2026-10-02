using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.SalaryRecipients;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/salary-recipients")]
[Authorize]
public class SalaryRecipientsController : ControllerBase
{
    private readonly ISalaryRecipientService _service;

    public SalaryRecipientsController(ISalaryRecipientService service)
    {
        _service = service;
    }

    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "SalaryRecipient.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "SalaryRecipient.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "SalaryRecipient.Add")]
    public async Task<IActionResult> Create([FromForm] CreateSalaryRecipientDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}/end")]
    [Authorize(Policy = "SalaryRecipient.Edit")]
    public async Task<IActionResult> End(int id, [FromBody] EndSalaryRecipientDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.EndAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/photo")]
    [Authorize(Policy = "SalaryRecipient.Edit")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(int id, IRemoteFile photo)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdatePhotoAsync(id, photo, userId);
        return result.ToActionResult();
    }
}


