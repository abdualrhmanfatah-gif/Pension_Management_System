using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Rules.Entitlements;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Core.Rules.RetirementFiles;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

internal static class RetirementFileActivationValidator
{
    public static bool IsEditable(PensionMS.Core.Entities.RetirementFile file)
        => !file.IsDeleted && PensionMS.Core.Constants.FileStatusConstants.IsCurrent(file.StatusID ?? 0);

    internal static async Task<int> ApprovedStatusAsync(IUnitOfWork uow, PensionMS.Core.Entities.RetirementFile file)
    {
        if (file.PreviousStatusID == FileStatusConstants.EntitlementEnded) return FileStatusConstants.EntitlementEnded;
        var entitlements = await uow.FinancialEntitlements.Query().AsNoTracking()
            .Where(e => e.RetireeID == file.RetireeID && !e.IsDeleted).ToListAsync();
        return PensionEntitlementRules.AllEnded(entitlements, YemenTime.Today)
                ? FileStatusConstants.EntitlementEnded : FileStatusConstants.Active;
    }

    internal static async Task<string?> ValidateAsync(IUnitOfWork uow, IMissingDocumentService missingDocuments, int fileId)
    {
        var file = await uow.RetirementFiles.Query().Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId && !f.IsDeleted);
        if (file == null) return "الملف غير موجود";
        var now = DateTime.UtcNow;
        var today = YemenTime.LocalDate(new DateTimeOffset(now));
        var hasEffectiveEntitlement = await uow.FinancialEntitlements.Query().Where(e => e.RetireeID == file.RetireeID)
            .AnyAsync(PensionEntitlementRules.EffectiveOn(today));
        var financialDecision = RetirementFileActivationRules.EvaluateFinancialPrerequisite(hasEffectiveEntitlement);
        if (!financialDecision.IsAllowed) return RejectionMessage(financialDecision);
        var recipients = await uow.SalaryRecipients.Query().Where(r => r.FileID == fileId && !r.IsDeleted && r.IsActive).ToListAsync();
        var recipientFacts = recipients.Select(r => new RetirementFileActivationRules.RecipientFacts(
            r.RetireeID, r.EntitledID, r.SharePercent)).ToArray();
        var recipientDecision = RetirementFileActivationRules.EvaluateRecipients(file.RetireeID, file.Retiree.IsAlive, recipientFacts);
        if (recipientDecision.Reason != RetirementFileActivationRules.RejectionReason.None) return RejectionMessage(recipientDecision);
        if (recipientDecision.RequiresEntitledPersonFacts)
        {
            var entitledIds = await uow.EntitledPersons.Query().Where(e => e.FileID == fileId &&
                !e.IsDeleted && e.IsActive && !e.IsDeceased).Select(e => e.EntitledID).ToListAsync();
            recipientDecision = RetirementFileActivationRules.EvaluateRecipients(file.RetireeID, file.Retiree.IsAlive, recipientFacts, entitledIds);
            if (!recipientDecision.IsAllowed) return RejectionMessage(recipientDecision);
        }
        var automatic = await missingDocuments.GetRequiredPairsAsync(fileId);
        var manual = await uow.MissingDocuments.Query().AsNoTracking()
            .Where(m => m.FileID == fileId && !m.IsDeleted).ToListAsync();
        var documents = await uow.Documents.Query().AsNoTracking()
            .Where(d => d.FileID == fileId && !d.IsDeleted).ToListAsync();
        var pledges = await uow.DocumentPledges.Query().Where(p => p.FileID == fileId && !p.IsDeleted &&
            p.Deadline > now && p.Status != PledgeStatusConstants.Expired && p.Status != PledgeStatusConstants.Fulfilled)
            .Select(p => p.PledgeID).ToListAsync();
        var exemptions = await uow.DocumentExemptions.Query().AsNoTracking()
            .Where(e => e.FileID == fileId && !e.IsDeleted).Select(e => e.ExemptionID).ToListAsync();
        var documentDecision = RetirementFileActivationRules.EvaluateDocuments(
            new RetirementFileActivationRules.DocumentFacts(automatic, manual, documents, pledges.ToHashSet(), exemptions.ToHashSet()), today);
        return RejectionMessage(documentDecision);
    }

    private static string? RejectionMessage(RetirementFileActivationRules.Decision decision) => decision.Reason switch
    {
        RetirementFileActivationRules.RejectionReason.None => null,
        RetirementFileActivationRules.RejectionReason.MissingEffectiveFinancialEntitlement => "لا يمكن اعتماد التفعيل دون استحقاق مالي سارٍ",
        RetirementFileActivationRules.RejectionReason.IneligibleDirectRetireeRecipient => "المتقاعد غير مؤهل للاستلام المباشر",
        RetirementFileActivationRules.RejectionReason.InvalidEntitledRecipients => "يجب استكمال مستلمي حصص المستحقين ومجموع النسب 100٪",
        RetirementFileActivationRules.RejectionReason.UnsatisfiedDocuments => "يجب تسوية الوثائق الناقصة بوثائق أو إعفاءات أو تعهدات سارية قبل الاعتماد",
        _ => throw new ArgumentOutOfRangeException(nameof(decision))
    };
}
