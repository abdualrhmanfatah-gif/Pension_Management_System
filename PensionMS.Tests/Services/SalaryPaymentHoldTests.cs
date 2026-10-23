using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.DTOs.Suspensions;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class SalaryPaymentHoldTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task UpdateItem_UsesStructuredHoldResultAndRejectsCheckFailure(bool held, bool failed)
    {
        var unit = new Mock<IUnitOfWork>();
        var item = new SalaryPaymentItem { PaymentItemID = 7, FileID = 2,
            StatusID = TransactionStatusConstants.PendingSend,
            Batch = new SalaryPaymentBatch { Status = BatchStatusConstants.Approved } };
        var items = new Mock<IRepository<SalaryPaymentItem>>();
        items.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<SalaryPaymentItem>([item]));
        unit.Setup(u => u.SalaryPaymentItems).Returns(items.Object);
        var suspensions = new Mock<IPensionSuspensionService>();
        // تعمّد اختلاف النص عن القرار لإثبات أن التنفيذ يستخدم IsHeld ولا يقارن نص العرض.
        suspensions.Setup(s => s.CheckPaymentHoldAsync(2)).ReturnsAsync(failed
            ? ApiResponse<PaymentHoldCheckDto>.Fail("تعذر فحص الملف")
            : ApiResponse<PaymentHoldCheckDto>.Success(new(held, held ? "إيقاف إداري" : null,
                held ? "قابل للصرف" : "موقوف")));
        var service = new SalaryPaymentBatchService(unit.Object, Mock.Of<IPayrollGenerationService>(),
            Mock.Of<IWorkflowTaskService>(), Mock.Of<INotificationService>(), Mock.Of<IJournalEntryService>(),
            NullLogger<SalaryPaymentBatchService>.Instance, new ExcelReportExporter(new ExcelReportTemplate()),
            TestPensionAccess.All(unit.Object), suspensions.Object);

        var result = await service.UpdateItemStatusAsync(7, new UpdateItemStatusDto
            { StatusID = TransactionStatusConstants.Sent, TransactionNumber = "TX7" });

        Assert.Equal(!held && !failed, result.IsSuccess);
        suspensions.Verify(s => s.CheckPaymentHoldAsync(2), Times.Once);
        if (held || failed)
        {
            Assert.Equal(TransactionStatusConstants.PendingSend, item.StatusID);
            Assert.Null(item.TransactionNumber);
            Assert.Equal(failed ? "تعذر فحص الملف" : "إيقاف إداري", result.Message);
            unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
        else
        {
            Assert.Equal(TransactionStatusConstants.Sent, item.StatusID);
            Assert.Equal("TX7", item.TransactionNumber);
            unit.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
