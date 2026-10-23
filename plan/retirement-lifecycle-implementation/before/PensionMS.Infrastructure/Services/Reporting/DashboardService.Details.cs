using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class DashboardService
{
    public async Task<ApiResponse<PensionStatisticsDto>> GetPensionStatisticsAsync()
    {
        var files = _queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking()
            .Include(f => f.Retiree)
                .ThenInclude(r => r!.ApprovalItem)
                    .ThenInclude(a => a!.Category)
            .Include(f => f.Retiree)
                .ThenInclude(r => r!.ApprovalItem)
                    .ThenInclude(a => a!.Batch)
            .Include(f => f.Rank)
            .Include(f => f.Status)
            .Include(f => f.PensionBranch)
            .Where(f => !f.IsDeleted);

        var totalRetirees = await _queries.Query(_unitOfWork.Retirees)
            .AsNoTracking().CountAsync(r => !r.IsDeleted);

        var allFiles = await files.ToListAsync();
        var totalFiles = allFiles.Count;
        double pct(int count) => totalFiles > 0 ? Math.Round((double)count / totalFiles * 100, 1) : 0;

        var activeFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.New);
        var suspendedFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.Suspended);
        var closedFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.Closed);
        var newFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.New);
        var underReviewFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.UnderReview || f.StatusID == FileStatusConstants.Pending);

        var activeEntitled = await _queries.Query(_unitOfWork.EntitledPersons)
            .AsNoTracking().CountAsync(e => !e.IsDeleted);

        var activeAgents = await _queries.Query(_unitOfWork.Agents)
            .AsNoTracking().CountAsync(a => !a.IsDeleted && a.IsActive);

        var activeGuardians = await _queries.Query(_unitOfWork.Guardians)
            .AsNoTracking().CountAsync(g => !g.IsDeleted && g.IsActive);

        var totalMonthlyAmount = await _queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.EndDate == null)
            .SumAsync(e => (decimal?)e.Amount) ?? 0m;

        var rankDist = allFiles
            .GroupBy(f => f.Rank?.RankName ?? "بدون رتبة")
            .Select(g => new RankDistributionDto { RankName = g.Key, Count = g.Count(), Percentage = pct(g.Count()) })
            .OrderByDescending(x => x.Count).ToList();

        var statusColors = new Dictionary<int, string>
        {
            { FileStatusConstants.New, "#3b82f6" },
            { FileStatusConstants.Active, "#10b981" },
            { FileStatusConstants.Suspended, "#f59e0b" },
            { FileStatusConstants.Closed, "#6b7280" },
            { FileStatusConstants.Incomplete, "#f97316" },
            { FileStatusConstants.Pending, "#8b5cf6" },
            { FileStatusConstants.UnderReview, "#06b6d4" },
            { FileStatusConstants.Rejected, "#ef4444" },
        };

        var statusDist = new List<FileStatusDistributionDto>();
        var allStatuses = new Dictionary<int, string>
        {
            { FileStatusConstants.New, "جديد" },
            { FileStatusConstants.Active, "نشط" },
            { FileStatusConstants.Suspended, "موقوف" },
            { FileStatusConstants.Closed, "مغلق" },
            { FileStatusConstants.Incomplete, "ناقص" },
            { FileStatusConstants.Pending, "معلق" },
            { FileStatusConstants.UnderReview, "تحت المراجعة" },
            { FileStatusConstants.Rejected, "مرفوض" },
        };

        foreach (var status in allStatuses)
        {
            var count = allFiles.Count(f => f.StatusID == status.Key);
            statusDist.Add(new FileStatusDistributionDto
            {
                StatusName = status.Value,
                Count = count,
                Percentage = pct(count),
                Color = statusColors[status.Key]
            });
        }

        statusDist = statusDist.OrderByDescending(x => x.Count).ToList();

        var allGovernorates = await _queries.Query(_unitOfWork.Retirees)
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Governorate != null && r.Governorate != "")
            .Select(r => r.Governorate!)
            .Distinct()
            .ToListAsync();

        var govDist = allGovernorates
            .Select(g => new GovernorateDistributionDto
            {
                Governorate = g,
                Count = allFiles.Count(f => f.Retiree?.Governorate == g),
                Percentage = pct(allFiles.Count(f => f.Retiree?.Governorate == g))
            })
            .OrderByDescending(x => x.Count).ToList();

        var catDist = allFiles
            .Where(f => f.Retiree?.ApprovalItem?.Category != null)
            .GroupBy(f => f.Retiree!.ApprovalItem!.Category!.CategoryName)
            .Select(g => new CategoryDistributionDto { CategoryName = g.Key, Count = g.Count(), Percentage = pct(g.Count()) })
            .OrderByDescending(x => x.Count).ToList();

        var branchDist = _queries.Query(_unitOfWork.PensionFundBranches)
            .AsNoTracking()
            .Where(b => b.IsActive)
            .ToList()
            .Select(b => new BranchDistributionDto
            {
                BranchName = b.BranchName,
                Count = allFiles.Count(f => f.PensionBranchID == b.PensionBranchID),
                Percentage = pct(allFiles.Count(f => f.PensionBranchID == b.PensionBranchID))
            })
            .OrderByDescending(x => x.Count).ToList();

        var settlementItems = await _queries.Query(_unitOfWork.SettlementItems)
            .AsNoTracking()
            .Include(s => s.Retiree)
            .Where(s => !s.IsDeleted && s.Retiree != null && !s.Retiree.IsDeleted)
            .ToListAsync();

        var totalSettlements = settlementItems.Count;
        double settlementPct(int count) => totalSettlements > 0 ? Math.Round((double)count / totalSettlements * 100, 1) : 0;

        var settlementByGov = settlementItems
            .Where(s => s.Retiree?.Governorate != null)
            .GroupBy(s => s.Retiree!.Governorate!)
            .Select(g => new SettlementByGovernorateDto
            {
                Governorate = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(s => s.NetAmount),
                Percentage = settlementPct(g.Count())
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
        var files = _queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Status)
            .Where(f => !f.IsDeleted);

        var totalRetirees = await _queries.Query(_unitOfWork.Retirees)
            .AsNoTracking().CountAsync(r => !r.IsDeleted);

        var allFiles = await files.ToListAsync();
        var totalFiles = allFiles.Count;

        var activeFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.New);
        var newFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.New);
        var underReviewFiles = allFiles.Count(f => f.StatusID == FileStatusConstants.UnderReview || f.StatusID == FileStatusConstants.Pending);

        // Arabic month names
        var monthNames = new[] { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
        var currentMonth = DateTime.UtcNow.Month;

        var retireesByMonth = await _queries.Query(_unitOfWork.Retirees).AsNoTracking()
            .Where(r => !r.IsDeleted).GroupBy(r => r.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Month, g => g.Count);
        var monthlyData = new List<MonthlyPerformanceItemDto>();
        for (int i = 1; i <= 12; i++)
        {
            var monthFiles = allFiles.Where(f => f.CreatedAt.Month == i).ToList();
            var monthRetirees = retireesByMonth.GetValueOrDefault(i);

            monthlyData.Add(new MonthlyPerformanceItemDto
            {
                Month = monthNames[i - 1],
                Retirees = monthRetirees,
                Files = monthFiles.Count,
                NewFiles = monthFiles.Count(f => f.StatusID == FileStatusConstants.New)
            });
        }

        var stats = new PerformanceStatisticsDto
        {
            TotalRetirees = totalRetirees,
            ActiveFiles = activeFiles,
            UnderReviewFiles = underReviewFiles,
            NewFiles = newFiles,
            MonthlyData = monthlyData
        };

        return ApiResponse<PerformanceStatisticsDto>.Success(stats);
    }

    public async Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync()
    {
        var activities = await _queries.Query(_unitOfWork.AuditLogs)
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
