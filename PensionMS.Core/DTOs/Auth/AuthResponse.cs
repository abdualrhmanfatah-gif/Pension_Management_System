namespace PensionMS.Core.DTOs.Auth;

public class AuthResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiry { get; set; }

    // User info
    public int? UserID { get; set; }
    public string? FullName { get; set; }
    public string? Username { get; set; }
    public string? RoleName { get; set; }
    public int? RoleID { get; set; }
    public int? BranchID { get; set; }
    public string? BranchName { get; set; }
    public bool MustChangePassword { get; set; }

    /// <summary>عدد أيام صلاحية refresh token — يُستخدم لـ httpOnly cookie MaxAge</summary>
    public int RefreshTokenExpiryDays { get; set; }

    public static AuthResponse Success(string token, string refreshToken, DateTime expiry, int userId, string fullName, string username, string roleName, int? roleId = null, int? branchId = null, string? branchName = null, bool mustChangePassword = false, int refreshTokenExpiryDays = 7)
    {
        return new AuthResponse
        {
            IsSuccess = true,
            Message = "تم تسجيل الدخول بنجاح",
            Token = token,
            RefreshToken = refreshToken,
            TokenExpiry = expiry,
            UserID = userId,
            FullName = fullName,
            Username = username,
            RoleName = roleName,
            RoleID = roleId,
            BranchID = branchId,
            BranchName = branchName,
            MustChangePassword = mustChangePassword,
            RefreshTokenExpiryDays = refreshTokenExpiryDays
        };
    }

    public static AuthResponse Fail(string message)
    {
        return new AuthResponse
        {
            IsSuccess = false,
            Message = message
        };
    }
}
