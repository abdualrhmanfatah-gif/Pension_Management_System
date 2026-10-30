using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IInventoryItemService
{
    Task<ApiResponse<List<ItemDto>>> GetAllAsync();
    Task<ApiResponse<ItemDto>> GetByIdAsync(int id);
    Task<ApiResponse<ItemDto>> CreateAsync(CreateItemDto dto, int userId);
    Task<ApiResponse<ItemDto>> UpdateAsync(int id, UpdateItemDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
}
