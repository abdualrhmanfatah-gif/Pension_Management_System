using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// حساب المتقاعد (CitizenAccount) - لإدارة الدخول للبوابة الإلكترونية
/// </summary>
public class CitizenAccount : ISoftDeletable
{
    public int CitizenAccountID { get; set; }
    
    public int RetireeID { get; set; }
    public virtual Retiree Retiree { get; set; } = null!;
    
    // مصادقة OTP عبر الهاتف
    public string PhoneNumber { get; set; } = string.Empty;  // مطابق لـ Retiree.Phone
    public string NationalID { get; set; } = string.Empty;   // مطابق لـ Retiree.NationalID
    
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }            // تم التحقق من الهوية
    public DateTime? LastLoginAt { get; set; }
    public int FailedOtpAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    
    // Refresh Token للجلسة
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }
}
