namespace PensionMS.Core.Security;

/// <summary>نطاق صلاحية واحدة؛ النطاق الفارغ منع صريح وليس وصولاً مركزياً.</summary>
public sealed class PermissionScope
{
    private readonly int[] _branchIds;
    public bool AllBranches { get; }
    public IReadOnlyList<int> BranchIds => Array.AsReadOnly(_branchIds);
    public bool IsDenied => !AllBranches && _branchIds.Length == 0;
    public string CacheKey => AllBranches ? "all" : string.Join(",", _branchIds);

    private PermissionScope(bool allBranches, IEnumerable<int> branchIds)
    {
        AllBranches = allBranches;
        _branchIds = branchIds.Where(id => id > 0).Distinct().Order().ToArray();
    }

    public static PermissionScope Denied { get; } = new(false, []);
    public static PermissionScope All { get; } = new(true, []);
    public static PermissionScope Restricted(IEnumerable<int> branchIds) => new(false, branchIds);
    public bool Allows(int branchId) => branchId > 0 && (AllBranches || Array.BinarySearch(_branchIds, branchId) >= 0);

    public PermissionScope NarrowTo(int? branchId)
    {
        if (branchId is null) return this;
        return Allows(branchId.Value) ? Restricted([branchId.Value]) : Denied;
    }

    public PermissionScope Intersect(PermissionScope other)
        => AllBranches ? other : other.AllBranches ? this : Restricted(_branchIds.Intersect(other._branchIds));
}
