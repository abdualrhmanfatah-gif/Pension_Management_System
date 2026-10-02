using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Financial;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/financial-entitlements")]
[Authorize]
public class FinancialEntitlementsController : ControllerBase
{
    private readonly IFinancialEntitlementService _service;

    public FinancialEntitlementsController(IFinancialEntitlementService service)
    {
        _service = service;
    }

    /// <summary>
    /// جلب جميع استحقاقات متقاعد
    /// </summary>
    [HttpGet("by-retiree/{retireeId:int}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetByRetireeId(int retireeId)
    {
        var result = await _service.GetByRetireeIdAsync(retireeId);
        return result.ToActionResult();
    }

    /// <summary>
    /// بحث الاستحقاقات المالية (رقم الملف، الرقم العسكري، هاتف المتتقاعد، اسم المتتقاعد، اسم الوكيل)
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var result = await _service.SearchAsync(q);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب الاستحقاق الساري حالياً
    /// </summary>
    [HttpGet("active/{retireeId:int}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetActive(int retireeId)
    {
        var result = await _service.GetActiveByRetireeIdAsync(retireeId);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب استحقاق بالمعرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// إنشاء استحقاق مالي جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "File.Edit.FinancialData")]
    public async Task<IActionResult> Create([FromBody] CreateFinancialEntitlementDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.EntitlementID }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// إنهاء استحقاق مالي
    /// </summary>
    [HttpPut("{id:int}/end")]
    [Authorize(Policy = "File.Edit.FinancialData")]
    public async Task<IActionResult> End(int id, [FromBody] EndEntitlementRequest request)
    {
        var userId = this.GetUserId();
        var result = await _service.EndAsync(id, request.EndDate, userId);
        return result.ToActionResult();
    }
}


