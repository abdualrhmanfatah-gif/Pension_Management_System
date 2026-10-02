namespace PensionMS.Core.Entities;

/// <summary>
/// قوالب التقارير
/// </summary>
public class ReportTemplate
{
    public int TemplateID { get; set; }

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Template { get; set; } = string.Empty;
    public string? QueryTemplate { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SavedReport> SavedReports { get; set; } = new List<SavedReport>();
}
