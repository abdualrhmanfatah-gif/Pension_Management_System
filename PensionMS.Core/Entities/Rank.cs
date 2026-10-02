namespace PensionMS.Core.Entities;

/// <summary>
/// الرتب العسكرية
/// </summary>
public class Rank
{
    public int RankID { get; set; }

    public string RankName { get; set; } = string.Empty;
    public int RankLevel { get; set; }
    public decimal BaseSalary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<RetirementFile> RetirementFiles { get; set; } = new List<RetirementFile>();
    public virtual ICollection<MinistryApprovalItem> MinistryApprovalItems { get; set; } = new List<MinistryApprovalItem>();
    public virtual ICollection<SettlementItem> CurrentRankSettlements { get; set; } = new List<SettlementItem>();
    public virtual ICollection<SettlementItem> ProcessedRankSettlements { get; set; } = new List<SettlementItem>();
}
