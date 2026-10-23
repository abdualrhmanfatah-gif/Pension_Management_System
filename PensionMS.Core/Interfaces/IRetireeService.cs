using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Retirees;

namespace PensionMS.Core.Interfaces;

public interface IRetireeService
{
    Task<ApiResponse<RetireeDto>> GetByIdAsync(int id);
    Task<ApiResponse<PagedResult<RetireeListDto>>> GetAllAsync(int page, int pageSize, string? search = null, SocialResearchQueryDto? filters = null);
    Task<ApiResponse<RetireeDto>> GetByNationalIDAsync(string nationalId);
    Task<ApiResponse<RetireeDto>> GetByMilitaryNumberAsync(string militaryNumber);

    Task<ApiResponse<RetireeDto>> UpdateAsync(int id, UpdateRetireeDto dto, int userId);
    /// <summary>يجهز تعديل الحقول والصورة فقط دون حفظ أو طلب مراجعة؛ الهوية تُرفع عبر خدمة الوثائق.</summary>
    Task<ApiResponse<bool>> StageProfileUpdateAsync(int id, UpdateRetireeDto dto, int? userId = null);
    /// <summary>يجهز بيانات المتقاعد وحالة حياته عند فتح أول ملف فقط، داخل معاملة المستدعي ودون حفظ.</summary>
    Task<ApiResponse<bool>> StageFileOpeningAsync(int id, UpdateRetireeDto dto, bool isAlive, DateOnly? deathDate, int userId);
    /// <summary>يحدث الامتثال ويفرغ الذاكرة المؤقتة للمتقاعد وملفاته المصرح بها بعد حفظ تعديل البيانات.</summary>
    Task RefreshProfileAsync(int id);
    Task<ApiResponse<string>> UploadPhotoAsync(int id, IRemoteFile photo);
    Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId);
    Task<ApiResponse<RetireeStatementDto>> GetStatementAsync(int id);
    Task<ApiResponse<RetireeFullDataDto>> GetFullDataAsync(int id);
}
