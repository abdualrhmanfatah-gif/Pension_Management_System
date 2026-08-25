using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.HR;

public class HrServiceHistoryService : IHrServiceHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public HrServiceHistoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<ServiceHistoryEventDto>>> GetGlobalHistoryAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _unitOfWork.ServiceHistoryEvents.Query()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Summary.Contains(search) || 
                                     (e.Employee != null && e.Employee.FullName.Contains(search)) || 
                                     (e.Employee != null && e.Employee.EmployeeNumber.Contains(search)) ||
                                     e.EventType.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.EventDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ServiceHistoryEventDto
            {
                ServiceHistoryEventID = e.ServiceHistoryEventID,
                EmployeeID = e.EmployeeID,
                EmployeeFullName = e.Employee != null ? e.Employee.FullName : "غير محدد",
                EmployeeNumber = e.Employee != null ? e.Employee.EmployeeNumber : "غير محدد",
                EventType = e.EventType,
                EventDate = e.EventDate,
                Summary = e.Summary,
                SourceEntityType = e.SourceEntityType,
                SourceEntityId = e.SourceEntityId,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<ServiceHistoryEventDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<ServiceHistoryEventDto>>.Success(result);
    }

    public async Task<ApiResponse<List<ServiceHistoryEventDto>>> GetByEmployeeIdAsync(int employeeId)
    {
        var items = await _unitOfWork.ServiceHistoryEvents.Query()
            .AsNoTracking()
            .Where(e => e.EmployeeID == employeeId)
            .OrderByDescending(e => e.EventDate)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new ServiceHistoryEventDto
            {
                ServiceHistoryEventID = e.ServiceHistoryEventID,
                EmployeeID = e.EmployeeID,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Summary = e.Summary,
                SourceEntityType = e.SourceEntityType,
                SourceEntityId = e.SourceEntityId,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<ServiceHistoryEventDto>>.Success(items);
    }

    public async Task<ApiResponse<ServiceHistoryEventDto>> CreateEventAsync(CreateServiceHistoryEventDto dto, int userId)
    {
        var employee = await _unitOfWork.Employees.Query()
            .FirstOrDefaultAsync(e => e.EmployeeID == dto.EmployeeID && !e.IsDeleted);
            
        if (employee == null) return ApiResponse<ServiceHistoryEventDto>.Fail("الموظف غير موجود");

        var ev = new ServiceHistoryEvent
        {
            EmployeeID = dto.EmployeeID,
            EventType = dto.EventType,
            EventDate = dto.EventDate,
            Summary = dto.Summary,
            SourceEntityType = dto.SourceEntityType,
            SourceEntityId = dto.SourceEntityId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ServiceHistoryEvents.AddAsync(ev);
        await _unitOfWork.SaveChangesAsync();

        var result = new ServiceHistoryEventDto
        {
            ServiceHistoryEventID = ev.ServiceHistoryEventID,
            EmployeeID = ev.EmployeeID,
            EmployeeFullName = employee.FullName,
            EmployeeNumber = employee.EmployeeNumber,
            EventType = ev.EventType,
            EventDate = ev.EventDate,
            Summary = ev.Summary,
            SourceEntityType = ev.SourceEntityType,
            SourceEntityId = ev.SourceEntityId,
            CreatedAt = ev.CreatedAt
        };

        return ApiResponse<ServiceHistoryEventDto>.Success(result, "تم تسجيل الحدث بنجاح");
    }

    public async Task<ApiResponse<ServiceHistoryEventDto>> UpdateEventAsync(int id, CreateServiceHistoryEventDto dto, int userId)
    {
        var ev = await _unitOfWork.ServiceHistoryEvents.Query()
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.ServiceHistoryEventID == id);

        if (ev == null) return ApiResponse<ServiceHistoryEventDto>.Fail("الحدث غير موجود");

        ev.EventType = dto.EventType;
        ev.EventDate = dto.EventDate;
        ev.Summary = dto.Summary;
        ev.SourceEntityType = dto.SourceEntityType;
        ev.SourceEntityId = dto.SourceEntityId;

        await _unitOfWork.SaveChangesAsync();

        var result = new ServiceHistoryEventDto
        {
            ServiceHistoryEventID = ev.ServiceHistoryEventID,
            EmployeeID = ev.EmployeeID,
            EmployeeFullName = ev.Employee?.FullName ?? "غير محدد",
            EmployeeNumber = ev.Employee?.EmployeeNumber ?? "غير محدد",
            EventType = ev.EventType,
            EventDate = ev.EventDate,
            Summary = ev.Summary,
            SourceEntityType = ev.SourceEntityType,
            SourceEntityId = ev.SourceEntityId,
            CreatedAt = ev.CreatedAt
        };

        return ApiResponse<ServiceHistoryEventDto>.Success(result, "تم تحديث الحدث بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteEventAsync(int id, int userId)
    {
        var ev = await _unitOfWork.ServiceHistoryEvents.GetByIdAsync(id);
        if (ev == null) return ApiResponse<bool>.Fail("الحدث غير موجود");

        _unitOfWork.ServiceHistoryEvents.Delete(ev);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف الحدث بنجاح");
    }
}
