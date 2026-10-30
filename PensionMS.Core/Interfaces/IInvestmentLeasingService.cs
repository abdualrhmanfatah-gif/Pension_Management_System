using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;

namespace PensionMS.Core.Interfaces;

public interface IInvestmentLeasingService
{
    // Tenants
    Task<ApiResponse<int>> CreateTenantAsync(CreateTenantDto dto, int userId);
    Task<ApiResponse<bool>> UpdateTenantAsync(int id, UpdateTenantDto dto, int userId);
    Task<ApiResponse<bool>> DeleteTenantAsync(int id, int userId);
    Task<ApiResponse<TenantDto>> GetTenantByIdAsync(int id);
    Task<ApiResponse<List<PropertyDto>>> GetPropertiesByTenantAsync(int tenantId);
    Task<ApiResponse<List<TenantDto>>> GetAllTenantsAsync();

    // Lease Contracts
    Task<ApiResponse<int>> CreateLeaseContractAsync(CreateLeaseDto dto, int userId);
    Task<ApiResponse<bool>> UpdateLeaseContractAsync(int id, UpdateLeaseDto dto, int userId);
    Task<ApiResponse<bool>> DeleteLeaseContractAsync(int id, int userId);
    Task<ApiResponse<LeaseContractDto>> GetLeaseContractByIdAsync(int id);
    Task<ApiResponse<List<LeaseContractDto>>> GetLeasesByPropertyAsync(int propertyId);
    Task<ApiResponse<List<LeaseContractDto>>> GetAllLeasesAsync();
    Task<ApiResponse<bool>> UpdateLeaseContractFilePathAsync(int id, string filePath, int userId);

    // Payment Schedules
    Task<ApiResponse<int>> CreatePaymentScheduleAsync(CreatePaymentScheduleDto dto, int userId);
    Task<ApiResponse<bool>> UpdatePaymentStatusAsync(int scheduleId, UpdatePaymentStatusDto dto, int userId);
    Task<ApiResponse<List<PaymentScheduleDto>>> GetSchedulesByContractAsync(int contractId);
    Task<ApiResponse<List<PaymentScheduleDto>>> GetAllSchedulesAsync();
}
