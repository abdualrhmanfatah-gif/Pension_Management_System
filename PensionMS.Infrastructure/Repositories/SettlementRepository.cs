using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Settlements;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Helpers;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Repositories;

public class SettlementRepository : ISettlementRepository
{
    private readonly PensionDbContext _context;

    public SettlementRepository(PensionDbContext context)
    {
        _context = context;
    }

    public async Task<List<SettlementBatchListDto>> GetAllBatchesAsync()
    {
        return await _context.Set<SettlementsBatch>()
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new SettlementBatchListDto
            {
                BatchID = b.BatchID,
                BatchNumber = b.BatchNumber,
                ReceivedDate = b.ReceivedDate,
                TotalCount = b.TotalCount,
                SourceRowsCount = b.SourceRowsCount,
                SkippedCount = b.SkippedCount,
                FailureReportJson = b.FailureReportJson,
                HasFailureReport = !string.IsNullOrEmpty(b.FailureReportJson),
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                Description = b.Description,
                CreatedByUserName = b.CreatedByUser != null ? b.CreatedByUser.FullName : "غير محدد",
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SettlementBatchListDto?> GetBatchByIdAsync(int id)
    {
        return await _context.Set<SettlementsBatch>()
            .AsNoTracking()
            .Where(b => b.BatchID == id && !b.IsDeleted)
            .Select(b => new SettlementBatchListDto
            {
                BatchID = b.BatchID,
                BatchNumber = b.BatchNumber,
                ReceivedDate = b.ReceivedDate,
                TotalCount = b.TotalCount,
                SourceRowsCount = b.SourceRowsCount,
                SkippedCount = b.SkippedCount,
                FailureReportJson = b.FailureReportJson,
                HasFailureReport = !string.IsNullOrEmpty(b.FailureReportJson),
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                Description = b.Description,
                CreatedByUserName = b.CreatedByUser != null ? b.CreatedByUser.FullName : "غير محدد",
                CreatedAt = b.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsBatchNumberExistsAsync(string batchNumber)
    {
        return await _context.Set<SettlementsBatch>()
            .AsNoTracking()
            .AnyAsync(b => b.BatchNumber == batchNumber && !b.IsDeleted);
    }

<<<<<<< HEAD
    // The earliest linked item selects batch metadata deterministically. Batch-scoped
    // calls use only the requested batch for both metadata and totals.
    private static IQueryable<SettlementDecisionDto> ProjectDecisions(IQueryable<SettlementDecision> query, int? batchId = null)
        => query.SelectMany(d => d.SettlementItems
            .Where(i => !i.IsDeleted && (!batchId.HasValue || i.BatchID == batchId))
            .OrderBy(i => i.SettlementItemID).Take(1).DefaultIfEmpty(), (d, first) => new SettlementDecisionDto
            {
                DecisionID = d.DecisionID,
                BatchID = first == null ? 0 : first.BatchID,
=======
    public async Task<List<SettlementDecisionDto>> GetAllDecisionsAsync()
    {
        return await _context.Set<SettlementDecision>()
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new SettlementDecisionDto
            {
                DecisionID = d.DecisionID,
                BatchID = d.SettlementItems.Any() ? d.SettlementItems.First().BatchID : 0,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                DecisionNumber = d.DecisionNumber,
                DecisionDate = d.DecisionDate,
                DecisionType = d.DecisionType,
                Notes = d.Notes,
<<<<<<< HEAD
                ItemCount = d.SettlementItems.Count(i => !i.IsDeleted && (!batchId.HasValue || i.BatchID == batchId)),
                SourceRowsCount = first == null ? 0 : first.Batch.SourceRowsCount,
                SkippedCount = first == null ? 0 : first.Batch.SkippedCount,
                HasFailureReport = first != null && !string.IsNullOrEmpty(first.Batch.FailureReportJson),
                TotalAmount = d.SettlementItems.Where(i => !i.IsDeleted && (!batchId.HasValue || i.BatchID == batchId))
                    .Sum(i => i.ProcessedRank.BaseSalary),
                CreatedByUserName = d.CreatedByUser != null ? d.CreatedByUser.FullName : "غير محدد",
                CreatedAt = d.CreatedAt
            });

    public Task<List<SettlementDecisionDto>> GetAllDecisionsAsync() =>
        ProjectDecisions(_context.Set<SettlementDecision>().AsNoTracking()
            .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.DecisionID)).ToListAsync();

    public async Task<SettlementDecisionDto?> GetDecisionByIdAsync(int decisionId)
    {
        var decision = await _context.Set<SettlementDecision>()
            .AsNoTracking()
            .Where(d => d.DecisionID == decisionId && !d.IsDeleted)
            .Select(d => new
            {
                Decision = d,
                FirstItem = d.SettlementItems.Where(i => !i.IsDeleted).OrderBy(i => i.SettlementItemID).FirstOrDefault(),
                ItemCount = d.SettlementItems.Count(i => !i.IsDeleted),
                TotalAmount = d.SettlementItems.Where(i => !i.IsDeleted).Sum(i => i.ProcessedRank.BaseSalary),
                CreatedByUserName = d.CreatedByUser != null ? d.CreatedByUser.FullName : "غير محدد"
            })
            .FirstOrDefaultAsync();

        if (decision == null) return null;

        int batchId = decision.FirstItem?.BatchID ?? 0;
        SettlementsBatch? batch = null;

        if (batchId > 0)
        {
            batch = await _context.Set<SettlementsBatch>().AsNoTracking().FirstOrDefaultAsync(b => b.BatchID == batchId && !b.IsDeleted);
        }
        else
        {
            var prefix = decision.Decision.DecisionNumber + "-";
            batch = await _context.Set<SettlementsBatch>().AsNoTracking().FirstOrDefaultAsync(b => !b.IsDeleted && (b.BatchNumber == decision.Decision.DecisionNumber || b.BatchNumber.StartsWith(prefix)));
        }

        return new SettlementDecisionDto
        {
            DecisionID = decision.Decision.DecisionID,
            BatchID = batch?.BatchID ?? 0,
            DecisionNumber = decision.Decision.DecisionNumber,
            DecisionDate = decision.Decision.DecisionDate,
            DecisionType = decision.Decision.DecisionType,
            Notes = decision.Decision.Notes,
            ItemCount = decision.ItemCount,
            SourceRowsCount = batch?.SourceRowsCount ?? 0,
            SkippedCount = batch?.SkippedCount ?? 0,
            FailureReportJson = batch?.FailureReportJson,
            HasFailureReport = !string.IsNullOrEmpty(batch?.FailureReportJson),
            TotalAmount = decision.TotalAmount,
            CreatedByUserName = decision.CreatedByUserName,
            CreatedAt = decision.Decision.CreatedAt
        };
    }

    public Task<List<SettlementDecisionDto>> GetBatchDecisionsAsync(int batchId) =>
        ProjectDecisions(_context.Set<SettlementDecision>().AsNoTracking()
            .Where(d => d.SettlementItems.Any(i => i.BatchID == batchId && !i.IsDeleted))
            .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.DecisionID), batchId).ToListAsync();
=======
                ItemCount = d.SettlementItems.Count(i => !i.IsDeleted),
                SourceRowsCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SourceRowsCount : 0,
                SkippedCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SkippedCount : 0,
                HasFailureReport = d.SettlementItems.Any() && !string.IsNullOrEmpty(d.SettlementItems.First().Batch.FailureReportJson),
                TotalAmount = d.SettlementItems.Where(i => !i.IsDeleted).Sum(i => i.ProcessedRank.BaseSalary),
                CreatedByUserName = d.CreatedByUser != null ? d.CreatedByUser.FullName : "غير محدد",
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SettlementDecisionDto?> GetDecisionByIdAsync(int decisionId)
    {
        return await _context.Set<SettlementDecision>()
            .AsNoTracking()
            .Where(d => d.DecisionID == decisionId && !d.IsDeleted)
            .Select(d => new SettlementDecisionDto
            {
                DecisionID = d.DecisionID,
                BatchID = d.SettlementItems.Any() ? d.SettlementItems.First().BatchID : 0,
                DecisionNumber = d.DecisionNumber,
                DecisionDate = d.DecisionDate,
                DecisionType = d.DecisionType,
                Notes = d.Notes,
                ItemCount = d.SettlementItems.Count(i => !i.IsDeleted),
                SourceRowsCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SourceRowsCount : 0,
                SkippedCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SkippedCount : 0,
                FailureReportJson = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.FailureReportJson : null,
                HasFailureReport = d.SettlementItems.Any() && !string.IsNullOrEmpty(d.SettlementItems.First().Batch.FailureReportJson),
                TotalAmount = d.SettlementItems.Where(i => !i.IsDeleted).Sum(i => i.ProcessedRank.BaseSalary),
                CreatedByUserName = d.CreatedByUser != null ? d.CreatedByUser.FullName : "غير محدد",
                CreatedAt = d.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<SettlementDecisionDto>> GetBatchDecisionsAsync(int batchId)
    {
        return await _context.Set<SettlementDecision>()
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.SettlementItems.Any(i => i.BatchID == batchId && !i.IsDeleted))
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new SettlementDecisionDto
            {
                DecisionID = d.DecisionID,
                BatchID = d.SettlementItems.Any() ? d.SettlementItems.First().BatchID : 0,
                DecisionNumber = d.DecisionNumber,
                DecisionDate = d.DecisionDate,
                DecisionType = d.DecisionType,
                Notes = d.Notes,
                ItemCount = d.SettlementItems.Count(i => i.BatchID == batchId && !i.IsDeleted),
                SourceRowsCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SourceRowsCount : 0,
                SkippedCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SkippedCount : 0,
                HasFailureReport = d.SettlementItems.Any() && !string.IsNullOrEmpty(d.SettlementItems.First().Batch.FailureReportJson),
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    public async Task<List<SettlementItemDto>> GetBatchItemsAsync(int batchId)
    {
        return await _context.Set<SettlementItem>()
            .AsNoTracking()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
<<<<<<< HEAD
            .OrderBy(i => i.SerialNumber).ThenBy(i => i.SettlementItemID)
=======
            .OrderBy(i => i.SerialNumber)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Select(i => new SettlementItemDto
            {
                SettlementItemID = i.SettlementItemID,
                SerialNumber = i.SerialNumber,
                MilitaryNumber = i.ApprovalItem != null ? i.ApprovalItem.MilitaryNumber : i.MilitaryNumber,
                RetireeFullName = i.ApprovalItem != null ? i.ApprovalItem.FullName : i.FullName,
                RetireeID = i.RetireeID,
                CurrentRankName = i.CurrentRank != null ? i.CurrentRank.RankName : "غير محدد",
                ProcessedRankName = i.ProcessedRank != null ? i.ProcessedRank.RankName : "غير محدد",
                ProcessedRankSalary = i.ProcessedRank != null ? i.ProcessedRank.BaseSalary : 0,
                DecisionNumber = i.Decision != null ? i.Decision.DecisionNumber : null,
                DecisionID = i.DecisionID,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<SettlementItemDto>> GetDecisionItemsAsync(int decisionId)
    {
        return await _context.Set<SettlementItem>()
            .AsNoTracking()
            .Where(i => i.DecisionID == decisionId && !i.IsDeleted)
<<<<<<< HEAD
            .OrderBy(i => i.SerialNumber).ThenBy(i => i.SettlementItemID)
=======
            .OrderBy(i => i.SerialNumber)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Select(i => new SettlementItemDto
            {
                SettlementItemID = i.SettlementItemID,
                SerialNumber = i.SerialNumber,
                MilitaryNumber = i.ApprovalItem != null ? i.ApprovalItem.MilitaryNumber : i.MilitaryNumber,
                RetireeFullName = i.ApprovalItem != null ? i.ApprovalItem.FullName : i.FullName,
                RetireeID = i.RetireeID,
                CurrentRankName = i.CurrentRank != null ? i.CurrentRank.RankName : "غير محدد",
                ProcessedRankName = i.ProcessedRank != null ? i.ProcessedRank.RankName : "غير محدد",
                ProcessedRankSalary = i.ProcessedRank != null ? i.ProcessedRank.BaseSalary : 0,
                DecisionNumber = i.Decision != null ? i.Decision.DecisionNumber : null,
                DecisionID = i.DecisionID,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PagedResult<SettlementDecisionDto>> GetDecisionsPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
<<<<<<< HEAD
        if (page < 1 || pageSize < 1 || (long)(page - 1) * pageSize > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(page));
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        var baseQuery = _context.Set<SettlementDecision>()
            .AsNoTracking()
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            baseQuery = baseQuery.Where(d =>
                d.DecisionNumber.Contains(searchTerm) ||
                (d.DecisionType != null && d.DecisionType.Contains(searchTerm)) ||
                (d.Notes != null && d.Notes.Contains(searchTerm)));
        }

<<<<<<< HEAD
        var totalCount = await baseQuery.CountAsync();
        var items = await ProjectDecisions(baseQuery
            .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.DecisionID)
            .Skip((page - 1) * pageSize).Take(pageSize)).ToListAsync();
=======
        var projectedQuery = baseQuery
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new SettlementDecisionDto
            {
                DecisionID = d.DecisionID,
                BatchID = d.SettlementItems.Any() ? d.SettlementItems.First().BatchID : 0,
                DecisionNumber = d.DecisionNumber,
                DecisionDate = d.DecisionDate,
                DecisionType = d.DecisionType,
                Notes = d.Notes,
                ItemCount = d.SettlementItems.Count(i => !i.IsDeleted),
                SourceRowsCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SourceRowsCount : 0,
                SkippedCount = d.SettlementItems.Any() ? d.SettlementItems.First().Batch.SkippedCount : 0,
                HasFailureReport = d.SettlementItems.Any() && !string.IsNullOrEmpty(d.SettlementItems.First().Batch.FailureReportJson),
                TotalAmount = d.SettlementItems.Where(i => !i.IsDeleted).Sum(i => i.ProcessedRank.BaseSalary),
                CreatedByUserName = d.CreatedByUser != null ? d.CreatedByUser.FullName : "غير محدد",
                CreatedAt = d.CreatedAt
            });

        var totalCount = await projectedQuery.CountAsync();
        var items = await projectedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        return new PagedResult<SettlementDecisionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<SettlementItemDto>> GetDecisionItemsPagedAsync(int decisionId, int page, int pageSize, string? searchTerm = null)
    {
<<<<<<< HEAD
        if (page < 1 || pageSize < 1 || (long)(page - 1) * pageSize > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(page));
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        var baseQuery = _context.Set<SettlementItem>()
            .AsNoTracking()
            .Where(i => i.DecisionID == decisionId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            baseQuery = baseQuery.Where(i =>
                (i.ApprovalItem != null && i.ApprovalItem.FullName.Contains(searchTerm)) ||
                (i.ApprovalItem == null && i.FullName != null && i.FullName.Contains(searchTerm)) ||
                (i.ApprovalItem != null && i.ApprovalItem.MilitaryNumber.Contains(searchTerm)) ||
                (i.ApprovalItem == null && i.MilitaryNumber != null && i.MilitaryNumber.Contains(searchTerm)));
        }

        var projectedQuery = baseQuery
<<<<<<< HEAD
            .OrderBy(i => i.SerialNumber).ThenBy(i => i.SettlementItemID)
=======
            .OrderBy(i => i.SerialNumber)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Select(i => new SettlementItemDto
            {
                SettlementItemID = i.SettlementItemID,
                SerialNumber = i.SerialNumber,
                MilitaryNumber = i.ApprovalItem != null ? i.ApprovalItem.MilitaryNumber : i.MilitaryNumber,
                RetireeFullName = i.ApprovalItem != null ? i.ApprovalItem.FullName : i.FullName,
                RetireeID = i.RetireeID,
                CurrentRankName = i.CurrentRank != null ? i.CurrentRank.RankName : "غير محدد",
                ProcessedRankName = i.ProcessedRank != null ? i.ProcessedRank.RankName : "غير محدد",
                ProcessedRankSalary = i.ProcessedRank != null ? i.ProcessedRank.BaseSalary : 0,
                DecisionNumber = i.Decision != null ? i.Decision.DecisionNumber : null,
                DecisionID = i.DecisionID,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            });

        var totalCount = await projectedQuery.CountAsync();
        var items = await projectedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<SettlementItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<SettlementItemDto>> GetByRetireeIdAsync(int retireeId)
    {
        var retiree = await _context.Set<Retiree>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RetireeID == retireeId && !r.IsDeleted);

        if (retiree == null)
            return new List<SettlementItemDto>();

<<<<<<< HEAD
        var number = SettlementMilitaryNumber.Normalize(retiree.MilitaryNumber);
        return await _context.Set<SettlementItem>().AsNoTracking()
            .Where(SettlementMilitaryNumber.Matches(retireeId, retiree.ApprovalItemID, number))
            .OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.SettlementItemID)
=======
        var milNum = StringHelpers.NormalizeDigits(retiree.MilitaryNumber);
        var approvalIds = retiree.ApprovalItemID > 0
            ? new List<int> { retiree.ApprovalItemID }
            : new List<int>();

        return await _context.Set<SettlementItem>()
            .AsNoTracking()
            .Where(i => !i.IsDeleted && (
                i.RetireeID == retireeId ||
                (!string.IsNullOrEmpty(milNum) && i.MilitaryNumber != null && StringHelpers.NormalizeDigits(i.MilitaryNumber) == milNum) ||
                (i.ApprovalItemID != null && approvalIds.Contains(i.ApprovalItemID.Value))
            ))
            .OrderByDescending(i => i.CreatedAt)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Select(i => new SettlementItemDto
            {
                SettlementItemID = i.SettlementItemID,
                SerialNumber = i.SerialNumber,
                MilitaryNumber = i.ApprovalItem != null ? i.ApprovalItem.MilitaryNumber : i.MilitaryNumber,
                RetireeFullName = i.ApprovalItem != null ? i.ApprovalItem.FullName : i.FullName,
                RetireeID = i.RetireeID,
                CurrentRankName = i.CurrentRank != null ? i.CurrentRank.RankName : "غير محدد",
                ProcessedRankName = i.ProcessedRank != null ? i.ProcessedRank.RankName : "غير محدد",
                ProcessedRankSalary = i.ProcessedRank != null ? i.ProcessedRank.BaseSalary : 0,
                DecisionNumber = i.Decision != null ? i.Decision.DecisionNumber : null,
                DecisionID = i.DecisionID,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<SettlementItem>> GetItemsByDecisionIdAsync(int decisionId)
    {
        return await _context.Set<SettlementItem>()
            .AsNoTracking()
            .Where(i => i.DecisionID == decisionId && !i.IsDeleted)
<<<<<<< HEAD
            .Include(i => i.CurrentRank)
            .Include(i => i.ProcessedRank)
            .Include(i => i.ApprovalItem)
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .OrderBy(i => i.SettlementItemID)
            .ToListAsync();
    }

    public async Task ApplyRetireeRankUpdatesAsync(List<RetireeRankUpdate> updates)
    {
        if (updates.Count == 0) return;

<<<<<<< HEAD
        var updatesByRetiree = updates.GroupBy(u => u.RetireeID).ToDictionary(g => g.Key, g => g.First());
        var retireeIds = updatesByRetiree.Keys.ToList();
=======
        var retireeIds = updates.Select(u => u.RetireeID).Distinct().ToList();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        var files = await _context.Set<RetirementFile>()
            .Where(f => retireeIds.Contains(f.RetireeID) && !f.IsDeleted)
            .ToListAsync();

        foreach (var file in files)
        {
<<<<<<< HEAD
            var update = updatesByRetiree.GetValueOrDefault(file.RetireeID);
            if (update == null) continue;
            file.RankID = update.ProcessedRankID;
            file.UpdatedAt = DateTime.UtcNow;
=======
            var update = updates.FirstOrDefault(u => u.RetireeID == file.RetireeID);
            if (update == null) continue;
            file.RankID = update.ProcessedRankID;
            file.UpdatedAt = DateTime.UtcNow;
            _context.Set<RetirementFile>().Update(file);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        }

        var retirees = await _context.Set<Retiree>()
            .Include(r => r.ApprovalItem)
            .Where(r => retireeIds.Contains(r.RetireeID) && !r.IsDeleted)
            .ToListAsync();

        foreach (var retiree in retirees)
        {
<<<<<<< HEAD
            var update = updatesByRetiree.GetValueOrDefault(retiree.RetireeID);
=======
            var update = updates.FirstOrDefault(u => u.RetireeID == retiree.RetireeID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (update == null || retiree.ApprovalItem == null) continue;

            retiree.ApprovalItem.RankID = update.ProcessedRankID;
            retiree.ApprovalItem.Salary = update.NewSalary;
        }

        await _context.SaveChangesAsync();
    }
}
