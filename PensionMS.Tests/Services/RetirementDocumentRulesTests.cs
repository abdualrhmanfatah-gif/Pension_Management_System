using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Entities;
using PensionMS.Core.Rules.Documents;

namespace PensionMS.Tests.Services;

public class RetirementDocumentRulesTests
{
    [Fact]
    public void Summary_CountsDistinctOwnerPairsAndExcludesRejectedDocuments()
    {
        var automatic = new List<AutoDetectedMissingDocumentDto>
        {
            new() { DocumentTypeID = 1 },
            new() { DocumentTypeID = 1, EntitledID = 7 },
            new() { DocumentTypeID = 2, AgentID = 5 }
        };
        var manual = new[]
        {
            new MissingDocument { DocumentTypeID = 5, Status = MissingDocumentStatusConstants.Pending },
            new MissingDocument { DocumentTypeID = 5, Status = MissingDocumentStatusConstants.Pending }
        };
        var documents = new[]
        {
            new Document { DocumentTypeID = 1 },
            new Document { DocumentTypeID = 5, PendingReviewStatus = PendingReviewStatus.Rejected }
        };
        var summary = RetirementFileDocumentRules.Summarize(automatic, manual, documents, new DateOnly(2026, 10, 23));
        Assert.Equal(4, summary.TotalRequiredCount);
        Assert.Equal(1, summary.UploadedCount);
        Assert.Equal(3, summary.MissingCount);
        Assert.Equal(new[] { 1, 2, 5 }, summary.MissingDocumentTypeIds.OrderBy(id => id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Coverage_AllowsActivationButDoesNotCountAsUploadedDocument(bool exemption)
    {
        var automatic = new[] { new AutoDetectedMissingDocumentDto { DocumentTypeID = 1, AgentID = 8 } };
        var manual = new[] { new MissingDocument { DocumentTypeID = 1, AgentID = 8,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null } };
        var required = RetirementFileDocumentRules.Requirements(automatic, manual);
        var requirement = Assert.Single(required);
        Assert.True(RetirementFileDocumentRules.IsCovered(requirement, required, manual,
            new HashSet<int> { 10 }, new HashSet<int> { 11 }));
        var summary = RetirementFileDocumentRules.Summarize(automatic, manual, [], new DateOnly(2026, 10, 23));
        Assert.Equal(1, summary.MissingCount);
        Assert.Equal(0, summary.UploadedCount);
    }
}
