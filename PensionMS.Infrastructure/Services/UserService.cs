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

namespace PensionMS.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public UserService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

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
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission);

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Role!.RoleName ?? "")
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = users.Select(MapToDto).ToList();
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
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserID == id);

        return user == null ? null : MapToDto(user);
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
            if (dto.RoleID.HasValue)
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

            // ═══ CRITICAL FIX: Save OLD values BEFORE updating ═══
            int? oldRoleId = user.RoleID;
            int? oldBranchId = user.BranchID;

            // Now update to new values
            if (dto.RoleID.HasValue) user.RoleID = dto.RoleID.Value;
            if (dto.BranchID.HasValue) user.BranchID = dto.BranchID.Value;

            // Detect actual changes using OLD values
            bool roleChanged = dto.RoleID.HasValue && dto.RoleID.Value != oldRoleId;
            bool branchChanged = dto.BranchID.HasValue && dto.BranchID.Value != oldBranchId;

            if (roleChanged || branchChanged)
            {
                // Deactivate old assignments that match the OLD role
                var oldAssignments = user.UserRoleAssignments
                    .Where(a => a.IsActive && a.RoleId == (oldRoleId ?? 0))
                    .ToList();

                foreach (var old in oldAssignments)
                {
                    old.IsActive = false;
                    _unitOfWork.UserRoleAssignments.Update(old);
                }

                // Create new assignment with the NEW values
                if (user.RoleID.HasValue)
                {
                    await _unitOfWork.UserRoleAssignments.AddAsync(new UserRoleAssignment
                    {
                        UserId = user.UserID,
                        RoleId = user.RoleID.Value,
                        BranchId = user.BranchID,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

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
        catch
        {
            await _unitOfWork.RollbackAsync();
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(int id, int deletedByUserId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserRoleAssignments)
            .FirstOrDefaultAsync(u => u.UserID == id);

        if (user == null) return false;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ═══ حذف نهائي ═══

            // 1) حذف صلاحيات المستخدم الفردية
            var userPerms = await _unitOfWork.UserPermissions.Query()
                .Where(p => p.UserId == id).ToListAsync();
            _unitOfWork.UserPermissions.DeleteRange(userPerms);

            // 2) حذف تعيينات الأدوار
            _unitOfWork.UserRoleAssignments.DeleteRange(user.UserRoleAssignments);

            // 3) حذف سجلات تسجيل الدخول
            var loginHistories = await _unitOfWork.LoginHistories.Query()
                .Where(l => l.UserID == id).ToListAsync();
            _unitOfWork.LoginHistories.DeleteRange(loginHistories);

            // 4) حذف التوكنات المحدثة
            var refreshTokens = await _unitOfWork.RefreshTokens.Query()
                .Where(r => r.UserId == id).ToListAsync();
            _unitOfWork.RefreshTokens.DeleteRange(refreshTokens);

            // 5) حذف الأجهزة المسجلة
            var devices = await _unitOfWork.DeviceRegistrations.Query()
                .Where(d => d.UserID == id).ToListAsync();
            _unitOfWork.DeviceRegistrations.DeleteRange(devices);

            // 6) حذف الإشعارات المرتبطة بالمستخدم
            var notifications = await _unitOfWork.Notifications.Query()
                .Where(n => n.UserID == id).ToListAsync();
            _unitOfWork.Notifications.DeleteRange(notifications);

            // 7) حذف المستخدم نفسه
            _unitOfWork.Users.Delete(user);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            try
            {
                await _notificationService.NotifyRoleAsync("Admin", new CreateNotificationDto
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
            }
            catch { }

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
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

    private static UserDto MapToDto(User u)
    {
        var activeAssignments = u.UserRoleAssignments?
            .Where(a => a.IsActive)
            .ToList() ?? [];

        // جمع كل الصلاحيات من كل التعيينات النشطة (أدوار)
        var rolePermCodes = activeAssignments
            .Where(a => a.Role?.RolePermissions != null)
            .SelectMany(a => a.Role.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission.Code)
            .Distinct();

        // صلاحيات فردية للمستخدم
        var userPerms = u.UserPermissions?.ToList() ?? [];
        var grantedCodes = userPerms.Where(p => p.IsGranted && p.Permission != null).Select(p => p.Permission.Code);
        var revokedCodes = userPerms.Where(p => !p.IsGranted && p.Permission != null).Select(p => p.Permission.Code).ToHashSet();

        var permissions = rolePermCodes
            .Union(grantedCodes)
            .Where(code => !revokedCodes.Contains(code))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

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
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }
}
