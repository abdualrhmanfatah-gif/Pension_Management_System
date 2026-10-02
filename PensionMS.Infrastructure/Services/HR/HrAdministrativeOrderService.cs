using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.HR;

public class HrAdministrativeOrderService : IHrAdministrativeOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public HrAdministrativeOrderService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<PagedResult<AdministrativeOrderListItemDto>> GetAllAsync(int page, int pageSize, int? employeeId = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _unitOfWork.AdministrativeOrders.Query()
            .AsNoTracking()
            .Where(o => !o.IsDeleted);

        if (employeeId.HasValue)
        {
            query = query.Where(o => o.EmployeeID == employeeId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(o => o.AdministrativeOrderType)
            .Include(o => o.Employee)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdministrativeOrderListItemDto
            {
                AdministrativeOrderID = o.AdministrativeOrderID,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TypeName = o.AdministrativeOrderType.Name,
                Subject = o.Subject,
                EmployeeFullName = o.Employee.FullName,
                Status = o.Status
            })
            .ToListAsync();

        return new PagedResult<AdministrativeOrderListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdministrativeOrderDetailsDto?> GetByIdAsync(int id)
    {
        return await _unitOfWork.AdministrativeOrders.Query()
            .AsNoTracking()
            .Where(o => o.AdministrativeOrderID == id && !o.IsDeleted)
            .Select(o => new AdministrativeOrderDetailsDto
            {
                AdministrativeOrderID = o.AdministrativeOrderID,
                EmployeeID = o.EmployeeID,
                AdministrativeOrderTypeID = o.AdministrativeOrderTypeID,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TypeName = o.AdministrativeOrderType.Name,
                Subject = o.Subject,
                EmployeeFullName = o.Employee.FullName,
                Status = o.Status,
                EffectiveDate = o.EffectiveDate,
                Notes = o.Notes,
                ApprovedByUserName = o.ApprovedByUser != null ? o.ApprovedByUser.FullName : null,
                ApprovedAt = o.ApprovedAt,
                CreatedAt = o.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AdministrativeOrderDetailsDto> CreateAsync(CreateAdministrativeOrderDto dto)
    {
        var order = new AdministrativeOrder
        {
            EmployeeID = dto.EmployeeID,
            AdministrativeOrderTypeID = dto.AdministrativeOrderTypeID,
            OrderNumber = dto.OrderNumber,
            OrderDate = dto.OrderDate,
            EffectiveDate = dto.EffectiveDate,
            Subject = dto.Subject,
            Notes = dto.Notes,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AdministrativeOrders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return (await GetByIdAsync(order.AdministrativeOrderID))!;
    }

    public async Task<AdministrativeOrderDetailsDto?> IssueAsync(int id, IssueAdministrativeOrderDto dto)
    {
        var order = await _unitOfWork.AdministrativeOrders.Query()
            .FirstOrDefaultAsync(o => o.AdministrativeOrderID == id && !o.IsDeleted);

        if (order == null) return null;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(order);

        order.Status = "Issued";
        order.ApprovedByUserID = dto.IssuedByUserID;
        order.ApprovedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(dto.IssuedByUserID, "", "Issue", "AdministrativeOrder", id, oldValues, System.Text.Json.JsonSerializer.Serialize(order));

        return await GetByIdAsync(order.AdministrativeOrderID);
    }
}
