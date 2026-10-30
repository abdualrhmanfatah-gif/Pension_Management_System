using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;

namespace PensionMS.Core.Interfaces;

public interface IHrServiceHistoryService
{
    Task<ApiResponse<PagedResult<ServiceHistoryEventDto>>> GetGlobalHistoryAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<ApiResponse<List<ServiceHistoryEventDto>>> GetByEmployeeIdAsync(int employeeId);
    Task<ApiResponse<ServiceHistoryEventDto>> CreateEventAsync(CreateServiceHistoryEventDto dto, int userId);
    Task<ApiResponse<ServiceHistoryEventDto>> UpdateEventAsync(int id, CreateServiceHistoryEventDto dto, int userId);
    Task<ApiResponse<bool>> DeleteEventAsync(int id, int userId);
}
