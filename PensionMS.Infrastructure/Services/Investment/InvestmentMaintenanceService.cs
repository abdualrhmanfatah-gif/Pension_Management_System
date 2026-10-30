using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentMaintenanceService : IInvestmentMaintenanceService
{
    private readonly IUnitOfWork _unitOfWork;
<<<<<<< HEAD
    private readonly IAccessiblePensionRepository _access;

    public InvestmentMaintenanceService(IUnitOfWork unitOfWork, IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _access = access;
=======
    public InvestmentMaintenanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }

    // ----------------------------------------------------------------------
    // MAINTENANCE REQUESTS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == dto.PropertyID && !p.IsDeleted);
=======
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(dto.PropertyID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (property == null || property.IsDeleted)
            return ApiResponse<int>.Fail("العقار غير موجود");

        if (dto.UnitID.HasValue)
        {
<<<<<<< HEAD
            var unit = await queries.Query(_unitOfWork.PropertyUnits)
                .FirstOrDefaultAsync(u => u.UnitID == dto.UnitID.Value && u.PropertyID == dto.PropertyID && !u.IsDeleted);
=======
            var unit = await _unitOfWork.PropertyUnits.GetByIdAsync(dto.UnitID.Value);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (unit == null || unit.IsDeleted)
                return ApiResponse<int>.Fail("الوحدة غير موجودة");
        }

        if (dto.TenantID.HasValue)
        {
<<<<<<< HEAD
            if (!await ReferenceableTenants(queries).AnyAsync(t => t.TenantID == dto.TenantID.Value))
=======
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(dto.TenantID.Value);
            if (tenant == null || tenant.IsDeleted)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                return ApiResponse<int>.Fail("المستأجر غير موجود");
        }

        var request = new MaintenanceRequest
        {
            PropertyID = dto.PropertyID,
            UnitID = dto.UnitID,
            TenantID = dto.TenantID,
            Priority = dto.Priority,
            Description = dto.Description,
            DamagePhotosJson = dto.DamagePhotosJson,
            ReportedDate = dto.ReportedDate,
            Status = MaintenanceStatusConstants.New,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MaintenanceRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(request.RequestID, "تم إنشاء طلب الصيانة بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdateMaintenanceRequestAsync(int id, UpdateMaintenanceRequestDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var request = await queries.Query(_unitOfWork.MaintenanceRequests)
            .FirstOrDefaultAsync(r => r.RequestID == id && !r.IsDeleted);
=======
        var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (request == null || request.IsDeleted)
            return ApiResponse<bool>.Fail("طلب الصيانة غير موجود");

        request.Status = dto.Status;
        // Optionally save notes or other details if added to entity later
        request.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.MaintenanceRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث حالة طلب الصيانة بنجاح");
    }

    public async Task<ApiResponse<MaintenanceRequestDto>> GetMaintenanceRequestByIdAsync(int id)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var dto = await ProjectRequests(queries.Query(_unitOfWork.MaintenanceRequests)
            .Where(r => r.RequestID == id && !r.IsDeleted), queries).FirstOrDefaultAsync();
        if (dto == null)
            return ApiResponse<MaintenanceRequestDto>.Fail("طلب الصيانة غير موجود");

=======
        var request = await _unitOfWork.MaintenanceRequests.Query()
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Include(r => r.Tenant)
            .FirstOrDefaultAsync(r => r.RequestID == id && !r.IsDeleted);

        if (request == null)
            return ApiResponse<MaintenanceRequestDto>.Fail("طلب الصيانة غير موجود");

        var dto = new MaintenanceRequestDto
        {
            RequestID = request.RequestID,
            PropertyID = request.PropertyID,
            PropertyName = request.Property?.PropertyName ?? string.Empty,
            UnitID = request.UnitID,
            UnitNumber = request.Unit?.UnitNumber,
            TenantID = request.TenantID,
            TenantName = request.Tenant?.FullName,
            Priority = request.Priority,
            Description = request.Description,
            Status = request.Status,
            ReportedDate = request.ReportedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        };

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return ApiResponse<MaintenanceRequestDto>.Success(dto);
    }

    public async Task<ApiResponse<List<MaintenanceRequestDto>>> GetRequestsByPropertyAsync(int propertyId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var query = queries.Query(_unitOfWork.MaintenanceRequests).Where(r => !r.IsDeleted);
        if (propertyId > 0)
            query = query.Where(r => r.PropertyID == propertyId);

        var requests = await ProjectRequests(query.OrderByDescending(r => r.ReportedDate), queries).ToListAsync();
=======
        var query = _unitOfWork.MaintenanceRequests.Query()
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Include(r => r.Tenant)
            .Where(r => !r.IsDeleted);

        if (propertyId > 0)
        {
            query = query.Where(r => r.PropertyID == propertyId);
        }

        var requests = await query
            .OrderByDescending(r => r.ReportedDate)
            .Select(r => new MaintenanceRequestDto
            {
                RequestID = r.RequestID,
                PropertyID = r.PropertyID,
                PropertyName = r.Property != null ? r.Property.PropertyName : string.Empty,
                UnitID = r.UnitID,
                UnitNumber = r.Unit != null ? r.Unit.UnitNumber : null,
                TenantID = r.TenantID,
                TenantName = r.Tenant != null ? r.Tenant.FullName : null,
                Priority = r.Priority,
                Description = r.Description,
                Status = r.Status,
                ReportedDate = r.ReportedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
            })
            .ToListAsync();

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return ApiResponse<List<MaintenanceRequestDto>>.Success(requests);
    }

    public async Task<ApiResponse<List<MaintenanceRequestDto>>> GetAllMaintenanceRequestsAsync()
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var requests = await ProjectRequests(queries.Query(_unitOfWork.MaintenanceRequests)
            .Where(r => !r.IsDeleted).OrderByDescending(r => r.ReportedDate), queries).ToListAsync();
        return ApiResponse<List<MaintenanceRequestDto>>.Success(requests);
    }

    private IQueryable<Tenant> ReferenceableTenants(IPensionDataQuery queries)
    {
        var contracts = queries.Query(_unitOfWork.LeaseContracts);
        // Tenants have no branch owner. Preserve first linkage and shared-party contexts,
        // without granting access to their other contracts or master profiles.
        return _unitOfWork.Tenants.Query().Where(t => !t.IsDeleted &&
            (!t.LeaseContracts.Any() || contracts.Any(c => c.TenantID == t.TenantID)));
    }

    private IQueryable<MaintenanceRequestDto> ProjectRequests(IQueryable<MaintenanceRequest> requests, IPensionDataQuery queries)
    {
        var units = queries.Query(_unitOfWork.PropertyUnits).Where(u => !u.IsDeleted);
        var tenants = ReferenceableTenants(queries);
        return requests.AsNoTracking().Select(r => new MaintenanceRequestDto
        {
            RequestID = r.RequestID,
            PropertyID = r.PropertyID,
            PropertyName = r.Property != null ? r.Property.PropertyName : string.Empty,
            // Legacy inconsistent references must not disclose another property's unit.
            UnitID = units.Where(u => u.UnitID == r.UnitID && u.PropertyID == r.PropertyID)
                .Select(u => (int?)u.UnitID).FirstOrDefault(),
            UnitNumber = units.Where(u => u.UnitID == r.UnitID && u.PropertyID == r.PropertyID)
                .Select(u => u.UnitNumber).FirstOrDefault(),
            TenantID = tenants.Where(t => t.TenantID == r.TenantID).Select(t => (int?)t.TenantID).FirstOrDefault(),
            TenantName = tenants.Where(t => t.TenantID == r.TenantID).Select(t => t.FullName).FirstOrDefault(),
            Priority = r.Priority,
            Description = r.Description,
            Status = r.Status,
            ReportedDate = r.ReportedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        });
    }

=======
        var requests = await _unitOfWork.MaintenanceRequests.Query()
            .Include(r => r.Property)
            .Include(r => r.Unit)
            .Include(r => r.Tenant)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.ReportedDate)
            .Select(r => new MaintenanceRequestDto
            {
                RequestID = r.RequestID,
                PropertyID = r.PropertyID,
                PropertyName = r.Property != null ? r.Property.PropertyName : string.Empty,
                UnitID = r.UnitID,
                UnitNumber = r.Unit != null ? r.Unit.UnitNumber : null,
                TenantID = r.TenantID,
                TenantName = r.Tenant != null ? r.Tenant.FullName : null,
                Priority = r.Priority,
                Description = r.Description,
                Status = r.Status,
                ReportedDate = r.ReportedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
            })
            .ToListAsync();

        return ApiResponse<List<MaintenanceRequestDto>>.Success(requests);
    }

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    // ----------------------------------------------------------------------
    // WORK ORDERS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateWorkOrderAsync(CreateWorkOrderDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var request = await queries.Query(_unitOfWork.MaintenanceRequests)
            .FirstOrDefaultAsync(r => r.RequestID == dto.RequestID && !r.IsDeleted);
=======
        var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(dto.RequestID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (request == null || request.IsDeleted)
            return ApiResponse<int>.Fail("طلب الصيانة غير موجود");

        var workOrder = new WorkOrder
        {
            RequestID = dto.RequestID,
            AssignedContractor = dto.AssignedContractor,
            EstimatedCost = dto.EstimatedCost,
            ActualCost = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.WorkOrders.AddAsync(workOrder);
        
        // Update request status
        if (request.Status == MaintenanceStatusConstants.New)
        {
            request.Status = MaintenanceStatusConstants.InProgress;
            _unitOfWork.MaintenanceRequests.Update(request);
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(workOrder.WorkOrderID, "تم إنشاء أمر العمل بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdateWorkOrderAsync(int id, UpdateWorkOrderDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var workOrder = await queries.Query(_unitOfWork.WorkOrders)
            .FirstOrDefaultAsync(w => w.WorkOrderID == id && !w.IsDeleted);
        if (workOrder == null || workOrder.IsDeleted)
            return ApiResponse<bool>.Fail("أمر العمل غير موجود");

        var request = await queries.Query(_unitOfWork.MaintenanceRequests)
            .FirstOrDefaultAsync(r => r.RequestID == workOrder.RequestID && !r.IsDeleted);
        if (request == null)
            return ApiResponse<bool>.Fail("طلب الصيانة غير موجود");

=======
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(id);
        if (workOrder == null || workOrder.IsDeleted)
            return ApiResponse<bool>.Fail("أمر العمل غير موجود");

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        workOrder.ActualCost = dto.ActualCost;
        workOrder.CompletionDate = dto.CompletionDate;
        workOrder.InvoiceFilePath = dto.InvoiceFilePath;
        workOrder.CompletionCertPath = dto.CompletionCertPath;
        workOrder.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.WorkOrders.Update(workOrder);

        // Check if request should be marked completed
<<<<<<< HEAD
        request.Status = MaintenanceStatusConstants.Completed;
        _unitOfWork.MaintenanceRequests.Update(request);
=======
        var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(workOrder.RequestID);
        if (request != null && !request.IsDeleted)
        {
            request.Status = MaintenanceStatusConstants.Completed;
            _unitOfWork.MaintenanceRequests.Update(request);
        }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        await _unitOfWork.SaveChangesAsync();



        return ApiResponse<bool>.Success(true, "تم تحديث أمر العمل بنجاح");
    }

    public async Task<ApiResponse<WorkOrderDto>> GetWorkOrderByIdAsync(int id)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var dto = await queries.Query(_unitOfWork.WorkOrders).AsNoTracking()
            .Where(w => w.WorkOrderID == id && !w.IsDeleted)
            .Select(w => new WorkOrderDto
            {
                WorkOrderID = w.WorkOrderID,
                RequestID = w.RequestID,
                AssignedContractor = w.AssignedContractor,
                EstimatedCost = w.EstimatedCost,
                ActualCost = w.ActualCost,
                CompletionDate = w.CompletionDate.HasValue ? w.CompletionDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : null,
                InvoiceFilePath = w.InvoiceFilePath,
                CompletionCertPath = w.CompletionCertPath
            }).FirstOrDefaultAsync();
        if (dto == null)
            return ApiResponse<WorkOrderDto>.Fail("أمر العمل غير موجود");

=======
        var workOrder = await _unitOfWork.WorkOrders.Query()
            .FirstOrDefaultAsync(w => w.WorkOrderID == id && !w.IsDeleted);

        if (workOrder == null)
            return ApiResponse<WorkOrderDto>.Fail("أمر العمل غير موجود");

        var dto = new WorkOrderDto
        {
            WorkOrderID = workOrder.WorkOrderID,
            RequestID = workOrder.RequestID,
            AssignedContractor = workOrder.AssignedContractor,
            EstimatedCost = workOrder.EstimatedCost,
            ActualCost = workOrder.ActualCost,
            CompletionDate = workOrder.CompletionDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            InvoiceFilePath = workOrder.InvoiceFilePath,
            CompletionCertPath = workOrder.CompletionCertPath
        };

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return ApiResponse<WorkOrderDto>.Success(dto);
    }

    public async Task<ApiResponse<List<WorkOrderDto>>> GetWorkOrdersByRequestAsync(int requestId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var workOrders = await queries.Query(_unitOfWork.WorkOrders).AsNoTracking()
=======
        var workOrders = await _unitOfWork.WorkOrders.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Where(w => w.RequestID == requestId && !w.IsDeleted)
            .Select(w => new WorkOrderDto
            {
                WorkOrderID = w.WorkOrderID,
                RequestID = w.RequestID,
                AssignedContractor = w.AssignedContractor,
                EstimatedCost = w.EstimatedCost,
                ActualCost = w.ActualCost,
                CompletionDate = w.CompletionDate.HasValue ? w.CompletionDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : null,
                InvoiceFilePath = w.InvoiceFilePath,
                CompletionCertPath = w.CompletionCertPath
            })
            .ToListAsync();

        return ApiResponse<List<WorkOrderDto>>.Success(workOrders);
    }
}
