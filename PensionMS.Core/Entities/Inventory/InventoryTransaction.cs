namespace PensionMS.Core.Entities.Inventory;

public class InventoryTransaction
{
    public int InventoryTransactionID { get; set; }
    public int InventoryItemID { get; set; }
    public virtual InventoryItem InventoryItem { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal? UnitCost { get; set; }
    public string Currency { get; set; } = "YER";
    public string? WithdrawalOrderNumber { get; set; }
    public int? PerformedByUserID { get; set; }
    public virtual User? PerformedByUser { get; set; }
    public Guid? BatchID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte[]? RowVersion { get; set; }
}
