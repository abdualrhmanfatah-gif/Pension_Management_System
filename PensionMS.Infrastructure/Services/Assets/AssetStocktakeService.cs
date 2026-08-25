using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Assets;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Assets;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Assets;

namespace PensionMS.Infrastructure.Services.Assets;

public class AssetStocktakeService : IAssetStocktakeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    private static class StocktakeStatus
    {
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
    }

    private static class AssetCondition
    {
        public const string Good = "Good";
        public const string Fair = "Fair";
        public const string Poor = "Poor";
    }

    public AssetStocktakeService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<ApiResponse<AssetStocktakeDto>> CreateStocktakeAsync(int fiscalYearId, int userId)
    {
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => y.FiscalYearID == fiscalYearId && !y.IsDeleted);

        if (fiscalYear == null)
            return ApiResponse<AssetStocktakeDto>.Fail("السنة المالية غير موجودة");

        var existing = await _unitOfWork.AssetStocktakes.Query()
            .AnyAsync(s => s.FiscalYearID == fiscalYearId && !s.IsDeleted);
        if (existing)
            return ApiResponse<AssetStocktakeDto>.Fail("يوجد جرد أصول مسبق لهذه السنة المالية");

        var assets = await _unitOfWork.FixedAssets.Query()
            .Where(a => !a.IsDeleted && a.Status != AssetStatus.Disposed)
            .Include(a => a.Category)
            .ToListAsync();

        if (assets.Count == 0)  
            return ApiResponse<AssetStocktakeDto>.Fail("لا توجد أصول ثابتة للجرد");

        var lines = assets.Select(a => new AssetStocktakeLine
        {
            FixedAssetID = a.AssetID,
            AssetNumber = a.AssetNumber,
            AssetName = a.AssetName,
            Location = a.Location ?? "",  
            NetBookValue = a.NetBookValue,
            SystemQuantity = a.Quantity,
            RecordedQuantity = 0,
            IsPresent = false,
            ActualCondition = ""
        }).ToList();

        var stocktake = new AssetStocktake
        {
            FiscalYearID = fiscalYearId,
            StocktakeDate = DateTime.UtcNow,
            Status = StocktakeStatus.InProgress,  
            CreatedByUserID = userId,
            Notes = null,
            Lines = lines
        };

        await _unitOfWork.AssetStocktakes.AddAsync(stocktake);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "AssetStocktake", stocktake.AssetStocktakeID,
            null, $"إنشاء جرد أصول للسنة المالية {fiscalYearId}");

        return ApiResponse<AssetStocktakeDto>.Success( await MapToDtoAsync(stocktake));
    }

    public async Task<ApiResponse<bool>> RecordActualCountAsync(int stocktakeId, List<AssetStocktakeLineDto> lines, int userId)
    {
        var stocktake = await _unitOfWork.AssetStocktakes.Query()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.AssetStocktakeID == stocktakeId && !s.IsDeleted);

        if (stocktake == null)
            return ApiResponse<bool>.Fail("الجرد غير موجود");

        if (stocktake.Status != StocktakeStatus.InProgress)  
            return ApiResponse<bool>.Fail("لا يمكن تعديل جرد مكتمل");

        var validLineIds = stocktake.Lines.Select(l => l.StocktakeLineID).ToHashSet();
        var invalidLines = lines.Where(l => !validLineIds.Contains(l.StocktakeLineID)).ToList();

        if (invalidLines.Count > 0)
            return ApiResponse<bool>.Fail($"بنود غير موجودة في الجرد: {string.Join(", ", invalidLines.Select(l => l.StocktakeLineID))}");


        var linesDict = stocktake.Lines.ToDictionary(l => l.StocktakeLineID);

        foreach (var lineDto in lines)
        {
            if (linesDict. TryGetValue(lineDto.StocktakeLineID, out var line))
            {
                line.ActualLocation = lineDto.ActualLocation ?? "";
                line.ActualCondition = lineDto.ActualCondition ?? "";
                line.IsPresent = lineDto.IsPresent;
                line.RecordedQuantity = lineDto.RecordedQuantity;
                line.Notes = lineDto.Notes;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "RecordCount", "AssetStocktake", stocktakeId,
            null, "تسجيل نتائج جرد الأصول");

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<AssetStocktakeSummaryDto>> CompleteStocktakeAsync(int stocktakeId, int userId)
    {
        var stocktake = await _unitOfWork.AssetStocktakes.Query()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.AssetStocktakeID == stocktakeId && !s.IsDeleted);

        if (stocktake == null)
            return ApiResponse<AssetStocktakeSummaryDto>.Fail("الجرد غير موجود");

        if (stocktake.Status == StocktakeStatus.Completed) 
            return ApiResponse<AssetStocktakeSummaryDto>.Fail("الجرد مكتمل مسبقاً");

        var uncountedLines = stocktake.Lines.Count(l => 
            l.RecordedQuantity == 0 && l.IsPresent == false && string.IsNullOrEmpty(l.ActualCondition));
        
        if (uncountedLines > 0)
            return ApiResponse<AssetStocktakeSummaryDto>.Fail($"يوجد {uncountedLines} بند لم يتم تسجيله بعد - لا يمكن إنهاء الجرد");

        var summary = CalculateSummary(stocktake.Lines);

        stocktake.Status = StocktakeStatus.Completed;  // ✅ تصحيح: استخدم Constant
        stocktake.VerifiedByUserID = userId;
        stocktake.VerifiedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Complete", "AssetStocktake", stocktakeId,
            $"{{\"Status\":\"{StocktakeStatus.InProgress}\"}}",
            $"{{\"Status\":\"{StocktakeStatus.Completed}\"}}");

        return ApiResponse<AssetStocktakeSummaryDto>.Success(summary);
    }

    public async Task<AssetStocktakeDto?> GetLatestStocktakeAsync()
    {
        var now = DateTime.UtcNow;
        var fiscalYear = await _unitOfWork.FiscalYears.Query()
            .FirstOrDefaultAsync(y => !y.IsDeleted && y.StartDate <= now && y.EndDate >= now);

        if (fiscalYear == null)
            return null;

        var stocktake = await _unitOfWork.AssetStocktakes.Query()
            .Where(s => s.FiscalYearID == fiscalYear.FiscalYearID && !s.IsDeleted)
            .OrderByDescending(s => s.StocktakeDate)
            .FirstOrDefaultAsync();

        return stocktake != null ? await MapToDtoAsync(stocktake) : null;
    }

    public async Task<AssetStocktakeDto?> GetStocktakeByFiscalYearAsync(int fiscalYearId)
    {
        var stocktake = await _unitOfWork.AssetStocktakes.Query()
            .Where(s => s.FiscalYearID == fiscalYearId && !s.IsDeleted)
            .OrderByDescending(s => s.StocktakeDate)
            .FirstOrDefaultAsync();

        return stocktake != null ? await MapToDtoAsync(stocktake) : null;
    }

    public async Task<AssetStocktakeDto?> GetStocktakeAsync(int stocktakeId)
    {
        var stocktake = await _unitOfWork.AssetStocktakes.Query()
            .Include(s => s.Lines)
                .ThenInclude(l => l.FixedAsset)
                    .ThenInclude(a => a.Category)
            .Include(s => s.CreatedByUser)
            .Include(s => s.VerifiedByUser)
            .FirstOrDefaultAsync(s => s.AssetStocktakeID == stocktakeId && !s.IsDeleted);

        return stocktake != null ? await MapToDtoAsync(stocktake) : null;
    }

    public async Task<List<AssetStocktakeDto>> GetAllStocktakesAsync(int fiscalYearId)
    {
  
        var stocktakes = await _unitOfWork.AssetStocktakes.Query()
            .Where(s => s.FiscalYearID == fiscalYearId && !s.IsDeleted)
            .Include(s => s.Lines)
            .Include(s => s.CreatedByUser)
            .Include(s => s.VerifiedByUser)
            .OrderByDescending(s => s.StocktakeDate)
            .ToListAsync();

        var tasks = stocktakes.Select(s => MapToDtoAsync(s));
        var result = await Task.WhenAll(tasks);
        return result.ToList();
    }

    private async Task<AssetStocktakeDto> MapToDtoAsync(AssetStocktake stocktake)
    {
        if (stocktake.Lines == null || stocktake.Lines.Count == 0)
        {
            stocktake.Lines = await _unitOfWork.AssetStocktakeLines.Query()
                .Where(l => l.AssetStocktakeID == stocktake.AssetStocktakeID)
                .ToListAsync();
        }

        var lines = stocktake.Lines.Select(l => new AssetStocktakeLineDto
        {
            StocktakeLineID = l.StocktakeLineID,
            FixedAssetID = l.FixedAssetID,
            AssetNumber = l.AssetNumber,
            AssetName = l.AssetName,
            Location = l.Location ?? "",
            NetBookValue = l.NetBookValue,
            SystemQuantity = l.SystemQuantity,
            ActualLocation = l.ActualLocation ?? "",
            ActualCondition = l.ActualCondition ?? "",
            IsPresent = l.IsPresent,
            RecordedQuantity = l.RecordedQuantity,
            Notes = l.Notes
        }).ToList();

        var summary = CalculateSummary(stocktake.Lines);

        return new AssetStocktakeDto
        {
            AssetStocktakeID = stocktake.AssetStocktakeID,
            FiscalYearID = stocktake.FiscalYearID,
            StocktakeDate = stocktake.StocktakeDate,
            Status = stocktake.Status,
            CreatedByName = stocktake.CreatedByUser?.FullName,
            VerifiedByName = stocktake.VerifiedByUser?.FullName,
            VerifiedAt = stocktake.VerifiedAt,
            Notes = stocktake.Notes,
            Summary = summary,
            Lines = lines
        };
    }

    private AssetStocktakeSummaryDto CalculateSummary(ICollection<AssetStocktakeLine> lines)
    {
        return new AssetStocktakeSummaryDto
        {
            TotalAssets = lines.Count,
            PresentAssets = lines.Count(l => l.IsPresent),
            MissingAssets = lines.Count(l => !l.IsPresent),
            AssetsInGoodCondition = lines.Count(l => l.ActualCondition == AssetCondition.Good),
            AssetsInFairCondition = lines.Count(l => l.ActualCondition == AssetCondition.Fair),
            AssetsInPoorCondition = lines.Count(l => l.ActualCondition == AssetCondition.Poor),
            TotalBookValue = lines.Sum(l => l.NetBookValue),
            MissingBookValue = lines.Where(l => !l.IsPresent).Sum(l => l.NetBookValue)  // ✅ أضف قيمة المفقودين
        };
    }
}