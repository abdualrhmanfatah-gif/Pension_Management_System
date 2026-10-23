using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;

namespace PensionMS.Infrastructure.Data;

public partial class PensionDbContext
{
    private bool _savingLifecycle;
    private readonly Core.Interfaces.ICurrentUserContext? _lifecycleScope;
    public int LifecycleUserID => _lifecycleScope?.GetCurrentUserId() ?? 0;

    // Review decisions, accounting, audit, cache refreshes and suspensions are technical state,
    // not edits to the underlying approved file data.
    private static readonly HashSet<string> LifecycleTechnicalFields = new(StringComparer.Ordinal)
    {
        "RowVersion", "CreatedAt", "UpdatedAt", "CreatedByUserID", "UpdatedByUserID",
        "DeletedAt", "DeletedByUserID", "PendingReviewStatus", "MissingDocumentsCount",
        "StatusID", "PreviousStatusID", "StatusReason", "ClosedDate", "ReviewStatus",
        "ReviewRequestedAt", "ReviewCompletedAt", "ReviewNotes", "LastReviewedFileState",
        "AssignedEmployeeID"
    };

    private static bool IsLifecycleEntity(object entity) => entity is RetirementFile or Retiree
        or EntitledPerson or Agent or Guardian or GuardianEntitledPerson or SalaryRecipient
        or PaymentBook or FinancialEntitlement or Document or DocumentPledge
        or DocumentExemption or MissingDocument or FileNote;

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        if (_savingLifecycle) return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        ChangeTracker.DetectChanges();
        var changes = ChangeTracker.Entries().Where(e => IsLifecycleEntity(e.Entity) &&
            (e.State is EntityState.Added or EntityState.Deleted ||
             e.State == EntityState.Modified && e.Properties.Any(p => p.IsModified &&
                 !LifecycleTechnicalFields.Contains(p.Metadata.Name) && !Equals(p.OriginalValue, p.CurrentValue))))
            .ToList();
        var paymentChanges = ChangeTracker.Entries<SalaryPaymentItem>()
            .Where(e => e.State == EntityState.Added && (e.Entity.StatusID == TransactionStatusConstants.Sent ||
                e.Entity.StatusID == TransactionStatusConstants.Received || e.Entity.SendDate.HasValue) ||
                e.State == EntityState.Modified && e.Properties.Any(p => p.IsModified &&
                (p.Metadata.Name is nameof(SalaryPaymentItem.StatusID) or nameof(SalaryPaymentItem.Amount) or
                    nameof(SalaryPaymentItem.RecipientID) or nameof(SalaryPaymentItem.SendDate) or
                    nameof(SalaryPaymentItem.ReceiveDate) or nameof(SalaryPaymentItem.IsDeleted)) &&
                !Equals(p.OriginalValue, p.CurrentValue))).ToList();
        if (changes.Count == 0 && paymentChanges.Count == 0)
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        // All business writes and their review/hold records share the same transaction,
        // including callers which save directly through a repository/DbContext.
        var ownsTransaction = Database.CurrentTransaction == null;
        await using var transaction = ownsTransaction ? await Database.BeginTransactionAsync(cancellationToken) : null;
        _savingLifecycle = true;
        try
        {
            var affected = new Dictionary<int, List<(EntityEntry Entry, string? OldJson, string NewJson)>>();
            foreach (var entry in changes)
            {
                var oldJson = entry.State == EntityState.Added ? null : SerializeValues(entry, original: true);
                var newJson = SerializeValues(entry, original: false);
                foreach (var fileId in await ResolveLifecycleFilesAsync(entry, cancellationToken))
                {
                    if (!affected.TryGetValue(fileId, out var entries)) affected[fileId] = entries = [];
                    entries.Add((entry, oldJson, newJson));
                }
            }

            var files = new List<RetirementFile>();
            foreach (var fileId in affected.Keys.Concat(paymentChanges.Select(e => e.Entity.FileID)).Distinct().Order())
            {
                // Same lock used by approval and batch validation; deterministic order for shared retirees.
                if (Database.IsSqlServer())
                    await Database.ExecuteSqlInterpolatedAsync($"SELECT FileID FROM RetirementFiles WITH (UPDLOCK, HOLDLOCK) WHERE FileID = {fileId}", cancellationToken);
                foreach (var payment in paymentChanges.Where(e => e.Entity.FileID == fileId))
                {
                    var originalStatus = payment.Property(i => i.StatusID).OriginalValue;
                    if (payment.State == EntityState.Modified && originalStatus == TransactionStatusConstants.Received)
                        throw new InvalidOperationException("لا يمكن تغيير بند معاش سبق استلامه");
                    if (payment.State == EntityState.Modified && !PensionPaymentStatusRules.CanChange(originalStatus, payment.Entity.StatusID))
                        throw new InvalidOperationException("انتقال حالة الحوالة غير مسموح؛ لا يجوز إعادة إرسال بند سبق صرفه أو إلغاؤه");
                    if (originalStatus == TransactionStatusConstants.Sent &&
                        (payment.Property(i => i.Amount).OriginalValue != payment.Entity.Amount ||
                         payment.Property(i => i.RecipientID).OriginalValue != payment.Entity.RecipientID))
                        throw new InvalidOperationException("لا يمكن تغيير مبلغ أو مستلم بند سبق إرساله");
                    if (payment.Entity.StatusID is TransactionStatusConstants.Sent or TransactionStatusConstants.Received ||
                        payment.Property(i => i.SendDate).IsModified && payment.Entity.SendDate.HasValue)
                    {
                        var active = await RetirementFiles.AsNoTracking().AnyAsync(f => f.FileID == fileId &&
                            f.StatusID == FileStatusConstants.Active && f.ReviewStatus == FileReviewStatus.Approved, cancellationToken);
                        var held = await PensionSuspensions.AnyAsync(s => s.FileID == fileId && !s.IsDeleted &&
                            (s.Status == SuspensionStatusConstants.Active || s.Status == SuspensionStatusConstants.CodeActive), cancellationToken);
                        var pendingHold = ChangeTracker.Entries<PensionSuspension>().Any(e =>
                            (e.State == EntityState.Added || e.State == EntityState.Modified) && e.Entity.FileID == fileId && !e.Entity.IsDeleted &&
                            (e.Entity.Status == SuspensionStatusConstants.Active || e.Entity.Status == SuspensionStatusConstants.CodeActive));
                        if (!active || held || pendingHold || affected.ContainsKey(fileId))
                            throw new InvalidOperationException("صرف المعاش موقوف حتى اعتماد المراجعة وزوال جميع الإيقافات");
                    }
                }
                if (!affected.ContainsKey(fileId)) continue;
                var persisted = await RetirementFiles.AsNoTracking().FirstOrDefaultAsync(f => f.FileID == fileId, cancellationToken);
                if (persisted == null || persisted.StatusID == FileStatusConstants.New && persisted.ReviewStatus == FileReviewStatus.None)
                    continue;
                var file = await RetirementFiles.FindAsync([fileId], cancellationToken);
                if (file == null || file.IsDeleted) continue;
                // Capture before business changes reach the DB. Pending edits never replace this version.
                if (ApprovedPensionSnapshot.Read(persisted.LastReviewedFileState) == null &&
                    (persisted.StatusID == FileStatusConstants.Active || persisted.ReviewStatus == FileReviewStatus.Approved))
                    file.LastReviewedFileState = (await ApprovedPensionSnapshot.CaptureAsync(this, fileId, persisted.RetireeID, cancellationToken)).Serialize();
                if (persisted.StatusID != FileStatusConstants.Pending) file.PreviousStatusID = persisted.StatusID;
                // The existing ReviewRequestedAt is the boundary of the current review.
                // Older decisions remain immutable history, without adding entity fields.
                file.StatusID = FileStatusConstants.Pending;
                file.ReviewStatus = FileReviewStatus.PendingReview;
                file.ReviewRequestedAt = persisted.StatusID == FileStatusConstants.Pending &&
                    persisted.ReviewStatus == FileReviewStatus.PendingReview && persisted.ReviewRequestedAt.HasValue
                        ? persisted.ReviewRequestedAt : DateTime.UtcNow;
                file.ReviewNotes = null;
                files.Add(file);
            }

            // Soft deletion preserves the data needed to review and audit removals.
            foreach (var entry in changes.Where(e => e.State == EntityState.Deleted && e.Entity is Core.Interfaces.ISoftDeletable))
            {
                entry.State = EntityState.Modified;
                entry.Property("IsDeleted").CurrentValue = true;
                if (entry.Metadata.FindProperty("DeletedAt") != null) entry.Property("DeletedAt").CurrentValue = DateTime.UtcNow;
            }
            var result = await base.SaveChangesAsync(true, cancellationToken);
            foreach (var file in files)
            {
                var now = DateTime.UtcNow;
                foreach (var change in affected[file.FileID])
                {
                    var tableId = LifecycleEntityType(change.Entry.Entity);
                    var key = change.Entry.Metadata.FindPrimaryKey()!.Properties.First();
                    var entityId = Convert.ToInt32(change.Entry.Property(key.Name).CurrentValue);
                    var actor = LifecycleUserID > 0 ? LifecycleUserID : ReadLifecycleActor(change.Entry);
                    var keys = change.Entry.Metadata.FindPrimaryKey()!.Properties;
                    int? secondaryKey = keys.Count > 1 ? Convert.ToInt32(change.Entry.Property(keys[1].Name).CurrentValue) : null;
                    var firstVersion = await FileReviewItems.AsNoTracking().Include(i => i.AuditLog)
                        .Where(i => i.RetirementFileID == file.FileID && i.CreatedAt >= file.ReviewRequestedAt &&
                            i.SectionType == change.Entry.Metadata.ClrType.Name && i.EntityID == entityId &&
                            i.AuditLog != null && i.AuditLog.EntityKey == secondaryKey)
                        .OrderBy(i => i.CreatedAt).ThenBy(i => i.ItemID).FirstOrDefaultAsync(cancellationToken);
                    var originalJson = firstVersion?.AuditLog != null ? firstVersion.AuditLog.OldValues : change.OldJson;
                    var audit = new AuditLog
                    {
                        UserID = actor, TableID = SystemTableConstants.RetirementFiles,
                        RecordID = file.FileID, Action = "ReviewDataChange",
                        EntityKey = secondaryKey,
                        EntityDisplayName = LifecycleSection(change.Entry.Entity),
                        OldValues = originalJson, NewValues = SerializeValues(change.Entry, false), CreatedAt = now
                    };
                    FileReviewItems.Add(new FileReviewItem
                    {
                        RetirementFileID = file.FileID,
                        EntityType = tableId, EntityID = entityId,
                        AuditLog = audit,
                        FieldName = change.OldJson == null ? "إضافة" : change.Entry.Metadata.FindProperty("IsDeleted") != null && change.Entry.Property("IsDeleted").CurrentValue is true ? "حذف" : "تعديل",
                        OldValue = change.OldJson == null ? null : "البيانات السابقة",
                        NewValue = "بانتظار مراجعة البيانات", SectionType = change.Entry.Metadata.ClrType.Name,
                        SectionName = LifecycleSection(change.Entry.Entity),
                        FieldChangesJson = CreateFieldChanges(originalJson, SerializeValues(change.Entry, false)),
                        CreatedAt = now
                    });
                }
                if (!await PensionSuspensions.AnyAsync(s => s.FileID == file.FileID && !s.IsDeleted &&
                    s.SuspensionSource == RetirementReviewConstants.SuspensionSource && s.Status == SuspensionStatusConstants.Active, cancellationToken))
                    PensionSuspensions.Add(new PensionSuspension
                    {
                        FileID = file.FileID,
                        SuspensionSource = RetirementReviewConstants.SuspensionSource,
                        SuspensionReason = RetirementReviewConstants.SuspensionReason,
                        Status = SuspensionStatusConstants.Active, SuspensionDate = now, CreatedAt = now,
                        CreatedByUserID = LifecycleUserID > 0 ? LifecycleUserID : ReadLifecycleActor(affected[file.FileID][0].Entry)
                    });
            }
            if (files.Count > 0) await base.SaveChangesAsync(true, cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally { _savingLifecycle = false; }
    }

    private static int ReadLifecycleActor(EntityEntry entry)
    {
        foreach (var name in new[] { "UpdatedByUserID", "DeletedByUserID", "CreatedByUserID" })
            if (entry.Metadata.FindProperty(name) != null && entry.Property(name).CurrentValue is int userId && userId > 0) return userId;
        return LookupConstants.SystemUserId;
    }

    private static string SerializeValues(EntityEntry entry, bool original) => JsonSerializer.Serialize(
        entry.Properties.Where(p => !LifecycleTechnicalFields.Contains(p.Metadata.Name))
            .ToDictionary(p => p.Metadata.Name, p => original ? p.OriginalValue : p.CurrentValue));

    private static string CreateFieldChanges(string? oldJson, string newJson)
    {
        var oldValues = oldJson == null ? new Dictionary<string, JsonElement>() : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldJson)!;
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(newJson)!;
        return JsonSerializer.Serialize(values.Where(p => !oldValues.TryGetValue(p.Key, out var old) || old.ToString() != p.Value.ToString())
            .Select(p => new { fieldName = p.Key, oldValue = oldValues.TryGetValue(p.Key, out var old) ? old.ToString() : null, newValue = p.Value.ToString() }));
    }

    private async Task<List<int>> ResolveLifecycleFilesAsync(EntityEntry entry, CancellationToken cancellationToken)
    {
        int Value(string name, bool original = false) => entry.Metadata.FindProperty(name) == null ? 0 :
            Convert.ToInt32(original ? entry.Property(name).OriginalValue : entry.Property(name).CurrentValue);
        var ids = new HashSet<int>();
        foreach (var original in new[] { false, true })
        {
            var fileId = Value("FileID", original);
            if (fileId > 0) ids.Add(fileId);
            if (entry.Entity is Retiree or FinancialEntitlement || entry.Entity is Document document &&
                Value("RetireeID", original) > 0 && (fileId == 0 || document.OwnerType == DocumentOwnerType.Retiree))
            {
                var retireeId = Value("RetireeID", original);
                foreach (var id in await RetirementFiles.AsNoTracking().Where(f => f.RetireeID == retireeId).Select(f => f.FileID).ToListAsync(cancellationToken)) ids.Add(id);
            }
            if (entry.Entity is GuardianEntitledPerson || entry.Entity is Document && fileId == 0)
            {
                var entitledId = Value("EntitledID", original);
                var agentId = Value("AgentID", original);
                var guardianId = Value("GuardianID", original);
                foreach (var id in await EntitledPersons.IgnoreQueryFilters().Where(e => e.EntitledID == entitledId).Select(e => e.FileID).ToListAsync(cancellationToken)) ids.Add(id);
                foreach (var id in await Agents.IgnoreQueryFilters().Where(e => e.AgentID == agentId).Select(e => e.FileID).ToListAsync(cancellationToken)) ids.Add(id);
                foreach (var id in await Guardians.IgnoreQueryFilters().Where(e => e.GuardianID == guardianId).Select(e => e.FileID).ToListAsync(cancellationToken)) ids.Add(id);
                var pledgeId = Value("PledgeID", original);
                var exemptionId = Value("ExemptionID", original);
                var noteId = Value("NoteID", original);
                var suspensionId = Value("SuspensionID", original);
                foreach (var id in await DocumentPledges.IgnoreQueryFilters().Where(e => e.PledgeID == pledgeId).Select(e => e.FileID).ToListAsync(cancellationToken)) ids.Add(id);
                foreach (var id in await DocumentExemptions.IgnoreQueryFilters().Where(e => e.ExemptionID == exemptionId).Select(e => e.FileID).ToListAsync(cancellationToken)) ids.Add(id);
                foreach (var id in await FileNotes.IgnoreQueryFilters().Where(e => e.NoteID == noteId).Select(e => e.FileID).ToListAsync(cancellationToken)) ids.Add(id);
                foreach (var id in await PensionSuspensions.IgnoreQueryFilters().Where(e => e.SuspensionID == suspensionId).Select(e => e.FileID).ToListAsync(cancellationToken)) ids.Add(id);
            }
        }
        return ids.ToList();
    }

    private static int LifecycleEntityType(object entity) => entity switch
    {
        Retiree => 1, RetirementFile => 2, EntitledPerson => 3, Agent => 4, Guardian => 5,
        Document => 6, FinancialEntitlement => 13, SalaryRecipient => 12,
        DocumentPledge => 7, MissingDocument => 8, DocumentExemption => 9, FileNote => 10,
        _ => 0
    };
    private static string LifecycleSection(object entity) => entity switch
    {
        Retiree => "بيانات المتقاعد", RetirementFile => "بيانات الملف", EntitledPerson => "المستحقون",
        Agent => "الوكلاء", Guardian or GuardianEntitledPerson => "الأوصياء", Document => "الوثائق",
        FinancialEntitlement => "الاستحقاقات المالية", SalaryRecipient => "مستلمو المعاش",
        PaymentBook => "دفاتر الصرف", DocumentPledge => "التعهدات", DocumentExemption => "الإعفاءات",
        MissingDocument => "نواقص الوثائق", FileNote => "ملاحظات الملف", _ => "بيانات مرتبطة بالملف"
    };
}
