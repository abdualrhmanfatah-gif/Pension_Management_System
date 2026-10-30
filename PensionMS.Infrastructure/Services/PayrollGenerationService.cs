using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// تطبيق قواعد أهلية توليد عناصر الصرف (إيقاف، مستلم، استحقاق، دفتر صرف، منع التكرار).
/// البيانات المرتبطة تُحمّل مسبقاً في الذاكرة لتجنب استعلامات N+1.
/// </summary>
public class PayrollGenerationService : IPayrollGenerationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public PayrollGenerationService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<PayrollGenerationResult> GenerateForBatchAsync(SalaryPaymentBatch batch, DateTime runAt, int userId)
    {
        var result = new PayrollGenerationResult();
        var batchId = batch.BatchID;

        var activeFiles = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Where(f => f.StatusID == FileStatusConstants.Active && !f.IsDeleted)
            .ToListAsync();

        if (activeFiles.Count == 0)
        {
            await NotifyCompletionAsync(batch, 0, 0, userId);
            return result;
        }

        var fileIds = activeFiles.Select(f => f.FileID).ToHashSet();
        var retireeIds = activeFiles.Select(f => f.RetireeID).ToHashSet();

        // Preload: FileIDs that have an active suspension
        var fileIdsWithSuspensionList = await _unitOfWork.PensionSuspensions.Query()
            .AsNoTracking()
            .Where(s => s.Status == SuspensionStatusConstants.Active && !s.IsDeleted && fileIds.Contains(s.FileID))
            .Select(s => s.FileID)
            .Distinct()
            .ToListAsync();
        var fileIdsWithSuspension = fileIdsWithSuspensionList.ToHashSet();

        // Preload: active recipients per file (multiple allowed)
        var recipientsByFile = await _unitOfWork.SalaryRecipients.Query()
            .AsNoTracking()
            .Where(r => r.IsActive && r.PendingReviewStatus != PendingReviewStatus.Pending && r.PendingReviewStatus != PendingReviewStatus.Rejected && fileIds.Contains(r.FileID))
            .ToListAsync();
        var recipientsLookup = recipientsByFile.ToLookup(r => r.FileID);

        // Preload: active financial entitlements (most recent per retiree)
        var allEntitlements = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking()
            .Where(e => !e.IsDeleted && retireeIds.Contains(e.RetireeID) && e.EffectiveDate <= runAt)
            .OrderByDescending(e => e.EffectiveDate)
            .ToListAsync();
        var entitlementLookup = allEntitlements
            .GroupBy(e => e.RetireeID)
            .ToDictionary(g => g.Key, g => g.First());

        // Preload: active payment books
        var activeBooks = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Where(b => b.Status == PaymentBookStatus.Active && !b.IsDeleted && b.PendingReviewStatus != PendingReviewStatusConstants.PendingReview && b.PendingReviewStatus != PendingReviewStatusConstants.Rejected && fileIds.Contains(b.FileID))
            .ToListAsync();
        var booksLookup = activeBooks.ToDictionary(b => b.FileID);

        // Preload: already generated items for this month (to prevent duplicates)
        var existingFileIds = await _unitOfWork.SalaryPaymentItems.Query()
            .AsNoTracking()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .Select(i => i.FileID)
            .Distinct()
            .ToListAsync();
        var processedFileIds = existingFileIds.ToHashSet();

        decimal totalAmount = 0;

        foreach (var file in activeFiles)
        {
            var fileNumber = file.FileNumber;
            var retireeName = file.Retiree?.FullName ?? "";

            // 1. Skip if already processed in this batch
            if (processedFileIds.Contains(file.FileID))
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "تم التوليد مسبقاً في هذه الدفعة" });
                continue;
            }

            // 2. Skip if suspended
            if (fileIdsWithSuspension.Contains(file.FileID))
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "الملف موقوف" });
                continue;
            }

            // 3. Skip if no active payment book
            if (!booksLookup.ContainsKey(file.FileID))
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "لا يوجد دفتر صرف نشط" });
                continue;
            }

            // 4. Skip if no active entitlement
            if (!entitlementLookup.TryGetValue(file.RetireeID, out var entitlement))
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "لا يوجد استحقاق مالي" });
                continue;
            }

            // 5. Get recipients
            var recipients = recipientsLookup[file.FileID].ToList();
            if (recipients.Count == 0)
            {
                result.SkippedItems.Add(new SkippedItemDto { FileNumber = fileNumber, RetireeFullName = retireeName, Reason = "لا يوجد مستلم نشط" });
                continue;
            }

            // 6. Split amount equally — ensure sum(splitAmounts) == totalFileAmount
            decimal totalFileAmount = entitlement.Amount;
            var splitAmounts = new List<decimal>();
            decimal remaining = totalFileAmount;
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
                    CreatedAt = runAt
                };
                await _unitOfWork.SalaryPaymentItems.AddAsync(item);
                result.GeneratedItems.Add(item);
                totalAmount += splitAmounts[idx];
                idx++;
            }
        }

        await NotifyCompletionAsync(batch, result.GeneratedItems.Count, totalAmount, userId);

        return result;
    }

    private async Task NotifyCompletionAsync(SalaryPaymentBatch batch, int count, decimal amount, int userId)
    {
        await _notificationService.NotifyRoleAsync("Manager", new CreateNotificationDto
        {
            UserID = 0,
            Title = "اكتمال توليد الرواتب",
            Message = $"تم الانتهاء من توليد كشف الرواتب لشهر {batch.BatchMonth}. تم معالجة {count} مستفيد بإجمالي مبلغ {amount:N2} ريال.",
            Type = NotificationConstants.Types.Success,
            Category = NotificationConstants.Categories.Financial,
            Priority = NotificationConstants.Priorities.High,
            ActorID = userId,
            RelatedRecordID = batch.BatchID
        });
    }
}
