using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Admin;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Administration;

public class AdminSettingsService : IAdminSettingsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminSettingsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> GetAllAsync(string? category)
    {
        var query = _unitOfWork.SystemSettings.Query()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(s => s.Category == category);

        var items = await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.SettingKey)
            .Select(s => new
            {
                s.SettingID,
                s.SettingKey,
                s.SettingValue,
                s.Description,
                s.Category,
                updatedByUser = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : null,
                s.CreatedAt,
                s.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<object>.Success(items);
    }

    public async Task<ApiResponse<List<string>>> GetCategoriesAsync()
    {
        var categories = await _unitOfWork.SystemSettings.Query()
            .AsNoTracking()
            .Where(s => s.Category != null)
            .Select(s => s.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return ApiResponse<List<string>>.Success(categories);
    }

    public async Task<ApiResponse<object>> UpdateAsync(int id, UpdateSettingDto dto, int userId)
    {
        var setting = await _unitOfWork.SystemSettings.GetByIdAsync(id);
        if (setting == null)
            return ApiResponse<object>.Fail("الإعداد غير موجود");

        setting.SettingValue = dto.Value;
        setting.UpdatedByUserID = userId;
        setting.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.SystemSettings.Update(setting);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<object>.Success(null, "تم تحديث الإعداد بنجاح");
    }

    public async Task<ApiResponse<object>> CreateAsync(CreateSettingDto dto, int userId)
    {
        var exists = await _unitOfWork.SystemSettings.AnyAsync(s => s.SettingKey == dto.Key);
        if (exists)
            return ApiResponse<object>.Fail("مفتاح الإعداد موجود مسبقاً");

        var setting = new SystemSetting
        {
            SettingKey = dto.Key,
            SettingValue = dto.Value,
            Description = dto.Description,
            Category = dto.Category,
            UpdatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SystemSettings.AddAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<object>.Success(new { setting.SettingID }, "تم إنشاء الإعداد بنجاح");
    }

    public async Task<ApiResponse<object>> DeleteAsync(int id)
    {
        var setting = await _unitOfWork.SystemSettings.GetByIdAsync(id);
        if (setting == null)
            return ApiResponse<object>.Fail("الإعداد غير موجود");

        _unitOfWork.SystemSettings.Delete(setting);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<object>.Success(null, "تم حذف الإعداد بنجاح");
    }
}
