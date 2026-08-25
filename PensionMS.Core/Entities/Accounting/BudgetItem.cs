using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// بند الموازنة — يمثل المبلغ المخطط لكل حساب في سنة مالية أو فصل مالي
/// </summary>
public class BudgetItem : ISoftDeletable
{
    [Key]
    public int BudgetItemID { get; set; }

    /// <summary>معرف السنة المالية</summary>
    public int FiscalYearID { get; set; }

    /// <summary>معرف الفصل (اختياري - للموازنة الفصلية)</summary>
    public int? QuarterID { get; set; }

    /// <summary>معرف الفترة الشهرية (اختياري - للموازنة الشهرية)</summary>
    public int? PeriodID { get; set; }

    /// <summary>معرف الحساب من الدليل المحاسبي</summary>
    public int AccountID { get; set; }

    /// <summary>المبلغ المخطط</summary>
    public decimal PlannedAmount { get; set; }

    /// <summary>ملاحظات</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FiscalYearID))]
    public virtual FiscalYear? FiscalYear { get; set; }

    [ForeignKey(nameof(QuarterID))]
    public virtual Quarter? Quarter { get; set; }

    [ForeignKey(nameof(PeriodID))]
    public virtual FiscalPeriod? Period { get; set; }

    [ForeignKey(nameof(AccountID))]
    public virtual ChartOfAccount? Account { get; set; }
}
