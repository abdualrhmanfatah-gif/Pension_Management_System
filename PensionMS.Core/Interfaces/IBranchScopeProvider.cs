namespace PensionMS.Core.Interfaces;

/// <summary>
/// يوفر معلومات النطاق (الفرع) للمستخدم الحالي من السياق
/// </summary>
public interface IBranchScopeProvider
{
    /// <summary>
    /// معرف الفرع الحالي للمستخدم (null إذا كان مستخدم مركزي)
    /// </summary>
    int? GetCurrentBranchId();

    /// <summary>
    /// معرف المستخدم الحالي
    /// </summary>
    int GetCurrentUserId();

    /// <summary>
    /// هل المستخدم من المقر الرئيسي
    /// </summary>
    bool IsHQ();
}
