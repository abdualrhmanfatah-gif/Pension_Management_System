using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.DocumentExemptions;

namespace PensionMS.Core.Interfaces;

public interface IDocumentExemptionService
{
    Task<ApiResponse<List<DocumentExemptionDto>>> GetByFileAsync(int fileId);
    Task<ApiResponse<DocumentExemptionDto>> GetByIdAsync(int id);
    Task<ApiResponse<DocumentExemptionDto>> CreateAsync(CreateDocumentExemptionDto dto, int userId);
    Task<ApiResponse<DocumentExemptionDto>> UpdateAsync(int id, UpdateDocumentExemptionDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
}
