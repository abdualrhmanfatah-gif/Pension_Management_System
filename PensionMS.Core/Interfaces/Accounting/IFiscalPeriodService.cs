using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IFiscalPeriodService
{
    Task<ApiResponse<List<FiscalYearSummaryDto>>> GetAllFiscalYearsAsync();
    Task<ApiResponse<FiscalYearSummaryDto>> CreateFiscalYearAsync(CreateFiscalYearDto dto);
    Task<ApiResponse<List<FiscalPeriodDto>>> GetFiscalPeriodsAsync(int fiscalYearId);
    Task<ApiResponse<FiscalPeriodDto>> GetActivePeriodAsync(int fiscalYearId);
    Task<ApiResponse<List<QuarterDto>>> GetQuartersAsync(int fiscalYearId);
    Task<ApiResponse<QuarterDto>> CloseQuarterAsync(int quarterId, int userId);
    Task<ApiResponse<QuarterDetailsDto>> GetQuarterDetailsAsync(int quarterId);
}
