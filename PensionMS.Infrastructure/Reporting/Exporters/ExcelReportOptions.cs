namespace PensionMS.Infrastructure.Reporting.Exporters;

/// <summary>Report-specific content and layout; all workbook rendering stays in ExcelReportExporter.</summary>
public sealed class ExcelReportOptions
{
    public string? Title { get; init; }
    public string? ReportId { get; init; }
    public string? Details { get; init; }
    public int? RecordCount { get; init; }
    public int HeaderRow { get; init; } = 1;
    public bool IncludeFooter { get; init; } = true;
    public string? FooterText { get; init; }
    public int FooterGap { get; init; }
    public int? FooterLabelSpan { get; init; }
    // Column numbers are one-based, matching the visible Excel columns.
    public IReadOnlyDictionary<int, decimal> FooterValues { get; init; } = new Dictionary<int, decimal>();
    public IReadOnlyDictionary<int, string> NumberFormats { get; init; } = new Dictionary<int, string>();
}
