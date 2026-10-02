using PensionMS.Core.Interfaces;

namespace PensionMS.Core.DTOs.Settlements;

// ── Batch DTOs ──────────────────────────────

public class SettlementBatchListDto
{
    public int BatchID { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int TotalCount { get; set; }
    public int SourceRowsCount { get; set; }
    public int SkippedCount { get; set; }
    public string? FailureReportJson { get; set; }
    public bool HasFailureReport { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSettlementBatchDto
{
    public string? BatchNumber { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Description { get; set; }
}

// ── Item DTOs ───────────────────────────────

public class SettlementItemDto
{
    public int SettlementItemID { get; set; }
    public int SerialNumber { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? RetireeFullName { get; set; }
    public int? RetireeID { get; set; }
    public string? CurrentRankName { get; set; }
    public string? ProcessedRankName { get; set; }
    public decimal? ProcessedRankSalary { get; set; }
    public string? DecisionNumber { get; set; }
    public int? DecisionID { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LinkSettlementItemDto
{
    public int SettlementItemID { get; set; }
    public int RetireeID { get; set; }
}

public class FailureItem
{
    public int Row { get; set; }
    public string MilNum { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CurRank { get; set; } = string.Empty;
    public string ProcRank { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

// ── Decision DTOs ───────────────────────────

public class SettlementDecisionDto
{
    public int DecisionID { get; set; }
    public int BatchID { get; set; }
    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }
    public string? DecisionType { get; set; }
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
    public int SourceRowsCount { get; set; }
    public int SkippedCount { get; set; }
    public string? FailureReportJson { get; set; }
    public bool HasFailureReport { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSettlementDecisionDto
{
    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime? DecisionDate { get; set; }
    public string? DecisionType { get; set; }
    public string? Notes { get; set; }
    public List<int> ItemIDs { get; set; } = new();
}

// ── Import DTO ──────────────────────────────

public class ImportSettlementExcelDto
{
    public IRemoteFile File { get; set; } = null!;
    public IRemoteFile? DecisionFile { get; set; }
    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime? DecisionDate { get; set; }
    public string? DecisionType { get; set; } = "تسوية رتب";
    public string? Notes { get; set; }
}
