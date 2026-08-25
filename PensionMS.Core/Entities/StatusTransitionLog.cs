namespace PensionMS.Core.Entities;

/// <summary>
/// سجل تغيير الحالات
/// </summary>
public class StatusTransitionLog
{
    public long TransitionID { get; set; }

    public int TableID { get; set; }
    public virtual SystemTable SystemTable { get; set; } = null!;

    public int? ChangedByUserID { get; set; }
    public virtual User? ChangedByUser { get; set; }

    public int RecordID { get; set; }
    public int? OldStatusID { get; set; }
    public int NewStatusID { get; set; }
    public string? Reason { get; set; }
    public DateTime TransitionDate { get; set; }
}
