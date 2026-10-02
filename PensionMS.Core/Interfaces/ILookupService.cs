using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface ILookupService
{
    Task<ApiResponse<List<LookupDto>>> GetRanksAsync();
    Task<ApiResponse<List<LookupDto>>> GetBanksAsync();
    Task<ApiResponse<List<LookupDto>>> GetRelationshipsAsync();
    Task<ApiResponse<List<LookupDto>>> GetDocumentTypesAsync();
    Task<ApiResponse<List<LookupDto>>> GetFileStatusesAsync();
    Task<ApiResponse<List<LookupDto>>> GetSocialStatusesAsync();
    Task<ApiResponse<List<LookupDto>>> GetHealthStatusesAsync();
    Task<ApiResponse<List<LookupDto>>> GetRetireeCategoriesAsync();
    Task<ApiResponse<List<LookupDto>>> GetBranchesAsync();
    Task<ApiResponse<List<LookupDto>>> GetRolesAsync();
    Task<ApiResponse<List<LookupDto>>> GetTransactionStatusesAsync();
    Task<ApiResponse<List<LookupDto>>> GetGovernoratesAsync();
    Task<ApiResponse<List<LookupDto>>> GetUnitsAsync();
}
