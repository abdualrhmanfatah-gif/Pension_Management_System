using PensionMS.Testing;
using Moq;
using OfficeOpenXml;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Infrastructure.Services.Reporting;

namespace PensionMS.Tests.Services;

public class SharedExcelReportTemplateTests
{
    public SharedExcelReportTemplateTests() => ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    private static IRepository<T> Repository<T>(params T[] rows) where T : class
    {
        var repository = new Mock<IRepository<T>>();
        repository.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<T>(rows));
        return repository.Object;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ActiveRetireesExport_UsesSharedTemplateAndIncludesRetireesAcrossBranches(bool empty)
    {
        var first = new Retiree { RetireeID = 1, FullName = "أحمد علي", NationalID = "001", MilitaryNumber = "101" };
        var second = new Retiree { RetireeID = 2, FullName = "متقاعد فرع آخر", NationalID = "002", IsAlive = false };
        var files = new[]
        {
            new RetirementFile { FileID = 1, RetireeID = 1, PensionBranchID = 1 },
            new RetirementFile { FileID = 2, RetireeID = 2, PensionBranchID = 2 }
        };
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(u => u.Retirees).Returns(Repository(empty ? Array.Empty<Retiree>() : new[] { first, second }));
        unit.Setup(u => u.RetirementFiles).Returns(Repository(files));
        unit.Setup(u => u.FinancialEntitlements).Returns(Repository(
            new FinancialEntitlement { RetireeID = 1, Amount = 125000m },
            new FinancialEntitlement { RetireeID = 2, Amount = 250000m }));
        var scope = Mock.Of<ITestBranchScope>(s => s.GetCurrentBranchId() == 1 && s.IsHQ() == false);
        var service = new ReportService(unit.Object, Mock.Of<IMissingDocumentService>(), TestPensionAccess.From(unit.Object, scope), new ExcelReportExporter(new ExcelReportTemplate()));

        using var package = new ExcelPackage(new MemoryStream(await service.ExportActiveRetireesAsync(new ReportFilter())));
        var sheet = Assert.Single(package.Workbook.Worksheets);
        var count = empty ? 0 : 1;
        AssertBranding(sheet, "تقرير المتقاعدين", count);
        Assert.Equal(9, sheet.Dimension.End.Column);
        Assert.Equal($"إجمالي المتقاعدين: {count}", sheet.Cells[count + 9, 1].Text);
        if (!empty)
        {
            Assert.Equal("أحمد علي", sheet.Cells[9, 2].Text);
            Assert.Equal("001", sheet.Cells[9, 3].Text);
            Assert.Equal(125000m, Convert.ToDecimal(sheet.Cells[9, 7].Value));
            Assert.Equal("حالة الحياة", sheet.Cells[8, 8].Text);
            Assert.Equal("حي", sheet.Cells[9, 8].Text);
        }
        unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AccountingExport_UsesSharedTemplateAndPreservesNumericAmounts()
    {
        var service = new AccountingExportService(new ExcelReportExporter(new ExcelReportTemplate()), new PdfReportExporter());
        var response = ApiResponse<List<TrialBalanceItemDto>>.Success(new()
        {
            new TrialBalanceItemDto { AccountCode = "001", AccountName = "الصندوق", TotalDebit = 125.50m,
                TotalCredit = 25m, Balance = 100.50m }
        });

        using var package = new ExcelPackage(new MemoryStream(await service.ExportTrialBalanceAsync(response, "excel")));
        var sheet = Assert.Single(package.Workbook.Worksheets);
        AssertBranding(sheet, "ميزان المراجعة", 1);
        Assert.Equal("001", sheet.Cells[9, 1].Text);
        Assert.Equal("الصندوق", sheet.Cells[9, 2].Text);
        Assert.Equal(125.50m, Convert.ToDecimal(sheet.Cells[9, 9].Value));
        Assert.Equal(100.50m, Convert.ToDecimal(sheet.Cells[9, 11].Value));
        Assert.Equal(10, sheet.PrinterSettings.PrintArea.End.Row);
    }

    [Fact]
    public void SharedTemplate_PreservesTitleTotalsAndRepeatsActualColumnHeaderRow()
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("قرار التسوية");
        sheet.View.RightToLeft = true;
        sheet.Cells[1, 1, 1, 8].Merge = true;
        sheet.Cells[1, 1].Value = "قرار تسوية رقم 12";
        sheet.Cells[3, 1].Value = "الاسم";
        sheet.Cells[3, 8].Value = "المبلغ";
        sheet.Cells[4, 1].Value = "أحمد";
        sheet.Cells[4, 8].Value = 1500m;
        sheet.Cells[6, 8].Formula = "SUM(H4:H5)";
        sheet.Cells[6, 8].Style.Numberformat.Format = "#,##0.00";

        new ExcelReportTemplate().Apply(sheet, 1, "قرار تسوية رقم 12", "12", headerRow: 3);
        using var reopened = new ExcelPackage(new MemoryStream(package.GetAsByteArray()));
        var result = Assert.Single(reopened.Workbook.Worksheets);
        AssertBranding(result, "قرار تسوية رقم 12", 1);
        Assert.Equal("قرار تسوية رقم 12", result.Cells[6, 1].Text);
        Assert.True(result.Cells[6, 1, 6, 8].Merge);
        Assert.Equal("SUM(H9:H10)", result.Cells[11, 8].Formula);
        Assert.Equal("#,##0.00", result.Cells[11, 8].Style.Numberformat.Format);
        Assert.Equal(11, result.PrinterSettings.PrintArea.End.Row);
        Assert.Equal(8, result.PrinterSettings.PrintArea.End.Column);
        Assert.Equal(1, result.PrinterSettings.RepeatRows.Start.Row);
        Assert.Equal(8, result.PrinterSettings.RepeatRows.End.Row);
    }

    private static void AssertBranding(ExcelWorksheet sheet, string title, int count)
    {
        Assert.True(sheet.View.RightToLeft);
        Assert.False(sheet.View.PageLayoutView);
        Assert.Equal(title, sheet.Workbook.Properties.Title);
        Assert.Equal($"عدد السجلات: {count}", sheet.Workbook.Properties.Subject);
        Assert.Equal(title, sheet.Cells[6, 1].Text);
        var metadataColumn = sheet.Dimension.End.Column * 2 / 3 + 1;
        Assert.Equal($"عدد السجلات: {count}", sheet.Cells[3, metadataColumn].Text);
        Assert.Equal("الجمهورية اليمنية", sheet.Cells[2, 1].Text);
        Assert.Equal("الإدارة العامة للصندوق التقاعد الأمني", sheet.Cells[4, 1].Text);
        Assert.Equal(1, sheet.PrinterSettings.RepeatRows.Start.Row);
        Assert.Equal(8, sheet.PrinterSettings.RepeatRows.End.Row);
        Assert.Equal(ePaperSize.A4, sheet.PrinterSettings.PaperSize);
        Assert.False(sheet.HeaderFooter.ScaleWithDocument);
        Assert.Equal(1, sheet.PrinterSettings.FitToWidth);
        Assert.Equal(0, sheet.PrinterSettings.FitToHeight);
        Assert.False(sheet.HeaderFooter.differentFirst);
        Assert.False(sheet.HeaderFooter.differentOddEven);
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            Assert.Contains("&G", sheet.HeaderFooter.OddHeader.CenteredText);
            Assert.Equal(1, sheet.HeaderFooter.Pictures.Count);
        }
    }
}
