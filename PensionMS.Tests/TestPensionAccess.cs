using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using PensionMS.Infrastructure.Repositories;
using Moq;

namespace PensionMS.Testing;

public interface ITestBranchScope
{
    int? GetCurrentBranchId();
    bool IsHQ();
}

// Test adapter only: exercise production ownership queries with a supplied permission decision.
internal sealed class TestPensionAccess(IUnitOfWork unit, Func<PermissionScope> decision) : IAccessiblePensionRepository
{
    public static IScopedPermissionResolver AllowAllPermissions()
    {
        var mock = new Mock<IScopedPermissionResolver>();
        mock.Setup(p => p.GetAllowedScopeAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(PermissionScope.All);
        return mock.Object;
    }
    public static IAccessiblePensionRepository All(IUnitOfWork unit) => new TestPensionAccess(unit, () => PermissionScope.All);
    public static IAccessiblePensionRepository From(IUnitOfWork unit, ITestBranchScope identity)
        => new TestPensionAccess(unit, () => Scope(identity));
    public static PermissionScope Scope(ITestBranchScope identity)
        => identity.IsHQ() ? PermissionScope.All : PermissionScope.Restricted([identity.GetCurrentBranchId() ?? 0]);
    public static IPensionDataQuery Query(IUnitOfWork unit, ITestBranchScope identity)
        => new PensionDataQuery(unit, Scope(identity));
    public Task<IPensionDataQuery> ForPermissionAsync(string permissionCode)
    {
        var scope = decision();
        if (scope.IsDenied) throw new BranchScopeAccessException();
        return Task.FromResult<IPensionDataQuery>(new PensionDataQuery(unit, scope));
    }
    public Task<IPensionDataQuery> ForCurrentOperationAsync() => ForPermissionAsync("test");
}
