using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Users;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        return await _unitOfWork.Roles.Query()
            .AsNoTracking()
            .Where(r => r.IsActive)
            .Select(r => new RoleDto
            {
                RoleID = r.RoleID,
                RoleName = r.RoleName,
                Description = r.Description,
                IsActive = r.IsActive,
                UserCount = r.UserRoleAssignments.Count(a => a.IsActive)
            })
            .OrderBy(r => r.RoleID)
            .ToListAsync();
    }

    public async Task<RoleDto?> GetRoleWithPermissionsAsync(int roleId)
    {
        var role = await _unitOfWork.Roles.Query()
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.TablePermissions)
                .ThenInclude(tp => tp.SystemTable)
            .FirstOrDefaultAsync(r => r.RoleID == roleId);

        if (role == null) return null;

        var userCount = await _unitOfWork.UserRoleAssignments.Query()
            .AsNoTracking()
            .CountAsync(ura => ura.RoleId == roleId && ura.IsActive);

        return new RoleDto
        {
            RoleID = role.RoleID,
            RoleName = role.RoleName,
            Description = role.Description,
            IsActive = role.IsActive,
            UserCount = userCount,
            
            // Legacy Permissions (kept for backward compatibility)
            Permissions = role.TablePermissions.Select(tp => new TablePermissionDto
            {
                PermissionID = tp.PermissionID,
                TableNameAr = tp.SystemTable?.TableNameAr ?? "",
                TableNameEn = tp.SystemTable?.TableNameEn ?? "",
                CanCreate = tp.CanCreate,
                CanRead = tp.CanRead,
                CanUpdate = tp.CanUpdate,
                CanDelete = tp.CanDelete
            }).OrderBy(p => p.PermissionID).ToList(),

            // New RBAC Granular Permissions
            GranularPermissions = role.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => new PermissionDto
                {
                    PermissionId = rp.Permission!.PermissionId,
                    Code = rp.Permission.Code,
                    Description = rp.Permission.Description,
                    Category = rp.Permission.Category
                })
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Code)
                .ToList()
        };
    }

    public async Task<bool> HasPermissionAsync(int roleId, string tableName, string operation)
    {
        var permission = await _unitOfWork.TablePermissions.Query()
            .AsNoTracking()
            .Include(tp => tp.SystemTable)
            .FirstOrDefaultAsync(tp =>
                tp.RoleID == roleId &&
                (tp.SystemTable != null && (tp.SystemTable.TableNameEn == tableName || tp.SystemTable.TableNameAr == tableName)));

        if (permission == null) return false;

        return operation.ToLower() switch
        {
            "create" => permission.CanCreate,
            "read" => permission.CanRead,
            "update" => permission.CanUpdate,
            "delete" => permission.CanDelete,
            _ => false
        };
    }

    // ═══════════════════════════════════════
    // RBAC PERMISSION MANAGEMENT
    // ═══════════════════════════════════════

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        return await _unitOfWork.Permissions.Query()
            .AsNoTracking()
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Code)
            .Select(p => new PermissionDto
            {
                PermissionId = p.PermissionId,
                Code = p.Code,
                Description = p.Description,
                Category = p.Category
            })
            .ToListAsync();
    }

    public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null) return false;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // حذف الصلاحيات القديمة
            var existing = await _unitOfWork.RolePermissions.Query()
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
            _unitOfWork.RolePermissions.DeleteRange(existing);

            // إضافة الصلاحيات الجديدة
            foreach (var pid in permissionIds.Distinct())
            {
                await _unitOfWork.RolePermissions.AddAsync(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = pid,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            return false;
        }
    }
}
