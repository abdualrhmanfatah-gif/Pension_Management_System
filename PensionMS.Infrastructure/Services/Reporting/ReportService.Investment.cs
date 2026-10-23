using Microsoft.EntityFrameworkCore;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class ReportService
{
    // ═══════════════════════════════════════
    // INV-01: Portfolio Performance
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<PortfolioPerformanceRow>>> GetPortfolioPerformanceAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.InvestmentPortfolios, filter.BranchId)
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (filter.PortfolioId.HasValue)
            query = query.Where(p => p.PortfolioID == filter.PortfolioId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(p => p.Name).ThenBy(p => p.PortfolioID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new PortfolioPerformanceRow
            {
                PortfolioID = p.PortfolioID,
                PortfolioName = p.Name,
                TotalInvested = p.TotalTargetCapital,
                CurrentValue = 0m,
                ActiveProjects = p.Projects.Count(proj => proj.Status == ProjectStatusConstants.Active && !proj.IsDeleted),
                TotalProjects = p.Projects.Count(proj => !proj.IsDeleted),
                TotalIncome = 0m,
                TotalExpenses = 0m
            })
            .ToListAsync();

        return ApiResponse<PagedResult<PortfolioPerformanceRow>>.Success(new PagedResult<PortfolioPerformanceRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportPortfolioPerformanceAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PortfolioId = filter.PortfolioId,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetPortfolioPerformanceAsync(pagedFilter);
        var data = ReportExportGuard.RequireComplete(result);
        var headers = new[] { "#", "المحفظة", "إجمالي الاستثمار", "القيمة الحالية", "الربح/الخسارة", "العائد ROI %", "المشاريع النشطة", "إجمالي الدخل", "إجمالي المصروفات" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.PortfolioName,
            r.TotalInvested,
            r.CurrentValue,
            r.ProfitLoss,
            Math.Round(r.ROI, 2),
            r.ActiveProjects,
            r.TotalIncome,
            r.TotalExpenses,
        }).ToArray();
        return _excelExporter.ExportRows("أداء المحافظ", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي المحافظ: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // INV-02: Occupancy & Vacancy
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<OccupancyRow>>> GetOccupancyReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.InvestmentProperties, filter.BranchId)
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (filter.PropertyId.HasValue)
            query = query.Where(p => p.PropertyID == filter.PropertyId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.PropertyName.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(p => p.PropertyName).ThenBy(p => p.PropertyID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new OccupancyRow
            {
                PropertyID = p.PropertyID,
                PropertyName = p.PropertyName,
                ProjectName = p.Project != null ? p.Project.Name : "",
                TotalUnits = p.Units.Count(u => !u.IsDeleted),
                OccupiedUnits = p.Units.Count(u => u.Status == UnitStatusConstants.Leased && !u.IsDeleted),
                VacantUnits = p.Units.Count(u => (u.Status == UnitStatusConstants.Vacant || u.Status == UnitStatusConstants.Available) && !u.IsDeleted),
                UnderMaintenance = p.Units.Count(u => u.Status == UnitStatusConstants.Maintenance && !u.IsDeleted),
                OccupancyRate = p.Units.Any(u => !u.IsDeleted) ? (double)p.Units.Count(u => u.Status == UnitStatusConstants.Leased && !u.IsDeleted) / p.Units.Count(u => !u.IsDeleted) * 100.0 : 0.0,
                EstimatedLostRevenue = 0m
            })
            .ToListAsync();

        return ApiResponse<PagedResult<OccupancyRow>>.Success(new PagedResult<OccupancyRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportOccupancyReportAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PropertyId = filter.PropertyId,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetOccupancyReportAsync(pagedFilter);
        var data = ReportExportGuard.RequireComplete(result);
        var headers = new[] { "#", "العقار", "المشروع", "إجمالي الوحدات", "مؤجرة", "شاغرة", "صيانة", "نسبة الإشغال %" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.PropertyName,
            r.ProjectName,
            r.TotalUnits,
            r.OccupiedUnits,
            r.VacantUnits,
            r.UnderMaintenance,
            Math.Round(r.OccupancyRate, 2),
        }).ToArray();
        return _excelExporter.ExportRows("تقرير الإشغال", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي العقارات: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // INV-03: Rent Collection & Receivables
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<RentCollectionRow>>> GetRentCollectionAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.PaymentSchedules, filter.BranchId)
            .AsNoTracking()
            .Where(ps => !ps.IsDeleted);

        if (filter.FromDate.HasValue)
            query = query.Where(ps => ps.DueDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(ps => ps.DueDate < filter.ToDate.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(ps =>
                (ps.LeaseContract != null && ps.LeaseContract.Tenant != null && ps.LeaseContract.Tenant.FullName.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var schedules = await query
            .OrderByDescending(ps => ps.DueDate).ThenBy(ps => ps.ScheduleID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(ps => new
            {
                ps.ContractID,
                TenantName = ps.LeaseContract != null && ps.LeaseContract.Tenant != null ? ps.LeaseContract.Tenant.FullName : "",
                PropertyName = ps.LeaseContract != null && ps.LeaseContract.Unit != null && ps.LeaseContract.Unit.Property != null ? ps.LeaseContract.Unit.Property.PropertyName : "",
                UnitNumber = ps.LeaseContract != null && ps.LeaseContract.Unit != null ? ps.LeaseContract.Unit.UnitNumber : "",
                DueAmount = ps.ExpectedAmount,
                PaidAmount = ps.PaidAmount,
                ps.DueDate,
                ps.Status
            })
            .ToListAsync();

        var data = schedules.Select(ps => {
            decimal outstanding = ps.DueAmount - ps.PaidAmount;
            int overdueDays = outstanding > 0 && ps.DueDate < DateTime.UtcNow ? (int)(DateTime.UtcNow - ps.DueDate).TotalDays : 0;
            return new RentCollectionRow
            {
                ContractID = ps.ContractID,
                TenantName = ps.TenantName,
                PropertyName = ps.PropertyName,
                UnitNumber = ps.UnitNumber,
                DueAmount = ps.DueAmount,
                PaidAmount = ps.PaidAmount,
                OverdueDays = overdueDays,
                PaymentStatus = ps.Status
            };
        }).ToList();

        return ApiResponse<PagedResult<RentCollectionRow>>.Success(new PagedResult<RentCollectionRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportRentCollectionAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetRentCollectionAsync(pagedFilter);
        var data = ReportExportGuard.RequireComplete(result);
        var headers = new[] { "#", "المستأجر", "العقار", "الوحدة", "المبلغ المستحق", "المدفوع", "المتبقي", "أيام التأخير", "حالة الدفع" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.TenantName,
            r.PropertyName,
            r.UnitNumber,
            r.DueAmount,
            r.PaidAmount,
            r.OutstandingAmount,
            r.OverdueDays,
            r.PaymentStatus,
        }).ToArray();
        return _excelExporter.ExportRows("تحصيل الإيجارات", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي الدفعات: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // INV-04: Active & Expiring Contracts
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<ContractStatusRow>>> GetContractStatusAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.LeaseContracts, filter.BranchId)
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (filter.StatusId.HasValue)
            query = query.Where(c => c.Status == (filter.StatusId.Value == 1 ? "Active" : "Expired"));

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(c =>
                (c.Tenant != null && c.Tenant.FullName.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var contracts = await query
            .OrderByDescending(c => c.EndDate).ThenBy(c => c.ContractID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new 
            {
                c.ContractID,
                TenantName = c.Tenant != null ? c.Tenant.FullName : "",
                PropertyName = c.Unit != null && c.Unit.Property != null ? c.Unit.Property.PropertyName : "",
                UnitNumber = c.Unit != null ? c.Unit.UnitNumber : "",
                Status = c.Status ?? "Active",
                c.StartDate,
                c.EndDate,
                MonthlyRent = c.TotalContractValue / 12m,
                AnnualRent = c.TotalContractValue
            })
            .ToListAsync();

        var data = contracts.Select(c => new ContractStatusRow
        {
            ContractID = c.ContractID,
            TenantName = c.TenantName,
            PropertyName = c.PropertyName,
            UnitNumber = c.UnitNumber,
            Status = c.Status,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            MonthlyRent = c.MonthlyRent,
            AnnualRent = c.AnnualRent,
            DaysToExpiry = (c.EndDate - DateTime.UtcNow).Days
        }).ToList();

        return ApiResponse<PagedResult<ContractStatusRow>>.Success(new PagedResult<ContractStatusRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportContractStatusAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            StatusId = filter.StatusId,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetContractStatusAsync(pagedFilter);
        var data = ReportExportGuard.RequireComplete(result);
        var headers = new[] { "#", "المستأجر", "العقار", "الوحدة", "حالة العقد", "تاريخ البداية", "تاريخ النهاية", "القيمة السنوية", "أيام حتى الانتهاء" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.TenantName,
            r.PropertyName,
            r.UnitNumber,
            r.Status,
            r.StartDate.ToString("yyyy-MM-dd"),
            r.EndDate.ToString("yyyy-MM-dd"),
            r.AnnualRent,
            r.DaysToExpiry,
        }).ToArray();
        return _excelExporter.ExportRows("حالة العقود", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي العقود: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // INV-05: Risk Exposure
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<RiskExposureRow>>> GetRiskExposureAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.InvestmentRiskAssessments, filter.BranchId)
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (filter.ProjectId.HasValue)
            query = query.Where(r => r.ProjectID == filter.ProjectId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(r => r.RiskType.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(r => r.AssessedDate).ThenBy(r => r.RiskID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new RiskExposureRow
            {
                RiskID = r.RiskID,
                ProjectName = r.Project != null ? r.Project.Name : "",
                RiskType = r.RiskType ?? "",
                Severity = r.RiskScore >= 8 ? "Critical" : r.RiskScore >= 5 ? "High" : "Medium",
                Status = "Active",
                MitigationPlan = r.MitigationPlan ?? "",
                IdentifiedDate = r.AssessedDate
            })
            .ToListAsync();

        return ApiResponse<PagedResult<RiskExposureRow>>.Success(new PagedResult<RiskExposureRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportRiskExposureAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            ProjectId = filter.ProjectId,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetRiskExposureAsync(pagedFilter);
        var data = ReportExportGuard.RequireComplete(result);
        var headers = new[] { "#", "المشروع", "نوع الخطر", "الخطورة", "الحالة", "تاريخ التقييم" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.ProjectName,
            r.RiskType,
            r.Severity,
            r.Status,
            r.IdentifiedDate?.ToString("yyyy-MM-dd") ?? "",
        }).ToArray();
        return _excelExporter.ExportRows("المخاطر", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي المخاطر: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // INV-06: Maintenance & Work Orders
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<MaintenanceSummaryRow>>> GetMaintenanceSummaryAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.MaintenanceRequests, filter.BranchId)
            .AsNoTracking()
            .Where(m => !m.IsDeleted);

        if (filter.PropertyId.HasValue)
            query = query.Where(m => m.PropertyID == filter.PropertyId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(m => m.Description != null && m.Description.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var requests = await query
            .OrderByDescending(m => m.ReportedDate).ThenBy(m => m.RequestID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(m => new 
            {
                m.RequestID,
                PropertyName = m.Property != null ? m.Property.PropertyName : "",
                m.Description,
                m.Priority,
                m.Status,
                m.ReportedDate,
                WorkOrder = m.WorkOrders.Where(wo => !wo.IsDeleted).FirstOrDefault()
            })
            .ToListAsync();

        var data = requests.Select(m => new MaintenanceSummaryRow
        {
            RequestID = m.RequestID,
            PropertyName = m.PropertyName,
            Description = m.Description ?? "",
            Priority = m.Priority ?? "Normal",
            Status = m.Status ?? "Open",
            EstimatedCost = m.WorkOrder?.EstimatedCost ?? 0m,
            ActualCost = m.WorkOrder?.ActualCost ?? 0m,
            ReportedDate = m.ReportedDate,
            ResponseDays = m.WorkOrder?.CompletionDate.HasValue == true ? (int)(m.WorkOrder.CompletionDate.Value - m.ReportedDate).TotalDays : 0
        }).ToList();

        return ApiResponse<PagedResult<MaintenanceSummaryRow>>.Success(new PagedResult<MaintenanceSummaryRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportMaintenanceSummaryAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PropertyId = filter.PropertyId,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetMaintenanceSummaryAsync(pagedFilter);
        var data = ReportExportGuard.RequireComplete(result);
        var headers = new[] { "#", "العقار", "الوصف", "الأولوية", "الحالة", "التكلفة التقديرية", "التكلفة الفعلية", "تاريخ الطلب" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.PropertyName,
            r.Description,
            r.Priority,
            r.Status,
            r.EstimatedCost,
            r.ActualCost,
            r.ReportedDate.ToString("yyyy-MM-dd"),
        }).ToArray();
        return _excelExporter.ExportRows("طلبات الصيانة", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي الطلبات: {data.Count}" });
    }

    // ═══════════════════════════════════════
    // INV-07: Budget vs Actual
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<BudgetVsActualRow>>> GetBudgetVsActualAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var query = queries.Query(_unitOfWork.InvestmentPlans, filter.BranchId)
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (filter.FiscalYear.HasValue)
            query = query.Where(p => p.FiscalYearID == filter.FiscalYear.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Project != null && p.Project.Name.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(p => p.Project != null ? p.Project.Name : "").ThenBy(p => p.PlanID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new BudgetVsActualRow
            {
                PlanID = p.PlanID,
                ProjectName = p.Project != null ? p.Project.Name : "",
                FiscalYear = p.FiscalYear != null ? p.FiscalYear.StartDate.Year : 0,
                PlannedAmount = p.PlannedAmount,
                ActualSpent = p.ActualSpent
            })
            .ToListAsync();

        return ApiResponse<PagedResult<BudgetVsActualRow>>.Success(new PagedResult<BudgetVsActualRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<byte[]> ExportBudgetVsActualAsync(ReportFilter filter, string format = "excel")
    {
        var queries = await _access.ForPermissionAsync("Report.Financial.View");
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            FiscalYear = filter.FiscalYear,
            Search = filter.Search,
            BranchId = filter.BranchId,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetBudgetVsActualAsync(pagedFilter);
        var data = ReportExportGuard.RequireComplete(result);
        var headers = new[] { "#", "المشروع", "السنة المالية", "المبلغ المخطط", "المنصرف الفعلي", "الفرق", "نسبة الانحراف %" };
        var rows = data.Select((r, i) => new object?[]
        {
            i + 1,
            r.ProjectName,
            r.FiscalYear,
            r.PlannedAmount,
            r.ActualSpent,
            r.Variance,
            Math.Round(r.VariancePct, 2),
        }).ToArray();
        return _excelExporter.ExportRows("الميزانية", headers, rows,
            new ExcelReportOptions { FooterText = $"إجمالي السجلات: {data.Count}" });
    }
}
