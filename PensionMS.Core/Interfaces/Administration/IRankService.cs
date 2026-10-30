using PensionMS.Core.DTOs.Ranks;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IRankService
{
    Task<List<RankDto>> GetAllAsync();
    Task<RankDto?> GetByIdAsync(int id);
    Task<ApiResponse<RankDto>> CreateAsync(CreateRankDto dto, int userId);
    Task<ApiResponse<RankDto>> UpdateAsync(int id, UpdateRankDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
}
