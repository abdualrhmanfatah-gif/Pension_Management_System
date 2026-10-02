using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.AchievementObstacles;

namespace PensionMS.Core.Interfaces;

public interface IAchievementObstacleService
{
    Task<ApiResponse<PagedResult<AchievementObstacleDto>>> GetPagedAsync(AchievementObstacleFilterDto filter);
    Task<ApiResponse<AchievementObstacleDto>> GetByIdAsync(int id);
    Task<ApiResponse<int>> CreateAsync(CreateAchievementObstacleDto dto, int userId);
    Task<ApiResponse<bool>> UpdateAsync(int id, UpdateAchievementObstacleDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
    Task<ApiResponse<AchievementObstacleStatsDto>> GetStatsAsync(AchievementObstacleFilterDto filter);
    Task<ApiResponse<List<TrackingCommentDto>>> GetCommentsAsync(int recordId);
    Task<ApiResponse<int>> AddCommentAsync(int recordId, string comment, int userId);
    Task<ApiResponse<bool>> DeleteCommentAsync(int commentId, int userId);
}
