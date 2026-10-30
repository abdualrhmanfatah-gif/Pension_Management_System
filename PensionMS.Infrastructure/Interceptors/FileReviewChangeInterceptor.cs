using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Payroll;

namespace PensionMS.Infrastructure.Interceptors;

/// <summary>Per-context persistence adapter. Review effects belong to FileReviewService;
/// payment protection belongs to SalaryPaymentBatchService. Neither owns saving here.</summary>
internal sealed class FileReviewChangeInterceptor(ICurrentUserContext? currentUser) : SaveChangesInterceptor
{
    private static readonly HashSet<string> TechnicalFields = new(StringComparer.Ordinal)
    {
        "RowVersion", "CreatedAt", "UpdatedAt", "CreatedByUserID", "UpdatedByUserID",
        "DeletedAt", "DeletedByUserID", "PendingReviewStatus", "MissingDocumentsCount",
        "StatusID", "PreviousStatusID", "StatusReason", "ClosedDate", "ReviewStatus",
        "ReviewRequestedAt", "ReviewCompletedAt", "ReviewNotes", "LastReviewedFileState", "AssignedEmployeeID"
    };

    private readonly List<(EntityEntry Entry, string? OldJson)> _changes = [];
    private readonly Dictionary<int, List<(EntityEntry Entry, string? OldJson)>> _affected = [];
    private readonly List<RetirementFile> _files = [];
    private List<EntityEntry<SalaryPaymentItem>> _payments = [];
    private bool _prepared;
    private bool _recorded;

    internal bool RequiresFollowupSave => _files.Count > 0;

    internal bool Capture(PensionDbContext db)
    {
        Reset();
        db.ChangeTracker.DetectChanges();
        foreach (var entry in db.ChangeTracker.Entries().Where(e => IsReviewEntity(e.Entity) &&
            (e.State is EntityState.Added or EntityState.Deleted || e.State == EntityState.Modified &&
                e.Properties.Any(p => p.IsModified && !TechnicalFields.Contains(p.Metadata.Name) &&
                    !Equals(p.OriginalValue, p.CurrentValue)))))
            _changes.Add((entry, entry.State == EntityState.Added ? null : Serialize(entry, original: true)));
        _payments = SalaryPaymentBatchService.CapturePaymentChanges(db);
        return _changes.Count > 0 || _payments.Count > 0;
    }

    internal void Prepare(PensionDbContext db)
    {
        foreach (var change in _changes)
            AddAffected(change, FileQueries(db, change.Entry).SelectMany(q => q.ToList()));
        foreach (var fileId in FileIds())
            if (db.Database.IsSqlServer()) UnitOfWork.LockFileReview(db, fileId);
        SalaryPaymentBatchService.ValidatePaymentChanges(db, _payments, _affected.Keys);
        foreach (var fileId in _affected.Keys.Order())
        {
            var persisted = db.RetirementFiles.AsNoTracking().FirstOrDefault(f => f.FileID == fileId);
            if (persisted == null) continue;
            var file = db.RetirementFiles.Find(fileId);
            if (file == null || file.IsDeleted) continue;
            if (FileReviewService.RequiresPreviousSnapshot(persisted))
                file.LastReviewedFileState = ApprovedPensionSnapshot.Capture(db, fileId, persisted.RetireeID).Serialize();
            if (FileReviewService.BeginReview(file, persisted, DateTime.UtcNow, automatic: true)) _files.Add(file);
        }
        PrepareSoftDeletes();
        _prepared = true;
    }

    internal async Task PrepareAsync(PensionDbContext db, CancellationToken cancellationToken)
    {
        foreach (var change in _changes)
        {
            var fileIds = new List<int>();
            foreach (var query in FileQueries(db, change.Entry))
                fileIds.AddRange(await query.ToListAsync(cancellationToken));
            AddAffected(change, fileIds);
        }
        foreach (var fileId in FileIds())
            if (db.Database.IsSqlServer()) await UnitOfWork.LockFileReviewAsync(db, fileId, cancellationToken);
        await SalaryPaymentBatchService.ValidatePaymentChangesAsync(db, _payments, _affected.Keys, cancellationToken);
        foreach (var fileId in _affected.Keys.Order())
        {
            var persisted = await db.RetirementFiles.AsNoTracking().FirstOrDefaultAsync(f => f.FileID == fileId, cancellationToken);
            if (persisted == null) continue;
            var file = await db.RetirementFiles.FindAsync([fileId], cancellationToken);
            if (file == null || file.IsDeleted) continue;
            if (FileReviewService.RequiresPreviousSnapshot(persisted))
                file.LastReviewedFileState = (await ApprovedPensionSnapshot.CaptureAsync(db, fileId, persisted.RetireeID, cancellationToken)).Serialize();
            if (FileReviewService.BeginReview(file, persisted, DateTime.UtcNow, automatic: true)) _files.Add(file);
        }
        PrepareSoftDeletes();
        _prepared = true;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (!_prepared || _recorded || eventData.Context is not PensionDbContext db) return result;
        _recorded = true;
        foreach (var file in _files)
        {
            var now = DateTime.UtcNow;
            foreach (var change in _affected[file.FileID])
                db.FileReviewItems.Add(FileReviewService.RecordDataChange(db.FileReviewItems, file, Describe(change), now));
            FileReviewService.EnsureReviewHold(db.PensionSuspensions, hold => db.PensionSuspensions.Add(hold),
                file.FileID, Actor(_affected[file.FileID][0].Entry), now);
        }
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (!_prepared || _recorded || eventData.Context is not PensionDbContext db) return result;
        _recorded = true;
        foreach (var file in _files)
        {
            var now = DateTime.UtcNow;
            foreach (var change in _affected[file.FileID])
                db.FileReviewItems.Add(await FileReviewService.RecordDataChangeAsync(db.FileReviewItems,
                    file, Describe(change), now, cancellationToken));
            await FileReviewService.EnsureReviewHoldAsync(db.PensionSuspensions,
                hold => { db.PensionSuspensions.Add(hold); return Task.CompletedTask; },
                file.FileID, Actor(_affected[file.FileID][0].Entry), now, cancellationToken);
        }
        return result;
    }

    internal void Reset()
    {
        _changes.Clear();
        _affected.Clear();
        _files.Clear();
        _payments.Clear();
        _prepared = false;
        _recorded = false;
    }

    private IEnumerable<int> FileIds() => _affected.Keys.Concat(_payments.Select(e => e.Entity.FileID)).Distinct().Order();

    private void AddAffected((EntityEntry Entry, string? OldJson) change, IEnumerable<int> relatedIds)
    {
        foreach (var fileId in relatedIds.Concat(new[] { Value(change.Entry, "FileID"), Value(change.Entry, "FileID", true) })
            .Where(id => id > 0).Distinct())
        {
            if (!_affected.TryGetValue(fileId, out var entries)) _affected[fileId] = entries = [];
            entries.Add(change);
        }
    }

    private void PrepareSoftDeletes()
    {
        foreach (var change in _changes.Where(c => c.Entry.State == EntityState.Deleted && c.Entry.Entity is ISoftDeletable))
        {
            change.Entry.State = EntityState.Modified;
            change.Entry.Property("IsDeleted").CurrentValue = true;
            if (change.Entry.Metadata.FindProperty("DeletedAt") != null)
                change.Entry.Property("DeletedAt").CurrentValue = DateTime.UtcNow;
        }
    }

    private FileReviewService.ReviewDataChange Describe((EntityEntry Entry, string? OldJson) change)
    {
        var keys = change.Entry.Metadata.FindPrimaryKey()!.Properties;
        return new(change.Entry.Entity, Convert.ToInt32(change.Entry.Property(keys[0].Name).CurrentValue),
            keys.Count > 1 ? Convert.ToInt32(change.Entry.Property(keys[1].Name).CurrentValue) : null,
            change.Entry.Metadata.ClrType.Name, change.OldJson, Serialize(change.Entry, original: false),
            change.Entry.Metadata.FindProperty("IsDeleted") != null && change.Entry.Property("IsDeleted").CurrentValue is true,
            Actor(change.Entry));
    }

    private int Actor(EntityEntry entry)
    {
        var userId = currentUser?.GetCurrentUserId() ?? 0;
        if (userId > 0) return userId;
        foreach (var name in new[] { "UpdatedByUserID", "DeletedByUserID", "CreatedByUserID" })
            if (entry.Metadata.FindProperty(name) != null && entry.Property(name).CurrentValue is int actor && actor > 0)
                return actor;
        return LookupConstants.SystemUserId;
    }

    private static string Serialize(EntityEntry entry, bool original) => JsonSerializer.Serialize(
        entry.Properties.Where(p => !TechnicalFields.Contains(p.Metadata.Name))
            .ToDictionary(p => p.Metadata.Name, p => original ? p.OriginalValue : p.CurrentValue));

    private static bool IsReviewEntity(object entity) => entity is RetirementFile or Retiree or EntitledPerson
        or Agent or Guardian or GuardianEntitledPerson or SalaryRecipient or PaymentBook or FinancialEntitlement
        or Document or DocumentPledge or DocumentExemption or MissingDocument or FileNote;

    private static int Value(EntityEntry entry, string name, bool original = false) =>
        entry.Metadata.FindProperty(name) == null ? 0 : Convert.ToInt32(original
            ? entry.Property(name).OriginalValue : entry.Property(name).CurrentValue);

    // Scope is inherited from the authorized mutation. Querying old owner links also needs
    // soft-deleted rows; these lookups establish affected aggregates, not operation permission.
    private static IEnumerable<IQueryable<int>> FileQueries(PensionDbContext db, EntityEntry entry)
    {
        foreach (var original in new[] { false, true })
        {
            var fileId = Value(entry, "FileID", original);
            if (entry.Entity is Retiree or FinancialEntitlement || entry.Entity is Document document &&
                Value(entry, "RetireeID", original) > 0 && (fileId == 0 || document.OwnerType == DocumentOwnerType.Retiree))
            {
                var retireeId = Value(entry, "RetireeID", original);
                yield return db.RetirementFiles.AsNoTracking().Where(f => f.RetireeID == retireeId).Select(f => f.FileID);
            }
            if (entry.Entity is GuardianEntitledPerson || entry.Entity is Document && fileId == 0)
            {
                var entitledId = Value(entry, "EntitledID", original);
                var agentId = Value(entry, "AgentID", original);
                var guardianId = Value(entry, "GuardianID", original);
                var pledgeId = Value(entry, "PledgeID", original);
                var exemptionId = Value(entry, "ExemptionID", original);
                var noteId = Value(entry, "NoteID", original);
                var suspensionId = Value(entry, "SuspensionID", original);
                yield return db.EntitledPersons.IgnoreQueryFilters().Where(e => e.EntitledID == entitledId).Select(e => e.FileID);
                yield return db.Agents.IgnoreQueryFilters().Where(e => e.AgentID == agentId).Select(e => e.FileID);
                yield return db.Guardians.IgnoreQueryFilters().Where(e => e.GuardianID == guardianId).Select(e => e.FileID);
                yield return db.DocumentPledges.IgnoreQueryFilters().Where(e => e.PledgeID == pledgeId).Select(e => e.FileID);
                yield return db.DocumentExemptions.IgnoreQueryFilters().Where(e => e.ExemptionID == exemptionId).Select(e => e.FileID);
                yield return db.FileNotes.IgnoreQueryFilters().Where(e => e.NoteID == noteId).Select(e => e.FileID);
                yield return db.PensionSuspensions.IgnoreQueryFilters().Where(e => e.SuspensionID == suspensionId).Select(e => e.FileID);
            }
        }
    }
}
