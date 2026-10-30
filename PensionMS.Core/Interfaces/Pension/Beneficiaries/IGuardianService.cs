using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Guardians;

namespace PensionMS.Core.Interfaces;

public interface IGuardianService
{
    Task<ApiResponse<GuardianDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<GuardianDto>>> GetByEntitledIdAsync(int entitledId);
    Task<ApiResponse<List<GuardianDto>>> GetByFileIdAsync(int fileId);

    /// <summary>
    /// تعيين وصي على مستحق قاصر — يُنهي أي وصاية نشطة سابقة تلقائياً.
    /// يُحوّل SalaryRecipient للوصي.
    /// </summary>
    Task<ApiResponse<GuardianDto>> AssignAsync(AssignGuardianDto dto, int userId);

    /// <summary>
    /// إنهاء الوصاية — يُعيد SalaryRecipient للمستحق نفسه.
    /// </summary>
    Task<ApiResponse<bool>> EndGuardianshipAsync(int guardianId, EndGuardianshipDto dto, int userId);

    /// <summary>
    /// إنهاء وصاية على قاصر محدد أو جميع القاصرين.
    /// </summary>
    Task<ApiResponse<bool>> EndGuardianshipAsync(int guardianId, int? entitledId, EndGuardianshipDto dto, int userId);

    /// <summary>
    /// إضافة قاصر إضافي لوصي موجود.
    /// </summary>
    Task<ApiResponse<GuardianDto>> AddMinorAsync(int guardianId, int entitledId, DateTime startDate, int userId);

    /// <summary>
    /// تحديث بيانات الوصي.
    /// </summary>
    Task<ApiResponse<GuardianDto>> UpdateAsync(int id, UpdateGuardianDto dto, int userId);

    /// <summary>
    /// بحث شامل عن الأوصياء — بالاسم، الرقم الوطني، رقم الملف، الهاتف، الرقم العسكري للمتقاعد
    /// </summary>
    Task<ApiResponse<List<GuardianSearchResultDto>>> SearchAsync(string query, int limit = 10);
}
