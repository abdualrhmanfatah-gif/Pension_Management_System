using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.RetirementFiles;

namespace PensionMS.Core.Interfaces;

public interface IFileReviewService
{
    Task<ApiResponse<List<RetirementFileReviewDto>>> GetPendingReviewsAsync();
    Task<ApiResponse<FileReviewDiffDto>> GetReviewDiffAsync(int fileId);
    Task<ApiResponse<bool>> SubmitReviewAsync(int fileId, int userId);
    Task<ApiResponse<bool>> ReinstateEntitlementAsync(int fileId, int userId);
    Task<ApiResponse<bool>> RequestReviewAsync(int fileId, int userId, DateTime? requestedAt = null);
    Task<ApiResponse<bool>> ApproveReviewAsync(int fileId, int userId, string? notes = null);
    Task<ApiResponse<bool>> RejectReviewAsync(int fileId, int userId, string reason);
    Task<ApiResponse<bool>> ApproveItemAsync(long itemId, int userId, string? notes = null);
    Task<ApiResponse<bool>> RejectItemAsync(long itemId, int userId, string reason);
    Task AutoResolveFileReviewAsync(int fileId);
    Task<ApiResponse<bool>> AcknowledgeEntityRejectionAsync(string entityType, int entityId, int userId);
}
