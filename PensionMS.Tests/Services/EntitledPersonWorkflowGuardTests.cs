using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Rules.Documents;
using PensionMS.Core.Time;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class EntitledPersonWorkflowGuardTests
{
    [Theory]
    [InlineData("manual")]
    [InlineData("automatic")]
    [InlineData("required")]
    public async Task MissingDocumentLists_RejectForeignFileBeforeReadingItsPeople(string method)
    {
        var (unit, service) = CreateScopedService();
        await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
        {
            if (method == "manual") await service.GetByFileAsync(2);
            else if (method == "automatic") await service.GetAutoDetectedByFileAsync(2);
            else await service.GetRequiredPairsAsync(2);
        });
        unit.VerifyGet(u => u.MissingDocuments, Times.Never);
        unit.VerifyGet(u => u.EntitledPersons, Times.Never);
        unit.VerifyGet(u => u.Retirees, Times.Never);
    }

    [Fact]
    public async Task MissingDocumentById_DoesNotReturnAForeignOwnersRecord()
    {
        var (unit, service) = CreateScopedService();
        var repo = new Mock<IRepository<MissingDocument>>();
        repo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<MissingDocument>([
            new MissingDocument { MissingDocumentID = 20, FileID = 2, DocumentType = new DocumentType { TypeName = "وثيقة" } }
        ]));
        unit.Setup(u => u.MissingDocuments).Returns(repo.Object);
        var result = await service.GetByIdAsync(20);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData("fulfill")]
    [InlineData("delete")]
    [InlineData("create")]
    public async Task MissingDocumentWrites_RejectForeignFilesBeforeLockingOrSaving(string operation)
    {
        var (unit, service) = CreateScopedService();
        var row = new MissingDocument { MissingDocumentID = 20, FileID = 2, Status = MissingDocumentStatusConstants.Pending };
        var repo = new Mock<IRepository<MissingDocument>>();
        repo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<MissingDocument>([row]));
        unit.Setup(u => u.MissingDocuments).Returns(repo.Object);
        if (operation == "create")
            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.CreateAsync(
                new PensionMS.Core.DTOs.MissingDocuments.CreateMissingDocumentDto { FileID = 2, DocumentTypeID = 1 }, 7));
        else
            Assert.False((operation == "delete" ? await service.DeleteAsync(20, 7) : await service.MarkFulfilledAsync(20, 7)).IsSuccess);
        Assert.False(row.IsDeleted);
        Assert.Equal(MissingDocumentStatusConstants.Pending, row.Status);
        unit.Verify(u => u.LockFileReviewAsync(It.IsAny<int>()), Times.Never);
        unit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FileReview_DoesNotReplaceIndependentDocumentReviewDecisions(bool rejectFile)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        f.RequiredDocuments.Add(new() { DocumentTypeID = DocumentTypeConstants.NationalId, AgentID = 8 });
        var rejected = new Document { DocumentID = 10, FileID = 1, DocumentTypeID = DocumentTypeConstants.NationalId,
            AgentID = 8, PendingReviewStatus = PendingReviewStatus.Rejected, RejectionReason = "هوية غير مطابقة" };
        var replacement = new Document { DocumentID = 11, FileID = 1, DocumentTypeID = DocumentTypeConstants.NationalId,
            AgentID = 8, PendingReviewStatus = PendingReviewStatus.Approved };
        f.Documents.AddRange([rejected, replacement]);
        var result = rejectFile ? await f.Service.RejectReviewAsync(1, 7, "رفض بيانات الملف") : await f.Service.ApproveReviewAsync(1, 7);
        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(PendingReviewStatus.Rejected, rejected.PendingReviewStatus);
        Assert.Equal("هوية غير مطابقة", rejected.RejectionReason);
        Assert.Equal(PendingReviewStatus.Approved, replacement.PendingReviewStatus);
        var owner = new RetirementDocumentRequirement(DocumentTypeConstants.NationalId, null, 8, null, null, null, null);
        Assert.Same(replacement, RetirementFileDocumentRules.FindValidDocument(f.Documents, owner, YemenTime.Today));
    }

    private static (Mock<IUnitOfWork>, MissingDocumentService) CreateScopedService()
    {
        var unit = new Mock<IUnitOfWork>();
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>([
            new RetirementFile { FileID = 1, PensionBranchID = 1 },
            new RetirementFile { FileID = 2, PensionBranchID = 2 }
        ]));
        unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        var identity = new Mock<ITestBranchScope>();
        identity.Setup(s => s.GetCurrentBranchId()).Returns(1);
        return (unit, new MissingDocumentService(unit.Object, Mock.Of<IEntitlementEngine>(),
            NullLogger<MissingDocumentService>.Instance, TestPensionAccess.From(unit.Object, identity.Object)));
    }
}
