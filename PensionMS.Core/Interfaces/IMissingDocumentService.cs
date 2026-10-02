using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.MissingDocuments;

namespace PensionMS.Core.Interfaces;

public interface IMissingDocumentService
{
    Task<ApiResponse<List<MissingDocumentDto>>> GetByFileAsync(int fileId);
    Task<ApiResponse<MissingDocumentDto>> GetByIdAsync(int id);
    Task<ApiResponse<MissingDocumentDto>> CreateAsync(CreateMissingDocumentDto dto, int userId);
    Task<ApiResponse<bool>> MarkFulfilledAsync(int id, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
    Task<ApiResponse<List<AutoDetectedMissingDocumentDto>>> GetAutoDetectedByFileAsync(int fileId);

    /// <summary>
    /// م22: زوج (نوع + مالك) كامل المطلوب للملف قبل فلتر الاستيفاء — الأساس الموحد لعدّاد الامتثال
    /// </summary>
    Task<List<AutoDetectedMissingDocumentDto>> GetRequiredPairsAsync(int fileId);
}
