using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Rules.Documents;
using PensionMS.Core.Rules.Entitlements;
using PensionMS.Infrastructure.Helpers;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Pension.Beneficiaries;

public class EntitledPersonService : IEntitledPersonService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IFileStorageService _fileStorage;
    private readonly IDocumentService _documentService;
    private readonly IEntitlementEngine _entitlementEngine;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly IFileReviewService _reviewService;
    private readonly ILogger<EntitledPersonService> _logger;

    public EntitledPersonService(
        IUnitOfWork unitOfWork, 
        IFileStorageService fileStorage, 
        IDocumentService documentService,
        IEntitlementEngine entitlementEngine,
        IMissingDocumentService missingDocumentService,
        IFileReviewService reviewService,
        ILogger<EntitledPersonService> logger, IAccessiblePensionRepository access)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _documentService = documentService;
        _entitlementEngine = entitlementEngine;
        _missingDocumentService = missingDocumentService;
        _reviewService = reviewService;
        _logger = logger;
    }

    // ════════════════════════════════════════════════════════════
    // READ
    // ════════════════════════════════════════════════════════════

    public async Task<ApiResponse<EntitledPersonDto>> GetByIdAsync(int id)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var ep = await queries.Query(_unitOfWork.EntitledPersons)
            .AsNoTracking()
            .Include(e => e.Relationship)
            .Include(e => e.SocialStatus)
            .Include(e => e.HealthStatus)
            .Include(e => e.Guardians.Where(g => g.IsActive && !g.IsDeleted))
            .Include(e => e.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(e => e.EntitledID == id);

        if (ep == null)
            return ApiResponse<EntitledPersonDto>.Fail("المستحق غير موجود");

        return ApiResponse<EntitledPersonDto>.Success(MapToDto(ep));
    }

    public async Task<ApiResponse<List<EntitledPersonDto>>> GetByFileIdAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var list = await queries.Query(_unitOfWork.EntitledPersons)
            .AsNoTracking()
            .Include(e => e.Relationship)
            .Include(e => e.SocialStatus)
            .Include(e => e.HealthStatus)
            .Include(e => e.Guardians.Where(g => g.IsActive && !g.IsDeleted))
            .Include(e => e.Documents.Where(d => !d.IsDeleted))
            .Where(e => e.FileID == fileId && !e.IsDeleted)
            .ToListAsync();

        return ApiResponse<List<EntitledPersonDto>>.Success(list.Select(MapToDto).ToList());
    }

    // ════════════════════════════════════════════════════════════
    // CREATE
    // ════════════════════════════════════════════════════════════

    public async Task<ApiResponse<EntitledPersonDto>> CreateAsync(CreateEntitledPersonDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (!await queries.Files().AnyAsync(f => f.FileID == dto.FileID)) throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود");
        // Guard: منع تكرار رقم الهوية في نفس الملف
        if (!string.IsNullOrWhiteSpace(dto.NationalID))
        {
            var duplicate = await queries.Query(_unitOfWork.EntitledPersons)
                .AnyAsync(e => e.FileID == dto.FileID && e.NationalID == dto.NationalID && !e.IsDeleted);
            if (duplicate)
                return ApiResponse<EntitledPersonDto>.Fail("هذا المستحق (رقم الهوية) مسجل مسبقاً في هذا الملف");
        }

        // ── فحص حالة المتقاعد: إذا كان حياً → المستحق يُضاف موقوفاً ──
        var file = await queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == dto.FileID);

        if (file == null)
            return ApiResponse<EntitledPersonDto>.Fail("الملف التقاعدي غير موجود");

        bool retireeIsAlive = file.Retiree?.IsAlive ?? true;

        // التحقق من صحة البيانات قبل الإدراج (Fail-fast بدلاً من القيم الافتراضية الصامتة)
        if (dto.RelationshipID == null || dto.RelationshipID <= 0)
            return ApiResponse<EntitledPersonDto>.Fail("صلة القرابة مطلوبة");

        if (!retireeIsAlive && dto.DateOfBirth == null)
            return ApiResponse<EntitledPersonDto>.Fail("تاريخ الميلاد مطلوب لاستكمال بيانات المستحق بعد وفاة المتقاعد");

        if (dto.SharePercent.HasValue && (dto.SharePercent <= 0 || dto.SharePercent > 100))
            return ApiResponse<EntitledPersonDto>.Fail("نسبة الاستحقاق يجب أن تكون أكبر من صفر ولا تتجاوز 100%");
        if (dto.EntitledAmount.HasValue && dto.EntitledAmount <= 0)
            return ApiResponse<EntitledPersonDto>.Fail("مبلغ الاستحقاق يجب أن يكون أكبر من صفر");
        if (dto.SharePercent.HasValue && dto.EntitledAmount.HasValue)
            return ApiResponse<EntitledPersonDto>.Fail("أدخل نسبة الاستحقاق أو المبلغ، وليس كليهما");

        // ── تحويل المبلغ المُدخل إلى نسبة مئوية من إجمالي معاش المتقاعد ──
        decimal? sharePercent = dto.SharePercent;
        if (dto.EntitledAmount.HasValue && dto.EntitledAmount > 0)
        {
            // جلب آخر حق مالي نشط للمتقاعد
            var entitlement = await queries.Query(_unitOfWork.FinancialEntitlements)
                .AsNoTracking()
                .Where(e => e.RetireeID == file.RetireeID && !e.IsDeleted)
                .OrderByDescending(e => e.EffectiveDate)
                .FirstOrDefaultAsync();

            if (entitlement == null || entitlement.Amount <= 0)
                return ApiResponse<EntitledPersonDto>.Fail("يجب تسجيل معاش مالي للمتقاعد قبل إدخال مبلغ المستحق");
            if (dto.EntitledAmount > entitlement.Amount)
                return ApiResponse<EntitledPersonDto>.Fail("مبلغ المستحق لا يمكن أن يتجاوز إجمالي المعاش");
            sharePercent = Math.Round((dto.EntitledAmount.Value / entitlement.Amount) * 100, 2);
        }

        var ep = new EntitledPerson
        {
            FileID                = dto.FileID,
            FullName              = dto.FullName,
            NationalID            = dto.NationalID ?? "",
            RelationshipID        = dto.RelationshipID.Value,
            BirthDate             = dto.DateOfBirth ?? PensionMS.Core.Time.YemenTime.Today,
            Phone                 = dto.Phone,
            // معالجة القيم الصفرية كـ Null لتجنب أخطاء المفاتيح الخارجية
            SocialStatusID        = (dto.SocialStatusID == 0) ? null : dto.SocialStatusID,
            HealthStatusID        = (dto.HealthStatusID == 0) ? null : dto.HealthStatusID,
            HasDisabilityException = dto.HasDisabilityException,
            SharePercent          = sharePercent,
            IsActive              = false, // مبدئياً غير نشط حتى يتم الفحص
            StopCode              = retireeIsAlive ? EntitledStopCodes.RetireeAlive : EntitledStopCodes.MissingData,
            StopReason            = retireeIsAlive ? "المتقاعد على قيد الحياة — الاستحقاق معلق" : "قيد فحص الأهلية والنواقص",
            StopDate              = DateTime.UtcNow,
            CreatedAt             = DateTime.UtcNow,
            CreatedByUserID       = userId
        };

        await _unitOfWork.EntitledPersons.AddAsync(ep);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // م27: أخطاء المرفقات تُجمع وتظهر في الاستجابة بدلاً من إخفائها
        var attachmentErrors = new List<string>();

        // حفظ الهوية الوطنية باستخدام الخدمة الموحدة (تدعم الدمج التلقائي)
        if (dto.NationalIdImage != null || dto.NationalIdBackImage != null)
        {
            try
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentTypeID = DocumentTypeConstants.NationalId,
                    EntitledPersonID = ep.EntitledID,
                    Notes = $"الهوية الوطنية للمستحق: {dto.FullName}",
                    File = dto.NationalIdImage!,
                    FileBack = dto.NationalIdBackImage
                };

                if (dto.NationalIdImage == null && dto.NationalIdBackImage != null)
                {
                    uploadDto.File = dto.NationalIdBackImage;
                    uploadDto.FileBack = null;
                }

                var result = await _documentService.UploadAsync(uploadDto, userId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("فشل في رفع الهوية للمستحق: {Message}", result.Message);
                    attachmentErrors.Add($"الهوية الوطنية: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ غير متوقع عند رفع هوية المستحق");
                attachmentErrors.Add("الهوية الوطنية: تعذر الرفع بشكل غير متوقع");
            }
        }

        // ── إعادة تقييم حالة الاستحقاق تلقائياً فور الإضافة ──
        // هذا يضمن تطبيق القواعد القانونية (العمر، الزواج) وقواعد النواقص حتى لو كان المتقاعد متوفى
        await ReEvaluateEntitlementStatusAsync(ep.EntitledID);

        await _missingDocumentService.RefreshComplianceCacheAsync(ep.FileID);
        await _reviewService.RequestReviewAsync(ep.FileID, userId, now);

        // إعادة جلب البيانات بعد التقييم لضمان رجوع الحالة الصحيحة
        var createdEp = await queries.Query(_unitOfWork.EntitledPersons)
            .Include(e => e.Relationship)
            .Include(e => e.SocialStatus)
            .Include(e => e.HealthStatus)
            .Include(e => e.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(e => e.EntitledID == ep.EntitledID);

        // م27: نتيجة منفصلة وواضحة للمرفقات — المستحق محفوظ والمرفقات قابلة لإعادة الرفع من التعديل
        var epMessage = "تم إضافة المستحق بنجاح";
        if (attachmentErrors.Count > 0)
            epMessage += " — تعذر رفع بعض المرفقات: " + string.Join("؛ ", attachmentErrors);

        var epResponse = ApiResponse<EntitledPersonDto>.Success(MapToDto(createdEp ?? ep), epMessage);
        if (attachmentErrors.Count > 0)
            epResponse.Errors = attachmentErrors;
        return epResponse;
    }

    // ════════════════════════════════════════════════════════════
    // UPDATE
    // ════════════════════════════════════════════════════════════

    public async Task<ApiResponse<EntitledPersonDto>> UpdateAsync(int id, UpdateEntitledPersonDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (dto.SharePercent.HasValue && (dto.SharePercent <= 0 || dto.SharePercent > 100))
            return ApiResponse<EntitledPersonDto>.Fail("نسبة الاستحقاق يجب أن تكون أكبر من صفر ولا تتجاوز 100%");
        if (dto.EntitledAmount.HasValue && dto.EntitledAmount <= 0)
            return ApiResponse<EntitledPersonDto>.Fail("مبلغ الاستحقاق يجب أن يكون أكبر من صفر");
        if (dto.SharePercent.HasValue && dto.EntitledAmount.HasValue)
            return ApiResponse<EntitledPersonDto>.Fail("أدخل نسبة الاستحقاق أو المبلغ، وليس كليهما");

        var ep = await queries.Query(_unitOfWork.EntitledPersons).FirstOrDefaultAsync(e => e.EntitledID == id);
        if (ep == null)
            return ApiResponse<EntitledPersonDto>.Fail("المستحق غير موجود");

        // التحقق من التعارض (Optimistic Concurrency)
        if (dto.IsEligible.HasValue && dto.IsEligible.Value != ep.IsActive)
            return ApiResponse<EntitledPersonDto>.Fail("لا يمكن تغيير حالة الاستحقاق من تعديل البيانات؛ استخدم إجراء الإيقاف أو إعادة التفعيل المخصص");
        if (dto.UpdatedAt.HasValue && ep.UpdatedAt.HasValue &&
            ep.UpdatedAt.Value > dto.UpdatedAt.Value)
        {
            return ApiResponse<EntitledPersonDto>.Fail(
                "تم تعديل هذا السجل من مستخدم آخر. يرجى إعادة تحميل البيانات والمحاولة مجدداً");
        }

        if (dto.FullName != null) ep.FullName = dto.FullName;
        if (dto.Phone != null) ep.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone;
        
        // معالجة القيم الصفرية كـ Null لتجنب أخطاء المفاتيح الخارجية
        if (dto.SocialStatusID.HasValue) 
            ep.SocialStatusID = (dto.SocialStatusID.Value == 0) ? null : dto.SocialStatusID;
            
        if (dto.HealthStatusID.HasValue) 
            ep.HealthStatusID = (dto.HealthStatusID.Value == 0) ? null : dto.HealthStatusID;

        // يبقى الحقل في العقد للتوافق؛ تغيير الحالة يتطلب مسار دورة الحياة وصلاحيته.
        if (dto.HasDisabilityException.HasValue) ep.HasDisabilityException = dto.HasDisabilityException.Value;
        if (dto.RelationshipID.HasValue) ep.RelationshipID = dto.RelationshipID.Value;
        if (dto.DateOfBirth.HasValue) ep.BirthDate = dto.DateOfBirth.Value;
        if (dto.NationalID != null) ep.NationalID = dto.NationalID;

        // ── تحويل المبلغ المُدخل إلى نسبة مئوية من إجمالي معاش المتقاعد ──
        if (dto.EntitledAmount.HasValue && dto.EntitledAmount > 0)
        {
            var file = await queries.Query(_unitOfWork.RetirementFiles)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FileID == ep.FileID);

            if (file != null)
            {
                var entitlement = await queries.Query(_unitOfWork.FinancialEntitlements)
                    .AsNoTracking()
                    .Where(e => e.RetireeID == file.RetireeID && !e.IsDeleted)
                    .OrderByDescending(e => e.EffectiveDate)
                    .FirstOrDefaultAsync();

                if (entitlement == null || entitlement.Amount <= 0)
                    return ApiResponse<EntitledPersonDto>.Fail("يجب تسجيل معاش مالي للمتقاعد قبل إدخال مبلغ المستحق");
                if (dto.EntitledAmount > entitlement.Amount)
                    return ApiResponse<EntitledPersonDto>.Fail("مبلغ المستحق لا يمكن أن يتجاوز إجمالي المعاش");
                ep.SharePercent = Math.Round((dto.EntitledAmount.Value / entitlement.Amount) * 100, 2);
            }
        }
        else if (dto.SharePercent.HasValue)
        {
            ep.SharePercent = dto.SharePercent;
        }

        ep.UpdatedAt = DateTime.UtcNow;
        ep.UpdatedByUserID = userId;
        _unitOfWork.EntitledPersons.Update(ep);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // تحديث الصورة الشخصية
        if (dto.PersonalPhoto != null && dto.PersonalPhoto.Length > 0)
        {
            try
            {
                if (!string.IsNullOrEmpty(ep.PersonalPhotoPath))
                {
                    await _fileStorage.DeleteFileAsync(ep.PersonalPhotoPath);
                }
                
                using var stream = dto.PersonalPhoto.OpenReadStream();
                ep.PersonalPhotoPath = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "photos");
                
                _unitOfWork.EntitledPersons.Update(ep);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading personal photo for entitled person {Id}", ep.EntitledID);
            }
        }

        // تحديث الهوية الوطنية إذا تم إرفاق ملفات جديدة
        if (dto.NationalIdImage != null || dto.NationalIdBackImage != null)
        {
            try
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentTypeID = DocumentTypeConstants.NationalId,
                    EntitledPersonID = ep.EntitledID,
                    Notes = $"تحديث الهوية الوطنية للمستحق: {ep.FullName}",
                    File = dto.NationalIdImage!,
                    FileBack = dto.NationalIdBackImage
                };

                if (dto.NationalIdImage == null && dto.NationalIdBackImage != null)
                {
                    uploadDto.File = dto.NationalIdBackImage;
                    uploadDto.FileBack = null;
                }

                await _documentService.UploadAsync(uploadDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل في تحديث هوية المستحق");
            }
        }

        // ── إعادة تقييم حالة الاستحقاق تلقائياً ──
        await ReEvaluateEntitlementStatusAsync(ep.EntitledID);

        await _missingDocumentService.RefreshComplianceCacheAsync(ep.FileID);
        await _reviewService.RequestReviewAsync(ep.FileID, userId, now);

        // إعادة جلب البيانات بعد التحديث التلقائي للحالة
        var updatedEp = await queries.Query(_unitOfWork.EntitledPersons)
            .Include(e => e.Relationship)
            .Include(e => e.SocialStatus)
            .Include(e => e.HealthStatus)
            .Include(e => e.Guardians.Where(g => g.IsActive && !g.IsDeleted))
            .Include(e => e.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(e => e.EntitledID == id);

        return ApiResponse<EntitledPersonDto>.Success(MapToDto(updatedEp ?? ep), "تم تحديث بيانات المستحق بنجاح");
    }

    // ════════════════════════════════════════════════════════════
    // RE-EVALUATE ENTITLEMENT STATUS
    // ════════════════════════════════════════════════════════════

    public async Task<ApiResponse<bool>> ReEvaluateEntitlementStatusAsync(int id)
    {
        return await _entitlementEngine.ReEvaluateEntitlementStatusAsync(id);
    }

    // ════════════════════════════════════════════════════════════
    // DELETE
    // ════════════════════════════════════════════════════════════

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var ep = await queries.Query(_unitOfWork.EntitledPersons).FirstOrDefaultAsync(e => e.EntitledID == id);
        if (ep == null)
            return ApiResponse<bool>.Fail("المستحق غير موجود");

        await DeactivateLinkedSalaryRecipientsAsync(ep.EntitledID);
        await DeactivateLinkedAgentsAsync(ep.EntitledID);
        ep.IsDeleted = true;
        ep.DeletedAt = DateTime.UtcNow;
        ep.DeletedByUserID = deletedByUserId;

        _unitOfWork.EntitledPersons.Update(ep);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(ep.FileID);
        await _reviewService.RequestReviewAsync(ep.FileID, deletedByUserId, now);

        return ApiResponse<bool>.Success(true, "تم حذف المستحق");
    }

    // ════════════════════════════════════════════════════════════
    // STOP — إيقاف الاستحقاق
    // ════════════════════════════════════════════════════════════

    public Task<ApiResponse<bool>> StopAsync(int id, StopEntitledPersonDto dto, int userId)
        => ExecuteLifecycleAsync(id, () => StopCoreAsync(id, dto, userId));

    private async Task<ApiResponse<bool>> StopCoreAsync(int id, StopEntitledPersonDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var ep = await queries.Query(_unitOfWork.EntitledPersons)
            .Include(e => e.Relationship)
            .Include(e => e.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(e => e.EntitledID == id);

        if (ep == null)
            return ApiResponse<bool>.Fail("المستحق غير موجود");

        var decision = EntitledPersonRules.EvaluateStop(new EntitledPersonRules.StopState(
            ep.IsActive, ep.Relationship?.RelationshipName, ep.BirthDate, ep.HasDisabilityException),
            dto.StopCode, PensionMS.Core.Time.YemenTime.Today);
        if (!decision.IsAllowed)
            return ApiResponse<bool>.Fail(decision.ErrorMessage);

        // ── تحديد نوع الوثيقة المطلوبة ────────────────────────────
        var requiredDocTypeId = EntitledPersonRules.StopDocumentType(dto.StopCode);
        var uploadedIds = new List<int>();

        ep.IsActive   = false;
        ep.StopDate   = DateTime.UtcNow;
        ep.StopCode   = dto.StopCode;
        ep.StopReason = dto.Notes ?? dto.StopCode;
        ep.UpdatedAt  = DateTime.UtcNow;
        ep.UpdatedByUserID = userId;
        if (decision.SocialStatusID.HasValue)
            ep.SocialStatusID = decision.SocialStatusID.Value;

        _unitOfWork.EntitledPersons.Update(ep);

        // تعطيل SalaryRecipient المرتبط تلقائياً
        await DeactivateLinkedSalaryRecipientsAsync(ep.EntitledID);

        // Q4: تعطيل الوكيل المرتبط تلقائياً عند توقف المستحق
        await DeactivateLinkedAgentsAsync(ep.EntitledID);

        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // ── حفظ الوثيقة المرفقة أو تسجيل نقص ─────────────────────
        if (dto.Document != null)
        {
            if (requiredDocTypeId is int docTypeId)
            {
                var uploaded = await UploadLifecycleProofAsync(ep, docTypeId, dto.Document, userId, uploadedIds);
                if (!uploaded.IsSuccess) return uploaded;
            }
            await _unitOfWork.SaveChangesAsync();
        }
        else if (requiredDocTypeId is int missingDocTypeId && !HasProof(ep, missingDocTypeId))
        {
            // ── تسجيل نقص للوثيقة المطلوبة ────────────────────────
            await _unitOfWork.MissingDocuments.AddAsync(new MissingDocument
            {
                FileID = ep.FileID,
                DocumentTypeID = missingDocTypeId,
                EntitledID = ep.EntitledID,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = $"وثيقة إسقاط المستحق ({dto.StopCode}): {ep.EntitledID}"
            });
            await _unitOfWork.SaveChangesAsync();
        }

        var completed = await CompleteLifecycleAsync(ep, userId, now, uploadedIds);
        if (!completed.IsSuccess) return completed;
        return ApiResponse<bool>.Success(true, "تم إيقاف الاستحقاق");
    }

    // ════════════════════════════════════════════════════════════
    // REACTIVATE — إعادة تفعيل الاستحقاق
    // ════════════════════════════════════════════════════════════

    public Task<ApiResponse<bool>> ReactivateAsync(int id, ReactivateEntitledPersonDto dto, int userId)
        => ExecuteLifecycleAsync(id, () => ReactivateCoreAsync(id, dto, userId));

    private async Task<ApiResponse<bool>> ReactivateCoreAsync(int id, ReactivateEntitledPersonDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var ep = await queries.Query(_unitOfWork.EntitledPersons)
            .Include(e => e.Relationship)
            .Include(e => e.RetirementFile).ThenInclude(f => f.Retiree)
            .Include(e => e.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(e => e.EntitledID == id);

        if (ep == null)
            return ApiResponse<bool>.Fail("المستحق غير موجود");

        var decision = EntitledPersonRules.EvaluateReactivation(new EntitledPersonRules.ReactivationState(
            ep.IsActive, ep.IsDeceased, ep.StopCode, ep.Relationship?.RelationshipName, ep.BirthDate,
            ep.SocialStatusID, ep.HasDisabilityException, ep.RetirementFile?.Retiree?.IsAlive),
            dto.ReactivationCode, PensionMS.Core.Time.YemenTime.Today);
        if (!decision.IsAllowed)
            return ApiResponse<bool>.Fail(decision.ErrorMessage);

        var requiredDocuments = EntitledPersonRules.ReactivationDocumentRequirements(dto.ReactivationCode);
        var uploadedIds = new List<int>();
        ep.SocialStatusID = decision.SocialStatusID;

        // ── تفعيل المستحق ─────────────────────────────────────────
        ep.IsActive   = true;
        ep.StopDate   = null;
        ep.StopCode   = null;
        ep.StopReason = null;
        ep.UpdatedAt  = DateTime.UtcNow;
        ep.UpdatedByUserID = userId;

        ep.HasDisabilityException = decision.HasDisabilityException;

        _unitOfWork.EntitledPersons.Update(ep);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // ── حفظ الوثائق المرفقة ───────────────────────────────────
        if (dto.Document != null)
        {
            if (requiredDocuments.DocumentTypeID is int docTypeId)
            {
                var uploaded = await UploadLifecycleProofAsync(ep, docTypeId, dto.Document, userId, uploadedIds);
                if (!uploaded.IsSuccess) return uploaded;
            }
        }

        if (dto.CivilServiceProof != null && requiredDocuments.CivilServiceProofTypeID is int civilServiceTypeId)
        {
            var uploaded = await UploadLifecycleProofAsync(ep, civilServiceTypeId, dto.CivilServiceProof, userId, uploadedIds);
            if (!uploaded.IsSuccess) return uploaded;
        }

        await _unitOfWork.SaveChangesAsync();

        // ── تسجيل النواقص للوثائق غير المرفقة ─────────────────────
        var missingDocs = new List<MissingDocument>();

        if (dto.Document == null && requiredDocuments.DocumentTypeID is int missingTypeId && !HasProof(ep, missingTypeId))
        {
            missingDocs.Add(new MissingDocument
            {
                FileID = ep.FileID,
                DocumentTypeID = missingTypeId,
                EntitledID = ep.EntitledID,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = $"وثيقة إعادة تفعيل المستحق ({dto.ReactivationCode}): {ep.EntitledID}"
            });
        }

        if (dto.CivilServiceProof == null && requiredDocuments.CivilServiceProofTypeID is int missingCivilServiceTypeId && !HasProof(ep, missingCivilServiceTypeId))
        {
            missingDocs.Add(new MissingDocument
            {
                FileID = ep.FileID,
                DocumentTypeID = missingCivilServiceTypeId,
                EntitledID = ep.EntitledID,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = $"إثبات خدمة مدنية لإعادة تفعيل المستحق: {ep.EntitledID}"
            });
        }

        if (missingDocs.Count > 0)
        {
            foreach (var md in missingDocs)
                await _unitOfWork.MissingDocuments.AddAsync(md);
            await _unitOfWork.SaveChangesAsync();
        }

        var completed = await CompleteLifecycleAsync(ep, userId, now, uploadedIds);
        if (!completed.IsSuccess) return completed;

        var reasonLabel = dto.ReactivationCode switch
        {
            ReactivationCodes.Divorced         => "الطلاق",
            ReactivationCodes.HusbandDeceased  => "وفاة الزوج",
            ReactivationCodes.DisabilityReport => "التقرير الطبي (إعاقة/مرض نفسي)",
            ReactivationCodes.AdminDecision    => "انتهاء القرار الإداري",
            _ => dto.ReactivationCode
        };
        return ApiResponse<bool>.Success(true, $"تم إعادة تفعيل الاستحقاق — السبب: {reasonLabel}");
    }

    // ════════════════════════════════════════════════════════════
    // RECORD DEATH — تسجيل وفاة المستحق
    // ════════════════════════════════════════════════════════════

    public Task<ApiResponse<bool>> RecordDeathAsync(int id, RecordDeathDto dto, int userId)
        => ExecuteLifecycleAsync(id, () => RecordDeathCoreAsync(id, dto, userId));

    private async Task<ApiResponse<bool>> RecordDeathCoreAsync(int id, RecordDeathDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var ep = await queries.Query(_unitOfWork.EntitledPersons).FirstOrDefaultAsync(e => e.EntitledID == id);
        if (ep == null)
            return ApiResponse<bool>.Fail("المستحق غير موجود");

        if (ep.IsDeceased)
            return ApiResponse<bool>.Fail("المستحق مُسجَّل متوفى مسبقاً");

        if (dto.DeathCertificate == null)
            return ApiResponse<bool>.Fail("يجب رفع شهادة الوفاة");
        if (dto.DeathDate == default || dto.DeathDate > PensionMS.Core.Time.YemenTime.Today || dto.DeathDate < ep.BirthDate)
            return ApiResponse<bool>.Fail("تاريخ الوفاة غير صحيح");

        ep.IsDeceased   = true;
        ep.DeceasedDate = dto.DeathDate;
        ep.IsActive     = false;
        ep.StopDate     = PensionMS.Core.Time.YemenTime.StartUtc(dto.DeathDate);
        ep.StopCode     = EntitledStopCodes.Deceased;
        ep.StopReason   = "وفاة المستحق";
        ep.UpdatedAt    = DateTime.UtcNow;
        ep.UpdatedByUserID = userId;

        _unitOfWork.EntitledPersons.Update(ep);

        // تعطيل SalaryRecipient تلقائياً
        await DeactivateLinkedSalaryRecipientsAsync(ep.EntitledID);

        // Q4: تعطيل الوكيل المرتبط تلقائياً عند وفاة المستحق
        await DeactivateLinkedAgentsAsync(ep.EntitledID);

        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // حفظ شهادة الوفاة
        var uploadedIds = new List<int>();
        var uploaded = await UploadLifecycleProofAsync(ep, DocumentTypeConstants.DeathCertificate, dto.DeathCertificate, userId, uploadedIds);
        if (!uploaded.IsSuccess) return uploaded;
        await _unitOfWork.SaveChangesAsync();

        var completed = await CompleteLifecycleAsync(ep, userId, now, uploadedIds);
        if (!completed.IsSuccess) return completed;

        return ApiResponse<bool>.Success(true, "تم تسجيل وفاة المستحق وإيقاف صرفه");
    }

    public Task<ApiResponse<bool>> ManualActivateAsync(int id, ManualActivateEntitledPersonDto dto, int userId)
        => ExecuteLifecycleAsync(id, () => ManualActivateCoreAsync(id, dto, userId));

    private async Task<ApiResponse<bool>> ManualActivateCoreAsync(int id, ManualActivateEntitledPersonDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        // جلب المستحق مع التأكد من وجوده وحالته
        var ep = await queries.Query(_unitOfWork.EntitledPersons)
            .Include(e => e.RetirementFile)
            .FirstOrDefaultAsync(e => e.EntitledID == id);

        if (ep == null) return ApiResponse<bool>.Fail("المستحق غير موجود");
        if (ep.IsDeceased || ep.IsDeleted) return ApiResponse<bool>.Fail("لا يمكن تفعيل مستحق متوفى أو محذوف");

        // 1. التحقق من شروط الاستحقاق القانونية الأساسية (بدون وثائق)
        var coreResult = await _entitlementEngine.CheckCoreEligibilityAsync(id);
        if (!coreResult.IsSuccess)
        {
            return ApiResponse<bool>.Fail($"لا يمكن التفعيل اليدوي: {coreResult.Message}");
        }

        // 2. تحديث حالة المستحق (EF سيقوم بتعقب التغييرات تلقائياً)
        ep.IsActive = true;
        ep.StopCode = null;
        ep.StopReason = null;
        ep.StopDate = null;
        ep.UpdatedAt = DateTime.UtcNow;
        ep.UpdatedByUserID = userId;

        // 3. معالجة النواقص وتحويلها لتعهدات إذا طلب ذلك
        if (dto.CreatePledgesForMissingDocs)
        {
            // جلب النواقص المسجلة يدوياً
            var manualMissing = await queries.Query(_unitOfWork.MissingDocuments)
                .Where(m => m.EntitledID == id && m.Status == MissingDocumentStatusConstants.Pending)
                .ToListAsync();

            // جلب النواقص المرصودة آلياً (لتحويلها لسجلات رسمية متعهد بها)
            var autoResult = await _missingDocumentService.GetAutoDetectedByFileAsync(ep.FileID);
            var epAutoMissing = autoResult.Data?
                .Where(a => a.EntityType == EntityTypeConstants.EntitledPerson && a.EntitledID == ep.EntitledID)
                .ToList();

            if (manualMissing.Any() || (epAutoMissing != null && epAutoMissing.Any()))
            {
                // إنشاء سجل التعهد
                var pledge = new DocumentPledge
                {
                    FileID = ep.FileID,
                    CreatedByUserID = userId,
                    PledgeNumber = $"PL-{ep.EntitledID}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}",
                    PledgeDate = DateTime.UtcNow,
                    Deadline = DateTime.UtcNow.AddMonths(dto.PledgeDeadlineMonths ?? 3),
                    CreatedAt = DateTime.UtcNow,
                    Status = MissingDocumentStatusConstants.Pending,
                    PledgedByName = ep.FullName,
                    PledgedByType = "EntitledPerson",
                    Reason = dto.Reason ?? "تفعيل يدوي للمستحق مع الالتزام باستكمال النواقص"
                };
                
                await _unitOfWork.DocumentPledges.AddAsync(pledge);
                // نحتاج لحفظ التعهد هنا للحصول على الـ ID لاستخدامه في ربط النواقص
                await _unitOfWork.SaveChangesAsync(); 

                // أ) ربط النواقص اليدوية بالتعهد
                foreach (var md in manualMissing)
                {
                    md.Status = MissingDocumentStatusConstants.Pledged;
                    md.PledgeID = pledge.PledgeID;
                }

                // ب) إنشاء سجلات للنواقص الآلية وربطها بالتعهد
                if (epAutoMissing != null)
                {
                    foreach (var am in epAutoMissing)
                    {
                        // منع التكرار إذا كان النقص مسجلاً يدوياً مسبقاً
                        if (manualMissing.Any(m => m.DocumentTypeID == am.DocumentTypeID)) continue;

                        var newMd = new MissingDocument
                        {
                            FileID = ep.FileID,
                            EntitledID = ep.EntitledID,
                            DocumentTypeID = am.DocumentTypeID,
                            Status = MissingDocumentStatusConstants.Pledged,
                            PledgeID = pledge.PledgeID,
                            CreatedAt = DateTime.UtcNow,
                            Notes = "نقص مرصود آلياً تم التعهد به عند التفعيل اليدوي"
                        };
                        await _unitOfWork.MissingDocuments.AddAsync(newMd);
                    }
                }
            }
        }

        // 4. حفظ كافة التغييرات النهائية
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var completed = await CompleteLifecycleAsync(ep, userId, now, []);
        if (!completed.IsSuccess) return completed;

        return ApiResponse<bool>.Success(true, "تم تفعيل المستحق يدوياً بنجاح");
    }

    // ════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════

    private Task<ApiResponse<bool>> ExecuteLifecycleAsync(int id, Func<Task<ApiResponse<bool>>> action)
        => UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            var queries = await _access.ForCurrentOperationAsync();
            var fileId = await queries.Query(_unitOfWork.EntitledPersons).Where(e => e.EntitledID == id)
                .Select(e => (int?)e.FileID).FirstOrDefaultAsync();
            if (!fileId.HasValue) return ApiResponse<bool>.Fail("المستحق غير موجود");
            await _unitOfWork.LockFileReviewAsync(fileId.Value);
            return await action();
        });

    private static bool HasProof(EntitledPerson person, int type)
    {
        var owner = new RetirementDocumentRequirement(type, null, null, null, person.EntitledID, null, null);
        return RetirementFileDocumentRules.FindValidDocument(person.Documents.Where(d => d.FileID == person.FileID),
            owner, PensionMS.Core.Time.YemenTime.Today) != null;
    }

    private async Task<ApiResponse<bool>> UploadLifecycleProofAsync(EntitledPerson person, int type,
        IRemoteFile proof, int userId, List<int> uploadedIds)
    {
        try
        {
            var result = await _documentService.UploadAsync(new UploadDocumentDto
            {
                DocumentTypeID = type, EntitledPersonID = person.EntitledID, File = proof
            }, userId, deferPostProcessing: true);
            if (!result.IsSuccess) return ApiResponse<bool>.Fail(result.Message ?? "تعذر حفظ وثيقة العملية");
            if (result.Data?.DocumentID > 0) uploadedIds.Add(result.Data.DocumentID);
        }
        catch (PensionMS.Core.Exceptions.DuplicateDocumentException ex)
        {
            if (ex.Document.EntitledID != person.EntitledID || ex.Document.FileID != person.FileID || ex.Document.DocumentTypeID != type)
                throw;
            uploadedIds.Add(ex.Document.DocumentID);
        }
        return ApiResponse<bool>.Success(true);
    }

    private async Task<ApiResponse<bool>> CompleteLifecycleAsync(EntitledPerson person, int userId,
        DateTime changedAt, List<int> uploadedIds)
    {
        if (uploadedIds.Count > 0)
        {
            var refreshed = await _documentService.RefreshAfterLifecycleUploadAsync(person.FileID,
                uploadedIds.Distinct().ToList(), userId, changedAt);
            return refreshed.IsSuccess ? ApiResponse<bool>.Success(true) : ApiResponse<bool>.Fail(refreshed.Message);
        }
        await _missingDocumentService.RefreshComplianceCacheAsync(person.FileID);
        return await _reviewService.RequestReviewAsync(person.FileID, userId, changedAt);
    }

    private async Task DeactivateLinkedSalaryRecipientsAsync(int entitledId)
    {
        await PensionMS.Infrastructure.Helpers.RecipientScope.EndForEntitledAsync(_unitOfWork, entitledId);
    }

    private async Task DeactivateLinkedAgentsAsync(int entitledId)
    {
        var agents = await _unitOfWork.Agents.Query()
            .Where(a => a.EntitledID == entitledId && a.IsActive && !a.IsDeleted)
            .ToListAsync();

        foreach (var a in agents)
        {
            a.IsActive = false;
            a.UpdatedAt = DateTime.UtcNow;
        }
    }

    private EntitledPersonDto MapToDto(EntitledPerson e)
    {
        var ageYears = (int)(PensionMS.Core.Time.YemenTime.Age(e.BirthDate));
        var owner = new RetirementDocumentRequirement(DocumentTypeConstants.NationalId, null, null, null,
            e.EntitledID, null, null);
        var nationalIdDoc = RetirementFileDocumentRules.FindValidDocument(
            e.Documents.Where(d => d.FileID == e.FileID), owner, PensionMS.Core.Time.YemenTime.Today);

        return new EntitledPersonDto
        {
            EntitledID             = e.EntitledID,
            FileID                 = e.FileID,
            FullName               = e.FullName,
            NationalID             = e.NationalID,
            RelationshipID         = e.RelationshipID,
            RelationshipName       = e.Relationship?.RelationshipName,
            DateOfBirth            = e.BirthDate,
            AgeYears               = ageYears,
            Phone                  = e.Phone,
            SocialStatusID         = e.SocialStatusID,
            SocialStatusName       = e.SocialStatus?.StatusName,
            HealthStatusID         = e.HealthStatusID,
            HealthStatusName       = e.HealthStatus?.HealthStatusName,
            IsEligible             = e.IsActive,
            IsDeceased             = e.IsDeceased,
            StopDate               = e.StopDate,
            StopReason             = e.StopReason,
            StopCode               = e.StopCode,
            HasDisabilityException = e.HasDisabilityException,
            HasActiveGuardian      = e.Guardians.Any(g => g.IsActive && !g.IsDeleted),
            HasNationalIdDocument  = nationalIdDoc != null,
            HasPersonalPhoto       = !string.IsNullOrEmpty(e.PersonalPhotoPath),
            PersonalPhotoPath      = e.PersonalPhotoPath,
            NationalIdImageUrl     = nationalIdDoc != null ? _fileStorage.GetFileUrl(nationalIdDoc.FilePath) : null,
            PendingReviewStatus    = (int)e.PendingReviewStatus,
            UpdatedAt              = e.UpdatedAt,
            SharePercent           = e.SharePercent
        };
    }

    public async Task<ApiResponse<List<EntitledPersonSearchResultDto>>> SearchAsync(string query, int limit = 10)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return ApiResponse<List<EntitledPersonSearchResultDto>>.Success(new());

        var searchTerm = query.Trim().ToLower();

        var persons = await queries.Query(_unitOfWork.EntitledPersons)
            .AsNoTracking()
            .Include(e => e.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Where(e => !e.IsDeleted && (
                e.FullName.ToLower().Contains(searchTerm) ||
                e.NationalID.Contains(searchTerm) ||
                (e.Phone != null && e.Phone.Contains(searchTerm)) ||
                (e.RetirementFile != null && e.RetirementFile.FileNumber.ToLower().Contains(searchTerm)) ||
                (e.RetirementFile != null && e.RetirementFile.Retiree != null &&
                 e.RetirementFile.Retiree.MilitaryNumber != null &&
                 e.RetirementFile.Retiree.MilitaryNumber.Contains(searchTerm))
            ))
            .OrderByDescending(e => e.IsActive)
            .ThenByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync();

        var results = persons.Select(e => new EntitledPersonSearchResultDto
        {
            EntitledID = e.EntitledID,
            FileID = e.FileID,
            FullName = e.FullName,
            NationalID = e.NationalID,
            Phone = e.Phone,
            RelationshipName = e.Relationship?.RelationshipName,
            DateOfBirth = e.BirthDate,
            IsActive = e.IsActive,
            IsDeceased = e.IsDeceased,
            PersonalPhotoPath = e.PersonalPhotoPath,
            FileNumber = e.RetirementFile?.FileNumber ?? "",
            RetireeMilitaryNumber = e.RetirementFile?.Retiree?.MilitaryNumber
        }).ToList();

        return ApiResponse<List<EntitledPersonSearchResultDto>>.Success(results);
    }
}
