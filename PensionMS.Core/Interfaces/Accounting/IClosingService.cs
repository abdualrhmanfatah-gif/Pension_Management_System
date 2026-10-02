using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IClosingService
{
    Task<ApiResponse<ClosingResultDto>> CloseFiscalYearAsync(int fiscalYearId, int userId);
    Task<ApiResponse<OpeningBalanceResultDto>> CreateOpeningBalancesAsync(int newFiscalYearId, int userId);
    Task<ApiResponse<OpeningBalanceResultDto>> CreateInitialOpeningBalancesAsync(InitialOpeningBalanceRequestDto request, int userId);
    Task<ApiResponse<InitialOpeningBalanceResponseDto>> GetInitialOpeningBalancesAsync(int fiscalYearId);
    Task<ApiResponse<SeedInitialDefaultsResultDto>> SeedInitialDefaultsAsync(int fiscalYearId, int userId);
}
