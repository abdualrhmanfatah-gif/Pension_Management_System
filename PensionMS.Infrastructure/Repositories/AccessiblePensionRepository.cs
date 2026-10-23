using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using PensionMS.Core.Security;

namespace PensionMS.Infrastructure.Repositories;

public sealed class AccessiblePensionRepository(
    IUnitOfWork unitOfWork,
    ICurrentUserContext identity,
    IScopedPermissionResolver permissions,
    IHttpContextAccessor httpContext) : IAccessiblePensionRepository
{
    public async Task<IPensionDataQuery> ForCurrentOperationAsync()
    {
        // Policies come from server endpoint metadata, never request parameters.
        var policies = httpContext.HttpContext?.GetEndpoint()?.Metadata
            .GetOrderedMetadata<IAuthorizeData>().Select(a => a.Policy)
            .Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToArray();
        if (policies is null || policies.Length == 0) throw new BranchScopeAccessException();
        var scope = PermissionScope.All;
        foreach (var policy in policies)
        {
            var decision = (await ForPermissionAsync(policy!)).Scope;
            scope = scope.Intersect(decision);
        }
        if (scope.IsDenied) throw new BranchScopeAccessException();
        return new PensionDataQuery(unitOfWork, scope, policies.All(p => IsReadOnly(p!)));
    }

    public async Task<IPensionDataQuery> ForPermissionAsync(string permissionCode)
    {
        var userId = identity.GetCurrentUserId();
        if (userId <= 0) throw new BranchScopeAccessException();
        var scope = await permissions.GetAllowedScopeAsync(userId, permissionCode);
        if (scope.IsDenied) throw new BranchScopeAccessException();
        return new PensionDataQuery(unitOfWork, scope, IsReadOnly(permissionCode));
    }

    private static bool IsReadOnly(string permission)
        => permission.EndsWith(".View", StringComparison.Ordinal) || permission.Contains(".View.", StringComparison.Ordinal)
            || permission.EndsWith(".Export", StringComparison.Ordinal);
}
