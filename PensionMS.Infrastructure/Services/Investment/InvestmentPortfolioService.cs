using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentPortfolioService : IInvestmentPortfolioService
{
    private readonly IUnitOfWork _unitOfWork;
    public InvestmentPortfolioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<int>> CreatePortfolioAsync(CreatePortfolioDto dto, int userId)
    {
        var manager = await _unitOfWork.Users.GetByIdAsync(dto.ManagerUserID);
        if (manager == null)
            return ApiResponse<int>.Fail("المدير المحدد غير موجود");

        var portfolio = new InvestmentPortfolio
        {
            Name = dto.Name,
            Description = dto.Description,
            ManagerUserID = dto.ManagerUserID,
            TotalTargetCapital = dto.TotalTargetCapital,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InvestmentPortfolios.AddAsync(portfolio);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(portfolio.PortfolioID, "تم إنشاء المحفظة بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdatePortfolioAsync(int id, UpdatePortfolioDto dto, int userId)
    {
        var portfolio = await _unitOfWork.InvestmentPortfolios.GetByIdAsync(id);
        if (portfolio == null || portfolio.IsDeleted)
            return ApiResponse<bool>.Fail("المحفظة غير موجودة");

        var manager = await _unitOfWork.Users.GetByIdAsync(dto.ManagerUserID);
        if (manager == null)
            return ApiResponse<bool>.Fail("المدير المحدد غير موجود");

        portfolio.Name = dto.Name;
        portfolio.Description = dto.Description;
        portfolio.ManagerUserID = dto.ManagerUserID;
        portfolio.TotalTargetCapital = dto.TotalTargetCapital;
        portfolio.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InvestmentPortfolios.Update(portfolio);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث المحفظة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeletePortfolioAsync(int id, int userId)
    {
        var portfolio = await _unitOfWork.InvestmentPortfolios.GetByIdAsync(id);
        if (portfolio == null || portfolio.IsDeleted)
            return ApiResponse<bool>.Fail("المحفظة غير موجودة");

        portfolio.IsDeleted = true;
        portfolio.DeletedAt = DateTime.UtcNow;
        portfolio.DeletedByUserID = userId;

        _unitOfWork.InvestmentPortfolios.Update(portfolio);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف المحفظة بنجاح");
    }

    public async Task<ApiResponse<PortfolioDto>> GetPortfolioByIdAsync(int id)
    {
        var portfolio = await _unitOfWork.InvestmentPortfolios.Query()
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(p => p.PortfolioID == id && !p.IsDeleted);

        if (portfolio == null)
            return ApiResponse<PortfolioDto>.Fail("المحفظة غير موجودة");

        var dto = new PortfolioDto
        {
            PortfolioID = portfolio.PortfolioID,
            Name = portfolio.Name,
            Description = portfolio.Description,
            ManagerUserID = portfolio.ManagerUserID,
            ManagerName = portfolio.Manager?.FullName ?? string.Empty,
            TotalTargetCapital = portfolio.TotalTargetCapital,
            CreatedAt = portfolio.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        };

        return ApiResponse<PortfolioDto>.Success(dto);
    }

    public async Task<ApiResponse<List<PortfolioDto>>> GetAllPortfoliosAsync()
    {
        var portfolios = await _unitOfWork.InvestmentPortfolios.Query()
            .Include(p => p.Manager)
            .Where(p => !p.IsDeleted)
            .Select(p => new PortfolioDto
            {
                PortfolioID = p.PortfolioID,
                Name = p.Name,
                Description = p.Description,
                ManagerUserID = p.ManagerUserID,
                ManagerName = p.Manager != null ? p.Manager.FullName : string.Empty,
                TotalTargetCapital = p.TotalTargetCapital,
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
            })
            .ToListAsync();

        return ApiResponse<List<PortfolioDto>>.Success(portfolios);
    }

    public async Task<ApiResponse<PortfolioSummaryDto>> GetPortfolioSummaryAsync(int id)
    {
        var portfolio = await _unitOfWork.InvestmentPortfolios.Query()
            .Include(p => p.Manager)
            .Include(p => p.Projects.Where(proj => !proj.IsDeleted))
                .ThenInclude(proj => proj.Plans.Where(pl => !pl.IsDeleted))
            .FirstOrDefaultAsync(p => p.PortfolioID == id && !p.IsDeleted);

        if (portfolio == null)
            return ApiResponse<PortfolioSummaryDto>.Fail("المحفظة غير موجودة");

        var summary = new PortfolioSummaryDto
        {
            PortfolioID = portfolio.PortfolioID,
            Name = portfolio.Name,
            ManagerName = portfolio.Manager?.FullName ?? string.Empty,
            TotalTargetCapital = portfolio.TotalTargetCapital,
            ProjectCount = portfolio.Projects.Count,
            TotalPlannedBudget = portfolio.Projects.Sum(p => p.PlannedBudget),
            TotalActualSpent = portfolio.Projects.SelectMany(p => p.Plans).Sum(pl => pl.ActualSpent),
            OccupancyRate = 0 // Would be calculated based on linked properties/units
        };

        return ApiResponse<PortfolioSummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<InvestmentStatsDto>> GetInvestmentStatsAsync()
    {
        var totalPortfolios = await _unitOfWork.InvestmentPortfolios.Query()
            .Where(p => !p.IsDeleted)
            .CountAsync();

        var totalAssetsValue = await _unitOfWork.InvestmentPortfolios.Query()
            .Where(p => !p.IsDeleted)
            .SumAsync(p => (decimal?)p.TotalTargetCapital) ?? 0m;

        var activeLeaseContracts = await _unitOfWork.LeaseContracts.Query()
            .Where(l => !l.IsDeleted && l.Status == LeaseStatusConstants.Active)
            .CountAsync();

        var openMaintenanceRequests = await _unitOfWork.MaintenanceRequests.Query()
            .Where(m => !m.IsDeleted && m.Status != MaintenanceStatusConstants.Completed && m.Status != MaintenanceStatusConstants.Cancelled)
            .CountAsync();

        var pendingApprovalsCount = await _unitOfWork.InvestmentApprovalRequests.Query()
            .Where(a => !a.IsDeleted && a.Status == ApprovalStatusConstants.Pending)
            .CountAsync();

        var stats = new InvestmentStatsDto
        {
            TotalPortfolios = totalPortfolios,
            TotalAssetsValue = totalAssetsValue,
            ActiveLeaseContracts = activeLeaseContracts,
            OpenMaintenanceRequests = openMaintenanceRequests,
            PendingApprovalsCount = pendingApprovalsCount
        };

        return ApiResponse<InvestmentStatsDto>.Success(stats);
    }
}
