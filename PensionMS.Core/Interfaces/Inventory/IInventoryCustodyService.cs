using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IInventoryCustodyService
{
    Task<ApiResponse<List<CustodyDto>>> GetAllAsync();
    Task<ApiResponse<CustodyDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<CustodyDto>>> GetActiveAsync();
    Task<ApiResponse<List<CustodyDto>>> GetByEmployeeAsync(int employeeId);
    Task<ApiResponse<CustodyDto>> CreateAsync(CreateCustodyDto dto, int userId);
    Task<ApiResponse<CustodyDto>> ReturnCustodyAsync(int id, UpdateCustodyDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
}
