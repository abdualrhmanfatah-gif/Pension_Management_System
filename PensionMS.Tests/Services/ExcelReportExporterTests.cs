using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using Microsoft.Extensions.Hosting;
using Moq;
using OfficeOpenXml;
using PensionMS.Core.Reporting;
using PensionMS.Infrastructure.Reporting.Exporters;

namespace PensionMS.Tests.Services;

public class ExcelReportExporterTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "PensionMS-Excel-" + Guid.NewGuid());

    private static ReportDefinition Report => new()
    {
        Id = "test&01",
        NameAr = "تقرير الاختبار",
        Columns = new[]
        {
            new ReportColumn { Key = "Name", Label = "الاسم" },
            new ReportColumn { Key = "Amount", Label = "المبلغ", Type = ReportColumnType.Currency },
            new ReportColumn { Key = "Active", Label = "نشط", Type = ReportColumnType.Boolean }
        }
    };

    public ExcelReportExporterTests()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        Directory.CreateDirectory(Path.Combine(_directory, "wwwroot"));
    }

    private byte[] Export(object[] data)
    {
        var environment = Mock.Of<IHostEnvironment>(e => e.ContentRootPath == _directory);
        return new ExcelReportExporter(new ExcelReportTemplate(environment)).Export(Report, data);
    }

    [Fact]
    public void ExportRows_PreservesTitleColumnsAmountsAndTotals()
    {
        var exporter = new ExcelReportExporter(new ExcelReportTemplate());
        var rows = new[]
        {
            new object?[] { 1, "أحمد", 1250m },
            new object?[] { 2, "علي", 750m }
        };
        var bytes = exporter.ExportRows("كشف الصرف", new[] { "م", "الاسم", "المبلغ" }, rows,
            new ExcelReportOptions
            {
                Title = "كشف صرف المعاشات",
                ReportId = "42",
                HeaderRow = 3,
                FooterGap = 1,
                FooterLabelSpan = 2,
                FooterText = "الإجمالي: 2 مستفيد",
                FooterValues = new Dictionary<int, decimal> { [3] = 2000m },
                NumberFormats = new Dictionary<int, string> { [3] = "#,##0" }
            });

        using var package = new ExcelPackage(new MemoryStream(bytes));
        var sheet = Assert.Single(package.Workbook.Worksheets);
        Assert.Equal("كشف صرف المعاشات", sheet.Cells[6, 1].Text);
        Assert.True(sheet.Cells[6, 1, 6, 3].Merge);
        Assert.Equal("المبلغ", sheet.Cells[8, 3].Text);
        Assert.Equal("أحمد", sheet.Cells[9, 2].Text);
        Assert.Equal(1250m, Convert.ToDecimal(sheet.Cells[9, 3].Value));
        Assert.Equal("#,##0", sheet.Cells[9, 3].Style.Numberformat.Format);
        Assert.Equal("الإجمالي: 2 مستفيد", sheet.Cells[12, 1].Text);
        Assert.Equal(2000m, Convert.ToDecimal(sheet.Cells[12, 3].Value));
        Assert.Equal(12, sheet.PrinterSettings.PrintArea.End.Row);
        Assert.Equal(1, sheet.PrinterSettings.RepeatRows.Start.Row);
        Assert.Equal(8, sheet.PrinterSettings.RepeatRows.End.Row);
        Assert.Equal("42", package.Workbook.Properties.Comments);
        Assert.Equal("الرقم: 42", sheet.Cells[4, 3].Text);
    }

    [Fact]
    public void Export_PreservesTableAndConfiguresBrandingOnAllPrintedPages()
    {
        var data = Enumerable.Range(1, 200)
            .Select(i => (object)new { Name = $"متقاعد {i}", Amount = 123.45m, Active = true }).ToArray();
        using var package = new ExcelPackage(new MemoryStream(Export(data)));
        var sheet = Assert.Single(package.Workbook.Worksheets);

        Assert.True(sheet.View.RightToLeft);
        Assert.False(sheet.View.PageLayoutView);
        Assert.Equal("الاسم", sheet.Cells[8, 1].Text);
        Assert.Equal("متقاعد 1", sheet.Cells[9, 1].Text);
        Assert.Equal(123.45m, Convert.ToDecimal(sheet.Cells[9, 2].Value));
        Assert.Equal("نعم", sheet.Cells[9, 3].Text);
        Assert.Equal("إجمالي السجلات: 200", sheet.Cells[209, 1].Text);
        Assert.Equal(ePaperSize.A4, sheet.PrinterSettings.PaperSize);
        Assert.Equal(eOrientation.Portrait, sheet.PrinterSettings.Orientation);
        Assert.True(sheet.PrinterSettings.FitToPage);
        Assert.Equal(1, sheet.PrinterSettings.FitToWidth);
        Assert.Equal(0, sheet.PrinterSettings.FitToHeight);
        Assert.Equal(1, sheet.PrinterSettings.RepeatRows.Start.Row);
        Assert.Equal(8, sheet.PrinterSettings.RepeatRows.End.Row);
        Assert.Equal(1, sheet.PrinterSettings.PrintArea.Start.Row);
        Assert.Equal(209, sheet.PrinterSettings.PrintArea.End.Row);
        Assert.Equal(3, sheet.PrinterSettings.PrintArea.End.Column);
        Assert.False(sheet.HeaderFooter.differentFirst);
        Assert.False(sheet.HeaderFooter.differentOddEven);
        Assert.False(sheet.HeaderFooter.ScaleWithDocument);
        Assert.True(sheet.Column(1).Width > 12);
        var printedTableWidth = Enumerable.Range(1, 3)
            .Sum(column => (sheet.Column(column).Width * 7 + 5) * 72 / 96);
        var printablePageWidth = 210d / 25.4 * 72 - 2 * 0.3 * 72;
        Assert.InRange(printedTableWidth, printablePageWidth - 1, printablePageWidth + 1);
        Assert.Equal("تقرير الاختبار", package.Workbook.Properties.Title);
        Assert.Equal("test&01", package.Workbook.Properties.Comments);
        Assert.Equal("عدد السجلات: 200", package.Workbook.Properties.Subject);
        Assert.Equal("تقرير الاختبار", sheet.Cells[6, 1].Text);
        Assert.Equal("الرقم: test&01", sheet.Cells[4, 3].Text);
        Assert.Equal("عدد السجلات: 200", sheet.Cells[3, 3].Text);
        Assert.Equal("الجمهورية اليمنية", sheet.Cells[2, 1].Text);
        Assert.Equal("وزارة الداخلية", sheet.Cells[3, 1].Text);
        Assert.Equal("الإدارة العامة للصندوق التقاعد الأمني", sheet.Cells[4, 1].Text);
        Assert.Equal("بسم الله الرحمن الرحيم", sheet.Cells[1, 1].Text);
        Assert.True(string.IsNullOrEmpty(sheet.HeaderFooter.OddHeader.LeftAlignedText));
        Assert.Contains("&P", sheet.HeaderFooter.OddFooter.CenteredText);
        Assert.Contains("&N", sheet.HeaderFooter.OddFooter.CenteredText);
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            Assert.Contains("&G", sheet.HeaderFooter.OddHeader.CenteredText);
            Assert.Equal(1, sheet.HeaderFooter.Pictures.Count);
            Assert.InRange(sheet.HeaderFooter.Pictures[0].Width, printablePageWidth - 1, printablePageWidth + 1);
        }
    }

    [Fact]
    public void ExportRows_WideReportUsesLandscapeAndKeepsHeaderAtPageWidth()
    {
        var headers = Enumerable.Range(1, 12).Select(index => $"عمود {index}").ToArray();
        var row = Enumerable.Range(1, headers.Length).Select(index => (object?)index).ToArray();
        var exporter = new ExcelReportExporter(new ExcelReportTemplate());
        using var package = new ExcelPackage(new MemoryStream(exporter.ExportRows("تقرير واسع", headers, new[] { row })));
        var sheet = Assert.Single(package.Workbook.Worksheets);

        Assert.Equal(eOrientation.Landscape, sheet.PrinterSettings.Orientation);
        Assert.False(sheet.HeaderFooter.ScaleWithDocument);
        Assert.Equal(1, sheet.PrinterSettings.FitToWidth);
        Assert.Equal("تقرير واسع", sheet.Cells[6, 1].Text);
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            Assert.True(sheet.HeaderFooter.Pictures[0].Width > 700);
    }

    [Fact]
    public void Export_HeaderTextRemainsEditableAfterSavingWorkbook()
    {
        using var package = new ExcelPackage(new MemoryStream(Export(Array.Empty<object>())));
        var sheet = Assert.Single(package.Workbook.Worksheets);
        sheet.Cells[6, 1].Value = "عنوان معدل";
        sheet.Cells[2, 1].Value = "جهة معدلة";
        sheet.Cells[1, 1].Value = "بسملة معدلة";
        sheet.Cells[2, 3].Value = "التاريخ: 2026/10/16";

        using var reopened = new ExcelPackage(new MemoryStream(package.GetAsByteArray()));
        var savedSheet = Assert.Single(reopened.Workbook.Worksheets);
        Assert.Equal("عنوان معدل", savedSheet.Cells[6, 1].Text);
        Assert.Equal("جهة معدلة", savedSheet.Cells[2, 1].Text);
        Assert.Equal("بسملة معدلة", savedSheet.Cells[1, 1].Text);
        Assert.Equal("التاريخ: 2026/10/16", savedSheet.Cells[2, 3].Text);
        Assert.False(savedSheet.Protection.IsProtected);
        Assert.False(savedSheet.Cells[2, 1].Style.Locked);
        Assert.Equal("نشط", savedSheet.Cells[8, 3].Text);
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            Assert.Single(savedSheet.HeaderFooter.Pictures);
    }

    [Fact]
    public void Export_EmbedsOfficialHeaderImagesAndLogoWithEditableTitleAndWatermark()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;
        SaveAsset("republic_header.png", Color.Red);
        SaveAsset("ministry_header.png", Color.Blue);
        SaveAsset("basmalah.png", Color.Green);
        SaveAsset("pension-logo.png", Color.FromArgb(128, 255, 255, 0));

        using var package = new ExcelPackage(new MemoryStream(Export(Array.Empty<object>())));
        var sheet = package.Workbook.Worksheets[0];
        Assert.Equal(3, sheet.Drawings.Count);
        var logo = Assert.IsType<OfficeOpenXml.Drawing.ExcelPicture>(sheet.Drawings["شعار الصندوق"]);
        using var logoStream = new MemoryStream(logo.Image.ImageBytes);
        using var logoImage = new Bitmap(logoStream);
        Assert.InRange(logoImage.GetPixel(16, 16).A, 127, 128);
        Assert.Empty(sheet.Cells[2, 1].Text);
        Assert.Empty(sheet.Cells[3, 1].Text);
        Assert.Equal("الإدارة العامة للصندوق التقاعد الأمني", sheet.Cells[4, 1].Text);
        Assert.Equal("بسم الله الرحمن الرحيم", sheet.Cells[1, 1].Text);
        foreach (var (name, color) in new[] { ("الجمهورية اليمنية", Color.Red), ("وزارة الداخلية", Color.Blue) })
        {
            var headerImage = Assert.IsType<OfficeOpenXml.Drawing.ExcelPicture>(sheet.Drawings[name]);
            using var assetStream = new MemoryStream(headerImage.Image.ImageBytes);
            using var assetImage = new Bitmap(assetStream);
            Assert.Equal(color.ToArgb(), assetImage.GetPixel(16, 16).ToArgb());
        }
        foreach (var drawing in sheet.Drawings)
        {
            Assert.Equal(OfficeOpenXml.Drawing.eEditAs.TwoCell, drawing.CellAnchor);
            Assert.Equal(OfficeOpenXml.Drawing.eEditAs.OneCell, drawing.EditAs);
            Assert.InRange(drawing.From.Row, 1, 3);
            Assert.True(drawing.To.Row < sheet.PrinterSettings.RepeatRows.End.Row);
            Assert.True(drawing.To.Column > drawing.From.Column || drawing.To.ColumnOff > drawing.From.ColumnOff);
            Assert.True(drawing.To.Row > drawing.From.Row || drawing.To.RowOff > drawing.From.RowOff);
        }
        var picture = sheet.HeaderFooter.Pictures[0];
        using var stream = new MemoryStream(picture.Image.ImageBytes);
        using var image = new Bitmap(stream);

        Assert.All(HeaderColors(image), color => Assert.Equal(0, Color.FromArgb(color).A));
        // The watermark keeps source transparency: 128 * 0.12, rather than opaque alpha.
        var watermarkY = (int)WatermarkCenter(image).Y;
        Assert.InRange(image.GetPixel(image.Width / 2, watermarkY).A, 14, 16);
        Assert.Equal(0, image.GetPixel(5, watermarkY).A);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(12)]
    public void Export_CentersWatermarkInPrintableBodyBelowRepeatedLetterhead(int columnCount)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;
        SaveAsset("pension-logo.png", Color.Green);
        var environment = Mock.Of<IHostEnvironment>(e => e.ContentRootPath == _directory);
        var exporter = new ExcelReportExporter(new ExcelReportTemplate(environment));
        var headers = Enumerable.Range(1, columnCount).Select(index => $"عمود {index}").ToArray();
        using var package = new ExcelPackage(new MemoryStream(exporter.ExportRows("تقرير", headers, Array.Empty<object?[]>())));
        var sheet = package.Workbook.Worksheets[0];
        using var stream = new MemoryStream(sheet.HeaderFooter.Pictures[0].Image.ImageBytes);
        using var image = new Bitmap(stream);
        var center = WatermarkCenter(image);
        Assert.InRange(center.X, image.Width / 2d - 2, image.Width / 2d + 2);

        var landscape = sheet.PrinterSettings.Orientation == eOrientation.Landscape;
        var pageHeight = (landscape ? 210 : 297) / 25.4 * 72;
        var pageWidth = (landscape ? 297 : 210) / 25.4 * 72;
        var printableWidth = pageWidth - (double)(sheet.PrinterSettings.LeftMargin + sheet.PrinterSettings.RightMargin) * 72;
        var tableWidth = Enumerable.Range(1, columnCount).Sum(column => (sheet.Column(column).Width * 7 + 5) * 72 / 96);
        var printScale = Math.Min(1, printableWidth / tableWidth);
        var repeatedHeight = Enumerable.Range(1, sheet.PrinterSettings.RepeatRows.End.Row).Sum(row => sheet.Row(row).Height);
        var bodyTop = (double)sheet.PrinterSettings.TopMargin * 72 + repeatedHeight * printScale;
        var bodyBottom = pageHeight - (double)sheet.PrinterSettings.BottomMargin * 72;
        var actualCenter = center.Y / 2 + (double)sheet.PrinterSettings.HeaderMargin * 72;
        Assert.InRange(actualCenter, (bodyTop + bodyBottom) / 2 - 2, (bodyTop + bodyBottom) / 2 + 2);
    }

    [Fact]
    public void Export_MissingOrCorruptAssetsStillProducesWorkbook()
    {
        File.WriteAllText(Path.Combine(_directory, "wwwroot", "republic_header.png"), "invalid image");
        File.WriteAllText(Path.Combine(_directory, "wwwroot", "ministry_header.png"), "invalid image");
        File.WriteAllText(Path.Combine(_directory, "wwwroot", "pension-logo.png"), "invalid image");

        using var package = new ExcelPackage(new MemoryStream(Export(Array.Empty<object>())));
        var sheet = Assert.Single(package.Workbook.Worksheets);
        Assert.Equal("إجمالي السجلات: 0", sheet.Cells[9, 1].Text);
        Assert.Equal(9, sheet.PrinterSettings.PrintArea.End.Row);
        Assert.Equal(3, sheet.PrinterSettings.PrintArea.End.Column);
        Assert.Equal("الإدارة العامة للصندوق التقاعد الأمني", sheet.Cells[4, 1].Text);
        Assert.Equal("الجمهورية اليمنية", sheet.Cells[2, 1].Text);
        Assert.Equal("وزارة الداخلية", sheet.Cells[3, 1].Text);
        Assert.Empty(sheet.Drawings);
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            using var stream = new MemoryStream(sheet.HeaderFooter.Pictures[0].Image.ImageBytes);
            using var image = new Bitmap(stream);
            Assert.All(HeaderColors(image), color => Assert.Equal(0, Color.FromArgb(color).A));
            Assert.Equal(0, image.GetPixel(image.Width / 2, image.Height / 2 + 8).A);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ExportRows_NarrowReportsKeepEditableLetterheadAndTable(int columns)
    {
        var exporter = new ExcelReportExporter(new ExcelReportTemplate());
        var headers = Enumerable.Range(1, columns).Select(index => $"عمود {index}").ToArray();
        using var package = new ExcelPackage(new MemoryStream(exporter.ExportRows("تقرير مختصر", headers, Array.Empty<object?[]>())));
        var sheet = Assert.Single(package.Workbook.Worksheets);
        Assert.Equal(columns, sheet.Dimension.End.Column);
        Assert.Equal("الإدارة العامة للصندوق التقاعد الأمني", sheet.Cells[4, 1].Text);
        Assert.Contains("عدد السجلات: 0", sheet.Cells[5, 1].Text);
        Assert.Equal("تقرير مختصر", sheet.Cells[6, 1].Text);
        Assert.Equal(headers[0], sheet.Cells[8, 1].Text);
        Assert.Equal(8, sheet.PrinterSettings.RepeatRows.End.Row);
    }

    [SupportedOSPlatform("windows6.1")]
    private static (double X, double Y) WatermarkCenter(Bitmap image)
    {
        var vertical = new List<int>();
        for (var y = 0; y < image.Height; y++)
            if (image.GetPixel(image.Width / 2, y).A > 0) vertical.Add(y);
        Assert.NotEmpty(vertical);
        var centerY = (vertical[0] + vertical[^1]) / 2d;
        var horizontal = new List<int>();
        for (var x = 0; x < image.Width; x++)
            if (image.GetPixel(x, (int)centerY).A > 0) horizontal.Add(x);
        Assert.NotEmpty(horizontal);
        return ((horizontal[0] + horizontal[^1]) / 2d, centerY);
    }

    [SupportedOSPlatform("windows6.1")]
    private void SaveAsset(string fileName, Color color)
    {
        using var image = new Bitmap(32, 32);
        using (var graphics = Graphics.FromImage(image)) graphics.Clear(color);
        image.Save(Path.Combine(_directory, "wwwroot", fileName), ImageFormat.Png);
    }

    [SupportedOSPlatform("windows6.1")]
    private static HashSet<int> HeaderColors(Bitmap image)
    {
        var colors = new HashSet<int>();
        for (var y = 0; y < 160; y++)
        for (var x = 0; x < image.Width; x++)
            colors.Add(image.GetPixel(x, y).ToArgb());
        return colors;
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);
}
