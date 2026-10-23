namespace PensionMS.Core.DTOs.Financial;

public class CreateFinancialEntitlementDto
{
    public int RetireeID { get; set; }
    // حقول توافق مع المستدعين الحاليين؛ خدمة الإنشاء تستمد المبلغ والتاريخ من كشف الإحالة والتسويات.
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
}

public class FinancialEntitlementDto
{
    /// <summary>رتبة المصدر الذي اعتمدته عملية إنشاء الاستحقاق؛ لا يمثل عموداً في جدول الاستحقاقات.</summary>
    public int? RankID { get; set; }
    public int EntitlementID { get; set; }
    public int RetireeID { get; set; }
    public string? RetireeFullName { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive => EffectiveDate.Date <= PensionMS.Core.Time.YemenTime.TodayDateTime
        && (EndDate == null || EndDate.Value.Date >= PensionMS.Core.Time.YemenTime.TodayDateTime);
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? FileID { get; set; }
    public string? FileNumber { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? Phone { get; set; }
    public string? AgentName { get; set; }
}
