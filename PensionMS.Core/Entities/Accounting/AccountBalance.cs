using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Accounting;

/// <summary>
/// رصيد الحساب — أرصدة الافتتاح والحركة والإغلاق لكل حساب في كل فترة
/// </summary>
public class AccountBalance : ISoftDeletable
{
    [Key]
    public int BalanceID { get; set; }

    /// <summary>معرف الحساب من الدليل المحاسبي</summary>
    public int AccountID { get; set; }

    /// <summary>معرف الفترة المحاسبية</summary>
    public int PeriodID { get; set; }

    /// <summary>الرصيد الافتتاحي مدين</summary>
    public decimal OpeningDebit { get; set; }

    /// <summary>الرصيد الافتتاحي دائن</summary>
    public decimal OpeningCredit { get; set; }

    /// <summary>حركة الفترة مدينة</summary>
    public decimal PeriodDebit { get; set; }

    /// <summary>حركة الفترة دائنة</summary>
    public decimal PeriodCredit { get; set; }

    /// <summary>الرصيد الختامي مدين</summary>
    public decimal ClosingDebit { get; set; }

    /// <summary>الرصيد الختامي دائن</summary>
    public decimal ClosingCredit { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AccountID))]
    public virtual ChartOfAccount? Account { get; set; }

    [ForeignKey(nameof(PeriodID))]
    public virtual FiscalPeriod? Period { get; set; }
}
