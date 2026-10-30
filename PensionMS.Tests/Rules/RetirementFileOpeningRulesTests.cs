using PensionMS.Core.Rules.RetirementFiles;
using PensionMS.Core.Time;
using static PensionMS.Core.Rules.RetirementFiles.RetirementFileOpeningRules;

namespace PensionMS.Tests.Rules;

public class RetirementFileOpeningRulesTests
{
    private static readonly SourceFacts Valid = new(true, false, true, false, true,
        new DateOnly(1980, 1, 1), new DateOnly(2020, 1, 1));

    [Theory]
    [InlineData("retiree", RejectionReason.MissingRetiree)]
    [InlineData("retiree-deleted", RejectionReason.MissingRetiree)]
    [InlineData("referral", RejectionReason.MissingProcessedReferral)]
    [InlineData("referral-deleted", RejectionReason.MissingProcessedReferral)]
    [InlineData("unprocessed", RejectionReason.MissingProcessedReferral)]
    [InlineData("enrollment-null", RejectionReason.MissingEnrollmentDate)]
    [InlineData("enrollment-default", RejectionReason.MissingEnrollmentDate)]
    [InlineData("retirement-null", RejectionReason.MissingRetirementDate)]
    [InlineData("retirement-default", RejectionReason.MissingRetirementDate)]
    [InlineData("equal", RejectionReason.InvalidServicePeriod)]
    [InlineData("earlier", RejectionReason.InvalidServicePeriod)]
    public void BR02To05_SourceFacts_RejectInvalidSources(string scenario, RejectionReason expected)
    {
        var facts = scenario switch
        {
            "retiree" => Valid with { RetireeExists = false },
            "retiree-deleted" => Valid with { RetireeIsDeleted = true },
            "referral" => Valid with { ReferralExists = false },
            "referral-deleted" => Valid with { ReferralIsDeleted = true },
            "unprocessed" => Valid with { ReferralIsProcessed = false },
            "enrollment-null" => Valid with { EnrollmentDate = null },
            "enrollment-default" => Valid with { EnrollmentDate = default(DateOnly) },
            "retirement-null" => Valid with { RetirementDate = null },
            "retirement-default" => Valid with { RetirementDate = default(DateOnly) },
            "equal" => Valid with { RetirementDate = Valid.EnrollmentDate },
            "earlier" => Valid with { RetirementDate = Valid.EnrollmentDate!.Value.AddDays(-1) },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var result = EvaluateSource(facts, new DateOnly(2026, 10, 28));
        Assert.False(result.IsAllowed);
        Assert.Equal(expected, result.Reason);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Theory]
    [InlineData("2026-10-27T20:59:59Z", false)]
    [InlineData("2026-10-27T21:00:00Z", true)]
    [InlineData("2026-10-28T00:00:00Z", true)]
    public void BR05_OpeningDay_UsesExplicitYemenCalendarDay(string instant, bool allowed)
    {
        var facts = Valid with { RetirementDate = new DateOnly(2026, 10, 28) };
        Assert.Equal(allowed, EvaluateSource(facts, YemenTime.LocalDate(DateTimeOffset.Parse(instant))).IsAllowed);
    }

    [Fact]
    public void BR05_OpeningDay_MustBeProvided()
    {
        Assert.Equal(RejectionReason.MissingOpeningDate, EvaluateSource(Valid, null).Reason);
        Assert.Equal(RejectionReason.MissingOpeningDate, EvaluateSource(Valid, default(DateOnly)).Reason);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void BR01_AnyPreviousFile_PreventsOpening(bool exists, bool allowed) =>
        Assert.Equal(allowed, EvaluatePreviousFile(exists).IsAllowed);

    [Theory]
    [InlineData(false, 1, 1, true, true, RejectionReason.InactiveUser)]
    [InlineData(true, null, null, true, true, RejectionReason.MissingAssignedBranch)]
    [InlineData(true, 0, null, true, true, RejectionReason.MissingAssignedBranch)]
    [InlineData(true, 1, 2, true, true, RejectionReason.DifferentRequestedBranch)]
    [InlineData(true, 1, 1, false, true, RejectionReason.InactiveBranch)]
    [InlineData(true, 1, 1, true, false, RejectionReason.BranchNotAllowed)]
    [InlineData(true, 1, null, true, true, RejectionReason.None)]
    [InlineData(true, 1, 1, true, true, RejectionReason.None)]
    public void BR06_BranchAssignment_IsAuthoritative(bool activeUser, int? assigned, int? requested,
        bool activeBranch, bool allowed, RejectionReason expected) =>
        Assert.Equal(expected, EvaluateBranch(activeUser, assigned, requested, activeBranch, allowed).Reason);
}
