namespace PensionMS.Core.DTOs;

/// <summary>
/// آخر عمليات النظام
/// </summary>
public class RecentActivityDto
{
    public long LogID { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? EntityDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}
