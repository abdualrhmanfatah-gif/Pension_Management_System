namespace PensionMS.Core.DTOs.RetirementFiles;

public class StatusTransitionDto
{
    public long TransitionID { get; set; }
    public int RecordID { get; set; }
    public int? OldStatusID { get; set; }
    public string? OldStatusName { get; set; }
    public int NewStatusID { get; set; }
    public string NewStatusName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime TransitionDate { get; set; }
    public int? ChangedByUserID { get; set; }
    public string? ChangedByUserName { get; set; }
}
