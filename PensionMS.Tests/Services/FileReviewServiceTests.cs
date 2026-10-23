using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Tests.Services;

public class FileReviewServiceTests
{
    [Fact]
    public async Task DraftPreparation_RemainsNewUntilExplicitSubmission()
    {
        var f = new RetirementLifecycleFixture();
        f.File.StatusID = FileStatusConstants.New;
        f.File.ReviewStatus = FileReviewStatus.None;
        Assert.True((await f.Service.RequestReviewAsync(1, 7)).IsSuccess);
        Assert.Equal(FileStatusConstants.New, f.File.StatusID);
        Assert.Empty(f.Holds);
        Assert.True((await f.Service.SubmitReviewAsync(1, 7)).IsSuccess);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        Assert.Equal(FileReviewStatus.PendingReview, f.File.ReviewStatus);
        Assert.Single(f.Holds);
        await f.Service.SubmitReviewAsync(1, 7);
        Assert.Single(f.Holds);
    }

    [Fact]
    public async Task Approval_ActivatesAndEndsOnlyReviewHold()
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        var review = new PensionSuspension { FileID = 1, SuspensionSource = RetirementReviewConstants.SuspensionSource, Status = SuspensionStatusConstants.Active };
        var independent = new PensionSuspension { FileID = 1, SuspensionSource = "إداري", Status = SuspensionStatusConstants.Active };
        f.Holds.AddRange([review, independent]);
        var result = await f.Service.ApproveReviewAsync(1, 7);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
        Assert.Equal(FileReviewStatus.Approved, f.File.ReviewStatus);
        Assert.Equal(SuspensionStatusConstants.Lifted, review.Status);
        Assert.Equal(SuspensionStatusConstants.Active, independent.Status);
        Assert.NotNull(f.File.LastReviewedFileState);
    }

    [Fact]
    public async Task Rejection_KeepsFilePendingAndReviewHoldActive()
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        f.Holds.Add(new PensionSuspension { FileID = 1, SuspensionSource = RetirementReviewConstants.SuspensionSource, Status = SuspensionStatusConstants.Active });
        Assert.True((await f.Service.RejectReviewAsync(1, 7, "تصحيح البيانات")).IsSuccess);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        Assert.Equal(FileReviewStatus.Rejected, f.File.ReviewStatus);
        Assert.Equal(SuspensionStatusConstants.Active, f.Holds[0].Status);
    }

    [Fact]
    public async Task HistoricalRejection_DoesNotRejectCurrentCycle()
    {
        var f = new RetirementLifecycleFixture();
        var old = f.AddItem(FileReviewItemStatusConstants.Rejected, historical: true);
        f.AddItem();
        Assert.True((await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
        Assert.Equal(FileReviewItemStatusConstants.Rejected, old.Status);
    }

    [Fact]
    public async Task EndedFile_CorrectionDoesNotReinstateEntitlement()
    {
        var f = new RetirementLifecycleFixture();
        f.File.PreviousStatusID = FileStatusConstants.EntitlementEnded;
        f.AddItem();
        Assert.True((await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
        Assert.Equal(FileStatusConstants.EntitlementEnded, f.File.StatusID);
    }

    [Fact]
    public async Task Reinstatement_ReusesSameFileAndRequiresApproval()
    {
        var f = new RetirementLifecycleFixture();
        f.File.StatusID = FileStatusConstants.EntitlementEnded;
        f.File.ReviewStatus = FileReviewStatus.Approved;
        Assert.True((await f.Service.ReinstateEntitlementAsync(1, 7)).IsSuccess);
        Assert.Equal(1, f.File.FileID);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        Assert.True((await f.Service.ApproveReviewAsync(1, 8)).IsSuccess);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Approval_AllowsTheSameDataEditor(bool perItem)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        item.AuditLog = new AuditLog { UserID = 7, Action = "ReviewDataChange" };
        var result = perItem ? await f.Service.ApproveItemAsync(item.ItemID, 7) : await f.Service.ApproveReviewAsync(1, 7);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(FileReviewItemStatusConstants.Approved, item.Status);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Approval_RequiresPaymentPrerequisites(bool missingBook)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        if (missingBook) f.Books.Clear(); else f.Entitlements.Clear();
        Assert.False((await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
        Assert.Equal(FileReviewItemStatusConstants.Pending, f.Items[0].Status);
    }

    [Fact]
    public async Task PartialApproval_DoesNotActivateBeforeAllItemsResolved()
    {
        var f = new RetirementLifecycleFixture();
        var first = f.AddItem();
        f.AddItem();
        Assert.True((await f.Service.ApproveItemAsync(first.ItemID, 7)).IsSuccess);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        Assert.Equal(FileReviewStatus.PendingReview, f.File.ReviewStatus);
    }
}
