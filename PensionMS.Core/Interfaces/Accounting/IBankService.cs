using PensionMS.Core.DTOs.Banks;
using PensionMS.Core.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PensionMS.Core.Interfaces;

public interface IBankService
{
    Task<ApiResponse<List<BankDto>>> GetAllAsync();
    Task<ApiResponse<BankDto>> GetByIdAsync(int id);
    Task<ApiResponse<BankDto>> CreateAsync(CreateBankDto dto, int userId);
    Task<ApiResponse<BankDto>> UpdateAsync(int id, UpdateBankDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
    Task<ApiResponse<bool>> ToggleActiveAsync(int id, int userId);
}
