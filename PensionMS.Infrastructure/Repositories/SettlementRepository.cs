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

    public async Task<List<SettlementItemDto>> GetBatchItemsAsync(int batchId)
    {
        return await _context.Set<SettlementItem>()
            .AsNoTracking()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .OrderBy(i => i.SerialNumber)
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
            .OrderBy(i => i.SerialNumber)
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
            .OrderBy(i => i.SerialNumber)
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

        var milNum = StringHelpers.NormalizeDigits(retiree.MilitaryNumber);
        var approvalIds = retiree.ApprovalItemID > 0
            ? new List<int> { retiree.ApprovalItemID }
            : new List<int>();

        var linkedItems = await _context.Set<SettlementItem>()
            .AsNoTracking()
            .Include(i => i.CurrentRank)
            .Include(i => i.ProcessedRank)
            .Include(i => i.ApprovalItem)
            .Include(i => i.Decision)
            .Where(i => !i.IsDeleted && (
                i.RetireeID == retireeId ||
                (i.ApprovalItemID != null && approvalIds.Contains(i.ApprovalItemID.Value))
            ))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        // مطابقة الرقم العسكري بعد التطبيع (لا يمكن ترجمتها إلى SQL)
        if (!string.IsNullOrEmpty(milNum))
        {
            var milCandidates = await _context.Set<SettlementItem>()
                .AsNoTracking()
                .Include(i => i.CurrentRank)
                .Include(i => i.ProcessedRank)
                .Include(i => i.ApprovalItem)
                .Where(i => !i.IsDeleted && i.MilitaryNumber != null)
                .ToListAsync();
            linkedItems.AddRange(milCandidates.Where(i =>
                StringHelpers.NormalizeDigits(i.MilitaryNumber) == milNum));
        }

        return linkedItems
            .DistinctBy(i => i.SettlementItemID)
            .OrderByDescending(i => i.CreatedAt)
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
            .ToList();
    }

    public async Task<List<SettlementItem>> GetItemsByDecisionIdAsync(int decisionId)
    {
        return await _context.Set<SettlementItem>()
            .AsNoTracking()
            .Where(i => i.DecisionID == decisionId && !i.IsDeleted)
            .OrderBy(i => i.SettlementItemID)
            .ToListAsync();
    }

    public async Task ApplyRetireeRankUpdatesAsync(List<RetireeRankUpdate> updates)
    {
        if (updates.Count == 0) return;

        var retireeIds = updates.Select(u => u.RetireeID).Distinct().ToList();

        var files = await _context.Set<RetirementFile>()
            .Where(f => retireeIds.Contains(f.RetireeID) && !f.IsDeleted)
            .ToListAsync();

        foreach (var file in files)
        {
            var update = updates.FirstOrDefault(u => u.RetireeID == file.RetireeID);
            if (update == null) continue;
            file.RankID = update.ProcessedRankID;
            file.UpdatedAt = DateTime.UtcNow;
            _context.Set<RetirementFile>().Update(file);
        }

        var retirees = await _context.Set<Retiree>()
            .Include(r => r.ApprovalItem)
            .Where(r => retireeIds.Contains(r.RetireeID) && !r.IsDeleted)
            .ToListAsync();

        foreach (var retiree in retirees)
        {
            var update = updates.FirstOrDefault(u => u.RetireeID == retiree.RetireeID);
            if (update == null || retiree.ApprovalItem == null) continue;

            retiree.ApprovalItem.RankID = update.ProcessedRankID;
            retiree.ApprovalItem.Salary = update.NewSalary;
        }

        await _context.SaveChangesAsync();
    }
}
