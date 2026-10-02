namespace PensionMS.Core.Entities;

/// <summary>
/// التقارير المحفوظة
/// </summary>
public class SavedReport
{
    public int ReportID { get; set; }

    public int TemplateID { get; set; }
    public virtual ReportTemplate ReportTemplate { get; set; } = null!;

    public int GeneratedByUserID { get; set; }
    public virtual User GeneratedByUser { get; set; } = null!;

    public string ReportName { get; set; } = string.Empty;
    public string? Parameters { get; set; }
    public DateTime GeneratedAt { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
