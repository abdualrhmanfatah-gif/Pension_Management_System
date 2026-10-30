using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Rules.Entitlements;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Time;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Helpers;

namespace PensionMS.Infrastructure.Services.Payroll;

/// <summary>
/// اختيار بيانات الاحتساب المعتمدة وتطبيق قواعد المستلم والاستحقاق ودفتر الصرف.
/// تستمر المراجعة والإيقافات في منع الصرف دون منع الاحتساب من آخر نسخة معتمدة.
/// يعيد البنود دون إضافتها أو حفظها؛ المستدعي يملك استبدال البنود والإشعار.
/// البيانات المرتبطة تُحمّل مسبقاً في الذاكرة لتجنب استعلامات N+1.
/// </summary>
public class PayrollGenerationService : IPayrollGenerationService
{
    private readonly IUnitOfWork _unitOfWork;

    public PayrollGenerationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollGenerationResult> GenerateForBatchAsync(SalaryPaymentBatch batch, DateTime runAt, int userId)
    {
        var result = new PayrollGenerationResult();
        var batchId = batch.BatchID;
        var onDate = batch.BatchMonth == default
            ? YemenTime.LocalDate(new DateTimeOffset(DateTime.SpecifyKind(runAt, DateTimeKind.Utc)))
            : DateOnly.FromDateTime(batch.BatchMonth);
        var periodEnd = new DateOnly(onDate.Year, onDate.Month, DateTime.DaysInMonth(onDate.Year, onDate.Month));
        var isEffectiveForPeriod = PensionEntitlementRules.EffectiveDuring(onDate, periodEnd).Compile();

        var activeFiles = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Where(f => (f.StatusID == FileStatusConstants.Active || f.StatusID == FileStatusConstants.Pending && f.PreviousStatusID != FileStatusConstants.EntitlementEnded) && !f.IsDeleted)
            .ToListAsync();

        if (activeFiles.Count == 0)
        {
            return result;
        }

        var recordedFiles = await _unitOfWork.SalaryPaymentItems.Query().AsNoTracking()
            .Where(i => !i.IsDeleted && i.BatchID != batchId && !i.Batch.IsDeleted &&
                i.Batch.BatchMonth.Year == onDate.Year && i.Batch.BatchMonth.Month == onDate.Month)
            .Select(i => i.FileID).Distinct().ToListAsync();

        // Only first-review drafts may read live financial data. All other files use their snapshot.
        var initialDrafts = await LoadInitialDraftsAsync(activeFiles
            .Where(f => IsInitialDraft(f) && !recordedFiles.Contains(f.FileID)).ToList(), onDate, periodEnd);

        foreach (var file in activeFiles)
        {
            var fileNumber = file.FileNumber;
            var retireeName = file.Retiree?.FullName ?? "";
            if (recordedFiles.Contains(file.FileID))
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName,
                    Reason = "سُجّل معاش هذا الملف لشهر الكشف في كشف آخر" });
                continue;
            }

            // Suspension prevents payment, never accrual. Use approved data during review.
            var approved = ReadCalculationState(file, out var stateError);
            if (stateError != null)
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber,
                    RetireeFullName = retireeName, Reason = stateError });
                continue;
            }
            if (approved == null)
            {
                // Initial submitted amounts are retained in a draft batch under a hold;
                // first approval of the file is required before accounting approval.
                approved = initialDrafts[file.FileID];
                // This detached draft is not saved as an approved version.
                approved.MarkReviewed();
            }
            var retiree = approved.Retiree;
            retireeName = retiree.FullName;
            var book = approved.Books.Where(b => b.Status == PaymentBookStatus.Active && !b.IsDeleted && b.IssueDate <= onDate && b.ExpiryDate >= onDate)
                .OrderByDescending(b => b.CreatedAt).FirstOrDefault();
            var entitlement = approved.Entitlements.Where(isEffectiveForPeriod)
                .OrderByDescending(e => e.EffectiveDate).ThenByDescending(e => e.EntitlementID).FirstOrDefault();

            // 3. Skip if no active payment book
            if (book == null)
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "لا يوجد دفتر صرف نشط" });
                continue;
            }

            // 4. Skip if no active entitlement
            if (entitlement == null)
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "لا يوجد استحقاق مالي" });
                continue;
            }

            // 5. Get recipients
            var recipients = approved.Recipients.Where(r => r.IsActive && !r.IsDeleted).OrderBy(r => r.RecipientID).ToList();
            if (recipients.Count == 0)
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "لا يوجد مستلم نشط" });
                continue;
            }

            var required = approved.EntitledPersons.Where(e => e.IsActive && !e.IsDeleted && !e.IsDeceased).ToList();
            var fileGuardianLinks = approved.GuardianLinks.Where(g => g.IsActive && !g.IsDeleted)
                .Select(g => (g.GuardianID, g.EntitledID)).ToList();
            var scopeIds = recipients.Select(RecipientScope.EntitledId).ToList();
            string? invalid = null;
            if (recipients.Any(r => RecipientScope.CalendarDate(r.StartDate) > onDate ||
                    (r.EndDate.HasValue && RecipientScope.CalendarDate(r.EndDate.Value) < onDate)))
                invalid = "تاريخ الصرف خارج مدة تعيين أحد المستلمين";
            else if (recipients.Any(r => r.AgentID.HasValue && (r.Agent == null || r.Agent.FileID != file.FileID || !RecipientScope.AgentPayable(r.Agent, onDate))))
                invalid = "يوجد وكيل معطل أو مرفوض أو وكالة غير سارية";
            else if (recipients.Any(r => r.GuardianID.HasValue && (r.Guardian == null || !r.Guardian.IsActive || r.Guardian.FileID != file.FileID || !RecipientScope.Reviewed(r.Guardian.PendingReviewStatus) ||
                    !r.EntitledID.HasValue || !fileGuardianLinks.Any(g => g.GuardianID == r.GuardianID && g.EntitledID == r.EntitledID))))
                invalid = "يوجد وصي غير صالح أو غير مرتبط بالمستحق";
            else if (recipients.Any(r => r.RetireeID.HasValue && (r.RetireeID != file.RetireeID || !retiree.IsAlive)))
                invalid = "المتقاعد غير مؤهل للاستلام المباشر";
            else if (scopeIds.Distinct().Count() != scopeIds.Count || (scopeIds.Contains(null) && recipients.Count != 1))
                invalid = "تعارض في نطاقات مستلمي المعاش";
            else if (scopeIds.All(id => id.HasValue))
            {
                var missing = required.Where(e => !scopeIds.Contains(e.EntitledID)).Select(e => e.FullName).ToList();
                if (missing.Count > 0) invalid = "لم يكتمل تعيين مستلمي الحصص: " + string.Join("، ", missing);
                else if (required.Any(e => !RecipientScope.Reviewed(e.PendingReviewStatus)) ||
                    scopeIds.Any(id => !required.Any(e => e.EntitledID == id)))
                    invalid = "يوجد مستحق موقوف أو متوفى أو قيد المراجعة";
                else if (recipients.Any(r => r.SharePercent is null or <= 0 or > 100) || recipients.Sum(r => r.SharePercent ?? 0) != 100)
                    invalid = "يجب استكمال حصص جميع المستحقين ليكون مجموعها 100٪";
            }
            if (invalid != null)
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = invalid });
                continue;
            }

            // Coverage is complete before distributing any amount for this file.
            decimal totalFileAmount = entitlement.Amount;
            var splitAmounts = new List<decimal>();
            decimal remaining = totalFileAmount;

            // جلب النسب من جميع المستلمين
            var percentages = recipients.Select(r => r.SharePercent).ToList();

            // التحقق: هل جميع النسب null أو صفر؟
            bool hasAnyPercent = scopeIds.All(id => id.HasValue);

            if (!hasAnyPercent)
            {
                // ── لا توجد نسب → تقسيم متساوي (المنطق الحالي) ──
                for (int i = 0; i < recipients.Count; i++)
                {
                    if (i == recipients.Count - 1)
                    {
                        splitAmounts.Add(remaining);
                    }
                    else
                    {
                        var amt = Math.Round(totalFileAmount / recipients.Count, 2);
                        splitAmounts.Add(amt);
                        remaining -= amt;
                    }
                }
            }
            else
            {
                // ── توجد نسب → حساب المبلغ لكل مستلم ──
                decimal totalPct = percentages.Where(p => p.HasValue && p > 0).Sum(p => p!.Value);
                
                for (int i = 0; i < recipients.Count; i++)
                {
                    if (i == recipients.Count - 1)
                    {
                        splitAmounts.Add(remaining);
                    }
                    else
                    {
                        decimal pct = percentages[i] ?? 0;
                        // إذا كانت النسبة أقل من 100 مجموعاً → تعديل تلقائي
                        decimal effectivePct = pct;
                        
                        var amt = Math.Round(totalFileAmount * effectivePct / 100, 2);
                        splitAmounts.Add(amt);
                        remaining -= amt;
                    }
                }
            }

            int idx = 0;
            foreach (var recipient in recipients)
            {
                var item = new SalaryPaymentItem
                {
                    BatchID = batchId,
                    FileID = file.FileID,
                    RetireeID = file.RetireeID,
                    RecipientID = recipient.RecipientID,
                    Amount = splitAmounts[idx],
                    StatusID = TransactionStatusConstants.PendingSend,
                    CreatedByUserID = userId,
                    PaymentBookID = book.PaymentBookID,
                    CreatedAt = runAt
                };
                result.GeneratedItems.Add(item);
                idx++;
            }
        }

        return result;
    }

    private async Task<Dictionary<int, ApprovedPensionSnapshot>> LoadInitialDraftsAsync(
        List<Core.Entities.RetirementFile> files, DateOnly onDate, DateOnly periodEnd)
    {
        if (files.Count == 0) return [];
        var fileIds = files.Select(f => f.FileID).ToHashSet();
        var retireeIds = files.Select(f => f.RetireeID).ToHashSet();
        var recipients = await _unitOfWork.SalaryRecipients.Query().AsNoTracking()
            .Include(r => r.Agent).ThenInclude(a => a!.EntitledPerson)
            .Include(r => r.EntitledPerson).Include(r => r.Guardian).Include(r => r.Retiree)
            .Where(r => r.IsActive && !r.IsDeleted && fileIds.Contains(r.FileID)).ToListAsync();
        var entitledPersons = await _unitOfWork.EntitledPersons.Query().AsNoTracking()
            .Where(e => fileIds.Contains(e.FileID) && !e.IsDeleted && !e.IsDeceased && e.IsActive).ToListAsync();
        var guardianLinks = await _unitOfWork.GuardianEntitledPersons.Query().AsNoTracking()
            .Where(g => g.IsActive && !g.IsDeleted && fileIds.Contains(g.EntitledPerson.FileID))
            .Select(g => new { g.EntitledPerson.FileID, g.GuardianID, g.EntitledID }).ToListAsync();
        var entitlements = await _unitOfWork.FinancialEntitlements.Query().AsNoTracking()
            .Where(e => retireeIds.Contains(e.RetireeID))
            .Where(PensionEntitlementRules.EffectiveDuring(onDate, periodEnd)).ToListAsync();
        var books = await _unitOfWork.PaymentBooks.Query().AsNoTracking()
            .Where(b => b.Status == PaymentBookStatus.Active && !b.IsDeleted && b.IssueDate <= onDate &&
                b.ExpiryDate >= onDate && fileIds.Contains(b.FileID)).ToListAsync();

        var recipientsByFile = recipients.ToLookup(r => r.FileID);
        var entitledByFile = entitledPersons.ToLookup(e => e.FileID);
        var linksByFile = guardianLinks.ToLookup(g => g.FileID);
        var entitlementsByRetiree = entitlements.ToLookup(e => e.RetireeID);
        var booksByFile = books.ToLookup(b => b.FileID);
        return files.ToDictionary(f => f.FileID, f => new ApprovedPensionSnapshot
        {
            Retiree = f.Retiree,
            Recipients = recipientsByFile[f.FileID].ToList(),
            EntitledPersons = entitledByFile[f.FileID].ToList(),
            GuardianLinks = linksByFile[f.FileID].Select(g => new GuardianEntitledPerson
                { GuardianID = g.GuardianID, EntitledID = g.EntitledID, IsActive = true }).ToList(),
            Entitlements = entitlementsByRetiree[f.RetireeID].ToList(),
            Books = booksByFile[f.FileID].ToList()
        });
    }

    private static bool IsInitialDraft(Core.Entities.RetirementFile file) =>
        string.IsNullOrWhiteSpace(file.LastReviewedFileState) &&
        file.StatusID == FileStatusConstants.Pending && file.PreviousStatusID == FileStatusConstants.New &&
        file.ReviewStatus == FileReviewStatus.PendingReview &&
        file.Retiree != null && !file.Retiree.IsDeleted && file.Retiree.RetireeID == file.RetireeID;

    // Source selection belongs to generation; capturing and persisting approval belongs to review.
    // A missing initial version permits a held draft only. An unreadable stored version never
    // permits falling back to live data, even during the first review.
    private static ApprovedPensionSnapshot? ReadCalculationState(Core.Entities.RetirementFile file, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(file.LastReviewedFileState))
        {
            if (IsInitialDraft(file)) return null;

            error = "لا توجد نسخة محفوظة من آخر بيانات معتمدة للملف";
            return null;
        }

        var state = ApprovedPensionSnapshot.Read(file.LastReviewedFileState);
        if (state == null)
        {
            error = "تعذر قراءة آخر بيانات معتمدة للملف أو أن إصدارها غير مدعوم";
            return null;
        }

        if (state.Retiree == null || state.Retiree.IsDeleted || state.Retiree.RetireeID != file.RetireeID ||
            state.Entitlements == null || state.Entitlements.Any(e => e == null || e.RetireeID != file.RetireeID) ||
            state.Books == null || state.Books.Any(b => b == null || b.FileID != file.FileID) ||
            state.Recipients == null || state.Recipients.Any(r => r == null || r.FileID != file.FileID) ||
            state.EntitledPersons == null || state.EntitledPersons.Any(e => e == null || e.FileID != file.FileID) ||
            state.GuardianLinks == null || state.GuardianLinks.Any(g => g == null ||
                !state.EntitledPersons.Any(e => e.EntitledID == g.EntitledID)))
        {
            error = "النسخة المعتمدة غير مكتملة أو لا تطابق الملف والمتقاعد";
            return null;
        }

        return state;
    }
}
