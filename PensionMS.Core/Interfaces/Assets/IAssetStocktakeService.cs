using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Assets;

namespace PensionMS.Core.Interfaces.Assets;

public interface IAssetStocktakeService
{
    Task<ApiResponse<AssetStocktakeDto>> CreateStocktakeAsync(int fiscalYearId, int userId);
    Task<ApiResponse<bool>> RecordActualCountAsync(int stocktakeId, List<AssetStocktakeLineDto> lines, int userId);
    Task<ApiResponse<AssetStocktakeSummaryDto>> CompleteStocktakeAsync(int stocktakeId, int userId);
    Task<AssetStocktakeDto?> GetLatestStocktakeAsync();
    Task<AssetStocktakeDto?> GetStocktakeByFiscalYearAsync(int fiscalYearId);
    Task<AssetStocktakeDto?> GetStocktakeAsync(int stocktakeId);
    Task<List<AssetStocktakeDto>> GetAllStocktakesAsync(int fiscalYearId);
}
