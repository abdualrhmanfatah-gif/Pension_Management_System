using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Rules.Entitlements;
using PensionMS.Core.Time;

namespace PensionMS.Core.Rules.Documents;

/// <summary>اختيار الوثائق المطلوبة من حقائق محملة ومقيدة بالملف؛ الصلاحية والتغطية قرارات منفصلة.</summary>
public static class RetirementDocumentRequirementRules
{
    public readonly record struct RetireeFacts(int RetireeID, bool IsAlive, int? HealthStatusID);

    public readonly record struct EntitledPersonFacts(int EntitledID, DateOnly BirthDate, int RelationshipID,
        int? SocialStatusID, int? HealthStatusID, bool HasDisabilityException, string? StopCode);

    /// <summary>قوائم الكيانات المختارة بواسطة استعلامات الخدمة، بترتيبها الأصلي ودون إعادة تصفية حالاتها.</summary>
    public sealed record FileFacts(RetireeFacts? Retiree, bool HasAnyPaymentBooks,
        IReadOnlyList<EntitledPersonFacts> EntitledPersons, IReadOnlyList<int> AgentIds,
        IReadOnlyList<int> GuardianIds, IReadOnlyList<int> PledgeIds, IReadOnlyList<int> ExemptionIds);

    public static IReadOnlyList<RetirementDocumentRequirement> SelectRequirements(FileFacts facts,
        IReadOnlySet<int> activeDocumentTypeIds, DateOnly evaluationDate)
    {
        var required = new List<RetirementDocumentRequirement>();
        void Add(int type, int? retireeId = null, int? agentId = null, int? guardianId = null,
            int? entitledId = null, int? pledgeId = null, int? exemptionId = null)
        {
            if (activeDocumentTypeIds.Contains(type))
                required.Add(new(type, retireeId, agentId, guardianId, entitledId, pledgeId, exemptionId));
        }

        Add(DocumentTypeConstants.FileOriginals);
        if (facts.HasAnyPaymentBooks) Add(DocumentTypeConstants.PaymentBookImage);

        if (facts.Retiree is { } retiree)
        {
            var requiresDeathEvidence = !retiree.IsAlive ||
                retiree.HealthStatusID is HealthStatusConstants.Deceased or HealthStatusConstants.Martyr;
            if (requiresDeathEvidence) Add(DocumentTypeConstants.InheritanceCert);
            if (retiree.IsAlive) Add(DocumentTypeConstants.NationalId, retireeId: retiree.RetireeID);
            if (requiresDeathEvidence) Add(DocumentTypeConstants.DeathCertificate, retireeId: retiree.RetireeID);
            if (retiree.HealthStatusID is HealthStatusConstants.PartiallyDisabled or HealthStatusConstants.FullyDisabled)
                Add(DocumentTypeConstants.MedicalReport, retireeId: retiree.RetireeID);
        }

        // حالة الحياة وحدها تتحكم في سياق المستحقين، حتى عند تعارضها مع الحالة الصحية.
        if (facts.Retiree is { IsAlive: false })
        {
            foreach (var person in facts.EntitledPersons)
            {
                if (person.StopCode is EntitledStopCodes.AgeLimit or EntitledStopCodes.DaughterMarried) continue;
                var profile = new EntitledPersonRules.EntitledPersonProfile(person.RelationshipID,
                    YemenTime.Age(person.BirthDate, evaluationDate), person.SocialStatusID,
                    person.HealthStatusID, person.HasDisabilityException);
                foreach (var requirement in EntitledPersonRules.DocumentRequirements(profile))
                    Add(requirement.DocumentTypeID, entitledId: person.EntitledID);
            }
        }

        foreach (var id in facts.AgentIds)
        {
            Add(DocumentTypeConstants.NationalId, agentId: id);
            Add(DocumentTypeConstants.AgencyDocument, agentId: id);
        }
        foreach (var id in facts.GuardianIds)
        {
            Add(DocumentTypeConstants.NationalId, guardianId: id);
            Add(DocumentTypeConstants.GuardianshipDocument, guardianId: id);
        }
        foreach (var id in facts.PledgeIds) Add(DocumentTypeConstants.PledgeDocument, pledgeId: id);
        foreach (var id in facts.ExemptionIds) Add(DocumentTypeConstants.ExemptionDirective, exemptionId: id);

        return required.AsReadOnly();
    }
}
