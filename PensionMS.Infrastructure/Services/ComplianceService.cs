using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Compliance;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// خدمة الامتثال — تجمع بيانات لوحة التحكم لمسؤول الامتثال.
/// </summary>
public class ComplianceService : IComplianceService
{
    private readonly IUnitOfWork _unitOfWork;

    public ComplianceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ComplianceDashboardDto>> GetDashboardAsync()
    {
        var openRisks = await _unitOfWork.RiskEvents.Query()
            .Where(r => r.Status == RiskStatus.Open)
            .ToListAsync();

        var dashboard = new ComplianceDashboardDto
        {
            TotalOpenRiskEvents = openRisks.Count,
            CriticalRiskEvents = openRisks.Count(r => r.Severity == RiskSeverity.Critical),
            HighRiskEvents = openRisks.Count(r => r.Severity == RiskSeverity.High),
            RecentRiskEvents = await GetOpenRiskEventsInternalAsync(),
            PendingOverrideReviews = await GetPendingOverridesInternalAsync(),
            BranchRiskSummary = await GetBranchRiskSummaryAsync()
        };

        return ApiResponse<ComplianceDashboardDto>.Success(dashboard);
    }

    public async Task<ApiResponse<bool>> ResolveRiskEventAsync(long riskEventId, int resolvedByUserId, string notes)
    {
        var risk = await _unitOfWork.RiskEvents.GetByIdAsync((int)riskEventId);
        if (risk == null) return ApiResponse<bool>.Fail("حدث المخاطر غير موجود");

        risk.Status = RiskStatus.Resolved;
        risk.ResolvedAt = DateTime.UtcNow;
        risk.ResolvedByUserId = resolvedByUserId;
        risk.ResolutionNotes = notes;

        _unitOfWork.RiskEvents.Update(risk);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم حل حدث المخاطر بنجاح");
    }

    public async Task<ApiResponse<List<RiskEventSummaryDto>>> GetOpenRiskEventsAsync(string? severity = null)
    {
        var data = await GetOpenRiskEventsInternalAsync(severity);
        return ApiResponse<List<RiskEventSummaryDto>>.Success(data);
    }

    public async Task<ApiResponse<List<OverrideReviewDto>>> GetPendingOverridesAsync()
    {
        var data = await GetPendingOverridesInternalAsync();
        return ApiResponse<List<OverrideReviewDto>>.Success(data);
    }

    private async Task<List<RiskEventSummaryDto>> GetOpenRiskEventsInternalAsync(string? severity = null)
    {
        var query = _unitOfWork.RiskEvents.Query()
            .Where(r => r.Status == RiskStatus.Open || r.Status == RiskStatus.Investigating);

        if (!string.IsNullOrEmpty(severity) && Enum.TryParse<RiskSeverity>(severity, true, out var severityEnum))
            query = query.Where(r => r.Severity == severityEnum);

        return await query
            .OrderByDescending(r => r.RaisedAt)
            .Take(10)
            .Select(r => new RiskEventSummaryDto
            {
                Id = r.Id,
                FileId = r.FileId,
                RiskType = r.RiskType.ToString(),
                Description = r.Description,
                Severity = r.Severity.ToString(),
                RiskScore = r.RiskScore,
                RaisedAt = r.RaisedAt,
                Status = r.Status.ToString()
            })
            .ToListAsync();
    }

    private async Task<List<OverrideReviewDto>> GetPendingOverridesInternalAsync()
    {
        // تمثيل مبسط للمهام المعلقة 
        return await _unitOfWork.PendingTasks.Query()
            .Where(t => t.Status == LeaveStatusConstants.Pending)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new OverrideReviewDto
            {
                LogId = t.Id,
                FileId = t.FileId ?? 0,
                ActorName = t.AssignedToUser != null ? t.AssignedToUser.FullName : "نظام",
                ActionDate = t.CreatedAt,
                Reason = "مراجعة تجاوز القيود"
            })
            .ToListAsync();
    }

    private async Task<List<BranchRiskDto>> GetBranchRiskSummaryAsync()
    {
        var branches = await _unitOfWork.PensionFundBranches.Query()
            .AsNoTracking()
            .Select(b => new BranchRiskDto
            {
                BranchId = b.PensionBranchID,
                BranchName = b.BranchName ?? "",
                OpenRiskCount = _unitOfWork.RiskEvents.Query()
                    .Count(r => r.File != null && r.File.PensionBranchID == b.PensionBranchID && r.Status == RiskStatus.Open),
                TotalPendingTasks = _unitOfWork.PendingTasks.Query()
                    .Count(t => t.File != null && t.File.PensionBranchID == b.PensionBranchID && t.Status == LeaveStatusConstants.Pending)
            })
            .Where(b => b.OpenRiskCount > 0)
            .OrderByDescending(b => b.OpenRiskCount)
            .Take(10)
            .ToListAsync();

        return branches;
    }
}
