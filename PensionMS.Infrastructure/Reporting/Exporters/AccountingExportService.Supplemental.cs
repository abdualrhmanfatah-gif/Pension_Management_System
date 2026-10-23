using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Infrastructure.Reporting.Exporters;

public partial class AccountingExportService
{
    public Task<byte[]> ExportQuarterlyAsync(ApiResponse<IncomeStatementDto> data, string format, string quarterName)
    {
        var report = RequireData(data);
        var rows = report.Revenues.Select(item => new object[] { item.AccountCode, item.AccountName, item.Amount, "إيرادات" })
            .Concat(report.Expenses.Select(item => new object[] { item.AccountCode, item.AccountName, item.Amount, "مصروفات" })).ToList();
        rows.Add(new object[] { "", "إجمالي الإيرادات", report.TotalRevenue, "" });
        rows.Add(new object[] { "", "إجمالي المصروفات", report.TotalExpenses, "" });
        rows.Add(new object[] { "", "صافي الدخل", report.NetIncome, "" });
        return Task.FromResult(ExportSupplemental("التقرير الفصلي", new[] { "كود الحساب", "الحساب", "المبلغ", "النوع" },
            rows, format, $"الفصل: {quarterName}", totalRows: 3));
    }

    public Task<byte[]> ExportBudgetAsync(ApiResponse<BudgetReportDto> data, string format)
    {
        var report = RequireData(data);
        var rows = report.Items.Select(item => new object[] { item.AccountCode, item.AccountName,
            item.PlannedAmount, item.ActualAmount, item.Variance, item.VariancePercentage, item.Status }).ToList();
        rows.Add(new object[] { "", "الإجمالي", report.TotalPlanned, report.TotalActual, report.TotalVariance,
            report.TotalVariancePercentage, "" });
        return Task.FromResult(ExportSupplemental("تقرير الموازنة السنوي",
            new[] { "كود الحساب", "اسم الحساب", "المخطط", "الفعلي", "الانحراف", "نسبة الانحراف %", "الحالة" },
            rows, format, $"السنة المالية: {report.YearName}"));
    }

    public Task<byte[]> ExportQuarterPeriodsAsync(ApiResponse<QuarterDetailsDto> data)
    {
        var report = RequireData(data);
        var rows = report.Periods.Select(period => new object[] { period.PeriodName, period.OpeningBalance,
            period.Withdrawn, period.Spent, period.ClosingBalance }).ToList();
        rows.Add(new object[] { "الإجمالي", "—", report.TotalWithdrawn, report.TotalSpent, report.RemainingBalance });
        return Task.FromResult(ExportSupplemental("تفصيل حسب الفترة",
            new[] { "الفترة", "الرصيد الافتتاحي", "سحب من البنك", "الصرف الفعلي", "الرصيد الختامي" }, rows,
            "excel", $"الفصل: {report.QuarterName}"));
    }

    public Task<byte[]> ExportQuarterExpensesAsync(ApiResponse<QuarterDetailsDto> data)
    {
        var report = RequireData(data);
        var rows = report.ExpenseBreakdown.Select(item => new object[] { item.AccountCode, item.AccountName,
            item.Amount, item.EntryCount }).ToList();
        rows.Add(new object[] { "", "الإجمالي", report.ExpenseBreakdown.Sum(item => item.Amount),
            report.ExpenseBreakdown.Sum(item => item.EntryCount) });
        return Task.FromResult(ExportSupplemental("تفصيل المصروفات حسب الحساب",
            new[] { "كود الحساب", "اسم الحساب", "المبلغ", "عدد القيود" }, rows, "excel", $"الفصل: {report.QuarterName}"));
    }

    private byte[] ExportSupplemental(string title, string[] headers, List<object[]> rows, string format, string details, int totalRows = 1) =>
        string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase)
            ? GeneratePdf($"{title} — {details}", headers, rows.ToArray())
            : _excelExporter.ExportRows(title, headers, rows,
                new ExcelReportOptions { IncludeFooter = false, Details = details, RecordCount = rows.Count - totalRows });

    private static T RequireData<T>(ApiResponse<T> response) where T : class =>
        response.IsSuccess && response.Data != null ? response.Data :
            throw new InvalidOperationException(response.Message ?? "تعذر إعداد بيانات التقرير");
}
