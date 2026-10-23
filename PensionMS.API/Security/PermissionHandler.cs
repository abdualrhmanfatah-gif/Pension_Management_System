using Microsoft.AspNetCore.Authorization;
using PensionMS.Core.Interfaces;
using System.Security.Claims;

namespace PensionMS.API.Security;

/// <summary>
/// معالج الصلاحيات المخصص الذي يعترض الطلبات ويتحقق من الصلاحيات
/// مباشرة من قاعدة البيانات مع كل طلب لضمان المنع الفوري عند سحب الصلاحية.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionHandler> _logger;

    public PermissionHandler(IServiceProvider serviceProvider, ILogger<PermissionHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            _logger.LogWarning("الطلب رُفض: لا يوجد مستخدم معرف (UserId Claim missing).");
            return; // لم يتم العثور على المستخدم
        }

        // Share the HTTP request's service scope with the data services.
        var http = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
        using var fallbackScope = http is null ? _serviceProvider.CreateScope() : null;
        var permissionResolver = (http?.RequestServices ?? fallbackScope!.ServiceProvider)
            .GetRequiredService<IScopedPermissionResolver>();
        var hasPermission = await permissionResolver.HasPermissionAsync(userId, requirement.Permission);
        // System administration operates on global configuration and accounts.
        if (hasPermission && requirement.Permission.StartsWith("System.", StringComparison.Ordinal))
            hasPermission = (await permissionResolver.GetAllowedScopeAsync(userId, requirement.Permission)).AllBranches;

        _logger.LogDebug("PermissionHandler: userId={UserId}, permission='{Permission}', result={Result}",
            userId, requirement.Permission, hasPermission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("الطلب رُفض: المستخدم '{UserId}' لا يملك الصلاحية '{Permission}'.",
                userId, requirement.Permission);
            context.Fail(new AuthorizationFailureReason(this, 
                $"المستخدم لا يملك الصلاحية المطلوبة: {requirement.Permission}"));
        }
    }
}
