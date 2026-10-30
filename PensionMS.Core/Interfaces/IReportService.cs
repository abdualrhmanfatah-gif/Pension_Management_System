using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IReportService
{
    Task<byte[]> ExportRetireesAsync(string? search = null, string format = "excel");
    Task<byte[]> ExportRetirementFilesAsync(int? statusId = null, string format = "excel");
    Task<byte[]> ExportEntitledPersonsAsync(int? fileId = null, string format = "excel");
    Task<byte[]> ExportUsersAsync(string format = "excel");

    // ── Investment (INV-01..07) ──
    Task<ApiResponse<PagedResult<PortfolioPerformanceRow>>> GetPortfolioPerformanceAsync(ReportFilter filter);
    Task<byte[]> ExportPortfolioPerformanceAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<OccupancyRow>>> GetOccupancyReportAsync(ReportFilter filter);
    Task<byte[]> ExportOccupancyReportAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<RentCollectionRow>>> GetRentCollectionAsync(ReportFilter filter);
    Task<byte[]> ExportRentCollectionAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<ContractStatusRow>>> GetContractStatusAsync(ReportFilter filter);
    Task<byte[]> ExportContractStatusAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<RiskExposureRow>>> GetRiskExposureAsync(ReportFilter filter);
    Task<byte[]> ExportRiskExposureAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<MaintenanceSummaryRow>>> GetMaintenanceSummaryAsync(ReportFilter filter);
    Task<byte[]> ExportMaintenanceSummaryAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<BudgetVsActualRow>>> GetBudgetVsActualAsync(ReportFilter filter);
    Task<byte[]> ExportBudgetVsActualAsync(ReportFilter filter, string format = "excel");

    // ── Retirement (RET-01..03) ──
    Task<ApiResponse<PagedResult<FileStatusRow>>> GetFileStatusReportAsync(ReportFilter filter);
    Task<byte[]> ExportFileStatusReportAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<ActiveRetireeRow>>> GetActiveRetireesAsync(ReportFilter filter);
    Task<byte[]> ExportActiveRetireesAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<SuspensionRow>>> GetSuspensionsReportAsync(ReportFilter filter);
    Task<byte[]> ExportSuspensionsReportAsync(ReportFilter filter, string format = "excel");

    // ── Financial (FIN-01..03) ──
    Task<ApiResponse<PagedResult<SalaryBatchRow>>> GetSalaryBatchesAsync(ReportFilter filter);
    Task<byte[]> ExportSalaryBatchesAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<SettlementRow>>> GetSettlementsReportAsync(ReportFilter filter);
    Task<byte[]> ExportSettlementsReportAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<FiscalPeriodRow>>> GetFiscalPeriodReportAsync(ReportFilter filter);
    Task<byte[]> ExportFiscalPeriodReportAsync(ReportFilter filter, string format = "excel");

    // ── Documents (DOC-01..02) ──
    Task<ApiResponse<PagedResult<FileMissingDocsRow>>> GetMissingDocsReportAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<MissingDocsByPersonRow>>> GetMissingDocsByPersonReportAsync(ReportFilter filter);
    Task<byte[]> ExportMissingDocsReportAsync(ReportFilter filter, string format = "excel");
    Task<byte[]> ExportMissingDocsByPersonReportAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<ExpiredDocRow>>> GetExpiredDocsReportAsync(ReportFilter filter);
    Task<byte[]> ExportExpiredDocsReportAsync(ReportFilter filter, string format = "excel");

    // ── Dependents (DEP-01) ──
    Task<ApiResponse<PagedResult<BeneficiaryRow>>> GetBeneficiariesReportAsync(ReportFilter filter);
    Task<byte[]> ExportBeneficiariesReportAsync(ReportFilter filter, string format = "excel");

    // ── Compliance (COM-01..02) ──
    Task<ApiResponse<PagedResult<MinistryBatchRow>>> GetMinistryBatchesAsync(ReportFilter filter);
    Task<byte[]> ExportMinistryBatchesAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<PendingApprovalRow>>> GetPendingApprovalsAsync(ReportFilter filter);
    Task<byte[]> ExportPendingApprovalsAsync(ReportFilter filter, string format = "excel");

    // ── Admin (ADM-01..02) ──
    Task<ApiResponse<PagedResult<AuditLogRow>>> GetAuditLogAsync(ReportFilter filter);
    Task<byte[]> ExportAuditLogAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<UserActivityRow>>> GetUserActivityAsync(ReportFilter filter);
    Task<byte[]> ExportUserActivityAsync(ReportFilter filter, string format = "excel");
}
