namespace PensionMS.Core.Entities.HR;

public class ServiceHistoryEvent
{
    public int ServiceHistoryEventID { get; set; }

    public int EmployeeID { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? SourceEntityType { get; set; }
    public int? SourceEntityId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
