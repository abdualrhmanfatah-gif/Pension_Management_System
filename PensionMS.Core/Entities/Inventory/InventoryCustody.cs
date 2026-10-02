using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Entities.Assets;

namespace PensionMS.Core.Entities.Inventory;

public class InventoryCustody : ISoftDeletable
{
    public int InventoryCustodyID { get; set; }
    public int AssetID { get; set; }
    public virtual FixedAsset Asset { get; set; } = null!;
    public int EmployeeID { get; set; }
    public virtual Employee Employee { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public CustodyStatus Status { get; set; } = CustodyStatus.Active;
    public int? AssignedByUserID { get; set; }
    public virtual User? AssignedByUser { get; set; }
    public int? ReturnedToUserID { get; set; }
    public virtual User? ReturnedToUser { get; set; }
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
}
