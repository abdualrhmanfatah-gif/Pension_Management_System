using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PensionMS.Core.Interfaces;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using System.Security.Claims;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? category = null)
    {
        var userId = GetUserId();
        var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, category);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var result = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var userId = GetUserId();
        var result = await _notificationService.MarkAsReadAsync(id, userId);
        return Ok(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        var result = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }
}
