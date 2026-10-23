using System.Text;
using System.Globalization;
using OfficeOpenXml;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Infrastructure.Reporting.Exporters;
using Xunit;

namespace PensionMS.Tests.Services;

public class RetirementFileReportExporterTests
{
    private readonly RetirementFileReportExporter _exporter = new(new ExcelReportExporter(new ExcelReportTemplate()));

    [Fact]
    public void ExcelExport_PreservesArabicColumnsDateAndRightToLeftLayout()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var rows = new[]
        {
            new RetirementFileReportRow
            {
                FileNumber = "ملف-١", RetireeName = "أحمد محمد", MilitaryNumber = "123",
                Gender = 2, RetirementDate = new DateOnly(2024, 3, 5), MissingDocumentsCount = 2
            }
        };

        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var bytes = _exporter.Export(rows, "excel");

            using var package = new ExcelPackage(new MemoryStream(bytes));
            var sheet = Assert.Single(package.Workbook.Worksheets);
            Assert.True(sheet.View.RightToLeft);
            Assert.False(sheet.View.PageLayoutView);
            Assert.Equal("تقرير ملفات التقاعد", package.Workbook.Properties.Title);
            Assert.Equal("تقرير ملفات التقاعد", sheet.Cells[6, 1].Text);
            Assert.Equal(12, sheet.Dimension.End.Column);
            Assert.Equal("ملف-١", sheet.Cells[9, 2].Text);
            Assert.Equal("أحمد محمد", sheet.Cells[9, 3].Text);
            Assert.Equal("أنثى", sheet.Cells[9, 7].Text);
            Assert.Equal("2024/03/05", sheet.Cells[9, 9].Text);
            Assert.Equal("2", sheet.Cells[9, 12].Text);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void PdfExport_GeneratesReadablePdfContainer()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var bytes = _exporter.Export(new[] { new RetirementFileReportRow { FileNumber = "ملف-١", RetireeName = "أحمد محمد" } }, "pdf");

        Assert.True(bytes.Length > 1000);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
    }
}
