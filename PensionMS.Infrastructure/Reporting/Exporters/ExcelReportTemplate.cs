using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using PensionMS.Core.Time;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PensionMS.Infrastructure.Reporting.Exporters;

/// <summary>Shared print layout applied by ExcelReportExporter to every generated report.</summary>
public class ExcelReportTemplate
{
    // Matches --font-sans in ui/src/index.css.
    internal const string FontName = "Noto Kufi Arabic";
    private const float PointsPerInch = 72;
    private const float LandscapePageWidth = 297 / 25.4f * PointsPerInch;
    private const float PortraitPageWidth = 210 / 25.4f * PointsPerInch;
    private const float SideMargin = 0.3f * PointsPerInch;
    private const float HeaderMargin = 0.2f * PointsPerInch;
    private const float TopMargin = 0.3f * PointsPerInch;
    private const float BottomMargin = 0.4f * PointsPerInch;
    private const float WatermarkWidth = 80 / 25.4f * PointsPerInch;
    private readonly string[] _imageDirectories;

    public ExcelReportTemplate(IHostEnvironment? environment = null)
    {
        _imageDirectories = new[]
        {
            Path.Combine(environment?.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot"),
            Path.Combine(AppContext.BaseDirectory, "wwwroot"),
            AppContext.BaseDirectory
        }.Distinct().ToArray();
    }

    public void Apply(ExcelWorksheet ws, int recordCount, string? reportTitle = null, string? reportId = null, int headerRow = 1, string? details = null)
    {
        ws.Cells[ws.Dimension.Address].Style.Font.Name = FontName;
        var title = reportTitle ?? ws.Name;
        var dateText = YemenTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        var printer = ws.PrinterSettings;
        var portraitPrintableWidth = PortraitPageWidth - 2 * SideMargin;
        var reportWidth = EstimateReportWidth(ws);
        var isLandscape = reportWidth > portraitPrintableWidth;
        var pageWidth = isLandscape ? LandscapePageWidth : PortraitPageWidth;
        var pageHeight = isLandscape ? PortraitPageWidth : LandscapePageWidth;
        // FitToWidth shrinks a wide report, but Excel does not enlarge a narrow one.
        // Fill the selected page width so the printed table and letterhead align.
        ExpandColumnsToPageWidth(ws, reportWidth, pageWidth - 2 * SideMargin);
        // Existing report titles and formulas move with their rows. Reports with
        // a title already above the table only need the five letterhead rows.
        var insertedRows = headerRow > 1 ? 5 : 7;
        ws.InsertRow(1, insertedRows);
        AddEditableLetterhead(ws, title, dateText, recordCount, reportId, headerRow == 1, details);
        headerRow += insertedRows;
        printer.PaperSize = ePaperSize.A4;
        printer.Orientation = isLandscape ? eOrientation.Landscape : eOrientation.Portrait;
        printer.FitToPage = true;
        printer.FitToWidth = 1;
        printer.FitToHeight = 0; // Preserve vertical pagination for long reports.
        printer.LeftMargin = printer.RightMargin = (decimal)(SideMargin / PointsPerInch);
        printer.TopMargin = (decimal)(TopMargin / PointsPerInch);
        printer.BottomMargin = (decimal)(BottomMargin / PointsPerInch);
        printer.HeaderMargin = (decimal)(HeaderMargin / PointsPerInch);
        printer.FooterMargin = 0.15m;
        printer.PrintArea = ws.Cells[ws.Dimension.Address];
        printer.RepeatRows = ws.Cells[$"1:{headerRow}"];
        // Open as a continuous, editable table; page layout can show columns on
        // adjacent sheets even when printing is fitted to one page in width.
        ws.View.PageLayoutView = false;

        // OddHeader applies to every page when first/even overrides are disabled.
        var headerFooter = ws.HeaderFooter;
        headerFooter.differentFirst = false;
        headerFooter.differentOddEven = false;
        headerFooter.ScaleWithDocument = false;
        headerFooter.AlignWithMargins = true;
        ws.Workbook.Properties.Title = title;
        ws.Workbook.Properties.Subject = $"عدد السجلات: {recordCount}";
        ws.Workbook.Properties.Comments = reportId ?? "";
        headerFooter.OddFooter.CenteredText =
            $"&\"{FontName},Regular\"&8صفحة {ExcelHeaderFooter.PageNumber} من {ExcelHeaderFooter.NumberOfPages}";

        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;

        AddHeaderImages(ws);

        // A transparent header picture can extend over the table and repeats in print.
        // Worksheet backgrounds do not print. Header drawings are anchored wholly
        // inside the repeated title rows so Excel repeats them with the letterhead.
        var printScale = Math.Min(1, (pageWidth - 2 * SideMargin) / EstimateReportWidth(ws));
        var repeatedHeight = Enumerable.Range(1, headerRow)
            .Where(row => !ws.Row(row).Hidden).Sum(row => ws.Row(row).Height);
        using var pictureStream = new MemoryStream(CreateWatermark(pageWidth, pageHeight, (float)(repeatedHeight * printScale)));
        var picture = headerFooter.OddHeader.InsertPicture(pictureStream, ePictureType.Png, PictureAlignment.Centered);
        picture.Width = pageWidth - 2 * SideMargin;
        picture.Height = pageHeight - HeaderMargin - BottomMargin;
        picture.Title = "العلامة المائية للصندوق بعتامة 12%";
    }

    private static void AddEditableLetterhead(ExcelWorksheet ws, string title, string dateText,
        int recordCount, string? reportId, bool addTitle, string? details)
    {
        var columns = ws.Dimension.End.Column;
        ws.View.RightToLeft = true;
        WriteHeaderCell(ws, 1, 1, columns, "بسم الله الرحمن الرحيم", 13);
        ws.Row(1).Height = 32;
        var agency = new[] { "الجمهورية اليمنية", "وزارة الداخلية", "الإدارة العامة للصندوق التقاعد الأمني" };
        var metadata = new[] { $"التاريخ: {dateText}", $"عدد السجلات: {recordCount}",
            string.Join("\n", new[] { string.IsNullOrEmpty(reportId) ? null : $"الرقم: {reportId}", details }
                .Where(value => !string.IsNullOrWhiteSpace(value))) };
        var rightEnd = Math.Max(1, columns / 3);
        var centerEnd = Math.Max(rightEnd + 1, columns * 2 / 3);
        for (var index = 0; index < agency.Length; index++)
        {
            var row = index + 2;
            WriteHeaderCell(ws, row, 1, columns >= 3 ? rightEnd : columns, agency[index], 11,
                columns >= 3 ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Right);
            if (columns >= 3)
                WriteHeaderCell(ws, row, centerEnd + 1, columns, metadata[index], 10);
            ws.Row(row).Height = index == 2 ? 52 : 28;
        }
        if (columns >= 3)
            ws.Cells[2, rightEnd + 1, 4, centerEnd].Merge = true;
        else
            WriteHeaderCell(ws, 5, 1, columns, string.Join("   |   ", metadata.Where(value => value.Length > 0)), 10);
        ws.Row(5).Height = columns >= 3 ? 6 : 32;
        if (addTitle)
        {
            WriteHeaderCell(ws, 6, 1, columns, title, 14);
            ws.Row(6).Height = 36;
            ws.Row(7).Height = 6;
        }
    }

    private static void WriteHeaderCell(ExcelWorksheet ws, int row, int firstColumn, int lastColumn,
        string text, float fontSize, ExcelHorizontalAlignment alignment = ExcelHorizontalAlignment.Center)
    {
        var range = ws.Cells[row, firstColumn, row, lastColumn];
        if (lastColumn > firstColumn) range.Merge = true;
        ws.Cells[row, firstColumn].Value = text;
        range.Style.Font.Name = FontName;
        range.Style.Font.Size = fontSize;
        range.Style.Font.Bold = true;
        range.Style.HorizontalAlignment = alignment;
        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        range.Style.WrapText = true;
        range.Style.Locked = false;
    }

    [SupportedOSPlatform("windows6.1")]
    private void AddHeaderImages(ExcelWorksheet ws)
    {
        var columns = ws.Dimension.End.Column;
        var rightEnd = columns >= 3 ? Math.Max(1, columns / 3) : columns;
        if (AddHeaderImage(ws, "republic_header.png", "الجمهورية اليمنية", 2, 2, 1, rightEnd, 180, 28))
            ws.Cells[2, 1].Value = null;
        if (AddHeaderImage(ws, "ministry_header.png", "وزارة الداخلية", 3, 3, 1, rightEnd, 180, 28))
            ws.Cells[3, 1].Value = null;
        var firstColumn = columns >= 3 ? Math.Max(1, columns / 3) + 1 : 1;
        var lastColumn = columns >= 3 ? Math.Max(firstColumn, columns * 2 / 3) : columns;
        AddHeaderImage(ws, "pension-logo.png", "شعار الصندوق", 2, 4, firstColumn, lastColumn, 80, 90,
            center: columns >= 3);
    }

    [SupportedOSPlatform("windows6.1")]
    private bool AddHeaderImage(ExcelWorksheet ws, string fileName, string name, int firstRow, int lastRow,
        int firstColumn, int lastColumn, int maximumWidth, int maximumHeight, bool center = true)
    {
        using var image = LoadImage(fileName);
        if (image == null) return false;
        using var stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        var picture = ws.Drawings.AddPicture(name, stream, ePictureType.Png);
        // Keep the broadly supported two-cell DrawingML anchor while allowing
        // users to move the picture without stretching it when rows are resized.
        picture.ChangeCellAnchor(eEditAs.TwoCell);
        picture.EditAs = eEditAs.OneCell;
        var availableWidth = Enumerable.Range(firstColumn, lastColumn - firstColumn + 1)
            .Sum(column => ws.Column(column).Width * 7 + 5);
        var availableHeight = Enumerable.Range(firstRow, lastRow - firstRow + 1)
            .Sum(row => ws.Row(row).Height * 96 / 72);
        var scale = Math.Min(Math.Min(maximumWidth, availableWidth - 8) / image.Width,
            Math.Min(maximumHeight, availableHeight - 4) / image.Height);
        var width = Math.Max(1, (int)Math.Round(image.Width * scale));
        var height = Math.Max(1, (int)Math.Round(image.Height * scale));
        picture.SetPosition(firstRow - 1, Math.Max(0, (int)(availableHeight - height) / 2),
            firstColumn - 1, center ? Math.Max(0, (int)(availableWidth - width) / 2) : 8);
        picture.SetSize(width, height);
        return true;
    }

    private static float EstimateReportWidth(ExcelWorksheet ws)
    {
        var dimension = ws.Dimension;
        if (dimension == null) return 0;
        var width = 0f;
        for (var column = dimension.Start.Column; column <= dimension.End.Column; column++)
        {
            if (ws.Column(column).Hidden) continue;
            // Excel column widths are character units; at the default font they are
            // approximately seven pixels per character plus five pixels of padding.
            width += (float)((ws.Column(column).Width * 7 + 5) * PointsPerInch / 96);
        }
        return width;
    }

    private static void ExpandColumnsToPageWidth(ExcelWorksheet ws, float reportWidth, float printableWidth)
    {
        if (reportWidth >= printableWidth || ws.Dimension == null) return;
        var visibleColumns = Enumerable.Range(ws.Dimension.Start.Column,
            ws.Dimension.End.Column - ws.Dimension.Start.Column + 1)
            .Where(column => !ws.Column(column).Hidden).ToArray();
        if (visibleColumns.Length == 0) return;
        var additionalCharacters = (printableWidth - reportWidth) / visibleColumns.Length / (7 * PointsPerInch / 96);
        foreach (var column in visibleColumns)
            ws.Column(column).Width += additionalCharacters;
    }

    [SupportedOSPlatform("windows6.1")]
    private byte[] CreateWatermark(float pageWidth, float pageHeight, float repeatedHeaderHeight)
    {
        const float renderScale = 2; // 144 DPI, with all layout measurements in points.
        var width = pageWidth - 2 * SideMargin;
        var height = pageHeight - HeaderMargin - BottomMargin;
        using var canvas = new Bitmap((int)Math.Ceiling(width * renderScale),
            (int)Math.Ceiling(height * renderScale), PixelFormat.Format32bppArgb);
        canvas.SetResolution(PointsPerInch * renderScale, PointsPerInch * renderScale);
        using (var graphics = Graphics.FromImage(canvas))
        {
            graphics.Clear(Color.Transparent);
            graphics.ScaleTransform(renderScale, renderScale);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var logo = LoadImage("pension-logo.png");
            if (logo != null)
            {
                // Centre within the page body after the repeated letterhead and
                // column titles, accounting for FitToWidth scaling on wide reports.
                var bodyTop = TopMargin + repeatedHeaderHeight;
                var centerY = (bodyTop + pageHeight - BottomMargin) / 2 - HeaderMargin;
                DrawFittedImage(graphics, logo,
                    new RectangleF((width - WatermarkWidth) / 2, centerY - WatermarkWidth / 2,
                        WatermarkWidth, WatermarkWidth), 0.12f);
            }
        }

        using var stream = new MemoryStream();
        canvas.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    [SupportedOSPlatform("windows6.1")]
    private Image? LoadImage(string fileName)
    {
        foreach (var directory in _imageDirectories)
        {
            var path = Path.Combine(directory, fileName);
            if (!File.Exists(path)) continue;
            try
            {
                return Image.FromFile(path);
            }
            // GDI+ also reports an invalid image format as OutOfMemoryException.
            catch (Exception ex) when (ex is ArgumentException or IOException or ExternalException or UnauthorizedAccessException or OutOfMemoryException)
            {
                Trace.TraceWarning($"[ExcelReportTemplate] Cannot load {fileName}: {ex.Message}");
            }
        }
        return null;
    }

    [SupportedOSPlatform("windows6.1")]
    private static void DrawFittedImage(Graphics graphics, Image image, RectangleF bounds, float opacity = 1)
    {
        var scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
        var width = image.Width * scale;
        var height = image.Height * scale;
        var destination = Rectangle.Round(new RectangleF(bounds.X + (bounds.Width - width) / 2,
            bounds.Y + (bounds.Height - height) / 2, width, height));
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = opacity });
        graphics.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
    }

}
