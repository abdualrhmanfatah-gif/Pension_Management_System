using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;

namespace PensionMS.Core.Interfaces;

public interface IInvestmentProjectService
{
    Task<ApiResponse<int>> CreateProjectAsync(CreateProjectDto dto, int userId);
    Task<ApiResponse<bool>> UpdateProjectAsync(int id, UpdateProjectDto dto, int userId);
    Task<ApiResponse<bool>> DeleteProjectAsync(int id, int userId);
    Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(int id);
    Task<ApiResponse<List<ProjectDto>>> GetAllProjectsAsync();
    Task<ApiResponse<List<ProjectDto>>> GetProjectsByPortfolioAsync(int portfolioId);
    Task<ApiResponse<ProjectSummaryDto>> GetProjectSummaryAsync(int id);

    // Investment Plans
    Task<ApiResponse<List<InvestmentPlanDto>>> GetProjectPlansAsync(int projectId);
    Task<ApiResponse<int>> CreatePlanAsync(CreateInvestmentPlanDto dto, int userId);
    Task<ApiResponse<bool>> UpdatePlanAsync(int id, UpdateInvestmentPlanDto dto, int userId);
    Task<ApiResponse<bool>> DeletePlanAsync(int id, int userId);
    }
