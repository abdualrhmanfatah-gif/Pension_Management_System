using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PensionMS.Core.Interfaces;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.API.Extensions;
using System.Security.Claims;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IAuthorizationService _authorizationService;

    public NotificationsController(INotificationService notificationService, IAuthorizationService authorizationService)
    {
        _notificationService = notificationService;
        _authorizationService = authorizationService;
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

    /// <summary>
    /// تنزيل مرفق إشعار — الملكية مفروضة في الخدمة: المستلم أو System.Admin فقط.
    /// </summary>
    [HttpGet("{id:long}/attachments/{attachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(long id, int attachmentId)
    {
        var userId = GetUserId();
        var isAdmin = (await _authorizationService.AuthorizeAsync(User, "System.Admin")).Succeeded;

        var result = await _notificationService.DownloadAttachmentAsync(id, attachmentId, userId, isAdmin);
        if (!result.IsSuccess)
            return result.ToActionResult();
        var attachment = result.Data;
        if (attachment is null)
            return NotFound(ApiResponse<NotificationAttachmentDownloadDto>.Fail("المرفق غير موجود"));
        return File(attachment.Content, attachment.ContentType, attachment.FileName);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }
}
