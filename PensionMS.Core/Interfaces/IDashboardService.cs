using System.Threading.Tasks;
using PensionMS.Core.DTOs;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardStatsDto>> GetStatsAsync();
}
