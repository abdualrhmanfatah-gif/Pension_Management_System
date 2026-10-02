using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PensionMS.Infrastructure.Services;

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var branchId = Context.User?.FindFirst("BranchId")?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

        if (!string.IsNullOrEmpty(branchId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Branch_{branchId}");

        if (!string.IsNullOrEmpty(role))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");

        await base.OnConnectedAsync();
    }
}
