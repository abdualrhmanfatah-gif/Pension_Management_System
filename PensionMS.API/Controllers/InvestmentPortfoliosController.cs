using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/portfolios")]
[Authorize]
public class InvestmentPortfoliosController : ControllerBase
{
    private readonly IInvestmentPortfolioService _service;

    public InvestmentPortfoliosController(IInvestmentPortfolioService service)
    {
        _service = service;
    }

    [HttpGet("all")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllPortfoliosAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetPortfolioByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("summary/{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetSummary(int id)
    {
        var result = await _service.GetPortfolioSummaryAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreatePortfolioAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePortfolioDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdatePortfolioAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeletePortfolioAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpGet("stats")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetInvestmentStatsAsync();
        return result.ToActionResult();
    }
}


