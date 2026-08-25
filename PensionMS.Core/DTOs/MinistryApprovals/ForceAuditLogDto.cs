using System;

namespace PensionMS.Core.DTOs.MinistryApprovals;

public class ForceAuditLogDto
{
    public DateTime ActionDate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Notes { get; set; }
}
