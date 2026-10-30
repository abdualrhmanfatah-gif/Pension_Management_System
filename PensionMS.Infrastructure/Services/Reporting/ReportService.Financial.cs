using Microsoft.EntityFrameworkCore;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class ReportService
{
    // ═══════════════════════════════════════
    // FIN-01: Salary Batches
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<SalaryBatchRow>>> GetSalaryBatchesAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var restricted = queries.IsRestricted(filter.BranchId);
        var fileIds = queries.FileIds(filter.BranchId);
        var query = queries.Query(_unitOfWork.SalaryPaymentBatches, filter.BranchId)
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
            .OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.BatchID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => new
            {
                b.BatchID,
                b.BatchMonth,
                b.Status,
                TotalAmount = restricted ? b.SalaryPaymentItems.Where(i => !i.IsDeleted && fileIds.Contains(i.FileID)).Sum(i => (decimal?)i.Amount) ?? 0m : b.TotalAmount,
                RecipientCount = b.SalaryPaymentItems.Count(i => !i.IsDeleted && (!restricted || fileIds.Contains(i.FileID))),
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

    // ═══════════════════════════════════════
    // FIN-02: Settlements (item-level)
    // ═══════════════════════════════════════
    internal IQueryable<PensionMS.Core.Entities.SettlementItem> BuildSettlementItemsQuery(PensionMS.Core.Interfaces.IPensionDataQuery queries, ReportFilter filter)
    {
        var query = queries.Query(_unitOfWork.SettlementItems, filter.BranchId)
            .Where(i => !i.IsDeleted && i.DecisionID != null && !i.Decision!.IsDeleted);

        if (filter.FromDate.HasValue)
            query = query.Where(i => i.Decision!.DecisionDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(i => i.Decision!.DecisionDate < filter.ToDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(filter.DecisionType))
            query = query.Where(i => i.Decision!.DecisionType != null && i.Decision.DecisionType.Contains(filter.DecisionType));
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(i =>
                (i.Decision!.DecisionNumber != null && i.Decision.DecisionNumber.Contains(filter.Search)) ||
                (i.FullName != null && i.FullName.Contains(filter.Search)) ||
                (i.MilitaryNumber != null && i.MilitaryNumber.Contains(filter.Search)));

        return query;
    }

    internal static IQueryable<SettlementRow> ProjectSettlements(IQueryable<PensionMS.Core.Entities.SettlementItem> query)
        => query.Select(i => new SettlementRow
        {
            SettlementID = i.SettlementItemID,
            DecisionID = i.DecisionID ?? 0,
            DecisionNumber = i.Decision!.DecisionNumber ?? "",
            RetireeName = i.FullName ?? (i.Retiree != null ? i.Retiree.FullName : "") ?? "",
            MilitaryNumber = i.MilitaryNumber ?? (i.Retiree != null ? i.Retiree.MilitaryNumber : "") ?? "",
            PreviousRank = i.CurrentRank.RankName,
            ProcessedRank = i.ProcessedRank.RankName,
            Status = i.Decision.DecisionType ?? "",
            DecisionDate = i.Decision.DecisionDate
        });

    public async Task<ApiResponse<PagedResult<SettlementRow>>> GetSettlementsReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = BuildSettlementItemsQuery(queries, filter)
            .OrderByDescending(i => i.Decision!.DecisionDate)
            .ThenBy(i => i.SettlementItemID);

        var totalCount = await query.CountAsync();
        var rows = await ProjectSettlements(query)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<SettlementRow>>.Success(new PagedResult<SettlementRow>
        {
            Items = rows,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<ApiResponse<SettlementSummaryDto>> GetSettlementsSummaryAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = BuildSettlementItemsQuery(queries, filter);
        var totalItems = await query.CountAsync();
        var summary = new SettlementSummaryDto { TotalItems = totalItems };
        if (totalItems > 0)
        {
            summary.TotalAmount = await query.SumAsync(i => i.Amount - (i.Deductions ?? 0));
            summary.DecisionCount = await query.Select(i => i.DecisionID).Distinct().CountAsync();
            summary.AverageAmount = summary.TotalAmount / totalItems;
        }

        return ApiResponse<SettlementSummaryDto>.Success(summary);
    }

    // ═══════════════════════════════════════
    // FIN-03: Fiscal Period Report
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<FiscalPeriodRow>>> GetFiscalPeriodReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.OperationalExpenses, filter.BranchId)
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        if (filter.FiscalYear.HasValue)
            query = query.Where(e => e.FiscalYearID == filter.FiscalYear.Value);

        var rawData = await query.GroupBy(e => new { e.ExpenseType, e.QuarterNumber })
            .OrderBy(g => g.Key.ExpenseType).ThenBy(g => g.Key.QuarterNumber)
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
}
