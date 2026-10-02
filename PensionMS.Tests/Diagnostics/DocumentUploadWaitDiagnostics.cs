using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services;
using PensionMS.Tests.Services;
using Xunit.Abstractions;

namespace PensionMS.Tests.Diagnostics;

// Characterization of the actual service with controlled dependencies.
// This proves response dependencies, not SQL timings or the user's root cause.
public class DocumentUploadWaitDiagnostics(ITestOutputHelper output)
{
    [Fact]
    public async Task Upload_WaitsForAuditThenComplianceThenReview_AfterDocumentSave()
    {
        var auditGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var complianceGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reviewGate = new TaskCompletionSource<ApiResponse<bool>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var auditEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var complianceEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reviewEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var unit = new Mock<IUnitOfWork>();
        var storage = new Mock<IFileStorageService>();
        var branch = new Mock<IBranchScopeProvider>();
        var audit = new Mock<IImmutableAuditService>();
        var compliance = new Mock<IComplianceEngine>();
        var review = new Mock<IFileReviewService>();
        branch.Setup(b => b.IsHQ()).Returns(true);
        unit.Setup(u => u.DocumentTypes.GetByIdAsync(1))
            .ReturnsAsync(new DocumentType { DocumentTypeID = 1, IsActive = true });
        unit.Setup(u => u.Documents.Query())
            .Returns(new TestAsyncEnumerable<Document>(Array.Empty<Document>()));
        unit.Setup(u => u.MissingDocuments.Query())
            .Returns(new TestAsyncEnumerable<MissingDocument>(Array.Empty<MissingDocument>()));
        unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<long?>()))
            .ReturnsAsync("uploads/documents/diagnostic.png");
        audit.Setup(a => a.LogAsync(1, "", "UploadDocument", "Document", It.IsAny<int>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<Guid?>()))
            .Returns(() => { auditEntered.TrySetResult(); return auditGate.Task; });
        compliance.Setup(c => c.RefreshComplianceCacheAsync(10))
            .Returns(() => { complianceEntered.TrySetResult(); return complianceGate.Task; });
        review.Setup(r => r.RequestReviewAsync(10, 1, It.IsAny<DateTime?>()))
            .Returns(() => { reviewEntered.TrySetResult(); return reviewGate.Task; });

        var service = new DocumentService(unit.Object, storage.Object, compliance.Object,
            Mock.Of<IEntitlementEngine>(), review.Object, branch.Object, audit.Object,
            NullLogger<DocumentService>.Instance);
        var upload = service.UploadAsync(new UploadDocumentDto
        {
            File = new DiagnosticFile(), RetirementFileID = 10, DocumentTypeID = 1
        }, 1);
        try
        {
            await auditEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            unit.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.False(upload.IsCompleted);
            Assert.False(complianceEntered.Task.IsCompleted);
            output.WriteLine("Document saved; response pending on audit; compliance not started.");

            auditGate.SetResult();
            await complianceEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.False(upload.IsCompleted);
            Assert.False(reviewEntered.Task.IsCompleted);
            output.WriteLine("Audit released; response pending on compliance; review not started.");

            complianceGate.SetResult();
            await reviewEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.False(upload.IsCompleted);
            output.WriteLine("Compliance released; response pending on review.");

            reviewGate.SetResult(ApiResponse<bool>.Success(true));
            var result = await upload.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.True(result.IsSuccess);
            unit.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
            output.WriteLine("Review released; final SaveChanges completed; response successful.");
        }
        finally
        {
            auditGate.TrySetResult(); complianceGate.TrySetResult();
            reviewGate.TrySetResult(ApiResponse<bool>.Success(true));
            await upload.WaitAsync(TimeSpan.FromSeconds(3));
        }
    }

    private sealed class DiagnosticFile : IRemoteFile
    {
        public string FileName => "diagnostic.png";
        public string ContentType => "image/png";
        public long Length => 3;
        public Stream OpenReadStream() => new MemoryStream(new byte[] { 1, 2, 3 });
    }
}
