using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Admin;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IAdminSettingsService
{
    Task<ApiResponse<object>> GetAllAsync(string? category);
    Task<ApiResponse<List<string>>> GetCategoriesAsync();
    Task<ApiResponse<object>> UpdateAsync(int id, UpdateSettingDto dto, int userId);
    Task<ApiResponse<object>> CreateAsync(CreateSettingDto dto, int userId);
    Task<ApiResponse<object>> DeleteAsync(int id);
}
