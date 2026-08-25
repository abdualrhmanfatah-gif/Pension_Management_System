using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Core.Interfaces.Inventory;

namespace PensionMS.Infrastructure.Services.Inventory;

public class SimpleStocktakeService : ISimpleStocktakeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJournalEntryService _journalEntry;
    private readonly IImmutableAuditService _auditService;


    private const string InventoryAccountPrefix = "13";
    private const string AdjustmentAccountCode = "399";

    public SimpleStocktakeService(
        IUnitOfWork unitOfWork,
        IJournalEntryService journalEntry,
        IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _journalEntry = journalEntry;
        _auditService = auditService;
    }

    private async Task<int> GetJournalEntryTypeIDAsync(string code)
    {
        var entryType = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted);
        return entryType?.JournalEntryTypeID ?? 1;
    }

    public async Task<ApiResponse<SimpleStocktakeDto>> CreateStocktakeAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && y.StartDate <= now && y.EndDate >= now);

        if (fiscalYear == null)
            return ApiResponse<SimpleStocktakeDto>.Fail("لا توجد سنة مالية مفتوحة حالياً");

        var fiscalYearId = fiscalYear.FiscalYearID;

        var existing = await _unitOfWork.SimpleStocktakes.Query()
            .AnyAsync(s => s.FiscalYearID == fiscalYearId && !s.IsDeleted);
        if (existing)
            return ApiResponse<SimpleStocktakeDto>.Fail("يوجد جرد مخزون مسبق لهذه السنة المالية");

        var items = await _unitOfWork.InventoryItems.Query()
            .Where(i => !i.IsDeleted && i.Quantity > 0)
            .ToListAsync();

        if (items.Count == 0)  
            return ApiResponse<SimpleStocktakeDto>.Fail("لا توجد أصناف في المخزون للجرد");


        var lines = items.Select(i => new SimpleStocktakeLine
        {
            InventoryItemID = i.InventoryItemID,
            ItemName = i.ItemName,
            ItemCode = i.ItemCode,
            SystemQuantity = i.Quantity,
            PhysicalQuantity = 0,
            UnitCost = 0
        }).ToList();

        var stocktake = new SimpleStocktake
        {
            FiscalYearID = fiscalYearId,
            StocktakeDate = DateTime.UtcNow,
            Status = "InProgress",
            CreatedByUserID = userId,
            TotalItems = items.Count,
            Lines = lines
        };

        await _unitOfWork.SimpleStocktakes.AddAsync(stocktake);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "SimpleStocktake", stocktake.SimpleStocktakeID,
            null, $"إنشاء جرد مخزون للسنة المالية {fiscalYearId}");

        return ApiResponse<SimpleStocktakeDto>.Success(await MapToDtoAsync(stocktake));
    }

    public async Task<ApiResponse<bool>> RecordPhysicalCountAsync(int stocktakeId, Dictionary<int, int> actualCounts, int userId)
    {
        var stocktake = await _unitOfWork.SimpleStocktakes.Query()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.SimpleStocktakeID == stocktakeId && !s.IsDeleted);

        if (stocktake == null)
            return ApiResponse<bool>.Fail("الجرد غير موجود");

        if (stocktake.Status != "InProgress")
            return ApiResponse<bool>.Fail("لا يمكن تعديل جرد مكتمل");


        var invalidIds = actualCounts.Keys
            .Where(id => !stocktake.Lines.Any(l => l.InventoryItemID == id))
            .ToList();

        if (invalidIds.Count > 0)
            return ApiResponse<bool>.Fail($"أصناف غير موجودة في الجرد: {string.Join(", ", invalidIds)}");

    
        foreach (var kvp in actualCounts)
        {
            var line = stocktake.Lines.FirstOrDefault(l => l.InventoryItemID == kvp.Key);
            if (line != null)
            {
                line.PhysicalQuantity = kvp.Value;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "RecordCount", "SimpleStocktake", stocktakeId,
            null, $"تسجيل نتائج جرد المخزون");

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<JournalEntryDto>> AdjustStocktakeAsync(int stocktakeId, int userId)
    {
        var stocktake = await _unitOfWork.SimpleStocktakes. Query()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.SimpleStocktakeID == stocktakeId && !s.IsDeleted);

        if (stocktake == null)
            return ApiResponse<JournalEntryDto>.Fail("الجرد غير موجود");

        if (stocktake.Status == "Adjusted")
            return ApiResponse<JournalEntryDto>.Fail("تم تسوية الجرد مسبقاً");


        var linesWithCount = stocktake.Lines
            .Where(l => l.PhysicalQuantity > 0 || l.SystemQuantity > 0)
            .ToList();

        var linesWithVariance = linesWithCount
            .Where(l => l.PhysicalQuantity - l.SystemQuantity != 0)
            .ToList();

        if (linesWithVariance.Count == 0)
        {
            stocktake.Status = "Adjusted";
            stocktake.CompletedByUserID = userId;
            stocktake.CompletedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<JournalEntryDto>.Fail("لا توجد فروق للتسوية");
        }


        decimal totalSurplus = linesWithVariance
            .Where(l => l.PhysicalQuantity > l.SystemQuantity)
            .Sum(l => l.VarianceValue);

        decimal totalDeficit = linesWithVariance
            .Where(l => l.PhysicalQuantity < l.SystemQuantity)
            .Sum(l => l.VarianceValue); // هذا سالب تلقائياً لأن VarianceValue سالب

   
        var inventoryAccount = await _unitOfWork.ChartOfAccounts. Query()
            .FirstOrDefaultAsync(a => a.AccountCode.StartsWith(InventoryAccountPrefix) && a.IsPostable);

        var adjustmentAccount = await _unitOfWork.ChartOfAccounts. Query()
            .FirstOrDefaultAsync(a => a.AccountCode == AdjustmentAccountCode && a.IsPostable);

        if (inventoryAccount == null || adjustmentAccount == null)
            return ApiResponse<JournalEntryDto>.Fail("الحسابات المطلوبة للتسوية غير موجودة (تأكد من وجود حساب 13xx و 399)");

        var journalLines = new List<CreateJournalEntryLineDto>();

        // فائض (مدين مخزون، دائن تسوية)
        if (totalSurplus > 0)
        {
            journalLines.Add(new CreateJournalEntryLineDto
            {
                AccountID = inventoryAccount.AccountID,
                Debit = totalSurplus,
                Credit = 0,
                Description = "فائض جرد المخزون"
            });
            journalLines.Add(new CreateJournalEntryLineDto
            {
                AccountID = adjustmentAccount.AccountID,
                Debit = 0,
                Credit = totalSurplus,
                Description = "فائض جرد المخزون - مقابل"
            });
        }

        // عجز (مدين تسوية، دائن مخزون)
        if (totalDeficit < 0) // فقط إذا كان هناك عجز
        {
            var deficitAbs = Math.Abs(totalDeficit);
            journalLines.Add(new CreateJournalEntryLineDto
            {
                AccountID = adjustmentAccount.AccountID,
                Debit = deficitAbs,
                Credit = 0,
                Description = "عجز جرد المخزون"
            });
            journalLines.Add(new CreateJournalEntryLineDto
            {
                AccountID = inventoryAccount.AccountID,
                Debit = 0,
                Credit = deficitAbs,
                Description = "عجز جرد المخزون - مقابل"
            });
        }

        var periodId = await _journalEntry.GetPeriodIdForDateAsync(DateTime.UtcNow);
        if (periodId <= 0)
            return ApiResponse<JournalEntryDto>.Fail("لا توجد فترة محاسبية مفتوحة لهذا التاريخ");

        var stocktakeTypeId = await GetJournalEntryTypeIDAsync("INV");

        var journalEntryResult = await _journalEntry.CreateJournalEntryAsync(
            new CreateJournalEntryDto
            {
                EntryDate = DateTime.UtcNow,
                PeriodID = periodId,
                JournalEntryTypeID = stocktakeTypeId,
                Description = $"تسوية جرد المخزون - فائض: {totalSurplus:N2} / عجز: {Math.Abs(totalDeficit):N2}",
                Lines = journalLines
            }, userId);

        if (!journalEntryResult. IsSuccess || journalEntryResult.Data == null)
            return ApiResponse<JournalEntryDto>.Fail(journalEntryResult.Message ?? "فشل إنشاء قيد التسوية");

        var postResult = await _journalEntry.PostJournalEntryAsync(journalEntryResult.Data.JournalEntryID, userId);
        if (!postResult.IsSuccess)
            return ApiResponse<JournalEntryDto>.Fail(postResult.Message ?? "فشل ترحيل قيد التسوية");

        var itemIds = stocktake.Lines.Select(l => l.InventoryItemID).ToList();
        var items = await _unitOfWork.InventoryItems. Query()
            .Where(i => itemIds.Contains(i.InventoryItemID))
            .ToDictionaryAsync(i => i.InventoryItemID);

        foreach (var line in stocktake.Lines)
        {
            if (items.TryGetValue(line.InventoryItemID, out var item))
            {
                item.Quantity = line.PhysicalQuantity;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        stocktake.Status = "Adjusted";
        stocktake.JournalEntryID = journalEntryResult.Data.JournalEntryID;
        stocktake.CompletedByUserID = userId;
        stocktake.CompletedAt = DateTime.UtcNow;
        stocktake.ItemsWithVariance = linesWithVariance.Count;
        stocktake.TotalVarianceValue = totalSurplus + totalDeficit; 

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Adjust", "SimpleStocktake", stocktakeId,
            "{\"Status\":\"InProgress\"}",
            $"{{\"Status\":\"Adjusted\",\"JournalEntryID\":{journalEntryResult.Data.JournalEntryID},\"TotalVariance\":{stocktake.TotalVarianceValue}}}");

        return ApiResponse<JournalEntryDto>.Success(journalEntryResult.Data);
    }

    public async Task<SimpleStocktakeDto?> GetLatestStocktakeAsync()
    {
        var now = DateTime.UtcNow;
        var fiscalYear = await _unitOfWork.FiscalYears. Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && y.StartDate <= now && y.EndDate >= now);

        if (fiscalYear == null)
            return null;

        var stocktake = await _unitOfWork.SimpleStocktakes. Query()
            .Where(s => s.FiscalYearID == fiscalYear.FiscalYearID && !s.IsDeleted)
            .OrderByDescending(s => s.StocktakeDate)
            .FirstOrDefaultAsync();

        return stocktake != null ? await MapToDtoAsync(stocktake) : null;
    }

    public async Task<SimpleStocktakeDto?> GetStocktakeByFiscalYearAsync(int fiscalYearId)
    {
        var stocktake = await _unitOfWork.SimpleStocktakes. Query()
            .Where(s => s.FiscalYearID == fiscalYearId && !s.IsDeleted)
            .OrderByDescending(s => s.StocktakeDate)
            .FirstOrDefaultAsync();

        return stocktake != null ? await MapToDtoAsync(stocktake) : null;
    }

    public async Task<SimpleStocktakeDto?> GetStocktakeAsync(int stocktakeId)
    {
        var stocktake = await _unitOfWork.SimpleStocktakes. Query()
            .Include(s => s.Lines)
                .ThenInclude(l => l.InventoryItem)
            .Include(s => s.CreatedByUser)
            .Include(s => s.CompletedByUser)
            .Include(s => s.JournalEntry)
            .FirstOrDefaultAsync(s => s.SimpleStocktakeID == stocktakeId && !s.IsDeleted);

        return stocktake != null ? await MapToDtoAsync(stocktake) : null;
    }

    public async Task<List<SimpleStocktakeDto>> GetAllStocktakesAsync(int fiscalYearId)
    {
        var stocktakes = await _unitOfWork.SimpleStocktakes.Query()
            .Where(s => s.FiscalYearID == fiscalYearId && !s.IsDeleted)
            .OrderByDescending(s => s.StocktakeDate)
            .ToListAsync();

        var result = new List<SimpleStocktakeDto>();
        foreach (var stocktake in stocktakes)
        {
            result.Add(await MapToDtoAsync(stocktake));
        }
        return result;
    }

    private async Task<SimpleStocktakeDto> MapToDtoAsync(SimpleStocktake stocktake)
    {
        if (stocktake.Lines == null || stocktake.Lines.Count == 0)
        {
            stocktake.Lines = await _unitOfWork.SimpleStocktakeLines.Query()
                .Where(l => l.SimpleStocktakeID == stocktake.SimpleStocktakeID)
                .ToListAsync();
        }

        var lines = stocktake.Lines.Select(l => new SimpleStocktakeLineDto
        {
            LineID = l.LineID,
            InventoryItemID = l.InventoryItemID,
            ItemName = l.ItemName,
            ItemCode = l.ItemCode,
            SystemQuantity = l.SystemQuantity,
            PhysicalQuantity = l.PhysicalQuantity,
            UnitCost = l.UnitCost
        }).ToList();

        return new SimpleStocktakeDto
        {
            SimpleStocktakeID = stocktake.SimpleStocktakeID,
            FiscalYearID = stocktake.FiscalYearID,
            StocktakeDate = stocktake.StocktakeDate,
            Status = stocktake.Status,
            CreatedByName = stocktake.CreatedByUser?.FullName,
            CompletedByName = stocktake.CompletedByUser?.FullName,
            CompletedAt = stocktake.CompletedAt,
            TotalItems = stocktake.TotalItems,
            ItemsWithVariance = stocktake.ItemsWithVariance,
            TotalVarianceValue = stocktake.TotalVarianceValue,
            JournalEntryID = stocktake.JournalEntryID,
            JournalEntryNumber = stocktake.JournalEntry?.EntryNumber,
            Lines = lines
        };
    }
}