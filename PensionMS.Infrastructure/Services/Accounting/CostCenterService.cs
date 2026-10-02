using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

public class CostCenterService : ICostCenterService
{
    private readonly IUnitOfWork _unitOfWork;

    public CostCenterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<CostCenterDto>>> GetAllCostCentersAsync()
    {
        var list = await _unitOfWork.CostCenters.Query()
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Code)
            .ToListAsync();
        return ApiResponse<List<CostCenterDto>>.Success(list.Select(MapToCostCenterDto).ToList());
    }

    public async Task<ApiResponse<CostCenterDto>> GetCostCenterByIdAsync(int id)
    {
        var entity = await _unitOfWork.CostCenters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CostCenterID == id && !c.IsDeleted);
        if (entity == null)
            return ApiResponse<CostCenterDto>.Fail("مركز التكلفة غير موجود");
        return ApiResponse<CostCenterDto>.Success(MapToCostCenterDto(entity));
    }

    public async Task<ApiResponse<CostCenterDto>> CreateCostCenterAsync(CreateCostCenterDto dto)
    {
        var exists = await _unitOfWork.CostCenters.Query()
            .AnyAsync(c => c.Code == dto.Code && !c.IsDeleted);
        if (exists)
            return ApiResponse<CostCenterDto>.Fail("رمز مركز التكلفة موجود مسبقاً");

        var entity = new CostCenter
        {
            Code = dto.Code,
            Name = dto.Name,
            ParentCostCenterID = dto.ParentCostCenterID,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.CostCenters.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<CostCenterDto>.Success(MapToCostCenterDto(entity), "تم إنشاء مركز التكلفة بنجاح");
    }

    public async Task<ApiResponse<CostCenterDto>> UpdateCostCenterAsync(int id, UpdateCostCenterDto dto)
    {
        var entity = await _unitOfWork.CostCenters.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<CostCenterDto>.Fail("مركز التكلفة غير موجود");

        entity.Name = dto.Name;
        entity.ParentCostCenterID = dto.ParentCostCenterID;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CostCenters.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<CostCenterDto>.Success(MapToCostCenterDto(entity), "تم تحديث مركز التكلفة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeactivateCostCenterAsync(int id)
    {
        var entity = await _unitOfWork.CostCenters.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<bool>.Fail("مركز التكلفة غير موجود");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.CostCenters.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم إلغاء تنشيط مركز التكلفة");
    }

    private static CostCenterDto MapToCostCenterDto(CostCenter entity)
    {
        return new CostCenterDto
        {
            CostCenterID = entity.CostCenterID,
            Code = entity.Code,
            Name = entity.Name,
            ParentCostCenterID = entity.ParentCostCenterID,
            ParentCostCenterName = entity.ParentCostCenter?.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }
}
