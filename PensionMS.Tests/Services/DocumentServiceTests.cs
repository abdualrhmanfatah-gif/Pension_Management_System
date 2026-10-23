using PensionMS.Testing;
using PensionMS.Infrastructure.Services.Documents;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using Xunit;

namespace PensionMS.Tests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITestBranchScope> _branchScopeMock;
    private readonly Mock<IImmutableAuditService> _auditMock;
    private readonly DocumentService _service;
    private readonly Mock<IMissingDocumentService> _missingService = new();

    public DocumentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _branchScopeMock = new Mock<ITestBranchScope>();
        _auditMock = new Mock<IImmutableAuditService>();
        _missingService.Setup(m => m.GetRequiredPairsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PensionMS.Core.DTOs.MissingDocuments.AutoDetectedMissingDocumentDto>());

        _service = new DocumentService(
            _unitOfWorkMock.Object,
            Mock.Of<IFileStorageService>(),
            Mock.Of<IEntitlementEngine>(),
            Mock.Of<IFileReviewService>(), TestPensionAccess.From(_unitOfWorkMock.Object, _branchScopeMock.Object),
            _auditMock.Object,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DocumentService>.Instance, _missingService.Object);
    }

    private void SetupDocument(Document doc)
    {
        _unitOfWorkMock.Setup(u => u.Documents.Query())
            .Returns(new TestAsyncEnumerable<Document>(new[] { doc }));
    }

    [Theory]
    [InlineData("valid", true)]
    [InlineData("expires-today", true)]
    [InlineData("expired", false)]
    [InlineData("rejected", false)]
    [InlineData("deleted", false)]
    [InlineData("other-retiree", false)]
    [InlineData("agent", false)]
    [InlineData("file-only", false)]
    [InlineData("other-file", false)]
    [InlineData("other-type", false)]
    public async Task IdentityPath_SelectsNewestValidDocumentForExactRetireeOwner(string scenario, bool useNewest)
    {
        SetupBranch(null, isHq: true);
        _unitOfWorkMock.Setup(u => u.RetirementFiles.Query()).Returns(new TestAsyncEnumerable<RetirementFile>([
            new RetirementFile { FileID = 10, RetireeID = 2 }
        ]));
        var older = new Document { DocumentID = 1, FileID = 10, RetireeID = 2,
            DocumentTypeID = DocumentTypeConstants.NationalId, FilePath = "older.png", CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var newest = new Document { DocumentID = 2, FileID = 10, RetireeID = 2,
            DocumentTypeID = DocumentTypeConstants.NationalId, FilePath = "newest.png", CreatedAt = older.CreatedAt.AddHours(1) };
        if (scenario == "expires-today") newest.ExpiryDate = PensionMS.Core.Time.YemenTime.Today;
        if (scenario == "expired") newest.ExpiryDate = PensionMS.Core.Time.YemenTime.Today.AddDays(-1);
        if (scenario == "rejected") newest.PendingReviewStatus = PendingReviewStatus.Rejected;
        if (scenario == "deleted") newest.IsDeleted = true;
        if (scenario == "other-retiree") newest.RetireeID = 3;
        if (scenario == "agent") newest.AgentID = 5;
        if (scenario == "file-only") newest.RetireeID = null;
        if (scenario == "other-file") newest.FileID = 11;
        if (scenario == "other-type") newest.DocumentTypeID = DocumentTypeConstants.DeathCertificate;
        _unitOfWorkMock.Setup(u => u.Documents.Query()).Returns(new TestAsyncEnumerable<Document>([older, newest]));

        var response = await _service.GetRetireeIdentityPathAsync(10);

        Assert.True(response.IsSuccess, response.Message);
        Assert.Equal(useNewest ? "newest.png" : "older.png", response.Data);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task IdentityPath_ReturnsNullWhenNoValidIdentityExists()
    {
        SetupBranch(null, isHq: true);
        _unitOfWorkMock.Setup(u => u.RetirementFiles.Query()).Returns(new TestAsyncEnumerable<RetirementFile>([
            new RetirementFile { FileID = 10, RetireeID = 2 }
        ]));
        SetupDocument(new Document { DocumentID = 1, FileID = 10, RetireeID = 2,
            DocumentTypeID = DocumentTypeConstants.NationalId, FilePath = "rejected.png",
            PendingReviewStatus = PendingReviewStatus.Rejected });
        var result = await _service.GetRetireeIdentityPathAsync(10);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task IdentityPath_RejectsForeignFileBeforeReadingDocuments()
    {
        SetupBranch(1);
        SetupFileBranch(10, 2);
        Assert.False((await _service.GetRetireeIdentityPathAsync(10)).IsSuccess);
        _unitOfWorkMock.VerifyGet(u => u.Documents, Times.Never);
    }

    private void SetupFileBranch(int fileId, int branchId)
    {
        var file = new RetirementFile { FileID = fileId, PensionBranchID = branchId };
        _unitOfWorkMock.Setup(u => u.RetirementFiles.Query())
            .Returns(new TestAsyncEnumerable<RetirementFile>(new[] { file }));
    }

    private void SetupBranch(int? branchId, bool isHq = false)
    {
        _branchScopeMock.Setup(b => b.IsHQ()).Returns(isHq);
        _branchScopeMock.Setup(b => b.GetCurrentBranchId()).Returns(branchId);
    }

    [Fact]
    public async Task ReviewAsync_InvalidStatusValue_ReturnsFail()
    {
        SetupBranch(null, isHq: true);
        SetupDocument(new Document { DocumentID = 1, UploadedByUserID = 5 });

        var result = await _service.ReviewAsync(1, new ReviewDocumentDto { PendingReviewStatus = 99 }, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal("حالة الفحص المرسلة غير صالحة", result.Message);
    }

    [Fact]
    public async Task ReviewAsync_NoneStatus_ReturnsFail()
    {
        SetupBranch(null, isHq: true);
        SetupDocument(new Document { DocumentID = 1, UploadedByUserID = 5 });

        var result = await _service.ReviewAsync(1, new ReviewDocumentDto { PendingReviewStatus = 0 }, 7);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ReviewAsync_RejectedWithoutReason_ReturnsFail()
    {
        SetupBranch(null, isHq: true);
        SetupDocument(new Document { DocumentID = 1, UploadedByUserID = 5 });

        var result = await _service.ReviewAsync(1, new ReviewDocumentDto { PendingReviewStatus = 3 }, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal("سبب الرفض مطلوب عند رفض الوثيقة", result.Message);
    }

    [Fact]
    public async Task ReviewAsync_SelfReview_ReturnsFail()
    {
        SetupBranch(null, isHq: true);
        SetupDocument(new Document { DocumentID = 1, UploadedByUserID = 5 });

        var result = await _service.ReviewAsync(1, new ReviewDocumentDto { PendingReviewStatus = 2 }, 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكنك فحص وثيقة رفعتها بنفسك", result.Message);
    }

    [Fact]
    public async Task ReviewAsync_DocumentInOtherBranch_ReturnsFail()
    {
        SetupBranch(1);
        SetupDocument(new Document { DocumentID = 1, FileID = 10, UploadedByUserID = 5 });
        SetupFileBranch(fileId: 10, branchId: 2);

        var result = await _service.ReviewAsync(1, new ReviewDocumentDto { PendingReviewStatus = 2 }, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal("الوثيقة غير موجودة", result.Message);
    }

    [Fact]
    public async Task ReviewAsync_ApprovedByOtherUser_CallsImmutableAudit()
    {
        SetupBranch(1);
        SetupDocument(new Document { DocumentID = 1, FileID = 10, UploadedByUserID = 5, PendingReviewStatus = PendingReviewStatus.Pending });
        SetupFileBranch(fileId: 10, branchId: 1);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.ReviewAsync(1, new ReviewDocumentDto { PendingReviewStatus = 2 }, 7);

        Assert.True(result.IsSuccess);
        _auditMock.Verify(a => a.LogAsync(
            7, It.IsAny<string>(), "ReviewDocument", "Document", 1,
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_DocumentInOtherBranch_ReturnsFail()
    {
        SetupBranch(1);
        SetupDocument(new Document { DocumentID = 1, FileID = 10, UploadedByUserID = 5 });
        SetupFileBranch(fileId: 10, branchId: 2);

        var result = await _service.GetByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal("الوثيقة غير موجودة", result.Message);
    }

    [Fact]
    public async Task GetByIdAsync_SameBranch_ReturnsSuccess()
    {
        SetupBranch(1);
        SetupDocument(new Document { DocumentID = 1, FileID = 10, UploadedByUserID = 5 });
        SetupFileBranch(fileId: 10, branchId: 1);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdAsync_HQ_AccessesAnyBranch()
    {
        SetupBranch(null, isHq: true);
        SetupDocument(new Document { DocumentID = 1, FileID = 10, UploadedByUserID = 5 });
        SetupFileBranch(fileId: 10, branchId: 2);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_NoteOnly_DoesNotClearOwner()
    {
        SetupBranch(null, isHq: true);
        var doc = new Document
        {
            DocumentID = 1,
            FileID = 10,
            UploadedByUserID = 5,
            DocumentTypeID = 1,
            RetireeID = 9,
            OwnerType = DocumentOwnerType.Retiree,
            Notes = "قديم"
        };
        SetupDocument(doc);
        _unitOfWorkMock.Setup(u => u.DocumentTypes.GetByIdAsync(1))
            .ReturnsAsync(new DocumentType { DocumentTypeID = 1 });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.UpdateAsync(1, new UpdateDocumentDto
        {
            DocumentTypeID = 1,
            Notes = "ملاحظة جديدة"
        }, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal(9, doc.RetireeID);
        Assert.Equal("ملاحظة جديدة", doc.Notes);
        Assert.Equal(DocumentOwnerType.Retiree, doc.OwnerType);
    }

    // ─── م18/19/20/24: رفع الوثائق ───

    private sealed class FakeRemoteFile : IRemoteFile
    {
        private readonly byte[] _bytes;
        public FakeRemoteFile(byte[] bytes, string fileName) { _bytes = bytes; FileName = fileName; }
        public string FileName { get; }
        public string ContentType => "image/png";
        public long Length => _bytes.Length;
        public Stream OpenReadStream() => new MemoryStream(_bytes, writable: false);
    }

    private DocumentService CreateUploadService(IFileStorageService storage) =>
        new DocumentService(
            _unitOfWorkMock.Object,
            storage,
            Mock.Of<IEntitlementEngine>(),
            Mock.Of<IFileReviewService>(), TestPensionAccess.From(_unitOfWorkMock.Object, _branchScopeMock.Object),
            _auditMock.Object,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DocumentService>.Instance, _missingService.Object);

    private Mock<IFileStorageService> SetupStorage()
    {
        var storage = new Mock<IFileStorageService>();
        storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<long?>()))
            .ReturnsAsync("documents/saved.png");
        storage.Setup(s => s.DeleteFileAsync(It.IsAny<string>())).ReturnsAsync(true);
        return storage;
    }

    private void SetupUploadBasics(int docTypeId = 1, DocumentType? docTypeOverride = null)
    {
        SetupBranch(null, isHq: true);
        _unitOfWorkMock.Setup(u => u.DocumentTypes.GetByIdAsync(docTypeId))
            .ReturnsAsync(docTypeOverride ?? new DocumentType { DocumentTypeID = docTypeId, TypeName = "نوع", IsActive = true });
        _unitOfWorkMock.Setup(u => u.MissingDocuments.Query())
            .Returns(new TestAsyncEnumerable<MissingDocument>(Array.Empty<MissingDocument>()));
        _unitOfWorkMock.Setup(u => u.Documents.Query())
            .Returns(new TestAsyncEnumerable<Document>(Array.Empty<Document>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
    }

    [Fact]
    public async Task UploadAsync_TwoPersonOwners_ReturnsFail()
    {
        SetupUploadBasics();
        var service = CreateUploadService(SetupStorage().Object);

        var result = await service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(new byte[] { 1, 2, 3 }, "doc.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10,
            AgentID = 5,
            EntitledPersonID = 7
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن ربط الوثيقة بأكثر من مالك شخصي واحد", result.Message);
    }

    [Fact]
    public async Task UploadAsync_PersonFromAnotherFile_ReturnsFail()
    {
        SetupUploadBasics();
        _unitOfWorkMock.Setup(u => u.Agents.Query())
            .Returns(new TestAsyncEnumerable<Agent>(new[] { new Agent { AgentID = 5, FileID = 11 } }));
        var service = CreateUploadService(SetupStorage().Object);

        var result = await service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(new byte[] { 1, 2, 3 }, "doc.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10,
            AgentID = 5
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("الشخص المحدد لا ينتمي إلى ملف التقاعد المُرسل", result.Message);
    }

    [Fact]
    public async Task UploadAsync_InactiveDocType_ReturnsFail()
    {
        SetupUploadBasics(docTypeOverride: new DocumentType { DocumentTypeID = 1, TypeName = "هوية", IsActive = false });
        var service = CreateUploadService(SetupStorage().Object);

        var result = await service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(new byte[] { 1, 2, 3 }, "doc.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("نوع الوثيقة «هوية» غير نشط", result.Message);
    }

    [Fact]
    public async Task UploadAsync_DocTypeOwnerMismatch_ReturnsFail()
    {
        SetupUploadBasics(docTypeOverride: new DocumentType
        {
            DocumentTypeID = 1,
            TypeName = "وثيقة موظف",
            IsActive = true,
            OwnerType = DocumentOwnerType.Employee
        });
        var service = CreateUploadService(SetupStorage().Object);

        var result = await service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(new byte[] { 1, 2, 3 }, "doc.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("نوع الوثيقة «وثيقة موظف» لا يسمح بالرفع لهذا المالك", result.Message);
    }

    [Fact]
    public async Task UploadAsync_GeneralUpload_KeepsPersonMissingRows()
    {
        SetupUploadBasics();
        var generalRow = new MissingDocument
        {
            MissingDocumentID = 1, FileID = 10, DocumentTypeID = 1,
            Status = MissingDocumentStatusConstants.Pending
        };
        var agentRow = new MissingDocument
        {
            MissingDocumentID = 2, FileID = 10, DocumentTypeID = 1, AgentID = 5,
            Status = MissingDocumentStatusConstants.Pending
        };
        _unitOfWorkMock.Setup(u => u.MissingDocuments.Query())
            .Returns(new TestAsyncEnumerable<MissingDocument>(new[] { generalRow, agentRow }));
        var service = CreateUploadService(SetupStorage().Object);

        var result = await service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(new byte[] { 1, 2, 3 }, "doc.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10
        }, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(MissingDocumentStatusConstants.Fulfilled, generalRow.Status);
        Assert.Equal(MissingDocumentStatusConstants.Pending, agentRow.Status);
    }

    [Fact]
    public async Task UploadAsync_DuplicateContentHashSameOwner_ThrowsDuplicateDocument()
    {
        SetupUploadBasics();
        var bytes = new byte[] { 9, 9, 9 };
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
        _unitOfWorkMock.Setup(u => u.Documents.Query())
            .Returns(new TestAsyncEnumerable<Document>(new[]
            {
                new Document { DocumentID = 77, FileID = 10, DocumentTypeID = 1, Checksum = hash, FileName = "other.png", FileSize = 999 }
            }));
        var service = CreateUploadService(SetupStorage().Object);

        await Assert.ThrowsAsync<DuplicateDocumentException>(() => service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(bytes, "new.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10
        }, 1));
    }

    [Fact]
    public async Task UploadAsync_SameContentDifferentOwner_Succeeds()
    {
        SetupUploadBasics();
        var bytes = new byte[] { 9, 9, 9 };
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
        _unitOfWorkMock.Setup(u => u.Documents.Query())
            .Returns(new TestAsyncEnumerable<Document>(new[]
            {
                new Document { DocumentID = 77, FileID = 10, DocumentTypeID = 1, Checksum = hash, EntitledID = 7 }
            }));
        var service = CreateUploadService(SetupStorage().Object);

        var result = await service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(bytes, "new.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10
        }, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UploadAsync_LegacyNoChecksum_MatchesNameSizeSameOwner_ThrowsDuplicateDocument()
    {
        SetupUploadBasics();
        _unitOfWorkMock.Setup(u => u.Documents.Query())
            .Returns(new TestAsyncEnumerable<Document>(new[]
            {
                new Document { DocumentID = 77, FileID = 10, DocumentTypeID = 1, Checksum = null, FileName = "doc.png", FileSize = 3 }
            }));
        var service = CreateUploadService(SetupStorage().Object);

        await Assert.ThrowsAsync<DuplicateDocumentException>(() => service.UploadAsync(new UploadDocumentDto
        {
            File = new FakeRemoteFile(new byte[] { 1, 2, 3 }, "doc.png"),
            DocumentTypeID = 1,
            RetirementFileID = 10
        }, 1));
    }

    [Fact]
    public async Task UpdateAsync_InactiveDocType_ReturnsFail()
    {
        SetupBranch(null, isHq: true);
        SetupDocument(new Document
        {
            DocumentID = 1, FileID = 10, UploadedByUserID = 5,
            DocumentTypeID = 1, OwnerType = DocumentOwnerType.RetirementFile
        });
        _unitOfWorkMock.Setup(u => u.DocumentTypes.GetByIdAsync(1))
            .ReturnsAsync(new DocumentType { DocumentTypeID = 1, TypeName = "نوع", IsActive = false });

        var result = await _service.UpdateAsync(1, new UpdateDocumentDto { DocumentTypeID = 1, Notes = "م" }, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal("نوع الوثيقة «نوع» غير نشط", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_TwoPersonOwners_ReturnsFail()
    {
        SetupBranch(null, isHq: true);
        SetupDocument(new Document
        {
            DocumentID = 1, FileID = 10, UploadedByUserID = 5,
            DocumentTypeID = 1, OwnerType = DocumentOwnerType.RetirementFile
        });
        _unitOfWorkMock.Setup(u => u.DocumentTypes.GetByIdAsync(1))
            .ReturnsAsync(new DocumentType { DocumentTypeID = 1, TypeName = "نوع", IsActive = true });

        var result = await _service.UpdateAsync(1, new UpdateDocumentDto
        {
            DocumentTypeID = 1,
            AgentID = 5,
            EntitledPersonID = 7
        }, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن ربط الوثيقة بأكثر من مالك شخصي واحد", result.Message);
    }

    // ─── م21/م23: إعادة حساب النواقص ───

    [Fact]
    public async Task UpdateAsync_TypeChange_RecalculatesOldAndNewOwnerScopes()
    {
        SetupBranch(null, isHq: true);
        var doc = new Document
        {
            DocumentID = 1, FileID = 10, UploadedByUserID = 5,
            DocumentTypeID = 1, OwnerType = DocumentOwnerType.RetirementFile
        };
        SetupDocument(doc);
        _unitOfWorkMock.Setup(u => u.DocumentTypes.GetByIdAsync(2))
            .ReturnsAsync(new DocumentType { DocumentTypeID = 2, TypeName = "نوع2", IsActive = true });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var oldGeneral = new MissingDocument { MissingDocumentID = 1, FileID = 10, DocumentTypeID = 1, Status = MissingDocumentStatusConstants.Fulfilled };
        var newGeneral = new MissingDocument { MissingDocumentID = 2, FileID = 10, DocumentTypeID = 2, Status = MissingDocumentStatusConstants.Pending };
        var newAgentRow = new MissingDocument { MissingDocumentID = 3, FileID = 10, DocumentTypeID = 2, AgentID = 5, Status = MissingDocumentStatusConstants.Pending };
        _unitOfWorkMock.Setup(u => u.MissingDocuments.Query())
            .Returns(new TestAsyncEnumerable<MissingDocument>(new[] { oldGeneral, newGeneral, newAgentRow }));

        var result = await _service.UpdateAsync(1, new UpdateDocumentDto { DocumentTypeID = 2 }, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal(MissingDocumentStatusConstants.Pending, oldGeneral.Status);
        Assert.Equal(MissingDocumentStatusConstants.Fulfilled, newGeneral.Status);
        Assert.Equal(MissingDocumentStatusConstants.Pending, newAgentRow.Status);
    }

    [Fact]
    public async Task DeleteAsync_ReopensFulfilledRows_KeepsExempted()
    {
        SetupBranch(null, isHq: true);
        var doc = new Document
        {
            DocumentID = 1, FileID = 10, UploadedByUserID = 5,
            DocumentTypeID = 1, OwnerType = DocumentOwnerType.RetirementFile
        };
        SetupDocument(doc);
        _unitOfWorkMock.Setup(u => u.Documents.GetByIdAsync(1)).ReturnsAsync(doc);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var fulfilled = new MissingDocument { MissingDocumentID = 1, FileID = 10, DocumentTypeID = 1, Status = MissingDocumentStatusConstants.Fulfilled };
        var exempted = new MissingDocument { MissingDocumentID = 2, FileID = 10, DocumentTypeID = 1, Status = MissingDocumentStatusConstants.Exempted };
        _unitOfWorkMock.Setup(u => u.MissingDocuments.Query())
            .Returns(new TestAsyncEnumerable<MissingDocument>(new[] { fulfilled, exempted }));

        var result = await _service.DeleteAsync(1, 7);

        Assert.True(result.IsSuccess);
        Assert.True(doc.IsDeleted);
        Assert.Equal(MissingDocumentStatusConstants.Pending, fulfilled.Status);
        Assert.Equal(MissingDocumentStatusConstants.Exempted, exempted.Status);
    }
}
