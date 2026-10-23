using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OfficeOpenXml;
using PensionMS.Infrastructure.Reporting.Exporters;
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

namespace PensionMS.Infrastructure.Services.Settlements;

public class SettlementService : ISettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly ISettlementRepository _settlementRepository;
    private readonly IFinancialEntitlementService _financialEntitlementService;
    private readonly INotificationService _notificationService;
    private readonly IJournalEntryService _journalEntry;
    private readonly IImmutableAuditService _auditService;
    private readonly IMemoryCache _cache;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly ExcelReportExporter _excelExporter;

    public SettlementService(
        IUnitOfWork unitOfWork, 
        ISettlementRepository settlementRepository,
        IFinancialEntitlementService financialEntitlementService, 
        INotificationService notificationService,
        IJournalEntryService journalEntry,
        IImmutableAuditService auditService,
        IMemoryCache cache,
        IMissingDocumentService missingDocumentService,
        ExcelReportExporter excelExporter, IAccessiblePensionRepository access)
    {
        _access = access;
        _unitOfWork = unitOfWork;
        _settlementRepository = settlementRepository;
        _financialEntitlementService = financialEntitlementService;
        _notificationService = notificationService;
        _journalEntry = journalEntry;
        _auditService = auditService;
        _cache = cache;
        _missingDocumentService = missingDocumentService;
        _excelExporter = excelExporter;
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
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetAllBatchesAsync();
        return ApiResponse<List<SettlementBatchListDto>>.Success(result);
    }

    public async Task<ApiResponse<SettlementBatchListDto>> GetBatchByIdAsync(int id)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetBatchByIdAsync(id);
        if (result == null) return ApiResponse<SettlementBatchListDto>.Fail("الكشف غير موجود");
        return ApiResponse<SettlementBatchListDto>.Success(result);
    }

    public async Task<ApiResponse<SettlementBatchListDto>> CreateBatchAsync(CreateSettlementBatchDto dto, int userId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
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
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await ImportExcelCoreAsync(batchId, dto, userId);
            if (!result.IsSuccess)
            {
                if (ownsTransaction) await _unitOfWork.RollbackAsync();
                return result;
            }

            if (ownsTransaction) await _unitOfWork.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
            return ApiResponse<int>.Fail("تعذر إكمال استيراد كشف التسوية وتحديث الاستحقاقات المالية");
        }
    }

    private async Task<ApiResponse<int>> ImportExcelCoreAsync(int batchId, ImportSettlementExcelDto dto, int userId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
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

        // ── بوابة الكشف المعتمد ──
        // الاعتماد يرحّل قيداً محاسبياً على batch.TotalAmount. أي استيراد بعده يعيد حساب
        // الإجمالي فينتج دفتر أستاذ غير مطابق. الرفض قبل أي تعديل على القرار أو البنود.
        if (IsBatchFinanciallyLocked(batch.Status))
            return ApiResponse<int>.Fail(
                "لا يمكن استيراد بيانات إضافية إلى كشف معتمد مالياً ومحاسبياً");

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
            .Where(r => !r.IsDeleted && r.ApprovalItem != null && !r.ApprovalItem.IsDeleted && r.ApprovalItem.IsProcessed && r.MilitaryNumber != null && r.MilitaryNumber != "")
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
                var retireeId = retiree?.RetireeID;

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

                var keysToRemove = existingFailures.Where(kv =>
                    (!string.IsNullOrEmpty(kv.Value.MilNum) && kv.Value.MilNum.Equals(militaryNumber, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(kv.Value.Name) && !string.IsNullOrEmpty(fullName) && kv.Value.Name.Equals(fullName, StringComparison.OrdinalIgnoreCase)) ||
                    kv.Key == row
                ).Select(kv => kv.Key).ToList();
                foreach (var k in keysToRemove) existingFailures.Remove(k);

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

        // ── حفظ البنود أولاً ──
        // الإجماليات تُشتق من البنود المخزنة، فلا تُحسب قبل flush ولا تُضاف عبر +=.
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ApiResponse<int>.Fail("خطأ في حفظ البيانات. يرجى المحاولة لاحقاً");
        }

        // ── تحديث إحصائيات الكشف ──
        await RecalculateBatchTotalsAsync(batch);

        int actualDataRowsInFile = Math.Max(0, rowCount - 1 - trailingEmptyRows);
        if (actualDataRowsInFile > batch.SourceRowsCount)
            batch.SourceRowsCount = actualDataRowsInFile;

        var mergedFailures = existingFailures.Values.ToList();
        if (mergedFailures.Count == 0 || batch.TotalCount >= batch.SourceRowsCount)
        {
            batch.FailureReportJson = null;
            batch.SkippedCount = 0;
            batch.SourceRowsCount = batch.TotalCount;
        }
        else
        {
            batch.FailureReportJson = System.Text.Json.JsonSerializer.Serialize(mergedFailures);
            batch.SkippedCount = mergedFailures.Count;
        }

        if (batch.Status == BatchStatusConstants.New) batch.Status = BatchStatusConstants.Imported;
        _unitOfWork.SettlementsBatches.Update(batch);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ApiResponse<int>.Fail("خطأ في حفظ البيانات. يرجى المحاولة لاحقاً");
        }

        // ── معالجة الاستحقاقات وتحديث الرتب (بعد الحفظ الرئيسي) ──
        foreach (var (retireeId, amount, effectiveDate, processedRankId) in entitlementQueue)
        {
            var entitlementResult = await _financialEntitlementService.CreateAsync(new CreateFinancialEntitlementDto
            {
                RetireeID = retireeId,
                Amount = amount,
                EffectiveDate = effectiveDate,
                Notes = $"تسوية رتبة — قرار رقم {dto.DecisionNumber}"
            }, userId);
            if (!entitlementResult.IsSuccess)
                return ApiResponse<int>.Fail($"تعذر إنشاء الاستحقاق المالي للمتقاعد {retireeId}: {entitlementResult.Message}");
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
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetBatchItemsAsync(batchId);
        return ApiResponse<List<SettlementItemDto>>.Success(result);
    }

    public Task<ApiResponse<bool>> ApproveBatchAsync(int id, int userId) =>
        PensionMS.Infrastructure.Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => ApproveBatchCoreAsync(id, userId));

    private async Task<ApiResponse<bool>> ApproveBatchCoreAsync(int id, int userId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var batch = await _unitOfWork.SettlementsBatches.GetByIdAsync(id);
        if (batch == null || batch.IsDeleted)
            return ApiResponse<bool>.Fail("الكشف غير موجود");

        if (IsBatchFinanciallyLocked(batch.Status))
            return ApiResponse<bool>.Fail("الكشف معتمد بالفعل");

        // بوابة الاعتماد: لا يُرحَّل قيد على أساس كشف لم يُستورد أو ناقص أو يحمل أخطاء مفتوحة.
        if (batch.Status != BatchStatusConstants.Imported)
            return ApiResponse<bool>.Fail("لا يمكن اعتماد كشف لم يتم استيراد بياناته بعد");

        if (batch.TotalCount <= 0)
            return ApiResponse<bool>.Fail("لا يمكن اعتماد كشف لا يحتوي على أي بند");

        if (!string.IsNullOrEmpty(batch.FailureReportJson))
            return ApiResponse<bool>.Fail("لا يمكن اعتماد كشف يحتوي على أخطاء استيراد لم تُعالج");

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
            return ApiResponse<bool>.Fail(accResult.Message);
        }

        await _notificationService.EnqueueRoleAsync("Accountant", new CreateNotificationDto
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
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم اعتماد الكشف بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteBatchAsync(int id, int userId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var batch = await _unitOfWork.SettlementsBatches.Query()
            .Include(b => b.SettlementItems)
            .FirstOrDefaultAsync(b => b.BatchID == id && !b.IsDeleted);

        if (batch == null) return ApiResponse<bool>.Fail("الكشف غير موجود");

        // الاعتماد يرحّل قيداً محاسبياً؛ الحذف بعده يفقد الأثر المالي بلا عكس.
        // الإلغاء المالي عملية صريحة (ReverseBatchAsync) لا Soft Delete.
        if (IsBatchFinanciallyLocked(batch.Status))
            return ApiResponse<bool>.Fail(
                "لا يمكن حذف كشف معتمد مالياً ومحاسبياً. استخدم عملية الإلغاء المالي");

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

        await _auditService.LogAsync(userId, userId.ToString(), "Delete", nameof(SettlementsBatch),
            batch.BatchID, null, $"الحذف المنطقي للكشف رقم {batch.BatchNumber} وحذف {batch.SettlementItems.Count} بند");

        return ApiResponse<bool>.Success(true, "تم حذف الكشف بنجاح");
    }

    // ── Decisions (primary UI flow) ─────────────────────

    public async Task<ApiResponse<PagedResult<SettlementDecisionDto>>> GetAllDecisionsAsync(int page, int pageSize, string? searchTerm = null)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetDecisionsPagedAsync(page, pageSize, searchTerm);
        return ApiResponse<PagedResult<SettlementDecisionDto>>.Success(result);
    }

    public async Task<ApiResponse<SettlementDecisionDto>> GetDecisionByIdAsync(int decisionId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetDecisionByIdAsync(decisionId);
        if (result == null) return ApiResponse<SettlementDecisionDto>.Fail("القرار غير موجود");
        return ApiResponse<SettlementDecisionDto>.Success(result);
    }

    public async Task<ApiResponse<PagedResult<SettlementItemDto>>> GetDecisionItemsAsync(int decisionId, int page, int pageSize, string? searchTerm = null)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetDecisionItemsPagedAsync(decisionId, page, pageSize, searchTerm);
        return ApiResponse<PagedResult<SettlementItemDto>>.Success(result);
    }

    public async Task<ApiResponse<SettlementDecisionDto>> CreateDecisionWithExcelAsync(
        ImportSettlementExcelDto dto, int userId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
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

    public Task<ApiResponse<bool>> DeleteDecisionAsync(int decisionId, int userId) =>
        PensionMS.Infrastructure.Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => DeleteDecisionCoreAsync(decisionId, userId));

    private async Task<ApiResponse<bool>> DeleteDecisionCoreAsync(int decisionId, int userId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var decision = await _unitOfWork.SettlementDecisions.Query()
            .Include(d => d.SettlementItems)
            .FirstOrDefaultAsync(d => d.DecisionID == decisionId && !d.IsDeleted);

        if (decision == null) return ApiResponse<bool>.Fail("القرار غير موجود");

        // القيد المحاسبي يُرحَّل على مستوى الكشف المرتبط بالقرار. حذف قرار داخل كشف معتمد
        // يغيّر قاعدة الاحتساب دون إذن مالي، فيُمنع حتى لو كان الكشف نفسه غير معتمد.
        var affectedBatchIds = decision.SettlementItems
            .Where(i => i.BatchID > 0)
            .Select(i => i.BatchID)
            .Distinct()
            .ToList();

        foreach (var batchId in affectedBatchIds)
        {
            var affectedBatch = await _unitOfWork.SettlementsBatches.GetByIdAsync(batchId);
            if (affectedBatch != null && IsBatchFinanciallyLocked(affectedBatch.Status))
                return ApiResponse<bool>.Fail(
                    "لا يمكن حذف قرار مرتبط بكشف معتمد مالياً ومحاسبياً. استخدم عملية الإلغاء المالي");
        }

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

        // إعادة اشتقاق إجماليات كل كشف تأثر — الحذف لا يترك أرقاماً قديمة.
        foreach (var batchId in affectedBatchIds)
        {
            var affectedBatch = await _unitOfWork.SettlementsBatches.GetByIdAsync(batchId);
            if (affectedBatch == null) continue;
            await RecalculateBatchTotalsAsync(affectedBatch);
        }

        if (affectedBatchIds.Count > 0)
            await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, userId.ToString(), "Delete", nameof(SettlementDecision),
            decision.DecisionID, null, $"الحذف المنطقي للقرار رقم {decision.DecisionNumber} وحذف {decision.SettlementItems.Count} بند");

        return ApiResponse<bool>.Success(true, "تم حذف القرار بنجاح");
    }

    public async Task<ApiResponse<List<SettlementDecisionDto>>> GetBatchDecisionsAsync(int batchId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetBatchDecisionsAsync(batchId);
        return ApiResponse<List<SettlementDecisionDto>>.Success(result);
    }

    public Task<ApiResponse<SettlementDecisionDto>> CreateDecisionAsync(
        int batchId, CreateSettlementDecisionDto dto, int userId) =>
        PensionMS.Infrastructure.Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => CreateDecisionCoreAsync(batchId, dto, userId));

    private async Task<ApiResponse<SettlementDecisionDto>> CreateDecisionCoreAsync(
        int batchId, CreateSettlementDecisionDto dto, int userId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
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
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var item = await _unitOfWork.SettlementItems.Query()
            .Include(i => i.ProcessedRank)
            .Include(i => i.Decision)
            .FirstOrDefaultAsync(i => i.SettlementItemID == dto.SettlementItemID && !i.IsDeleted);

        if (item == null)
            return ApiResponse<bool>.Fail("القيد غير موجود");

        var retiree = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Include(r => r.ApprovalItem)
            .FirstOrDefaultAsync(r => r.RetireeID == dto.RetireeID && !r.IsDeleted);

        if (retiree == null)
            return ApiResponse<bool>.Fail("المتقاعد غير موجود");

        if (retiree.ApprovalItem == null || retiree.ApprovalItem.IsDeleted || !retiree.ApprovalItem.IsProcessed)
            return ApiResponse<bool>.Fail("المتقاعد لم تُستكمل معالجته في كشف الإحالة");

        if (item.RetireeID.HasValue && item.RetireeID.Value != retiree.RetireeID)
            return ApiResponse<bool>.Fail("القيد مرتبط بمتقاعد آخر");

        if (item.ProcessedRank == null)
            return ApiResponse<bool>.Fail("الرتبة المعالجة غير موجودة لهذا القيد");

        var decisionNumber = item.Decision?.DecisionNumber ?? "غير محدد";
        var effectiveDate = item.Decision?.DecisionDate ?? item.CreatedAt;
        var alreadyApplied = item.RetireeID == retiree.RetireeID &&
            await _unitOfWork.FinancialEntitlements.Query().AsNoTracking().AnyAsync(e =>
                e.RetireeID == retiree.RetireeID && !e.IsDeleted &&
                e.Amount == item.ProcessedRank.BaseSalary && e.EffectiveDate.Date == effectiveDate.Date &&
                e.Notes != null && e.Notes.Contains($"قرار رقم {decisionNumber}"));

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (!alreadyApplied)
            {
                var entitlementResult = await _financialEntitlementService.CreateAsync(new CreateFinancialEntitlementDto
                {
                    RetireeID = retiree.RetireeID,
                    Amount = item.ProcessedRank.BaseSalary,
                    EffectiveDate = effectiveDate,
                    Notes = $"تسوية رتبة — قرار رقم {decisionNumber}"
                }, userId);
                if (!entitlementResult.IsSuccess)
                {
                    if (ownsTransaction) await _unitOfWork.RollbackAsync();
                    return ApiResponse<bool>.Fail($"تعذر تحديث الاستحقاق المالي: {entitlementResult.Message}");
                }
            }

        item.RetireeID = retiree.RetireeID;
        if (!string.IsNullOrWhiteSpace(retiree.MilitaryNumber))
            item.MilitaryNumber = retiree.MilitaryNumber;

        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedByUserID = userId;
        _unitOfWork.SettlementItems.Update(item);

        if (item.BatchID > 0)
        {
            var batch = await _unitOfWork.SettlementsBatches.GetByIdAsync(item.BatchID);
            if (batch != null && !string.IsNullOrEmpty(batch.FailureReportJson))
            {
                var failures = System.Text.Json.JsonSerializer.Deserialize<List<FailureReportItem>>(batch.FailureReportJson);
                if (failures != null && failures.Count > 0)
                {
                    failures.RemoveAll(f => !string.IsNullOrEmpty(item.MilitaryNumber)
                        ? f.MilNum.Equals(item.MilitaryNumber, StringComparison.OrdinalIgnoreCase)
                        : !string.IsNullOrEmpty(item.FullName) && f.Name.Equals(item.FullName, StringComparison.OrdinalIgnoreCase));

                    batch.FailureReportJson = failures.Count > 0
                        ? System.Text.Json.JsonSerializer.Serialize(failures)
                        : null;
                    batch.SkippedCount = failures.Count;
                    _unitOfWork.SettlementsBatches.Update(batch);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        await ApplyRetireeRankUpdatesAsync(new List<RetireeRankUpdate>
        {
            new() { RetireeID = retiree.RetireeID, ProcessedRankID = item.ProcessedRankID, NewSalary = item.ProcessedRank.BaseSalary }
        });

        await _auditService.LogAsync(userId, userId.ToString(), "LinkSettlementItem", "SettlementItem",
            item.SettlementItemID, null, System.Text.Json.JsonSerializer.Serialize(new
            {
                item.RetireeID, item.ProcessedRankID, Amount = item.ProcessedRank.BaseSalary, EffectiveDate = effectiveDate
            }));

        if (ownsTransaction) await _unitOfWork.CommitAsync();
        }
        catch (Exception)
        {
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Fail("تعذر إكمال ربط المتقاعد وتحديث الاستحقاق المالي");
        }

        _cache.Remove("retiree_" + retiree.RetireeID);

        try
        {
            var linkFileIds = await _unitOfWork.RetirementFiles.Query()
                .Where(f => f.RetireeID == retiree.RetireeID && !f.IsDeleted)
                .Select(f => f.FileID)
                .ToListAsync();

            foreach (var linkFileId in linkFileIds)
            {
                _cache.Remove("retirement_file_" + linkFileId);
                try
                {
                    await _missingDocumentService.RefreshComplianceCacheAsync(linkFileId);
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
                }
                catch { /* اكتمل الحفظ؛ فشل تحديث الذاكرة أو الإشعار لا يلغيه */ }
            }
        }
        catch { /* اكتمل الحفظ؛ فشل قراءة الملفات لا يلغيه */ }

        return ApiResponse<bool>.Success(true, "تم الربط وتحديث الاستحقاق المالي والرتبة بنجاح");
    }

    public async Task<ApiResponse<List<SettlementItemDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var result = await _settlementRepository.GetByRetireeIdAsync(retireeId);
        return ApiResponse<List<SettlementItemDto>>.Success(result);
    }

    public async Task<byte[]> ExportDecisionToExcelAsync(int decisionId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        var decision = await _unitOfWork.SettlementDecisions.Query()
            .AsNoTracking()
            .Where(d => d.DecisionID == decisionId && !d.IsDeleted)
            .Select(d => new { d.DecisionNumber, d.DecisionDate, d.DecisionType })
            .FirstOrDefaultAsync();

        var items = await _settlementRepository.GetItemsByDecisionIdAsync(decisionId);

        var title = decision != null
            ? $"قرار تسوية رتب — رقم {decision.DecisionNumber} — {decision.DecisionDate:yyyy/MM/dd}"
            : "قرار تسوية رتب";
        var headers = new[] { "#", "الرقم العسكري", "الاسم الكامل", "الرتبة الحالية", "الرتبة المعالجة", "المعاش المعتمد", "حالة الربط", "ملاحظات" };
        var rows = items.Select((item, index) => new object?[]
        {
            index + 1, item.MilitaryNumber, item.FullName,
            item.CurrentRank?.RankName, item.ProcessedRank?.RankName,
            item.ProcessedRank?.BaseSalary ?? 0,
            item.RetireeID.HasValue ? "مطابق" : "غير مطابق", item.Notes
        }).ToArray();
        return _excelExporter.ExportRows("قرار التسوية", headers, rows, new ExcelReportOptions
        {
            Title = title,
            ReportId = decisionId.ToString(),
            HeaderRow = 3,
            FooterGap = 1,
            FooterLabelSpan = 3,
            FooterText = $"الإجمالي: {items.Count} بند",
            FooterValues = new Dictionary<int, decimal> { [6] = items.Sum(item => item.ProcessedRank?.BaseSalary ?? 0) },
            NumberFormats = new Dictionary<int, string> { [6] = "#,##0" }
        });
    }

    public async Task<byte[]> GenerateImportFailuresExcelAsync(IRemoteFile file, int decisionId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
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

        var headers = new[] { "رقم الصف", "الرقم العسكري", "الرتبة الحالية", "الاسم الكامل", "الرتبة المعالجة", "سبب الفشل" };
        var rows = new List<object?[]>();
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

                    rows.Add(new object?[] { row, militaryNumber, currentRankName, fullName, processedRankName, reason });
                }
            }
            catch (Exception ex)
            {
                rows.Add(new object?[] { row, null, null, null, null, $"خطأ تقني: {ex.Message}" });
            }
        }

        return _excelExporter.ExportRows("صفوف فاشلة", headers, rows,
            new ExcelReportOptions { IncludeFooter = false, ReportId = decisionId.ToString() });
    }

    public async Task<byte[]> ExportBatchFailureReportAsync(int decisionId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (!access.Scope.AllBranches) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        // Resolve batch ID from decision items
        var batchId = await _unitOfWork.SettlementItems.Query()
            .AsNoTracking()
            .Where(i => i.DecisionID == decisionId)
            .Select(i => i.BatchID)
            .FirstOrDefaultAsync();

        if (batchId == 0)
        {
            var decision = await _unitOfWork.SettlementDecisions.GetByIdAsync(decisionId);
            if (decision != null)
            {
                batchId = await _unitOfWork.SettlementsBatches.Query()
                    .AsNoTracking()
                    .Where(b => !b.IsDeleted && b.BatchNumber.StartsWith(decision.DecisionNumber))
                    .Select(b => b.BatchID)
                    .FirstOrDefaultAsync();
            }
        }

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

        var headers = new[] { "رقم الصف", "الرقم العسكري", "الاسم الكامل", "الرتبة الحالية", "الرتبة المعالجة", "سبب الفشل" };
        var rows = failures.Select(f => new object?[]
            { f.Row, f.MilNum, f.Name, f.CurRank, f.ProcRank, f.Reason }).ToArray();
        return _excelExporter.ExportRows("صفوف فاشلة", headers, rows,
            new ExcelReportOptions { IncludeFooter = false, ReportId = decisionId.ToString() });
    }

    // ═══════════════════════════════════════════════════════
    // دوال خاصة مُحسَّنة (Helpers)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// هلCASE الكشف مغلق مالياً ومحاسبياً؟ يعتمد BatchStatusConstants.IsApproved، ويفحص
    /// Completed صراحة لأنها ليست ضمن IsApproved (Completed = "مكتمل/معتمد").
    /// </summary>
    private static bool IsBatchFinanciallyLocked(string? status) =>
        BatchStatusConstants.IsApproved(status)
        || status == BatchStatusConstants.Completed;

    /// <summary>
    /// إعادة اشتقاق إجماليات الكشف من البنود غير المحذوفة — مصدر الحقيقة الوحيد.
    /// يمنع الانحراف عن تطبيق += أو -= (إعادة محاولة استيراد، حذف بند، حذف كشف).
    /// </summary>
    private async Task RecalculateBatchTotalsAsync(SettlementsBatch batch)
    {
        var items = _unitOfWork.SettlementItems.Query()
            .AsNoTracking()
            .Where(i => i.BatchID == batch.BatchID);

        batch.TotalCount = await items.CountAsync();

        batch.TotalAmount = await items
            .SumAsync(i => (decimal?)i.ProcessedRank!.BaseSalary) ?? 0m;

        _unitOfWork.SettlementsBatches.Update(batch);
    }

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
