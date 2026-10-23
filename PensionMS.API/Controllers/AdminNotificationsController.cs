using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Interfaces;
using PensionMS.API.Extensions;
using System.Security.Claims;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Policy = "System.Admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly IAdminNotificationService _adminNotificationService;

    public AdminNotificationsController(IAdminNotificationService adminNotificationService)
    {
        _adminNotificationService = adminNotificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminNotificationFilterDto filter)
    {
        var result = await _adminNotificationService.GetAllNotificationsAsync(filter);
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _adminNotificationService.GetStatsAsync();
        return Ok(result);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] AdminSendNotificationDto dto)
    {
        var actorId = GetUserId();
        var result = await _adminNotificationService.SendNotificationAsync(dto, actorId);
        return Ok(result);
    }

    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] AdminBroadcastNotificationDto dto)
    {
        var actorId = GetUserId();
        var result = await _adminNotificationService.BroadcastNotificationAsync(dto, actorId);
        return Ok(result);
    }

    [HttpPost("send-multi")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SendMulti(
        [FromForm] string title,
        [FromForm] string message,
        [FromForm] List<int> userIds,
        [FromForm] string? category,
        [FromForm] byte priority,
        [FromForm] string? actionUrl,
        [FromForm] List<IFormFile>? attachments)
    {
        var actorId = GetUserId();
        
        var uploadAttachments = attachments?.Select(f => new NotificationAttachmentUpload
        {
            FileName = f.FileName,
            ContentType = f.ContentType,
            Stream = f.OpenReadStream(),
            Length = f.Length
        }).ToList();

        var result = await _adminNotificationService.SendNotificationWithAttachmentsAsync(
            title, message, userIds, uploadAttachments, category, priority, actionUrl, actorId);
        return Ok(result);
    }

    [HttpGet("outbox")]
    public async Task<IActionResult> GetOutbox([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var result = await _adminNotificationService.GetOutboxAsync(page, pageSize, status);
        return Ok(result);
    }

    [HttpPost("outbox/{id:long}/retry")]
    public async Task<IActionResult> RetryOutbox(long id)
    {
        var result = await _adminNotificationService.RetryOutboxAsync(id);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _adminNotificationService.DeleteNotificationAsync(id);
        return Ok(result);
    }

    [HttpDelete("bulk")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteDto dto)
    {
        var result = await _adminNotificationService.BulkDeleteAsync(dto, GetUserId());
        return result.ToActionResult();
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }
}
