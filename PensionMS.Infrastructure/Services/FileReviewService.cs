using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class FileReviewService : IFileReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public FileReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<RetirementFileReviewDto>>> GetPendingReviewsAsync()
    {
        var files = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.ReviewStatus == FileReviewStatus.PendingReview)
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
        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.Retiree)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileID == fileId && !f.IsDeleted);

        if (file == null)
            return ApiResponse<FileReviewDiffDto>.Fail("الملف غير موجود");

        if (file.ReviewStatus is not (FileReviewStatus.PendingReview or FileReviewStatus.Approved or FileReviewStatus.Rejected))
            return ApiResponse<FileReviewDiffDto>.Fail("الملف لا يحتاج مراجعة");

        var items = await _unitOfWork.FileReviewItems.Query()
            .Include(i => i.AuditLog)
            .ThenInclude(a => a.User)
            .AsNoTracking()
            .Where(i => i.RetirementFileID == fileId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();

        var lookups = await LoadLookupsAsync();

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

    private async Task<ApiResponse<bool>> WithReviewTransactionAsync(int fileId, Func<Task<ApiResponse<bool>>> action)
    {
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.LockFileReviewAsync(fileId);
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

    public Task<ApiResponse<bool>> RequestReviewAsync(int fileId, int userId, DateTime? requestedAt = null)
        => WithReviewTransactionAsync(fileId, () => RequestReviewCoreAsync(fileId, userId, requestedAt));

    private async Task<ApiResponse<bool>> RequestReviewCoreAsync(int fileId, int userId, DateTime? requestedAt)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);

        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        // الملفات الموقوفة أو المغلقة أو المرفوضة لا تعود إلى المراجعة تلقائياً
        if (file.StatusID == FileStatusConstants.Suspended
            || file.StatusID == FileStatusConstants.Closed
            || file.StatusID == FileStatusConstants.Rejected)
        {
            return ApiResponse<bool>.Fail("لا يمكن طلب مراجعة لملف بحالة موقوف أو مغلق أو مرفوض");
        }

        var now = requestedAt ?? DateTime.UtcNow;
        var wasPending = file.ReviewStatus == FileReviewStatus.PendingReview;

        // تحميل البنود الحالية مرة واحدة: الكل لمنع التكرار مع قرارات سابقة، والمعلقة للحذف/التحديث فقط
        var existingAll = await _unitOfWork.FileReviewItems.Query()
            .Where(i => i.RetirementFileID == fileId)
            .ToListAsync();
        var existingPending = existingAll
            .Where(i => i.Status == FileReviewItemStatusConstants.Pending)
            .ToList();

        // البدء من تاريخ آخر مراجعة مكتملة، أو تاريخ إنشاء الملف إذا لم تكن هناك مراجعة سابقة
        // نستخدم DateTime.MinValue في حال عدم وجود تاريخ مراجعة مكتملة لضمان التقاط التعديلات الأولى
        var auditStartTime = file.ReviewCompletedAt ?? DateTime.MinValue;

        // إذا كان الملف جديداً ولم يسبق مراجعته، نأخذ التعديلات التي تمت بعد إنشائه بـ 5 ثواني
        // لتجنب سحب سجلات الـ "Create" الأصلية إذا لم يكن ذلك مطلوباً
        // داخل دورة معلقة فقدنا الحد بعد تصفيره: نشتقه من أقدم تدقيق مغطى بدل السقوط لإعادة بناء كاملة
        if (auditStartTime == DateTime.MinValue)
        {
            if (wasPending)
            {
                var coveredAuditIds = existingAll
                    .Where(i => i.AuditLogID.HasValue)
                    .Select(i => i.AuditLogID!.Value)
                    .Distinct()
                    .ToList();
                DateTime? floor = null;
                if (coveredAuditIds.Count > 0)
                {
                    floor = await _unitOfWork.AuditLogs.Query()
                        .Where(a => coveredAuditIds.Contains(a.LogID))
                        .OrderBy(a => a.CreatedAt)
                        .Select(a => (DateTime?)a.CreatedAt)
                        .FirstOrDefaultAsync();
                }
                auditStartTime = floor ?? file.CreatedAt.AddSeconds(-5);
            }
            else
            {
                auditStartTime = file.CreatedAt.AddSeconds(-5);
            }
        }

        file.ReviewStatus = FileReviewStatus.PendingReview;
        file.ReviewRequestedAt = now;
        file.ReviewNotes = null;
        // Preserve the last completed boundary across every upload in the new cycle.
        file.UpdatedAt = now;

        // Save current status before moving to UnderReview
        if (file.StatusID.HasValue && file.StatusID != FileStatusConstants.UnderReview)
        {
            file.PreviousStatusID = file.StatusID;
        }
        file.StatusID = FileStatusConstants.UnderReview;

        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.SaveChangesAsync();

        var desiredItems = new List<FileReviewItem>();

        // تحديد معرفات السجلات المرتبطة بالملف
        var entitledIds = await _unitOfWork.EntitledPersons.Query().AsNoTracking()
            .Where(e => e.FileID == fileId).Select(e => e.EntitledID).ToListAsync();
        var agentIds = await _unitOfWork.Agents.Query().AsNoTracking()
            .Where(a => a.FileID == fileId).Select(a => a.AgentID).ToListAsync();
        var guardianIds = await _unitOfWork.Guardians.Query().AsNoTracking()
            .Where(g => g.FileID == fileId).Select(g => g.GuardianID).ToListAsync();
        var paymentBookIds = await _unitOfWork.PaymentBooks.Query().AsNoTracking()
            .Where(pb => pb.FileID == fileId).Select(pb => pb.PaymentBookID).ToListAsync();
        var recipientIds = await _unitOfWork.SalaryRecipients.Query().AsNoTracking()
            .Where(r => r.FileID == fileId).Select(r => r.RecipientID).ToListAsync();
        var documentIds = await _unitOfWork.Documents.Query().AsNoTracking()
            .Where(d => d.FileID == fileId).Select(d => d.DocumentID).ToListAsync();

        // تعريفات الجداول: (TableID, SectionName, SectionType, ActionLabels)
        var tableDefs = new List<(int TableId, string SectionName, string SectionType, Func<string, string> ActionLabel)>
        {
            (SystemTableConstants.RetirementFiles, "بيانات الملف الأساسية", "File",
                action => action switch { "Create" => "إنشاء ملف", "Update" => "تعديل بيانات", "Delete" => "حذف ملف", _ => action }),
            (1, "بيانات المتقاعد", "Retiree",
                action => action switch { "Create" => "إنشاء بيانات", "Update" => "تعديل بيانات", _ => action }),
            (6, "الوثائق المرفوعة", "Document",
                action => action switch { "Create" => "رفع وثيقة", "Update" => "تعديل وثيقة", "Delete" => "حذف وثيقة", _ => action }),
        };

        if (entitledIds.Count > 0)
            tableDefs.Add((3, "المستحقون", "EntitledPerson",
                action => action switch { "Create" => "إضافة مستحق", "Update" => "تعديل مستحق", "Delete" => "حذف مستحق", _ => action }));

        if (agentIds.Count > 0)
            tableDefs.Add((4, "الوكلاء", "Agent",
                action => action switch { "Create" => "إضافة وكيل", "Update" => "تعديل وكيل", "Delete" => "حذف وكيل", _ => action }));

        if (guardianIds.Count > 0)
            tableDefs.Add((5, "الأوصياء", "Guardian",
                action => action switch { "Create" => "إضافة وصي", "Update" => "تعديل وصي", "Delete" => "حذف وصي", _ => action }));

        if (paymentBookIds.Count > 0)
            tableDefs.Add((19, "دفاتر الصرف", "PaymentBook",
                action => action switch { "Create" => "إصدار دفتر", "Update" => "تعديل دفتر", "Delete" => "حذف دفتر", _ => action }));

        if (recipientIds.Count > 0)
            tableDefs.Add((12, "مستلمو المعاش", "SalaryRecipient",
                action => action switch { "Create" => "إضافة مستلم", "Update" => "تعديل مستلم", "Delete" => "حذف مستلم", _ => action }));

        // معالجة كل جدول لسحب العمليات الجديدة فقط
        var lookups = await LoadLookupsAsync();
        foreach (var (tableId, sectionName, sectionType, getActionLabel) in tableDefs)
        {
            List<int>? recordIds = null;
            if (tableId == 3) recordIds = entitledIds;
            else if (tableId == 4) recordIds = agentIds;
            else if (tableId == 5) recordIds = guardianIds;
            else if (tableId == 19) recordIds = paymentBookIds;
            else if (tableId == 12) recordIds = recipientIds;
            else if (tableId == 6) recordIds = documentIds;
            else if (tableId == 1 && file.Retiree != null) recordIds = new List<int> { file.Retiree.RetireeID };

            var query = _unitOfWork.AuditLogs.Query()
                .AsNoTracking()
                .Where(a => a.TableID == tableId && a.CreatedAt >= auditStartTime);

            if (tableId == SystemTableConstants.RetirementFiles)
                query = query.Where(a => a.RecordID == fileId);
            else if (recordIds != null && recordIds.Count > 0)
                query = query.Where(a => a.RecordID.HasValue && recordIds.Contains(a.RecordID.Value));
            else
                continue;

            var audits = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            var recordsWithCreate = audits.Where(a => a.Action == "Create" && a.RecordID.HasValue)
                .Select(a => a.RecordID!.Value).ToHashSet();
            var filteredAudits = audits.Where(a =>
                a.Action != "Update" || !a.RecordID.HasValue || !recordsWithCreate.Contains(a.RecordID.Value)).ToList();

            foreach (var audit in filteredAudits)
            {
                var fieldChanges = BuildFieldChanges(audit.OldValues, audit.NewValues, lookups);
                var actionLabel = getActionLabel(audit.Action);

                // إذا كانت عملية تعديل ولا توجد تغييرات حقيقية في الحقول المستهدفة، نتجاوزها
                if (audit.Action == "Update" && fieldChanges.Count == 0) continue;

                desiredItems.Add(new FileReviewItem
                {
                    RetirementFileID = fileId,
                    AuditLogID = audit.LogID,
                    EntityType = tableId,
                    EntityID = audit.RecordID ?? fileId,
                    FieldName = actionLabel,
                    OldValue = audit.Action switch { "Create" => "جديد", "Update" => audit.EntityDisplayName, "Delete" => audit.EntityDisplayName, _ => audit.EntityDisplayName },
                    NewValue = audit.EntityDisplayName,
                    SectionName = sectionName,
                    SectionType = sectionType,
                    EntityDisplayName = audit.EntityDisplayName,
                    FieldChangesJson = fieldChanges.Count > 0 ? JsonSerializer.Serialize(fieldChanges) : null,
                    Status = FileReviewItemStatusConstants.Pending,
                    CreatedAt = now
                });
            }
        }

        // مطابقة تفاضلية بالمفتاح الطبيعي AuditLogID: نفس الحالة النهائية المعلقة دون هدم وإعادة بناء
        // البنود المعتمدة/المرفوضة تمنع إنشاء مكرر معلق لنفس التدقيق، وقراراتها لا تمس
        // البنود القديمة دون رابط تدقيق تحفظ دائماً
        var coveredByAudit = existingAll
            .Where(i => i.AuditLogID.HasValue)
            .GroupBy(i => i.AuditLogID!.Value)
            .ToDictionary(g => g.Key, g => g.First());
        var pendingByAudit = existingPending
            .Where(i => i.AuditLogID.HasValue)
            .GroupBy(i => i.AuditLogID!.Value)
            .ToDictionary(g => g.Key, g => g.First());
        var desiredByAudit = desiredItems
            .Where(i => i.AuditLogID.HasValue)
            .GroupBy(i => i.AuditLogID!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var staleItems = existingPending
            .Where(i => i.AuditLogID.HasValue && !desiredByAudit.ContainsKey(i.AuditLogID!.Value))
            .ToList();
        var itemsToInsert = desiredByAudit.Values
            .Where(d => !coveredByAudit.ContainsKey(d.AuditLogID!.Value))
            .ToList();

        // تحديث محتوى البند المعلق عند تغير عرضه فقط؛ القرار وتاريخ الإنشاء والهوية تحفظ
        var changedForMarking = new List<FileReviewItem>();
        foreach (var desired in desiredByAudit.Values)
        {
            if (!pendingByAudit.TryGetValue(desired.AuditLogID!.Value, out var current))
                continue;
            if (ReviewDisplayEquals(current, desired))
                continue;
            current.EntityType = desired.EntityType;
            current.EntityID = desired.EntityID;
            current.FieldName = desired.FieldName;
            current.OldValue = desired.OldValue;
            current.NewValue = desired.NewValue;
            current.SectionName = desired.SectionName;
            current.SectionType = desired.SectionType;
            current.EntityDisplayName = desired.EntityDisplayName;
            current.FieldChangesJson = desired.FieldChangesJson;
            changedForMarking.Add(current);
        }

        foreach (var stale in staleItems)
            _unitOfWork.FileReviewItems.Delete(stale);
        foreach (var item in itemsToInsert)
            await _unitOfWork.FileReviewItems.AddAsync(item);
        if (staleItems.Count > 0 || itemsToInsert.Count > 0 || changedForMarking.Count > 0)
            await _unitOfWork.SaveChangesAsync();

        // استخراج معرفات الكيانات التي تعدلت فعلياً في هذه الدفعة فقط؛ الثابتة معلمة مسبقاً
        var deltaItems = itemsToInsert.Concat(changedForMarking).ToList();
        var affectedEntitledIds = deltaItems.Where(i => i.EntityType == 3 && i.EntityID.HasValue).Select(i => i.EntityID!.Value).Distinct().ToList();
        var affectedAgentIds = deltaItems.Where(i => i.EntityType == 4 && i.EntityID.HasValue).Select(i => i.EntityID!.Value).Distinct().ToList();
        var affectedGuardianIds = deltaItems.Where(i => i.EntityType == 5 && i.EntityID.HasValue).Select(i => i.EntityID!.Value).Distinct().ToList();
        var affectedPaymentBookIds = deltaItems.Where(i => i.EntityType == 19 && i.EntityID.HasValue).Select(i => i.EntityID!.Value).Distinct().ToList();
        var affectedRecipientIds = deltaItems.Where(i => i.EntityType == 12 && i.EntityID.HasValue).Select(i => i.EntityID!.Value).Distinct().ToList();
        var affectedDocumentIds = deltaItems.Where(i => i.EntityType == 6 && i.EntityID.HasValue).Select(i => i.EntityID!.Value).Distinct().ToList();
        var affectedRetireeIds = deltaItems.Where(i => i.EntityType == 1 && i.EntityID.HasValue).Select(i => i.EntityID!.Value).Distinct().ToList();

        // تعليم الكيانات المتأثرة فقط بأنها قيد المراجعة
        if (deltaItems.Count > 0)
            await MarkAffectedEntitiesAsPendingAsync(fileId, affectedEntitledIds, affectedAgentIds,
                affectedGuardianIds, affectedPaymentBookIds, affectedRecipientIds, affectedDocumentIds, affectedRetireeIds);

        return ApiResponse<bool>.Success(true, "تم طلب مراجعة الملف");
    }

    public Task<ApiResponse<bool>> ApproveReviewAsync(int fileId, int userId, string? notes = null)
        => WithReviewTransactionAsync(fileId, () => ApproveReviewCoreAsync(fileId, userId, notes));

    private async Task<ApiResponse<bool>> ApproveReviewCoreAsync(int fileId, int userId, string? notes)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        if (file.ReviewStatus != FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Fail("الملف لا يحتاج مراجعة");

        var pendingItems = await _unitOfWork.FileReviewItems.Query()
            .Where(i => i.RetirementFileID == fileId && i.Status == FileReviewItemStatusConstants.Pending)
            .ToListAsync();

        var now = DateTime.UtcNow;
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
        => WithReviewTransactionAsync(fileId, () => RejectReviewCoreAsync(fileId, userId, reason));

    private async Task<ApiResponse<bool>> RejectReviewCoreAsync(int fileId, int userId, string reason)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file != null) await _unitOfWork.RetirementFiles.ReloadAsync(file);
        if (file == null || file.IsDeleted)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        if (file.ReviewStatus != FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Fail("الملف لا يحتاج مراجعة");

        var pendingItems = await _unitOfWork.FileReviewItems.Query()
            .Where(i => i.RetirementFileID == fileId && i.Status == FileReviewItemStatusConstants.Pending)
            .ToListAsync();

        var now = DateTime.UtcNow;
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

    private async Task<ApiResponse<bool>> WithItemReviewTransactionAsync(long itemId, Func<Task<ApiResponse<bool>>> action)
    {
        var fileId = await _unitOfWork.FileReviewItems.Query().AsNoTracking()
            .Where(i => i.ItemID == itemId).Select(i => (int?)i.RetirementFileID).FirstOrDefaultAsync();
        return fileId.HasValue
            ? await WithReviewTransactionAsync(fileId.Value, action)
            : ApiResponse<bool>.Fail("بند المراجعة غير موجود");
    }

    public Task<ApiResponse<bool>> ApproveItemAsync(long itemId, int userId, string? notes = null)
        => WithItemReviewTransactionAsync(itemId, () => ApproveItemCoreAsync(itemId, userId, notes));

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

        if (item.Status != FileReviewItemStatusConstants.Pending)
            return ApiResponse<bool>.Fail("تم البت في هذا البند مسبقاً");

        item.Status = FileReviewItemStatusConstants.Approved;
        item.ReviewedByUserID = userId;
        item.ReviewedAt = DateTime.UtcNow;
        item.ReviewNotes = notes;

        _unitOfWork.FileReviewItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        // إعادة تعيين PendingReviewStatus للكيان بعد الموافقة على التغيير
        await UpdateEntityPendingStatusForItemAsync(item, (int)PendingReviewStatus.None);
        await AutoResolveFileReviewCoreAsync(item.RetirementFileID);

        return ApiResponse<bool>.Success(true, "تم اعتماد التغيير");
    }

    public Task<ApiResponse<bool>> RejectItemAsync(long itemId, int userId, string reason)
        => WithItemReviewTransactionAsync(itemId, () => RejectItemCoreAsync(itemId, userId, reason));

    private async Task<ApiResponse<bool>> RejectItemCoreAsync(long itemId, int userId, string reason)
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

        if (item.Status != FileReviewItemStatusConstants.Pending)
            return ApiResponse<bool>.Fail("تم البت في هذا البند مسبقاً");

        item.Status = FileReviewItemStatusConstants.Rejected;
        item.ReviewedByUserID = userId;
        item.ReviewedAt = DateTime.UtcNow;
        item.ReviewNotes = reason;

        _unitOfWork.FileReviewItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        // تعليم الكيان بأنه مرفوض
        await UpdateEntityPendingStatusForItemAsync(item, (int)PendingReviewStatus.Rejected);
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

        var pendingCount = await _unitOfWork.FileReviewItems.Query()
            .CountAsync(i => i.RetirementFileID == fileId && i.Status == FileReviewItemStatusConstants.Pending);

        if (pendingCount > 0)
            return;

        var rejectedCount = await _unitOfWork.FileReviewItems.Query()
            .CountAsync(i => i.RetirementFileID == fileId && i.Status == FileReviewItemStatusConstants.Rejected);

        var now = DateTime.UtcNow;
        file.ReviewStatus = rejectedCount > 0 ? FileReviewStatus.Rejected : FileReviewStatus.Approved;
        file.ReviewCompletedAt = now;
        file.UpdatedAt = now;

        // ── ربط حالة الملفات بنتيجة المراجعة ──
        if (rejectedCount == 0)
        {
            // الكل معتمد: استعادة الحالة السابقة
            if (file.PreviousStatusID.HasValue)
            {
                file.StatusID = file.PreviousStatusID.Value;
                file.PreviousStatusID = null;
            }
            else if (file.StatusID == FileStatusConstants.UnderReview)
            {
                // fallback: إذا لم تكن هناك حالة سابقة، أرجع إلى Active
                file.StatusID = FileStatusConstants.Active;
            }
        }
        else
        {
            // فيه مرفوض: تعيين حالة الرفض
            file.StatusID = FileStatusConstants.Rejected;
        }

        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.SaveChangesAsync();

        var entityStatus = rejectedCount > 0 ? (int)PendingReviewStatus.Rejected : (int)PendingReviewStatus.Approved;
        await SetEntityReviewStatusForFileAsync(fileId, entityStatus);
    }

    // ═══════════════════════════════════════
    //  PENDING REVIEW STATUS
    // ═══════════════════════════════════════

    private async Task MarkAffectedEntitiesAsPendingAsync(
        int fileId,
        List<int> entitledIds, List<int> agentIds, List<int> guardianIds,
        List<int> paymentBookIds, List<int> recipientIds,
        List<int> documentIds, List<int> retireeIds)
    {
        if (entitledIds.Count > 0)
        {
            var eps = await _unitOfWork.EntitledPersons.Query()
                .Where(e => entitledIds.Contains(e.EntitledID) && !e.IsDeleted).ToListAsync();
            foreach (var e in eps) e.PendingReviewStatus = PendingReviewStatus.Pending;
        }

        if (agentIds.Count > 0)
        {
            var agents = await _unitOfWork.Agents.Query()
                .Where(a => agentIds.Contains(a.AgentID) && !a.IsDeleted).ToListAsync();
            foreach (var a in agents) a.PendingReviewStatus = PendingReviewStatus.Pending;
        }

        if (guardianIds.Count > 0)
        {
            var guardians = await _unitOfWork.Guardians.Query()
                .Where(g => guardianIds.Contains(g.GuardianID) && !g.IsDeleted).ToListAsync();
            foreach (var g in guardians) g.PendingReviewStatus = PendingReviewStatus.Pending;
        }

        if (paymentBookIds.Count > 0)
        {
            var books = await _unitOfWork.PaymentBooks.Query()
                .Where(b => paymentBookIds.Contains(b.PaymentBookID) && !b.IsDeleted).ToListAsync();
            foreach (var b in books) b.PendingReviewStatus = PendingReviewStatusConstants.PendingReview;
        }

        if (recipientIds.Count > 0)
        {
            var recipients = await _unitOfWork.SalaryRecipients.Query()
                .Where(r => recipientIds.Contains(r.RecipientID)).ToListAsync();
            foreach (var r in recipients) r.PendingReviewStatus = PendingReviewStatus.Pending;
        }

        if (documentIds.Count > 0)
        {
            var docs = await _unitOfWork.Documents.Query()
                .Where(d => documentIds.Contains(d.DocumentID) && !d.IsDeleted).ToListAsync();
            foreach (var d in docs) d.PendingReviewStatus = PendingReviewStatus.Pending;
        }

        if (retireeIds.Count > 0)
        {
            var retirees = await _unitOfWork.Retirees.Query()
                .Where(r => retireeIds.Contains(r.RetireeID)).ToListAsync();
            foreach (var r in retirees) r.PendingReviewStatus = PendingReviewStatus.Pending;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SetEntityReviewStatusForFileAsync(int fileId, int newStatus)
    {
        var entitledPersons = await _unitOfWork.EntitledPersons.Query()
            .Where(e => e.FileID == fileId
                     && e.PendingReviewStatus == PendingReviewStatus.Pending)
            .ToListAsync();
        foreach (var e in entitledPersons) e.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var agents = await _unitOfWork.Agents.Query()
            .Where(a => a.FileID == fileId
                     && a.PendingReviewStatus == PendingReviewStatus.Pending)
            .ToListAsync();
        foreach (var a in agents) a.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var guardians = await _unitOfWork.Guardians.Query()
            .Where(g => g.FileID == fileId
                     && g.PendingReviewStatus == PendingReviewStatus.Pending)
            .ToListAsync();
        foreach (var g in guardians) g.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var books = await _unitOfWork.PaymentBooks.Query()
            .Where(b => b.FileID == fileId
                     && b.PendingReviewStatus == PendingReviewStatusConstants.PendingReview)
            .ToListAsync();
        foreach (var b in books) b.PendingReviewStatus = newStatus;

        var recipients = await _unitOfWork.SalaryRecipients.Query()
            .Where(r => r.FileID == fileId
                     && r.PendingReviewStatus == PendingReviewStatus.Pending)
            .ToListAsync();
        foreach (var r in recipients) r.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var documents = await _unitOfWork.Documents.Query()
            .Where(d => d.FileID == fileId
                     && d.PendingReviewStatus == PendingReviewStatus.Pending)
            .ToListAsync();
        foreach (var d in documents) d.PendingReviewStatus = (PendingReviewStatus)newStatus;

        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);
        if (file?.Retiree != null
            && file.Retiree.PendingReviewStatus == PendingReviewStatus.Pending)
            file.Retiree.PendingReviewStatus = (PendingReviewStatus)newStatus;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ApiResponse<bool>> AcknowledgeEntityRejectionAsync(
        string entityType, int entityId, int userId)
    {
        switch (entityType)
        {
            case "EntitledPerson":
            {
                var ep = await _unitOfWork.EntitledPersons.GetByIdAsync(entityId);
                if (ep == null || ep.PendingReviewStatus != PendingReviewStatus.Rejected)
                    return ApiResponse<bool>.Fail("الكيان غير موجود أو غير مرفوض");
                ep.PendingReviewStatus = PendingReviewStatus.None;
                var epDocs = await _unitOfWork.Documents.Query()
                    .Where(d => d.EntitledID == entityId
                             && d.PendingReviewStatus == PendingReviewStatus.Rejected)
                    .ToListAsync();
                foreach (var d in epDocs) d.PendingReviewStatus = PendingReviewStatus.None;
                break;
            }

            case "Agent":
            {
                var agent = await _unitOfWork.Agents.GetByIdAsync(entityId);
                if (agent == null || agent.PendingReviewStatus != PendingReviewStatus.Rejected)
                    return ApiResponse<bool>.Fail("الكيان غير موجود أو غير مرفوض");
                agent.PendingReviewStatus = PendingReviewStatus.None;
                var agentDocs = await _unitOfWork.Documents.Query()
                    .Where(d => d.AgentID == entityId
                             && d.PendingReviewStatus == PendingReviewStatus.Rejected)
                    .ToListAsync();
                foreach (var d in agentDocs) d.PendingReviewStatus = PendingReviewStatus.None;
                break;
            }

            case "Guardian":
            {
                var guardian = await _unitOfWork.Guardians.GetByIdAsync(entityId);
                if (guardian == null || guardian.PendingReviewStatus != PendingReviewStatus.Rejected)
                    return ApiResponse<bool>.Fail("الكيان غير موجود أو غير مرفوض");
                guardian.PendingReviewStatus = PendingReviewStatus.None;
                var guardianDocs = await _unitOfWork.Documents.Query()
                    .Where(d => d.GuardianID == entityId
                             && d.PendingReviewStatus == PendingReviewStatus.Rejected)
                    .ToListAsync();
                foreach (var d in guardianDocs) d.PendingReviewStatus = PendingReviewStatus.None;
                break;
            }

            case "PaymentBook":
            {
                var book = await _unitOfWork.PaymentBooks.GetByIdAsync(entityId);
                if (book == null || book.PendingReviewStatus != PendingReviewStatusConstants.Rejected)
                    return ApiResponse<bool>.Fail("الكيان غير موجود أو غير مرفوض");
                book.PendingReviewStatus = PendingReviewStatusConstants.None;
                break;
            }

            case "SalaryRecipient":
            {
                var recipient = await _unitOfWork.SalaryRecipients.GetByIdAsync(entityId);
                if (recipient == null || recipient.PendingReviewStatus != PendingReviewStatus.Rejected)
                    return ApiResponse<bool>.Fail("الكيان غير موجود أو غير مرفوض");
                recipient.PendingReviewStatus = PendingReviewStatus.None;
                break;
            }

            case "Retiree":
            {
                var retiree = await _unitOfWork.Retirees.GetByIdAsync(entityId);
                if (retiree == null || retiree.PendingReviewStatus != PendingReviewStatus.Rejected)
                    return ApiResponse<bool>.Fail("الكيان غير موجود أو غير مرفوض");
                retiree.PendingReviewStatus = PendingReviewStatus.None;
                break;
            }

            default:
                return ApiResponse<bool>.Fail("نوع الكيان غير معروف");
        }

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم قبول الرفض وإعادة الكيان لحالته الطبيعية");
    }

    private async Task UpdateEntityPendingStatusForItemAsync(FileReviewItem item, int newStatus)
    {
        if (!item.EntityID.HasValue) return;

        switch (item.EntityType)
        {
            case 3: // EntitledPerson
            {
                var ep = await _unitOfWork.EntitledPersons.GetByIdAsync(item.EntityID.Value);
                if (ep != null) ep.PendingReviewStatus = (PendingReviewStatus)newStatus;
                break;
            }
            case 4: // Agent
            {
                var agent = await _unitOfWork.Agents.GetByIdAsync(item.EntityID.Value);
                if (agent != null) agent.PendingReviewStatus = (PendingReviewStatus)newStatus;
                break;
            }
            case 5: // Guardian
            {
                var guardian = await _unitOfWork.Guardians.GetByIdAsync(item.EntityID.Value);
                if (guardian != null) guardian.PendingReviewStatus = (PendingReviewStatus)newStatus;
                break;
            }
            case 19: // PaymentBook
            {
                var book = await _unitOfWork.PaymentBooks.GetByIdAsync(item.EntityID.Value);
                if (book != null) book.PendingReviewStatus = newStatus;
                break;
            }
            case 12: // SalaryRecipient
            {
                var recipient = await _unitOfWork.SalaryRecipients.GetByIdAsync(item.EntityID.Value);
                if (recipient != null) recipient.PendingReviewStatus = (PendingReviewStatus)newStatus;
                break;
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static bool ReviewDisplayEquals(FileReviewItem current, FileReviewItem desired)
    {
        return current.EntityType == desired.EntityType
            && current.EntityID == desired.EntityID
            && string.Equals(current.FieldName, desired.FieldName, StringComparison.Ordinal)
            && string.Equals(current.OldValue, desired.OldValue, StringComparison.Ordinal)
            && string.Equals(current.NewValue, desired.NewValue, StringComparison.Ordinal)
            && string.Equals(current.SectionName, desired.SectionName, StringComparison.Ordinal)
            && string.Equals(current.SectionType, desired.SectionType, StringComparison.Ordinal)
            && string.Equals(current.EntityDisplayName, desired.EntityDisplayName, StringComparison.Ordinal)
            && string.Equals(current.FieldChangesJson, desired.FieldChangesJson, StringComparison.Ordinal);
    }

    // ═══════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════

    private static List<FieldChangeDetailDto> BuildFieldChanges(string? oldValuesJson, string? newValuesJson, Dictionary<string, Dictionary<int, string>>? lookups = null)
    {
        var changes = new List<FieldChangeDetailDto>();

        if (string.IsNullOrWhiteSpace(oldValuesJson) && string.IsNullOrWhiteSpace(newValuesJson))
            return changes;

        var oldDict = string.IsNullOrWhiteSpace(oldValuesJson)
            ? new Dictionary<string, object?>()
            : JsonSerializer.Deserialize<Dictionary<string, object?>>(oldValuesJson) ?? new();

        var newDict = string.IsNullOrWhiteSpace(newValuesJson)
            ? new Dictionary<string, object?>()
            : JsonSerializer.Deserialize<Dictionary<string, object?>>(newValuesJson) ?? new();

        var excludedFields = new HashSet<string>
        {
            "RowVersion", "IsDeleted", "DeletedAt", "DeletedByUserID",
            "UpdatedAt", "CreatedAt", "CreatedByUserID", "UpdatedByUserID",
            "FileID", "RetireeID", "EntitledID", "AgentID", "GuardianID",
            "PaymentBookID", "RecipientID", "MissingDocumentID", "DocumentID",
            "SettlementID", "TransitionID", "PledgeID", "SuspensionID",
            "NoteID", "ExemptionID", "ReviewStatus", "ReviewRequestedAt",
            "ReviewCompletedAt", "ReviewNotes", "LastReviewedFileState",
            "PendingReviewStatus"
        };

        var allKeys = oldDict.Keys.Union(newDict.Keys)
            .Where(k => !excludedFields.Contains(k));

        var defaultLookups = lookups ?? new Dictionary<string, Dictionary<int, string>>();

        foreach (var key in allKeys)
        {
            oldDict.TryGetValue(key, out var oldVal);
            newDict.TryGetValue(key, out var newVal);

            var oldStr = ResolveValueFromLookups(key, oldVal?.ToString() ?? "", defaultLookups);
            var newStr = ResolveValueFromLookups(key, newVal?.ToString() ?? "", defaultLookups);

            if (oldStr != newStr)
            {
                changes.Add(new FieldChangeDetailDto
                {
                    FieldName = GetArabicFieldName(key),
                    OldValue = oldStr,
                    NewValue = newStr,
                    IsMedia = IsMediaField(key)
                });
            }
        }

        return changes;
    }

    private static bool IsMediaField(string fieldName)
    {
        var lower = fieldName.ToLower();
        if (lower.EndsWith("id")) return false;

        return lower.EndsWith("path") || lower.Contains("photo") || lower.Contains("image") 
            || lower.Contains("attachment") || lower.Contains("signature");
    }

    private static string GetArabicFieldName(string fieldName)
    {
        return fieldName switch
        {
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

    private async Task<Dictionary<string, Dictionary<int, string>>> LoadLookupsAsync()
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

        return lookups;
    }

    private static string ResolveValueFromLookups(string fieldName, string? value, Dictionary<string, Dictionary<int, string>> lookups)
    {
        if (string.IsNullOrWhiteSpace(value)) return value ?? "";

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
