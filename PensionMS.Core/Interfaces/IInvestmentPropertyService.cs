using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;

namespace PensionMS.Core.Interfaces;

public interface IInvestmentPropertyService
{
    // Properties
    Task<ApiResponse<int>> CreatePropertyAsync(CreatePropertyDto dto, int userId);
    Task<ApiResponse<bool>> UpdatePropertyAsync(int id, UpdatePropertyDto dto, int userId);
    Task<ApiResponse<bool>> DeletePropertyAsync(int id, int userId);
    Task<ApiResponse<PropertyDetailDto>> GetPropertyByIdAsync(int id);
    Task<ApiResponse<List<PropertyDto>>> GetAllPropertiesAsync(int page = 1, int pageSize = 50);
    Task<ApiResponse<List<PropertyDto>>> GetPropertiesByProjectAsync(int projectId);
    Task<ApiResponse<bool>> UpdatePropertyMainPhotoPathAsync(int id, string photoPath, int userId);

    // Units
    Task<ApiResponse<int>> CreateUnitAsync(CreateUnitDto dto, int userId);
    Task<ApiResponse<bool>> UpdateUnitAsync(int id, UpdateUnitDto dto, int userId);
    Task<ApiResponse<bool>> DeleteUnitAsync(int id, int userId);
    Task<ApiResponse<List<PropertyUnitDto>>> GetUnitsByPropertyAsync(int propertyId);
    Task<ApiResponse<List<PropertyUnitDto>>> GetAllUnitsAsync();

    // Valuations
    Task<ApiResponse<int>> CreateValuationAsync(CreateValuationDto dto, int userId);
    Task<ApiResponse<List<ValuationDto>>> GetValuationsByPropertyAsync(int propertyId);
    Task<ApiResponse<bool>> DeleteValuationAsync(int id, int userId);

    // Depreciation
    Task<ApiResponse<int>> CreateDepreciationAsync(CreateDepreciationDto dto, int userId);
    Task<ApiResponse<List<DepreciationRecordDto>>> GetDepreciationByPropertyAsync(int propertyId);
    Task<ApiResponse<bool>> DeleteDepreciationAsync(int id, int userId);
}
