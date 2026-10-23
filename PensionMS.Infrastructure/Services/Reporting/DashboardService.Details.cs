using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class DashboardService
{
    public async Task<ApiResponse<PensionStatisticsDto>> GetPensionStatisticsAsync()
    {
        var queries = await _access.ForPermissionAsync("Retiree.View");
        var files = queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking()
            .Where(f => !f.IsDeleted);

        var totalRetirees = await queries.Query(_unitOfWork.Retirees)
            .AsNoTracking().CountAsync(r => !r.IsDeleted);

        var statusCounts = await files
            .GroupBy(f => f.StatusID)
            .Select(g => new { StatusID = g.Key, Count = g.Count() })
            .ToListAsync();
        var totalFiles = statusCounts.Sum(c => c.Count);
        int CountStatus(int statusId) => statusCounts.Where(c => c.StatusID == statusId).Sum(c => c.Count);
        double pct(int count) => totalFiles > 0 ? Math.Round((double)count / totalFiles * 100, 1) : 0;

        var activeFiles = CountStatus(FileStatusConstants.Active);
        var suspendedFiles = await queries.Query(_unitOfWork.PensionSuspensions).AsNoTracking()
            .Where(s => !s.IsDeleted &&
                (s.Status == SuspensionStatusConstants.Active || s.Status == SuspensionStatusConstants.CodeActive) &&
                files.Select(f => f.FileID).Contains(s.FileID))
            .Select(s => s.FileID).Distinct().CountAsync();
        var closedFiles = CountStatus(FileStatusConstants.EntitlementEnded);
        var newFiles = CountStatus(FileStatusConstants.New);
        var underReviewFiles = CountStatus(FileStatusConstants.Pending);

        var activeEntitled = await queries.Query(_unitOfWork.EntitledPersons)
            .AsNoTracking().CountAsync(e => !e.IsDeleted);

        var activeAgents = await queries.Query(_unitOfWork.Agents)
            .AsNoTracking().CountAsync(a => !a.IsDeleted && a.IsActive);

        var activeGuardians = await queries.Query(_unitOfWork.Guardians)
            .AsNoTracking().CountAsync(g => !g.IsDeleted && g.IsActive);

        var totalMonthlyAmount = await queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.EndDate == null)
            .SumAsync(e => (decimal?)e.Amount) ?? 0m;

        var rankDist = (await files
            .GroupBy(f => f.Rank!.RankName ?? "بدون رتبة")
            .Select(g => new { RankName = g.Key, Count = g.Count() })
            .ToListAsync())
            .Select(x => new RankDistributionDto { RankName = x.RankName, Count = x.Count, Percentage = pct(x.Count) })
            .OrderByDescending(x => x.Count).ToList();

        var statusColors = new Dictionary<int, string>
        {
            { FileStatusConstants.New, "#3b82f6" },
            { FileStatusConstants.Active, "#10b981" },
            { FileStatusConstants.EntitlementEnded, "#6b7280" },
            { FileStatusConstants.Pending, "#8b5cf6" },
        };

        var statusDist = new List<FileStatusDistributionDto>();
        var allStatuses = new Dictionary<int, string>
        {
            { FileStatusConstants.New, "جديد" },
            { FileStatusConstants.Active, "نشط" },
            { FileStatusConstants.EntitlementEnded, "انتهى استحقاقه" },
            { FileStatusConstants.Pending, "معلق" },
        };

        foreach (var status in allStatuses)
        {
            var count = CountStatus(status.Key);
            statusDist.Add(new FileStatusDistributionDto
            {
                StatusName = status.Value,
                Count = count,
                Percentage = pct(count),
                Color = statusColors[status.Key]
            });
        }

        statusDist = statusDist.OrderByDescending(x => x.Count).ToList();

        var allGovernorates = await queries.Query(_unitOfWork.Retirees)
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Governorate != null && r.Governorate != "")
            .Select(r => r.Governorate!)
            .Distinct()
            .ToListAsync();

        var govCounts = (await files
            .Where(f => f.Retiree.Governorate != null && f.Retiree.Governorate != "")
            .GroupBy(f => f.Retiree.Governorate!)
            .Select(g => new { Governorate = g.Key, Count = g.Count() })
            .ToListAsync())
            .ToDictionary(x => x.Governorate, x => x.Count);

        var govDist = allGovernorates
            .Select(g => new GovernorateDistributionDto
            {
                Governorate = g,
                Count = govCounts.GetValueOrDefault(g),
                Percentage = pct(govCounts.GetValueOrDefault(g))
            })
            .OrderByDescending(x => x.Count).ToList();

        var catDist = (await files
            .Where(f => f.Retiree.ApprovalItem.Category != null)
            .GroupBy(f => f.Retiree.ApprovalItem.Category!.CategoryName)
            .Select(g => new { CategoryName = g.Key, Count = g.Count() })
            .ToListAsync())
            .Select(x => new CategoryDistributionDto { CategoryName = x.CategoryName, Count = x.Count, Percentage = pct(x.Count) })
            .OrderByDescending(x => x.Count).ToList();

        var branchCounts = (await files
            .GroupBy(f => f.PensionBranchID)
            .Select(g => new { BranchID = g.Key, Count = g.Count() })
            .ToListAsync())
            .ToDictionary(x => x.BranchID, x => x.Count);

        var branchDist = (await queries.Query(_unitOfWork.PensionFundBranches)
            .AsNoTracking()
            .Where(b => b.IsActive)
            .Select(b => new { b.PensionBranchID, b.BranchName })
            .ToListAsync())
            .Select(b => new BranchDistributionDto
            {
                BranchName = b.BranchName,
                Count = branchCounts.GetValueOrDefault(b.PensionBranchID),
                Percentage = pct(branchCounts.GetValueOrDefault(b.PensionBranchID))
            })
            .OrderByDescending(x => x.Count).ToList();

        var settlementGroups = await queries.Query(_unitOfWork.SettlementItems)
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Retiree != null && !s.Retiree.IsDeleted)
            .GroupBy(s => s.Retiree!.Governorate)
            .Select(g => new
            {
                Governorate = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(s => s.Amount - (s.Deductions ?? 0m))
            })
            .ToListAsync();

        var totalSettlements = settlementGroups.Sum(x => x.Count);
        double settlementPct(int count) => totalSettlements > 0 ? Math.Round((double)count / totalSettlements * 100, 1) : 0;

        var settlementByGov = settlementGroups
            .Where(x => x.Governorate != null)
            .Select(x => new SettlementByGovernorateDto
            {
                Governorate = x.Governorate!,
                Count = x.Count,
                TotalAmount = x.TotalAmount,
                Percentage = settlementPct(x.Count)
            })
            .OrderByDescending(x => x.Count).ToList();

        var stats = new PensionStatisticsDto
        {
            TotalRetirees = totalRetirees,
            ActiveFiles = activeFiles,
            SuspendedFiles = suspendedFiles,
            ClosedFiles = closedFiles,
            NewFiles = newFiles,
            UnderReviewFiles = underReviewFiles,
            ActiveEntitled = activeEntitled,
            ActiveAgents = activeAgents,
            ActiveGuardians = activeGuardians,
            TotalMonthlyAmount = totalMonthlyAmount,
            RankDistribution = rankDist,
            FileStatusDistribution = statusDist,
            GovernorateDistribution = govDist,
            CategoryDistribution = catDist,
            BranchDistribution = branchDist,
            SettlementByGovernorate = settlementByGov,
        };

        return ApiResponse<PensionStatisticsDto>.Success(stats);
    }

    public async Task<ApiResponse<PerformanceStatisticsDto>> GetPerformanceStatisticsAsync()
    {
        var queries = await _access.ForPermissionAsync("Retiree.View");
        var files = queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking()
            .Where(f => !f.IsDeleted);

        var totalRetirees = await queries.Query(_unitOfWork.Retirees)
            .AsNoTracking().CountAsync(r => !r.IsDeleted);

        var statusCounts = await files
            .GroupBy(f => f.StatusID)
            .Select(g => new { StatusID = g.Key, Count = g.Count() })
            .ToListAsync();
        int CountStatus(int statusId) => statusCounts.Where(c => c.StatusID == statusId).Sum(c => c.Count);

        // Arabic month names
        var monthNames = new[] { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

        var retireesByMonth = (await queries.Query(_unitOfWork.Retirees).AsNoTracking()
            .Where(r => !r.IsDeleted).GroupBy(r => r.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync())
            .ToDictionary(x => x.Month, x => x.Count);

        var filesByMonth = (await files
            .GroupBy(f => f.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync())
            .ToDictionary(x => x.Month, x => x.Count);

        var newFilesByMonth = (await files
            .Where(f => f.StatusID == FileStatusConstants.New)
            .GroupBy(f => f.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync())
            .ToDictionary(x => x.Month, x => x.Count);

        var monthlyData = new List<MonthlyPerformanceItemDto>();
        for (int i = 1; i <= 12; i++)
        {
            monthlyData.Add(new MonthlyPerformanceItemDto
            {
                Month = monthNames[i - 1],
                Retirees = retireesByMonth.GetValueOrDefault(i),
                Files = filesByMonth.GetValueOrDefault(i),
                NewFiles = newFilesByMonth.GetValueOrDefault(i)
            });
        }

        var stats = new PerformanceStatisticsDto
        {
            TotalRetirees = totalRetirees,
            ActiveFiles = CountStatus(FileStatusConstants.Active),
            UnderReviewFiles = CountStatus(FileStatusConstants.Pending),
            NewFiles = CountStatus(FileStatusConstants.New),
            MonthlyData = monthlyData
        };

        return ApiResponse<PerformanceStatisticsDto>.Success(stats);
    }

    public async Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync()
    {
        var queries = await _access.ForPermissionAsync("Dashboard.View");
        var activities = await queries.Query(_unitOfWork.AuditLogs)
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.SystemTable)
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.LogID)
            .Take(10)
            .Select(a => new RecentActivityDto
            {
                LogID = a.LogID,
                UserName = a.User != null ? a.User.FullName : null,
                Action = a.Action,
                EntityName = a.SystemTable != null ? a.SystemTable.TableNameAr : null,
                EntityDisplayName = a.EntityDisplayName,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<RecentActivityDto>>.Success(activities);
    }
}
