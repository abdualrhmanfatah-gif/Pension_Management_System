using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Helpers;

namespace PensionMS.Infrastructure.Services.Pension.Entitlements;

public class FinancialEntitlementService : IFinancialEntitlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly INotificationService _notificationService;
    private readonly IImmutableAuditService _auditService;
    private readonly ILogger<FinancialEntitlementService> _logger;

    public FinancialEntitlementService(IUnitOfWork unitOfWork, IMissingDocumentService missingDocumentService, INotificationService notificationService, IImmutableAuditService auditService, ILogger<FinancialEntitlementService> logger, IAccessiblePensionRepository access)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _missingDocumentService = missingDocumentService;
        _notificationService = notificationService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<FinancialEntitlementDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var entitlements = await queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking()
            .Where(e => e.RetireeID == retireeId && !e.IsDeleted)
            .Include(e => e.Retiree)
            .OrderByDescending(e => e.EffectiveDate)
            .Select(e => MapToDto(e))
            .ToListAsync();

        // جلب المعاش الأساسي قبل التسوية من كشف الإحالة
        var retiree = await queries.Query(_unitOfWork.Retirees)
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(r => r.ApprovalItem)
            .FirstOrDefaultAsync(r => r.RetireeID == retireeId && !r.IsDeleted && r.ApprovalItem != null && !r.ApprovalItem.IsDeleted && r.ApprovalItem.IsProcessed);

        if (entitlements.Count == 0 && retiree?.ApprovalItem?.Salary != null)
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
        var queries = await _access.ForCurrentOperationAsync();
        var today = PensionMS.Core.Time.YemenTime.Today;
        var entitlement = await queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking()
            .Where(e => e.RetireeID == retireeId)
            .Where(PensionEntitlementRules.EffectiveOn(today))
            .Include(e => e.Retiree)
            .OrderByDescending(e => e.EffectiveDate)
            .FirstOrDefaultAsync();

        if (entitlement == null && !await queries.Query(_unitOfWork.FinancialEntitlements)
            .AnyAsync(e => e.RetireeID == retireeId && !e.IsDeleted))
        {
            // الاعتماد على المعاش الأساسي قبل التسوية إذا لم توجد استحقاقات مسجلة
            var retiree = await queries.Query(_unitOfWork.Retirees)
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(r => r.ApprovalItem)
                .FirstOrDefaultAsync(r => r.RetireeID == retireeId && !r.IsDeleted && r.ApprovalItem != null && !r.ApprovalItem.IsDeleted && r.ApprovalItem.IsProcessed);

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
        var queries = await _access.ForCurrentOperationAsync();
        var entitlement = await queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking()
            .Include(e => e.Retiree)
            .FirstOrDefaultAsync(e => e.EntitlementID == id && !e.IsDeleted);

        if (entitlement == null)
            return ApiResponse<FinancialEntitlementDto>.Fail("الاستحقاق المالي غير موجود");

        return ApiResponse<FinancialEntitlementDto>.Success(MapToDto(entitlement));
    }

    public Task<ApiResponse<FinancialEntitlementDto>> CreateAsync(CreateFinancialEntitlementDto dto, int userId) =>
        CreateCoreAsync(dto, userId);

    public async Task<ApiResponse<FinancialEntitlementDto>> CreateForFileAsync(int fileId, int userId)
    {
        if (!_unitOfWork.HasActiveTransaction)
            throw new InvalidOperationException("تجهيز استحقاقات فتح الملف يتطلب معاملة نشطة");
        var queries = await _access.ForCurrentOperationAsync();
        var file = await queries.Query(_unitOfWork.RetirementFiles)
            .FirstOrDefaultAsync(f => f.FileID == fileId && !f.IsDeleted);
        if (file == null) return ApiResponse<FinancialEntitlementDto>.Fail("الملف غير موجود ضمن نطاق الصلاحية");
        if (file.StatusID != FileStatusConstants.New || file.ReviewStatus != FileReviewStatus.None)
            return ApiResponse<FinancialEntitlementDto>.Fail("تهيئة استحقاقات فتح الملف متاحة للملف الجديد فقط");
        return await CreateCoreAsync(new CreateFinancialEntitlementDto { RetireeID = file.RetireeID }, userId, fileId);
    }

    private async Task<ApiResponse<FinancialEntitlementDto>> CreateCoreAsync(CreateFinancialEntitlementDto dto, int userId, int? openingFileId = null)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var retiree = await queries.Query(_unitOfWork.Retirees).IgnoreQueryFilters().Include(r => r.ApprovalItem)
            .FirstOrDefaultAsync(r => r.RetireeID == dto.RetireeID && !r.IsDeleted);
        if (retiree == null) return ApiResponse<FinancialEntitlementDto>.Fail("المتقاعد غير موجود");
        if (retiree.ApprovalItem == null || retiree.ApprovalItem.IsDeleted || !retiree.ApprovalItem.IsProcessed)
            return ApiResponse<FinancialEntitlementDto>.Fail("المتقاعد لم تُستكمل معالجته في كشف الإحالة");
        if (!await queries.CanEditRetireeAsync(dto.RetireeID))
            throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.LockRetireeEntitlementsAsync(dto.RetireeID);
            var approval = retiree.ApprovalItem;
            if (!approval.Salary.HasValue || approval.Salary <= 0 || approval.Salary > 10_000_000)
                return await RejectAsync("معاش كشف الإحالة يجب أن يكون أكبر من صفر وألا يتجاوز 10 ملايين");
            if (!approval.RetirementDate.HasValue || approval.RetirementDate.Value == default)
                return await RejectAsync("تاريخ التقاعد في كشف الإحالة مطلوب لإنشاء الاستحقاق الأساسي");
            var baselineDate = approval.RetirementDate.Value.ToDateTime(TimeOnly.MinValue);
            var plan = new List<EntitlementSource>
            {
                new(approval.Salary.Value, baselineDate, $"المعاش الأساسي — كشف الإحالة رقم {approval.ApprovalItemID}", null, approval.RankID)
            };
            foreach (var settlement in await GetSettlementsAsync(retiree))
            {
                if (settlement.Decision == null || settlement.Decision.DecisionDate.Date == DateTime.MinValue.Date)
                    return await RejectAsync($"تاريخ قرار التسوية للقيد {settlement.SettlementItemID} مطلوب");
                if (settlement.ProcessedRank.BaseSalary <= 0 || settlement.ProcessedRank.BaseSalary > 10_000_000)
                    return await RejectAsync($"معاش الرتبة المعالجة للقيد {settlement.SettlementItemID} غير صالح");
                plan.Add(new(settlement.ProcessedRank.BaseSalary, settlement.Decision.DecisionDate.Date,
                    $"تسوية رتبة — قرار رقم {settlement.Decision.DecisionNumber}، قيد {settlement.SettlementItemID}", settlement.SettlementItemID, settlement.ProcessedRankID));
            }
            plan = plan.OrderBy(p => p.EffectiveDate).ToList();
            if (plan[0].EffectiveDate < baselineDate || plan.Where(p => p.SettlementItemID.HasValue)
                .GroupBy(p => p.EffectiveDate).Any(g => g.Count() > 1))
                return await RejectAsync("تواريخ التسويات يجب أن تلي تاريخ التقاعد وألا تتكرر في اليوم نفسه");
            DateTime? openingDate = null;
            if (openingFileId.HasValue)
            {
                var file = await queries.Query(_unitOfWork.RetirementFiles)
                    .FirstOrDefaultAsync(f => f.FileID == openingFileId.Value &&
                        f.RetireeID == retiree.RetireeID && !f.IsDeleted);
                if (file == null) return await RejectAsync("الملف غير موجود ضمن نطاق الصلاحية");
                if (file.FileOpenDate == default) return await RejectAsync("تاريخ فتح الملف مطلوب");
                // تاريخ الفتح يحدد المعاش والرتبة المعادين للملف فقط؛ الفترات تتبع تواريخ مصادرها.
                openingDate = PensionMS.Core.Time.YemenTime.LocalDate(new DateTimeOffset(
                    DateTime.SpecifyKind(file.FileOpenDate, DateTimeKind.Utc))).ToDateTime(TimeOnly.MinValue);
                if (openingDate < baselineDate) return await RejectAsync("تاريخ فتح الملف لا يمكن أن يسبق تاريخ التقاعد في كشف الإحالة");
            }
            if (plan.Select(p => p.EffectiveDate).Distinct().Count() != plan.Count)
                return await RejectAsync("تواريخ مصادر الاستحقاق متعارضة في اليوم نفسه");
            var existing = await queries.Query(_unitOfWork.FinancialEntitlements)
                .Where(e => e.RetireeID == retiree.RetireeID && !e.IsDeleted).ToListAsync();
            if (existing.Any(e => e.EndDate.HasValue && e.EndDate.Value.Date < e.EffectiveDate.Date))
                return await RejectAsync("توجد فترة استحقاق غير صحيحة؛ راجع السجل المالي");
            if (existing.GroupBy(e => new { e.Amount, Day = e.EffectiveDate.Date }).Any(g => g.Count() > 1))
                return await RejectAsync("توجد استحقاقات مكررة؛ راجع السجل المالي");
            var rows = new List<FinancialEntitlement>();
            foreach (var source in plan)
            {
                var matches = existing.Where(e => e.Amount == source.Amount && e.EffectiveDate.Date == source.EffectiveDate).ToList();
                if (matches.Count > 1) return await RejectAsync("توجد استحقاقات مكررة لنفس المبلغ والتاريخ؛ راجع السجل المالي");
                rows.Add(matches.SingleOrDefault() ?? new FinancialEntitlement
                {
                    RetireeID = retiree.RetireeID, Amount = source.Amount, EffectiveDate = source.EffectiveDate,
                    Notes = source.Notes, Retiree = retiree
                });
            }
            if (existing.Any(e => !rows.Contains(e) &&
                (!e.EndDate.HasValue || e.EndDate.Value.Date >= baselineDate)))
                return await RejectAsync("توجد استحقاقات تتعارض مع كشف الإحالة أو التسويات؛ راجع السجل المالي قبل المتابعة");

            // تحقق من الخطة كاملة قبل تعديل أي سجل. لا تعاد إتاحة فترة أُغلقت سابقاً.
            var oldValues = System.Text.Json.JsonSerializer.Serialize(existing.Select(e => new
                { e.EntitlementID, e.Amount, e.EffectiveDate, e.EndDate }));
            var changed = false;
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (i + 1 < rows.Count)
                {
                    var endDate = rows[i + 1].EffectiveDate.Date.AddDays(-1);
                    if (!row.EndDate.HasValue || row.EndDate.Value.Date > endDate)
                    {
                        row.EndDate = endDate;
                        row.UpdatedAt = DateTime.UtcNow;
                        changed = true;
                    }
                }
                if (!existing.Contains(row))
                {
                    await _unitOfWork.FinancialEntitlements.AddAsync(row);
                    changed = true;
                }
            }
            var selectedIndex = openingDate.HasValue
                ? plan.FindLastIndex(p => p.EffectiveDate <= openingDate.Value)
                : rows.Count - 1;
            var latest = rows[selectedIndex];
            latest.Retiree = retiree;
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogAsync(userId, userId.ToString(), "SynchronizeFinancialEntitlements",
                    "FinancialEntitlement", latest.EntitlementID, oldValues,
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        ApprovalItemID = approval.ApprovalItemID, Sources = plan,
                        Entitlements = rows.Select(e => new { e.EntitlementID, e.Amount, e.EffectiveDate, e.EndDate })
                    }));
                var fileIds = await queries.Query(_unitOfWork.RetirementFiles)
                    .Where(f => f.RetireeID == retiree.RetireeID && !f.IsDeleted).Select(f => f.FileID).ToListAsync();
                foreach (var fileId in fileIds)
                {
                    var notifyUserId = await GetNotifyUserIdForFileAsync(fileId);
                    if (notifyUserId is > 0)
                        await _notificationService.EnqueueAsync(new CreateNotificationDto
                        {
                            UserID = notifyUserId.Value, Title = "تحديث الاستحقاقات المالية",
                            Message = $"تم تحديث استحقاقات المتقاعد ({retiree.FullName}) من كشف الإحالة والتسويات. آخر معاش: {latest.Amount:N0} من {latest.EffectiveDate:yyyy/MM/dd}.",
                            Type = NotificationConstants.Types.Info, Category = NotificationConstants.Categories.Financial,
                            Priority = NotificationConstants.Priorities.Normal, ActorID = userId,
                            RelatedTableID = SystemTableConstants.RetirementFiles, RelatedRecordID = fileId
                        });
                }
                await _unitOfWork.SaveChangesAsync();
                if (ownsTransaction) await _unitOfWork.CommitAsync();
                if (ownsTransaction)
                    foreach (var fileId in fileIds)
                        try { await _missingDocumentService.RefreshComplianceCacheAsync(fileId); }
                        catch (Exception ex) { _logger.LogWarning(ex, "تعذر تحديث مراجعة الملف {FileID}", fileId); }
            }
            else if (ownsTransaction) await _unitOfWork.CommitAsync();
            var result = MapToDto(latest);
            result.RankID = plan[selectedIndex].RankID;
            return ApiResponse<FinancialEntitlementDto>.Success(result, changed
                ? "تم إعداد الاستحقاقات المالية من المصادر المعتمدة" : "الاستحقاقات مطابقة للمصادر؛ لم يتم إنشاء نسخ أخرى");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في إعداد استحقاقات المتقاعد {RetireeID}", dto.RetireeID);
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
            return ApiResponse<FinancialEntitlementDto>.Fail("تعذر حفظ الاستحقاقات المالية. لم يتم تأكيد العملية");
        }
        async Task<ApiResponse<FinancialEntitlementDto>> RejectAsync(string message)
        {
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
            return ApiResponse<FinancialEntitlementDto>.Fail(message);
        }
    }

    private sealed record EntitlementSource(decimal Amount, DateTime EffectiveDate, string Notes, int? SettlementItemID, int? RankID);

    private async Task<List<SettlementItem>> GetSettlementsAsync(Retiree retiree)
    {
        var militaryNumber = StringHelpers.NormalizeDigits(retiree.MilitaryNumber);
        return await _unitOfWork.SettlementItems.Query().AsNoTracking()
            .Include(s => s.ProcessedRank).Include(s => s.Decision)
            .Where(s => !s.IsDeleted && s.ProcessedRank != null &&
                (s.Decision == null || !s.Decision.IsDeleted) && (s.Batch == null || !s.Batch.IsDeleted) &&
                (s.RetireeID == retiree.RetireeID ||
                 !s.RetireeID.HasValue && (s.ApprovalItemID == retiree.ApprovalItemID ||
                 !s.ApprovalItemID.HasValue && !string.IsNullOrEmpty(militaryNumber) && s.MilitaryNumber == militaryNumber)))
            .ToListAsync();
    }

    public async Task<ApiResponse<bool>> EndAsync(int id, DateTime endDate, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var entitlement = await queries.Query(_unitOfWork.FinancialEntitlements).FirstOrDefaultAsync(e => e.EntitlementID == id);
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

        var endFileIds = await queries.Query(_unitOfWork.RetirementFiles)
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
        var queries = await _access.ForCurrentOperationAsync();
        if (string.IsNullOrWhiteSpace(query))
            return ApiResponse<List<FinancialEntitlementDto>>.Success(new List<FinancialEntitlementDto>());

        var s = query.Trim();

        var retireeIds = await queries.Query(_unitOfWork.Retirees)
            .AsNoTracking()
            .Where(r => !r.IsDeleted
                && (r.FullName.Contains(s)
                    || (r.MilitaryNumber != null && r.MilitaryNumber.Contains(s))
                    || (r.Phone != null && r.Phone.Contains(s))))
            .Select(r => r.RetireeID)
            .ToListAsync();

        var fileNumbers = await queries.Query(_unitOfWork.RetirementFiles)
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.FileNumber.Contains(s))
            .Select(f => new { f.FileNumber, f.RetireeID })
            .ToListAsync();

        var agentRetireeIds = await queries.Query(_unitOfWork.Agents)
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.FullName.Contains(s))
            .Join(queries.Query(_unitOfWork.RetirementFiles).Where(f => !f.IsDeleted),
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

        var entitlements = await queries.Query(_unitOfWork.FinancialEntitlements)
            .AsNoTracking()
            .Where(e => allRetireeIds.Contains(e.RetireeID) && !e.IsDeleted)
            .Include(e => e.Retiree)
            .OrderByDescending(e => e.EffectiveDate)
            .ToListAsync();

        var fileDict = fileNumbers.ToDictionary(f => f.RetireeID, f => f.FileNumber);

        var agentDict = await queries.Query(_unitOfWork.Agents)
            .AsNoTracking()
            .Where(a => !a.IsDeleted && allRetireeIds.Contains(a.RetirementFile.RetireeID))
            .Join(queries.Query(_unitOfWork.RetirementFiles).Where(f => !f.IsDeleted),
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
