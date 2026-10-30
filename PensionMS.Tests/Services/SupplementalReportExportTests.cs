using OfficeOpenXml;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Infrastructure.Reporting.Exporters;

namespace PensionMS.Tests.Services;

public class SupplementalReportExportTests
{
    private readonly ExcelReportExporter _excel = new(new ExcelReportTemplate());
    private AccountingExportService Accounting => new(_excel, new PdfReportExporter());

    [Theory]
    [InlineData(null, 3, 60)]
    [InlineData(2, 2, 25)]
    public async Task TrialBalance_FiltersLevelsAndTotalsPostableAccountsOnly(int? level, int records, decimal total)
    {
        var items = new List<TrialBalanceItemDto>
        {
            new() { AccountCode = "001", AccountLevel = 1, TotalDebit = 60 },
            new() { AccountCode = "00101", AccountLevel = 2, IsPostable = true,
                OpeningDebit = 5, OpeningCredit = 1, MovementDebit = 20, MovementCredit = 2,
                ClosingDebit = 22, ClosingCredit = 0, TotalDebit = 25, TotalCredit = 3, Balance = 22, BalanceType = "Debit" },
            new() { AccountCode = "0010101", AccountLevel = 3, IsPostable = true, TotalDebit = 35 }
        };
        using var package = Open(await Accounting.ExportTrialBalanceAsync(ApiResponse<List<TrialBalanceItemDto>>.Success(items), "excel", level));
        var sheet = AssertOfficial(package, "ميزان المراجعة", records);
        Assert.Equal(12, sheet.Dimension.End.Column);
        Assert.Equal("001", sheet.Cells[9, 1].Text);
        var values = new decimal[] { 5, 1, 20, 2, 22, 0, 25, 3, 22 };
        for (var index = 0; index < values.Length; index++)
            Assert.Equal(values[index], Amount(sheet, 10, index + 3));
        Assert.Equal("مدين", sheet.Cells[10, 12].Text);
        Assert.Equal(total, Amount(sheet, records + 9, 9));
    }

    [Fact]
    public async Task Budget_PreservesAmountsVarianceStatusAndYear()
    {
        var report = new BudgetReportDto { YearName = "2026", TotalPlanned = 100, TotalActual = 125.5m,
            Items = new() { new() { AccountCode = "001", PlannedAmount = 100, ActualAmount = 125.5m } } };
        using var package = Open(await Accounting.ExportBudgetAsync(ApiResponse<BudgetReportDto>.Success(report), "excel"));
        var sheet = AssertOfficial(package, "تقرير الموازنة السنوي", 1);
        Assert.Equal("001", sheet.Cells[9, 1].Text);
        Assert.Equal(125.5m, Amount(sheet, 9, 4));
        Assert.Equal(25.5m, Amount(sheet, 9, 5));
        Assert.Equal(25.5m, Amount(sheet, 9, 6));
        Assert.Equal("تجاوز", sheet.Cells[9, 7].Text);
        Assert.Equal(125.5m, Amount(sheet, 10, 4));
        Assert.Contains("2026", sheet.Cells[4, 5].Text);
    }

    [Fact]
    public async Task QuarterPeriods_UsesQuarterClosingBalanceInsteadOfSummingPeriodBalances()
    {
        var report = new QuarterDetailsDto { QuarterName = "الفصل الأول", TotalWithdrawn = 100, TotalSpent = 60, RemainingBalance = 50,
            Periods = new() {
                new() { PeriodName = "يناير", OpeningBalance = 10, Withdrawn = 100, Spent = 20, ClosingBalance = 90 },
                new() { PeriodName = "فبراير", OpeningBalance = 90, Spent = 40, ClosingBalance = 50 } } };
        using var package = Open(await Accounting.ExportQuarterPeriodsAsync(ApiResponse<QuarterDetailsDto>.Success(report)));
        var sheet = AssertOfficial(package, "تفصيل حسب الفترة", 2);
        Assert.Equal(90, Amount(sheet, 10, 2));
        Assert.Equal(100, Amount(sheet, 11, 3));
        Assert.Equal(60, Amount(sheet, 11, 4));
        Assert.Equal(50, Amount(sheet, 11, 5));
        Assert.Equal("—", sheet.Cells[11, 2].Text);
    }

    [Fact]
    public async Task QuarterExpenses_PreservesAccountCodesAndTotalsCounts()
    {
        var report = new QuarterDetailsDto { ExpenseBreakdown = new() {
            new() { AccountCode = "001", Amount = 12.5m, EntryCount = 2 },
            new() { AccountCode = "002", Amount = 7.5m, EntryCount = 3 } } };
        using var package = Open(await Accounting.ExportQuarterExpensesAsync(ApiResponse<QuarterDetailsDto>.Success(report)));
        var sheet = AssertOfficial(package, "تفصيل المصروفات حسب الحساب", 2);
        Assert.Equal("001", sheet.Cells[9, 1].Text);
        Assert.Equal(12.5m, Amount(sheet, 9, 3));
        Assert.Equal(20m, Amount(sheet, 11, 3));
        Assert.Equal(5m, Amount(sheet, 11, 4));
    }

    [Fact]
    public async Task Quarterly_ContainsRevenueExpensesAndThreeSummaries()
    {
        var report = new IncomeStatementDto { TotalRevenue = 150, TotalExpenses = 100, NetIncome = 50,
            Revenues = new() { new() { AccountCode = "004", Amount = 150 } },
            Expenses = new() { new() { AccountCode = "003", Amount = 100 } } };
        using var package = Open(await Accounting.ExportQuarterlyAsync(ApiResponse<IncomeStatementDto>.Success(report), "excel", "الفصل الثاني"));
        var sheet = AssertOfficial(package, "التقرير الفصلي", 2);
        Assert.Equal("إيرادات", sheet.Cells[9, 4].Text);
        Assert.Equal("مصروفات", sheet.Cells[10, 4].Text);
        Assert.Equal(150m, Amount(sheet, 11, 3));
        Assert.Equal(100m, Amount(sheet, 12, 3));
        Assert.Equal(50m, Amount(sheet, 13, 3));
        Assert.Contains("الفصل الثاني", sheet.Cells[4, 3].Text);
    }

    [Fact]
    public async Task FailedQueries_DoNotProduceSuccessfulEmptyReports()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => Accounting.ExportBudgetAsync(ApiResponse<BudgetReportDto>.Fail("خطأ"), "excel"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Accounting.ExportQuarterPeriodsAsync(ApiResponse<QuarterDetailsDto>.Fail("خطأ")));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Accounting.ExportQuarterExpensesAsync(ApiResponse<QuarterDetailsDto>.Fail("خطأ")));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Accounting.ExportQuarterlyAsync(ApiResponse<IncomeStatementDto>.Fail("خطأ"), "excel", ""));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Accounting.ExportTrialBalanceAsync(ApiResponse<List<TrialBalanceItemDto>>.Fail("خطأ"), "excel"));
    }

    [Fact]
    public void UploaderStats_PreservesCountsFallbackNamesAndYemenDates()
    {
        var exporter = new UploaderStatsReportExporter(_excel);
        var at = new DateTime(2026, 10, 15, 22, 30, 0, DateTimeKind.Unspecified);
        var filter = new UploaderStatsFilterDto { FromDate = new(2026, 10, 16), ToDate = new(2026, 10, 16) };
        using var package = Open(exporter.ExportUploads(new[] {
            new UploaderStatsDto { UserName = "user", FullName = " ", TotalUploads = 12, UniqueFiles = 4, LastUploadDate = at },
            new UploaderStatsDto { FullName = "أحمد", TotalUploads = 3, UniqueFiles = 1 } }, filter));
        var sheet = AssertOfficial(package, "إحصائيات مدخلي الوثائق", 2);
        Assert.Equal("user", sheet.Cells[9, 2].Text);
        Assert.Equal("أحمد", sheet.Cells[10, 2].Text);
        Assert.Equal("2026/10/16 01:30", sheet.Cells[9, 5].Text);
        Assert.Equal("—", sheet.Cells[10, 5].Text);
        Assert.Equal(15m, Amount(sheet, 11, 3));
        Assert.Equal(5m, Amount(sheet, 11, 4));
        Assert.Equal("#,##0", sheet.Cells[9, 3].Style.Numberformat.Format);
        Assert.Contains("2026/10/16", sheet.Cells[4, 4].Text);
    }

    [Fact]
    public void DataEntryStats_ContainsAllStatusesAndHonoursPresetPeriodPrecedence()
    {
        var exporter = new UploaderStatsReportExporter(_excel);
        using var package = Open(exporter.ExportEntries(new[] {
            new DataEntryStatsDto { FullName = "أحمد", TotalFilesCreated = 9, CompletedFiles = 4, PendingFiles = 3, RejectedFiles = 2 } },
            new UploaderStatsFilterDto { Period = "today", FromDate = new(2000, 1, 1) }));
        var sheet = AssertOfficial(package, "إحصائيات مدخلي البيانات", 1);
        for (var col = 3; col <= 6; col++)
            Assert.Equal(Amount(sheet, 9, col), Amount(sheet, 10, col));
        Assert.Equal(9m, Amount(sheet, 9, 3));
        Assert.Equal(4m, Amount(sheet, 9, 4));
        Assert.Equal(3m, Amount(sheet, 9, 5));
        Assert.Equal(2m, Amount(sheet, 9, 6));
        Assert.Equal("الفترة: اليوم", sheet.Cells[4, 5].Text);
    }

    private static ExcelPackage Open(byte[] bytes) => new(new MemoryStream(bytes));
    private static decimal Amount(ExcelWorksheet sheet, int row, int col) => Convert.ToDecimal(sheet.Cells[row, col].Value);

    private static ExcelWorksheet AssertOfficial(ExcelPackage package, string title, int records)
    {
        var sheet = Assert.Single(package.Workbook.Worksheets);
        Assert.True(sheet.View.RightToLeft);
        Assert.Equal(title, package.Workbook.Properties.Title);
        Assert.Equal($"عدد السجلات: {records}", package.Workbook.Properties.Subject);
        Assert.Equal("بسم الله الرحمن الرحيم", sheet.Cells[1, 1].Text);
        Assert.Equal("Noto Kufi Arabic", sheet.Cells[9, 1].Style.Font.Name);
        Assert.Equal(8, sheet.PrinterSettings.RepeatRows.End.Row);
        Assert.Equal(1, sheet.PrinterSettings.FitToWidth);
        return sheet;
    }
}
