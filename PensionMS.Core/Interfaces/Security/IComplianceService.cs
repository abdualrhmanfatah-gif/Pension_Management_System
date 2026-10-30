using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Compliance;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة الامتثال (Compliance) — تجمع بيانات المخاطر والتجاوزات للوحة التحكم
/// </summary>
public interface IComplianceService
{
    Task<ApiResponse<ComplianceDashboardDto>> GetDashboardAsync();
    Task<ApiResponse<List<RiskEventSummaryDto>>> GetOpenRiskEventsAsync(string? severity = null);
    Task<ApiResponse<List<OverrideReviewDto>>> GetPendingOverridesAsync();
    Task<ApiResponse<bool>> ResolveRiskEventAsync(long riskEventId, int resolvedByUserId, string notes);
}
