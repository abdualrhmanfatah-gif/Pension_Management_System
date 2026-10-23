using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly PensionDbContext _context;

    public AuditLogService(PensionDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> GetAllAsync(int page, int pageSize, string? action, int? userId, int? tableId, DateTime? from, DateTime? to, string? search)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.SystemTable)
            .AsQueryable();

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);
        if (userId.HasValue)
            query = query.Where(a => a.UserID == userId.Value);
        if (tableId.HasValue)
            query = query.Where(a => a.TableID == tableId.Value);
        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(a =>
                (a.EntityDisplayName != null && a.EntityDisplayName.Contains(search)) ||
                (a.Action != null && a.Action.Contains(search)));

        var totalCount = await query.CountAsync();

        var rawItems = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.LogID,
                a.UserID,
                userName = a.User != null ? a.User.FullName : null,
                a.TableID,
                tableName = a.SystemTable != null ? a.SystemTable.TableNameAr : null,
                a.Action,
                a.RecordID,
                a.EntityKey,
                a.EntityDisplayName,
                a.OldValues,
                a.NewValues,
                a.IPAddress,
                a.UserAgent,
                a.CorrelationId,
                a.CreatedAt
            })
            .ToListAsync();

        var keys = rawItems
            .Where(i => i.TableID != null && i.RecordID != null)
            .Select(i => (i.TableID!.Value, i.RecordID!.Value))
            .Distinct()
            .ToList();
        var fileMap = await ResolveAffectedFilesAsync(_context, keys);

        var items = rawItems.Select(i =>
        {
            var key = i.TableID.HasValue && i.RecordID.HasValue ? (i.TableID.Value, i.RecordID.Value) : ((int, int)?)null;
            var fileInfo = key.HasValue && fileMap.TryGetValue(key.Value, out var f) ? f : ((int FileID, string? FileNumber)?)null;
            return new
            {
                i.LogID,
                i.UserID,
                i.userName,
                i.TableID,
                i.tableName,
                i.Action,
                i.RecordID,
                i.EntityKey,
                i.EntityDisplayName,
                i.OldValues,
                i.NewValues,
                i.IPAddress,
                i.UserAgent,
                i.CorrelationId,
                i.CreatedAt,
                affectedFileId = fileInfo?.FileID,
                affectedFileNumber = fileInfo?.FileNumber
            };
        }).ToList();

        return ApiResponse<object>.Success(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    public async Task<ApiResponse<object>> GetImmutableAsync(int page, int pageSize, string? action, string? entityType, int? actorId, DateTime? from, DateTime? to)
    {
        var query = _context.ImmutableAuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (actorId.HasValue)
            query = query.Where(a => a.ActorId == actorId.Value);
        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<object>.Success(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    public async Task<ApiResponse<List<string>>> GetDistinctActionsAsync()
    {
        var actions = await _context.AuditLogs
            .AsNoTracking()
            .Select(a => a.Action)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();

        return ApiResponse<List<string>>.Success(actions);
    }

    public async Task<ApiResponse<object>> GetTablesAsync()
    {
        var tables = await _context.SystemTables
            .AsNoTracking()
            .Select(t => new { t.TableID, t.TableNameAr })
            .OrderBy(t => t.TableNameAr)
            .ToListAsync();

        return ApiResponse<object>.Success(tables);
    }

    public async Task<ApiResponse<object>> GetByRecordAsync(int tableId, int recordId)
    {
        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.TableID == tableId && a.RecordID == recordId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.LogID,
                userName = a.User != null ? a.User.FullName : "نظام",
                a.Action,
                a.OldValues,
                a.NewValues,
                a.IPAddress,
                a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<object>.Success(logs);
    }

    private static async Task<Dictionary<(int TableID, int RecordID), (int FileID, string? FileNumber)>> ResolveAffectedFilesAsync(
        PensionDbContext context,
        List<(int TableID, int RecordID)> keys)
    {
        if (keys.Count == 0)
            return new Dictionary<(int, int), (int, string?)>();

        var result = new Dictionary<(int, int), (int, string?)>();
        const int tableRetirementFiles = 2;

        // TableID 2: RecordID هو FileID
        var fileIdsFromTable2 = keys.Where(k => k.Item1 == tableRetirementFiles).Select(k => k.Item2).Distinct().ToList();
        if (fileIdsFromTable2.Count > 0)
        {
            var fileNumbers = await context.RetirementFiles
                .AsNoTracking()
                .Where(f => fileIdsFromTable2.Contains(f.FileID))
                .Select(f => new { f.FileID, f.FileNumber })
                .ToListAsync();
            foreach (var (_, recordId) in keys.Where(k => k.Item1 == tableRetirementFiles))
            {
                var fn = fileNumbers.FirstOrDefault(f => f.FileID == recordId);
                result[(tableRetirementFiles, recordId)] = (recordId, fn?.FileNumber);
            }
        }

        // جداول مرتبطة بملف: 3–12
        var recordIdsByTable = keys.Where(k => k.Item1 != tableRetirementFiles && k.Item1 >= 3 && k.Item1 <= 12)
            .GroupBy(k => k.Item1)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Item2).Distinct().ToList());

        foreach (var (tableId, recordIds) in recordIdsByTable)
        {
            List<(int RecordID, int FileID)> recordFilePairs = tableId switch
            {
                3 => (await context.EntitledPersons.AsNoTracking().Where(e => recordIds.Contains(e.EntitledID)).Select(e => new { e.EntitledID, e.FileID }).ToListAsync()).Select(x => (x.EntitledID, x.FileID)).ToList(),
                4 => (await context.Agents.AsNoTracking().Where(a => recordIds.Contains(a.AgentID)).Select(a => new { a.AgentID, a.FileID }).ToListAsync()).Select(x => (x.AgentID, x.FileID)).ToList(),
                5 => (await context.Guardians.AsNoTracking().Where(g => recordIds.Contains(g.GuardianID)).Select(g => new { g.GuardianID, g.FileID }).ToListAsync()).Select(x => (x.GuardianID, x.FileID)).ToList(),
                6 => (await context.Documents.AsNoTracking().Where(d => recordIds.Contains(d.DocumentID) && d.FileID != null).Select(d => new { d.DocumentID, FileID = d.FileID!.Value }).ToListAsync()).Select(x => (x.DocumentID, x.FileID)).ToList(),
                7 => (await context.DocumentPledges.AsNoTracking().Where(p => recordIds.Contains(p.PledgeID)).Select(p => new { p.PledgeID, p.FileID }).ToListAsync()).Select(x => (x.PledgeID, x.FileID)).ToList(),
                8 => (await context.MissingDocuments.AsNoTracking().Where(m => recordIds.Contains(m.MissingDocumentID)).Select(m => new { m.MissingDocumentID, m.FileID }).ToListAsync()).Select(x => (x.MissingDocumentID, x.FileID)).ToList(),
                9 => (await context.DocumentExemptions.AsNoTracking().Where(e => recordIds.Contains(e.ExemptionID)).Select(e => new { e.ExemptionID, e.FileID }).ToListAsync()).Select(x => (x.ExemptionID, x.FileID)).ToList(),
                10 => (await context.FileNotes.AsNoTracking().Where(n => recordIds.Contains(n.NoteID)).Select(n => new { n.NoteID, n.FileID }).ToListAsync()).Select(x => (x.NoteID, x.FileID)).ToList(),
                11 => (await context.PensionSuspensions.AsNoTracking().Where(s => recordIds.Contains(s.SuspensionID)).Select(s => new { s.SuspensionID, s.FileID }).ToListAsync()).Select(x => (x.SuspensionID, x.FileID)).ToList(),
                12 => (await context.SalaryRecipients.AsNoTracking().Where(r => recordIds.Contains(r.RecipientID)).Select(r => new { r.RecipientID, r.FileID }).ToListAsync()).Select(x => (x.RecipientID, x.FileID)).ToList(),
                _ => new List<(int, int)>()
            };

            var fileIds = recordFilePairs.Select(x => x.FileID).Distinct().ToList();
            if (fileIds.Count == 0)
                continue;
            var fileNumberList = await context.RetirementFiles.AsNoTracking().Where(f => fileIds.Contains(f.FileID)).Select(f => new { f.FileID, f.FileNumber }).ToListAsync();
            foreach (var (recordId, fileId) in recordFilePairs)
            {
                var fn = fileNumberList.FirstOrDefault(f => f.FileID == fileId);
                result[(tableId, recordId)] = (fileId, fn?.FileNumber);
            }
        }

        return result;
    }
}
