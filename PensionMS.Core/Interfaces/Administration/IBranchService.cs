using PensionMS.Core.DTOs.Branches;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IBranchService
{
    Task<List<BranchDto>> GetAllAsync();
    Task<BranchDto?> GetByIdAsync(int id);
    Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchDto dto, int userId);
    Task<ApiResponse<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto, int userId);
    Task<ApiResponse<bool>> ToggleActiveAsync(int id, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
}
