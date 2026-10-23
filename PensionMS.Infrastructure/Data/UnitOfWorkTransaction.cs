using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Data;

/// <summary>Owns commit/rollback only when the operation starts the transaction.
/// Nested failures must be propagated to the outer owner. Saving remains explicit.</summary>
internal static class UnitOfWorkTransaction
{
    public static async Task<ApiResponse<T>> ExecuteAsync<T>(IUnitOfWork unitOfWork,
        Func<Task<ApiResponse<T>>> operation)
    {
        var ownsTransaction = !unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await operation();
            if (ownsTransaction)
            {
                if (result.IsSuccess) await unitOfWork.CommitAsync();
                else await unitOfWork.RollbackAsync();
            }
            return result;
        }
        catch
        {
            if (ownsTransaction) await unitOfWork.RollbackAsync();
            throw;
        }
    }
}
