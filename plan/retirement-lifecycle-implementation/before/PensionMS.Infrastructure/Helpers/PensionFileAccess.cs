using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Helpers;

internal static class PensionFileAccess
{
    public static IQueryable<RetirementFile> Files(IUnitOfWork unit, IBranchScopeProvider scope)
    {
        var files = unit.RetirementFiles.Query().Where(f => !f.IsDeleted);
        if (scope.IsHQ()) return files;
        var branch = scope.GetCurrentBranchId();
        if (branch is null or <= 0) throw new BranchScopeAccessException();
        return files.Where(f => f.PensionBranchID == branch);
    }

    public static bool Editable(RetirementFile file) => file.StatusID != FileStatusConstants.Closed &&
        file.StatusID != FileStatusConstants.Suspended && file.StatusID != FileStatusConstants.Rejected;
}
