using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;

namespace PensionMS.Core.Interfaces;

public interface IInvestmentPortfolioService
{
    Task<ApiResponse<int>> CreatePortfolioAsync(CreatePortfolioDto dto, int userId);
    Task<ApiResponse<bool>> UpdatePortfolioAsync(int id, UpdatePortfolioDto dto, int userId);
    Task<ApiResponse<bool>> DeletePortfolioAsync(int id, int userId);
    Task<ApiResponse<PortfolioDto>> GetPortfolioByIdAsync(int id);
    Task<ApiResponse<List<PortfolioDto>>> GetAllPortfoliosAsync();
    Task<ApiResponse<PortfolioSummaryDto>> GetPortfolioSummaryAsync(int id);
    Task<ApiResponse<InvestmentStatsDto>> GetInvestmentStatsAsync();
}
