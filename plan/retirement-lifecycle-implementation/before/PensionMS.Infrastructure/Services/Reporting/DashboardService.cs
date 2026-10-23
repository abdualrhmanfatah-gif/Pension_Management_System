using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReportingQueries _queries;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "DashboardStats";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DashboardService(IUnitOfWork unitOfWork, IMemoryCache cache, IBranchScopeProvider branchScope)
    {
        _unitOfWork = unitOfWork;
        _queries = new ReportingQueries(unitOfWork, branchScope);
        _cache = cache;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetStatsAsync()
    {
        var branch = _queries.ResolveBranch();
        var cacheKey = CacheKey + ":" + (branch?.ToString() ?? "HQ");
        if (_cache.TryGetValue<DashboardStatsDto>(cacheKey, out var cached))
            return ApiResponse<DashboardStatsDto>.Success(cached);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var yearStart = new DateTime(now.Year, 1, 1);

        var totalRetirees = await _queries.Query(_unitOfWork.Retirees)
            .AsNoTracking().Where(r => !r.IsDeleted).CountAsync();

        var activeFilesCount = await _queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking().Where(f => !f.IsDeleted && (f.StatusID == FileStatusConstants.New || f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.Incomplete)).CountAsync();

        var closedFilesCount = await _queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking().Where(f => !f.IsDeleted && f.StatusID == FileStatusConstants.Suspended).CountAsync();

        var rejectedFilesCount = await _queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking().Where(f => !f.IsDeleted && f.StatusID == FileStatusConstants.Closed).CountAsync();

        var totalEntitled = await _queries.Query(_unitOfWork.EntitledPersons)
            .AsNoTracking().Where(e => !e.IsDeleted).CountAsync();

        var monthlySalaryPayments = await _queries.Query(_unitOfWork.SalaryPaymentItems)
            .AsNoTracking().Where(p => !p.IsDeleted && p.CreatedAt >= monthStart).SumAsync(p => (decimal?)p.Amount);

        var monthlyOpExpenses = await _queries.Query(_unitOfWork.OperationalExpenses)
            .AsNoTracking().Where(e => !e.IsDeleted && e.ExpenseDate >= monthStart).SumAsync(e => (decimal?)e.Amount);

        var activeUsers = await _queries.Query(_unitOfWork.Users)
            .AsNoTracking().Where(u => u.IsActive).CountAsync();

        var activeBranches = await _queries.Query(_unitOfWork.PensionFundBranches)
            .AsNoTracking().Where(b => b.IsActive).CountAsync();

        var draftBatchesCount = await _queries.Query(_unitOfWork.SalaryPaymentBatches)
            .AsNoTracking().Where(b => !b.IsDeleted && b.Status == BatchStatusConstants.Draft).CountAsync();

        var approvedBatchesCount = await _queries.Query(_unitOfWork.SalaryPaymentBatches)
            .AsNoTracking().Where(b => !b.IsDeleted && b.Status == BatchStatusConstants.Approved).CountAsync();

        var totalSalaryPaidThisYear = await _queries.Query(_unitOfWork.SalaryPaymentItems)
            .AsNoTracking().Where(p => !p.IsDeleted && p.CreatedAt >= yearStart).SumAsync(p => (decimal?)p.Amount);

        var totalOpExpensesThisYear = await _queries.Query(_unitOfWork.OperationalExpenses)
            .AsNoTracking().Where(e => !e.IsDeleted && e.ExpenseDate >= yearStart).SumAsync(e => (decimal?)e.Amount);

        var activeEntitlements = await _queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking().Where(e => !e.IsDeleted && e.EffectiveDate <= now && (e.EndDate == null || e.EndDate > now)).CountAsync();

        var pendingSettlementsCount = await _queries.Query(_unitOfWork.SettlementsBatches)
            .AsNoTracking().Where(b => !b.IsDeleted && b.Status != BatchStatusConstants.Approved && b.Status != BatchStatusConstants.LegacyApproved).CountAsync();

        var approvedSettlementsCount = await _queries.Query(_unitOfWork.SettlementsBatches)
            .AsNoTracking().Where(b => !b.IsDeleted && (b.Status == BatchStatusConstants.Approved || b.Status == BatchStatusConstants.LegacyApproved) && b.CreatedAt >= monthStart).CountAsync();

        var monthlyPayments = (monthlySalaryPayments ?? 0m) + (monthlyOpExpenses ?? 0m);
        var totalPaidThisYear = (totalSalaryPaidThisYear ?? 0m) + (totalOpExpensesThisYear ?? 0m);

        var stats = new DashboardStatsDto
        {
            TotalRetirees = totalRetirees,
            ActiveFilesCount = activeFilesCount,
            ClosedFilesCount = closedFilesCount,
            RejectedFilesCount = rejectedFilesCount,
            TotalEntitledPersons = totalEntitled,
            TotalMonthlyPayments = monthlyPayments,
            ActiveUsersCount = activeUsers,
            ActiveBranchesCount = activeBranches,
            DraftBatchesCount = draftBatchesCount,
            ApprovedBatchesCount = approvedBatchesCount,
            TotalPaidThisYear = totalPaidThisYear,
            ActiveEntitlementsCount = activeEntitlements,
            PendingSettlementsCount = pendingSettlementsCount,
            ApprovedSettlementsCount = approvedSettlementsCount,
            TotalBudgetAmount = 0m,
            UsedBudgetAmount = 0m,
        };

        _cache.Set(cacheKey, stats, CacheDuration);

        return ApiResponse<DashboardStatsDto>.Success(stats);
    }
}
