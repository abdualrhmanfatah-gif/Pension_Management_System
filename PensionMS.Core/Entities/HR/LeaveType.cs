using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

public class LeaveType : ISoftDeletable
{
    public int LeaveTypeID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPaid { get; set; } = true;
    public bool CountsTowardsService { get; set; } = true;
    public int MaxDaysPerRequest { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
