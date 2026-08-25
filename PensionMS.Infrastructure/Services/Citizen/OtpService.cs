using System.Security.Cryptography;
using System.Text;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Citizen;

public class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyOtp(string plainOtp, string hashedOtp)
    {
        var hashedInput = HashOtp(plainOtp);
        return hashedInput == hashedOtp;
    }
}