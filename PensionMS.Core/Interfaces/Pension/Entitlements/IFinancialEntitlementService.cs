using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Financial;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة الاستحقاقات المالية
/// </summary>
public interface IFinancialEntitlementService
{
    /// <summary>
    /// جلب جميع استحقاقات متقاعد
    /// </summary>
    Task<ApiResponse<List<FinancialEntitlementDto>>> GetByRetireeIdAsync(int retireeId);

    /// <summary>
    /// جلب الاستحقاق الساري حالياً
    /// </summary>
    Task<ApiResponse<FinancialEntitlementDto?>> GetActiveByRetireeIdAsync(int retireeId);

    /// <summary>
    /// جلب استحقاق بالمعرف
    /// </summary>
    Task<ApiResponse<FinancialEntitlementDto>> GetByIdAsync(int id);

    /// <summary>
    /// يسجل معاش كشف الإحالة من تاريخ التقاعد ثم يطبق التسويات بترتيب تاريخ القرار،
    /// وينهي كل استحقاق قبل التالي بيوم دون تكرار السجلات المطابقة أو إعادة فتح المنتهي.
    /// </summary>
    Task<ApiResponse<FinancialEntitlementDto>> CreateAsync(CreateFinancialEntitlementDto dto, int userId);
    /// <summary>يجهز استحقاقات الملف من تاريخ التقاعد ثم تواريخ قرارات التسوية، ويعيد معاش ورتبة يوم الفتح داخل معاملة المستدعي.</summary>
    Task<ApiResponse<FinancialEntitlementDto>> CreateForFileAsync(int fileId, int userId);

    /// <summary>
    /// إنهاء استحقاق مالي
    /// </summary>
    Task<ApiResponse<bool>> EndAsync(int id, DateTime endDate, int userId);

    /// <summary>
    /// بحث الاستحقاقات المالية (رقم الملف، الرقم العسكري، هاتف المتتقاعد، اسم المتتقاعد، اسم الوكيل)
    /// </summary>
    Task<ApiResponse<List<FinancialEntitlementDto>>> SearchAsync(string query);
}
