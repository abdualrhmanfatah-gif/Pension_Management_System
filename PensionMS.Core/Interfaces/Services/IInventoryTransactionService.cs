using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IInventoryTransactionService
{
    Task<ApiResponse<List<TransactionDto>>> GetAllAsync();
    Task<ApiResponse<List<TransactionDto>>> GetByItemAsync(int itemId);
    Task<ApiResponse<List<TransactionDto>>> GetByWithdrawalOrderAsync(string orderNumber);
    Task<ApiResponse<TransactionDto>> GetByIdAsync(int id);
    Task<ApiResponse<TransactionDto>> CreateAsync(CreateTransactionDto dto, int userId);
    Task<ApiResponse<List<TransactionDto>>> CreateBulkAsync(CreateBulkTransactionDto dto, int userId);
}
