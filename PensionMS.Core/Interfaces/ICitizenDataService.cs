using PensionMS.Core.DTOs.Citizen;

namespace PensionMS.Core.Interfaces;

public interface ICitizenDataService
{
    // Profile
    Task<CitizenProfileDto?> GetProfileAsync(int retireeId);
    Task<bool> UpdateProfileAsync(int retireeId, UpdateCitizenProfileDto dto);
    
    // File
    Task<CitizenFileDto?> GetFileAsync(int retireeId);
    Task<IEnumerable<CitizenEntitledPersonDto>> GetEntitledPersonsAsync(int retireeId);
    
    // Payments
    Task<IEnumerable<CitizenPaymentDto>> GetPaymentsAsync(int retireeId, int page, int pageSize);
    Task<CitizenPaymentSummaryDto> GetPaymentSummaryAsync(int retireeId);
}
