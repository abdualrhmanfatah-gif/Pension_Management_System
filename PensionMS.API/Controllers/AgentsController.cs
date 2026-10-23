using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Agents;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/agents")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _service;

    public AgentsController(IAgentService service)
    {
        _service = service;
    }

    [HttpGet("search")]
    [Authorize(Policy = "Agent.View")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
    {
        var result = await _service.SearchAsync(q, limit);
        return result.ToActionResult();
    }

    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "Agent.View")]
    public async Task<IActionResult> GetByFile(int fileId)
    {
        var result = await _service.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Agent.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "Agent.Add")]
    public async Task<IActionResult> Create([FromForm] CreateAgentDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "Agent.Edit")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateAgentDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/photo")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "Agent.Edit")]
    public async Task<IActionResult> UploadPhoto(int id, IRemoteFile photo) =>
        (await _service.UpdatePhotoAsync(id, photo, this.GetUserId())).ToActionResult();

    [HttpPut("{id:int}/deactivate")]
    [Authorize(Policy = "Agent.Delete")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeactivateAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Agent.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }
}


