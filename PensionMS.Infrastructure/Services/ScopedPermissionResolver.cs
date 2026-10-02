using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// يحل صلاحيات المستخدم بناءً على تعيينات الأدوار مع مراعاة النطاق (الفرع/الإقليم/المركزي)
/// </summary>
public class ScopedPermissionResolver : IScopedPermissionResolver
{
    private readonly PensionDbContext _db;

    public ScopedPermissionResolver(PensionDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
    {
        // ═══ 1. فحص الصلاحيات الفردية (UserPermissions) — أعلى أولوية ═══
        var individualPerm = await _db.UserPermissions
            .Where(up => up.UserId == userId)
            .Join(_db.Set<PensionMS.Core.Entities.Permission>(),
                  up => up.PermissionId,
                  p => p.PermissionId,
                  (up, p) => new { p.Code, up.IsGranted })
            .FirstOrDefaultAsync(x => x.Code == permissionCode);

        if (individualPerm != null)
            return individualPerm.IsGranted; // سحب فردي يلغي حتى صلاحية الدور

        // ═══ 2. فحص عبر UserRoleAssignments → RolePermissions ═══
        var hasViaAssignment = await _db.UserRoleAssignments
            .Where(ura => ura.UserId == userId && ura.IsActive)
            .SelectMany(ura => ura.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Code == permissionCode);

        if (hasViaAssignment) return true;

        // ═══ 3. Fallback: User.RoleID → RolePermissions ═══
        var user = await _db.Users.FindAsync(userId);
        if (user?.RoleID != null)
        {
            var hasViaRole = await _db.Set<PensionMS.Core.Entities.RolePermission>()
                .Where(rp => rp.RoleId == user.RoleID)
                .AnyAsync(rp => rp.Permission.Code == permissionCode);
            if (hasViaRole) return true;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionInBranchAsync(int userId, string permissionCode, int branchId)
    {
        // ═══ 1. فحص الصلاحيات الفردية (UserPermissions) — أعلى أولوية ═══
        var individualPerm = await _db.UserPermissions
            .Where(up => up.UserId == userId)
            .Join(_db.Set<PensionMS.Core.Entities.Permission>(),
                  up => up.PermissionId,
                  p => p.PermissionId,
                  (up, p) => new { p.Code, up.IsGranted })
            .FirstOrDefaultAsync(x => x.Code == permissionCode);

        if (individualPerm != null)
            return individualPerm.IsGranted; // سحب فردي يلغي حتى صلاحية الدور

        // ═══ 2. فحص عبر UserRoleAssignments → RolePermissions (ضمن نطاق الفرع) ═══
        var assignments = await _db.UserRoleAssignments
            .Include(ura => ura.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ura => ura.UserId == userId && ura.IsActive)
            .ToListAsync();

        foreach (var assignment in assignments)
        {
            var hasPermission = assignment.Role.RolePermissions
                .Any(rp => rp.Permission.Code == permissionCode);

            if (!hasPermission) continue;

            // 1. نطاق مركزي (BranchId == null && RegionId == null) → يملك صلاحية على كل الفروع
            if (assignment.BranchId == null && assignment.RegionId == null)
                return true;

            // 2. نطاق فرع مباشر
            if (assignment.BranchId == branchId)
                return true;

            // 3. نطاق إقليمي → فحص هل الفرع المطلوب تابع للإقليم
            if (assignment.RegionId != null)
            {
                var childBranchIds = await GetChildBranchIdsAsync(assignment.RegionId.Value);
                if (childBranchIds.Contains(branchId))
                    return true;
            }
        }

        // ═══ 3. Fallback: User.RoleID → RolePermissions (بدون نطاق فرع) ═══
        if (assignments.Count == 0)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.RoleID != null)
            {
                var hasViaRole = await _db.Set<PensionMS.Core.Entities.RolePermission>()
                    .Where(rp => rp.RoleId == user.RoleID)
                    .AnyAsync(rp => rp.Permission.Code == permissionCode);
                if (hasViaRole) return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var permCodes = new HashSet<string>();

        // 1. من UserRoleAssignments
        var assignmentPerms = await _db.UserRoleAssignments
            .Where(ura => ura.UserId == userId && ura.IsActive)
            .SelectMany(ura => ura.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();
        foreach (var c in assignmentPerms) permCodes.Add(c);

        // 2. Fallback: User.RoleID → RolePermissions (إذا ما فيه أي assignment نشط)
        if (assignmentPerms.Count == 0)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.RoleID != null)
            {
                var rolePerms = await _db.Set<PensionMS.Core.Entities.RolePermission>()
                    .Where(rp => rp.RoleId == user.RoleID)
                    .Select(rp => rp.Permission.Code)
                    .ToListAsync();
                foreach (var c in rolePerms) permCodes.Add(c);
            }
        }

        // 3. Individual UserPermissions (grant adds, revoke removes)
        var individualPerms = await _db.UserPermissions
            .Where(up => up.UserId == userId)
            .Join(_db.Set<PensionMS.Core.Entities.Permission>(),
                  up => up.PermissionId,
                  p => p.PermissionId,
                  (up, p) => new { p.Code, up.IsGranted })
            .ToListAsync();

        foreach (var ip in individualPerms)
        {
            if (ip.IsGranted)
                permCodes.Add(ip.Code);
            else
                permCodes.Remove(ip.Code);
        }

        return permCodes.ToList();
    }

    /// <inheritdoc />
    public async Task<List<int>> GetUserBranchIdsAsync(int userId)
    {
        var assignments = await _db.UserRoleAssignments
            .Where(ura => ura.UserId == userId && ura.IsActive)
            .ToListAsync();

        var branchIds = new HashSet<int>();

        foreach (var a in assignments)
        {
            if (a.BranchId == null && a.RegionId == null)
            {
                // مركزي → كل الفروع
                var allBranches = await _db.PensionFundBranches
                    .Select(b => b.PensionBranchID)
                    .ToListAsync();
                return allBranches;
            }

            if (a.BranchId != null)
                branchIds.Add(a.BranchId.Value);

            if (a.RegionId != null)
            {
                var childIds = await GetChildBranchIdsAsync(a.RegionId.Value);
                foreach (var cid in childIds)
                    branchIds.Add(cid);
            }
        }

        return branchIds.ToList();
    }

    /// <summary>
    /// يجلب معرّفات الفروع الأبناء لفرع/إقليم محدد (بما فيه نفسه)
    /// </summary>
    private async Task<List<int>> GetChildBranchIdsAsync(int parentBranchId)
    {
        var allBranches = await _db.PensionFundBranches
            .AsNoTracking()
            .Select(b => new { b.PensionBranchID, b.ParentBranchId })
            .ToListAsync();

        var result = new HashSet<int> { parentBranchId };
        var queue = new Queue<int>();
        queue.Enqueue(parentBranchId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = allBranches.Where(b => b.ParentBranchId == current);
            foreach (var child in children)
            {
                if (result.Add(child.PensionBranchID))
                {
                    queue.Enqueue(child.PensionBranchID);
                }
            }
        }

        return result.ToList();
    }
}
