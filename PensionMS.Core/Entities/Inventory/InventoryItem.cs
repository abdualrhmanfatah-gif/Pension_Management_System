using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Inventory;

public class InventoryItem : ISoftDeletable
{
    public int InventoryItemID { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; }
    public string? Location { get; set; }
    public ItemCondition Condition { get; set; } = ItemCondition.New;
    public ItemStatus Status { get; set; } = ItemStatus.Active;
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public byte[]? RowVersion { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
