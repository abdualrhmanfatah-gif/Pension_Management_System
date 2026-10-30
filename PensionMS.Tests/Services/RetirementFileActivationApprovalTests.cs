using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Time;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class RetirementFileActivationApprovalTests
{
    private const string FinancialError = "لا يمكن اعتماد التفعيل دون استحقاق مالي سارٍ";
    private const string DirectError = "المتقاعد غير مؤهل للاستلام المباشر";
    private const string EntitledError = "يجب استكمال مستلمي حصص المستحقين ومجموع النسب 100٪";
    private const string DocumentError = "يجب تسوية الوثائق الناقصة بوثائق أو إعفاءات أو تعهدات سارية قبل الاعتماد";

    public static IEnumerable<object[]> Rejections()
    {
        foreach (var perItem in new[] { false, true })
        foreach (var scenario in new[] { "financial", "direct-foreign", "direct-deceased", "missing-person", "foreign-person",
            "inactive-person", "deceased-person", "deleted-person", "missing-recipient", "duplicate", "null-share",
            "zero-share", "negative-share", "share-over-100", "total-below", "total-above", "mixed-recipients", "documents" })
            yield return [perItem, scenario];
    }

    [Theory]
    [MemberData(nameof(Rejections))]
    public async Task Approval_PreservesRejectionPrecedenceAndLeavesReviewUnchanged(bool perItem, string scenario)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        var people = new List<EntitledPerson> { new() { EntitledID = 7, FileID = 1, IsActive = true } };
        var recipients = new List<SalaryRecipient> { new() { FileID = 1, EntitledID = 7, IsActive = true, SharePercent = 100 } };
        f.RequiredDocuments.Add(new() { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 });
        var expected = EntitledError;
        if (scenario == "financial")
        {
            f.Entitlements.Clear();
            recipients[0].RetireeID = 99;
            expected = FinancialError;
        }
        if (scenario.StartsWith("direct-"))
        {
            recipients[0].RetireeID = scenario == "direct-foreign" ? 99 : 2;
            f.File.Retiree.IsAlive = scenario != "direct-deceased";
            people.Clear();
            expected = DirectError;
        }
        if (scenario == "missing-person") people.Clear();
        if (scenario == "foreign-person") people[0].FileID = 2;
        if (scenario == "inactive-person") people[0].IsActive = false;
        if (scenario == "deceased-person") people[0].IsDeceased = true;
        if (scenario == "deleted-person") people[0].IsDeleted = true;
        if (scenario == "missing-recipient") people.Add(new() { EntitledID = 8, FileID = 1, IsActive = true });
        if (scenario == "duplicate")
        {
            recipients[0].SharePercent = 50;
            recipients.Add(new() { FileID = 1, EntitledID = 7, IsActive = true, SharePercent = 50 });
        }
        if (scenario == "null-share") recipients[0].SharePercent = null;
        if (scenario == "zero-share") recipients[0].SharePercent = 0;
        if (scenario == "negative-share") recipients[0].SharePercent = -1;
        if (scenario == "share-over-100") recipients[0].SharePercent = 101;
        if (scenario is "total-below" or "total-above")
        {
            people.Add(new() { EntitledID = 8, FileID = 1, IsActive = true });
            recipients[0].SharePercent = 50;
            recipients.Add(new() { FileID = 1, EntitledID = 8, IsActive = true,
                SharePercent = scenario == "total-below" ? 49 : 51 });
        }
        if (scenario == "mixed-recipients") recipients.Add(new() { FileID = 1, RetireeID = 2, IsActive = true });
        if (scenario == "documents") expected = DocumentError;
        BindRecipients(f, recipients);
        Mock.Get(f.Unit.Object.EntitledPersons).Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<EntitledPerson>(people));

        var result = await Approve(f, item.ItemID, perItem);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected, result.Message);
        Assert.Equal(FileReviewItemStatusConstants.Pending, item.Status);
        Assert.Null(item.ReviewedAt);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        Assert.Equal(FileReviewStatus.PendingReview, f.File.ReviewStatus);
        Assert.Null(f.File.LastReviewedFileState);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        if (expected != DocumentError) Mock.Get(f.Unit.Object.Documents).Verify(r => r.Query(), Times.Never);
        if (expected == FinancialError) Mock.Get(f.Unit.Object.SalaryRecipients).Verify(r => r.Query(), Times.Never);
        if (expected is FinancialError or DirectError) Mock.Get(f.Unit.Object.EntitledPersons).Verify(r => r.Query(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Approval_SucceedsWithoutBooksOrRecipientsWhenOtherPrerequisitesAreSatisfied(bool perItem)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        f.Books.Clear();
        BindRecipients(f, []);
        f.RequiredDocuments.Add(new() { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 });
        f.Documents.Add(new() { FileID = 1, DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 });

        var result = await Approve(f, item.ItemID, perItem);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
        Assert.Equal(FileReviewStatus.Approved, f.File.ReviewStatus);
        Assert.Equal(FileReviewItemStatusConstants.Approved, item.Status);
        Assert.NotNull(f.File.LastReviewedFileState);
    }

    [Theory]
    [InlineData(false, "entitled")]
    [InlineData(true, "entitled")]
    [InlineData(false, "both-owner-fields")]
    [InlineData(true, "both-owner-fields")]
    [InlineData(false, "no-entitled-fields")]
    [InlineData(true, "no-entitled-fields")]
    public async Task Approval_PreservesExistingRecipientChecksWithoutAddingRestrictions(bool perItem, string scenario)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        var people = new[] { new EntitledPerson { FileID = 1, EntitledID = 7, IsActive = true } };
        Mock.Get(f.Unit.Object.EntitledPersons).Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<EntitledPerson>(people));
        BindRecipients(f, [new SalaryRecipient { FileID = 1, IsActive = true,
            EntitledID = scenario == "no-entitled-fields" ? null : 7,
            RetireeID = scenario == "both-owner-fields" ? 2 : null,
            SharePercent = scenario == "no-entitled-fields" ? null : 100 }]);
        var result = await Approve(f, item.ItemID, perItem);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
    }

    [Fact]
    public async Task PartialItemApproval_StillWaitsForRemainingItemsWithoutBooksOrRecipients()
    {
        var f = new RetirementLifecycleFixture();
        var first = f.AddItem();
        var second = f.AddItem();
        f.Books.Clear();
        BindRecipients(f, []);
        Assert.True((await f.Service.ApproveItemAsync(first.ItemID, 7)).IsSuccess);
        Assert.Equal(FileReviewItemStatusConstants.Approved, first.Status);
        Assert.Equal(FileReviewItemStatusConstants.Pending, second.Status);
        Assert.Equal(FileStatusConstants.Pending, f.File.StatusID);
        Assert.Equal(FileReviewStatus.PendingReview, f.File.ReviewStatus);
        Assert.Null(f.File.LastReviewedFileState);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task EntitlementEnded_BypassesActivationFailuresForBothApprovalPaths(bool perItem, bool endedByDate)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        if (endedByDate) f.Entitlements[0].EndDate = YemenTime.TodayDateTime.AddDays(-1);
        else
        {
            f.File.PreviousStatusID = FileStatusConstants.EntitlementEnded;
            f.Entitlements.Clear();
        }
        BindRecipients(f, [new SalaryRecipient { FileID = 1, IsActive = true, RetireeID = 99 }]);
        f.RequiredDocuments.Add(new() { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 });
        var result = await Approve(f, item.ItemID, perItem);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(FileStatusConstants.EntitlementEnded, f.File.StatusID);
        Assert.Equal(FileReviewStatus.Approved, f.File.ReviewStatus);
        Assert.Equal(FileReviewItemStatusConstants.Approved, item.Status);
    }

    private static Task<ApiResponse<bool>> Approve(RetirementLifecycleFixture f, long itemId, bool perItem) =>
        perItem ? f.Service.ApproveItemAsync(itemId, 7) : f.Service.ApproveReviewAsync(1, 7);

    private static void BindRecipients(RetirementLifecycleFixture f, List<SalaryRecipient> recipients) =>
        Mock.Get(f.Unit.Object.SalaryRecipients).Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<SalaryRecipient>(recipients));
}
