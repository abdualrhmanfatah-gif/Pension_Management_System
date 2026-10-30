using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Retirees;

namespace PensionMS.Core.Interfaces;

public interface IRetireeService
{
    Task<ApiResponse<RetireeDto>> GetByIdAsync(int id);
    Task<ApiResponse<PagedResult<RetireeListDto>>> GetAllAsync(int page, int pageSize, string? search = null, SocialResearchQueryDto? filters = null);
    Task<ApiResponse<RetireeDto>> GetByNationalIDAsync(string nationalId);
    Task<ApiResponse<RetireeDto>> GetByMilitaryNumberAsync(string militaryNumber);

    Task<ApiResponse<RetireeDto>> UpdateAsync(int id, UpdateRetireeDto dto);
    Task<ApiResponse<string>> UploadPhotoAsync(int id, IRemoteFile photo);
    Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId);
    Task<ApiResponse<RetireeStatementDto>> GetStatementAsync(int id);
    Task<ApiResponse<RetireeFullDataDto>> GetFullDataAsync(int id);
}
