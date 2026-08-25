namespace PensionMS.Core.DTOs.Investment;

public class InvestmentStatsDto
{
    public int TotalPortfolios { get; set; }
    public decimal TotalAssetsValue { get; set; }
    public int ActiveLeaseContracts { get; set; }
    public int OpenMaintenanceRequests { get; set; }
    public int PendingApprovalsCount { get; set; }
}
