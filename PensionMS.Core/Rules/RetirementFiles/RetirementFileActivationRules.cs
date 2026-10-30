using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Entities;
using PensionMS.Core.Rules.Documents;

namespace PensionMS.Core.Rules.RetirementFiles;

/// <summary>سياسة السماح باعتماد تفعيل الملف؛ لا تنفذ الاعتماد أو حساب الاستحقاق أو إنشاء المتطلبات.</summary>
public static class RetirementFileActivationRules
{
    public enum RejectionReason
    {
        None,
        MissingEffectiveFinancialEntitlement,
        IneligibleDirectRetireeRecipient,
        InvalidEntitledRecipients,
        UnsatisfiedDocuments
    }

    /// <summary>طلب حقائق المستحقين مرحلة متابعة، وليس سماحاً بالاعتماد أو سبب رفض.</summary>
    public readonly record struct Decision(RejectionReason Reason, bool RequiresEntitledPersonFacts = false)
    {
        public bool IsAllowed => Reason == RejectionReason.None && !RequiresEntitledPersonFacts;
    }

    public readonly record struct RecipientFacts(int? RetireeID, int? EntitledID, decimal? SharePercent);

    public static Decision EvaluateFinancialPrerequisite(bool hasEffectiveFinancialEntitlement) =>
        new(hasEffectiveFinancialEntitlement ? RejectionReason.None : RejectionReason.MissingEffectiveFinancialEntitlement);

    /// <summary>المستلمون النشطون المختارون للملف؛ تُطلب معرفات المستحقين فقط عند الحاجة إلى فحص حصصهم.</summary>
    public static Decision EvaluateRecipients(int fileRetireeId, bool retireeIsAlive,
        IReadOnlyCollection<RecipientFacts> recipients, IReadOnlyCollection<int>? activeEntitledIds = null)
    {
        if (recipients.Any(r => r.RetireeID.HasValue && (r.RetireeID != fileRetireeId || !retireeIsAlive)))
            return new(RejectionReason.IneligibleDirectRetireeRecipient);
        if (!recipients.Any(r => r.EntitledID.HasValue)) return new(RejectionReason.None);
        if (activeEntitledIds == null) return new(RejectionReason.None, RequiresEntitledPersonFacts: true);

        if (recipients.Any(r => !r.EntitledID.HasValue || !activeEntitledIds.Contains(r.EntitledID.Value)) ||
            activeEntitledIds.Any(id => !recipients.Any(r => r.EntitledID == id)) ||
            recipients.Select(r => r.EntitledID).Distinct().Count() != recipients.Count ||
            recipients.Any(r => r.SharePercent is null or <= 0 or > 100) || recipients.Sum(r => r.SharePercent ?? 0) != 100)
            return new(RejectionReason.InvalidEntitledRecipients);
        return new(RejectionReason.None);
    }

    /// <summary>حقائق وثائق مقيدة بالملف، ومعرفات التعهدات والإعفاءات السارية من استعلامات الخدمة.</summary>
    public sealed record DocumentFacts(IReadOnlyCollection<AutoDetectedMissingDocumentDto> AutomaticRequirements,
        IReadOnlyCollection<MissingDocument> ManualRecords, IReadOnlyCollection<Document> UploadedDocuments,
        ISet<int> ValidPledgeIds, ISet<int> ValidExemptionIds);

    public static Decision EvaluateDocuments(DocumentFacts facts, DateOnly evaluationDate)
    {
        var required = RetirementFileDocumentRules.Requirements(facts.AutomaticRequirements, facts.ManualRecords);
        var uploaded = facts.UploadedDocuments.Where(d => RetirementFileDocumentRules.IsValid(d, evaluationDate))
            .Select(RetirementFileDocumentRules.Owner).ToHashSet();
        if (required.Any(r => !uploaded.Contains(r) && !RetirementFileDocumentRules.IsCovered(r, required,
            facts.ManualRecords, facts.ValidPledgeIds, facts.ValidExemptionIds)))
            return new(RejectionReason.UnsatisfiedDocuments);
        return new(RejectionReason.None);
    }
}
