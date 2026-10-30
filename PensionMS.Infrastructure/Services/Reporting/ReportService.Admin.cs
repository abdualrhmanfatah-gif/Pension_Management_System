using Microsoft.EntityFrameworkCore;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.Constants;
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
    // DEP-01: Beneficiaries
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<BeneficiaryRow>>> GetBeneficiariesReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Entitlements.View");
        var query = queries.Query(_unitOfWork.EntitledPersons, filter.BranchId)
            .AsNoTracking()
            .Include(e => e.RetirementFile)
            .Include(e => e.Relationship)
            .Include(e => e.Guardians)
            .Where(e => !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(e => e.FullName.Contains(filter.Search) || (e.NationalID != null && e.NationalID.Contains(filter.Search)) ||
                (e.RetirementFile != null && e.RetirementFile.FileNumber.Contains(filter.Search)) ||
                (e.RetirementFile != null && e.RetirementFile.Retiree != null && e.RetirementFile.Retiree.FullName.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(e => e.RetirementFile.FileNumber)
            .ThenBy(e => e.FullName).ThenBy(e => e.EntitledID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new BeneficiaryRow
            {
                FileID = e.FileID,
                PersonID = e.EntitledID,
                FullName = e.FullName ?? "",
                NationalID = e.NationalID ?? "",
                Relationship = e.Relationship != null ? e.Relationship.RelationshipName : "",
                GuardianName = e.Guardians.Where(g => !g.IsDeleted).Select(g => g.FullName).FirstOrDefault() ?? "",
                FileNumber = e.RetirementFile != null ? e.RetirementFile.FileNumber : "",
                MonthlyAmount = 0m,
                RetireeName = e.RetirementFile != null && e.RetirementFile.Retiree != null ? e.RetirementFile.Retiree.FullName : "",
                MilitaryNumber = e.RetirementFile != null && e.RetirementFile.Retiree != null ? e.RetirementFile.Retiree.MilitaryNumber : "",
                PersonPhone = e.Phone,
                PersonType = "مستحق"
            })
            .ToListAsync();

        return ApiResponse<PagedResult<BeneficiaryRow>>.Success(new PagedResult<BeneficiaryRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // COM-01: Ministry Batches
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<MinistryBatchRow>>> GetMinistryBatchesAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Admin.View");
        var restricted = queries.IsRestricted(filter.BranchId);
        var visibleItems = queries.Query(_unitOfWork.MinistryApprovalItems, filter.BranchId).Select(i => i.ApprovalItemID);
        var query = queries.Query(_unitOfWork.MinistryApprovalBatches, filter.BranchId)
            .AsNoTracking()
            .Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(b => b.BatchNumber.Contains(filter.Search) || (b.Status != null && b.Status.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.BatchID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => new MinistryBatchRow
            {
                BatchID = b.BatchID,
                BatchNumber = b.BatchNumber ?? "",
                Status = b.Status ?? "",
                TotalCount = restricted ? b.MinistryApprovalItems.Count(i => visibleItems.Contains(i.ApprovalItemID)) : b.TotalCount,
                ProcessedCount = restricted ? b.MinistryApprovalItems.Count(i => i.IsProcessed && visibleItems.Contains(i.ApprovalItemID)) : b.ProcessedCount,
                ItemCount = b.MinistryApprovalItems.Count(i => !i.IsDeleted && (!restricted || visibleItems.Contains(i.ApprovalItemID))),
                SubmittedDate = b.CreatedAt,
                BatchDate = b.BatchDate
            })
            .ToListAsync();

        return ApiResponse<PagedResult<MinistryBatchRow>>.Success(new PagedResult<MinistryBatchRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportMinistryBatchesAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Admin.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetMinistryBatchesAsync(pagedFilter);
        var data = result.RequireComplete();
        var headers = new[] { "#", "رقم الدفعة", "الحالة", "العدد الكلي", "العدد المعالج", "عدد العناصر", "تاريخ الدفعة", "تاريخ الإنشاء" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.BatchNumber,
            r.Status,
            r.TotalCount,
            r.ProcessedCount,
            r.ItemCount,
            r.BatchDate?.ToString("yyyy-MM-dd") ?? "",
            r.SubmittedDate.ToString("yyyy-MM-dd"),
        }).ToArray();
        return _excelExporter.ExportRows("دفعات الوزارة", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي الدفعات: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // COM-02: Pending Approvals
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<PendingApprovalRow>>> GetPendingApprovalsAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Admin.View");
        var query = queries.Query(_unitOfWork.InvestmentApprovalRequests, filter.BranchId)
            .AsNoTracking()
            .Where(a => a.Status == AuditStatus.Pending && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(a => a.EntityType != null && a.EntityType.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var approvals = await query
            .Include(a => a.RequestedBy)
            .OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.ApprovalID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var data = approvals.Select(a => new PendingApprovalRow
        {
            RequestID = a.ApprovalID,
            RequestType = a.EntityType ?? "",
            ProjectName = "Entity " + a.EntityID,
            RequestedBy = a.RequestedBy?.FullName ?? "",
            Amount = null,
            DaysPending = (int)(DateTime.UtcNow - a.CreatedAt).TotalDays,
            CreatedAt = a.CreatedAt
        }).ToList();

        return ApiResponse<PagedResult<PendingApprovalRow>>.Success(new PagedResult<PendingApprovalRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // ADM-01: Audit Log
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<AuditLogRow>>> GetAuditLogAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Admin.View");
        var query = queries.Query(_unitOfWork.AuditLogs, filter.BranchId)
            .AsNoTracking();

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.CreatedAt < filter.ToDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(a =>
                (a.User != null && a.User.FullName != null && a.User.FullName.Contains(filter.Search)) ||
                (a.Action != null && a.Action.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.CreatedAt)
            .ThenBy(a => a.LogID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogRow
            {
                LogID = a.LogID,
                UserName = a.User != null ? a.User.FullName : "System",
                Action = a.Action ?? "",
                TableName = a.EntityDisplayName ?? "",
                RecordID = a.RecordID,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                Timestamp = a.CreatedAt,
                IPAddress = a.IPAddress
            })
            .ToListAsync();

        return ApiResponse<PagedResult<AuditLogRow>>.Success(new PagedResult<AuditLogRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // ADM-02: User Activity
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<UserActivityRow>>> GetUserActivityAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Admin.View");
        var query = queries.Query(_unitOfWork.Users, filter.BranchId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(u => u.FullName.Contains(filter.Search) || u.Username.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(u => u.FullName).ThenBy(u => u.UserID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new UserActivityRow
            {
                UserID = u.UserID,
                UserName = u.Username ?? "",
                FullName = u.FullName ?? "",
                RoleName = u.Role != null ? u.Role.RoleName : "",
                BranchName = u.Branch != null ? u.Branch.BranchName : "",
                LoginCount = 0,
                LastLogin = u.LastLogin,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return ApiResponse<PagedResult<UserActivityRow>>.Success(new PagedResult<UserActivityRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // KASHF: كشوفات الإحالة
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<KashfReportRow>>> GetKashfReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Ministry.View");
        var query = from ai in queries.Query(_unitOfWork.MinistryApprovalItems, filter.BranchId)
                        .AsNoTracking()
                        .Include(i => i.Rank)
                        .Include(i => i.Category)
                    join r in queries.Query(_unitOfWork.Retirees, filter.BranchId) on ai.ApprovalItemID equals r.ApprovalItemID into rJoin
                    from r in rJoin.DefaultIfEmpty()
                    join rf in queries.Query(_unitOfWork.RetirementFiles, filter.BranchId) on r.RetireeID equals rf.RetireeID into rfJoin
                    from rf in rfJoin.DefaultIfEmpty()
                    select new KashfReportRow
                    {
                        ApprovalItemID = ai.ApprovalItemID,
                        FileID = rf != null ? rf.FileID : null,
                        MilitaryNumber = ai.MilitaryNumber ?? "",
                        FullName = ai.FullName ?? "",
                        RankName = ai.Rank != null ? ai.Rank.RankName : "",
                        UnitName = ai.UnitName ?? "",
                        CategoryName = ai.Category != null ? ai.Category.CategoryName : "",
                        Gender = ai.Gender == 2 ? "أنثى" : "ذكر",
                        Salary = ai.Salary,
                        RetirementDate = ai.RetirementDate,
                        Status = rf != null ? "ملف مكتمل" : (ai.IsProcessed ? "تمت المعالجة" : "كشف إحالة فقط"),
                        FileNumber = rf != null ? rf.FileNumber : ""
                    };

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(x => x.FullName.Contains(filter.Search) || x.MilitaryNumber.Contains(filter.Search) || x.FileNumber.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.ApprovalItemID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<KashfReportRow>>.Success(new PagedResult<KashfReportRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<ApiResponse<object>> GetKashfReportSummaryAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Ministry.View");
        var query = from ai in queries.Query(_unitOfWork.MinistryApprovalItems, filter.BranchId).AsNoTracking()
                    join r in queries.Query(_unitOfWork.Retirees, filter.BranchId) on ai.ApprovalItemID equals r.ApprovalItemID into rJoin
                    from r in rJoin.DefaultIfEmpty()
                    join rf in queries.Query(_unitOfWork.RetirementFiles, filter.BranchId) on r.RetireeID equals rf.RetireeID into rfJoin
                    from rf in rfJoin.DefaultIfEmpty()
                    select new { ai, r, rf };

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(x => x.ai.FullName!.Contains(filter.Search) || x.ai.MilitaryNumber!.Contains(filter.Search));

        object summary = await query.GroupBy(_ => 1).Select(g => new
        {
            totalCount = g.Count(),
            fileCreatedCount = g.Count(x => x.rf != null),
            processedCount = g.Count(x => x.rf == null && x.ai.IsProcessed),
            pendingCount = g.Count(x => x.rf == null && !x.ai.IsProcessed)
        }).FirstOrDefaultAsync() ?? new
        {
            totalCount = 0, fileCreatedCount = 0, processedCount = 0, pendingCount = 0
        };

        return ApiResponse<object>.Success(summary);
    }
}
