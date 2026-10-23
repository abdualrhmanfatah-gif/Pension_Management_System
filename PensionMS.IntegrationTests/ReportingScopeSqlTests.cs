using PensionMS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OfficeOpenXml;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.API.Middleware;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Reporting;

namespace PensionMS.IntegrationTests;

public class ReportingScopeSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    private static ITestBranchScope Scope(int? branch, bool hq = false)
    {
        var mock = new Mock<ITestBranchScope>();
        mock.Setup(s => s.GetCurrentBranchId()).Returns(branch);
        mock.Setup(s => s.IsHQ()).Returns(hq);
        return mock.Object;
    }

    [Fact]
    public async Task ReportAndExportSources_RestrictRowsAndSharedBatchTotals()
    {
        await using var db = database.OpenContext();
        var actor = new User { Username = "sql-test", FullName = "مستخدم اختبار", PasswordHash = "test" };
        var branchA = new PensionFundBranch { BranchName = "فرع اختبار أ", IsActive = true };
        var branchB = new PensionFundBranch { BranchName = "فرع اختبار ب", IsActive = true };
        var ministry = new MinistryApprovalBatch { ImportedByUser = actor, Status = "Test" };
        var retireeA = new Retiree { FullName = "متقاعد اختبار أ", NationalID = "test-a",
            ApprovalItem = new MinistryApprovalItem { Batch = ministry, FullName = "متقاعد اختبار أ", MilitaryNumber = "100" } };
        var retireeB = new Retiree { FullName = "متقاعد اختبار ب", NationalID = "test-b",
            ApprovalItem = new MinistryApprovalItem { Batch = ministry, FullName = "متقاعد اختبار ب", MilitaryNumber = "200" } };
        var fileA = new RetirementFile { FileNumber = "test-file-a", Retiree = retireeA, PensionBranch = branchA, StatusID = FileStatusConstants.Active };
        var fileB = new RetirementFile { FileNumber = "test-file-b", Retiree = retireeB, PensionBranch = branchB, StatusID = FileStatusConstants.Active };
        fileB.CreatedAt = fileA.CreatedAt;
        db.AddRange(fileA, fileB);
        await db.SaveChangesAsync();
        var batch = new SalaryPaymentBatch { BatchMonth = new DateTime(2026, 10, 1), ImportedByUserID = actor.UserID,
            Bank = new Bank { BankName = "بنك اختبار" },
            TotalAmount = 30, TotalCount = 2, Status = BatchStatusConstants.Draft };
        batch.SalaryPaymentItems.Add(new SalaryPaymentItem { FileID = fileA.FileID, RetireeID = retireeA.RetireeID, Amount = 10, CreatedByUserID = actor.UserID });
        batch.SalaryPaymentItems.Add(new SalaryPaymentItem { FileID = fileB.FileID, RetireeID = retireeB.RetireeID, Amount = 20, CreatedByUserID = actor.UserID });
        db.Add(batch);
        await db.SaveChangesAsync();
        using var uow = new UnitOfWork(db);
        var missing = Mock.Of<IMissingDocumentService>();
        var exporter = new ExcelReportExporter(new ExcelReportTemplate());
        var a = new ReportService(uow, missing, TestPensionAccess.From(uow, Scope(branchA.PensionBranchID)), exporter);
        var b = new ReportService(uow, missing, TestPensionAccess.From(uow, Scope(branchB.PensionBranchID)), exporter);
        var hq = new ReportService(uow, missing, TestPensionAccess.From(uow, Scope(null, true)), exporter);
        var filter = new ReportFilter();
        Assert.Equal(fileA.FileID, Assert.Single((await a.GetFileStatusReportAsync(filter)).Data!.Items).FileID);
        Assert.Equal(fileB.FileID, Assert.Single((await b.GetFileStatusReportAsync(filter)).Data!.Items).FileID);
        Assert.Equal(2, (await hq.GetFileStatusReportAsync(filter)).Data!.TotalCount);
        var summaryA = (await a.GetKashfReportSummaryAsync(filter)).Data!;
        var summaryHQ = (await hq.GetKashfReportSummaryAsync(filter)).Data!;
        Assert.Equal(1, summaryA.GetType().GetProperty("totalCount")!.GetValue(summaryA));
        Assert.Equal(2, summaryHQ.GetType().GetProperty("totalCount")!.GetValue(summaryHQ));
        var firstPage = (await hq.GetFileStatusReportAsync(new ReportFilter { PageNumber = 1, PageSize = 1 })).Data!;
        var secondPage = (await hq.GetFileStatusReportAsync(new ReportFilter { PageNumber = 2, PageSize = 1 })).Data!;
        Assert.Equal(fileB.FileID, Assert.Single(firstPage.Items).FileID);
        Assert.Equal(fileA.FileID, Assert.Single(secondPage.Items).FileID);
        Assert.Equal(10m, Assert.Single((await a.GetSalaryBatchesAsync(filter)).Data!.Items).TotalAmount);
        Assert.Equal(20m, Assert.Single((await b.GetSalaryBatchesAsync(filter)).Data!.Items).TotalAmount);
        Assert.Equal(30m, Assert.Single((await hq.GetSalaryBatchesAsync(filter)).Data!.Items).TotalAmount);
        var source = new ReportDataService(a, uow, TestPensionAccess.From(uow, Scope(branchA.PensionBranchID)));
        Assert.Single(await source.GetDataAsync("basic-retirement-files", filter));
        var selectedBranch = new ReportFilter { BranchId = branchB.PensionBranchID };
        var pdfSource = await new ReportDataService(hq, uow, TestPensionAccess.From(uow, Scope(null, true))).GetDataAsync("basic-retirement-files", selectedBranch);
        var pdfRow = Assert.Single(pdfSource);
        Assert.Equal(fileB.FileNumber, pdfRow.GetType().GetProperty("FileNumber")!.GetValue(pdfRow));
        using (var excel = new ExcelPackage(new MemoryStream(await hq.ExportRetirementFilesAsync(branchId: branchB.PensionBranchID))))
        {
            var sheet = excel.Workbook.Worksheets[0];
            var header = Enumerable.Range(1, sheet.Dimension.End.Row).Single(row => sheet.Cells[row, 2].Text == "رقم الملف");
            Assert.Equal(fileB.FileNumber, sheet.Cells[header + 1, 2].GetValue<string>());
            Assert.DoesNotContain(sheet.Cells[sheet.Dimension.Address], cell => cell.Text == fileA.FileNumber);
        }
        await Assert.ThrowsAsync<BranchScopeAccessException>(() => a.GetFileStatusReportAsync(new ReportFilter { BranchId = branchB.PensionBranchID }));
        Assert.Empty(await TestPensionAccess.Query(uow, Scope(branchA.PensionBranchID)).Query(uow.AuditLogs).ToListAsync());
    }

}
