using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface ICostCenterService
{
    Task<ApiResponse<List<CostCenterDto>>> GetAllCostCentersAsync();
    Task<ApiResponse<CostCenterDto>> GetCostCenterByIdAsync(int id);
    Task<ApiResponse<CostCenterDto>> CreateCostCenterAsync(CreateCostCenterDto dto);
    Task<ApiResponse<CostCenterDto>> UpdateCostCenterAsync(int id, UpdateCostCenterDto dto);
    Task<ApiResponse<bool>> DeactivateCostCenterAsync(int id);
}
