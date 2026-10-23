using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Security;

/// <summary>Reject previously issued access tokens after user deletion or deactivation.</summary>
public static class ActiveUserValidation
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var id) || id <= 0
            || !await context.HttpContext.RequestServices.GetRequiredService<IUserService>().IsActiveAsync(id))
            context.Fail("حساب المستخدم غير نشط أو محذوف");
    }
}
