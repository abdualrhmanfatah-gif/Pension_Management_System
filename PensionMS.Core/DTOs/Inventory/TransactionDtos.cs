namespace PensionMS.Core.DTOs.Inventory;

public class TransactionDto
{
    public int InventoryTransactionID { get; set; }
    public int InventoryItemID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int TransactionType { get; set; }
    public int Quantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? WithdrawalOrderNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? PerformedByName { get; set; }
    public string? BatchID { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTransactionDto
{
    public int InventoryItemID { get; set; }
    public int TransactionType { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Currency { get; set; }
    public string? WithdrawalOrderNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
}

public class BulkTransactionItemDto
{
    public int InventoryItemID { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
}

public class CreateBulkTransactionDto
{
    public int TransactionType { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Currency { get; set; }
    public string? WithdrawalOrderNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public List<BulkTransactionItemDto> Items { get; set; } = new();
}
