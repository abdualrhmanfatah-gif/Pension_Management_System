using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Inventory;

public class InventoryItemService : IInventoryItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public InventoryItemService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<ApiResponse<List<ItemDto>>> GetAllAsync()
    {
        var items = await _unitOfWork.InventoryItems.Query()
            .AsNoTracking()
            .OrderBy(i => i.ItemName)
            .Select(i => new ItemDto
            {
                InventoryItemID = i.InventoryItemID,
                ItemName = i.ItemName,
                ItemCode = i.ItemCode,
                Description = i.Description,
                Unit = i.Unit,
                Quantity = i.Quantity,
                MinimumQuantity = i.MinimumQuantity,
                Location = i.Location,
                Condition = (int)i.Condition,
                Status = (int)i.Status,
                SerialNumber = i.SerialNumber,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<ItemDto>>.Success(items);
    }

    public async Task<ApiResponse<ItemDto>> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.InventoryItems.Query()
            .AsNoTracking()
            .Where(i => i.InventoryItemID == id)
            .Select(i => new ItemDto
            {
                InventoryItemID = i.InventoryItemID,
                ItemName = i.ItemName,
                ItemCode = i.ItemCode,
                Description = i.Description,
                Unit = i.Unit,
                Quantity = i.Quantity,
                MinimumQuantity = i.MinimumQuantity,
                Location = i.Location,
                Condition = (int)i.Condition,
                Status = (int)i.Status,
                SerialNumber = i.SerialNumber,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return ApiResponse<ItemDto>.Fail("الصنف غير موجود");

        return ApiResponse<ItemDto>.Success(item);
    }

    public async Task<ApiResponse<ItemDto>> CreateAsync(CreateItemDto dto, int userId)
    {
        var item = new InventoryItem
        {
            ItemName = dto.ItemName,
            ItemCode = dto.ItemCode,
            Description = dto.Description,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            MinimumQuantity = dto.MinimumQuantity,
            Location = dto.Location,
            Condition = dto.Condition.HasValue ? (ItemCondition)dto.Condition.Value : ItemCondition.New,
            Status = ItemStatus.Active,
            SerialNumber = dto.SerialNumber,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserID = userId
        };

        await _unitOfWork.InventoryItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "InventoryItem", item.InventoryItemID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        return await GetByIdAsync(item.InventoryItemID);
    }

    public async Task<ApiResponse<ItemDto>> UpdateAsync(int id, UpdateItemDto dto, int userId)
    {
        var item = await _unitOfWork.InventoryItems.GetByIdAsync(id);
        if (item == null) return ApiResponse<ItemDto>.Fail("الصنف غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(item);
        var oldQuantity = item.Quantity;

        if (dto.ItemName != null) item.ItemName = dto.ItemName;
        if (dto.ItemCode != null) item.ItemCode = dto.ItemCode;
        if (dto.Description != null) item.Description = dto.Description;
        if (dto.Unit != null) item.Unit = dto.Unit;
        if (dto.Quantity.HasValue) item.Quantity = dto.Quantity.Value;
        if (dto.MinimumQuantity.HasValue) item.MinimumQuantity = dto.MinimumQuantity.Value;
        if (dto.Location != null) item.Location = dto.Location;
        if (dto.Condition.HasValue) item.Condition = (ItemCondition)dto.Condition.Value;
        if (dto.Status.HasValue) item.Status = (ItemStatus)dto.Status.Value;
        if (dto.SerialNumber != null) item.SerialNumber = dto.SerialNumber;
        if (dto.Notes != null) item.Notes = dto.Notes;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedByUserID = userId;

        _unitOfWork.InventoryItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        // Log quantity change as adjustment transaction
        if (dto.Quantity.HasValue && dto.Quantity.Value != oldQuantity)
        {
            var adjustment = new InventoryTransaction
            {
                InventoryItemID = id,
                TransactionType = TransactionType.Adjustment,
                Quantity = dto.Quantity.Value - oldQuantity,
                PreviousQuantity = oldQuantity,
                NewQuantity = dto.Quantity.Value,
                Reason = "تعديل الكمية يدوياً",
                TransactionDate = DateTime.UtcNow,
                PerformedByUserID = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.InventoryTransactions.AddAsync(adjustment);
            await _unitOfWork.SaveChangesAsync();
        }

        await _auditService.LogAsync(userId, "", "Update", "InventoryItem", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        return await GetByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var item = await _unitOfWork.InventoryItems.GetByIdAsync(id);
        if (item == null) return ApiResponse<bool>.Fail("الصنف غير موجود");

        _unitOfWork.InventoryItems.Delete(item);
        await _auditService.LogAsync(userId, "", "Delete", "InventoryItem", id, null, null);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم حذف الصنف بنجاح");
    }
}
