using Microsoft.EntityFrameworkCore;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Reporting;

public class DiscrepancyService : IDiscrepancyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly ExcelReportExporter _excelExporter;
    private readonly PdfReportExporter _pdfExporter;

    public DiscrepancyService(IUnitOfWork unitOfWork, IAccessiblePensionRepository access, ExcelReportExporter excelExporter, PdfReportExporter pdfExporter)
    {
        _unitOfWork = unitOfWork;
        _access = access;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
    }

    public async Task<ApiResponse<PagedResult<RetireeDiscrepancyDto>>> GetDiscrepancyReportAsync(DiscrepancyFilterDto filter)
    {
        var queries = await _access.ForPermissionAsync("Ministry.View");
        var query = BuildBaseQuery(queries);

        // Apply Filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(x =>
                x.ApprovalFullName!.Contains(filter.SearchTerm) ||
                x.RetireeFullName!.Contains(filter.SearchTerm) ||
                x.MilitaryNumber!.Contains(filter.SearchTerm) ||
                x.FileNumber!.Contains(filter.SearchTerm)
            );
        }

        if (filter.OnlyMismatches == true)
        {
            query = query.Where(x =>
                (x.ApprovalFullName != x.RetireeFullName && x.RetireeFullName != null) ||
                (x.ApprovalRankName != x.FileRankName && x.FileRankName != null) ||
                (x.ApprovalRetirementDate != x.FileRetirementDate && x.FileRetirementDate != null)
            );
        }

        if (!string.IsNullOrEmpty(filter.Stage))
        {
            if (filter.Stage == "Stage 1") query = query.Where(x => x.RetireeID == null);
            else if (filter.Stage == "Stage 2") query = query.Where(x => x.RetireeID != null && x.FileID == null);
            else if (filter.Stage == "Stage 3") query = query.Where(x => x.FileID != null);
        }

        // Apply Kashf Report Filters
        query = ApplyExtendedFilters(query, filter);

        // Count & Paging
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.ApprovalItemID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<RetireeDiscrepancyDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponse<PagedResult<RetireeDiscrepancyDto>>.Success(result);
    }

    public async Task<ApiResponse<KashfReportSummaryDto>> GetKashfReportSummaryAsync(DiscrepancyFilterDto filter)
    {
        var queries = await _access.ForPermissionAsync("Ministry.View");
        var query = BuildBaseQuery(queries);
        query = ApplyExtendedFilters(query, filter);

        var counts = await query.GroupBy(_ => 1).Select(g => new
        {
            Total = g.Count(), Processed = g.Count(x => x.IsProcessed),
            WithFile = g.Count(x => x.FileID.HasValue)
        }).FirstOrDefaultAsync();

        var summary = new KashfReportSummaryDto
        {
            TotalCount = counts?.Total ?? 0,
            ProcessedCount = counts?.Processed ?? 0,
            PendingCount = (counts?.Total ?? 0) - (counts?.Processed ?? 0),
            FileCreatedCount = counts?.WithFile ?? 0,
            ByUnit = await query
                .Where(x => !string.IsNullOrEmpty(x.ApprovalUnitName))
                .GroupBy(x => x.ApprovalUnitName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            ByRank = await query
                .Where(x => !string.IsNullOrEmpty(x.ApprovalRankName))
                .GroupBy(x => x.ApprovalRankName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            ByCategory = await query
                .Where(x => !string.IsNullOrEmpty(x.CategoryName))
                .GroupBy(x => x.CategoryName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            ByGender = await query
                .GroupBy(x => x.Gender)
                .Select(g => new NameValuePairDto
                {
                    Name = g.Key == 2 ? "أنثى" : "ذكر",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync()
        };

        return ApiResponse<KashfReportSummaryDto>.Success(summary);
    }

    public async Task<byte[]> ExportKashfReportAsync(DiscrepancyFilterDto filter, string format)
    {
        var queries = await _access.ForPermissionAsync("Ministry.View");
        var query = BuildBaseQuery(queries);
        query = ApplyExtendedFilters(query, filter);

        var items = await query.OrderByDescending(x => x.ApprovalItemID).ToListAsync();

        var headers = new[] { "م", "الرقم العسكري", "الرتبة", "الاسم الكامل", "الوحدة", "الفئة", "الربط", "الجنس", "المعاش", "تاريخ الإحالة", "الحالة", "الملف" };
        var rows = items.Select((it, i) => new object?[]
        {
            i + 1,
            it.MilitaryNumber,
            it.ApprovalRankName,
            it.ApprovalFullName,
            it.ApprovalUnitName,
            it.CategoryName,
            it.LinkNumber,
            it.Gender == 2 ? "أنثى" : "ذكر",
            it.Salary,
            it.ApprovalRetirementDate?.ToString("yyyy/MM/dd"),
            it.IsProcessed ? "معالج" : "معلق",
            it.FileID.HasValue ? "ملف مكتمل" : (it.IsProcessed ? "تمت المعالجة" : "كشف إحالة فقط"),
        }).ToArray();

        if (format.ToLower() == ExportFormatConstants.Excel)
            return _excelExporter.ExportRows("تقرير كشوفات الإحالة", headers, rows,
                new ExcelReportOptions { IncludeFooter = false });

        if (format.ToLower() == ExportFormatConstants.Pdf)
            return _pdfExporter.ExportRows("تقرير كشوفات الإحالة إلى التقاعد", headers, rows);

        return Array.Empty<byte>();
    }

    private IQueryable<RetireeDiscrepancyDto> BuildBaseQuery(IPensionDataQuery queries)
    {
        return from ai in queries.Query(_unitOfWork.MinistryApprovalItems)
                    .AsNoTracking()
                    .Include(i => i.Rank)
                    .Include(i => i.Category)
                join r in queries.Query(_unitOfWork.Retirees) on ai.ApprovalItemID equals r.ApprovalItemID into rJoin
                from r in rJoin.DefaultIfEmpty()
                join rf in queries.Query(_unitOfWork.RetirementFiles) on r.RetireeID equals rf.RetireeID into rfJoin
                from rf in rfJoin.DefaultIfEmpty()
                select new RetireeDiscrepancyDto
                {
                    ApprovalItemID = ai.ApprovalItemID,
                    RetireeID = r != null ? r.RetireeID : (int?)null,
                    FileID = rf != null ? rf.FileID : (int?)null,

                    MilitaryNumber = ai.MilitaryNumber,
                    NationalID = r != null ? r.NationalID : null,

                    ApprovalFullName = ai.FullName,
                    ApprovalRankName = ai.Rank != null ? ai.Rank.RankName : null,
                    ApprovalRetirementDate = ai.RetirementDate,
                    ApprovalUnitName = ai.UnitName,
                    Gender = ai.Gender,
                    Salary = ai.Salary,
                    CategoryName = ai.Category != null ? ai.Category.CategoryName : null,
                    LinkNumber = ai.LinkNumber,

                    RetireeFullName = r != null ? r.FullName : null,
                    IsProcessed = ai.IsProcessed,

                    FileNumber = rf != null ? rf.FileNumber : null,
                    FileRankName = rf != null && rf.Rank != null ? rf.Rank.RankName : null,
                    FileRetirementDate = rf != null ? rf.RetirementDate : null,
                    FileStatusName = rf != null && rf.Status != null ? rf.Status.StatusName : null
                };
    }

    private static IQueryable<RetireeDiscrepancyDto> ApplyExtendedFilters(IQueryable<RetireeDiscrepancyDto> query, DiscrepancyFilterDto filter)
    {
        if (!string.IsNullOrEmpty(filter.UnitName))
            query = query.Where(x => x.ApprovalUnitName == filter.UnitName);

        if (!string.IsNullOrEmpty(filter.RankName))
            query = query.Where(x => x.ApprovalRankName == filter.RankName);

        if (!string.IsNullOrEmpty(filter.CategoryName))
            query = query.Where(x => x.CategoryName == filter.CategoryName);

        if (filter.Gender.HasValue)
            query = query.Where(x => x.Gender == filter.Gender.Value);

        if (!string.IsNullOrEmpty(filter.ProcessingStatus))
        {
            if (filter.ProcessingStatus == "processed")
                query = query.Where(x => x.IsProcessed);
            else if (filter.ProcessingStatus == "pending")
                query = query.Where(x => !x.IsProcessed);
        }

        if (!string.IsNullOrEmpty(filter.FileStatus))
        {
            if (filter.FileStatus == "file_created")
                query = query.Where(x => x.FileID.HasValue);
            else if (filter.FileStatus == "processed_only")
                query = query.Where(x => x.IsProcessed && !x.FileID.HasValue);
            else if (filter.FileStatus == "referral_only")
                query = query.Where(x => !x.IsProcessed);
        }

        return query;
    }
}
