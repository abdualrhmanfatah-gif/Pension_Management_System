using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/governance")]
[Authorize]
public class InvestmentGovernanceController : ControllerBase
{
    private readonly IInvestmentGovernanceService _service;

    public InvestmentGovernanceController(IInvestmentGovernanceService service)
    {
        _service = service;
    }

    // ----------------------------------------------------
    // Risk Assessments
    // ----------------------------------------------------

    [HttpGet("projects/{projectId:int}/risks")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetRisksByProject(int projectId)
    {
        var result = await _service.GetRisksByProjectAsync(projectId);
        return result.ToActionResult();
    }

    [HttpGet("risks")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAllRisks()
    {
        var result = await _service.GetAllRisksAsync();
        return result.ToActionResult();
    }

    [HttpPost("risks")]
    [Authorize(Policy = "Investment.Approve")]
    public async Task<IActionResult> CreateRiskAssessment([FromBody] CreateRiskDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateRiskAssessmentAsync(dto, userId);
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Approval Requests
    // ----------------------------------------------------

    [HttpGet("approvals")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAllApprovals()
    {
        var result = await _service.GetAllApprovalRequestsAsync();
        return result.ToActionResult();
    }

    [HttpGet("approvals/{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetApprovalById(int id)
    {
        var result = await _service.GetApprovalRequestByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("approvals")]
    [Authorize(Policy = "Investment.Approve")]
    public async Task<IActionResult> CreateApprovalRequest([FromBody] CreateApprovalDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateApprovalRequestAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("approvals/{id:int}/status")]
    [Authorize(Policy = "Investment.Approve")]
    public async Task<IActionResult> UpdateApprovalStatus(int id, [FromBody] UpdateApprovalDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateApprovalStatusAsync(id, dto, userId);
        return result.ToActionResult();
    }
}


