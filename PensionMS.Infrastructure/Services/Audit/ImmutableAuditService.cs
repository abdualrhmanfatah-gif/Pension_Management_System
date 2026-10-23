using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Audit;

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
        await using var transaction = _db.Database.CurrentTransaction == null
            ? await _db.Database.BeginTransactionAsync() : null;
        try
        {
            // Keep the shared lock until the business transaction commits.
            await _db.Database.ExecuteSqlRawAsync("""
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock
                    @Resource = N'PensionMS.ImmutableAudit.Append',
                    @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
                IF @result < 0 THROW 51001, N'تعذر الحصول على قفل سجل التدقيق', 1;
                """);
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

            if (transaction != null) await transaction.CommitAsync();
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> VerifyChainIntegrityAsync()
    {
        const int batchSize = 1000;
        int failedCount = 0;
        string expectedPreviousHash = "GENESIS";
        long lastId = 0;

        var upperId = await _db.ImmutableAuditLogs.MaxAsync(l => (long?)l.Id) ?? 0;
        if (upperId == 0) return 0;

        while (lastId < upperId)
        {
            var batch = await _db.ImmutableAuditLogs
                .AsNoTracking()
                .Where(l => l.Id > lastId && l.Id <= upperId)
                .OrderBy(l => l.Id)
                .Take(batchSize)
                .ToListAsync();

            if (batch.Count == 0) break;
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

            lastId = batch[^1].Id;
        }

        return failedCount;
    }

    private static string ComputeHash(ImmutableAuditLog entry)
    {
        var payload = $"{entry.PreviousHash}|{entry.ActorId}|{entry.Action}|{entry.EntityType}|{entry.EntityId}|{DateTime.SpecifyKind(entry.CreatedAt, DateTimeKind.Utc):O}|{entry.OldValues}|{entry.NewValues}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
