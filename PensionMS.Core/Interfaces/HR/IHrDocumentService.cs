using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Interfaces;

public interface IHrDocumentService
{
    Task<ApiResponse<List<DocumentDto>>> GetEmployeeDocumentsAsync(int employeeId);
    Task<ApiResponse<DocumentDto>> UploadEmployeeDocumentAsync(UploadDocumentDto dto, int uploadedByUserId);
    Task<ApiResponse<bool>> DeleteDocumentAsync(int id, int userId);
}
