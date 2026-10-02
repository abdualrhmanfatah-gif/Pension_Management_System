namespace PensionMS.Core.DTOs;

public class PensionStatisticsDto
{
    public int TotalRetirees { get; set; }
    public int ActiveFiles { get; set; }
    public int SuspendedFiles { get; set; }
    public int ClosedFiles { get; set; }
    public int NewFiles { get; set; }
    public int UnderReviewFiles { get; set; }
    public int ActiveEntitled { get; set; }
    public int ActiveAgents { get; set; }
    public int ActiveGuardians { get; set; }
    public decimal TotalMonthlyAmount { get; set; }
    public List<RankDistributionDto> RankDistribution { get; set; } = new();
    public List<FileStatusDistributionDto> FileStatusDistribution { get; set; } = new();
    public List<GovernorateDistributionDto> GovernorateDistribution { get; set; } = new();
    public List<CategoryDistributionDto> CategoryDistribution { get; set; } = new();
    public List<BranchDistributionDto> BranchDistribution { get; set; } = new();
    public List<SettlementByGovernorateDto> SettlementByGovernorate { get; set; } = new();
}

public class RankDistributionDto
{
    public string RankName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class FileStatusDistributionDto
{
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class GovernorateDistributionDto
{
    public string Governorate { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class CategoryDistributionDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class BranchDistributionDto
{
    public string BranchName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class SettlementByGovernorateDto
{
    public string Governorate { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public double Percentage { get; set; }
}
