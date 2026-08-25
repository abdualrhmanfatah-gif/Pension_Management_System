using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Services;

public partial class ReportService
{
    // ═══════════════════════════════════════
    // FIN-01: Salary Batches
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<SalaryBatchRow>>> GetSalaryBatchesAsync(ReportFilter filter)
    {
        var query = _unitOfWork.SalaryPaymentBatches.Query()
            .AsNoTracking()
            .Where(b => !b.IsDeleted);

        if (filter.FromDate.HasValue)
            query = query.Where(b => b.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(b => b.CreatedAt < filter.ToDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(b => b.BatchMonth.ToString("yyyy-MM").Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var batches = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => new
            {
                b.BatchID,
                b.BatchMonth,
                b.Status,
                b.TotalAmount,
                RecipientCount = b.SalaryPaymentItems.Count(i => !i.IsDeleted),
                b.CreatedAt,
                b.ApprovedAt
            })
            .ToListAsync();

        var data = batches.Select(b => new SalaryBatchRow
        {
            BatchID = b.BatchID,
            Period = b.BatchMonth.ToString("yyyy-MM"),
            Status = b.Status ?? "",
            TotalAmount = b.TotalAmount,
            RecipientCount = b.RecipientCount,
            CreatedAt = b.CreatedAt,
            ApprovedAt = b.ApprovedAt
        }).ToList();

        return ApiResponse<PagedResult<SalaryBatchRow>>.Success(new PagedResult<SalaryBatchRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportSalaryBatchesAsync(ReportFilter filter, string format = "excel")
    {
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetSalaryBatchesAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<SalaryBatchRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("مسيرات المعاشات");
        var headers = new[] { "#", "الفترة", "الحالة", "إجمالي المبلغ", "عدد المستفيدين", "تاريخ الإنشاء", "تاريخ الاعتماد" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.Period;
            ws.Cells[i + 2, 3].Value = r.Status;
            ws.Cells[i + 2, 4].Value = r.TotalAmount;
            ws.Cells[i + 2, 5].Value = r.RecipientCount;
            ws.Cells[i + 2, 6].Value = r.CreatedAt.ToString("yyyy-MM-dd");
            ws.Cells[i + 2, 7].Value = r.ApprovedAt?.ToString("yyyy-MM-dd") ?? "";
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المسيرات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // FIN-02: Settlements
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<SettlementRow>>> GetSettlementsReportAsync(ReportFilter filter)
    {
        var query = _unitOfWork.SettlementDecisions.Query()
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        if (filter.StatusId.HasValue)
            query = query.Where(s => s.DecisionID == filter.StatusId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(s =>
                (s.DecisionNumber != null && s.DecisionNumber.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var decisions = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => new SettlementRow
            {
                SettlementID = s.DecisionID,
                DecisionNumber = s.DecisionNumber ?? "",
                RetireeName = s.SettlementItems.Where(i => !i.IsDeleted)
                    .Select(i => i.FullName ?? (i.Retiree != null ? i.Retiree.FullName : ""))
                    .FirstOrDefault() ?? "Multiple",
                Amount = s.SettlementItems.Where(i => !i.IsDeleted).Sum(i => (decimal?)(i.ProcessedRank != null ? i.ProcessedRank.BaseSalary : 0)) ?? 0m,
                Status = s.DecisionType ?? "Pending",
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<SettlementRow>>.Success(new PagedResult<SettlementRow>
        {
            Items = decisions,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportSettlementsReportAsync(ReportFilter filter, string format = "excel")
    {
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            StatusId = filter.StatusId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetSettlementsReportAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<SettlementRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("التسويات");
        var headers = new[] { "#", "رقم القرار", "المتقاعد", "المبلغ", "الحالة", "تاريخ الإنشاء" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.DecisionNumber;
            ws.Cells[i + 2, 3].Value = r.RetireeName;
            ws.Cells[i + 2, 4].Value = r.Amount;
            ws.Cells[i + 2, 5].Value = r.Status;
            ws.Cells[i + 2, 6].Value = r.CreatedAt.ToString("yyyy-MM-dd");
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي التسويات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // FIN-03: Fiscal Period Report
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<FiscalPeriodRow>>> GetFiscalPeriodReportAsync(ReportFilter filter)
    {
        var query = _unitOfWork.OperationalExpenses.Query()
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        if (filter.FiscalYear.HasValue)
            query = query.Where(e => e.FiscalYearID == filter.FiscalYear.Value);

        var rawData = await query.GroupBy(e => new { e.ExpenseType, e.QuarterNumber })
            .Select(g => new 
            {
                ExpenseType = g.Key.ExpenseType ?? "غير محدد",
                QuarterNumber = g.Key.QuarterNumber ?? 0,
                TotalNet = g.Sum(e => e.NetAmount),
                TotalTax = g.Sum(e => e.TaxAmount),
                Count = g.Count()
            }).ToListAsync();

        var allData = rawData.Select(d => new FiscalPeriodRow
        {
            ChapterName = d.ExpenseType,
            SectionName = "",
            Period = d.QuarterNumber > 0 ? "الفصل " + d.QuarterNumber : "سنوي",
            Allocated = d.TotalNet + d.TotalTax,
            Spent = d.TotalNet,
            Carryover = d.TotalTax
        }).ToList();

        var totalCount = allData.Count;
        var data = allData
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return ApiResponse<PagedResult<FiscalPeriodRow>>.Success(new PagedResult<FiscalPeriodRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportFiscalPeriodReportAsync(ReportFilter filter, string format = "excel")
    {
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            FiscalYear = filter.FiscalYear,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetFiscalPeriodReportAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<FiscalPeriodRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("تقارير الفترات");
        var headers = new[] { "#", "القائمة", "الحساب", "الفترة", "المخصص", "المنصرف", "المتبقي", "نسبة الصرف %", "المدور" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.ChapterName;
            ws.Cells[i + 2, 3].Value = r.SectionName;
            ws.Cells[i + 2, 4].Value = r.Period;
            ws.Cells[i + 2, 5].Value = r.Allocated;
            ws.Cells[i + 2, 6].Value = r.Spent;
            ws.Cells[i + 2, 7].Value = r.Remaining;
            ws.Cells[i + 2, 8].Value = Math.Round(r.SpentPct, 2);
            ws.Cells[i + 2, 9].Value = r.Carryover;
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي السجلات: {data.Count}");
        return package.GetAsByteArray();
    }
}
