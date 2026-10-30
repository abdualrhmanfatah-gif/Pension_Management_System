<<<<<<< HEAD
using PensionMS.Infrastructure.Data;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.HR;

public class HrAttendanceService : IHrAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public HrAttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<AttendanceEventDto>>> GetAttendanceEventsAsync(int? employeeId, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _unitOfWork.AttendanceEvents.Query()
            .AsNoTracking()
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeID == employeeId.Value);

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            query = query.Where(a => a.EventDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date;
            query = query.Where(a => a.EventDate <= to);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.EventDate)
            .ThenByDescending(a => a.EventTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AttendanceEventDto
            {
                AttendanceEventID = a.AttendanceEventID,
                EmployeeID = a.EmployeeID,
                EmployeeFullName = a.Employee != null ? a.Employee.FullName : "غير محدد",
                EmployeeNumber = a.Employee != null ? a.Employee.EmployeeNumber : "غير محدد",
                EventDate = a.EventDate,
                EventType = a.EventType,
                EventTime = a.EventTime,
                Location = a.Location,
                DeviceInfo = a.DeviceInfo,
                Notes = a.Notes,
                IsManualEntry = a.IsManualEntry,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<AttendanceEventDto> 
        { 
            Items = items, 
            TotalCount = totalCount, 
            Page = page, 
            PageSize = pageSize 
        };

        return ApiResponse<PagedResult<AttendanceEventDto>>.Success(result);
    }

<<<<<<< HEAD
    public Task<ApiResponse<AttendanceEventDto>> RecordAttendanceAsync(CreateAttendanceEventDto dto, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => RecordAttendanceCoreAsync(dto, userId));

    private async Task<ApiResponse<AttendanceEventDto>> RecordAttendanceCoreAsync(CreateAttendanceEventDto dto, int userId)
=======
    public async Task<ApiResponse<AttendanceEventDto>> RecordAttendanceAsync(CreateAttendanceEventDto dto, int userId)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeID);
        if (employee == null || employee.IsDeleted) return ApiResponse<AttendanceEventDto>.Fail("الموظف غير موجود");

        var eventTime = dto.EventTime != null && TimeSpan.TryParse(dto.EventTime, out var parsed) 
            ? parsed 
            : DateTime.UtcNow.TimeOfDay;

        var attendance = new AttendanceEvent
        {
            EmployeeID = dto.EmployeeID,
            EventDate = dto.EventDate.Date,
            EventType = dto.EventType,
            EventTime = eventTime,
            Location = dto.Location,
            Notes = dto.Notes,
            IsManualEntry = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AttendanceEvents.AddAsync(attendance);
<<<<<<< HEAD
        await _unitOfWork.SaveChangesAsync(); // Materialize Identity inside the operation transaction.
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        
        // Also add to service history
        var history = new ServiceHistoryEvent
        {
            EmployeeID = dto.EmployeeID,
            EventDate = DateTime.UtcNow,
            EventType = HrEventTypeConstants.Attendance,
            Summary = $"تسجيل {dto.EventType} - {dto.Notes}",
            SourceEntityType = "AttendanceEvent",
            SourceEntityId = attendance.AttendanceEventID,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.ServiceHistoryEvents.AddAsync(history);

        await _unitOfWork.SaveChangesAsync();

        // Reload to get employee name
        var saved = await _unitOfWork.AttendanceEvents.Query()
            .AsNoTracking()
            .Include(a => a.Employee)
            .FirstAsync(a => a.AttendanceEventID == attendance.AttendanceEventID);

        var savedDto = new AttendanceEventDto
        {
            AttendanceEventID = saved.AttendanceEventID,
            EmployeeID = saved.EmployeeID,
            EmployeeFullName = saved.Employee?.FullName ?? "غير محدد",
            EmployeeNumber = saved.Employee?.EmployeeNumber ?? "غير محدد",
            EventDate = saved.EventDate,
            EventType = saved.EventType,
            EventTime = saved.EventTime,
            Location = saved.Location,
            Notes = saved.Notes,
            IsManualEntry = saved.IsManualEntry,
            CreatedAt = saved.CreatedAt
        };

        return ApiResponse<AttendanceEventDto>.Success(savedDto, "تم تسجيل الحضور بنجاح");
    }

    public async Task<ApiResponse<AttendanceSummaryDto>> GetEmployeeSummaryAsync(int employeeId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var events = await _unitOfWork.AttendanceEvents.Query()
            .AsNoTracking()
            .Where(a => a.EmployeeID == employeeId && a.EventDate >= startDate && a.EventDate <= endDate)
            .ToListAsync();

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null || employee.IsDeleted) return ApiResponse<AttendanceSummaryDto>.Fail("الموظف غير موجود");

        var summary = new AttendanceSummaryDto
        {
            EmployeeID = employeeId,
            EmployeeFullName = employee.FullName,
            PresentDays = events.Where(e => e.EventType == AttendanceConstants.CheckIn).Select(e => e.EventDate.Date).Distinct().Count(),
            AbsentDays = 0, // Simplified for now
            LateArrivals = events.Count(e => e.EventType == AttendanceConstants.CheckIn && e.EventTime > new TimeSpan(8, 15, 0)),
            EarlyDepartures = events.Count(e => e.EventType == AttendanceConstants.CheckOut && e.EventTime < new TimeSpan(14, 0, 0)),
            TotalWorkHours = 0 // Needs more complex calculation
        };

        return ApiResponse<AttendanceSummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<AttendanceDailyStatsDto>> GetDailyStatsAsync(DateTime date)
    {
        var day = date.Date;
        var nextDay = day.AddDays(1);

        var dayEvents = await _unitOfWork.AttendanceEvents.Query()
            .AsNoTracking()
            .Where(a => a.EventDate >= day && a.EventDate < nextDay)
            .ToListAsync();

        var totalEmployees = await _unitOfWork.Employees.Query()
            .AsNoTracking()
            .Include(e => e.EmploymentStatus)
            .CountAsync(e => (e.EmploymentStatus != null && e.EmploymentStatus.IsInService) && !e.IsDeleted);

        var presentCount = dayEvents.Where(e => e.EventType == AttendanceConstants.CheckIn).Select(e => e.EmployeeID).Distinct().Count();

        var leavesToday = await _unitOfWork.LeaveRequests.Query()
            .AsNoTracking()
            .CountAsync(l => l.Status == LeaveStatusConstants.Approved && l.StartDate <= day && l.EndDate >= day && !l.IsDeleted);

        // Late arrival (after 08:15)
        var lateTime = new TimeSpan(8, 15, 0);
        var lateCount = dayEvents.Count(e => e.EventType == AttendanceConstants.CheckIn && e.EventTime.HasValue && e.EventTime.Value > lateTime);
        var stats = new AttendanceDailyStatsDto
        {
            Date = date,
            TotalEmployees = totalEmployees,
            PresentCount = presentCount,
            AbsentCount = Math.Max(0, totalEmployees - presentCount - leavesToday),
            LateCount = lateCount,
            ExternalTaskCount = 0 // Needs more logic
        };

        return ApiResponse<AttendanceDailyStatsDto>.Success(stats);
    }

    public Task<List<AttendanceSummaryDto>> GetMonthlySummaryAsync(int year, int month, int? departmentId)
    {
        // This would be aggregate data for reports
        return Task.FromResult(new List<AttendanceSummaryDto>());
    }
}
