using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Documents;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public DocumentTypeService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetAllAsync(int? ownerType)
    {
        var query = _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .AsQueryable();

        if (ownerType.HasValue)
        {
            query = query.Where(t => t.OwnerType != null && (int)t.OwnerType == ownerType.Value && t.IsActive);
        }

        var types = await query
            .OrderBy(t => t.SortOrder)
            .Select(t => new DocumentTypeDto
            {
                DocumentTypeID = t.DocumentTypeID,
                TypeName = t.TypeName,
                TypeNameEn = t.TypeNameEn,
                SystemCode = t.SystemCode,
                OwnerType = t.OwnerType.HasValue ? (int)t.OwnerType.Value : null,
                IsRequired = t.IsRequired,
                HasExpiry = t.HasExpiry,
                MaxFileSize = t.MaxFileSize,
                AllowedExtensions = t.AllowedExtensions,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return ApiResponse<List<DocumentTypeDto>>.Success(types);
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetByOwnerTypeAsync(int ownerType)
    {
        var types = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(t => t.OwnerType != null && (int)t.OwnerType == ownerType && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .Select(t => new DocumentTypeDto
            {
                DocumentTypeID = t.DocumentTypeID,
                TypeName = t.TypeName,
                TypeNameEn = t.TypeNameEn,
                SystemCode = t.SystemCode,
                OwnerType = t.OwnerType.HasValue ? (int)t.OwnerType.Value : null,
                IsRequired = t.IsRequired,
                HasExpiry = t.HasExpiry,
                MaxFileSize = t.MaxFileSize,
                AllowedExtensions = t.AllowedExtensions,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return ApiResponse<List<DocumentTypeDto>>.Success(types);
    }

    public async Task<ApiResponse<DocumentTypeDto>> GetByIdAsync(int id)
    {
        var t = await _unitOfWork.DocumentTypes.GetByIdAsync(id);
        if (t == null) return ApiResponse<DocumentTypeDto>.Fail("نوع الوثيقة غير موجود");

        var dto = new DocumentTypeDto
        {
            DocumentTypeID = t.DocumentTypeID,
            TypeName = t.TypeName,
            TypeNameEn = t.TypeNameEn,
            SystemCode = t.SystemCode,
            OwnerType = t.OwnerType.HasValue ? (int)t.OwnerType.Value : null,
            IsRequired = t.IsRequired,
            HasExpiry = t.HasExpiry,
            MaxFileSize = t.MaxFileSize,
            AllowedExtensions = t.AllowedExtensions,
            IsActive = t.IsActive
        };

        return ApiResponse<DocumentTypeDto>.Success(dto);
    }

    public async Task<ApiResponse<DocumentTypeDto>> CreateAsync(CreateDocumentTypeDto dto, int userId)
    {
        var type = new DocumentType
        {
            TypeName = dto.TypeName,
            OwnerType = dto.OwnerType.HasValue ? (DocumentOwnerType?)dto.OwnerType.Value : null,
            IsRequired = dto.IsRequired,
            HasExpiry = dto.HasExpiry,
            MaxFileSize = dto.MaxFileSize,
            AllowedExtensions = dto.AllowedExtensions,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.DocumentTypes.AddAsync(type);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "DocumentType", type.DocumentTypeID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        var resultDto = new DocumentTypeDto
        {
            DocumentTypeID = type.DocumentTypeID,
            TypeName = type.TypeName,
            OwnerType = type.OwnerType.HasValue ? (int)type.OwnerType.Value : null,
            IsRequired = type.IsRequired,
            HasExpiry = type.HasExpiry,
            MaxFileSize = type.MaxFileSize,
            AllowedExtensions = type.AllowedExtensions,
            IsActive = true
        };

        return ApiResponse<DocumentTypeDto>.Success(resultDto, "تم إضافة نوع الوثيقة بنجاح");
    }

    public async Task<ApiResponse<DocumentTypeDto>> UpdateAsync(int id, UpdateDocumentTypeDto dto, int userId)
    {
        var type = await _unitOfWork.DocumentTypes.GetByIdAsync(id);
        if (type == null) return ApiResponse<DocumentTypeDto>.Fail("نوع الوثيقة غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(type);

        if (dto.TypeName != null) type.TypeName = dto.TypeName;
        if (dto.OwnerType.HasValue) type.OwnerType = (DocumentOwnerType)dto.OwnerType.Value;
        if (dto.IsRequired.HasValue) type.IsRequired = dto.IsRequired.Value;
        if (dto.HasExpiry.HasValue) type.HasExpiry = dto.HasExpiry.Value;
        if (dto.MaxFileSize.HasValue) type.MaxFileSize = dto.MaxFileSize.Value;
        if (dto.AllowedExtensions != null) type.AllowedExtensions = dto.AllowedExtensions;
        if (dto.IsActive.HasValue) type.IsActive = dto.IsActive.Value;

        type.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.DocumentTypes.Update(type);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Update", "DocumentType", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        var resultDto = new DocumentTypeDto
        {
            DocumentTypeID = type.DocumentTypeID,
            TypeName = type.TypeName,
            OwnerType = type.OwnerType.HasValue ? (int)type.OwnerType.Value : null,
            IsRequired = type.IsRequired,
            HasExpiry = type.HasExpiry,
            MaxFileSize = type.MaxFileSize,
            AllowedExtensions = type.AllowedExtensions,
            IsActive = type.IsActive
        };

        return ApiResponse<DocumentTypeDto>.Success(resultDto, "تم تحديث نوع الوثيقة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var type = await _unitOfWork.DocumentTypes.Query()
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.DocumentTypeID == id);

        if (type == null) return ApiResponse<bool>.Fail("نوع الوثيقة غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(type);

        if (type.Documents.Any())
        {
             type.IsActive = false;
             type.UpdatedAt = DateTime.UtcNow;
             _unitOfWork.DocumentTypes.Update(type);
             await _auditService.LogAsync(userId, "", "Deactivate", "DocumentType", id, oldValues, "Has dependencies - deactivated");
             await _unitOfWork.SaveChangesAsync();
             return ApiResponse<bool>.Success(true, "تم تعطيل النوع لوجود وثائق مرتبطة به");
        }

        _unitOfWork.DocumentTypes.Delete(type);
        await _auditService.LogAsync(userId, "", "Delete", "DocumentType", id, oldValues, null);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم حذف النوع بنجاح");
    }
}
