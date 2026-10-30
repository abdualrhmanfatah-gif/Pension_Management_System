using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OfficeOpenXml;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Infrastructure.Services.Pension.Entitlements;

namespace PensionMS.Tests.Services;

public class SalaryImportIdentityTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task MultiRecipientFile_RequiresStableItemIdentityAndNoPaymentHold(bool includeIdentity, bool held)
    {
        var unit = new Mock<IUnitOfWork>();
        var batchRepo = new Mock<IRepository<SalaryPaymentBatch>>();
        batchRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SalaryPaymentBatch { BatchID = 1, Status = BatchStatusConstants.Approved });
        batchRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<SalaryPaymentBatch>(new[] { new SalaryPaymentBatch { BatchID = 1, Status = BatchStatusConstants.Approved } }));
        unit.Setup(u => u.SalaryPaymentBatches).Returns(batchRepo.Object);
        var file = new RetirementFile { FileID = 1, FileNumber = "F1", StatusID = FileStatusConstants.Active, ReviewStatus = FileReviewStatus.Approved };
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>(new[] { file }));
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        var holds = new Mock<IRepository<PensionSuspension>>();
        holds.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PensionSuspension>(held
            ? [new PensionSuspension { FileID = 1, Status = SuspensionStatusConstants.Active }] : []));
        unit.Setup(u => u.PensionSuspensions).Returns(holds.Object);
        var items = new[] { new SalaryPaymentItem { PaymentItemID = 11, BatchID = 1, FileID = 1, RetirementFile = file },
            new SalaryPaymentItem { PaymentItemID = 12, BatchID = 1, FileID = 1, RetirementFile = file } };
        var itemRepo = new Mock<IRepository<SalaryPaymentItem>>();
        itemRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<SalaryPaymentItem>(items));
        unit.Setup(u => u.SalaryPaymentItems).Returns(itemRepo.Object);
        var statusRepo = new Mock<IRepository<TransactionStatus>>();
        statusRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<TransactionStatus>(Array.Empty<TransactionStatus>()));
        unit.Setup(u => u.TransactionStatuses).Returns(statusRepo.Object);
        var service = new SalaryPaymentBatchService(unit.Object, Mock.Of<IPayrollGenerationService>(), Mock.Of<IWorkflowTaskService>(),
            Mock.Of<INotificationService>(), Mock.Of<IJournalEntryService>(),
            NullLogger<SalaryPaymentBatchService>.Instance, new ExcelReportExporter(new ExcelReportTemplate()), PensionMS.Testing.TestPensionAccess.All(unit.Object),
            new PensionSuspensionService(unit.Object, Mock.Of<INotificationService>(), PensionMS.Testing.TestPensionAccess.All(unit.Object), Mock.Of<IDocumentPledgeService>()));

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("نتائج");
        sheet.Cells[1, 1].Value = "رقم الملف";
        sheet.Cells[1, 2].Value = "رقم الحوالة";
        if (includeIdentity) sheet.Cells[1, 11].Value = "معرف بند الصرف";
        for (var row = 2; row <= 3; row++)
        {
            sheet.Cells[row, 1].Value = "F1";
            sheet.Cells[row, 2].Value = "TX" + row;
            if (includeIdentity) sheet.Cells[row, 11].Value = row + 9;
        }
        using var stream = new MemoryStream(package.GetAsByteArray());
        var result = await service.ImportTransactionResultsAsync(1, stream);
        Assert.True(result.IsSuccess);
        if (held)
        {
            Assert.All(items, item => Assert.Null(item.TransactionNumber));
            Assert.Equal(2, result.Data!.SkippedCount);
            Assert.Equal(0, result.Data.UpdatedCount);
            Assert.All(result.Data.Errors, error => Assert.Contains("يوجد إيقاف معاش نشط", error));
        }
        else if (includeIdentity)
        {
            Assert.Equal("TX2", items[0].TransactionNumber);
            Assert.Equal("TX3", items[1].TransactionNumber);
            Assert.Equal(2, result.Data!.UpdatedCount);
        }
        else
        {
            Assert.All(items, item => Assert.Null(item.TransactionNumber));
            Assert.Equal(2, result.Data!.SkippedCount);
            Assert.All(result.Data.Errors, error => Assert.Contains("عدة مستلمين", error));
        }
    }
}
