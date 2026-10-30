using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Entities;
using static PensionMS.Core.Rules.RetirementFiles.RetirementFileActivationRules;

namespace PensionMS.Tests.Rules;

public class RetirementFileActivationRulesTests
{
    private static readonly DateOnly EvaluationDate = new(2026, 10, 25);

    [Theory]
    [InlineData(false, RejectionReason.MissingEffectiveFinancialEntitlement)]
    [InlineData(true, RejectionReason.None)]
    public void FinancialPrerequisite_UsesTheExistingExistenceFact(bool exists, RejectionReason expected)
    {
        AssertDecision(EvaluateFinancialPrerequisite(exists), expected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NoActiveRecipients_ArePermittedEvenWhenActiveEntitledPersonsExist(bool retireeAlive)
    {
        AssertDecision(EvaluateRecipients(2, retireeAlive, [], [7, 8]), RejectionReason.None);
    }

    [Theory]
    [InlineData(2, true, RejectionReason.None)]
    [InlineData(2, false, RejectionReason.IneligibleDirectRetireeRecipient)]
    [InlineData(99, true, RejectionReason.IneligibleDirectRetireeRecipient)]
    [InlineData(99, false, RejectionReason.IneligibleDirectRetireeRecipient)]
    public void DirectRetireeRecipients_MustMatchAndBeAlive(int recipientRetireeId, bool alive, RejectionReason expected)
    {
        AssertDecision(EvaluateRecipients(2, alive, [new(recipientRetireeId, null, null)]), expected);
    }

    [Fact]
    public void NoEntitledRecipients_DoesNotIntroduceShareOrDuplicateRestrictions()
    {
        AssertDecision(EvaluateRecipients(2, true, [new(2, null, -1), new(2, null, 101)]), RejectionReason.None);
        AssertDecision(EvaluateRecipients(2, false, [new(null, null, null)]), RejectionReason.None);
    }

    [Fact]
    public void EntitledRecipients_RequestSelectedPersonFactsBeforeACompletionDecision()
    {
        var pending = EvaluateRecipients(2, false, [new(null, 7, 100)]);
        Assert.Equal(RejectionReason.None, pending.Reason);
        Assert.True(pending.RequiresEntitledPersonFacts);
        Assert.False(pending.IsAllowed);
        AssertDecision(EvaluateRecipients(2, false, [new(null, 7, 100)], []), RejectionReason.InvalidEntitledRecipients);
    }

    [Fact]
    public void DirectRetireeRejection_PrecedesEntitledLoadingAndShareFailures()
    {
        var recipients = new[] { new RecipientFacts(99, 7, 0), new RecipientFacts(null, null, null) };
        AssertDecision(EvaluateRecipients(2, true, recipients), RejectionReason.IneligibleDirectRetireeRecipient);
        AssertDecision(EvaluateRecipients(2, true, recipients, []), RejectionReason.IneligibleDirectRetireeRecipient);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("inactive")]
    [InlineData("deceased")]
    [InlineData("deleted")]
    public void RecipientsWhosePeopleAreExcludedByTheExistingSelection_AreRejected(string excludedBy)
    {
        // Each query exclusion produces the same materialized fact: the recipient's ID is absent.
        Assert.NotEmpty(excludedBy);
        AssertDecision(EvaluateRecipients(2, false, [new(null, 7, 100)], [8]), RejectionReason.InvalidEntitledRecipients);
    }

    [Fact]
    public void EverySelectedPersonMustHaveOneRecipient_AndMixedCombinationsAreRejected()
    {
        AssertDecision(EvaluateRecipients(2, false, [new(null, 7, 100)], [7, 8]), RejectionReason.InvalidEntitledRecipients);
        AssertDecision(EvaluateRecipients(2, false, [new(null, 7, 50), new(null, 7, 50)], [7]), RejectionReason.InvalidEntitledRecipients);
        AssertDecision(EvaluateRecipients(2, true, [new(null, 7, 100), new(2, null, null)], [7]), RejectionReason.InvalidEntitledRecipients);
        AssertDecision(EvaluateRecipients(2, true, [new(null, 7, 100), new(null, null, null)], [7]), RejectionReason.InvalidEntitledRecipients);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void EntitledShares_MustBePresentPositiveAndAtMost100(int? share)
    {
        AssertDecision(EvaluateRecipients(2, false, [new(null, 7, share)], [7]), RejectionReason.InvalidEntitledRecipients);
    }

    [Theory]
    [InlineData(49, RejectionReason.InvalidEntitledRecipients)]
    [InlineData(50, RejectionReason.None)]
    [InlineData(51, RejectionReason.InvalidEntitledRecipients)]
    public void EntitledShares_MustTotalExactly100(int secondShare, RejectionReason expected)
    {
        AssertDecision(EvaluateRecipients(2, false, [new(null, 7, 50), new(null, 8, secondShare)], [7, 8]), expected);
    }

    [Fact]
    public void ValidRecipients_PreserveDecimalSharesAndInputCollections()
    {
        var recipients = new List<RecipientFacts> { new(null, 7, 33.3333m), new(null, 8, 66.6667m) };
        var ids = new List<int> { 8, 7 };
        var before = recipients.ToArray();
        AssertDecision(EvaluateRecipients(2, false, recipients, ids), RejectionReason.None);
        Assert.Equal(before, recipients);
        Assert.Equal(new[] { 8, 7 }, ids);
    }

    [Fact]
    public void ExistingCombinedOwnerFields_AreNotNewlyRejected()
    {
        AssertDecision(EvaluateRecipients(2, true, [new(2, 7, 100)], [7]), RejectionReason.None);
    }

    [Theory]
    [InlineData("valid", RejectionReason.None)]
    [InlineData("expires-today", RejectionReason.None)]
    [InlineData("pending-review", RejectionReason.None)]
    [InlineData("missing", RejectionReason.UnsatisfiedDocuments)]
    [InlineData("expired", RejectionReason.UnsatisfiedDocuments)]
    [InlineData("rejected", RejectionReason.UnsatisfiedDocuments)]
    [InlineData("deleted", RejectionReason.UnsatisfiedDocuments)]
    [InlineData("wrong-owner", RejectionReason.UnsatisfiedDocuments)]
    [InlineData("mixed-owner", RejectionReason.UnsatisfiedDocuments)]
    [InlineData("wrong-type", RejectionReason.UnsatisfiedDocuments)]
    public void Evidence_MustBeValidAndForTheExactRequiredOwner(string scenario, RejectionReason expected)
    {
        var requirement = new AutoDetectedMissingDocumentDto { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 };
        var proof = new Document { FileID = 1, DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 };
        if (scenario == "expires-today") proof.ExpiryDate = EvaluationDate;
        if (scenario == "expired") proof.ExpiryDate = EvaluationDate.AddDays(-1);
        if (scenario == "rejected") proof.PendingReviewStatus = PendingReviewStatus.Rejected;
        if (scenario == "pending-review") proof.PendingReviewStatus = PendingReviewStatus.Pending;
        if (scenario == "deleted") proof.IsDeleted = true;
        if (scenario == "wrong-owner") proof.AgentID = 9;
        if (scenario == "mixed-owner") proof.EntitledID = 7;
        if (scenario == "wrong-type") proof.DocumentTypeID = DocumentTypeConstants.AgencyDocument;
        AssertDecision(EvaluateDocuments(Facts([requirement], documents: scenario == "missing" ? [] : [proof]), EvaluationDate), expected);
    }

    [Theory]
    [InlineData("file")]
    [InlineData("retiree")]
    [InlineData("entitled")]
    [InlineData("guardian")]
    [InlineData("pledge")]
    [InlineData("exemption")]
    public void Evidence_UsesExactAssociationsForEveryOtherOwner(string owner)
    {
        var r = new AutoDetectedMissingDocumentDto { DocumentTypeID = DocumentTypeConstants.NationalId };
        var d = new Document { DocumentTypeID = r.DocumentTypeID };
        if (owner == "retiree") { r.RetireeID = 2; d.RetireeID = 2; }
        if (owner == "entitled") { r.EntitledID = 7; d.EntitledID = 7; }
        if (owner == "guardian") { r.GuardianID = 9; d.GuardianID = 9; }
        if (owner == "pledge") { r.PledgeID = 10; d.PledgeID = 10; }
        if (owner == "exemption") { r.ExemptionID = 11; d.ExemptionID = 11; }
        AssertDecision(EvaluateDocuments(Facts([r], documents: [d]), EvaluationDate), RejectionReason.None);
        d.AgentID = 99;
        AssertDecision(EvaluateDocuments(Facts([r], documents: [d]), EvaluationDate), RejectionReason.UnsatisfiedDocuments);
    }

    [Fact]
    public void Evidence_UsesValidAlternativeWithoutOverwritingRejectedHistory()
    {
        var r = new AutoDetectedMissingDocumentDto { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 };
        var rejected = new Document { DocumentTypeID = r.DocumentTypeID, AgentID = 8, PendingReviewStatus = PendingReviewStatus.Rejected };
        var valid = new Document { DocumentTypeID = r.DocumentTypeID, AgentID = 8 };
        AssertDecision(EvaluateDocuments(Facts([r], documents: [rejected, valid]), EvaluationDate), RejectionReason.None);
        Assert.Equal(PendingReviewStatus.Rejected, rejected.PendingReviewStatus);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidCoverage_SatisfiesRequirementWithoutCountingAsUploadedEvidence(bool exemption)
    {
        var r = new AutoDetectedMissingDocumentDto { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 };
        var missing = new MissingDocument { FileID = 1, AgentID = 8, DocumentTypeID = r.DocumentTypeID,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null };
        var rows = new[] { missing };
        var automatic = new[] { r };
        var facts = Facts(automatic, rows);
        var before = (missing.Status, missing.PledgeID, missing.ExemptionID, missing.DocumentTypeID, missing.AgentID);
        AssertDecision(EvaluateDocuments(facts, EvaluationDate), RejectionReason.None);
        Assert.Empty(facts.UploadedDocuments);
        Assert.Same(r, Assert.Single(automatic));
        Assert.Same(missing, Assert.Single(rows));
        Assert.Equal(before, (missing.Status, missing.PledgeID, missing.ExemptionID, missing.DocumentTypeID, missing.AgentID));
        Assert.Equal(new[] { 10 }, facts.ValidPledgeIds);
        Assert.Equal(new[] { 11 }, facts.ValidExemptionIds);
    }

    [Theory]
    [InlineData("wrong-owner")]
    [InlineData("deleted-row")]
    [InlineData("unavailable-pledge")]
    [InlineData("unavailable-exemption")]
    public void UnavailableOrMismatchedCoverage_CannotSatisfyRequirement(string scenario)
    {
        var r = new AutoDetectedMissingDocumentDto { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 };
        var missing = new MissingDocument { AgentID = scenario == "wrong-owner" ? 9 : 8, DocumentTypeID = r.DocumentTypeID,
            IsDeleted = scenario == "deleted-row", Status = scenario == "unavailable-exemption" ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = 10, ExemptionID = 11 };
        var facts = Facts([r], [missing]) with
        {
            ValidPledgeIds = scenario == "unavailable-pledge" ? new HashSet<int>() : new HashSet<int> { 10 },
            ValidExemptionIds = scenario == "unavailable-exemption" ? new HashSet<int>() : new HashSet<int> { 11 }
        };
        AssertDecision(EvaluateDocuments(facts, EvaluationDate), RejectionReason.UnsatisfiedDocuments);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Coverage_CannotReplaceItsOwnDirective(bool exemption)
    {
        var type = exemption ? DocumentTypeConstants.ExemptionDirective : DocumentTypeConstants.PledgeDocument;
        var r = new AutoDetectedMissingDocumentDto { DocumentTypeID = type, PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null };
        var missing = new MissingDocument { DocumentTypeID = type, PledgeID = r.PledgeID, ExemptionID = r.ExemptionID,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pending };
        var rejected = new Document { DocumentTypeID = type, PledgeID = r.PledgeID, ExemptionID = r.ExemptionID,
            PendingReviewStatus = PendingReviewStatus.Rejected };
        AssertDecision(EvaluateDocuments(Facts([r], [missing], [rejected]), EvaluationDate), RejectionReason.UnsatisfiedDocuments);
    }

    [Fact]
    public void OutstandingManualRequirements_AreIncludedEvenWithoutAutomaticRequirements()
    {
        var missing = new MissingDocument { AgentID = 8, DocumentTypeID = DocumentTypeConstants.NationalId, Status = MissingDocumentStatusConstants.Pending };
        var facts = Facts([], [missing]) with { ValidPledgeIds = new HashSet<int>(), ValidExemptionIds = new HashSet<int>() };
        AssertDecision(EvaluateDocuments(facts, EvaluationDate), RejectionReason.UnsatisfiedDocuments);
        AssertDecision(EvaluateDocuments(facts with { UploadedDocuments = [new Document { DocumentTypeID = missing.DocumentTypeID, AgentID = 8 }] }, EvaluationDate), RejectionReason.None);
    }

    private static DocumentFacts Facts(AutoDetectedMissingDocumentDto[] automatic, MissingDocument[]? manual = null, Document[]? documents = null) =>
        new(automatic, manual ?? [], documents ?? [], new HashSet<int> { 10 }, new HashSet<int> { 11 });

    private static void AssertDecision(Decision result, RejectionReason expected)
    {
        Assert.Equal(expected, result.Reason);
        Assert.Equal(expected == RejectionReason.None, result.IsAllowed);
        Assert.False(result.RequiresEntitledPersonFacts);
    }
}
