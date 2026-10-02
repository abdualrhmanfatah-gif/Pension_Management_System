using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services;

namespace PensionMS.Tests.Services;

/// <summary>
/// Phase 2: deferred batch upload + single final refresh.
/// Proves file-level work runs once per batch, doc-level guarantees stay,
/// refresh is idempotent, partial failures stay consistent.
/// </summary>
public class DocumentBatchUploadTests
{
    private sealed class FakeRemoteFile : IRemoteFile
    {
        private readonly byte[] _bytes;
        public FakeRemoteFile(byte[] bytes) { _bytes = bytes; }
        public string FileName => "batch.png";
        public string ContentType => "image/png";
        public long Length => _bytes.Length;
        public Stream OpenReadStream() => new MemoryStream(_bytes, writable: false);
    }

    private sealed class Fixture
    {
        public readonly Mock<IUnitOfWork> Unit = new();
        public readonly Mock<IComplianceEngine> Compliance = new();
        public readonly Mock<IFileReviewService> Review = new();
        public readonly Mock<IEntitlementEngine> Entitlement = new();
        public readonly Mock<IImmutableAuditService> Audit = new();
        public readonly Mock<IBranchScopeProvider> Branch = new();
        public readonly List<Document> Documents = new();
        public readonly List<MissingDocument> Missing = new();
        public readonly Mock<IRepository<Document>> DocsRepo;
        public readonly RetirementFile File = new()
        {
            FileID = 10, PensionBranchID = 1, StatusID = FileStatusConstants.Active,
            ReviewStatus = FileReviewStatus.None, CreatedAt = new DateTime(2025, 1, 1)
        };
        public DocumentService Service { get; }

        public Fixture()
        {
            Branch.Setup(b => b.IsHQ()).Returns(true);
            DocsRepo = new Mock<IRepository<Document>>();
            DocsRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<Document>(Documents));
            DocsRepo.Setup(r => r.AddAsync(It.IsAny<Document>()))
                .Callback<Document>(d => { d.DocumentID = 1000 + Documents.Count; Documents.Add(d); })
                .Returns(Task.CompletedTask);
            var missingRepo = new Mock<IRepository<MissingDocument>>();
            missingRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<MissingDocument>(Missing));
            Unit.Setup(u => u.Documents).Returns(DocsRepo.Object);
            Unit.Setup(u => u.MissingDocuments).Returns(missingRepo.Object);
            Unit.Setup(u => u.RetirementFiles.GetByIdAsync(10)).ReturnsAsync(File);
            Unit.Setup(u => u.DocumentTypes.GetByIdAsync(1))
                .ReturnsAsync(new DocumentType { DocumentTypeID = 1, TypeName = "نوع", IsActive = true });
            Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            Review.Setup(r => r.RequestReviewAsync(10, It.IsAny<int>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(PensionMS.Core.DTOs.Common.ApiResponse<bool>.Success(true));
            var storage = new Mock<IFileStorageService>();
            storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<long?>()))
                .ReturnsAsync("documents/batch.png");
            Service = new DocumentService(Unit.Object, storage.Object, Compliance.Object,
                Entitlement.Object, Review.Object, Branch.Object, Audit.Object,
                NullLogger<DocumentService>.Instance);
        }

        public void SeedDoc(int id, int typeId = 1) => Documents.Add(new Document
        {
            DocumentID = id, FileID = 10, DocumentTypeID = typeId,
            OwnerType = DocumentOwnerType.RetirementFile, FileName = $"d{id}.png", IsDeleted = false
        });

        public void SeedMissing(int id, int typeId) => Missing.Add(new MissingDocument
        {
            MissingDocumentID = id, FileID = 10, DocumentTypeID = typeId,
            Status = MissingDocumentStatusConstants.Pending
        });

        public UploadDocumentDto UploadDto() => new()
        {
            File = new FakeRemoteFile(new byte[] { 1, 2, 3 }),
            DocumentTypeID = 1,
            RetirementFileID = 10
        };
    }

    [Fact]
    public async Task UploadAsync_Deferred_SkipsFileLevelWork_KeepsDocGuarantees()
    {
        var f = new Fixture();
        f.SeedMissing(1, 1);

        var result = await f.Service.UploadAsync(f.UploadDto(), 7, requestId: null, deferPostProcessing: true);

        Assert.True(result.IsSuccess);
        Assert.Single(f.Documents);
        f.Audit.Verify(a => a.LogAsync(7, It.IsAny<string>(), "UploadDocument", "Document",
            It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<Guid?>()), Times.Once);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(It.IsAny<int>()), Times.Never);
        f.Review.Verify(r => r.RequestReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
        Assert.Equal(MissingDocumentStatusConstants.Pending, f.Missing[0].Status);
    }

    [Fact]
    public async Task UploadAsync_Eager_RunsFileLevelOnce_OldBehavior()
    {
        var f = new Fixture();
        f.SeedMissing(1, 1);

        var result = await f.Service.UploadAsync(f.UploadDto(), 7);

        Assert.True(result.IsSuccess);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(10), Times.Once);
        f.Review.Verify(r => r.RequestReviewAsync(10, 7, It.IsAny<DateTime?>()), Times.Once);
        Assert.Equal(MissingDocumentStatusConstants.Fulfilled, f.Missing[0].Status);
    }

    [Fact]
    public async Task RefreshAfterUpload_Batch_RunsFileLevelOnce()
    {
        var f = new Fixture();
        f.SeedDoc(1); f.SeedDoc(2); f.SeedDoc(3);
        f.SeedMissing(1, 1); f.SeedMissing(2, 1);

        var result = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 1, 2, 3 }, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data!.ProcessedDocuments);
        Assert.Empty(result.Data.SkippedDocumentIds);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(10), Times.Once);
        f.Review.Verify(r => r.RequestReviewAsync(10, 7, It.IsAny<DateTime?>()), Times.Once);
        Assert.All(f.Missing, m => Assert.Equal(MissingDocumentStatusConstants.Fulfilled, m.Status));
    }

    [Fact]
    public async Task RefreshAfterUpload_PartialIds_SkipsUnknown_KeepsConsistency()
    {
        var f = new Fixture();
        f.SeedDoc(1); f.SeedDoc(2); f.SeedDoc(3); f.SeedDoc(4);
        f.SeedMissing(1, 1);

        var result = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 1, 2, 3, 4, 999 }, 7);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Data!.ProcessedDocuments);
        Assert.Equal(new List<int> { 999 }, result.Data.SkippedDocumentIds);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(10), Times.Once);
        f.Review.Verify(r => r.RequestReviewAsync(10, 7, It.IsAny<DateTime?>()), Times.Once);
        Assert.Equal(MissingDocumentStatusConstants.Fulfilled, f.Missing[0].Status);
    }

    [Fact]
    public async Task RefreshAfterUpload_ReviewFails_ReturnsFail_DocsPersisted()
    {
        var f = new Fixture();
        f.SeedDoc(1); f.SeedDoc(2);
        f.Review.Setup(r => r.RequestReviewAsync(10, It.IsAny<int>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(PensionMS.Core.DTOs.Common.ApiResponse<bool>.Fail("لا يمكن طلب مراجعة لملف بحالة موقوف أو مغلق أو مرفوض"));

        var result = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 1, 2 }, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, f.Documents.Count(d => !d.IsDeleted));
    }

    [Fact]
    public async Task RefreshAfterUpload_Retry_Idempotent_NoDuplicateWork()
    {
        var f = new Fixture();
        f.SeedDoc(1); f.SeedDoc(2);
        f.SeedMissing(1, 1);

        var first = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 1, 2 }, 7);
        var second = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 1, 2 }, 7);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(1, first.Data!.FulfilledMissingCount);
        Assert.Equal(0, second.Data!.FulfilledMissingCount);
        Assert.Equal(MissingDocumentStatusConstants.Fulfilled, f.Missing[0].Status);
        f.Review.Verify(r => r.RequestReviewAsync(10, 7, It.IsAny<DateTime?>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RefreshAfterUpload_ConcurrentDoc_PickedUpOnNextRefresh()
    {
        var f = new Fixture();
        f.SeedDoc(1);

        var first = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 1 }, 7);
        f.SeedDoc(2);
        f.SeedMissing(2, 1);
        var second = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 2 }, 7);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        // Review recomputes from all audits each call: concurrent doc covered without batch coupling.
        f.Review.Verify(r => r.RequestReviewAsync(10, 7, It.IsAny<DateTime?>()), Times.Exactly(2));
        Assert.Equal(MissingDocumentStatusConstants.Fulfilled, f.Missing[0].Status);
    }

    [Fact]
    public async Task RefreshAfterUpload_OtherBranch_ReturnsFail()
    {
        var f = new Fixture();
        f.Branch.Setup(b => b.IsHQ()).Returns(false);
        f.Branch.Setup(b => b.GetCurrentBranchId()).Returns(2);
        f.Unit.Setup(u => u.RetirementFiles.Query()).Returns(
            new TestAsyncEnumerable<RetirementFile>(new[]
            {
                new RetirementFile { FileID = 10, PensionBranchID = 1 }
            }));

        var result = await f.Service.RefreshAfterUploadAsync(10, new List<int> { 1 }, 7);

        Assert.False(result.IsSuccess);
        f.Compliance.Verify(c => c.RefreshComplianceCacheAsync(It.IsAny<int>()), Times.Never);
        f.Review.Verify(r => r.RequestReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
    }
}
