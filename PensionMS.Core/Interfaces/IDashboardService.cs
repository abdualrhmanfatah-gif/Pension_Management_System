using System.Threading.Tasks;
using PensionMS.Core.DTOs;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardStatsDto>> GetStatsAsync();
    Task<ApiResponse<PensionStatisticsDto>> GetPensionStatisticsAsync();
    Task<ApiResponse<PerformanceStatisticsDto>> GetPerformanceStatisticsAsync();
    Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync();
}
