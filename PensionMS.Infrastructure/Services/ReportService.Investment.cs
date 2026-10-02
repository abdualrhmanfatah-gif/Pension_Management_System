using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services;

public partial class ReportService
{
    // ═══════════════════════════════════════
    // INV-01: Portfolio Performance
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<PortfolioPerformanceRow>>> GetPortfolioPerformanceAsync(ReportFilter filter)
    {
        var query = _unitOfWork.InvestmentPortfolios.Query()
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (filter.PortfolioId.HasValue)
            query = query.Where(p => p.PortfolioID == filter.PortfolioId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(p => p.Name)
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PortfolioId = filter.PortfolioId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetPortfolioPerformanceAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<PortfolioPerformanceRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("أداء المحافظ");
        var headers = new[] { "#", "المحفظة", "إجمالي الاستثمار", "القيمة الحالية", "الربح/الخسارة", "العائد ROI %", "المشاريع النشطة", "إجمالي الدخل", "إجمالي المصروفات" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.PortfolioName;
            ws.Cells[i + 2, 3].Value = r.TotalInvested;
            ws.Cells[i + 2, 4].Value = r.CurrentValue;
            ws.Cells[i + 2, 5].Value = r.ProfitLoss;
            ws.Cells[i + 2, 6].Value = Math.Round(r.ROI, 2);
            ws.Cells[i + 2, 7].Value = r.ActiveProjects;
            ws.Cells[i + 2, 8].Value = r.TotalIncome;
            ws.Cells[i + 2, 9].Value = r.TotalExpenses;
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المحافظ: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // INV-02: Occupancy & Vacancy
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<OccupancyRow>>> GetOccupancyReportAsync(ReportFilter filter)
    {
        var query = _unitOfWork.InvestmentProperties.Query()
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (filter.PropertyId.HasValue)
            query = query.Where(p => p.PropertyID == filter.PropertyId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.PropertyName.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(p => p.PropertyName)
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PropertyId = filter.PropertyId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetOccupancyReportAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<OccupancyRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("تقرير الإشغال");
        var headers = new[] { "#", "العقار", "المشروع", "إجمالي الوحدات", "مؤجرة", "شاغرة", "صيانة", "نسبة الإشغال %" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.PropertyName;
            ws.Cells[i + 2, 3].Value = r.ProjectName;
            ws.Cells[i + 2, 4].Value = r.TotalUnits;
            ws.Cells[i + 2, 5].Value = r.OccupiedUnits;
            ws.Cells[i + 2, 6].Value = r.VacantUnits;
            ws.Cells[i + 2, 7].Value = r.UnderMaintenance;
            ws.Cells[i + 2, 8].Value = Math.Round(r.OccupancyRate, 2);
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي العقارات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // INV-03: Rent Collection & Receivables
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<RentCollectionRow>>> GetRentCollectionAsync(ReportFilter filter)
    {
        var query = _unitOfWork.PaymentSchedules.Query()
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
            .OrderByDescending(ps => ps.DueDate)
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetRentCollectionAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<RentCollectionRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("تحصيل الإيجارات");
        var headers = new[] { "#", "المستأجر", "العقار", "الوحدة", "المبلغ المستحق", "المدفوع", "المتبقي", "أيام التأخير", "حالة الدفع" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.TenantName;
            ws.Cells[i + 2, 3].Value = r.PropertyName;
            ws.Cells[i + 2, 4].Value = r.UnitNumber;
            ws.Cells[i + 2, 5].Value = r.DueAmount;
            ws.Cells[i + 2, 6].Value = r.PaidAmount;
            ws.Cells[i + 2, 7].Value = r.OutstandingAmount;
            ws.Cells[i + 2, 8].Value = r.OverdueDays;
            ws.Cells[i + 2, 9].Value = r.PaymentStatus;
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي الدفعات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // INV-04: Active & Expiring Contracts
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<ContractStatusRow>>> GetContractStatusAsync(ReportFilter filter)
    {
        var query = _unitOfWork.LeaseContracts.Query()
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (filter.StatusId.HasValue)
            query = query.Where(c => c.Status == (filter.StatusId.Value == 1 ? "Active" : "Expired"));

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(c =>
                (c.Tenant != null && c.Tenant.FullName.Contains(filter.Search)));

        var totalCount = await query.CountAsync();
        var contracts = await query
            .OrderByDescending(c => c.EndDate)
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            StatusId = filter.StatusId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetContractStatusAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<ContractStatusRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("حالة العقود");
        var headers = new[] { "#", "المستأجر", "العقار", "الوحدة", "حالة العقد", "تاريخ البداية", "تاريخ النهاية", "القيمة السنوية", "أيام حتى الانتهاء" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.TenantName;
            ws.Cells[i + 2, 3].Value = r.PropertyName;
            ws.Cells[i + 2, 4].Value = r.UnitNumber;
            ws.Cells[i + 2, 5].Value = r.Status;
            ws.Cells[i + 2, 6].Value = r.StartDate.ToString("yyyy-MM-dd");
            ws.Cells[i + 2, 7].Value = r.EndDate.ToString("yyyy-MM-dd");
            ws.Cells[i + 2, 8].Value = r.AnnualRent;
            ws.Cells[i + 2, 9].Value = r.DaysToExpiry;
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي العقود: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // INV-05: Risk Exposure
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<RiskExposureRow>>> GetRiskExposureAsync(ReportFilter filter)
    {
        var query = _unitOfWork.InvestmentRiskAssessments.Query()
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (filter.ProjectId.HasValue)
            query = query.Where(r => r.ProjectID == filter.ProjectId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(r => r.RiskType.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderByDescending(r => r.AssessedDate)
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            ProjectId = filter.ProjectId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetRiskExposureAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<RiskExposureRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("المخاطر");
        var headers = new[] { "#", "المشروع", "نوع الخطر", "الخطورة", "الحالة", "تاريخ التقييم" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.ProjectName;
            ws.Cells[i + 2, 3].Value = r.RiskType;
            ws.Cells[i + 2, 4].Value = r.Severity;
            ws.Cells[i + 2, 5].Value = r.Status;
            ws.Cells[i + 2, 6].Value = r.IdentifiedDate?.ToString("yyyy-MM-dd") ?? "";
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي المخاطر: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // INV-06: Maintenance & Work Orders
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<MaintenanceSummaryRow>>> GetMaintenanceSummaryAsync(ReportFilter filter)
    {
        var query = _unitOfWork.MaintenanceRequests.Query()
            .AsNoTracking()
            .Where(m => !m.IsDeleted);

        if (filter.PropertyId.HasValue)
            query = query.Where(m => m.PropertyID == filter.PropertyId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(m => m.Description != null && m.Description.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var requests = await query
            .OrderByDescending(m => m.ReportedDate)
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PropertyId = filter.PropertyId,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetMaintenanceSummaryAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<MaintenanceSummaryRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("طلبات الصيانة");
        var headers = new[] { "#", "العقار", "الوصف", "الأولوية", "الحالة", "التكلفة التقديرية", "التكلفة الفعلية", "تاريخ الطلب" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.PropertyName;
            ws.Cells[i + 2, 3].Value = r.Description;
            ws.Cells[i + 2, 4].Value = r.Priority;
            ws.Cells[i + 2, 5].Value = r.Status;
            ws.Cells[i + 2, 6].Value = r.EstimatedCost;
            ws.Cells[i + 2, 7].Value = r.ActualCost;
            ws.Cells[i + 2, 8].Value = r.ReportedDate.ToString("yyyy-MM-dd");
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي الطلبات: {data.Count}");
        return package.GetAsByteArray();
    }

    // ═══════════════════════════════════════
    // INV-07: Budget vs Actual
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<BudgetVsActualRow>>> GetBudgetVsActualAsync(ReportFilter filter)
    {
        var query = _unitOfWork.InvestmentPlans.Query()
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (filter.FiscalYear.HasValue)
            query = query.Where(p => p.FiscalYearID == filter.FiscalYear.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Project != null && p.Project.Name.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(p => p.Project != null ? p.Project.Name : "")
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
        var pagedFilter = new ReportFilter
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            FiscalYear = filter.FiscalYear,
            Search = filter.Search,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await GetBudgetVsActualAsync(pagedFilter);
        var data = result.Data?.Items ?? new List<BudgetVsActualRow>();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("الميزانية");
        var headers = new[] { "#", "المشروع", "السنة المالية", "المبلغ المخطط", "المنصرف الفعلي", "الفرق", "نسبة الانحراف %" };
        for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];
        StyleHeader(ws, 1, headers.Length);

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cells[i + 2, 1].Value = i + 1;
            ws.Cells[i + 2, 2].Value = r.ProjectName;
            ws.Cells[i + 2, 3].Value = r.FiscalYear;
            ws.Cells[i + 2, 4].Value = r.PlannedAmount;
            ws.Cells[i + 2, 5].Value = r.ActualSpent;
            ws.Cells[i + 2, 6].Value = r.Variance;
            ws.Cells[i + 2, 7].Value = Math.Round(r.VariancePct, 2);
        }
        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Count + 2, headers.Length, $"إجمالي السجلات: {data.Count}");
        return package.GetAsByteArray();
    }
}
