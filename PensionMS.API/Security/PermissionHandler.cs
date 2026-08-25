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

        // استخدام نطاق جديد لحل خدمة IScopedPermissionResolver (لأن Handler يتم تسجيله كـ Singleton)
        using var scope = _serviceProvider.CreateScope();
        var permissionResolver = scope.ServiceProvider.GetRequiredService<IScopedPermissionResolver>();

        // التحقق المباشر من قاعدة البيانات مع مراعاة نطاق الفرع
        var branchClaim = context.User.FindFirst("BranchId")?.Value;
        bool hasPermission;

        if (int.TryParse(branchClaim, out int branchId) && branchId > 0)
        {
            // مستخدم مقيد بفرع: التحقق من الصلاحية ضمن نطاق فرعه
            hasPermission = await permissionResolver.HasPermissionInBranchAsync(userId, requirement.Permission, branchId);
        }
        else
        {
            // مستخدم مركزي/الإدارة العامة: التحقق من الصلاحية العامة
            hasPermission = await permissionResolver.HasPermissionAsync(userId, requirement.Permission);
        }

        _logger.LogDebug("PermissionHandler: userId={UserId}, branchId={BranchId}, permission='{Permission}', result={Result}",
            userId, branchId, requirement.Permission, hasPermission);

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
