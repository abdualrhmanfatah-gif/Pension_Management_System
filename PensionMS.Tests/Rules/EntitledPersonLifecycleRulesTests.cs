using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using static PensionMS.Core.Rules.Entitlements.EntitledPersonRules;

namespace PensionMS.Tests.Rules;

public class EntitledPersonLifecycleRulesTests
{
    private static readonly DateOnly EvaluationDate = new(2026, 10, 25);
    private static StopState ActiveSon => new(true, RelationshipConstants.Son, new(2002, 10, 25), false);
    private static ReactivationState InactivePerson => new(false, false, EntitledStopCodes.AdminDecision,
        RelationshipConstants.Daughter, new(1996, 10, 25), SocialStatusConstants.Single, false, false);

    [Fact]
    public void Stop_InactiveTakesPrecedenceOverRelationshipDisabilityAndAge()
    {
        var state = ActiveSon with { IsActive = false, Relationship = RelationshipConstants.Daughter,
            HasDisabilityException = true, BirthDate = EvaluationDate };
        Assert.Equal(new StopDecision(false, "المستحق موقوف مسبقاً", null),
            EvaluateStop(state, EntitledStopCodes.AgeLimit, EvaluationDate));
    }

    [Theory]
    [InlineData(RelationshipConstants.Daughter)]
    [InlineData(RelationshipConstants.Wife)]
    [InlineData(null)]
    public void Stop_AgeLimitRejectsNonSonBeforeDisability(string? relationship)
    {
        var result = EvaluateStop(ActiveSon with { Relationship = relationship, HasDisabilityException = true },
            EntitledStopCodes.AgeLimit, EvaluationDate);
        Assert.Equal(new StopDecision(false, "إيقاف بسبب العمر مخصص للأبناء فقط", null), result);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    public void Stop_AgeBoundaryUsesTheExplicitEvaluationDate(int birthDayOffset, bool allowed)
    {
        var state = ActiveSon with { BirthDate = new DateOnly(2002, 10, 25).AddDays(birthDayOffset) };
        var result = EvaluateStop(state, EntitledStopCodes.AgeLimit, EvaluationDate);
        Assert.Equal(allowed, result.IsAllowed);
        Assert.Equal(allowed ? null : "الابن لم يبلغ 24 سنة بعد (العمر الحالي: 23 سنة)", result.ErrorMessage);
        Assert.Null(result.SocialStatusID);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Stop_DisabilityPrecedesAgeValidation(int birthDayOffset)
    {
        var state = ActiveSon with { HasDisabilityException = true,
            BirthDate = new DateOnly(2002, 10, 25).AddDays(birthDayOffset) };
        Assert.Equal(new StopDecision(false, "لا يمكن إيقاف هذا الابن بسبب العمر — لديه استثناء إعاقة/مرض نفسي", null),
            EvaluateStop(state, EntitledStopCodes.AgeLimit, EvaluationDate));
    }

    [Theory]
    [InlineData(EntitledStopCodes.DaughterMarried, RelationshipConstants.Daughter, null)]
    [InlineData(EntitledStopCodes.WidowRemarried, RelationshipConstants.Wife, null)]
    [InlineData(EntitledStopCodes.DaughterMarried, RelationshipConstants.Wife, "إيقاف بسبب الزواج مخصص للبنات فقط")]
    [InlineData(EntitledStopCodes.WidowRemarried, RelationshipConstants.Daughter, "إيقاف بسبب إعادة الزواج مخصص للزوجات فقط")]
    [InlineData(EntitledStopCodes.DaughterMarried, null, "إيقاف بسبب الزواج مخصص للبنات فقط")]
    [InlineData(EntitledStopCodes.WidowRemarried, null, "إيقاف بسبب إعادة الزواج مخصص للزوجات فقط")]
    public void Stop_MarriageRequiresMatchingRelationshipAndProposesMarriedStatus(string code, string? relationship, string? error)
    {
        var result = EvaluateStop(ActiveSon with { Relationship = relationship }, code, EvaluationDate);
        Assert.Equal(new StopDecision(error == null, error, error == null ? SocialStatusConstants.Married : null), result);
    }

    [Theory]
    [InlineData(EntitledStopCodes.Deceased)]
    [InlineData(EntitledStopCodes.AdminDecision)]
    [InlineData(EntitledStopCodes.Employed)]
    [InlineData(EntitledStopCodes.RetireeAlive)]
    [InlineData(EntitledStopCodes.MissingData)]
    [InlineData("UNKNOWN")]
    [InlineData("age_limit")]
    [InlineData("")]
    [InlineData(null)]
    public void Stop_OtherCodesRemainAllowedWithoutSocialStatusChange(string? code)
    {
        Assert.Equal(new StopDecision(true, null, null), EvaluateStop(ActiveSon, code, EvaluationDate));
    }

    [Fact]
    public void Stop_DoesNotIntroduceBirthDateValidation()
    {
        Assert.True(EvaluateStop(ActiveSon with { BirthDate = default }, EntitledStopCodes.AgeLimit, EvaluationDate).IsAllowed);
    }

    [Theory]
    [InlineData(true, true, "المستحق نشط بالفعل")]
    [InlineData(true, false, "المستحق نشط بالفعل")]
    [InlineData(false, true, "لا يمكن إعادة تفعيل مستحق متوفى")]
    public void Reactivation_StateValidationPrecedesPairingAndMissingRetiree(bool active, bool deceased, string error)
    {
        var state = InactivePerson with { IsActive = active, IsDeceased = deceased, StopCode = "UNKNOWN", RetireeIsAlive = null };
        AssertRejected(EvaluateReactivation(state, "UNKNOWN", EvaluationDate), error);
    }

    public static IEnumerable<object?[]> Pairings()
    {
        string?[] stops = [EntitledStopCodes.DaughterMarried, EntitledStopCodes.WidowRemarried,
            EntitledStopCodes.AgeLimit, EntitledStopCodes.AdminDecision, EntitledStopCodes.Deceased,
            EntitledStopCodes.Employed, EntitledStopCodes.RetireeAlive, EntitledStopCodes.MissingData,
            "UNKNOWN", "daughter_married", "", null];
        string?[] reactivations = [ReactivationCodes.Divorced, ReactivationCodes.HusbandDeceased,
            ReactivationCodes.DisabilityReport, ReactivationCodes.AdminDecision, "UNKNOWN", "divorced", "", null];
        var allowed = new HashSet<(string?, string?)>
        {
            (EntitledStopCodes.DaughterMarried, ReactivationCodes.Divorced),
            (EntitledStopCodes.DaughterMarried, ReactivationCodes.HusbandDeceased),
            (EntitledStopCodes.WidowRemarried, ReactivationCodes.Divorced),
            (EntitledStopCodes.WidowRemarried, ReactivationCodes.HusbandDeceased),
            (EntitledStopCodes.AgeLimit, ReactivationCodes.DisabilityReport),
            (EntitledStopCodes.AdminDecision, ReactivationCodes.AdminDecision)
        };
        foreach (var stop in stops)
        foreach (var reactivation in reactivations)
            yield return [stop, reactivation, allowed.Contains((stop, reactivation))];
    }

    [Theory]
    [MemberData(nameof(Pairings))]
    public void Reactivation_PreservesEveryAllowedAndRejectedCodePairing(string? stopCode, string? code, bool allowed)
    {
        var state = InactivePerson with { StopCode = stopCode };
        var result = EvaluateReactivation(state, code, EvaluationDate);
        Assert.Equal(allowed, result.IsAllowed);
        if (!allowed)
            AssertRejected(result, $"رمز إعادة التفعيل ({code}) غير متوافق مع سبب الإيقاف ({stopCode})");
        else
        {
            Assert.Null(result.ErrorMessage);
            Assert.Equal(code == ReactivationCodes.Divorced ? SocialStatusConstants.Divorced :
                code == ReactivationCodes.HusbandDeceased ? SocialStatusConstants.Widowed : SocialStatusConstants.Single,
                result.SocialStatusID);
            Assert.Equal(code == ReactivationCodes.DisabilityReport, result.HasDisabilityException);
        }
    }

    [Theory]
    [InlineData(ReactivationCodes.Divorced, SocialStatusConstants.Divorced)]
    [InlineData(ReactivationCodes.HusbandDeceased, SocialStatusConstants.Widowed)]
    public void Reactivation_LegalEligibilityUsesProposedSocialStatus(string code, int status)
    {
        var state = InactivePerson with { StopCode = EntitledStopCodes.DaughterMarried,
            SocialStatusID = SocialStatusConstants.Married };
        var result = EvaluateReactivation(state, code, EvaluationDate);
        Assert.True(result.IsAllowed, result.ErrorMessage);
        Assert.Equal(status, result.SocialStatusID);
        Assert.Equal(SocialStatusConstants.Married, state.SocialStatusID);
    }

    [Fact]
    public void Reactivation_LegalEligibilityUsesProposedDisabilityExceptionForOverAgeSon()
    {
        var state = InactivePerson with { StopCode = EntitledStopCodes.AgeLimit, Relationship = RelationshipConstants.Son };
        var result = EvaluateReactivation(state, ReactivationCodes.DisabilityReport, EvaluationDate);
        Assert.True(result.IsAllowed, result.ErrorMessage);
        Assert.True(result.HasDisabilityException);
        Assert.False(state.HasDisabilityException);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(SocialStatusConstants.Widowed, true)]
    public void Reactivation_AdminDecisionPreservesSocialStatusAndExistingException(int? status, bool disability)
    {
        var state = InactivePerson with { SocialStatusID = status, HasDisabilityException = disability };
        Assert.Equal(new ReactivationDecision(true, null, status, disability),
            EvaluateReactivation(state, ReactivationCodes.AdminDecision, EvaluationDate));
    }

    [Fact]
    public void Reactivation_MissingRetireeIsRejectedAfterCompatiblePairing()
    {
        AssertRejected(EvaluateReactivation(InactivePerson with { RetireeIsAlive = null },
            ReactivationCodes.AdminDecision, EvaluationDate), "بيانات المتقاعد غير موجودة");
        var incompatible = InactivePerson with { RetireeIsAlive = null, StopCode = "UNKNOWN" };
        AssertRejected(EvaluateReactivation(incompatible, ReactivationCodes.AdminDecision, EvaluationDate),
            "رمز إعادة التفعيل (ADMIN_DECISION) غير متوافق مع سبب الإيقاف (UNKNOWN)");
    }

    [Theory]
    [InlineData("retiree-alive", "لا يمكن التفعيل: المتقاعد لا يزال على قيد الحياة")]
    [InlineData("missing-birth", "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية")]
    [InlineData("future-birth", "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية")]
    [InlineData("over-age", "المستحق غير مؤهل: تجاوز السن القانوني (24 سنة)")]
    [InlineData("married", "المستحق غير مؤهل: البنت متزوجة")]
    public void Reactivation_PropagatesExistingLegalEligibilityFailure(string scenario, string error)
    {
        var state = InactivePerson;
        if (scenario == "retiree-alive") state = state with { RetireeIsAlive = true, BirthDate = default };
        if (scenario == "missing-birth") state = state with { BirthDate = default };
        if (scenario == "future-birth") state = state with { BirthDate = EvaluationDate.AddDays(1) };
        if (scenario == "over-age") state = state with { Relationship = RelationshipConstants.Son };
        if (scenario == "married") state = state with { SocialStatusID = SocialStatusConstants.Married };
        AssertRejected(EvaluateReactivation(state, ReactivationCodes.AdminDecision, EvaluationDate), error);
    }

    private static void AssertRejected(ReactivationDecision result, string error)
    {
        Assert.False(result.IsAllowed);
        Assert.Equal(error, result.ErrorMessage);
        Assert.Null(result.SocialStatusID);
        Assert.False(result.HasDisabilityException);
    }
}
