using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class EntitledPersonManualDocumentTests
{
    [Theory]
    [InlineData(EntitledStopCodes.DaughterMarried, RelationshipConstants.Daughter, DocumentTypeConstants.MarriageContract)]
    [InlineData(EntitledStopCodes.WidowRemarried, RelationshipConstants.Wife, DocumentTypeConstants.MarriageContract)]
    [InlineData(EntitledStopCodes.Deceased, RelationshipConstants.Son, DocumentTypeConstants.DeathCertificate)]
    [InlineData(EntitledStopCodes.AgeLimit, RelationshipConstants.Son, null)]
    [InlineData(EntitledStopCodes.AdminDecision, RelationshipConstants.Son, null)]
    [InlineData(EntitledStopCodes.Employed, RelationshipConstants.Son, null)]
    [InlineData("UNKNOWN", RelationshipConstants.Son, null)]
    public async Task Stop_UsesTheSameEvidenceTypeForUploadAndMissingRecord(string code, string relationship, int? expectedType)
    {
        foreach (var attached in new[] { false, true })
        {
            var f = new Fixture();
            f.Person.Relationship.RelationshipName = relationship;
            var proof = attached ? Mock.Of<IRemoteFile>() : null;
            var result = await f.Service.StopAsync(7, new StopEntitledPersonDto { StopCode = code, Document = proof }, 9);
            Assert.True(result.IsSuccess, result.Message);
            Assert.False(f.Person.IsActive);
            Assert.Equal(code, f.Person.StopCode);
            Assert.Equal(code, f.Person.StopReason);
            Assert.NotNull(f.Person.StopDate);
            Assert.Equal(DateTimeKind.Utc, f.Person.StopDate.Value.Kind);
            Assert.Equal(9, f.Person.UpdatedByUserID);
            Assert.Equal(code is EntitledStopCodes.DaughterMarried or EntitledStopCodes.WidowRemarried
                ? SocialStatusConstants.Married : (int?)null, f.Person.SocialStatusID);
            if (expectedType.HasValue && attached)
            {
                var upload = Assert.Single(f.Uploads);
                Assert.Equal(expectedType.Value, upload.DocumentTypeID);
                Assert.Equal(7, upload.EntitledPersonID);
                Assert.Same(proof, upload.File);
                Assert.Empty(f.Missing);
            }
            else if (expectedType.HasValue)
            {
                AssertMissing(f, expectedType.Value);
                Assert.Empty(f.Uploads);
            }
            else
            {
                Assert.Empty(f.Uploads);
                Assert.Empty(f.Missing);
            }
        }
    }

    [Theory]
    [InlineData(EntitledStopCodes.DaughterMarried, ReactivationCodes.Divorced, DocumentTypeConstants.DivorceCertificate)]
    [InlineData(EntitledStopCodes.WidowRemarried, ReactivationCodes.Divorced, DocumentTypeConstants.DivorceCertificate)]
    [InlineData(EntitledStopCodes.AgeLimit, ReactivationCodes.DisabilityReport, DocumentTypeConstants.MedicalReport)]
    [InlineData(EntitledStopCodes.AdminDecision, ReactivationCodes.AdminDecision, null)]
    public async Task Reactivate_UsesTheSameEvidenceTypeForUploadAndMissingRecord(string stopCode, string code, int? expectedType)
    {
        foreach (var attached in new[] { false, true })
        {
            var f = new Fixture();
            f.Person.IsActive = false;
            f.Person.StopCode = stopCode;
            f.Person.Relationship.RelationshipName = stopCode == EntitledStopCodes.DaughterMarried ? RelationshipConstants.Daughter :
                stopCode == EntitledStopCodes.WidowRemarried ? RelationshipConstants.Wife : RelationshipConstants.Son;
            if (stopCode == EntitledStopCodes.AdminDecision) f.Person.BirthDate = YemenTime.Today.AddYears(-20);
            var proof = attached ? Mock.Of<IRemoteFile>() : null;
            var result = await f.Service.ReactivateAsync(7,
                new ReactivateEntitledPersonDto { ReactivationCode = code, Document = proof }, 9);
            Assert.True(result.IsSuccess, result.Message);
            Assert.True(f.Person.IsActive);
            Assert.Null(f.Person.StopCode);
            Assert.Null(f.Person.StopDate);
            Assert.Null(f.Person.StopReason);
            Assert.Equal(9, f.Person.UpdatedByUserID);
            Assert.Equal(code == ReactivationCodes.DisabilityReport, f.Person.HasDisabilityException);
            if (expectedType.HasValue && attached)
            {
                var upload = Assert.Single(f.Uploads);
                Assert.Equal(expectedType.Value, upload.DocumentTypeID);
                Assert.Equal(7, upload.EntitledPersonID);
                Assert.Same(proof, upload.File);
                Assert.Empty(f.Missing);
            }
            else if (expectedType.HasValue)
            {
                AssertMissing(f, expectedType.Value);
                Assert.Empty(f.Uploads);
            }
            else
            {
                Assert.Empty(f.Uploads);
                Assert.Empty(f.Missing);
            }
        }
    }

    [Theory]
    [InlineData(EntitledStopCodes.DaughterMarried, false, false)]
    [InlineData(EntitledStopCodes.DaughterMarried, true, false)]
    [InlineData(EntitledStopCodes.DaughterMarried, false, true)]
    [InlineData(EntitledStopCodes.DaughterMarried, true, true)]
    [InlineData(EntitledStopCodes.WidowRemarried, false, false)]
    [InlineData(EntitledStopCodes.WidowRemarried, true, false)]
    [InlineData(EntitledStopCodes.WidowRemarried, false, true)]
    [InlineData(EntitledStopCodes.WidowRemarried, true, true)]
    public async Task HusbandDeath_ProcessesPrimaryAndSupportingEvidenceIndependently(string stopCode, bool primaryAttached, bool civilAttached)
    {
        var f = new Fixture();
        f.Person.IsActive = false;
        f.Person.StopCode = stopCode;
        f.Person.Relationship.RelationshipName = stopCode == EntitledStopCodes.DaughterMarried ? RelationshipConstants.Daughter : RelationshipConstants.Wife;
        var primary = primaryAttached ? Mock.Of<IRemoteFile>() : null;
        var civil = civilAttached ? Mock.Of<IRemoteFile>() : null;
        var result = await f.Service.ReactivateAsync(7, new ReactivateEntitledPersonDto
        {
            ReactivationCode = ReactivationCodes.HusbandDeceased, Document = primary, CivilServiceProof = civil
        }, 9);
        Assert.True(result.IsSuccess, result.Message);
        Assert.True(f.Person.IsActive);
        Assert.Equal((primaryAttached ? 1 : 0) + (civilAttached ? 1 : 0), f.Uploads.Count);
        Assert.Equal((primaryAttached ? 0 : 1) + (civilAttached ? 0 : 1), f.Missing.Count);
        foreach (var (type, attached, proof) in new[]
        {
            (DocumentTypeConstants.DeathCertificate, primaryAttached, primary),
            (DocumentTypeConstants.CivilServiceProof, civilAttached, civil)
        })
        {
            if (attached)
            {
                var upload = Assert.Single(f.Uploads.Where(u => u.DocumentTypeID == type));
                Assert.Same(proof, upload.File);
                Assert.Equal(7, upload.EntitledPersonID);
            }
            else AssertMissing(f, type);
        }
    }

    [Fact]
    public async Task ManualEvidenceRefactor_PreservesTransitionValidation()
    {
        var f = new Fixture();
        var stopped = await f.Service.StopAsync(7,
            new StopEntitledPersonDto { StopCode = EntitledStopCodes.DaughterMarried }, 9);
        Assert.False(stopped.IsSuccess);
        Assert.True(f.Person.IsActive);
        f.Person.IsActive = false;
        f.Person.StopCode = EntitledStopCodes.AdminDecision;
        var reactivated = await f.Service.ReactivateAsync(7,
            new ReactivateEntitledPersonDto { ReactivationCode = ReactivationCodes.Divorced }, 9);
        Assert.False(reactivated.IsSuccess);
        Assert.False(f.Person.IsActive);
        Assert.Empty(f.Missing);
        Assert.Empty(f.Uploads);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("valid", true)]
    [InlineData("expires-today", true)]
    [InlineData("pending-review", true)]
    [InlineData("expired", false)]
    [InlineData("rejected", false)]
    [InlineData("deleted", false)]
    [InlineData("wrong-type", false)]
    [InlineData("wrong-owner", false)]
    [InlineData("mixed-owner", false)]
    [InlineData("wrong-file", false)]
    public async Task IdentityDisplay_UsesTheSameValidityAndOwnerPolicyAsActivation(string scenario, bool expected)
    {
        var f = new Fixture();
        var identity = f.AddIdentity("identity.pdf");
        if (scenario == "expires-today") identity.ExpiryDate = YemenTime.Today;
        if (scenario == "expired") identity.ExpiryDate = YemenTime.Today.AddDays(-1);
        if (scenario == "pending-review") identity.PendingReviewStatus = PendingReviewStatus.Pending;
        if (scenario == "rejected") identity.PendingReviewStatus = PendingReviewStatus.Rejected;
        if (scenario == "deleted") identity.IsDeleted = true;
        if (scenario == "wrong-type") identity.DocumentTypeID = DocumentTypeConstants.DivorceCertificate;
        if (scenario == "wrong-owner") identity.EntitledID = 8;
        if (scenario == "mixed-owner") identity.RetireeID = 2;
        if (scenario == "wrong-file") identity.FileID = 2;

        var single = await f.Service.GetByIdAsync(7);
        Assert.True(single.IsSuccess);
        var list = await f.Service.GetByFileIdAsync(1);
        Assert.True(list.IsSuccess);
        foreach (var dto in new[] { single.Data!, Assert.Single(list.Data!) })
        {
            Assert.Equal(expected, dto.HasNationalIdDocument);
            Assert.Equal(expected ? "/uploads/identity.pdf" : null, dto.NationalIdImageUrl);
            Assert.Equal(f.Person.IsActive, dto.IsEligible);
        }
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task IdentityDisplay_SkipsInvalidHistoricalUploadAndReturnsValidReplacement()
    {
        var f = new Fixture();
        f.AddIdentity("expired.pdf").ExpiryDate = YemenTime.Today.AddDays(-1);
        f.AddIdentity("rejected.pdf").PendingReviewStatus = PendingReviewStatus.Rejected;
        f.AddIdentity("replacement.pdf");
        var result = await f.Service.GetByIdAsync(7);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data!.HasNationalIdDocument);
        Assert.Equal("/uploads/replacement.pdf", result.Data.NationalIdImageUrl);
    }

    private static void AssertMissing(Fixture f, int type)
    {
        var row = Assert.Single(f.Missing.Where(m => m.DocumentTypeID == type));
        Assert.Equal(1, row.FileID);
        Assert.Equal(7, row.EntitledID);
        Assert.Equal(MissingDocumentStatusConstants.Pending, row.Status);
    }

    [Theory]
    [InlineData("inactive", "المستحق موقوف مسبقاً")]
    [InlineData("non-son", "إيقاف بسبب العمر مخصص للأبناء فقط")]
    [InlineData("young-son", "الابن لم يبلغ 24 سنة بعد (العمر الحالي: 23 سنة)")]
    [InlineData("disabled-son", "لا يمكن إيقاف هذا الابن بسبب العمر — لديه استثناء إعاقة/مرض نفسي")]
    [InlineData("daughter-mismatch", "إيقاف بسبب الزواج مخصص للبنات فقط")]
    [InlineData("wife-mismatch", "إيقاف بسبب إعادة الزواج مخصص للزوجات فقط")]
    public async Task Stop_RejectionLeavesLifecycleStateAndEvidenceUntouched(string scenario, string error)
    {
        var f = new Fixture();
        var code = EntitledStopCodes.AgeLimit;
        if (scenario == "inactive") f.Person.IsActive = false;
        if (scenario == "non-son") f.Person.Relationship.RelationshipName = RelationshipConstants.Daughter;
        if (scenario is "young-son" or "disabled-son") f.Person.BirthDate = YemenTime.Today.AddYears(-24).AddDays(1);
        if (scenario == "disabled-son") f.Person.HasDisabilityException = true;
        if (scenario == "daughter-mismatch") code = EntitledStopCodes.DaughterMarried;
        if (scenario == "wife-mismatch") code = EntitledStopCodes.WidowRemarried;
        var before = LifecycleState(f.Person);

        var result = await f.Service.StopAsync(7,
            new StopEntitledPersonDto { StopCode = code, Document = Mock.Of<IRemoteFile>() }, 9);

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Message);
        Assert.Equal(before, LifecycleState(f.Person));
        AssertNoLifecycleWrites(f);
    }

    [Theory]
    [InlineData("active", "المستحق نشط بالفعل")]
    [InlineData("deceased", "لا يمكن إعادة تفعيل مستحق متوفى")]
    [InlineData("pairing", "رمز إعادة التفعيل (DIVORCED) غير متوافق مع سبب الإيقاف (ADMIN_DECISION)")]
    [InlineData("missing-retiree", "بيانات المتقاعد غير موجودة")]
    [InlineData("retiree-alive", "لا يمكن التفعيل: المتقاعد لا يزال على قيد الحياة")]
    [InlineData("missing-birth", "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية")]
    [InlineData("over-age", "المستحق غير مؤهل: تجاوز السن القانوني (24 سنة)")]
    [InlineData("married", "المستحق غير مؤهل: البنت متزوجة")]
    public async Task Reactivate_RejectionLeavesLifecycleStateAndEvidenceUntouched(string scenario, string error)
    {
        var f = new Fixture();
        f.Person.IsActive = scenario == "active";
        f.Person.IsDeceased = scenario is "active" or "deceased";
        f.Person.StopCode = EntitledStopCodes.AdminDecision;
        f.Person.StopReason = "سبب سابق";
        f.Person.StopDate = DateTime.UtcNow.AddDays(-1);
        var code = ReactivationCodes.AdminDecision;
        if (scenario == "pairing") code = ReactivationCodes.Divorced;
        if (scenario == "missing-retiree") f.Person.RetirementFile.Retiree = null!;
        if (scenario == "retiree-alive") f.Person.RetirementFile.Retiree.IsAlive = true;
        if (scenario == "missing-birth") f.Person.BirthDate = default;
        if (scenario == "married")
        {
            f.Person.Relationship.RelationshipName = RelationshipConstants.Daughter;
            f.Person.SocialStatusID = SocialStatusConstants.Married;
        }
        var before = LifecycleState(f.Person);

        var result = await f.Service.ReactivateAsync(7, new ReactivateEntitledPersonDto
        {
            ReactivationCode = code, Document = Mock.Of<IRemoteFile>(), CivilServiceProof = Mock.Of<IRemoteFile>()
        }, 9);

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Message);
        Assert.Equal(before, LifecycleState(f.Person));
        AssertNoLifecycleWrites(f);
    }

    private static object LifecycleState(EntitledPerson p) =>
        (p.IsActive, p.IsDeceased, p.StopCode, p.StopReason, p.StopDate, p.SocialStatusID,
            p.HasDisabilityException, p.UpdatedAt, p.UpdatedByUserID);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Stop_AtOrAboveAgeLimitPreservesStateAndLinkedDeactivation(int birthDayOffset)
    {
        var f = new Fixture();
        f.Person.BirthDate = YemenTime.Today.AddYears(-24).AddDays(birthDayOffset);
        f.Person.SocialStatusID = SocialStatusConstants.Single;
        var agent = new Agent { AgentID = 8, EntitledID = 7, IsActive = true };
        var direct = new SalaryRecipient { EntitledID = 7, IsActive = true };
        var viaAgent = new SalaryRecipient { AgentID = 8, IsActive = true };
        Mock.Get(f.Unit.Object.Agents).Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Agent>([agent]));
        Mock.Get(f.Unit.Object.SalaryRecipients).Setup(r => r.Query())
            .Returns(new TestAsyncEnumerable<SalaryRecipient>([direct, viaAgent]));

        var result = await f.Service.StopAsync(7,
            new StopEntitledPersonDto { StopCode = EntitledStopCodes.AgeLimit, Notes = "سبب مفصل" }, 9);

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal("تم إيقاف الاستحقاق", result.Message);
        Assert.False(f.Person.IsActive);
        Assert.Equal("سبب مفصل", f.Person.StopReason);
        Assert.Equal(SocialStatusConstants.Single, f.Person.SocialStatusID);
        Assert.False(agent.IsActive);
        Assert.False(direct.IsActive);
        Assert.False(viaAgent.IsActive);
        Assert.NotNull(direct.EndDate);
        Assert.NotNull(viaAgent.EndDate);
        Assert.Empty(f.Uploads);
        Assert.Empty(f.Missing);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
    }

    private static void AssertNoLifecycleWrites(Fixture f)
    {
        Assert.Empty(f.Uploads);
        Assert.Empty(f.Missing);
        Mock.Get(f.Unit.Object.EntitledPersons).Verify(r => r.Update(It.IsAny<EntitledPerson>()), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.VerifyGet(u => u.Agents, Times.Never);
        f.Unit.VerifyGet(u => u.SalaryRecipients, Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task OrdinaryUpdate_RejectsLifecycleOverrideBeforeAnySave()
    {
        var f = new Fixture();
        var result = await f.Service.UpdateAsync(7, new UpdateEntitledPersonDto { IsEligible = false, FullName = "تغيير" }, 9);
        Assert.False(result.IsSuccess);
        Assert.True(f.Person.IsActive);
        Assert.Equal("مستحق", f.Person.FullName);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("stop")]
    [InlineData("reactivate")]
    [InlineData("death")]
    public async Task FailedEvidence_RollsBackTheWholeLifecycleOperation(string operation)
    {
        var f = new Fixture();
        f.Documents.Setup(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 9, null, true))
            .ReturnsAsync(ApiResponse<DocumentDto>.Fail("فشل رفع الوثيقة"));
        var proof = Mock.Of<IRemoteFile>();
        ApiResponse<bool> result;
        if (operation == "stop")
            result = await f.Service.StopAsync(7, new StopEntitledPersonDto { StopCode = EntitledStopCodes.Deceased, Document = proof }, 9);
        else if (operation == "death")
            result = await f.Service.RecordDeathAsync(7, new RecordDeathDto { DeathDate = YemenTime.Today, DeathCertificate = proof }, 9);
        else
        {
            f.Person.IsActive = false;
            f.Person.StopCode = EntitledStopCodes.DaughterMarried;
            f.Person.Relationship.RelationshipName = RelationshipConstants.Daughter;
            result = await f.Service.ReactivateAsync(7, new ReactivateEntitledPersonDto { ReactivationCode = ReactivationCodes.Divorced, Document = proof }, 9);
        }
        Assert.False(result.IsSuccess);
        Assert.Equal("فشل رفع الوثيقة", result.Message);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData(ReactivationCodes.Divorced, SocialStatusConstants.Divorced)]
    [InlineData(ReactivationCodes.HusbandDeceased, SocialStatusConstants.Widowed)]
    public async Task Reactivation_UpdatesTheSocialStatusUsedByLaterEvaluations(string code, int status)
    {
        var f = new Fixture();
        f.Person.IsActive = false;
        f.Person.StopCode = EntitledStopCodes.DaughterMarried;
        f.Person.Relationship.RelationshipName = RelationshipConstants.Daughter;
        f.Person.SocialStatusID = SocialStatusConstants.Married;
        var result = await f.Service.ReactivateAsync(7, new ReactivateEntitledPersonDto { ReactivationCode = code }, 9);
        Assert.True(result.IsSuccess);
        Assert.True(f.Person.IsActive);
        Assert.Equal(status, f.Person.SocialStatusID);
        f.Unit.Verify(u => u.LockFileReviewAsync(1), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Once);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public List<UploadDocumentDto> Uploads { get; } = [];
        public List<MissingDocument> Missing { get; } = [];
        public Mock<IDocumentService> Documents { get; } = new();
        public EntitledPerson Person { get; } = new()
        {
            EntitledID = 7, FileID = 1, FullName = "مستحق", IsActive = true,
            BirthDate = YemenTime.Today.AddYears(-30),
            RetirementFile = new RetirementFile { FileID = 1, RetireeID = 2,
                Retiree = new Retiree { RetireeID = 2, IsAlive = false } },
            Relationship = new Relationship { RelationshipName = RelationshipConstants.Son }
        };
        public EntitledPersonService Service { get; }

        public Fixture()
        {
            Bind(u => u.EntitledPersons, new[] { Person });
            Bind<Agent>(u => u.Agents, []);
            Bind<SalaryRecipient>(u => u.SalaryRecipients, []);
            var missing = Bind(u => u.MissingDocuments, Missing);
            missing.Setup(r => r.AddAsync(It.IsAny<MissingDocument>())).Callback<MissingDocument>(Missing.Add)
                .Returns(Task.CompletedTask);
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            var storage = new Mock<IFileStorageService>();
            storage.Setup(s => s.GetFileUrl(It.IsAny<string>())).Returns((string path) => "/uploads/" + path);
            var documents = Documents;
            documents.Setup(d => d.UploadAsync(It.IsAny<UploadDocumentDto>(), 9, null, true))
                .Callback<UploadDocumentDto, int, string?, bool>((dto, _, _, _) => Uploads.Add(dto))
                .ReturnsAsync(ApiResponse<DocumentDto>.Success(new DocumentDto()));
            var review = new Mock<IFileReviewService>();
            review.Setup(r => r.RequestReviewAsync(1, 9, It.IsAny<DateTime?>())).ReturnsAsync(ApiResponse<bool>.Success(true));
            Service = new EntitledPersonService(Unit.Object, storage.Object, documents.Object,
                Mock.Of<IEntitlementEngine>(), Mock.Of<IMissingDocumentService>(), review.Object,
                NullLogger<EntitledPersonService>.Instance, TestPensionAccess.All(Unit.Object));
        }

        public Document AddIdentity(string path)
        {
            var document = new Document { DocumentTypeID = DocumentTypeConstants.NationalId,
                FileID = 1, EntitledID = 7, FilePath = path };
            Person.Documents.Add(document);
            return document;
        }

        private Mock<IRepository<T>> Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, IEnumerable<T> rows) where T : class
        {
            var repo = new Mock<IRepository<T>>();
            repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows));
            Unit.Setup(property).Returns(repo.Object);
            return repo;
        }
    }
}
