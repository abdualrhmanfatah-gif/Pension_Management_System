using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.DocumentPledges;

namespace PensionMS.Core.Interfaces;

public interface IDocumentPledgeService
{
    Task<ApiResponse<List<DocumentPledgeDto>>> GetByFileAsync(int fileId);
    Task<ApiResponse<DocumentPledgeDto>> GetByIdAsync(int id);
    Task<ApiResponse<DocumentPledgeDto>> CreateAsync(CreateDocumentPledgeDto dto, int userId);
    Task<ApiResponse<DocumentPledgeDto>> UpdateAsync(int id, UpdateDocumentPledgeDto dto, int userId);
    Task<ApiResponse<bool>> MarkFulfilledAsync(int id, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);

    /// <summary>أعداد النواقص المرتبطة بتعهدات محددة دفعة واحدة ضمن النطاق؛ المعرّفات غير الموجودة لا تظهر في النتيجة. المدخل الفارغ يعيد قاموسًا فارغًا دون استعلام.</summary>
    Task<ApiResponse<Dictionary<int, int>>> GetMissingDocumentCountsAsync(IReadOnlyCollection<int> pledgeIds);
}
