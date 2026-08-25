using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Settlements;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/settlements")]
[Authorize]
public class SettlementsController : ControllerBase
{
    private readonly ISettlementService _settlementService;

    public SettlementsController(ISettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    // ── Decision-centric endpoints (primary UI) ────────

    [HttpGet("decisions")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetAllDecisions([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null)
    {
        var result = await _settlementService.GetAllDecisionsAsync(page, pageSize, searchTerm);
        return Ok(result);
    }

    [HttpGet("decisions/{id:int}")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetDecision(int id)
    {
        var result = await _settlementService.GetDecisionByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("decisions/{id:int}/items")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetDecisionItems(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null)
    {
        var result = await _settlementService.GetDecisionItemsAsync(id, page, pageSize, searchTerm);
        return Ok(result);
    }

    [HttpPost("decisions/upload")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> CreateDecisionWithExcel([FromForm] ImportSettlementExcelDto dto)
    {
        var userId = this.GetUserId();
        var result = await _settlementService.CreateDecisionWithExcelAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("decisions/{id:int}")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> DeleteDecision(int id)
    {
        var userId = this.GetUserId();
        var result = await _settlementService.DeleteDecisionAsync(id, userId);
        return result.ToActionResult();
    }

    // ── Batch endpoints (kept for flexibility) ─────────

    [HttpGet("batches")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetAllBatches()
    {
        var result = await _settlementService.GetAllBatchesAsync();
        return result.ToActionResult();
    }

    [HttpGet("batches/{id:int}")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetBatchById(int id)
    {
        var result = await _settlementService.GetBatchByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("batches")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateSettlementBatchDto dto)
    {
        var userId = this.GetUserId();
        var result = await _settlementService.CreateBatchAsync(dto, userId);
        if (!result.IsSuccess) return result.ToActionResult();
        return result.ToActionResult();
    }

    [HttpPost("batches/{id:int}/import")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> ImportExcel(int id, [FromForm] ImportSettlementExcelDto dto)
    {
        var userId = this.GetUserId();
        var result = await _settlementService.ImportExcelAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpGet("batches/{id:int}/items")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetBatchItems(int id)
    {
        var result = await _settlementService.GetBatchItemsAsync(id);
        return result.ToActionResult();
    }

    [HttpDelete("batches/{id:int}")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> DeleteBatch(int id)
    {
        var userId = this.GetUserId();
        var result = await _settlementService.DeleteBatchAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpGet("batches/{id:int}/decisions")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetBatchDecisions(int id)
    {
        var result = await _settlementService.GetBatchDecisionsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("batches/{id:int}/decisions")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> CreateDecision(int id, [FromBody] CreateSettlementDecisionDto dto)
    {
        var userId = this.GetUserId();
        var result = await _settlementService.CreateDecisionAsync(id, dto, userId);
        return result.ToActionResult();
    }

    // ── By Retiree ──────────────────────────────────────

    [HttpPut("items/link")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> LinkItemToRetiree([FromBody] LinkSettlementItemDto dto)
    {
        var userId = this.GetUserId();
        var result = await _settlementService.LinkItemToRetireeAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpGet("by-retiree/{retireeId:int}")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetByRetireeId(int retireeId)
    {
        var result = await _settlementService.GetByRetireeIdAsync(retireeId);
        return result.ToActionResult();
    }

    // ── Export ────────────────────────────────────────────

    [HttpGet("decisions/{id:int}/export")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> ExportDecisionToExcel(int id)
    {
        var bytes = await _settlementService.ExportDecisionToExcelAsync(id);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"settlement_decision_{id}.xlsx");
    }

    [HttpPost("decisions/{id:int}/export-failures")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "Settlement.Manage")]
    public async Task<IActionResult> ExportImportFailures(int id, [FromForm] IFormFile file)
    {
        if (file == null) return BadRequest("الملف مطلوب");
        var remoteFile = new PensionMS.API.Helpers.RemoteFileWrapper(file);
        var bytes = await _settlementService.GenerateImportFailuresExcelAsync(remoteFile, id);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "import_failures_report.xlsx");
    }

    [HttpGet("decisions/{id:int}/failures-report")]
    [Authorize(Policy = "Settlement.View")]
    public async Task<IActionResult> GetBatchFailureReport(int id)
    {
        var bytes = await _settlementService.ExportBatchFailureReportAsync(id);
        if (bytes == null || bytes.Length == 0) return NotFound("لا يوجد تقرير أخطاء متاح لهذا القرار");
        
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"failures_report_{id}.xlsx");
    }
}


