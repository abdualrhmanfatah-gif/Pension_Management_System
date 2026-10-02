namespace PensionMS.Core.DTOs;

/// <summary>
/// إحصائيات لوحة التحكم
/// </summary>
public class DashboardStatsDto
{
    /// <summary>إجمالي المتقاعدين</summary>
    public int TotalRetirees { get; set; }

    /// <summary>ملفات نشطة (جديد + قيد المراجعة + معلق)</summary>
    public int ActiveFilesCount { get; set; }

    /// <summary>ملفات مكتملة</summary>
    public int ClosedFilesCount { get; set; }

    /// <summary>ملفات مرفوضة</summary>
    public int RejectedFilesCount { get; set; }

    /// <summary>إجمالي المستحقين</summary>
    public int TotalEntitledPersons { get; set; }

    /// <summary>إجمالي مدفوعات الشهر الحالي</summary>
    public decimal TotalMonthlyPayments { get; set; }

    /// <summary>عدد المستخدمين النشطين</summary>
    public int ActiveUsersCount { get; set; }

    /// <summary>عدد الفروع النشطة</summary>
    public int ActiveBranchesCount { get; set; }

    // ── Salary Module ──

    /// <summary>كشوفات مسودة</summary>
    public int DraftBatchesCount { get; set; }

    /// <summary>كشوفات معتمدة</summary>
    public int ApprovedBatchesCount { get; set; }

    /// <summary>إجمالي المدفوعات هذا العام</summary>
    public decimal TotalPaidThisYear { get; set; }

    /// <summary>استحقاقات مالية سارية</summary>
    public int ActiveEntitlementsCount { get; set; }

    // ── Settlements Module ──

    /// <summary>قرارات تسوية جديدة (معلقة)</summary>
    public int PendingSettlementsCount { get; set; }

    /// <summary>قرارات تسوية معتمدة هذا الشهر</summary>
    public int ApprovedSettlementsCount { get; set; }

    // ── Operational Budget Module ──

    /// <summary>إجمالي الموازنة المعتمدة للعام</summary>
    public decimal TotalBudgetAmount { get; set; }

    /// <summary>إجمالي المنصرف من الموازنة للعام</summary>
    public decimal UsedBudgetAmount { get; set; }
}
