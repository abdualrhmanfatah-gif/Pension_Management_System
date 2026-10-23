using PensionMS.Core.Interfaces.Reporting;
using Color = System.Drawing.Color;
using System.Drawing;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PensionMS.Infrastructure.Reporting.Exporters;

public partial class AccountingExportService : IAccountingExportService
{
    private readonly ExcelReportExporter _excelExporter;

    public AccountingExportService(ExcelReportExporter excelExporter) => _excelExporter = excelExporter;

    public Task<byte[]> ExportTrialBalanceAsync(ApiResponse<List<TrialBalanceItemDto>> data, string format, int? accountLevel = null)
    {
        var items = RequireData(data).Where(item => !accountLevel.HasValue || item.AccountLevel <= accountLevel).ToList();
        var rows = items.Select(i => new object[] { i.AccountCode, i.AccountName, i.OpeningDebit, i.OpeningCredit,
            i.MovementDebit, i.MovementCredit, i.ClosingDebit, i.ClosingCredit, i.TotalDebit, i.TotalCredit,
            i.Balance, i.BalanceType == "Credit" ? "دائن" : "مدين" }).ToList();
        var postable = items.Where(item => item.IsPostable).ToList();
        rows.Add(new object[] { "", "الإجمالي", postable.Sum(i => i.OpeningDebit), postable.Sum(i => i.OpeningCredit),
            postable.Sum(i => i.MovementDebit), postable.Sum(i => i.MovementCredit), postable.Sum(i => i.ClosingDebit),
            postable.Sum(i => i.ClosingCredit), postable.Sum(i => i.TotalDebit), postable.Sum(i => i.TotalCredit),
            postable.Sum(i => i.Balance), "" });
        var headers = new[] { "رقم الحساب", "اسم الحساب", "افتتاحي مدين", "افتتاحي دائن", "حركة مدين", "حركة دائن",
            "ختامي مدين", "ختامي دائن", "إجمالي المدين", "إجمالي الدائن", "الرصيد", "نوع الرصيد" };
        return Task.FromResult(ExportSupplemental("ميزان المراجعة", headers, rows, format,
            accountLevel.HasValue ? $"حتى المستوى: {accountLevel}" : "جميع المستويات"));
    }

    public async Task<byte[]> ExportBalanceSheetAsync(ApiResponse<BalanceSheetDto> data, string format)
    {
        var bs = data.Data;
        var rows = new List<object[]>();
        FlattenBalanceSheet(bs?.Assets ?? new(), "أصول", rows);
        FlattenBalanceSheet(bs?.Liabilities ?? new(), "التزامات", rows);
        FlattenBalanceSheet(bs?.Equity ?? new(), "حقوق ملكية", rows);
        rows.Add(new object[] { "", "إجمالي الأصول", bs?.TotalAssets ?? 0, "أصول" });
        rows.Add(new object[] { "", "إجمالي الالتزامات", bs?.TotalLiabilities ?? 0, "التزامات" });
        rows.Add(new object[] { "", "إجمالي حقوق الملكية", bs?.TotalEquity ?? 0, "حقوق ملكية" });

        var headers = new[] { "رقم الحساب", "اسم الحساب", "المبلغ", "النوع" };
        return format.ToLower() == "pdf"
            ? GeneratePdf("الميزانية العمومية", headers, rows.ToArray())
            : GenerateExcel("الميزانية العمومية", headers, rows.ToArray());
    }

    public async Task<byte[]> ExportIncomeStatementAsync(ApiResponse<IncomeStatementDto> data, string format)
    {
        var inc = data.Data;
        var rows = new List<object[]>();
        foreach (var r in inc?.Revenues ?? new())
            rows.Add(new object[] { r.AccountCode, r.AccountName, r.Amount, "إيرادات" });
        foreach (var e in inc?.Expenses ?? new())
            rows.Add(new object[] { e.AccountCode, e.AccountName, e.Amount, "مصروفات" });
        rows.Add(new object[] { "", "إجمالي الإيرادات", inc?.TotalRevenue ?? 0, "" });
        rows.Add(new object[] { "", "إجمالي المصروفات", inc?.TotalExpenses ?? 0, "" });
        rows.Add(new object[] { "", "صافي الربح/الخسارة", inc?.NetIncome ?? 0, "" });

        var headers = new[] { "رقم الحساب", "اسم الحساب", "المبلغ", "النوع" };
        return format.ToLower() == "pdf"
            ? GeneratePdf("قائمة الدخل", headers, rows.ToArray())
            : GenerateExcel("قائمة الدخل", headers, rows.ToArray());
    }

    public async Task<byte[]> ExportGeneralLedgerAsync(ApiResponse<GeneralLedgerDto> data, string format)
    {
        var gl = data.Data;
        var rows = (gl?.Entries ?? new List<GeneralLedgerLineDto>())
            .Select(e => new object[] { e.EntryDate.ToString("yyyy-MM-dd"), e.EntryNumber, e.Description, e.Debit, e.Credit, e.RunningBalance })
            .ToArray();
        var headers = new[] { "التاريخ", "رقم القيد", "البيان", "مدين", "دائن", "الرصيد" };

        return format.ToLower() == "pdf"
            ? GeneratePdf($"الأستاذ العام — {gl?.AccountName ?? ""}", headers, rows)
            : GenerateExcel($"الأستاذ العام — {gl?.AccountName ?? ""}", headers, rows);
    }

    public async Task<byte[]> ExportAccountStatementAsync(ApiResponse<AccountStatementResultDto> data, string format)
    {
        var stmt = data.Data;
        var rows = new List<object[]>();
        rows.Add(new object[] { "", "الرصيد الافتتاحي", stmt?.OpeningBalance ?? 0, "", "" });
        foreach (var item in stmt?.Items ?? new List<AccountStatementItemDto>())
            rows.Add(new object[] { item.EntryDate.ToString("yyyy-MM-dd"), item.EntryNumber, item.Description, item.Debit, item.Credit });
        rows.Add(new object[] { "", "الرصيد الختامي", stmt?.ClosingBalance ?? 0, "", "" });

        var headers = new[] { "التاريخ", "رقم القيد", "البيان", "مدين", "دائن" };
        return format.ToLower() == "pdf"
            ? GeneratePdf($"كشف حساب — {stmt?.AccountName ?? ""}", headers, rows.ToArray())
            : GenerateExcel($"كشف حساب — {stmt?.AccountName ?? ""}", headers, rows.ToArray());
    }

    public async Task<byte[]> ExportSubledgerAsync(ApiResponse<SubledgerReportResultDto> data, string format)
    {
        var sub = data.Data;
        var rows = (sub?.Entries ?? new List<SubledgerReportLineDto>())
            .Select(e => new object[] { e.EntryDate.ToString("yyyy-MM-dd"), e.EntryNumber, e.Description ?? "", e.Debit, e.Credit, e.RunningBalance })
            .ToArray();
        var headers = new[] { "التاريخ", "رقم القيد", "البيان", "مدين", "دائن", "الرصيد" };

        return format.ToLower() == "pdf"
            ? GeneratePdf($"دفتر الأستاذ الفرعي — {sub?.AccountName ?? ""}", headers, rows)
            : GenerateExcel($"دفتر الأستاذ الفرعي — {sub?.AccountName ?? ""}", headers, rows);
    }

    public async Task<byte[]> ExportJournalRegisterAsync(ApiResponse<JournalRegisterDto> data, string format)
    {
        var jr = data.Data;
        var rows = (jr?.Entries ?? new List<JournalRegisterItemDto>())
            .Select(e => new object[] { e.EntryNumber, e.EntryDate.ToString("yyyy-MM-dd"), e.Description, e.JournalEntryTypeName ?? "", e.TotalDebit, e.TotalCredit, e.Status })
            .ToArray();
        var headers = new[] { "رقم القيد", "التاريخ", "البيان", "النوع", "مدين", "دائن", "الحالة" };

        return format.ToLower() == "pdf"
            ? GeneratePdf("سجل القيود اليومية", headers, rows)
            : GenerateExcel("سجل القيود اليومية", headers, rows);
    }

    public async Task<byte[]> ExportCashFlowAsync(ApiResponse<CashFlowStatementDto> data, string format)
    {
        var cf = data.Data;
        var rows = new List<object[]>();
        foreach (var item in cf?.OperatingItems ?? new())
            rows.Add(new object[] { item.AccountCode, item.AccountName, item.Amount, item.Type == "Inflow" ? "وارد" : "صادر", "الأنشطة التشغيلية" });
        foreach (var item in cf?.InvestingItems ?? new())
            rows.Add(new object[] { item.AccountCode, item.AccountName, item.Amount, item.Type == "Inflow" ? "وارد" : "صادر", "الأنشطة الاستثمارية" });
        foreach (var item in cf?.FinancingItems ?? new())
            rows.Add(new object[] { item.AccountCode, item.AccountName, item.Amount, item.Type == "Inflow" ? "وارد" : "صادر", "الأنشطة التمويلية" });
        rows.Add(new object[] { "", "صافي التدفق التشغيلي", cf?.NetOperatingCashFlow ?? 0, "", "" });
        rows.Add(new object[] { "", "صافي التدفق الاستثماري", cf?.NetInvestingCashFlow ?? 0, "", "" });
        rows.Add(new object[] { "", "صافي التدفق التمويلي", cf?.NetFinancingCashFlow ?? 0, "", "" });
        rows.Add(new object[] { "", "صافي التدفق النقدي", cf?.NetCashFlow ?? 0, "", "" });

        var headers = new[] { "رقم الحساب", "اسم الحساب", "المبلغ", "النوع", "النشاط" };
        return format.ToLower() == "pdf"
            ? GeneratePdf("قائمة التدفقات النقدية", headers, rows.ToArray())
            : GenerateExcel("قائمة التدفقات النقدية", headers, rows.ToArray());
    }

    // ═══════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════
    private static void FlattenBalanceSheet(List<BalanceSheetSectionDto> sections, string type, List<object[]> rows)
    {
        foreach (var s in sections)
        {
            if (s.IsPostable || !s.Children.Any())
                rows.Add(new object[] { s.AccountCode, s.AccountName, s.Balance, type });
            if (s.Children.Any())
                FlattenBalanceSheet(s.Children, type, rows);
        }
    }

    // ═══════════════════════════════════════
    // Excel Generator
    // ═══════════════════════════════════════
    private byte[] GenerateExcel(string title, string[] headers, object[][] data) =>
        _excelExporter.ExportRows(title, headers, data, new ExcelReportOptions { IncludeFooter = false });

    // ═══════════════════════════════════════
    // PDF Generator
    // ═══════════════════════════════════════
    private static byte[] GeneratePdf(string title, string[] headers, object[][] data)
    {
        QuestPDF.Settings.DocumentLayoutExceptionThreshold = 1000;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("الجمهورية اليمنية").Bold();
                        col.Item().Text("صندوق التقاعد الأمني");
                    });
                    row.ConstantItem(150).AlignCenter().Text(title).FontSize(14).Bold();
                    row.RelativeItem().AlignLeft().Text($"التاريخ: {PensionMS.Core.Time.YemenTime.Now:yyyy/MM/dd}");
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        for (int i = 0; i < headers.Length; i++)
                            cols.RelativeColumn(1);
                    });

                    table.Header(h =>
                    {
                        foreach (var header in headers)
                            h.Cell().Element(c => c.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).AlignCenter())
                                .Text(header);
                    });

                    foreach (var row in data)
                    {
                        foreach (var cell in row)
                        {
                            var text = cell switch
                            {
                                decimal d => d.ToString("N2"),
                                double dbl => dbl.ToString("N2"),
                                DateTime dt => dt.ToString("yyyy/MM/dd"),
                                null => "",
                                _ => cell.ToString()
                            };
                            table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignCenter())
                                .Text(text);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("صفحة ");
                    x.CurrentPageNumber();
                    x.Span(" من ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
