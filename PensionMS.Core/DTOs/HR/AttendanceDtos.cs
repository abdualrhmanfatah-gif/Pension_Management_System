namespace PensionMS.Core.DTOs.HR;

public class AttendanceEventDto
{
    public int AttendanceEventID { get; set; }
    public int EmployeeID { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty; // CheckIn, CheckOut, Absent, Medical, etc.
    public TimeSpan? EventTime { get; set; }
    public string? Location { get; set; }
    public string? DeviceInfo { get; set; }
    public string? Notes { get; set; }
    public bool IsManualEntry { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAttendanceEventDto
{
    public int EmployeeID { get; set; }
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = "CheckIn";
    public string? EventTime { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class AttendanceSummaryDto
{
    public int EmployeeID { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateArrivals { get; set; }
    public int EarlyDepartures { get; set; }
    public double TotalWorkHours { get; set; }
}

public class AttendanceDailyStatsDto
{
    public DateTime Date { get; set; }
    public int TotalEmployees { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExternalTaskCount { get; set; }
    public double PresencePercentage => TotalEmployees > 0 ? (double)PresentCount / TotalEmployees * 100 : 0;
}
