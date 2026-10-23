using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Administration;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessiblePensionRepository _access;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DocumentTypesCacheDuration = TimeSpan.FromHours(1);
    private static readonly SemaphoreSlim DocumentTypesCacheLock = new(1, 1);

    public LookupService(IUnitOfWork unitOfWork, IAccessiblePensionRepository access, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _access = access;
        _cache = cache;
    }

    public async Task<ApiResponse<List<LookupDto>>> GetRanksAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.Ranks.Query()
                .Select(r => new LookupDto { Id = r.RankID, Name = r.RankName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetBanksAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.Banks.Query()
                .Select(b => new LookupDto { Id = b.BankID, Name = b.BankName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetRelationshipsAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.Relationships.Query()
                .Select(r => new LookupDto { Id = r.RelationshipID, Name = r.RelationshipName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetDocumentTypesAsync()
    {
        var queries = await _access.ForCurrentOperationAsync();
        var branches = queries.Scope.BranchIds.ToArray();
        var cacheKey = $"Lookup:DocumentTypes:Usage:{queries.Scope.CacheKey}";

        if (_cache.TryGetValue<List<LookupDto>>(cacheKey, out var cached) && cached != null)
            return ApiResponse<List<LookupDto>>.Success(cached);

        // Prevent simultaneous cache misses from repeating the count query.
        await DocumentTypesCacheLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue<List<LookupDto>>(cacheKey, out cached) && cached != null)
                return ApiResponse<List<LookupDto>>.Success(cached);

            var types = await _unitOfWork.DocumentTypes.Query()
                .AsNoTracking()
                .Where(t => t.IsActive && (t.OwnerType == null || (int)t.OwnerType < 15))
                .OrderByDescending(t => t.Documents.Count(d => !d.IsDeleted &&
                    (queries.Scope.AllBranches || (d.RetirementFile != null && branches.Contains(d.RetirementFile.PensionBranchID)))))
                .ThenBy(t => t.TypeName)
                .ThenBy(t => t.DocumentTypeID)
                .Select(t => new LookupDto { Id = t.DocumentTypeID, Name = t.TypeName })
                .ToListAsync();

            _cache.Set(cacheKey, types, DocumentTypesCacheDuration);
            return ApiResponse<List<LookupDto>>.Success(types);
        }
        finally
        {
            DocumentTypesCacheLock.Release();
        }
    }

    public async Task<ApiResponse<List<LookupDto>>> GetFileStatusesAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.FileStatuses.Query()
                .Select(s => new LookupDto { Id = s.StatusID, Name = s.StatusName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetSocialStatusesAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.SocialStatuses.Query()
                .Select(s => new LookupDto { Id = s.SocialStatusID, Name = s.StatusName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetHealthStatusesAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.HealthStatuses.Query()
                .Select(h => new LookupDto { Id = h.HealthStatusID, Name = h.HealthStatusName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetRetireeCategoriesAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.RetireeCategories.Query()
                .Select(c => new LookupDto { Id = c.CategoryID, Name = c.CategoryName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetBranchesAsync()
    {
        var queries = await _access.ForCurrentOperationAsync();
        var query = queries.Query(_unitOfWork.PensionFundBranches);

        return ApiResponse<List<LookupDto>>.Success(
            await query
                .Select(b => new LookupDto { Id = b.PensionBranchID, Name = b.BranchName })
                .ToListAsync());
    }

    public async Task<ApiResponse<List<LookupDto>>> GetRolesAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.Roles.Query()
                .Select(r => new LookupDto { Id = r.RoleID, Name = r.RoleName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetTransactionStatusesAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.TransactionStatuses.Query()
                .Select(t => new LookupDto { Id = t.StatusID, Name = t.StatusName })
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetGovernoratesAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.Retirees.Query()
                .Where(r => r.Governorate != null && r.Governorate != "")
                .Select(r => new LookupDto { Id = 0, Name = r.Governorate! })
                .Distinct()
                .OrderBy(g => g.Name)
                .ToListAsync());

    public async Task<ApiResponse<List<LookupDto>>> GetUnitsAsync()
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.MinistryApprovalItems.Query()
                .Where(a => a.UnitName != null && a.UnitName != "")
                .Select(a => new LookupDto { Id = 0, Name = a.UnitName! })
                .Distinct()
                .OrderBy(u => u.Name)
                .ToListAsync());
}
