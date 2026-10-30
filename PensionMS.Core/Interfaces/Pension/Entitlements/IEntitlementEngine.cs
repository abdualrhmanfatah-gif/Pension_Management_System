using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IEntitlementEngine
{
    /// <summary>
    /// إعادة تقييم حالة الاستحقاق للمستحق (تفعيل تلقائي في حال اكتمال البيانات والوثائق وانتفاء موانع الصرف)
    /// </summary>
    Task<ApiResponse<bool>> ReEvaluateEntitlementStatusAsync(int entitledId);

    /// <summary>
    /// التحقق من صحة شروط الاستحقاق القانونية الأساسية (العمر، الحالة الاجتماعية، صلة القرابة) 
    /// بمعزل عن اكتمال الوثائق أو البيانات.
    /// </summary>
    Task<ApiResponse<bool>> CheckCoreEligibilityAsync(int entitledId);
}
