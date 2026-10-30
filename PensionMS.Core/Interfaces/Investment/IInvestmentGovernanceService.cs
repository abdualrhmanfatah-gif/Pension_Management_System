using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;

namespace PensionMS.Core.Interfaces;

public interface IInvestmentGovernanceService
{
    // Risk Assessments
    Task<ApiResponse<int>> CreateRiskAssessmentAsync(CreateRiskDto dto, int userId);
    Task<ApiResponse<List<RiskAssessmentDto>>> GetRisksByProjectAsync(int projectId);
    Task<ApiResponse<List<RiskAssessmentDto>>> GetAllRisksAsync();

    // Approval Requests
    Task<ApiResponse<int>> CreateApprovalRequestAsync(CreateApprovalDto dto, int userId);
    Task<ApiResponse<bool>> UpdateApprovalStatusAsync(int id, UpdateApprovalDto dto, int userId);
    Task<ApiResponse<ApprovalRequestDto>> GetApprovalRequestByIdAsync(int id);
    Task<ApiResponse<List<ApprovalRequestDto>>> GetAllApprovalRequestsAsync();
}
