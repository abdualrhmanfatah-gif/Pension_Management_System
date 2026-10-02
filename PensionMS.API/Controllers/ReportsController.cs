using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Reporting;
using PensionMS.API.Extensions;
using PensionMS.Infrastructure.Reporting;
using System;
using System.Threading.Tasks;
using ReportFilter = PensionMS.Core.DTOs.Reports.ReportFilter;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IReportRegistry _reportRegistry;
    private readonly IReportDataService _reportDataService;
    private readonly PdfReportExporter _pdfExporter;

    public ReportsController(
        IReportService reportService,
        IReportRegistry reportRegistry,
        IReportDataService reportDataService,
        PdfReportExporter pdfExporter)
    {
        _reportService = reportService;
        _reportRegistry = reportRegistry;
        _reportDataService = reportDataService;
        _pdfExporter = pdfExporter;
    }

    private IActionResult ReturnFile(byte[] bytes, string format, string baseName)
    {
        if (format.ToLower() == ExportFormatConstants.Pdf)
            return File(bytes, "application/pdf", $"{baseName}_{PensionMS.Core.Time.YemenTime.Now:yyyyMMdd}.pdf");

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{baseName}_{PensionMS.Core.Time.YemenTime.Now:yyyyMMdd}.xlsx");
    }

    private async Task<IActionResult> ExportWithFormatAsync(string reportId, string format, string baseName, ReportFilter filter)
    {
        if (format.ToLower() == ExportFormatConstants.Pdf)
        {
            var definition = _reportRegistry.GetById(reportId);
            if (definition == null)
                return BadRequest("تقرير غير موجود");

            var data = await _reportDataService.GetDataAsync(reportId, filter);
            var pdfBytes = _pdfExporter.Export(definition, data);
            return ReturnFile(pdfBytes, format, baseName);
        }

        return await ExportExcelAsync(reportId, filter, baseName);
    }

    private async Task<IActionResult> ExportExcelAsync(string reportId, ReportFilter filter, string baseName)
    {
        byte[] bytes = reportId switch
        {
            "basic-retirees" => await _reportService.ExportRetireesAsync(filter.Search),
            "basic-retirement-files" => await _reportService.ExportRetirementFilesAsync(filter.StatusId),
            "basic-entitled-persons" => await _reportService.ExportEntitledPersonsAsync(filter.StatusId),
            "basic-users" => await _reportService.ExportUsersAsync(),
            "inv-01" => await _reportService.ExportPortfolioPerformanceAsync(filter),
            "inv-02" => await _reportService.ExportOccupancyReportAsync(filter),
            "inv-03" => await _reportService.ExportRentCollectionAsync(filter),
            "inv-04" => await _reportService.ExportContractStatusAsync(filter),
            "inv-05" => await _reportService.ExportRiskExposureAsync(filter),
            "inv-06" => await _reportService.ExportMaintenanceSummaryAsync(filter),
            "inv-07" => await _reportService.ExportBudgetVsActualAsync(filter),
            "ret-01" => await _reportService.ExportFileStatusReportAsync(filter),
            "ret-02" => await _reportService.ExportActiveRetireesAsync(filter),
            "ret-03" => await _reportService.ExportSuspensionsReportAsync(filter),
            "fin-01" => await _reportService.ExportSalaryBatchesAsync(filter),
            "fin-02" => await _reportService.ExportSettlementsReportAsync(filter),
            "fin-03" => await _reportService.ExportFiscalPeriodReportAsync(filter),
            "doc-01" => await _reportService.ExportMissingDocsReportAsync(filter),
            "doc-01b" => await _reportService.ExportMissingDocsByPersonReportAsync(filter),
            "doc-02" => await _reportService.ExportExpiredDocsReportAsync(filter),
            "dep-01" => await _reportService.ExportBeneficiariesReportAsync(filter),
            "kashf-01" => await _reportService.ExportKashfReportAsync(filter),
            "com-02" => await _reportService.ExportPendingApprovalsAsync(filter),
            "adm-01" => await _reportService.ExportAuditLogAsync(filter),
            "adm-02" => await _reportService.ExportUserActivityAsync(filter),
            _ => throw new InvalidOperationException($"Unknown report: {reportId}")
        };
        return ReturnFile(bytes, "excel", baseName);
    }

    // ── Basic Entity Exports ──

    [HttpGet("retirees")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> ExportRetirees([FromQuery] string? search = null, [FromQuery] string format = "excel")
    {
        var filter = new ReportFilter { Search = search };
        return await ExportWithFormatAsync("basic-retirees", format, "Retirees", filter);
    }

    [HttpGet("retirement-files")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> ExportRetirementFiles([FromQuery] int? statusId = null, [FromQuery] string format = "excel")
    {
        var filter = new ReportFilter { StatusId = statusId };
        return await ExportWithFormatAsync("basic-retirement-files", format, "RetirementFiles", filter);
    }

    [HttpGet("entitled-persons")]
    [Authorize(Policy = "Report.Entitlements.View")]
    public async Task<IActionResult> ExportEntitledPersons([FromQuery] int? fileId = null, [FromQuery] string format = "excel")
    {
        var filter = new ReportFilter { StatusId = fileId };
        return await ExportWithFormatAsync("basic-entitled-persons", format, "EntitledPersons", filter);
    }

    [HttpGet("users")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> ExportUsers([FromQuery] string format = "excel")
    {
        var filter = new ReportFilter();
        return await ExportWithFormatAsync("basic-users", format, "Users", filter);
    }

    // ── Investment Reports ──

    [HttpGet("investment/portfolio-performance")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetPortfolioPerformance([FromQuery] ReportFilter filter) => (await _reportService.GetPortfolioPerformanceAsync(filter)).ToActionResult();

    [HttpGet("investment/portfolio-performance/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportPortfolioPerformance([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("inv-01", format, "PortfolioPerformance", filter);

    [HttpGet("investment/occupancy")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetOccupancy([FromQuery] ReportFilter filter) => (await _reportService.GetOccupancyReportAsync(filter)).ToActionResult();

    [HttpGet("investment/occupancy/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportOccupancy([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("inv-02", format, "Occupancy", filter);

    [HttpGet("investment/rent-collection")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetRentCollection([FromQuery] ReportFilter filter) => (await _reportService.GetRentCollectionAsync(filter)).ToActionResult();

    [HttpGet("investment/rent-collection/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportRentCollection([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("inv-03", format, "RentCollection", filter);

    [HttpGet("investment/contract-status")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetContractStatus([FromQuery] ReportFilter filter) => (await _reportService.GetContractStatusAsync(filter)).ToActionResult();

    [HttpGet("investment/contract-status/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportContractStatus([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("inv-04", format, "ContractStatus", filter);

    [HttpGet("investment/risk-exposure")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetRiskExposure([FromQuery] ReportFilter filter) => (await _reportService.GetRiskExposureAsync(filter)).ToActionResult();

    [HttpGet("investment/risk-exposure/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportRiskExposure([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("inv-05", format, "RiskExposure", filter);

    [HttpGet("investment/maintenance-summary")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetMaintenanceSummary([FromQuery] ReportFilter filter) => (await _reportService.GetMaintenanceSummaryAsync(filter)).ToActionResult();

    [HttpGet("investment/maintenance-summary/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportMaintenanceSummary([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("inv-06", format, "MaintenanceSummary", filter);

    [HttpGet("investment/budget-vs-actual")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetBudgetVsActual([FromQuery] ReportFilter filter) => (await _reportService.GetBudgetVsActualAsync(filter)).ToActionResult();

    [HttpGet("investment/budget-vs-actual/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportBudgetVsActual([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("inv-07", format, "BudgetVsActual", filter);

    // ── Retirement Reports ──

    [HttpGet("retirement/file-status")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> GetFileStatusReport([FromQuery] ReportFilter filter) => (await _reportService.GetFileStatusReportAsync(filter)).ToActionResult();

    [HttpGet("retirement/file-status/export")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> ExportFileStatusReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("ret-01", format, "FileStatus", filter);

    [HttpGet("retirement/active-retirees")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> GetActiveRetirees([FromQuery] ReportFilter filter) => (await _reportService.GetActiveRetireesAsync(filter)).ToActionResult();

    [HttpGet("retirement/active-retirees/export")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> ExportActiveRetirees([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("ret-02", format, "ActiveRetirees", filter);

    [HttpGet("retirement/suspensions")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> GetSuspensionsReport([FromQuery] ReportFilter filter) => (await _reportService.GetSuspensionsReportAsync(filter)).ToActionResult();

    [HttpGet("retirement/suspensions/export")]
    [Authorize(Policy = "Report.Pension.View")]
    public async Task<IActionResult> ExportSuspensionsReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("ret-03", format, "Suspensions", filter);

    // ── Financial Reports ──

    [HttpGet("financial/salary-batches")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetSalaryBatches([FromQuery] ReportFilter filter) => (await _reportService.GetSalaryBatchesAsync(filter)).ToActionResult();

    [HttpGet("financial/salary-batches/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportSalaryBatches([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("fin-01", format, "SalaryBatches", filter);

    [HttpGet("financial/settlements")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetSettlementsReport([FromQuery] ReportFilter filter) => (await _reportService.GetSettlementsReportAsync(filter)).ToActionResult();

    [HttpGet("financial/settlements/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportSettlementsReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("fin-02", format, "Settlements", filter);

    [HttpGet("financial/fiscal-period")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> GetFiscalPeriodReport([FromQuery] ReportFilter filter) => (await _reportService.GetFiscalPeriodReportAsync(filter)).ToActionResult();

    [HttpGet("financial/fiscal-period/export")]
    [Authorize(Policy = "Report.Financial.View")]
    public async Task<IActionResult> ExportFiscalPeriodReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("fin-03", format, "FiscalPeriod", filter);

    // ── Documents Reports ──

    [HttpGet("documents/missing")]
    [Authorize(Policy = "Report.Documents.View")]
    public async Task<IActionResult> GetMissingDocsReport([FromQuery] ReportFilter filter) => (await _reportService.GetMissingDocsReportAsync(filter)).ToActionResult();

    [HttpGet("documents/missing/export")]
    [Authorize(Policy = "Report.Documents.View")]
    public async Task<IActionResult> ExportMissingDocsReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("doc-01", format, "MissingDocs", filter);

    [HttpGet("documents/missing-by-person")]
    [Authorize(Policy = "Report.Documents.View")]
    public async Task<IActionResult> GetMissingDocsByPersonReport([FromQuery] ReportFilter filter) => (await _reportService.GetMissingDocsByPersonReportAsync(filter)).ToActionResult();

    [HttpGet("documents/missing-by-person/export")]
    [Authorize(Policy = "Report.Documents.View")]
    public async Task<IActionResult> ExportMissingDocsByPersonReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("doc-01b", format, "MissingDocsByPerson", filter);

    [HttpGet("documents/expired")]
    [Authorize(Policy = "Report.Documents.View")]
    public async Task<IActionResult> GetExpiredDocsReport([FromQuery] ReportFilter filter) => (await _reportService.GetExpiredDocsReportAsync(filter)).ToActionResult();

    [HttpGet("documents/expired/export")]
    [Authorize(Policy = "Report.Documents.View")]
    public async Task<IActionResult> ExportExpiredDocsReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("doc-02", format, "ExpiredDocs", filter);

    // ── Dependents Reports ──

    [HttpGet("dependents/beneficiaries")]
    [Authorize(Policy = "Report.Entitlements.View")]
    public async Task<IActionResult> GetBeneficiariesReport([FromQuery] ReportFilter filter) => (await _reportService.GetBeneficiariesReportAsync(filter)).ToActionResult();

    [HttpGet("dependents/beneficiaries/export")]
    [Authorize(Policy = "Report.Entitlements.View")]
    public async Task<IActionResult> ExportBeneficiariesReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("dep-01", format, "Beneficiaries", filter);

    // ── Kashf Reports (كشوفات الإحالة) ──

    [HttpGet("kashf/report")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetKashfReport([FromQuery] ReportFilter filter) => (await _reportService.GetKashfReportAsync(filter)).ToActionResult();

    [HttpGet("kashf/report/summary")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetKashfReportSummary([FromQuery] ReportFilter filter) => (await _reportService.GetKashfReportSummaryAsync(filter)).ToActionResult();

    [HttpGet("kashf/report/export")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> ExportKashfReport([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("kashf-01", format, "KashfReport", filter);

    // ── Compliance Reports ──

    [HttpGet("compliance/ministry-batches")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> GetMinistryBatches([FromQuery] ReportFilter filter) => (await _reportService.GetMinistryBatchesAsync(filter)).ToActionResult();

    [HttpGet("compliance/ministry-batches/export")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> ExportMinistryBatches([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
    {
        var bytes = await _reportService.ExportMinistryBatchesAsync(filter);
        return ReturnFile(bytes, "excel", "MinistryBatches");
    }

    [HttpGet("compliance/pending-approvals")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] ReportFilter filter) => (await _reportService.GetPendingApprovalsAsync(filter)).ToActionResult();

    [HttpGet("compliance/pending-approvals/export")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> ExportPendingApprovals([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("com-02", format, "PendingApprovals", filter);

    // ── Admin Reports ──

    [HttpGet("admin/audit-log")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> GetAuditLog([FromQuery] ReportFilter filter) => (await _reportService.GetAuditLogAsync(filter)).ToActionResult();

    [HttpGet("admin/audit-log/export")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> ExportAuditLog([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("adm-01", format, "AuditLog", filter);

    [HttpGet("admin/user-activity")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> GetUserActivity([FromQuery] ReportFilter filter) => (await _reportService.GetUserActivityAsync(filter)).ToActionResult();

    [HttpGet("admin/user-activity/export")]
    [Authorize(Policy = "Report.Admin.View")]
    public async Task<IActionResult> ExportUserActivity([FromQuery] ReportFilter filter, [FromQuery] string format = "excel")
        => await ExportWithFormatAsync("adm-02", format, "UserActivity", filter);
}
