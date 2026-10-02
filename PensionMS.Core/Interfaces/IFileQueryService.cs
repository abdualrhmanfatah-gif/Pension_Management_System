using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.RetirementFiles;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة استعلامات ملفات التقاعد — عمليات القراءة فقط (CQRS: Query side)
/// </summary>
public interface IFileQueryService
{
    Task<ApiResponse<RetirementFileDto>> GetByIdAsync(int id);
    Task<ApiResponse<PagedResult<RetirementFileListDto>>> GetAllAsync(int page, int pageSize, string? search = null, int? statusId = null, int? userId = null, string? statusIds = null, int? uploadedBy = null, int? cursor = null, string? period = null, DateTime? fromDate = null, DateTime? toDate = null, int? categoryId = null, int? rankId = null, string? governorate = null, byte? gender = null, string? unitName = null, bool? hasSettlements = null);
    Task<ApiResponse<RetirementFileDto>> GetByFileNumberAsync(string fileNumber);
    Task<ApiResponse<List<RetirementFileListDto>>> GetByRetireeIdAsync(int retireeId);
    Task<ApiResponse<List<StatusTransitionDto>>> GetStatusTransitionsAsync(int fileId);
    Task<ApiResponse<List<FileChangeLogDto>>> GetFileChangeLogAsync(int fileId, int page = 1, int pageSize = 200);

    // Role-based query endpoints
    Task<ApiResponse<PagedResult<RetirementFileListDto>>> GetForDataEntryAsync(int page, int pageSize, string? search = null);
    Task<ApiResponse<List<RetirementFileDocumentUploadDto>>> GetForDocumentUploadAsync();
    Task<ApiResponse<DocumentUploadStatsDto>> GetDocumentUploadStatsAsync();
    Task<ApiResponse<List<UploaderStatsDto>>> GetUploaderStatsAsync(UploaderStatsFilterDto filter);
    Task<ApiResponse<List<DataEntryStatsDto>>> GetDataEntryStatsAsync(UploaderStatsFilterDto filter);
    Task<ApiResponse<PagedResult<RetirementFileListDto>>> GetDataEntryFilesByUserAsync(int userId, int page, int pageSize, DateTime? fromDate = null, DateTime? toDate = null);
}
