using System.Drawing;
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PensionMS.Core.Reporting;

namespace PensionMS.Infrastructure.Reporting.Exporters;

public class ExcelReportExporter
{
    private readonly ExcelReportTemplate _template;

    public ExcelReportExporter(ExcelReportTemplate template)
    {
        _template = template;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public byte[] Export(ReportDefinition report, object[] data)
    {
        var properties = data.Select(item => item.GetType().GetProperties()
            .ToDictionary(property => property.Name)).ToArray();
        var rows = data.Select((item, index) => report.Columns.Select(column =>
        {
            var value = properties[index].TryGetValue(column.Key, out var property) ? property.GetValue(item) : null;
            return FormatValue(value, column.Type);
        }).ToArray()).ToArray();
        return Render(report.NameAr, report.Columns, rows, new ExcelReportOptions { ReportId = report.Id });
    }

    public byte[] ExportRows(string name, string[] headers, IReadOnlyList<object?[]> rows, ExcelReportOptions? options = null)
    {
        var columns = headers.Select(label => new ReportColumn { Label = label }).ToArray();
        return Render(name, columns, rows, options ?? new ExcelReportOptions());
    }

    private byte[] Render(string name, ReportColumn[] columns, IReadOnlyList<object?[]> rows, ExcelReportOptions options)
    {
        if (columns.Length == 0 || options.HeaderRow < 1 || options.FooterGap < 0
            || options.RecordCount < 0 || options.RecordCount > rows.Count
            || rows.Any(row => row.Length > columns.Length))
            throw new ArgumentException("بنية أعمدة التقرير أو صفوفه غير صالحة");
        var footerSpan = options.FooterLabelSpan ?? columns.Length;
        if (footerSpan < 1 || footerSpan > columns.Length || options.FooterValues.Keys.Any(column => column <= footerSpan || column > columns.Length)
            || options.NumberFormats.Keys.Any(column => column < 1 || column > columns.Length))
            throw new ArgumentException("إعدادات أعمدة التقرير أو إجمالياته غير صالحة");

        using var package = new ExcelPackage();
        var sheetName = new string(name.Where(character => !"[]:*?/\\".Contains(character)).ToArray()).Trim().Trim('\'');
        if (string.IsNullOrWhiteSpace(sheetName)) sheetName = "تقرير";
        var ws = package.Workbook.Worksheets.Add(sheetName[..Math.Min(sheetName.Length, 31)]);
        ws.View.RightToLeft = true;
        var headerRow = options.HeaderRow;
        if (headerRow > 1)
        {
            ws.Cells[1, 1, 1, columns.Length].Merge = true;
            ws.Cells[1, 1].Value = options.Title ?? name;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Row(1).Height = 36;
        }

        for (var column = 0; column < columns.Length; column++)
            ws.Cells[headerRow, column + 1].Value = columns[column].Label;
        StyleHeader(ws, headerRow, columns.Length);

        for (var row = 0; row < rows.Count; row++)
        {
            // Noto Kufi Arabic needs more line height than the default Excel row.
            ws.Row(headerRow + row + 1).Height = 26;
            for (var column = 0; column < columns.Length; column++)
            {
                var cell = ws.Cells[headerRow + row + 1, column + 1];
                var value = column < rows[row].Length ? rows[row][column] : null;
                cell.Value = value switch
                {
                    DateOnly date => date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
                    bool flag => flag ? "نعم" : "لا",
                    _ => value
                };
                cell.Style.Font.Name = ExcelReportTemplate.FontName;
                cell.Style.Font.Size = 10;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(189, 195, 206));
                cell.Style.HorizontalAlignment = columns[column].Alignment switch
                {
                    ColumnAlignment.Left => ExcelHorizontalAlignment.Left,
                    ColumnAlignment.Right => ExcelHorizontalAlignment.Right,
                    _ => ExcelHorizontalAlignment.Center
                };
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                var format = options.NumberFormats.GetValueOrDefault(column + 1) ?? columns[column].Format ?? columns[column].Type switch
                {
                    ReportColumnType.Currency => "#,##0.00",
                    ReportColumnType.Percent => "0.0%",
                    _ => value switch
                    {
                        decimal => "#,##0.00",
                        DateTime => "yyyy/mm/dd",
                        _ => null
                    }
                };
                if (format != null) cell.Style.Numberformat.Format = format;
            }
        }

        if (options.IncludeFooter)
        {
            var footerRow = headerRow + rows.Count + 1 + options.FooterGap;
            ws.Row(footerRow).Height = 28;
            ws.Cells[footerRow, 1, footerRow, footerSpan].Merge = true;
            ws.Cells[footerRow, 1].Value = options.FooterText ?? $"إجمالي السجلات: {rows.Count}";
            var footer = ws.Cells[footerRow, 1, footerRow, columns.Length];
            footer.Style.Font.Bold = true;
            footer.Style.Font.Size = 11;
            footer.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            foreach (var total in options.FooterValues)
            {
                ws.Cells[footerRow, total.Key].Value = total.Value;
                ws.Cells[footerRow, total.Key].Style.Numberformat.Format = options.NumberFormats.GetValueOrDefault(total.Key) ?? "#,##0.00";
            }
        }

        for (var column = 1; column <= columns.Length; column++)
        {
            // Titles and totals must not inflate the width of the data columns.
            ws.Cells[headerRow, column, headerRow + rows.Count, column].AutoFitColumns(12);
        }
        _template.Apply(ws, options.RecordCount ?? rows.Count, options.Title ?? name, options.ReportId, headerRow, options.Details);
        return package.GetAsByteArray();
    }

    private static object? FormatValue(object? value, ReportColumnType type)
    {
        return type switch
        {
            ReportColumnType.Currency => value is decimal amount ? amount : 0m,
            ReportColumnType.Number => value is int or long ? value : 0,
            ReportColumnType.Percent => value is double percent ? percent : value is decimal decimalPercent ? (double)decimalPercent : 0d,
            ReportColumnType.Date => value switch
            {
                DateTime date => date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
                DateOnly date => date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
                _ => value?.ToString() ?? ""
            },
            ReportColumnType.Boolean => value is true ? "نعم" : "لا",
            _ => value?.ToString() ?? ""
        };
    }

    private static void StyleHeader(ExcelWorksheet ws, int row, int count)
    {
        var header = ws.Cells[row, 1, row, count];
        header.Style.Font.Name = ExcelReportTemplate.FontName;
        header.Style.Font.Bold = true;
        header.Style.Font.Size = 12;
        header.Style.Font.Color.SetColor(Color.White);
        header.Style.Fill.PatternType = ExcelFillStyle.Solid;
        header.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125));
        header.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        header.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Row(row).Height = 32;
    }
}
