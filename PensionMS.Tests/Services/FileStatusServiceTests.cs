using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;

namespace PensionMS.Tests.Services;

public class FileStatusServiceTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task DirectActivationAndLegacyStates_CannotBypassReview(int statusId)
    {
        var f = new RetirementLifecycleFixture();
        var service = Create(f);
        Assert.False((await service.ChangeStatusAsync(1, statusId, "تغيير", 7)).IsSuccess);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EndingEntitlement_IsRejectedWhileEntitlementsRemain()
    {
        var f = new RetirementLifecycleFixture();
        f.File.StatusID = FileStatusConstants.Active;
        f.File.ReviewStatus = Core.Entities.FileReviewStatus.Approved;
        Assert.False((await Create(f).ChangeStatusAsync(1, FileStatusConstants.EntitlementEnded, "سبب", 7)).IsSuccess);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
    }

    private static FileStatusService Create(RetirementLifecycleFixture f) => new(f.Unit.Object,
        Mock.Of<IImmutableAuditService>(), Mock.Of<INotificationService>(), f.Service, PensionMS.Testing.TestPensionAccess.All(f.Unit.Object));
}
