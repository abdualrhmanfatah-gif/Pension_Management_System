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
    public InvestmentMaintenanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ----------------------------------------------------------------------
    // MAINTENANCE REQUESTS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto, int userId)
    {
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(dto.PropertyID);
        if (property == null || property.IsDeleted)
            return ApiResponse<int>.Fail("العقار غير موجود");

        if (dto.UnitID.HasValue)
        {
            var unit = await _unitOfWork.PropertyUnits.GetByIdAsync(dto.UnitID.Value);
            if (unit == null || unit.IsDeleted)
                return ApiResponse<int>.Fail("الوحدة غير موجودة");
        }

        if (dto.TenantID.HasValue)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(dto.TenantID.Value);
            if (tenant == null || tenant.IsDeleted)
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
        var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(id);
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

        return ApiResponse<MaintenanceRequestDto>.Success(dto);
    }

    public async Task<ApiResponse<List<MaintenanceRequestDto>>> GetRequestsByPropertyAsync(int propertyId)
    {
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

        return ApiResponse<List<MaintenanceRequestDto>>.Success(requests);
    }

    public async Task<ApiResponse<List<MaintenanceRequestDto>>> GetAllMaintenanceRequestsAsync()
    {
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

    // ----------------------------------------------------------------------
    // WORK ORDERS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateWorkOrderAsync(CreateWorkOrderDto dto, int userId)
    {
        var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(dto.RequestID);
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
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(id);
        if (workOrder == null || workOrder.IsDeleted)
            return ApiResponse<bool>.Fail("أمر العمل غير موجود");

        workOrder.ActualCost = dto.ActualCost;
        workOrder.CompletionDate = dto.CompletionDate;
        workOrder.InvoiceFilePath = dto.InvoiceFilePath;
        workOrder.CompletionCertPath = dto.CompletionCertPath;
        workOrder.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.WorkOrders.Update(workOrder);

        // Check if request should be marked completed
        var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(workOrder.RequestID);
        if (request != null && !request.IsDeleted)
        {
            request.Status = MaintenanceStatusConstants.Completed;
            _unitOfWork.MaintenanceRequests.Update(request);
        }

        await _unitOfWork.SaveChangesAsync();



        return ApiResponse<bool>.Success(true, "تم تحديث أمر العمل بنجاح");
    }

    public async Task<ApiResponse<WorkOrderDto>> GetWorkOrderByIdAsync(int id)
    {
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

        return ApiResponse<WorkOrderDto>.Success(dto);
    }

    public async Task<ApiResponse<List<WorkOrderDto>>> GetWorkOrdersByRequestAsync(int requestId)
    {
        var workOrders = await _unitOfWork.WorkOrders.Query()
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
