namespace PensionMS.Core.DTOs.Financial;

public class CreateFinancialEntitlementDto
{
    public int RetireeID { get; set; }
<<<<<<< HEAD
    // حقول توافق مع المستدعين الحاليين؛ خدمة الإنشاء تستمد المبلغ والتاريخ من كشف الإحالة والتسويات.
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
}

public class FinancialEntitlementDto
{
<<<<<<< HEAD
    /// <summary>رتبة المصدر الذي اعتمدته عملية إنشاء الاستحقاق؛ لا يمثل عموداً في جدول الاستحقاقات.</summary>
    public int? RankID { get; set; }
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public int EntitlementID { get; set; }
    public int RetireeID { get; set; }
    public string? RetireeFullName { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
<<<<<<< HEAD
    public bool IsActive => EffectiveDate.Date <= PensionMS.Core.Time.YemenTime.TodayDateTime
        && (EndDate == null || EndDate.Value.Date >= PensionMS.Core.Time.YemenTime.TodayDateTime);
=======
    public bool IsActive => EndDate == null || EndDate > DateTime.UtcNow;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? FileID { get; set; }
    public string? FileNumber { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? Phone { get; set; }
    public string? AgentName { get; set; }
}
