namespace PensionMS.Core.Interfaces;

/// <summary>
/// يحل صلاحيات المستخدم مع مراعاة النطاق (الفرع/الإقليم)
/// </summary>
public interface IScopedPermissionResolver
{
    Task<PensionMS.Core.Security.PermissionScope> GetAllowedScopeAsync(int userId, string permissionCode);
    /// <summary>
    /// هل يملك المستخدم صلاحية معينة؟ (بدون فحص نطاق)
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string permissionCode);

    /// <summary>
    /// هل يملك المستخدم صلاحية معينة ضمن نطاق فرع محدد؟
    /// </summary>
    Task<bool> HasPermissionInBranchAsync(int userId, string permissionCode, int branchId);

    /// <summary>
    /// يجلب جميع رموز الصلاحيات الفعالة للمستخدم
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(int userId);

}
