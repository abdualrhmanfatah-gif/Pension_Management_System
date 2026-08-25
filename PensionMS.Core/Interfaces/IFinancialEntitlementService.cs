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
    /// إنشاء استحقاق مالي جديد (ينهي الساري تلقائياً)
    /// </summary>
    Task<ApiResponse<FinancialEntitlementDto>> CreateAsync(CreateFinancialEntitlementDto dto, int userId);

    /// <summary>
    /// إنهاء استحقاق مالي
    /// </summary>
    Task<ApiResponse<bool>> EndAsync(int id, DateTime endDate, int userId);

    /// <summary>
    /// بحث الاستحقاقات المالية (رقم الملف، الرقم العسكري، هاتف المتتقاعد، اسم المتتقاعد، اسم الوكيل)
    /// </summary>
    Task<ApiResponse<List<FinancialEntitlementDto>>> SearchAsync(string query);
}
