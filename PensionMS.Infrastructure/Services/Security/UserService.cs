using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Users;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Security;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IAccessiblePensionRepository _access;
    private readonly IImmutableAuditService _audit;
    private readonly IScopedPermissionResolver _permissions;

    public UserService(IUnitOfWork unitOfWork, INotificationService notificationService,
        IAccessiblePensionRepository access, IImmutableAuditService audit, IScopedPermissionResolver permissions)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _access = access;
        _audit = audit;
        _permissions = permissions;
    }

    public Task<bool> IsActiveAsync(int userId) => _unitOfWork.Users.AnyAsync(
        u => u.UserID == userId && u.IsActive && !u.IsDeleted);

    public async Task<ApiResponse<PagedResult<UserDto>>> GetAllUsersAsync(int page, int pageSize)
    {
        var query = _unitOfWork.Users.Query()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Include(u => u.UserRoleAssignments)
                .ThenInclude(a => a.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.UserRoleAssignments)
                .ThenInclude(a => a.Branch)
            .Include(u => u.UserRoleAssignments).ThenInclude(a => a.Region)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission);

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Role!.RoleName ?? "")
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserDto>();
        foreach (var user in users)
            items.Add(MapToDto(user, await _permissions.GetUserPermissionsAsync(user.UserID)));
        return ApiResponse<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Include(u => u.UserRoleAssignments)
                .ThenInclude(a => a.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.UserRoleAssignments)
                .ThenInclude(a => a.Branch)
            .Include(u => u.UserRoleAssignments).ThenInclude(a => a.Region)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserID == id);

        return user == null ? null : MapToDto(user, await _permissions.GetUserPermissionsAsync(user.UserID));
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto, int createdByUserId)
    {
        // Check username uniqueness
        var exists = await _unitOfWork.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists)
            return ApiResponse<UserDto>.Fail("اسم المستخدم موجود مسبقاً");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleID = dto.RoleID,
                BranchID = dto.BranchID,
                CreatedByUserID = createdByUserId,
                IsActive = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // ═══ إنشاء UserRoleAssignment تلقائياً ═══
            if (dto.RoleID.HasValue && dto.BranchID.HasValue)
            {
                await _unitOfWork.UserRoleAssignments.AddAsync(new UserRoleAssignment
                {
                    UserId = user.UserID,
                    RoleId = dto.RoleID.Value,
                    BranchId = dto.BranchID,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitAsync();

            try
            {
                await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
                {
                    UserID = 0,
                    Title = "مستخدم جديد",
                    Message = $"تم إنشاء مستخدم جديد: ({user.FullName}) — اسم المستخدم: {user.Username}",
                    Type = NotificationConstants.Types.Info,
                    Category = NotificationConstants.Categories.Security,
                    Priority = NotificationConstants.Priorities.Normal,
                    ActorID = createdByUserId,
                    RelatedRecordID = user.UserID
                });
            }
            catch { }

            // Reload with all navigation properties
            var createdUser = await GetUserByIdAsync(user.UserID);
            return ApiResponse<UserDto>.Success(createdUser!, "تم إنشاء المستخدم بنجاح");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<UserDto>.Fail("فشل إنشاء المستخدم. يرجى المحاولة لاحقاً");
        }
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto, int updatedByUserId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserRoleAssignments)
            .FirstOrDefaultAsync(u => u.UserID == id);

        if (user == null) return null;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
            if (dto.IsLocked.HasValue)
            {
                user.IsLocked = dto.IsLocked.Value;
                if (!dto.IsLocked.Value)
                {
                    user.LockedUntil = null;
                    user.FailedLoginAttempts = 0;
                }
            }

            if (dto.RoleID.HasValue && dto.RoleID != user.RoleID)
            {
                var assignments = user.UserRoleAssignments.Where(a => a.IsActive).ToList();
                if (!assignments.Any(a => a.RoleId == user.RoleID))
                    throw new PensionMS.Core.Exceptions.BusinessRuleException("حدد تعيين الدور ونطاقه صراحةً قبل تغيير الدور");
                var result = await SetRoleAssignmentsAsync(id, new SetUserRoleAssignmentsDto
                {
                    Assignments = assignments.Select(a => new UserRoleScopeDto
                    {
                        RoleId = a.RoleId == user.RoleID ? dto.RoleID.Value : a.RoleId,
                        BranchId = a.BranchId, RegionId = a.RegionId,
                        IsCentral = a.BranchId is null && a.RegionId is null
                    }).ToList()
                }, updatedByUserId);
                if (!result.IsSuccess) throw new PensionMS.Core.Exceptions.BusinessRuleException(result.Message ?? "تعذر تعديل تعيين الدور");
                user.RoleID = dto.RoleID;
            }
            if (dto.BranchID.HasValue) user.BranchID = dto.BranchID.Value;

            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            if (dto.IsActive.HasValue)
            {
                try
                {
                    await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
                    {
                        UserID = 0,
                        Title = dto.IsActive.Value ? "تفعيل مستخدم" : "تعطيل مستخدم",
                        Message = dto.IsActive.Value
                            ? $"تم تفعيل المستخدم ({user.FullName}) — اسم المستخدم: {user.Username}"
                            : $"تم تعطيل المستخدم ({user.FullName}) — اسم المستخدم: {user.Username}",
                        Type = dto.IsActive.Value ? NotificationConstants.Types.Success : NotificationConstants.Types.Warning,
                        Category = NotificationConstants.Categories.Security,
                        Priority = NotificationConstants.Priorities.High,
                        ActorID = updatedByUserId,
                        RelatedRecordID = user.UserID
                    });
                }
                catch { }
            }

            // Reload with all navigation properties
            return await GetUserByIdAsync(id);
        }
        catch (Exception exception)
        {
            await _unitOfWork.RollbackAsync();
            if (exception is PensionMS.Core.Exceptions.BusinessRuleException or PensionMS.Core.Exceptions.BranchScopeAccessException)
                throw;
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(int id, int deletedByUserId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserRoleAssignments)
            .FirstOrDefaultAsync(u => u.UserID == id);

        if (user == null) return false;

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
        try
        {
            var now = DateTime.UtcNow;
            user.IsDeleted = true;
            user.IsActive = false;
            user.IsLocked = true;
            user.DeletedAt = now;
            user.DeletedByUserID = deletedByUserId;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            var tokens = await _unitOfWork.RefreshTokens.Query()
                .Where(t => t.UserId == id && !t.IsRevoked).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = now;
            }
            var devices = await _unitOfWork.DeviceRegistrations.Query()
                .Where(d => d.UserID == id && d.IsActive).ToListAsync();
            foreach (var device in devices)
            {
                device.IsActive = false;
                device.UpdatedAt = now;
            }
            await _notificationService.EnqueueRoleAsync("Admin", new CreateNotificationDto
            {
                UserID = 0,
                Title = "حذف مستخدم",
                Message = $"تم حذف المستخدم ({user.FullName}) — اسم المستخدم: {user.Username}",
                Type = NotificationConstants.Types.Warning,
                Category = NotificationConstants.Categories.Security,
                Priority = NotificationConstants.Priorities.High,
                ActorID = deletedByUserId,
                RelatedRecordID = user.UserID
            });
            await _unitOfWork.SaveChangesAsync();
            if (ownsTransaction) await _unitOfWork.CommitAsync();

            return true;
        }
        catch
        {
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword, int resetByUserId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = true;
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserID = id,
                Title = "إعادة تعيين كلمة المرور",
                Message = "تم إعادة تعيين كلمة المرور الخاصة بك. يرجى تغييرها عند تسجيل الدخول التالي.",
                Type = NotificationConstants.Types.Warning,
                Category = NotificationConstants.Categories.Security,
                Priority = NotificationConstants.Priorities.High,
                ActorID = resetByUserId,
                RelatedRecordID = user.UserID
            });
        }
        catch { }

        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, int changedByUserId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserID = userId,
                Title = "تغيير كلمة المرور",
                Message = "تم تغيير كلمة المرور الخاصة بك بنجاح.",
                Type = NotificationConstants.Types.Success,
                Category = NotificationConstants.Categories.Security,
                Priority = NotificationConstants.Priorities.Normal,
                ActorID = changedByUserId,
                RelatedRecordID = user.UserID
            });
        }
        catch { }

        return true;
    }

    public async Task<ApiResponse<object>> GetDebugPermsAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return ApiResponse<object>.Fail("المستخدم غير موجود");

        var legacyRoleId = user.RoleID;
        
        var userRoleAssignments = await _unitOfWork.UserRoleAssignments.Query()
            .AsNoTracking()
            .Where(ura => ura.UserId == userId)
            .Select(ura => new { ura.RoleId, ura.IsActive })
            .ToListAsync();

        var individualPerms = await _unitOfWork.UserPermissions.Query()
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => new { up.PermissionId, up.IsGranted })
            .ToListAsync();

        var legacyRolePerms = await _unitOfWork.RolePermissions.Query()
            .AsNoTracking()
            .Where(rp => rp.RoleId == legacyRoleId)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();

        return ApiResponse<object>.Success(new
        {
            User = new { user.UserID, user.FullName, LegacyRoleID = user.RoleID, user.IsActive },
            LegacyRolePermissions = legacyRolePerms,
            UserRoleAssignments = userRoleAssignments,
            IndividualPermissions = individualPerms
        });
    }

    public async Task<ApiResponse<object>> GetUserPermissionsAsync(int id)
    {
        var perms = await _unitOfWork.UserPermissions.Query()
            .AsNoTracking()
            .Where(up => up.UserId == id)
            .Select(up => new
            {
                up.Id,
                up.PermissionId,
                permissionCode = up.Permission.Code,
                permissionDescription = up.Permission.Description,
                permissionCategory = up.Permission.Category,
                up.IsGranted,
                up.CreatedAt
            })
            .OrderBy(p => p.permissionCode)
            .ToListAsync();

        return ApiResponse<object>.Success(perms);
    }

    public async Task<ApiResponse<object>> SetUserPermissionsAsync(int id, SetUserPermissionsDto dto, int currentUserId)
    {
        var userExists = await _unitOfWork.Users.AnyAsync(u => u.UserID == id);
        if (!userExists)
            return ApiResponse<object>.Fail("المستخدم غير موجود");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // حذف الصلاحيات الفردية القديمة
            var existing = await _unitOfWork.UserPermissions.Query()
                .Where(up => up.UserId == id)
                .ToListAsync();
            _unitOfWork.UserPermissions.DeleteRange(existing);

            // إضافة الصلاحيات الجديدة
            foreach (var item in dto.Permissions)
            {
                await _unitOfWork.UserPermissions.AddAsync(new UserPermission
                {
                    UserId = id,
                    PermissionId = item.PermissionId,
                    IsGranted = item.IsGranted,
                    GrantedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var targetUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (targetUser != null)
            {
                try
                {
                    await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
                    {
                        UserID = 0,
                        Title = "تحديث صلاحيات مستخدم",
                        Message = $"تم تحديث الصلاحيات الفردية للمستخدم ({targetUser.FullName}) — اسم المستخدم: {targetUser.Username}",
                        Type = NotificationConstants.Types.Info,
                        Category = NotificationConstants.Categories.Security,
                        Priority = NotificationConstants.Priorities.Normal,
                        ActorID = currentUserId,
                        RelatedRecordID = id
                    });
                }
                catch { }
            }

            return ApiResponse<object>.Success(null, "تم تحديث صلاحيات المستخدم الفردية");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<object>.Fail("فشل تحديث الصلاحيات");
        }
    }

    // ═══════════════════════════════════════
    // MAPPING
    // ═══════════════════════════════════════

    public async Task<ApiResponse<object>> SetRoleAssignmentsAsync(int id, SetUserRoleAssignmentsDto dto, int currentUserId)
    {
        // Delegating role scopes is a central administrative action, with an explicit policy scope.
        var access = await _access.ForPermissionAsync("System.ManageUsers");
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        if (dto.Assignments is null || dto.Assignments.Count > 100)
            return ApiResponse<object>.Fail("قائمة تعيينات الأدوار غير صالحة");
        foreach (var assignment in dto.Assignments)
        {
            var modes = (assignment.IsCentral ? 1 : 0) + (assignment.BranchId.HasValue ? 1 : 0) + (assignment.RegionId.HasValue ? 1 : 0);
            if (modes != 1 || assignment.RoleId <= 0 || assignment.BranchId is <= 0 || assignment.RegionId is <= 0)
                return ApiResponse<object>.Fail("حدد نطاقاً واحداً صريحاً لكل دور: فرع أو إقليم أو مركزي");
            if (!await _unitOfWork.Roles.AnyAsync(r => r.RoleID == assignment.RoleId))
                return ApiResponse<object>.Fail("الدور غير موجود");
            var branch = assignment.BranchId ?? assignment.RegionId;
            if (branch.HasValue && !await _unitOfWork.PensionFundBranches.AnyAsync(b => b.PensionBranchID == branch && b.IsActive))
                return ApiResponse<object>.Fail("الفرع أو الإقليم غير موجود أو غير فعّال");
        }
        return await PensionMS.Infrastructure.Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user is null || user.IsDeleted) return ApiResponse<object>.Fail("المستخدم غير موجود");
            var previous = await _unitOfWork.UserRoleAssignments.Query().Where(a => a.UserId == id && a.IsActive).ToListAsync();
            var oldValues = System.Text.Json.JsonSerializer.Serialize(previous.Select(a => new { a.RoleId, a.BranchId, a.RegionId }));
            foreach (var assignment in previous) assignment.IsActive = false;
            user.RoleID = dto.Assignments.Select(a => (int?)a.RoleId).FirstOrDefault();
            await _unitOfWork.SaveChangesAsync();
            foreach (var assignment in dto.Assignments.DistinctBy(a => (a.RoleId, a.BranchId, a.RegionId, a.IsCentral)))
                await _unitOfWork.UserRoleAssignments.AddAsync(new UserRoleAssignment
                {
                    UserId = id, RoleId = assignment.RoleId, BranchId = assignment.BranchId,
                    RegionId = assignment.RegionId, IsActive = true, CreatedAt = DateTime.UtcNow
                });
            await _unitOfWork.SaveChangesAsync();
            await _audit.LogAsync(currentUserId, "", "SetRoleAssignments", "User", id, oldValues,
                System.Text.Json.JsonSerializer.Serialize(dto.Assignments));
            return ApiResponse<object>.Success(new { UserId = id }, "تم تحديث تعيينات الأدوار ونطاقاتها");
        });
    }

    private static UserDto MapToDto(User u, List<string> permissions)
    {
        var activeAssignments = u.UserRoleAssignments?
            .Where(a => a.IsActive)
            .ToList() ?? [];

        return new UserDto
        {
            UserID = u.UserID,
            FullName = u.FullName,
            Username = u.Username,
            Email = u.Email,
            RoleID = u.RoleID,
            RoleName = u.Role?.RoleName,
            BranchID = u.BranchID,
            BranchName = u.Branch?.BranchName,
            IsActive = u.IsActive,
            IsLocked = u.IsLocked,
            LastLogin = u.LastLogin,
            CreatedAt = u.CreatedAt,
            Permissions = permissions,
            Assignments = activeAssignments.Select(a => new UserAssignmentDto
            {
                Id = a.Id,
                RoleId = a.RoleId,
                RoleName = a.Role?.RoleName ?? "",
                BranchId = a.BranchId,
                BranchName = a.Branch?.BranchName,
                RegionId = a.RegionId,
                RegionName = a.Region?.BranchName,
                IsCentral = a.BranchId is null && a.RegionId is null,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }
}
