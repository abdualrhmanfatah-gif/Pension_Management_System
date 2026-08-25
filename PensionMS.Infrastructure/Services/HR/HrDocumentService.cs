using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.HR;

public class HrDocumentService : IHrDocumentService
{
    private readonly IDocumentService _documentService;

    public HrDocumentService(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetEmployeeDocumentsAsync(int employeeId)
    {
        return await _documentService.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<ApiResponse<DocumentDto>> UploadEmployeeDocumentAsync(UploadDocumentDto dto, int uploadedByUserId)
    {
        // Enforce employee ownership in this specialized service
        if (!dto.EmployeeID.HasValue)
            return ApiResponse<DocumentDto>.Fail("يجب تحديد الموظف لهذه الوثيقة");

        return await _documentService.UploadAsync(dto, uploadedByUserId);
    }

    public async Task<ApiResponse<bool>> DeleteDocumentAsync(int id, int userId)
    {
        return await _documentService.DeleteAsync(id, userId);
    }
}
