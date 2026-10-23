using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.Guardians;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Helpers;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Pension.Beneficiaries;

public class GuardianService : IGuardianService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDocumentService _documentService;
    private readonly IComplianceEngine _complianceEngine;
    private readonly IFileStorageService _fileStorage;
    private readonly IFileReviewService _reviewService;
    private readonly ILogger<GuardianService> _logger;
    private readonly IBranchScopeProvider _scope;

    public GuardianService(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        IComplianceEngine complianceEngine,
        IFileStorageService fileStorage,
        IFileReviewService reviewService,
        ILogger<GuardianService> logger, IBranchScopeProvider scope)
    {
        _unitOfWork = unitOfWork;
        _documentService = documentService;
        _complianceEngine = complianceEngine;
        _fileStorage = fileStorage;
        _reviewService = reviewService;
        _logger = logger;
        _scope = scope;
    }

    private async Task<bool> CanEditAsync(int fileId) =>
        await PensionFileAccess.Files(_unitOfWork, _scope).AnyAsync(f => f.FileID == fileId &&
            f.StatusID != FileStatusConstants.Closed && f.StatusID != FileStatusConstants.Suspended && f.StatusID != FileStatusConstants.Rejected);

    public async Task<ApiResponse<List<GuardianDto>>> GetByFileIdAsync(int fileId)
    {
        var guardians = await _unitOfWork.Guardians.Query()
            .Include(g => g.GuardianEntitledPersons).ThenInclude(ge => ge.EntitledPerson)
            .Where(g => g.FileID == fileId && !g.IsDeleted)
            .ToListAsync();

        return ApiResponse<List<GuardianDto>>.Success(guardians.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<GuardianDto>>> GetByEntitledIdAsync(int entitledId)
    {
        var links = await _unitOfWork.GuardianEntitledPersons.Query()
            .Include(ge => ge.Guardian)
            .Where(ge => ge.EntitledID == entitledId && !ge.Guardian.IsDeleted)
            .ToListAsync();

        return ApiResponse<List<GuardianDto>>.Success(links.Select(l => MapToDto(l.Guardian)).ToList());
    }

    public async Task<ApiResponse<GuardianDto>> GetByIdAsync(int id)
    {
        var guardian = await _unitOfWork.Guardians.Query()
            .Include(g => g.GuardianEntitledPersons).ThenInclude(ge => ge.EntitledPerson)
            .FirstOrDefaultAsync(g => g.GuardianID == id && !g.IsDeleted);

        if (guardian == null)
            return ApiResponse<GuardianDto>.Fail("الوصي غير موجود");

        return ApiResponse<GuardianDto>.Success(MapToDto(guardian));
    }

    public Task<ApiResponse<GuardianDto>> AssignAsync(AssignGuardianDto dto, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            if (!await CanEditAsync(dto.FileID)) return ApiResponse<GuardianDto>.Fail("الملف غير متاح لتعديل الوصاية");
            await _unitOfWork.LockFileReviewAsync(dto.FileID);
            return await AssignCoreAsync(dto, userId);
        });

    private async Task<ApiResponse<GuardianDto>> AssignCoreAsync(AssignGuardianDto dto, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(dto.FileID);
        if (file == null)
            return ApiResponse<GuardianDto>.Fail("الملف غير موجود");

        var entitledPersons = await _unitOfWork.EntitledPersons.Query()
            .Where(e => dto.EntitledIDs.Contains(e.EntitledID) && e.FileID == dto.FileID && !e.IsDeleted)
            .ToListAsync();

        if (entitledPersons.Count != dto.EntitledIDs.Count)
            return ApiResponse<GuardianDto>.Fail("بعض المستحقين المحددين غير موجودين في هذا الملف");

        if (await _unitOfWork.Agents.Query().AnyAsync(a => a.FileID == dto.FileID && a.IsActive && !a.IsDeleted &&
            a.EntitledID.HasValue && dto.EntitledIDs.Contains(a.EntitledID.Value)))
            return ApiResponse<GuardianDto>.Fail("لا يمكن تعيين وصي لمستحق لديه وكيل نشط؛ أنه الوكالة أولاً");
        if (entitledPersons.Any(e => e.IsDeceased))
            return ApiResponse<GuardianDto>.Fail("لا يمكن تعيين وصي لمستحق متوفى");

        var guardian = await _unitOfWork.Guardians.Query()
            .FirstOrDefaultAsync(g => g.NationalID == dto.NationalID && g.FileID == dto.FileID && !g.IsDeleted);

        bool isNewGuardian = false;
        var captureTime = DateTime.UtcNow;
        if (guardian == null)
        {
            isNewGuardian = true;
            guardian = new Guardian
            {
                FileID = dto.FileID,
                FullName = dto.FullName,
                NationalID = dto.NationalID,
                Phone = dto.Phone,
                GuardianshipDocNumber = dto.GuardianshipDocNumber,
                StartDate = dto.StartDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Guardians.AddAsync(guardian);
            await _unitOfWork.SaveChangesAsync();
        }

        foreach (var ep in entitledPersons)
        {
            var activeLinks = await _unitOfWork.GuardianEntitledPersons.Query()
                .Where(ge => ge.EntitledID == ep.EntitledID && ge.IsActive)
                .ToListAsync();
            foreach (var link in activeLinks)
            {
                link.IsActive = false;
                link.EndDate = DateTime.UtcNow;
            }

            await _unitOfWork.GuardianEntitledPersons.AddAsync(new GuardianEntitledPerson
            {
                GuardianID = guardian.GuardianID,
                EntitledID = ep.EntitledID,
                StartDate = dto.StartDate,
                IsActive = true
            });

            await TransferSalaryToGuardianAsync(guardian.GuardianID, ep.EntitledID, dto.FileID, userId);
        }

        if (dto.PersonalPhoto != null)
        {
            using var stream = dto.PersonalPhoto.OpenReadStream();
            var photoPath = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "guardians");
            guardian.PersonalPhotoPath = photoPath;
        }

        if (dto.NationalIDDocument != null || dto.NationalIDBackDocument != null)
        {
            try
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentTypeID = DocumentTypeConstants.NationalId, // الهوية الوطنية
                    GuardianID = guardian.GuardianID,
                    Notes = $"الهوية الوطنية للوصي: {dto.FullName}",
                    File = dto.NationalIDDocument!,
                    FileBack = dto.NationalIDBackDocument
                };

                if (dto.NationalIDDocument == null && dto.NationalIDBackDocument != null)
                {
                    uploadDto.File = dto.NationalIDBackDocument;
                    uploadDto.FileBack = null;
                }

                var uploadResult = await _documentService.UploadAsync(uploadDto, userId);
                if (!uploadResult.IsSuccess)
                {
                    _logger.LogWarning("فشل رفع الهوية الوطنية للوصي: {Message}", uploadResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل غير متوقع عند رفع هوية الوصي");
            }
        }

        if (dto.GuardianshipDocument != null)
        {
            var guardianshipUploadDto = new UploadDocumentDto
            {
                File = dto.GuardianshipDocument,
                DocumentTypeID = DocumentTypeConstants.GuardianshipDocument,
                GuardianID = guardian.GuardianID,
                Notes = $"وثيقة الوصاية للوصي: {dto.FullName}"
            };
            await _documentService.UploadAsync(guardianshipUploadDto, userId);
        }

        if (isNewGuardian)
        {
            var missingDocs = new List<MissingDocument>();
            if (dto.NationalIDDocument == null && dto.NationalIDBackDocument == null)
                missingDocs.Add(new MissingDocument { FileID = dto.FileID, DocumentTypeID = DocumentTypeConstants.NationalId, GuardianID = guardian.GuardianID, Status = MissingDocumentStatusConstants.Pending, Notes = "هوية الوصي" });
            if (dto.GuardianshipDocument == null)
                missingDocs.Add(new MissingDocument { FileID = dto.FileID, DocumentTypeID = DocumentTypeConstants.GuardianshipDocument, GuardianID = guardian.GuardianID, Status = MissingDocumentStatusConstants.Pending, Notes = "وثيقة الوصاية" });

            foreach (var md in missingDocs) await _unitOfWork.MissingDocuments.AddAsync(md);
        }

        await _unitOfWork.SaveChangesAsync();
        await _complianceEngine.RefreshComplianceCacheAsync(dto.FileID);
        var review = await _reviewService.RequestReviewAsync(dto.FileID, userId, captureTime);
        if (!review.IsSuccess) return ApiResponse<GuardianDto>.Fail(review.Message ?? "تعذر طلب مراجعة الوصاية");

        return ApiResponse<GuardianDto>.Success(MapToDto(guardian), "تم تعيين الوصي بنجاح");
    }

    public async Task<ApiResponse<bool>> EndGuardianshipAsync(int guardianId, EndGuardianshipDto dto, int userId)
    {
        return await EndGuardianshipAsync(guardianId, null, dto, userId);
    }

    public async Task<ApiResponse<bool>> EndGuardianshipAsync(int guardianId, int? entitledId, EndGuardianshipDto dto, int userId)
    {
        var guardian = await _unitOfWork.Guardians.Query()
            .Include(g => g.GuardianEntitledPersons)
            .FirstOrDefaultAsync(g => g.GuardianID == guardianId && !g.IsDeleted);

        if (guardian == null) return ApiResponse<bool>.Fail("الوصي غير موجود");

        var linksQuery = _unitOfWork.GuardianEntitledPersons.Query()
            .Where(ge => ge.GuardianID == guardianId && ge.IsActive);
        
        if (entitledId.HasValue)
            linksQuery = linksQuery.Where(ge => ge.EntitledID == entitledId.Value);

        var links = await linksQuery.ToListAsync();
        if (!links.Any()) return ApiResponse<bool>.Fail("لا توجد وصاية نشطة لإنهاؤها");

        foreach (var link in links)
        {
            link.IsActive = false;
            link.EndDate = DateTime.UtcNow; 
            await ReturnSalaryToEntitledAsync(guardianId, link.EntitledID, guardian.FileID, userId);
        }

        if (!entitledId.HasValue || !await _unitOfWork.GuardianEntitledPersons.Query().AnyAsync(ge => ge.GuardianID == guardianId && ge.IsActive))
        {
            guardian.IsActive = false;
            guardian.EndDate = DateTime.UtcNow;
            _unitOfWork.Guardians.Update(guardian);
        }

        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        await _complianceEngine.RefreshComplianceCacheAsync(guardian.FileID);
        await _reviewService.RequestReviewAsync(guardian.FileID, userId, now);
        return ApiResponse<bool>.Success(true, "تم إنهاء الوصاية بنجاح");
    }

    public async Task<ApiResponse<GuardianDto>> UpdateAsync(int id, UpdateGuardianDto dto, int userId)
    {
        var guardian = await _unitOfWork.Guardians.Query()
            .Include(g => g.GuardianEntitledPersons).ThenInclude(ge => ge.EntitledPerson)
            .FirstOrDefaultAsync(g => g.GuardianID == id && !g.IsDeleted);

        if (guardian == null)
            return ApiResponse<GuardianDto>.Fail("الوصي غير موجود");

        // تحديث البيانات الأساسية
        guardian.FullName = dto.FullName;
        guardian.NationalID = dto.NationalID;
        guardian.Phone = dto.Phone;
        guardian.GuardianshipDocNumber = dto.GuardianshipDocNumber ?? guardian.GuardianshipDocNumber;

        // رفع الصورة الشخصية الجديدة
        if (dto.PersonalPhoto != null)
        {
            if (!string.IsNullOrEmpty(guardian.PersonalPhotoPath))
            {
                try { await _fileStorage.DeleteFileAsync(guardian.PersonalPhotoPath); }
                catch (Exception ex) { _logger.LogWarning(ex, "فشل حذف الصورة القديمة للوصي"); }
            }

            using var stream = dto.PersonalPhoto.OpenReadStream();
            guardian.PersonalPhotoPath = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "guardians");
        }

        guardian.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Guardians.Update(guardian);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<GuardianDto>.Success(MapToDto(guardian), "تم تحديث بيانات الوصي بنجاح");
    }

    public Task<ApiResponse<GuardianDto>> AddMinorAsync(int guardianId, int entitledId, DateTime startDate, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            var guardian = await _unitOfWork.Guardians.GetByIdAsync(guardianId);
            if (guardian == null) return ApiResponse<GuardianDto>.Fail("الوصي غير موجود");
            if (!await CanEditAsync(guardian.FileID)) return ApiResponse<GuardianDto>.Fail("الملف غير متاح لتعديل الوصاية");
            await _unitOfWork.LockFileReviewAsync(guardian.FileID);
            return await AddMinorCoreAsync(guardianId, entitledId, startDate, userId);
        });

    private async Task<ApiResponse<GuardianDto>> AddMinorCoreAsync(int guardianId, int entitledId, DateTime startDate, int userId)
    {
        var guardian = await _unitOfWork.Guardians.GetByIdAsync(guardianId);
        if (guardian == null || !guardian.IsActive) return ApiResponse<GuardianDto>.Fail("الوصي غير موجود أو غير نشط");

        var entitled = await _unitOfWork.EntitledPersons.GetByIdAsync(entitledId);
        if (entitled == null || entitled.IsDeleted || entitled.IsDeceased || entitled.FileID != guardian.FileID)
            return ApiResponse<GuardianDto>.Fail("المستحق غير موجود أو لا ينتمي إلى ملف الوصي");
        if (await _unitOfWork.Agents.Query().AnyAsync(a => a.EntitledID == entitledId && a.IsActive && !a.IsDeleted))
            return ApiResponse<GuardianDto>.Fail("لا يمكن تعيين وصي لمستحق لديه وكيل نشط");
        if (await _unitOfWork.GuardianEntitledPersons.Query().AnyAsync(g => g.EntitledID == entitledId && g.IsActive))
            return ApiResponse<GuardianDto>.Fail("المستحق مرتبط بوصاية نشطة بالفعل");

        await _unitOfWork.GuardianEntitledPersons.AddAsync(new GuardianEntitledPerson
        {
            GuardianID = guardianId,
            EntitledID = entitledId,
            StartDate = startDate,
            IsActive = true
        });

        await TransferSalaryToGuardianAsync(guardianId, entitledId, guardian.FileID, userId);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var review = await _reviewService.RequestReviewAsync(guardian.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<GuardianDto>.Fail(review.Message ?? "تعذر طلب مراجعة الوصاية");
        
        return await GetByIdAsync(guardianId);
    }

    private async Task TransferSalaryToGuardianAsync(int guardianId, int entitledId, int fileId, int userId)
    {
        // Q5: يُمنع الجمع وصي + وكيل لنفس المستحق
        var hasActiveAgent = await _unitOfWork.Agents.Query()
            .AnyAsync(a => a.FileID == fileId && a.EntitledID == entitledId && a.IsActive && !a.IsDeleted);
        if (hasActiveAgent)
            throw new InvalidOperationException("لا يمكن تحويل حصة مستحق لديه وكيل نشط إلى وصي");

        var currentRecipients = await RecipientScope.Query(_unitOfWork, fileId, entitledId)
            .Where(s => s.IsActive)
            .ToListAsync();

        foreach (var r in currentRecipients)
        {
            r.IsActive = false;
            r.EndDate = DateTime.UtcNow;
            r.UpdatedAt = DateTime.UtcNow;
            r.UpdatedByUserID = userId;
            _unitOfWork.SalaryRecipients.Update(r);
        }

        var entitled = await _unitOfWork.EntitledPersons.GetByIdAsync(entitledId);
        await _unitOfWork.SalaryRecipients.AddAsync(new SalaryRecipient
        {
            FileID = fileId,
            GuardianID = guardianId,
            SharePercent = entitled?.SharePercent ?? currentRecipients.FirstOrDefault()?.SharePercent,
            EntitledID = entitledId,
            RecipientType = LookupConstants.RecipientTypeGuardian,
            StartDate = DateTime.UtcNow,
            IsActive = true,
            Notes = $"تحويل للوصي {guardianId}",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserID = userId
        });
    }

    private async Task ReturnSalaryToEntitledAsync(int guardianId, int entitledId, int fileId, int userId)
    {
        var currentRecipients = await _unitOfWork.SalaryRecipients.Query()
            .Where(s => s.GuardianID == guardianId && s.EntitledID == entitledId && s.IsActive)
            .ToListAsync();

        foreach (var r in currentRecipients)
        {
            r.IsActive = false;
            r.EndDate = DateTime.UtcNow;
            r.UpdatedAt = DateTime.UtcNow;
            r.UpdatedByUserID = userId;
            _unitOfWork.SalaryRecipients.Update(r);
        }

        await _unitOfWork.SalaryRecipients.AddAsync(new SalaryRecipient
        {
            FileID = fileId,
            EntitledID = entitledId,
            RecipientType = LookupConstants.RecipientTypeEntitled,
            StartDate = DateTime.UtcNow,
            IsActive = true,
            Notes = $"إعادة للمستحق {entitledId}",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserID = userId
        });
    }

    private static GuardianDto MapToDto(Guardian g) => new()
    {
        GuardianID = g.GuardianID,
        FileID = g.FileID,
        FullName = g.FullName,
        NationalID = g.NationalID,
        PersonalPhotoPath = g.PersonalPhotoPath,
        Phone = g.Phone,
        GuardianshipDocNumber = g.GuardianshipDocNumber,
        StartDate = g.StartDate,
        EndDate = g.EndDate,
        IsActive = g.IsActive,
        PendingReviewStatus = (int)g.PendingReviewStatus,
        Minors = g.GuardianEntitledPersons?.Select(ge => new GuardianMinorDto
        {
            EntitledID = ge.EntitledID,
            EntitledPersonName = ge.EntitledPerson?.FullName ?? string.Empty,
            IsActive = ge.IsActive,
            StartDate = ge.StartDate,
            EndDate = ge.EndDate
        }).ToList() ?? new List<GuardianMinorDto>()
    };

    private async Task<MemoryStream> ToMemoryStreamAsync(Stream stream)
    {
        if (stream is MemoryStream ms) return ms;
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<ApiResponse<List<GuardianSearchResultDto>>> SearchAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return ApiResponse<List<GuardianSearchResultDto>>.Success(new());

        var searchTerm = query.Trim().ToLower();

        var guardians = await _unitOfWork.Guardians.Query()
            .AsNoTracking()
            .Include(g => g.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Where(g => !g.IsDeleted && (
                g.FullName.ToLower().Contains(searchTerm) ||
                g.NationalID.Contains(searchTerm) ||
                (g.Phone != null && g.Phone.Contains(searchTerm)) ||
                (g.RetirementFile != null && g.RetirementFile.FileNumber.ToLower().Contains(searchTerm)) ||
                (g.RetirementFile != null && g.RetirementFile.Retiree != null &&
                 g.RetirementFile.Retiree.MilitaryNumber != null &&
                 g.RetirementFile.Retiree.MilitaryNumber.Contains(searchTerm))
            ))
            .OrderByDescending(g => g.IsActive)
            .ThenByDescending(g => g.CreatedAt)
            .Take(limit)
            .ToListAsync();

        var results = guardians.Select(g => new GuardianSearchResultDto
        {
            GuardianID = g.GuardianID,
            FileID = g.FileID,
            FullName = g.FullName,
            NationalID = g.NationalID,
            Phone = g.Phone,
            GuardianshipDocNumber = g.GuardianshipDocNumber,
            IsActive = g.IsActive,
            PersonalPhotoPath = g.PersonalPhotoPath,
            FileNumber = g.RetirementFile?.FileNumber ?? "",
            RetireeMilitaryNumber = g.RetirementFile?.Retiree?.MilitaryNumber
        }).ToList();

        return ApiResponse<List<GuardianSearchResultDto>>.Success(results);
    }
}
