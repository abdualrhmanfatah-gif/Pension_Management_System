using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Rules.Documents;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class RequiredDocumentSelectionServiceTests
{
    public static IEnumerable<object?[]> RetireeContexts()
    {
        foreach (var alive in new[] { true, false })
        foreach (int? health in new int?[] { null, 0, HealthStatusConstants.Healthy,
            HealthStatusConstants.PartiallyDisabled, HealthStatusConstants.FullyDisabled,
            HealthStatusConstants.Deceased, HealthStatusConstants.Martyr, 99 })
            yield return [alive, health];
    }

    [Theory]
    [MemberData(nameof(RetireeContexts))]
    public async Task RequiredPairs_PreserveLifeAndHealthCombinationsAndExactOwnerOrder(bool alive, int? health)
    {
        var f = new Fixture();
        f.Retiree.IsAlive = alive;
        f.Retiree.HealthStatusID = health;
        var expected = new List<RetirementDocumentRequirement> { FileRequirement(DocumentTypeConstants.FileOriginals) };
        var deathEvidence = !alive || health is HealthStatusConstants.Deceased or HealthStatusConstants.Martyr;
        if (deathEvidence) expected.Add(FileRequirement(DocumentTypeConstants.InheritanceCert));
        if (alive) expected.Add(RetireeRequirement(DocumentTypeConstants.NationalId));
        if (deathEvidence) expected.Add(RetireeRequirement(DocumentTypeConstants.DeathCertificate));
        if (health is HealthStatusConstants.PartiallyDisabled or HealthStatusConstants.FullyDisabled)
            expected.Add(RetireeRequirement(DocumentTypeConstants.MedicalReport));
        if (!alive) expected.Add(new(DocumentTypeConstants.NationalId, null, null, null, 7, null, null));

        var pairs = await f.Service.GetRequiredPairsAsync(1);

        Assert.Equal(expected, pairs.Select(RetirementFileDocumentRules.Owner));
        Assert.All(pairs, p => { Assert.True(p.IsRequired); Assert.True(p.IsAutoDetected); Assert.Equal("وثيقة " + p.DocumentTypeID, p.DocumentTypeName); });
        Assert.Equal("الملف العام", pairs[0].EntityName);
        Assert.All(pairs.Where(p => p.RetireeID.HasValue), p => Assert.Equal("متقاعد", p.EntityName));
        Assert.All(pairs.Where(p => p.EntitledID.HasValue), p => Assert.Equal("مستحق", p.EntityName));
        Assert.Single(f.Queries.Where(t => t == typeof(DocumentType)));
        Assert.All(f.Queries.GroupBy(t => t), group => Assert.Single(group));
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingRetiree_RequiresOnlyFileEvidenceAndOptionalExistingBookImage(bool book)
    {
        var f = new Fixture();
        f.Retirees.Clear();
        if (book) f.Books.Add(new PaymentBook { FileID = 1 });
        var pairs = await f.Service.GetRequiredPairsAsync(1);
        Assert.Equal(book ? new[] { DocumentTypeConstants.FileOriginals, DocumentTypeConstants.PaymentBookImage }
            : new[] { DocumentTypeConstants.FileOriginals }, pairs.Select(p => p.DocumentTypeID));
        Assert.All(pairs, p => Assert.Equal(EntityTypeConstants.File, p.EntityType));
        if (book) Assert.Equal("دفتر الصرف", pairs[1].EntityName);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(EntitledStopCodes.MissingData, true)]
    [InlineData(EntitledStopCodes.RetireeAlive, true)]
    [InlineData(EntitledStopCodes.AdminDecision, true)]
    [InlineData(EntitledStopCodes.WidowRemarried, true)]
    [InlineData(EntitledStopCodes.Deceased, true)]
    [InlineData(EntitledStopCodes.AgeLimit, false)]
    [InlineData(EntitledStopCodes.DaughterMarried, false)]
    [InlineData("UNKNOWN", true)]
    [InlineData("", true)]
    public async Task EntitledStopFiltering_DoesNotInventOtherExclusions(string? stopCode, bool included)
    {
        var f = new Fixture();
        f.Retiree.IsAlive = false;
        f.People[0].StopCode = stopCode;
        Assert.Equal(included, (await f.Service.GetRequiredPairsAsync(1)).Any(p => p.EntitledID == 7));
    }

    [Fact]
    public async Task SelectedSources_KeepScopeOrderingAndDisplayNameFallbacks()
    {
        var f = new Fixture();
        f.People.AddRange([new EntitledPerson { EntitledID = 70, FileID = 1, IsDeceased = true },
            new EntitledPerson { EntitledID = 71, FileID = 1, IsDeleted = true },
            new EntitledPerson { EntitledID = 72, FileID = 2 }]);
        f.Agents.AddRange([new Agent { AgentID = 8, FileID = 1, IsActive = true, FullName = "اسم مشترك" },
            new Agent { AgentID = 18, FileID = 1, IsActive = true, FullName = "اسم مشترك" },
            new Agent { AgentID = 80, FileID = 1, IsActive = false },
            new Agent { AgentID = 81, FileID = 1, IsActive = true, IsDeleted = true },
            new Agent { AgentID = 82, FileID = 2, IsActive = true }]);
        f.Guardians.AddRange([new Guardian { GuardianID = 8, FileID = 1, IsActive = true, FullName = "اسم مشترك" },
            new Guardian { GuardianID = 19, FileID = 1, IsActive = true, FullName = "اسم مشترك" },
            new Guardian { GuardianID = 90, FileID = 1, IsActive = false },
            new Guardian { GuardianID = 91, FileID = 1, IsActive = true, IsDeleted = true },
            new Guardian { GuardianID = 92, FileID = 2, IsActive = true }]);
        f.Pledges.AddRange([new DocumentPledge { PledgeID = 10, FileID = 1, PledgedByName = "اسم متعهد" },
            new DocumentPledge { PledgeID = 20, FileID = 1, PledgeNumber = "رقم تعهد", Deadline = DateTime.UtcNow.AddDays(-1), Status = PledgeStatusConstants.Expired },
            new DocumentPledge { PledgeID = 30, FileID = 1 },
            new DocumentPledge { PledgeID = 40, FileID = 1, PledgedByName = "", PledgeNumber = "لا يستخدم" },
            new DocumentPledge { PledgeID = 50, FileID = 1, Status = MissingDocumentStatusConstants.Fulfilled },
            new DocumentPledge { PledgeID = 60, FileID = 1, IsDeleted = true },
            new DocumentPledge { PledgeID = 70, FileID = 2 }]);
        f.Exemptions.AddRange([new DocumentExemption { ExemptionID = 11, FileID = 1, DirectiveSource = "مصدر" },
            new DocumentExemption { ExemptionID = 21, FileID = 1, DirectiveSource = "" },
            new DocumentExemption { ExemptionID = 31, FileID = 1, DirectiveSource = " " },
            new DocumentExemption { ExemptionID = 41, FileID = 2 }]);
        f.Books.AddRange([new PaymentBook { FileID = 2 }, new PaymentBook { FileID = 1, IsDeleted = true }]);

        var pairs = await f.Service.GetRequiredPairsAsync(1);

        Assert.Equal(new[] { EntityTypeConstants.File, EntityTypeConstants.Retiree,
            EntityTypeConstants.Agent, EntityTypeConstants.Agent, EntityTypeConstants.Agent, EntityTypeConstants.Agent,
            EntityTypeConstants.Guardian, EntityTypeConstants.Guardian, EntityTypeConstants.Guardian, EntityTypeConstants.Guardian,
            EntityTypeConstants.Pledge, EntityTypeConstants.Pledge, EntityTypeConstants.Pledge, EntityTypeConstants.Pledge,
            EntityTypeConstants.Exemption, EntityTypeConstants.Exemption, EntityTypeConstants.Exemption }, pairs.Select(p => p.EntityType));
        Assert.Equal(new int?[] { 8, 8, 18, 18 }, pairs.Where(p => p.AgentID.HasValue).Select(p => p.AgentID));
        Assert.Equal(new int?[] { 8, 8, 19, 19 }, pairs.Where(p => p.GuardianID.HasValue).Select(p => p.GuardianID));
        Assert.Equal(new[] { "اسم متعهد", "رقم تعهد", "تعهد نشط", "" }, pairs.Where(p => p.PledgeID.HasValue).Select(p => p.EntityName));
        Assert.Equal(new[] { "مصدر", "إعفاء نشط", " " }, pairs.Where(p => p.ExemptionID.HasValue).Select(p => p.EntityName));
        Assert.DoesNotContain(pairs, p => p.DocumentTypeID == DocumentTypeConstants.PaymentBookImage || p.EntitledID.HasValue);
    }

    [Fact]
    public async Task IdenticalEntitledNames_KeepSeparateOwnersAndPerPersonRequirementOrder()
    {
        var f = new Fixture();
        f.Retiree.IsAlive = false;
        f.People[0].FullName = "اسم مشترك";
        f.People[0].RelationshipID = 3;
        f.People[0].SocialStatusID = SocialStatusConstants.Divorced;
        f.People.Add(new EntitledPerson { EntitledID = 17, FileID = 1, FullName = "اسم مشترك",
            RelationshipID = 3, SocialStatusID = SocialStatusConstants.Divorced, BirthDate = YemenTime.Today.AddYears(-18) });
        var pairs = (await f.Service.GetRequiredPairsAsync(1)).Where(p => p.EntitledID.HasValue).ToArray();
        Assert.Equal(new int?[] { 7, 7, 17, 17 }, pairs.Select(p => p.EntitledID));
        Assert.Equal(new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.DivorceCertificate,
            DocumentTypeConstants.NationalId, DocumentTypeConstants.DivorceCertificate }, pairs.Select(p => p.DocumentTypeID));
        Assert.All(pairs, p => Assert.Equal("اسم مشترك", p.EntityName));
    }

    [Fact]
    public async Task InactiveOrAbsentTypes_AreOmittedFromThePublicContract()
    {
        var f = new Fixture();
        f.Types.Single(t => t.DocumentTypeID == DocumentTypeConstants.FileOriginals).IsActive = false;
        f.Types.RemoveAll(t => t.DocumentTypeID == DocumentTypeConstants.NationalId);
        Assert.Empty(await f.Service.GetRequiredPairsAsync(1));
    }

    private static RetirementDocumentRequirement FileRequirement(int type) => new(type, null, null, null, null, null, null);
    private static RetirementDocumentRequirement RetireeRequirement(int type) => new(type, 2, null, null, null, null, null);

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Retiree Retiree { get; } = new() { RetireeID = 2, IsAlive = true, FullName = "متقاعد" };
        public List<Retiree> Retirees { get; } = [];
        public List<EntitledPerson> People { get; } = [];
        public List<Agent> Agents { get; } = [];
        public List<Guardian> Guardians { get; } = [];
        public List<PaymentBook> Books { get; } = [];
        public List<DocumentPledge> Pledges { get; } = [];
        public List<DocumentExemption> Exemptions { get; } = [];
        public List<DocumentType> Types { get; } = [];
        public List<Type> Queries { get; } = [];
        public MissingDocumentService Service { get; }

        public Fixture()
        {
            var file = new RetirementFile { FileID = 1, RetireeID = 2, Retiree = Retiree };
            Retiree.RetirementFiles.Add(file);
            Retirees.Add(Retiree);
            People.Add(new EntitledPerson { EntitledID = 7, FileID = 1, FullName = "مستحق", RelationshipID = 5,
                BirthDate = YemenTime.Today.AddYears(-30), SocialStatusID = SocialStatusConstants.Single });
            Types.AddRange(Enumerable.Range(1, 17).Select(id => new DocumentType { DocumentTypeID = id, TypeName = "وثيقة " + id, IsActive = true }));
            Bind(u => u.RetirementFiles, new[] { file });
            Bind(u => u.Retirees, Retirees);
            Bind(u => u.EntitledPersons, People);
            Bind(u => u.Agents, Agents);
            Bind(u => u.Guardians, Guardians);
            Bind(u => u.PaymentBooks, Books);
            Bind(u => u.DocumentPledges, Pledges);
            Bind(u => u.DocumentExemptions, Exemptions);
            Bind(u => u.DocumentTypes, Types);
            Bind<MissingDocument>(u => u.MissingDocuments, []);
            Bind<Document>(u => u.Documents, []);
            Service = new MissingDocumentService(Unit.Object, Mock.Of<IEntitlementEngine>(),
                NullLogger<MissingDocumentService>.Instance, TestPensionAccess.All(Unit.Object));
        }

        private void Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, IEnumerable<T> rows) where T : class
        {
            var repository = new Mock<IRepository<T>>();
            repository.Setup(r => r.Query()).Returns(() => { Queries.Add(typeof(T)); return new TestAsyncEnumerable<T>(rows); });
            Unit.Setup(property).Returns(repository.Object);
        }
    }
}
