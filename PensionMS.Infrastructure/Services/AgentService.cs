using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Agents;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Helpers;

namespace PensionMS.Infrastructure.Services;

public class AgentService : IAgentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IDocumentService _documentService;
    private readonly IComplianceEngine _complianceEngine;
    private readonly IFileReviewService _reviewService;
    private readonly ILogger<AgentService> _logger;

    public AgentService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, IDocumentService documentService, IComplianceEngine complianceEngine, IFileReviewService reviewService, ILogger<AgentService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _documentService = documentService;
        _complianceEngine = complianceEngine;
        _reviewService = reviewService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<AgentDto>>> GetByFileIdAsync(int fileId)
    {
        var agents = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .Include(a => a.RetirementFile)
            .Where(a => a.FileID == fileId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<AgentDto>>.Success(agents.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<AgentDto>> GetByIdAsync(int id)
    {
        var agent = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .Include(a => a.RetirementFile)
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<AgentDto>.Fail("الوكيل غير موجود");

        return ApiResponse<AgentDto>.Success(MapToDto(agent));
    }

    public async Task<ApiResponse<AgentDto>> CreateAsync(CreateAgentDto dto, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (file == null)
            return ApiResponse<AgentDto>.Fail("الملف غير موجود");

        var activeAgentExists = await _unitOfWork.Agents.Query()
            .AnyAsync(a => a.FileID == dto.FileID && a.IsActive && !a.IsDeleted);
        if (activeAgentExists)
            return ApiResponse<AgentDto>.Fail("يوجد وكيل نشط بالفعل لهذا الملف. يُرجى إلغاء تفعيله أولاً.");

        // Validate agency fields when agency exists
        var hasAgency = !string.IsNullOrWhiteSpace(dto.AgencyNumber);
        if (hasAgency)
        {
            if (dto.AgencyDate == null || dto.AgencyDate.Value == default)
                return ApiResponse<AgentDto>.Fail("تاريخ الوكالة مطلوب عند وجود وكالة");
        }

        // Save personal photo if provided
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل غير متوقع عند رفع هوية الوكيل");
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

            var docResult = await _documentService.UploadAsync(uploadDto, userId);
            if (!docResult.IsSuccess)
            {
                // Log warning but don't fail the agent creation
                // The agent is already created, we just couldn't save the document
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
            .FirstAsync(a => a.AgentID == agent.AgentID);

        await _complianceEngine.RefreshComplianceCacheAsync(agent.FileID);
        await _reviewService.RequestReviewAsync(agent.FileID, userId, now);

        return ApiResponse<AgentDto>.Success(MapToDto(created), "تم إنشاء الوكيل بنجاح");
    }

    public async Task<ApiResponse<AgentDto>> UpdateAsync(int id, UpdateAgentDto dto, int userId)
    {
        var agent = await _unitOfWork.Agents.Query()
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<AgentDto>.Fail("الوكيل غير موجود");

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
            // Delete old photo if exists
            if (!string.IsNullOrEmpty(agent.PersonalPhotoPath))
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(agent.PersonalPhotoPath);
                }
                catch (Exception ex) { _logger.LogWarning(ex, "فشل في حذف الصورة القديمة: {Path}", agent.PersonalPhotoPath); }
            }

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
            .FirstAsync(a => a.AgentID == agent.AgentID);

        await _complianceEngine.RefreshComplianceCacheAsync(agent.FileID);
        await _reviewService.RequestReviewAsync(agent.FileID, userId, now);

        return ApiResponse<AgentDto>.Success(MapToDto(updated), "تم تحديث الوكيل بنجاح");
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(int id, int userId)
    {
        var agent = await _unitOfWork.Agents.Query()
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<bool>.Fail("الوكيل غير موجود");

        agent.IsActive = false;
        agent.AgencyEndDate = DateTime.UtcNow;
        agent.UpdatedAt = DateTime.UtcNow;
        agent.UpdatedByUserID = userId;

        _unitOfWork.Agents.Update(agent);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        await _complianceEngine.RefreshComplianceCacheAsync(agent.FileID);
        await _reviewService.RequestReviewAsync(agent.FileID, userId, now);

        return ApiResponse<bool>.Success(true, "تم إلغاء تفعيل الوكيل بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var agent = await _unitOfWork.Agents.Query()
            .FirstOrDefaultAsync(a => a.AgentID == id && !a.IsDeleted);

        if (agent == null)
            return ApiResponse<bool>.Fail("الوكيل غير موجود");

        agent.IsDeleted = true;
        agent.DeletedAt = DateTime.UtcNow;
        agent.DeletedByUserID = userId;

        _unitOfWork.Agents.Update(agent);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        await _complianceEngine.RefreshComplianceCacheAsync(agent.FileID);
        await _reviewService.RequestReviewAsync(agent.FileID, userId, now);

        return ApiResponse<bool>.Success(true, "تم حذف الوكيل بنجاح");
    }

    private static AgentDto MapToDto(Agent a) => new()
    {
        AgentID = a.AgentID,
        FileID = a.FileID,
        FileNumber = a.RetirementFile?.FileNumber ?? "",
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

        var agents = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .Include(a => a.RetirementFile)
                .ThenInclude(f => f.Retiree)
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
            .Take(limit)
            .ToListAsync();

        var results = agents.Select(a => new AgentSearchResultDto
        {
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
            RetireeMilitaryNumber = a.RetirementFile?.Retiree?.MilitaryNumber
        }).ToList();

        return ApiResponse<List<AgentSearchResultDto>>.Success(results);
    }
}
