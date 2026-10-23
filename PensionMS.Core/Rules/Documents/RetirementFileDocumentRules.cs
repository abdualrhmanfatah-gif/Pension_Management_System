using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Entities;

namespace PensionMS.Core.Rules.Documents;

public readonly record struct RetirementDocumentRequirement(int DocumentTypeID, int? RetireeID,
    int? AgentID, int? GuardianID, int? EntitledID, int? PledgeID, int? ExemptionID);

/// <summary>قواعد مشتركة لاستيفاء الوثيقة بدلالة نوعها ومالكها وصلاحيتها.</summary>
public static class RetirementFileDocumentRules
{
    /// <summary>يحسب الوثائق المرفوعة الصحيحة فقط؛ التغطية بتعهد أو إعفاء لا تُعد رفعاً لوثيقة.</summary>
    public static ComplianceSummary Summarize(IEnumerable<AutoDetectedMissingDocumentDto> automatic,
        IEnumerable<MissingDocument> manual, IEnumerable<Document> documents, DateOnly today)
    {
        var required = Requirements(automatic, manual);
        var uploaded = documents.Where(d => IsValid(d, today)).Select(Owner).ToHashSet();
        var missing = required.Where(p => !uploaded.Contains(p)).ToList();
        return new ComplianceSummary
        {
            TotalRequiredCount = required.Count, UploadedCount = required.Count - missing.Count,
            MissingCount = missing.Count,
            MissingDocumentTypeIds = missing.Select(p => p.DocumentTypeID).Distinct().ToList()
        };
    }

    public static bool IsValid(Document document, DateOnly today) => !document.IsDeleted &&
        document.PendingReviewStatus != PendingReviewStatus.Rejected &&
        (!document.ExpiryDate.HasValue || document.ExpiryDate.Value >= today);

    public static RetirementDocumentRequirement Owner(Document d) => new(d.DocumentTypeID, d.RetireeID,
        d.AgentID, d.GuardianID, d.EntitledID, d.PledgeID, d.ExemptionID);

    public static RetirementDocumentRequirement Owner(AutoDetectedMissingDocumentDto p) => new(p.DocumentTypeID,
        p.RetireeID, p.AgentID, p.GuardianID, p.EntitledID, p.PledgeID, p.ExemptionID);

    public static List<RetirementDocumentRequirement> Requirements(
        IEnumerable<AutoDetectedMissingDocumentDto> automatic, IEnumerable<MissingDocument> manual)
    {
        var result = automatic.Select(Owner).ToHashSet();
        foreach (var item in manual.Where(m => !m.IsDeleted &&
            MissingDocumentStatusConstants.OutstandingStatuses.Contains(m.Status)))
            result.Add(Owner(item, result));
        return result.ToList();
    }

    // PledgeID/ExemptionID on a missing record normally identify its coverage, not its document owner.
    public static RetirementDocumentRequirement Owner(MissingDocument m,
        IEnumerable<RetirementDocumentRequirement> requirements)
    {
        var owner = new RetirementDocumentRequirement(m.DocumentTypeID, null, m.AgentID, m.GuardianID,
            m.EntitledID, m.DocumentTypeID == DocumentTypeConstants.PledgeDocument ? m.PledgeID : null,
            m.DocumentTypeID == DocumentTypeConstants.ExemptionDirective ? m.ExemptionID : null);
        return requirements.FirstOrDefault(p => (p with { RetireeID = null }) == owner, owner);
    }

    public static bool IsCovered(RetirementDocumentRequirement requirement,
        IReadOnlyCollection<RetirementDocumentRequirement> requirements, IEnumerable<MissingDocument> missing,
        ISet<int> pledges, ISet<int> exemptions) =>
        // The directive proves the coverage; the coverage cannot replace its own directive.
        !requirement.PledgeID.HasValue && !requirement.ExemptionID.HasValue && missing.Any(m => !m.IsDeleted &&
        Owner(m, requirements) == requirement &&
        (m.Status == MissingDocumentStatusConstants.Exempted && m.ExemptionID.HasValue && exemptions.Contains(m.ExemptionID.Value) ||
         MissingDocumentStatusConstants.OutstandingStatuses.Contains(m.Status) && m.PledgeID.HasValue && pledges.Contains(m.PledgeID.Value)));
}
