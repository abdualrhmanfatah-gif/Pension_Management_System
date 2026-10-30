using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using System.Collections.Generic;

namespace PensionMS.Infrastructure.Services.Accounting;

public class JournalEntryService : IJournalEntryService
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly SemaphoreSlim _entryNumberLock = new(1, 1);

    public JournalEntryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(CreateJournalEntryDto dto, int userId)
    {
        if (dto.Lines == null || dto.Lines.Count < 2)
            return ApiResponse<JournalEntryDto>.Fail("يجب أن يحتوي القيد على بندين على الأقل");

        var totalDebit = dto.Lines.Sum(l => l.Debit);
        var totalCredit = dto.Lines.Sum(l => l.Credit);
        if (totalDebit != totalCredit)
            return ApiResponse<JournalEntryDto>.Fail("مجموع المدين لا يساوي مجموع الدائن");

        if (totalDebit == 0 && totalCredit == 0)
            return ApiResponse<JournalEntryDto>.Fail("لا يمكن إنشاء قيد بقيمة صفر");

        var period = await _unitOfWork.FiscalPeriods.GetByIdAsync(dto.PeriodID);
        if (period == null || period.IsDeleted)
            return ApiResponse<JournalEntryDto>.Fail("الفترة المحاسبية غير موجودة");

        // If JournalEntryTypeID not provided, default to "GEN" (General Journal)
        if (dto.JournalEntryTypeID == 0)
        {
            var genType = await _unitOfWork.JournalEntryTypes.Query()
                .FirstOrDefaultAsync(t => t.Code == "GEN" && !t.IsDeleted);
            if (genType != null)
                dto.JournalEntryTypeID = genType.JournalEntryTypeID;
        }

        var entryType = await _unitOfWork.JournalEntryTypes.GetByIdAsync(dto.JournalEntryTypeID);
        if (entryType == null || entryType.IsDeleted)
            return ApiResponse<JournalEntryDto>.Fail("نوع القيد غير موجود");

        if (period.IsClosed && entryType.Code != "CLS")
            return ApiResponse<JournalEntryDto>.Fail("الفترة المحاسبية مغلقة ولا يمكن الترحيل إليها");

        var accountIds = dto.Lines.Select(l => l.AccountID).Distinct().ToList();
        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .Where(a => accountIds.Contains(a.AccountID) && !a.IsDeleted)
            .ToListAsync();

        if (accounts.Count != accountIds.Count)
            return ApiResponse<JournalEntryDto>.Fail("أحد الحسابات غير موجود أو محذوف");

        var inactiveAccounts = accounts.Where(a => !a.IsActive).Select(a => a.AccountCode).ToList();
        if (inactiveAccounts.Any())
            return ApiResponse<JournalEntryDto>.Fail($"الحسابات التالية غير نشطة: {string.Join(", ", inactiveAccounts)}");

        var nonPostableAccounts = accounts.Where(a => !a.IsPostable).Select(a => a.AccountCode).ToList();
        if (nonPostableAccounts.Any())
            return ApiResponse<JournalEntryDto>.Fail($"الحسابات التالية غير قابلة للترحيل (رئيسية): {string.Join(", ", nonPostableAccounts)}");

        var costCenterIds = dto.Lines.Where(l => l.CostCenterID.HasValue).Select(l => l.CostCenterID!.Value).Distinct().ToList();
        if (costCenterIds.Any())
        {
            var validCenters = await _unitOfWork.CostCenters.Query()
                .Where(c => costCenterIds.Contains(c.CostCenterID) && !c.IsDeleted && c.IsActive)
                .Select(c => c.CostCenterID)
                .ToListAsync();
            var invalidCenters = costCenterIds.Except(validCenters).ToList();
            if (invalidCenters.Any())
                return ApiResponse<JournalEntryDto>.Fail("أحد مراكز التكلفة غير موجود أو غير نشط");
        }

        var nestedTransaction = !_unitOfWork.HasActiveTransaction;
        if (nestedTransaction)
            await _unitOfWork.BeginTransactionAsync();
        try
        {
            var entryNumber = await GenerateEntryNumberAsync(dto.EntryDate);

            var entry = new JournalEntry
            {
                EntryNumber = entryNumber,
                EntryDate = dto.EntryDate,
                PeriodID = dto.PeriodID,
                JournalEntryTypeID = dto.JournalEntryTypeID,
                SourceReferenceID = dto.SourceReferenceID,
                Description = dto.Description,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Status = "Draft",
                CreatedByUserID = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.JournalEntries.AddAsync(entry);
            await _unitOfWork.SaveChangesAsync();

            foreach (var lineDto in dto.Lines)
            {
                var lineIndex = dto.Lines.IndexOf(lineDto);
                var line = new JournalEntryLine
                {
                    JournalEntryID = entry.JournalEntryID,
                    LineNumber = lineIndex + 1,
                    AccountID = lineDto.AccountID,
                    CostCenterID = lineDto.CostCenterID,
                    Debit = lineDto.Debit,
                    Credit = lineDto.Credit,
                    Description = lineDto.Description,
                    CheckNo = lineDto.CheckNo,
                    CheckDueDate = lineDto.CheckDueDate,
                    IsDeleted = false
                };
                await _unitOfWork.JournalEntryLines.AddAsync(line);
            }
            await _unitOfWork.SaveChangesAsync();
            if (nestedTransaction)
                await _unitOfWork.CommitAsync();

            return ApiResponse<JournalEntryDto>.Success(
                await GetJournalEntryDtoAsync(entry.JournalEntryID),
                "تم إنشاء القيد بنجاح");
        }
        catch
        {
            if (nestedTransaction)
                await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(int id)
    {
        var dto = await GetJournalEntryDtoAsync(id);
        if (dto == null)
            return ApiResponse<JournalEntryDto>.Fail("القيد غير موجود");
        return ApiResponse<JournalEntryDto>.Success(dto);
    }

    public async Task<ApiResponse<PagedResult<JournalEntryDto>>> GetJournalEntriesAsync(JournalEntryFilterDto filter)
    {
        var query = _unitOfWork.JournalEntries.Query().Where(e => !e.IsDeleted);

        if (filter.FromDate.HasValue) query = query.Where(e => e.EntryDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(e => e.EntryDate <= filter.ToDate.Value);
        if (filter.JournalEntryTypeID.HasValue) query = query.Where(e => e.JournalEntryTypeID == filter.JournalEntryTypeID.Value);
        if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(e => e.Status == filter.Status);
        if (filter.PeriodID.HasValue) query = query.Where(e => e.PeriodID == filter.PeriodID.Value);
        if (filter.CreatedByUserID.HasValue) query = query.Where(e => e.CreatedByUserID == filter.CreatedByUserID.Value);

        var total = await query.CountAsync();

        var items = await query
            .AsNoTracking()
            .OrderByDescending(e => e.EntryDate).ThenByDescending(e => e.JournalEntryID)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(e => e.JournalEntryType)
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .Include(e => e.Lines).ThenInclude(l => l.CostCenter)
            .Include(e => e.Period)
            .ToListAsync();

        var dtos = items.Select(e => ToJournalEntryDtoFromEntity(e)).ToList();

        return ApiResponse<PagedResult<JournalEntryDto>>.Success(new PagedResult<JournalEntryDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<ApiResponse<JournalEntryDto>> PostJournalEntryAsync(int id, int userId)
    {
        var entry = await _unitOfWork.JournalEntries.Query()
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.JournalEntryID == id && !e.IsDeleted);

        if (entry == null)
            return ApiResponse<JournalEntryDto>.Fail("القيد غير موجود");
        if (entry.Status != "Draft")
            return ApiResponse<JournalEntryDto>.Fail("يمكن ترحيل القيود المسودة فقط");

        var period = await _unitOfWork.FiscalPeriods.GetByIdAsync(entry.PeriodID);
        if (period == null || period.IsDeleted)
            return ApiResponse<JournalEntryDto>.Fail("الفترة المحاسبية غير موجودة");

        var entryType = await _unitOfWork.JournalEntryTypes.GetByIdAsync(entry.JournalEntryTypeID);
        if (period.IsClosed && (entryType == null || entryType.Code != "CLS"))
            return ApiResponse<JournalEntryDto>.Fail("الفترة المحاسبية مغلقة ولا يمكن الترحيل");

        var nestedTransaction = !_unitOfWork.HasActiveTransaction;
        if (nestedTransaction)
            await _unitOfWork.BeginTransactionAsync();
        try
        {
            entry.Status = "Posted";
            entry.PostedByUserID = userId;
            entry.PostedAt = DateTime.UtcNow;
<<<<<<< HEAD
=======
            _unitOfWork.JournalEntries.Update(entry);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            foreach (var line in entry.Lines)
            {
                await UpdateAccountBalanceAsync(entry.PeriodID, line.AccountID, line.Debit, line.Credit);

                // تسجيل معاملة بنكية إذا كان الحساب مرتبطاً ببنك
                await CreateBankTransactionIfBankAccount(entry, line);
            }
            await _unitOfWork.SaveChangesAsync();
            if (nestedTransaction)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (nestedTransaction)
                await _unitOfWork.RollbackAsync();
            throw;
        }

        return ApiResponse<JournalEntryDto>.Success(
            await GetJournalEntryDtoAsync(id), "تم ترحيل القيد بنجاح");
    }

    public async Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(int id, int userId, string reason)
    {
        var entry = await _unitOfWork.JournalEntries.Query()
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.JournalEntryID == id && !e.IsDeleted);

        if (entry == null)
            return ApiResponse<JournalEntryDto>.Fail("القيد غير موجود");
        if (entry.Status == "Reversed")
            return ApiResponse<JournalEntryDto>.Fail("تم عكس هذا القيد مسبقاً");
        if (entry.Status != "Posted")
            return ApiResponse<JournalEntryDto>.Fail("يمكن عكس القيود المرحلة فقط");

        var alreadyReversed = await _unitOfWork.JournalEntries.Query()
            .AnyAsync(e => e.ReversedEntryID == id && !e.IsDeleted);
        if (alreadyReversed)
            return ApiResponse<JournalEntryDto>.Fail("تم عكس هذا القيد مسبقاً");

        var currentPeriod = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow && !p.IsDeleted && !p.IsClosed)
            .FirstOrDefaultAsync();

        if (currentPeriod == null)
            return ApiResponse<JournalEntryDto>.Fail("لا توجد فترة محاسبية مفتوحة حالياً لإنشاء قيد العكس");

        JournalEntry reversal;
<<<<<<< HEAD
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
        try
        {
            entry.Status = "Reversed";
=======
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            entry.Status = "Reversed";
            _unitOfWork.JournalEntries.Update(entry);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            var reversalEntryNumber = await GenerateEntryNumberAsync(DateTime.UtcNow);
            reversal = new JournalEntry
            {
                EntryNumber = reversalEntryNumber,
                EntryDate = DateTime.UtcNow,
                PeriodID = currentPeriod.PeriodID,
                JournalEntryTypeID = entry.JournalEntryTypeID,
                SourceReferenceID = entry.SourceReferenceID,
                Description = $"عكس القيد {entry.EntryNumber}: {reason}",
                TotalDebit = entry.TotalCredit,
                TotalCredit = entry.TotalDebit,
                Status = "Posted",
                PostedByUserID = userId,
                PostedAt = DateTime.UtcNow,
                ReversedEntryID = id,
                CreatedByUserID = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.JournalEntries.AddAsync(reversal);
            await _unitOfWork.SaveChangesAsync();

            var reversalLines = new List<JournalEntryLine>();
            foreach (var line in entry.Lines)
            {
                var reversalLine = new JournalEntryLine
                {
                    JournalEntryID = reversal.JournalEntryID,
                    AccountID = line.AccountID,
                    CostCenterID = line.CostCenterID,
                    Debit = line.Credit,
                    Credit = line.Debit,
                    Description = $"عكس: {line.Description}",
                    IsDeleted = false
                };
                reversalLines.Add(reversalLine);
                await _unitOfWork.JournalEntryLines.AddAsync(reversalLine);
            }
            await _unitOfWork.SaveChangesAsync();

            foreach (var rl in reversalLines)
            {
                await UpdateAccountBalanceAsync(reversal.PeriodID, rl.AccountID, rl.Debit, rl.Credit);
<<<<<<< HEAD
                await CreateBankTransactionIfBankAccount(reversal, rl);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            }

            var receiptVoucherType = await _unitOfWork.JournalEntryTypes.Query()
                .FirstOrDefaultAsync(t => t.Code == "REC" && !t.IsDeleted);
            var paymentVoucherType = await _unitOfWork.JournalEntryTypes.Query()
                .FirstOrDefaultAsync(t => t.Code == "PAY" && !t.IsDeleted);

            bool isVoucherEntry = (receiptVoucherType != null && entry.JournalEntryTypeID == receiptVoucherType.JournalEntryTypeID) ||
                                  (paymentVoucherType != null && entry.JournalEntryTypeID == paymentVoucherType.JournalEntryTypeID);

            if (isVoucherEntry && entry.SourceReferenceID.HasValue)
            {
                var voucher = await _unitOfWork.Vouchers.Query()
                    .FirstOrDefaultAsync(v => v.VoucherID == entry.SourceReferenceID.Value && !v.IsDeleted);
                if (voucher != null && voucher.Status == VoucherStatus.Posted)
                {
                    voucher.Status = VoucherStatus.Draft;
                    voucher.UpdatedAt = DateTime.UtcNow;
<<<<<<< HEAD
=======
                    _unitOfWork.Vouchers.Update(voucher);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                }
            }

            await _unitOfWork.SaveChangesAsync();
<<<<<<< HEAD
            if (ownsTransaction) await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
=======
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            throw;
        }

        return ApiResponse<JournalEntryDto>.Success(
            await GetJournalEntryDtoAsync(reversal.JournalEntryID), "تم عكس القيد بنجاح");
    }

    public async Task<FinancialOperationResultDto> ExecuteOperationAsync(FinancialOperationRequestDto request, int userId)
    {
        var mapping = GetOperationMapping(request.OperationType);
        if (mapping == null)
            return new FinancialOperationResultDto
            {
                IsSuccess = false,
                Message = $"نوع العملية {request.OperationType} غير مدعوم بعد"
            };

        var period = await _unitOfWork.FiscalPeriods.GetByIdAsync(request.PeriodID);
        if (period == null || period.IsDeleted || period.IsClosed)
            return new FinancialOperationResultDto { IsSuccess = false, Message = "الفترة المحاسبية غير موجودة أو مغلقة" };

        var debitAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == mapping.DebitAccountCode && !a.IsDeleted);
        var creditAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == mapping.CreditAccountCode && !a.IsDeleted);

        if (debitAccount == null)
            return new FinancialOperationResultDto { IsSuccess = false, Message = $"حساب المدين '{mapping.DebitAccountCode}' غير موجود في الدليل" };
        if (creditAccount == null)
            return new FinancialOperationResultDto { IsSuccess = false, Message = $"حساب الدائن '{mapping.CreditAccountCode}' غير موجود في الدليل" };
        if (!debitAccount.IsActive || !debitAccount.IsPostable)
            return new FinancialOperationResultDto { IsSuccess = false, Message = $"حساب المدين '{mapping.DebitAccountCode}' غير نشط أو غير قابل للترحيل" };
        if (!creditAccount.IsActive || !creditAccount.IsPostable)
            return new FinancialOperationResultDto { IsSuccess = false, Message = $"حساب الدائن '{mapping.CreditAccountCode}' غير نشط أو غير قابل للترحيل" };

<<<<<<< HEAD
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        if (ownsTransaction) await _unitOfWork.BeginTransactionAsync();
=======
        await _unitOfWork.BeginTransactionAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        try
        {
            var entryNumber = await GenerateEntryNumberAsync(request.OperationDate);

            var journalEntryTypeId = await GetJournalEntryTypeIDAsync(request.OperationType);

            var entry = new JournalEntry
            {
                EntryNumber = entryNumber,
                EntryDate = request.OperationDate,
                PeriodID = request.PeriodID,
                JournalEntryTypeID = journalEntryTypeId,
                SourceReferenceID = request.SourceReferenceID,
                Description = request.Description,
                TotalDebit = request.Amount,
                TotalCredit = request.Amount,
                Status = "Posted",
                PostedByUserID = userId,
                PostedAt = DateTime.UtcNow,
                CreatedByUserID = userId,
                CreatedAt = DateTime.UtcNow
            };

            var lines = new List<JournalEntryLine>
            {
                new() { AccountID = debitAccount.AccountID, Debit = request.Amount, Credit = 0, CostCenterID = request.CostCenterID, Description = request.Description, IsDeleted = false },
                new() { AccountID = creditAccount.AccountID, Debit = 0, Credit = request.Amount, CostCenterID = request.CostCenterID, Description = request.Description, IsDeleted = false }
            };
            foreach (var line in lines) entry.Lines.Add(line);

            await _unitOfWork.JournalEntries.AddAsync(entry);
            await _unitOfWork.SaveChangesAsync();

            await UpdateAccountBalanceAsync(request.PeriodID, debitAccount.AccountID, request.Amount, 0);
            await UpdateAccountBalanceAsync(request.PeriodID, creditAccount.AccountID, 0, request.Amount);
            await _unitOfWork.SaveChangesAsync();

<<<<<<< HEAD
            if (ownsTransaction) await _unitOfWork.CommitAsync();
=======
            await _unitOfWork.CommitAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            return new FinancialOperationResultDto
            {
                IsSuccess = true,
                Message = "تم تنفيذ العملية بنجاح",
                JournalEntryID = entry.JournalEntryID,
                EntryNumber = entry.EntryNumber,
                OperationName = request.OperationType.ToString(),
                TotalDebit = request.Amount,
                TotalCredit = request.Amount,
                GeneratedLines = lines.Select(l => new JournalEntryLineDto
                {
                    AccountID = l.AccountID,
                    AccountCode = l.AccountID == debitAccount.AccountID ? debitAccount.AccountCode : creditAccount.AccountCode,
                    AccountName = l.AccountID == debitAccount.AccountID ? debitAccount.AccountName : creditAccount.AccountName,
                    Debit = l.Debit,
                    Credit = l.Credit,
                    Description = l.Description
                }).ToList()
            };
        }
        catch
        {
<<<<<<< HEAD
            if (ownsTransaction) await _unitOfWork.RollbackAsync();
=======
            await _unitOfWork.RollbackAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            throw;
        }
    }

    public async Task<FinancialOperationResultDto> CreateIntegrationEntryAsync(
        FinancialOperationRequestDto request, int userId)
    {
        var mapping = GetOperationMapping(request.OperationType);
        if (mapping == null)
            return new FinancialOperationResultDto { IsSuccess = false, Message = $"نوع العملية {request.OperationType} غير مدعوم" };

        var debitAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == mapping.DebitAccountCode && !a.IsDeleted);
        var creditAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == mapping.CreditAccountCode && !a.IsDeleted);

        if (debitAccount == null)
            return new FinancialOperationResultDto { IsSuccess = false, Message = $"حساب المدين '{mapping.DebitAccountCode}' غير موجود" };
        if (creditAccount == null)
            return new FinancialOperationResultDto { IsSuccess = false, Message = $"حساب الدائن '{mapping.CreditAccountCode}' غير موجود" };
        if (!debitAccount.IsActive || !debitAccount.IsPostable)
            return new FinancialOperationResultDto { IsSuccess = false, Message = "حساب المدين غير نشط أو غير قابل للترحيل" };
        if (!creditAccount.IsActive || !creditAccount.IsPostable)
            return new FinancialOperationResultDto { IsSuccess = false, Message = "حساب الدائن غير نشط أو غير قابل للترحيل" };

        var entryNumber = await GenerateEntryNumberAsync(request.OperationDate);
        var journalEntryTypeId = await GetJournalEntryTypeIDAsync(request.OperationType);

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            EntryDate = request.OperationDate,
            PeriodID = request.PeriodID,
            JournalEntryTypeID = journalEntryTypeId,
            SourceReferenceID = request.SourceReferenceID,
            Description = request.Description,
            TotalDebit = request.Amount,
            TotalCredit = request.Amount,
            Status = "Posted",
            PostedByUserID = userId,
            PostedAt = DateTime.UtcNow,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        var lines = new List<JournalEntryLine>
        {
            new() { AccountID = debitAccount.AccountID, Debit = request.Amount, Credit = 0, CostCenterID = request.CostCenterID, Description = request.Description },
            new() { AccountID = creditAccount.AccountID, Debit = 0, Credit = request.Amount, CostCenterID = request.CostCenterID, Description = request.Description }
        };
        foreach (var line in lines) entry.Lines.Add(line);
        await _unitOfWork.JournalEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        await UpdateAccountBalanceAsync(request.PeriodID, debitAccount.AccountID, request.Amount, 0);
        await UpdateAccountBalanceAsync(request.PeriodID, creditAccount.AccountID, 0, request.Amount);

        return new FinancialOperationResultDto
        {
            IsSuccess = true,
            Message = "تم إنشاء القيد المحاسبي بنجاح",
            EntryNumber = entryNumber,
            OperationName = request.OperationType.ToString(),
            TotalDebit = request.Amount,
            TotalCredit = request.Amount,
            GeneratedLines = lines.Select(l => new JournalEntryLineDto
            {
                AccountID = l.AccountID,
                AccountCode = l.AccountID == debitAccount.AccountID ? debitAccount.AccountCode : creditAccount.AccountCode,
                AccountName = l.AccountID == debitAccount.AccountID ? debitAccount.AccountName : creditAccount.AccountName,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        };
    }

    public async Task<int> GetPeriodIdForDateAsync(DateTime date)
    {
        var period = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.StartDate <= date && p.EndDate >= date && !p.IsDeleted)
            .FirstOrDefaultAsync();
        return period?.PeriodID ?? 0;
    }

    private async Task<string> GenerateEntryNumberAsync(DateTime date)
    {
        var year = date.Year;
        await _entryNumberLock.WaitAsync();
        try
        {
            var count = await _unitOfWork.JournalEntries.Query()
                .CountAsync(e => e.EntryDate.Year == year && !e.IsDeleted) + 1;
            return $"JE-{year}-{count:D6}";
        }
        finally
        {
            _entryNumberLock.Release();
        }
    }

    private async Task UpdateAccountBalanceAsync(int periodId, int accountId, decimal debit, decimal credit)
    {
        var balance = await _unitOfWork.AccountBalances.Query()
            .FirstOrDefaultAsync(b => b.AccountID == accountId && b.PeriodID == periodId);

        if (balance == null)
        {
            balance = new AccountBalance
            {
                AccountID = accountId,
                PeriodID = periodId,
                OpeningDebit = 0,
                OpeningCredit = 0,
                PeriodDebit = debit,
                PeriodCredit = credit,
                ClosingDebit = debit,
                ClosingCredit = credit,
                IsDeleted = false
            };
            await _unitOfWork.AccountBalances.AddAsync(balance);
        }
        else
        {
            if (balance.IsDeleted)
            {
                balance.IsDeleted = false;
                balance.DeletedAt = null;
            }
            balance.PeriodDebit += debit;
            balance.PeriodCredit += credit;
            balance.ClosingDebit = balance.OpeningDebit + balance.PeriodDebit;
            balance.ClosingCredit = balance.OpeningCredit + balance.PeriodCredit;
        }
    }

    private static OperationMapping? GetOperationMapping(FinancialOperationType type)
    {
        var treasury = AccountingConstants.AccountCodes.Treasury;
        return type switch
        {
            FinancialOperationType.SalaryPermanent => new("3111", treasury),
            FinancialOperationType.SalaryContract => new("3112", treasury),
            FinancialOperationType.SalaryTemporary => new("3115", treasury),
            FinancialOperationType.AllowanceRepresentation => new("3121", treasury),
            FinancialOperationType.AllowanceHousing => new("3127", treasury),
            FinancialOperationType.AllowanceNature => new("3123", treasury),
            FinancialOperationType.AllowanceExtra => new("3129", treasury),
            FinancialOperationType.StaffBonus => new("3141", treasury),
            FinancialOperationType.SocialInsurance => new("3161", treasury),
            FinancialOperationType.PensionDisbursement => new("359", treasury),
            FinancialOperationType.EndOfServiceBonus => new("3142", treasury),
            FinancialOperationType.InjuryCompensation => new("3149", treasury),
            FinancialOperationType.ExpenseElectricity => new("3222", treasury),
            FinancialOperationType.ExpenseWater => new("3223", treasury),
            FinancialOperationType.ExpenseFuel => new("3221", treasury),
            FinancialOperationType.ExpenseSpareParts => new("3231", treasury),
            FinancialOperationType.ExpenseStationery => new("3252", treasury),
            FinancialOperationType.ExpenseMaintenance => new("3311", treasury),
            FinancialOperationType.ExpenseTravel => new("3351", treasury),
            FinancialOperationType.ExpenseRent => new("3361", treasury),
            FinancialOperationType.ExpenseGov => new("337", treasury),
            FinancialOperationType.ExpenseMisc => new("3389", treasury),
            FinancialOperationType.RevenuePensionSubscription => new(treasury, "4144"),
            FinancialOperationType.RevenueBankInterest => new(treasury, "451"),
            FinancialOperationType.RevenueInvestment => new(treasury, "4131"),
            FinancialOperationType.RevenuePropertyRent => new(treasury, "4154"),
            FinancialOperationType.RevenueGrantLocal => new(treasury, "4254"),
            FinancialOperationType.RevenueGrantForeign => new(treasury, "4255"),
            FinancialOperationType.RevenueCapitalGain => new(treasury, "457"),
            FinancialOperationType.SecurityReceiveTender => new(treasury, AccountingConstants.AccountCodes.SecurityDeposit),
            FinancialOperationType.SecurityReturnTender => new(AccountingConstants.AccountCodes.SecurityRefund, treasury),
            FinancialOperationType.AccrualSalaries => new("3111", AccountingConstants.AccountCodes.AccruedSalaries),
            FinancialOperationType.AccrualSalariesSettle => new(AccountingConstants.AccountCodes.AccruedSalaries, treasury),
            FinancialOperationType.DeferredRentReceive => new(treasury, AccountingConstants.AccountCodes.DeferredRent),
            FinancialOperationType.DeferredRentEarn => new(AccountingConstants.AccountCodes.DeferredRent, "4154"),
            FinancialOperationType.DeferredGrantReceive => new(treasury, AccountingConstants.AccountCodes.DeferredGrant),
            FinancialOperationType.DeferredGrantEarn => new(AccountingConstants.AccountCodes.DeferredGrant, "4254"),
            FinancialOperationType.LegalReserve => new(AccountingConstants.AccountCodes.RevenueLegalReserve, AccountingConstants.AccountCodes.LegalReserve),
            FinancialOperationType.GeneralReserve => new(AccountingConstants.AccountCodes.RevenueGeneralReserve, AccountingConstants.AccountCodes.GeneralReserve),
            _ => null
        };
    }

    private async Task<int> GetJournalEntryTypeIDAsync(FinancialOperationType type)
    {
        var code = type switch
        {
            >= FinancialOperationType.SalaryPermanent and <= FinancialOperationType.SocialInsurance => "SAL",
            >= FinancialOperationType.PensionDisbursement and <= FinancialOperationType.InjuryCompensation => "PEN",
            >= FinancialOperationType.ExpenseElectricity and <= FinancialOperationType.ExpenseMisc => "EXP",
            >= FinancialOperationType.RevenuePensionSubscription and <= FinancialOperationType.RevenueCapitalGain => "REV",
            >= FinancialOperationType.SecurityReceiveTender and <= FinancialOperationType.SecurityReturnTender => "GEN",
            >= FinancialOperationType.AccrualSalaries and <= FinancialOperationType.DeferredGrantEarn => "ADJ",
            >= FinancialOperationType.LegalReserve => "CLS",
            _ => "GEN"
        };

        var entryType = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted);

        return entryType?.JournalEntryTypeID ?? 1; // fallback to first type
    }

    private async Task<JournalEntryDto?> GetJournalEntryDtoAsync(int id)
    {
        var entry = await _unitOfWork.JournalEntries.Query()
            .Include(e => e.JournalEntryType)
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .Include(e => e.Lines).ThenInclude(l => l.CostCenter)
            .Include(e => e.Period)
            .FirstOrDefaultAsync(e => e.JournalEntryID == id && !e.IsDeleted);
        if (entry == null) return null;
        return ToJournalEntryDtoFromEntity(entry);
    }

    private static JournalEntryDto ToJournalEntryDtoFromEntity(JournalEntry entry)
    {
        return new JournalEntryDto
        {
            JournalEntryID = entry.JournalEntryID,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            PeriodID = entry.PeriodID,
            PeriodName = entry.Period?.PeriodName,
            JournalEntryTypeID = entry.JournalEntryTypeID,
            JournalEntryTypeName = entry.JournalEntryType?.TypeName,
            JournalEntryTypeColor = entry.JournalEntryType?.Color,
            SourceReferenceID = entry.SourceReferenceID,
            Description = entry.Description,
            TotalDebit = entry.TotalDebit,
            TotalCredit = entry.TotalCredit,
            Status = entry.Status,
            PostedAt = entry.PostedAt,
            ReversedEntryID = entry.ReversedEntryID,
            CreatedAt = entry.CreatedAt,
            Lines = entry.Lines?.Select(l => new JournalEntryLineDto
            {
                LineID = l.LineID,
                JournalEntryID = l.JournalEntryID,
                LineNumber = l.LineNumber,
                AccountID = l.AccountID,
                AccountCode = l.Account?.AccountCode ?? "",
                AccountName = l.Account?.AccountName ?? "",
                CostCenterID = l.CostCenterID,
                CostCenterName = l.CostCenter?.Name,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description,
                CheckNo = l.CheckNo,
                CheckDueDate = l.CheckDueDate,
                IsReconciled = l.IsReconciled,
                ReconciledAt = l.ReconciledAt
            }).ToList() ?? new List<JournalEntryLineDto>()
        };
    }

    private record OperationMapping(string DebitAccountCode, string CreditAccountCode);

    private async Task CreateBankTransactionIfBankAccount(JournalEntry entry, JournalEntryLine line)
    {
        // التحقق مما إذا كان الحساب مرتبطاً ببنك
        var bank = await _unitOfWork.Banks.Query()
            .FirstOrDefaultAsync(b => b.AccountID == line.AccountID && b.IsActive);

        if (bank == null)
        {
            // لا يوجد بنك مرتبط بالحساب - لا نسجل شيئاً
            return;
        }

        // تحديد نوع المعاملة بناءً على المدين/الدائن
        string transactionType;
        decimal amount;

        if (line.Debit > 0)
        {
            // إيداع في البنك (البنك مدين)
            transactionType = "Deposit";
            amount = line.Debit;
        }
        else if (line.Credit > 0)
        {
            // سحب من البنك (البنك دائن)
            transactionType = "Withdrawal";
            amount = line.Credit;
        }
        else
        {
            return;
        }

        // تسجيل المعاملة البنكية
        var bankTransaction = new BankTransaction
        {
            BankID = bank.BankID,
            JournalEntryID = entry.JournalEntryID,
            LineID = line.LineID,
            TransactionDate = entry.EntryDate,
            TransactionType = transactionType,
            Amount = amount,
            Description = line.Description ?? entry.Description,
            ReferenceNo = line.CheckNo,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BankTransactions.AddAsync(bankTransaction);
    }
}
