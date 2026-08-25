using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/projects")]
[Authorize]
public class InvestmentProjectsController : ControllerBase
{
    private readonly IInvestmentProjectService _service;

    public InvestmentProjectsController(IInvestmentProjectService service)
    {
        _service = service;
    }

    [HttpGet("all")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllProjectsAsync();
        return result.ToActionResult();
    }

    [HttpGet("portfolio/{portfolioId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByPortfolio(int portfolioId)
    {
        var result = await _service.GetProjectsByPortfolioAsync(portfolioId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetProjectByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("summary/{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetSummary(int id)
    {
        var result = await _service.GetProjectSummaryAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateProjectAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateProjectAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteProjectAsync(id, userId);
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Investment Plans
    // ----------------------------------------------------

    [HttpGet("{projectId:int}/plans")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetPlans(int projectId)
    {
        var result = await _service.GetProjectPlansAsync(projectId);
        return result.ToActionResult();
    }

    [HttpPost("plans")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateInvestmentPlanDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreatePlanAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("plans/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdateInvestmentPlanDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdatePlanAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("plans/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeletePlanAsync(id, userId);
        return result.ToActionResult();
    }
}


