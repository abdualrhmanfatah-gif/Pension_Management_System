using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

public class JournalEntryTypeService : IJournalEntryTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public JournalEntryTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<JournalEntryTypeDto>>> GetAllAsync()
    {
        var types = await _unitOfWork.JournalEntryTypes.Query()
            .Where(t => t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.TypeName)
            .ToListAsync();

        return ApiResponse<List<JournalEntryTypeDto>>.Success(types.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<JournalEntryTypeDto>>> GetAllIncludingInactiveAsync()
    {
        var types = await _unitOfWork.JournalEntryTypes.Query()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.TypeName)
            .ToListAsync();

        return ApiResponse<List<JournalEntryTypeDto>>.Success(types.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<JournalEntryTypeDto>> GetByIdAsync(int id)
    {
        var type = await _unitOfWork.JournalEntryTypes.GetByIdAsync(id);
        if (type == null || type.IsDeleted)
            return ApiResponse<JournalEntryTypeDto>.Fail("نوع القيد غير موجود");

        return ApiResponse<JournalEntryTypeDto>.Success(MapToDto(type));
    }

    public async Task<ApiResponse<JournalEntryTypeDto>> CreateAsync(CreateJournalEntryTypeDto dto)
    {
        var existingByName = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.TypeName == dto.TypeName && !t.IsDeleted);
        if (existingByName != null)
            return ApiResponse<JournalEntryTypeDto>.Fail("يوجد نوع قيد بهذا الاسم بالفعل");

        if (!string.IsNullOrEmpty(dto.Code))
        {
            var existingByCode = await _unitOfWork.JournalEntryTypes.Query()
                .FirstOrDefaultAsync(t => t.Code == dto.Code && !t.IsDeleted);
            if (existingByCode != null)
                return ApiResponse<JournalEntryTypeDto>.Fail("يوجد نوع قيد بهذا الكود بالفعل");
        }

        var entity = new JournalEntryType
        {
            TypeName = dto.TypeName,
            TypeNameEn = dto.TypeNameEn,
            Code = dto.Code,
            Color = dto.Color,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.JournalEntryTypes.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<JournalEntryTypeDto>.Success(MapToDto(entity));
    }

    public async Task<ApiResponse<JournalEntryTypeDto>> UpdateAsync(int id, UpdateJournalEntryTypeDto dto)
    {
        var entity = await _unitOfWork.JournalEntryTypes.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<JournalEntryTypeDto>.Fail("نوع القيد غير موجود");

        var existingByName = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.TypeName == dto.TypeName && t.JournalEntryTypeID != id && !t.IsDeleted);
        if (existingByName != null)
            return ApiResponse<JournalEntryTypeDto>.Fail("يوجد نوع قيد بهذا الاسم بالفعل");

        if (!string.IsNullOrEmpty(dto.Code))
        {
            var existingByCode = await _unitOfWork.JournalEntryTypes.Query()
                .FirstOrDefaultAsync(t => t.Code == dto.Code && t.JournalEntryTypeID != id && !t.IsDeleted);
            if (existingByCode != null)
                return ApiResponse<JournalEntryTypeDto>.Fail("يوجد نوع قيد بهذا الكود بالفعل");
        }

        entity.TypeName = dto.TypeName;
        entity.TypeNameEn = dto.TypeNameEn;
        entity.Code = dto.Code;
        entity.Color = dto.Color;
        entity.Description = dto.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<JournalEntryTypeDto>.Success(MapToDto(entity));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.JournalEntryTypes.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<bool>.Fail("نوع القيد غير موجود");

        var hasEntries = await _unitOfWork.JournalEntries.Query()
            .AnyAsync(e => e.JournalEntryTypeID == id && !e.IsDeleted);
        if (hasEntries)
            return ApiResponse<bool>.Fail("لا يمكن حذف هذا النوع لارتباطه بقيود يومية");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    private static JournalEntryTypeDto MapToDto(JournalEntryType entity)
    {
        return new JournalEntryTypeDto
        {
            JournalEntryTypeID = entity.JournalEntryTypeID,
            TypeName = entity.TypeName,
            TypeNameEn = entity.TypeNameEn,
            Code = entity.Code,
            Color = entity.Color,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }
}
