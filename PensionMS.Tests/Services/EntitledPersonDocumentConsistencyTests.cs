using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Rules.Entitlements;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class EntitledPersonDocumentConsistencyTests
{
    public static IEnumerable<object[]> RequirementCases()
    {
        // Expected lists pin the existing missing-document policy, not the former engine shortcut.
        foreach (var relationship in new[] { 1, 3, 4, 6 })
            yield return [relationship, 30, SocialStatusConstants.Single, HealthStatusConstants.Healthy, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.SingleStatusCert }];
        foreach (var relationship in new[] { 2, 5, 7, 8 })
            yield return [relationship, 20, SocialStatusConstants.Single, HealthStatusConstants.Healthy, false,
                new[] { DocumentTypeConstants.NationalId }];
        yield return [3, 17, SocialStatusConstants.Single, HealthStatusConstants.Healthy, false, Array.Empty<int>()];
        yield return [3, 18, SocialStatusConstants.Single, HealthStatusConstants.Healthy, false,
            new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.SingleStatusCert }];
        yield return [1, 30, SocialStatusConstants.Married, HealthStatusConstants.Healthy, false,
            new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.MarriageContract }];
        yield return [1, 30, SocialStatusConstants.Divorced, HealthStatusConstants.Healthy, false,
            new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.DivorceCertificate }];
        yield return [3, 17, SocialStatusConstants.Divorced, HealthStatusConstants.Healthy, false,
            new[] { DocumentTypeConstants.DivorceCertificate }];
        yield return [2, 30, SocialStatusConstants.Single, HealthStatusConstants.Healthy, true,
            new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.MedicalReport, DocumentTypeConstants.MaintenanceJudgment }];
        foreach (var health in new[] { HealthStatusConstants.PartiallyDisabled, HealthStatusConstants.FullyDisabled })
            yield return [5, 30, SocialStatusConstants.Single, health, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.MedicalReport, DocumentTypeConstants.MaintenanceJudgment }];
        foreach (var health in new[] { HealthStatusConstants.Deceased, HealthStatusConstants.Martyr })
        {
            yield return [1, 30, SocialStatusConstants.Divorced, health, false,
                new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.DeathCertificate }];
            yield return [5, 30, SocialStatusConstants.Single, health, false,
                new[] { DocumentTypeConstants.NationalId }];
        }
    }

    [Theory]
    [MemberData(nameof(RequirementCases))]
    public async Task Requirements_AndAutomaticActivation_UseTheSameDocumentPolicy(
        int relationship, int age, int social, int health, bool disability, int[] expected)
    {
        var f = new Fixture(relationship, age, social, health, disability);
        var profile = new EntitledPersonRules.EntitledPersonProfile(relationship, age, social, health, disability);
        Assert.Equal(expected.Order(), EntitledPersonRules.DocumentRequirements(profile).Select(r => r.DocumentTypeID).Order());
        var pairs = (await f.Missing.GetRequiredPairsAsync(1)).Where(p => p.EntitledID == f.Person.EntitledID);
        Assert.Equal(expected.Order(), pairs.Select(p => p.DocumentTypeID).Order());
        Assert.Equal(expected.Length, (await f.Missing.GetComplianceSummaryAsync(1)).MissingCount);
        Assert.True((await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID)).IsSuccess);
        Assert.Equal(expected.Length == 0, f.Person.IsActive);
        foreach (var type in expected) f.Upload(type);
        Assert.True((await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID)).IsSuccess);
        Assert.True(f.Person.IsActive);
        Assert.Null(f.Person.StopCode);
        Assert.Equal(0, (await f.Missing.GetComplianceSummaryAsync(1)).MissingCount);
    }

    [Theory]
    [InlineData("valid", true)]
    [InlineData("expires-today", true)]
    [InlineData("pending-review", true)]
    [InlineData("expired", false)]
    [InlineData("rejected", false)]
    [InlineData("deleted", false)]
    [InlineData("wrong-owner", false)]
    [InlineData("mixed-owner", false)]
    [InlineData("wrong-file", false)]
    public async Task Reassessment_AndCompliance_RequireValidProofForTheExactOwner(string scenario, bool valid)
    {
        var f = new Fixture();
        f.Upload(DocumentTypeConstants.NationalId);
        var proof = f.Upload(DocumentTypeConstants.DivorceCertificate);
        if (scenario == "expires-today") proof.ExpiryDate = YemenTime.Today;
        if (scenario == "expired") proof.ExpiryDate = YemenTime.Today.AddDays(-1);
        if (scenario == "rejected") proof.PendingReviewStatus = PendingReviewStatus.Rejected;
        if (scenario == "pending-review") proof.PendingReviewStatus = PendingReviewStatus.Pending;
        if (scenario == "deleted") proof.IsDeleted = true;
        if (scenario == "wrong-owner") proof.EntitledID = 999;
        if (scenario == "mixed-owner") proof.AgentID = 999;
        if (scenario == "wrong-file") proof.FileID = 2;

        var automatic = (await f.Missing.GetAutoDetectedByFileAsync(1)).Data!;
        Assert.Equal(!valid, automatic.Any(p => p.EntitledID == f.Person.EntitledID &&
            p.DocumentTypeID == DocumentTypeConstants.DivorceCertificate));
        Assert.Equal(valid ? 0 : 1, (await f.Missing.GetComplianceSummaryAsync(1)).MissingCount);
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.Equal(valid, f.Person.IsActive);
        if (!valid)
        {
            Assert.Equal(EntitledStopCodes.MissingData, f.Person.StopCode);
            Assert.Contains("الطلاق", f.Person.StopReason);
            // Invalid historical uploads must not prevent a valid replacement from satisfying the requirement.
            f.Upload(DocumentTypeConstants.DivorceCertificate);
            await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
            Assert.True(f.Person.IsActive);
            Assert.Equal(0, (await f.Missing.GetComplianceSummaryAsync(1)).MissingCount);
        }
    }

    [Theory]
    [InlineData(HealthStatusConstants.Healthy, true)]
    [InlineData(HealthStatusConstants.PartiallyDisabled, false)]
    [InlineData(HealthStatusConstants.FullyDisabled, false)]
    public async Task Disability_RequiresMaintenanceJudgmentEvenWhenMedicalReportExists(int health, bool exception)
    {
        var f = new Fixture(5, 30, SocialStatusConstants.Single, health, exception);
        f.Upload(DocumentTypeConstants.NationalId);
        f.Upload(DocumentTypeConstants.MedicalReport);
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.False(f.Person.IsActive);
        Assert.Contains("حكم الإعالة", f.Person.StopReason);
        Assert.Equal(new[] { DocumentTypeConstants.MaintenanceJudgment },
            (await f.Missing.GetComplianceSummaryAsync(1)).MissingDocumentTypeIds);
        f.Upload(DocumentTypeConstants.MaintenanceJudgment);
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.True(f.Person.IsActive);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Coverage_DoesNotReplaceActualProofForAutomaticActivation(bool exemption)
    {
        var f = new Fixture();
        f.Upload(DocumentTypeConstants.NationalId);
        f.ManualMissing.Add(new MissingDocument
        {
            FileID = 1, EntitledID = f.Person.EntitledID, DocumentTypeID = DocumentTypeConstants.DivorceCertificate,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null
        });
        if (exemption)
        {
            f.Exemptions.Add(new DocumentExemption { FileID = 1, ExemptionID = 11 });
            f.Documents.Add(new Document { FileID = 1, DocumentTypeID = DocumentTypeConstants.ExemptionDirective, ExemptionID = 11 });
        }
        else
        {
            f.Pledges.Add(new DocumentPledge { FileID = 1, PledgeID = 10, Deadline = DateTime.UtcNow.AddDays(1) });
            f.Documents.Add(new Document { FileID = 1, DocumentTypeID = DocumentTypeConstants.PledgeDocument, PledgeID = 10 });
        }
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.False(f.Person.IsActive);
        Assert.Equal(EntitledStopCodes.MissingData, f.Person.StopCode);
        Assert.Equal(1, (await f.Missing.GetComplianceSummaryAsync(1)).MissingCount);
        f.Upload(DocumentTypeConstants.DivorceCertificate);
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.True(f.Person.IsActive);
    }

    [Theory]
    [InlineData("alive")]
    [InlineData("deleted")]
    [InlineData("deceased")]
    [InlineData("age-limit")]
    [InlineData("daughter-married")]
    public async Task RequirementDiscovery_PreservesExistingContextExclusions(string scenario)
    {
        var f = new Fixture();
        if (scenario == "alive") f.File.Retiree.IsAlive = true;
        if (scenario == "deleted") f.Person.IsDeleted = true;
        if (scenario == "deceased") f.Person.IsDeceased = true;
        if (scenario == "age-limit") f.Person.StopCode = EntitledStopCodes.AgeLimit;
        if (scenario == "daughter-married") f.Person.StopCode = EntitledStopCodes.DaughterMarried;
        Assert.DoesNotContain(await f.Missing.GetRequiredPairsAsync(1), p => p.EntitledID == f.Person.EntitledID);
    }

    [Theory]
    [InlineData("alive")]
    [InlineData("age-limit")]
    [InlineData("daughter-married")]
    [InlineData("manual-stop")]
    [InlineData("pending")]
    [InlineData("rejected")]
    public async Task CompleteDocuments_DoNotBypassExistingEligibilityOrLifecycleGuards(string scenario)
    {
        var f = new Fixture();
        f.Upload(DocumentTypeConstants.NationalId);
        f.Upload(DocumentTypeConstants.DivorceCertificate);
        f.Upload(DocumentTypeConstants.MarriageContract);
        if (scenario == "alive") f.File.Retiree.IsAlive = true;
        if (scenario == "age-limit")
        {
            f.Person.RelationshipID = 2;
            f.Person.Relationship.RelationshipName = RelationshipConstants.Son;
        }
        if (scenario == "daughter-married")
        {
            f.Person.RelationshipID = 3;
            f.Person.Relationship.RelationshipName = RelationshipConstants.Daughter;
            f.Person.SocialStatusID = SocialStatusConstants.Married;
        }
        if (scenario == "manual-stop") f.Person.StopCode = EntitledStopCodes.AdminDecision;
        if (scenario == "pending") f.Person.PendingReviewStatus = PendingReviewStatus.Pending;
        if (scenario == "rejected") f.Person.PendingReviewStatus = PendingReviewStatus.Rejected;
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.Equal(scenario is "pending" or "rejected", f.Person.IsActive);
        if (scenario == "pending") Assert.Equal(PendingReviewStatus.Pending, f.Person.PendingReviewStatus);
        if (scenario == "rejected") Assert.Equal(PendingReviewStatus.Rejected, f.Person.PendingReviewStatus);
        if (scenario == "manual-stop") Assert.Equal(EntitledStopCodes.AdminDecision, f.Person.StopCode);
    }

    [Fact]
    public async Task InactiveDocumentTypes_AreExcludedConsistentlyWithRequirementDiscovery()
    {
        var f = new Fixture();
        f.Upload(DocumentTypeConstants.NationalId);
        f.Types.Single(t => t.DocumentTypeID == DocumentTypeConstants.DivorceCertificate).IsActive = false;
        Assert.DoesNotContain(await f.Missing.GetRequiredPairsAsync(1), p =>
            p.EntitledID == f.Person.EntitledID && p.DocumentTypeID == DocumentTypeConstants.DivorceCertificate);
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.True(f.Person.IsActive);
        Assert.Equal(0, (await f.Missing.GetComplianceSummaryAsync(1)).MissingCount);
    }

    [Fact]
    public async Task Reassessment_AppliesOnlyToTheRequestedPerson()
    {
        var f = new Fixture();
        var other = new EntitledPerson { EntitledID = 8, FileID = 1, IsActive = true };
        f.Persons.Add(other);
        await f.Engine.ReEvaluateEntitlementStatusAsync(f.Person.EntitledID);
        Assert.False(f.Person.IsActive);
        Assert.True(other.IsActive);
        Assert.Null(other.StopCode);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ExistingActivation_RetainsOnlyValidCoverageWithoutLiftingReview(bool exemption, bool expired)
    {
        var f = new Fixture();
        f.Person.IsActive = true;
        f.Person.StopCode = null;
        f.Person.PendingReviewStatus = PendingReviewStatus.Pending;
        f.Upload(DocumentTypeConstants.NationalId);
        f.ManualMissing.Add(new MissingDocument { FileID = 1, EntitledID = 7,
            DocumentTypeID = DocumentTypeConstants.DivorceCertificate,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null });
        f.Pledges.Add(new DocumentPledge { FileID = 1, PledgeID = 10, Deadline = DateTime.UtcNow.AddDays(expired ? -1 : 1) });
        f.Exemptions.Add(new DocumentExemption { FileID = 1, ExemptionID = 11 });
        await f.Engine.ReEvaluateEntitlementStatusAsync(7);
        Assert.Equal(!expired, f.Person.IsActive);
        Assert.Equal(PendingReviewStatus.Pending, f.Person.PendingReviewStatus);
        Assert.Contains(DocumentTypeConstants.DivorceCertificate,
            (await f.Missing.GetComplianceSummaryAsync(1)).MissingDocumentTypeIds);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public EntitledPerson Person { get; }
        public RetirementFile File { get; }
        public List<EntitledPerson> Persons { get; } = [];
        public List<Document> Documents { get; } = [];
        public List<DocumentType> Types { get; } = [];
        public List<MissingDocument> ManualMissing { get; } = [];
        public List<DocumentPledge> Pledges { get; } = [];
        public List<DocumentExemption> Exemptions { get; } = [];
        public EntitlementEngine Engine { get; }
        public MissingDocumentService Missing { get; }

        public Fixture(int relationship = 1, int age = 30, int social = SocialStatusConstants.Divorced,
            int health = HealthStatusConstants.Healthy, bool disability = false)
        {
            var retiree = new Retiree { RetireeID = 2, FullName = "متقاعد", IsAlive = false };
            File = new RetirementFile { FileID = 1, RetireeID = 2, Retiree = retiree };
            retiree.RetirementFiles.Add(File);
            Person = new EntitledPerson
            {
                EntitledID = 7, FileID = 1, RetirementFile = File, FullName = "مستحق", NationalID = "123456789",
                BirthDate = YemenTime.Today.AddYears(-age), RelationshipID = relationship,
                Relationship = new Relationship { RelationshipID = relationship,
                    RelationshipName = relationship == 2 ? RelationshipConstants.Son :
                        relationship == 3 ? RelationshipConstants.Daughter : RelationshipConstants.Wife },
                SocialStatusID = social, HealthStatusID = health, HasDisabilityException = disability,
                IsActive = false, StopCode = EntitledStopCodes.MissingData
            };
            Persons.Add(Person);
            foreach (var id in new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.SingleStatusCert,
                DocumentTypeConstants.MarriageContract, DocumentTypeConstants.DivorceCertificate,
                DocumentTypeConstants.MedicalReport, DocumentTypeConstants.MaintenanceJudgment,
                DocumentTypeConstants.DeathCertificate, DocumentTypeConstants.FileOriginals,
                DocumentTypeConstants.InheritanceCert, DocumentTypeConstants.PledgeDocument,
                DocumentTypeConstants.ExemptionDirective })
                Types.Add(new DocumentType { DocumentTypeID = id, TypeName = "وثيقة " + id, IsActive = true });
            Documents.Add(new Document { FileID = 1, DocumentTypeID = DocumentTypeConstants.FileOriginals });
            Documents.Add(new Document { FileID = 1, DocumentTypeID = DocumentTypeConstants.InheritanceCert });
            Documents.Add(new Document { FileID = 1, RetireeID = 2, DocumentTypeID = DocumentTypeConstants.DeathCertificate });
            Bind(u => u.RetirementFiles, new[] { File });
            Bind(u => u.Retirees, new[] { retiree });
            Bind(u => u.EntitledPersons, Persons);
            Bind(u => u.Documents, Documents);
            Bind(u => u.DocumentTypes, Types);
            Bind(u => u.MissingDocuments, ManualMissing);
            Bind(u => u.DocumentPledges, Pledges);
            Bind(u => u.DocumentExemptions, Exemptions);
            Bind<Agent>(u => u.Agents, []);
            Bind<Guardian>(u => u.Guardians, []);
            Bind<PaymentBook>(u => u.PaymentBooks, []);
            Bind<SalaryRecipient>(u => u.SalaryRecipients, []);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Engine = new EntitlementEngine(Unit.Object);
            Missing = new MissingDocumentService(Unit.Object, Engine, NullLogger<MissingDocumentService>.Instance,
                TestPensionAccess.All(Unit.Object));
        }

        public Document Upload(int type)
        {
            var document = new Document { FileID = 1, EntitledID = Person.EntitledID, DocumentTypeID = type,
                OwnerType = DocumentOwnerType.EntitledPerson };
            Documents.Add(document);
            Person.Documents.Add(document);
            return document;
        }

        private void Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, IEnumerable<T> rows) where T : class
        {
            var repo = new Mock<IRepository<T>>();
            repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows));
            Unit.Setup(property).Returns(repo.Object);
        }
    }
}
