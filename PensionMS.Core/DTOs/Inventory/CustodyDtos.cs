namespace PensionMS.Core.DTOs.Inventory;

public class CustodyDto
{
    public int InventoryCustodyID { get; set; }
    public int AssetID { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetNumber { get; set; } = string.Empty;
    public int EmployeeID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public int Status { get; set; }
    public string? AssignedByName { get; set; }
    public string? ReturnedToName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCustodyDto
{
    public int AssetID { get; set; }
    public int EmployeeID { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public class UpdateCustodyDto
{
    public DateTime ReturnedDate { get; set; } = DateTime.UtcNow;
    public int Status { get; set; } = 1;
    public int? ReturnedToUserID { get; set; }
    public string? Notes { get; set; }
}
