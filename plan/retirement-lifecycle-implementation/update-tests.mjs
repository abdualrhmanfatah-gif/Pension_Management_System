import fs from 'node:fs';
const root='F:/Projects/Pension_Management_System/';
function write(path,code){const s=fs.readFileSync(root+path,'utf8'),backup=root+'plan/retirement-lifecycle-implementation/before/'+path;if(!fs.existsSync(backup)){fs.mkdirSync(backup.slice(0,backup.lastIndexOf('/')),{recursive:true});fs.writeFileSync(backup,s);}fs.writeFileSync(root+path,code.replace(/\r?\n/g,'\r\n'));}
write('PensionMS.Tests/Services/FileReviewServiceTests.cs',`using Moq;
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
    public async Task Approval_RespectsSeparationOfDuties(bool perItem)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        f.Sod.Setup(s => s.ValidateAsync(1, 7, SoDAction.Approve)).ReturnsAsync(SoDResult.Fail("فصل المهام"));
        var result = perItem ? await f.Service.ApproveItemAsync(item.ItemID, 7) : await f.Service.ApproveReviewAsync(1, 7);
        Assert.False(result.IsSuccess);
        Assert.Equal(FileReviewItemStatusConstants.Pending, item.Status);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
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
`);
write('PensionMS.Tests/Services/FileReviewIncrementalTests.cs',`using Moq;
using PensionMS.Core.Constants;

namespace PensionMS.Tests.Services;

public class FileReviewIncrementalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Review_RespectsTransactionOwnership(bool callerOwnsTransaction)
    {
        var f = new RetirementLifecycleFixture();
        f.Unit.SetupGet(u => u.HasActiveTransaction).Returns(callerOwnsTransaction);
        await f.Service.RequestReviewAsync(1, 7);
        f.Unit.Verify(u => u.LockFileReviewAsync(1), Times.Once);
        f.Unit.Verify(u => u.BeginTransactionAsync(), callerOwnsTransaction ? Times.Never() : Times.Once());
        f.Unit.Verify(u => u.CommitAsync(), callerOwnsTransaction ? Times.Never() : Times.Once());
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Theory]
    [InlineData("submit")]
    [InlineData("approve")]
    [InlineData("reject")]
    [InlineData("approve-item")]
    [InlineData("reject-item")]
    public async Task ReviewMutations_WaitForFileLockBeforeChangingState(string operation)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Unit.Setup(u => u.LockFileReviewAsync(1)).Returns(gate.Task);
        Task action = operation switch {
            "submit" => f.Service.SubmitReviewAsync(1, 7),
            "approve" => f.Service.ApproveReviewAsync(1, 7),
            "reject" => f.Service.RejectReviewAsync(1, 7, "سبب"),
            "approve-item" => f.Service.ApproveItemAsync(1, 7),
            _ => f.Service.RejectItemAsync(1, 7, "سبب")
        };
        Assert.False(action.IsCompleted);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        gate.SetException(new InvalidOperationException("test"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => action);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task DecisionOnHistoricalItem_IsRejectedWithoutChangingHistory()
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem(historical: true);
        Assert.False((await f.Service.ApproveItemAsync(item.ItemID, 7)).IsSuccess);
        Assert.Equal(FileReviewItemStatusConstants.Pending, item.Status);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
`);
write('PensionMS.Tests/Services/FileStatusServiceTests.cs',`using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.Files;

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

    private static FileStatusService Create(RetirementLifecycleFixture f) => new(f.Unit.Object, f.Sod.Object,
        Mock.Of<IWorkflowEngine>(), Mock.Of<IRiskScoringService>(), Mock.Of<IImmutableAuditService>(),
        Mock.Of<ISnapshotService>(), Mock.Of<INotificationService>(), f.Service);
}
`);
