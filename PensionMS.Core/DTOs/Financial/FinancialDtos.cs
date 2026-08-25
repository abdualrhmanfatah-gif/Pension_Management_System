namespace PensionMS.Core.DTOs.Financial;

public class CreateFinancialEntitlementDto
{
    public int RetireeID { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
}

public class FinancialEntitlementDto
{
    public int EntitlementID { get; set; }
    public int RetireeID { get; set; }
    public string? RetireeFullName { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive => EndDate == null || EndDate > DateTime.UtcNow;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? FileID { get; set; }
    public string? FileNumber { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? Phone { get; set; }
    public string? AgentName { get; set; }
}
