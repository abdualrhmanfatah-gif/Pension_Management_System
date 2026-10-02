using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

public class AdministrativeOrder : ISoftDeletable
{
    public int AdministrativeOrderID { get; set; }

    public int EmployeeID { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public int AdministrativeOrderTypeID { get; set; }
    public virtual AdministrativeOrderType AdministrativeOrderType { get; set; } = null!;

    public int? ApprovedByUserID { get; set; }
    public virtual User? ApprovedByUser { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
