using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class PensionSalaryStatusTests
{
    [Theory]
    [InlineData(FileStatusConstants.Active, FileReviewStatus.Approved, null, false, "قابل للصرف")]
    [InlineData(FileStatusConstants.Active, FileReviewStatus.Approved, "فعّال", false, "موقوف")]
    [InlineData(FileStatusConstants.Active, FileReviewStatus.Approved, "Active", false, "موقوف")]
    [InlineData(FileStatusConstants.Active, FileReviewStatus.Approved, "Active", true, "قابل للصرف")]
    [InlineData(FileStatusConstants.Active, FileReviewStatus.Approved, "منتهي", false, "قابل للصرف")]
    [InlineData(FileStatusConstants.Pending, FileReviewStatus.Approved, null, false, "موقوف")]
    [InlineData(FileStatusConstants.Active, FileReviewStatus.PendingReview, null, false, "موقوف")]
    [InlineData(FileStatusConstants.EntitlementEnded, FileReviewStatus.Approved, "Active", false, "انتهى الاستحقاق")]
    public async Task SalaryStatus_ReadsStoredHoldsWithoutDependingOnLoadedNavigation(
        int status, FileReviewStatus review, string? holdStatus, bool deleted, string expected)
    {
        var unit = new Mock<IUnitOfWork>();
        var file = new RetirementFile { FileID = 1, PensionBranchID = 1, StatusID = status, ReviewStatus = review };
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>([file]));
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        var holds = new Mock<IRepository<PensionSuspension>>();
        holds.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PensionSuspension>(holdStatus == null ? [] :
            [new PensionSuspension { FileID = 1, Status = holdStatus, IsDeleted = deleted }]));
        unit.Setup(u => u.PensionSuspensions).Returns(holds.Object);
        var service = new PensionSuspensionService(unit.Object, Mock.Of<INotificationService>(), TestPensionAccess.All(unit.Object));

        var result = await service.GetSalaryStatusAsync(1);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(expected, result.Data);
        var payment = await service.CheckPaymentHoldAsync(1);
        Assert.True(payment.IsSuccess, payment.Message);
        Assert.Equal(expected, payment.Data!.SalaryStatus);
        Assert.Equal(expected != "قابل للصرف", payment.Data.IsHeld);
        if (payment.Data.IsHeld) Assert.False(string.IsNullOrWhiteSpace(payment.Data.Reason));
        else Assert.Null(payment.Data.Reason);
        Assert.Empty(file.PensionSuspensions);
        unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task SalaryStatus_DoesNotDiscloseForeignFileState()
    {
        var unit = new Mock<IUnitOfWork>();
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>([
            new RetirementFile { FileID = 1, PensionBranchID = 2, StatusID = FileStatusConstants.Active }
        ]));
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        var holds = new Mock<IRepository<PensionSuspension>>();
        holds.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PensionSuspension>([]));
        unit.Setup(u => u.PensionSuspensions).Returns(holds.Object);
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var service = new PensionSuspensionService(unit.Object, Mock.Of<INotificationService>(),
            TestPensionAccess.From(unit.Object, identity.Object));
        Assert.False((await service.GetSalaryStatusAsync(1)).IsSuccess);
        Assert.False((await service.CheckPaymentHoldAsync(1)).IsSuccess);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PaymentHold_MissingOrDeletedFileCannotAllowPayment(bool deleted)
    {
        var unit = new Mock<IUnitOfWork>();
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>(deleted
            ? [new RetirementFile { FileID = 1, StatusID = FileStatusConstants.Active,
                ReviewStatus = FileReviewStatus.Approved, IsDeleted = true }] : []));
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        var holds = new Mock<IRepository<PensionSuspension>>();
        holds.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PensionSuspension>([]));
        unit.Setup(u => u.PensionSuspensions).Returns(holds.Object);
        var service = new PensionSuspensionService(unit.Object, Mock.Of<INotificationService>(), TestPensionAccess.All(unit.Object));
        var result = await service.CheckPaymentHoldAsync(1);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }
}
