using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.MissingDocuments;

namespace PensionMS.Core.Interfaces;

public interface IMissingDocumentService
{
    Task<ComplianceSummary> GetComplianceSummaryAsync(int fileId);

    /// <summary>يحفظ عدّاد النواقص دون إنشاء معاملة أو إنهائها؛ يستخدم معاملة المستدعي إن وجدت.</summary>
    Task RefreshComplianceCacheAsync(int fileId);

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

    /// <summary>
    /// يجهز النواقص المطلوبة غير المسجلة دون حفظ أو إنهاء المعاملة.
    /// يستلزم ملفاً مصرحاً به ومعاملة نشطة؛ المستدعي يحفظ قبل الاستدعاء وبعده.
    /// </summary>
    Task<int> StageRequiredAsync(int fileId, int userId);
}
