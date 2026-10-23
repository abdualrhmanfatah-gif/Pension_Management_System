using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class MissingDocumentDetectionTests
{
    public static IEnumerable<object[]> DocumentCases()
    {
        foreach (var owner in new[] { EntityTypeConstants.File, EntityTypeConstants.Retiree,
            EntityTypeConstants.Agent, EntityTypeConstants.Guardian, EntityTypeConstants.EntitledPerson,
            EntityTypeConstants.Pledge, EntityTypeConstants.Exemption })
        foreach (var scenario in new[] { "valid", "no-expiry", "expires-today", "expired", "rejected",
            "deleted", "wrong-owner", "wrong-file" })
            yield return [owner, scenario];
    }

    [Theory]
    [MemberData(nameof(DocumentCases))]
    public async Task DetectionAndCompliance_AgreeForEveryDocumentOwner(string owner, string scenario)
    {
        var f = new Fixture();
        if (owner == EntityTypeConstants.EntitledPerson) f.Retiree.IsAlive = false;
        var pairs = await f.Service.GetRequiredPairsAsync(1);
        var required = pairs.First(p => p.EntityType == owner);
        f.Upload(pairs);
        var document = f.Documents[pairs.IndexOf(required)];
        if (scenario == "no-expiry") document.ExpiryDate = null;
        if (scenario == "expires-today") document.ExpiryDate = YemenTime.Today;
        if (scenario == "expired") document.ExpiryDate = YemenTime.Today.AddDays(-1);
        if (scenario == "rejected") document.PendingReviewStatus = PendingReviewStatus.Rejected;
        if (scenario == "deleted") document.IsDeleted = true;
        if (scenario == "wrong-file") document.FileID = 2;
        if (scenario == "wrong-owner")
        {
            if (document.RetireeID.HasValue) document.RetireeID++;
            else if (document.AgentID.HasValue) document.AgentID++;
            else if (document.GuardianID.HasValue) document.GuardianID++;
            else if (document.EntitledID.HasValue) document.EntitledID++;
            else if (document.PledgeID.HasValue) document.PledgeID++;
            else if (document.ExemptionID.HasValue) document.ExemptionID++;
            else document.AgentID = 999;
        }

        var expectedMissing = scenario is "valid" or "no-expiry" or "expires-today" ? 0 : 1;
        var detected = await f.Service.GetAutoDetectedByFileAsync(1);
        Assert.True(detected.IsSuccess, detected.Message);
        Assert.Equal(expectedMissing, detected.Data!.Count);
        if (expectedMissing == 1) Assert.Equal(required.EntityType, Assert.Single(detected.Data).EntityType);
        Assert.Equal(expectedMissing, (await f.Service.GetComplianceSummaryAsync(1)).MissingCount);
        Assert.Equal(expectedMissing, await f.Service.StageRequiredAsync(1, 7));
        Assert.Equal(0, await f.Service.StageRequiredAsync(1, 7));
        Assert.Equal(expectedMissing, f.Missing.Count);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false, MissingDocumentStatusConstants.Pending, false)]
    [InlineData(true, MissingDocumentStatusConstants.Pending, false)]
    [InlineData(false, MissingDocumentStatusConstants.Pledged, false)]
    [InlineData(true, MissingDocumentStatusConstants.Exempted, false)]
    [InlineData(false, MissingDocumentStatusConstants.Fulfilled, false)]
    [InlineData(true, MissingDocumentStatusConstants.Fulfilled, false)]
    [InlineData(false, MissingDocumentStatusConstants.Pending, true)]
    [InlineData(true, MissingDocumentStatusConstants.Pending, true)]
    public async Task ManualDirectiveRecord_SuppressesOnlyItsOwnOwner(bool exemption, string status, bool deleted)
    {
        var f = new Fixture();
        var pairs = await f.Service.GetRequiredPairsAsync(1);
        f.Upload(pairs);
        var type = exemption ? DocumentTypeConstants.ExemptionDirective : DocumentTypeConstants.PledgeDocument;
        f.Documents.RemoveAll(d => d.DocumentTypeID == type);
        f.Missing.Add(new MissingDocument { FileID = 1, DocumentTypeID = type, Status = status,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null, IsDeleted = deleted });

        var detected = (await f.Service.GetAutoDetectedByFileAsync(1)).Data!;
        var suppress = !deleted && status != MissingDocumentStatusConstants.Fulfilled;
        Assert.Equal(suppress ? 1 : 2, detected.Count);
        Assert.Contains(detected, p => exemption ? p.ExemptionID == 21 : p.PledgeID == 20);
        Assert.Equal(!suppress, detected.Any(p => exemption ? p.ExemptionID == 11 : p.PledgeID == 10));
    }

    [Fact]
    public async Task StageRequired_PreservesPledgedAndExemptedRows()
    {
        var f = new Fixture();
        var pledged = new MissingDocument { FileID = 1, DocumentTypeID = DocumentTypeConstants.NationalId,
            Status = MissingDocumentStatusConstants.Pledged, PledgeID = 10 };
        var exempted = new MissingDocument { FileID = 1, DocumentTypeID = DocumentTypeConstants.FileOriginals,
            Status = MissingDocumentStatusConstants.Exempted, ExemptionID = 11 };
        f.Missing.AddRange([pledged, exempted]);
        f.File.MissingDocuments.Add(pledged);
        f.File.MissingDocuments.Add(exempted);
        await f.Service.StageRequiredAsync(1, 7);
        Assert.Equal(MissingDocumentStatusConstants.Pledged, pledged.Status);
        Assert.Equal(10, pledged.PledgeID);
        Assert.Equal(MissingDocumentStatusConstants.Exempted, exempted.Status);
        Assert.Equal(11, exempted.ExemptionID);
        Assert.Single(f.Missing.Where(m => m.DocumentTypeID == DocumentTypeConstants.NationalId &&
            m.AgentID == null && m.GuardianID == null && m.EntitledID == null));
        Assert.Single(f.Missing.Where(m => m.DocumentTypeID == DocumentTypeConstants.FileOriginals));
    }

    [Fact]
    public async Task StageRequired_RequiresAnExistingTransaction()
    {
        var f = new Fixture();
        f.Unit.Setup(u => u.HasActiveTransaction).Returns(false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.StageRequiredAsync(1, 7));
        Assert.Empty(f.Missing);
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ManualCreate_KeepsDuplicateRejection_AndUsesTheFileLock()
    {
        var f = new Fixture();
        var dto = new CreateMissingDocumentDto { FileID = 1, DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 };
        Assert.True((await f.Service.CreateAsync(dto, 7)).IsSuccess);
        Assert.False((await f.Service.CreateAsync(dto, 7)).IsSuccess);
        Assert.Single(f.Missing);
        f.Unit.Verify(u => u.LockFileReviewAsync(1), Times.Exactly(2));
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Once);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ComplianceCache_UsesUnifiedSummaryAndLeavesTransactionOwnershipToCaller()
    {
        var f = new Fixture();
        var pairs = await f.Service.GetRequiredPairsAsync(1);
        f.Upload(pairs);
        f.Documents.RemoveAt(0);
        f.File.MissingDocumentsCount = 999;
        var summary = await f.Service.GetComplianceSummaryAsync(1);
        Assert.Equal(1, summary.MissingCount);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
        await f.Service.RefreshComplianceCacheAsync(1);
        Assert.Equal(summary.MissingCount, f.File.MissingDocumentsCount);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Once);
        f.Unit.Verify(u => u.RetirementFiles.Update(It.IsAny<RetirementFile>()), Times.Never);
        f.Unit.Verify(u => u.BeginTransactionAsync(), Times.Never);
        f.Unit.Verify(u => u.CommitAsync(), Times.Never);
        f.Unit.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Compliance_RejectsForeignFileBeforeReadingSourcesOrSaving(bool refresh)
    {
        var f = new Fixture();
        f.File.PensionBranchID = 2;
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(s => s.GetCurrentBranchId()).Returns(1);
        var service = new MissingDocumentService(f.Unit.Object, Mock.Of<IEntitlementEngine>(),
            NullLogger<MissingDocumentService>.Instance, TestPensionAccess.From(f.Unit.Object, identity.Object));
        if (refresh)
            await Assert.ThrowsAsync<PensionMS.Core.Exceptions.EntityNotFoundException>(() => service.RefreshComplianceCacheAsync(1));
        else
            await Assert.ThrowsAsync<PensionMS.Core.Exceptions.EntityNotFoundException>(() => service.GetComplianceSummaryAsync(1));
        Mock.Get(f.Unit.Object.Retirees).Verify(r => r.Query(), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Compliance_RejectsDeletedFileBeforeReadingSourcesOrSaving(bool refresh)
    {
        var f = new Fixture();
        f.File.IsDeleted = true;
        if (refresh)
            await Assert.ThrowsAsync<PensionMS.Core.Exceptions.EntityNotFoundException>(() => f.Service.RefreshComplianceCacheAsync(1));
        else
            await Assert.ThrowsAsync<PensionMS.Core.Exceptions.EntityNotFoundException>(() => f.Service.GetComplianceSummaryAsync(1));
        Mock.Get(f.Unit.Object.Retirees).Verify(r => r.Query(), Times.Never);
        f.Unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Retiree Retiree { get; } = new() { RetireeID = 2, IsAlive = true, FullName = "متقاعد" };
        public List<Document> Documents { get; } = [];
        public List<MissingDocument> Missing { get; } = [];
        public MissingDocumentService Service { get; }
        public RetirementFile File { get; }

        public Fixture()
        {
            var file = File = new RetirementFile { FileID = 1, RetireeID = 2, Retiree = Retiree };
            Unit.Setup(u => u.HasActiveTransaction).Returns(true);
            Retiree.RetirementFiles.Add(file);
            Bind(u => u.Retirees, [Retiree]);
            Bind(u => u.RetirementFiles, [file]);
            Bind(u => u.Agents, [new Agent { AgentID = 8, FileID = 1 }]);
            Bind(u => u.Guardians, [new Guardian { GuardianID = 9, FileID = 1 }]);
            Bind(u => u.EntitledPersons, [new EntitledPerson { EntitledID = 7, FileID = 1,
                BirthDate = YemenTime.Today.AddYears(-30) }]);
            Bind<PaymentBook>(u => u.PaymentBooks, []);
            Bind(u => u.DocumentPledges, [new DocumentPledge { PledgeID = 10, FileID = 1 },
                new DocumentPledge { PledgeID = 20, FileID = 1 }]);
            Bind(u => u.DocumentExemptions, [new DocumentExemption { ExemptionID = 11, FileID = 1 },
                new DocumentExemption { ExemptionID = 21, FileID = 1 }]);
            Bind(u => u.Documents, Documents);
            Bind(u => u.MissingDocuments, Missing);
            Bind(u => u.DocumentTypes, Enumerable.Range(1, 17).Select(id =>
                new DocumentType { DocumentTypeID = id, TypeName = "وثيقة " + id, IsActive = true }).ToList());
            Service = new MissingDocumentService(Unit.Object, Mock.Of<IEntitlementEngine>(),
                NullLogger<MissingDocumentService>.Instance, TestPensionAccess.All(Unit.Object));
        }

        public void Upload(IEnumerable<AutoDetectedMissingDocumentDto> pairs) => Documents.AddRange(pairs.Select(p =>
            new Document { FileID = 1, DocumentTypeID = p.DocumentTypeID, RetireeID = p.RetireeID,
                AgentID = p.AgentID, GuardianID = p.GuardianID, EntitledID = p.EntitledID,
                PledgeID = p.PledgeID, ExemptionID = p.ExemptionID, ExpiryDate = YemenTime.Today.AddDays(1) }));

        private void Bind<T>(Expression<Func<IUnitOfWork, IRepository<T>>> property, List<T> rows) where T : class
        {
            var repository = new Mock<IRepository<T>>();
            repository.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows));
            repository.Setup(r => r.AddAsync(It.IsAny<T>())).Callback<T>(rows.Add).Returns(Task.CompletedTask);
            Unit.Setup(property).Returns(repository.Object);
        }
    }
}
