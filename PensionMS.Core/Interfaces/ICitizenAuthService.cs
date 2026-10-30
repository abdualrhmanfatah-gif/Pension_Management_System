using PensionMS.Core.DTOs.Citizen;

namespace PensionMS.Core.Interfaces;

public interface ICitizenAuthService
{
    Task<bool> RequestRegistrationOtpAsync(CitizenRegisterRequest request);
    Task<bool> RequestLoginOtpAsync(string nationalID);
    Task<CitizenAuthResponse?> VerifyOtpAndLoginAsync(CitizenVerifyOtpRequest request);
}
