using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// خدمة التدقيق المقاوم للتلاعب — كل سجل يحمل تجزئة SHA-256 مرتبطة بالسجل السابق
/// </summary>
public class ImmutableAuditService : IImmutableAuditService
{
    private readonly PensionDbContext _db;

    public ImmutableAuditService(PensionDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        int actorId,
        string actorName,
        string action,
        string entityType,
        int entityId,
        string? oldValues,
        string? newValues,
        int? branchId = null,
        string? ipAddress = null,
        Guid? correlationId = null)
    {
        // جلب تجزئة السجل الأخير (أو GENESIS لأول سجل)
        var previousHash = await _db.ImmutableAuditLogs
            .OrderByDescending(l => l.Id)
            .Select(l => l.Hash)
            .FirstOrDefaultAsync() ?? "GENESIS";

        var entry = new ImmutableAuditLog
        {
            ActorId = actorId,
            ActorName = actorName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            BranchId = branchId,
            IpAddress = ipAddress,
            PreviousHash = previousHash,
            CorrelationId = correlationId,
            CreatedAt = DateTime.UtcNow
        };

        // حساب التجزئة: SHA256(PreviousHash + ActorId + Action + EntityType + EntityId + Timestamp)
        entry.Hash = ComputeHash(entry);

        _db.ImmutableAuditLogs.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<int> VerifyChainIntegrityAsync()
    {
        const int batchSize = 1000;
        int failedCount = 0;
        string expectedPreviousHash = "GENESIS";
        int processedCount = 0;

        var totalLogs = await _db.ImmutableAuditLogs.CountAsync();
        if (totalLogs == 0) return 0;

        while (processedCount < totalLogs)
        {
            var batch = await _db.ImmutableAuditLogs
                .AsNoTracking()
                .OrderBy(l => l.Id)
                .Skip(processedCount)
                .Take(batchSize)
                .ToListAsync();

            foreach (var log in batch)
            {
                // 1. فحص ارتباط السلسلة
                if (log.PreviousHash != expectedPreviousHash)
                {
                    failedCount++;
                    // لا نتوقف عند أول خطأ، بل نكمل لنعرف حجم التلاعب بالسلسلة
                }

                // 2. فحص صحة التجزئة
                var computedHash = ComputeHash(log);
                if (log.Hash != computedHash)
                {
                    failedCount++;
                }

                expectedPreviousHash = log.Hash;
            }

            processedCount += batch.Count;
        }

        return failedCount;
    }

    private static string ComputeHash(ImmutableAuditLog entry)
    {
        var payload = $"{entry.PreviousHash}|{entry.ActorId}|{entry.Action}|{entry.EntityType}|{entry.EntityId}|{entry.CreatedAt:O}|{entry.OldValues}|{entry.NewValues}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
