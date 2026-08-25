using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PensionMS.Core.Constants;
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
    // DEP-01: Beneficiaries
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<BeneficiaryRow>>> GetBeneficiariesReportAsync(ReportFilter filter)
    {
        var query = _unitOfWork.EntitledPersons.Query()
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
            .ThenBy(e => e.FullName)
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

    public async Task<byte[]> ExportBeneficiariesReportAsync(ReportFilter filter, string format = "excel")
    {
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetBeneficiariesReportAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<BeneficiaryRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("المستفيدين");
        var headers = new[] { "#", "رقم الملف", "اسم المتقاعد", "الرقم العسكري", "اسم المستفيد", "الهوية", "القرابة", "نوع الشخص", "الوصي", "رقم هاتف المستفيد", "المبلغ الشهري" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.FileNumber;
            ws.Cells[i + 2, 3].Value = r.RetireeName;
            ws.Cells[i + 2, 4].Value = r.MilitaryNumber;
            ws.Cells[i + 2, 5].Value = r.FullName;
            ws.Cells[i + 2, 6].Value = r.NationalID;
            ws.Cells[i + 2, 7].Value = r.Relationship;
            ws.Cells[i + 2, 8].Value = r.PersonType;
            ws.Cells[i + 2, 9].Value = r.GuardianName;
            ws.Cells[i + 2, 10].Value = r.PersonPhone;
            ws.Cells[i + 2, 11].Value = r.MonthlyAmount;
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المستفيدين: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // COM-01: Ministry Batches
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<MinistryBatchRow>>> GetMinistryBatchesAsync(ReportFilter filter)
    {
        var query = _unitOfWork.MinistryApprovalBatches.Query()
            .AsNoTracking()
            .Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(b => b.BatchNumber.Contains(filter.Search) || (b.Status != null && b.Status.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => new MinistryBatchRow
            {
                BatchID = b.BatchID,
                BatchNumber = b.BatchNumber ?? "",
                Status = b.Status ?? "",
                TotalCount = b.TotalCount,
                ProcessedCount = b.ProcessedCount,
                ItemCount = b.MinistryApprovalItems.Count(i => !i.IsDeleted),
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetMinistryBatchesAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<MinistryBatchRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("دفعات الوزارة");
        var headers = new[] { "#", "رقم الدفعة", "الحالة", "العدد الكلي", "العدد المعالج", "عدد العناصر", "تاريخ الدفعة", "تاريخ الإنشاء" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.BatchNumber;
            ws.Cells[i + 2, 3].Value = r.Status;
            ws.Cells[i + 2, 4].Value = r.TotalCount;
            ws.Cells[i + 2, 5].Value = r.ProcessedCount;
            ws.Cells[i + 2, 6].Value = r.ItemCount;
            ws.Cells[i + 2, 7].Value = r.BatchDate?.ToString("yyyy-MM-dd") ?? "";
            ws.Cells[i + 2, 8].Value = r.SubmittedDate.ToString("yyyy-MM-dd");
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي الدفعات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // COM-02: Pending Approvals
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<PendingApprovalRow>>> GetPendingApprovalsAsync(ReportFilter filter)
    {
        var query = _unitOfWork.InvestmentApprovalRequests.Query()
            .AsNoTracking()
            .Where(a => a.Status == AuditStatus.Pending && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(a => a.EntityType != null && a.EntityType.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var approvals = await query
            .Include(a => a.RequestedBy)
            .OrderByDescending(a => a.CreatedAt)
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

    public async Task<byte[]> ExportPendingApprovalsAsync(ReportFilter filter, string format = "excel")
    {
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetPendingApprovalsAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<PendingApprovalRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("الموافقات المعلقة");
        var headers = new[] { "#", "نوع الطلب", "المشروع", "مقدم الطلب", "المبلغ", "أيام الانتظار", "تاريخ الإنشاء" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.RequestType;
            ws.Cells[i + 2, 3].Value = r.ProjectName;
            ws.Cells[i + 2, 4].Value = r.RequestedBy;
            ws.Cells[i + 2, 5].Value = r.Amount;
            ws.Cells[i + 2, 6].Value = r.DaysPending;
            ws.Cells[i + 2, 7].Value = r.CreatedAt.ToString("yyyy-MM-dd");
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي الموافقات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // ADM-01: Audit Log
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<AuditLogRow>>> GetAuditLogAsync(ReportFilter filter)
    {
        var query = _unitOfWork.AuditLogs.Query()
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

    public async Task<byte[]> ExportAuditLogAsync(ReportFilter filter, string format = "excel")
    {
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetAuditLogAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<AuditLogRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("سجل النظام");
        var headers = new[] { "#", "المستخدم", "الإجراء", "الجدول", "رقم السجل", "القيم القديمة", "القيم الجديدة", "الوقت", "IP" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.UserName;
            ws.Cells[i + 2, 3].Value = r.Action;
            ws.Cells[i + 2, 4].Value = r.TableName;
            ws.Cells[i + 2, 5].Value = r.RecordID;
            ws.Cells[i + 2, 6].Value = r.OldValues;
            ws.Cells[i + 2, 7].Value = r.NewValues;
            ws.Cells[i + 2, 8].Value = r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cells[i + 2, 9].Value = r.IPAddress;
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي السجلات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // ADM-02: User Activity
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<UserActivityRow>>> GetUserActivityAsync(ReportFilter filter)
    {
        var query = _unitOfWork.Users.Query()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(u => u.FullName.Contains(filter.Search) || u.Username.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(u => u.FullName)
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

    public async Task<byte[]> ExportUserActivityAsync(ReportFilter filter, string format = "excel")
    {
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetUserActivityAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<UserActivityRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("نشاط المستخدمين");
        var headers = new[] { "#", "اسم المستخدم", "الاسم", "الدور", "الفرع", "عدد الدخول", "آخر دخول", "نشط" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.UserName;
            ws.Cells[i + 2, 3].Value = r.FullName;
            ws.Cells[i + 2, 4].Value = r.RoleName;
            ws.Cells[i + 2, 5].Value = r.BranchName;
            ws.Cells[i + 2, 6].Value = r.LoginCount;
            ws.Cells[i + 2, 7].Value = r.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            ws.Cells[i + 2, 8].Value = r.IsActive ? "نعم" : "لا";
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المستخدمين: {data.Count}");
        return package.GetAsByteArray();
    }
}
