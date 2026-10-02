using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.RetirementFiles;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة إدارة حالات ملفات التقاعد — تغيير الحالة + فحص SoD + التحولات المسموحة
/// </summary>
public interface IFileStatusService
{
    Task<ApiResponse<bool>> ChangeStatusAsync(int id, int newStatusId, string? reason, int userId);
    Task<ApiResponse<List<FileStatusDto>>> GetAllowedStatusTransitionsAsync(int fileId);
    Task<ApiResponse<List<FileStatusDto>>> GetFileStatusesAsync();

    /// <summary>
    /// يُشغَّل من الـ Background Job — يوقف الملفات النشطة التي انتهت مدة تعهداتها
    /// </summary>
    Task ProcessExpiredPledgesAsync();
}
