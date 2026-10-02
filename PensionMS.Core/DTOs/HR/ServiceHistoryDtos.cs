namespace PensionMS.Core.DTOs.HR;

public class ServiceHistoryEventDto
{
    public int ServiceHistoryEventID { get; set; }
    public int EmployeeID { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? SourceEntityType { get; set; }
    public int? SourceEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateServiceHistoryEventDto
{
    public int EmployeeID { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? SourceEntityType { get; set; }
    public int? SourceEntityId { get; set; }
}
