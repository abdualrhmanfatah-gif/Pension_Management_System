using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Ministry;
using PensionMS.Core.DTOs.MinistryApprovals;

namespace PensionMS.Core.Interfaces;

public interface IMinistryApprovalService
{
    Task<ApiResponse<List<MinistryApprovalBatchDto>>> GetAllBatchesAsync();
    Task<ApiResponse<MinistryApprovalBatchDto>> GetBatchByIdAsync(int id);
    Task<ApiResponse<MinistryApprovalBatchDto>> CreateBatchAsync(CreateBatchDto dto, int userId);
    Task<ApiResponse<int>> ImportExcelAsync(int batchId, IRemoteFile file);
    Task<ApiResponse<PagedResult<MinistryApprovalItemDto>>> GetBatchItemsAsync(int batchId, int pageNumber, int pageSize, string? searchTerm);
    Task<ApiResponse<PagedResult<MinistryApprovalItemDto>>> GetAllItemsAsync(int pageNumber, int pageSize, string? searchTerm, bool? isProcessed);
    Task<ApiResponse<bool>> DeleteBatchAsync(int id, int userId);
    Task<ApiResponse<MinistryApprovalItemDto>> AddManualItemAsync(int batchId, AddManualItemDto dto, int userId);
    Task<ApiResponse<object>> ProcessItemAsync(int itemId, int processedByUserId);
    Task<ApiResponse<MinistryApprovalItemDto>> GetItemByIdAsync(int itemId);
    Task<ApiResponse<ForceDashboardStatsDto>> GetDashboardStatsAsync();
    Task<ApiResponse<List<ForceGlobalSearchDto>>> SearchGlobalForceAsync(string searchTerm);
    Task<ApiResponse<PagedResult<ReconciliationRowDto>>> GetReconciliationMatrixAsync(int batchId, int pageNumber, int pageSize);
    Task<ApiResponse<int>> ProcessBulkApprovalsAsync(List<int> approvalIds, int userId);
    Task<ApiResponse<List<ForceAuditLogDto>>> GetBatchAuditTrailAsync(int batchId);
    Task<byte[]> ExportBatchReportAsync(int batchId, string format);
}
