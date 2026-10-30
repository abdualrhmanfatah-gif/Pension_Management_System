using PensionMS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class ReportService
{
    // ═══════════════════════════════════════
    // RET-01: File Status
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<FileStatusRow>>> GetFileStatusReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Pension.View");
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var query = queries.Query(_unitOfWork.RetirementFiles, filter.BranchId)
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Rank)
            .Include(f => f.Status)
            .Include(f => f.PensionBranch)
            .Include(f => f.MissingDocuments.Where(md => md.SubmittedAt == null))
            .Where(f => !f.IsDeleted);

        if (filter.StatusId.HasValue)
            query = query.Where(f => f.StatusID == filter.StatusId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(f =>
                (f.FileNumber != null && f.FileNumber.Contains(filter.Search)) ||
                (f.Retiree != null && f.Retiree.FullName != null && f.Retiree.FullName.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var files = await query
            .OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.FileID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var data = files.Select(f => new FileStatusRow
        {
            FileID = f.FileID,
            FileNumber = f.FileNumber ?? "",
            RetireeName = f.Retiree?.FullName ?? "",
            RankName = f.Rank?.RankName ?? "",
            Status = f.Status?.StatusName ?? "",
            BranchName = f.PensionBranch?.BranchName ?? "",
            DaysPending = f.StatusID == FileStatusConstants.New ? (int)(DateTime.UtcNow - f.CreatedAt).TotalDays : 0,
            MissingDocsCount = f.MissingDocuments?.Count(md => md.SubmittedAt == null) ?? 0,
            CreatedAt = f.CreatedAt
        }).ToList();

        return ApiResponse<PagedResult<FileStatusRow>>.Success(new PagedResult<FileStatusRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // RET-02: Retirees (alive and deceased)
    // ═══════════════════════════════════════
    internal IQueryable<Retiree> BuildActiveRetireesQuery(IPensionDataQuery queries, ReportFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        // Retirees are a shared register; branch ownership belongs to retirement files.
        var query = queries.Query(_unitOfWork.Retirees, filter.BranchId)
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(r => r.FullName.Contains(filter.Search) || r.NationalID.Contains(filter.Search));

        return query;
    }

    private IQueryable<FinancialEntitlement> BuildLatestRetireePensionsQuery(IPensionDataQuery queries)
    {
        var entitlements = queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        // One pension per retiree, with a stable tie-breaker for identical timestamps.
        return entitlements.Where(e => !entitlements.Any(newer => newer.RetireeID == e.RetireeID
            && (newer.CreatedAt > e.CreatedAt
                || (newer.CreatedAt == e.CreatedAt && newer.EntitlementID > e.EntitlementID))));
    }

    internal IQueryable<ActiveRetireesSummaryDto> BuildActiveRetireesSummaryQuery(IPensionDataQuery queries, ReportFilter filter)
    {
        var retirees = BuildActiveRetireesQuery(queries, filter);
        var pensions = BuildLatestRetireePensionsQuery(queries);

        // Aggregate before paging. The left join includes retirees with no pension record.
        var rows = from retiree in retirees
                   join pension in pensions on retiree.RetireeID equals pension.RetireeID into matches
                   from pension in matches.DefaultIfEmpty()
                   select new
                   {
                       Amount = pension == null ? 0m : pension.Amount,
                       retiree.Governorate
                   };

        return rows.GroupBy(_ => 1).Select(group => new ActiveRetireesSummaryDto
        {
            TotalCount = group.Count(),
            TotalPension = group.Sum(row => row.Amount),
            AvgPension = group.Average(row => row.Amount),
            GovernorateCount = group.Where(row => row.Governorate != null && row.Governorate.Trim() != "")
                .Select(row => row.Governorate!.Trim()).Distinct().Count()
        });
    }

    public async Task<ApiResponse<ActiveRetireesSummaryDto>> GetActiveRetireesSummaryAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Pension.View");
        var summary = await BuildActiveRetireesSummaryQuery(queries, filter).FirstOrDefaultAsync()
            ?? new ActiveRetireesSummaryDto();
        return ApiResponse<ActiveRetireesSummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<PagedResult<ActiveRetireeRow>>> GetActiveRetireesAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Pension.View");
        var baseQuery = BuildActiveRetireesQuery(queries, filter);
        var totalCount = await baseQuery.CountAsync();

        // ── جلب IDs المتقاعدين المرشحين ──
        var retireeIds = await baseQuery
            .OrderBy(r => r.FullName).ThenBy(r => r.RetireeID)
            .Select(r => r.RetireeID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // ── جلب بيانات المتقاعدين مع الملفات ──
        var retirees = await baseQuery
            // Keep registered retirees visible when their source approval item was soft-deleted.
            // The root query and the file include still explicitly exclude deleted records.
            .IgnoreQueryFilters()
            .Include(r => r.ApprovalItem)
                .ThenInclude(a => a.Category)
            .Include(r => r.RetirementFiles.Where(f => !f.IsDeleted))
                .ThenInclude(f => f.Rank)
            .Where(r => retireeIds.Contains(r.RetireeID))
            .ToListAsync();

        // ترتيب حسب IDs الأصلية
        retirees = retirees.OrderBy(r => retireeIds.IndexOf(r.RetireeID)).ToList();

        // ── جلب الماشات ──
        var financialData = await BuildLatestRetireePensionsQuery(queries)
            .Where(e => retireeIds.Contains(e.RetireeID))
            .ToDictionaryAsync(x => x.RetireeID, x => x.Amount);

        var data = retirees.Select(r => {
            var activeFile = r.RetirementFiles?.FirstOrDefault(f => !f.IsDeleted);
            var monthlyPension = financialData.TryGetValue(r.RetireeID, out var amt) ? amt : 0m;
            return new ActiveRetireeRow
            {
                RetireeID = r.RetireeID,
                FullName = r.FullName ?? "",
                NationalID = r.NationalID ?? "",
                MilitaryNumber = r.MilitaryNumber ?? "",
                RankName = activeFile?.Rank?.RankName ?? "",
                Category = r.ApprovalItem?.Category?.CategoryName ?? "",
                MonthlyPension = monthlyPension,
                Status = r.IsAlive ? "حي" : "متوفى",
                Governorate = r.Governorate ?? ""
            };
        }).ToList();

        return ApiResponse<PagedResult<ActiveRetireeRow>>.Success(new PagedResult<ActiveRetireeRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportActiveRetireesAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Pension.View");
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetActiveRetireesAsync(pagedFilter);
        var data = result.RequireComplete();

        var headers = new[] { "#", "الاسم", "الهوية", "الرقم العسكري", "الرتبة", "الفئة", "المعاش الشهري", "حالة الحياة", "المحافظة" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.FullName ?? "",
            r.NationalID ?? "",
            r.MilitaryNumber ?? "",
            r.RankName ?? "",
            r.Category ?? "",
            r.MonthlyPension,
            r.Status ?? "",
            r.Governorate ?? "",
        }).ToArray();
        return _excelExporter.ExportRows("تقرير المتقاعدين", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي المتقاعدين: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // RET-03: Suspensions
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<SuspensionRow>>> GetSuspensionsReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Pension.View");
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var query = queries.Query(_unitOfWork.PensionSuspensions, filter.BranchId)
            .AsNoTracking()
            .Include(s => s.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Where(s => !s.IsDeleted);

        if (filter.StatusId.HasValue)
            query = filter.StatusId.Value == 1 ? query.Where(s => s.EndDate == null) : query.Where(s => s.EndDate != null);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(s =>
                (s.RetirementFile != null && s.RetirementFile.FileNumber != null && s.RetirementFile.FileNumber.Contains(filter.Search)) ||
                (s.RetirementFile != null && s.RetirementFile.Retiree != null && s.RetirementFile.Retiree.FullName != null && s.RetirementFile.Retiree.FullName.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var suspensions = await query
            .OrderByDescending(s => s.SuspensionDate).ThenByDescending(s => s.SuspensionID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var data = suspensions.Select(s => new SuspensionRow
        {
            SuspensionID = s.SuspensionID,
            RetireeName = s.RetirementFile?.Retiree?.FullName ?? "",
            FileNumber = s.RetirementFile?.FileNumber ?? "",
            Reason = s.SuspensionReason ?? "",
            SuspendedDate = s.SuspensionDate,
            LiftedDate = s.EndDate,
            DaysSuspended = (int)((s.EndDate ?? DateTime.UtcNow) - s.SuspensionDate).TotalDays,
            IsActive = s.EndDate == null
        }).ToList();

        return ApiResponse<PagedResult<SuspensionRow>>.Success(new PagedResult<SuspensionRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }
}
