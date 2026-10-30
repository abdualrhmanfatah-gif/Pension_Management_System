<<<<<<< HEAD
using PensionMS.Infrastructure.Data;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

public class VoucherService : IVoucherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJournalEntryService _journalEntry;

    public VoucherService(IUnitOfWork unitOfWork, IJournalEntryService journalEntry)
    {
        _unitOfWork = unitOfWork;
        _journalEntry = journalEntry;
    }

    private async Task<int> GetJournalEntryTypeIDAsync(string code)
    {
        var entryType = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted);
        return entryType?.JournalEntryTypeID ?? 1;
    }

    // ═══════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════
    public async Task<ApiResponse<VoucherDto>> CreateVoucherAsync(CreateVoucherDto dto, int userId)
    {
        if (dto.Amount <= 0)
            return ApiResponse<VoucherDto>.Fail("المبلغ يجب أن أكبر من صفر");

        if (string.IsNullOrWhiteSpace(dto.Description))
            return ApiResponse<VoucherDto>.Fail("البيان مطلوب");

        // Resolve accounts: Payment → expense (مدين) / 1811 خزينة (دائن)
        //                  Receipt  → 1811 خزينة (مدين) / revenue (دائن)
        int? debitAccountId = dto.DebitAccountID;
        int creditAccountId = dto.CreditAccountID;

        // Treasury account 1811
        var treasuryAcc = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == "1811" && !a.IsDeleted);
        if (treasuryAcc == null)
            return ApiResponse<VoucherDto>.Fail("حساب الخزينة الرئيسية (1811) غير موجود");

        if (dto.VoucherType == VoucherType.Payment)
        {
            // Debit: حساب المصروف (يختاره المستخدم)
            if (debitAccountId <= 0)
                return ApiResponse<VoucherDto>.Fail("اختر حساب المصروف");
            // Credit: 1811 الخزينة الرئيسية
            creditAccountId = treasuryAcc.AccountID;
        }
        else // Receipt
        {
            // Debit: 1811 الخزينة الرئيسية
            debitAccountId = treasuryAcc.AccountID;
            // Credit: حساب الإيراد (يختاره المستخدم)
            if (creditAccountId <= 0)
                return ApiResponse<VoucherDto>.Fail("اختر حساب الإيراد");
        }

        // Validate both accounts exist and are postable
        var debitAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountID == debitAccountId && !a.IsDeleted);
        if (debitAccount == null || !debitAccount.IsPostable)
            return ApiResponse<VoucherDto>.Fail("حساب المدين غير صالح");

        var creditAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountID == creditAccountId && !a.IsDeleted);
        if (creditAccount == null || !creditAccount.IsPostable)
            return ApiResponse<VoucherDto>.Fail("حساب الدائن غير صالح");

        // Validate party name based on type
        if (dto.VoucherType == VoucherType.Payment && string.IsNullOrWhiteSpace(dto.PayeeName))
            return ApiResponse<VoucherDto>.Fail("اسم المستفيد مطلوب لسند الصرف");
        if (dto.VoucherType == VoucherType.Receipt && string.IsNullOrWhiteSpace(dto.PayerName))
            return ApiResponse<VoucherDto>.Fail("اسم الدافع مطلوب لسند القبض");

        var voucherNumber = await GenerateVoucherNumberAsync(dto.VoucherType, dto.VoucherDate);

        var voucher = new Voucher
        {
            VoucherNumber = voucherNumber,
            VoucherType = dto.VoucherType,
            Status = VoucherStatus.Draft,
            VoucherDate = dto.VoucherDate,
            PayeeName = dto.PayeeName,
            PayerName = dto.PayerName,
            Amount = dto.Amount,
            Description = dto.Description,
            DebitAccountID = debitAccountId,
            CreditAccountID = creditAccountId,
            ChequeNumber = dto.ChequeNumber,
            ReferenceNumber = dto.ReferenceNumber,
            ReferralSource = dto.ReferralSource,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Vouchers.AddAsync(voucher);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<VoucherDto>.Success(
            await GetVoucherDtoAsync(voucher.VoucherID), "تم إنشاء السند بنجاح");
    }

    // ═══════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════
    public async Task<ApiResponse<VoucherDto>> GetVoucherByIdAsync(int id)
    {
        var dto = await GetVoucherDtoAsync(id);
        if (dto == null)
            return ApiResponse<VoucherDto>.Fail("السند غير موجود");
        return ApiResponse<VoucherDto>.Success(dto);
    }

    // ═══════════════════════════════════════
    // LIST
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<VoucherDto>>> GetVouchersAsync(VoucherFilterDto filter)
    {
        var query = _unitOfWork.Vouchers.Query()
            .Include(v => v.DebitAccount)
            .Include(v => v.CreditAccount)
            .Include(v => v.CreatedByUser)
            .Where(v => !v.IsDeleted);

        if (filter.VoucherType.HasValue)
            query = query.Where(v => v.VoucherType == filter.VoucherType.Value);
        if (filter.Status.HasValue)
            query = query.Where(v => v.Status == filter.Status.Value);
        if (filter.FromDate.HasValue)
            query = query.Where(v => v.VoucherDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(v => v.VoucherDate <= filter.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(v =>
                v.VoucherNumber.Contains(filter.Search) ||
                v.Description.Contains(filter.Search) ||
                (v.PayeeName != null && v.PayeeName.Contains(filter.Search)) ||
                (v.PayerName != null && v.PayerName.Contains(filter.Search)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.VoucherDate).ThenByDescending(v => v.VoucherID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(v => ToVoucherDto(v)).ToList();

        return ApiResponse<PagedResult<VoucherDto>>.Success(new PagedResult<VoucherDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // UPDATE (Draft only)
    // ═══════════════════════════════════════
    public async Task<ApiResponse<VoucherDto>> UpdateVoucherAsync(int id, UpdateVoucherDto dto)
    {
        var voucher = await _unitOfWork.Vouchers.Query()
            .FirstOrDefaultAsync(v => v.VoucherID == id && !v.IsDeleted);
        if (voucher == null)
            return ApiResponse<VoucherDto>.Fail("السند غير موجود");
        if (voucher.Status != VoucherStatus.Draft)
            return ApiResponse<VoucherDto>.Fail("لا يمكن تعديل سند مرحّل أو ملغي");

        if (dto.VoucherDate.HasValue) voucher.VoucherDate = dto.VoucherDate.Value;
        if (dto.PayeeName != null) voucher.PayeeName = dto.PayeeName;
        if (dto.PayerName != null) voucher.PayerName = dto.PayerName;
        if (dto.Amount.HasValue) voucher.Amount = dto.Amount.Value;
        if (dto.Description != null) voucher.Description = dto.Description;
        if (dto.DebitAccountID.HasValue) voucher.DebitAccountID = dto.DebitAccountID.Value;
        if (dto.CreditAccountID.HasValue) voucher.CreditAccountID = dto.CreditAccountID.Value;
        if (dto.ChequeNumber != null) voucher.ChequeNumber = dto.ChequeNumber;
        if (dto.ReferenceNumber != null) voucher.ReferenceNumber = dto.ReferenceNumber;
        if (dto.ReferralSource != null) voucher.ReferralSource = dto.ReferralSource;
        voucher.UpdatedAt = DateTime.UtcNow;

<<<<<<< HEAD
=======
        _unitOfWork.Vouchers.Update(voucher);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<VoucherDto>.Success(
            await GetVoucherDtoAsync(id), "تم تعديل السند بنجاح");
    }

    // ═══════════════════════════════════════
    // DELETE (Draft only)
    // ═══════════════════════════════════════
    public async Task<ApiResponse<bool>> DeleteVoucherAsync(int id, int userId)
    {
        var voucher = await _unitOfWork.Vouchers.Query()
            .FirstOrDefaultAsync(v => v.VoucherID == id && !v.IsDeleted);
        if (voucher == null)
            return ApiResponse<bool>.Fail("السند غير موجود");
        if (voucher.Status != VoucherStatus.Draft)
            return ApiResponse<bool>.Fail("لا يمكن حذف سند مرحّل أو ملغي");

        voucher.IsDeleted = true;
        voucher.DeletedAt = DateTime.UtcNow;
        voucher.DeletedByUserID = userId;

<<<<<<< HEAD
=======
        _unitOfWork.Vouchers.Update(voucher);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف السند بنجاح");
    }

    // ═══════════════════════════════════════
    // POST (Create + Post Journal Entry)
    // ═══════════════════════════════════════
<<<<<<< HEAD
    public Task<ApiResponse<VoucherDto>> PostVoucherAsync(int id, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => PostVoucherCoreAsync(id, userId));

    private async Task<ApiResponse<VoucherDto>> PostVoucherCoreAsync(int id, int userId)
=======
    public async Task<ApiResponse<VoucherDto>> PostVoucherAsync(int id, int userId)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    {
        var voucher = await _unitOfWork.Vouchers.Query()
            .Include(v => v.DebitAccount)
            .Include(v => v.CreditAccount)
            .FirstOrDefaultAsync(v => v.VoucherID == id && !v.IsDeleted);
        if (voucher == null)
            return ApiResponse<VoucherDto>.Fail("السند غير موجود");
        if (voucher.Status != VoucherStatus.Draft)
            return ApiResponse<VoucherDto>.Fail("السند مرحّل أو ملغي مسبقاً");
        if (voucher.DebitAccountID == null || voucher.DebitAccountID <= 0)
            return ApiResponse<VoucherDto>.Fail("يجب تحديد حساب المصروف قبل الترحيل");

        // Find the current fiscal period for voucher date
        var period = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.StartDate <= voucher.VoucherDate && p.EndDate >= voucher.VoucherDate && !p.IsDeleted)
            .FirstOrDefaultAsync();
        if (period == null)
            return ApiResponse<VoucherDto>.Fail("لا توجد فترة محاسبية مفتوحة لتاريخ السند");

        // Create journal entry
        var typeName = voucher.VoucherType == VoucherType.Payment ? "سند صرف" : "سند قبض";
        var partyName = voucher.VoucherType == VoucherType.Payment ? voucher.PayeeName : voucher.PayerName;
        var voucherTypeCode = voucher.VoucherType == VoucherType.Payment ? "PAY" : "REC";
        var voucherTypeId = await GetJournalEntryTypeIDAsync(voucherTypeCode);

        var jeResult = await _journalEntry.CreateJournalEntryAsync(new CreateJournalEntryDto
        {
            EntryDate = voucher.VoucherDate,
            PeriodID = period.PeriodID,
            JournalEntryTypeID = voucherTypeId,
            SourceReferenceID = voucher.VoucherID,
            Description = $"{typeName} رقم {voucher.VoucherNumber} - {partyName} - {voucher.Description}",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountID = voucher.DebitAccountID!.Value, Debit = voucher.Amount, Credit = 0, Description = $"{typeName}: {voucher.Description}" },
                new() { AccountID = voucher.CreditAccountID, Debit = 0, Credit = voucher.Amount, Description = $"{typeName}: {voucher.Description}" }
            }
        }, userId);

        if (!jeResult.IsSuccess)
            return ApiResponse<VoucherDto>.Fail($"فشل إنشاء القيد: {jeResult.Message}");

        // Post the journal entry
        var postResult = await _journalEntry.PostJournalEntryAsync(jeResult.Data.JournalEntryID, userId);
        if (!postResult.IsSuccess)
            return ApiResponse<VoucherDto>.Fail($"فشل ترحيل القيد: {postResult.Message}");

<<<<<<< HEAD
=======
        // تسجيل معاملة بنكية إذا كان أحد الحسابات بنكاً
        await CreateBankTransactionIfBankAccount(voucher, jeResult.Data.JournalEntryID);

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        // Update voucher
        voucher.Status = VoucherStatus.Posted;
        voucher.JournalEntryID = jeResult.Data.JournalEntryID;
        voucher.JournalEntryNumber = jeResult.Data.EntryNumber;
        voucher.UpdatedAt = DateTime.UtcNow;

<<<<<<< HEAD
=======
        _unitOfWork.Vouchers.Update(voucher);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<VoucherDto>.Success(
            await GetVoucherDtoAsync(id), "تم ترحيل السند وإنشاء القيد بنجاح");
    }

<<<<<<< HEAD
    public Task<ApiResponse<VoucherDto>> CancelVoucherAsync(int id, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => CancelVoucherCoreAsync(id, userId));

    private async Task<ApiResponse<VoucherDto>> CancelVoucherCoreAsync(int id, int userId)
=======
    private async Task CreateBankTransactionIfBankAccount(Voucher voucher, int journalEntryId)
    {
        // البحث عن البنك المرتبط بالحساب الدائن (سند قبض) أو المدين (سند صرف)
        int? bankId = null;
        int? accountId = null;

        if (voucher.VoucherType == VoucherType.Receipt)
        {
            // سند قبض: الحساب الدائن هو البنك
            accountId = voucher.CreditAccountID;
        }
        else
        {
            // سند صرف: الحساب المدين هو البنك
            accountId = voucher.DebitAccountID;
        }

        if (accountId.HasValue)
        {
            var bank = await _unitOfWork.Banks.Query()
                .FirstOrDefaultAsync(b => b.AccountID == accountId.Value && b.IsActive);
            if (bank != null)
            {
                bankId = bank.BankID;
            }
        }

        // إذا وجدنا بنكاً، نسجل المعاملة
        if (bankId.HasValue)
        {
            var transactionType = voucher.VoucherType == VoucherType.Payment ? "Withdrawal" : "Deposit";
            var bankTransaction = new Core.Entities.Accounting.BankTransaction
            {
                BankID = bankId.Value,
                JournalEntryID = journalEntryId,
                LineID = 0,
                TransactionDate = voucher.VoucherDate,
                TransactionType = transactionType,
                Amount = voucher.Amount,
                Description = voucher.Description,
                ReferenceNo = voucher.ChequeNumber,
                IsReconciled = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BankTransactions.AddAsync(bankTransaction);
        }
    }

    // ═══════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════
    public async Task<ApiResponse<VoucherDto>> CancelVoucherAsync(int id, int userId)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    {
        var voucher = await _unitOfWork.Vouchers.Query()
            .FirstOrDefaultAsync(v => v.VoucherID == id && !v.IsDeleted);
        if (voucher == null)
            return ApiResponse<VoucherDto>.Fail("السند غير موجود");
        if (voucher.Status == VoucherStatus.Cancelled)
            return ApiResponse<VoucherDto>.Fail("السند ملغي مسبقاً");

        // If posted, reverse the journal entry first
        if (voucher.Status == VoucherStatus.Posted && voucher.JournalEntryID.HasValue)
        {
            var reverseResult = await _journalEntry.ReverseJournalEntryAsync(
                voucher.JournalEntryID.Value, userId, $"إلغاء {voucher.VoucherNumber}");
            if (!reverseResult.IsSuccess)
                return ApiResponse<VoucherDto>.Fail($"فشل عكس القيد: {reverseResult.Message}");
        }

        voucher.Status = VoucherStatus.Cancelled;
        voucher.UpdatedAt = DateTime.UtcNow;

<<<<<<< HEAD
=======
        _unitOfWork.Vouchers.Update(voucher);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<VoucherDto>.Success(
            await GetVoucherDtoAsync(id), "تم إلغاء السند بنجاح");
    }

    // ═══════════════════════════════════════
    // NEXT NUMBER
    // ═══════════════════════════════════════
    public async Task<ApiResponse<string>> GetNextVoucherNumberAsync(VoucherType type, DateTime? date = null)
    {
        var number = await GenerateVoucherNumberAsync(type, date ?? DateTime.UtcNow);
        return ApiResponse<string>.Success(number);
    }

    // ═══════════════════════════════════════
    // IMPORT FROM EXCEL
    // ═══════════════════════════════════════
    public async Task<ApiResponse<ImportVoucherResultDto>> ImportVouchersAsync(Stream fileStream, string fileName, int userId)
    {
        if (fileStream == null || fileStream.Length == 0)
            return ApiResponse<ImportVoucherResultDto>.Fail("الملف فارغ");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
            return ApiResponse<ImportVoucherResultDto>.Fail("يجب رفع ملف Excel (.xlsx أو .xls)");

        // Treasury account 1811
        var treasuryAcc = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == "1811" && !a.IsDeleted);
        if (treasuryAcc == null)
            return ApiResponse<ImportVoucherResultDto>.Fail("حساب الخزينة الرئيسية (1811) غير موجود");

        var result = new ImportVoucherResultDto();
        var importedRefNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var stream = fileStream;
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                return ApiResponse<ImportVoucherResultDto>.Fail("الملف لا يحتوي على أوراق عمل");

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 2)
                return ApiResponse<ImportVoucherResultDto>.Fail("الملف فارغ (لا توجد بيانات)");

            result.TotalRows = rowCount - 1; // skip header

            // Check existing reference numbers in DB
            var existingRefNumbers = await _unitOfWork.Vouchers.Query()
                .Where(v => v.ReferenceNumber != null && !v.IsDeleted)
                .Select(v => v.ReferenceNumber!)
                .ToListAsync();
            var existingRefSet = new HashSet<string>(existingRefNumbers, StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                var rowNumber = row; // for error reporting

                try
                {
                    // Column A: الرقم (ReferenceNumber)
                    var excelNumber = worksheet.Cells[row, 1].Text?.Trim();

                    // Column B: التاريخ
                    var dateCell = worksheet.Cells[row, 2];
                    DateTime voucherDate;
                    if (dateCell.Value is DateTime dt)
                    {
                        voucherDate = dt;
                    }
                    else if (DateTime.TryParse(dateCell.Text, out var parsedDate))
                    {
                        voucherDate = parsedDate;
                    }
                    else
                    {
                        result.Errors.Add(new ImportErrorDto { RowNumber = rowNumber, Message = "التاريخ غير صحيح" });
                        result.SkipCount++;
                        continue;
                    }

                    // Column C: المبلغ
                    var amountText = worksheet.Cells[row, 3].Text?.Trim();
                    if (!decimal.TryParse(amountText, out var amount) || amount <= 0)
                    {
                        result.Errors.Add(new ImportErrorDto { RowNumber = rowNumber, Message = "المبلغ غير صحيح أو صفر" });
                        result.SkipCount++;
                        continue;
                    }

                    // Column D: جهة الاستلام (PayeeName)
                    var payeeName = worksheet.Cells[row, 4].Text?.Trim();

                    // Column E: جهة الأمر (ReferralSource)
                    var referralSource = worksheet.Cells[row, 5].Text?.Trim();

                    // Column F: البيان (Description)
                    var description = worksheet.Cells[row, 6].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        result.Errors.Add(new ImportErrorDto { RowNumber = rowNumber, Message = "البيان مطلوب" });
                        result.SkipCount++;
                        continue;
                    }

                    // Validate: amount > 0
                    if (amount <= 0)
                    {
                        result.Errors.Add(new ImportErrorDto { RowNumber = rowNumber, Message = "المبلغ يجب أن أكبر من صفر" });
                        result.SkipCount++;
                        continue;
                    }

                    // Check duplicate reference number in DB
                    if (!string.IsNullOrWhiteSpace(excelNumber))
                    {
                        if (existingRefSet.Contains(excelNumber))
                        {
                            result.Errors.Add(new ImportErrorDto { RowNumber = rowNumber, Message = $"الرقم '{excelNumber}' موجود مسبقاً في النظام" });
                            result.SkipCount++;
                            continue;
                        }
                        if (importedRefNumbers.Contains(excelNumber))
                        {
                            result.Errors.Add(new ImportErrorDto { RowNumber = rowNumber, Message = $"الرقم '{excelNumber}' مكرر في نفس الملف" });
                            result.SkipCount++;
                            continue;
                        }
                        importedRefNumbers.Add(excelNumber);
                    }

                    // Generate voucher number
                    var voucherNumber = await GenerateVoucherNumberAsync(VoucherType.Payment, voucherDate);

                    var voucher = new Voucher
                    {
                        VoucherNumber = voucherNumber,
                        VoucherType = VoucherType.Payment,
                        Status = VoucherStatus.Draft,
                        VoucherDate = voucherDate,
                        PayeeName = payeeName,
                        Amount = amount,
                        Description = description,
                        DebitAccountID = null, // فارغ — المستخدم يعيّنه لاحقاً
                        CreditAccountID = treasuryAcc.AccountID,
                        ReferenceNumber = excelNumber,
                        ReferralSource = referralSource,
                        CreatedByUserID = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Vouchers.AddAsync(voucher);
                    await _unitOfWork.SaveChangesAsync();

                    result.CreatedVoucherIds.Add(voucher.VoucherID);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    var innerMsg = ex.InnerException?.Message ?? ex.Message;
                    result.Errors.Add(new ImportErrorDto { RowNumber = rowNumber, Message = $"خطأ: {innerMsg}" });
                    result.SkipCount++;
                }
            }

            return ApiResponse<ImportVoucherResultDto>.Success(result,
                $"تم استيراد {result.SuccessCount} سند من أصل {result.TotalRows}");
        }
        catch (Exception ex)
        {
            return ApiResponse<ImportVoucherResultDto>.Fail($"خطأ في قراءة الملف: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════
    private async Task<string> GenerateVoucherNumberAsync(VoucherType type, DateTime date)
    {
        var prefix = type == VoucherType.Payment ? "PV" : "RV";
        var year = date.Year;
        var count = await _unitOfWork.Vouchers.Query()
            .CountAsync(v => v.VoucherType == type && v.VoucherDate.Year == year && !v.IsDeleted) + 1;
        return $"{prefix}-{year}-{count:D6}";
    }

    private async Task<VoucherDto?> GetVoucherDtoAsync(int id)
    {
        var voucher = await _unitOfWork.Vouchers.Query()
            .Include(v => v.DebitAccount)
            .Include(v => v.CreditAccount)
            .Include(v => v.CreatedByUser)
            .FirstOrDefaultAsync(v => v.VoucherID == id && !v.IsDeleted);
        if (voucher == null) return null;
        return ToVoucherDto(voucher);
    }

    private VoucherDto ToVoucherDto(Voucher v)
    {
        return new VoucherDto
        {
            VoucherID = v.VoucherID,
            VoucherNumber = v.VoucherNumber,
            VoucherType = v.VoucherType,
            Status = v.Status,
            VoucherDate = v.VoucherDate,
            PayeeName = v.PayeeName,
            PayerName = v.PayerName,
            Amount = v.Amount,
            Description = v.Description,
            DebitAccountID = v.DebitAccountID,
            DebitAccountCode = v.DebitAccount?.AccountCode ?? "",
            DebitAccountName = v.DebitAccount?.AccountName ?? "",
            CreditAccountID = v.CreditAccountID,
            CreditAccountCode = v.CreditAccount?.AccountCode ?? "",
            CreditAccountName = v.CreditAccount?.AccountName ?? "",
            ChequeNumber = v.ChequeNumber,
            ReferenceNumber = v.ReferenceNumber,
            ReferralSource = v.ReferralSource,
            JournalEntryID = v.JournalEntryID,
            JournalEntryNumber = v.JournalEntryNumber,
            CreatedByUserName = v.CreatedByUser?.FullName ?? "",
            CreatedAt = v.CreatedAt
        };
    }
}
