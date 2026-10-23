using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PensionMS.Infrastructure.Reporting.Exporters;

public class RetirementFileReportExporter
{
    private readonly ExcelReportExporter _excelExporter;

    public RetirementFileReportExporter(ExcelReportExporter excelExporter) => _excelExporter = excelExporter;

    public byte[] Export(IReadOnlyList<RetirementFileReportRow> items, string format)
    {
        if (format.ToLower() == ExportFormatConstants.Excel)
        {
            var headers = new[] { "م", "رقم الملف", "المتقاعد", "الرقم العسكري", "الرتبة", "الفرع", "الجنس", "الحالة الاجتماعية", "تاريخ التقاعد", "حالة الملفات", "مراجع", "مستندات ناقصة" };
            var rows = items.Select((it, i) => new object?[]
            {
                i + 1,
                it.FileNumber,
                it.RetireeName,
                it.MilitaryNumber,
                it.RankName,
                it.BranchName,
                it.Gender == 2 ? "أنثى" : "ذكر",
                it.SocialStatusName,
                it.RetirementDate?.ToString("yyyy/MM/dd"),
                it.StatusName,
                it.ReviewStatusName,
                it.MissingDocumentsCount,
            }).ToArray();
            return _excelExporter.ExportRows("تقرير ملفات التقاعد", headers, rows,
                new ExcelReportOptions { IncludeFooter = false });
        }
        else if (format.ToLower() == ExportFormatConstants.Pdf)
        {
            QuestPDF.Settings.DocumentLayoutExceptionThreshold = 1000;
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.ContentFromRightToLeft();
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("الجمهورية اليمنية").Bold();
                            col.Item().Text("وزارة الداخلية");
                            col.Item().Text("صندوق التقاعد الأمني");
                        });
                        row.ConstantItem(120).AlignCenter().Text("تقرير ملفات التقاعد").FontSize(14).Bold();
                        row.RelativeItem().AlignLeft().Column(col =>
                        {
                            col.Item().Text($"التاريخ: {PensionMS.Core.Time.YemenTime.Now:yyyy/MM/dd}");
                        });
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(25);
                            cols.ConstantColumn(60);
                            cols.RelativeColumn(3);
                            cols.ConstantColumn(60);
                            cols.ConstantColumn(60);
                            cols.ConstantColumn(50);
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(50);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("م");
                            h.Cell().Element(CellStyle).Text("رقم الملف");
                            h.Cell().Element(CellStyle).Text("المتقاعد");
                            h.Cell().Element(CellStyle).Text("الرقم العسكري");
                            h.Cell().Element(CellStyle).Text("الرتبة");
                            h.Cell().Element(CellStyle).Text("الجنس");
                            h.Cell().Element(CellStyle).Text("تاريخ التقاعد");
                            h.Cell().Element(CellStyle).Text("الحالة");

                            static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer c) => c.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).AlignCenter();
                        });

                        int rowNumber = 1;
                        foreach (var it in items)
                        {
                            table.Cell().Element(ContentStyle).Text(rowNumber.ToString());
                            table.Cell().Element(ContentStyle).Text(it.FileNumber ?? "");
                            table.Cell().Element(ContentStyle).Text(it.RetireeName ?? "");
                            table.Cell().Element(ContentStyle).Text(it.MilitaryNumber ?? "");
                            table.Cell().Element(ContentStyle).Text(it.RankName ?? "");
                            table.Cell().Element(ContentStyle).Text(it.Gender == 2 ? "أنثى" : "ذكر");
                            table.Cell().Element(ContentStyle).Text(it.RetirementDate?.ToString("yyyy/MM/dd") ?? "---");
                            table.Cell().Element(ContentStyle).Text(it.StatusName ?? "");

                            static QuestPDF.Infrastructure.IContainer ContentStyle(QuestPDF.Infrastructure.IContainer c) => c.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5).AlignCenter();
                            rowNumber++;
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

        return Array.Empty<byte>();
    }

}
