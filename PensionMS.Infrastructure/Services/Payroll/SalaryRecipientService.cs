using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.SalaryRecipients;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Helpers;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;

namespace PensionMS.Infrastructure.Services.Payroll;

public class SalaryRecipientService : ISalaryRecipientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IDocumentService _documentService;
    private readonly IEntitlementEngine _entitlementEngine;
    private readonly IFileReviewService _reviewService;
    private readonly INotificationService _notificationService;
    private readonly IAccessiblePensionRepository _access;

    public SalaryRecipientService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, IDocumentService documentService, IEntitlementEngine entitlementEngine, IFileReviewService reviewService, INotificationService notificationService, IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _documentService = documentService;
        _entitlementEngine = entitlementEngine;
        _reviewService = reviewService;
        _notificationService = notificationService;
        _access = access;
    }

    private async Task<IQueryable<SalaryRecipient>> RecipientsAsync(string permission = "SalaryRecipient.View")
    {
        var fileIds = (await _access.ForPermissionAsync(permission)).Files().Select(f => f.FileID);
        return _unitOfWork.SalaryRecipients.Query().Where(r => fileIds.Contains(r.FileID));
    }

    public Task<ApiResponse<SalaryRecipientDto>> CreateAsync(CreateSalaryRecipientDto dto, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            await _unitOfWork.LockFileReviewAsync(dto.FileID);
            var file = await (await _access.ForPermissionAsync("SalaryRecipient.Add")).Files().FirstOrDefaultAsync(f => f.FileID == dto.FileID);
            if (file == null || !RetirementFileActivationValidator.IsEditable(file))
                return ApiResponse<SalaryRecipientDto>.Fail("الملف غير متاح لتعديل مستلمي المعاش");
            return await CreateCoreAsync(dto, userId);
        });

    public async Task<ApiResponse<List<SalaryRecipientDto>>> GetByFileIdAsync(int fileId)
    {
        var recipients = await (await RecipientsAsync())
            .AsNoTracking()
            .Include(s => s.RetirementFile)
            .Include(s => s.Retiree)
            .Include(s => s.Agent).ThenInclude(a => a!.EntitledPerson)
            .Include(s => s.EntitledPerson)
            .Include(s => s.Guardian)
            .Where(s => s.FileID == fileId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<SalaryRecipientDto>>.Success(recipients.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<SalaryRecipientDto>> GetByIdAsync(int id)
    {
        var recipient = await (await RecipientsAsync())
            .AsNoTracking()
            .Include(s => s.RetirementFile)
            .Include(s => s.Retiree)
            .Include(s => s.Agent).ThenInclude(a => a!.EntitledPerson)
            .Include(s => s.EntitledPerson)
            .Include(s => s.Guardian)
            .FirstOrDefaultAsync(s => s.RecipientID == id);

        if (recipient == null)
            return ApiResponse<SalaryRecipientDto>.Fail("مستلم المعاش غير موجود");

        return ApiResponse<SalaryRecipientDto>.Success(MapToDto(recipient));
    }

    private async Task<ApiResponse<SalaryRecipientDto>> CreateCoreAsync(CreateSalaryRecipientDto dto, int userId)
    {
        // Validate CHECK constraint:
        // 1) نوع واحد فقط (Retiree / Agent / Entitled / Guardian)
        // 2) أو حالة خاصة: Guardian + Entitled (وصي يستلم عن مستحق محدد)
        var typeCount = (dto.RetireeID.HasValue ? 1 : 0) +
                       (dto.AgentID.HasValue ? 1 : 0) +
                       (dto.EntitledID.HasValue ? 1 : 0) +
                       (dto.GuardianID.HasValue ? 1 : 0);

        var isGuardianWithEntitled =
            dto.GuardianID.HasValue &&
            dto.EntitledID.HasValue &&
            !dto.RetireeID.HasValue &&
            !dto.AgentID.HasValue;

        if (typeCount != 1 && !isGuardianWithEntitled)
            return ApiResponse<SalaryRecipientDto>.Fail("يجب تحديد نوع واحد فقط لمستلم المعاش (أو وصي مرتبط بمستحق واحد)");

        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (file == null)
            return ApiResponse<SalaryRecipientDto>.Fail("الملف غير موجود");

        if (dto.GuardianID.HasValue)
        {
            var guardian = await _unitOfWork.Guardians.GetByIdAsync(dto.GuardianID.Value);
            if (guardian == null || guardian.IsDeleted || !guardian.IsActive || guardian.FileID != dto.FileID || !dto.EntitledID.HasValue)
                return ApiResponse<SalaryRecipientDto>.Fail("يجب تحديد وصي نشط ومستحق من الملف نفسه");
            if (!await _unitOfWork.GuardianEntitledPersons.Query().AnyAsync(g => g.GuardianID == dto.GuardianID && g.EntitledID == dto.EntitledID && g.IsActive))
                return ApiResponse<SalaryRecipientDto>.Fail("الوصي غير مرتبط بهذا المستحق");
        }

        Agent? selectedAgent = null;
        if (dto.AgentID.HasValue)
        {
            selectedAgent = await _unitOfWork.Agents.Query().Include(a => a.EntitledPerson)
                .FirstOrDefaultAsync(a => a.AgentID == dto.AgentID);
            if (selectedAgent == null || selectedAgent.FileID != dto.FileID || !RecipientScope.AgentPayable(selectedAgent, RecipientScope.CalendarDate(dto.StartDate)))
                return ApiResponse<SalaryRecipientDto>.Fail("الوكيل غير متاح للصرف أو لا ينتمي لهذا الملف أو وكالته غير سارية");
            if (selectedAgent.EntitledID.HasValue && (selectedAgent.EntitledPerson == null || !selectedAgent.EntitledPerson.IsActive || selectedAgent.EntitledPerson.IsDeleted || selectedAgent.EntitledPerson.IsDeceased || !RecipientScope.Reviewed(selectedAgent.EntitledPerson.PendingReviewStatus)))
                return ApiResponse<SalaryRecipientDto>.Fail("المستحق الذي يمثله الوكيل غير مؤهل للصرف");
            if (selectedAgent.EntitledID.HasValue && await _unitOfWork.GuardianEntitledPersons.Query().AnyAsync(g => g.EntitledID == selectedAgent.EntitledID && g.IsActive))
                return ApiResponse<SalaryRecipientDto>.Fail("المستحق مرتبط بوصاية نشطة");
        }
        int? effectiveEntitledID = selectedAgent?.EntitledID ?? dto.EntitledID;
        decimal? previousShare = null;
        if (effectiveEntitledID.HasValue)
            previousShare = await RecipientScope.Query(_unitOfWork, dto.FileID, effectiveEntitledID)
                .Where(r => r.IsActive).OrderByDescending(r => r.CreatedAt).Select(r => r.SharePercent).FirstOrDefaultAsync();
        var hasOtherMode = await _unitOfWork.SalaryRecipients.Query().Where(r => r.FileID == dto.FileID && r.IsActive)
            .AnyAsync(r => ((r.AgentID.HasValue ? r.Agent!.EntitledID : r.EntitledID) == null) != (effectiveEntitledID == null));
        if (hasOtherMode)
            return ApiResponse<SalaryRecipientDto>.Fail("أنه مستلمي وضع الصرف السابق قبل التحويل بين الصرف العام وصرف حصص المستحقين");

        if (dto.SharePercent.HasValue && (dto.SharePercent <= 0 || dto.SharePercent > 100))
            return ApiResponse<SalaryRecipientDto>.Fail("نسبة الاستحقاق يجب أن تكون أكبر من صفر ولا تتجاوز 100%");
        if (dto.EntitledAmount.HasValue && dto.EntitledAmount <= 0)
            return ApiResponse<SalaryRecipientDto>.Fail("مبلغ الاستحقاق يجب أن يكون أكبر من صفر");
        if (dto.SharePercent.HasValue && dto.EntitledAmount.HasValue)
            return ApiResponse<SalaryRecipientDto>.Fail("أدخل نسبة الاستحقاق أو المبلغ، وليس كليهما");

        // Validate the referenced entity exists
        decimal? calculatedSharePercent = dto.SharePercent;
        if (dto.EntitledID.HasValue && dto.EntitledAmount.HasValue)
        {
            var entitlement = await _unitOfWork.FinancialEntitlements.Query()
                .AsNoTracking()
                .Where(e => e.RetireeID == file.RetireeID && !e.IsDeleted)
                .OrderByDescending(e => e.EffectiveDate)
                .FirstOrDefaultAsync();
            if (entitlement == null || entitlement.Amount <= 0)
                return ApiResponse<SalaryRecipientDto>.Fail("يجب تسجيل معاش مالي للمتقاعد قبل إدخال مبلغ المستحق");
            if (dto.EntitledAmount > entitlement.Amount)
                return ApiResponse<SalaryRecipientDto>.Fail("مبلغ المستحق لا يمكن أن يتجاوز إجمالي المعاش");
            calculatedSharePercent = Math.Round((dto.EntitledAmount.Value / entitlement.Amount) * 100, 2);
        }

        if (dto.RetireeID.HasValue)
        {
            var retiree = await _unitOfWork.Retirees.Query()
                .IgnoreQueryFilters()
                .Where(r => !r.IsDeleted)
                .FirstOrDefaultAsync(r => r.RetireeID == dto.RetireeID.Value);
            if (retiree == null || retiree.RetireeID != file.RetireeID)
                return ApiResponse<SalaryRecipientDto>.Fail("المتقاعد غير موجود");
        }
        else if (dto.AgentID.HasValue)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(dto.AgentID.Value);
            if (agent == null || agent.IsDeleted)
                return ApiResponse<SalaryRecipientDto>.Fail("الوكيل غير موجود");
            
            if (!agent.IsActive)
                return ApiResponse<SalaryRecipientDto>.Fail("لا يمكن اختيار وكيل غير نشط كمستلم للمعاش");
        }
        else if (dto.EntitledID.HasValue)
        {
            var entitled = await _unitOfWork.EntitledPersons.Query()
                .Include(e => e.Documents.Where(d => !d.IsDeleted))
                .FirstOrDefaultAsync(e => e.EntitledID == dto.EntitledID.Value && !e.IsDeleted);
            if (entitled == null || entitled.FileID != dto.FileID)
                return ApiResponse<SalaryRecipientDto>.Fail("المستحق غير موجود");

            if (entitled.IsDeceased)
                return ApiResponse<SalaryRecipientDto>.Fail("لا يمكن اختيار مستحق متوفى كمستلم للمعاش");

            // ─── 1. تحديث البيانات التكميلية (في حال وجود نواقص) ───
            bool dataUpdated = false;
            if (!string.IsNullOrWhiteSpace(dto.NationalID) && entitled.NationalID != dto.NationalID) { entitled.NationalID = dto.NationalID; dataUpdated = true; }
            if (dto.BirthDate.HasValue && dto.BirthDate != entitled.BirthDate) { entitled.BirthDate = dto.BirthDate.Value; dataUpdated = true; }
            if (dto.SocialStatusID.HasValue && dto.SocialStatusID > 0 && entitled.SocialStatusID != dto.SocialStatusID) { entitled.SocialStatusID = dto.SocialStatusID; dataUpdated = true; }
            if (dto.HealthStatusID.HasValue && dto.HealthStatusID > 0 && entitled.HealthStatusID != dto.HealthStatusID) { entitled.HealthStatusID = dto.HealthStatusID; dataUpdated = true; }

            if (dataUpdated)
            {
                entitled.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.EntitledPersons.Update(entitled);
                await _unitOfWork.SaveChangesAsync();
            }

            // ─── 2. رفع الوثائق المطلوبة (الصورة والهوية) ───
            var identityOwner = new PensionMS.Core.Rules.Documents.RetirementDocumentRequirement(
                DocumentTypeConstants.NationalId, null, null, null, entitled.EntitledID, null, null);
            var hasNationalId = PensionMS.Core.Rules.Documents.RetirementFileDocumentRules.FindValidDocument(
                entitled.Documents.Where(d => d.FileID == entitled.FileID), identityOwner,
                PensionMS.Core.Time.YemenTime.Today) != null;

            // حفظ الصورة الشخصية
            if (dto.PersonalPhoto != null)
            {
                using var stream = dto.PersonalPhoto.OpenReadStream();
                var path = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "entitled-photos");
                entitled.PersonalPhotoPath = path;
                _unitOfWork.EntitledPersons.Update(entitled);
                await _unitOfWork.SaveChangesAsync();
            }

            // رفع الهوية الوطنية
            if (dto.NationalIdImage != null && !hasNationalId)
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentTypeID = DocumentTypeConstants.NationalId,
                    EntitledPersonID = entitled.EntitledID,
                    Notes = $"الهوية الوطنية للمستحق: {entitled.FullName}",
                    File = dto.NationalIdImage
                };
                var uploaded = await _documentService.UploadAsync(uploadDto, userId);
                if (!uploaded.IsSuccess)
                    return ApiResponse<SalaryRecipientDto>.Fail(uploaded.Message ?? "تعذر رفع هوية المستحق");
            }

            // ─── 3. إعادة التقييم والتحقق النهائي من الحالة ───
            await _entitlementEngine.ReEvaluateEntitlementStatusAsync(entitled.EntitledID);
            
            // إعادة جلب الحالة المحدثة للتأكد من التفعيل
            var updatedEp = await _unitOfWork.EntitledPersons.GetByIdAsync(entitled.EntitledID);
            if (updatedEp == null || !updatedEp.IsActive)
            {
                return ApiResponse<SalaryRecipientDto>.Fail("لا يمكن اختيار المستحق كمستلم معاش بسبب وجود نواقص (مثل نقص الهوية أو الصورة أو بيانات أساسية). يرجى استكمال كافة متطلبات الصرف أولاً.");
            }

            // ─── 4. حساب النسبة المئوية ───
            
            if (!calculatedSharePercent.HasValue)
            {
                // لا مبلغ مُدخل → استخدام النسبة الافتراضية من المستحق
                calculatedSharePercent = entitled.SharePercent;
            }
        }
        else if (dto.GuardianID.HasValue)
        {
            var guardian = await _unitOfWork.Guardians.GetByIdAsync(dto.GuardianID.Value);
            if (guardian == null || guardian.IsDeleted)
                return ApiResponse<SalaryRecipientDto>.Fail("الوصي غير موجود");
                
            if (!guardian.IsActive)
                return ApiResponse<SalaryRecipientDto>.Fail("لا يمكن اختيار وصي غير نشط كمستلم للمعاش");
        }

        var recipient = new SalaryRecipient
        {
            FileID = dto.FileID,
            RetireeID = dto.RetireeID,
            AgentID = dto.AgentID,
            EntitledID = dto.EntitledID,
            GuardianID = dto.GuardianID,
            // إذا كان هناك GuardianID نعتبر النوع "وصي" حتى في حالة Guardian + Entitled
            RecipientType = dto.GuardianID.HasValue ? "وصي" :
                           dto.RetireeID.HasValue ? "متقاعد" :
                           dto.AgentID.HasValue ? "وكيل" :
                           dto.EntitledID.HasValue ? "مستحق" : "",
            StartDate = dto.StartDate,
            IsActive = true,
            Notes = dto.Notes,
            SharePercent = selectedAgent?.EntitledID.HasValue == true ? selectedAgent.EntitledPerson?.SharePercent ?? previousShare :
                dto.EntitledID.HasValue ? calculatedSharePercent : dto.SharePercent,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserID = userId
        };

        // ── تحديد النطاق الفعّال (EntitledID المكافئ) ────────────────

        // ── Q5: يُمنع الجمع وصي + وكيل لنفس المستحق ──────────────
        if (dto.AgentID.HasValue && effectiveEntitledID.HasValue)
        {
            var hasActiveGuardian = await _unitOfWork.SalaryRecipients.Query()
                .AnyAsync(s => s.FileID == dto.FileID && s.EntitledID == effectiveEntitledID
                             && s.GuardianID.HasValue && s.IsActive && !s.IsDeleted);
            if (hasActiveGuardian)
                return ApiResponse<SalaryRecipientDto>.Fail("لا يمكن إضافة وكيل كمستلم لهذا المستحق لوجود وصي نشط بالفعل.");
        }
        else if (dto.GuardianID.HasValue)
        {
            var entitledIdForGuardian = dto.EntitledID ?? effectiveEntitledID;
            if (entitledIdForGuardian.HasValue)
            {
                var hasActiveAgent = await _unitOfWork.SalaryRecipients.Query()
                    .AnyAsync(s => s.FileID == dto.FileID && s.Agent!.EntitledID == entitledIdForGuardian
                                 && s.AgentID.HasValue && s.IsActive && !s.IsDeleted);
                if (hasActiveAgent)
                    return ApiResponse<SalaryRecipientDto>.Fail("لا يمكن إضافة وصي كمستلم لهذا المستحق لوجود وكيل نشط بالفعل.");
            }
        }

        // ── إلغاء تفعيل مستلمي نفس النطاق فقط ─────────────────────
        var activeRecipients = await RecipientScope.Query(_unitOfWork, dto.FileID, effectiveEntitledID)
            .Where(s => s.IsActive)
            .ToListAsync();

        foreach (var active in activeRecipients)
        {
            active.IsActive = false;
            active.EndDate = dto.StartDate;
            active.UpdatedAt = DateTime.UtcNow;
            active.UpdatedByUserID = userId;
            _unitOfWork.SalaryRecipients.Update(active);
        }

        await _unitOfWork.SalaryRecipients.AddAsync(recipient);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var review = await _reviewService.RequestReviewAsync(dto.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<SalaryRecipientDto>.Fail(review.Message ?? "تعذر طلب مراجعة مستلم المعاش");

        var fileForNotify = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (fileForNotify != null)
        {
            await SendFileNotificationAsync(
                dto.FileID,
                userId,
                "مستلم معاش جديد",
                $"تم إضافة مستلم معاش جديد للملف رقم ({fileForNotify.FileNumber}).",
                NotificationConstants.Types.Info,
                NotificationConstants.Categories.Financial,
                NotificationConstants.Priorities.Normal);
        }

        var created = await _unitOfWork.SalaryRecipients.Query()
            .Include(s => s.RetirementFile)
            .Include(s => s.Retiree)
            .Include(s => s.Agent).ThenInclude(a => a!.EntitledPerson)
            .Include(s => s.EntitledPerson)
            .Include(s => s.Guardian)
            .FirstAsync(s => s.RecipientID == recipient.RecipientID);

        return ApiResponse<SalaryRecipientDto>.Success(MapToDto(created), "تم إنشاء مستلم المعاش بنجاح");
    }

    public Task<ApiResponse<bool>> EndAsync(int id, EndSalaryRecipientDto dto, int userId) =>
        ChangeRecipientAsync(id, () => EndCoreAsync(id, dto, userId));

    private async Task<ApiResponse<T>> ChangeRecipientAsync<T>(int id, Func<Task<ApiResponse<T>>> operation)
    {
        var fileId = await (await RecipientsAsync("SalaryRecipient.Edit")).Where(r => r.RecipientID == id).Select(r => (int?)r.FileID).FirstOrDefaultAsync();
        if (!fileId.HasValue) return ApiResponse<T>.Fail("مستلم المعاش غير موجود");
        return await UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            await _unitOfWork.LockFileReviewAsync(fileId.Value);
            var file = await (await _access.ForPermissionAsync("SalaryRecipient.Edit")).Files().FirstAsync(f => f.FileID == fileId);
            if (!RetirementFileActivationValidator.IsEditable(file)) return ApiResponse<T>.Fail("حالة الملف لا تسمح بتعديل المستلم");
            return await operation();
        });
    }

    private async Task<ApiResponse<bool>> EndCoreAsync(int id, EndSalaryRecipientDto dto, int userId)
    {
        var recipient = await (await RecipientsAsync("SalaryRecipient.Edit"))
            .FirstOrDefaultAsync(s => s.RecipientID == id);

        if (recipient == null)
            return ApiResponse<bool>.Fail("مستلم المعاش غير موجود");

        if (!recipient.IsActive)
            return ApiResponse<bool>.Fail("مستلم المعاش منتهي بالفعل");
        if (dto.EndDate < recipient.StartDate)
            return ApiResponse<bool>.Fail("تاريخ الإنهاء يجب ألا يسبق تاريخ بدء الاستلام");

        recipient.IsActive = false;
        recipient.EndDate = dto.EndDate;
        recipient.Notes = dto.Notes ?? recipient.Notes;
        recipient.UpdatedAt = DateTime.UtcNow;
        recipient.UpdatedByUserID = userId;

        _unitOfWork.SalaryRecipients.Update(recipient);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var review = await _reviewService.RequestReviewAsync(recipient.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<bool>.Fail(review.Message ?? "تعذر طلب مراجعة إنهاء المستلم");

        var endFile = await _unitOfWork.RetirementFiles.GetByIdAsync(recipient.FileID);
        if (endFile != null)
        {
            await SendFileNotificationAsync(
                recipient.FileID,
                userId,
                "إنهاء مستلم معاش",
                $"تم إنهاء مستلم المعاش للملف رقم ({endFile.FileNumber}).",
                NotificationConstants.Types.Warning,
                NotificationConstants.Categories.Financial,
                NotificationConstants.Priorities.Normal);
        }

        return ApiResponse<bool>.Success(true, "تم إنهاء مستلم المعاش بنجاح");
    }

    public async Task<ApiResponse<string>> UpdatePhotoAsync(int id, IRemoteFile photo, int userId)
    {
        var recipient = await (await RecipientsAsync("SalaryRecipient.Edit"))
            .Include(s => s.Retiree)
            .Include(s => s.Agent)
            .Include(s => s.EntitledPerson)
            .Include(s => s.Guardian)
            .FirstOrDefaultAsync(s => s.RecipientID == id);

        if (recipient == null)
            return ApiResponse<string>.Fail("مستلم المعاش غير موجود");

        string folder = recipient.RecipientType switch
        {
            "متقاعد" => "retiree-photos",
            "وكيل" => "agent-photos",
            "مستحق" => "entitled-photos",
            "وصي" => "guardian-photos",
            _ => "recipient-photos"
        };

        using var stream = photo.OpenReadStream();
        var path = await _fileStorage.SaveFileAsync(stream, photo.FileName, folder);

        if (recipient.RetireeID.HasValue && recipient.Retiree != null)
        {
            recipient.Retiree.PersonalPhotoPath = path;
            _unitOfWork.Retirees.Update(recipient.Retiree);
        }
        else if (recipient.AgentID.HasValue && recipient.Agent != null)
        {
            recipient.Agent.PersonalPhotoPath = path;
            _unitOfWork.Agents.Update(recipient.Agent);
        }
        else if (recipient.EntitledID.HasValue && recipient.EntitledPerson != null)
        {
            recipient.EntitledPerson.PersonalPhotoPath = path;
            _unitOfWork.EntitledPersons.Update(recipient.EntitledPerson);
        }
        else if (recipient.GuardianID.HasValue && recipient.Guardian != null)
        {
            recipient.Guardian.PersonalPhotoPath = path;
            _unitOfWork.Guardians.Update(recipient.Guardian);
        }
        else
        {
            return ApiResponse<string>.Fail("تعذر تحديد الكيان المرتبط لتحديث الصورة");
        }

        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        await _reviewService.RequestReviewAsync(recipient.FileID, userId, now);
        return ApiResponse<string>.Success(path, "تم تحديث الصورة الشخصية بنجاح");
    }

    private static SalaryRecipientDto MapToDto(SalaryRecipient s)
    {
        string recipientName = "";
        string recipientType = s.RecipientType;
        string? photoPath = null;

        if (s.GuardianID.HasValue && s.Guardian != null)
        {
            recipientName = s.Guardian.FullName;
            photoPath = s.Guardian.PersonalPhotoPath;
        }
        else if (s.RetireeID.HasValue && s.Retiree != null)
        {
            recipientName = s.Retiree.FullName;
            photoPath = s.Retiree.PersonalPhotoPath;
        }
        else if (s.AgentID.HasValue && s.Agent != null)
        {
            recipientName = s.Agent.FullName;
            photoPath = s.Agent.PersonalPhotoPath;
        }
        else if (!s.GuardianID.HasValue && s.EntitledID.HasValue && s.EntitledPerson != null)
        {
            recipientName = s.EntitledPerson.FullName;
            photoPath = s.EntitledPerson.PersonalPhotoPath;
        }
        else if (s.GuardianID.HasValue && s.Guardian != null)
        {
            recipientName = s.Guardian.FullName;
            photoPath = s.Guardian.PersonalPhotoPath;
        }

        return new SalaryRecipientDto
        {
            EffectiveEntitledID = RecipientScope.EntitledId(s),
            EntitledPersonName = s.Agent?.EntitledPerson?.FullName ?? s.EntitledPerson?.FullName,
            RecipientID = s.RecipientID,
            FileID = s.FileID,
            FileNumber = s.RetirementFile?.FileNumber ?? "",
            RetireeID = s.RetireeID,
            AgentID = s.AgentID,
            EntitledID = s.EntitledID,
            GuardianID = s.GuardianID,
            RecipientName = recipientName,
            RecipientType = recipientType,
            RecipientPhotoPath = photoPath,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            IsActive = s.IsActive,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            PendingReviewStatus = (int)s.PendingReviewStatus,
            SharePercent = s.SharePercent
        };
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
