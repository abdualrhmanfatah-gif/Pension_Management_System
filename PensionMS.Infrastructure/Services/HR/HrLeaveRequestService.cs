using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.HR;

public class HrLeaveRequestService : IHrLeaveRequestService
{
    private readonly IUnitOfWork _unitOfWork;

    public HrLeaveRequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<LeaveRequestListItemDto>>> GetAllAsync(int page = 1, int pageSize = 20, int? employeeId = null, string? search = null, string? status = null)
    {
        var query = _unitOfWork.LeaveRequests.Query()
            .AsNoTracking()
            .Where(l => !l.IsDeleted);

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeID == employeeId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status == status);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l => l.Employee != null && l.Employee.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeaveRequestListItemDto
            {
                LeaveRequestID = l.LeaveRequestID,
                EmployeeFullName = l.Employee != null ? l.Employee.FullName : string.Empty,
                EmployeeNumber = l.Employee != null ? l.Employee.EmployeeNumber : string.Empty,
                LeaveTypeName = l.LeaveType != null ? l.LeaveType.Name : string.Empty,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                RequestedDays = (l.EndDate - l.StartDate).Days + 1,
                Status = l.Status,
                Reason = l.Reason,
                Notes = l.Notes,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<LeaveRequestListItemDto>>.Success(new PagedResult<LeaveRequestListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<LeaveRequestDetailsDto>> GetByIdAsync(int id)
    {
        var result = await _unitOfWork.LeaveRequests.Query()
            .AsNoTracking()
            .Where(l => l.LeaveRequestID == id && !l.IsDeleted)
            .Select(l => new LeaveRequestDetailsDto
            {
                LeaveRequestID = l.LeaveRequestID,
                EmployeeID = l.EmployeeID,
                EmployeeFullName = l.Employee != null ? l.Employee.FullName : string.Empty,
                EmployeeNumber = l.Employee != null ? l.Employee.EmployeeNumber : string.Empty,
                LeaveTypeID = l.LeaveTypeID,
                LeaveTypeName = l.LeaveType != null ? l.LeaveType.Name : string.Empty,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                RequestedDays = (l.EndDate - l.StartDate).Days + 1,
                Status = l.Status,
                Reason = l.Reason,
                Notes = l.Notes,
                ApprovedByUserName = l.ApprovedByUser != null ? l.ApprovedByUser.FullName : null,
                CreatedAt = l.CreatedAt,
                ApprovedAt = l.UpdatedAt // Assuming UpdatedAt is ApprovedAt for simple workflow
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return ApiResponse<LeaveRequestDetailsDto>.Fail("طلب الإجازة غير موجود");

        return ApiResponse<LeaveRequestDetailsDto>.Success(result);
    }

    public async Task<ApiResponse<LeaveRequestDetailsDto>> CreateAsync(CreateLeaveRequestDto dto)
    {
        // Basic validation
        if (dto.StartDate >= dto.EndDate)
            return ApiResponse<LeaveRequestDetailsDto>.Fail("تاريخ النهاية يجب أن يكون بعد تاريخ البداية");

        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeID);
        if (employee == null || employee.IsDeleted)
            return ApiResponse<LeaveRequestDetailsDto>.Fail("الموظف غير موجود");

        var leaveRequest = new LeaveRequest
        {
            EmployeeID = dto.EmployeeID,
            LeaveTypeID = dto.LeaveTypeID,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Status = LeaveStatusConstants.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveRequests.AddAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(leaveRequest.LeaveRequestID);
    }

    public async Task<ApiResponse<LeaveRequestDetailsDto>> ProcessAsync(int id, ProcessLeaveRequestDto dto)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.Query()
            .FirstOrDefaultAsync(l => l.LeaveRequestID == id && !l.IsDeleted);

        if (leaveRequest == null)
            return ApiResponse<LeaveRequestDetailsDto>.Fail("طلب الإجازة غير موجود");

        if (leaveRequest.Status != LeaveStatusConstants.Pending)
            return ApiResponse<LeaveRequestDetailsDto>.Fail("تمت معالجة هذا الطلب مسبقاً");

        leaveRequest.Status = dto.Status;
        leaveRequest.Notes = dto.Notes;
        leaveRequest.ApprovedByUserID = dto.ProcessedByUserID;
        leaveRequest.UpdatedAt = DateTime.UtcNow;


        if (dto.Status == LeaveStatusConstants.Approved)
        {
            // Log to Service History
            await _unitOfWork.ServiceHistoryEvents.AddAsync(new ServiceHistoryEvent
            {
                EmployeeID = leaveRequest.EmployeeID,
                EventType = HrEventTypeConstants.Leave,
                EventDate = DateTime.UtcNow,
                Summary = $"إجازة من {leaveRequest.StartDate:yyyy-MM-dd} إلى {leaveRequest.EndDate:yyyy-MM-dd}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ApiResponse<LeaveRequestStatsDto>> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var pendingCount = await _unitOfWork.LeaveRequests.Query()
            .CountAsync(l => l.Status == LeaveStatusConstants.Pending && !l.IsDeleted);
            
        var approvedThisMonthCount = await _unitOfWork.LeaveRequests.Query()
            .CountAsync(l => l.Status == LeaveStatusConstants.Approved && l.UpdatedAt >= new DateTime(now.Year, now.Month, 1) && !l.IsDeleted);
            
        var totalThisYearCount = await _unitOfWork.LeaveRequests.Query()
            .CountAsync(l => l.CreatedAt >= new DateTime(now.Year, 1, 1) && !l.IsDeleted);

        var stats = new LeaveRequestStatsDto
        {
            PendingCount = pendingCount,
            ApprovedThisMonthCount = approvedThisMonthCount,
            TotalThisYearCount = totalThisYearCount
        };

        return ApiResponse<LeaveRequestStatsDto>.Success(stats);
    }
}
