using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.Constants;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Ministry;
using PensionMS.Core.DTOs.MinistryApprovals;
using PensionMS.Core.Interfaces;
using System.Security.Claims;
<<<<<<< HEAD
using System.ComponentModel.DataAnnotations;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/ministry-approvals")]
[Authorize]
public class MinistryApprovalsController : ControllerBase
{
    private readonly IMinistryApprovalService _approvalService;

    public MinistryApprovalsController(IMinistryApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet("batches")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _approvalService.GetAllBatchesAsync();
        return result.ToActionResult();
    }

    [HttpGet("batches/{id:int}")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _approvalService.GetBatchByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("batches")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> Create([FromBody] CreateBatchDto dto)
    {
        var userId = this.GetUserId();
        var result = await _approvalService.CreateBatchAsync(dto, userId);
        if (!result.IsSuccess) return result.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.BatchID }, result);
    }

    [HttpPost("batches/{id:int}/import")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> ImportExcel(int id, [FromForm] ImportBatchFromExcelDto dto)
    {
        var result = await _approvalService.ImportExcelAsync(id, dto.File);
        return result.ToActionResult();
    }

    [HttpGet("batches/{id:int}/items")]
    [Authorize(Policy = "Ministry.View")]
<<<<<<< HEAD
    public async Task<IActionResult> GetItems(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? q = null,
        [FromQuery, Range(GenderConstants.Male, GenderConstants.Female, ErrorMessage = "قيمة الجنس يجب أن تكون 1 (ذكر) أو 2 (أنثى)")] byte? gender = null)
    {
        var result = await _approvalService.GetBatchItemsAsync(id, pageNumber, pageSize, q, gender);
=======
    public async Task<IActionResult> GetItems(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? q = null)
    {
        var result = await _approvalService.GetBatchItemsAsync(id, pageNumber, pageSize, q);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return result.ToActionResult();
    }

    [HttpGet("all-items")]
    [Authorize(Policy = "Ministry.View")]
<<<<<<< HEAD
    public async Task<IActionResult> GetAllItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null, [FromQuery] bool? isProcessed = null,
        [FromQuery, Range(GenderConstants.Male, GenderConstants.Female, ErrorMessage = "قيمة الجنس يجب أن تكون 1 (ذكر) أو 2 (أنثى)")] byte? gender = null)
    {
        var result = await _approvalService.GetAllItemsAsync(pageNumber, pageSize, q, isProcessed, gender);
=======
    public async Task<IActionResult> GetAllItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null, [FromQuery] bool? isProcessed = null)
    {
        var result = await _approvalService.GetAllItemsAsync(pageNumber, pageSize, q, isProcessed);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return result.ToActionResult();
    }

    [HttpGet("items/{itemId:int}")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetItem(int itemId)
    {
        var result = await _approvalService.GetItemByIdAsync(itemId);
        return result.ToActionResult();
    }

    [HttpDelete("batches/{id:int}")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _approvalService.DeleteBatchAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpPost("batches/{id:int}/items")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> AddManualItem(int id, [FromBody] AddManualItemDto dto)
    {
        var userId = this.GetUserId();
        var result = await _approvalService.AddManualItemAsync(id, dto, userId);
        return result.ToActionResult();
    }

<<<<<<< HEAD
    [HttpPut("items/{itemId:int}")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> UpdateManualItem(int itemId, [FromBody] UpdateManualItemDto dto)
    {
        var result = await _approvalService.UpdateManualItemAsync(itemId, dto);
        return result.ToActionResult();
    }

    [HttpDelete("items/{itemId:int}")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> DeleteManualItem(int itemId)
    {
        var userId = this.GetUserId();
        var result = await _approvalService.DeleteManualItemAsync(itemId, userId);
        return result.ToActionResult();
    }

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    [HttpPost("items/{itemId:int}/process")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> ProcessItem(int itemId)
    {
        var userId = this.GetUserId();
        var result = await _approvalService.ProcessItemAsync(itemId, userId);
        return result.ToActionResult();
    }

    [HttpGet("analytics/dashboard")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await _approvalService.GetDashboardStatsAsync();
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> SearchGlobalForce([FromQuery] string q)
    {
        var result = await _approvalService.SearchGlobalForceAsync(q);
        return result.ToActionResult();
    }

    [HttpGet("batches/{id:int}/reconciliation")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetReconciliationMatrix(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _approvalService.GetReconciliationMatrixAsync(id, pageNumber, pageSize);
        return result.ToActionResult();
    }

    [HttpPost("bulk-process")]
    [Authorize(Policy = "Ministry.Manage")]
    public async Task<IActionResult> ProcessBulkApprovals([FromBody] BulkProcessRequestDto dto)
    {
        var userId = this.GetUserId();
        var result = await _approvalService.ProcessBulkApprovalsAsync(dto.ApprovalIds, userId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}/audit-trail")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetBatchAuditTrail(int id)
    {
        var result = await _approvalService.GetBatchAuditTrailAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}/export")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> ExportBatchReport(int id, [FromQuery] string format = "excel")
    {
        var bytes = await _approvalService.ExportBatchReportAsync(id, format);
        if (bytes == null || bytes.Length == 0) return NotFound();

<<<<<<< HEAD
        var fileName = $"Ministry_Batch_{id}_{PensionMS.Core.Time.YemenTime.Now:yyyyMMdd}";
=======
        var fileName = $"Ministry_Batch_{id}_{DateTime.Now:yyyyMMdd}";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (format.ToLower() == ExportFormatConstants.Pdf)
        {
            return File(bytes, "application/pdf", $"{fileName}.pdf");
        }
        
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }
}
