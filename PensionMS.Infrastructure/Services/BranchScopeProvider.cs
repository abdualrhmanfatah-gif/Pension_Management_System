using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// تنفيذ مزود نطاق الفرع باستخدام HttpContext
/// </summary>
public class BranchScopeProvider : IBranchScopeProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchScopeProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentBranchId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("BranchId")?.Value;
        return int.TryParse(claim, out int branchId) && branchId > 0 ? branchId : null;
    }

    public int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int userId) ? userId : 0;
    }

    public bool IsHQ()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("IsHQ")?.Value;
        return bool.TryParse(claim, out bool isHQ) && isHQ;
    }
}
