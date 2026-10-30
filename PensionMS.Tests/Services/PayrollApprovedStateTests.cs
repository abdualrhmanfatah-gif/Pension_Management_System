using System.Text.Json.Nodes;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Entities;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class PayrollApprovedStateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ApprovedState_IsTheOnlyCalculationSource_EvenDuringReview(bool pending)
    {
        var f = new RetirementLifecycleFixture();
        await SaveApprovedState(f);
        var stored = f.File.LastReviewedFileState;
        f.File.StatusID = pending ? FileStatusConstants.Pending : FileStatusConstants.Active;
        f.File.ReviewStatus = pending ? FileReviewStatus.PendingReview : FileReviewStatus.Approved;
        f.Entitlements[0].Amount = 9000;
        f.Books[0].Status = PaymentBookStatus.Cancelled;
        f.File.Retiree.IsAlive = false;
        f.File.Retiree.FullName = "تعديل معلق";
        f.Unit.Object.SalaryRecipients.Query().Single().IsActive = false;
        f.Unit.Invocations.Clear();

        var result = await Generate(f);

        Assert.Empty(result.SkippedItems);
        var item = Assert.Single(result.GeneratedItems);
        Assert.Equal(1000m, item.Amount);
        Assert.Equal(6, item.PaymentBookID);
        Assert.Equal(3, item.RecipientID);
        Assert.Equal(TransactionStatusConstants.PendingSend, item.StatusID);
        Assert.Equal(stored, f.File.LastReviewedFileState);
        Assert.False(f.File.Retiree.IsAlive);
        Assert.Equal("تعديل معلق", f.File.Retiree.FullName);
        VerifyReadOnlySnapshotCalculation(f);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{\"Version\":2}")]
    [InlineData("{\"Version\":\"1\"}")]
    [InlineData("{\"Version\":null}")]
    [InlineData("{\"Version\":1.5}")]
    [InlineData("{\"Version\":1,\"Retiree\":[]}")]
    public async Task UnreadableState_SkipsOnlyThisFile_WithoutLiveFallback(string json)
    {
        var f = new RetirementLifecycleFixture();
        // Even a first-review file must not silently replace a stored, unreadable version.
        f.File.PreviousStatusID = FileStatusConstants.New;
        f.File.LastReviewedFileState = json;

        var result = await Generate(f);

        Assert.Empty(result.GeneratedItems);
        Assert.Contains("تعذر قراءة", Assert.Single(result.SkippedItems).Reason);
        Assert.Equal(json, f.File.LastReviewedFileState);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        VerifyReadOnlySnapshotCalculation(f);
    }

    [Theory]
    [InlineData("Retiree")]
    [InlineData("Entitlements")]
    [InlineData("Books")]
    [InlineData("Recipients")]
    [InlineData("EntitledPersons")]
    [InlineData("GuardianLinks")]
    public async Task NullSection_SkipsInsteadOfThrowingOrUsingLiveData(string section)
    {
        var f = new RetirementLifecycleFixture();
        await SaveApprovedState(f);
        var state = JsonNode.Parse(f.File.LastReviewedFileState!)!;
        state[section] = null;
        f.File.LastReviewedFileState = state.ToJsonString();

        var result = await Generate(f);

        Assert.Empty(result.GeneratedItems);
        Assert.Contains("غير مكتملة", Assert.Single(result.SkippedItems).Reason);
        VerifyReadOnlySnapshotCalculation(f);
    }

    [Theory]
    [InlineData("Retiree")]
    [InlineData("Entitlements")]
    [InlineData("Books")]
    [InlineData("Recipients")]
    [InlineData("EntitledPersons")]
    [InlineData("GuardianLinks")]
    public async Task MissingSection_IsNotReplacedByADefaultObjectOrEmptyList(string section)
    {
        var f = new RetirementLifecycleFixture();
        await SaveApprovedState(f);
        var state = JsonNode.Parse(f.File.LastReviewedFileState!)!.AsObject();
        state.Remove(section);
        f.File.LastReviewedFileState = state.ToJsonString();

        var result = await Generate(f);

        Assert.Empty(result.GeneratedItems);
        Assert.Contains("تعذر قراءة", Assert.Single(result.SkippedItems).Reason);
        VerifyReadOnlySnapshotCalculation(f);
    }

    [Theory]
    [InlineData("Retiree")]
    [InlineData("Entitlements")]
    [InlineData("Books")]
    [InlineData("Recipients")]
    [InlineData("EntitledPersons")]
    [InlineData("GuardianLinks")]
    public async Task ForeignOwnerOrNullRow_SkipsWholeFile(string section)
    {
        var f = new RetirementLifecycleFixture();
        await SaveApprovedState(f);
        var state = JsonNode.Parse(f.File.LastReviewedFileState!)!;
        if (section == "Retiree") state[section]!["RetireeID"] = 99;
        else if (section == "Entitlements") state[section]![0]!["RetireeID"] = 99;
        else if (section is "Books" or "Recipients") state[section]![0]!["FileID"] = 99;
        else state[section] = JsonNode.Parse("[null]");
        f.File.LastReviewedFileState = state.ToJsonString();

        var result = await Generate(f);

        Assert.Empty(result.GeneratedItems);
        Assert.Contains("لا تطابق", Assert.Single(result.SkippedItems).Reason);
        VerifyReadOnlySnapshotCalculation(f);
    }

    [Theory]
    [InlineData(FileStatusConstants.Active, FileStatusConstants.New, FileReviewStatus.Approved)]
    [InlineData(FileStatusConstants.Pending, FileStatusConstants.Active, FileReviewStatus.PendingReview)]
    [InlineData(FileStatusConstants.Pending, FileStatusConstants.New, FileReviewStatus.Rejected)]
    public async Task MissingApprovedState_OutsideFirstReview_SkipsFile(int status, int previous, FileReviewStatus review)
    {
        var f = new RetirementLifecycleFixture();
        f.File.StatusID = status;
        f.File.PreviousStatusID = previous;
        f.File.ReviewStatus = review;

        var result = await Generate(f);

        Assert.Empty(result.GeneratedItems);
        Assert.Contains("لا توجد نسخة", Assert.Single(result.SkippedItems).Reason);
        VerifyReadOnlySnapshotCalculation(f);
    }

    [Fact]
    public async Task FirstReview_ProducesOnlyAnUnsavedDraft_WithoutInventingAnApproval()
    {
        var f = new RetirementLifecycleFixture();
        f.File.PreviousStatusID = FileStatusConstants.New;

        var result = await Generate(f);

        Assert.Empty(result.SkippedItems);
        var item = Assert.Single(result.GeneratedItems);
        Assert.Equal(1000m, item.Amount);
        Assert.Equal(TransactionStatusConstants.PendingSend, item.StatusID);
        Assert.Null(f.File.LastReviewedFileState);
        Assert.Equal(FileReviewStatus.PendingReview, f.File.ReviewStatus);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task OneCorruptFile_DoesNotBlockOtherFilesInTheBatch()
    {
        var f = new RetirementLifecycleFixture();
        var state = await ApprovedPensionSnapshot.CaptureAsync(f.Unit.Object, 1, 2);
        state.MarkReviewed();
        foreach (var book in state.Books) book.FileID = 10;
        foreach (var recipient in state.Recipients) recipient.FileID = 10;
        var valid = new RetirementFile
        {
            FileID = 10, FileNumber = "VALID", RetireeID = 2, Retiree = f.File.Retiree,
            StatusID = FileStatusConstants.Active, ReviewStatus = FileReviewStatus.Approved,
            LastReviewedFileState = state.Serialize()
        };
        f.File.LastReviewedFileState = "{";
        Mock.Get(f.Unit.Object.RetirementFiles).Setup(r => r.Query())
            .Returns(new TestAsyncEnumerable<RetirementFile>([f.File, valid]));
        f.Unit.Invocations.Clear();

        var result = await Generate(f);

        Assert.Equal(f.File.FileNumber, Assert.Single(result.SkippedItems).FileNumber);
        Assert.Equal(valid.FileID, Assert.Single(result.GeneratedItems).FileID);
        VerifyReadOnlySnapshotCalculation(f);
    }

    [Fact]
    public async Task Approval_ReplacesTheSourceForSubsequentGeneration()
    {
        var f = new RetirementLifecycleFixture();
        await SaveApprovedState(f);
        f.Entitlements[0].Amount = 2000;
        Assert.Equal(1000m, Assert.Single((await Generate(f)).GeneratedItems).Amount);
        f.AddItem();

        var approval = await f.Service.ApproveReviewAsync(1, 7);

        Assert.True(approval.IsSuccess, approval.Message);
        Assert.Equal(2000m, Assert.Single((await Generate(f)).GeneratedItems).Amount);
    }

    private static async Task SaveApprovedState(RetirementLifecycleFixture f)
    {
        var state = await ApprovedPensionSnapshot.CaptureAsync(f.Unit.Object, f.File.FileID, f.File.RetireeID);
        state.MarkReviewed();
        f.File.LastReviewedFileState = state.Serialize();
        // Verify generation itself does not read mutable financial tables.
        f.Unit.Invocations.Clear();
    }

    private static Task<PayrollGenerationResult> Generate(RetirementLifecycleFixture f) =>
        new PayrollGenerationService(f.Unit.Object).GenerateForBatchAsync(
            new SalaryPaymentBatch { BatchID = 1, BatchMonth = YemenTime.TodayDateTime }, DateTime.UtcNow, 7);

    private static void VerifyReadOnlySnapshotCalculation(RetirementLifecycleFixture f)
    {
        f.Unit.VerifyGet(u => u.FinancialEntitlements, Times.Never);
        f.Unit.VerifyGet(u => u.SalaryRecipients, Times.Never);
        f.Unit.VerifyGet(u => u.PaymentBooks, Times.Never);
        f.Unit.VerifyGet(u => u.EntitledPersons, Times.Never);
        f.Unit.VerifyGet(u => u.GuardianEntitledPersons, Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }
}
