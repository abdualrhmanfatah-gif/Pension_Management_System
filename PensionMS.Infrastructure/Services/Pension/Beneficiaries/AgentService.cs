using PensionMS.Infrastructure.Services.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Agents;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Helpers;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;

namespace PensionMS.Infrastructure.Services.Pension.Beneficiaries;

public class AgentService : IAgentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IDocumentService _documentService;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly IFileReviewService _reviewService;
    private readonly ILogger<AgentService> _logger;
    private readonly IAccessiblePensionRepository _access;

    public AgentService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, IDocumentService documentService, IMissingDocumentService missingDocumentService, IFileReviewService reviewService, ILogger<AgentService> logger, IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _documentService = documentService;
        _missingDocumentService = missingDocumentService;
        _reviewService = reviewService;
        _logger = logger;
        _access = access;
    }

    private async Task<IQueryable<Agent>> AgentsAsync(string permission = "Agent.View")
    {
        var fileIds = (await _access.ForPermissionAsync(permission)).Files().Select(f => f.FileID);
        return _unitOfWork.Agents.Query().Where(a => fileIds.Contains(a.FileID) && !a.IsDeleted);
    }

    private Task<ApiResponse<T>> WithinFileAsync<T>(int fileId, Func<Task<ApiResponse<T>>> operation, string permission) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            await _unitOfWork.LockFileReviewAsync(fileId);
            var file = await (await _access.ForPermissionAsync(permission)).Files().FirstOrDefaultAsync(f => f.FileID == fileId);
            if (file == null) return ApiResponse<T>.Fail("الملف غير موجود أو خارج نطاق الفرع");
            if (!RetirementFileActivationValidator.IsEditable(file)) return ApiResponse<T>.Fail("حالة الملف لا تسمح بتعديل الوكالة");
            return await operation();
        });

    private async Task<ApiResponse<T>> ChangeAgentAsync<T>(int id, Func<Task<ApiResponse<T>>> operation, string permission)
    {
        var fileId = await (await AgentsAsync(permission)).Where(a => a.AgentID == id).Select(a => (int?)a.FileID).FirstOrDefaultAsync();
        return fileId.HasValue ? await WithinFileAsync(fileId.Value, operation, permission) : ApiResponse<T>.Fail("الوكيل غير موجود");
    }

    public Task<ApiResponse<AgentDto>> CreateAsync(CreateAgentDto dto, int userId) =>
        WithinFileAsync(dto.FileID, () => CreateCoreAsync(dto, userId), "Agent.Add");

    public Task<ApiResponse<AgentDto>> UpdateAsync(int id, UpdateAgentDto dto, int userId) =>
        ChangeAgentAsync(id, () => UpdateCoreAsync(id, dto, userId), "Agent.Edit");

    public Task<ApiResponse<bool>> DeactivateAsync(int id, int userId) =>
        ChangeAgentAsync(id, () => DeactivateCoreAsync(id, userId), "Agent.Delete");

    public Task<ApiResponse<bool>> DeleteAsync(int id, int userId) =>
        ChangeAgentAsync(id, () => DeleteCoreAsync(id, userId), "Agent.Delete");

    public async Task<ApiResponse<List<AgentDto>>> GetByFileIdAsync(int fileId)
    {
        var agents = await (await AgentsAsync())
            .AsNoTracking()
            .Include(a => a.RetirementFile)
            .Include(a => a.EntitledPerson)
            .Where(a => a.FileID == fileId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<AgentDto>>.Success(agents.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<AgentDto>> GetByIdAsync(int id)
    {
        var agent = await (await AgentsAsync())
            .AsNoTracking()
            .Include(a => a.RetirementFile)
            .Include(a => a.EntitledPerson)
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<AgentDto>.Fail("الوكيل غير موجود");

        return ApiResponse<AgentDto>.Success(MapToDto(agent));
    }

    private async Task<ApiResponse<AgentDto>> CreateCoreAsync(CreateAgentDto dto, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (file == null)
            return ApiResponse<AgentDto>.Fail("الملف غير موجود");

        if (!ValidScope((AgentScope?)dto.AgencyScope, dto.EntitledID))
            return ApiResponse<AgentDto>.Fail("حدد نطاق وكالة صحيحًا، واختر المستحق عند اختيار وكالة عن مستحق معين");

        // ── التحقق من المستحق (إن وُجد) ──────────────────────
        if (dto.EntitledID.HasValue)
        {
            var entitled = await _unitOfWork.EntitledPersons.GetByIdAsync(dto.EntitledID.Value);
            if (entitled == null || entitled.FileID != dto.FileID || entitled.IsDeleted || entitled.IsDeceased)
                return ApiResponse<AgentDto>.Fail("المستحق غير موجود أو لا ينتمي لهذا الملف");

            // Q5: يُمنع الجمع وصي + وكيل لنفس المستحق
            var hasActiveGuardian = await _unitOfWork.GuardianEntitledPersons.Query()
                .AnyAsync(ge => ge.EntitledID == dto.EntitledID && ge.IsActive);
            if (hasActiveGuardian)
                return ApiResponse<AgentDto>.Fail("لا يمكن إضافة وكيل لهذا المستحق لوجود وصي نشط. يُجب إنهاء الوصاية أولاً.");
        }

        // ── قيد النطاق: وكيل واحد لكل نطاق ────────────────────
        var activeAgentExists = await _unitOfWork.Agents.Query()
            .AnyAsync(a => a.FileID == dto.FileID
                         && a.EntitledID == dto.EntitledID
                         && a.IsActive && !a.IsDeleted);
        if (activeAgentExists)
        {
            var msg = dto.EntitledID.HasValue
                ? "يوجد وكيل نشط بالفعل لهذا المستحق في هذا الملف."
                : "يوجد وكيل عام نشط بالفعل لهذا الملف. يُرجى إلغاء تفعيله أولاً.";
            return ApiResponse<AgentDto>.Fail(msg);
        }

        // Validate agency fields when agency exists
        var hasAgency = !string.IsNullOrWhiteSpace(dto.AgencyNumber);
        if (hasAgency)
        {
            if (dto.AgencyDate == null || dto.AgencyDate.Value == default)
                return ApiResponse<AgentDto>.Fail("تاريخ الوكالة مطلوب عند وجود وكالة");
        }

        // Save personal photo if provided
        if (dto.AgencyEndDate.HasValue && (!dto.AgencyDate.HasValue || dto.AgencyEndDate < dto.AgencyDate))
            return ApiResponse<AgentDto>.Fail("تاريخ انتهاء الوكالة يجب ألا يسبق تاريخ إصدارها");
        if (string.IsNullOrWhiteSpace(dto.FullName) || dto.FullName.Trim().Length < 3 || string.IsNullOrWhiteSpace(dto.NationalID))
            return ApiResponse<AgentDto>.Fail("اسم الوكيل ورقم هويته مطلوبان");
        string? personalPhotoPath = null;
        if (dto.PersonalPhoto != null && dto.PersonalPhoto.Length > 0)
        {
            try
            {
                using var stream = dto.PersonalPhoto.OpenReadStream();
                personalPhotoPath = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "photos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving agent photo");
                return ApiResponse<AgentDto>.Fail("فشل في حفظ الصورة الشخصية. يرجى المحاولة لاحقاً");
            }
        }

        var agent = new Agent
        {
            FileID = dto.FileID,
            EntitledID = dto.EntitledID,
            AgencyScope = (AgentScope?)dto.AgencyScope,
            FullName = dto.FullName,
            NationalID = dto.NationalID,
            PersonalPhotoPath = personalPhotoPath,
            Phone = dto.Phone,
            AgencyNumber = dto.AgencyNumber,
            AgencyDate = dto.AgencyDate,
            AgencyEndDate = dto.AgencyEndDate,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserID = userId
        };

        await _unitOfWork.Agents.AddAsync(agent);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // م27: أخطاء المرفقات تُجمع وتظهر في الاستجابة بدلاً من إخفائها
        var attachmentErrors = new List<string>();

        // Save national ID document (front and back) if provided
        if (dto.NationalIDDocument != null || dto.NationalIDBackDocument != null)
        {
            try
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentTypeID = DocumentTypeConstants.NationalId, // الهوية الوطنية
                    AgentID = agent.AgentID,
                    Notes = $"الهوية الوطنية للوكيل: {dto.FullName} - رقم الهوية: {dto.NationalID}",
                    File = dto.NationalIDDocument!,
                    FileBack = dto.NationalIDBackDocument
                };

                // If front is null but back exists, we still want to save the back (which UploadAsync handles)
                // Wait, if front is null, dto.File will be null. But DocumentService.UploadAsync checks if dto.File is null.
                if (dto.NationalIDDocument == null && dto.NationalIDBackDocument != null)
                {
                    uploadDto.File = dto.NationalIDBackDocument;
                    uploadDto.FileBack = null;
                }

                var result = await _documentService.UploadAsync(uploadDto, userId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("فشل في رفع الهوية للوكيل: {Message}", result.Message);
                    attachmentErrors.Add($"الهوية الوطنية: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل غير متوقع عند رفع هوية الوكيل");
                attachmentErrors.Add("الهوية الوطنية: تعذر الرفع بشكل غير متوقع");
            }
        }

        // Save agency document if provided
        if (hasAgency && dto.AgencyDocument != null && dto.AgencyDocument.Length > 0)
        {
            var uploadDto = new UploadDocumentDto
            {
                File = dto.AgencyDocument,
                DocumentTypeID = DocumentTypeConstants.AgencyDocument, // وثيقة الوكالة
                AgentID = agent.AgentID,
                Notes = $"وثيقة الوكالة للوكيل: {dto.FullName} - رقم الوكالة: {dto.AgencyNumber ?? ""}"
            };

            try
            {
                var docResult = await _documentService.UploadAsync(uploadDto, userId);
                if (!docResult.IsSuccess)
                {
                    _logger.LogWarning("فشل في رفع وثيقة الوكالة للوكيل: {Message}", docResult.Message);
                    attachmentErrors.Add($"وثيقة الوكالة: {docResult.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل غير متوقع عند رفع وثيقة الوكالة للوكيل");
                attachmentErrors.Add("وثيقة الوكالة: تعذر الرفع بشكل غير متوقع");
            }
        }

        // ── تسجيل النواقص تلقائياً ──────────────────────────────
        var missingDocs = new List<MissingDocument>();

        if ((dto.NationalIDDocument == null || dto.NationalIDDocument.Length == 0) &&
            (dto.NationalIDBackDocument == null || dto.NationalIDBackDocument.Length == 0))
        {
            missingDocs.Add(new MissingDocument
            {
                FileID = dto.FileID,
                DocumentTypeID = DocumentTypeConstants.NationalId, // الهوية الوطنية
                AgentID = agent.AgentID,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = $"هوية الوكيل: {dto.FullName}"
            });
        }

        if (hasAgency && (dto.AgencyDocument == null || dto.AgencyDocument.Length == 0))
        {
            missingDocs.Add(new MissingDocument
            {
                FileID = dto.FileID,
                DocumentTypeID = DocumentTypeConstants.AgencyDocument, // وثيقة الوكالة
                AgentID = agent.AgentID,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = $"وثيقة وكالة الوكيل: {dto.FullName}"
            });
        }

        if (missingDocs.Count > 0)
        {
            foreach (var md in missingDocs)
                await _unitOfWork.MissingDocuments.AddAsync(md);
            await _unitOfWork.SaveChangesAsync();
        }

        var created = await _unitOfWork.Agents.Query()
            .Include(a => a.RetirementFile)
            .Include(a => a.EntitledPerson)
            .FirstAsync(a => a.AgentID == agent.AgentID);

        await _missingDocumentService.RefreshComplianceCacheAsync(agent.FileID);
        var review = await _reviewService.RequestReviewAsync(agent.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<AgentDto>.Fail(review.Message ?? "تعذر طلب مراجعة الوكالة");

        // م27: نتيجة منفصلة وواضحة للمرفقات — الوكيل محفوظ والمرفقات قابلة لإعادة الرفع من التعديل
        var agentMessage = "تم إنشاء الوكيل بنجاح";
        if (attachmentErrors.Count > 0)
            agentMessage += " — تعذر رفع بعض المرفقات: " + string.Join("؛ ", attachmentErrors);

        var agentResponse = ApiResponse<AgentDto>.Success(MapToDto(created), agentMessage);
        if (attachmentErrors.Count > 0)
            agentResponse.Errors = attachmentErrors;
        return agentResponse;
    }

    private async Task<ApiResponse<AgentDto>> UpdateCoreAsync(int id, UpdateAgentDto dto, int userId)
    {
        var agent = await _unitOfWork.Agents.Query()
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<AgentDto>.Fail("الوكيل غير موجود");

        var targetEntitledId = dto.UpdateEntitledScope ? dto.EntitledID : agent.EntitledID;
        var targetScope = dto.UpdateEntitledScope ? (AgentScope?)dto.AgencyScope : agent.AgencyScope;
        if (!ValidScope(targetScope, targetEntitledId))
            return ApiResponse<AgentDto>.Fail("حدد نطاق وكالة صحيحًا، واختر المستحق عند اختيار وكالة عن مستحق معين");
        if (targetEntitledId != agent.EntitledID || targetScope != agent.AgencyScope)
        {
            if (await _unitOfWork.SalaryRecipients.Query().IgnoreQueryFilters().AnyAsync(r => r.AgentID == id))
                return ApiResponse<AgentDto>.Fail("لا يمكن نقل وكالة سبق استخدامها في الصرف؛ أنه الوكالة وسجل وكالة جديدة");
            if (targetEntitledId.HasValue)
            {
                var person = await _unitOfWork.EntitledPersons.GetByIdAsync(targetEntitledId.Value);
                if (person == null || person.FileID != agent.FileID || person.IsDeleted || person.IsDeceased)
                    return ApiResponse<AgentDto>.Fail("المستحق غير موجود أو لا ينتمي لهذا الملف");
                if (await _unitOfWork.GuardianEntitledPersons.Query().AnyAsync(g => g.EntitledID == targetEntitledId && g.IsActive))
                    return ApiResponse<AgentDto>.Fail("المستحق لديه وصي نشط");
            }
            if (await _unitOfWork.Agents.Query().AnyAsync(a => a.FileID == agent.FileID && a.EntitledID == targetEntitledId && a.AgentID != id && a.IsActive && !a.IsDeleted))
                return ApiResponse<AgentDto>.Fail("يوجد وكيل نشط بالفعل لهذا النطاق");
        }
        if (string.IsNullOrWhiteSpace(dto.FullName) || dto.FullName.Trim().Length < 3 || string.IsNullOrWhiteSpace(dto.NationalID))
            return ApiResponse<AgentDto>.Fail("اسم الوكيل ورقم هويته مطلوبان");
        if (dto.AgencyEndDate.HasValue && (!dto.AgencyDate.HasValue || dto.AgencyEndDate < dto.AgencyDate))
            return ApiResponse<AgentDto>.Fail("تاريخ انتهاء الوكالة يجب ألا يسبق تاريخ إصدارها");

        // التحقق من التعارض (Optimistic Concurrency)
        if (dto.UpdatedAt.HasValue && agent.UpdatedAt.HasValue &&
            agent.UpdatedAt.Value > dto.UpdatedAt.Value)
        {
            return ApiResponse<AgentDto>.Fail(
                "تم تعديل هذا السجل من مستخدم آخر. يرجى إعادة تحميل البيانات والمحاولة مجدداً");
        }

        // Update personal photo if provided
        if (dto.PersonalPhoto != null && dto.PersonalPhoto.Length > 0)
        {
            // Keep the old file available to review history if the replacement fails.
            // Save new photo
            try
            {
                using var stream = dto.PersonalPhoto.OpenReadStream();
                agent.PersonalPhotoPath = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "photos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving agent photo during update");
                return ApiResponse<AgentDto>.Fail("فشل في حفظ الصورة الشخصية. يرجى المحاولة لاحقاً");
            }
        }

        // Validate agency fields when agency exists
        var hasAgencyOnUpdate = !string.IsNullOrWhiteSpace(dto.AgencyNumber);
        if (hasAgencyOnUpdate)
        {
            if (dto.AgencyDate == null || dto.AgencyDate.Value == default)
                return ApiResponse<AgentDto>.Fail("تاريخ الوكالة مطلوب عند وجود وكالة");
        }

        agent.FullName = dto.FullName;
        agent.NationalID = dto.NationalID;
        agent.Phone = dto.Phone;
        agent.AgencyNumber = dto.AgencyNumber;
        agent.AgencyDate = dto.AgencyDate;
        agent.AgencyEndDate = dto.AgencyEndDate;
        agent.Notes = dto.Notes;
        agent.UpdatedAt = DateTime.UtcNow;
        agent.UpdatedByUserID = userId;

        agent.EntitledID = targetEntitledId;
        agent.AgencyScope = targetScope;

        _unitOfWork.Agents.Update(agent);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Save national ID document (front/back) if provided (new document)
        if (dto.NationalIDDocument != null || dto.NationalIDBackDocument != null)
        {
            try
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentTypeID = DocumentTypeConstants.NationalId, // الهوية الوطنية
                    AgentID = agent.AgentID,
                    Notes = $"الهوية الوطنية للوكيل: {dto.FullName} - رقم الهوية: {dto.NationalID}",
                    File = dto.NationalIDDocument!,
                    FileBack = dto.NationalIDBackDocument
                };

                if (dto.NationalIDDocument == null && dto.NationalIDBackDocument != null)
                {
                    uploadDto.File = dto.NationalIDBackDocument;
                    uploadDto.FileBack = null;
                }

                var result = await _documentService.UploadAsync(uploadDto, userId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("فشل في رفع الهوية للوكيل (تحديث): {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل غير متوقع عند رفع هوية الوكيل (تحديث)");
            }
        }

        // Save agency document if provided (new document)
        if (hasAgencyOnUpdate && dto.AgencyDocument != null && dto.AgencyDocument.Length > 0)
        {
            var uploadDto = new UploadDocumentDto
            {
                File = dto.AgencyDocument,
                DocumentTypeID = DocumentTypeConstants.AgencyDocument, // وثيقة الوكالة
                AgentID = agent.AgentID,
                Notes = $"وثيقة الوكالة للوكيل: {dto.FullName} - رقم الوكالة: {dto.AgencyNumber ?? ""}"
            };

            var docResult = await _documentService.UploadAsync(uploadDto, userId);
            if (!docResult.IsSuccess)
            {
                // Log warning but don't fail the update
            }
        }

        var updated = await _unitOfWork.Agents.Query()
            .Include(a => a.RetirementFile)
            .Include(a => a.EntitledPerson)
            .FirstAsync(a => a.AgentID == agent.AgentID);

        await _missingDocumentService.RefreshComplianceCacheAsync(agent.FileID);
        var review = await _reviewService.RequestReviewAsync(agent.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<AgentDto>.Fail(review.Message ?? "تعذر طلب مراجعة الوكالة");

        return ApiResponse<AgentDto>.Success(MapToDto(updated), "تم تحديث الوكيل بنجاح");
    }

    private async Task<ApiResponse<bool>> DeactivateCoreAsync(int id, int userId)
    {
        var agent = await _unitOfWork.Agents.Query()
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<bool>.Fail("الوكيل غير موجود");

        agent.IsActive = false;
        await EndRecipientsAsync(id, userId);
        agent.UpdatedAt = DateTime.UtcNow;
        agent.UpdatedByUserID = userId;

        _unitOfWork.Agents.Update(agent);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(agent.FileID);
        var review = await _reviewService.RequestReviewAsync(agent.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<bool>.Fail(review.Message ?? "تعذر طلب مراجعة الوكالة");

        return ApiResponse<bool>.Success(true, "تم إلغاء تفعيل الوكيل بنجاح");
    }

    private async Task<ApiResponse<bool>> DeleteCoreAsync(int id, int userId)
    {
        var agent = await _unitOfWork.Agents.Query()
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<bool>.Fail("الوكيل غير موجود");

        await EndRecipientsAsync(id, userId);
        agent.IsActive = false;
        agent.IsDeleted = true;
        agent.DeletedAt = DateTime.UtcNow;
        agent.DeletedByUserID = userId;

        _unitOfWork.Agents.Update(agent);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(agent.FileID);
        var review = await _reviewService.RequestReviewAsync(agent.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<bool>.Fail(review.Message ?? "تعذر طلب مراجعة الوكالة");

        return ApiResponse<bool>.Success(true, "تم حذف الوكيل بنجاح");
    }

    private static AgentDto MapToDto(Agent a) => new()
    {
        AgencyScope = (int?)a.AgencyScope,
        AgentID = a.AgentID,
        FileID = a.FileID,
        FileNumber = a.RetirementFile?.FileNumber ?? "",
        EntitledID = a.EntitledID,
        EntitledPersonName = a.EntitledPerson?.FullName,
        FullName = a.FullName,
        NationalID = a.NationalID,
        PersonalPhotoPath = a.PersonalPhotoPath,
        Phone = a.Phone,
        AgencyNumber = a.AgencyNumber,
        AgencyDate = a.AgencyDate,
        AgencyEndDate = a.AgencyEndDate,
        Notes = a.Notes,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        PendingReviewStatus = (int)a.PendingReviewStatus
    };

    private async Task<MemoryStream> ToMemoryStreamAsync(Stream stream)
    {
        if (stream is MemoryStream ms) return ms;
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<ApiResponse<List<AgentSearchResultDto>>> SearchAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return ApiResponse<List<AgentSearchResultDto>>.Success(new());

        var searchTerm = query.Trim().ToLower();

        var agents = await (await AgentsAsync())
            .AsNoTracking()
            .Include(a => a.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Include(a => a.EntitledPerson)
            .Where(a => !a.IsDeleted && (
                a.FullName.ToLower().Contains(searchTerm) ||
                a.NationalID.Contains(searchTerm) ||
                (a.Phone != null && a.Phone.Contains(searchTerm)) ||
                (a.RetirementFile != null && a.RetirementFile.FileNumber.ToLower().Contains(searchTerm)) ||
                (a.RetirementFile != null && a.RetirementFile.Retiree != null &&
                 a.RetirementFile.Retiree.MilitaryNumber != null &&
                 a.RetirementFile.Retiree.MilitaryNumber.Contains(searchTerm))
            ))
            .OrderByDescending(a => a.IsActive)
            .ThenByDescending(a => a.CreatedAt)
            .Take(Math.Clamp(limit, 1, 100))
            .ToListAsync();

        var results = agents.Select(a => new AgentSearchResultDto
        {
            AgencyScope = (int?)a.AgencyScope,
            IsActive = a.IsActive,
            AgentID = a.AgentID,
            FullName = a.FullName,
            NationalID = a.NationalID,
            Phone = a.Phone,
            AgencyNumber = a.AgencyNumber,
            AgencyDate = a.AgencyDate,
            AgencyEndDate = a.AgencyEndDate,
            Notes = a.Notes,
            PersonalPhotoPath = a.PersonalPhotoPath,
            FileID = a.FileID,
            FileNumber = a.RetirementFile?.FileNumber ?? "",
            EntitledID = a.EntitledID,
            EntitledPersonName = a.EntitledPerson?.FullName,
            RetireeMilitaryNumber = a.RetirementFile?.Retiree?.MilitaryNumber
        }).ToList();

        return ApiResponse<List<AgentSearchResultDto>>.Success(results);
    }

    private static bool ValidScope(AgentScope? access, int? entitledId) => access switch
    {
        null => true,
        AgentScope.Retiree or AgentScope.AllEntitled => !entitledId.HasValue,
        AgentScope.SpecificEntitled => entitledId is > 0,
        _ => false
    };

    private async Task EndRecipientsAsync(int agentId, int userId)
    {
        var recipients = await _unitOfWork.SalaryRecipients.Query().Where(r => r.AgentID == agentId && r.IsActive).ToListAsync();
        foreach (var recipient in recipients) RecipientScope.End(recipient, DateTime.UtcNow, userId);
    }

    public Task<ApiResponse<string>> UpdatePhotoAsync(int id, IRemoteFile photo, int userId) =>
        ChangeAgentAsync(id, async () =>
        {
            if (photo.Length == 0) return ApiResponse<string>.Fail("الصورة مطلوبة");
            var agent = await _unitOfWork.Agents.Query().FirstAsync(a => a.AgentID == id);
            using var stream = photo.OpenReadStream();
            var path = await _fileStorage.SaveFileAsync(stream, photo.FileName, "photos");
            agent.PersonalPhotoPath = path;
            agent.UpdatedAt = DateTime.UtcNow;
            agent.UpdatedByUserID = userId;
            await _unitOfWork.SaveChangesAsync();
            var review = await _reviewService.RequestReviewAsync(agent.FileID, userId, agent.UpdatedAt);
            if (!review.IsSuccess)
            {
                await _fileStorage.DeleteFileAsync(path);
                return ApiResponse<string>.Fail(review.Message ?? "تعذر طلب مراجعة الصورة");
            }
            return ApiResponse<string>.Success(path, "تم تحديث صورة الوكيل");
        }, "Agent.Edit");
}
