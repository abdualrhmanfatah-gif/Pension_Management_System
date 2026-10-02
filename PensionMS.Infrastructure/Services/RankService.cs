using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Ranks;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class RankService : IRankService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public RankService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<List<RankDto>> GetAllAsync()
    {
        var ranks = await _unitOfWork.Ranks.Query()
            .AsNoTracking()
            .OrderBy(r => r.RankLevel)
            .ToListAsync();
            
        return ranks.Select(MapToDto).ToList();
    }

    public async Task<RankDto?> GetByIdAsync(int id)
    {
        var rank = await _unitOfWork.Ranks.GetByIdAsync(id);
        return rank == null ? null : MapToDto(rank);
    }

    public async Task<ApiResponse<RankDto>> CreateAsync(CreateRankDto dto, int userId)
    {
        var rank = new Rank
        {
            RankName = dto.RankName,
            RankLevel = dto.RankLevel,
            BaseSalary = dto.BaseSalary,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Ranks.AddAsync(rank);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "Rank", rank.RankID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        return ApiResponse<RankDto>.Success(MapToDto(rank), "تم إضافة الرتبة بنجاح");
    }

    public async Task<ApiResponse<RankDto>> UpdateAsync(int id, UpdateRankDto dto, int userId)
    {
        var rank = await _unitOfWork.Ranks.GetByIdAsync(id);
        if (rank == null) return ApiResponse<RankDto>.Fail("الرتبة غير موجودة");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(rank);

        if (dto.RankName != null) rank.RankName = dto.RankName;
        if (dto.RankLevel.HasValue) rank.RankLevel = dto.RankLevel.Value;
        if (dto.BaseSalary.HasValue) rank.BaseSalary = dto.BaseSalary.Value;

        rank.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Ranks.Update(rank);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Update", "Rank", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        return ApiResponse<RankDto>.Success(MapToDto(rank), "تم تحديث بيانات الرتبة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var rank = await _unitOfWork.Ranks.Query()
            .Include(r => r.RetirementFiles)
            .FirstOrDefaultAsync(r => r.RankID == id);

        if (rank == null) return ApiResponse<bool>.Fail("الرتبة غير موجودة");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(rank);

        var hasMinistryApprovalItems = await _unitOfWork.MinistryApprovalItems.Query()
            .AnyAsync(a => a.RankID == id && !a.IsDeleted);

        if (rank.RetirementFiles.Any() || hasMinistryApprovalItems)
        {
            return ApiResponse<bool>.Fail("لا يمكن حذف الرتبة لوجود ملفات أو بيانات مرتبطة بها");
        }

        _unitOfWork.Ranks.Delete(rank);
        await _auditService.LogAsync(userId, "", "Delete", "Rank", id, oldValues, null);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم حذف الرتبة بنجاح");
    }

    private static RankDto MapToDto(Rank r) => new()
    {
        RankID = r.RankID,
        RankName = r.RankName,
        RankLevel = r.RankLevel,
        BaseSalary = r.BaseSalary,
        CreatedAt = r.CreatedAt
    };
}
