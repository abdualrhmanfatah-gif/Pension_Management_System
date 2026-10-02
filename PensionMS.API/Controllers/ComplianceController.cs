using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Compliance;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService _complianceService;
    private readonly IImmutableAuditService _auditService;

    public ComplianceController(
        IComplianceService complianceService,
        IImmutableAuditService auditService)
    {
        _complianceService = complianceService;
        _auditService = auditService;
    }

    /// <summary>
    /// لوحة تحكم مسؤول الامتثال — ملخص شامل للمخاطر والتجاوزات
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _complianceService.GetDashboardAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// قائمة أحداث المخاطر المفتوحة (مع فلتر اختياري للخطورة)
    /// </summary>
    [HttpGet("risk-events")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> GetRiskEvents([FromQuery] string? severity)
    {
        var result = await _complianceService.GetOpenRiskEventsAsync(severity);
        return result.ToActionResult();
    }

    /// <summary>
    /// قائمة التجاوزات المعلقة التي تحتاج مراجعة
    /// </summary>
    [HttpGet("overrides/pending")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> GetPendingOverrides()
    {
        var result = await _complianceService.GetPendingOverridesAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// حل حدث مخاطر والتأشير عليه كـ Resolved
    /// </summary>
    [HttpPost("risk-events/{id:long}/resolve")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> ResolveRiskEvent(long id, [FromBody] ResolveRiskRequest request)
    {
        var userId = this.GetUserId();
        var result = await _complianceService.ResolveRiskEventAsync(id, userId, request.Notes);
        return result.ToActionResult();
    }

    /// <summary>
    /// التحقق من سلامة سلسلة التدقيق (Hash Chain Integrity)
    /// </summary>
    [HttpGet("audit/verify-chain")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> VerifyAuditChain()
    {
        var failedCount = await _auditService.VerifyChainIntegrityAsync();
        return ApiResponse<object>.Success(new
        {
            isIntact = failedCount == 0,
            failedRecords = failedCount,
            message = failedCount == 0
                ? "سلسلة التدقيق سليمة"
                : $"تم اكتشاف {failedCount} سجل(ات) تالفة في السلسلة"
        }).ToActionResult();
    }
}
