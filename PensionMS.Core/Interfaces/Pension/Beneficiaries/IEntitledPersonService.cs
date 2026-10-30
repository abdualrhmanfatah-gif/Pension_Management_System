using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.EntitledPersons;

namespace PensionMS.Core.Interfaces;

public interface IEntitledPersonService
{
    Task<ApiResponse<EntitledPersonDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<EntitledPersonDto>>> GetByFileIdAsync(int fileId);
    Task<ApiResponse<EntitledPersonDto>> CreateAsync(CreateEntitledPersonDto dto, int userId);
    Task<ApiResponse<EntitledPersonDto>> UpdateAsync(int id, UpdateEntitledPersonDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId);

    // ─── Business lifecycle ────────────────────────────────────────────
    /// <summary>
    /// إيقاف الاستحقاق مع تعطيل SalaryRecipient المرتبط تلقائياً.
    /// </summary>
    Task<ApiResponse<bool>> StopAsync(int id, StopEntitledPersonDto dto, int userId);

    /// <summary>
    /// إعادة تفعيل الاستحقاق بعد زوال سبب الإيقاف، وفق توافق سبب الإيقاف ودليل إعادة التفعيل.
    /// </summary>
    Task<ApiResponse<bool>> ReactivateAsync(int id, ReactivateEntitledPersonDto dto, int userId);

    /// <summary>
    /// تسجيل وفاة المستحق — يُوقف الاستحقاق ويُعطّل SalaryRecipient.
    /// </summary>
    Task<ApiResponse<bool>> RecordDeathAsync(int id, RecordDeathDto dto, int userId);

    /// <summary>
    /// تفعيل المستحق يدوياً مع وجود نواقص (مع إمكانية تحويل النواقص إلى تعهدات)
    /// </summary>
    Task<ApiResponse<bool>> ManualActivateAsync(int id, ManualActivateEntitledPersonDto dto, int userId);

    /// <summary>
    /// إعادة تقييم حالة الاستحقاق للمستحق (تفعيل تلقائي في حال اكتمال البيانات والوثائق وانتفاء موانع الصرف)
    /// </summary>
    Task<ApiResponse<bool>> ReEvaluateEntitlementStatusAsync(int id);

    /// <summary>
    /// بحث شامل عن المستحقين — بالاسم، الرقم الوطني، رقم الملف، الهاتف، الرقم العسكري للمتقاعد
    /// </summary>
    Task<ApiResponse<List<EntitledPersonSearchResultDto>>> SearchAsync(string query, int limit = 10);
}
