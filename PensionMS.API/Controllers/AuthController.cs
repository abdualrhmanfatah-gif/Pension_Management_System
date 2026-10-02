using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.API.Extensions;
using PensionMS.API.Helpers;
using PensionMS.Core.DTOs.Auth;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Users;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IScopedPermissionResolver _permissionResolver;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    private const string MeCachePrefix = "auth_me_";
    private const string PermsCachePrefix = "auth_perms_";
    private static readonly TimeSpan AuthCacheDuration = TimeSpan.FromMinutes(5);

    public AuthController(IAuthService authService, IScopedPermissionResolver permissionResolver, IConfiguration configuration, IMemoryCache cache)
    {
        _authService = authService;
        _permissionResolver = permissionResolver;
        _configuration = configuration;
        _cache = cache;
    }

    private bool IsHttps => Request.IsHttps;

    private void SetRefreshTokenCookie(string refreshToken, int expiryDays)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = IsHttps,
            SameSite = IsHttps ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = "/api/auth",
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(expiryDays > 0 ? expiryDays : 7)
        };
        Response.Cookies.Append("RefreshToken", refreshToken, options);
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete("RefreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = IsHttps,
            SameSite = IsHttps ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = "/api/auth"
        });
    }

    private void SetJwtCookie(string token)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = IsHttps,
            SameSite = IsHttps ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromMinutes(GetJwtExpiryMinutes())
        };
        Response.Cookies.Append("jwt", token, options);
    }

    private void ClearJwtCookie()
    {
        Response.Cookies.Delete("jwt", new CookieOptions
        {
            HttpOnly = true,
            Secure = IsHttps,
            SameSite = IsHttps ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = "/"
        });
    }

    private int GetJwtExpiryMinutes()
    {
        return int.TryParse(_configuration["JwtSettings:ExpiryInMinutes"], out var m) ? m : 60;
    }

    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(AuthResponse.Fail("يرجى إدخال اسم المستخدم وكلمة المرور"));

        if (string.IsNullOrWhiteSpace(request.IPAddress))
        {
            request.IPAddress = RequestHelper.GetClientIp(HttpContext);
        }

        var result = await _authService.LoginAsync(request);

        if (!result.IsSuccess)
            return Unauthorized(result);

        SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiryDays);
        SetJwtCookie(result.Token!);
        return Ok(result);
    }

    /// <summary>
    /// تحديث رمز الوصول — يقرأ refresh token من httpOnly cookie إن وُجد،
    /// وإلا يسقط إلى الـ request body (للتوافق مع العملاء القدامى)
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest body)
    {
        var cookieToken = Request.Cookies["RefreshToken"];

        if (!string.IsNullOrEmpty(cookieToken))
        {
            var result = await _authService.RefreshFromCookieAsync(cookieToken);

            if (result.IsSuccess)
            {
                SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiryDays);
                SetJwtCookie(result.Token!);
                return Ok(result);
            }

            ClearRefreshTokenCookie();
        }

        if (string.IsNullOrWhiteSpace(body.Token) || string.IsNullOrWhiteSpace(body.RefreshToken))
            return BadRequest(AuthResponse.Fail("يرجى إرسال بيانات التحديث"));

        var bodyResult = await _authService.RefreshTokenAsync(body);

        if (!bodyResult.IsSuccess)
            return Unauthorized(bodyResult);

        SetRefreshTokenCookie(bodyResult.RefreshToken!, bodyResult.RefreshTokenExpiryDays);
        SetJwtCookie(bodyResult.Token!);
        return Ok(bodyResult);
    }

    /// <summary>
    /// تسجيل الخروج (إلغاء رمز التحديث)
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized(AuthResponse.Fail("غير مصرح"));

        var cookieToken = Request.Cookies["RefreshToken"];
        if (!string.IsNullOrEmpty(cookieToken))
            await _authService.RevokeByValueAsync(cookieToken);

        await _authService.RevokeTokenAsync(userId);
        ClearRefreshTokenCookie();
        ClearJwtCookie();
        return ApiResponse<object>.Success(null, "تم تسجيل الخروج بنجاح").ToActionResult();
    }

    /// <summary>
    /// جلب معلومات المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
            return Unauthorized(ApiResponse<object>.Fail("غير مصرح"));

        var cacheKey = MeCachePrefix + userId;
        if (_cache.TryGetValue(cacheKey, out object? cached))
            return Ok(cached);

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("المستخدم غير موجود"));

        var result = ApiResponse<object>.Success(new
        {
            id = user.UserID,
            fullName = user.FullName,
            username = user.Username,
            email = user.Email,
            role = user.Role?.RoleName ?? "Unknown",
            roleId = user.RoleID,
            branchId = user.BranchID,
            branchName = user.Branch?.BranchName,
            mustChangePassword = user.MustChangePassword,
            createdAt = user.CreatedAt,
            lastLogin = user.LastLogin,
            isActive = user.IsActive
        });

        _cache.Set(cacheKey, result, AuthCacheDuration);

        return result.ToActionResult();
    }

    /// <summary>
    /// جلب قائمة الصلاحيات الفعالة للمستخدم الحالي من قاعدة البيانات
    /// </summary>
    [HttpGet("me/permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
            return Unauthorized(ApiResponse<object>.Fail("غير مصرح"));

        // ⚠️ No cache — permissions must always be fresh (security-critical)
        var permissions = await _permissionResolver.GetUserPermissionsAsync(userId);
        var result = ApiResponse<object>.Success(permissions);

        return result.ToActionResult();
    }

    /// <summary>
    /// تغيير كلمة المرور
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
            return Unauthorized(AuthResponse.Fail("غير مصرح"));

        var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, userId);
        if (!result)
            return BadRequest(AuthResponse.Fail("كلمة المرور الحالية غير صحيحة أو كلمة المرور الجديدة غير صالحة"));

        return ApiResponse<object>.Success(null, "تم تغيير كلمة المرور بنجاح").ToActionResult();
    }
}
