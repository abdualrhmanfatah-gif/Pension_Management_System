namespace PensionMS.Core.Entities;

/// <summary>
/// رمز التحديث (Refresh Token) — يدعم التدوير (Rotation) والتعقب
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    /// <summary>القيمة العشوائية المشفرة (64-byte random)</summary>
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    /// <summary>التوكن الذي استبدل هذا — لتعقب سلسلة التدوير</summary>
    public string? ReplacedByToken { get; set; }
}
