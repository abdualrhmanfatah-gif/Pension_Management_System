using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// الاستحقاقات المالية
/// </summary>
public class FinancialEntitlement : ISoftDeletable
{
    public int EntitlementID { get; set; }

    public int RetireeID { get; set; }
    public virtual Retiree Retiree { get; set; } = null!;

    public int? BatchID { get; set; }
    public virtual SettlementsBatch? Batch { get; set; }

    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
