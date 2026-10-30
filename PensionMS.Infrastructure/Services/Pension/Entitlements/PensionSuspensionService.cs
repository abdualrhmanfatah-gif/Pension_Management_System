using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Rules.Payroll;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.DocumentPledges;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Suspensions;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Pension.Entitlements;

/// <summary>
/// منسق إيقاف المعاش: إنشاء الإيقاف وتفويض تعهد النواقص إلى مالك التعهدات،
/// وإنهاء الإيقاف (إعادة التفعيل)، وقوائم الإيقافات وفحص منع الصرف.
/// </summary>
public class PensionSuspensionService : IPensionSuspensionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly INotificationService _notificationService;
    private readonly IDocumentPledgeService _documentPledgeService;

    public PensionSuspensionService(IUnitOfWork unitOfWork, INotificationService notificationService, IAccessiblePensionRepository access,
        IDocumentPledgeService documentPledgeService)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _documentPledgeService = documentPledgeService;
    }

    // ════════════════════════════════════════
    // SUSPENSION + PLEDGE (Item 9)
    // ════════════════════════════════════════

    public async Task<ApiResponse<string>> GetSalaryStatusAsync(int fileId)
    {
        var result = await CheckPaymentHoldAsync(fileId);
        return result.IsSuccess
            ? ApiResponse<string>.Success(result.Data!.SalaryStatus)
            : ApiResponse<string>.Fail(result.Message!);
    }

    public async Task<ApiResponse<PaymentHoldCheckDto>> CheckPaymentHoldAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var holds = queries.Query(_unitOfWork.PensionSuspensions);
        var state = await queries.Files().AsNoTracking().Where(f => f.FileID == fileId && !f.IsDeleted)
            .Select(f => new
            {
                f.StatusID, f.ReviewStatus,
                HasActiveSuspension = holds.Any(s => s.FileID == f.FileID && !s.IsDeleted &&
                    (s.Status == SuspensionStatusConstants.Active || s.Status == SuspensionStatusConstants.CodeActive))
            }).FirstOrDefaultAsync();
        if (state == null) return ApiResponse<PaymentHoldCheckDto>.Fail("الملف غير موجود ضمن نطاق الصلاحية");
        return ApiResponse<PaymentHoldCheckDto>.Success(PensionSalaryStatusRules.Evaluate(
            state.StatusID, state.ReviewStatus, state.HasActiveSuspension));
    }

    public async Task<ApiResponse<SuspensionDto>> CreateWithPledgeAsync(CreateSuspensionDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var file = await queries.Query(_unitOfWork.RetirementFiles).FirstOrDefaultAsync(e => e.FileID == dto.FileID);
        if (file == null)
            return ApiResponse<SuspensionDto>.Fail("الملف غير موجود");

        if (dto.SuspensionSource == RetirementReviewConstants.SuspensionSource)
            return ApiResponse<SuspensionDto>.Fail("إيقاف المراجعة يديره مسار المراجعة تلقائياً");

        // 1. إنشاء التعهد عبر مالك التعهدات قبل أي كتابة؛ رفض المالك يلغي الطلب كاملًا
        PledgeSummaryDto? pledgeSummary = null;
        if (dto.MissingDocumentIDs?.Any() == true)
        {
            var missingIds = dto.MissingDocumentIDs!;
            var pledgeResult = await _documentPledgeService.CreateAsync(new CreateDocumentPledgeDto
            {
                FileID = dto.FileID,
                MissingDocumentIDs = missingIds,
                Deadline = DateTime.UtcNow.AddDays(dto.DeadlineDays ?? 30),
                PledgedByType = dto.PledgedByType ?? "متقاعد",
                PledgedByName = dto.PledgedByName
            }, userId);

            if (!pledgeResult.IsSuccess || pledgeResult.Data == null)
                return ApiResponse<SuspensionDto>.Fail(pledgeResult.Message ?? "فشل إنشاء التعهد");

            pledgeSummary = new PledgeSummaryDto
            {
                PledgeID = pledgeResult.Data.PledgeID,
                PledgeDate = pledgeResult.Data.PledgeDate,
                Deadline = pledgeResult.Data.Deadline,
                Status = pledgeResult.Data.Status,
                FulfilledAt = pledgeResult.Data.FulfilledAt,
                PledgedByType = pledgeResult.Data.PledgedByType ?? string.Empty,
                PledgedByName = pledgeResult.Data.PledgedByName,
                MissingDocumentCount = missingIds.Count
            };
        }

        // 2. Create Suspension
        var suspension = new PensionSuspension
        {
            FileID = dto.FileID,
            SuspensionDate = DateTime.UtcNow,
            SuspensionReason = dto.SuspensionReason,
            SuspensionSource = dto.SuspensionSource,
            Status = SuspensionStatusConstants.Active,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.PensionSuspensions.AddAsync(suspension);
        await _unitOfWork.SaveChangesAsync();

        // 3. ربط التعهد بالإيقاف — دور المنسق فقط
        if (pledgeSummary != null)
        {
            var pledge = await _unitOfWork.DocumentPledges.GetByIdAsync(pledgeSummary.PledgeID);
            if (pledge == null)
                return ApiResponse<SuspensionDto>.Fail("تعذر ربط التعهد بالإيقاف");

            pledge.SuspensionID = suspension.SuspensionID;
            _unitOfWork.DocumentPledges.Update(pledge);
            await _unitOfWork.SaveChangesAsync();
        }

        // 4. الإشعار
        await SendFileNotificationAsync(
            dto.FileID,
            userId,
            "إيقاف معاش",
            $"تم إيقاف المعاش للملف رقم ({file.FileNumber}). السبب: {dto.SuspensionReason}",
            NotificationConstants.Types.Warning,
            NotificationConstants.Categories.Workflow,
            NotificationConstants.Priorities.High);

        return ApiResponse<SuspensionDto>.Success(new SuspensionDto
        {
            SuspensionID = suspension.SuspensionID,
            FileID = suspension.FileID,
            SuspensionDate = suspension.SuspensionDate,
            SuspensionReason = suspension.SuspensionReason,
            SuspensionSource = suspension.SuspensionSource,
            Status = suspension.Status,
            CreatedAt = suspension.CreatedAt,
            LinkedPledge = pledgeSummary
        }, "تم إيقاف المعاش بنجاح");
    }

    public async Task<ApiResponse<bool>> EndSuspensionAsync(int suspensionId, EndSuspensionDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var suspension = await queries.Query(_unitOfWork.PensionSuspensions)
            .Include(s => s.RetirementFile)
            .FirstOrDefaultAsync(s => s.SuspensionID == suspensionId);

        if (suspension == null)
            return ApiResponse<bool>.Fail("الإيقاف غير موجود");

        if (suspension.Status != SuspensionStatusConstants.Active)
            return ApiResponse<bool>.Fail("الإيقاف منتهي بالفعل");

        if (suspension.SuspensionSource == RetirementReviewConstants.SuspensionSource)
            return ApiResponse<bool>.Fail("لا ينتهي إيقاف المراجعة إلا باعتماد مراجعة الملف");

        // End only this pension hold; other holds and file review remain unchanged.
        suspension.Status = SuspensionStatusConstants.Expired;
        suspension.EndDate = DateTime.UtcNow;
        suspension.EndReason = dto.EndReason;
        suspension.EndedByUserID = userId;
        suspension.UpdatedAt = DateTime.UtcNow;
        suspension.UpdatedByUserID = userId;
        _unitOfWork.PensionSuspensions.Update(suspension);

        var file = suspension.RetirementFile;
        await _unitOfWork.SaveChangesAsync();

        await SendFileNotificationAsync(
            file.FileID,
            userId,
            "إنهاء إيقاف المعاش",
            $"تم إنهاء الإيقاف المحدد للمعاش رقم ({file.FileNumber}). السبب: {dto.EndReason}",
            NotificationConstants.Types.Success,
            NotificationConstants.Categories.Workflow,
            NotificationConstants.Priorities.Normal);

        return ApiResponse<bool>.Success(true, "تم إنهاء الإيقاف المحدد للمعاش");
    }

    public async Task<ApiResponse<List<SuspensionDto>>> GetByFileIdAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var suspensions = await queries.Query(_unitOfWork.PensionSuspensions)
            .AsNoTracking()
            .Where(s => s.FileID == fileId)
            .Include(s => s.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Include(s => s.CreatedByUser)
            .Include(s => s.EndedByUser)
            .Include(s => s.DocumentPledges)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SuspensionDto
            {
                SuspensionID = s.SuspensionID,
                FileID = s.FileID,
                FileNumber = s.RetirementFile.FileNumber,
                RetireeName = s.RetirementFile.Retiree != null ? s.RetirementFile.Retiree.FullName : null,
                NationalID = s.RetirementFile.Retiree != null ? s.RetirementFile.Retiree.NationalID : null,
                MilitaryNumber = s.RetirementFile.Retiree != null ? s.RetirementFile.Retiree.MilitaryNumber : null,
                SuspensionDate = s.SuspensionDate,
                SuspensionReason = s.SuspensionReason,
                SuspensionSource = s.SuspensionSource,
                Status = s.Status,
                EndDate = s.EndDate,
                EndReason = s.EndReason,
                CreatedByUser = s.CreatedByUser.FullName,
                EndedByUser = s.EndedByUser != null ? s.EndedByUser.FullName : null,
                CreatedAt = s.CreatedAt,
                LinkedPledge = s.DocumentPledges.Select(p => new PledgeSummaryDto
                {
                    PledgeID = p.PledgeID,
                    PledgeDate = p.PledgeDate,
                    Deadline = p.Deadline,
                    Status = p.Status,
                    FulfilledAt = p.FulfilledAt,
                    PledgedByType = p.PledgedByType,
                    PledgedByName = p.PledgedByName
                }).FirstOrDefault()
            })
            .ToListAsync();

        var fillResult = await FillPledgeMissingDocumentCountsAsync(suspensions);
        if (!fillResult.IsSuccess)
            return ApiResponse<List<SuspensionDto>>.Fail(fillResult.Message ?? "تعذر جلب أعداد النواقص المرتبطة بالتعهدات");

        return ApiResponse<List<SuspensionDto>>.Success(suspensions);
    }

    public async Task<ApiResponse<PagedResult<SuspensionDto>>> GetAllSuspensionsAsync(int page, int pageSize, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var query = queries.Query(_unitOfWork.PensionSuspensions)
            .AsNoTracking()
            .Include(s => s.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Include(s => s.CreatedByUser)
            .Include(s => s.EndedByUser)
            .Include(s => s.DocumentPledges)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SuspensionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.SuspensionDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.RetirementFile.FileNumber.Contains(search) ||
                (s.RetirementFile.Retiree != null && s.RetirementFile.Retiree.FullName.Contains(search)) ||
                (s.RetirementFile.Retiree != null && s.RetirementFile.Retiree.MilitaryNumber != null && s.RetirementFile.Retiree.MilitaryNumber.Contains(search)) ||
                s.SuspensionReason.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var suspensions = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SuspensionDto
            {
                SuspensionID = s.SuspensionID,
                FileID = s.FileID,
                FileNumber = s.RetirementFile.FileNumber,
                RetireeName = s.RetirementFile.Retiree != null ? s.RetirementFile.Retiree.FullName : null,
                NationalID = s.RetirementFile.Retiree != null ? s.RetirementFile.Retiree.NationalID : null,
                MilitaryNumber = s.RetirementFile.Retiree != null ? s.RetirementFile.Retiree.MilitaryNumber : null,
                SuspensionDate = s.SuspensionDate,
                SuspensionReason = s.SuspensionReason,
                SuspensionSource = s.SuspensionSource,
                Status = s.Status,
                EndDate = s.EndDate,
                EndReason = s.EndReason,
                CreatedByUser = s.CreatedByUser.FullName,
                EndedByUser = s.EndedByUser != null ? s.EndedByUser.FullName : null,
                CreatedAt = s.CreatedAt,
                LinkedPledge = s.DocumentPledges.Select(p => new PledgeSummaryDto
                {
                    PledgeID = p.PledgeID,
                    PledgeDate = p.PledgeDate,
                    Deadline = p.Deadline,
                    Status = p.Status,
                    FulfilledAt = p.FulfilledAt,
                    PledgedByType = p.PledgedByType,
                    PledgedByName = p.PledgedByName
                }).FirstOrDefault()
            })
            .ToListAsync();

        var fillResult = await FillPledgeMissingDocumentCountsAsync(suspensions);
        if (!fillResult.IsSuccess)
            return ApiResponse<PagedResult<SuspensionDto>>.Fail(fillResult.Message ?? "تعذر جلب أعداد النواقص المرتبطة بالتعهدات");

        var result = new PagedResult<SuspensionDto>
        {
            Items = suspensions,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<SuspensionDto>>.Success(result);
    }

    private async Task<ApiResponse<bool>> FillPledgeMissingDocumentCountsAsync(IEnumerable<SuspensionDto> suspensions)
    {
        var pledgeIds = suspensions
            .Where(s => s.LinkedPledge != null)
            .Select(s => s.LinkedPledge!.PledgeID)
            .Distinct()
            .ToList();

        if (pledgeIds.Count == 0)
            return ApiResponse<bool>.Success(true);

        var countsResult = await _documentPledgeService.GetMissingDocumentCountsAsync(pledgeIds);
        if (!countsResult.IsSuccess || countsResult.Data == null)
            return ApiResponse<bool>.Fail(countsResult.Message ?? "تعذر جلب أعداد النواقص المرتبطة بالتعهدات");

        foreach (var suspension in suspensions)
        {
            if (suspension.LinkedPledge != null)
                suspension.LinkedPledge.MissingDocumentCount = countsResult.Data.GetValueOrDefault(suspension.LinkedPledge.PledgeID);
        }

        return ApiResponse<bool>.Success(true);
    }

    private async Task SendFileNotificationAsync(int fileId, int currentUserId, string title, string message, string type, string category, byte priority)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file == null) return;

        var notifyUserId = file.AssignedEmployeeID;
        if (notifyUserId == null || notifyUserId == 0)
        {
            var creatorId = await _unitOfWork.AuditLogs.Query()
                .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == fileId && a.Action == "Create")
                .Select(a => a.UserID)
                .FirstOrDefaultAsync();
            notifyUserId = creatorId;
        }

        if (notifyUserId.HasValue && notifyUserId.Value > 0 && notifyUserId.Value != currentUserId)
        {
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserID = notifyUserId.Value,
                Title = title,
                Message = message,
                Type = type,
                Category = category,
                Priority = priority,
                ActorID = currentUserId,
                RelatedTableID = SystemTableConstants.RetirementFiles,
                RelatedRecordID = fileId
            });
        }
    }
}
