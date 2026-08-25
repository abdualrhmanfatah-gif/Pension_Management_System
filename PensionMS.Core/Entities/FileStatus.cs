namespace PensionMS.Core.Entities;

/// <summary>
/// حالات الملفات
/// </summary>
public class FileStatus
{
    public int StatusID { get; set; }

    public string StatusName { get; set; } = string.Empty;
    public string? StatusColor { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<RetirementFile> RetirementFiles { get; set; } = new List<RetirementFile>();
}
