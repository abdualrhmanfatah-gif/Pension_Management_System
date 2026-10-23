using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IBankTransactionService
{
    Task<ApiResponse<List<BankTransactionDto>>> GetByBankAsync(int bankId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<List<BankTransactionDto>>> GetByAccountAsync(int accountId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<BankTransactionDto>> CreateAsync(CreateBankTransactionDto dto);
}
