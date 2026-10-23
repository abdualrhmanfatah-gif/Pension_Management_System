using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Security;

public sealed class CurrentUserContext(IHttpContextAccessor httpContext) : ICurrentUserContext
{
    public int GetCurrentUserId()
    {
        var user = httpContext.HttpContext?.User;
        var value = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return user?.Identity?.IsAuthenticated == true && int.TryParse(value, out var id) && id > 0 ? id : 0;
    }
}
