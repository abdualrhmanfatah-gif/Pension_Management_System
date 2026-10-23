using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Suspensions;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة إدارة إيقاف المعاش والتعهدات والإعفاءات
/// </summary>
public interface IPensionSuspensionService
{
    /// <summary>يفحص حالة الملف والمراجعة والإيقافات ضمن نطاق الصلاحية. تعذر الفحص يمنع الصرف.</summary>
    Task<ApiResponse<PaymentHoldCheckDto>> CheckPaymentHoldAsync(int fileId);

    /// <summary>حالة المعاش المعروضة من حالة الملف واعتماد المراجعة والإيقافات، وليست فحص أهلية الصرف الكامل.</summary>
    Task<ApiResponse<string>> GetSalaryStatusAsync(int fileId);

    // ── Suspensions ──
    Task<ApiResponse<SuspensionDto>> CreateWithPledgeAsync(CreateSuspensionDto dto, int userId);
    Task<ApiResponse<bool>> EndSuspensionAsync(int suspensionId, EndSuspensionDto dto, int userId);
    Task<ApiResponse<List<SuspensionDto>>> GetByFileIdAsync(int fileId);
    Task<ApiResponse<PagedResult<SuspensionDto>>> GetAllSuspensionsAsync(int page, int pageSize, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null);

    // ── Exemptions ──
    Task<ApiResponse<ExemptionDto>> CreateExemptionAsync(CreateExemptionDto dto, int userId);
    Task<ApiResponse<List<ExemptionDto>>> GetExemptionsByFileIdAsync(int fileId);
}
