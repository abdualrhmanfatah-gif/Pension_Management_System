using PensionMS.Core.Interfaces.Reporting;
using PensionMS.Infrastructure.Reporting.Exporters;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Reporting;

public class RetirementFileReportService : IRetirementFileReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly RetirementFileReportExporter _exporter;

    public RetirementFileReportService(IUnitOfWork unitOfWork, RetirementFileReportExporter exporter, IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _access = access;
        _exporter = exporter;
    }

    // ═══════════════════════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<RetirementFileReportRow>>> GetRetirementFileReportAsync(RetirementFileReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("File.View");
        var query = BuildReportQuery(queries, filter.BranchId);
        query = ApplyReportFilters(query, filter);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.FileID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<RetirementFileReportRow>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponse<PagedResult<RetirementFileReportRow>>.Success(result);
    }

    public async Task<ApiResponse<RetirementFileReportSummaryDto>> GetRetirementFileReportSummaryAsync(RetirementFileReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("File.View");
        var query = ApplyReportFilters(BuildReportQuery(queries, filter.BranchId), filter);

        var totalCount = await query.CountAsync();

        var statusCounts = await query
            .Where(x => x.StatusName != null && x.StatusName != "")
            .GroupBy(x => x.StatusName)
            .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
            .ToListAsync();

        var reviewStatusCounts = await query
            .Where(x => x.ReviewStatusName != null)
            .GroupBy(x => x.ReviewStatusName)
            .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
            .ToListAsync();

        var aliveCounts = await query
            .GroupBy(x => x.IsAlive)
            .Select(g => new { Alive = g.Key, Count = g.Count() })
            .ToListAsync();

        var byRank = await query
            .Where(x => x.RankName != null && x.RankName != "")
            .GroupBy(x => x.RankName)
            .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
            .ToListAsync();

        var byBranch = await query
            .Where(x => x.BranchName != null && x.BranchName != "")
            .GroupBy(x => x.BranchName)
            .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
            .ToListAsync();

        var byGovernorate = await query
            .Where(x => x.GovernorateName != null && x.GovernorateName != "")
            .GroupBy(x => x.GovernorateName)
            .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
            .ToListAsync();

        var byGender = await query
            .GroupBy(x => x.Gender)
            .Select(g => new { Name = g.Key == 2 ? "أنثى" : "ذكر", Count = g.Count() })
            .ToListAsync();

        var bySocialStatus = await query
            .Where(x => x.SocialStatusName != null && x.SocialStatusName != "")
            .GroupBy(x => x.SocialStatusName)
            .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
            .ToListAsync();

        int CountContaining(List<NameValuePairDto> counts, string token) =>
            counts.Where(c => c.Name.Contains(token)).Sum(c => c.Count);

        var summary = new RetirementFileReportSummaryDto
        {
            TotalCount = totalCount,
            ActiveCount = CountContaining(statusCounts, "نشط"),
            SuspendedCount = CountContaining(statusCounts, "معلق"),
            ClosedCount = CountContaining(statusCounts, "مغلق"),
            IncompleteCount = CountContaining(statusCounts, "ناقص"),
            PendingReviewCount = CountContaining(reviewStatusCounts, "بانتظار"),
            AliveCount = aliveCounts.FirstOrDefault(c => c.Alive)?.Count ?? 0,
            DeceasedCount = aliveCounts.FirstOrDefault(c => !c.Alive)?.Count ?? 0,
            ByRank = byRank.OrderByDescending(x => x.Count).ToList(),
            ByBranch = byBranch.OrderByDescending(x => x.Count).ToList(),
            ByGovernorate = byGovernorate.OrderByDescending(x => x.Count).ToList(),
            ByGender = byGender.Select(x => new NameValuePairDto { Name = x.Name, Count = x.Count })
                .OrderByDescending(x => x.Count).ToList(),
            ByStatus = statusCounts.OrderByDescending(x => x.Count).ToList(),
            BySocialStatus = bySocialStatus.OrderByDescending(x => x.Count).ToList(),
        };

        return ApiResponse<RetirementFileReportSummaryDto>.Success(summary);
    }
    public async Task<byte[]> ExportRetirementFileReportAsync(RetirementFileReportFilter filter, string format)
    {
        var queries = await _access.ForPermissionAsync("File.View");
        var query = ApplyReportFilters(BuildReportQuery(queries, filter.BranchId), filter);
        var items = await query.OrderByDescending(x => x.FileID).ToListAsync();
        return _exporter.Export(items, format);
    }

    private IQueryable<RetirementFileReportRow> BuildReportQuery(IPensionDataQuery queries, int? branchId)
    {
        return from rf in queries.Query(_unitOfWork.RetirementFiles, branchId)
                    .AsNoTracking()
                    .Include(f => f.Rank)
                    .Include(f => f.Status)
                    .Include(f => f.PensionBranch)
                join r in queries.Query(_unitOfWork.Retirees, branchId) on rf.RetireeID equals r.RetireeID
                join ss in queries.Query(_unitOfWork.SocialStatuses) on r.SocialStatusID equals ss.SocialStatusID into ssJoin
                from ss in ssJoin.DefaultIfEmpty()
                join hs in queries.Query(_unitOfWork.HealthStatuses) on r.HealthStatusID equals hs.HealthStatusID into hsJoin
                from hs in hsJoin.DefaultIfEmpty()
                where !rf.IsDeleted
                select new RetirementFileReportRow
                {
                    FileID = rf.FileID,
                    FileNumber = rf.FileNumber,
                    RetireeName = r.FullName,
                    MilitaryNumber = r.MilitaryNumber,
                    NationalID = r.NationalID,
                    RankID = rf.RankID,
                    RankName = rf.Rank != null ? rf.Rank.RankName : null,
                    PensionBranchID = rf.PensionBranchID,
                    BranchName = rf.PensionBranch != null ? rf.PensionBranch.BranchName : null,
                    GovernorateName = r.Governorate,
                    StatusID = rf.StatusID,
                    StatusName = rf.Status != null ? rf.Status.StatusName : null,
                    Gender = r.Gender,
                    SocialStatusID = r.SocialStatusID,
                    SocialStatusName = ss != null ? ss.StatusName : null,
                    HealthStatusName = hs != null ? hs.HealthStatusName : null,
                    RetirementDate = rf.RetirementDate,
                    FileOpenDate = rf.FileOpenDate,
                    MissingDocumentsCount = rf.MissingDocumentsCount,
                    ReviewStatusName = rf.ReviewStatus == Core.Entities.FileReviewStatus.PendingReview ? "بانتظار المراجعة" :
                                       rf.ReviewStatus == Core.Entities.FileReviewStatus.Approved ? "تمت المراجعة" :
                                       rf.ReviewStatus == Core.Entities.FileReviewStatus.Rejected ? "مرفوض" : "---",
                    IsAlive = r.IsAlive
                };
    }

    private static IQueryable<RetirementFileReportRow> ApplyReportFilters(IQueryable<RetirementFileReportRow> query, RetirementFileReportFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(x => x.RetireeName!.Contains(filter.SearchTerm) || x.MilitaryNumber!.Contains(filter.SearchTerm) || x.FileNumber!.Contains(filter.SearchTerm));

        if (filter.StatusId.HasValue)
            query = query.Where(x => x.StatusID == filter.StatusId.Value);

        if (filter.RankId.HasValue)
            query = query.Where(x => x.RankID == filter.RankId.Value);

        if (filter.Gender.HasValue)
            query = query.Where(x => x.Gender == filter.Gender.Value);

        if (filter.BranchId.HasValue)
            query = query.Where(x => x.PensionBranchID == filter.BranchId.Value);

        if (!string.IsNullOrEmpty(filter.Governorate))
            query = query.Where(x => x.GovernorateName == filter.Governorate);

        if (filter.ReviewStatus.HasValue)
        {
            if (filter.ReviewStatus.Value == 1)
                query = query.Where(x => x.ReviewStatusName != null && x.ReviewStatusName.Contains("بانتظار"));
            else if (filter.ReviewStatus.Value == 2)
                query = query.Where(x => x.ReviewStatusName != null && x.ReviewStatusName.Contains("تمت"));
            else if (filter.ReviewStatus.Value == 3)
                query = query.Where(x => x.ReviewStatusName != null && x.ReviewStatusName.Contains("مرفوض"));
        }

        if (filter.SocialStatusId.HasValue)
            query = query.Where(x => x.SocialStatusID == filter.SocialStatusId.Value);

        return query;
    }
}
