using Microsoft.EntityFrameworkCore.Query;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services;
using Xunit;

namespace PensionMS.Tests.Services;

public class ComplianceEngineTests
{
    [Fact]
    public async Task GetComplianceSummary_CountsOwnerPairs_AndExcludesRejectedDocs()
    {
        var missingServiceMock = new Mock<IMissingDocumentService>();
        missingServiceMock.Setup(m => m.GetRequiredPairsAsync(10))
            .ReturnsAsync(new List<AutoDetectedMissingDocumentDto>
            {
                new() { DocumentTypeID = 1 },
                new() { DocumentTypeID = 1, EntitledID = 7 },
                new() { DocumentTypeID = 2, AgentID = 5 }
            });

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.MissingDocuments.Query())
            .Returns(new TestAsyncEnumerable<MissingDocument>(new[]
            {
                new MissingDocument
                {
                    MissingDocumentID = 1, FileID = 10, DocumentTypeID = 5,
                    Status = MissingDocumentStatusConstants.Pending
                }
            }));
        unitOfWorkMock.Setup(u => u.Documents.Query())
            .Returns(new TestAsyncEnumerable<Document>(new[]
            {
                new Document { DocumentID = 1, FileID = 10, DocumentTypeID = 1, PendingReviewStatus = PendingReviewStatus.None },
                new Document { DocumentID = 2, FileID = 10, DocumentTypeID = 5, PendingReviewStatus = PendingReviewStatus.Rejected }
            }));

        var engine = new ComplianceEngine(unitOfWorkMock.Object, missingServiceMock.Object);

        var summary = await engine.GetComplianceSummaryAsync(10);

        // 3 أزواج: (نوع1 عام) + (نوع1 للمستحق7) + (نوع2 للوكيل5) — اليدوي (نوع5) رابع... يُحسب أيضًا
        Assert.Equal(4, summary.TotalRequiredCount);
        // المستوفى فقط النوع1 العام؛ المرفوض (نوع5) لا يُحتسب
        Assert.Equal(1, summary.UploadedCount);
        Assert.Equal(3, summary.MissingCount);
        Assert.Contains(1, summary.MissingDocumentTypeIds);
        Assert.Contains(2, summary.MissingDocumentTypeIds);
        Assert.Contains(5, summary.MissingDocumentTypeIds);
        Assert.Equal(3, summary.MissingDocumentTypeIds.Distinct().Count());
    }
}
