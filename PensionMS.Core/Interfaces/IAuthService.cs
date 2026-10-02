using PensionMS.Core.DTOs.Auth;

namespace PensionMS.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>تحديث التوكن عبر الـ request body (للتوافق القديم)</summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>تحديث التوكن عبر httpOnly cookie — يُستخدم بعد نقل refresh token إلى cookie</summary>
    Task<AuthResponse> RefreshFromCookieAsync(string refreshToken);

    /// <summary>جلب معلومات المستخدم بالمعرف</summary>
    Task<PensionMS.Core.Entities.User?> GetUserByIdAsync(int userId);

    /// <summary>إلغاء توكن التحديث الخاص بمستخدم</summary>
    Task<bool> RevokeTokenAsync(int userId);

    /// <summary>إلغاء توكن تحديث بقيمته — للاستخدام مع الـ cookie</summary>
    Task<bool> RevokeByValueAsync(string refreshToken);

    /// <summary>تغيير كلمة المرور</summary>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, int changedByUserId);
}
