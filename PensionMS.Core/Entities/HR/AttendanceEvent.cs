using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

public class AttendanceEvent
{
    public int AttendanceEventID { get; set; }

    public int EmployeeID { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty; // e.g., CheckIn, CheckOut, Late, Absent
    public TimeSpan? EventTime { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? DeviceInfo { get; set; }
    public bool IsManualEntry { get; set; }

    public int? RecordedByUserID { get; set; }
    public virtual User? RecordedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
