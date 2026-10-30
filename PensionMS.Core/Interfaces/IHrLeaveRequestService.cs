using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;

namespace PensionMS.Core.Interfaces;

public interface IHrLeaveRequestService
{
    Task<ApiResponse<PagedResult<LeaveRequestListItemDto>>> GetAllAsync(int page = 1, int pageSize = 20, int? employeeId = null, string? search = null, string? status = null);
    Task<ApiResponse<LeaveRequestDetailsDto>> GetByIdAsync(int id);
    Task<ApiResponse<LeaveRequestDetailsDto>> CreateAsync(CreateLeaveRequestDto dto);
    Task<ApiResponse<LeaveRequestDetailsDto>> ProcessAsync(int id, ProcessLeaveRequestDto dto);
    Task<ApiResponse<LeaveRequestStatsDto>> GetStatsAsync();
}
