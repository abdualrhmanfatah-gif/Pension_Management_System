using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "DashboardStats";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DashboardService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetStatsAsync()
    {
        if (_cache.TryGetValue<DashboardStatsDto>(CacheKey, out var cached))
            return ApiResponse<DashboardStatsDto>.Success(cached);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var yearStart = new DateTime(now.Year, 1, 1);

        var totalRetirees = await _unitOfWork.Retirees.Query()
            .AsNoTracking().Where(r => !r.IsDeleted).CountAsync();

        var activeFilesCount = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking().Where(f => !f.IsDeleted && (f.StatusID == FileStatusConstants.New || f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.Incomplete)).CountAsync();

        var closedFilesCount = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking().Where(f => !f.IsDeleted && f.StatusID == FileStatusConstants.Suspended).CountAsync();

        var rejectedFilesCount = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking().Where(f => !f.IsDeleted && f.StatusID == FileStatusConstants.Closed).CountAsync();

        var totalEntitled = await _unitOfWork.EntitledPersons.Query()
            .AsNoTracking().Where(e => !e.IsDeleted).CountAsync();

        var monthlySalaryPayments = await _unitOfWork.SalaryPaymentItems.Query()
            .AsNoTracking().Where(p => !p.IsDeleted && p.CreatedAt >= monthStart).SumAsync(p => (decimal?)p.Amount);

        var monthlyOpExpenses = await _unitOfWork.OperationalExpenses.Query()
            .AsNoTracking().Where(e => !e.IsDeleted && e.ExpenseDate >= monthStart).SumAsync(e => (decimal?)e.Amount);

        var activeUsers = await _unitOfWork.Users.Query()
            .AsNoTracking().Where(u => u.IsActive).CountAsync();

        var activeBranches = await _unitOfWork.PensionFundBranches.Query()
            .AsNoTracking().Where(b => b.IsActive).CountAsync();

        var draftBatchesCount = await _unitOfWork.SalaryPaymentBatches.Query()
            .AsNoTracking().Where(b => !b.IsDeleted && b.Status == BatchStatusConstants.Draft).CountAsync();

        var approvedBatchesCount = await _unitOfWork.SalaryPaymentBatches.Query()
            .AsNoTracking().Where(b => !b.IsDeleted && b.Status == BatchStatusConstants.Approved).CountAsync();

        var totalSalaryPaidThisYear = await _unitOfWork.SalaryPaymentItems.Query()
            .AsNoTracking().Where(p => !p.IsDeleted && p.CreatedAt >= yearStart).SumAsync(p => (decimal?)p.Amount);

        var totalOpExpensesThisYear = await _unitOfWork.OperationalExpenses.Query()
            .AsNoTracking().Where(e => !e.IsDeleted && e.ExpenseDate >= yearStart).SumAsync(e => (decimal?)e.Amount);

        var activeEntitlements = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking().Where(e => !e.IsDeleted && e.EffectiveDate <= now && (e.EndDate == null || e.EndDate > now)).CountAsync();

        var pendingSettlementsCount = await _unitOfWork.SettlementsBatches.Query()
            .AsNoTracking().Where(b => !b.IsDeleted && b.Status != BatchStatusConstants.Approved && b.Status != BatchStatusConstants.LegacyApproved).CountAsync();

        var approvedSettlementsCount = await _unitOfWork.SettlementsBatches.Query()
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

        _cache.Set(CacheKey, stats, CacheDuration);

        return ApiResponse<DashboardStatsDto>.Success(stats);
    }
}
