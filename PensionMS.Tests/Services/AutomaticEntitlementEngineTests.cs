using System.Linq.Expressions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class AutomaticEntitlementEngineTests
{
    private static readonly DateTime PreviousTime = new(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    [Theory]
    [InlineData("missing", false, "المستحق غير موجود")]
    [InlineData("deleted", true, "المستحق محذوف أو متوفى")]
    [InlineData("deceased", true, "المستحق محذوف أو متوفى")]
    [InlineData("missing-retiree", false, "بيانات المتقاعد غير موجودة")]
    public async Task EarlyReturn_PreservesAllStateAndDoesNotLoadEvidence(string scenario, bool success, string message)
    {
        var f = new Fixture();
        f.Person.StopCode = EntitledStopCodes.AdminDecision;
        f.File.Retiree = null!;
        if (scenario == "missing") f.Persons.Clear();
        if (scenario == "deleted") f.Person.IsDeleted = true;
        if (scenario == "deceased") f.Person.IsDeceased = true;
        var before = State(f.Person);

        var result = await f.Engine.ReEvaluateEntitlementStatusAsync(7);

        Assert.Equal(success, result.IsSuccess);
        Assert.Equal(message, result.Message);
        Assert.Equal(before, State(f.Person));
        Assert.Equal(new[] { typeof(EntitledPerson) }, f.Queries);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        Assert.True(f.Agent.IsActive);
        Assert.True(f.Direct.IsActive);
    }

    [Theory]
    [InlineData(EntitledStopCodes.AdminDecision, true)]
    [InlineData(EntitledStopCodes.AdminDecision, false)]
    [InlineData(EntitledStopCodes.AgeLimit, true)]
    [InlineData(EntitledStopCodes.DaughterMarried, false)]
    [InlineData("UNKNOWN", true)]
    [InlineData("", true)]
    public async Task ManualStop_PrecedesLifeStatusAndInvalidBirthAndPreservesStopFields(string code, bool retireeAlive)
    {
        var f = new Fixture();
        f.File.Retiree.IsAlive = retireeAlive;
        f.Person.StopCode = code;
        f.Person.BirthDate = default;

        var result = await f.Engine.ReEvaluateEntitlementStatusAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Equal("المستحق لديه إيقاف يدوي أو سبب آخر فلا يتم تفعيله تلقائياً", result.Message);
        Assert.False(f.Person.IsActive);
        Assert.Equal(code, f.Person.StopCode);
        Assert.Equal("سبب سابق", f.Person.StopReason);
        Assert.Equal(PreviousTime, f.Person.StopDate);
        Assert.Equal(PreviousTime, f.Person.UpdatedAt);
        AssertNoEvidenceQueries(f);
        AssertDeactivatedAndSaved(f);
    }

    [Theory]
    [InlineData(false, EntitledStopCodes.RetireeAlive, false)]
    [InlineData(true, EntitledStopCodes.RetireeAlive, true)]
    [InlineData(false, EntitledStopCodes.MissingData, true)]
    [InlineData(true, null, true)]
    public async Task AliveSuspension_OnlyRefreshesTimestampsWhenItsStateChanges(bool active, string? code, bool refresh)
    {
        var f = new Fixture();
        f.Person.IsActive = active;
        f.Person.StopCode = code;
        f.File.Retiree.IsAlive = true;
        f.Person.BirthDate = default;
        var before = DateTime.UtcNow;

        var result = await f.Engine.ReEvaluateEntitlementStatusAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Equal("تم تحديث الحالة: المتقاعد على قيد الحياة", result.Message);
        Assert.False(f.Person.IsActive);
        Assert.Equal(EntitledStopCodes.RetireeAlive, f.Person.StopCode);
        Assert.Equal(refresh ? "المتقاعد على قيد الحياة — الاستحقاق معلق" : "سبب سابق", f.Person.StopReason);
        if (refresh)
        {
            Assert.InRange(f.Person.StopDate!.Value, before, DateTime.UtcNow);
            Assert.InRange(f.Person.UpdatedAt!.Value, before, DateTime.UtcNow);
        }
        else
        {
            Assert.Equal(PreviousTime, f.Person.StopDate);
            Assert.Equal(PreviousTime, f.Person.UpdatedAt);
        }
        AssertNoEvidenceQueries(f);
        AssertDeactivatedAndSaved(f);
    }

    [Theory]
    [InlineData("age", EntitledStopCodes.AgeLimit, "المستحق غير مؤهل: تجاوز السن القانوني (24 سنة)")]
    [InlineData("marriage", EntitledStopCodes.DaughterMarried, "المستحق غير مؤهل: البنت متزوجة")]
    [InlineData("missing-birth", EntitledStopCodes.MissingData, "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية")]
    [InlineData("future-birth", EntitledStopCodes.MissingData, "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية")]
    public async Task LegalFailure_RefreshesOnlyStopDateAndShortCircuitsEvidence(string scenario, string code, string message)
    {
        var f = new Fixture();
        if (scenario == "age") f.Person.Relationship.RelationshipName = RelationshipConstants.Son;
        if (scenario == "marriage")
        {
            f.Person.Relationship.RelationshipName = RelationshipConstants.Daughter;
            f.Person.SocialStatusID = SocialStatusConstants.Married;
        }
        if (scenario == "missing-birth") f.Person.BirthDate = default;
        if (scenario == "future-birth") f.Person.BirthDate = YemenTime.Today.AddDays(1);
        f.Person.FullName = "";
        var before = DateTime.UtcNow;

        var result = await f.Engine.ReEvaluateEntitlementStatusAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Equal(message, result.Message);
        Assert.False(f.Person.IsActive);
        Assert.Equal(code, f.Person.StopCode);
        Assert.Equal(message, f.Person.StopReason);
        Assert.InRange(f.Person.StopDate!.Value, before, DateTime.UtcNow);
        Assert.Equal(PreviousTime, f.Person.UpdatedAt);
        AssertNoEvidenceQueries(f);
        AssertDeactivatedAndSaved(f);
    }

    [Theory]
    [InlineData("missing-documents")]
    [InlineData("covered-active")]
    [InlineData("covered-inactive")]
    [InlineData("mixed-deficiencies")]
    [InlineData("complete")]
    [InlineData("no-requirements")]
    public async Task Completion_PreservesQueryGatesStopDatesReviewAndLinkedEffects(string scenario)
    {
        var f = new Fixture();
        f.Person.IsActive = scenario != "covered-inactive";
        var coverageExpected = scenario is "missing-documents" or "covered-active";
        if (scenario == "mixed-deficiencies")
        {
            f.Person.FullName = " ";
            f.Person.SocialStatusID = 0;
            f.Person.HealthStatusID = null;
            f.Person.NationalID = "";
        }
        if (scenario == "complete")
        {
            f.Upload(DocumentTypeConstants.NationalId);
            f.Upload(DocumentTypeConstants.DivorceCertificate);
        }
        if (scenario == "no-requirements")
        {
            f.Person.BirthDate = YemenTime.Today.AddYears(-17);
            f.Person.SocialStatusID = SocialStatusConstants.Single;
        }
        if (scenario.StartsWith("covered"))
        {
            foreach (var type in new[] { DocumentTypeConstants.NationalId, DocumentTypeConstants.DivorceCertificate })
                f.Missing.Add(new MissingDocument { FileID = 1, EntitledID = 7, DocumentTypeID = type,
                    PledgeID = 10, Status = MissingDocumentStatusConstants.Pledged });
            f.Pledges.Add(new DocumentPledge { FileID = 1, PledgeID = 10, Deadline = DateTime.UtcNow.AddDays(1) });
        }
        var activated = scenario is "covered-active" or "complete" or "no-requirements";
        var before = DateTime.UtcNow;

        var result = await f.Engine.ReEvaluateEntitlementStatusAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Equal(activated ? "تم تفعيل المستحق بنجاح" : "المستحق لا يزال لديه نواقص", result.Message);
        Assert.Equal(activated, f.Person.IsActive);
        Assert.InRange(f.Person.UpdatedAt!.Value, before, DateTime.UtcNow);
        Assert.Equal(activated ? null : PreviousTime, f.Person.StopDate);
        Assert.Equal(activated ? null : EntitledStopCodes.MissingData, f.Person.StopCode);
        Assert.Equal(activated ? null : scenario == "mixed-deficiencies"
            ? "بيانات/وثائق ناقصة: الاسم الكامل، الحالة الاجتماعية، الحالة الصحية، الرقم الوطني، وثيقة الهوية الوطنية"
            : "بيانات/وثائق ناقصة: وثيقة الهوية الوطنية، وثيقة الطلاق", f.Person.StopReason);
        Assert.Equal(coverageExpected, f.Queries.Contains(typeof(MissingDocument)));
        Assert.Equal(coverageExpected, f.Queries.Contains(typeof(DocumentPledge)));
        Assert.Equal(coverageExpected, f.Queries.Contains(typeof(DocumentExemption)));
        Assert.Equal(scenario != "no-requirements", f.Queries.Contains(typeof(DocumentType)));
        foreach (var type in new[] { typeof(DocumentType), typeof(MissingDocument), typeof(DocumentPledge), typeof(DocumentExemption) })
            Assert.InRange(f.Queries.Count(t => t == type), 0, 1);
        if (activated)
        {
            Assert.True(f.Agent.IsActive);
            Assert.True(f.Direct.IsActive);
            Assert.True(f.ViaAgent.IsActive);
            f.Unit.Verify(u => u.SaveChangesAsync(), Times.Once);
            AssertReviewPreserved(f);
        }
        else AssertDeactivatedAndSaved(f);
    }

    private static object State(EntitledPerson p) =>
        (p.IsActive, p.StopCode, p.StopReason, p.StopDate, p.UpdatedAt, p.PendingReviewStatus);

    private static void AssertNoEvidenceQueries(Fixture f) => Assert.DoesNotContain(f.Queries,
        t => t == typeof(DocumentType) || t == typeof(MissingDocument) || t == typeof(DocumentPledge) || t == typeof(DocumentExemption));

    private static void AssertReviewPreserved(Fixture f)
    {
        Assert.Equal(PendingReviewStatus.Rejected, f.Person.PendingReviewStatus);
        Assert.Equal(15, f.Person.UpdatedByUserID);
    }

    private static void AssertDeactivatedAndSaved(Fixture f)
    {
        Assert.False(f.Agent.IsActive);
        Assert.False(f.Direct.IsActive);
        Assert.False(f.ViaAgent.IsActive);
        Assert.NotNull(f.Direct.EndDate);
        Assert.NotNull(f.ViaAgent.EndDate);
        Assert.True(f.OtherAgent.IsActive);
        Assert.True(f.OtherRecipient.IsActive);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Once);
        AssertReviewPreserved(f);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public List<Type> Queries { get; } = [];
        public List<EntitledPerson> Persons { get; } = [];
        public List<MissingDocument> Missing { get; } = [];
        public List<DocumentPledge> Pledges { get; } = [];
        public EntitledPerson Person { get; }
        public RetirementFile File { get; } = new() { FileID = 1, Retiree = new Retiree { IsAlive = false } };
        public Agent Agent { get; } = new() { AgentID = 8, EntitledID = 7, IsActive = true };
        public Agent OtherAgent { get; } = new() { AgentID = 9, EntitledID = 99, IsActive = true };
        public SalaryRecipient Direct { get; } = new() { EntitledID = 7, IsActive = true };
        public SalaryRecipient ViaAgent { get; } = new() { AgentID = 8, IsActive = true };
        public SalaryRecipient OtherRecipient { get; } = new() { EntitledID = 99, IsActive = true };
        public EntitlementEngine Engine { get; }

        public Fixture()
        {
            Person = new EntitledPerson { EntitledID = 7, FileID = 1, RetirementFile = File,
                IsActive = true, StopCode = null, StopReason = "سبب سابق", StopDate = PreviousTime, UpdatedAt = PreviousTime,
                FullName = "مستحق", BirthDate = YemenTime.Today.AddYears(-30), NationalID = "123456789",
                RelationshipID = 1, Relationship = new Relationship { RelationshipName = RelationshipConstants.Wife },
                SocialStatusID = SocialStatusConstants.Divorced, HealthStatusID = HealthStatusConstants.Healthy,
                PendingReviewStatus = PendingReviewStatus.Rejected, UpdatedByUserID = 15 };
            Persons.Add(Person);
            Bind(u => u.EntitledPersons, Persons);
            Bind(u => u.DocumentTypes, new[] { new DocumentType { DocumentTypeID = DocumentTypeConstants.NationalId, IsActive = true },
                new DocumentType { DocumentTypeID = DocumentTypeConstants.DivorceCertificate, IsActive = true } });
            Bind(u => u.MissingDocuments, Missing);
            Bind(u => u.DocumentPledges, Pledges);
            Bind<DocumentExemption>(u => u.DocumentExemptions, []);
            Bind(u => u.Agents, new[] { Agent, OtherAgent });
            Bind(u => u.SalaryRecipients, new[] { Direct, ViaAgent, OtherRecipient });
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Engine = new EntitlementEngine(Unit.Object);
        }

        public void Upload(int type) => Person.Documents.Add(new Document { FileID = 1, EntitledID = 7, DocumentTypeID = type });

        private void Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, IEnumerable<T> rows) where T : class
        {
            var repo = new Mock<IRepository<T>>();
            repo.Setup(r => r.Query()).Returns(() => { Queries.Add(typeof(T)); return new TestAsyncEnumerable<T>(rows); });
            Unit.Setup(property).Returns(repo.Object);
        }
    }
}
