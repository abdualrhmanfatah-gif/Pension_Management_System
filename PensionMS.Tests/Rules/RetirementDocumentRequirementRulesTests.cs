using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Rules.Documents;
using static PensionMS.Core.Rules.Documents.RetirementDocumentRequirementRules;

namespace PensionMS.Tests.Rules;

public class RetirementDocumentRequirementRulesTests
{
    private static readonly DateOnly EvaluationDate = new(2026, 10, 25);
    private static FileFacts Empty => new(null, false, [], [], [], [], []);
    private static EntitledPersonFacts Adult => new(7, new(1996, 10, 25), 3,
        SocialStatusConstants.Single, HealthStatusConstants.Healthy, false, null);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingRetiree_SkipsRetireeAndEntitledEvidenceButKeepsFileAndBook(bool book)
    {
        var result = Select(Empty with { HasAnyPaymentBooks = book, EntitledPersons = [Adult] });
        Assert.Equal(book ? new[] { FileRequirement(DocumentTypeConstants.FileOriginals), FileRequirement(DocumentTypeConstants.PaymentBookImage) }
            : new[] { FileRequirement(DocumentTypeConstants.FileOriginals) }, result);
    }

    public static IEnumerable<object?[]> RetireeContexts()
    {
        foreach (var alive in new[] { true, false })
        foreach (int? health in new int?[] { null, 0, HealthStatusConstants.Healthy,
            HealthStatusConstants.PartiallyDisabled, HealthStatusConstants.FullyDisabled,
            HealthStatusConstants.Deceased, HealthStatusConstants.Martyr, 99 })
            yield return [alive, health];
    }

    [Fact]
    public void MissingRetiree_DoesNotSuppressSelectedAgentsGuardiansOrDirectives()
    {
        var facts = Empty with { AgentIds = [8], GuardianIds = [9], PledgeIds = [10], ExemptionIds = [11] };
        Assert.Equal(new[]
        {
            FileRequirement(DocumentTypeConstants.FileOriginals),
            new RetirementDocumentRequirement(DocumentTypeConstants.NationalId, null, 8, null, null, null, null),
            new(DocumentTypeConstants.AgencyDocument, null, 8, null, null, null, null),
            new(DocumentTypeConstants.NationalId, null, null, 9, null, null, null),
            new(DocumentTypeConstants.GuardianshipDocument, null, null, 9, null, null, null),
            new(DocumentTypeConstants.PledgeDocument, null, null, null, null, 10, null),
            new(DocumentTypeConstants.ExemptionDirective, null, null, null, null, null, 11)
        }, Select(facts));
    }

    [Theory]
    [MemberData(nameof(RetireeContexts))]
    public void RetireeRequirements_PreserveConflictingLifeAndHealthFlags(bool alive, int? health)
    {
        var facts = Empty with { Retiree = new RetireeFacts(2, alive, health), HasAnyPaymentBooks = true,
            EntitledPersons = [Adult with { RelationshipID = 5 }] };
        var expected = new List<RetirementDocumentRequirement>
        {
            FileRequirement(DocumentTypeConstants.FileOriginals), FileRequirement(DocumentTypeConstants.PaymentBookImage)
        };
        var deathEvidence = !alive || health is HealthStatusConstants.Deceased or HealthStatusConstants.Martyr;
        if (deathEvidence) expected.Add(FileRequirement(DocumentTypeConstants.InheritanceCert));
        if (alive) expected.Add(new(DocumentTypeConstants.NationalId, 2, null, null, null, null, null));
        if (deathEvidence) expected.Add(new(DocumentTypeConstants.DeathCertificate, 2, null, null, null, null, null));
        if (health is HealthStatusConstants.PartiallyDisabled or HealthStatusConstants.FullyDisabled)
            expected.Add(new(DocumentTypeConstants.MedicalReport, 2, null, null, null, null, null));
        if (!alive) expected.Add(PersonRequirement(DocumentTypeConstants.NationalId, 7));
        Assert.Equal(expected, Select(facts));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(EntitledStopCodes.RetireeAlive, true)]
    [InlineData(EntitledStopCodes.MissingData, true)]
    [InlineData(EntitledStopCodes.AdminDecision, true)]
    [InlineData(EntitledStopCodes.WidowRemarried, true)]
    [InlineData(EntitledStopCodes.Deceased, true)]
    [InlineData(EntitledStopCodes.Employed, true)]
    [InlineData(EntitledStopCodes.AgeLimit, false)]
    [InlineData(EntitledStopCodes.DaughterMarried, false)]
    [InlineData("UNKNOWN", true)]
    [InlineData("", true)]
    [InlineData("age_limit", true)]
    public void EntitledInclusion_ExcludesOnlyTheTwoExistingStopCodes(string? code, bool included)
    {
        var facts = WithPeople(Adult with { StopCode = code, HasDisabilityException = true });
        Assert.Equal(included, Select(facts).Any(r => r.EntitledID == 7));
    }

    public static IEnumerable<object[]> EntitledCases()
    {
        foreach (var relationship in new[] { 1, 3, 4, 6 })
        {
            yield return [relationship, 30, SocialStatusConstants.Single, HealthStatusConstants.Healthy, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.SingleStatusCert }];
            yield return [relationship, 30, SocialStatusConstants.Married, HealthStatusConstants.Healthy, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.MarriageContract }];
            yield return [relationship, 30, SocialStatusConstants.Divorced, HealthStatusConstants.Healthy, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.DivorceCertificate }];
            yield return [relationship, 17, SocialStatusConstants.Divorced, HealthStatusConstants.Healthy, false,
                new[] { DocumentTypeConstants.DivorceCertificate }];
        }
        foreach (var relationship in new[] { 2, 5, 7, 8 })
            yield return [relationship, 30, SocialStatusConstants.Married, HealthStatusConstants.Healthy, false,
                new[] { DocumentTypeConstants.NationalId }];
        foreach (var health in new[] { HealthStatusConstants.Deceased, HealthStatusConstants.Martyr })
        {
            yield return [3, 30, SocialStatusConstants.Divorced, health, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.DeathCertificate }];
            yield return [5, 30, SocialStatusConstants.Divorced, health, false,
                new[] { DocumentTypeConstants.NationalId }];
        }
        foreach (var health in new[] { HealthStatusConstants.PartiallyDisabled, HealthStatusConstants.FullyDisabled })
            yield return [3, 30, SocialStatusConstants.Divorced, health, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.DivorceCertificate,
                    DocumentTypeConstants.MedicalReport, DocumentTypeConstants.MaintenanceJudgment }];
        yield return [2, 30, SocialStatusConstants.Single, HealthStatusConstants.Healthy, true,
            new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.MedicalReport, DocumentTypeConstants.MaintenanceJudgment }];
    }

    [Theory]
    [MemberData(nameof(EntitledCases))]
    public void EntitledRequirements_ReuseSocialHealthAndDisabilityPolicyInOrder(
        int relationship, int age, int social, int health, bool disability, int[] types)
    {
        var person = Adult with { RelationshipID = relationship, BirthDate = EvaluationDate.AddYears(-age),
            SocialStatusID = social, HealthStatusID = health, HasDisabilityException = disability };
        Assert.Equal(types.Select(t => PersonRequirement(t, 7)), Select(WithPeople(person)).Where(r => r.EntitledID == 7));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(0, true)]
    public void EntitledAdultRequirements_BeginExactlyAt18(int birthDayOffset, bool adult)
    {
        var person = Adult with { BirthDate = EvaluationDate.AddYears(-18).AddDays(birthDayOffset) };
        var requirements = Select(WithPeople(person)).Where(r => r.EntitledID.HasValue);
        Assert.Equal(adult ? new[] { PersonRequirement(DocumentTypeConstants.NationalId, 7),
            PersonRequirement(DocumentTypeConstants.SingleStatusCert, 7) } : Array.Empty<RetirementDocumentRequirement>(), requirements);
    }

    [Fact]
    public void BirthDates_AreNotNormalizedOrSubjectedToNewEligibilityChecks()
    {
        var defaultBirth = Adult with { BirthDate = default };
        Assert.Equal(new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.SingleStatusCert },
            Select(WithPeople(defaultBirth)).Where(r => r.EntitledID.HasValue).Select(r => r.DocumentTypeID));
        var futureBirth = Adult with { BirthDate = EvaluationDate.AddDays(1), SocialStatusID = SocialStatusConstants.Divorced };
        Assert.Equal(new[] { PersonRequirement(DocumentTypeConstants.DivorceCertificate, 7) },
            Select(WithPeople(futureBirth)).Where(r => r.EntitledID.HasValue));
    }

    [Fact]
    public void MultipleOwners_PreserveInputOrderAndMultiplicityWithoutMutation()
    {
        var people = new List<EntitledPersonFacts> { Adult with { EntitledID = 17, SocialStatusID = SocialStatusConstants.Divorced }, Adult };
        var agents = new List<int> { 18, 8, 8 };
        var guardians = new List<int> { 19, 8 };
        var pledges = new List<int> { 20, 10 };
        var exemptions = new List<int> { 21, 11 };
        var facts = WithPeople() with { EntitledPersons = people, AgentIds = agents, GuardianIds = guardians,
            PledgeIds = pledges, ExemptionIds = exemptions };
        var before = facts;
        var peopleBefore = people.ToArray();
        var activeTypes = Enumerable.Range(1, 17).ToHashSet();
        var typesBefore = activeTypes.ToArray();
        var expected = new[]
        {
            FileRequirement(DocumentTypeConstants.FileOriginals), FileRequirement(DocumentTypeConstants.InheritanceCert),
            new RetirementDocumentRequirement(DocumentTypeConstants.DeathCertificate, 2, null, null, null, null, null),
            PersonRequirement(DocumentTypeConstants.NationalId, 17), PersonRequirement(DocumentTypeConstants.DivorceCertificate, 17),
            PersonRequirement(DocumentTypeConstants.NationalId, 7), PersonRequirement(DocumentTypeConstants.SingleStatusCert, 7),
            new(DocumentTypeConstants.NationalId, null, 18, null, null, null, null),
            new(DocumentTypeConstants.AgencyDocument, null, 18, null, null, null, null),
            new(DocumentTypeConstants.NationalId, null, 8, null, null, null, null),
            new(DocumentTypeConstants.AgencyDocument, null, 8, null, null, null, null),
            new(DocumentTypeConstants.NationalId, null, 8, null, null, null, null),
            new(DocumentTypeConstants.AgencyDocument, null, 8, null, null, null, null),
            new(DocumentTypeConstants.NationalId, null, null, 19, null, null, null),
            new(DocumentTypeConstants.GuardianshipDocument, null, null, 19, null, null, null),
            new(DocumentTypeConstants.NationalId, null, null, 8, null, null, null),
            new(DocumentTypeConstants.GuardianshipDocument, null, null, 8, null, null, null),
            new(DocumentTypeConstants.PledgeDocument, null, null, null, null, 20, null),
            new(DocumentTypeConstants.PledgeDocument, null, null, null, null, 10, null),
            new(DocumentTypeConstants.ExemptionDirective, null, null, null, null, null, 21),
            new(DocumentTypeConstants.ExemptionDirective, null, null, null, null, null, 11)
        };
        Assert.Equal(expected, SelectRequirements(facts, activeTypes, EvaluationDate));
        Assert.Equal(expected, SelectRequirements(facts, activeTypes, EvaluationDate));
        Assert.Equal(before, facts);
        Assert.Equal(peopleBefore, people);
        Assert.Equal(new[] { 18, 8, 8 }, agents);
        Assert.Equal(new[] { 19, 8 }, guardians);
        Assert.Equal(new[] { 20, 10 }, pledges);
        Assert.Equal(new[] { 21, 11 }, exemptions);
        Assert.Equal(typesBefore, activeTypes);
    }

    [Fact]
    public void AbsentOrInactiveTypes_AreExcludedForEveryOwnerCategory()
    {
        var facts = WithPeople(Adult) with { HasAnyPaymentBooks = true, AgentIds = [8], GuardianIds = [9], PledgeIds = [10], ExemptionIds = [11] };
        var baseline = Select(facts);
        foreach (var type in baseline.Select(r => r.DocumentTypeID).Distinct())
        {
            var active = Enumerable.Range(1, 17).Where(t => t != type).ToHashSet();
            Assert.Equal(baseline.Where(r => r.DocumentTypeID != type), SelectRequirements(facts, active, EvaluationDate));
        }
        Assert.Empty(SelectRequirements(facts, new HashSet<int>(), EvaluationDate));
        Assert.Empty(SelectRequirements(facts, new HashSet<int> { 999 }, EvaluationDate));
    }

    private static FileFacts WithPeople(params EntitledPersonFacts[] people) =>
        Empty with { Retiree = new RetireeFacts(2, false, HealthStatusConstants.Healthy), EntitledPersons = people };

    private static IReadOnlyList<RetirementDocumentRequirement> Select(FileFacts facts) =>
        SelectRequirements(facts, Enumerable.Range(1, 17).ToHashSet(), EvaluationDate);

    private static RetirementDocumentRequirement FileRequirement(int type) => new(type, null, null, null, null, null, null);
    private static RetirementDocumentRequirement PersonRequirement(int type, int id) => new(type, null, null, null, id, null, null);
}
