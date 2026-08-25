using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Suspensions;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// خدمة إدارة إيقاف المعاش والتعهدات والإعفاءات
/// </summary>
public class PensionSuspensionService : IPensionSuspensionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public PensionSuspensionService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    // ════════════════════════════════════════
    // SUSPENSION + PLEDGE (Item 9)
    // ════════════════════════════════════════

    public async Task<ApiResponse<SuspensionDto>> CreateWithPledgeAsync(CreateSuspensionDto dto, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (file == null)
            return ApiResponse<SuspensionDto>.Fail("الملف غير موجود");

        // Check for existing active suspension
        var existingSuspension = await _unitOfWork.PensionSuspensions.Query()
            .FirstOrDefaultAsync(s => s.FileID == dto.FileID && s.Status == SuspensionStatusConstants.Active && !s.IsDeleted);
        if (existingSuspension != null)
            return ApiResponse<SuspensionDto>.Fail("الملف لديه إيقاف نشط بالفعل");

        // 1. Create Suspension
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

        PledgeSummaryDto? pledgeSummary = null;

        // 2. Auto-create Pledge if missing docs specified
        if (dto.MissingDocumentIDs?.Any() == true)
        {
            var pledge = new DocumentPledge
            {
                FileID = dto.FileID,
                SuspensionID = suspension.SuspensionID,
                PledgeDate = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(dto.DeadlineDays ?? 30),
                Status = PledgeStatusConstants.Pending,
                PledgedByType = dto.PledgedByType ?? "متقاعد",
                PledgedByName = dto.PledgedByName,
                CreatedByUserID = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.DocumentPledges.AddAsync(pledge);
            await _unitOfWork.SaveChangesAsync();

            // Link missing docs to pledge
            foreach (var mdId in dto.MissingDocumentIDs)
            {
                var md = await _unitOfWork.MissingDocuments.GetByIdAsync(mdId);
                if (md != null)
                {
                    md.PledgeID = pledge.PledgeID;
                    md.Status = MissingDocumentStatusConstants.PendingPledge;
                    _unitOfWork.MissingDocuments.Update(md);
                }
            }
            await _unitOfWork.SaveChangesAsync();

            pledgeSummary = new PledgeSummaryDto
            {
                PledgeID = pledge.PledgeID,
                PledgeDate = pledge.PledgeDate,
                Deadline = pledge.Deadline,
                Status = pledge.Status,
                PledgedByType = pledge.PledgedByType,
                PledgedByName = pledge.PledgedByName,
                MissingDocumentCount = dto.MissingDocumentIDs.Count
            };
        }

        // 3. Update file status to Suspended
        var oldStatusID = file.StatusID; // Capture old status BEFORE changing it
        file.PreviousStatusID = file.StatusID; // Save for restoration later
        file.StatusID = FileStatusConstants.Suspended;
        file.StatusReason = $"إيقاف: {dto.SuspensionReason}";
        file.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.RetirementFiles.Update(file);

        // 4. Log transition
        var log = new StatusTransitionLog
        {
            TableID = SystemTableConstants.RetirementFiles,
            RecordID = file.FileID,
            OldStatusID = oldStatusID,
            NewStatusID = FileStatusConstants.Suspended,
            Reason = $"إيقاف: {dto.SuspensionReason}",
            ChangedByUserID = userId,
            TransitionDate = DateTime.UtcNow
        };
        await _unitOfWork.StatusTransitionLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

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
        var suspension = await _unitOfWork.PensionSuspensions.Query()
            .Include(s => s.RetirementFile)
            .FirstOrDefaultAsync(s => s.SuspensionID == suspensionId);

        if (suspension == null)
            return ApiResponse<bool>.Fail("الإيقاف غير موجود");

        if (suspension.Status != SuspensionStatusConstants.Active)
            return ApiResponse<bool>.Fail("الإيقاف منتهي بالفعل");

        // End suspension
        suspension.Status = SuspensionStatusConstants.Expired;
        suspension.EndDate = DateTime.UtcNow;
        suspension.EndReason = dto.EndReason;
        suspension.EndedByUserID = userId;
        suspension.UpdatedAt = DateTime.UtcNow;
        suspension.UpdatedByUserID = userId;
        _unitOfWork.PensionSuspensions.Update(suspension);

        // Reactivate file — restore previous status if available
        var file = suspension.RetirementFile;
        var oldStatusId = file.StatusID;
        file.StatusID = file.PreviousStatusID ?? FileStatusConstants.Active;
        file.PreviousStatusID = null;
        file.StatusReason = $"إنهاء إيقاف: {dto.EndReason}";
        file.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.RetirementFiles.Update(file);

        // Log transition
        var log = new StatusTransitionLog
        {
            TableID = SystemTableConstants.RetirementFiles,
            RecordID = file.FileID,
            OldStatusID = oldStatusId,
            NewStatusID = FileStatusConstants.Active,
            Reason = $"إنهاء إيقاف: {dto.EndReason}",
            ChangedByUserID = userId,
            TransitionDate = DateTime.UtcNow
        };
        await _unitOfWork.StatusTransitionLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        await SendFileNotificationAsync(
            file.FileID,
            userId,
            "إنهاء إيقاف المعاش",
            $"تم إنهاء الإيقاف وإعادة تفعيل الملف رقم ({file.FileNumber}). السبب: {dto.EndReason}",
            NotificationConstants.Types.Success,
            NotificationConstants.Categories.Workflow,
            NotificationConstants.Priorities.Normal);

        return ApiResponse<bool>.Success(true, "تم إنهاء الإيقاف وإعادة تفعيل الملف");
    }

    public async Task<ApiResponse<List<SuspensionDto>>> GetByFileIdAsync(int fileId)
    {
        var suspensions = await _unitOfWork.PensionSuspensions.Query()
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
                    PledgedByName = p.PledgedByName,
                    MissingDocumentCount = p.MissingDocuments.Count
                }).FirstOrDefault()
            })
            .ToListAsync();

        return ApiResponse<List<SuspensionDto>>.Success(suspensions);
    }

    public async Task<ApiResponse<PagedResult<SuspensionDto>>> GetAllSuspensionsAsync(int page, int pageSize, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null)
    {
        var query = _unitOfWork.PensionSuspensions.Query()
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
                    PledgedByName = p.PledgedByName,
                    MissingDocumentCount = p.MissingDocuments.Count
                }).FirstOrDefault()
            })
            .ToListAsync();

        var result = new PagedResult<SuspensionDto>
        {
            Items = suspensions,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<SuspensionDto>>.Success(result);
    }

    // ════════════════════════════════════════
    // EXEMPTIONS (Item 12)
    // ════════════════════════════════════════

    public async Task<ApiResponse<ExemptionDto>> CreateExemptionAsync(CreateExemptionDto dto, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (file == null)
            return ApiResponse<ExemptionDto>.Fail("الملف غير موجود");

        if (!dto.MissingDocumentIDs.Any())
            return ApiResponse<ExemptionDto>.Fail("يجب تحديد وثيقة واحدة على الأقل للإعفاء");

        // Create exemption
        var exemption = new DocumentExemption
        {
            FileID = dto.FileID,
            DirectiveSource = dto.DirectiveSource,
            ExemptionDate = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.DocumentExemptions.AddAsync(exemption);
        await _unitOfWork.SaveChangesAsync();

        // Link missing documents to this exemption
        foreach (var mdId in dto.MissingDocumentIDs)
        {
            var md = await _unitOfWork.MissingDocuments.GetByIdAsync(mdId);
            if (md != null)
            {
                md.ExemptionID = exemption.ExemptionID;
                md.Status = MissingDocumentStatusConstants.ExemptedAr;
                _unitOfWork.MissingDocuments.Update(md);
            }
        }
        await _unitOfWork.SaveChangesAsync();

        await SendFileNotificationAsync(
            dto.FileID,
            userId,
            "إعفاء وثائق",
            $"تم إعفاء {dto.MissingDocumentIDs.Count} وثيقة/وثائق للملف رقم ({file.FileNumber}).",
            NotificationConstants.Types.Info,
            NotificationConstants.Categories.Workflow,
            NotificationConstants.Priorities.Normal);

        return ApiResponse<ExemptionDto>.Success(new ExemptionDto
        {
            ExemptionID = exemption.ExemptionID,
            FileID = exemption.FileID,
            DirectiveSource = exemption.DirectiveSource,
            ExemptionDate = exemption.ExemptionDate,
            Notes = exemption.Notes,
            CreatedAt = exemption.CreatedAt,
            ExemptedDocumentCount = dto.MissingDocumentIDs.Count
        }, "تم إعفاء الوثائق بنجاح");
    }

    public async Task<ApiResponse<List<ExemptionDto>>> GetExemptionsByFileIdAsync(int fileId)
    {
        var exemptions = await _unitOfWork.DocumentExemptions.Query()
            .AsNoTracking()
            .Where(e => e.FileID == fileId)
            .Include(e => e.CreatedByUser)
            .Include(e => e.MissingDocuments)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new ExemptionDto
            {
                ExemptionID = e.ExemptionID,
                FileID = e.FileID,
                DirectiveSource = e.DirectiveSource,
                ExemptionDate = e.ExemptionDate,
                Notes = e.Notes,
                CreatedByUser = e.CreatedByUser.FullName,
                CreatedAt = e.CreatedAt,
                ExemptedDocumentCount = e.MissingDocuments.Count
            })
            .ToListAsync();

        return ApiResponse<List<ExemptionDto>>.Success(exemptions);
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
