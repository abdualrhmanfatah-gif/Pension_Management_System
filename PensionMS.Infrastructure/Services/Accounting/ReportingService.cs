using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

public class ReportingService : IReportingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public ReportingService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    private async Task<int> GetJournalEntryTypeIDAsync(string code)
    {
        var entryType = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted);

        if (entryType == null)
            throw new InvalidOperationException($"نوع القيد '{code}' غير موجود في قاعدة البيانات");

        return entryType.JournalEntryTypeID;
    }

    public async Task<ApiResponse<List<TrialBalanceItemDto>>> GetTrialBalanceAsync(int? periodId, DateTime? fromDate, DateTime? toDate)
    {
        if (!periodId.HasValue && !fromDate.HasValue)
            return ApiResponse<List<TrialBalanceItemDto>>.Fail("يجب تحديد الفترة المحاسبية أو تاريخ البداية");

        // جلب جميع الحسابات (الرئيسية والفرعية)
        var allAccounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        // ═══ حساب الأرصدة الافتتاحية (مدين + دائن بشكل منفصل) ═══
        var openingDebits = new Dictionary<int, decimal>();
        var openingCredits = new Dictionary<int, decimal>();
        if (periodId.HasValue)
        {
            var balances = await _unitOfWork.AccountBalances.Query()
                .Where(b => b.PeriodID == periodId.Value && !b.IsDeleted)
                .ToListAsync();
            foreach (var b in balances)
            {
                openingDebits[b.AccountID] = b.OpeningDebit;
                openingCredits[b.AccountID] = b.OpeningCredit;
            }
        }
        else if (fromDate.HasValue)
        {
            var openingBalanceTypeId = await GetJournalEntryTypeIDAsync("OPN");
            var openingDate = fromDate.Value.AddDays(-1);
            var openingLines = _unitOfWork.JournalEntryLines.Query()
                .Include(l => l.JournalEntry)
                .Where(l => !l.JournalEntry.IsDeleted
                    && l.JournalEntry.Status == "Posted"
                    && (l.JournalEntry.EntryDate <= openingDate
                        || (l.JournalEntry.JournalEntryTypeID == openingBalanceTypeId && l.JournalEntry.EntryDate == fromDate.Value)));

            var openingGrouped = await openingLines.GroupBy(l => l.AccountID)
                .Select(g => new { AccountID = g.Key, TotalDebit = g.Sum(l => l.Debit), TotalCredit = g.Sum(l => l.Credit) })
                .ToListAsync();

            foreach (var item in openingGrouped)
            {
                openingDebits[item.AccountID] = item.TotalDebit;
                openingCredits[item.AccountID] = item.TotalCredit;
            }
        }

        // ═══ جلب حركة الفترة (باستثناء قيود النظام والأرصدة الافتتاحية) ═══
        var systemTypeIdForFilter = await GetJournalEntryTypeIDAsync("SYS");
        var openingBalanceTypeIdForFilter = await GetJournalEntryTypeIDAsync("OPN");
        var lines = _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry)
            .Where(l => !l.JournalEntry.IsDeleted
                && l.JournalEntry.Status == "Posted"
                && l.JournalEntry.JournalEntryTypeID != systemTypeIdForFilter
                && l.JournalEntry.JournalEntryTypeID != openingBalanceTypeIdForFilter);

        if (periodId.HasValue)
            lines = lines.Where(l => l.JournalEntry.PeriodID == periodId.Value);
        if (fromDate.HasValue)
            lines = lines.Where(l => l.JournalEntry.EntryDate >= fromDate.Value);
        if (toDate.HasValue)
            lines = lines.Where(l => l.JournalEntry.EntryDate <= toDate.Value);

        var grouped = await lines.GroupBy(l => l.AccountID)
            .Select(g => new
            {
                AccountID = g.Key,
                MovementDebit = g.Sum(l => l.Debit),
                MovementCredit = g.Sum(l => l.Credit)
            }).ToListAsync();

        var groupedDict = grouped.ToDictionary(g => g.AccountID);

        // ═══ بناء النتيجة ═══
        var result = new List<TrialBalanceItemDto>();
        foreach (var account in allAccounts)
        {
            var openingDebit = openingDebits.GetValueOrDefault(account.AccountID, 0);
            var openingCredit = openingCredits.GetValueOrDefault(account.AccountID, 0);

            var hasMovement = groupedDict.TryGetValue(account.AccountID, out var movement);
            var movementDebit = hasMovement ? movement.MovementDebit : 0;
            var movementCredit = hasMovement ? movement.MovementCredit : 0;

            // المجاميع الإجمالية
            var totalDebit = openingDebit + movementDebit;
            var totalCredit = openingCredit + movementCredit;

            // الرصيد الختامي يُحسب حسب طبيعة الحساب
            decimal closingDebit, closingCredit;
            if (account.AccountNature == "Debit")
            {
                var net = totalDebit - totalCredit;
                closingDebit = net >= 0 ? net : 0;
                closingCredit = net < 0 ? Math.Abs(net) : 0;
            }
            else // Credit
            {
                var net = totalCredit - totalDebit;
                closingDebit = net < 0 ? Math.Abs(net) : 0;
                closingCredit = net >= 0 ? net : 0;
            }

            // الرصيد الصافي
            var balance = account.AccountNature == "Debit"
                ? closingDebit - closingCredit
                : closingCredit - closingDebit;

            var balanceType = balance > 0
                ? (account.AccountNature == "Debit" ? "Debit" : "Credit")
                : (account.AccountNature == "Debit" ? "Credit" : "Debit");

            result.Add(new TrialBalanceItemDto
            {
                AccountID = account.AccountID,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountNature = account.AccountNature,
                AccountLevel = account.AccountLevel,
                ParentAccountID = account.ParentAccountID,
                IsPostable = account.IsPostable,
                OpeningDebit = openingDebit,
                OpeningCredit = openingCredit,
                MovementDebit = movementDebit,
                MovementCredit = movementCredit,
                ClosingDebit = closingDebit,
                ClosingCredit = closingCredit,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Balance = Math.Abs(balance),
                BalanceType = balanceType
            });
        }

        // ═══ حساب المجاميع للحسابات الرئيسية (bottom-up) ═══
        foreach (var account in result.Where(a => !a.IsPostable).OrderByDescending(a => a.AccountLevel))
        {
            var children = result.Where(r => r.ParentAccountID == account.AccountID).ToList();
            account.OpeningDebit = children.Sum(c => c.OpeningDebit);
            account.OpeningCredit = children.Sum(c => c.OpeningCredit);
            account.MovementDebit = children.Sum(c => c.MovementDebit);
            account.MovementCredit = children.Sum(c => c.MovementCredit);
            account.TotalDebit = account.OpeningDebit + account.MovementDebit;
            account.TotalCredit = account.OpeningCredit + account.MovementCredit;

            if (account.AccountNature == "Debit")
            {
                var net = account.TotalDebit - account.TotalCredit;
                account.ClosingDebit = net >= 0 ? net : 0;
                account.ClosingCredit = net < 0 ? Math.Abs(net) : 0;
            }
            else
            {
                var net = account.TotalCredit - account.TotalDebit;
                account.ClosingDebit = net < 0 ? Math.Abs(net) : 0;
                account.ClosingCredit = net >= 0 ? net : 0;
            }

            var bal = account.AccountNature == "Debit"
                ? account.ClosingDebit - account.ClosingCredit
                : account.ClosingCredit - account.ClosingDebit;
            account.Balance = Math.Abs(bal);
            account.BalanceType = bal > 0
                ? (account.AccountNature == "Debit" ? "Debit" : "Credit")
                : (account.AccountNature == "Debit" ? "Credit" : "Debit");
        }

        // ═══ إخفاء الحسابات الصفرية ═══
        var filtered = result
            .Where(a => Math.Abs(a.TotalDebit) > 0.0001m || Math.Abs(a.TotalCredit) > 0.0001m || Math.Abs(a.Balance) > 0.0001m)
            .ToList();

        return ApiResponse<List<TrialBalanceItemDto>>.Success(filtered);
    }

    public async Task<ApiResponse<BalanceSheetDto>> GetBalanceSheetAsync(DateTime asOfDate)
    {
        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.BookNumber <= 2 && a.IsPostable)
            .ToListAsync();

        var balances = await GetAccountBalancesUpToDateAsync(asOfDate, considerAccountNature: true);

        var assets = new List<BalanceSheetSectionDto>();
        var liabilities = new List<BalanceSheetSectionDto>();
        var equity = new List<BalanceSheetSectionDto>();

        foreach (var account in accounts)
        {
            if (!balances.TryGetValue(account.AccountID, out var bal)) continue;

            var displayBalance = Math.Abs(bal);

            var item = new BalanceSheetSectionDto
            {
                AccountID = account.AccountID,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountLevel = account.AccountLevel,
                IsPostable = account.IsPostable,
                Balance = displayBalance
            };

            if (account.BookNumber == 1)
                assets.Add(item);
            else if (account.BookNumber == 2 && (account.AccountCode.StartsWith("21") || account.AccountCode.StartsWith("22")))
                equity.Add(item);
            else if (account.BookNumber == 2)
                liabilities.Add(item);
        }

        var totalAssets = assets.Sum(a => a.Balance);
        var totalLiabilities = liabilities.Sum(l => l.Balance);
        var totalEquity = equity.Sum(e => e.Balance);

        return ApiResponse<BalanceSheetDto>.Success(new BalanceSheetDto
        {
            AsOfDate = asOfDate,
            Assets = assets,
            Liabilities = liabilities,
            Equity = equity,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            IsBalanced = Math.Abs(totalAssets - (totalLiabilities + totalEquity)) < 0.01m
        });
    }

    public async Task<ApiResponse<IncomeStatementDto>> GetIncomeStatementAsync(int fiscalYearId, int? quarterId = null)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<IncomeStatementDto>.Fail("السنة المالية غير موجودة");

        DateTime fromDate;
        DateTime toDate;

        if (quarterId.HasValue)
        {
            var quarter = await _unitOfWork.Quarters.GetByIdAsync(quarterId.Value);
            if (quarter == null || quarter.IsDeleted)
                return ApiResponse<IncomeStatementDto>.Fail("الفصل غير موجود");
            if (quarter.FiscalYearID != fiscalYearId)
                return ApiResponse<IncomeStatementDto>.Fail("الفصل لا ينتمي للسنة المالية المحددة");

            fromDate = quarter.StartDate;
            toDate = quarter.EndDate;
        }
        else
        {
            fromDate = fiscalYear.StartDate;
            toDate = fiscalYear.EndDate;
        }

        var lines = _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry)
            .Where(l => !l.JournalEntry.IsDeleted && l.JournalEntry.Status == "Posted"
                && l.JournalEntry.EntryDate >= fromDate && l.JournalEntry.EntryDate <= toDate);

        var grouped = await lines.GroupBy(l => l.AccountID)
            .Select(g => new { AccountID = g.Key, Net = g.Sum(l => l.Credit - l.Debit) })
            .ToListAsync();

        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.BookNumber >= 3 && a.BookNumber <= 4)
            .ToListAsync();

        var revenues = new List<IncomeStatementLineDto>();
        var expenses = new List<IncomeStatementLineDto>();

        foreach (var item in grouped)
        {
            var account = accounts.FirstOrDefault(a => a.AccountID == item.AccountID);
            if (account == null) continue;

            var lineDto = new IncomeStatementLineDto
            {
                AccountID = account.AccountID,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountLevel = account.AccountLevel,
                Amount = item.Net
            };

            if (account.BookNumber == 4)
            {
                revenues.Add(lineDto);
            }
            else if (account.BookNumber == 3)
            {
                lineDto.Amount = -item.Net;
                expenses.Add(lineDto);
            }
        }

        var totalRev = revenues.Sum(r => r.Amount);
        var totalExp = expenses.Sum(e => e.Amount);

        return ApiResponse<IncomeStatementDto>.Success(new IncomeStatementDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Revenues = revenues,
            Expenses = expenses,
            TotalRevenue = totalRev,
            TotalExpenses = totalExp,
            NetIncome = totalRev - totalExp
        });
    }

    public async Task<ApiResponse<GeneralLedgerDto>> GetGeneralLedgerAsync(int accountId, int? periodId, DateTime? fromDate, DateTime? toDate)
    {
        var account = await _unitOfWork.ChartOfAccounts.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
            return ApiResponse<GeneralLedgerDto>.Fail("الحساب غير موجود");

        var accountIds = new List<int> { accountId };
        if (!account.IsPostable)
        {
            var childAccounts = await GetAllChildAccountIdsAsync(accountId);
            accountIds.AddRange(childAccounts);
        }

        var query = _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry).ThenInclude(e => e.Period)
            .Include(l => l.Account)
            .Where(l => accountIds.Contains(l.AccountID) && !l.JournalEntry.IsDeleted && l.JournalEntry.Status == "Posted");

        if (periodId.HasValue)
            query = query.Where(l => l.JournalEntry.PeriodID == periodId.Value);
        if (fromDate.HasValue) query = query.Where(l => l.JournalEntry.EntryDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(l => l.JournalEntry.EntryDate <= toDate.Value);

        var entryLines = await query.AsNoTracking().OrderBy(l => l.JournalEntry.EntryDate).ThenBy(l => l.LineID).ToListAsync();

        var openingBalance = await GetOpeningBalanceAsync(accountId, periodId, fromDate);
        decimal running = openingBalance;
        var lines = new List<GeneralLedgerLineDto>();

        foreach (var line in entryLines)
        {
            running += line.Debit - line.Credit;
            lines.Add(new GeneralLedgerLineDto
            {
                EntryDate = line.JournalEntry.EntryDate,
                EntryNumber = line.JournalEntry.EntryNumber,
                Description = line.Description ?? line.JournalEntry.Description,
                Debit = line.Debit,
                Credit = line.Credit,
                RunningBalance = running,
                AccountCode = line.Account?.AccountCode,
                AccountName = line.Account?.AccountName
            });
        }

        return ApiResponse<GeneralLedgerDto>.Success(new GeneralLedgerDto
        {
            AccountID = account.AccountID,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountNature = account.AccountNature,
            OpeningBalance = openingBalance,
            TotalDebit = entryLines.Sum(l => l.Debit),
            TotalCredit = entryLines.Sum(l => l.Credit),
            ClosingBalance = running,
            Entries = lines
        });
    }

    private async Task<List<int>> GetAllChildAccountIdsAsync(int parentAccountId)
    {
        var allAccounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Select(a => new { a.AccountID, a.ParentAccountID, a.IsPostable })
            .ToListAsync();

        var childIds = new List<int>();
        var queue = new Queue<int>();
        queue.Enqueue(parentAccountId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allAccounts.Where(a => a.ParentAccountID == currentId).ToList();
            foreach (var child in children)
            {
                childIds.Add(child.AccountID);
                if (!child.IsPostable)
                    queue.Enqueue(child.AccountID);
            }
        }

        return childIds;
    }

    public async Task<ApiResponse<AccountStatementResultDto>> GetAccountStatementAsync(int accountId, DateTime fromDate, DateTime toDate)
    {
        var account = await _unitOfWork.ChartOfAccounts.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
            return ApiResponse<AccountStatementResultDto>.Fail("الحساب غير موجود");

        var entryLines = await _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry)
            .AsNoTracking()
            .Where(l => l.AccountID == accountId && !l.JournalEntry.IsDeleted
                && l.JournalEntry.Status == "Posted"
                && l.JournalEntry.EntryDate >= fromDate && l.JournalEntry.EntryDate <= toDate)
            .OrderBy(l => l.JournalEntry.EntryDate).ThenBy(l => l.LineID)
            .ToListAsync();

        var openingBalance = await GetOpeningBalanceAsync(accountId, null, fromDate);
        var items = new List<AccountStatementItemDto>();
        decimal running = openingBalance;

        foreach (var line in entryLines)
        {
            running += line.Debit - line.Credit;
            items.Add(new AccountStatementItemDto
            {
                EntryDate = line.JournalEntry.EntryDate,
                EntryNumber = line.JournalEntry.EntryNumber,
                Description = line.Description ?? line.JournalEntry.Description,
                Debit = line.Debit,
                Credit = line.Credit,
                RunningBalance = running
            });
        }

        return ApiResponse<AccountStatementResultDto>.Success(new AccountStatementResultDto
        {
            AccountID = account.AccountID,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountNature = account.AccountNature,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            TotalDebit = entryLines.Sum(l => l.Debit),
            TotalCredit = entryLines.Sum(l => l.Credit),
            ClosingBalance = running,
            Items = items
        });
    }

    public async Task<ApiResponse<JournalRegisterDto>> GetJournalRegisterAsync(JournalRegisterFilterDto filter)
    {
        if (filter == null)
            return ApiResponse<JournalRegisterDto>.Fail("معايير الفلترة مطلوبة");

        if (filter.Page <= 0)
            filter.Page = 1;
        if (filter.PageSize <= 0 || filter.PageSize > 100)
            filter.PageSize = 20;

        var query = _unitOfWork.JournalEntries.Query()
            .Where(e => !e.IsDeleted && e.EntryDate >= filter.FromDate && e.EntryDate <= filter.ToDate);

        var grandTotalDebit = await query.SumAsync(e => e.TotalDebit);
        var grandTotalCredit = await query.SumAsync(e => e.TotalCredit);
        var total = await query.CountAsync();

        var entries = await query
            .AsNoTracking()
            .OrderBy(e => e.EntryDate).ThenBy(e => e.JournalEntryID)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(e => e.JournalEntryType)
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .Include(e => e.Lines).ThenInclude(l => l.CostCenter)
            .ToListAsync();

        var entryDtos = entries.Select(entry => new JournalRegisterItemDto
        {
            JournalEntryID = entry.JournalEntryID,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            JournalEntryTypeID = entry.JournalEntryTypeID,
            JournalEntryTypeName = entry.JournalEntryType?.TypeName,
            Status = entry.Status,
            TotalDebit = entry.TotalDebit,
            TotalCredit = entry.TotalCredit,
            Lines = entry.Lines.Select(l => new JournalEntryLineDto
            {
                LineID = l.LineID,
                JournalEntryID = l.JournalEntryID,
                AccountID = l.AccountID,
                AccountCode = l.Account?.AccountCode ?? "",
                AccountName = l.Account?.AccountName ?? "",
                CostCenterID = l.CostCenterID,
                CostCenterName = l.CostCenter?.Name,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        }).ToList();

        return ApiResponse<JournalRegisterDto>.Success(new JournalRegisterDto
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            TotalEntries = total,
            Page = filter.Page,
            PageSize = filter.PageSize,
            GrandTotalDebit = grandTotalDebit,
            GrandTotalCredit = grandTotalCredit,
            Entries = entryDtos
        });
    }

    public async Task<ApiResponse<CashFlowStatementDto>> GetCashFlowStatementAsync(int fiscalYearId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<CashFlowStatementDto>.Fail("السنة المالية غير موجودة");

        var treasuryAccounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => a.AccountCode.StartsWith("18") && a.AccountLevel >= 4 && a.IsPostable && !a.IsDeleted)
            .ToListAsync();

        if (treasuryAccounts.Count == 0)
            return ApiResponse<CashFlowStatementDto>.Fail("حسابات الخزينة غير موجودة في الدليل");

        var treasuryAccountIds = treasuryAccounts.Select(a => a.AccountID).ToHashSet();

        // استعلام واحد مع GroupJoin بدلاً من تحميل الكل في الذاكرة
        var cashFlowData = await _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .AsNoTracking()
            .Where(l => treasuryAccountIds.Contains(l.AccountID)
                && !l.JournalEntry.IsDeleted
                && l.JournalEntry.Status == "Posted"
                && l.JournalEntry.EntryDate >= fiscalYear.StartDate
                && l.JournalEntry.EntryDate <= fiscalYear.EndDate)
            .Select(l => new
            {
                l.JournalEntryID,
                l.Debit,
                l.Credit,
                l.AccountID
            })
            .ToListAsync();

        var entryIds = cashFlowData.Select(l => l.JournalEntryID).Distinct().ToHashSet();

        var pairedLines = await _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.Account)
            .AsNoTracking()
            .Where(l => entryIds.Contains(l.JournalEntryID)
                && !treasuryAccountIds.Contains(l.AccountID)
                && !l.IsDeleted)
            .Select(l => new
            {
                l.JournalEntryID,
                l.AccountID,
                l.Account!.AccountCode,
                l.Account.AccountName
            })
            .ToListAsync();

        var pairedDict = pairedLines
            .GroupBy(l => l.JournalEntryID)
            .ToDictionary(g => g.Key, g => g.ToList());

        var operatingItems = new List<CashFlowItemDto>();
        var investingItems = new List<CashFlowItemDto>();
        var financingItems = new List<CashFlowItemDto>();

        decimal operatingInflow = 0, operatingOutflow = 0;
        decimal investingInflow = 0, investingOutflow = 0;
        decimal financingInflow = 0, financingOutflow = 0;

        foreach (var line in cashFlowData)
        {
            if (!pairedDict.TryGetValue(line.JournalEntryID, out var relatedLines))
                continue;

            foreach (var related in relatedLines)
            {
                var amount = line.Debit > 0 ? line.Debit : line.Credit;
                var isInflow = line.Debit > 0;
                var classification = ClassifyCashFlow(related.AccountCode);

                var item = new CashFlowItemDto
                {
                    AccountID = related.AccountID,
                    AccountCode = related.AccountCode,
                    AccountName = related.AccountName ?? "",
                    Amount = amount,
                    Type = isInflow ? "Inflow" : "Outflow"
                };

                switch (classification)
                {
                    case "Operating":
                        if (isInflow) operatingInflow += amount; else operatingOutflow += amount;
                        operatingItems.Add(item);
                        break;
                    case "Investing":
                        if (isInflow) investingInflow += amount; else investingOutflow += amount;
                        investingItems.Add(item);
                        break;
                    case "Financing":
                        if (isInflow) financingInflow += amount; else financingOutflow += amount;
                        financingItems.Add(item);
                        break;
                }
            }
        }

        // حساب الرصيد الافتتاحي للخزينة
        var openingBalance = 0m;
        var firstPeriod = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == fiscalYearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync();

        if (firstPeriod != null)
        {
            var openingBalanceRecords = await _unitOfWork.AccountBalances.Query()
                .Where(b => treasuryAccountIds.Contains(b.AccountID)
                    && b.PeriodID == firstPeriod.PeriodID
                    && !b.IsDeleted)
                .ToListAsync();

            openingBalance = openingBalanceRecords.Sum(b => b.OpeningDebit - b.OpeningCredit);

            // إضافة حركات ما قبل بداية السنة المالية
            var preYearLines = await _unitOfWork.JournalEntryLines.Query()
                .Include(l => l.JournalEntry)
                .Where(l => treasuryAccountIds.Contains(l.AccountID)
                    && !l.JournalEntry.IsDeleted
                    && l.JournalEntry.Status == "Posted"
                    && l.JournalEntry.EntryDate < fiscalYear.StartDate)
                .SumAsync(l => l.Debit - l.Credit);

            openingBalance += preYearLines;
        }

        return ApiResponse<CashFlowStatementDto>.Success(new CashFlowStatementDto
        {
            FiscalYearID = fiscalYearId,
            YearName = fiscalYear.YearName,
            FromDate = fiscalYear.StartDate,
            ToDate = fiscalYear.EndDate,
            OperatingCashInflow = operatingInflow,
            OperatingCashOutflow = operatingOutflow,
            InvestingCashInflow = investingInflow,
            InvestingCashOutflow = investingOutflow,
            FinancingCashInflow = financingInflow,
            FinancingCashOutflow = financingOutflow,
            BeginningCashBalance = openingBalance,
            OperatingItems = operatingItems,
            InvestingItems = investingItems,
            FinancingItems = financingItems
        });
    }

    public async Task<ApiResponse<AccountingDashboardDto>> GetDashboardAsync(int? fiscalYearId)
    {
        var cacheKey = $"dashboard_{fiscalYearId ?? 0}";
        if (_cache.TryGetValue(cacheKey, out AccountingDashboardDto? cached))
            return ApiResponse<AccountingDashboardDto>.Success(cached!);

        var today = DateTime.UtcNow;
        var fy = fiscalYearId.HasValue
            ? await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId.Value)
            : await _unitOfWork.FiscalYears.Query().FirstOrDefaultAsync();

        var fromDate = fy?.StartDate ?? new DateTime(today.Year, 1, 1);
        var toDate = fy?.EndDate ?? new DateTime(today.Year, 12, 31);

        var balances = await GetAccountBalancesUpToDateAsync(toDate, considerAccountNature: true);

        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.IsPostable)
            .ToListAsync();

        decimal totalAssets = 0;
        decimal totalLiabilities = 0;
        decimal totalEquity = 0;

        foreach (var account in accounts)
        {
            if (!balances.TryGetValue(account.AccountID, out var bal)) continue;

            if (account.BookNumber == 1)
                totalAssets += bal;
            else if (account.BookNumber == 2)
            {
                if (account.AccountCode.StartsWith("21"))
                    totalEquity += bal;
                else
                    totalLiabilities += bal;
            }
        }

        var entries = await _unitOfWork.JournalEntries.Query()
            .Include(e => e.JournalEntryType)
            .Where(e => !e.IsDeleted && e.EntryDate >= fromDate && e.EntryDate <= toDate)
            .ToListAsync();

        var recentEntries = entries.OrderByDescending(e => e.CreatedAt).Take(10)
            .Select(e => new RecentJournalEntryDto
            {
                JournalEntryID = e.JournalEntryID,
                EntryNumber = e.EntryNumber,
                EntryDate = e.EntryDate,
                Description = e.Description,
                JournalEntryTypeID = e.JournalEntryTypeID,
                JournalEntryTypeName = e.JournalEntryType?.TypeName,
                Status = e.Status,
                TotalDebit = e.TotalDebit
            }).ToList();

        int? fyId = fy?.FiscalYearID;
        var periods = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => fyId.HasValue && p.FiscalYearID == fyId.Value && !p.IsDeleted)
            .ToListAsync();

        var result = new AccountingDashboardDto
        {
            TotalJournalEntries = entries.Count,
            DraftEntries = entries.Count(e => e.Status == "Draft"),
            PostedEntries = entries.Count(e => e.Status == "Posted"),
            OpenPeriods = periods.Count(p => !p.IsClosed),
            ClosedPeriods = periods.Count(p => p.IsClosed),
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            RecentEntries = recentEntries
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return ApiResponse<AccountingDashboardDto>.Success(result);
    }

    public async Task<ApiResponse<AccountBalanceDto>> GetAccountBalanceAsync(int accountId, int periodId)
    {
        var balance = await _unitOfWork.AccountBalances.Query()
            .Include(b => b.Account)
            .Include(b => b.Period)
            .FirstOrDefaultAsync(b => b.AccountID == accountId && b.PeriodID == periodId && !b.IsDeleted);

        if (balance == null)
        {
            var account = await _unitOfWork.ChartOfAccounts.GetByIdAsync(accountId);
            var period = await _unitOfWork.FiscalPeriods.GetByIdAsync(periodId);
            if (account == null || period == null)
                return ApiResponse<AccountBalanceDto>.Fail("الحساب أو الفترة غير موجودة");

            return ApiResponse<AccountBalanceDto>.Success(new AccountBalanceDto
            {
                AccountID = accountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountNature = account.AccountNature,
                PeriodID = periodId,
                PeriodName = period.PeriodName
            });
        }

        if (balance.Account == null || balance.Period == null)
            return ApiResponse<AccountBalanceDto>.Fail("بيانات الحساب أو الفترة غير مكتملة");

        var netBalance = balance.Account.AccountNature == "Debit"
            ? balance.ClosingDebit - balance.ClosingCredit
            : balance.ClosingCredit - balance.ClosingDebit;

        return ApiResponse<AccountBalanceDto>.Success(new AccountBalanceDto
        {
            BalanceID = balance.BalanceID,
            AccountID = balance.AccountID,
            AccountCode = balance.Account.AccountCode,
            AccountName = balance.Account.AccountName,
            AccountNature = balance.Account.AccountNature,
            PeriodID = balance.PeriodID,
            PeriodName = balance.Period.PeriodName,
            OpeningDebit = balance.OpeningDebit,
            OpeningCredit = balance.OpeningCredit,
            PeriodDebit = balance.PeriodDebit,
            PeriodCredit = balance.PeriodCredit,
            ClosingDebit = balance.ClosingDebit,
            ClosingCredit = balance.ClosingCredit,
            NetBalance = netBalance
        });
    }

    private async Task<decimal> GetOpeningBalanceAsync(int accountId, int? periodId, DateTime? fromDate)
    {
        decimal rawBalance = 0;

        if (periodId.HasValue)
        {
            var balance = await _unitOfWork.AccountBalances.Query()
                .FirstOrDefaultAsync(b => b.AccountID == accountId && b.PeriodID == periodId.Value && !b.IsDeleted);
            if (balance != null)
                rawBalance = balance.OpeningDebit - balance.OpeningCredit;
        }
        else if (fromDate.HasValue)
        {
            var period = await _unitOfWork.FiscalPeriods.Query()
                .FirstOrDefaultAsync(p => p.StartDate <= fromDate.Value && p.EndDate >= fromDate.Value && !p.IsDeleted);

            if (period != null)
            {
                var accountBalance = await _unitOfWork.AccountBalances.Query()
                    .FirstOrDefaultAsync(b => b.AccountID == accountId && b.PeriodID == period.PeriodID && !b.IsDeleted);
                if (accountBalance != null)
                    rawBalance = accountBalance.OpeningDebit - accountBalance.OpeningCredit;

                var periodLines = _unitOfWork.JournalEntryLines.Query()
                    .Include(l => l.JournalEntry)
                    .Where(l => l.AccountID == accountId && !l.JournalEntry.IsDeleted
                        && l.JournalEntry.Status == "Posted"
                        && l.JournalEntry.PeriodID == period.PeriodID
                        && l.JournalEntry.EntryDate < fromDate.Value);
                rawBalance += await periodLines.SumAsync(l => l.Debit - l.Credit);
            }
            else
            {
                var beforeLines = _unitOfWork.JournalEntryLines.Query()
                    .Include(l => l.JournalEntry)
                    .Where(l => l.AccountID == accountId && !l.JournalEntry.IsDeleted
                        && l.JournalEntry.Status == "Posted"
                        && l.JournalEntry.EntryDate < fromDate.Value);
                rawBalance = await beforeLines.SumAsync(l => l.Debit - l.Credit);
            }
        }

        return rawBalance;
    }

    private async Task<Dictionary<int, decimal>> GetAccountBalancesUpToDateAsync(DateTime date, bool considerAccountNature = false)
    {
        var lines = _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .Where(l => !l.JournalEntry.IsDeleted && l.JournalEntry.Status == "Posted"
                && l.JournalEntry.EntryDate <= date);

        var grouped = await lines.GroupBy(l => l.AccountID)
            .Select(g => new
            {
                AccountID = g.Key,
                Balance = g.Sum(l => l.Debit - l.Credit)
            })
            .ToListAsync();

        var result = new Dictionary<int, decimal>();

        if (considerAccountNature)
        {
            var accountIds = grouped.Select(g => g.AccountID).ToList();
            var accounts = await _unitOfWork.ChartOfAccounts.Query()
                .AsNoTracking()
                .Where(a => accountIds.Contains(a.AccountID))
                .Select(a => new { a.AccountID, a.AccountNature })
                .ToDictionaryAsync(a => a.AccountID, a => a.AccountNature);

            foreach (var item in grouped)
            {
                if (accounts.TryGetValue(item.AccountID, out var nature) && nature == "Credit")
                    result[item.AccountID] = -item.Balance;
                else
                    result[item.AccountID] = item.Balance;
            }
        }
        else
        {
            foreach (var item in grouped)
                result[item.AccountID] = item.Balance;
        }

        return result;
    }

    private static string ClassifyCashFlow(string accountCode)
    {
        if (string.IsNullOrEmpty(accountCode)) return "Operating";
        if (accountCode.StartsWith(AccountingConstants.CashFlowPrefixes.Investing)) return "Investing";
        if (accountCode.StartsWith(AccountingConstants.CashFlowPrefixes.FinancingEquity) || accountCode.StartsWith(AccountingConstants.CashFlowPrefixes.FinancingLiability)) return "Financing";
        return "Operating";
    }

public async Task<ApiResponse<SubledgerReportResultDto>> GetSubledgerReportAsync(int accountId, DateTime? fromDate, DateTime? toDate)
    {
        var account = await _unitOfWork.ChartOfAccounts.GetByIdAsync(accountId);
        if (account == null)
            return ApiResponse<SubledgerReportResultDto>.Fail("الحساب غير موجود");

        var accountIds = new List<int> { accountId };
        var childAccounts = await GetAllChildAccountIdsAsync(accountId);
        accountIds.AddRange(childAccounts);

        var query = _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry).ThenInclude(e => e.Period)
            .Include(l => l.Account)
            .Where(l => !l.JournalEntry.IsDeleted
                && l.JournalEntry.Status == "Posted"
                && accountIds.Contains(l.AccountID));

        if (fromDate.HasValue)
            query = query.Where(l => l.JournalEntry.EntryDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.JournalEntry.EntryDate <= toDate.Value);

        var entryLines = await query.AsNoTracking()
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ThenBy(l => l.LineID)
            .ToListAsync();

        decimal openingBalance = 0;
        if (fromDate.HasValue)
        {
            var openingDate = fromDate.Value.AddDays(-1);
            var openingLines = _unitOfWork.JournalEntryLines.Query()
                .Include(l => l.JournalEntry)
                .Where(l => !l.JournalEntry.IsDeleted
                    && l.JournalEntry.Status == "Posted"
                    && accountIds.Contains(l.AccountID)
                    && l.JournalEntry.EntryDate <= openingDate);

            openingBalance = await openingLines.SumAsync(l => l.Debit - l.Credit);

            if (openingBalance == 0)
            {
                var openingBalanceTypeId = await GetJournalEntryTypeIDAsync("OPN");
                var sameDateLines = _unitOfWork.JournalEntryLines.Query()
                    .Include(l => l.JournalEntry)
                    .Where(l => !l.JournalEntry.IsDeleted
                        && l.JournalEntry.Status == "Posted"
                        && l.JournalEntry.JournalEntryTypeID == openingBalanceTypeId
                        && accountIds.Contains(l.AccountID)
                        && l.JournalEntry.EntryDate == fromDate.Value);

                openingBalance = await sameDateLines.SumAsync(l => l.Debit - l.Credit);
            }
        }

        decimal running = openingBalance;
        var lines = new List<SubledgerReportLineDto>();

        foreach (var line in entryLines)
        {
            running += line.Debit - line.Credit;
            lines.Add(new SubledgerReportLineDto
            {
                EntryDate = line.JournalEntry.EntryDate,
                EntryNumber = line.JournalEntry.EntryNumber,
                AccountCode = line.Account?.AccountCode,
                AccountName = line.Account?.AccountName,
                Description = line.Description ?? line.JournalEntry.Description,
                Debit = line.Debit,
                Credit = line.Credit,
                RunningBalance = running
            });
        }

        return ApiResponse<SubledgerReportResultDto>.Success(new SubledgerReportResultDto
        {
            AccountID = accountId,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            OpeningBalance = openingBalance,
            TotalDebit = entryLines.Sum(l => l.Debit),
            TotalCredit = entryLines.Sum(l => l.Credit),
            ClosingBalance = running,
            Entries = lines
        });
    }
}