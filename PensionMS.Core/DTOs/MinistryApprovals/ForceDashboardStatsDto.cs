namespace PensionMS.Core.DTOs.MinistryApprovals;

public class ForceDashboardStatsDto
{
    public int TotalBatches { get; set; }
    public int ProcessedRecords { get; set; }
    public int PendingRecords { get; set; }
    public int ReconciliationGaps { get; set; }
}
