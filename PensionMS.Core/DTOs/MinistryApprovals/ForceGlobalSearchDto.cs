namespace PensionMS.Core.DTOs.MinistryApprovals;

public class ForceGlobalSearchDto
{
    public int ApprovalItemID { get; set; }
    public string MilitaryID { get; set; } = string.Empty;
    public string? NationalID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? RankName { get; set; }
    public string? UnitName { get; set; }
    public string? BatchName { get; set; }
    public byte? Gender { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public int? RetireeID { get; set; }
}
