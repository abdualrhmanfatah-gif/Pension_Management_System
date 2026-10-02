namespace PensionMS.Core.DTOs.Inventory;

public class ItemDto
{
    public int InventoryItemID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; }
    public string? Location { get; set; }
    public int Condition { get; set; }
    public int Status { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; }
    public string? Location { get; set; }
    public int? Condition { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
}

public class UpdateItemDto
{
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public int? Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public string? Location { get; set; }
    public int? Condition { get; set; }
    public int? Status { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
}
