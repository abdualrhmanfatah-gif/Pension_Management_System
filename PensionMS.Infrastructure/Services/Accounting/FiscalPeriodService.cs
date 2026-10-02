using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

public class FiscalPeriodService : IFiscalPeriodService
{
    private readonly IUnitOfWork _unitOfWork;

    public FiscalPeriodService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<FiscalYearSummaryDto>>> GetAllFiscalYearsAsync()
    {
        var years = await _unitOfWork.FiscalYears.Query()
            .AsNoTracking()
            .Where(y => !y.IsDeleted)
            .OrderByDescending(y => y.YearName)
            .ToListAsync();

        return ApiResponse<List<FiscalYearSummaryDto>>.Success(years.Select(y => new FiscalYearSummaryDto
        {
            FiscalYearID = y.FiscalYearID,
            YearName = y.YearName,
            StartDate = y.StartDate,
            EndDate = y.EndDate,
            TotalBudget = y.TotalBudget,
            IsClosed = y.IsClosed,
        }).ToList());
    }

    public async Task<ApiResponse<FiscalYearSummaryDto>> CreateFiscalYearAsync(CreateFiscalYearDto dto)
    {
        var exists = await _unitOfWork.FiscalYears.Query()
            .AnyAsync(y => y.YearName == dto.YearName && !y.IsDeleted);
        if (exists)
            return ApiResponse<FiscalYearSummaryDto>.Fail($"السنة المالية {dto.YearName} موجودة مسبقاً");

        if (!int.TryParse(dto.YearName, out var year))
            return ApiResponse<FiscalYearSummaryDto>.Fail("اسم السنة يجب أن يكون رقماً (مثل 2027)");

        var fiscalYear = new FiscalYear
        {
            YearName = dto.YearName,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalBudget = dto.TotalBudget,
            IsClosed = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.FiscalYears.AddAsync(fiscalYear);
        await _unitOfWork.SaveChangesAsync();

        var monthsInQuarter = 3;
        var quarters = new List<Quarter>();
        for (int q = 0; q < 4; q++)
        {
            var qStart = dto.StartDate.AddMonths(q * monthsInQuarter);
            var qEnd = qStart.AddMonths(monthsInQuarter).AddDays(-1);
            if (qEnd > dto.EndDate) qEnd = dto.EndDate;

            var quarter = new Quarter
            {
                FiscalYearID = fiscalYear.FiscalYearID,
                QuarterNumber = q + 1,
                StartDate = qStart,
                EndDate = qEnd,
                IsClosed = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Quarters.AddAsync(quarter);
            quarters.Add(quarter);
        }
        await _unitOfWork.SaveChangesAsync();

        var monthNames = new[] { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
        var totalMonths = (dto.EndDate.Year - dto.StartDate.Year) * 12 + dto.EndDate.Month - dto.StartDate.Month + 1;

        for (int m = 0; m < totalMonths; m++)
        {
            var mDate = dto.StartDate.AddMonths(m);
            var mStart = new DateTime(mDate.Year, mDate.Month, 1);
            var mEnd = new DateTime(mDate.Year, mDate.Month, DateTime.DaysInMonth(mDate.Year, mDate.Month));
            if (mEnd > dto.EndDate) mEnd = dto.EndDate;
            var quarterIndex = Math.Min(m / 3, 3);

            var period = new FiscalPeriod
            {
                FiscalYearID = fiscalYear.FiscalYearID,
                QuarterID = quarters[quarterIndex].QuarterID,
                PeriodNumber = m + 1,
                PeriodName = $"{monthNames[mDate.Month - 1]} {mDate.Year}",
                StartDate = mStart,
                EndDate = mEnd,
                IsClosed = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.FiscalPeriods.AddAsync(period);
        }
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<FiscalYearSummaryDto>.Success(new FiscalYearSummaryDto
        {
            FiscalYearID = fiscalYear.FiscalYearID,
            YearName = fiscalYear.YearName,
            StartDate = fiscalYear.StartDate,
            EndDate = fiscalYear.EndDate,
            TotalBudget = fiscalYear.TotalBudget,
            IsClosed = fiscalYear.IsClosed,
        });
    }

    public async Task<ApiResponse<List<FiscalPeriodDto>>> GetFiscalPeriodsAsync(int fiscalYearId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && (y.FiscalYearID == fiscalYearId || y.YearName == fiscalYearId.ToString()));

        var yearId = fiscalYear?.FiscalYearID ?? fiscalYearId;

        var periods = await _unitOfWork.FiscalPeriods.Query()
            .AsNoTracking()
            .Where(p => p.FiscalYearID == yearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .ToListAsync();

        return ApiResponse<List<FiscalPeriodDto>>.Success(periods.Select(MapPeriodToDto).ToList());
    }

    public async Task<ApiResponse<FiscalPeriodDto>> GetActivePeriodAsync(int fiscalYearId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(y => !y.IsDeleted && (y.FiscalYearID == fiscalYearId || y.YearName == fiscalYearId.ToString()));

        if (fiscalYear == null)
            return ApiResponse<FiscalPeriodDto>.Fail("لا توجد سنة محاسبية بهذا الرقم");

        var period = await _unitOfWork.FiscalPeriods.Query()
            .AsNoTracking()
            .Where(p => p.FiscalYearID == fiscalYear.FiscalYearID && !p.IsClosed && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync();

        if (period == null)
            return ApiResponse<FiscalPeriodDto>.Fail("لا توجد فترة نشطة مفتوحة");

        return ApiResponse<FiscalPeriodDto>.Success(MapPeriodToDto(period));
    }

    public async Task<ApiResponse<List<QuarterDto>>> GetQuartersAsync(int fiscalYearId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && (y.FiscalYearID == fiscalYearId || y.YearName == fiscalYearId.ToString()));

        var yearId = fiscalYear?.FiscalYearID ?? fiscalYearId;

        var quarters = await _unitOfWork.Quarters.Query()
            .AsNoTracking()
            .Where(q => q.FiscalYearID == yearId && !q.IsDeleted)
            .OrderBy(q => q.QuarterNumber)
            .ToListAsync();

        var periods = await _unitOfWork.FiscalPeriods.Query()
            .AsNoTracking()
            .Where(p => p.FiscalYearID == yearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .ToListAsync();

        var quarterDtos = quarters.Select(q =>
        {
            var quarterPeriods = periods.Where(p => p.QuarterID == q.QuarterID).ToList();
            return new QuarterDto
            {
                QuarterID = q.QuarterID,
                FiscalYearID = q.FiscalYearID,
                QuarterNumber = q.QuarterNumber,
                QuarterName = $"Q{q.QuarterNumber} {fiscalYear?.YearName ?? yearId.ToString()}",
                StartDate = q.StartDate,
                EndDate = q.EndDate,
                IsClosed = q.IsClosed,
                Periods = quarterPeriods.Select(MapPeriodToDto).ToList()
            };
        }).ToList();

        return ApiResponse<List<QuarterDto>>.Success(quarterDtos);
    }

    public async Task<ApiResponse<QuarterDto>> CloseQuarterAsync(int quarterId, int userId)
    {
        var quarter = await _unitOfWork.Quarters.GetByIdAsync(quarterId);
        if (quarter == null || quarter.IsDeleted)
            return ApiResponse<QuarterDto>.Fail("الفصل المالي غير موجود");

        if (quarter.IsClosed)
            return ApiResponse<QuarterDto>.Fail("الفصل مغلق مسبقاً");

        var previousQuarter = await _unitOfWork.Quarters.Query()
            .Where(q => q.FiscalYearID == quarter.FiscalYearID
                && q.QuarterNumber == quarter.QuarterNumber - 1
                && !q.IsDeleted)
            .FirstOrDefaultAsync();

        if (previousQuarter != null && !previousQuarter.IsClosed)
            return ApiResponse<QuarterDto>.Fail("يجب إغلاق الفصل المالي السابق أولاّ");

        var periods = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.QuarterID == quarterId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .ToListAsync();

        for (int i = 1; i < periods.Count; i++)
        {
            if (!periods[i - 1].IsClosed && periods[i].IsClosed)
            {
                return ApiResponse<QuarterDto>.Fail(
                    $"لا يمكن إغلاق الفترة {periods[i].PeriodName} قبل إغلاق {periods[i - 1].PeriodName}");
            }
        }

        var periodIds = periods.Select(p => p.PeriodID).ToList();
        var draftEntries = await _unitOfWork.JournalEntries.Query()
            .Where(e => periodIds.Contains(e.PeriodID) && e.Status == "Draft" && !e.IsDeleted)
            .CountAsync();

        if (draftEntries > 0)
            return ApiResponse<QuarterDto>.Fail($"يوجد {draftEntries} قيد مسودة يجب ترحيلها أولاّ");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var period in periods.Where(p => !p.IsClosed))
            {
                period.IsClosed = true;
                period.ClosedByUserID = userId;
                period.ClosedAt = DateTime.UtcNow;
            }

            foreach (var period in periods)
            {
                var balances = await _unitOfWork.AccountBalances.Query()
                    .Where(b => b.PeriodID == period.PeriodID && !b.IsDeleted)
                    .ToListAsync();

                foreach (var balance in balances)
                {
                    balance.ClosingDebit = balance.OpeningDebit + balance.PeriodDebit;
                    balance.ClosingCredit = balance.OpeningCredit + balance.PeriodCredit;
                    _unitOfWork.AccountBalances.Update(balance);
                }
            }

            quarter.IsClosed = true;
            quarter.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(quarter.FiscalYearID);

        return ApiResponse<QuarterDto>.Success(new QuarterDto
        {
            QuarterID = quarter.QuarterID,
            FiscalYearID = quarter.FiscalYearID,
            QuarterNumber = quarter.QuarterNumber,
            QuarterName = $"Q{quarter.QuarterNumber} {fiscalYear?.YearName ?? quarter.FiscalYearID.ToString()}",
            StartDate = quarter.StartDate,
            EndDate = quarter.EndDate,
            IsClosed = quarter.IsClosed,
            Periods = periods.Select(MapPeriodToDto).ToList()
        }, "تم إغلاق الفصل بنجاح");
    }

    public async Task<ApiResponse<QuarterDetailsDto>> GetQuarterDetailsAsync(int quarterId)
    {
        var quarter = await _unitOfWork.Quarters.GetByIdAsync(quarterId);
        if (quarter == null || quarter.IsDeleted)
            return ApiResponse<QuarterDetailsDto>.Fail("الفصل غير موجود");

        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(quarter.FiscalYearID);
        var periods = await _unitOfWork.FiscalPeriods.Query()
            .AsNoTracking()
            .Where(p => p.QuarterID == quarterId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .ToListAsync();

        var periodIds = periods.Select(p => p.PeriodID).ToList();

        var treasuryAccount = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountCode == AccountingConstants.AccountCodes.Treasury && !a.IsDeleted);
        if (treasuryAccount == null)
            return ApiResponse<QuarterDetailsDto>.Fail("حساب الخزينة الرئيسية (1811) غير موجود في الدليل");

        var treasuryAccountId = treasuryAccount.AccountID;

        var balances = await _unitOfWork.AccountBalances.Query()
            .Where(b => b.AccountID == treasuryAccountId && periodIds.Contains(b.PeriodID) && !b.IsDeleted)
            .OrderBy(b => b.PeriodID)
            .ToListAsync();

        var treasuryLines = await _unitOfWork.JournalEntryLines.Query()
            .Where(l => l.AccountID == treasuryAccountId
                && periodIds.Contains(l.JournalEntry!.PeriodID)
                && !l.JournalEntry!.IsDeleted
                && l.JournalEntry!.Status == "Posted")
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .ToListAsync();

        var periodBreakdowns = periods.Select(p =>
        {
            var balance = balances.FirstOrDefault(b => b.PeriodID == p.PeriodID);
            var periodLines = treasuryLines.Where(l => l.JournalEntry.PeriodID == p.PeriodID).ToList();

            var withdrawn = periodLines.Where(l => l.Debit > 0).Sum(l => l.Debit);
            var spent = periodLines.Where(l => l.Credit > 0).Sum(l => l.Credit);

            var opening = (balance?.OpeningDebit ?? 0) - (balance?.OpeningCredit ?? 0);
            var closing = (balance?.ClosingDebit ?? 0) - (balance?.ClosingCredit ?? 0);

            return new PeriodBreakdownDto
            {
                PeriodID = p.PeriodID,
                PeriodName = p.PeriodName,
                PeriodNumber = p.PeriodNumber,
                OpeningBalance = opening,
                Withdrawn = withdrawn,
                Spent = spent,
                ClosingBalance = closing
            };
        }).ToList();

        var treasuryCreditLines = treasuryLines.Where(l => l.Credit > 0).ToList();
        var entryIds = treasuryCreditLines.Select(l => l.JournalEntryID).Distinct().ToList();

        var pairedDebitLines = await _unitOfWork.JournalEntryLines.Query()
            .Where(l => entryIds.Contains(l.JournalEntryID)
                && l.Debit > 0
                && l.AccountID != treasuryAccountId
                && !l.IsDeleted)
            .Include(l => l.Account)
            .ToListAsync();

        var expenseBreakdown = new List<ExpenseBreakdownDto>();
        foreach (var creditLine in treasuryCreditLines)
        {
            var relatedDebits = pairedDebitLines.Where(d => d.JournalEntryID == creditLine.JournalEntryID).ToList();
            if (!relatedDebits.Any()) continue;

            var totalDebits = relatedDebits.Sum(d => d.Debit);
            foreach (var debitLine in relatedDebits)
            {
                var ratio = totalDebits > 0 ? debitLine.Debit / totalDebits : 0;
                var allocatedAmount = creditLine.Credit * ratio;

                var existing = expenseBreakdown.FirstOrDefault(e => e.AccountID == debitLine.AccountID);
                if (existing != null)
                {
                    existing.Amount += allocatedAmount;
                    existing.EntryCount++;
                }
                else
                {
                    expenseBreakdown.Add(new ExpenseBreakdownDto
                    {
                        AccountID = debitLine.AccountID,
                        AccountCode = debitLine.Account?.AccountCode ?? "",
                        AccountName = debitLine.Account?.AccountName ?? "",
                        Amount = allocatedAmount,
                        EntryCount = 1
                    });
                }
            }
        }

        expenseBreakdown = expenseBreakdown.OrderByDescending(e => e.Amount).ToList();

        var totalWithdrawn = periodBreakdowns.Sum(p => p.Withdrawn);
        var totalSpent = periodBreakdowns.Sum(p => p.Spent);
        var openingBalance = periodBreakdowns.FirstOrDefault()?.OpeningBalance ?? 0;

        return ApiResponse<QuarterDetailsDto>.Success(new QuarterDetailsDto
        {
            QuarterID = quarter.QuarterID,
            FiscalYearID = quarter.FiscalYearID,
            QuarterNumber = quarter.QuarterNumber,
            QuarterName = $"Q{quarter.QuarterNumber} {fiscalYear?.YearName ?? quarter.FiscalYearID.ToString()}",
            StartDate = quarter.StartDate,
            EndDate = quarter.EndDate,
            IsClosed = quarter.IsClosed,
            TotalWithdrawn = totalWithdrawn,
            TotalSpent = totalSpent,
            OpeningBalance = openingBalance,
            RemainingBalance = openingBalance + totalWithdrawn - totalSpent,
            Periods = periodBreakdowns,
            ExpenseBreakdown = expenseBreakdown
        });
    }

    private static FiscalPeriodDto MapPeriodToDto(FiscalPeriod period)
    {
        return new FiscalPeriodDto
        {
            PeriodID = period.PeriodID,
            FiscalYearID = period.FiscalYearID,
            QuarterID = period.QuarterID,
            PeriodName = period.PeriodName,
            PeriodNumber = period.PeriodNumber,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            IsClosed = period.IsClosed,
            ClosedAt = period.ClosedAt,
            CreatedAt = period.CreatedAt
        };
    }
}
