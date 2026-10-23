using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.DocumentExemptions;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Documents;

public class DocumentExemptionService : IDocumentExemptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IMissingDocumentService _missingDocumentService;

    public DocumentExemptionService(IUnitOfWork unitOfWork, IMissingDocumentService missingDocumentService, IAccessiblePensionRepository access)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _missingDocumentService = missingDocumentService;
    }

    public async Task<ApiResponse<List<DocumentExemptionDto>>> GetByFileAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var list = await queries.Query(_unitOfWork.DocumentExemptions)
            .AsNoTracking()
            .Include(e => e.MissingDocuments).ThenInclude(m => m.DocumentType)
            .Where(e => e.FileID == fileId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<DocumentExemptionDto>>.Success(list.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<DocumentExemptionDto>> GetByIdAsync(int id)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var exemption = await queries.Query(_unitOfWork.DocumentExemptions)
            .AsNoTracking()
            .Include(e => e.MissingDocuments).ThenInclude(m => m.DocumentType)
            .FirstOrDefaultAsync(e => e.ExemptionID == id);

        if (exemption == null)
            return ApiResponse<DocumentExemptionDto>.Fail("الإعفاء غير موجود");

        return ApiResponse<DocumentExemptionDto>.Success(MapToDto(exemption));
    }

    public async Task<ApiResponse<DocumentExemptionDto>> CreateAsync(CreateDocumentExemptionDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (!await queries.Files().AnyAsync(f => f.FileID == dto.FileID)) throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود");
        if (dto.MissingDocumentIDs == null || dto.MissingDocumentIDs.Count == 0)
            return ApiResponse<DocumentExemptionDto>.Fail("يجب اختيار وثيقة ناقصة واحدة على الأقل للإعفاء");

        // 1. التحقق من وجود الوثائق الناقصة
        var allPending = await queries.Query(_unitOfWork.MissingDocuments)
            .Where(m => m.FileID == dto.FileID && m.Status == MissingDocumentStatusConstants.Pending)
            .ToListAsync();
        
        var missingDocs = allPending
            .Where(m => dto.MissingDocumentIDs.Contains(m.MissingDocumentID))
            .ToList();

        if (missingDocs.Count != dto.MissingDocumentIDs.Count)
            return ApiResponse<DocumentExemptionDto>.Fail("بعض الوثائق المحددة غير موجودة أو ليست بحالة Pending");

        // 2. إنشاء الإعفاء
        var exemption = new DocumentExemption
        {
            FileID          = dto.FileID,
            CreatedByUserID = userId,
            ExemptionDate   = DateTime.UtcNow,
            DirectiveSource = dto.DirectiveSource,
            Notes           = dto.Notes
        };

        await _unitOfWork.DocumentExemptions.AddAsync(exemption);
        await _unitOfWork.SaveChangesAsync();

        // 3. ربط جميع الوثائق الناقصة بالإعفاء وتحديث حالتها
        foreach (var missing in missingDocs)
        {
            missing.ExemptionID = exemption.ExemptionID;
            missing.Status      = MissingDocumentStatusConstants.Exempted;
        }
        await _unitOfWork.SaveChangesAsync();

        // إعادة القراءة مع العلاقات
        var created = await queries.Query(_unitOfWork.DocumentExemptions)
            .Include(e => e.MissingDocuments).ThenInclude(m => m.DocumentType)
            .FirstAsync(e => e.ExemptionID == exemption.ExemptionID);

        // ── تسجيل نقص لتوجيه الإعفاء ─────────────────────────────
        await _unitOfWork.MissingDocuments.AddAsync(new MissingDocument
        {
            FileID = dto.FileID,
            DocumentTypeID = DocumentTypeConstants.ExemptionDirective, // توجيه بإعفاء من النواقص
            ExemptionID = exemption.ExemptionID,
            Status = MissingDocumentStatusConstants.Pending,
            Notes = $"توجيه إعفاء رقم: {exemption.ExemptionID}"
        });
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(dto.FileID);

        return ApiResponse<DocumentExemptionDto>.Success(MapToDto(created), "تم تسجيل الإعفاء بنجاح");
    }

    public async Task<ApiResponse<DocumentExemptionDto>> UpdateAsync(int id, UpdateDocumentExemptionDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var exemption = await queries.Query(_unitOfWork.DocumentExemptions).FirstOrDefaultAsync(e => e.ExemptionID == id);
        if (exemption == null)
            return ApiResponse<DocumentExemptionDto>.Fail("الإعفاء غير موجود");

        exemption.DirectiveSource = dto.DirectiveSource;
        exemption.Notes = dto.Notes;

        _unitOfWork.DocumentExemptions.Update(exemption);
        await _unitOfWork.SaveChangesAsync();

        var updated = await queries.Query(_unitOfWork.DocumentExemptions)
            .Include(e => e.MissingDocuments).ThenInclude(m => m.DocumentType)
            .FirstAsync(e => e.ExemptionID == id);

        return ApiResponse<DocumentExemptionDto>.Success(MapToDto(updated), "تم تحديث الإعفاء بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var exemption = await queries.Query(_unitOfWork.DocumentExemptions)
            .Include(e => e.MissingDocuments)
            .FirstOrDefaultAsync(e => e.ExemptionID == id);

        if (exemption == null)
            return ApiResponse<bool>.Fail("الإعفاء غير موجود");

        // Reset MissingDocuments status
        foreach (var missing in exemption.MissingDocuments)
        {
            missing.ExemptionID = null;
            missing.Status = MissingDocumentStatusConstants.Pending;
        }

        exemption.IsDeleted = true;
        exemption.DeletedAt = DateTime.UtcNow;
        exemption.DeletedByUserID = userId;
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(exemption.FileID);

        return ApiResponse<bool>.Success(true, "تم حذف الإعفاء بنجاح وإعادة الوثائق لحالة النقص");
    }

    private static DocumentExemptionDto MapToDto(DocumentExemption e)
    {
        var missingDocsList = e.MissingDocuments?
            .Select(m => new PensionMS.Core.DTOs.DocumentExemptions.MissingDocumentInfo
            {
                MissingDocumentID = m.MissingDocumentID,
                DocumentTypeID = m.DocumentTypeID,
                DocumentTypeName = m.DocumentType?.TypeName ?? ""
            })
            .ToList() ?? new List<PensionMS.Core.DTOs.DocumentExemptions.MissingDocumentInfo>();

        return new DocumentExemptionDto
        {
            ExemptionID             = e.ExemptionID,
            FileID                  = e.FileID,
            MissingDocuments        = missingDocsList,
            DirectiveSource         = e.DirectiveSource,
            ExemptionDate           = e.ExemptionDate,
            Notes                   = e.Notes,
            CreatedAt               = e.CreatedAt
        };
    }
}
