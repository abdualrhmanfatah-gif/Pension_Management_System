using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Rules.Documents;
using static PensionMS.Core.Rules.Entitlements.EntitledPersonRules;

namespace PensionMS.Tests.Rules;

public class AutomaticEntitlementRulesTests
{
    private static readonly DateOnly EvaluationDate = new(2026, 10, 25);
    private static AutomaticEntitlementState Person => new(7, 1, false, false, false, false,
        EntitledStopCodes.MissingData, 1, RelationshipConstants.Wife, new(1996, 10, 25), "مستحق",
        SocialStatusConstants.Divorced, HealthStatusConstants.Healthy, false, "123456789");

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void DeletedOrDeceased_PrecedesMissingRetireeAndManualStops(bool deleted, bool deceased)
    {
        var state = Person with { IsDeleted = deleted, IsDeceased = deceased,
            RetireeIsAlive = null, StopCode = EntitledStopCodes.AdminDecision };
        Assert.Equal(new AutomaticDecision(true, "المستحق محذوف أو متوفى", null),
            EvaluateAutomaticPrerequisites(state, EvaluationDate));
    }

    [Fact]
    public void MissingRetiree_PrecedesManualStop()
    {
        Assert.Equal(new AutomaticDecision(false, "بيانات المتقاعد غير موجودة", null),
            EvaluateAutomaticPrerequisites(Person with { RetireeIsAlive = null, StopCode = EntitledStopCodes.AdminDecision }, EvaluationDate));
    }

    [Theory]
    [InlineData(EntitledStopCodes.AdminDecision, true)]
    [InlineData(EntitledStopCodes.AgeLimit, false)]
    [InlineData(EntitledStopCodes.DaughterMarried, true)]
    [InlineData(EntitledStopCodes.WidowRemarried, false)]
    [InlineData(EntitledStopCodes.Deceased, true)]
    [InlineData(EntitledStopCodes.Employed, false)]
    [InlineData("UNKNOWN", true)]
    [InlineData("", false)]
    [InlineData("missing_data", true)]
    public void ManualOrOtherStops_PrecedeAutomaticRulesAndPreserveStopFields(string code, bool alive)
    {
        var state = Person with { IsActive = true, StopCode = code, RetireeIsAlive = alive, BirthDate = default, FullName = "" };
        var before = state;
        Assert.Equal(new AutomaticDecision(true, "المستحق لديه إيقاف يدوي أو سبب آخر فلا يتم تفعيله تلقائياً",
            new(false, false, null, null, AutomaticStopDateChange.Preserve, false)),
            EvaluateAutomaticPrerequisites(state, EvaluationDate));
        Assert.Equal(before, state);
    }

    [Theory]
    [InlineData(false, EntitledStopCodes.RetireeAlive, false)]
    [InlineData(true, EntitledStopCodes.RetireeAlive, true)]
    [InlineData(false, EntitledStopCodes.MissingData, true)]
    [InlineData(false, null, true)]
    public void RetireeAlive_ExplicitlyPreservesOrRefreshesStopFieldsAndTimestamps(bool active, string? code, bool refresh)
    {
        var state = Person with { IsActive = active, StopCode = code, RetireeIsAlive = true, BirthDate = default };
        var result = EvaluateAutomaticPrerequisites(state, EvaluationDate)!;
        Assert.True(result.IsSuccess);
        Assert.Equal("تم تحديث الحالة: المتقاعد على قيد الحياة", result.Message);
        Assert.Equal(new AutomaticStateChange(false, refresh, EntitledStopCodes.RetireeAlive,
            "المتقاعد على قيد الحياة — الاستحقاق معلق",
            refresh ? AutomaticStopDateChange.SetUtcNow : AutomaticStopDateChange.Preserve, refresh), result.Change);
    }

    [Theory]
    [InlineData("age", EntitledStopCodes.AgeLimit, "المستحق غير مؤهل: تجاوز السن القانوني (24 سنة)")]
    [InlineData("marriage", EntitledStopCodes.DaughterMarried, "المستحق غير مؤهل: البنت متزوجة")]
    [InlineData("missing-birth", EntitledStopCodes.MissingData, "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية")]
    [InlineData("future-birth", EntitledStopCodes.MissingData, "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية")]
    public void LegalFailure_ReusesEligibilityAndSetsStopDateWithoutRefreshingUpdatedAt(string scenario, string code, string message)
    {
        var state = Person with { FullName = "", SocialStatusID = null, HealthStatusID = null };
        if (scenario == "age") state = state with { Relationship = RelationshipConstants.Son, BirthDate = EvaluationDate.AddYears(-24) };
        if (scenario == "marriage") state = state with { Relationship = RelationshipConstants.Daughter, SocialStatusID = SocialStatusConstants.Married };
        if (scenario == "missing-birth") state = state with { BirthDate = default };
        if (scenario == "future-birth") state = state with { BirthDate = EvaluationDate.AddDays(1) };
        Assert.Equal(new AutomaticDecision(true, message,
            new(false, true, code, message, AutomaticStopDateChange.SetUtcNow, false)),
            EvaluateAutomaticPrerequisites(state, EvaluationDate));
    }

    [Fact]
    public void LegalEligibility_DisabilityExceptionAllowsOverAgeSonToProceed()
    {
        Assert.Null(EvaluateAutomaticPrerequisites(Person with { Relationship = RelationshipConstants.Son,
            HasDisabilityException = true }, EvaluationDate));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0, 0)]
    [InlineData(null, 0)]
    [InlineData(0, null)]
    public void BasicDeficiencies_PrecedeEvidenceAndNullAndZeroStatusesAreMissing(int? social, int? health)
    {
        var state = Person with { IsActive = true, FullName = " \t", SocialStatusID = social,
            HealthStatusID = health, NationalID = " " };
        var result = Deficiencies(state);
        Assert.Equal(new[] { "الاسم الكامل", "الحالة الاجتماعية", "الحالة الصحية", "الرقم الوطني", "وثيقة الهوية الوطنية" }, result.Issues);
        Assert.False(result.NeedsCoverage);
        Assert.Single(result.MissingProofs);
    }

    [Fact]
    public void BasicBirthDateCheck_IsRetainedButPrerequisiteEligibilityStillShortCircuitsIt()
    {
        var state = Person with { FullName = "", BirthDate = default, SocialStatusID = null, HealthStatusID = null, NationalID = "" };
        Assert.Equal(new[] { "الاسم الكامل", "تاريخ الميلاد", "الحالة الاجتماعية", "الحالة الصحية" }, Deficiencies(state).Issues);
        Assert.Equal("تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية",
            EvaluateAutomaticPrerequisites(state, EvaluationDate)!.Message);
    }

    [Fact]
    public void NonzeroStatuses_AreNotNewlyRejected()
    {
        var state = Person with { SocialStatusID = -1, HealthStatusID = -1 };
        Assert.Empty(Deficiencies(state, [Proof(DocumentTypeConstants.NationalId)]).Issues);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(0, true)]
    public void NationalIdentity_BasicValueAndProofBecomeRequiredExactlyAt18(int birthDayOffset, bool adult)
    {
        var state = Person with { RelationshipID = 2, Relationship = RelationshipConstants.Son,
            BirthDate = EvaluationDate.AddYears(-18).AddDays(birthDayOffset), NationalID = "" };
        Assert.Null(EvaluateAutomaticPrerequisites(state, EvaluationDate));
        Assert.Equal(adult ? new[] { "الرقم الوطني", "وثيقة الهوية الوطنية" } : Array.Empty<string>(), Deficiencies(state).Issues);
    }

    [Theory]
    [InlineData("valid", true)]
    [InlineData("expires-today", true)]
    [InlineData("pending", true)]
    [InlineData("expired", false)]
    [InlineData("rejected", false)]
    [InlineData("deleted", false)]
    [InlineData("wrong-file", false)]
    [InlineData("wrong-entitled", false)]
    [InlineData("no-owner", false)]
    [InlineData("mixed-retiree", false)]
    [InlineData("mixed-agent", false)]
    [InlineData("mixed-guardian", false)]
    [InlineData("mixed-pledge", false)]
    [InlineData("mixed-exemption", false)]
    [InlineData("wrong-type", false)]
    public void Evidence_UsesSharedValidityAndExactSameFileOwner(string scenario, bool valid)
    {
        var document = Proof(DocumentTypeConstants.DivorceCertificate);
        if (scenario == "expires-today") document.ExpiryDate = EvaluationDate;
        if (scenario == "expired") document.ExpiryDate = EvaluationDate.AddDays(-1);
        if (scenario == "pending") document.PendingReviewStatus = PendingReviewStatus.Pending;
        if (scenario == "rejected") document.PendingReviewStatus = PendingReviewStatus.Rejected;
        if (scenario == "deleted") document.IsDeleted = true;
        if (scenario == "wrong-file") document.FileID = 2;
        if (scenario == "wrong-entitled") document.EntitledID = 8;
        if (scenario == "no-owner") document.EntitledID = null;
        if (scenario == "mixed-retiree") document.RetireeID = 2;
        if (scenario == "mixed-agent") document.AgentID = 8;
        if (scenario == "mixed-guardian") document.GuardianID = 8;
        if (scenario == "mixed-pledge") document.PledgeID = 10;
        if (scenario == "mixed-exemption") document.ExemptionID = 11;
        if (scenario == "wrong-type") document.DocumentTypeID = DocumentTypeConstants.MarriageContract;
        var before = DocumentState(document);
        var documents = new[] { Proof(DocumentTypeConstants.NationalId), document };
        var result = Deficiencies(Person, documents);
        Assert.Equal(valid ? Array.Empty<string>() : new[] { "وثيقة الطلاق" }, result.Issues);
        Assert.Equal(valid ? 0 : 1, result.MissingProofs.Count);
        if (!valid) Assert.Equal(new RetirementDocumentRequirement(DocumentTypeConstants.DivorceCertificate,
            null, null, null, 7, null, null), Assert.Single(result.MissingProofs));
        Assert.Equal(before, DocumentState(document));
        Assert.Same(document, documents[1]);
    }

    [Fact]
    public void ValidAlternative_SatisfiesRequirementWithoutChangingRejectedHistory()
    {
        var rejected = Proof(DocumentTypeConstants.DivorceCertificate);
        rejected.PendingReviewStatus = PendingReviewStatus.Rejected;
        Assert.Empty(Deficiencies(Person, [Proof(DocumentTypeConstants.NationalId), rejected,
            Proof(DocumentTypeConstants.DivorceCertificate)]).Issues);
        Assert.Equal(PendingReviewStatus.Rejected, rejected.PendingReviewStatus);
    }

    [Fact]
    public void InactiveDocumentType_IsExcludedWithoutRemovingBasicDataRequirements()
    {
        var requirements = AutomaticDocumentRequirements(Person, EvaluationDate);
        var activeTypes = new HashSet<int>();
        var result = EvaluateAutomaticDeficiencies(Person with { NationalID = "" }, requirements, activeTypes, [], EvaluationDate);
        Assert.Equal(new[] { "الرقم الوطني" }, result.Issues);
        Assert.Empty(result.MissingProofs);
        Assert.False(result.NeedsCoverage);
        Assert.Empty(activeTypes);
        Assert.Equal(2, requirements.Count);
    }

    [Fact]
    public void DeficiencyOrdering_FollowsExistingDocumentRequirements()
    {
        var result = Deficiencies(Person with { FullName = "", NationalID = "", HasDisabilityException = true });
        Assert.Equal(new[] { "الاسم الكامل", "الرقم الوطني", "وثيقة الهوية الوطنية", "وثيقة الطلاق", "التقرير الطبي", "حكم الإعالة" }, result.Issues);
    }

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    public void Coverage_OnlyPreservesAlreadyActiveDocumentOnlyCases(bool active, bool exemption, bool remainsActive)
    {
        var state = Person with { IsActive = active };
        var deficiencies = Deficiencies(state, [Proof(DocumentTypeConstants.NationalId)]);
        var missing = new MissingDocument { FileID = 1, EntitledID = 7, DocumentTypeID = DocumentTypeConstants.DivorceCertificate,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null };
        var rows = new[] { missing };
        var coverage = new AutomaticCoverage(rows, new HashSet<int> { 10 }, new HashSet<int> { 11 });
        var issueSnapshot = deficiencies.Issues.ToArray();
        var before = state;
        var decision = CompleteAutomaticEvaluation(deficiencies, coverage);
        Assert.Equal(active, deficiencies.NeedsCoverage);
        Assert.Equal(remainsActive, decision.Change!.Value.IsActive);
        Assert.Equal(remainsActive ? AutomaticStopDateChange.Clear : AutomaticStopDateChange.Preserve, decision.Change.Value.StopDateChange);
        Assert.True(decision.Change.Value.RefreshUpdatedAt);
        Assert.Equal(issueSnapshot, deficiencies.Issues);
        Assert.Equal(before, state);
        Assert.Equal(exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged, missing.Status);
        Assert.Same(missing, Assert.Single(rows));
        Assert.Equal(new[] { 10 }, coverage.ValidPledgeIds);
        Assert.Equal(new[] { 11 }, coverage.ValidExemptionIds);
    }

    [Theory]
    [InlineData("missing-basic")]
    [InlineData("wrong-owner")]
    [InlineData("deleted-row")]
    [InlineData("invalid-pledge")]
    [InlineData("partial-coverage")]
    public void Coverage_CannotCompensateForBasicDataOrInvalidOrPartialCoverage(string scenario)
    {
        var state = Person with { IsActive = true, FullName = scenario == "missing-basic" ? "" : "مستحق" };
        var deficiencies = Deficiencies(state, scenario == "partial-coverage" ? [] : [Proof(DocumentTypeConstants.NationalId)]);
        var rows = new[] { new MissingDocument { FileID = 1, EntitledID = scenario == "wrong-owner" ? 8 : 7,
            IsDeleted = scenario == "deleted-row", DocumentTypeID = DocumentTypeConstants.DivorceCertificate,
            Status = MissingDocumentStatusConstants.Pledged, PledgeID = 10 } };
        var decision = CompleteAutomaticEvaluation(deficiencies,
            new AutomaticCoverage(rows, scenario == "invalid-pledge" ? new HashSet<int>() : new HashSet<int> { 10 }, new HashSet<int>()));
        Assert.Equal(new AutomaticDecision(true, "المستحق لا يزال لديه نواقص",
            new(false, true, EntitledStopCodes.MissingData, $"بيانات/وثائق ناقصة: {string.Join("، ", deficiencies.Issues)}",
                AutomaticStopDateChange.Preserve, true)), decision);
        Assert.Equal(scenario != "missing-basic", deficiencies.NeedsCoverage);
    }

    [Fact]
    public void CompleteEvidence_ActivatesAndExplicitlyClearsAllStopFields()
    {
        var deficiencies = Deficiencies(Person, [Proof(DocumentTypeConstants.NationalId), Proof(DocumentTypeConstants.DivorceCertificate)]);
        Assert.Empty(deficiencies.Issues);
        Assert.False(deficiencies.NeedsCoverage);
        Assert.Equal(new AutomaticDecision(true, "تم تفعيل المستحق بنجاح",
            new(true, true, null, null, AutomaticStopDateChange.Clear, true)), CompleteAutomaticEvaluation(deficiencies));
    }

    private static AutomaticDeficiencies Deficiencies(AutomaticEntitlementState state, Document[]? documents = null)
    {
        var requirements = AutomaticDocumentRequirements(state, EvaluationDate);
        return EvaluateAutomaticDeficiencies(state, requirements, requirements.Select(r => r.DocumentTypeID).ToHashSet(),
            documents ?? [], EvaluationDate);
    }

    private static Document Proof(int type) => new() { FileID = 1, EntitledID = 7, DocumentTypeID = type };

    private static object DocumentState(Document d) => (d.FileID, d.DocumentTypeID, d.EntitledID, d.RetireeID,
        d.AgentID, d.GuardianID, d.PledgeID, d.ExemptionID, d.PendingReviewStatus, d.ExpiryDate, d.IsDeleted);
}
