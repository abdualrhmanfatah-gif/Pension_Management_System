using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentGovernanceService : IInvestmentGovernanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvestmentGovernanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ----------------------------------------------------------------------
    // RISK ASSESSMENTS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateRiskAssessmentAsync(CreateRiskDto dto, int userId)
    {
        var project = await _unitOfWork.InvestmentProjects.GetByIdAsync(dto.ProjectID);
        if (project == null || project.IsDeleted)
            return ApiResponse<int>.Fail("المشروع غير موجود");

        var risk = new InvestmentRiskAssessment
        {
            ProjectID = dto.ProjectID,
            RiskType = dto.RiskType,
            RiskDescription = dto.RiskDescription,
            RiskScore = dto.RiskScore,
            MitigationPlan = dto.MitigationPlan,
            ResponsiblePerson = dto.ResponsiblePerson,
            AssessedDate = dto.AssessedDate,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InvestmentRiskAssessments.AddAsync(risk);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(risk.RiskID, "تم تسجيل تقييم المخاطر بنجاح");
    }

    public async Task<ApiResponse<List<RiskAssessmentDto>>> GetRisksByProjectAsync(int projectId)
    {
        var rawRisks = await _unitOfWork.InvestmentRiskAssessments.Query()
            .Where(r => r.ProjectID == projectId && !r.IsDeleted)
            .OrderByDescending(r => r.AssessedDate)
            .ToListAsync();

        var risks = rawRisks.Select(r => new RiskAssessmentDto
        {
            RiskID = r.RiskID,
            ProjectID = r.ProjectID,
            RiskType = r.RiskType,
            RiskDescription = r.RiskDescription,
            RiskScore = r.RiskScore,
            MitigationPlan = r.MitigationPlan,
            ResponsiblePerson = r.ResponsiblePerson,
            AssessedDate = r.AssessedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        }).ToList();

        return ApiResponse<List<RiskAssessmentDto>>.Success(risks);
    }

    public async Task<ApiResponse<List<RiskAssessmentDto>>> GetAllRisksAsync()
    {
        var rawRisks = await _unitOfWork.InvestmentRiskAssessments.Query()
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.AssessedDate)
            .ToListAsync();

        var risks = rawRisks.Select(r => new RiskAssessmentDto
        {
            RiskID = r.RiskID,
            ProjectID = r.ProjectID,
            RiskType = r.RiskType,
            RiskDescription = r.RiskDescription,
            RiskScore = r.RiskScore,
            MitigationPlan = r.MitigationPlan,
            ResponsiblePerson = r.ResponsiblePerson,
            AssessedDate = r.AssessedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        }).ToList();

        return ApiResponse<List<RiskAssessmentDto>>.Success(risks);
    }

    // ----------------------------------------------------------------------
    // APPROVAL REQUESTS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateApprovalRequestAsync(CreateApprovalDto dto, int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<int>.Fail("المستخدم غير موجود");

        var request = new InvestmentApprovalRequest
        {
            EntityType = dto.EntityType,
            EntityID = dto.EntityID,
            RequestedByUserID = userId,
            WorkflowStage = dto.WorkflowStage,
            DecisionHistory = "[]", // Initially empty JSON array
            Status = LookupConstants.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InvestmentApprovalRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(request.ApprovalID, "تم إنشاء طلب الاعتماد بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdateApprovalStatusAsync(int id, UpdateApprovalDto dto, int userId)
    {
        var request = await _unitOfWork.InvestmentApprovalRequests.GetByIdAsync(id);
        if (request == null || request.IsDeleted)
            return ApiResponse<bool>.Fail("طلب الاعتماد غير موجود");

        request.Status = dto.Status;
        
        // Fix JSON injection: Use proper serialization
        var history = new List<object>();
        if (!string.IsNullOrEmpty(request.DecisionHistory) && request.DecisionHistory != InvestmentConstants.EmptyJsonArray)
        {
            try {
                history = System.Text.Json.JsonSerializer.Deserialize<List<object>>(request.DecisionHistory) ?? new List<object>();
            } catch { /* Handle potential legacy malformed JSON if necessary */ }
        }

        history.Add(new
        {
            Status = dto.Status,
            Note = dto.DecisionNote,
            Date = DateTime.UtcNow.ToString("O"),
            UserID = userId
        });

        request.DecisionHistory = System.Text.Json.JsonSerializer.Serialize(history);

        request.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InvestmentApprovalRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث حالة طلب الاعتماد بنجاح");
    }

    public async Task<ApiResponse<ApprovalRequestDto>> GetApprovalRequestByIdAsync(int id)
    {
        var request = await _unitOfWork.InvestmentApprovalRequests.Query()
            .Include(r => r.RequestedBy)
            .FirstOrDefaultAsync(r => r.ApprovalID == id && !r.IsDeleted);

        if (request == null)
            return ApiResponse<ApprovalRequestDto>.Fail("طلب الاعتماد غير موجود");

        var dto = new ApprovalRequestDto
        {
            ApprovalID = request.ApprovalID,
            EntityType = request.EntityType,
            EntityID = request.EntityID,
            RequestedByUserID = request.RequestedByUserID,
            RequestedByUserName = request.RequestedBy?.FullName ?? string.Empty,
            WorkflowStage = request.WorkflowStage,
            DecisionHistory = request.DecisionHistory,
            Status = request.Status,
            CreatedAt = request.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        };

        return ApiResponse<ApprovalRequestDto>.Success(dto);
    }

    public async Task<ApiResponse<List<ApprovalRequestDto>>> GetAllApprovalRequestsAsync()
    {
        var requests = await _unitOfWork.InvestmentApprovalRequests.Query()
            .Include(r => r.RequestedBy)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ApprovalRequestDto
            {
                ApprovalID = r.ApprovalID,
                EntityType = r.EntityType,
                EntityID = r.EntityID,
                RequestedByUserID = r.RequestedByUserID,
                RequestedByUserName = r.RequestedBy != null ? r.RequestedBy.FullName : string.Empty,
                WorkflowStage = r.WorkflowStage,
                DecisionHistory = r.DecisionHistory,
                Status = r.Status,
                CreatedAt = r.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
            })
            .ToListAsync();

        return ApiResponse<List<ApprovalRequestDto>>.Success(requests);
    }
}
