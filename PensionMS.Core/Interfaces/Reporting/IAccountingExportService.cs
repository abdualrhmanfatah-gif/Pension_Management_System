using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Reporting;

public interface IAccountingExportService
{
    Task<byte[]> ExportTrialBalanceAsync(ApiResponse<List<TrialBalanceItemDto>> data, string format, int? accountLevel = null);
    Task<byte[]> ExportBalanceSheetAsync(ApiResponse<BalanceSheetDto> data, string format);
    Task<byte[]> ExportIncomeStatementAsync(ApiResponse<IncomeStatementDto> data, string format);
    Task<byte[]> ExportGeneralLedgerAsync(ApiResponse<GeneralLedgerDto> data, string format);
    Task<byte[]> ExportAccountStatementAsync(ApiResponse<AccountStatementResultDto> data, string format);
    Task<byte[]> ExportSubledgerAsync(ApiResponse<SubledgerReportResultDto> data, string format);
    Task<byte[]> ExportJournalRegisterAsync(ApiResponse<JournalRegisterDto> data, string format);
    Task<byte[]> ExportCashFlowAsync(ApiResponse<CashFlowStatementDto> data, string format);
    Task<byte[]> ExportQuarterlyAsync(ApiResponse<IncomeStatementDto> data, string format, string quarterName);
    Task<byte[]> ExportBudgetAsync(ApiResponse<BudgetReportDto> data, string format);
    Task<byte[]> ExportQuarterPeriodsAsync(ApiResponse<QuarterDetailsDto> data);
    Task<byte[]> ExportQuarterExpensesAsync(ApiResponse<QuarterDetailsDto> data);
}
