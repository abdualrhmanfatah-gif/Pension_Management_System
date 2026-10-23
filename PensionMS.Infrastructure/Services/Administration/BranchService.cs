using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Branches;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Administration;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public BranchService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<List<BranchDto>> GetAllAsync()
    {
        return await _unitOfWork.PensionFundBranches.Query()
            .AsNoTracking()
            .Select(b => new BranchDto
            {
                PensionBranchID = b.PensionBranchID,
                BranchName = b.BranchName,
                Governorate = b.Governorate,
                Address = b.Address,
                Phone = b.Phone,
                ManagerName = b.ManagerName,
                ManagerPhone = b.ManagerPhone,
                IsActive = b.IsActive,
                UsersCount = b.Users.Count,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<BranchDto?> GetByIdAsync(int id)
    {
        var b = await _unitOfWork.PensionFundBranches.GetByIdAsync(id);
        if (b == null) return null;

        // Note: Users Count is needed in MapToDto, but GetByIdAsync doesn't include it.
        // We'll manual map for now as per DTO needs or use Query().
        var branchWithUsers = await _unitOfWork.PensionFundBranches.Query()
            .AsNoTracking()
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x => x.PensionBranchID == id);

        return branchWithUsers == null ? null : MapToDto(branchWithUsers);
    }

    public async Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchDto dto, int userId)
    {
        var branch = new PensionFundBranch
        {
            BranchName = dto.BranchName,
            Governorate = dto.Governorate,
            Address = dto.Address,
            Phone = dto.Phone,
            ManagerName = dto.ManagerName,
            ManagerPhone = dto.ManagerPhone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PensionFundBranches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "PensionFundBranch", branch.PensionBranchID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        return ApiResponse<BranchDto>.Success(MapToDto(branch), "تم إضافة الفرع بنجاح");
    }

    public async Task<ApiResponse<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto, int userId)
    {
        var branch = await _unitOfWork.PensionFundBranches.GetByIdAsync(id);
        if (branch == null) return ApiResponse<BranchDto>.Fail("الفرع غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(branch);

        if (dto.BranchName != null) branch.BranchName = dto.BranchName;
        if (dto.Governorate != null) branch.Governorate = dto.Governorate;
        if (dto.Address != null) branch.Address = dto.Address;
        if (dto.Phone != null) branch.Phone = dto.Phone;
        if (dto.ManagerName != null) branch.ManagerName = dto.ManagerName;
        if (dto.ManagerPhone != null) branch.ManagerPhone = dto.ManagerPhone;
        if (dto.IsActive.HasValue) branch.IsActive = dto.IsActive.Value;

        branch.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.PensionFundBranches.Update(branch);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Update", "PensionFundBranch", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        return ApiResponse<BranchDto>.Success(MapToDto(branch), "تم تحديث بيانات الفرع بنجاح");
    }

    public async Task<ApiResponse<bool>> ToggleActiveAsync(int id, int userId)
    {
        var branch = await _unitOfWork.PensionFundBranches.GetByIdAsync(id);
        if (branch == null) return ApiResponse<bool>.Fail("الفرع غير موجود");

        var oldValues = branch.IsActive.ToString();
        branch.IsActive = !branch.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.PensionFundBranches.Update(branch);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "ToggleActive", "PensionFundBranch", id, oldValues, branch.IsActive.ToString());

        return ApiResponse<bool>.Success(true, "تم تغيير حالة الفرع بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var branch = await _unitOfWork.PensionFundBranches.Query()
            .Include(b => b.Users)
            .Include(b => b.RetirementFiles)
            .FirstOrDefaultAsync(b => b.PensionBranchID == id);

        if (branch == null) return ApiResponse<bool>.Fail("الفرع غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(branch);

        // If it has dependencies, preventing hard delete or just deactivate
        if (branch.Users.Any() || branch.RetirementFiles.Any())
        {
            branch.IsActive = false; 
            branch.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PensionFundBranches.Update(branch);
            await _auditService.LogAsync(userId, "", "Deactivate", "PensionFundBranch", id, oldValues, "Has dependencies - deactivated");
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "تم إلغاء تفعيل الفرع لوجود مستخدمين أو ملفات مرتبطة به");
        }
        else
        {
            _unitOfWork.PensionFundBranches.Delete(branch);
            await _auditService.LogAsync(userId, "", "Delete", "PensionFundBranch", id, oldValues, null);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "تم حذف الفرع بنجاح");
        }
    }

    private static BranchDto MapToDto(PensionFundBranch b) => new()
    {
        PensionBranchID = b.PensionBranchID,
        BranchName = b.BranchName,
        Governorate = b.Governorate,
        Address = b.Address,
        Phone = b.Phone,
        ManagerName = b.ManagerName,
        ManagerPhone = b.ManagerPhone,
        IsActive = b.IsActive,
        UsersCount = b.Users?.Count ?? 0,
        CreatedAt = b.CreatedAt
    };
}
