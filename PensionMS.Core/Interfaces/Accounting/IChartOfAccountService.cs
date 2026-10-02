using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IChartOfAccountService
{
    Task<ApiResponse<List<ChartOfAccountDto>>> GetAllAccountsAsync(int? level = null);
    Task<ApiResponse<ChartOfAccountDto>> GetAccountByIdAsync(int id);
    Task<ApiResponse<ChartOfAccountDto>> GetAccountByCodeAsync(string code);
    Task<ApiResponse<List<ChartOfAccountDto>>> GetAccountTreeAsync(int? parentId = null);
    Task<ApiResponse<List<ChartOfAccountDto>>> GetPostableAccountsAsync();
    Task<ApiResponse<ChartOfAccountDto>> CreateAccountAsync(CreateChartOfAccountDto dto);
    Task<ApiResponse<ChartOfAccountDto>> UpdateAccountAsync(int id, UpdateChartOfAccountDto dto);
    Task<ApiResponse<bool>> DeactivateAccountAsync(int id);
    Task<ApiResponse<bool>> ReactivateAccountAsync(int id);
    Task<ApiResponse<bool>> ResetChartOfAccountsAsync();
    Task<ApiResponse<bool>> ResetAllDataAsync();
    Task<ApiResponse<int>> DisableUnusedAccountsAsync();
}
