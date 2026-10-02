using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.DTOs.Settlements;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Core.Helpers;

namespace PensionMS.Infrastructure.Services;

public class SettlementService : ISettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettlementRepository _settlementRepository;
    private readonly IFinancialEntitlementService _financialEntitlementService;
    private readonly INotificationService _notificationService;
    private readonly IJournalEntryService _journalEntry;

    public SettlementService(
        IUnitOfWork unitOfWork, 
        ISettlementRepository settlementRepository,
        IFinancialEntitlementService financialEntitlementService, 
        INotificationService notificationService,
        IJournalEntryService journalEntry)
    {
        _unitOfWork = unitOfWork;
        _settlementRepository = settlementRepository;
        _financialEntitlementService = financialEntitlementService;
        _notificationService = notificationService;
        _journalEntry = journalEntry;
    }

    // ═══════════════════════════════════════════════════════
    // DTOs داخلية لتحليل تقارير الفشل (بدلاً من dynamic)
    // ═══════════════════════════════════════════════════════
    private class FailureReportItem
    {
        public int Row { get; set; }
        public string MilNum { get; set; } = "";
        public string Name { get; set; } = "";
        public string CurRank { get; set; } = "";
        public string ProcRank { get; set; } = "";
        public string Reason { get; set; } = "";
    }



    // ── Batches ─────────────────────────────────────────

    public async Task<ApiResponse<List<SettlementBatchListDto>>> GetAllBatchesAsync()
    {
        var result = await _settlementRepository.GetAllBatchesAsync();
        return ApiResponse<List<SettlementBatchListDto>>.Success(result);
    }

    public async Task<ApiResponse<SettlementBatchListDto>> GetBatchByIdAsync(int id)
    {
        var result = await _settlementRepository.GetBatchByIdAsync(id);
        if (result == null) return ApiResponse<SettlementBatchListDto>.Fail("الكشف غير موجود");
        return ApiResponse<SettlementBatchListDto>.Success(result);
    }

    public async Task<ApiResponse<SettlementBatchListDto>> CreateBatchAsync(CreateSettlementBatchDto dto, int userId)
    {
        // التحقق من عدم تكرار رقم الكشف
        var existing = await _settlementRepository.IsBatchNumberExistsAsync(dto.BatchNumber);

        if (existing)
            return ApiResponse<SettlementBatchListDto>.Fail("رقم الكشف مستخدم مسبقاً");

        var batch = new SettlementsBatch
        {
            BatchNumber = dto.BatchNumber,
            ReceivedDate = dto.ReceivedDate ?? DateTime.UtcNow,
            Description = dto.Description,
            CreatedByUserID = userId,
            Status = BatchStatusConstants.New,
            TotalCount = 0,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SettlementsBatches.AddAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        // إرسال إشعار — يُعزل فشله عن العملية الرئيسية
        try
        {
            await _notificationService.NotifyRoleAsync("Auditor", new CreateNotificationDto
            {
                UserID = 0,
                Title = "كشف تسوية جديد",
                Message = $"تم إنشاء كشف تسوية جديد برقم ({batch.BatchNumber}) بانتظار المراجعة.",
                Type = NotificationConstants.Types.Info,
                Category = NotificationConstants.Categories.Financial,
                Priority = NotificationConstants.Priorities.Normal,
                ActorID = userId,
                RelatedRecordID = batch.BatchID
            });
        }
        catch (Exception ex)
        {
            // تسجيل الخطأ دون إفساد الاستجابة
            // _logger.LogWarning(ex, "فشل إرسال إشعار المراجعين للكشف {BatchId}", batch.BatchID);
        }

        var result = await GetBatchByIdAsync(batch.BatchID);
        return ApiResponse<SettlementBatchListDto>.Success(result.Data!, "تم إنشاء الكشف بنجاح");
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(int batchId, ImportSettlementExcelDto dto, int userId)
    {
        // ── التحققات الأولية من الملف ──
        if (dto.File == null || dto.File.Length == 0)
            return ApiResponse<int>.Fail("الملف مطلوب");

        if (!dto.File.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<int>.Fail("يجب أن يكون الملف بصيغة .xlsx");

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        if (dto.File.Length > maxFileSize)
            return ApiResponse<int>.Fail("حجم الملف يتجاوز الحد المسموح (10 ميجابايت)");

        if (string.IsNullOrWhiteSpace(dto.DecisionNumber))
            return ApiResponse<int>.Fail("رقم القرار مطلوب");

        var batch = await _unitOfWork.SettlementsBatches.GetByIdAsync(batchId);
        if (batch == null || batch.IsDeleted)
            return ApiResponse<int>.Fail("الكشف غير موجود");

        // ── قرار التسوية ──
        var decision = await _unitOfWork.SettlementDecisions.Query()
            .FirstOrDefaultAsync(d => d.DecisionNumber == dto.DecisionNumber && !d.IsDeleted);

        if (decision == null)
        {
            decision = new SettlementDecision
            {
                DecisionNumber = dto.DecisionNumber,
                DecisionDate = dto.DecisionDate ?? DateTime.UtcNow,
                DecisionType = dto.DecisionType,
                Notes = dto.Notes,
                CreatedByUserID = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.SettlementDecisions.AddAsync(decision);
            await _unitOfWork.SaveChangesAsync();
        }

        // ── قراءة Excel ──
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var stream = dto.File.OpenReadStream();
        using var package = new ExcelPackage(stream);

        if (package.Workbook.Worksheets.Count == 0)
            return ApiResponse<int>.Fail("الملف لا يحتوي على أي ورقة عمل");

        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;

        if (rowCount < 2)
            return ApiResponse<int>.Fail("الملف فارغ أو لا يحتوي على بيانات");

        // ── تحميل الرتب في قاموس مُجهَّز (Normalization مرة واحدة) ──
        var rankList = await _unitOfWork.Ranks.Query().AsNoTracking().ToListAsync();
        var ranks = rankList
            .Select(r => new { Normalized = NormalizeText(r.RankName), Rank = r })
            .Where(x => x.Normalized != null)
            .ToDictionary(x => x.Normalized!, x => x.Rank, StringComparer.OrdinalIgnoreCase);

        // ── تحميل المتقاعدين وبنود الاعتماد (ملاحظة: يُفضَّل Batching للجداول الضخمة) ──
        var allApprovalItems = await _unitOfWork.MinistryApprovalItems.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.MilitaryNumber != null && a.MilitaryNumber != "")
            .ToListAsync();
        var approvalByMilNum = allApprovalItems
            .GroupBy(a => a.MilitaryNumber)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);

        var allRetirees = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.PendingReviewStatus != PendingReviewStatus.Pending && r.PendingReviewStatus != PendingReviewStatus.Rejected && r.MilitaryNumber != null && r.MilitaryNumber != "")
            .ToListAsync();
        var retireeByMilNum = allRetirees
            .GroupBy(r => r.MilitaryNumber!)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // ── تحميل البنود الموجودة مسبقاً في هذا الكشف ──
        var existingBatchItems = await _unitOfWork.SettlementItems.Query()
            .AsNoTracking()
            .Where(s => s.BatchID == batchId)
            .Select(s => new { s.ApprovalItemID, s.MilitaryNumber })
            .ToListAsync();

        var currentBatchApprovalIds = new HashSet<int>(
            existingBatchItems.Where(x => x.ApprovalItemID != null).Select(x => x.ApprovalItemID!.Value));
        var currentBatchMilNums = new HashSet<string>(
            existingBatchItems.Where(x => !string.IsNullOrEmpty(x.MilitaryNumber)).Select(x => x.MilitaryNumber!),
            StringComparer.OrdinalIgnoreCase);

        // ── دمج تقرير الفشل القديم (للحفاظ على الأخطاء من الملفات السابقة) ──
        var existingFailures = new Dictionary<int, FailureReportItem>();
        if (!string.IsNullOrEmpty(batch.FailureReportJson))
        {
            try
            {
                var oldList = System.Text.Json.JsonSerializer.Deserialize<List<FailureReportItem>>(batch.FailureReportJson);
                if (oldList != null)
                    foreach (var f in oldList)
                        existingFailures[f.Row] = f;
            }
            catch
            {
                // تجاهل JSON تالف
            }
        }

        int importedCount = 0;
        int skippedEmpty = 0;
        int skippedFileDuplicate = 0;
        int skippedDbDuplicate = 0;
        int failedRank = 0;
        int failedTechnical = 0;
        int trailingEmptyRows = 0;
        var failedRowsDetails = new List<string>();
        var missingRanks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        decimal totalAmount = 0;
        var entitlementQueue = new List<(int RetireeID, decimal Amount, DateTime EffectiveDate, int ProcessedRankID)>();
        var rankUpdates = new List<RetireeRankUpdate>();
        var processedInFileMilNums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int row = 2; row <= rowCount; row++)
        {
            string militaryNumber = "";
            string fullName = "";
            string currentRankName = "";
            string processedRankName = "";

            try
            {
                militaryNumber = StringHelpers.NormalizeDigits(worksheet.Cells[row, 2].Value?.ToString());
                currentRankName = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                fullName = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";
                processedRankName = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";

                // 0) صف فارغ تماماً (جميع الأعمدة) — لا يُحتسب في SourceRowsCount ولا يُسجل
                bool isCompletelyEmpty = string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Value?.ToString())
                    && string.IsNullOrWhiteSpace(militaryNumber)
                    && string.IsNullOrWhiteSpace(currentRankName)
                    && string.IsNullOrWhiteSpace(fullName)
                    && string.IsNullOrWhiteSpace(processedRankName);
                if (isCompletelyEmpty)
                {
                    trailingEmptyRows++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(militaryNumber))
                {
                    skippedEmpty++;
                    var reason = "رقم عسكري فارغ";
                    failedRowsDetails.Add($"صف {row}: {fullName} — {reason}");
                    existingFailures[row] = new FailureReportItem
                    {
                        Row = row,
                        MilNum = "",
                        Name = fullName,
                        CurRank = currentRankName,
                        ProcRank = processedRankName,
                        Reason = reason
                    };
                    continue;
                }

                // 1) تكرار داخل الملف نفسه
                if (processedInFileMilNums.Contains(militaryNumber))
                {
                    skippedFileDuplicate++;
                    var dupItem = new FailureReportItem
                    {
                        Row = row,
                        MilNum = militaryNumber,
                        Name = fullName,
                        CurRank = currentRankName,
                        ProcRank = processedRankName,
                        Reason = "مكرر داخل ملف Excel نفسه"
                    };
                    existingFailures[row] = dupItem;
                    failedRowsDetails.Add($"صف {row}: {fullName} ({militaryNumber}) — مكرر داخل الملف");
                    continue;
                }
                processedInFileMilNums.Add(militaryNumber);

                var normalizedCurrent = NormalizeText(currentRankName);
                var normalizedProcessed = NormalizeText(processedRankName);
                ranks.TryGetValue(normalizedCurrent ?? "", out var currentRank);
                ranks.TryGetValue(normalizedProcessed ?? "", out var processedRank);

                if (currentRank == null || processedRank == null)
                {
                    failedRank++;
                    var reason = (currentRank == null ? $"رتبة حالية غير معروفة ({currentRankName})" : "") +
                                 (currentRank == null && processedRank == null ? " | " : "") +
                                 (processedRank == null ? $"رتبة معالجة غير معروفة ({processedRankName})" : "");

                    failedRowsDetails.Add($"صف {row}: {fullName} ({militaryNumber}) — {reason}");
                    existingFailures[row] = new FailureReportItem
                    {
                        Row = row,
                        MilNum = militaryNumber,
                        Name = fullName,
                        CurRank = currentRankName,
                        ProcRank = processedRankName,
                        Reason = reason
                    };

                    if (currentRank == null && !string.IsNullOrWhiteSpace(currentRankName)) missingRanks.Add(currentRankName);
                    if (processedRank == null && !string.IsNullOrWhiteSpace(processedRankName)) missingRanks.Add(processedRankName);
                    continue;
                }

                approvalByMilNum.TryGetValue(militaryNumber, out var approvalItem);

                // 2) تكرار في قاعدة البيانات (نفس الكشف)
                bool isDbDuplicate = (approvalItem != null && currentBatchApprovalIds.Contains(approvalItem.ApprovalItemID))
                                     || currentBatchMilNums.Contains(militaryNumber);
                if (isDbDuplicate)
                {
                    skippedDbDuplicate++;
                    var reason = "موجود مسبقاً في الكشف";
                    failedRowsDetails.Add($"صف {row}: {fullName} ({militaryNumber}) — {reason}");
                    existingFailures[row] = new FailureReportItem
                    {
                        Row = row,
                        MilNum = militaryNumber,
                        Name = fullName,
                        CurRank = currentRankName,
                        ProcRank = processedRankName,
                        Reason = reason
                    };
                    continue;
                }

                retireeByMilNum.TryGetValue(militaryNumber, out var retiree);
                var retireeId = retiree?.RetireeID ?? approvalItem?.Retiree?.RetireeID;

                var item = new SettlementItem
                {
                    BatchID = batchId,
                    SerialNumber = int.TryParse(worksheet.Cells[row, 1].Value?.ToString(), out int sr) ? sr : 0,
                    ApprovalItemID = approvalItem?.ApprovalItemID,
                    RetireeID = retireeId,
                    MilitaryNumber = militaryNumber,
                    FullName = fullName,
                    CurrentRankID = currentRank.RankID,
                    ProcessedRankID = processedRank.RankID,
                    DecisionID = decision.DecisionID,
                    Notes = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserID = userId
                };

                await _unitOfWork.SettlementItems.AddAsync(item);
                if (approvalItem != null)
                    currentBatchApprovalIds.Add(approvalItem.ApprovalItemID);
                currentBatchMilNums.Add(militaryNumber);
                totalAmount += processedRank.BaseSalary;
                importedCount++;

                if (retireeId != null)
                {
                    entitlementQueue.Add((retireeId.Value, processedRank.BaseSalary, decision.DecisionDate, processedRank.RankID));
                    rankUpdates.Add(new RetireeRankUpdate { RetireeID = retireeId.Value, ProcessedRankID = processedRank.RankID, NewSalary = processedRank.BaseSalary });
                }
            }
            catch (Exception ex)
            {
                failedTechnical++;
                var errMsg = $"خطأ تقني: {ex.Message}";
                failedRowsDetails.Add($"صف {row}: {fullName} ({militaryNumber}) — {errMsg}");
                existingFailures[row] = new FailureReportItem
                {
                    Row = row,
                    MilNum = militaryNumber,
                    Name = fullName,
                    CurRank = currentRankName,
                    ProcRank = processedRankName,
                    Reason = errMsg
                };
            }
        }

        // ── تحديث إحصائيات الكشف ──
        batch.TotalCount += importedCount;
        batch.TotalAmount += totalAmount;

        int actualDataRowsInFile = Math.Max(0, rowCount - 1 - trailingEmptyRows);
        if (actualDataRowsInFile > batch.SourceRowsCount)
            batch.SourceRowsCount = actualDataRowsInFile;

        batch.SkippedCount = skippedEmpty + skippedFileDuplicate + skippedDbDuplicate + failedRank + failedTechnical;

        var mergedFailures = existingFailures.Values.ToList();
        if (mergedFailures.Count > 0)
            batch.FailureReportJson = System.Text.Json.JsonSerializer.Serialize(mergedFailures);
        else if (batch.TotalCount >= batch.SourceRowsCount)
            batch.FailureReportJson = null;

        if (batch.Status == BatchStatusConstants.New) batch.Status = BatchStatusConstants.Imported;
        _unitOfWork.SettlementsBatches.Update(batch);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            // _logger.LogError(ex, "DbUpdateException أثناء استيراد الكشف {BatchId}", batchId);
            return ApiResponse<int>.Fail("خطأ في حفظ البيانات. يرجى المحاولة لاحقاً");
        }

        // ── معالجة الاستحقاقات وتحديث الرتب (بعد الحفظ الرئيسي) ──
        foreach (var (retireeId, amount, effectiveDate, processedRankId) in entitlementQueue)
        {
            await _financialEntitlementService.CreateAsync(new CreateFinancialEntitlementDto
            {
                RetireeID = retireeId,
                Amount = amount,
                EffectiveDate = effectiveDate,
                Notes = $"تسوية رتبة — قرار رقم {dto.DecisionNumber}"
            }, userId);
        }

        // تحديث رتب المتقاعدين دفعة واحدة (بدون SaveChanges متكرر)
        await ApplyRetireeRankUpdatesAsync(rankUpdates);

        // ── بناء رسالة الاستجابة ──
        var message = $"تم استيراد {importedCount} بند وربطهم بالقرار {dto.DecisionNumber}";
        if (skippedEmpty > 0)
            message += $" | فشل {skippedEmpty} صف (رقم عسكري فارغ)";
        if (skippedFileDuplicate > 0)
            message += $" | تم تخطي {skippedFileDuplicate} بند (مكرر داخل الملف)";
        if (skippedDbDuplicate > 0)
            message += $" | تم تخطي {skippedDbDuplicate} بند (مكرر في الكشف)";
        if (failedRank > 0)
            message += $" | فشل {failedRank} صف (رتب غير معروفة)";
        if (failedTechnical > 0)
            message += $" | فشل {failedTechnical} صف (أخطاء تقنية)";

        if (failedRowsDetails.Count > 0)
        {
            message += $" | التفاصيل: {string.Join(" ، ", failedRowsDetails.Take(15))}";
            if (failedRowsDetails.Count > 15) message += " ... وغيرها";
        }

        if (failedRowsDetails.Count > 0)
        {
            try
            {
                await _notificationService.NotifyRoleAsync("Auditor", new CreateNotificationDto
                {
                    UserID = 0,
                    Title = "تحذير: أخطاء في استيراد كشف تسوية",
                    Message = $"تم استيراد كشف التسوية رقم ({batch.BatchNumber}) مع وجود {failedRowsDetails.Count} خطأ. يرجى مراجعة تقرير الفشل.", 
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Financial,
                    Priority = NotificationConstants.Priorities.Normal,
                    ActorID = userId,
                    RelatedRecordID = batch.BatchID
                });
            }
            catch { }
        }

        var response = ApiResponse<int>.Success(importedCount, message);
        if (missingRanks.Count > 0)
            response.Errors = missingRanks.ToList();

        return response;
    }

    public async Task<ApiResponse<List<SettlementItemDto>>> GetBatchItemsAsync(int batchId)
    {
        var result = await _settlementRepository.GetBatchItemsAsync(batchId);
        return ApiResponse<List<SettlementItemDto>>.Success(result);
    }

    public async Task<ApiResponse<bool>> ApproveBatchAsync(int id, int userId)
    {
        var batch = await _unitOfWork.SettlementsBatches.GetByIdAsync(id);
        if (batch == null || batch.IsDeleted)
            return ApiResponse<bool>.Fail("الكشف غير موجود");

        if (batch.Status == BatchStatusConstants.Approved)
            return ApiResponse<bool>.Fail("الكشف معتمد بالفعل");

        await _unitOfWork.BeginTransactionAsync();

        batch.Status = BatchStatusConstants.Approved;
        batch.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SettlementsBatches.Update(batch);

        // Accounting: end-of-service compensation entry
        var periodId = await _journalEntry.GetPeriodIdForDateAsync(batch.UpdatedAt.Value);
        var accResult = await _journalEntry.CreateIntegrationEntryAsync(new FinancialOperationRequestDto
        {
            OperationType = FinancialOperationType.EndOfServiceBonus,
            Amount = batch.TotalAmount,
            PeriodID = periodId,
            OperationDate = batch.UpdatedAt.Value,
            Description = $"تعويضات نهاية خدمة — كشف رقم {batch.BatchNumber}",
            SourceReferenceID = batch.BatchID
        }, userId);

        if (!accResult.IsSuccess)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Fail(accResult.Message);
        }

        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();

        try
        {
            await _notificationService.NotifyRoleAsync("Accountant", new CreateNotificationDto
            {
                UserID = 0,
                Title = "اعتماد كشف تسوية",
                Message = $"تم اعتماد كشف التسوية رقم ({batch.BatchNumber}) وهو جاهز الآن لعمليات الصرف.",
                Type = NotificationConstants.Types.Success,
                Category = NotificationConstants.Categories.Financial,
                Priority = NotificationConstants.Priorities.High,
                ActorID = userId,
                RelatedRecordID = batch.BatchID
            });
        }
        catch (Exception ex)
        {
            // _logger.LogWarning(ex, "فشل إرسال إشعار المحاسبين للكشف {BatchId}", id);
        }

        return ApiResponse<bool>.Success(true, "تم اعتماد الكشف بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteBatchAsync(int id, int userId)
    {
        var batch = await _unitOfWork.SettlementsBatches.Query()
            .Include(b => b.SettlementItems)
            .FirstOrDefaultAsync(b => b.BatchID == id && !b.IsDeleted);

        if (batch == null) return ApiResponse<bool>.Fail("الكشف غير موجود");

        foreach (var item in batch.SettlementItems)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedByUserID = userId;
            _unitOfWork.SettlementItems.Update(item);
        }

        batch.IsDeleted = true;
        batch.DeletedAt = DateTime.UtcNow;
        batch.DeletedByUserID = userId;
        _unitOfWork.SettlementsBatches.Update(batch);

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم حذف الكشف بنجاح");
    }

    // ── Decisions (primary UI flow) ─────────────────────

    public async Task<ApiResponse<PagedResult<SettlementDecisionDto>>> GetAllDecisionsAsync(int page, int pageSize, string? searchTerm = null)
    {
        var result = await _settlementRepository.GetDecisionsPagedAsync(page, pageSize, searchTerm);
        return ApiResponse<PagedResult<SettlementDecisionDto>>.Success(result);
    }

    public async Task<ApiResponse<SettlementDecisionDto>> GetDecisionByIdAsync(int decisionId)
    {
        var result = await _settlementRepository.GetDecisionByIdAsync(decisionId);
        if (result == null) return ApiResponse<SettlementDecisionDto>.Fail("القرار غير موجود");
        return ApiResponse<SettlementDecisionDto>.Success(result);
    }

    public async Task<ApiResponse<PagedResult<SettlementItemDto>>> GetDecisionItemsAsync(int decisionId, int page, int pageSize, string? searchTerm = null)
    {
        var result = await _settlementRepository.GetDecisionItemsPagedAsync(decisionId, page, pageSize, searchTerm);
        return ApiResponse<PagedResult<SettlementItemDto>>.Success(result);
    }

    public async Task<ApiResponse<SettlementDecisionDto>> CreateDecisionWithExcelAsync(
        ImportSettlementExcelDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.DecisionNumber))
            return ApiResponse<SettlementDecisionDto>.Fail("رقم القرار مطلوب");

        var batchNumber = $"{dto.DecisionNumber}-{PensionMS.Core.Time.YemenTime.Now:yyyyMMddHHmmss}";
        var batch = new SettlementsBatch
        {
            BatchNumber = batchNumber,
            ReceivedDate = dto.DecisionDate ?? DateTime.UtcNow,
            Description = $"كشف القرار {dto.DecisionNumber}",
            CreatedByUserID = userId,
            Status = BatchStatusConstants.New,
            TotalCount = 0,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.SettlementsBatches.AddAsync(batch);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            return ApiResponse<SettlementDecisionDto>.Fail("خطأ في إنشاء الكشف. يرجى المحاولة لاحقاً");
        }

        var importResult = await ImportExcelAsync(batch.BatchID, dto, userId);
        if (!importResult.IsSuccess)
        {
            // حذف الكشف الفارغ/الفاشل لمنع تلوث قاعدة البيانات
            try
            {
                batch.IsDeleted = true;
                batch.DeletedAt = DateTime.UtcNow;
                batch.DeletedByUserID = userId;
                _unitOfWork.SettlementsBatches.Update(batch);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                // تجاهل فشل الحذف — لا نريد إخفاء الخطأ الأصلي
            }
            return ApiResponse<SettlementDecisionDto>.Fail(importResult.Message ?? "فشل الاستيراد");
        }

        var decision = await _unitOfWork.SettlementDecisions.Query()
            .AsNoTracking()
            .Where(d => d.DecisionNumber == dto.DecisionNumber && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync();

        if (decision == null)
            return ApiResponse<SettlementDecisionDto>.Fail("خطأ في إنشاء القرار");

        var resultResponse = await GetDecisionByIdAsync(decision.DecisionID);
        var finalResponse = ApiResponse<SettlementDecisionDto>.Success(resultResponse.Data!, importResult.Message);
        finalResponse.Errors = importResult.Errors;
        return finalResponse;
    }

    public async Task<ApiResponse<bool>> DeleteDecisionAsync(int decisionId, int userId)
    {
        var decision = await _unitOfWork.SettlementDecisions.Query()
            .Include(d => d.SettlementItems)
            .FirstOrDefaultAsync(d => d.DecisionID == decisionId && !d.IsDeleted);

        if (decision == null) return ApiResponse<bool>.Fail("القرار غير موجود");

        foreach (var item in decision.SettlementItems)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedByUserID = userId;
            _unitOfWork.SettlementItems.Update(item);
        }

        decision.IsDeleted = true;
        decision.DeletedAt = DateTime.UtcNow;
        decision.DeletedByUserID = userId;
        _unitOfWork.SettlementDecisions.Update(decision);

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم حذف القرار بنجاح");
    }

    public async Task<ApiResponse<List<SettlementDecisionDto>>> GetBatchDecisionsAsync(int batchId)
    {
        var result = await _settlementRepository.GetBatchDecisionsAsync(batchId);
        return ApiResponse<List<SettlementDecisionDto>>.Success(result);
    }

    public async Task<ApiResponse<SettlementDecisionDto>> CreateDecisionAsync(
        int batchId, CreateSettlementDecisionDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.DecisionNumber))
            return ApiResponse<SettlementDecisionDto>.Fail("رقم القرار مطلوب");

        if (dto.ItemIDs == null || dto.ItemIDs.Count == 0)
            return ApiResponse<SettlementDecisionDto>.Fail("يجب اختيار بند واحد على الأقل");

        var items = await _unitOfWork.SettlementItems.Query()
            .Where(i => !i.IsDeleted && dto.ItemIDs.Contains(i.SettlementItemID) && i.BatchID == batchId)
            .ToListAsync();

        if (items.Count != dto.ItemIDs.Count)
            return ApiResponse<SettlementDecisionDto>.Fail("بعض البنود غير موجودة في هذا الكشف");

        var decision = new SettlementDecision
        {
            DecisionNumber = dto.DecisionNumber,
            DecisionDate = dto.DecisionDate ?? DateTime.UtcNow,
            DecisionType = dto.DecisionType,
            Notes = dto.Notes,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SettlementDecisions.AddAsync(decision);
        await _unitOfWork.SaveChangesAsync();

        foreach (var item in items)
        {
            item.DecisionID = decision.DecisionID;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedByUserID = userId;
            _unitOfWork.SettlementItems.Update(item);
        }
        await _unitOfWork.SaveChangesAsync();

        var result = new SettlementDecisionDto
        {
            DecisionID = decision.DecisionID,
            DecisionNumber = decision.DecisionNumber,
            DecisionDate = decision.DecisionDate,
            DecisionType = decision.DecisionType,
            Notes = decision.Notes,
            ItemCount = items.Count,
            CreatedAt = decision.CreatedAt
        };

        return ApiResponse<SettlementDecisionDto>.Success(result, "تم إنشاء القرار وربط البنود بنجاح");
    }

    public async Task<ApiResponse<bool>> LinkItemToRetireeAsync(LinkSettlementItemDto dto, int userId)
    {
        var item = await _unitOfWork.SettlementItems.Query()
            .Include(i => i.ProcessedRank)
            .Include(i => i.Decision)
            .FirstOrDefaultAsync(i => i.SettlementItemID == dto.SettlementItemID && !i.IsDeleted);

        if (item == null)
            return ApiResponse<bool>.Fail("القيد غير موجود");

        var retiree = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RetireeID == dto.RetireeID && !r.IsDeleted);

        if (retiree == null)
            return ApiResponse<bool>.Fail("المتقاعد غير موجود");

        item.RetireeID = retiree.RetireeID;
        if (!string.IsNullOrWhiteSpace(retiree.MilitaryNumber))
            item.MilitaryNumber = retiree.MilitaryNumber;

        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedByUserID = userId;
        _unitOfWork.SettlementItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        if (item.ProcessedRank != null)
        {
            var decisionNumber = item.Decision?.DecisionNumber ?? "غير محدد";
            await _financialEntitlementService.CreateAsync(new CreateFinancialEntitlementDto
            {
                RetireeID = retiree.RetireeID,
                Amount = item.ProcessedRank.BaseSalary,
                EffectiveDate = item.Decision?.DecisionDate ?? DateTime.UtcNow,
                Notes = $"تسوية رتبة — قرار رقم {decisionNumber}"
            }, userId);

            await ApplyRetireeRankUpdatesAsync(new List<RetireeRankUpdate>
            {
                new() { RetireeID = retiree.RetireeID, ProcessedRankID = item.ProcessedRankID, NewSalary = item.ProcessedRank.BaseSalary }
            });
        }

        var linkFileIds = await _unitOfWork.RetirementFiles.Query()
            .Where(f => f.RetireeID == retiree.RetireeID && !f.IsDeleted)
            .Select(f => f.FileID)
            .ToListAsync();

        foreach (var linkFileId in linkFileIds)
        {
            var linkFile = await _unitOfWork.RetirementFiles.GetByIdAsync(linkFileId);
            if (linkFile == null) continue;
            var linkNotifyUserId = linkFile.AssignedEmployeeID;
            if (linkNotifyUserId == null || linkNotifyUserId == 0)
            {
                var creatorId = await _unitOfWork.AuditLogs.Query()
                    .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == linkFileId && a.Action == "Create")
                    .Select(a => a.UserID)
                    .FirstOrDefaultAsync();
                linkNotifyUserId = creatorId;
            }
            if (linkNotifyUserId.HasValue && linkNotifyUserId.Value > 0 && linkNotifyUserId.Value != userId)
            {
                try
                {
                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserID = linkNotifyUserId.Value,
                        Title = "تسوية رتبة - ربط متقاعد",
                        Message = $"تم ربط المتقاعد ({retiree.FullName}) بقيد تسوية وإنشاء استحقاق مالي جديد للملف رقم ({linkFile.FileNumber}).",
                        Type = NotificationConstants.Types.Info,
                        Category = NotificationConstants.Categories.Financial,
                        Priority = NotificationConstants.Priorities.Normal,
                        ActorID = userId,
                        RelatedTableID = SystemTableConstants.RetirementFiles,
                        RelatedRecordID = linkFileId
                    });
                }
                catch { }
            }
        }

        return ApiResponse<bool>.Success(true, "تم الربط وتحديث الاستحقاق المالي والرتبة بنجاح");
    }

    public async Task<ApiResponse<List<SettlementItemDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var result = await _settlementRepository.GetByRetireeIdAsync(retireeId);
        return ApiResponse<List<SettlementItemDto>>.Success(result);
    }

    public async Task<byte[]> ExportDecisionToExcelAsync(int decisionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var decision = await _unitOfWork.SettlementDecisions.Query()
            .AsNoTracking()
            .Where(d => d.DecisionID == decisionId && !d.IsDeleted)
            .Select(d => new { d.DecisionNumber, d.DecisionDate, d.DecisionType })
            .FirstOrDefaultAsync();

        var items = await _settlementRepository.GetItemsByDecisionIdAsync(decisionId);

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("قرار التسوية");
        ws.View.RightToLeft = true;

        var title = decision != null
            ? $"قرار تسوية رتب — رقم {decision.DecisionNumber} — {decision.DecisionDate:yyyy/MM/dd}"
            : "قرار تسوية رتب";
        ws.Cells[1, 1, 1, 8].Merge = true;
        ws.Cells[1, 1].Value = title;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;
        ws.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        ws.Row(1).Height = 32;

        var headers = new[] { "#", "الرقم العسكري", "الاسم الكامل", "الرتبة الحالية", "الرتبة المعالجة", "المعاش المعتمد", "حالة الربط", "ملاحظات" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cells[3, i + 1].Value = headers[i];

        using (var range = ws.Cells[3, 1, 3, headers.Length])
        {
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 11;
            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 73, 125));
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
        }
        ws.Row(3).Height = 26;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 4;
            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = item.MilitaryNumber;
            ws.Cells[row, 3].Value = item.FullName;
            ws.Cells[row, 4].Value = item.CurrentRank?.RankName;
            ws.Cells[row, 5].Value = item.ProcessedRank?.RankName;
            ws.Cells[row, 6].Value = item.ProcessedRank?.BaseSalary ?? 0;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 7].Value = item.RetireeID.HasValue ? "مطابق" : "غير مطابق";
            ws.Cells[row, 8].Value = item.Notes;
        }

        var footerRow = items.Count + 5;
        ws.Cells[footerRow, 1, footerRow, 3].Merge = true;
        ws.Cells[footerRow, 1].Value = $"الإجمالي: {items.Count} بند";
        ws.Cells[footerRow, 1].Style.Font.Bold = true;
        ws.Cells[footerRow, 6].Value = items.Sum(i => i.ProcessedRank?.BaseSalary ?? 0);
        ws.Cells[footerRow, 6].Style.Numberformat.Format = "#,##0";
        ws.Cells[footerRow, 6].Style.Font.Bold = true;

        for (int i = 1; i <= headers.Length; i++)
        {
            ws.Column(i).AutoFit();
            if (ws.Column(i).Width < 12) ws.Column(i).Width = 12;
        }

        return package.GetAsByteArray();
    }

    public async Task<byte[]> GenerateImportFailuresExcelAsync(IRemoteFile file, int decisionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var stream = file.OpenReadStream();
        using var package = new ExcelPackage(stream);

        if (package.Workbook.Worksheets.Count == 0)
            return Array.Empty<byte>();

        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;

        // Load existing items for this decision to check for duplicates
        var existingMilNums = new HashSet<string>(
            await _unitOfWork.SettlementItems.Query()
                .AsNoTracking()
                .Where(i => i.DecisionID == decisionId && !i.IsDeleted && !string.IsNullOrEmpty(i.MilitaryNumber))
                .Select(i => i.MilitaryNumber!)
                .ToListAsync(),
            StringComparer.OrdinalIgnoreCase
        );

        // Load all ranks for matching
        var rankList = await _unitOfWork.Ranks.Query().AsNoTracking().ToListAsync();
        var ranks = rankList
            .Select(r => new { Normalized = NormalizeText(r.RankName), Rank = r })
            .Where(x => x.Normalized != null)
            .ToDictionary(x => x.Normalized!, x => x.Rank, StringComparer.OrdinalIgnoreCase);

        using var resultPackage = new ExcelPackage();
        var ws = resultPackage.Workbook.Worksheets.Add("صفوف فاشلة");
        ws.View.RightToLeft = true;

        var headers = new[] { "رقم الصف", "الرقم العسكري", "الرتبة الحالية", "الاسم الكامل", "الرتبة المعالجة", "سبب الفشل" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
        }

        int failureIndex = 2;
        var processedInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                var militaryNumber = StringHelpers.NormalizeDigits(worksheet.Cells[row, 2].Value?.ToString());
                if (string.IsNullOrWhiteSpace(militaryNumber)) continue;

                // Skip if already in database (request of the user: exclude from report)
                if (existingMilNums.Contains(militaryNumber)) continue;

                // Skip if duplicate within the same file being checked
                if (processedInFile.Contains(militaryNumber)) continue;
                processedInFile.Add(militaryNumber);

                var currentRankName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                var fullName = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";
                var processedRankName = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

                var normalizedCurrent = NormalizeText(currentRankName);
                var normalizedProcessed = NormalizeText(processedRankName);
                ranks.TryGetValue(normalizedCurrent ?? "", out var currentRank);
                ranks.TryGetValue(normalizedProcessed ?? "", out var processedRank);

                if (currentRank == null || processedRank == null)
                {
                    var reason = (currentRank == null ? $"رتبة حالية غير معروفة ({currentRankName})" : "") +
                                 (currentRank == null && processedRank == null ? " | " : "") +
                                 (processedRank == null ? $"رتبة معالجة غير معروفة ({processedRankName})" : "");

                    ws.Cells[failureIndex, 1].Value = row;
                    ws.Cells[failureIndex, 2].Value = militaryNumber;
                    ws.Cells[failureIndex, 3].Value = currentRankName;
                    ws.Cells[failureIndex, 4].Value = fullName;
                    ws.Cells[failureIndex, 5].Value = processedRankName;
                    ws.Cells[failureIndex, 6].Value = reason;
                    failureIndex++;
                }
            }
            catch (Exception ex)
            {
                ws.Cells[failureIndex, 1].Value = row;
                ws.Cells[failureIndex, 6].Value = $"خطأ تقني: {ex.Message}";
                failureIndex++;
            }
        }

        for (int i = 1; i <= headers.Length; i++) ws.Column(i).AutoFit();

        return resultPackage.GetAsByteArray();
    }

    public async Task<byte[]> ExportBatchFailureReportAsync(int decisionId)
    {
        // Resolve batch ID from decision items
        var batchId = await _unitOfWork.SettlementItems.Query()
            .AsNoTracking()
            .Where(i => i.DecisionID == decisionId)
            .Select(i => i.BatchID)
            .FirstOrDefaultAsync();

        if (batchId == 0) return Array.Empty<byte>();

        var batch = await _unitOfWork.SettlementsBatches.Query()
            .AsNoTracking()
            .Where(b => b.BatchID == batchId)
            .Select(b => new { b.FailureReportJson, b.BatchNumber })
            .FirstOrDefaultAsync();

        if (batch == null || string.IsNullOrEmpty(batch.FailureReportJson))
            return Array.Empty<byte>();

        var failures = System.Text.Json.JsonSerializer.Deserialize<List<FailureReportItem>>(batch.FailureReportJson);
        if (failures == null) return Array.Empty<byte>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var resultPackage = new ExcelPackage();
        var ws = resultPackage.Workbook.Worksheets.Add("صفوف فاشلة");
        ws.View.RightToLeft = true;

        var headers = new[] { "رقم الصف", "الرقم العسكري", "الاسم الكامل", "الرتبة الحالية", "الرتبة المعالجة", "سبب الفشل" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
        }

        int rowIdx = 2;
        foreach (var f in failures)
        {
            ws.Cells[rowIdx, 1].Value = f.Row;
            ws.Cells[rowIdx, 2].Value = f.MilNum;
            ws.Cells[rowIdx, 3].Value = f.Name;
            ws.Cells[rowIdx, 4].Value = f.CurRank;
            ws.Cells[rowIdx, 5].Value = f.ProcRank;
            ws.Cells[rowIdx, 6].Value = f.Reason;
            rowIdx++;
        }

        for (int i = 1; i <= headers.Length; i++) ws.Column(i).AutoFit();

        return resultPackage.GetAsByteArray();
    }

    // ═══════════════════════════════════════════════════════
    // دوال خاصة مُحسَّنة (Helpers)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// تطبيق تحديثات الرتب دفعة واحدة دون استدعاء SaveChanges متكرر.
    /// </summary>
    private async Task ApplyRetireeRankUpdatesAsync(List<RetireeRankUpdate> updates)
    {
        await _settlementRepository.ApplyRetireeRankUpdatesAsync(updates);
    }

    private static string? NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = text.Trim();
        var diacritics = new[] { '\u064B', '\u064C', '\u064D', '\u064E', '\u064F', '\u0650', '\u0651', '\u0652', '\u0670' };
        foreach (var d in diacritics) normalized = normalized.Replace(d.ToString(), "");
        normalized = normalized.Replace("أ", "ا").Replace("إ", "ا").Replace("آ", "ا");
        normalized = normalized.Replace("ة", "ه");
        return normalized;
    }
}