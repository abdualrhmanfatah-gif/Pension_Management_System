using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Assets;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

public class ClosingService : IClosingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJournalEntryService _journalEntryService;
    private readonly Data.PensionDbContext _dbContext;

    public ClosingService(IUnitOfWork unitOfWork, IJournalEntryService journalEntryService, Data.PensionDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _journalEntryService = journalEntryService;
        _dbContext = dbContext;
    }

    private async Task<int> GetJournalEntryTypeIDAsync(string code)
    {
        var entryType = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted);
        return entryType?.JournalEntryTypeID ?? 1;
    }

    public async Task<ApiResponse<ClosingResultDto>> CloseFiscalYearAsync(int fiscalYearId, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<ClosingResultDto>.Fail("السنة المالية غير موجودة أو تم حذفها");

        if (fiscalYear.IsClosed)
            return ApiResponse<ClosingResultDto>.Fail($"السنة المالية ({fiscalYear.YearName}) مغلقة مسبقاً بتاريخ {fiscalYear.ClosedAt:yyyy/MM/dd}");

        var periods = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == fiscalYearId && !p.IsDeleted)
            .ToListAsync();

        if (periods.Any(p => !p.IsClosed))
        {
            var openCount = periods.Count(p => !p.IsClosed);
            var totalCount = periods.Count;
            return ApiResponse<ClosingResultDto>.Fail(
                $"يجب إغلاق جميع الفترات أولاّ — مفتوحة: {openCount} من أصل {totalCount} فترة");
        }

        var periodIds = periods.Select(p => p.PeriodID).ToList();
        var draftEntries = await _unitOfWork.JournalEntries.Query()
            .Where(e => periodIds.Contains(e.PeriodID) && e.Status == "Draft" && !e.IsDeleted)
            .CountAsync();

        if (draftEntries > 0)
            return ApiResponse<ClosingResultDto>.Fail(
                $"يوجد {draftEntries} قيد مسودة يجب ترحيلها أولاّ قبل إقفال السنة");

        var trialBalance = await _journalEntryService.GetPeriodIdForDateAsync(fiscalYear.StartDate);
        var incomeResult = await GetIncomeStatementAsync(fiscalYearId);
        if (!incomeResult.IsSuccess)
            return ApiResponse<ClosingResultDto>.Fail("فشل في حساب قائمة الدخل");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var revResult = await CloseAccountGroupAsync(fiscalYear.StartDate, fiscalYear.EndDate,
                bookNumber: 4, targetAccountCode: "2814", userId, "إقفال الإيرادات");
            if (!revResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<ClosingResultDto>.Fail(revResult.Message ?? "فشل إقفال الإيرادات");
            }

            var expResult = await CloseAccountGroupAsync(fiscalYear.StartDate, fiscalYear.EndDate,
                bookNumber: 3, targetAccountCode: "2812", userId, "إقفال المصروفات");
            if (!expResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<ClosingResultDto>.Fail(expResult.Message ?? "فشل إقفال المصروفات");
            }

            var netIncome = incomeResult.NetIncome;
            if (Math.Abs(netIncome) >= 0.01m)
            {
                var close281Result = await CloseOperatingResultsAsync(fiscalYear, netIncome,
                    incomeResult.TotalRevenue, incomeResult.TotalExpenses, userId);
                if (!close281Result.IsSuccess)
                {
                    await _unitOfWork.RollbackAsync();
                    return ApiResponse<ClosingResultDto>.Fail(close281Result.Message ?? "فشل إقفال حساب النتائج");
                }
            }

            fiscalYear.IsClosed = true;
            fiscalYear.ClosedAt = DateTime.UtcNow;
            fiscalYear.ClosedByUserID = userId;
            _unitOfWork.FiscalYears.Update(fiscalYear);

            await LogAuditAsync("CloseFiscalYear", "FiscalYear", fiscalYearId,
                new { IsClosed = false },
                new { IsClosed = true, ClosedAt = DateTime.UtcNow, ClosedByUserID = userId,
                    NetIncome = netIncome, TotalRevenue = incomeResult.TotalRevenue,
                    TotalExpenses = incomeResult.TotalExpenses },
                userId,
                $"إقفال السنة المالية {fiscalYear.YearName}");

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return ApiResponse<ClosingResultDto>.Success(new ClosingResultDto
            {
                TotalRevenuesClosed = incomeResult.TotalRevenue,
                TotalExpensesClosed = incomeResult.TotalExpenses,
                NetResult = netIncome,
                ResultType = netIncome >= 0 ? "فائض" : "عجز"
            }, "تم إقفال السنة المالية بنجاح");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<OpeningBalanceResultDto>> CreateOpeningBalancesAsync(int newFiscalYearId, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && (y.FiscalYearID == newFiscalYearId || y.YearName == newFiscalYearId.ToString()));
        if (fiscalYear == null)
            return ApiResponse<OpeningBalanceResultDto>.Fail("السنة المالية غير موجودة");

        var firstPeriod = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == newFiscalYearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync();

        if (firstPeriod == null)
            return ApiResponse<OpeningBalanceResultDto>.Fail("لا توجد فترات محاسبية لهذه السنة");

        var existingBalances = await _unitOfWork.AccountBalances.Query()
            .AnyAsync(b => b.PeriodID == firstPeriod.PeriodID && !b.IsDeleted
                && (b.OpeningDebit != 0 || b.OpeningCredit != 0));
        if (existingBalances)
            return ApiResponse<OpeningBalanceResultDto>.Fail("تم إنشاء أرصدة افتتاحية مسبقاً لهذه الفترة");

        var prevFiscalYear = await _unitOfWork.FiscalYears.Query()
            .Where(y => y.EndDate < fiscalYear.StartDate && !y.IsDeleted && y.IsClosed)
            .OrderByDescending(y => y.EndDate)
            .FirstOrDefaultAsync();

        if (prevFiscalYear == null)
            return ApiResponse<OpeningBalanceResultDto>.Fail("لا توجد بيانات سنة سابقة مغلقة");

        var prevYearPeriods = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == prevFiscalYear.FiscalYearID && !p.IsDeleted)
            .ToListAsync();

        var lastPeriod = prevYearPeriods.OrderByDescending(p => p.PeriodNumber).FirstOrDefault();
        if (lastPeriod == null)
            return ApiResponse<OpeningBalanceResultDto>.Fail("لا توجد بيانات السنة السابقة");

        var closingBalances = await _unitOfWork.AccountBalances.Query()
            .Where(b => b.PeriodID == lastPeriod.PeriodID && !b.IsDeleted
                && (b.ClosingDebit > 0 || b.ClosingCredit > 0))
            .ToListAsync();

        if (closingBalances.Count == 0)
            return ApiResponse<OpeningBalanceResultDto>.Fail("لا توجد أرصدة إغلاق للسنة السابقة");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            decimal totalDebits = 0;
            decimal totalCredits = 0;

            foreach (var balance in closingBalances)
            {
                totalDebits += balance.ClosingDebit;
                totalCredits += balance.ClosingCredit;
            }

            if (Math.Abs(totalDebits - totalCredits) >= 0.01m)
                return ApiResponse<OpeningBalanceResultDto>.Fail(
                    $"الأرصدة غير متوازنة: إجمالي المدين {totalDebits} != إجمالي الدائن {totalCredits}");

            var openingBalanceTypeId = await GetJournalEntryTypeIDAsync("OPN");

            var entryResult = await _journalEntryService.CreateJournalEntryAsync(new CreateJournalEntryDto
            {
                EntryDate = fiscalYear.StartDate,
                PeriodID = firstPeriod.PeriodID,
                JournalEntryTypeID = openingBalanceTypeId,
                Description = "أرصدة افتتاحية للسنة المالية الجديدة",
                Lines = closingBalances.Select(b => new CreateJournalEntryLineDto
                {
                    AccountID = b.AccountID,
                    Debit = b.ClosingDebit,
                    Credit = b.ClosingCredit
                }).ToList()
            }, userId);

            if (!entryResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<OpeningBalanceResultDto>.Fail(entryResult.Message ?? "فشل إنشاء قيد الأرصدة الافتتاحية");
            }

            var postResult = await _journalEntryService.PostJournalEntryAsync(entryResult.Data!.JournalEntryID, userId);
            if (!postResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<OpeningBalanceResultDto>.Fail(postResult.Message ?? "فشل ترحيل قيد الأرصدة الافتتاحية");
            }

            foreach (var balance in closingBalances)
            {
                var newBalance = new AccountBalance
                {
                    AccountID = balance.AccountID,
                    PeriodID = firstPeriod.PeriodID,
                    OpeningDebit = balance.ClosingDebit,
                    OpeningCredit = balance.ClosingCredit,
                    PeriodDebit = 0,
                    PeriodCredit = 0,
                    ClosingDebit = balance.ClosingDebit,
                    ClosingCredit = balance.ClosingCredit,
                    IsDeleted = false
                };
                await _unitOfWork.AccountBalances.AddAsync(newBalance);
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return ApiResponse<OpeningBalanceResultDto>.Success(new OpeningBalanceResultDto
            {
                JournalEntryID = entryResult.Data.JournalEntryID,
                EntryNumber = entryResult.Data.EntryNumber,
                AccountsCarriedForward = closingBalances.Count,
                TotalDebits = totalDebits,
                TotalCredits = totalCredits
            }, "تم ترحيل الأرصدة الافتتاحية بنجاح");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<OpeningBalanceResultDto>> CreateInitialOpeningBalancesAsync(
        InitialOpeningBalanceRequestDto request, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(request.FiscalYearID);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<OpeningBalanceResultDto>.Fail("السنة المالية غير موجودة");

        if (fiscalYear.IsClosed)
            return ApiResponse<OpeningBalanceResultDto>.Fail("السنة المالية مغلقة");

        var firstPeriod = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == request.FiscalYearID && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync();

        if (firstPeriod == null)
            return ApiResponse<OpeningBalanceResultDto>.Fail("لا توجد فترات محاسبية لهذه السنة");

        var existingBalanceCount = await _unitOfWork.AccountBalances.Query()
            .CountAsync(b => b.PeriodID == firstPeriod.PeriodID && !b.IsDeleted);
        if (existingBalanceCount > 0)
        {
            var oldBalances = await _unitOfWork.AccountBalances.Query()
                .Where(b => b.PeriodID == firstPeriod.PeriodID && !b.IsDeleted)
                .ToListAsync();
            foreach (var old in oldBalances)
            {
                old.IsDeleted = true;
                old.DeletedAt = DateTime.UtcNow;
                _unitOfWork.AccountBalances.Update(old);
            }

            var openingBalanceTypeId = await GetJournalEntryTypeIDAsync("OPN");
            var oldEntries = await _unitOfWork.JournalEntries.Query()
                .Where(j => j.PeriodID == firstPeriod.PeriodID && j.JournalEntryTypeID == openingBalanceTypeId && !j.IsDeleted)
                .ToListAsync();
            foreach (var oldEntry in oldEntries)
            {
                oldEntry.IsDeleted = true;
                oldEntry.DeletedAt = DateTime.UtcNow;
                _unitOfWork.JournalEntries.Update(oldEntry);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        if (request.Lines == null || !request.Lines.Any())
            return ApiResponse<OpeningBalanceResultDto>.Fail("يجب إدخال حساب واحد على الأقل");

        var accountIds = request.Lines.Select(l => l.AccountID).Distinct().ToList();
        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .Where(a => accountIds.Contains(a.AccountID) && !a.IsDeleted)
            .ToListAsync();

        if (accounts.Count != accountIds.Count)
            return ApiResponse<OpeningBalanceResultDto>.Fail("أحد الحسابات غير موجود");

        var inactiveAccounts = accounts.Where(a => !a.IsActive).Select(a => a.AccountCode).ToList();
        if (inactiveAccounts.Any())
            return ApiResponse<OpeningBalanceResultDto>.Fail($"الحسابات التالية غير نشطة: {string.Join(", ", inactiveAccounts)}");

        var nonPostableAccounts = accounts.Where(a => !a.IsPostable).Select(a => a.AccountCode).ToList();
        if (nonPostableAccounts.Any())
            return ApiResponse<OpeningBalanceResultDto>.Fail($"الحسابات التالية غير قابلة للترحيل: {string.Join(", ", nonPostableAccounts)}");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            decimal totalDebits = 0;
            decimal totalCredits = 0;

            foreach (var lineDto in request.Lines)
            {
                if (lineDto.Balance == 0) continue;

                var account = accounts.First(a => a.AccountID == lineDto.AccountID);
                decimal debit = 0;
                decimal credit = 0;

                if (account.AccountNature == "Debit")
                {
                    if (lineDto.Balance > 0)
                        debit = lineDto.Balance;
                    else
                        credit = Math.Abs(lineDto.Balance);
                }
                else
                {
                    if (lineDto.Balance > 0)
                        credit = lineDto.Balance;
                    else
                        debit = Math.Abs(lineDto.Balance);
                }

                totalDebits += debit;
                totalCredits += credit;
            }

            if (Math.Abs(totalDebits - totalCredits) >= 0.01m)
                return ApiResponse<OpeningBalanceResultDto>.Fail(
                    $"الأرصدة غير متوازنة: إجمالي المدين {totalDebits} != إجمالي الدائن {totalCredits}");

            if (totalDebits == 0 && totalCredits == 0)
                return ApiResponse<OpeningBalanceResultDto>.Fail("جميع الأرصدة صفر");

            var openingBalanceTypeId = await GetJournalEntryTypeIDAsync("OPN");

            var entryResult = await _journalEntryService.CreateJournalEntryAsync(new CreateJournalEntryDto
            {
                EntryDate = request.AsOfDate,
                PeriodID = firstPeriod.PeriodID,
                JournalEntryTypeID = openingBalanceTypeId,
                Description = request.Description,
                Lines = request.Lines.Where(l => l.Balance != 0).Select(lineDto =>
                {
                    var account = accounts.First(a => a.AccountID == lineDto.AccountID);
                    decimal openingDebit = 0;
                    decimal openingCredit = 0;

                    if (account.AccountNature == "Debit")
                    {
                        if (lineDto.Balance > 0)
                            openingDebit = lineDto.Balance;
                        else
                            openingCredit = Math.Abs(lineDto.Balance);
                    }
                    else
                    {
                        if (lineDto.Balance > 0)
                            openingCredit = lineDto.Balance;
                        else
                            openingDebit = Math.Abs(lineDto.Balance);
                    }

                    return new CreateJournalEntryLineDto
                    {
                        AccountID = lineDto.AccountID,
                        Debit = openingDebit,
                        Credit = openingCredit
                    };
                }).ToList()
            }, userId);

            if (!entryResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<OpeningBalanceResultDto>.Fail(entryResult.Message ?? "فشل إنشاء قيد الأرصدة الافتتاحية");
            }

            var postResult = await _journalEntryService.PostJournalEntryAsync(entryResult.Data!.JournalEntryID, userId);
            if (!postResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<OpeningBalanceResultDto>.Fail(postResult.Message ?? "فشل ترحيل قيد الأرصدة الافتتاحية");
            }

            foreach (var lineDto in request.Lines)
            {
                if (lineDto.Balance == 0) continue;

                var account = accounts.First(a => a.AccountID == lineDto.AccountID);
                decimal openingDebit = 0;
                decimal openingCredit = 0;

                if (account.AccountNature == "Debit")
                {
                    if (lineDto.Balance > 0)
                        openingDebit = lineDto.Balance;
                    else
                        openingCredit = Math.Abs(lineDto.Balance);
                }
                else
                {
                    if (lineDto.Balance > 0)
                        openingCredit = lineDto.Balance;
                    else
                        openingDebit = Math.Abs(lineDto.Balance);
                }

                var existingBalance = await _unitOfWork.AccountBalances.Query()
                    .FirstOrDefaultAsync(b => b.AccountID == lineDto.AccountID && b.PeriodID == firstPeriod.PeriodID && !b.IsDeleted);

                if (existingBalance != null)
                {
                    existingBalance.OpeningDebit = openingDebit;
                    existingBalance.OpeningCredit = openingCredit;
                    existingBalance.ClosingDebit = openingDebit;
                    existingBalance.ClosingCredit = openingCredit;
                    _unitOfWork.AccountBalances.Update(existingBalance);
                }
                else
                {
                    var balance = new AccountBalance
                    {
                        AccountID = lineDto.AccountID,
                        PeriodID = firstPeriod.PeriodID,
                        OpeningDebit = openingDebit,
                        OpeningCredit = openingCredit,
                        PeriodDebit = 0,
                        PeriodCredit = 0,
                        ClosingDebit = openingDebit,
                        ClosingCredit = openingCredit,
                        IsDeleted = false
                    };
                    await _unitOfWork.AccountBalances.AddAsync(balance);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return ApiResponse<OpeningBalanceResultDto>.Success(new OpeningBalanceResultDto
            {
                JournalEntryID = entryResult.Data.JournalEntryID,
                EntryNumber = entryResult.Data.EntryNumber,
                AccountsCarriedForward = request.Lines.Count(l => l.Balance != 0),
                TotalDebits = totalDebits,
                TotalCredits = totalCredits
            }, "تم إدخال الأرصدة الافتتاحية بنجاح");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<InitialOpeningBalanceResponseDto>> GetInitialOpeningBalancesAsync(int fiscalYearId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<InitialOpeningBalanceResponseDto>.Fail("السنة المالية غير موجودة");

        var firstPeriod = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == fiscalYearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync();

        if (firstPeriod == null)
            return ApiResponse<InitialOpeningBalanceResponseDto>.Fail("لا توجد فترات محاسبية لهذه السنة");

        var balances = await _unitOfWork.AccountBalances.Query()
            .Where(b => b.PeriodID == firstPeriod.PeriodID && !b.IsDeleted
                && (b.OpeningDebit != 0 || b.OpeningCredit != 0))
            .ToListAsync();

        if (!balances.Any())
            return ApiResponse<InitialOpeningBalanceResponseDto>.Success(new InitialOpeningBalanceResponseDto
            {
                FiscalYearID = fiscalYearId,
                YearName = fiscalYear.YearName,
                AsOfDate = fiscalYear.StartDate,
                Lines = new List<OpeningBalanceLineDto>()
            });

        var accountIds = balances.Select(b => b.AccountID).Distinct().ToList();
        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .Where(a => accountIds.Contains(a.AccountID) && !a.IsDeleted)
            .ToDictionaryAsync(a => a.AccountID, a => a);

        var openingBalanceTypeId = await GetJournalEntryTypeIDAsync("OPN");
        var journalEntry = await _unitOfWork.JournalEntries.Query()
            .Where(j => j.PeriodID == firstPeriod.PeriodID && j.JournalEntryTypeID == openingBalanceTypeId && !j.IsDeleted)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync();

        var lines = balances.Select(b =>
        {
            var account = accounts.GetValueOrDefault(b.AccountID);
            decimal balance = 0;
            if (account?.AccountNature == "Debit")
                balance = b.OpeningDebit - b.OpeningCredit;
            else
                balance = b.OpeningCredit - b.OpeningDebit;

            return new OpeningBalanceLineDto
            {
                AccountID = b.AccountID,
                AccountCode = account?.AccountCode ?? "",
                AccountName = account?.AccountName ?? "",
                AccountNature = account?.AccountNature ?? "",
                Balance = balance
            };
        }).ToList();

        return ApiResponse<InitialOpeningBalanceResponseDto>.Success(new InitialOpeningBalanceResponseDto
        {
            FiscalYearID = fiscalYearId,
            YearName = fiscalYear.YearName,
            AsOfDate = journalEntry?.EntryDate ?? fiscalYear.StartDate,
            Description = journalEntry?.Description,
            JournalEntryID = journalEntry?.JournalEntryID ?? 0,
            EntryNumber = journalEntry?.EntryNumber,
            Lines = lines
        });
    }

    public async Task<ApiResponse<SeedInitialDefaultsResultDto>> SeedInitialDefaultsAsync(int fiscalYearId, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<SeedInitialDefaultsResultDto>.Fail("السنة المالية غير موجودة");

        if (fiscalYear.IsClosed)
            return ApiResponse<SeedInitialDefaultsResultDto>.Fail("السنة المالية مغلقة");

        var firstPeriod = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == fiscalYearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync();

        if (firstPeriod == null)
            return ApiResponse<SeedInitialDefaultsResultDto>.Fail("لا توجد فترات محاسبية لهذه السنة");

        var accountEntities = await _unitOfWork.ChartOfAccounts.Query()
            .Where(a => a.AccountCode != null && !a.IsDeleted)
            .ToDictionaryAsync(a => a.AccountCode!, a => a);

        int? GetAccountId(string code) =>
            accountEntities.TryGetValue(code, out var a) ? a.AccountID : (int?)null;

        ChartOfAccount? GetAccountEntity(int id) =>
            accountEntities.Values.FirstOrDefault(a => a.AccountID == id);

        decimal centralBankBalance = 50000000m;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var resultAssets = new List<SeedInitialAssetDto>();
            var resultBalances = new List<SeedInitialBalanceLineDto>();

            var centralBankAsset = GetAccountId("18211").HasValue ? GetAccountEntity(GetAccountId("18211")!.Value) : null;
            if (centralBankAsset != null)
                resultBalances.Add(new SeedInitialBalanceLineDto
                {
                    AccountID = centralBankAsset.AccountID,
                    AccountCode = centralBankAsset.AccountCode ?? "",
                    AccountName = centralBankAsset.AccountName,
                    AccountNature = centralBankAsset.AccountNature,
                    Balance = centralBankBalance
                });

            var centralBankLiability = GetAccountId("256").HasValue ? GetAccountEntity(GetAccountId("256")!.Value) : null;
            if (centralBankLiability != null)
                resultBalances.Add(new SeedInitialBalanceLineDto
                {
                    AccountID = centralBankLiability.AccountID,
                    AccountCode = centralBankLiability.AccountCode ?? "",
                    AccountName = centralBankLiability.AccountName,
                    AccountNature = centralBankLiability.AccountNature,
                    Balance = -centralBankBalance
                });

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var msg = $"تم إعداد البيانات الافتتاحية للسنة {fiscalYearId}. اضغط ترحيل الأرصدة الافتتاحية لإثبات الأرصدة.";

            return ApiResponse<SeedInitialDefaultsResultDto>.Success(new SeedInitialDefaultsResultDto
            {
                FiscalYearID = fiscalYearId,
                PeriodID = firstPeriod.PeriodID,
                BalanceCount = 0,
                AssetCount = 0,
                CentralBankBalance = centralBankBalance,
                Assets = resultAssets,
                Balances = resultBalances
            }, msg);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // ═══════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════

    private async Task<ApiResponse<bool>> CloseAccountGroupAsync(DateTime fromDate, DateTime toDate,
        int bookNumber, string targetAccountCode, int userId, string description)
    {
        var targetAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == targetAccountCode && !a.IsDeleted);
        if (targetAccount == null)
            return ApiResponse<bool>.Fail($"حساب الترحيل {targetAccountCode} غير موجود");

        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .Where(y => y.StartDate <= toDate && y.EndDate >= toDate && !y.IsDeleted)
            .FirstOrDefaultAsync();

        if (fiscalYear == null)
            return ApiResponse<bool>.Fail("لا توجد سنة مالية لهذا التاريخ");

        var period = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == fiscalYear.FiscalYearID && !p.IsDeleted)
            .OrderByDescending(p => p.PeriodNumber)
            .FirstOrDefaultAsync();

        if (period == null)
            return ApiResponse<bool>.Fail("لا توجد فترة محاسبية للسنة المالية");

        var accountBalances = await _unitOfWork.ChartOfAccounts.Query()
            .Where(a => a.BookNumber == bookNumber && a.IsPostable && !a.IsDeleted)
            .Join(_unitOfWork.JournalEntryLines.Query()
                .Include(l => l.JournalEntry)
                .Where(l => !l.JournalEntry.IsDeleted && l.JournalEntry.Status == "Posted"
                    && l.JournalEntry.EntryDate >= fromDate && l.JournalEntry.EntryDate <= toDate),
                account => account.AccountID,
                line => line.AccountID,
                (account, line) => new { account, line })
            .GroupBy(x => x.account.AccountID)
            .Select(g => new
            {
                AccountID = g.Key,
                Account = g.First().account,
                NetAmount = bookNumber == 4
                    ? g.Sum(x => x.line.Credit - x.line.Debit)
                    : g.Sum(x => x.line.Debit - x.line.Credit)
            })
            .Where(x => Math.Abs(x.NetAmount) >= 0.01m)
            .ToListAsync();

        if (accountBalances.Count == 0)
            return ApiResponse<bool>.Success(true, "لا توجد أرصدة لإقفالها");

        var lineDtos = new List<CreateJournalEntryLineDto>();
        decimal totalTargetAmount = 0;

        foreach (var item in accountBalances)
        {
            if (bookNumber == 4)
            {
                lineDtos.Add(new CreateJournalEntryLineDto { AccountID = item.Account.AccountID, Debit = item.NetAmount, Credit = 0 });
                totalTargetAmount += item.NetAmount;
            }
            else
            {
                lineDtos.Add(new CreateJournalEntryLineDto { AccountID = item.Account.AccountID, Debit = 0, Credit = item.NetAmount });
                totalTargetAmount += item.NetAmount;
            }
        }

        if (bookNumber == 4)
            lineDtos.Add(new CreateJournalEntryLineDto { AccountID = targetAccount.AccountID, Debit = 0, Credit = totalTargetAmount });
        else
            lineDtos.Add(new CreateJournalEntryLineDto { AccountID = targetAccount.AccountID, Debit = totalTargetAmount, Credit = 0 });

        var closingTypeId = await GetJournalEntryTypeIDAsync("CLS");

        var createDto = new CreateJournalEntryDto
        {
            EntryDate = toDate,
            PeriodID = period.PeriodID,
            JournalEntryTypeID = closingTypeId,
            Description = description,
            Lines = lineDtos
        };

        var result = await _journalEntryService.CreateJournalEntryAsync(createDto, userId);
        if (!result.IsSuccess)
            return ApiResponse<bool>.Fail(result.Message!);

        await _journalEntryService.PostJournalEntryAsync(result.Data!.JournalEntryID, userId);
        return ApiResponse<bool>.Success(true);
    }

    private async Task<ApiResponse<bool>> CloseOperatingResultsAsync(FiscalYear fiscalYear, decimal netIncome,
        decimal totalRevenue, decimal totalExpenses, int userId)
    {
        var profitAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == AccountingConstants.AccountCodes.Profit && !a.IsDeleted);
        if (profitAccount == null)
            return ApiResponse<bool>.Fail("حساب الفائض (2222) غير موجود");

        var lossAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == AccountingConstants.AccountCodes.Loss && !a.IsDeleted);
        if (lossAccount == null)
            return ApiResponse<bool>.Fail("حساب العجز (2221) غير موجود");

        var incomeCreditAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == AccountingConstants.AccountCodes.IncomeCredit && !a.IsDeleted);
        if (incomeCreditAccount == null)
            return ApiResponse<bool>.Fail("حساب النتيجة دائنة (2814) غير موجود");

        var incomeDebitAccount = await _unitOfWork.ChartOfAccounts.Query()
            .FirstOrDefaultAsync(a => a.AccountCode == AccountingConstants.AccountCodes.IncomeDebit && !a.IsDeleted);
        if (incomeDebitAccount == null)
            return ApiResponse<bool>.Fail("حساب النتيجة مدينة (2812) غير موجود");

        var period = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == fiscalYear.FiscalYearID && !p.IsDeleted)
            .OrderByDescending(p => p.PeriodNumber)
            .FirstOrDefaultAsync();
        if (period == null)
            return ApiResponse<bool>.Fail("لا توجد فترة محاسبية للسنة");

        List<CreateJournalEntryLineDto> lines;
        if (netIncome >= 0)
        {
            lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountID = incomeCreditAccount.AccountID, Debit = totalRevenue, Credit = 0 },
                new() { AccountID = incomeDebitAccount.AccountID, Debit = 0, Credit = totalExpenses },
                new() { AccountID = profitAccount.AccountID, Debit = 0, Credit = netIncome }
            };
        }
        else
        {
            var absIncome = Math.Abs(netIncome);
            lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountID = incomeCreditAccount.AccountID, Debit = totalRevenue, Credit = 0 },
                new() { AccountID = incomeDebitAccount.AccountID, Debit = 0, Credit = totalExpenses },
                new() { AccountID = lossAccount.AccountID, Debit = absIncome, Credit = 0 }
            };
        }

        var closingTypeId = await GetJournalEntryTypeIDAsync("CLS");

        var createDto = new CreateJournalEntryDto
        {
            EntryDate = fiscalYear.EndDate,
            PeriodID = period.PeriodID,
            JournalEntryTypeID = closingTypeId,
            Description = $"إقفال حساب النتائج إلى رأس المال - {(netIncome >= 0 ? "فائض" : "عجز")}",
            Lines = lines
        };

        var result = await _journalEntryService.CreateJournalEntryAsync(createDto, userId);
        if (!result.IsSuccess)
            return ApiResponse<bool>.Fail(result.Message!);

        await _journalEntryService.PostJournalEntryAsync(result.Data!.JournalEntryID, userId);
        return ApiResponse<bool>.Success(true, "تم إقفال حساب النتائج");
    }

    private async Task<IncomeStatementResult> GetIncomeStatementAsync(int fiscalYearId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return new IncomeStatementResult { IsSuccess = false };

        var lines = _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry)
            .Where(l => !l.JournalEntry.IsDeleted && l.JournalEntry.Status == "Posted"
                && l.JournalEntry.EntryDate >= fiscalYear.StartDate && l.JournalEntry.EntryDate <= fiscalYear.EndDate);

        var grouped = await lines.GroupBy(l => l.AccountID)
            .Select(g => new { AccountID = g.Key, Net = g.Sum(l => l.Credit - l.Debit) })
            .ToListAsync();

        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.BookNumber >= 3 && a.BookNumber <= 4)
            .ToListAsync();

        decimal totalRevenue = 0;
        decimal totalExpenses = 0;

        foreach (var item in grouped)
        {
            var account = accounts.FirstOrDefault(a => a.AccountID == item.AccountID);
            if (account == null) continue;

            if (account.BookNumber == 4)
                totalRevenue += item.Net;
            else if (account.BookNumber == 3)
                totalExpenses += -item.Net;
        }

        return new IncomeStatementResult
        {
            IsSuccess = true,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetIncome = totalRevenue - totalExpenses
        };
    }

    private async Task LogAuditAsync(string action, string entityName, int? entityId,
        object? oldValues, object? newValues, int userId, string? summary = null)
    {
        var log = new AuditLog
        {
            Action = action,
            TableID = null,
            RecordID = entityId,
            EntityDisplayName = summary,
            OldValues = oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues) : null,
            UserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.AuditLogs.AddAsync(log);
    }

    private class IncomeStatementResult
    {
        public bool IsSuccess { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
    }
}
