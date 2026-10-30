using PensionMS.Core.Entities;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// نموذج تقييم المخاطر (Fraud Detection)
/// </summary>
public interface IRiskScoringService
{
    /// <summary>
    /// يفحص التعديلات على الملف ويولد حدث مخاطر (RiskEvent) إذا لزم الأمر
    /// </summary>
    Task EvaluateRiskAsync(int fileId, string action, int userId, string? oldValues, string? newValues);
}
