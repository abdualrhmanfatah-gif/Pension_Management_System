namespace PensionMS.Core.DTOs.Compliance;

/// <summary>
/// لوحة التحكم لمسؤول الامتثال — ملخص المخاطر والتجاوزات
/// </summary>
public class ComplianceDashboardDto
{
    public int TotalOpenRiskEvents { get; set; }
    public int CriticalRiskEvents { get; set; }
    public int HighRiskEvents { get; set; }
    public int PendingOverrides { get; set; }
    public List<RiskEventSummaryDto> RecentRiskEvents { get; set; } = new();
    public List<OverrideReviewDto> PendingOverrideReviews { get; set; } = new();
    public List<BranchRiskDto> BranchRiskSummary { get; set; } = new();
}

public class RiskEventSummaryDto
{
    public long Id { get; set; }
    public int FileId { get; set; }
    public string RiskType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime RaisedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class OverrideReviewDto
{
    public long LogId { get; set; }
    public int FileId { get; set; }
    public int ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public string? Reason { get; set; }
}

public class BranchRiskDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int OpenRiskCount { get; set; }
    public int TotalPendingTasks { get; set; }
}
