using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.RetirementFiles;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة عمليات التغيير على ملفات التقاعد — إنشاء، تحديث، حذف (CQRS: Command side).
/// القراءة → IFileQueryService | الحالات → IFileStatusService
/// </summary>
public interface IRetirementFileService
{
    Task<ApiResponse<RetirementFileDto>> CreateAsync(CreateRetirementFileDto dto, int userId);
    Task<ApiResponse<RetirementFileDto>> UpdateAsync(int id, UpdateRetirementFileDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId);
    Task<ApiResponse<bool>> HandleRetireeDeathAsync(int fileId, DateOnly deathDate, int userId);
    Task<ApiResponse<bool>> CorrectRetireeLifeStatusAsync(int fileId, CorrectRetireeLifeStatusDto dto, int userId);
    Task<ApiResponse<bool>> AssignEmployeeAsync(int fileId, int employeeId, int userId);

    // التقارير
    Task<ApiResponse<PagedResult<RetirementFileReportRow>>> GetRetirementFileReportAsync(RetirementFileReportFilter filter);
    Task<ApiResponse<RetirementFileReportSummaryDto>> GetRetirementFileReportSummaryAsync(RetirementFileReportFilter filter);
    Task<byte[]> ExportRetirementFileReportAsync(RetirementFileReportFilter filter, string format);
}

