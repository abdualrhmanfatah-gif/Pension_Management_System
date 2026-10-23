using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Security;

/// <summary>المصدر الوحيد لنطاق المستخدم لكل صلاحية. لا صلاحية مركزية ضمنية من JWT أو RoleID.</summary>
public sealed class ScopedPermissionResolver(PensionDbContext db) : IScopedPermissionResolver
{
    public async Task<PermissionScope> GetAllowedScopeAsync(int userId, string permissionCode)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(permissionCode))
            return PermissionScope.Denied;
        var user = await db.Users.Where(u => u.UserID == userId && u.IsActive && !u.IsDeleted)
            .Select(u => new { u.BranchID }).FirstOrDefaultAsync();
        if (user is null) return PermissionScope.Denied;

        var individual = await db.UserPermissions
            .Where(p => p.UserId == userId && p.Permission.Code == permissionCode)
            .OrderBy(p => p.IsGranted)
            .Select(p => (bool?)p.IsGranted).FirstOrDefaultAsync();
        if (individual == false) return PermissionScope.Denied;

        var assignments = await db.UserRoleAssignments.AsNoTracking()
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => new { a.BranchId, a.RegionId, Grants = a.Role.RolePermissions.Any(p => p.Permission.Code == permissionCode) })
            .ToListAsync();
        var branches = assignments.Any(a => a.RegionId.HasValue)
            ? await db.PensionFundBranches.AsNoTracking()
                .Select(b => new ScopeBranch(b.PensionBranchID, b.ParentBranchId)).ToListAsync()
            : [];
        return PermissionScopeRules.ResolvePermission(
            assignments.Where(a => a.Grants).Select(a => new ScopeAssignment(a.BranchId, a.RegionId)).ToList(),
            assignments.Select(a => new ScopeAssignment(a.BranchId, a.RegionId)).ToList(), branches, individual, user.BranchID);
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
        => !(await GetAllowedScopeAsync(userId, permissionCode)).IsDenied;

    public async Task<bool> HasPermissionInBranchAsync(int userId, string permissionCode, int branchId)
        => (await GetAllowedScopeAsync(userId, permissionCode)).Allows(branchId);

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var user = await db.Users.Where(u => u.UserID == userId && u.IsActive && !u.IsDeleted)
            .Select(u => new { u.BranchID }).FirstOrDefaultAsync();
        if (user is null) return [];
        var assignments = await db.UserRoleAssignments.AsNoTracking()
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => new { a.BranchId, a.RegionId, Codes = a.Role.RolePermissions.Select(p => p.Permission.Code).ToList() })
            .ToListAsync();
        var personal = await db.UserPermissions.AsNoTracking().Where(p => p.UserId == userId)
            .Select(p => new { p.Permission.Code, p.IsGranted }).ToListAsync();
        var overrides = personal.GroupBy(p => p.Code).ToDictionary(g => g.Key, g => g.All(p => p.IsGranted));
        var branches = assignments.Any(a => a.RegionId.HasValue)
            ? await db.PensionFundBranches.AsNoTracking().Select(b => new ScopeBranch(b.PensionBranchID, b.ParentBranchId)).ToListAsync()
            : [];
        var candidates = assignments.SelectMany(a => a.Codes).Union(overrides.Where(p => p.Value).Select(p => p.Key));
        var allowed = new List<string>();
        foreach (var permission in candidates)
        {
            if (overrides.TryGetValue(permission, out var granted) && !granted) continue;
            var sources = assignments.Where(a => a.Codes.Contains(permission))
                .Select(a => new ScopeAssignment(a.BranchId, a.RegionId)).ToList();
            var functional = assignments.Select(a => new ScopeAssignment(a.BranchId, a.RegionId)).ToList();
            if (!PermissionScopeRules.ResolvePermission(sources, functional, branches,
                    overrides.ContainsKey(permission) ? granted : null, user.BranchID).IsDenied) allowed.Add(permission);
        }
        return allowed.Order(StringComparer.Ordinal).ToList();
    }
}

internal sealed record ScopeAssignment(int? BranchId, int? RegionId);
internal sealed record ScopeBranch(int Id, int? ParentId);

internal static class PermissionScopeRules
{
    internal static PermissionScope ResolvePermission(IReadOnlyList<ScopeAssignment> permissionAssignments,
        IReadOnlyList<ScopeAssignment> functionalAssignments, IReadOnlyList<ScopeBranch> branches, bool? individualGrant, int? homeBranch)
    {
        if (individualGrant == false) return PermissionScope.Denied;
        var scope = Resolve(permissionAssignments, branches);
        if (!scope.IsDenied) return scope;
        // The legacy personal-grant row has no branch/region field. It cannot expand an
        // existing permission scope or infer a central grant from an unrelated role.
        if (individualGrant == true && homeBranch is > 0 && Resolve(functionalAssignments, branches).Allows(homeBranch.Value))
            return PermissionScope.Restricted([homeBranch.Value]);
        return PermissionScope.Denied;
    }

    internal static PermissionScope Resolve(IReadOnlyList<ScopeAssignment> assignments, IReadOnlyList<ScopeBranch> branches)
    {
        if (assignments.Any(a => a.BranchId is null && a.RegionId is null)) return PermissionScope.All;
        var allowed = new HashSet<int>();
        foreach (var assignment in assignments)
        {
            if (assignment.BranchId.HasValue && assignment.RegionId.HasValue) continue;
            if (assignment.BranchId is > 0) allowed.Add(assignment.BranchId.Value);
            if (assignment.RegionId is not > 0) continue;
            var queue = new Queue<int>();
            queue.Enqueue(assignment.RegionId.Value);
            var visited = new HashSet<int>();
            while (queue.TryDequeue(out var parent))
            {
                if (!visited.Add(parent)) continue;
                allowed.Add(parent);
                foreach (var child in branches.Where(b => b.ParentId == parent)) queue.Enqueue(child.Id);
            }
        }
        return PermissionScope.Restricted(allowed);
    }
}
