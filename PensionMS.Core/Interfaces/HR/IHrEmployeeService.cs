using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;

namespace PensionMS.Core.Interfaces;

public interface IHrEmployeeService
{
    Task<ApiResponse<PagedResult<EmployeeListItemDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, int? cursor = null);
    Task<ApiResponse<EmployeeDetailsDto>> GetByIdAsync(int id);
    Task<ApiResponse<EmployeeDetailsDto>> CreateAsync(CreateEmployeeDto dto, int userId);
    Task<ApiResponse<EmployeeDetailsDto>> UpdateAsync(int id, UpdateEmployeeDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
    Task<ApiResponse<HrStatsDto>> GetStatsAsync();
    Task<ApiResponse<string>> UploadPhotoAsync(int id, Stream fileStream, string fileName, int userId);
    Task<ApiResponse<PagedResult<HrGlobalDocumentDto>>> GetGlobalEmployeeDocumentsAsync(int page = 1, int pageSize = 20, string? search = null);
}
