using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Correspondence;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services.HR;

public class CorrespondenceService : ICorrespondenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;

    public CorrespondenceService(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
    }

    // ─── Correspondences ─────────────────────────────────────

    public async Task<ApiResponse<PagedResult<CorrespondenceListItemDto>>> GetAllAsync(
        byte? direction = null, int? typeId = null,
        int page = 1, int pageSize = 20, string? search = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _unitOfWork.Correspondences.Query()
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        // Filters
        if (direction.HasValue)
            query = query.Where(c => c.Direction == direction.Value);

        if (typeId.HasValue)
            query = query.Where(c => c.CorrespondenceTypeID == typeId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Subject.Contains(term) ||
                c.ReferenceNumber.Contains(term) ||
                (c.ExternalParty != null && c.ExternalParty.Name.Contains(term)) ||
                (c.Employee != null && c.Employee.FullName.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CorrespondenceDate)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CorrespondenceListItemDto
            {
                CorrespondenceID = c.CorrespondenceID,
                Direction = c.Direction,
                TypeName = c.CorrespondenceType.Name,
                SequentialNumber = c.SequentialNumber,
                ReferenceNumber = c.ReferenceNumber,
                CorrespondenceDate = c.CorrespondenceDate,
                Subject = c.Subject,
                PageNumber = c.PageNumber,
                ExternalPartyName = c.ExternalParty != null ? c.ExternalParty.Name : null,
                EmployeeName = c.Employee != null ? c.Employee.FullName : null,
                CreatedByUserName = c.CreatedByUser.FullName,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<CorrespondenceListItemDto>>.Success(new PagedResult<CorrespondenceListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<CorrespondenceDetailsDto>> GetByIdAsync(int id)
    {
        var result = await _unitOfWork.Correspondences.Query()
            .AsNoTracking()
            .Where(c => c.CorrespondenceID == id && !c.IsDeleted)
            .Select(c => new CorrespondenceDetailsDto
            {
                CorrespondenceID = c.CorrespondenceID,
                Direction = c.Direction,
                CorrespondenceTypeID = c.CorrespondenceTypeID,
                TypeName = c.CorrespondenceType.Name,
                SequentialNumber = c.SequentialNumber,
                ReferenceNumber = c.ReferenceNumber,
                CorrespondenceDate = c.CorrespondenceDate,
                Subject = c.Subject,
                PageNumber = c.PageNumber,
                ExternalPartyID = c.ExternalPartyID,
                ExternalPartyName = c.ExternalParty != null ? c.ExternalParty.Name : null,
                EmployeeID = c.EmployeeID,
                EmployeeName = c.Employee != null ? c.Employee.FullName : null,
                CreatedByUserName = c.CreatedByUser.FullName,
                CreatedAt = c.CreatedAt,
                Notes = c.Notes,
                DocumentCount = c.Documents.Count(d => !d.IsDeleted)
            })
            .FirstOrDefaultAsync();

        if (result == null) return ApiResponse<CorrespondenceDetailsDto>.Fail("المراسلة غير موجودة");
        return ApiResponse<CorrespondenceDetailsDto>.Success(result);
    }

    public async Task<ApiResponse<CorrespondenceDetailsDto>> CreateAsync(CreateCorrespondenceDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Subject)) return ApiResponse<CorrespondenceDetailsDto>.Fail("الموضوع مطلوب");
        if (string.IsNullOrWhiteSpace(dto.ReferenceNumber)) return ApiResponse<CorrespondenceDetailsDto>.Fail("رقم القيد مطلوب");

        var entry = new Correspondence
        {
            Direction = dto.Direction,
            CorrespondenceTypeID = dto.CorrespondenceTypeID,
            SequentialNumber = dto.SequentialNumber,
            ReferenceNumber = dto.ReferenceNumber.Trim(),
            CorrespondenceDate = dto.CorrespondenceDate,
            Subject = dto.Subject.Trim(),
            PageNumber = dto.PageNumber?.Trim(),
            ExternalPartyID = dto.ExternalPartyID,
            EmployeeID = dto.EmployeeID,
            CreatedByUserID = userId,
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Correspondences.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(entry.CorrespondenceID);
    }

    public async Task<ApiResponse<CorrespondenceDetailsDto>> UpdateAsync(int id, UpdateCorrespondenceDto dto)
    {
        var entry = await _unitOfWork.Correspondences.GetByIdAsync(id);
        if (entry == null) return ApiResponse<CorrespondenceDetailsDto>.Fail("المراسلة غير موجودة");

        if (string.IsNullOrWhiteSpace(dto.Subject)) return ApiResponse<CorrespondenceDetailsDto>.Fail("الموضوع مطلوب");
        if (string.IsNullOrWhiteSpace(dto.ReferenceNumber)) return ApiResponse<CorrespondenceDetailsDto>.Fail("رقم القيد مطلوب");

        entry.Direction = dto.Direction;
        entry.CorrespondenceTypeID = dto.CorrespondenceTypeID;
        entry.SequentialNumber = dto.SequentialNumber;
        entry.ReferenceNumber = dto.ReferenceNumber.Trim();
        entry.CorrespondenceDate = dto.CorrespondenceDate;
        entry.Subject = dto.Subject.Trim();
        entry.PageNumber = dto.PageNumber?.Trim();
        entry.ExternalPartyID = dto.ExternalPartyID;
        entry.EmployeeID = dto.EmployeeID;
        entry.Notes = dto.Notes?.Trim();
        entry.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Correspondences.Update(entry);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entry = await _unitOfWork.Correspondences.GetByIdAsync(id);
        if (entry == null) return ApiResponse<bool>.Fail("المراسلة غير موجودة");

        entry.IsDeleted = true;
        entry.DeletedAt = DateTime.UtcNow;

        _unitOfWork.Correspondences.Update(entry);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف المراسلة بنجاح");
    }

    // ─── External Parties ────────────────────────────────────

    public async Task<ApiResponse<List<ExternalPartyDto>>> GetExternalPartiesAsync()
    {
        var result = await _unitOfWork.ExternalParties.Query()
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Select(p => new ExternalPartyDto
            {
                ExternalPartyID = p.ExternalPartyID,
                Name = p.Name,
                PartyType = p.PartyType,
                ContactInfo = p.ContactInfo,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return ApiResponse<List<ExternalPartyDto>>.Success(result);
    }

    public async Task<ApiResponse<ExternalPartyDto>> CreateExternalPartyAsync(CreateExternalPartyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return ApiResponse<ExternalPartyDto>.Fail("الاسم مطلوب");

        var party = new ExternalParty
        {
            Name = dto.Name.Trim(),
            PartyType = dto.PartyType.Trim(),
            ContactInfo = dto.ContactInfo?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ExternalParties.AddAsync(party);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ExternalPartyDto>.Success(new ExternalPartyDto
        {
            ExternalPartyID = party.ExternalPartyID,
            Name = party.Name,
            PartyType = party.PartyType,
            ContactInfo = party.ContactInfo,
            IsActive = party.IsActive
        });
    }

    public async Task<ApiResponse<ExternalPartyDto>> UpdateExternalPartyAsync(int id, UpdateExternalPartyDto dto)
    {
        var party = await _unitOfWork.ExternalParties.GetByIdAsync(id);
        if (party == null) return ApiResponse<ExternalPartyDto>.Fail("الجهة غير موجودة");

        if (string.IsNullOrWhiteSpace(dto.Name)) return ApiResponse<ExternalPartyDto>.Fail("الاسم مطلوب");

        party.Name = dto.Name.Trim();
        party.PartyType = dto.PartyType.Trim();
        party.ContactInfo = dto.ContactInfo?.Trim();
        party.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ExternalParties.Update(party);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ExternalPartyDto>.Success(new ExternalPartyDto
        {
            ExternalPartyID = party.ExternalPartyID,
            Name = party.Name,
            PartyType = party.PartyType,
            ContactInfo = party.ContactInfo,
            IsActive = party.IsActive
        });
    }

    public async Task<ApiResponse<bool>> DeleteExternalPartyAsync(int id)
    {
        var party = await _unitOfWork.ExternalParties.GetByIdAsync(id);
        if (party == null) return ApiResponse<bool>.Fail("الجهة غير موجودة");

        // Check if party is used in correspondences
        if (await _unitOfWork.Correspondences.Query().AnyAsync(c => c.ExternalPartyID == id && !c.IsDeleted))
            return ApiResponse<bool>.Fail("لا يمكن حذف هذه الجهة لوجود مراسلات مرتبطة بها");

        party.IsDeleted = true;
        party.DeletedAt = DateTime.UtcNow;

        _unitOfWork.ExternalParties.Update(party);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف الجهة بنجاح");
    }

    // ─── Correspondence Types ────────────────────────────────

    public async Task<ApiResponse<List<CorrespondenceTypeDto>>> GetTypesAsync()
    {
        var result = await _unitOfWork.CorrespondenceTypes.Query()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new CorrespondenceTypeDto
            {
                CorrespondenceTypeID = t.CorrespondenceTypeID,
                Code = t.Code,
                Name = t.Name
            })
            .ToListAsync();

        return ApiResponse<List<CorrespondenceTypeDto>>.Success(result);
    }

    // ─── Documents ──────────────────────────────────────────

    public async Task<ApiResponse<List<DocumentDto>>> GetDocumentsAsync(int correspondenceId)
    {
        var docs = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => d.CorrespondenceID == correspondenceId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentDto
            {
                DocumentID = d.DocumentID,
                DocumentTypeID = d.DocumentTypeID,
                DocumentTypeName = d.DocumentType.TypeName,
                FileName = d.FileName,
                FilePath = d.FilePath,
                FileExtension = Path.GetExtension(d.FileName),
                FileSize = d.FileSize,
                UploadedByUser = d.UploadedByUser != null ? d.UploadedByUser.FullName : null,
                CreatedAt = d.CreatedAt,
                CorrespondenceID = d.CorrespondenceID
            })
            .ToListAsync();

        return ApiResponse<List<DocumentDto>>.Success(docs);
    }

    public async Task<ApiResponse<DocumentDto>> UploadDocumentAsync(int correspondenceId, IRemoteFile file, int userId)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<DocumentDto>.Fail("لم يتم اختيار ملف");

        var correspondence = await _unitOfWork.Correspondences.GetByIdAsync(correspondenceId);
        if (correspondence == null)
            return ApiResponse<DocumentDto>.Fail("المراسلة غير موجودة");

        try
        {
            using var stream = file.OpenReadStream();
            var relativePath = await _fileStorage.SaveFileAsync(stream, file.FileName, "correspondence");

            var doc = new Document
            {
                CorrespondenceID = correspondenceId,
                DocumentTypeID = DocumentTypeConstants.FileOriginals,
                FileName = file.FileName,
                FilePath = relativePath,
                FileSize = file.Length,
                MimeType = file.ContentType,
                UploadedByUserID = userId,
                OwnerType = DocumentOwnerType.Correspondence,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Documents.AddAsync(doc);
            await _unitOfWork.SaveChangesAsync();

            // Refresh to get navigation properties for the DTO
            var createdDoc = await _unitOfWork.Documents.Query()
                .Include(d => d.DocumentType)
                .Include(d => d.UploadedByUser)
                .FirstOrDefaultAsync(d => d.DocumentID == doc.DocumentID);

            return ApiResponse<DocumentDto>.Success(new DocumentDto
            {
                DocumentID = doc.DocumentID,
                DocumentTypeID = doc.DocumentTypeID,
                DocumentTypeName = createdDoc?.DocumentType?.TypeName ?? "وثيقة رسمية",
                FileName = doc.FileName,
                FilePath = doc.FilePath,
                FileExtension = Path.GetExtension(doc.FileName),
                FileSize = doc.FileSize,
                UploadedByUser = createdDoc?.UploadedByUser?.FullName,
                CreatedAt = doc.CreatedAt,
                CorrespondenceID = doc.CorrespondenceID
            }, "تم رفع الملف بنجاح");
        }
        catch (Exception ex)
        {
            return ApiResponse<DocumentDto>.Fail("حدث خطأ أثناء رفع الملف. يرجى المحاولة لاحقاً");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDocumentAsync(int correspondenceId, int documentId)
    {
        var doc = await _unitOfWork.Documents.Query()
            .FirstOrDefaultAsync(d => d.DocumentID == documentId && d.CorrespondenceID == correspondenceId);

        if (doc == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        
        _unitOfWork.Documents.Update(doc);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف الملف بنجاح");
    }
}
