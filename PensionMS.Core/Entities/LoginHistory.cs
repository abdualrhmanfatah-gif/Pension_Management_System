namespace PensionMS.Core.Entities;

/// <summary>
/// سجل تسجيلات الدخول
/// </summary>
public class LoginHistory
{
    public long LoginID { get; set; }

    public int? UserID { get; set; }
    public virtual User? User { get; set; }

    public string AttemptedUsername { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? DeviceID { get; set; }
}
