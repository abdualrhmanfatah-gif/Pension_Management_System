using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using System.Drawing;

namespace PensionMS.Infrastructure.Services;

public class SalaryPaymentBatchService : ISalaryPaymentBatchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayrollGenerationService _payrollGenerationService;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ISoDGuard _soDGuard;
    private readonly INotificationService _notificationService;
    private readonly IJournalEntryService _journalEntry;
    private readonly ILogger<SalaryPaymentBatchService> _logger;

    public SalaryPaymentBatchService(
        IUnitOfWork unitOfWork,
        IPayrollGenerationService payrollGenerationService,
        IWorkflowEngine workflowEngine,
        ISoDGuard soDGuard,
        INotificationService notificationService,
        IJournalEntryService journalEntry,
        ILogger<SalaryPaymentBatchService> logger)
    {
        _unitOfWork = unitOfWork;
        _payrollGenerationService = payrollGenerationService;
        _workflowEngine = workflowEngine;
        _soDGuard = soDGuard;
        _notificationService = notificationService;
        _journalEntry = journalEntry;
        _logger = logger;
    }

    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ
    // BATCH CRUD
    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ

    public async Task<ApiResponse<PagedResult<SalaryBatchListDto>>> GetAllAsync(
        int page, int pageSize, string? status = null)
    {
        var query = _unitOfWork.SalaryPaymentBatches.Query()
            .AsNoTracking()
            .Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.BatchMonth)
            .ThenByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new SalaryBatchListDto
            {
                BatchID = b.BatchID,
                BankName = b.Bank.BankName,
                BatchNumber = b.BatchNumber,
                BatchMonth = b.BatchMonth,
                TotalCount = b.TotalCount,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<SalaryBatchListDto>>.Success(new PagedResult<SalaryBatchListDto>
        {
            Items = items, TotalCount = total, Page = page, PageSize = pageSize
        });
    }

    public async Task<ApiResponse<SalaryBatchDto>> GetByIdAsync(int id)
    {
        var batch = await _unitOfWork.SalaryPaymentBatches.Query()
            .AsNoTracking()
            .Include(b => b.Bank)
            .Include(b => b.ImportedByUser)
            .Include(b => b.ApprovedByUser)
            .FirstOrDefaultAsync(b => b.BatchID == id && !b.IsDeleted);

        if (batch == null)
            return ApiResponse<SalaryBatchDto>.Fail("كشف الصرف غير موجود");

        return ApiResponse<SalaryBatchDto>.Success(MapToDto(batch));
    }

    public async Task<ApiResponse<SalaryBatchDto>> CreateAsync(CreateSalaryBatchDto dto, int userId)
    {
        // Validate bank
        var bank = await _unitOfWork.Banks.GetByIdAsync(dto.BankID);
        if (bank == null)
            return ApiResponse<SalaryBatchDto>.Fail("البنك غير موجود");

        var batch = new SalaryPaymentBatch
        {
            BankID = dto.BankID,
            BatchMonth = dto.BatchMonth,
            BatchNumber = dto.BatchNumber,
            ImportedByUserID = userId,
            ReceivedDate = DateTime.UtcNow,
            Status = BatchStatusConstants.Draft,
            Notes = dto.Notes,
            TotalCount = 0,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SalaryPaymentBatches.AddAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        await _workflowEngine.CreateSalaryBatchApprovalTaskAsync(batch.BatchID, batch.BatchMonth);

        try
        {
            await _notificationService.NotifyRoleAsync("Accountant", new CreateNotificationDto
            {
                UserID = 0,
                Title = "كشف صرف جديد",
                Message = $"تم إنشاء كشف صرف جديد رقم ({batch.BatchNumber}) لشهر {batch.BatchMonth:yyyy/MM} بانتظار الاعتماد.",
                Type = NotificationConstants.Types.Info,
                Category = NotificationConstants.Categories.Financial,
                Priority = NotificationConstants.Priorities.Normal,
                ActorID = userId,
                RelatedRecordID = batch.BatchID
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "فشل إرسال إشعار إنشاء كشف الصرف {BatchId}", batch.BatchID);
        }

        // Reload for DTO
        var created = await _unitOfWork.SalaryPaymentBatches.Query()
            .Include(b => b.Bank)
            .Include(b => b.ImportedByUser)
            .FirstAsync(b => b.BatchID == batch.BatchID);

        return ApiResponse<SalaryBatchDto>.Success(MapToDto(created), "تم إنشاء كشف الصرف بنجاح");
    }

    public async Task<ApiResponse<bool>> ApproveAsync(int batchId, int userId)
    {
        var batch = await _unitOfWork.SalaryPaymentBatches.GetByIdAsync(batchId);
        if (batch == null || batch.IsDeleted)
            return ApiResponse<bool>.Fail("كشف الصرف غير موجود");

        if (!BatchStatusConstants.IsDraft(batch.Status))
            return ApiResponse<bool>.Fail("لا يمكن اعتماد كشف غير مسودة");

        if (batch.TotalCount == 0)
            return ApiResponse<bool>.Fail("لا يمكن اعتماد كشف فارغ");

        var sodResult = await _soDGuard.ValidateSalaryBatchApprovalAsync(batchId, userId);
        if (!sodResult.IsAllowed)
            return ApiResponse<bool>.Fail(sodResult.Reason ?? "غير مسموح باعتماد هذا الكشف");

        await _unitOfWork.BeginTransactionAsync();

        batch.Status = BatchStatusConstants.Approved;
        batch.ApprovedByUserID = userId;
        batch.ApprovedAt = DateTime.UtcNow;
        _unitOfWork.SalaryPaymentBatches.Update(batch);

        // Accounting: pension disbursement entry
        var periodId = await _journalEntry.GetPeriodIdForDateAsync(batch.ApprovedAt.Value);
        var accResult = await _journalEntry.CreateIntegrationEntryAsync(new FinancialOperationRequestDto
        {
            OperationType = FinancialOperationType.PensionDisbursement,
            Amount = batch.TotalAmount,
            PeriodID = periodId,
            OperationDate = batch.ApprovedAt.Value,
            Description = $"صرف معاشات تقاعدية — كشف رقم {batch.BatchNumber} — شهر {batch.BatchMonth:yyyy/MM}",
            SourceReferenceID = batch.BatchID
        }, userId);

        if (!accResult.IsSuccess)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Fail(accResult.Message);
        }

        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();

        await _workflowEngine.ResolveSalaryBatchApprovalTasksAsync(batchId, userId);

        try
        {
            await _notificationService.NotifyRoleAsync("Accountant", new CreateNotificationDto
            {
                UserID = 0,
                Title = "اعتماد كشف صرف",
                Message = $"تم اعتماد كشف صرف المعاشات رقم ({batch.BatchNumber}) لشهر {batch.BatchMonth:yyyy/MM} بنجاح وهو جاهز للصرف.",
                Type = NotificationConstants.Types.Success,
                Category = NotificationConstants.Categories.Financial,
                Priority = NotificationConstants.Priorities.High,
                ActorID = userId,
                RelatedRecordID = batch.BatchID
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "فشل إرسال إشعار اعتماد كشف الصرف {BatchId}", batch.BatchID);
        }

        return ApiResponse<bool>.Success(true, "تم اعتماد كشف الصرف");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int batchId, int userId)
    {
        var batch = await _unitOfWork.SalaryPaymentBatches.GetByIdAsync(batchId);
        if (batch == null || batch.IsDeleted)
            return ApiResponse<bool>.Fail("كشف الصرف غير موجود");

        if (!BatchStatusConstants.IsDraft(batch.Status))
            return ApiResponse<bool>.Fail("لا يمكن حذف كشف معتمد");

        batch.IsDeleted = true;
        batch.DeletedAt = DateTime.UtcNow;
        batch.DeletedByUserID = userId;
        _unitOfWork.SalaryPaymentBatches.Update(batch);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف كشف الصرف");
    }

    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ
    // ITEM GENERATION â€” ط§ظ„ظ…ظ†ط·ظ‚ ط§ظ„ط£ط³ط§ط³ظٹ
    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ

    public async Task<ApiResponse<GenerateItemsResultDto>> GenerateItemsAsync(int batchId, int userId)
    {
        var batch = await _unitOfWork.SalaryPaymentBatches.GetByIdAsync(batchId);
        if (batch == null || batch.IsDeleted)
            return ApiResponse<GenerateItemsResultDto>.Fail("كشف الصرف غير موجود");

        if (!BatchStatusConstants.IsDraft(batch.Status))
            return ApiResponse<GenerateItemsResultDto>.Fail("لا يمكن توليد عناصر لكشف معتمد");

        // 1. Soft-delete existing items (if re-running generation)
        var existingItems = await _unitOfWork.SalaryPaymentItems.Query()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .ToListAsync();
        foreach (var item in existingItems)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedByUserID = userId;
            _unitOfWork.SalaryPaymentItems.Update(item);
        }

        // 2. Delegate eligibility and item creation to payroll generation service
        var now = DateTime.UtcNow;
        var genResult = await _payrollGenerationService.GenerateForBatchAsync(batch, now, userId);

        // 3. Persist generated items and update batch totals
        foreach (var item in genResult.GeneratedItems)
            await _unitOfWork.SalaryPaymentItems.AddAsync(item);

        batch.TotalCount = genResult.GeneratedItems.Count;
        batch.TotalAmount = genResult.GeneratedItems.Sum(i => i.Amount);
        _unitOfWork.SalaryPaymentBatches.Update(batch);

        await _unitOfWork.SaveChangesAsync();

        var result = new GenerateItemsResultDto
        {
            GeneratedCount = genResult.GeneratedItems.Count,
            SkippedCount = genResult.SkippedItems.Count,
            TotalAmount = batch.TotalAmount,
            SkippedItems = genResult.SkippedItems
        };

        return ApiResponse<GenerateItemsResultDto>.Success(result,
            $"تم توليد {result.GeneratedCount} عنصر صرف بإجمالي {result.TotalAmount:N0}");
    }

    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ
    // ITEMS
    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ

    public async Task<ApiResponse<List<SalaryPaymentItemDto>>> GetItemsByBatchIdAsync(int batchId)
    {
        var items = await _unitOfWork.SalaryPaymentItems.Query()
            .AsNoTracking()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .Include(i => i.Retiree)
            .Include(i => i.RetirementFile)
                .ThenInclude(f => f.PensionSuspensions)
            .Include(i => i.Status)
            .Include(i => i.PaymentBook)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.Agent)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.Guardian)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.EntitledPerson)
            .OrderBy(i => i.RetirementFile.FileNumber)
            .Select(i => MapItemToDto(i))
            .ToListAsync();

        return ApiResponse<List<SalaryPaymentItemDto>>.Success(items);
    }

    public async Task<ApiResponse<List<SalaryPaymentItemDto>>> GetItemsByFileIdAsync(int fileId)
    {
        var items = await _unitOfWork.SalaryPaymentItems.Query()
            .AsNoTracking()
            .Where(i => i.FileID == fileId && !i.IsDeleted)
            .Include(i => i.Retiree)
            .Include(i => i.RetirementFile)
                .ThenInclude(f => f.PensionSuspensions)
            .Include(i => i.Status)
            .Include(i => i.Batch)
            .Include(i => i.PaymentBook)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.Agent)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.Guardian)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.EntitledPerson)
            .OrderByDescending(i => i.Batch!.BatchMonth)
            .Select(i => MapItemToDto(i))
            .ToListAsync();

        return ApiResponse<List<SalaryPaymentItemDto>>.Success(items);
    }

    public async Task<ApiResponse<bool>> UpdateItemStatusAsync(int itemId, UpdateItemStatusDto dto)
    {
        var item = await _unitOfWork.SalaryPaymentItems.Query()
            .Include(i => i.Batch)
            .FirstOrDefaultAsync(i => i.PaymentItemID == itemId);
        if (item == null || item.IsDeleted)
            return ApiResponse<bool>.Fail("عنصر الصرف غير موجود");

        if (item.Batch == null)
            return ApiResponse<bool>.Fail("بيانات الدفعة غير مرتبطة بهذا العنصر");

        if (!BatchStatusConstants.IsApproved(item.Batch.Status))
            return ApiResponse<bool>.Fail("لا يمكن تحديث حالة عنصر الصرف قبل اعتماد كشف الصرف");

        item.StatusID = dto.StatusID;
        if (dto.TransactionNumber != null) item.TransactionNumber = dto.TransactionNumber;
        if (dto.SendDate.HasValue) item.SendDate = dto.SendDate;
        if (dto.ReceiveDate.HasValue) item.ReceiveDate = dto.ReceiveDate;
        if (dto.FailureReason != null) item.FailureReason = dto.FailureReason;

        _unitOfWork.SalaryPaymentItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث حالة عنصر الصرف");
    }

    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ
    // EXCEL EXPORT
    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ

    public async Task<byte[]> ExportBatchToExcelAsync(int batchId)
    {
        var batch = await _unitOfWork.SalaryPaymentBatches.Query()
            .AsNoTracking()
            .Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.BatchID == batchId && !b.IsDeleted);

        var items = await _unitOfWork.SalaryPaymentItems.Query()
            .AsNoTracking()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .Include(i => i.Retiree)
            .Include(i => i.RetirementFile)
                .ThenInclude(f => f.PensionSuspensions)
            .Include(i => i.Status)
            .Include(i => i.PaymentBook)
            .Include(i => i.Recipient)                // Snapshot of المستلم
                .ThenInclude(r => r.Agent)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.Guardian)
            .Include(i => i.Recipient)
                .ThenInclude(r => r.EntitledPerson)
            .OrderBy(i => i.RetirementFile.FileNumber)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("كشف الصرف");
        ws.View.RightToLeft = true;

        // ── Header info ──
        string bankName = batch?.Bank?.BankName ?? "غير محدد";
        DateTime batchMonth = batch?.BatchMonth ?? DateTime.MinValue;
        var title = batch != null
            ? $"كشف صرف المعاشات — {bankName} — {batchMonth:yyyy/MM}"
            : "كشف صرف المعاشات";
        ws.Cells[1, 1, 1, 12].Merge = true;
        ws.Cells[1, 1].Value = title;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;
        ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Row(1).Height = 32;

        // â”€â”€ Column headers â”€â”€
        var headers = new[] {
            "#",
            "رقم الملف",
            "اسم المتقاعد",
            "اسم المستلم",
            "الرقم الوطني للمستلم",
            "هاتف المستلم",
            "المبلغ",
            "الحالة",
            "رقم الحوالة",
            "تاريخ الإرسال",
            "تاريخ الاستلام",
            "الفرع المرسل",
            "الفرع المستلم",
            "العملة",
            "المرسل",
            "حالة المعاش",
            "دفتر الصرف",
            "نوع المستلم",
            "ملاحظات"
        };
        for (int i = 0; i < headers.Length; i++)
            ws.Cells[3, i + 1].Value = headers[i];

        // Style header row
        using (var range = ws.Cells[3, 1, 3, headers.Length])
        {
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 11;
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125));
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
        }
        ws.Row(3).Height = 26;

        // â”€â”€ Data rows â”€â”€
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 4;

            // بيانات المستلم (متقاعد / وكيل / وصي / مستحق)
            var sr = item.Recipient;
            string recipientName;
            string recipientNationalId;
            string recipientPhone;

            if (sr != null && (sr.AgentID.HasValue || sr.Agent != null))
            {
                var a = sr.Agent;
                recipientName = a?.FullName ?? string.Empty;
                recipientNationalId = a?.NationalID ?? string.Empty;
                recipientPhone = a?.Phone ?? string.Empty;
            }
            else if (sr != null && (sr.GuardianID.HasValue || sr.Guardian != null))
            {
                var g = sr.Guardian;
                recipientName = g?.FullName ?? string.Empty;
                recipientNationalId = g?.NationalID ?? string.Empty;
                recipientPhone = g?.Phone ?? string.Empty;
            }
            else if (sr != null && (sr.EntitledID.HasValue || sr.EntitledPerson != null))
            {
                var e = sr.EntitledPerson;
                recipientName = e?.FullName ?? string.Empty;
                recipientNationalId = e?.NationalID ?? string.Empty;
                recipientPhone = e?.Phone ?? string.Empty;
            }
            else
            {
                // المستلم هو المتقاعد نفسه أو غير محدد
                var r = item.Retiree;
                recipientName = r?.FullName ?? string.Empty;
                recipientNationalId = r?.NationalID ?? string.Empty;
                recipientPhone = r?.Phone ?? string.Empty;
            }

            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = item.RetirementFile?.FileNumber;
            ws.Cells[row, 3].Value = item.Retiree?.FullName;
            ws.Cells[row, 4].Value = recipientName;
            ws.Cells[row, 5].Value = recipientNationalId;
            ws.Cells[row, 6].Value = recipientPhone;
            ws.Cells[row, 7].Value = item.Amount;
            ws.Cells[row, 7].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 8].Value = item.Status?.StatusName;
            ws.Cells[row, 9].Value = item.TransactionNumber;
            ws.Cells[row, 10].Value = item.SendDate?.ToString("yyyy/MM/dd");
            ws.Cells[row, 11].Value = item.ReceiveDate?.ToString("yyyy/MM/dd");
            ws.Cells[row, 12].Value = item.SenderBranch;
            ws.Cells[row, 13].Value = item.ReceiverBranch;
            ws.Cells[row, 14].Value = item.Currency;
            ws.Cells[row, 15].Value = item.Sender;
            
            var hasSuspension = item.RetirementFile?.PensionSuspensions.Any(s => s.Status == SuspensionStatusConstants.CodeActive && !s.IsDeleted) ?? false;
            ws.Cells[row, 16].Value = hasSuspension ? "موقوف" : "نشط";

            ws.Cells[row, 17].Value = item.PaymentBook?.BookNumber;
            ws.Cells[row, 18].Value = item.Recipient?.RecipientType;
            ws.Cells[row, 19].Value = item.FailureReason;
        }

        // â”€â”€ Footer â”€â”€
        var footerRow = items.Count + 5;
        ws.Cells[footerRow, 1, footerRow, 3].Merge = true;
        ws.Cells[footerRow, 1].Value = $"الإجمالي: {items.Count} مستفيد";
        ws.Cells[footerRow, 1].Style.Font.Bold = true;
        ws.Cells[footerRow, 7].Value = items.Sum(i => i.Amount);
        ws.Cells[footerRow, 7].Style.Numberformat.Format = "#,##0";
        ws.Cells[footerRow, 7].Style.Font.Bold = true;

        // Auto-fit
        for (int i = 1; i <= headers.Length; i++)
        {
            ws.Column(i).AutoFit();
            if (ws.Column(i).Width < 12) ws.Column(i).Width = 12;
        }

        return package.GetAsByteArray();
    }

    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ
    // EXCEL IMPORT (Transaction Results)
    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ

    public async Task<ApiResponse<ImportResultDto>> ImportTransactionResultsAsync(int batchId, Stream excelStream)
    {
        var batch = await _unitOfWork.SalaryPaymentBatches.GetByIdAsync(batchId);
        if (batch == null || batch.IsDeleted)
            return ApiResponse<ImportResultDto>.Fail("كشف الصرف غير موجود");

        if (!BatchStatusConstants.IsApproved(batch.Status))
            return ApiResponse<ImportResultDto>.Fail("لا يمكن استيراد نتائج الحوالات إلا بعد اعتماد كشف الصرف");

        var result = new ImportResultDto();

        using var package = new ExcelPackage(excelStream);
        var ws = package.Workbook.Worksheets.FirstOrDefault();
        if (ws == null)
            return ApiResponse<ImportResultDto>.Fail("ملف Excel فارغ");

        // Load all items for this batch
        var batchItems = await _unitOfWork.SalaryPaymentItems.Query()
            .Where(i => i.BatchID == batchId && !i.IsDeleted)
            .Include(i => i.RetirementFile)
            .ToListAsync();

        var statuses = await _unitOfWork.TransactionStatuses.Query()
            .AsNoTracking()
            .ToListAsync();

        // Two supported formats:
        // 1) "تقرير مبسط" للاستيراد: FileNumber | TransactionNumber | StatusName | FailureReason | SendDate | ReceiveDate | SenderBranch | ReceiverBranch | Currency | Sender (from row 2)
        // 2) نفس ملف التصدير من النظام بعد تعبئة الحقول (Export layout):
        //    # | FileNumber | RetireeName | RecipientName | NationalID | Phone | Amount | StatusName | TransactionNumber | SendDate | ReceiveDate | SenderBranch | ReceiverBranch | Currency | Sender | SalaryStatus | BookNumber | RecipientType | FailureReason

        var endRow = ws.Dimension?.End.Row ?? 0;

        // نحاول كشف ما إذا كان الملف هو نسخة من ملف التصدير
        var headerFileNumberExport = ws.Cells[3, 2].Text?.Trim();
        var isExportLayout = headerFileNumberExport == ExportLayoutConstants.FileNumber;

        int startRow = isExportLayout ? 4 : 2;

        for (int row = startRow; row <= endRow; row++)
        {
            string? fileNumber;
            string? transactionNumber;
            string? statusName;
            string? failureReason;
            string? sendDateText = null;
            string? receiveDateText = null;
            string? senderBranch = null;
            string? receiverBranch = null;
            string? currency = null;
            string? sender = null;

            if (isExportLayout)
            {
                // نفس تنسيق التصدير
                fileNumber = ws.Cells[row, 2].Text?.Trim();   // رقم الملف
                statusName = ws.Cells[row, 8].Text?.Trim();   // الحالة
                transactionNumber = ws.Cells[row, 9].Text?.Trim(); // رقم الحوالة
                sendDateText = ws.Cells[row, 10].Text?.Trim();
                receiveDateText = ws.Cells[row, 11].Text?.Trim();
                senderBranch = ws.Cells[row, 12].Text?.Trim();
                receiverBranch = ws.Cells[row, 13].Text?.Trim();
                currency = ws.Cells[row, 14].Text?.Trim();
                sender = ws.Cells[row, 15].Text?.Trim();
                failureReason = ws.Cells[row, 19].Text?.Trim();    // ملاحظات / سبب الفشل
            }
            else
            {
                // التنسيق المبسط
                fileNumber = ws.Cells[row, 1].Text?.Trim();
                transactionNumber = ws.Cells[row, 2].Text?.Trim();
                statusName = ws.Cells[row, 3].Text?.Trim();
                failureReason = ws.Cells[row, 4].Text?.Trim();
                // Optionally extract from simplified if present but typically simpler
                sendDateText = ws.Cells[row, 5].Text?.Trim();
                receiveDateText = ws.Cells[row, 6].Text?.Trim();
                senderBranch = ws.Cells[row, 7].Text?.Trim();
                receiverBranch = ws.Cells[row, 8].Text?.Trim();
                currency = ws.Cells[row, 9].Text?.Trim();
                sender = ws.Cells[row, 10].Text?.Trim();
            }

            if (string.IsNullOrWhiteSpace(fileNumber)) continue;
            if (fileNumber.StartsWith("الإجمالي")) continue;

            var item = batchItems.FirstOrDefault(i => i.RetirementFile?.FileNumber == fileNumber);
            if (item == null || item.RetirementFile == null)
            {
                result.Errors.Add($"سطر {row}: رقم الملف {fileNumber} غير موجود في الكشف");
                result.SkippedCount++;
                continue;
            }

            // Match status (Fuzzy matching)
            if (!string.IsNullOrWhiteSpace(statusName))
            {
                var normalizedInput = statusName.Replace("إ", "ا").Replace("أ", "ا").Replace("آ", "ا").Replace("ى", "ي").Replace("ة", "ه").Trim();
                var status = statuses.FirstOrDefault(s => 
                    s.StatusName.Replace("إ", "ا").Replace("أ", "ا").Replace("آ", "ا").Replace("ى", "ي").Replace("ة", "ه").Trim() == normalizedInput);
                
                if (status != null)
                {
                    item.StatusID = status.StatusID;
                }
                else
                {
                    result.Errors.Add($"سطر {row}: حالة غير معروفة '{statusName}' للملف {fileNumber}");
                }
            }

            if (!string.IsNullOrWhiteSpace(transactionNumber))
                item.TransactionNumber = transactionNumber;

            if (!string.IsNullOrWhiteSpace(failureReason))
                item.FailureReason = failureReason;

            if (!string.IsNullOrWhiteSpace(senderBranch)) item.SenderBranch = senderBranch;
            if (!string.IsNullOrWhiteSpace(receiverBranch)) item.ReceiverBranch = receiverBranch;
            if (!string.IsNullOrWhiteSpace(currency)) item.Currency = currency;
            if (!string.IsNullOrWhiteSpace(sender)) item.Sender = sender;

            if (DateTime.TryParse(sendDateText, out var sDate)) item.SendDate = sDate;
            if (DateTime.TryParse(receiveDateText, out var rDate)) item.ReceiveDate = rDate;

            _unitOfWork.SalaryPaymentItems.Update(item);
            result.UpdatedCount++;
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ImportResultDto>.Success(result,
            $"تم تحديث {result.UpdatedCount} عنصر — {result.SkippedCount} مستبعد");
    }

    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ
    // MAPPING
    // â•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گâ•گ

    private static SalaryBatchDto MapToDto(SalaryPaymentBatch b) => new()
    {
        BatchID = b.BatchID,
        BankID = b.BankID,
        BankName = b.Bank?.BankName,
        BatchNumber = b.BatchNumber,
        BatchMonth = b.BatchMonth,
        ReceivedDate = b.ReceivedDate,
        TotalCount = b.TotalCount,
        TotalAmount = b.TotalAmount,
        Status = b.Status,
        ApprovedAt = b.ApprovedAt,
        ApprovedByUserName = b.ApprovedByUser?.FullName,
        ImportedByUserName = b.ImportedByUser?.FullName,
        Notes = b.Notes,
        CreatedAt = b.CreatedAt
    };

    private static SalaryPaymentItemDto MapItemToDto(SalaryPaymentItem i)
    {
        // تحديد نوع المستلم اعتماداً على مفاتيح الربط (وليس النص المخزن)
        string recipientType;
        string recipientName;

        if (i.Recipient != null && (i.Recipient.AgentID.HasValue || i.Recipient.Agent != null))
        {
            recipientType = "Agent";
            recipientName = i.Recipient.Agent?.FullName ?? string.Empty;
        }
        else if (i.Recipient != null && (i.Recipient.GuardianID.HasValue || i.Recipient.Guardian != null))
        {
            recipientType = "Guardian";
            recipientName = i.Recipient.Guardian?.FullName ?? string.Empty;
        }
        else if (i.Recipient != null && (i.Recipient.EntitledID.HasValue || i.Recipient.EntitledPerson != null))
        {
            recipientType = "EntitledPerson";
            recipientName = i.Recipient.EntitledPerson?.FullName ?? string.Empty;
        }
        else
        {
            recipientType = "Retiree";
            recipientName = i.Retiree?.FullName ?? string.Empty;
        }

        return new SalaryPaymentItemDto
        {
            PaymentItemID = i.PaymentItemID,
            BatchID = i.BatchID,
            RetireeFullName = i.Retiree?.FullName,
            FileNumber = i.RetirementFile?.FileNumber,
            Amount = i.Amount,
            StatusName = i.Status?.StatusName,
            PaymentBookNumber = i.PaymentBook?.BookNumber,
            RecipientName = recipientName,
            RecipientType = recipientType,
            TransactionNumber = i.TransactionNumber,
            SendDate = i.SendDate,
            ReceiveDate = i.ReceiveDate,
            FailureReason = i.FailureReason,
            SenderBranch = i.SenderBranch,
            ReceiverBranch = i.ReceiverBranch,
            Currency = i.Currency,
            Sender = i.Sender,
            SalaryStatus = (i.RetirementFile?.PensionSuspensions.Any(s => s.Status == SuspensionStatusConstants.Active && !s.IsDeleted) ?? false) ? "موقوف" : "نشط"
        };
    }
}
