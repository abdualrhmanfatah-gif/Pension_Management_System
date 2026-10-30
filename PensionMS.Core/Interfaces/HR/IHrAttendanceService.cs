using PensionMS.Core.DTOs.HR;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IHrAttendanceService
{
    Task<ApiResponse<PagedResult<AttendanceEventDto>>> GetAttendanceEventsAsync(int? employeeId, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<ApiResponse<AttendanceEventDto>> RecordAttendanceAsync(CreateAttendanceEventDto dto, int userId);
    Task<ApiResponse<AttendanceSummaryDto>> GetEmployeeSummaryAsync(int employeeId, int year, int month);
    Task<List<AttendanceSummaryDto>> GetMonthlySummaryAsync(int year, int month, int? departmentId);
    Task<ApiResponse<AttendanceDailyStatsDto>> GetDailyStatsAsync(DateTime date);
}
