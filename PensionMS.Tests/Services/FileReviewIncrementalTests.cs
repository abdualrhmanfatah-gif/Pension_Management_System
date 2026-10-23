using Moq;
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
