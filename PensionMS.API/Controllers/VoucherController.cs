using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces.Accounting;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/accounting/vouchers")]
[Authorize]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VoucherController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    private int UserId => int.Parse(User.FindFirst("identifier")?.Value ?? "1");

    /// <summary>
    /// إنشاء سند صرف أو قبض
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
    {
        var result = await _voucherService.CreateVoucherAsync(dto, UserId);
        return result.ToActionResult();
    }

    /// <summary>
    /// عرض سند
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _voucherService.GetVoucherByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// قائمة السندات
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> List([FromQuery] VoucherFilterDto filter)
    {
        var result = await _voucherService.GetVouchersAsync(filter);
        return result.ToActionResult();
    }

    /// <summary>
    /// تعديل سند (مسودة فقط)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVoucherDto dto)
    {
        var result = await _voucherService.UpdateVoucherAsync(id, dto);
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف سند (مسودة فقط)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _voucherService.DeleteVoucherAsync(id, UserId);
        return result.ToActionResult();
    }

    /// <summary>
    /// ترحيل السند وإنشاء القيد اليومي
    /// </summary>
    [HttpPost("{id:int}/post")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Post(int id)
    {
        var result = await _voucherService.PostVoucherAsync(id, UserId);
        return result.ToActionResult();
    }

    /// <summary>
    /// إلغاء السند وعكس القيد
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _voucherService.CancelVoucherAsync(id, UserId);
        return result.ToActionResult();
    }

    /// <summary>
    /// رقم السند التالي
    /// </summary>
    [HttpGet("next-number")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetNextNumber([FromQuery] VoucherType type, [FromQuery] DateTime? date)
    {
        var result = await _voucherService.GetNextVoucherNumberAsync(type, date);
        return result.ToActionResult();
    }

    /// <summary>
    /// استيراد سندات من ملف Excel
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;
        var result = await _voucherService.ImportVouchersAsync(stream, file.FileName, UserId);
        return result.ToActionResult();
    }
}
