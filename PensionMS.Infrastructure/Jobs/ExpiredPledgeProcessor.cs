using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Jobs;

// A named system use case. It does not interpret a missing HTTP identity as general access.
internal sealed class ExpiredPledgeProcessor(IUnitOfWork _unitOfWork, INotificationService _notificationService) : IExpiredPledgeProcessor
{
    public async Task ProcessExpiredPledgesAsync()
    {
        var now = DateTime.UtcNow;

        // جلب جميع التعهدات المنتهية المدة على ملفات نشطة
        var expiredPledges = await _unitOfWork.DocumentPledges.Query()
            .Include(p => p.RetirementFile)
            .Where(p => !p.IsDeleted
                     && p.Status == GeneralStatusConstants.Active
                     && p.Deadline < now
                     && (p.RetirementFile.StatusID == FileStatusConstants.Active || p.RetirementFile.StatusID == FileStatusConstants.Pending)
                     && !p.RetirementFile.IsDeleted)
            .ToListAsync();

        if (!expiredPledges.Any()) return;

        // ملفات فريدة للمعالجة (لتجنب التكرار إذا كان للملف أكثر من تعهد)
        var fileIds = expiredPledges.Select(p => p.FileID).Distinct().ToList();

        foreach (var fileId in fileIds)
        {
            var file = expiredPledges.First(p => p.FileID == fileId).RetirementFile;

            // تحديث حالة التعهدات المنتهية
            var pledgesForFile = expiredPledges.Where(p => p.FileID == fileId).ToList();
            foreach (var pledge in pledgesForFile)
            {
                pledge.Status = PledgeStatusConstants.Expired;
                pledge.UpdatedAt = now;
                _unitOfWork.DocumentPledges.Update(pledge);
            }

            // إنشاء إيقاف تلقائي
            var suspension = new PensionSuspension
            {
                FileID = fileId,
                SuspensionDate = now,
                SuspensionReason = "انتهاء مدة تعهد الوثائق المطلوبة",
                SuspensionSource = "تلقائي",
                Status = SuspensionStatusConstants.Active,
                CreatedByUserID = LookupConstants.SystemUserId, // System user
                CreatedAt = now
            };
            await _unitOfWork.PensionSuspensions.AddAsync(suspension);

        }

        await _unitOfWork.SaveChangesAsync();

        foreach (var fileId in fileIds)
        {
            var expiredFile = expiredPledges.First(p => p.FileID == fileId).RetirementFile;
            var notifyUserId = expiredFile.AssignedEmployeeID;

            if (notifyUserId == null || notifyUserId == 0)
            {
                var creatorId = await _unitOfWork.AuditLogs.Query()
                    .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == fileId && a.Action == "Create")
                    .Select(a => a.UserID)
                    .FirstOrDefaultAsync();
                notifyUserId = creatorId;
            }

            if (notifyUserId.HasValue && notifyUserId.Value > 0)
            {
                await _notificationService.SendNotificationAsync(new CreateNotificationDto
                {
                    UserID = notifyUserId.Value,
                    Title = "إيقاف المعاش - انتهاء تعهد",
                    Message = $"تم إيقاف معاش الملف رقم ({expiredFile.FileNumber}) تلقائياً لانتهاء مدة تعهد تقديم الوثائق.",
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.High,
                    ActorID = LookupConstants.SystemUserId,
                    RelatedTableID = SystemTableConstants.RetirementFiles,
                    RelatedRecordID = fileId
                });
            }
        }
    }

}
