using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Services;

public partial class ReportService
{
    // ═══════════════════════════════════════
    // RET-01: File Status
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<FileStatusRow>>> GetFileStatusReportAsync(ReportFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var query = _unitOfWork.RetirementFiles.Query()
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
            .OrderByDescending(f => f.CreatedAt)
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

    public async Task<byte[]> ExportFileStatusReportAsync(ReportFilter filter, string format = "excel")
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            StatusId = filter.StatusId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetFileStatusReportAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<FileStatusRow>();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("حالة الملفاتات");
        ws.View.RightToLeft = true;

        var headers = new[] { "#", "رقم الملف", "المتقاعد", "الرتبة", "الحالة", "الفرع", "أيام الانتظار", "مستندات ناقصة", "تاريخ الإنشاء" };
        for (int i = 0; i < headers.Length; i++) 
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = r.FileNumber ?? "";
            ws.Cells[row, 3].Value = r.RetireeName ?? "";
            ws.Cells[row, 4].Value = r.RankName ?? "";
            ws.Cells[row, 5].Value = r.Status ?? "";
            ws.Cells[row, 6].Value = r.BranchName ?? "";
            ws.Cells[row, 7].Value = r.DaysPending;
            ws.Cells[row, 8].Value = r.MissingDocsCount;
            ws.Cells[row, 9].Value = r.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي الملفات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // RET-02: Active Retirees
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<ActiveRetireeRow>>> GetActiveRetireesAsync(ReportFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        // ── استعلام العدد (بدون Include لتجنب تكرار الصفوف) ──
        var baseQuery = _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.IsAlive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            baseQuery = baseQuery.Where(r => r.FullName.Contains(filter.Search) || r.NationalID.Contains(filter.Search));

        var totalCount = await baseQuery.CountAsync();

        // ── جلب IDs المتقاعدين المرشحين ──
        var retireeIds = await baseQuery
            .OrderBy(r => r.FullName)
            .Select(r => r.RetireeID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // ── جلب بيانات المتقاعدين مع الملفات ──
        var retirees = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Include(r => r.RetirementFiles.Where(f => !f.IsDeleted))
                .ThenInclude(f => f.Rank)
            .Where(r => retireeIds.Contains(r.RetireeID))
            .ToListAsync();

        // ترتيب حسب IDs الأصلية
        retirees = retirees.OrderBy(r => retireeIds.IndexOf(r.RetireeID)).ToList();

        // ── جلب الماشات ──
        var financialData = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking()
            .Where(e => !e.IsDeleted && retireeIds.Contains(e.RetireeID))
            .GroupBy(e => e.RetireeID)
            .Select(g => new { RetireeID = g.Key, Amount = g.OrderByDescending(e => e.CreatedAt).FirstOrDefault().Amount })
            .ToDictionaryAsync(x => x.RetireeID, x => x.Amount);

        var data = retirees.Select(r => {
            var activeFile = r.RetirementFiles?.FirstOrDefault(f => !f.IsDeleted);
            var monthlyPension = financialData.TryGetValue(r.RetireeID, out var amt) ? amt : 0m;
            return new ActiveRetireeRow
            {
                RetireeID = r.RetireeID,
                FullName = r.FullName ?? "",
                NationalID = r.NationalID ?? "",
                RankName = activeFile?.Rank?.RankName ?? "",
                Category = "Standard",
                MonthlyPension = monthlyPension,
                Status = "Active",
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
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetActiveRetireesAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<ActiveRetireeRow>();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("المتقاعدين النشطين");
        ws.View.RightToLeft = true;

        var headers = new[] { "#", "الاسم", "الهوية", "الرتبة", "الفئة", "المعاش الشهري", "الحالة", "المحافظة" };
        for (int i = 0; i < headers.Length; i++) 
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = r.FullName ?? "";
            ws.Cells[row, 3].Value = r.NationalID ?? "";
            ws.Cells[row, 4].Value = r.RankName ?? "";
            ws.Cells[row, 5].Value = r.Category ?? "";
            ws.Cells[row, 6].Value = r.MonthlyPension;
            ws.Cells[row, 7].Value = r.Status ?? "";
            ws.Cells[row, 8].Value = r.Governorate ?? "";
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المتقاعدين: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // RET-03: Suspensions
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<SuspensionRow>>> GetSuspensionsReportAsync(ReportFilter filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var query = _unitOfWork.PensionSuspensions.Query()
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
            .OrderByDescending(s => s.SuspensionDate)
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

    public async Task<byte[]> ExportSuspensionsReportAsync(ReportFilter filter, string format = "excel")
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            StatusId = filter.StatusId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetSuspensionsReportAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<SuspensionRow>();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("الإيقافات");
        ws.View.RightToLeft = true;

        var headers = new[] { "#", "المتقاعد", "رقم الملف", "السبب", "تاريخ الإيقاف", "تاريخ الرفع", "أيام الإيقاف", "نشط" };
        for (int i = 0; i < headers.Length; i++) 
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = r.RetireeName ?? "";
            ws.Cells[row, 3].Value = r.FileNumber ?? "";
            ws.Cells[row, 4].Value = r.Reason ?? "";
            ws.Cells[row, 5].Value = r.SuspendedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            ws.Cells[row, 6].Value = r.LiftedDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "";
            ws.Cells[row, 7].Value = r.DaysSuspended;
            ws.Cells[row, 8].Value = r.IsActive ? "نعم" : "لا";
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي الإيقافات: {data.Count}");
        return package.GetAsByteArray();
    }
}