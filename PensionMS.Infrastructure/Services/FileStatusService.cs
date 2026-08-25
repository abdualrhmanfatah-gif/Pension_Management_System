using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// خدمة إدارة حالات ملفات التقاعد — مسؤولة عن تغيير الحالة + فحص SoD + التحولات المسموحة.
/// تم فصل هذه الخدمة عن RetirementFileService لتحقيق مبدأ المسؤولية الأحادية (SRP).
/// </summary>
public class FileStatusService : IFileStatusService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISoDGuard _sodGuard;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IRiskScoringService _riskScoringService;
    private readonly IImmutableAuditService _auditService;
    private readonly ISnapshotService _snapshotService;
    private readonly INotificationService _notificationService;

    public FileStatusService(
        IUnitOfWork unitOfWork,
        ISoDGuard sodGuard,
        IWorkflowEngine workflowEngine,
        IRiskScoringService riskScoringService,
        IImmutableAuditService auditService,
        ISnapshotService snapshotService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _sodGuard = sodGuard;
        _workflowEngine = workflowEngine;
        _riskScoringService = riskScoringService;
        _auditService = auditService;
        _snapshotService = snapshotService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, int newStatusId, string? reason, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.MissingDocuments)
            .Include(f => f.Documents.Where(d => !d.IsDeleted))
            .Include(f => f.DocumentPledges.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(f => f.FileID == id);
        if (file == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        // ── Gate: منع تغيير الحالة أثناء المراجعة المعلقة (إلا إلى UnderReview) ──
        if (file.ReviewStatus == FileReviewStatus.PendingReview && newStatusId != FileStatusConstants.UnderReview)
            return ApiResponse<bool>.Fail("لا يمكن تغيير حالة الملفات أثناء وجود مراجعة معلقة. يجب إكمال المراجعة أولاً.");

        // ── 2. SoD Guard — فحص تعارض المصالح ──
        var sodAction = newStatusId switch
        {
            FileStatusConstants.UnderReview => SoDAction.Approve,
            FileStatusConstants.Active => SoDAction.Activate,
            _ => (SoDAction?)null
        };

        if (sodAction.HasValue)
        {
            var sodResult = await _sodGuard.ValidateAsync(id, userId, sodAction.Value);
            if (!sodResult.IsAllowed)
                return ApiResponse<bool>.Fail(sodResult.Reason ?? "تعارض فصل المهام");
        }

        // ── 3. Pre-condition Guards ──
        switch (newStatusId)
        {
            case FileStatusConstants.Active: // نشط — شروط التفعيل المعززة
                var hasEntitlement = await _unitOfWork.FinancialEntitlements.Query()
                    .AnyAsync(fe => fe.RetireeID == file.RetireeID && !fe.IsDeleted);
                if (!hasEntitlement)
                    return ApiResponse<bool>.Fail("لا يمكن تفعيل الملف بدون استحقاق مالي");

                var hasActivePaymentBook = await _unitOfWork.PaymentBooks.Query()
                    .AnyAsync(pb => pb.FileID == id && pb.Status == LookupConstants.ActiveAr && !pb.IsDeleted && pb.PendingReviewStatus != PendingReviewStatusConstants.PendingReview && pb.PendingReviewStatus != PendingReviewStatusConstants.Rejected);
                if (!hasActivePaymentBook)
                    return ApiResponse<bool>.Fail("لا يمكن تفعيل الملف بدون دفتر صرف فعّال");

                var hasActiveSalaryRecipient = await _unitOfWork.SalaryRecipients.Query()
                    .AnyAsync(sr => sr.FileID == id && sr.IsActive && sr.PendingReviewStatus != PendingReviewStatus.Pending && sr.PendingReviewStatus != PendingReviewStatus.Rejected);
                if (!hasActiveSalaryRecipient)
                    return ApiResponse<bool>.Fail("لا يمكن تفعيل الملف بدون مستلم المعاش نشط");

                // فحص الوثائق الناقصة (يدوياً + تلقائياً)
                var pendingMissingDocs = file.MissingDocuments
                    .Where(m => MissingDocumentStatusConstants.OutstandingStatuses.Contains(m.Status))
                    .ToList();

                // Auto-detect missing required documents
                var requiredDocTypes = await GetContextualRequiredDocTypeIdsAsync(id);
                var uploadedDocTypeIds = file.Documents?
                    .Where(d => !d.IsDeleted)
                    .Select(d => d.DocumentTypeID)
                    .Distinct()
                    .ToList() ?? new List<int>();
                var manuallyMissingTypeIds = file.MissingDocuments
                    .Select(m => m.DocumentTypeID).Distinct().ToList();
                var autoMissingTypeIds = requiredDocTypes
                    .Where(dt => !uploadedDocTypeIds.Contains(dt) && !manuallyMissingTypeIds.Contains(dt))
                    .ToList();

                bool hasMissing = pendingMissingDocs.Any() || autoMissingTypeIds.Any();

                if (hasMissing)
                {
                    // السماح بالتفعيل إذا كانت جميع النواقص مغطاة بتعهدات فعّالة غير منتهية
                    var activePledges = file.DocumentPledges
                        .Where(p => p.Status == GeneralStatusConstants.Active && p.Deadline > DateTime.UtcNow)
                        .ToList();

                    // فحص أن النواقص اليدوية مرتبطة كلها بتعهد فعّال
                    var pendingMissingDocIds = pendingMissingDocs.Select(m => m.MissingDocumentID).ToHashSet();
                    var coveredByPledge = file.MissingDocuments
                        .Where(m => m.PledgeID != null &&
                                    activePledges.Any(p => p.PledgeID == m.PledgeID) &&
                                    pendingMissingDocIds.Contains(m.MissingDocumentID))
                        .Select(m => m.MissingDocumentID)
                        .ToHashSet();

                    bool allManualCovered = pendingMissingDocs.All(m => coveredByPledge.Contains(m.MissingDocumentID));
                    // النواقص التلقائية: تقبل بوجود أي تعهد فعّال على الملف
                    bool autoMissingCoveredByPledge = !autoMissingTypeIds.Any() || activePledges.Any();

                    if (!allManualCovered || !autoMissingCoveredByPledge)
                        return ApiResponse<bool>.Fail(
                            "لا يمكن تفعيل الملف مع وجود وثائق ناقصة. يجب تسوية جميع النواقص بتعهدات فعّالة أو إعفاء.");

                    // ملاحظة: الملف سيُوقف تلقائياً عند انتهاء مدة التعهد
                }
                break;

            case FileStatusConstants.Closed: // مغلق — auto-set ClosedDate
                file.ClosedDate = DateTime.UtcNow;
                break;
        }

        // ── 4. Log Transition ──
        var log = new StatusTransitionLog
        {
            TableID = SystemTableConstants.RetirementFiles, // RetirementFiles (from SystemTables seed)
            RecordID = file.FileID,
            OldStatusID = file.StatusID,
            NewStatusID = newStatusId,
            Reason = reason,
            ChangedByUserID = userId,
            TransitionDate = DateTime.UtcNow
        };

        // ── 5. Apply ──
        file.StatusID = newStatusId;
        file.StatusReason = reason;
        file.UpdatedAt = DateTime.UtcNow;
        file.UpdatedByUserID = userId;

        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.StatusTransitionLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        // ── 6. Workflow & Fraud Detection ──
        await _workflowEngine.RouteFileAsync(file.FileID, $"ChangeStatus_{newStatusId}", userId);
        await _riskScoringService.EvaluateRiskAsync(file.FileID, $"ChangeStatus_{newStatusId}", userId, null, newStatusId.ToString());

        // ── 7. Immutable Audit + Snapshot ──
        await _auditService.LogAsync(
            userId, "", "ChangeStatus", "RetirementFile", file.FileID,
            log.OldStatusID?.ToString(), newStatusId.ToString());
        await _snapshotService.CaptureAsync(file.FileID, "StatusChange", userId, reason);

        var fileCreatorId = await _unitOfWork.AuditLogs.Query()
            .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == id && a.Action == "Create")
            .Select(a => a.UserID)
            .FirstOrDefaultAsync();

        if (fileCreatorId.HasValue && fileCreatorId.Value > 0)
        {
            var isRejected = newStatusId == FileStatusConstants.Rejected;
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserID = fileCreatorId.Value,
                Title = isRejected ? "رفض الملف" : "تحديث حالة الملفات",
                Message = isRejected
                    ? $"تم رفض ملفك رقم ({file.FileNumber}). السبب: {reason ?? "غير محدد"}."
                    : $"تم تغيير حالة ملفك رقم ({file.FileNumber}) إلى الحالة الجديدة.",
                Type = isRejected ? NotificationConstants.Types.Error : NotificationConstants.Types.Info,
                Category = NotificationConstants.Categories.Workflow,
                Priority = isRejected ? NotificationConstants.Priorities.High : NotificationConstants.Priorities.Normal,
                ActorID = userId,
                RelatedTableID = SystemTableConstants.RetirementFiles,
                RelatedRecordID = file.FileID
            });
        }

        return ApiResponse<bool>.Success(true, "تم تغيير حالة الملفات");
    }

    public async Task<ApiResponse<List<FileStatusDto>>> GetAllowedStatusTransitionsAsync(int fileId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file == null)
            return ApiResponse<List<FileStatusDto>>.Fail("الملف غير موجود");

        var currentStatusId = file.StatusID ?? 0;

        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking()
            .ToListAsync();

        var statuses = allStatuses
            .Where(s => s.StatusID != currentStatusId)
            .Select(s => new FileStatusDto
            {
                StatusID = s.StatusID,
                StatusName = s.StatusID == FileStatusConstants.UnderReview
                    ? "قيد المراجعة"
                    : s.StatusName,
                Color = s.StatusColor
            })
            .ToList();

        return ApiResponse<List<FileStatusDto>>.Success(statuses);
    }

    public async Task<ApiResponse<List<FileStatusDto>>> GetFileStatusesAsync()
    {
        var allowedIds = new[]
        {
            FileStatusConstants.UnderReview,
            FileStatusConstants.Active,
            FileStatusConstants.Suspended,
            FileStatusConstants.Closed
        };

        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking()
            .ToListAsync();

        var statuses = allStatuses
            .Where(s => allowedIds.Contains(s.StatusID))
            .Select(s => new FileStatusDto
            {
                StatusID = s.StatusID,
                StatusName = s.StatusID == FileStatusConstants.UnderReview
                    ? "قيد المراجعة"
                    : s.StatusName,
                Color = s.StatusColor
            })
            .ToList();

        return ApiResponse<List<FileStatusDto>>.Success(statuses);
    }

    // ═══════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════

    /// <summary>
    /// يُشغَّل من الـ Background Job — يفحص الملفات النشطة التي انتهت مدة تعهداتها
    /// ويوقفها تلقائياً لحين تقديم الوثائق المطلوبة
    /// </summary>
    public async Task ProcessExpiredPledgesAsync()
    {
        var now = DateTime.UtcNow;

        // جلب جميع التعهدات المنتهية المدة على ملفات نشطة
        var expiredPledges = await _unitOfWork.DocumentPledges.Query()
            .Include(p => p.RetirementFile)
            .Where(p => !p.IsDeleted
                     && p.Status == GeneralStatusConstants.Active
                     && p.Deadline < now
                     && p.RetirementFile.StatusID == FileStatusConstants.Active
                     && !p.RetirementFile.IsDeleted)
            .ToListAsync();

        if (!expiredPledges.Any()) return;

        // ملفات فريدة للمعالجة (لتجنب التكرار إذا كان للملف أكثر من تعهد)
        var fileIds = expiredPledges.Select(p => p.FileID).Distinct().ToList();

        foreach (var fileId in fileIds)
        {
            var file = expiredPledges.First(p => p.FileID == fileId).RetirementFile;

            // تحديث حالة التعهدات المنتهية
            var pledgesForFile = expiredPledges.Where(p => p.FileID == fileId).ToList();
            foreach (var pledge in pledgesForFile)
            {
                pledge.Status = PledgeStatusConstants.Expired;
                pledge.UpdatedAt = now;
                _unitOfWork.DocumentPledges.Update(pledge);
            }

            // إنشاء إيقاف تلقائي
            var suspension = new PensionSuspension
            {
                FileID = fileId,
                SuspensionDate = now,
                SuspensionReason = "انتهاء مدة تعهد الوثائق المطلوبة",
                SuspensionSource = "تلقائي",
                Status = SuspensionStatusConstants.Active,
                CreatedByUserID = LookupConstants.SystemUserId, // System user
                CreatedAt = now
            };
            await _unitOfWork.PensionSuspensions.AddAsync(suspension);

            // تحديث حالة الملفات إلى موقوف
            var oldStatus = file.StatusID;
            file.StatusID = FileStatusConstants.Suspended;
            file.StatusReason = "انتهاء مدة تعهد تقديم الوثائق — إيقاف تلقائي";
            file.UpdatedAt = now;
            _unitOfWork.RetirementFiles.Update(file);

            // تسجيل التحول
            await _unitOfWork.StatusTransitionLogs.AddAsync(new StatusTransitionLog
            {
                TableID = SystemTableConstants.RetirementFiles,
                RecordID = fileId,
                OldStatusID = oldStatus,
                NewStatusID = FileStatusConstants.Suspended,
                Reason = "انتهاء مدة التعهد — إيقاف تلقائي من النظام",
                ChangedByUserID = LookupConstants.SystemUserId, // System user
                TransitionDate = now
            });
        }

        await _unitOfWork.SaveChangesAsync();

        foreach (var fileId in fileIds)
        {
            var expiredFile = expiredPledges.First(p => p.FileID == fileId).RetirementFile;
            var notifyUserId = expiredFile.AssignedEmployeeID;

            if (notifyUserId == null || notifyUserId == 0)
            {
                var creatorId = await _unitOfWork.AuditLogs.Query()
                    .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == fileId && a.Action == "Create")
                    .Select(a => a.UserID)
                    .FirstOrDefaultAsync();
                notifyUserId = creatorId;
            }

            if (notifyUserId.HasValue && notifyUserId.Value > 0)
            {
                await _notificationService.SendNotificationAsync(new CreateNotificationDto
                {
                    UserID = notifyUserId.Value,
                    Title = "إيقاف الملف - انتهاء تعهد",
                    Message = $"تم إيقاف الملف رقم ({expiredFile.FileNumber}) تلقائياً لانتهاء مدة تعهد تقديم الوثائق.",
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.High,
                    ActorID = LookupConstants.SystemUserId,
                    RelatedTableID = SystemTableConstants.RetirementFiles,
                    RelatedRecordID = fileId
                });
            }
        }
    }

    /// <summary>
    /// مصفوفة التحولات المسموحة لحالات الملف
    /// </summary>
    internal static HashSet<int> GetAllowedTransitions(int currentStatus) => currentStatus switch
    {
        FileStatusConstants.New         => new() { FileStatusConstants.UnderReview, FileStatusConstants.Pending, FileStatusConstants.Active, FileStatusConstants.Incomplete, FileStatusConstants.Rejected },
        FileStatusConstants.UnderReview => new() { FileStatusConstants.Active, FileStatusConstants.Pending, FileStatusConstants.Incomplete, FileStatusConstants.Rejected, FileStatusConstants.Suspended },
        FileStatusConstants.Pending     => new() { FileStatusConstants.Active, FileStatusConstants.UnderReview, FileStatusConstants.Incomplete, FileStatusConstants.Rejected },
        FileStatusConstants.Incomplete  => new() { FileStatusConstants.UnderReview, FileStatusConstants.Active, FileStatusConstants.Pending, FileStatusConstants.Rejected },
        FileStatusConstants.Active      => new() { FileStatusConstants.Suspended, FileStatusConstants.Closed, FileStatusConstants.Incomplete },
        FileStatusConstants.Suspended   => new() { FileStatusConstants.Active, FileStatusConstants.Closed, FileStatusConstants.Incomplete },
        FileStatusConstants.Rejected    => new() { FileStatusConstants.UnderReview, FileStatusConstants.Closed }, // Allow re-review or final closure
        FileStatusConstants.Closed      => new(),  // Terminal — no further transitions
        _ => new()
    };

    /// <summary>
    /// جلب أنواع الوثائق المطلوبة (عامة + سياقية) للملف
    /// </summary>
    private async Task<List<int>> GetContextualRequiredDocTypeIdsAsync(int fileId)
    {
        var required = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.IsRequired && dt.IsActive 
                         && dt.DocumentTypeID != DocumentTypeConstants.AgencyDocument 
                         && dt.DocumentTypeID != DocumentTypeConstants.PledgeDocument
                         && (dt.OwnerType == null || (int)dt.OwnerType <= (int)DocumentOwnerType.PensionSuspension))
            .Select(dt => dt.DocumentTypeID)
            .ToListAsync();

        var hasActiveAgent = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .AnyAsync(a => a.FileID == fileId && a.IsActive && !a.IsDeleted);
        if (hasActiveAgent)
            required.Add(DocumentTypeConstants.AgencyDocument);

        var hasActivePledge = await _unitOfWork.DocumentPledges.Query()
            .AsNoTracking()
            .AnyAsync(p => p.FileID == fileId && !p.IsDeleted && p.Status != MissingDocumentStatusConstants.Fulfilled);
        if (hasActivePledge)
            required.Add(DocumentTypeConstants.PledgeDocument);

        return required;
    }
}
