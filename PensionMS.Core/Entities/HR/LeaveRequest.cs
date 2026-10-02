using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

public class LeaveRequest : ISoftDeletable
{
    public int LeaveRequestID { get; set; }

    public int EmployeeID { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public int LeaveTypeID { get; set; }
    public virtual LeaveType LeaveType { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RequestedDays { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    public int? ApprovedByUserID { get; set; }
    public virtual User? ApprovedByUser { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
