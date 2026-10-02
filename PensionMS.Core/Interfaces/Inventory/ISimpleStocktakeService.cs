using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Accounting;

namespace PensionMS.Core.Interfaces.Inventory;

public interface ISimpleStocktakeService
{
    Task<ApiResponse<SimpleStocktakeDto>> CreateStocktakeAsync(int userId);
    Task<ApiResponse<bool>> RecordPhysicalCountAsync(int stocktakeId, Dictionary<int, int> actualCounts, int userId);
    Task<ApiResponse<JournalEntryDto>> AdjustStocktakeAsync(int stocktakeId, int userId);
    Task<SimpleStocktakeDto?> GetLatestStocktakeAsync();
    Task<SimpleStocktakeDto?> GetStocktakeByFiscalYearAsync(int fiscalYearId);
    Task<SimpleStocktakeDto?> GetStocktakeAsync(int stocktakeId);
    Task<List<SimpleStocktakeDto>> GetAllStocktakesAsync(int fiscalYearId);
}
