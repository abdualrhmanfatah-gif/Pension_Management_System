namespace PensionMS.Core.DTOs.Accounting;

// ═══════════════════════════════════════
// REQUEST DTOs
// ═══════════════════════════════════════

public class CreateBudgetItemDto
{
    public int FiscalYearID { get; set; }
    public int? QuarterID { get; set; }
    public int? PeriodID { get; set; }
    public int AccountID { get; set; }
    public decimal PlannedAmount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBudgetItemDto
{
    public decimal PlannedAmount { get; set; }
    public string? Notes { get; set; }
}

public class BudgetFilterDto
{
    public int FiscalYearID { get; set; }
    public int? QuarterID { get; set; }
    public int? AccountID { get; set; }
}

// ═══════════════════════════════════════
// RESPONSE DTOs
// ═══════════════════════════════════════

public class BudgetItemDto
{
    public int BudgetItemID { get; set; }
    public int FiscalYearID { get; set; }
    public string? FiscalYearName { get; set; }
    public int? QuarterID { get; set; }
    public string? QuarterName { get; set; }
    public int? PeriodID { get; set; }
    public string? PeriodName { get; set; }
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal PlannedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance => ActualAmount - PlannedAmount;
    public decimal VariancePercentage => PlannedAmount != 0
        ? Math.Round((Variance / PlannedAmount) * 100, 2) : 0;
    public string Status => Variance > 0 ? "تجاوز" : Variance < 0 ? "أقل" : "مطابق";
    public string? Notes { get; set; }
}

public class BudgetReportDto
{
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public decimal TotalPlanned { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalVariance => TotalActual - TotalPlanned;
    public decimal TotalVariancePercentage => TotalPlanned != 0
        ? Math.Round((TotalVariance / TotalPlanned) * 100, 2) : 0;
    public List<BudgetItemDto> Items { get; set; } = new();
}

public class QuarterlyBudgetReportDto
{
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public List<QuarterBudgetDto> Quarters { get; set; } = new();
}

public class QuarterBudgetDto
{
    public int QuarterID { get; set; }
    public string QuarterName { get; set; } = string.Empty;
    public decimal TotalPlanned { get; set; }
    public decimal TotalActual { get; set; }
    public decimal Variance => TotalActual - TotalPlanned;
    public List<BudgetItemDto> Items { get; set; } = new();
}

public class MonthlyBudgetReportDto
{
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public List<MonthBudgetDto> Months { get; set; } = new();
}

public class MonthBudgetDto
{
    public int PeriodID { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public decimal TotalPlanned { get; set; }
    public decimal TotalActual { get; set; }
    public decimal Variance => TotalActual - TotalPlanned;
    public List<BudgetItemDto> Items { get; set; } = new();
}
