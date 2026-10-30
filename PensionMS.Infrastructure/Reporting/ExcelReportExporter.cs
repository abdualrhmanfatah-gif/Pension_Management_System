using OfficeOpenXml;
using OfficeOpenXml.Style;
using PensionMS.Core.Reporting;
using System.Drawing;

namespace PensionMS.Infrastructure.Reporting;

public class ExcelReportExporter
{
    public byte[] Export(ReportDefinition report, object[] data)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add(report.NameAr);
        ws.View.RightToLeft = true;

        var headers = report.Columns.Select(c => c.Label).ToArray();
        for (int i = 0; i < headers.Length; i++)
            ws.Cells[1, i + 1].Value = headers[i];

        StyleHeader(ws, 1, headers.Length);

        for (int row = 0; row < data.Length; row++)
        {
            var item = data[row];
            var props = item.GetType().GetProperties();
            for (int col = 0; col < report.Columns.Length; col++)
            {
                var column = report.Columns[col];
                var prop = props.FirstOrDefault(p => p.Name == column.Key);
                if (prop == null) continue;

                var value = prop.GetValue(item);
                var cell = ws.Cells[row + 2, col + 1];

                switch (column.Type)
                {
                    case ReportColumnType.Currency:
                        cell.Value = value is decimal d ? d : 0m;
                        cell.Style.Numberformat.Format = "#,##0.00";
                        break;
                    case ReportColumnType.Number:
                        cell.Value = value is int i ? i : value is long l ? l : 0;
                        break;
                    case ReportColumnType.Percent:
                        cell.Value = value is double dbl ? dbl : value is decimal dec ? (double)dec : 0;
                        cell.Style.Numberformat.Format = "0.0%";
                        break;
                    case ReportColumnType.Date:
                        cell.Value = value is DateTime dt ? dt.ToString("yyyy/MM/dd") : value?.ToString() ?? "";
                        break;
                    case ReportColumnType.Boolean:
                        cell.Value = value is bool b && b ? "نعم" : "لا";
                        break;
                    default:
                        cell.Value = value?.ToString() ?? "";
                        break;
                }

                cell.Style.Font.Size = 10;
                cell.Style.Font.Name = "Arial";
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(189, 195, 206));
                cell.Style.HorizontalAlignment = column.Alignment switch
                {
                    ColumnAlignment.Left => ExcelHorizontalAlignment.Left,
                    ColumnAlignment.Right => ExcelHorizontalAlignment.Right,
                    _ => ExcelHorizontalAlignment.Center
                };
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                if (row % 2 == 1)
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            }
        }

        AutoFitColumns(ws, headers.Length);
        AddFooter(ws, data.Length + 2, headers.Length, $"إجمالي السجلات: {data.Length}");

        return package.GetAsByteArray();
    }

    private static void StyleHeader(ExcelWorksheet ws, int row, int colCount)
    {
        var range = ws.Cells[row, 1, row, colCount];
        range.Style.Font.Bold = true;
        range.Style.Font.Size = 12;
        range.Style.Font.Color.SetColor(Color.White);
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125));
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Row(row).Height = 28;
    }

    private static void AutoFitColumns(ExcelWorksheet ws, int colCount)
    {
        for (int i = 1; i <= colCount; i++)
        {
            ws.Column(i).AutoFit();
            if (ws.Column(i).Width < 12)
                ws.Column(i).Width = 12;
        }
    }

    private static void AddFooter(ExcelWorksheet ws, int row, int colCount, string text)
    {
        ws.Cells[row, 1, row, colCount].Merge = true;
        ws.Cells[row, 1].Value = text;
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 11;
        ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
    }
}
