using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Auth;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Users;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PensionMS.Infrastructure.Services.Security;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null)
        {
            _logger.LogWarning("محاولة تسجيل دخول فاشلة للمستخدم: {Username} (المستخدم غير موجود أو غير نشط)", request.Username);
            return AuthResponse.Fail("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            _logger.LogWarning("محاولة دخول لحساب مقفل: {Username} حتى {LockedUntil}", user.Username, user.LockedUntil);
            return AuthResponse.Fail($"الحساب مقفل حتى {user.LockedUntil:yyyy-MM-dd HH:mm}");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                _logger.LogCritical("تم إقفال حساب المستخدم {Username} بسبب محاولات دخول فاشلة متكررة", user.Username);

                // إرسال إشعار لمدراء النظام
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = 1, // مسؤول النظام (يمكن تحسينها لاحقاً لترسل لكل من يملك صلاحية System.Admin)
                    Title = "تنبيه أمني: إقفال حساب",
                    Message = $"تم إقفال حساب المستخدم ({user.Username}) تلقائياً بسبب تكرار محاولات الدخول الفاشلة.",
                    Type = NotificationConstants.Types.Error,
                    Category = NotificationConstants.Categories.Security,
                    Priority = NotificationConstants.Priorities.High,
                    CreatedAt = DateTime.UtcNow,
                    ActorID = user.UserID
                });
            }
            await _unitOfWork.SaveChangesAsync();
            return AuthResponse.Fail("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        // Reset failed login attempts on successful login
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockedUntil = null;
        user.LastLogin = DateTime.UtcNow;

        // Generate tokens
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

        // Save refresh token (both old column and new table for migration)
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());
        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.UserID,
            Token = refreshToken,
            ExpiresAt = user.RefreshTokenExpiryTime.Value,
            CreatedAt = DateTime.UtcNow
        });

        // Handle Device Registration
        if (!string.IsNullOrEmpty(request.DeviceID))
        {
            var device = await _unitOfWork.DeviceRegistrations.Query()
                .FirstOrDefaultAsync(d => d.DeviceID == request.DeviceID);

            if (device == null)
            {
                device = new DeviceRegistration
                {
                    DeviceID = request.DeviceID,
                    UserID = user.UserID,
                    DeviceName = request.DeviceName,
                    DeviceType = request.DeviceType,
                    LastKnownIP = request.IPAddress,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.DeviceRegistrations.AddAsync(device);
            }
            else
            {
                device.UserID = user.UserID;
                if (!string.IsNullOrEmpty(request.DeviceName)) device.DeviceName = request.DeviceName;
                if (!string.IsNullOrEmpty(request.DeviceType)) device.DeviceType = request.DeviceType;
                if (!string.IsNullOrEmpty(request.IPAddress)) device.LastKnownIP = request.IPAddress;
                device.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Log login
        await _unitOfWork.LoginHistories.AddAsync(new LoginHistory
        {
            UserID = user.UserID,
            LoginTime = DateTime.UtcNow,
            IsSuccessful = true,
            DeviceID = request.DeviceID,
            IPAddress = request.IPAddress
        });

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("تسجيل دخول ناجح للمستخدم: {Username} (ID: {UserID})", user.Username, user.UserID);

        return AuthResponse.Success(
            token,
            refreshToken,
            expiry,
            user.UserID,
            user.FullName,
            user.Username,
            user.Role?.RoleName ?? "Unknown",
            user.RoleID,
            user.BranchID,
            user.Branch?.BranchName,
            user.MustChangePassword,
            GetRefreshTokenExpiryDays()
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            return AuthResponse.Fail("رمز الوصول غير صالح");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return AuthResponse.Fail("رمز الوصول غير صالح");

        // 1. Search RefreshTokens table first (supports rotation tracking)
        var stored = await _unitOfWork.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (stored != null)
        {
            // Grace period: السماح للتوكن المُلغى خلال 30 ثانية (لدعم التبويبات المتعددة)
            if (stored.IsRevoked)
            {
                if (stored.RevokedAt.HasValue && (DateTime.UtcNow - stored.RevokedAt.Value).TotalSeconds <= 30
                    && !string.IsNullOrEmpty(stored.ReplacedByToken))
                {
                    // استخدام التوكن البديل — تبويب آخر جدد التوكن مؤخراً
                    var replacement = await _unitOfWork.RefreshTokens.Query()
                        .FirstOrDefaultAsync(rt => rt.Token == stored.ReplacedByToken && !rt.IsRevoked);
                    if (replacement != null)
                    {
                        stored = replacement; // استخدام التوكن الجديد
                    }
                    else
                    {
                        return AuthResponse.Fail("رمز التحديث ملغي");
                    }
                }
                else
                {
                    return AuthResponse.Fail("رمز التحديث ملغي");
                }
            }

            if (stored.ExpiresAt <= DateTime.UtcNow)
                return AuthResponse.Fail("رمز التحديث منتهي الصلاحية");

            var user = await _unitOfWork.Users.Query()
                .Include(u => u.Role)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.UserID == stored.UserId && u.IsActive);

            if (user == null)
            {
                stored.IsRevoked = true;
                stored.RevokedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                return AuthResponse.Fail("المستخدم غير موجود أو غير نشط");
            }

            // Rotate: generate new tokens, revoke old
            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var expiry = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByToken = newRefreshToken;

            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.UserID,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
                CreatedAt = DateTime.UtcNow
            });

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

            await _unitOfWork.SaveChangesAsync();

            return AuthResponse.Success(
                newToken,
                newRefreshToken,
                expiry,
                user.UserID,
                user.FullName,
                user.Username,
                user.Role?.RoleName ?? "Unknown",
                user.RoleID,
                user.BranchID,
                user.Branch?.BranchName,
                user.MustChangePassword,
                GetRefreshTokenExpiryDays()
            );
        }

        // 2. Fallback: check legacy User.RefreshToken field (pre-migration tokens)
        var legacyUser = await _unitOfWork.Users.Query()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.UserID == userId && u.IsActive);

        if (legacyUser == null)
            return AuthResponse.Fail("المستخدم غير موجود");

        if (legacyUser.RefreshToken != request.RefreshToken)
            return AuthResponse.Fail("رمز التحديث غير صالح");

        if (legacyUser.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return AuthResponse.Fail("رمز التحديث منتهي الصلاحية");

        // Revoke old token in RefreshTokens table if it exists there
        var oldRecord = await _unitOfWork.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);
        if (oldRecord != null)
        {
            oldRecord.IsRevoked = true;
            oldRecord.RevokedAt = DateTime.UtcNow;
        }

        // Generate new tokens
        var newJwt = GenerateJwtToken(legacyUser);
        var newRefresh = GenerateRefreshToken();
        var tokenExpiry = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

        legacyUser.RefreshToken = newRefresh;
        legacyUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = legacyUser.UserID,
            Token = newRefresh,
            ExpiresAt = legacyUser.RefreshTokenExpiryTime.Value,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        return AuthResponse.Success(
            newJwt,
            newRefresh,
            tokenExpiry,
            legacyUser.UserID,
            legacyUser.FullName,
            legacyUser.Username,
            legacyUser.Role?.RoleName ?? "Unknown",
            legacyUser.RoleID,
            legacyUser.BranchID,
            legacyUser.Branch?.BranchName,
            legacyUser.MustChangePassword,
            GetRefreshTokenExpiryDays()
        );
    }

    public async Task<bool> RevokeTokenAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        // Revoke all active refresh tokens for this user
        var activeTokens = await _unitOfWork.RefreshTokens.Query()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<AuthResponse> RefreshFromCookieAsync(string refreshToken)
    {
        var stored = await _unitOfWork.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (stored == null)
            return AuthResponse.Fail("رمز التحديث غير موجود");

        // Grace period: السماح للتوكن المُلغى خلال 30 ثانية (لدعم التبويبات المتعددة)
        if (stored.IsRevoked)
        {
            if (stored.RevokedAt.HasValue && (DateTime.UtcNow - stored.RevokedAt.Value).TotalSeconds <= 30
                && !string.IsNullOrEmpty(stored.ReplacedByToken))
            {
                var replacement = await _unitOfWork.RefreshTokens.Query()
                    .FirstOrDefaultAsync(rt => rt.Token == stored.ReplacedByToken && !rt.IsRevoked);
                if (replacement != null)
                {
                    stored = replacement;
                }
                else
                {
                    return AuthResponse.Fail("رمز التحديث ملغي");
                }
            }
            else
            {
                return AuthResponse.Fail("رمز التحديث ملغي");
            }
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            return AuthResponse.Fail("رمز التحديث منتهي الصلاحية");

        var user = await _unitOfWork.Users.Query()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.UserID == stored.UserId && u.IsActive);

        if (user == null)
        {
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return AuthResponse.Fail("المستخدم غير موجود أو غير نشط");
        }

        // Rotate: generate new tokens, revoke old
        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByToken = newRefreshToken;

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.UserID,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
            CreatedAt = DateTime.UtcNow
        });

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

        await _unitOfWork.SaveChangesAsync();

        return AuthResponse.Success(
            newToken,
            newRefreshToken,
            expiry,
            user.UserID,
            user.FullName,
            user.Username,
            user.Role?.RoleName ?? "Unknown",
            user.RoleID,
            user.BranchID,
            user.Branch?.BranchName,
            user.MustChangePassword,
            GetRefreshTokenExpiryDays()
        );
    }

    public async Task<bool> RevokeByValueAsync(string refreshToken)
    {
        var stored = await _unitOfWork.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (stored == null) return false;

        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _unitOfWork.Users.Query()
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.UserID == userId);
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

        // Send notification about password change
        try
        {
            await _unitOfWork.Notifications.AddAsync(new Notification
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

    // ═══════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));


        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new("BranchId", (user.BranchID ?? 0).ToString()),
            new(ClaimTypes.Role, user.Role?.RoleName ?? "Unknown")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes()),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // Allow expired tokens for refresh
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidAudience = _configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private int GetTokenExpiryMinutes() =>
        int.TryParse(_configuration["JwtSettings:ExpiryInMinutes"], out var m) ? m : 60;

    private int GetRefreshTokenExpiryDays() =>
        int.TryParse(_configuration["JwtSettings:RefreshTokenExpiryInDays"], out var d) ? d : 7;
}
