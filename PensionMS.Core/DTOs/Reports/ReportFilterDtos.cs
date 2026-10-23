using System;

namespace PensionMS.Core.DTOs.Reports;

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
    public string? DecisionType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
