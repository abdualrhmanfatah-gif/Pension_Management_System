using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Core.Rules.Documents;

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
        if (!await uow.FinancialEntitlements.Query().Where(e => e.RetireeID == file.RetireeID)
            .AnyAsync(PensionEntitlementRules.EffectiveOn(today)))
            return "لا يمكن اعتماد التفعيل دون استحقاق مالي سارٍ";
        if (!await uow.PaymentBooks.Query().AnyAsync(b => b.FileID == fileId && !b.IsDeleted &&
            b.Status == PaymentBookStatus.Active && b.IssueDate <= today && b.ExpiryDate >= today))
            return "لا يمكن اعتماد التفعيل دون دفتر صرف سارٍ";
        var recipients = await uow.SalaryRecipients.Query().Where(r => r.FileID == fileId && !r.IsDeleted && r.IsActive).ToListAsync();
        if (recipients.Count == 0) return "لا يمكن اعتماد التفعيل دون مستلم معاش نشط";
        if (recipients.Any(r => r.RetireeID.HasValue && (r.RetireeID != file.RetireeID || !file.Retiree.IsAlive)))
            return "المتقاعد غير مؤهل للاستلام المباشر";
        if (recipients.Any(r => r.EntitledID.HasValue))
        {
            var entitledIds = await uow.EntitledPersons.Query().Where(e => e.FileID == fileId &&
                !e.IsDeleted && e.IsActive && !e.IsDeceased).Select(e => e.EntitledID).ToListAsync();
            if (recipients.Any(r => !r.EntitledID.HasValue || !entitledIds.Contains(r.EntitledID.Value)) ||
                entitledIds.Any(id => !recipients.Any(r => r.EntitledID == id)) ||
                recipients.Select(r => r.EntitledID).Distinct().Count() != recipients.Count ||
                recipients.Any(r => r.SharePercent is null or <= 0 or > 100) || recipients.Sum(r => r.SharePercent ?? 0) != 100)
                return "يجب استكمال مستلمي حصص المستحقين ومجموع النسب 100٪";
        }
        var automatic = await missingDocuments.GetRequiredPairsAsync(fileId);
        var manual = await uow.MissingDocuments.Query().AsNoTracking()
            .Where(m => m.FileID == fileId && !m.IsDeleted).ToListAsync();
        var required = RetirementFileDocumentRules.Requirements(automatic, manual);
        var documents = await uow.Documents.Query().AsNoTracking()
            .Where(d => d.FileID == fileId && !d.IsDeleted).ToListAsync();
        var uploaded = documents.Where(d => RetirementFileDocumentRules.IsValid(d, today))
            .Select(RetirementFileDocumentRules.Owner).ToHashSet();
        var pledges = await uow.DocumentPledges.Query().Where(p => p.FileID == fileId && !p.IsDeleted &&
            p.Deadline > now && p.Status != PledgeStatusConstants.Expired && p.Status != PledgeStatusConstants.Fulfilled)
            .Select(p => p.PledgeID).ToListAsync();
        var exemptions = await uow.DocumentExemptions.Query().AsNoTracking()
            .Where(e => e.FileID == fileId && !e.IsDeleted).Select(e => e.ExemptionID).ToListAsync();
        var validPledges = pledges.ToHashSet();
        var validExemptions = exemptions.ToHashSet();
        if (required.Any(r => !uploaded.Contains(r) &&
            !RetirementFileDocumentRules.IsCovered(r, required, manual, validPledges, validExemptions)))
            return "يجب تسوية الوثائق الناقصة بوثائق أو إعفاءات أو تعهدات سارية قبل الاعتماد";
        return null;
    }
}
