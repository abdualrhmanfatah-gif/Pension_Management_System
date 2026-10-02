using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PensionMS.API.Extensions;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Infrastructure.Reporting;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/accounting")]
[Authorize]
public class AccountingController : ControllerBase
{
    private readonly IChartOfAccountService _chartOfAccount;
    private readonly IJournalEntryService _journalEntry;
    private readonly ICostCenterService _costCenter;
    private readonly IReportingService _reporting;
    private readonly IFiscalPeriodService _fiscalPeriod;
    private readonly IClosingService _closing;
    private readonly IAccountingExportService _exportService;

    public AccountingController(
        IChartOfAccountService chartOfAccount,
        IJournalEntryService journalEntry,
        ICostCenterService costCenter,
        IReportingService reporting,
        IFiscalPeriodService fiscalPeriod,
        IClosingService closing,
        IAccountingExportService exportService)
    {
        _chartOfAccount = chartOfAccount;
        _journalEntry = journalEntry;
        _costCenter = costCenter;
        _reporting = reporting;
        _fiscalPeriod = fiscalPeriod;
        _closing = closing;
        _exportService = exportService;
    }

    private IActionResult ReturnFile(byte[] bytes, string format, string baseName)
    {
        if (format.ToLower() == ExportFormatConstants.Pdf)
            return File(bytes, "application/pdf", $"{baseName}_{PensionMS.Core.Time.YemenTime.Now:yyyyMMdd}.pdf");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{baseName}_{PensionMS.Core.Time.YemenTime.Now:yyyyMMdd}.xlsx");
    }

    // ═══════════════════════════════════════════
    // CHART OF ACCOUNTS
    // ═══════════════════════════════════════════

    [HttpGet("accounts")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAllAccounts([FromQuery] int? level)
    {
        var result = await _chartOfAccount.GetAllAccountsAsync(level);
        return result.ToActionResult();
    }

    [HttpGet("accounts/{id:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAccountById(int id)
    {
        var result = await _chartOfAccount.GetAccountByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("accounts/by-code/{code}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAccountByCode(string code)
    {
        var result = await _chartOfAccount.GetAccountByCodeAsync(code);
        return result.ToActionResult();
    }

    [HttpGet("accounts/tree")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAccountTree([FromQuery] int? parentId)
    {
        var result = await _chartOfAccount.GetAccountTreeAsync(parentId);
        return result.ToActionResult();
    }

    [HttpGet("accounts/postable")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetPostableAccounts()
    {
        var result = await _chartOfAccount.GetPostableAccountsAsync();
        return result.ToActionResult();
    }

    [HttpPost("accounts")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateChartOfAccountDto dto)
    {
        var result = await _chartOfAccount.CreateAccountAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("accounts/{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateChartOfAccountDto dto)
    {
        var result = await _chartOfAccount.UpdateAccountAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("accounts/{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> DeactivateAccount(int id)
    {
        var result = await _chartOfAccount.DeactivateAccountAsync(id);
        return result.ToActionResult();
    }

    [HttpPut("accounts/{id:int}/activate")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> ReactivateAccount(int id)
    {
        var result = await _chartOfAccount.ReactivateAccountAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("accounts/reset")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> ResetChartOfAccounts()
    {
        var result = await _chartOfAccount.ResetChartOfAccountsAsync();
        return result.ToActionResult();
    }

    [HttpPost("reset-all")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> ResetAllData()
    {
        var result = await _chartOfAccount.ResetAllDataAsync();
        return result.ToActionResult();
    }

    [HttpPost("accounts/disable-unused")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> DisableUnusedAccounts()
    {
        var result = await _chartOfAccount.DisableUnusedAccountsAsync();
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // COST CENTERS
    // ═══════════════════════════════════════════

    [HttpGet("cost-centers")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAllCostCenters()
    {
        var result = await _costCenter.GetAllCostCentersAsync();
        return result.ToActionResult();
    }

    [HttpGet("cost-centers/{id:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetCostCenterById(int id)
    {
        var result = await _costCenter.GetCostCenterByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("cost-centers")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterDto dto)
    {
        var result = await _costCenter.CreateCostCenterAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("cost-centers/{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> UpdateCostCenter(int id, [FromBody] UpdateCostCenterDto dto)
    {
        var result = await _costCenter.UpdateCostCenterAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("cost-centers/{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> DeactivateCostCenter(int id)
    {
        var result = await _costCenter.DeactivateCostCenterAsync(id);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // JOURNAL ENTRIES
    // ═══════════════════════════════════════════

    [HttpGet("journal-entries")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetJournalEntries([FromQuery] JournalEntryFilterDto filter)
    {
        var result = await _journalEntry.GetJournalEntriesAsync(filter);
        return result.ToActionResult();
    }

    [HttpGet("journal-entries/{id:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetJournalEntryById(int id)
    {
        var result = await _journalEntry.GetJournalEntryByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("journal-entries")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryDto dto)
    {
        var result = await _journalEntry.CreateJournalEntryAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("journal-entries/{id:int}/post")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> PostJournalEntry(int id)
    {
        var result = await _journalEntry.PostJournalEntryAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("journal-entries/{id:int}/reverse")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> ReverseJournalEntry(int id, [FromBody] ReverseJournalEntryRequest request)
    {
        var result = await _journalEntry.ReverseJournalEntryAsync(id, this.GetUserId(), request.Reason);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════════

    [HttpGet("reports/trial-balance")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] int? periodId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _reporting.GetTrialBalanceAsync(periodId, fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("reports/balance-sheet")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate)
    {
        var result = await _reporting.GetBalanceSheetAsync(asOfDate);
        return result.ToActionResult();
    }

    [HttpGet("reports/income-statement")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] int fiscalYearId, [FromQuery] int? quarterId = null)
    {
        var result = await _reporting.GetIncomeStatementAsync(fiscalYearId, quarterId);
        return result.ToActionResult();
    }

    [HttpGet("reports/general-ledger")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetGeneralLedger(
        [FromQuery] int accountId, [FromQuery] int? periodId,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _reporting.GetGeneralLedgerAsync(accountId, periodId, fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("reports/account-statement")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetAccountStatement(
        [FromQuery] int accountId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _reporting.GetAccountStatementAsync(accountId, fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("reports/subledger")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetSubledgerReport(
        [FromQuery] int accountId,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _reporting.GetSubledgerReportAsync(accountId, fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("reports/journal-register")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetJournalRegister([FromQuery] JournalRegisterFilterDto filter)
    {
        var result = await _reporting.GetJournalRegisterAsync(filter);
        return result.ToActionResult();
    }

    [HttpGet("reports/cash-flow")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetCashFlowStatement([FromQuery] int fiscalYearId)
    {
        var result = await _reporting.GetCashFlowStatementAsync(fiscalYearId);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // REPORT EXPORTS
    // ═══════════════════════════════════════════

    [HttpGet("reports/trial-balance/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportTrialBalance(
        [FromQuery] int? periodId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetTrialBalanceAsync(periodId, fromDate, toDate);
        var bytes = await _exportService.ExportTrialBalanceAsync(data, format);
        return ReturnFile(bytes, format, "TrialBalance");
    }

    [HttpGet("reports/balance-sheet/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportBalanceSheet([FromQuery] DateTime asOfDate, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetBalanceSheetAsync(asOfDate);
        var bytes = await _exportService.ExportBalanceSheetAsync(data, format);
        return ReturnFile(bytes, format, "BalanceSheet");
    }

    [HttpGet("reports/income-statement/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportIncomeStatement([FromQuery] int fiscalYearId, [FromQuery] int? quarterId = null, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetIncomeStatementAsync(fiscalYearId, quarterId);
        var bytes = await _exportService.ExportIncomeStatementAsync(data, format);
        return ReturnFile(bytes, format, "IncomeStatement");
    }

    [HttpGet("reports/general-ledger/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportGeneralLedger(
        [FromQuery] int accountId, [FromQuery] int? periodId,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetGeneralLedgerAsync(accountId, periodId, fromDate, toDate);
        var bytes = await _exportService.ExportGeneralLedgerAsync(data, format);
        return ReturnFile(bytes, format, "GeneralLedger");
    }

    [HttpGet("reports/account-statement/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportAccountStatement(
        [FromQuery] int accountId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetAccountStatementAsync(accountId, fromDate, toDate);
        var bytes = await _exportService.ExportAccountStatementAsync(data, format);
        return ReturnFile(bytes, format, "AccountStatement");
    }

    [HttpGet("reports/subledger/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportSubledger(
        [FromQuery] int accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetSubledgerReportAsync(accountId, fromDate, toDate);
        var bytes = await _exportService.ExportSubledgerAsync(data, format);
        return ReturnFile(bytes, format, "Subledger");
    }

    [HttpGet("reports/journal-register/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportJournalRegister([FromQuery] JournalRegisterFilterDto filter, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetJournalRegisterAsync(filter);
        var bytes = await _exportService.ExportJournalRegisterAsync(data, format);
        return ReturnFile(bytes, format, "JournalRegister");
    }

    [HttpGet("reports/cash-flow/export")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> ExportCashFlow([FromQuery] int fiscalYearId, [FromQuery] string format = "excel")
    {
        var data = await _reporting.GetCashFlowStatementAsync(fiscalYearId);
        var bytes = await _exportService.ExportCashFlowAsync(data, format);
        return ReturnFile(bytes, format, "CashFlow");
    }

    // ═══════════════════════════════════════════
    // DASHBOARD
    // ═══════════════════════════════════════════

    [HttpGet("dashboard")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetDashboard([FromQuery] int? fiscalYearId)
    {
        var result = await _reporting.GetDashboardAsync(fiscalYearId);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // ACCOUNT BALANCES
    // ═══════════════════════════════════════════

    [HttpGet("balances")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetAccountBalance(
        [FromQuery] int accountId, [FromQuery] int periodId)
    {
        var result = await _reporting.GetAccountBalanceAsync(accountId, periodId);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // FISCAL YEARS & PERIODS
    // ═══════════════════════════════════════════

    [HttpGet("fiscal-years")]
    public async Task<IActionResult> GetFiscalYears()
    {
        var result = await _fiscalPeriod.GetAllFiscalYearsAsync();
        return result.ToActionResult();
    }

    [HttpPost("fiscal-years")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CreateFiscalYear([FromBody] CreateFiscalYearDto dto)
    {
        var result = await _fiscalPeriod.CreateFiscalYearAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("periods")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetFiscalPeriods([FromQuery] int fiscalYearId)
    {
        var result = await _fiscalPeriod.GetFiscalPeriodsAsync(fiscalYearId);
        return result.ToActionResult();
    }

    [HttpGet("periods/active")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetActivePeriod([FromQuery] int fiscalYearId)
    {
        var result = await _fiscalPeriod.GetActivePeriodAsync(fiscalYearId);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // QUARTERS
    // ═══════════════════════════════════════════

    [HttpGet("quarters")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetQuarters([FromQuery] int fiscalYearId)
    {
        var result = await _fiscalPeriod.GetQuartersAsync(fiscalYearId);
        return result.ToActionResult();
    }

    [HttpPost("close-quarter")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CloseQuarter([FromBody] CloseQuarterRequestDto dto)
    {
        var result = await _fiscalPeriod.CloseQuarterAsync(dto.QuarterID, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("quarters/{quarterId}/details")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetQuarterDetails(int quarterId)
    {
        var result = await _fiscalPeriod.GetQuarterDetailsAsync(quarterId);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // YEAR-END CLOSING
    // ═══════════════════════════════════════════

    [HttpPost("close-year")]
    [Authorize(Policy = "Accounting.CloseYear")]
    [EnableRateLimiting("CloseYearEndpoint")]
    public async Task<IActionResult> CloseFiscalYear([FromBody] YearEndCloseDto dto)
    {
        var result = await _closing.CloseFiscalYearAsync(dto.FiscalYearID, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("opening-balances")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CreateOpeningBalances([FromBody] OpeningBalanceDto dto)
    {
        var result = await _closing.CreateOpeningBalancesAsync(dto.NewFiscalYearID, this.GetUserId());
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════════
    // INITIAL OPENING BALANCES (الأرصدة الافتتاحية الأولية)
    // ═══════════════════════════════════════════

    [HttpPost("initial-opening-balances")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CreateInitialOpeningBalances([FromBody] InitialOpeningBalanceRequestDto dto)
    {
        var result = await _closing.CreateInitialOpeningBalancesAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("initial-opening-balances/{fiscalYearId}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetInitialOpeningBalances(int fiscalYearId)
    {
        var result = await _closing.GetInitialOpeningBalancesAsync(fiscalYearId);
        return result.ToActionResult();
    }

    [HttpPost("initial-opening-balances/seed")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> SeedInitialDefaults([FromBody] SeedInitialDefaultsRequestDto dto)
    {
        var result = await _closing.SeedInitialDefaultsAsync(dto.FiscalYearID, this.GetUserId());
        return result.ToActionResult();
    }
}
