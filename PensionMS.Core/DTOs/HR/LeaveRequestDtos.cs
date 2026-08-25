namespace PensionMS.Core.DTOs.HR;

public class LeaveRequestListItemDto
{
    public int LeaveRequestID { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RequestedDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LeaveRequestStatsDto
{
    public int PendingCount { get; set; }
    public int ApprovedThisMonthCount { get; set; }
    public int TotalThisYearCount { get; set; }
}

public class LeaveRequestDetailsDto : LeaveRequestListItemDto
{
    public int EmployeeID { get; set; }
    public int LeaveTypeID { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class CreateLeaveRequestDto
{
    public int EmployeeID { get; set; }
    public int LeaveTypeID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class ProcessLeaveRequestDto
{
    public string Status { get; set; } = string.Empty; // Approved, Rejected
    public string? Notes { get; set; }
    public int ProcessedByUserID { get; set; }
}
