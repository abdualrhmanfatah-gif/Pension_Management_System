using PensionMS.Core.Entities;
using PensionMS.Core.Security;

namespace PensionMS.Core.Interfaces;

/// <summary>مصدر بيانات مقيد بهوية المستخدم وصلاحية العملية.</summary>
public interface IAccessiblePensionRepository
{
    Task<IPensionDataQuery> ForPermissionAsync(string permissionCode);
    Task<IPensionDataQuery> ForCurrentOperationAsync();
}

/// <summary>قرار ثابت للعملية؛ جميع الاستعلامات المشتقة منه تستعمل النطاق نفسه.</summary>
public interface IPensionDataQuery
{
    PermissionScope Scope { get; }
    IQueryable<RetirementFile> Files(int? branchId = null);
    IQueryable<int> FileIds(int? branchId = null);
    IQueryable<T> Query<T>(IRepository<T> repository, int? branchId = null) where T : class;
    int? ResolveBranch(int? branchId = null);
    bool IsRestricted(int? branchId = null);
    Task<bool> CanAccessDocumentOwnerAsync(Document owner);
    Task<bool> CanEditRetireeAsync(int retireeId);
}
