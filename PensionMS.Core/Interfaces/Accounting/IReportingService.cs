using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IReportingService
{
    Task<ApiResponse<List<TrialBalanceItemDto>>> GetTrialBalanceAsync(int? periodId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<BalanceSheetDto>> GetBalanceSheetAsync(DateTime asOfDate);
    Task<ApiResponse<IncomeStatementDto>> GetIncomeStatementAsync(int fiscalYearId, int? quarterId = null);
    Task<ApiResponse<GeneralLedgerDto>> GetGeneralLedgerAsync(int accountId, int? periodId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<AccountStatementResultDto>> GetAccountStatementAsync(int accountId, DateTime fromDate, DateTime toDate);
    Task<ApiResponse<JournalRegisterDto>> GetJournalRegisterAsync(JournalRegisterFilterDto filter);
    Task<ApiResponse<CashFlowStatementDto>> GetCashFlowStatementAsync(int fiscalYearId);
    Task<ApiResponse<AccountingDashboardDto>> GetDashboardAsync(int? fiscalYearId);
    Task<ApiResponse<AccountBalanceDto>> GetAccountBalanceAsync(int accountId, int periodId);
    Task<ApiResponse<SubledgerReportResultDto>> GetSubledgerReportAsync(int accountId, DateTime? fromDate, DateTime? toDate);
}
