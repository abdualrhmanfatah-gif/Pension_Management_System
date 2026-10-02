namespace PensionMS.Core.DTOs.MinistryApprovals;

public class ReconciliationRowDto
{
    public int ApprovalItemID { get; set; }
    public string MilitaryId { get; set; } = string.Empty;
    public string MinistryName { get; set; } = string.Empty;
    public string? DbName { get; set; }
    public string StatusGap { get; set; } = string.Empty; // e.g., "Matched", "Missing in DB", "Name Mismatch"
    public bool IsActionRequired { get; set; }
}
