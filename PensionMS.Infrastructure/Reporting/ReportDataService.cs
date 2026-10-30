using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Reporting;
using ReportFilterDto = PensionMS.Core.DTOs.Reports.ReportFilter;

namespace PensionMS.Infrastructure.Reporting;

public interface IReportDataService
{
    Task<object[]> GetDataAsync(string reportId, ReportFilterDto filter);
}

public class ReportDataService : IReportDataService
{
    private readonly IReportService _reportService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Dictionary<string, Func<ReportFilterDto, Task<object[]>>> _resolvers;

    public ReportDataService(IReportService reportService, IUnitOfWork unitOfWork)
    {
        _reportService = reportService;
        _unitOfWork = unitOfWork;

        _resolvers = new Dictionary<string, Func<ReportFilterDto, Task<object[]>>>
        {
            // ── Basic (direct DB queries) ──
            ["basic-retirees"] = async f => await _unitOfWork.Retirees.Query()
                .AsNoTracking()
                .Include(r => r.SocialStatus)
                .Include(r => r.HealthStatus)
                .Where(r => !r.IsDeleted)
                .Where(r => string.IsNullOrEmpty(f.Search) || r.FullName.Contains(f.Search) || r.NationalID.Contains(f.Search))
                .OrderBy(r => r.FullName)
                .Select(r => new { r.FullName, r.NationalID, r.Governorate, r.Phone,
                    SocialStatusName = r.SocialStatus != null ? r.SocialStatus.StatusName : "",
                    HealthStatusName = r.HealthStatus != null ? r.HealthStatus.HealthStatusName : "",
                    r.CreatedAt })
                .ToListAsync().ContinueWith(t => t.Result.Cast<object>().ToArray()),

            ["basic-retirement-files"] = async f => await _unitOfWork.RetirementFiles.Query()
                .AsNoTracking()
                .Include(r => r.Retiree)
                .Include(r => r.Status)
                .Include(r => r.Rank)
                .Include(r => r.PensionBranch)
                .Where(r => !r.IsDeleted)
                .Where(r => !f.StatusId.HasValue || r.StatusID == f.StatusId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new { r.FileNumber, RetireeName = r.Retiree != null ? r.Retiree.FullName : "",
                    RankName = r.Rank != null ? r.Rank.RankName : "",
                    BranchName = r.PensionBranch != null ? r.PensionBranch.BranchName : "",
                    StatusName = r.Status != null ? r.Status.StatusName : "",
                    r.FileOpenDate, r.CreatedAt })
                .ToListAsync().ContinueWith(t => t.Result.Cast<object>().ToArray()),

            ["basic-entitled-persons"] = async f => await _unitOfWork.EntitledPersons.Query()
                .AsNoTracking()
                .Include(e => e.RetirementFile)
                .Include(e => e.Relationship)
                .Include(e => e.SocialStatus)
                .Include(e => e.HealthStatus)
                .Where(e => !e.IsDeleted)
                .Where(e => !f.StatusId.HasValue || e.FileID == f.StatusId.Value)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.FullName, e.NationalID,
                    RelationshipName = e.Relationship != null ? e.Relationship.RelationshipName : "",
                    FileNumber = e.RetirementFile != null ? e.RetirementFile.FileNumber : "",
                    SocialStatusName = e.SocialStatus != null ? e.SocialStatus.StatusName : "",
                    e.Phone })
                .ToListAsync().ContinueWith(t => t.Result.Cast<object>().ToArray()),

            ["basic-users"] = async f => await _unitOfWork.Users.Query()
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Branch)
                .OrderBy(u => u.FullName)
                .Select(u => new { u.FullName, u.Username, u.Email,
                    RoleName = u.Role != null ? u.Role.RoleName : "",
                    BranchName = u.Branch != null ? u.Branch.BranchName : "",
                    u.IsActive, u.LastLogin })
                .ToListAsync().ContinueWith(t => t.Result.Cast<object>().ToArray()),

            // ── Investment (via ReportService) ──
            ["inv-01"] = async f => await GetPagedDataAsync<PortfolioPerformanceRow>(
                _reportService.GetPortfolioPerformanceAsync, f),
            ["inv-02"] = async f => await GetPagedDataAsync<OccupancyRow>(
                _reportService.GetOccupancyReportAsync, f),
            ["inv-03"] = async f => await GetPagedDataAsync<RentCollectionRow>(
                _reportService.GetRentCollectionAsync, f),
            ["inv-04"] = async f => await GetPagedDataAsync<ContractStatusRow>(
                _reportService.GetContractStatusAsync, f),
            ["inv-05"] = async f => await GetPagedDataAsync<RiskExposureRow>(
                _reportService.GetRiskExposureAsync, f),
            ["inv-06"] = async f => await GetPagedDataAsync<MaintenanceSummaryRow>(
                _reportService.GetMaintenanceSummaryAsync, f),
            ["inv-07"] = async f => await GetPagedDataAsync<BudgetVsActualRow>(
                _reportService.GetBudgetVsActualAsync, f),

            // ── Retirement (via ReportService) ──
            ["ret-01"] = async f => await GetPagedDataAsync<FileStatusRow>(
                _reportService.GetFileStatusReportAsync, f),
            ["ret-02"] = async f => await GetPagedDataAsync<ActiveRetireeRow>(
                _reportService.GetActiveRetireesAsync, f),
            ["ret-03"] = async f => await GetPagedDataAsync<SuspensionRow>(
                _reportService.GetSuspensionsReportAsync, f),

            // ── Financial (via ReportService) ──
            ["fin-01"] = async f => await GetPagedDataAsync<SalaryBatchRow>(
                _reportService.GetSalaryBatchesAsync, f),
            ["fin-02"] = async f => await GetPagedDataAsync<SettlementRow>(
                _reportService.GetSettlementsReportAsync, f),
            ["fin-03"] = async f => await GetPagedDataAsync<FiscalPeriodRow>(
                _reportService.GetFiscalPeriodReportAsync, f),

            // ── Documents (via ReportService) ──
            ["doc-01"] = async f => await GetPagedDataAsync<FileMissingDocsRow>(
                _reportService.GetMissingDocsReportAsync, f),
            ["doc-01b"] = async f => await GetPagedDataAsync<MissingDocsByPersonRow>(
                _reportService.GetMissingDocsByPersonReportAsync, f),
            ["doc-02"] = async f => await GetPagedDataAsync<ExpiredDocRow>(
                _reportService.GetExpiredDocsReportAsync, f),

            // ── Dependents (via ReportService) ──
            ["dep-01"] = async f => await GetPagedDataAsync<BeneficiaryRow>(
                _reportService.GetBeneficiariesReportAsync, f),

            // ── Compliance (via ReportService) ──
            ["com-01"] = async f => await GetPagedDataAsync<MinistryBatchRow>(
                _reportService.GetMinistryBatchesAsync, f),
            ["com-02"] = async f => await GetPagedDataAsync<PendingApprovalRow>(
                _reportService.GetPendingApprovalsAsync, f),

            // ── Admin (via ReportService) ──
            ["adm-01"] = async f => await GetPagedDataAsync<AuditLogRow>(
                _reportService.GetAuditLogAsync, f),
            ["adm-02"] = async f => await GetPagedDataAsync<UserActivityRow>(
                _reportService.GetUserActivityAsync, f),
        };
    }

    public async Task<object[]> GetDataAsync(string reportId, ReportFilterDto filter)
    {
        if (_resolvers.TryGetValue(reportId, out var resolver))
            return await resolver(filter);

        return Array.Empty<object>();
    }

    private static async Task<object[]> GetPagedDataAsync<T>(
        Func<ReportFilterDto, Task<Core.DTOs.Common.ApiResponse<Core.DTOs.Common.PagedResult<T>>>> serviceCall,
        ReportFilterDto filter)
    {
        var pagedFilter = new ReportFilterDto
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PortfolioId = filter.PortfolioId,
            ProjectId = filter.ProjectId,
            PropertyId = filter.PropertyId,
            BranchId = filter.BranchId,
            StatusId = filter.StatusId,
            FiscalYear = filter.FiscalYear,
            Quarter = filter.Quarter,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };

        var result = await serviceCall(pagedFilter);
        var items = result.Data?.Items ?? new List<T>();
        return items.Cast<object>().ToArray();
    }
}
