using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.API.Controllers;

/// <summary>
/// التحكم في نظام الموازنة
/// </summary>
[ApiController]
[Route("api/budget")]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budget;

    public BudgetController(IBudgetService budget)
    {
        _budget = budget;
    }

    // ═══════════════════════════════════════
    // CRUD
    // ═══════════════════════════════════════

    [HttpGet("items")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetBudgetItems([FromQuery] BudgetFilterDto filter)
    {
        var result = await _budget.GetBudgetItemsAsync(filter);
        return result.ToActionResult();
    }

    [HttpGet("items/{id:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetBudgetItemById(int id)
    {
        var result = await _budget.GetBudgetItemByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("items")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> CreateBudgetItem([FromBody] CreateBudgetItemDto dto)
    {
        var result = await _budget.CreateBudgetItemAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPut("items/{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> UpdateBudgetItem(
        int id, [FromBody] UpdateBudgetItemDto dto)
    {
        var result = await _budget.UpdateBudgetItemAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("items/{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> DeleteBudgetItem(int id)
    {
        var result = await _budget.DeleteBudgetItemAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // BULK OPERATIONS
    // ═══════════════════════════════════════

    [HttpPost("items/bulk")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> BulkCreateBudgetItems(
        [FromBody] List<CreateBudgetItemDto> items)
    {
        var result = await _budget.BulkCreateBudgetItemsAsync(items, this.GetUserId());
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // SEED BUDGET
    // ═══════════════════════════════════════

    [HttpPost("seed/{fiscalYearId:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> SeedBudget(int fiscalYearId)
    {
        var result = await _budget.SeedBudgetAsync(fiscalYearId, this.GetUserId());
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════

    [HttpGet("reports/annual/{fiscalYearId:int}")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetBudgetReport(int fiscalYearId)
    {
        var result = await _budget.GetBudgetReportAsync(fiscalYearId);
        return result.ToActionResult();
    }

    [HttpGet("reports/quarterly/{fiscalYearId:int}")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetQuarterlyBudgetReport(int fiscalYearId)
    {
        var result = await _budget.GetQuarterlyBudgetReportAsync(fiscalYearId);
        return result.ToActionResult();
    }

    [HttpGet("reports/quarter/{quarterId:int}")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetQuarterBudgetReport(int quarterId)
    {
        var result = await _budget.GetQuarterBudgetReportAsync(quarterId);
        return result.ToActionResult();
    }

    [HttpGet("reports/monthly/{fiscalYearId:int}")]
    [Authorize(Policy = "Accounting.Reports")]
    public async Task<IActionResult> GetMonthlyBudgetReport(int fiscalYearId)
    {
        var result = await _budget.GetMonthlyBudgetReportAsync(fiscalYearId);
        return result.ToActionResult();
    }
}
