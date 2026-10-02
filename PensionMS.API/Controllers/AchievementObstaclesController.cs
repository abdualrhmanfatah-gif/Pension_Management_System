using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.AchievementObstacles;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/achievement-obstacles")]
[Authorize]
public class AchievementObstaclesController : ControllerBase
{
    private readonly IAchievementObstacleService _service;

    public AchievementObstaclesController(IAchievementObstacleService service)
    {
        _service = service;
    }

    // ─── CRUD ───────────────────────────────────────────

    [HttpGet]
    [Authorize(Policy = "AchievementObstacle.View")]
    public async Task<IActionResult> GetPaged([FromQuery] AchievementObstacleFilterDto filter)
    {
        var result = await _service.GetPagedAsync(filter);
        return result.ToActionResult();
    }

    [HttpGet("stats")]
    [Authorize(Policy = "AchievementObstacle.View")]
    public async Task<IActionResult> GetStats([FromQuery] AchievementObstacleFilterDto filter)
    {
        var result = await _service.GetStatsAsync(filter);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "AchievementObstacle.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "AchievementObstacle.Add")]
    public async Task<IActionResult> Create([FromBody] CreateAchievementObstacleDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AchievementObstacle.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAchievementObstacleDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AchievementObstacle.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }

    // ─── Comments ───────────────────────────────────────

    [HttpGet("{id:int}/comments")]
    [Authorize(Policy = "AchievementObstacle.View")]
    public async Task<IActionResult> GetComments(int id)
    {
        var result = await _service.GetCommentsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/comments")]
    [Authorize(Policy = "AchievementObstacle.Add")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.AddCommentAsync(id, dto.Comment, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    [HttpDelete("comments/{commentId:int}")]
    [Authorize(Policy = "AchievementObstacle.Delete")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteCommentAsync(commentId, userId);
        return result.ToActionResult();
    }
}

// ─── Helper DTO ────────────────────────────────────────

public class AddCommentDto
{
    public string Comment { get; set; } = string.Empty;
}
