namespace PensionMS.Core.DTOs.Citizen;

public class CitizenRegisterRequest
{
    public string NationalID { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class CitizenVerifyOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class CitizenLoginRequest
{
    public string NationalID { get; set; } = string.Empty;
}

public class CitizenAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
}
