using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Salary;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/salary-batches")]
[Authorize]
public class SalaryPaymentBatchesController : ControllerBase
{
    private readonly ISalaryPaymentBatchService _service;

    public SalaryPaymentBatchesController(ISalaryPaymentBatchService service)
    {
        _service = service;
    }

    /// <summary>
    /// جلب جميع كشوفات الصرف
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SalaryBatch.View")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, status);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب تفاصيل كشف صرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "SalaryBatch.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// إنشاء كشف صرف جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SalaryBatch.Manage")]
    public async Task<IActionResult> Create([FromBody] CreateSalaryBatchDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.BatchID }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// توليد عناصر الصرف تلقائياً
    /// </summary>
    [HttpPost("{id:int}/generate")]
    [Authorize(Policy = "SalaryBatch.Manage")]
    public async Task<IActionResult> GenerateItems(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.GenerateItemsAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// اعتماد كشف الصرف
    /// </summary>
    [HttpPut("{id:int}/approve")]
    [Authorize(Policy = "SalaryBatch.Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.ApproveAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف كشف صرف (مسودة فقط)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SalaryBatch.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب عناصر صرف كشف معين
    /// </summary>
    [HttpGet("{id:int}/items")]
    [Authorize(Policy = "SalaryBatch.View")]
    public async Task<IActionResult> GetBatchItems(int id)
    {
        var result = await _service.GetItemsByBatchIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب عناصر صرف ملف تقاعد معين
    /// </summary>
    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "SalaryBatch.View")]
    public async Task<IActionResult> GetFileItems(int fileId)
    {
        var result = await _service.GetItemsByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    /// <summary>
    /// تحديث حالة عنصر صرف
    /// </summary>
    [HttpPut("items/{itemId:int}/status")]
    [Authorize(Policy = "SalaryBatch.Manage")]
    public async Task<IActionResult> UpdateItemStatus(int itemId, [FromBody] UpdateItemStatusDto dto)
    {
        var result = await _service.UpdateItemStatusAsync(itemId, dto);
        return result.ToActionResult();
    }

    /// <summary>
    /// تصدير كشف صرف إلى Excel
    /// </summary>
    [HttpGet("{id:int}/export")]
    [Authorize(Policy = "SalaryBatch.Export")]
    public async Task<IActionResult> ExportToExcel(int id)
    {
        var bytes = await _service.ExportBatchToExcelAsync(id);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"salary_batch_{id}.xlsx");
    }

    /// <summary>
    /// استيراد نتائج المعاملات من Excel
    /// </summary>
    [HttpPost("{id:int}/import")]
    [Authorize(Policy = "SalaryBatch.ImportResults")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportResults(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<object>.Fail("يرجى رفع ملف Excel").ToActionResult();

        using var stream = file.OpenReadStream();
        var result = await _service.ImportTransactionResultsAsync(id, stream);
        return result.ToActionResult();
    }
}


