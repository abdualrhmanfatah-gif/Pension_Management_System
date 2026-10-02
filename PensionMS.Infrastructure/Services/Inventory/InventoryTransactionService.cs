using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Inventory;

public class InventoryTransactionService : IInventoryTransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public InventoryTransactionService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    private async Task<string> GenerateWithdrawalOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"WO-{year}-";
        var lastOrder = await _unitOfWork.InventoryTransactions.Query()
            .AsNoTracking()
            .Where(t => t.WithdrawalOrderNumber != null && t.WithdrawalOrderNumber.StartsWith(prefix))
            .OrderByDescending(t => t.WithdrawalOrderNumber)
            .Select(t => t.WithdrawalOrderNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastOrder != null)
        {
            var parts = lastOrder.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D5}";
    }

    public async Task<ApiResponse<List<TransactionDto>>> GetAllAsync()
    {
        var transactions = await _unitOfWork.InventoryTransactions.Query()
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Include(t => t.PerformedByUser)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.InventoryTransactionID)
            .Select(t => new TransactionDto
            {
                InventoryTransactionID = t.InventoryTransactionID,
                InventoryItemID = t.InventoryItemID,
                ItemName = t.InventoryItem.ItemName,
                ItemCode = t.InventoryItem.ItemCode,
                TransactionType = (int)t.TransactionType,
                Quantity = t.Quantity,
                PreviousQuantity = t.PreviousQuantity,
                NewQuantity = t.NewQuantity,
                InvoiceNumber = t.InvoiceNumber,
                ReferenceNumber = t.ReferenceNumber,
                Reason = t.Reason,
                UnitCost = t.UnitCost,
                Currency = t.Currency,
                WithdrawalOrderNumber = t.WithdrawalOrderNumber,
                TransactionDate = t.TransactionDate,
                PerformedByName = t.PerformedByUser != null ? t.PerformedByUser.FullName : null,
                BatchID = t.BatchID != null ? t.BatchID.ToString() : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<TransactionDto>>.Success(transactions);
    }

    public async Task<ApiResponse<List<TransactionDto>>> GetByItemAsync(int itemId)
    {
        var item = await _unitOfWork.InventoryItems.GetByIdAsync(itemId);
        if (item == null)
            return ApiResponse<List<TransactionDto>>.Fail("الصنف غير موجود");

        var transactions = await _unitOfWork.InventoryTransactions.Query()
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Include(t => t.PerformedByUser)
            .Where(t => t.InventoryItemID == itemId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.InventoryTransactionID)
            .Select(t => new TransactionDto
            {
                InventoryTransactionID = t.InventoryTransactionID,
                InventoryItemID = t.InventoryItemID,
                ItemName = t.InventoryItem.ItemName,
                ItemCode = t.InventoryItem.ItemCode,
                TransactionType = (int)t.TransactionType,
                Quantity = t.Quantity,
                PreviousQuantity = t.PreviousQuantity,
                NewQuantity = t.NewQuantity,
                InvoiceNumber = t.InvoiceNumber,
                ReferenceNumber = t.ReferenceNumber,
                Reason = t.Reason,
                UnitCost = t.UnitCost,
                Currency = t.Currency,
                WithdrawalOrderNumber = t.WithdrawalOrderNumber,
                TransactionDate = t.TransactionDate,
                PerformedByName = t.PerformedByUser != null ? t.PerformedByUser.FullName : null,
                BatchID = t.BatchID != null ? t.BatchID.ToString() : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<TransactionDto>>.Success(transactions);
    }

    public async Task<ApiResponse<List<TransactionDto>>> GetByWithdrawalOrderAsync(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return ApiResponse<List<TransactionDto>>.Fail("رقم أمر الصرف مطلوب");

        var transactions = await _unitOfWork.InventoryTransactions.Query()
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Include(t => t.PerformedByUser)
            .Where(t => t.WithdrawalOrderNumber == orderNumber)
            .OrderBy(t => t.InventoryItem.ItemName)
            .Select(t => new TransactionDto
            {
                InventoryTransactionID = t.InventoryTransactionID,
                InventoryItemID = t.InventoryItemID,
                ItemName = t.InventoryItem.ItemName,
                ItemCode = t.InventoryItem.ItemCode,
                TransactionType = (int)t.TransactionType,
                Quantity = t.Quantity,
                PreviousQuantity = t.PreviousQuantity,
                NewQuantity = t.NewQuantity,
                InvoiceNumber = t.InvoiceNumber,
                ReferenceNumber = t.ReferenceNumber,
                Reason = t.Reason,
                UnitCost = t.UnitCost,
                Currency = t.Currency,
                WithdrawalOrderNumber = t.WithdrawalOrderNumber,
                TransactionDate = t.TransactionDate,
                PerformedByName = t.PerformedByUser != null ? t.PerformedByUser.FullName : null,
                BatchID = t.BatchID != null ? t.BatchID.ToString() : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<TransactionDto>>.Success(transactions);
    }

    public async Task<ApiResponse<TransactionDto>> GetByIdAsync(int id)
    {
        var transaction = await _unitOfWork.InventoryTransactions.Query()
            .AsNoTracking()
            .Include(t => t.InventoryItem)
            .Include(t => t.PerformedByUser)
            .Where(t => t.InventoryTransactionID == id)
            .Select(t => new TransactionDto
            {
                InventoryTransactionID = t.InventoryTransactionID,
                InventoryItemID = t.InventoryItemID,
                ItemName = t.InventoryItem.ItemName,
                ItemCode = t.InventoryItem.ItemCode,
                TransactionType = (int)t.TransactionType,
                Quantity = t.Quantity,
                PreviousQuantity = t.PreviousQuantity,
                NewQuantity = t.NewQuantity,
                InvoiceNumber = t.InvoiceNumber,
                ReferenceNumber = t.ReferenceNumber,
                Reason = t.Reason,
                UnitCost = t.UnitCost,
                Currency = t.Currency,
                WithdrawalOrderNumber = t.WithdrawalOrderNumber,
                TransactionDate = t.TransactionDate,
                PerformedByName = t.PerformedByUser != null ? t.PerformedByUser.FullName : null,
                BatchID = t.BatchID != null ? t.BatchID.ToString() : null,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (transaction == null)
            return ApiResponse<TransactionDto>.Fail("الحركة غير موجودة");

        return ApiResponse<TransactionDto>.Success(transaction);
    }

    public async Task<ApiResponse<TransactionDto>> CreateAsync(CreateTransactionDto dto, int userId)
    {
        var item = await _unitOfWork.InventoryItems.GetByIdAsync(dto.InventoryItemID);
        if (item == null) return ApiResponse<TransactionDto>.Fail("الصنف غير موجود");

        var previousQty = item.Quantity;
        int newQty = previousQty;

        switch ((TransactionType)dto.TransactionType)
        {
            case TransactionType.StockIn:
                newQty = previousQty + dto.Quantity;
                break;
            case TransactionType.StockOut:
                if (previousQty < dto.Quantity)
                    return ApiResponse<TransactionDto>.Fail("الكمية غير متوفرة في المخزون");
                newQty = previousQty - dto.Quantity;
                break;
            case TransactionType.Adjustment:
                newQty = dto.Quantity;
                break;
            case TransactionType.Disposal:
                if (previousQty < dto.Quantity)
                    return ApiResponse<TransactionDto>.Fail("الكمية غير متوفرة في المخزون");
                newQty = previousQty - dto.Quantity;
                break;
            case TransactionType.Transfer:
                if (previousQty < dto.Quantity)
                    return ApiResponse<TransactionDto>.Fail("الكمية غير متوفرة في المخزون");
                newQty = previousQty - dto.Quantity;
                break;
        }

        var transaction = new InventoryTransaction
        {
            InventoryItemID = dto.InventoryItemID,
            TransactionType = (TransactionType)dto.TransactionType,
            Quantity = dto.Quantity,
            PreviousQuantity = previousQty,
            NewQuantity = newQty,
            InvoiceNumber = dto.InvoiceNumber,
            ReferenceNumber = dto.ReferenceNumber,
            Reason = dto.Reason,
            UnitCost = dto.UnitCost,
            Currency = dto.Currency,
            WithdrawalOrderNumber = dto.WithdrawalOrderNumber,
            TransactionDate = dto.TransactionDate,
            PerformedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InventoryTransactions.AddAsync(transaction);

        item.Quantity = newQty;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedByUserID = userId;
        _unitOfWork.InventoryItems.Update(item);

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "InventoryTransaction", transaction.InventoryTransactionID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        return await GetByIdAsync(transaction.InventoryTransactionID);
    }

    public async Task<ApiResponse<List<TransactionDto>>> CreateBulkAsync(CreateBulkTransactionDto dto, int userId)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return ApiResponse<List<TransactionDto>>.Fail("يجب إضافة صنف واحد على الأقل");

        var transactionType = (TransactionType)dto.TransactionType;

        if (transactionType == TransactionType.StockOut && string.IsNullOrEmpty(dto.WithdrawalOrderNumber))
            dto.WithdrawalOrderNumber = await GenerateWithdrawalOrderNumberAsync();

        var batchId = Guid.NewGuid();
        var inventoryTransactions = new List<InventoryTransaction>();
        var itemIds = dto.Items.Select(i => i.InventoryItemID).ToList();
        var items = await _unitOfWork.InventoryItems.Query()
            .Where(i => itemIds.Contains(i.InventoryItemID))
            .ToListAsync();

        foreach (var itemDto in dto.Items)
        {
            var item = items.FirstOrDefault(i => i.InventoryItemID == itemDto.InventoryItemID);
            if (item == null)
                return ApiResponse<List<TransactionDto>>.Fail($"الصنف (ID: {itemDto.InventoryItemID}) غير موجود");

            var previousQty = item.Quantity;
            int newQty = previousQty;

            switch (transactionType)
            {
                case TransactionType.StockIn:
                    newQty = previousQty + itemDto.Quantity;
                    break;
                case TransactionType.StockOut:
                case TransactionType.Disposal:
                case TransactionType.Transfer:
                    if (previousQty < itemDto.Quantity)
                        return ApiResponse<List<TransactionDto>>.Fail($"الكمية غير متوفرة للصنف '{item.ItemName}' (المتوفر: {previousQty})");
                    newQty = previousQty - itemDto.Quantity;
                    break;
                case TransactionType.Adjustment:
                    newQty = itemDto.Quantity;
                    break;
            }

            inventoryTransactions.Add(new InventoryTransaction
            {
                InventoryItemID = itemDto.InventoryItemID,
                TransactionType = transactionType,
                Quantity = itemDto.Quantity,
                PreviousQuantity = previousQty,
                NewQuantity = newQty,
                InvoiceNumber = dto.InvoiceNumber,
                ReferenceNumber = dto.ReferenceNumber,
                Reason = dto.Reason,
                UnitCost = itemDto.UnitCost,
                Currency = dto.Currency,
                WithdrawalOrderNumber = dto.WithdrawalOrderNumber,
                TransactionDate = dto.TransactionDate,
                PerformedByUserID = userId,
                BatchID = batchId,
                CreatedAt = DateTime.UtcNow
            });

            item.Quantity = newQty;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedByUserID = userId;
        }

        await _unitOfWork.InventoryTransactions.AddRangeAsync(inventoryTransactions);
        await _unitOfWork.SaveChangesAsync();

        var itemMap = items.ToDictionary(i => i.InventoryItemID);
        var result = inventoryTransactions.Select(t => new TransactionDto
        {
            InventoryTransactionID = t.InventoryTransactionID,
            InventoryItemID = t.InventoryItemID,
            ItemName = itemMap[t.InventoryItemID].ItemName,
            ItemCode = itemMap[t.InventoryItemID].ItemCode,
            TransactionType = (int)t.TransactionType,
            Quantity = t.Quantity,
            PreviousQuantity = t.PreviousQuantity,
            NewQuantity = t.NewQuantity,
            InvoiceNumber = t.InvoiceNumber,
            ReferenceNumber = t.ReferenceNumber,
            Reason = t.Reason,
            UnitCost = t.UnitCost,
            Currency = t.Currency,
            WithdrawalOrderNumber = t.WithdrawalOrderNumber,
            TransactionDate = t.TransactionDate,
            PerformedByName = null,
            BatchID = t.BatchID?.ToString(),
            CreatedAt = t.CreatedAt
        }).ToList();

        await _auditService.LogAsync(userId, "", "CreateBulk", "InventoryTransaction", 0, null, System.Text.Json.JsonSerializer.Serialize(dto));

        return ApiResponse<List<TransactionDto>>.Success(result);
    }
}
