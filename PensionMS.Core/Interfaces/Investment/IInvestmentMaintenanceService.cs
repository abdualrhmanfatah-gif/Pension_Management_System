using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;

namespace PensionMS.Core.Interfaces;

public interface IInvestmentMaintenanceService
{
    // Maintenance Requests
    Task<ApiResponse<int>> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto, int userId);
    Task<ApiResponse<bool>> UpdateMaintenanceRequestAsync(int id, UpdateMaintenanceRequestDto dto, int userId);
    Task<ApiResponse<MaintenanceRequestDto>> GetMaintenanceRequestByIdAsync(int id);
    Task<ApiResponse<List<MaintenanceRequestDto>>> GetRequestsByPropertyAsync(int propertyId);
    Task<ApiResponse<List<MaintenanceRequestDto>>> GetAllMaintenanceRequestsAsync();

    // Work Orders
    Task<ApiResponse<int>> CreateWorkOrderAsync(CreateWorkOrderDto dto, int userId);
    Task<ApiResponse<bool>> UpdateWorkOrderAsync(int id, UpdateWorkOrderDto dto, int userId);
    Task<ApiResponse<WorkOrderDto>> GetWorkOrderByIdAsync(int id);
    Task<ApiResponse<List<WorkOrderDto>>> GetWorkOrdersByRequestAsync(int requestId);
}
