using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PensionMS.Core.DTOs.Citizen;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PensionMS.Infrastructure.Services.Citizen;

public class CitizenAuthService : ICitizenAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;

    public CitizenAuthService(
        IUnitOfWork unitOfWork, 
        IConfiguration configuration, 
        IOtpService otpService, 
        ISmsService smsService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _otpService = otpService;
        _smsService = smsService;
    }

    public async Task<bool> RequestRegistrationOtpAsync(CitizenRegisterRequest request)
    {
        // 1. Check if Retiree exists with this NationalID and Phone
        var retiree = await _unitOfWork.Retirees.FirstOrDefaultAsync(r => r.NationalID == request.NationalID && r.Phone == request.PhoneNumber);
        if (retiree == null) return false;

        // 2. Generate OTP
        var otp = _otpService.GenerateOtp();
        var otpHash = _otpService.HashOtp(otp);

        // 3. Save OtpVerification
        var otpVerification = new OtpVerification
        {
            PhoneNumber = request.PhoneNumber,
            OtpHash = otpHash,
            Purpose = "REGISTER",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };
        await _unitOfWork.OtpVerifications.AddAsync(otpVerification);
        await _unitOfWork.SaveChangesAsync();

        // 4. Send SMS
        await _smsService.SendSmsAsync(request.PhoneNumber, $"رمز التحقق الخاص بك هو: {otp}");

        return true;
    }

    public async Task<bool> RequestLoginOtpAsync(string nationalID)
    {
        var account = await _unitOfWork.CitizenAccounts.FirstOrDefaultAsync(a => a.NationalID == nationalID && a.IsActive);
        if (account == null) return false;

        var otp = _otpService.GenerateOtp();
        var otpHash = _otpService.HashOtp(otp);

        var otpVerification = new OtpVerification
        {
            CitizenAccountID = account.CitizenAccountID,
            PhoneNumber = account.PhoneNumber,
            OtpHash = otpHash,
            Purpose = "LOGIN",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };
        await _unitOfWork.OtpVerifications.AddAsync(otpVerification);
        await _unitOfWork.SaveChangesAsync();

        await _smsService.SendSmsAsync(account.PhoneNumber, $"رمز التحقق للدخول هو: {otp}");

        return true;
    }

    public async Task<CitizenAuthResponse?> VerifyOtpAndLoginAsync(CitizenVerifyOtpRequest request)
    {
        var otpVerification = await _unitOfWork.OtpVerifications.FirstOrDefaultAsync(o => 
            o.PhoneNumber == request.PhoneNumber && 
            !o.IsUsed && 
            o.ExpiresAt > DateTime.UtcNow);

        if (otpVerification == null || !_otpService.VerifyOtp(request.Otp, otpVerification.OtpHash))
            return null;

        otpVerification.IsUsed = true;
        
        // Find or create account if purpose was REGISTER
        CitizenAccount? account;
        if (otpVerification.Purpose == "REGISTER")
        {
            var retiree = await _unitOfWork.Retirees.FirstOrDefaultAsync(r => r.Phone == request.PhoneNumber);
            if (retiree == null) return null;

            account = await _unitOfWork.CitizenAccounts.FirstOrDefaultAsync(a => a.RetireeID == retiree.RetireeID);
            if (account == null)
            {
                account = new CitizenAccount
                {
                    RetireeID = retiree.RetireeID,
                    PhoneNumber = retiree.Phone!,
                    NationalID = retiree.NationalID,
                    IsVerified = true,
                    IsActive = true
                };
                await _unitOfWork.CitizenAccounts.AddAsync(account);
            }
        }
        else
        {
            account = await _unitOfWork.CitizenAccounts.GetByIdAsync(otpVerification.CitizenAccountID!.Value);
        }

        if (account == null) return null;

        account.LastLoginAt = DateTime.UtcNow;
        
        var token = GenerateJwtToken(account);
        var refreshToken = GenerateRefreshToken();
        
        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        
        await _unitOfWork.SaveChangesAsync();

        return new CitizenAuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiry = DateTime.UtcNow.AddMinutes(60)
        };
    }

    private string GenerateJwtToken(CitizenAccount account)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "DefaultSecretKeyForCitizenPortal2026!");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.CitizenAccountID.ToString()),
            new Claim("RetireeID", account.RetireeID.ToString()),
            new Claim(ClaimTypes.Role, "Citizen")
        };

        var key = new SymmetricSecurityKey(secretKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "PensionMS.CitizenAPI",
            audience: jwtSettings["Audience"] ?? "PensionMS.Citizens",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
