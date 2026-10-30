using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Inventory;

public class InventoryCustodyService : IInventoryCustodyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public InventoryCustodyService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<ApiResponse<List<CustodyDto>>> GetAllAsync()
    {
        var custodies = await _unitOfWork.InventoryCustodies.Query()
            .AsNoTracking()
            .Include(c => c.Asset)
            .Include(c => c.Employee)
            .Include(c => c.AssignedByUser)
            .OrderByDescending(c => c.AssignedDate)
            .Select(c => new CustodyDto
            {
                InventoryCustodyID = c.InventoryCustodyID,
                AssetID = c.AssetID,
                AssetName = c.Asset.AssetName,
                AssetNumber = c.Asset.AssetNumber,
                EmployeeID = c.EmployeeID,
                EmployeeName = c.Employee.FullName,
                Quantity = c.Quantity,
                AssignedDate = c.AssignedDate,
                ReturnedDate = c.ReturnedDate,
                Status = (int)c.Status,
                AssignedByName = c.AssignedByUser != null ? c.AssignedByUser.FullName : null,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CustodyDto>>.Success(custodies);
    }

    public async Task<ApiResponse<CustodyDto>> GetByIdAsync(int id)
    {
        var custody = await _unitOfWork.InventoryCustodies.Query()
            .AsNoTracking()
            .Include(c => c.Asset)
            .Include(c => c.Employee)
            .Include(c => c.AssignedByUser)
            .Include(c => c.ReturnedToUser)
            .Where(c => c.InventoryCustodyID == id)
            .Select(c => new CustodyDto
            {
                InventoryCustodyID = c.InventoryCustodyID,
                AssetID = c.AssetID,
                AssetName = c.Asset.AssetName,
                AssetNumber = c.Asset.AssetNumber,
                EmployeeID = c.EmployeeID,
                EmployeeName = c.Employee.FullName,
                Quantity = c.Quantity,
                AssignedDate = c.AssignedDate,
                ReturnedDate = c.ReturnedDate,
                Status = (int)c.Status,
                AssignedByName = c.AssignedByUser != null ? c.AssignedByUser.FullName : null,
                ReturnedToName = c.ReturnedToUser != null ? c.ReturnedToUser.FullName : null,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (custody == null)
            return ApiResponse<CustodyDto>.Fail("العهدة غير موجودة");

        return ApiResponse<CustodyDto>.Success(custody);
    }

    public async Task<ApiResponse<List<CustodyDto>>> GetActiveAsync()
    {
        var custodies = await _unitOfWork.InventoryCustodies.Query()
            .AsNoTracking()
            .Include(c => c.Asset)
            .Include(c => c.Employee)
            .Include(c => c.AssignedByUser)
            .Where(c => c.Status == CustodyStatus.Active)
            .OrderByDescending(c => c.AssignedDate)
            .Select(c => new CustodyDto
            {
                InventoryCustodyID = c.InventoryCustodyID,
                AssetID = c.AssetID,
                AssetName = c.Asset.AssetName,
                AssetNumber = c.Asset.AssetNumber,
                EmployeeID = c.EmployeeID,
                EmployeeName = c.Employee.FullName,
                Quantity = c.Quantity,
                AssignedDate = c.AssignedDate,
                ReturnedDate = c.ReturnedDate,
                Status = (int)c.Status,
                AssignedByName = c.AssignedByUser != null ? c.AssignedByUser.FullName : null,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CustodyDto>>.Success(custodies);
    }

    public async Task<ApiResponse<List<CustodyDto>>> GetByEmployeeAsync(int employeeId)
    {
        var custodies = await _unitOfWork.InventoryCustodies.Query()
            .AsNoTracking()
            .Include(c => c.Asset)
            .Include(c => c.Employee)
            .Include(c => c.AssignedByUser)
            .Where(c => c.EmployeeID == employeeId)
            .OrderByDescending(c => c.AssignedDate)
            .Select(c => new CustodyDto
            {
                InventoryCustodyID = c.InventoryCustodyID,
                AssetID = c.AssetID,
                AssetName = c.Asset.AssetName,
                AssetNumber = c.Asset.AssetNumber,
                EmployeeID = c.EmployeeID,
                EmployeeName = c.Employee.FullName,
                Quantity = c.Quantity,
                AssignedDate = c.AssignedDate,
                ReturnedDate = c.ReturnedDate,
                Status = (int)c.Status,
                AssignedByName = c.AssignedByUser != null ? c.AssignedByUser.FullName : null,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CustodyDto>>.Success(custodies);
    }

    public async Task<ApiResponse<CustodyDto>> CreateAsync(CreateCustodyDto dto, int userId)
    {
        var asset = await _unitOfWork.FixedAssets.GetByIdAsync(dto.AssetID);
        if (asset == null) return ApiResponse<CustodyDto>.Fail("الأصل غير موجود");

        var custody = new InventoryCustody
        {
            AssetID = dto.AssetID,
            EmployeeID = dto.EmployeeID,
            Quantity = dto.Quantity,
            AssignedDate = dto.AssignedDate,
            Status = CustodyStatus.Active,
            AssignedByUserID = userId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserID = userId
        };

        await _unitOfWork.InventoryCustodies.AddAsync(custody);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "InventoryCustody", custody.InventoryCustodyID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        return await GetByIdAsync(custody.InventoryCustodyID);
    }

    public async Task<ApiResponse<CustodyDto>> ReturnCustodyAsync(int id, UpdateCustodyDto dto, int userId)
    {
        var custody = await _unitOfWork.InventoryCustodies.GetByIdAsync(id);
        if (custody == null) return ApiResponse<CustodyDto>.Fail("العهدة غير موجودة");
        if (custody.Status != CustodyStatus.Active)
            return ApiResponse<CustodyDto>.Fail("العهدة قد أغلقت بالفعل");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(custody);

        custody.Status = (CustodyStatus)dto.Status;
        custody.ReturnedDate = dto.ReturnedDate;
        custody.ReturnedToUserID = dto.ReturnedToUserID ?? userId;
        custody.Notes = dto.Notes ?? custody.Notes;
        custody.UpdatedAt = DateTime.UtcNow;
        custody.UpdatedByUserID = userId;

        _unitOfWork.InventoryCustodies.Update(custody);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Return", "InventoryCustody", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        return await GetByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var custody = await _unitOfWork.InventoryCustodies.GetByIdAsync(id);
        if (custody == null) return ApiResponse<bool>.Fail("العهدة غير موجودة");

<<<<<<< HEAD
        custody.IsDeleted = true;
        custody.DeletedAt = DateTime.UtcNow;
        custody.DeletedByUserID = userId;
=======
        _unitOfWork.InventoryCustodies.Delete(custody);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        await _auditService.LogAsync(userId, "", "Delete", "InventoryCustody", id, null, null);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم حذف العهدة بنجاح");
    }
}