using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Correspondence;
using PensionMS.Core.DTOs.Documents;

namespace PensionMS.Core.Interfaces;

public interface ICorrespondenceService
{
    // ─── Correspondences ─────────────────────────────────────
    
    Task<ApiResponse<PagedResult<CorrespondenceListItemDto>>> GetAllAsync(
        byte? direction = null, int? typeId = null, 
        int page = 1, int pageSize = 20, string? search = null);
        
    Task<ApiResponse<CorrespondenceDetailsDto>> GetByIdAsync(int id);
    
    Task<ApiResponse<CorrespondenceDetailsDto>> CreateAsync(CreateCorrespondenceDto dto, int userId);
    
    Task<ApiResponse<CorrespondenceDetailsDto>> UpdateAsync(int id, UpdateCorrespondenceDto dto);
    
    Task<ApiResponse<bool>> DeleteAsync(int id);
    
    // ─── External Parties ────────────────────────────────────
    
    Task<ApiResponse<List<ExternalPartyDto>>> GetExternalPartiesAsync();
    
    Task<ApiResponse<ExternalPartyDto>> CreateExternalPartyAsync(CreateExternalPartyDto dto);
    
    Task<ApiResponse<ExternalPartyDto>> UpdateExternalPartyAsync(int id, UpdateExternalPartyDto dto);
    
    Task<ApiResponse<bool>> DeleteExternalPartyAsync(int id);
    
    // ─── Correspondence Types ────────────────────────────────
    
    Task<ApiResponse<List<CorrespondenceTypeDto>>> GetTypesAsync();

    // ─── Documents ──────────────────────────────────────────
    
    Task<ApiResponse<List<DocumentDto>>> GetDocumentsAsync(int correspondenceId);
    
    Task<ApiResponse<DocumentDto>> UploadDocumentAsync(int correspondenceId, IRemoteFile file, int userId);
    
    Task<ApiResponse<bool>> DeleteDocumentAsync(int id, int documentId);
}
