namespace PensionMS.Core.Reporting;

public class ReportColumn
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public ReportColumnType Type { get; init; } = ReportColumnType.Text;
    public int? Width { get; init; }
    public ColumnAlignment Alignment { get; init; } = ColumnAlignment.Center;
    public string? Format { get; init; }
}

public enum ReportColumnType
{
    Text,
    Number,
    Currency,
    Date,
    Percent,
    Boolean
}

public enum ColumnAlignment
{
    Left,
    Center,
    Right
}
