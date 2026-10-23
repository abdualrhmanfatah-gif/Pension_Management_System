using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.DocumentPledges;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Documents;

public class DocumentPledgeService : IDocumentPledgeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IMissingDocumentService _missingDocumentService;

    public DocumentPledgeService(IUnitOfWork unitOfWork, IMissingDocumentService missingDocumentService, IAccessiblePensionRepository access)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _missingDocumentService = missingDocumentService;
    }

    public async Task<ApiResponse<List<DocumentPledgeDto>>> GetByFileAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var list = await queries.Query(_unitOfWork.DocumentPledges)
            .AsNoTracking()
            .Include(p => p.RetirementFile).ThenInclude(f => f.Retiree)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.DocumentType)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Agent)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Guardian)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.EntitledPerson)
            .Where(p => p.FileID == fileId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<DocumentPledgeDto>>.Success(list.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<DocumentPledgeDto>> GetByIdAsync(int id)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var pledge = await queries.Query(_unitOfWork.DocumentPledges)
            .AsNoTracking()
            .Include(p => p.RetirementFile).ThenInclude(f => f.Retiree)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.DocumentType)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Agent)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Guardian)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.EntitledPerson)
            .FirstOrDefaultAsync(p => p.PledgeID == id && !p.IsDeleted);

        if (pledge == null)
            return ApiResponse<DocumentPledgeDto>.Fail("التعهد غير موجود");

        return ApiResponse<DocumentPledgeDto>.Success(MapToDto(pledge));
    }

    public async Task<ApiResponse<DocumentPledgeDto>> CreateAsync(CreateDocumentPledgeDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (!await queries.Files().AnyAsync(f => f.FileID == dto.FileID)) throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود");
        if (dto.MissingDocumentIDs == null || dto.MissingDocumentIDs.Count == 0)
            return ApiResponse<DocumentPledgeDto>.Fail("يجب اختيار وثيقة ناقصة واحدة على الأقل");

        // 1. التحقق من جميع الوثائق الناقصة
        var allPending = await queries.Query(_unitOfWork.MissingDocuments)
            .Where(m => m.FileID == dto.FileID && m.Status == MissingDocumentStatusConstants.Pending)
            .ToListAsync();
        
        var missingDocs = allPending
            .Where(m => dto.MissingDocumentIDs.Contains(m.MissingDocumentID))
            .ToList();

        if (missingDocs.Count != dto.MissingDocumentIDs.Count)
            return ApiResponse<DocumentPledgeDto>.Fail("بعض الوثائق الناقصة غير موجودة أو ليست بحالة Pending");

        // 2. إنشاء التعهد
        var pledge = new DocumentPledge
        {
            FileID          = dto.FileID,
            CreatedByUserID = userId,
            PledgeDate      = DateTime.UtcNow,
            Deadline        = dto.Deadline,
            Status          = "Active",
            PledgedByName   = dto.PledgedByName,
            PledgedByType   = dto.PledgedByType ?? "ذاتي",
            Reason          = dto.Reason
        };

        await _unitOfWork.DocumentPledges.AddAsync(pledge);
        await _unitOfWork.SaveChangesAsync();

        // 3. ربط جميع الوثائق الناقصة بالتعهد وتحديث حالتها
        foreach (var missing in missingDocs)
        {
            missing.PledgeID = pledge.PledgeID;
            missing.Status   = MissingDocumentStatusConstants.Pledged;
        }
        await _unitOfWork.SaveChangesAsync();

        // إعادة القراءة مع العلاقات
        var created = await queries.Query(_unitOfWork.DocumentPledges)
            .Include(p => p.RetirementFile).ThenInclude(f => f.Retiree)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.DocumentType)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Agent)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Guardian)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.EntitledPerson)
            .FirstAsync(p => p.PledgeID == pledge.PledgeID);

        // ── تسجيل نقص لوثيقة التعهد ─────────────────────────────
        await RegisterPledgeDirectiveMissingDoc(dto.FileID, pledge.PledgeID);

        await _missingDocumentService.RefreshComplianceCacheAsync(dto.FileID);

        return ApiResponse<DocumentPledgeDto>.Success(MapToDto(created), $"تم إنشاء التعهد وربطه بـ {missingDocs.Count} وثيقة ناقصة");
    }

    private async Task RegisterPledgeDirectiveMissingDoc(int fileId, int pledgeId)
    {
        await _unitOfWork.MissingDocuments.AddAsync(new MissingDocument
        {
            FileID = fileId,
            DocumentTypeID = DocumentTypeConstants.PledgeDocument, // وثيقة تعهد
            PledgeID = pledgeId,
            Status = MissingDocumentStatusConstants.Pending,
            Notes = $"وثيقة التعهد رقم: {pledgeId}"
        });
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ApiResponse<bool>> MarkFulfilledAsync(int id, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var pledge = await queries.Query(_unitOfWork.DocumentPledges)
            .Include(p => p.MissingDocuments)
            .FirstOrDefaultAsync(p => p.PledgeID == id && !p.IsDeleted);

        if (pledge == null)
            return ApiResponse<bool>.Fail("التعهد غير موجود");

        // التحقق من وجود وثائق لم يتم استيفاؤها بعد
        var outstandingDocs = pledge.MissingDocuments
            .Where(m => m.DocumentTypeID != DocumentTypeConstants.PledgeDocument) // استثناء وثيقة التعهد نفسها
            .Any(m => m.Status != MissingDocumentStatusConstants.Fulfilled && m.Status != MissingDocumentStatusConstants.Exempted);

        if (outstandingDocs)
        {
            return ApiResponse<bool>.Fail("لا يمكن تعليم التعهد كمستوفى لوجود وثائق ناقصة مرتبطة به لم يتم رفعها بعد");
        }

        pledge.Status      = MissingDocumentStatusConstants.Fulfilled;
        pledge.FulfilledAt = DateTime.UtcNow;
        pledge.UpdatedAt   = DateTime.UtcNow;

        // استيفاء وثيقة التعهد التلقائية إن وجدت
        var pledgeDirectiveDocs = pledge.MissingDocuments
            .Where(m => m.DocumentTypeID == DocumentTypeConstants.PledgeDocument && m.Status == MissingDocumentStatusConstants.Pending)
            .ToList();

        foreach (var pd in pledgeDirectiveDocs)
        {
            pd.Status = MissingDocumentStatusConstants.Fulfilled;
            pd.Notes += " (تم الإيفاء به تلقائياً عند إغلاق التعهد)";
        }

        _unitOfWork.DocumentPledges.Update(pledge);
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(pledge.FileID);

        return ApiResponse<bool>.Success(true, "تم تعليم التعهد كمستوفى بنجاح");
    }


    public async Task<ApiResponse<DocumentPledgeDto>> UpdateAsync(int id, UpdateDocumentPledgeDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var pledge = await queries.Query(_unitOfWork.DocumentPledges)
            .FirstOrDefaultAsync(p => p.PledgeID == id && !p.IsDeleted);

        if (pledge == null)
            return ApiResponse<DocumentPledgeDto>.Fail("التعهد غير موجود");

        pledge.Deadline = dto.Deadline;
        pledge.PledgedByName = dto.PledgedByName;
        pledge.PledgedByType = dto.PledgedByType ?? pledge.PledgedByType;
        pledge.Reason = dto.Reason;
        pledge.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.DocumentPledges.Update(pledge);
        await _unitOfWork.SaveChangesAsync();

        var updated = await queries.Query(_unitOfWork.DocumentPledges)
            .Include(p => p.RetirementFile).ThenInclude(f => f.Retiree)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.DocumentType)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Agent)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.Guardian)
            .Include(p => p.MissingDocuments).ThenInclude(m => m.EntitledPerson)
            .FirstAsync(p => p.PledgeID == id);

        return ApiResponse<DocumentPledgeDto>.Success(MapToDto(updated), "تم تحديث التعهد بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var pledge = await queries.Query(_unitOfWork.DocumentPledges)
            .Include(p => p.MissingDocuments)
            .FirstOrDefaultAsync(p => p.PledgeID == id && !p.IsDeleted);

        if (pledge == null)
            return ApiResponse<bool>.Fail("التعهد غير موجود");

        // Soft delete
        pledge.IsDeleted = true;
        pledge.DeletedAt = DateTime.UtcNow;
        pledge.DeletedByUserID = userId;

        // Reset MissingDocuments status
        foreach (var missing in pledge.MissingDocuments)
        {
            missing.PledgeID = null;
            missing.Status = MissingDocumentStatusConstants.Pending;
        }

        _unitOfWork.DocumentPledges.Update(pledge);
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(pledge.FileID);

        return ApiResponse<bool>.Success(true, "تم حذف التعهد بنجاح");
    }

    private static DocumentPledgeDto MapToDto(DocumentPledge p)
    {
        var missingDocsList = p.MissingDocuments?
            .Select(m => {
                string? entityName = null;
                string? entityType = null;

                if (m.AgentID != null) { entityName = m.Agent?.FullName; entityType = "Agent"; }
                else if (m.GuardianID != null) { entityName = m.Guardian?.FullName; entityType = "Guardian"; }
                else if (m.EntitledID != null) { entityName = m.EntitledPerson?.FullName; entityType = "EntitledPerson"; }
                else { entityType = "Retiree"; }

                return new PensionMS.Core.DTOs.DocumentPledges.MissingDocumentInfo
                {
                    MissingDocumentID = m.MissingDocumentID,
                    DocumentTypeID = m.DocumentTypeID,
                    DocumentTypeName = m.DocumentType?.TypeName ?? "",
                    EntityName = entityName,
                    EntityType = entityType,
                    AgentID = m.AgentID,
                    GuardianID = m.GuardianID,
                    EntitledID = m.EntitledID,
                    Status = m.Status
                };
            })
            .ToList() ?? new List<PensionMS.Core.DTOs.DocumentPledges.MissingDocumentInfo>();

        return new DocumentPledgeDto
        {
            PledgeID                = p.PledgeID,
            FileID                  = p.FileID,
            RetireeName             = p.RetirementFile?.Retiree?.FullName,
            MissingDocuments        = missingDocsList,
            PledgeNumber            = p.PledgeNumber,
            PledgeDate              = p.PledgeDate,
            Deadline                = p.Deadline,
            Status                  = p.Status,
            PledgedByName           = p.PledgedByName,
            PledgedByType           = p.PledgedByType,
            Reason                  = p.Reason,
            FulfilledAt             = p.FulfilledAt,
            CreatedAt               = p.CreatedAt
        };
    }
}
