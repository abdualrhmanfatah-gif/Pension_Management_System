using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

/// <summary>
/// خدمة الموازنة — التخطيط والمتابعة
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;

    public BudgetService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ═══════════════════════════════════════
    // CRUD Operations
    // ═══════════════════════════════════════

    public async Task<ApiResponse<List<BudgetItemDto>>> GetBudgetItemsAsync(BudgetFilterDto filter)
    {
        var query = _unitOfWork.BudgetItems.Query()
            .Where(b => b.FiscalYearID == filter.FiscalYearID && !b.IsDeleted);

        if (filter.QuarterID.HasValue)
            query = query.Where(b => b.QuarterID == filter.QuarterID.Value);

        if (filter.AccountID.HasValue)
            query = query.Where(b => b.AccountID == filter.AccountID.Value);

        var items = await query
            .Include(b => b.Account)
            .Include(b => b.FiscalYear)
            .Include(b => b.Quarter)
            .Include(b => b.Period)
            .ToListAsync();

        var dtos = new List<BudgetItemDto>();
        foreach (var item in items)
        {
            var actualAmount = await CalculateActualAmountAsync(
                item.AccountID, item.FiscalYearID, item.QuarterID, item.PeriodID);

            dtos.Add(new BudgetItemDto
            {
                BudgetItemID = item.BudgetItemID,
                FiscalYearID = item.FiscalYearID,
                FiscalYearName = item.FiscalYear?.YearName,
                QuarterID = item.QuarterID,
                QuarterName = item.Quarter != null
                    ? $"Q{item.Quarter.QuarterNumber} {item.FiscalYear?.YearName}" : null,
                PeriodID = item.PeriodID,
                PeriodName = item.Period?.PeriodName,
                AccountID = item.AccountID,
                AccountCode = item.Account?.AccountCode ?? "",
                AccountName = item.Account?.AccountName ?? "",
                PlannedAmount = item.PlannedAmount,
                ActualAmount = actualAmount,
                Notes = item.Notes
            });
        }

        return ApiResponse<List<BudgetItemDto>>.Success(dtos);
    }

    public async Task<ApiResponse<BudgetItemDto>> GetBudgetItemByIdAsync(int id)
    {
        var item = await _unitOfWork.BudgetItems.Query()
            .Include(b => b.Account)
            .Include(b => b.FiscalYear)
            .Include(b => b.Quarter)
            .Include(b => b.Period)
            .FirstOrDefaultAsync(b => b.BudgetItemID == id && !b.IsDeleted);

        if (item == null)
            return ApiResponse<BudgetItemDto>.Fail("بند الموازنة غير موجود");

        var actualAmount = await CalculateActualAmountAsync(
            item.AccountID, item.FiscalYearID, item.QuarterID, item.PeriodID);

        return ApiResponse<BudgetItemDto>.Success(new BudgetItemDto
        {
            BudgetItemID = item.BudgetItemID,
            FiscalYearID = item.FiscalYearID,
            FiscalYearName = item.FiscalYear?.YearName,
            QuarterID = item.QuarterID,
            QuarterName = item.Quarter != null
                ? $"Q{item.Quarter.QuarterNumber} {item.FiscalYear?.YearName}" : null,
            PeriodID = item.PeriodID,
            PeriodName = item.Period?.PeriodName,
            AccountID = item.AccountID,
            AccountCode = item.Account?.AccountCode ?? "",
            AccountName = item.Account?.AccountName ?? "",
            PlannedAmount = item.PlannedAmount,
            ActualAmount = actualAmount,
            Notes = item.Notes
        });
    }

    public async Task<ApiResponse<BudgetItemDto>> CreateBudgetItemAsync(
        CreateBudgetItemDto dto, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(dto.FiscalYearID);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<BudgetItemDto>.Fail("السنة المالية غير موجودة");

        if (fiscalYear.IsClosed)
            return ApiResponse<BudgetItemDto>.Fail("السنة المالية مغلقة");

        var account = await _unitOfWork.ChartOfAccounts.GetByIdAsync(dto.AccountID);
        if (account == null || account.IsDeleted)
            return ApiResponse<BudgetItemDto>.Fail("الحساب غير موجود");

        var exists = await _unitOfWork.BudgetItems.Query()
            .AnyAsync(b => b.FiscalYearID == dto.FiscalYearID
                && b.AccountID == dto.AccountID
                && b.QuarterID == dto.QuarterID
                && b.PeriodID == dto.PeriodID
                && !b.IsDeleted);

        if (exists)
            return ApiResponse<BudgetItemDto>.Fail("يوجد بند موازنة مسبقاً لهذا الحساب");

        var entity = new BudgetItem
        {
            FiscalYearID = dto.FiscalYearID,
            QuarterID = dto.QuarterID,
            PeriodID = dto.PeriodID,
            AccountID = dto.AccountID,
            PlannedAmount = dto.PlannedAmount,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BudgetItems.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return await GetBudgetItemByIdAsync(entity.BudgetItemID);
    }

    public async Task<ApiResponse<BudgetItemDto>> UpdateBudgetItemAsync(
        int id, UpdateBudgetItemDto dto, int userId)
    {
        var entity = await _unitOfWork.BudgetItems.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<BudgetItemDto>.Fail("بند الموازنة غير موجود");

        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(entity.FiscalYearID);
        if (fiscalYear != null && fiscalYear.IsClosed)
            return ApiResponse<BudgetItemDto>.Fail("لا يمكن تعديل بند في سنة مالية مغلقة");

        entity.PlannedAmount = dto.PlannedAmount;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.BudgetItems.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return await GetBudgetItemByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> DeleteBudgetItemAsync(int id, int userId)
    {
        var entity = await _unitOfWork.BudgetItems.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<bool>.Fail("بند الموازنة غير موجود");

        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(entity.FiscalYearID);
        if (fiscalYear != null && fiscalYear.IsClosed)
            return ApiResponse<bool>.Fail("لا يمكن حذف بند في سنة مالية مغلقة");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserID = userId;

        _unitOfWork.BudgetItems.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف بند الموازنة بنجاح");
    }

    // ═══════════════════════════════════════
    // BULK OPERATIONS
    // ═══════════════════════════════════════

    public async Task<ApiResponse<bool>> BulkCreateBudgetItemsAsync(
        List<CreateBudgetItemDto> items, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(items.First().FiscalYearID);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<bool>.Fail("السنة المالية غير موجودة");

        if (fiscalYear.IsClosed)
            return ApiResponse<bool>.Fail("السنة المالية مغلقة");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var dto in items)
            {
                var exists = await _unitOfWork.BudgetItems.Query()
                    .AnyAsync(b => b.FiscalYearID == dto.FiscalYearID
                        && b.AccountID == dto.AccountID
                        && b.QuarterID == dto.QuarterID
                        && b.PeriodID == dto.PeriodID
                        && !b.IsDeleted);

                if (!exists)
                {
                    var entity = new BudgetItem
                    {
                        FiscalYearID = dto.FiscalYearID,
                        QuarterID = dto.QuarterID,
                        PeriodID = dto.PeriodID,
                        AccountID = dto.AccountID,
                        PlannedAmount = dto.PlannedAmount,
                        Notes = dto.Notes,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.BudgetItems.AddAsync(entity);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return ApiResponse<bool>.Success(true, $"تم إنشاء {items.Count} بند موازنة بنجاح");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // ═══════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════

    public async Task<ApiResponse<BudgetReportDto>> GetBudgetReportAsync(int fiscalYearId)
    {
        // Resolve year number (e.g. 2026) to FiscalYearID
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && (y.FiscalYearID == fiscalYearId || y.YearName == fiscalYearId.ToString()));

        if (fiscalYear == null)
            return ApiResponse<BudgetReportDto>.Fail("السنة المالية غير موجودة");

        var yearId = fiscalYear.FiscalYearID;

        // جلب بنود الموازنة السنوية فقط (بدون فصل أو شهر محدد)
        var items = await _unitOfWork.BudgetItems.Query()
            .Where(b => b.FiscalYearID == yearId && !b.IsDeleted
                && b.QuarterID == null && b.PeriodID == null)
            .Include(b => b.Account)
            .ToListAsync();

        var dtos = new List<BudgetItemDto>();
        foreach (var item in items)
        {
            var actualAmount = await CalculateActualAmountAsync(
                item.AccountID, yearId, null, null);

            dtos.Add(new BudgetItemDto
            {
                BudgetItemID = item.BudgetItemID,
                FiscalYearID = item.FiscalYearID,
                AccountID = item.AccountID,
                AccountCode = item.Account?.AccountCode ?? "",
                AccountName = item.Account?.AccountName ?? "",
                PlannedAmount = item.PlannedAmount,
                ActualAmount = actualAmount
            });
        }

        return ApiResponse<BudgetReportDto>.Success(new BudgetReportDto
        {
            FiscalYearID = yearId,
            YearName = fiscalYear.YearName,
            TotalPlanned = dtos.Sum(d => d.PlannedAmount),
            TotalActual = dtos.Sum(d => d.ActualAmount),
            Items = dtos
        });
    }

    public async Task<ApiResponse<QuarterlyBudgetReportDto>> GetQuarterlyBudgetReportAsync(
        int fiscalYearId)
    {
        // Resolve year number (e.g. 2026) to FiscalYearID
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && (y.FiscalYearID == fiscalYearId || y.YearName == fiscalYearId.ToString()));

        if (fiscalYear == null)
            return ApiResponse<QuarterlyBudgetReportDto>.Fail("السنة المالية غير موجودة");

        var yearId = fiscalYear.FiscalYearID;

        var quarters = await _unitOfWork.Quarters.Query()
            .Where(q => q.FiscalYearID == yearId && !q.IsDeleted)
            .OrderBy(q => q.QuarterNumber)
            .ToListAsync();

        var quarterDtos = new List<QuarterBudgetDto>();
        foreach (var quarter in quarters)
        {
            var items = await _unitOfWork.BudgetItems.Query()
                .Where(b => b.QuarterID == quarter.QuarterID && !b.IsDeleted)
                .Include(b => b.Account)
                .ToListAsync();

            var dtos = new List<BudgetItemDto>();
            foreach (var item in items)
            {
                var actualAmount = await CalculateActualAmountAsync(
                    item.AccountID, item.FiscalYearID, quarter.QuarterID, null);

                dtos.Add(new BudgetItemDto
                {
                    BudgetItemID = item.BudgetItemID,
                    AccountID = item.AccountID,
                    AccountCode = item.Account?.AccountCode ?? "",
                    AccountName = item.Account?.AccountName ?? "",
                    PlannedAmount = item.PlannedAmount,
                    ActualAmount = actualAmount
                });
            }

            quarterDtos.Add(new QuarterBudgetDto
            {
                QuarterID = quarter.QuarterID,
                QuarterName = $"Q{quarter.QuarterNumber} {fiscalYear.YearName}",
                TotalPlanned = dtos.Sum(d => d.PlannedAmount),
                TotalActual = dtos.Sum(d => d.ActualAmount),
                Items = dtos
            });
        }

        return ApiResponse<QuarterlyBudgetReportDto>.Success(new QuarterlyBudgetReportDto
        {
            FiscalYearID = yearId,
            YearName = fiscalYear.YearName,
            Quarters = quarterDtos
        });
    }

    public async Task<ApiResponse<BudgetReportDto>> GetQuarterBudgetReportAsync(int quarterId)
    {
        var quarter = await _unitOfWork.Quarters.GetByIdAsync(quarterId);
        if (quarter == null || quarter.IsDeleted)
            return ApiResponse<BudgetReportDto>.Fail("الفصل غير موجود");

        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(quarter.FiscalYearID);
        var quarterName = $"Q{quarter.QuarterNumber} {fiscalYear?.YearName ?? ""}";

        var items = await _unitOfWork.BudgetItems.Query()
            .Where(b => b.QuarterID == quarterId && !b.IsDeleted)
            .Include(b => b.Account)
            .ToListAsync();

        var dtos = new List<BudgetItemDto>();
        foreach (var item in items)
        {
            var actualAmount = await CalculateActualAmountAsync(
                item.AccountID, quarter.FiscalYearID, quarterId, null);

            dtos.Add(new BudgetItemDto
            {
                BudgetItemID = item.BudgetItemID,
                FiscalYearID = quarter.FiscalYearID,
                QuarterID = quarterId,
                QuarterName = quarterName,
                AccountID = item.AccountID,
                AccountCode = item.Account?.AccountCode ?? "",
                AccountName = item.Account?.AccountName ?? "",
                PlannedAmount = item.PlannedAmount,
                ActualAmount = actualAmount
            });
        }

        return ApiResponse<BudgetReportDto>.Success(new BudgetReportDto
        {
            FiscalYearID = quarter.FiscalYearID,
            YearName = quarterName,
            TotalPlanned = dtos.Sum(d => d.PlannedAmount),
            TotalActual = dtos.Sum(d => d.ActualAmount),
            Items = dtos
        });
    }

    public async Task<ApiResponse<MonthlyBudgetReportDto>> GetMonthlyBudgetReportAsync(int fiscalYearId)
    {
        // Resolve year number (e.g. 2026) to FiscalYearID
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && (y.FiscalYearID == fiscalYearId || y.YearName == fiscalYearId.ToString()));

        if (fiscalYear == null)
            return ApiResponse<MonthlyBudgetReportDto>.Fail("السنة المالية غير موجودة");

        var yearId = fiscalYear.FiscalYearID;

        var periods = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == yearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .ToListAsync();

        var monthDtos = new List<MonthBudgetDto>();
        foreach (var period in periods)
        {
            var items = await _unitOfWork.BudgetItems.Query()
                .Where(b => b.PeriodID == period.PeriodID && !b.IsDeleted)
                .Include(b => b.Account)
                .ToListAsync();

            var dtos = new List<BudgetItemDto>();
            foreach (var item in items)
            {
                var actualAmount = await CalculateActualAmountAsync(
                    item.AccountID, yearId, null, period.PeriodID);

                dtos.Add(new BudgetItemDto
                {
                    BudgetItemID = item.BudgetItemID,
                    AccountID = item.AccountID,
                    AccountCode = item.Account?.AccountCode ?? "",
                    AccountName = item.Account?.AccountName ?? "",
                    PlannedAmount = item.PlannedAmount,
                    ActualAmount = actualAmount
                });
            }

            monthDtos.Add(new MonthBudgetDto
            {
                PeriodID = period.PeriodID,
                PeriodName = period.PeriodName,
                PeriodNumber = period.PeriodNumber,
                TotalPlanned = dtos.Sum(d => d.PlannedAmount),
                TotalActual = dtos.Sum(d => d.ActualAmount),
                Items = dtos
            });
        }

        return ApiResponse<MonthlyBudgetReportDto>.Success(new MonthlyBudgetReportDto
        {
            FiscalYearID = yearId,
            YearName = fiscalYear.YearName,
            Months = monthDtos
        });
    }

    // ═══════════════════════════════════════
    // Private Helpers
    // ═══════════════════════════════════════

    private async Task<decimal> CalculateActualAmountAsync(
        int accountId, int fiscalYearId, int? quarterId, int? periodId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null) return 0;

        DateTime fromDate = fiscalYear.StartDate;
        DateTime toDate = fiscalYear.EndDate;

        if (quarterId.HasValue)
        {
            var quarter = await _unitOfWork.Quarters.GetByIdAsync(quarterId.Value);
            if (quarter != null)
            {
                fromDate = quarter.StartDate;
                toDate = quarter.EndDate;
            }
        }

        if (periodId.HasValue)
        {
            var period = await _unitOfWork.FiscalPeriods.GetByIdAsync(periodId.Value);
            if (period != null)
            {
                fromDate = period.StartDate;
                toDate = period.EndDate;
            }
        }

        var account = await _unitOfWork.ChartOfAccounts.GetByIdAsync(accountId);
        if (account == null) return 0;

        // جلب جميع الحسابات الفرعية لهذا الحساب
        var allAccounts = await _unitOfWork.ChartOfAccounts.Query()
            .Where(a => !a.IsDeleted && a.IsActive)
            .ToListAsync();

        var accountCodes = new List<string> { account.AccountCode };
        // إضافة أكواد الحسابات الفرعية التي تبدأ برمز الحساب الأب
        var childAccounts = allAccounts.Where(a => a.AccountCode.StartsWith(account.AccountCode) && a.AccountID != accountId);
        accountCodes.AddRange(childAccounts.Select(a => a.AccountCode));

        // جلب جميع معرفات الحسابات
        var accountIds = allAccounts
            .Where(a => accountCodes.Contains(a.AccountCode))
            .Select(a => a.AccountID)
            .ToList();

        var lines = _unitOfWork.JournalEntryLines.Query()
            .Include(l => l.JournalEntry)
            .Where(l => accountIds.Contains(l.AccountID)
                && !l.JournalEntry.IsDeleted
                && l.JournalEntry.Status == "Posted"
                && l.JournalEntry.EntryDate >= fromDate
                && l.JournalEntry.EntryDate <= toDate);

        if (account.AccountNature == "Debit")
            return await lines.SumAsync(l => l.Debit - l.Credit);
        else
            return await lines.SumAsync(l => l.Credit - l.Debit);
    }

    // ═══════════════════════════════════════
    // SEED BUDGET
    // ═══════════════════════════════════════

    public async Task<ApiResponse<bool>> SeedBudgetAsync(int fiscalYearId, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.GetByIdAsync(fiscalYearId);
        if (fiscalYear == null || fiscalYear.IsDeleted)
            return ApiResponse<bool>.Fail("السنة المالية غير موجودة");

        if (fiscalYear.IsClosed)
            return ApiResponse<bool>.Fail("السنة المالية مغلقة");

        // جلب جميع الحسابات وتحويلها لقاموس by code
        var allAccounts = await _unitOfWork.ChartOfAccounts.Query()
            .Where(a => !a.IsDeleted)
            .ToListAsync();
        var codeToId = allAccounts.ToDictionary(a => a.AccountCode, a => a.AccountID);

        // جلب الفصول والفترات الشهرية
        var quarters = await _unitOfWork.Quarters.Query()
            .Where(q => q.FiscalYearID == fiscalYearId && !q.IsDeleted)
            .OrderBy(q => q.QuarterNumber)
            .ToListAsync();

        var periods = await _unitOfWork.FiscalPeriods.Query()
            .Where(p => p.FiscalYearID == fiscalYearId && !p.IsDeleted)
            .OrderBy(p => p.PeriodNumber)
            .ToListAsync();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ═══ حذف البيانات القديمة للسنة المالية ═══
            var oldItems = await _unitOfWork.BudgetItems.Query()
                .Where(b => b.FiscalYearID == fiscalYearId && !b.IsDeleted)
                .ToListAsync();

            foreach (var old in oldItems)
            {
                old.IsDeleted = true;
                old.DeletedAt = DateTime.UtcNow;
                old.DeletedByUserID = userId;
                _unitOfWork.BudgetItems.Update(old);
            }

            // ═══ البيانات ═══
            // (AccountCode, AnnualAmount, QuarterlyAmount, MonthlyAmount)
            var budgetData = new (string Code, decimal Annual, decimal Quarterly, decimal Monthly)[]
            {
                // ── 31 المرتبات والأجور ──
                ("311", 17280000, 4320000, 1440000),
                ("312", 4320000, 1080000, 360000),
                ("313", 1440000, 360000, 120000),
                ("314", 1440000, 360000, 120000),
                ("316", 4320000, 1080000, 360000),

                // ── 32 المستلزمات السلعية ──
                ("322", 8136000, 2034000, 678000),
                ("323", 2034000, 508500, 169500),
                ("325", 3390000, 847500, 282500),

                // ── 33 المستلزمات الخدمية ──
                ("331", 5085000, 1271250, 423750),
                ("334", 4068000, 1017000, 339000),
                ("335", 4068000, 1017000, 339000),
                ("338", 2034000, 508500, 169500),
                ("339", 5085000, 1271250, 423750),

                // ── 35 المصروفات الجارية ──
                ("353", 14400000, 3600000, 1200000),
            };

            int created = 0;

            foreach (var (code, annual, quarterly, monthly) in budgetData)
            {
                if (!codeToId.TryGetValue(code, out var accountId))
                    continue;

                // ═══ الموازنة السنوية ═══
                await _unitOfWork.BudgetItems.AddAsync(new BudgetItem
                {
                    FiscalYearID = fiscalYearId,
                    AccountID = accountId,
                    PlannedAmount = annual,
                    Notes = "موازنة سنوية",
                    CreatedAt = DateTime.UtcNow
                });
                created++;

                // ═══ الموازنة الربعية ═══
                foreach (var q in quarters)
                {
                    await _unitOfWork.BudgetItems.AddAsync(new BudgetItem
                    {
                        FiscalYearID = fiscalYearId,
                        QuarterID = q.QuarterID,
                        AccountID = accountId,
                        PlannedAmount = quarterly,
                        Notes = $"موازنة ربعية - الفصل {q.QuarterNumber}",
                        CreatedAt = DateTime.UtcNow
                    });
                    created++;
                }

                // ═══ الموازنة الشهرية ═══
                foreach (var p in periods)
                {
                    await _unitOfWork.BudgetItems.AddAsync(new BudgetItem
                    {
                        FiscalYearID = fiscalYearId,
                        PeriodID = p.PeriodID,
                        AccountID = accountId,
                        PlannedAmount = monthly,
                        Notes = $"موازنة شهرية - {p.PeriodName}",
                        CreatedAt = DateTime.UtcNow
                    });
                    created++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return ApiResponse<bool>.Success(true, $"تم إدخال {created} بند موازنة بنجاح");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
