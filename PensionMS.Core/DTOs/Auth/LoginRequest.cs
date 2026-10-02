namespace PensionMS.Core.DTOs.Auth;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DeviceID { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public string? IPAddress { get; set; }
}
