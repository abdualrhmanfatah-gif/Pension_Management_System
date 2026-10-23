using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

public class FileReviewService : IFileReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IMissingDocumentService _missingDocuments;

    public FileReviewService(IUnitOfWork unitOfWork, IAccessiblePensionRepository access, IMissingDocumentService missingDocuments)
    {
        _unitOfWork = unitOfWork;
        _access = access;
        _missingDocuments = missingDocuments;
    }

    public async Task<ApiResponse<List<RetirementFileReviewDto>>> GetPendingReviewsAsync()
    {
        var files = await (await _access.ForPermissionAsync("File.Review.View")).Files()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.StatusID == FileStatusConstants.Pending)
            .Select(f => new RetirementFileReviewDto
            {
                FileID = f.FileID,
                FileNumber = f.FileNumber,
                RetireeFullName = f.Retiree.FullName,
                NationalID = f.Retiree.NationalID,
                StatusID = f.StatusID,
                FileStatusName = f.Status!.StatusName,
                FileOpenDate = f.FileOpenDate,
                DocumentsCount = f.Documents.Count(d => !d.IsDeleted),
                MissingDocumentsCount = f.MissingDocuments.Count(m => m.Status == MissingDocumentStatusConstants.Pending),
                MissingDataCount = f.MissingDocumentsCount,
                HasRetireeInfo = f.Retiree != null,
                HasAllRequiredData = f.MissingDocumentsCount == 0,
                ReviewStatus = (int)f.ReviewStatus,
                ReviewRequestedAt = f.ReviewRequestedAt
            })
            .OrderByDescending(f => f.ReviewRequestedAt)
            .ToListAsync();

        return ApiResponse<List<RetirementFileReviewDto>>.Success(files);
    }

    public async Task<ApiResponse<FileReviewDiffDto>> GetReviewDiffAsync(int fileId)
    {
        var file = await (await _access.ForPermissionAsync("File.Review.View")).Files()
            .Include(f => f.Retiree)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileID == fileId && !f.IsDeleted);

        if (file == null)
            return ApiResponse<FileReviewDiffDto>.Fail("الملف غير موجود");

        if (file.ReviewStatus is not (FileReviewStatus.PendingReview or FileReviewStatus.Approved or FileReviewStatus.Rejected))
            return ApiResponse<FileReviewDiffDto>.Fail("الملف لا يحتاج مراجعة");

        var items = await LoadCurrentReviewItemsAsync(_unitOfWork.FileReviewItems.Query().AsNoTracking(), file);

        var lookups = await LoadLookupsAsync(fileId);

        var sections = items
            .GroupBy(i => new { i.SectionName, i.SectionType })
            .Select(g =>
            {
                var entityName = g.First().EntityDisplayName;
                return new FileReviewSectionDto
                {
                    SectionName = g.Key.SectionName ?? "",
                    SectionType = g.Key.SectionType ?? "",
                    EntityDisplayName = entityName,
                    FieldChanges = g.Select(i =>
                    {
                        var dto = new FieldChangeDto
                        {
                            FieldName = i.FieldName,
                            OldValue = i.OldValue,
                            NewValue = i.NewValue,
                            ItemId = i.ItemID,
                            ItemStatus = i.Status,
                            FieldChangesJson = i.FieldChangesJson,
                            ModifiedByUserName = i.AuditLog?.User?.FullName
                        };

                        if (!string.IsNullOrWhiteSpace(i.FieldChangesJson))
                        {
                            try
                            {
                                var fieldDetails = JsonSerializer.Deserialize<List<FieldChangeDetailDto>>(i.FieldChangesJson,
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                                if (fieldDetails != null)
                                {
                                    foreach (var field in fieldDetails)
                                    {
                                        var rawFieldName = ReverseArabicFieldName(field.FieldName);
                                        field.OldValue = ResolveValueFromLookups(rawFieldName, field.OldValue, lookups);
                                        field.NewValue = ResolveValueFromLookups(rawFieldName, field.NewValue, lookups);
                                        field.FieldName = GetArabicFieldName(rawFieldName);
                                    }

                                    dto.FieldDetails = fieldDetails;
                                    dto.FieldChangesJson = JsonSerializer.Serialize(fieldDetails, new JsonSerializerOptions
                                    {
                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                    });
                                }
                            }
                            catch { }
                        }

                        return dto;
                    }).ToList()
                };
            })
            .ToList();

        var firstItemWithUser = items.FirstOrDefault(i => i.AuditLog?.User != null);
        string? requestedByUserName = firstItemWithUser?.AuditLog?.User?.FullName;

        var result = new FileReviewDiffDto
        {
            FileID = file.FileID,
            FileNumber = file.FileNumber,
            RetireeFullName = file.Retiree?.FullName ?? "",
            ReviewRequestedAt = file.ReviewRequestedAt ?? DateTime.UtcNow,
            ReviewRequestedByUserName = requestedByUserName,
            Sections = sections
        };

        return ApiResponse<FileReviewDiffDto>.Success(result);
    }

    private async Task<ApiResponse<bool>> WithReviewTransactionAsync(int fileId, Func<Task<ApiResponse<bool>>> action, string? permission = null)
    {
        var queries = permission is null ? null : await _access.ForPermissionAsync(permission);
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.LockFileReviewAsync(fileId);
            if (queries is not null && !await queries.Files().AnyAsync(f => f.FileID == fileId))
            {
                if (ownsTransaction) await _unitOfWork.RollbackAsync();
                return ApiResponse<bool>.Fail("الملف غير موجود أو خارج نطاق صلاحياتك");
            }
            var result = await action();
            if (ownsTransaction)
            {
                if (result.IsSuccess) await _unitOfWork.CommitAsync();
                else await _unitOfWork.RollbackAsync();
            }
            return result;
        }
        catch
        {
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // Mutation callers are idempotent; initial preparation only leaves New on explicit submission.
    public Task<ApiResponse<bool>> RequestReviewAsync(int fileId, int userId, DateTime? requestedAt = null)
        => WithReviewTransactionAsync(fileId, () => RequestReviewCoreAsync(fileId, userId, false, false));

    public Task<ApiResponse<bool>> SubmitReviewAsync(int fileId, int userId)
        => WithReviewTransactionAsync(fileId, () => RequestReviewCoreAsync(fileId, userId, true, false), "File.Edit.BasicData");

    public Task<ApiResponse<bool>> ReinstateEntitlementAsync(int fileId, int userId)
        => WithReviewTransactionAsync(fileId, () => RequestReviewCoreAsync(fileId, userId, true, true), "File.Edit.BasicData");

    private async Task<ApiResponse<bool>> RequestReviewCoreAsync(int fileId, int userId, bool submit, bool reinstate)
    {
        var file = await _unitOfWork.RetirementFiles.Query().Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted) return ApiResponse<bool>.Fail("الملف غير موجود");
        if (reinstate && file.StatusID != FileStatusConstants.EntitlementEnded &&
            !(file.StatusID == FileStatusConstants.Pending && file.PreviousStatusID == FileStatusConstants.EntitlementEnded))
            return ApiResponse<bool>.Fail("إعادة الاستحقاق متاحة للملف الذي انتهى استحقاقه فقط");
        if (!submit && file.StatusID == FileStatusConstants.New && file.ReviewStatus == FileReviewStatus.None)
            return ApiResponse<bool>.Success(true, "الملف جديد؛ أرسله للمراجعة بعد استكمال التجهيز");
        if (!reinstate && file.StatusID == FileStatusConstants.Pending && file.ReviewStatus == FileReviewStatus.PendingReview &&
            (!submit || !(await LoadCurrentReviewItemsAsync(_unitOfWork.FileReviewItems.Query(), file))
                .Any(i => i.Status == FileReviewItemStatusConstants.Rejected)))
            return ApiResponse<bool>.Success(true, "الملف بانتظار المراجعة");
        var now = DateTime.UtcNow;
        if (reinstate) file.PreviousStatusID = FileStatusConstants.Active;
        else if (file.StatusID != FileStatusConstants.Pending) file.PreviousStatusID = file.StatusID;
        file.StatusID = FileStatusConstants.Pending;
        file.ReviewStatus = FileReviewStatus.PendingReview;
        file.ReviewRequestedAt = now;
        file.ReviewNotes = null;
        file.UpdatedAt = now;
        var state = await ApprovedPensionSnapshot.CaptureAsync(_unitOfWork, fileId, file.RetireeID);
        await _unitOfWork.FileReviewItems.AddAsync(new FileReviewItem
        {
            RetirementFileID = fileId, EntityType = SystemTableConstants.RetirementFiles,
            EntityID = fileId, SectionName = "مراجعة بيانات الملف", SectionType = "File",
            FieldName = reinstate ? "إعادة الاستحقاق" : "إرسال للمراجعة",
            NewValue = "مراجعة جميع بيانات الملف والوثائق المرتبطة به",
            FieldChangesJson = JsonSerializer.Serialize(new[]
            {
                new { fieldName = "رقم الملف", oldValue = (string?)null, newValue = file.FileNumber },
                new { fieldName = "اسم المتقاعد", oldValue = (string?)null, newValue = file.Retiree.FullName },
                new { fieldName = "المستحقون", oldValue = (string?)null, newValue = state.EntitledPersons.Count.ToString() },
                new { fieldName = "مستلمو المعاش", oldValue = (string?)null, newValue = state.Recipients.Count.ToString() },
                new { fieldName = "دفاتر الصرف", oldValue = (string?)null, newValue = state.Books.Count.ToString() }
            }),
            AuditLog = new AuditLog { UserID = userId, TableID = SystemTableConstants.RetirementFiles,
                RecordID = fileId, Action = "RequestReview", CreatedAt = now },
            CreatedAt = now
        });
        if (!await _unitOfWork.PensionSuspensions.Query().AnyAsync(s => s.FileID == fileId && !s.IsDeleted &&
            s.SuspensionSource == RetirementReviewConstants.SuspensionSource && s.Status == SuspensionStatusConstants.Active))
            await _unitOfWork.PensionSuspensions.AddAsync(new PensionSuspension
            {
                FileID = fileId, CreatedByUserID = userId, SuspensionDate = now, CreatedAt = now,
                SuspensionSource = RetirementReviewConstants.SuspensionSource,
                SuspensionReason = RetirementReviewConstants.SuspensionReason, Status = SuspensionStatusConstants.Active
            });
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم إرسال الملف للمراجعة وإيقاف صرف المعاش");
    }

    public Task<ApiResponse<bool>> ApproveReviewAsync(int fileId, int userId, string? notes = null)
        => WithReviewTransactionAsync(fileId, () => ApproveReviewCoreAsync(fileId, userId, notes), "File.Review.Approve");

    private async Task<ApiResponse<bool>> ApproveReviewCoreAsync(int fileId, int userId, string? notes)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        if (file.ReviewStatus != FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Fail("الملف لا يحتاج مراجعة");

        var activationError = await RetirementFileActivationValidator.ApprovedStatusAsync(_unitOfWork, file) == FileStatusConstants.EntitlementEnded ? null
            : await RetirementFileActivationValidator.ValidateAsync(_unitOfWork, _missingDocuments, fileId);
        if (activationError != null) return ApiResponse<bool>.Fail(activationError);

        var currentItems = await LoadCurrentReviewItemsAsync(_unitOfWork.FileReviewItems.Query(), file);
        if (currentItems.Any(i => i.Status == FileReviewItemStatusConstants.Rejected))
            return ApiResponse<bool>.Fail("توجد بنود مرفوضة؛ صحّح البيانات وأعد إرسالها للمراجعة");
        var pendingItems = currentItems.Where(i => i.Status == FileReviewItemStatusConstants.Pending).ToList();

        var now = DateTime.UtcNow;
        file.ReviewNotes = notes;
        foreach (var item in pendingItems)
        {
            item.Status = FileReviewItemStatusConstants.Approved;
            item.ReviewedByUserID = userId;
            item.ReviewedAt = now;
            item.ReviewNotes = notes;
            _unitOfWork.FileReviewItems.Update(item);
        }

        await _unitOfWork.SaveChangesAsync();
        await AutoResolveFileReviewCoreAsync(fileId);
        await SetEntityReviewStatusForFileAsync(fileId, (int)PendingReviewStatus.Approved);

        return ApiResponse<bool>.Success(true, "تم اعتماد جميع البنود");
    }

    public Task<ApiResponse<bool>> RejectReviewAsync(int fileId, int userId, string reason)
        => WithReviewTransactionAsync(fileId, () => RejectReviewCoreAsync(fileId, userId, reason), "File.Review.Reject");

    private async Task<ApiResponse<bool>> RejectReviewCoreAsync(int fileId, int userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return ApiResponse<bool>.Fail("يجب إدخال سبب رفض المراجعة");
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        if (file.ReviewStatus != FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Fail("الملف لا يحتاج مراجعة");

        var pendingItems = (await LoadCurrentReviewItemsAsync(_unitOfWork.FileReviewItems.Query(), file))
            .Where(i => i.Status == FileReviewItemStatusConstants.Pending).ToList();

        var now = DateTime.UtcNow;
        file.ReviewNotes = reason.Trim();
        foreach (var item in pendingItems)
        {
            item.Status = FileReviewItemStatusConstants.Rejected;
            item.ReviewedByUserID = userId;
            item.ReviewedAt = now;
            item.ReviewNotes = reason;
            _unitOfWork.FileReviewItems.Update(item);
        }

        await _unitOfWork.SaveChangesAsync();
        await AutoResolveFileReviewCoreAsync(fileId);
        await SetEntityReviewStatusForFileAsync(fileId, (int)PendingReviewStatus.Rejected);

        return ApiResponse<bool>.Success(true, "تم رفض جميع البنود");
    }

    private async Task<ApiResponse<bool>> WithItemReviewTransactionAsync(long itemId, Func<Task<ApiResponse<bool>>> action, string permission)
    {
        var fileId = await _unitOfWork.FileReviewItems.Query().AsNoTracking()
            .Where(i => i.ItemID == itemId).Select(i => (int?)i.RetirementFileID).FirstOrDefaultAsync();
        return fileId.HasValue
            ? await WithReviewTransactionAsync(fileId.Value, action, permission)
            : ApiResponse<bool>.Fail("بند المراجعة غير موجود");
    }

    public Task<ApiResponse<bool>> ApproveItemAsync(long itemId, int userId, string? notes = null)
        => WithItemReviewTransactionAsync(itemId, () => ApproveItemCoreAsync(itemId, userId, notes), "File.Review.Approve");

    private async Task<ApiResponse<bool>> ApproveItemCoreAsync(long itemId, int userId, string? notes)
    {
        var item = await _unitOfWork.FileReviewItems.Query()
            .FirstOrDefaultAsync(i => i.ItemID == itemId);
        if (item == null)
            return ApiResponse<bool>.Fail("بند المراجعة غير موجود");

        await _unitOfWork.FileReviewItems.ReloadAsync(item);
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(item.RetirementFileID);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted || file.ReviewStatus != FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Fail("الملف لا يحتاج مراجعة حالياً");

        if (!(await LoadCurrentReviewItemsAsync(_unitOfWork.FileReviewItems.Query(), file)).Any(i => i.ItemID == itemId))
            return ApiResponse<bool>.Fail("هذا البند من مراجعة سابقة؛ حدّث الصفحة");

        if (item.Status != FileReviewItemStatusConstants.Pending)
            return ApiResponse<bool>.Fail("تم البت في هذا البند مسبقاً");

        var activationError = await RetirementFileActivationValidator.ApprovedStatusAsync(_unitOfWork, file) == FileStatusConstants.EntitlementEnded ? null
            : await RetirementFileActivationValidator.ValidateAsync(_unitOfWork, _missingDocuments, item.RetirementFileID);
        if (activationError != null) return ApiResponse<bool>.Fail(activationError);

        item.Status = FileReviewItemStatusConstants.Approved;
        item.ReviewedByUserID = userId;
        item.ReviewedAt = DateTime.UtcNow;
        item.ReviewNotes = notes;

        _unitOfWork.FileReviewItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        await AutoResolveFileReviewCoreAsync(item.RetirementFileID);

        return ApiResponse<bool>.Success(true, "تم اعتماد التغيير");
    }

    public Task<ApiResponse<bool>> RejectItemAsync(long itemId, int userId, string reason)
        => WithItemReviewTransactionAsync(itemId, () => RejectItemCoreAsync(itemId, userId, reason), "File.Review.Reject");

    private async Task<ApiResponse<bool>> RejectItemCoreAsync(long itemId, int userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return ApiResponse<bool>.Fail("يجب إدخال سبب رفض البند");
        var item = await _unitOfWork.FileReviewItems.Query()
            .FirstOrDefaultAsync(i => i.ItemID == itemId);
        if (item == null)
            return ApiResponse<bool>.Fail("بند المراجعة غير موجود");

        await _unitOfWork.FileReviewItems.ReloadAsync(item);
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(item.RetirementFileID);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted || file.ReviewStatus != FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Fail("الملف لا يحتاج مراجعة حالياً");

        if (!(await LoadCurrentReviewItemsAsync(_unitOfWork.FileReviewItems.Query(), file)).Any(i => i.ItemID == itemId))
            return ApiResponse<bool>.Fail("هذا البند من مراجعة سابقة؛ حدّث الصفحة");

        if (item.Status != FileReviewItemStatusConstants.Pending)
            return ApiResponse<bool>.Fail("تم البت في هذا البند مسبقاً");

        item.Status = FileReviewItemStatusConstants.Rejected;
        item.ReviewedByUserID = userId;
        item.ReviewedAt = DateTime.UtcNow;
        item.ReviewNotes = reason;

        _unitOfWork.FileReviewItems.Update(item);
        await _unitOfWork.SaveChangesAsync();


        await AutoResolveFileReviewCoreAsync(item.RetirementFileID);

        return ApiResponse<bool>.Success(true, "تم رفض التغيير");
    }

    public async Task AutoResolveFileReviewAsync(int fileId)
        => await WithReviewTransactionAsync(fileId, async () =>
        {
            var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
            if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
            await AutoResolveFileReviewCoreAsync(fileId);
            return ApiResponse<bool>.Success(true);
        });

    private async Task AutoResolveFileReviewCoreAsync(int fileId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file == null || file.IsDeleted)
            return;

        if (file.ReviewStatus != FileReviewStatus.PendingReview) return;
        var currentItems = await LoadCurrentReviewItemsAsync(_unitOfWork.FileReviewItems.Query(), file);
        if (currentItems.Count == 0 || currentItems.Any(i => i.Status == FileReviewItemStatusConstants.Pending)) return;
        var rejectedCount = currentItems.Count(i => i.Status == FileReviewItemStatusConstants.Rejected);

        var now = DateTime.UtcNow;
        file.ReviewStatus = rejectedCount > 0 ? FileReviewStatus.Rejected : FileReviewStatus.Approved;
        file.ReviewCompletedAt = now;
        file.UpdatedAt = now;

        file.StatusID = rejectedCount > 0 ? FileStatusConstants.Pending
            : await RetirementFileActivationValidator.ApprovedStatusAsync(_unitOfWork, file);
        if (rejectedCount == 0)
        {
            var reviewerId = currentItems.OrderByDescending(i => i.ReviewedAt).FirstOrDefault()?.ReviewedByUserID;
            var approvedState = await ApprovedPensionSnapshot.CaptureAsync(_unitOfWork, fileId, file.RetireeID);
            approvedState.MarkReviewed();
            approvedState.ApprovedAtUtc = now;
            file.LastReviewedFileState = approvedState.Serialize();
            file.PreviousStatusID = null;
            var reviewHolds = await _unitOfWork.PensionSuspensions.Query().Where(s => s.FileID == fileId && !s.IsDeleted &&
                s.Status == SuspensionStatusConstants.Active && s.SuspensionSource == RetirementReviewConstants.SuspensionSource).ToListAsync();
            foreach (var hold in reviewHolds)
            {
                hold.Status = SuspensionStatusConstants.Lifted;
                hold.EndDate = now;
                hold.EndReason = "اعتماد مراجعة بيانات الملف";
                hold.EndedByUserID = reviewerId;
            }
            await _unitOfWork.StatusTransitionLogs.AddAsync(new StatusTransitionLog
            {
                TableID = SystemTableConstants.RetirementFiles, RecordID = fileId,
                OldStatusID = FileStatusConstants.Pending, NewStatusID = file.StatusID!.Value,
                Reason = "اعتماد مراجعة بيانات الملف", ChangedByUserID = reviewerId ?? LookupConstants.SystemUserId,
                TransitionDate = now
            });
        }

        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.SaveChangesAsync();

        var entityStatus = rejectedCount > 0 ? (int)PendingReviewStatus.Rejected : (int)PendingReviewStatus.Approved;
        await SetEntityReviewStatusForFileAsync(fileId, entityStatus);
    }

    // ═══════════════════════════════════════
    //  PENDING REVIEW STATUS
    // ═══════════════════════════════════════

    private static async Task<List<FileReviewItem>> LoadCurrentReviewItemsAsync(
        IQueryable<FileReviewItem> query, PensionMS.Core.Entities.RetirementFile file)
    {
        var items = await query.Include(i => i.AuditLog).ThenInclude(a => a!.User)
            .Where(i => i.RetirementFileID == file.FileID && file.ReviewRequestedAt.HasValue &&
                i.CreatedAt >= file.ReviewRequestedAt.Value).ToListAsync();
        // أحدث نسخة من السجل تحل محل القرار السابق في الدورة الحالية، مع حفظ البنود القديمة للتدقيق.
        return items.GroupBy(GetReviewItemKey).Select(g => g.OrderByDescending(i => i.CreatedAt)
            .ThenByDescending(i => i.ItemID).First()).OrderBy(i => i.CreatedAt).ToList();
    }

    private static string GetReviewItemKey(FileReviewItem item) => item.EntityID.HasValue
        ? $"{item.SectionType}:{item.EntityType}:{item.EntityID}:{item.AuditLog?.EntityKey}"
        : $"item:{item.ItemID}";

    private async Task SetEntityReviewStatusForFileAsync(int fileId, int newStatus)
    {
        var entitledPersons = await _unitOfWork.EntitledPersons.Query()
            .Where(e => e.FileID == fileId)
            .ToListAsync();
        foreach (var e in entitledPersons) e.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var agents = await _unitOfWork.Agents.Query()
            .Where(a => a.FileID == fileId)
            .ToListAsync();
        foreach (var a in agents) a.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var guardians = await _unitOfWork.Guardians.Query()
            .Where(g => g.FileID == fileId)
            .ToListAsync();
        foreach (var g in guardians) g.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var books = await _unitOfWork.PaymentBooks.Query()
            .Where(b => b.FileID == fileId)
            .ToListAsync();
        foreach (var b in books) b.PendingReviewStatus = newStatus;

        var recipients = await _unitOfWork.SalaryRecipients.Query()
            .Where(r => r.FileID == fileId)
            .ToListAsync();
        foreach (var r in recipients) r.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var documents = await _unitOfWork.Documents.Query()
            .Where(d => d.FileID == fileId)
            .ToListAsync();
        foreach (var d in documents) d.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);
        if (file?.Retiree != null)
            file.Retiree.PendingReviewStatus = (PendingReviewStatus)newStatus;

        await _unitOfWork.SaveChangesAsync();
    }

    // Compatibility endpoint: resubmit for review; never clear rejection or restore data implicitly.
    public async Task<ApiResponse<bool>> AcknowledgeEntityRejectionAsync(string entityType, int entityId, int userId)
    {
        var fileIds = entityType switch
        {
            "EntitledPerson" => await _unitOfWork.EntitledPersons.Query().Where(e => e.EntitledID == entityId).Select(e => e.FileID).ToListAsync(),
            "Agent" => await _unitOfWork.Agents.Query().Where(e => e.AgentID == entityId).Select(e => e.FileID).ToListAsync(),
            "Guardian" => await _unitOfWork.Guardians.Query().Where(e => e.GuardianID == entityId).Select(e => e.FileID).ToListAsync(),
            "PaymentBook" => await _unitOfWork.PaymentBooks.Query().Where(e => e.PaymentBookID == entityId).Select(e => e.FileID).ToListAsync(),
            "SalaryRecipient" => await _unitOfWork.SalaryRecipients.Query().Where(e => e.RecipientID == entityId).Select(e => e.FileID).ToListAsync(),
            "Retiree" => await (await _access.ForPermissionAsync("File.Edit.BasicData")).Files().Where(f => f.RetireeID == entityId).Select(f => f.FileID).ToListAsync(),
            _ => new List<int>()
        };
        if (fileIds.Count == 0) return ApiResponse<bool>.Fail("السجل غير موجود أو نوعه غير مدعوم");
        return await Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            foreach (var fileId in fileIds.Distinct().Order())
            {
                var result = await SubmitReviewAsync(fileId, userId);
                if (!result.IsSuccess) return result;
            }
            return ApiResponse<bool>.Success(true, "تم إرسال البيانات للمراجعة؛ يبقى الصرف موقوفًا حتى الاعتماد");
        });
    }

    private static string GetArabicFieldName(string fieldName)
    {
        return fieldName switch
        {
            "AgencyScope" => "نطاق الوكالة",
            "FileNumber" => "رقم الملف",
            "StatusID" => "حالة الملفات",
            "Notes" => "ملاحظات",
            "FullName" => "الاسم الكامل",
            "NationalID" => "رقم الهوية",
            "Phone" => "رقم الهاتف",
            "Address" => "العنوان",
            "City" => "المدينة",
            "Governorate" => "المحافظة",
            "BirthDate" => "تاريخ الميلاد",
            "RetirementDate" => "تاريخ التقاعد",
            "ServiceStartDate" => "تاريخ بداية الخدمة",
            "ServiceEndDate" => "تاريخ نهاية الخدمة",
            "Gender" => "الجنس",
            "SocialStatusID" => "الحالة الاجتماعية",
            "HealthStatusID" => "الحالة الصحية",
            "IsAlive" => "على قيد الحياة",
            "DeathDate" => "تاريخ الوفاة",
            "RankID" => "الرتبة",
            "RankName" => "الرتبة",
            "PensionBranchID" => "فرع المعاش",
            "BranchID" => "فرع العمالات",
            "BranchName" => "فرع العمالات",
            "AssignedEmployeeID" => "الموظف المسؤول",
            "BookNumber" => "رقم الدفتر",
            "BookType" => "نوع الدفتر",
            "IssueDate" => "تاريخ الإصدار",
            "ExpiryDate" => "تاريخ الانتهاء",
            "Status" => "الحالة",
            "ChangeReason" => "سبب التغيير",
            "LegacyColor" => "لون الدفتر",
            "RecipientName" => "اسم المستلم",
            "RecipientType" => "نوع المستلم",
            "StartDate" => "تاريخ البداية",
            "EndDate" => "تاريخ النهاية",
            "EffectiveDate" => "تاريخ بدء الاستحقاق",
            "FileID" => "معرف الملف",
            "RetireeID" => "معرف المتقاعد",
            "EntitledID" => "المستحق",
            "GuardianID" => "الوصي",
            "AgentID" => "الوكيل",
            "RecipientID" => "معرف المستلم",
            "PaymentBookID" => "معرف دفتر الصرف",
            "DocumentID" => "معرف الوثيقة",
            "DocumentTypeID" => "نوع الوثيقة",
            "FileName" => "اسم الوثيقة",
            "FilePath" => "الوثيقة",
            "DocumentNumber" => "رقم الوثيقة",
            "OwnerType" => "مالك الوثيقة",
            "SharePercent" => "نسبة الحصة",
            "IsDeleted" => "محذوف",
            "IsActive" => "نشط",
            "Amount" => "المبلغ",
            "Percentage" => "النسبة",
            "Reason" => "السبب",
            "StopCode" => "رمز الإيقاف",
            "RelationshipID" => "صلة القرابة",
            "RelationshipName" => "صلة القرابة",
            "IsEligible" => "مؤهل",
            "IsDeceased" => "متوفي",
            "HasDisabilityException" => "استثناء إعاقة",
            "FileOpenDate" => "تاريخ فتح الملف",
            "MissingDocumentsCount" => "عدد الوثائق الناقصة",
            "MissingDataCount" => "عدد البيانات الناقصة",
            "DocumentsCount" => "عدد الوثائق",
            "EntitledPersonsCount" => "عدد المستحقين",
            "AgentsCount" => "عدد الوكلاء",
            "GuardiansCount" => "عدد الأوصياء",
            "PaymentBooksCount" => "عدد دفاتر الصرف",
            "RecipientsCount" => "عدد المستلمين",
            "MilitaryNumber" => "الرقم العسكري",
            "UnitName" => "الوحدة",
            "CategoryName" => "الفئة",
            "LinkNumber" => "رقم الربط",
            "PersonalPhotoPath" => "الصورة الشخصية",
            "NationalIDDocumentPath" => "وثيقة الهوية",
            "AgencyDocumentPath" => "وثيقة الوكالة",
            "GuardianshipDocumentPath" => "وثيقة الوصاية",
            "AgencyNumber" => "رقم الوكالة",
            "AgencyDate" => "تاريخ الوكالة",
            "AgencyEndDate" => "تاريخ انتهاء الوكالة",
            "GuardianshipDocNumber" => "رقم وثيقة الوصاية",
            "SocialStatusName" => "الحالة الاجتماعية",
            "HealthStatusName" => "الحالة الصحية",
            "CategoryID" => "الفئة",
            "UnitID" => "الوحدة",
            _ => fieldName
        };
    }

    private async Task<Dictionary<string, Dictionary<int, string>>> LoadLookupsAsync(int fileId)
    {
        var lookups = new Dictionary<string, Dictionary<int, string>>();

        var socialStatuses = await _unitOfWork.SocialStatuses.Query().AsNoTracking()
            .ToDictionaryAsync(s => s.SocialStatusID, s => s.StatusName);
        lookups["SocialStatusID"] = socialStatuses;

        var healthStatuses = await _unitOfWork.HealthStatuses.Query().AsNoTracking()
            .ToDictionaryAsync(s => s.HealthStatusID, s => s.HealthStatusName);
        lookups["HealthStatusID"] = healthStatuses;

        var fileStatuses = await _unitOfWork.FileStatuses.Query().AsNoTracking()
            .ToDictionaryAsync(s => s.StatusID, s => s.StatusName);
        lookups["StatusID"] = fileStatuses;

        var ranks = await _unitOfWork.Ranks.Query().AsNoTracking()
            .ToDictionaryAsync(r => r.RankID, r => r.RankName);
        lookups["RankID"] = ranks;

        var categories = await _unitOfWork.RetireeCategories.Query().AsNoTracking()
            .Where(c => c.IsActive)
            .ToDictionaryAsync(c => c.CategoryID, c => c.CategoryName);
        lookups["CategoryID"] = categories;

        var branches = await _unitOfWork.PensionFundBranches.Query().AsNoTracking()
            .Where(b => b.IsActive)
            .ToDictionaryAsync(b => b.PensionBranchID, b => b.BranchName);
        lookups["PensionBranchID"] = branches;
        lookups["BranchID"] = branches;

        var relationships = await _unitOfWork.Relationships.Query().AsNoTracking()
            .ToDictionaryAsync(r => r.RelationshipID, r => r.RelationshipName);
        lookups["RelationshipID"] = relationships;

        lookups["EntitledID"] = await _unitOfWork.EntitledPersons.Query().IgnoreQueryFilters().AsNoTracking().Where(e => e.FileID == fileId)
            .ToDictionaryAsync(e => e.EntitledID, e => e.FullName);

        return lookups;
    }

    private static string ResolveValueFromLookups(string fieldName, string? value, Dictionary<string, Dictionary<int, string>> lookups)
    {
        if (string.IsNullOrWhiteSpace(value)) return value ?? "";

        if (fieldName == "AgencyScope") return value switch
        {
            "1" or "Retiree" => "عن المتقاعد",
            "2" or "AllEntitled" => "عن جميع المستحقين",
            "3" or "SpecificEntitled" => "عن مستحق معين",
            _ => value
        };

        if (fieldName == "Gender")
        {
            return value switch
            {
                "1" => "ذكر",
                "2" => "أنثى",
                _ => value
            };
        }

        if (fieldName is "IsAlive" or "IsEligible" or "IsDeceased" or "IsActive")
        {
            return value switch
            {
                "true" or "True" => "نعم",
                "false" or "False" => "لا",
                _ => value
            };
        }

        if (fieldName == "Status")
        {
            return value switch
            {
                "فعّال" or "Active" => "فعّال",
                "مرفوع" or "Lifted" => "مرفوع",
                "منتهي" or "Expired" => "منتهي",
                _ => value
            };
        }

        if (fieldName == "BookType")
        {
            return value switch
            {
                "1" => "كتابي",
                "2" => "إلكتروني",
                _ => value
            };
        }

        if (fieldName == "StopCode")
        {
            return value switch
            {
                "1" => "إيقاف مؤقت",
                "2" => "إيقاف دائم",
                "3" => "رفع الإيقاف",
                _ => value
            };
        }

        if (fieldName is "BirthDate" or "DeathDate" or "RetirementDate" or "ServiceStartDate"
            or "ServiceEndDate" or "FileOpenDate" or "IssueDate" or "ExpiryDate"
            or "AgencyDate" or "AgencyEndDate" or "StartDate" or "EndDate")
        {
            if (DateTime.TryParse(value, out var dt))
            {
                return dt.ToString("yyyy-MM-dd");
            }
        }

        if (lookups.TryGetValue(fieldName, out var dict) && int.TryParse(value, out var id))
        {
            return dict.TryGetValue(id, out var name) ? name : value;
        }

        return value;
    }

    private static string ReverseArabicFieldName(string name)
    {
        return name switch
        {
            // Arabic → English
            "رقم الملف" => "FileNumber",
            "حالة الملفات" => "StatusID",
            "ملاحظات" => "Notes",
            "الاسم الكامل" => "FullName",
            "رقم الهوية" => "NationalID",
            "رقم الهاتف" => "Phone",
            "العنوان" => "Address",
            "المدينة" => "City",
            "المحافظة" => "Governorate",
            "تاريخ الميلاد" => "BirthDate",
            "تاريخ التقاعد" => "RetirementDate",
            "تاريخ بداية الخدمة" => "ServiceStartDate",
            "تاريخ نهاية الخدمة" => "ServiceEndDate",
            "الجنس" => "Gender",
            "الحالة الاجتماعية" => "SocialStatusID",
            "الحالة الصحية" => "HealthStatusID",
            "على قيد الحياة" => "IsAlive",
            "تاريخ الوفاة" => "DeathDate",
            "الرتبة" => "RankID",
            "فرع المعاش" => "PensionBranchID",
            "فرع العمالات" => "BranchID",
            "الفئة" => "CategoryID",
            "الوحدة" => "UnitID",
            "الرقم العسكري" => "MilitaryNumber",
            "رقم الربط" => "LinkNumber",
            "الصورة الشخصية" => "PersonalPhotoPath",
            "وثيقة الهوية" => "NationalIDDocumentPath",
            "وثيقة الوكالة" => "AgencyDocumentPath",
            "وثيقة الوصاية" => "GuardianshipDocumentPath",
            "رقم الوكالة" => "AgencyNumber",
            "تاريخ الوكالة" => "AgencyDate",
            "تاريخ انتهاء الوكالة" => "AgencyEndDate",
            "رقم وثيقة الوصاية" => "GuardianshipDocNumber",
            "تاريخ فتح الملف" => "FileOpenDate",
            "عدد الوثائق الناقصة" => "MissingDocumentsCount",
            "عدد البيانات الناقصة" => "MissingDataCount",
            "عدد الوثائق" => "DocumentsCount",
            "عدد المستحقين" => "EntitledPersonsCount",
            "عدد الوكلاء" => "AgentsCount",
            "عدد الأوصياء" => "GuardiansCount",
            "عدد دفاتر الصرف" => "PaymentBooksCount",
            "عدد المستلمين" => "RecipientsCount",
            "الموظف المسؤول" => "AssignedEmployeeID",
            "رقم الدفتر" => "BookNumber",
            "نوع الدفتر" => "BookType",
            "تاريخ الإصدار" => "IssueDate",
            "تاريخ الانتهاء" => "ExpiryDate",
            "الحالة" => "Status",
            "سبب التغيير" => "ChangeReason",
            "لون الدفتر" => "LegacyColor",
            "اسم المستلم" => "RecipientName",
            "نوع المستلم" => "RecipientType",
            "تاريخ البداية" => "StartDate",
            "تاريخ النهاية" => "EndDate",
            "نشط" => "IsActive",
            "المبلغ" => "Amount",
            "النسبة" => "Percentage",
            "السبب" => "Reason",
            "رمز الإيقاف" => "StopCode",
            "صلة القرابة" => "RelationshipID",
            "مؤهل" => "IsEligible",
            "متوفي" => "IsDeceased",
            "استثناء إعاقة" => "HasDisabilityException",
            "الاسم" => "FullName",

            // English → English (pass-through for already-English keys)
            "FileNumber" or "StatusID" or "Notes" or "FullName" or "NationalID"
            or "Phone" or "Address" or "City" or "Governorate" or "BirthDate"
            or "RetirementDate" or "ServiceStartDate" or "ServiceEndDate"
            or "Gender" or "SocialStatusID" or "HealthStatusID" or "IsAlive"
            or "DeathDate" or "RankID" or "PensionBranchID" or "BranchID"
            or "CategoryID" or "UnitID" or "MilitaryNumber" or "LinkNumber"
            or "PersonalPhotoPath" or "NationalIDDocumentPath" or "AgencyDocumentPath"
            or "GuardianshipDocumentPath" or "AgencyNumber" or "AgencyDate"
            or "AgencyEndDate" or "GuardianshipDocNumber" or "FileOpenDate"
            or "MissingDocumentsCount" or "MissingDataCount" or "DocumentsCount"
            or "EntitledPersonsCount" or "AgentsCount" or "GuardiansCount"
            or "PaymentBooksCount" or "RecipientsCount" or "AssignedEmployeeID"
            or "BookNumber" or "BookType" or "IssueDate" or "ExpiryDate"
            or "Status" or "ChangeReason" or "LegacyColor" or "RecipientName"
            or "RecipientType" or "StartDate" or "EndDate" or "IsActive"
            or "Amount" or "Percentage" or "Reason" or "StopCode"
            or "RelationshipID" or "IsEligible" or "IsDeceased"
            or "HasDisabilityException"
                => name,

            _ => name
        };
    }
}
