using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Settlements;

namespace PensionMS.Core.Interfaces;

public interface ISettlementService
{
    // Decision-centric (primary UI flow)
    Task<ApiResponse<PagedResult<SettlementDecisionDto>>> GetAllDecisionsAsync(int page = 1, int pageSize = 20, string? searchTerm = null);
    Task<ApiResponse<SettlementDecisionDto>> GetDecisionByIdAsync(int decisionId);
    Task<ApiResponse<PagedResult<SettlementItemDto>>> GetDecisionItemsAsync(int decisionId, int page = 1, int pageSize = 20, string? searchTerm = null);
    Task<ApiResponse<SettlementDecisionDto>> CreateDecisionWithExcelAsync(ImportSettlementExcelDto dto, int userId);

    // Batch internals (kept for flexibility)
    Task<ApiResponse<List<SettlementBatchListDto>>> GetAllBatchesAsync();
    Task<ApiResponse<SettlementBatchListDto>> GetBatchByIdAsync(int id);
    Task<ApiResponse<SettlementBatchListDto>> CreateBatchAsync(CreateSettlementBatchDto dto, int userId);
    Task<ApiResponse<int>> ImportExcelAsync(int batchId, ImportSettlementExcelDto dto, int userId);
    Task<ApiResponse<List<SettlementItemDto>>> GetBatchItemsAsync(int batchId);
    Task<ApiResponse<bool>> ApproveBatchAsync(int id, int userId);
    Task<ApiResponse<bool>> DeleteBatchAsync(int id, int userId);

    // Decisions per batch
    Task<ApiResponse<List<SettlementDecisionDto>>> GetBatchDecisionsAsync(int batchId);
    Task<ApiResponse<SettlementDecisionDto>> CreateDecisionAsync(int batchId, CreateSettlementDecisionDto dto, int userId);
    Task<ApiResponse<bool>> DeleteDecisionAsync(int decisionId, int userId);

    // By retiree
    Task<ApiResponse<bool>> LinkItemToRetireeAsync(LinkSettlementItemDto dto, int userId);
    Task<ApiResponse<List<SettlementItemDto>>> GetByRetireeIdAsync(int retireeId);

    // Export
    Task<byte[]> ExportDecisionToExcelAsync(int decisionId);
    Task<byte[]> GenerateImportFailuresExcelAsync(IRemoteFile file, int decisionId);
    Task<byte[]> ExportBatchFailureReportAsync(int decisionId);
}
