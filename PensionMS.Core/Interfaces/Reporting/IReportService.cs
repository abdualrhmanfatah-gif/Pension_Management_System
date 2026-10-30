using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IReportService
{

    // ── Investment (INV-01..07) ──
    Task<ApiResponse<PagedResult<PortfolioPerformanceRow>>> GetPortfolioPerformanceAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<OccupancyRow>>> GetOccupancyReportAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<RentCollectionRow>>> GetRentCollectionAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<ContractStatusRow>>> GetContractStatusAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<RiskExposureRow>>> GetRiskExposureAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<MaintenanceSummaryRow>>> GetMaintenanceSummaryAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<BudgetVsActualRow>>> GetBudgetVsActualAsync(ReportFilter filter);

    // ── Retirement (RET-01..03) ──
    Task<ApiResponse<PagedResult<FileStatusRow>>> GetFileStatusReportAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<ActiveRetireeRow>>> GetActiveRetireesAsync(ReportFilter filter);
    Task<ApiResponse<ActiveRetireesSummaryDto>> GetActiveRetireesSummaryAsync(ReportFilter filter);
    Task<byte[]> ExportActiveRetireesAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<SuspensionRow>>> GetSuspensionsReportAsync(ReportFilter filter);

    // ── Financial (FIN-01..03) ──
    Task<ApiResponse<PagedResult<SalaryBatchRow>>> GetSalaryBatchesAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<SettlementRow>>> GetSettlementsReportAsync(ReportFilter filter);
    Task<ApiResponse<SettlementSummaryDto>> GetSettlementsSummaryAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<FiscalPeriodRow>>> GetFiscalPeriodReportAsync(ReportFilter filter);

    // ── Documents (DOC-01..02) ──
    Task<ApiResponse<PagedResult<FileMissingDocsRow>>> GetMissingDocsReportAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<MissingDocsByPersonRow>>> GetMissingDocsByPersonReportAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<ExpiredDocRow>>> GetExpiredDocsReportAsync(ReportFilter filter);

    // ── Dependents (DEP-01) ──
    Task<ApiResponse<PagedResult<BeneficiaryRow>>> GetBeneficiariesReportAsync(ReportFilter filter);

    // ── Kashf (كشوفات الإحالة) ──
    Task<ApiResponse<PagedResult<KashfReportRow>>> GetKashfReportAsync(ReportFilter filter);
    Task<ApiResponse<object>> GetKashfReportSummaryAsync(ReportFilter filter);

    // ── Compliance (COM-01..02) ──
    Task<ApiResponse<PagedResult<MinistryBatchRow>>> GetMinistryBatchesAsync(ReportFilter filter);
    Task<byte[]> ExportMinistryBatchesAsync(ReportFilter filter, string format = "excel");
    Task<ApiResponse<PagedResult<PendingApprovalRow>>> GetPendingApprovalsAsync(ReportFilter filter);

    // ── Admin (ADM-01..02) ──
    Task<ApiResponse<PagedResult<AuditLogRow>>> GetAuditLogAsync(ReportFilter filter);
    Task<ApiResponse<PagedResult<UserActivityRow>>> GetUserActivityAsync(ReportFilter filter);
}
