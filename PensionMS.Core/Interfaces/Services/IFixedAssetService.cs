using PensionMS.Core.DTOs.Assets;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Core.Interfaces.Services;

public interface IFixedAssetService
{
    Task<ApiResponse<List<FixedAssetDto>>> GetAllAsync();
    Task<ApiResponse<FixedAssetDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<FixedAssetDto>>> GetByTypeAsync(int typeId);
    Task<ApiResponse<List<AssetTypeDto>>> GetTypesAsync();
    Task<ApiResponse<List<AssetCategoryDto>>> GetCategoriesAsync(int? typeId = null);
    Task<ApiResponse<FixedAssetDto>> CreateAsync(CreateFixedAssetDto dto, int userId);
    Task<ApiResponse<FixedAssetDto>> UpdateAsync(int id, UpdateFixedAssetDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
    Task<ApiResponse<string>> GenerateAssetNumberAsync(int assetTypeId);
    Task<ApiResponse<List<DepreciationScheduleDto>>> GetScheduleAsync(int assetId);
    Task<ApiResponse<bool>> RunYearlyDepreciationAsync(int year, int userId);
    Task<ApiResponse<bool>> PostYearlyDepreciationAsync(int year, int userId);
    Task<ApiResponse<int>> ResetCategoriesAsync();

    // Category CRUD
    Task<ApiResponse<AssetCategoryDto>> CreateCategoryAsync(CreateAssetCategoryDto dto, int userId);
    Task<ApiResponse<AssetCategoryDto>> UpdateCategoryAsync(int id, UpdateAssetCategoryDto dto, int userId);
    Task<ApiResponse<bool>> DeleteCategoryAsync(int id, int userId);
    Task<ApiResponse<bool>> ToggleCategoryActiveAsync(int id, int userId);
}
