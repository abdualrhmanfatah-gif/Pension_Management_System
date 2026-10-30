using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;

namespace PensionMS.Core.Interfaces;

public interface IDocumentTypeService
{
    Task<ApiResponse<List<DocumentTypeDto>>> GetAllAsync(int? ownerType);
    Task<ApiResponse<List<DocumentTypeDto>>> GetByOwnerTypeAsync(int ownerType);
    Task<ApiResponse<DocumentTypeDto>> GetByIdAsync(int id);
    Task<ApiResponse<DocumentTypeDto>> CreateAsync(CreateDocumentTypeDto dto, int userId);
    Task<ApiResponse<DocumentTypeDto>> UpdateAsync(int id, UpdateDocumentTypeDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
}
