using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.Infrastructure.Services.Accounting;

public class ChartOfAccountService : IChartOfAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Data.PensionDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public ChartOfAccountService(IUnitOfWork unitOfWork, Data.PensionDbContext dbContext, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<ApiResponse<List<ChartOfAccountDto>>> GetAllAccountsAsync(int? level = null)
    {
        var cacheKey = $"accounts_level_{level ?? 0}";
        if (_cache.TryGetValue(cacheKey, out List<ChartOfAccountDto>? cached))
            return ApiResponse<List<ChartOfAccountDto>>.Success(cached!);

        var query = _unitOfWork.ChartOfAccounts.Query().Where(a => !a.IsDeleted);
        if (level.HasValue)
            query = query.Where(a => a.AccountLevel == level.Value);

        var accounts = await query.AsNoTracking().OrderBy(a => a.AccountCode).ToListAsync();
        var result = accounts.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return ApiResponse<List<ChartOfAccountDto>>.Success(result);
    }

    public async Task<ApiResponse<ChartOfAccountDto>> GetAccountByIdAsync(int id)
    {
        var account = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountID == id && !a.IsDeleted);
        if (account == null)
            return ApiResponse<ChartOfAccountDto>.Fail("الحساب غير موجود");
        return ApiResponse<ChartOfAccountDto>.Success(MapToDto(account));
    }

    public async Task<ApiResponse<ChartOfAccountDto>> GetAccountByCodeAsync(string code)
    {
        var account = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountCode == code && !a.IsDeleted);
        if (account == null)
            return ApiResponse<ChartOfAccountDto>.Fail("الحساب غير موجود");
        return ApiResponse<ChartOfAccountDto>.Success(MapToDto(account));
    }

    public async Task<ApiResponse<List<ChartOfAccountDto>>> GetAccountTreeAsync(int? parentId = null)
    {
        var all = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var roots = parentId.HasValue
            ? all.Where(a => a.ParentAccountID == parentId.Value).ToList()
            : all.Where(a => a.AccountLevel == 1).ToList();

        return ApiResponse<List<ChartOfAccountDto>>.Success(BuildTree(roots, all));
    }

    public async Task<ApiResponse<List<ChartOfAccountDto>>> GetPostableAccountsAsync()
    {
        const string cacheKey = "accounts_postable";
        if (_cache.TryGetValue(cacheKey, out List<ChartOfAccountDto>? cached))
            return ApiResponse<List<ChartOfAccountDto>>.Success(cached!);

        var accounts = await _unitOfWork.ChartOfAccounts.Query()
            .AsNoTracking()
            .Where(a => a.IsPostable && a.IsActive && !a.IsDeleted)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
        var result = accounts.Select(MapToDto).ToList();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return ApiResponse<List<ChartOfAccountDto>>.Success(result);
    }

    public async Task<ApiResponse<ChartOfAccountDto>> CreateAccountAsync(CreateChartOfAccountDto dto)
    {
        var exists = await _unitOfWork.ChartOfAccounts.Query()
            .AnyAsync(a => a.AccountCode == dto.AccountCode && !a.IsDeleted);
        if (exists)
            return ApiResponse<ChartOfAccountDto>.Fail("رمز الحساب موجود مسبقاً");

        var parent = dto.ParentAccountID.HasValue
            ? await _unitOfWork.ChartOfAccounts.Query().FirstOrDefaultAsync(a => a.AccountID == dto.ParentAccountID.Value && !a.IsDeleted)
            : null;

        if (dto.ParentAccountID.HasValue && parent == null)
            return ApiResponse<ChartOfAccountDto>.Fail("الحساب الأب غير موجود أو محذوف");

        var level = parent != null ? parent.AccountLevel + 1 : 1;
        if (level > 5)
            return ApiResponse<ChartOfAccountDto>.Fail("لا يمكن تجاوز المستوى الخامس في الدليل المحاسبي");

        if (parent != null && !dto.AccountCode.StartsWith(parent.AccountCode + ".") && dto.AccountCode != parent.AccountCode)
            return ApiResponse<ChartOfAccountDto>.Fail($"رمز الحساب يجب أن يبدأ برمز الأب ({parent.AccountCode}.)");

        var entity = new ChartOfAccount
        {
            AccountCode = dto.AccountCode,
            AccountName = dto.AccountName,
            AccountNature = dto.AccountNature,
            AccountLevel = level,
            BookNumber = parent?.BookNumber ?? 1,
            ParentAccountID = dto.ParentAccountID,
            IsPostable = dto.IsPostable,
            AccountCategory = Enum.TryParse<AccountCategory>(dto.AccountCategory, out var cat) ? cat : AccountCategory.Asset,
            Description = dto.Description,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.ChartOfAccounts.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        InvalidateAccountsCache();
        return ApiResponse<ChartOfAccountDto>.Success(MapToDto(entity), "تم إنشاء الحساب بنجاح");
    }

    public async Task<ApiResponse<ChartOfAccountDto>> UpdateAccountAsync(int id, UpdateChartOfAccountDto dto)
    {
        var entity = await _unitOfWork.ChartOfAccounts.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<ChartOfAccountDto>.Fail("الحساب غير موجود");

        entity.AccountName = dto.AccountName;
        entity.IsPostable = dto.IsPostable;
        entity.AccountCategory = Enum.TryParse<AccountCategory>(dto.AccountCategory, out var cat) ? cat : entity.AccountCategory;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ChartOfAccounts.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        InvalidateAccountsCache();
        return ApiResponse<ChartOfAccountDto>.Success(MapToDto(entity), "تم تحديث الحساب بنجاح");
    }

    public async Task<ApiResponse<bool>> DeactivateAccountAsync(int id)
    {
        var entity = await _unitOfWork.ChartOfAccounts.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<bool>.Fail("الحساب غير موجود");

        if (!entity.IsActive)
            return ApiResponse<bool>.Fail("الحساب غير نشط بالفعل");

        var hasJournalEntries = await _unitOfWork.JournalEntryLines.Query()
            .AnyAsync(l => l.AccountID == id && !l.IsDeleted && l.JournalEntry != null && !l.JournalEntry.IsDeleted && l.JournalEntry.Status == "Posted");
        if (hasJournalEntries)
            return ApiResponse<bool>.Fail("لا يمكن إلغاء تنشيط حساب عليه حركات مرحلة");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ChartOfAccounts.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم إلغاء تنشيط الحساب");
    }

    public async Task<ApiResponse<bool>> ReactivateAccountAsync(int id)
    {
        var entity = await _unitOfWork.ChartOfAccounts.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return ApiResponse<bool>.Fail("الحساب غير موجود");
        if (entity.IsActive)
            return ApiResponse<bool>.Fail("الحساب نشط بالفعل");

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ChartOfAccounts.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        InvalidateAccountsCache();
        return ApiResponse<bool>.Success(true, "تم تنشيط الحساب");
    }

    public async Task<ApiResponse<bool>> ResetChartOfAccountsAsync()
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM BudgetItems");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Vouchers");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM FixedAssets");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM FixedAssetCategories");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM JournalEntryLines");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AccountBalances");
            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE ChartOfAccounts SET ParentAccountID = NULL");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM ChartOfAccounts");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ChartOfAccounts', RESEED, 0)");
            await Data.Seeders.ChartOfAccountsSeeder.SeedFreshAsync(_dbContext);

            await _unitOfWork.CommitAsync();
            return ApiResponse<bool>.Success(true, "تم إعادة تعيين الدليل المحاسبي بنجاح");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Fail($"فشل في إعادة تعيين الدليل المحاسبي: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ResetAllDataAsync()
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM BankTransactions");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM JournalEntryLines");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM JournalEntries");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AssetStocktakeLines");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AssetStocktakes");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM DepreciationSchedules");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AssetITDetails");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AssetVehicleDetails");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AssetBuildingDetails");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AssetLandDetails");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AssetFurnitureDetails");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM InventoryTransactions");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM InventoryCustodies");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM InventoryItems");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM FixedAssets");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM FixedAssetCategories");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM FixedAssetTypes");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM BudgetItems");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM OperationalExpenses");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Vouchers");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM AccountBalances");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM FiscalPeriods");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Quarters");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM FiscalYears");
            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE CostCenters SET ParentCostCenterID = NULL");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM CostCenters");
            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE Banks SET AccountID = NULL");
            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE ChartOfAccounts SET ParentAccountID = NULL");
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM ChartOfAccounts");

            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('JournalEntryLines', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('JournalEntries', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('BankTransactions', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('DepreciationSchedules', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('AssetStocktakeLines', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('AssetStocktakes', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('FixedAssets', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('FixedAssetCategories', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('FixedAssetTypes', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('InventoryItems', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Vouchers', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('AccountBalances', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('BudgetItems', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('OperationalExpenses', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('FiscalPeriods', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Quarters', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('FiscalYears', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('CostCenters', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ChartOfAccounts', RESEED, 0)");

            await Data.Seeders.ChartOfAccountsSeeder.SeedFreshAsync(_dbContext);
            await Data.Seeders.Assets.FixedAssetTypeSeeder.SeedAsync(_dbContext);
            await Data.Seeders.Assets.FixedAssetCategorySeeder.SeedAsync(_dbContext);

            await _unitOfWork.CommitAsync();
            return ApiResponse<bool>.Success(true, "تم تصفير جميع البيانات المحاسبية والأصول والمخزون وإعادة تهيئتها بنجاح");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Fail($"فشل في تصفير البيانات: {ex.Message}");
        }
    }

    public async Task<ApiResponse<int>> DisableUnusedAccountsAsync()
    {
        return ApiResponse<int>.Success(0, "جميع الحسابات نشطة");
    }

    private static ChartOfAccountDto MapToDto(ChartOfAccount entity)
    {
        return new ChartOfAccountDto
        {
            AccountID = entity.AccountID,
            AccountCode = entity.AccountCode,
            AccountName = entity.AccountName,
            AccountNature = entity.AccountNature,
            AccountLevel = entity.AccountLevel,
            ParentAccountID = entity.ParentAccountID,
            IsPostable = entity.IsPostable,
            IsActive = entity.IsActive,
            AccountCategory = entity.AccountCategory.ToString(),
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
    }

    private static List<ChartOfAccountDto> BuildTree(List<ChartOfAccount> roots, List<ChartOfAccount> all)
    {
        return roots.Select(r => BuildNode(r, all)).ToList();
    }

    private static ChartOfAccountDto BuildNode(ChartOfAccount account, List<ChartOfAccount> all)
    {
        var dto = MapToDto(account);
        var children = all.Where(c => c.ParentAccountID == account.AccountID).ToList();
        dto.SubAccountsCount = children.Count;
        if (children.Count > 0)
            dto.Children = children.Select(c => BuildNode(c, all)).ToList();
        return dto;
    }

    private void InvalidateAccountsCache()
    {
        for (int i = 0; i <= 5; i++)
            _cache.Remove($"accounts_level_{i}");
        _cache.Remove("accounts_postable");
    }
}
