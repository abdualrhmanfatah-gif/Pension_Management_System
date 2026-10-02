namespace PensionMS.Core.Entities;

/// <summary>
/// التحقق عبر OTP (OtpVerification)
/// </summary>
public class OtpVerification
{
    public int OtpVerificationID { get; set; }
    
    public int? CitizenAccountID { get; set; }
    public virtual CitizenAccount? CitizenAccount { get; set; }
    
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpHash { get; set; } = string.Empty;     // مُشفّر
    public string Purpose { get; set; } = string.Empty;      // LOGIN | REGISTER | VERIFY_ACTION
    public bool IsUsed { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
