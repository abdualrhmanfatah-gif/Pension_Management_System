using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class FinancialEntitlementService : IFinancialEntitlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IComplianceEngine _complianceEngine;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FinancialEntitlementService> _logger;

    public FinancialEntitlementService(IUnitOfWork unitOfWork, IComplianceEngine complianceEngine, INotificationService notificationService, ILogger<FinancialEntitlementService> logger)
    {
        _unitOfWork = unitOfWork;
        _complianceEngine = complianceEngine;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<FinancialEntitlementDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var entitlements = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking()
            .Where(e => e.RetireeID == retireeId && !e.IsDeleted)
            .Include(e => e.Retiree)
            .OrderByDescending(e => e.EffectiveDate)
            .Select(e => MapToDto(e))
            .ToListAsync();

        // جلب المعاش الأساسي قبل التسوية من كشف الإحالة
        var retiree = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(r => r.ApprovalItem)
            .FirstOrDefaultAsync(r => r.RetireeID == retireeId && !r.IsDeleted && r.PendingReviewStatus != PendingReviewStatus.Pending && r.PendingReviewStatus != PendingReviewStatus.Rejected);

        if (retiree?.ApprovalItem?.Salary != null)
        {
            var firstEntitlement = entitlements.OrderBy(e => e.EffectiveDate).FirstOrDefault();
            DateTime? endDate = firstEntitlement != null 
                ? firstEntitlement.EffectiveDate.AddDays(-1) 
                : null;

            entitlements.Add(new FinancialEntitlementDto
            {
                EntitlementID = -1, // معرف وهمي
                RetireeID = retireeId,
                RetireeFullName = retiree.FullName,
                Amount = retiree.ApprovalItem.Salary.Value,
                EffectiveDate = retiree.ApprovalItem.RetirementDate?.ToDateTime(TimeOnly.MinValue) ?? PensionMS.Core.Time.YemenTime.LocalTime(new DateTimeOffset(DateTime.SpecifyKind(retiree.ApprovalItem.CreatedAt, DateTimeKind.Utc))).Date,
                EndDate = endDate,
                Notes = "المعاش الأساسي (من كشف الإحالة)",
                CreatedAt = retiree.ApprovalItem.CreatedAt
            });

            // إعادة ترتيب القائمة بحيث تكون الأحدث أولاً
            entitlements = entitlements.OrderByDescending(e => e.EffectiveDate).ToList();
        }

        return ApiResponse<List<FinancialEntitlementDto>>.Success(entitlements);
    }

    public async Task<ApiResponse<FinancialEntitlementDto?>> GetActiveByRetireeIdAsync(int retireeId)
    {
        var today = PensionMS.Core.Time.YemenTime.TodayDateTime;
        var entitlement = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking()
            .Where(e => e.RetireeID == retireeId
                     && !e.IsDeleted
                     && e.EffectiveDate.Date <= today
                     && (e.EndDate == null || e.EndDate.Value.Date >= today))
            .Include(e => e.Retiree)
            .OrderByDescending(e => e.EffectiveDate)
            .FirstOrDefaultAsync();

        if (entitlement == null)
        {
            // الاعتماد على المعاش الأساسي قبل التسوية إذا لم توجد استحقاقات مسجلة
            var retiree = await _unitOfWork.Retirees.Query()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(r => r.ApprovalItem)
                .FirstOrDefaultAsync(r => r.RetireeID == retireeId && !r.IsDeleted && r.PendingReviewStatus != PendingReviewStatus.Pending && r.PendingReviewStatus != PendingReviewStatus.Rejected);

            if (retiree?.ApprovalItem?.Salary != null)
            {
                return ApiResponse<FinancialEntitlementDto?>.Success(new FinancialEntitlementDto
                {
                    EntitlementID = -1,
                    RetireeID = retireeId,
                    RetireeFullName = retiree.FullName,
                    Amount = retiree.ApprovalItem.Salary.Value,
                    EffectiveDate = retiree.ApprovalItem.RetirementDate?.ToDateTime(TimeOnly.MinValue) ?? PensionMS.Core.Time.YemenTime.LocalTime(new DateTimeOffset(DateTime.SpecifyKind(retiree.ApprovalItem.CreatedAt, DateTimeKind.Utc))).Date,
                    EndDate = null,
                    Notes = "المعاش الأساسي (من كشف الإحالة)",
                    CreatedAt = retiree.ApprovalItem.CreatedAt
                });
            }
        }

        return ApiResponse<FinancialEntitlementDto?>.Success(
            entitlement != null ? MapToDto(entitlement) : null);
    }

    public async Task<ApiResponse<FinancialEntitlementDto>> GetByIdAsync(int id)
    {
        var entitlement = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking()
            .Include(e => e.Retiree)
            .FirstOrDefaultAsync(e => e.EntitlementID == id && !e.IsDeleted);

        if (entitlement == null)
            return ApiResponse<FinancialEntitlementDto>.Fail("الاستحقاق المالي غير موجود");

        return ApiResponse<FinancialEntitlementDto>.Success(MapToDto(entitlement));
    }

    public async Task<ApiResponse<FinancialEntitlementDto>> CreateAsync(
        CreateFinancialEntitlementDto dto, int userId)
    {
        // ── 1. Validate Retiree ──
        var retiree = await _unitOfWork.Retirees.Query()
            .IgnoreQueryFilters()
            .Where(r => !r.IsDeleted && r.PendingReviewStatus != PendingReviewStatus.Pending && r.PendingReviewStatus != PendingReviewStatus.Rejected)
            .FirstOrDefaultAsync(r => r.RetireeID == dto.RetireeID);
        if (retiree == null)
            return ApiResponse<FinancialEntitlementDto>.Fail("المتقاعد غير موجود");

        // ── 2. Validate Amount ──
        if (dto.Amount <= 0)
            return ApiResponse<FinancialEntitlementDto>.Fail("المبلغ يجب أن يكون أكبر من صفر");
        if (dto.Amount > 10_000_000)
            return ApiResponse<FinancialEntitlementDto>.Fail("المبلغ يتجاوز الحد الأقصى المسموح");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ── 3. End current active entitlement ──
            var now = DateTime.UtcNow;
            var today = PensionMS.Core.Time.YemenTime.TodayDateTime;
            var activeEntitlement = await _unitOfWork.FinancialEntitlements.Query()
                .Where(e => e.RetireeID == dto.RetireeID
                         && !e.IsDeleted
                         && (e.EndDate == null || e.EndDate.Value.Date >= today))
                .FirstOrDefaultAsync();

            if (activeEntitlement != null)
            {
                // إنهاء الاستحقاق السابق — يوم واحد قبل تاريخ السريان الجديد
                activeEntitlement.EndDate = dto.EffectiveDate.AddDays(-1);
                activeEntitlement.UpdatedAt = now;
                _unitOfWork.FinancialEntitlements.Update(activeEntitlement);
            }

            // ── 4. Create new entitlement ──
            var entitlement = new FinancialEntitlement
            {
                RetireeID = dto.RetireeID,
                Amount = dto.Amount,
                EffectiveDate = dto.EffectiveDate,
                Notes = dto.Notes,
                CreatedAt = now
            };

            await _unitOfWork.FinancialEntitlements.AddAsync(entitlement);
            await _unitOfWork.SaveChangesAsync();

            // Refresh compliance for all linked files
            var fileIds = await _unitOfWork.RetirementFiles.Query()
                .Where(f => f.RetireeID == dto.RetireeID && !f.IsDeleted)
                .Select(f => f.FileID)
                .ToListAsync();
            foreach (var fileId in fileIds)
                await _complianceEngine.RefreshComplianceCacheAsync(fileId);

            await _unitOfWork.CommitAsync();

            foreach (var fileId in fileIds)
            {
                var notifyUserId = await GetNotifyUserIdForFileAsync(fileId);
                if (notifyUserId.HasValue && notifyUserId.Value > 0)
                {
                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserID = notifyUserId.Value,
                        Title = "استحقاق مالي جديد",
                        Message = $"تم إنشاء استحقاق مالي جديد للمتقاعد ({retiree.FullName}) بقيمة {dto.Amount:N0} ابتداءً من {dto.EffectiveDate:yyyy/MM/dd}.",
                        Type = NotificationConstants.Types.Info,
                        Category = NotificationConstants.Categories.Financial,
                        Priority = NotificationConstants.Priorities.Normal,
                        ActorID = userId,
                        RelatedTableID = SystemTableConstants.RetirementFiles,
                        RelatedRecordID = fileId
                    });
                }
            }

            // Reload with Retiree for DTO mapping
            var created = await _unitOfWork.FinancialEntitlements.Query()
                .AsNoTracking()
                .Include(e => e.Retiree)
                .FirstAsync(e => e.EntitlementID == entitlement.EntitlementID);

            return ApiResponse<FinancialEntitlementDto>.Success(
                MapToDto(created), "تم إنشاء الاستحقاق المالي بنجاح");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ أثناء إنشاء استحقاق مالي للمتقاعد {RetireeID}", dto.RetireeID);
            await _unitOfWork.RollbackAsync();
            return ApiResponse<FinancialEntitlementDto>.Fail("حدث خطأ أثناء حفظ الاستحقاق المالي. يرجى المحاولة لاحقاً");
        }
    }

    public async Task<ApiResponse<bool>> EndAsync(int id, DateTime endDate, int userId)
    {
        var entitlement = await _unitOfWork.FinancialEntitlements.GetByIdAsync(id);
        if (entitlement == null || entitlement.IsDeleted)
            return ApiResponse<bool>.Fail("الاستحقاق المالي غير موجود");

        if (entitlement.EndDate != null && entitlement.EndDate.Value.Date < PensionMS.Core.Time.YemenTime.TodayDateTime)
            return ApiResponse<bool>.Fail("هذا الاستحقاق منتهي مسبقاً");

        if (endDate < entitlement.EffectiveDate)
            return ApiResponse<bool>.Fail("تاريخ الانتهاء لا يمكن أن يسبق تاريخ السريان");

        entitlement.EndDate = endDate;
        entitlement.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.FinancialEntitlements.Update(entitlement);
        await _unitOfWork.SaveChangesAsync();

        var endFileIds = await _unitOfWork.RetirementFiles.Query()
            .Where(f => f.RetireeID == entitlement.RetireeID && !f.IsDeleted)
            .Select(f => f.FileID)
            .ToListAsync();

        foreach (var fileId in endFileIds)
        {
            var notifyUserId = await GetNotifyUserIdForFileAsync(fileId);
            if (notifyUserId.HasValue && notifyUserId.Value > 0)
            {
                await _notificationService.SendNotificationAsync(new CreateNotificationDto
                {
                    UserID = notifyUserId.Value,
                    Title = "إنهاء استحقاق مالي",
                    Message = $"تم إنهاء الاستحقاق المالي للمتقاعد رقم ({entitlement.RetireeID}) بتاريخ {endDate:yyyy/MM/dd}.",
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Financial,
                    Priority = NotificationConstants.Priorities.Normal,
                    ActorID = userId,
                    RelatedTableID = SystemTableConstants.RetirementFiles,
                    RelatedRecordID = fileId
                });
            }
        }

        return ApiResponse<bool>.Success(true, "تم إنهاء الاستحقاق المالي");
    }

    public async Task<ApiResponse<List<FinancialEntitlementDto>>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ApiResponse<List<FinancialEntitlementDto>>.Success(new List<FinancialEntitlementDto>());

        var s = query.Trim();

        var retireeIds = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Where(r => !r.IsDeleted
                && (r.FullName.Contains(s)
                    || (r.MilitaryNumber != null && r.MilitaryNumber.Contains(s))
                    || (r.Phone != null && r.Phone.Contains(s))))
            .Select(r => r.RetireeID)
            .ToListAsync();

        var fileNumbers = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.FileNumber.Contains(s))
            .Select(f => new { f.FileNumber, f.RetireeID })
            .ToListAsync();

        var agentRetireeIds = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.FullName.Contains(s))
            .Join(_unitOfWork.RetirementFiles.Query().Where(f => !f.IsDeleted),
                a => a.FileID, f => f.FileID, (a, f) => f.RetireeID)
            .Distinct()
            .ToListAsync();

        var allRetireeIds = retireeIds
            .Union(fileNumbers.Select(f => f.RetireeID))
            .Union(agentRetireeIds)
            .Distinct()
            .ToList();

        if (!allRetireeIds.Any())
            return ApiResponse<List<FinancialEntitlementDto>>.Success(new List<FinancialEntitlementDto>());

        var entitlements = await _unitOfWork.FinancialEntitlements.Query()
            .AsNoTracking()
            .Where(e => allRetireeIds.Contains(e.RetireeID) && !e.IsDeleted)
            .Include(e => e.Retiree)
            .OrderByDescending(e => e.EffectiveDate)
            .ToListAsync();

        var fileDict = fileNumbers.ToDictionary(f => f.RetireeID, f => f.FileNumber);

        var agentDict = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && allRetireeIds.Contains(a.RetirementFile.RetireeID))
            .Join(_unitOfWork.RetirementFiles.Query().Where(f => !f.IsDeleted),
                a => a.FileID, f => f.FileID, (a, f) => new { f.RetireeID, a.FullName, a.IsActive })
            .GroupBy(x => x.RetireeID)
            .ToDictionaryAsync(g => g.Key, g => g.OrderByDescending(x => x.IsActive).First().FullName);

        var dtos = entitlements.Select(e =>
        {
            var dto = MapToDto(e);

            fileDict.TryGetValue(e.RetireeID, out var fileNum);
            dto.FileNumber = fileNum;

            if (e.Retiree != null)
            {
                dto.MilitaryNumber = e.Retiree.MilitaryNumber;
                dto.Phone = e.Retiree.Phone;
            }

            if (e.Retiree != null && fileNum != null)
            {
                agentDict.TryGetValue(e.RetireeID, out var agentName);
                dto.AgentName = agentName;
            }

            return dto;
        }).ToList();

        return ApiResponse<List<FinancialEntitlementDto>>.Success(dtos);
    }

    private async Task<int?> GetNotifyUserIdForFileAsync(int fileId)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileID == fileId && !f.IsDeleted);
        if (file == null) return null;

        if (file.AssignedEmployeeID.HasValue && file.AssignedEmployeeID.Value > 0)
            return file.AssignedEmployeeID.Value;

        var creatorId = await _unitOfWork.AuditLogs.Query()
            .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == fileId && a.Action == "Create")
            .Select(a => a.UserID)
            .FirstOrDefaultAsync();

        return creatorId;
    }

    private static FinancialEntitlementDto MapToDto(FinancialEntitlement e) => new()
    {
        EntitlementID = e.EntitlementID,
        RetireeID = e.RetireeID,
        RetireeFullName = e.Retiree?.FullName,
        Amount = e.Amount,
        EffectiveDate = e.EffectiveDate,
        EndDate = e.EndDate,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
