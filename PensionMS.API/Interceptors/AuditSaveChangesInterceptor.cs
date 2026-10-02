using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Data;
using PensionMS.Core.Constants;

namespace PensionMS.API.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CorrelationIdItemKey = "CorrelationId";

    // Track newly created entities — their PKs are 0 before save and get
    // populated by the database after save. We fix them up in SavedChanges.
    private List<(AuditLog AuditLog, object Entity)>? _pendingCreates;
    private bool _isPostSaveFixup;

    private static ConcurrentDictionary<string, int>? _tableCache;
    private static DateTime _tableCacheTime;
    private static readonly TimeSpan TableCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly object TableCacheLock = new();

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (!_isPostSaveFixup)
            _pendingCreates = BeforeSaveChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (!_isPostSaveFixup)
            _pendingCreates = BeforeSaveChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        FixupCreatedRecordIds(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await FixupCreatedRecordIdsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<(AuditLog, object)>? BeforeSaveChanges(DbContext? context)
    {
        if (context is not PensionDbContext dbContext) return null;

        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        if (!entries.Any()) return null;

        var userId = GetUserIdFromClaims();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers?.UserAgent.ToString();
        var correlationId = GetOrCreateCorrelationId();
        
        // جلب SystemTables لاستخراج TableID — مع cache ثابت 10 دقائق
        var tableDictionary = GetTableDictionary(dbContext);

        var auditLogs = new List<AuditLog>();
        var pendingCreates = new List<(AuditLog, object)>();

        foreach (var entry in entries)
        {
            // استثناء الكيانات التي لا نريد تدقيقها
            var entityType = entry.Entity.GetType();
            // Handle Proxies if any
            if (entityType.Namespace == InfrastructureConstants.CastleProxiesNamespace)
                entityType = entityType.BaseType!;

            var entityName = entityType.Name;
            if (entityName == nameof(AuditLog) || entityName == nameof(ImmutableAuditLog) || entityName == nameof(SystemTable))
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            // محاولة إيجاد TableID
            var tableName = context.Model.FindEntityType(entityType)?.GetTableName();
            int? tableId = null;
            if (!string.IsNullOrEmpty(tableName) && tableDictionary.TryGetValue(tableName, out var tid))
            {
                tableId = tid;
            }

            var auditLog = new AuditLog
            {
                UserID = userId,
                TableID = tableId,
                Action = action,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = correlationId,
                CreatedAt = DateTime.UtcNow,
                EntityDisplayName = null 
            };

            // القيم
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            try 
            {
                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) continue;

                    var propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditLog.OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
                auditLog.NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;
            }
            catch 
            {
                auditLog.OldValues = "{\"error\": \"Serialization failed\"}";
            }

            // الحصول على Primary Key
            var pk = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (pk != null && pk.CurrentValue is int recordId)
            {
                auditLog.RecordID = recordId;
                auditLog.EntityKey = recordId;
            }

            // Track Added entries — PK = 0 now, will be real after DB save
            if (entry.State == EntityState.Added)
            {
                pendingCreates.Add((auditLog, entry.Entity));
            }

            auditLogs.Add(auditLog);
        }

        if (auditLogs.Any())
        {
            dbContext.AuditLogs.AddRange(auditLogs);
        }

        return pendingCreates.Count > 0 ? pendingCreates : null;
    }

    /// <summary>
    /// بعد الحفظ، تقوم قاعدة البيانات بتوليد المفاتيح الأساسية (PKs).
    /// هنا نُحدّث RecordID في سجلات AuditLog لكيانات Create بالقيم الحقيقية.
    /// </summary>
    private void FixupCreatedRecordIds(DbContext? context)
    {
        if (_pendingCreates == null || _pendingCreates.Count == 0 || context is not PensionDbContext db) return;

        var pending = _pendingCreates;
        _pendingCreates = null;

        bool anyUpdated = false;
        foreach (var (auditLog, entity) in pending)
        {
            var entry = db.Entry(entity);
            var pk = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (pk?.CurrentValue is int realId && realId > 0 && auditLog.RecordID != realId)
            {
                auditLog.RecordID = realId;
                auditLog.EntityKey = realId;
                anyUpdated = true;
            }
        }

        if (anyUpdated)
        {
            _isPostSaveFixup = true;
            try { db.SaveChanges(); }
            finally { _isPostSaveFixup = false; }
        }
    }

    /// <summary>
    /// النسخة غير المتزامنة من FixupCreatedRecordIds
    /// </summary>
    private async Task FixupCreatedRecordIdsAsync(DbContext? context, CancellationToken ct)
    {
        if (_pendingCreates == null || _pendingCreates.Count == 0 || context is not PensionDbContext db) return;

        var pending = _pendingCreates;
        _pendingCreates = null;

        bool anyUpdated = false;
        foreach (var (auditLog, entity) in pending)
        {
            var entry = db.Entry(entity);
            var pk = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (pk?.CurrentValue is int realId && realId > 0 && auditLog.RecordID != realId)
            {
                auditLog.RecordID = realId;
                auditLog.EntityKey = realId;
                anyUpdated = true;
            }
        }

        if (anyUpdated)
        {
            _isPostSaveFixup = true;
            try { await db.SaveChangesAsync(ct); }
            finally { _isPostSaveFixup = false; }
        }
    }

    private int? GetUserIdFromClaims()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : null;
    }

    private Guid GetOrCreateCorrelationId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return Guid.NewGuid();

        if (context.Items.TryGetValue(CorrelationIdItemKey, out var existing) && existing is Guid guid)
        {
            return guid;
        }

        var newId = Guid.NewGuid();
        context.Items[CorrelationIdItemKey] = newId;
        return newId;
    }

    private static IDictionary<string, int> GetTableDictionary(PensionDbContext dbContext)
    {
        if (_tableCache != null && DateTime.UtcNow - _tableCacheTime < TableCacheDuration)
            return _tableCache;

        lock (TableCacheLock)
        {
            if (_tableCache != null && DateTime.UtcNow - _tableCacheTime < TableCacheDuration)
                return _tableCache;

            var systemTables = dbContext.SystemTables.AsNoTracking().ToList();
            _tableCache = new ConcurrentDictionary<string, int>(
                systemTables.ToDictionary(t => t.TableNameEn, t => t.TableID, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase
            );
            _tableCacheTime = DateTime.UtcNow;
        }

        return _tableCache;
    }
}
