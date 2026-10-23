using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

/// <summary>
/// خدمة إدارة حالات ملفات التقاعد والتحولات المسموحة وتوثيقها.
/// تم فصل هذه الخدمة عن RetirementFileService لتحقيق مبدأ المسؤولية الأحادية (SRP).
/// </summary>
public class FileStatusService : IFileStatusService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IFileReviewService _reviewService;
    private readonly IImmutableAuditService _auditService;
    private readonly INotificationService _notificationService;

    public FileStatusService(
        IUnitOfWork unitOfWork,
        IImmutableAuditService auditService,
        INotificationService notificationService, IFileReviewService reviewService, IAccessiblePensionRepository access)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _reviewService = reviewService;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, int newStatusId, string? reason, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (newStatusId == FileStatusConstants.Pending)
            return await _reviewService.SubmitReviewAsync(id, userId);
        if (newStatusId != FileStatusConstants.EntitlementEnded)
            return ApiResponse<bool>.Fail("التفعيل يتم تلقائياً باعتماد المراجعة؛ الإيقاف يخص المعاش فقط");
        return await Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            await _unitOfWork.LockFileReviewAsync(id);
            var file = await queries.Query(_unitOfWork.RetirementFiles).FirstOrDefaultAsync(e => e.FileID == id);
            if (file == null || file.IsDeleted) return ApiResponse<bool>.Fail("الملف غير موجود");
            await _unitOfWork.RetirementFiles.ReloadAsync(file);
            if (file.StatusID != FileStatusConstants.Active || file.ReviewStatus != FileReviewStatus.Approved)
                return ApiResponse<bool>.Fail("يجب اعتماد المراجعة قبل إنهاء استحقاق الملف");
            var now = DateTime.UtcNow;
            if (await queries.Query(_unitOfWork.FinancialEntitlements).Where(e => e.RetireeID == file.RetireeID)
                .AnyAsync(PensionEntitlementRules.NotEndedOn(Core.Time.YemenTime.Today)))
                return ApiResponse<bool>.Fail("لا يمكن إنهاء الملف ما دامت هناك استحقاقات مالية لم تنتهِ");
            file.StatusID = FileStatusConstants.EntitlementEnded;
            file.ClosedDate = now;
            file.StatusReason = reason;
            file.UpdatedAt = now;
            file.UpdatedByUserID = userId;
            await _unitOfWork.StatusTransitionLogs.AddAsync(new StatusTransitionLog
            {
                TableID = SystemTableConstants.RetirementFiles, RecordID = id,
                OldStatusID = FileStatusConstants.Active, NewStatusID = FileStatusConstants.EntitlementEnded,
                ChangedByUserID = userId, Reason = reason, TransitionDate = now
            });
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync(userId, "", "EntitlementEnded", "RetirementFile", id, "2", "4");
            return ApiResponse<bool>.Success(true, "انتهت جميع استحقاقات الملف مع الاحتفاظ ببياناته وسجله");
        });
    }

    public async Task<ApiResponse<List<FileStatusDto>>> GetAllowedStatusTransitionsAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var file = await queries.Query(_unitOfWork.RetirementFiles).FirstOrDefaultAsync(e => e.FileID == fileId);
        if (file == null)
            return ApiResponse<List<FileStatusDto>>.Fail("الملف غير موجود");

        var currentStatusId = file.StatusID ?? 0;
        var allowedTransitions = GetAllowedTransitions(currentStatusId);
        if (file.ReviewStatus == FileReviewStatus.PendingReview)
            allowedTransitions.Clear();

        if (allowedTransitions.Count == 0)
            return ApiResponse<List<FileStatusDto>>.Success(new List<FileStatusDto>());

        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking()
            .ToListAsync();

        var statuses = allStatuses
            .Where(s => allowedTransitions.Contains(s.StatusID))
            .Select(s => new FileStatusDto
            {
                StatusID = s.StatusID,
                StatusName = FileStatusConstants.Name(s.StatusID),
                Color = s.StatusColor
            })
            .ToList();

        return ApiResponse<List<FileStatusDto>>.Success(statuses);
    }

    public async Task<ApiResponse<List<FileStatusDto>>> GetFileStatusesAsync()
    {
        var allowedIds = new[] { FileStatusConstants.New, FileStatusConstants.Active,
            FileStatusConstants.Pending, FileStatusConstants.EntitlementEnded };

        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking()
            .ToListAsync();

        var statuses = allStatuses
            .Where(s => allowedIds.Contains(s.StatusID))
            .Select(s => new FileStatusDto
            {
                StatusID = s.StatusID,
                StatusName = FileStatusConstants.Name(s.StatusID),
                Color = s.StatusColor
            })
            .ToList();

        return ApiResponse<List<FileStatusDto>>.Success(statuses);
    }

    // ═══════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════

    /// <summary>
    /// مصفوفة التحولات المسموحة لحالات الملف
    /// </summary>
    internal static HashSet<int> GetAllowedTransitions(int currentStatus) => currentStatus switch
    {
        FileStatusConstants.New => new() { FileStatusConstants.Pending },
        FileStatusConstants.Active => new() { FileStatusConstants.EntitlementEnded },
        _ => new()
    };

}
