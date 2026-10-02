using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBranchScopeProvider _branchScopeProvider;

    public LookupService(IUnitOfWork unitOfWork, IBranchScopeProvider branchScopeProvider)
    {
        _unitOfWork = unitOfWork;
        _branchScopeProvider = branchScopeProvider;
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
        => ApiResponse<List<LookupDto>>.Success(
            await _unitOfWork.DocumentTypes.Query()
                .Where(d => d.IsActive && (d.OwnerType == null || (int)d.OwnerType < 15)) // استبعاد أنواع وثائق الاستثمار (InvestmentProject = 15+)
                .Select(d => new LookupDto { Id = d.DocumentTypeID, Name = d.TypeName })
                .ToListAsync());

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
        var query = _unitOfWork.PensionFundBranches.Query();

        var branchId = _branchScopeProvider.GetCurrentBranchId();
        if (branchId.HasValue && !_branchScopeProvider.IsHQ())
        {
            query = query.Where(b => b.PensionBranchID == branchId.Value);
        }

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
