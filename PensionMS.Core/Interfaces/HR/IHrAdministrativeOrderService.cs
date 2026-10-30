using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// واجهة خدمة الأوامر الإدارية للموارد البشرية
/// </summary>
public interface IHrAdministrativeOrderService
{
    /// <summary>
    /// الحصول على قائمة الأوامر الإدارية مع دعم التنقيل والبحث حسب الموظف
    /// </summary>
    Task<PagedResult<AdministrativeOrderListItemDto>> GetAllAsync(int page, int pageSize, int? employeeId = null);

    /// <summary>
    /// الحصول على تفاصيل أمر إداري محدد
    /// </summary>
    Task<AdministrativeOrderDetailsDto?> GetByIdAsync(int id);

    /// <summary>
    /// إنشاء أمر إداري جديد (مسودة)
    /// </summary>
    Task<AdministrativeOrderDetailsDto> CreateAsync(CreateAdministrativeOrderDto dto);

    /// <summary>
    /// إصدار الأمر الإداري (تغيير حالته وتثبيت المصادقة)
    /// </summary>
    Task<AdministrativeOrderDetailsDto?> IssueAsync(int id, IssueAdministrativeOrderDto dto);
}
