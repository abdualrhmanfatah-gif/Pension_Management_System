namespace PensionMS.Core.Reporting;

public class ReportDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Permission { get; init; } = string.Empty;
    public bool SupportsExcel { get; init; } = true;
    public bool SupportsPdf { get; init; } = true;
    public ReportColumn[] Columns { get; init; } = Array.Empty<ReportColumn>();
    public Func<ReportFilter, Task<ReportDataResult>>? DataProvider { get; init; }
}

public class ReportDataResult
{
    public object[] Items { get; init; } = Array.Empty<object>();
    public int TotalCount { get; init; }
}

public class ReportFilter
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? PortfolioId { get; set; }
    public int? ProjectId { get; set; }
    public int? PropertyId { get; set; }
    public int? BranchId { get; set; }
    public int? StatusId { get; set; }
    public int? FiscalYear { get; set; }
    public string? Quarter { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
