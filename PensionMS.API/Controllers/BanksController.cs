using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Banks;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BanksController : ControllerBase
{
    private readonly IBankService _bankService;

    public BanksController(IBankService bankService)
    {
        _bankService = bankService;
    }

    /// <summary>
    /// جلب قائمة البنوك المتاحة
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "System.Settings.Banks")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _bankService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب بيانات بنك محدد باستخدام المعرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "System.Settings.Banks")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _bankService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// إضافة بنك جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBankDto dto)
    {
        var result = await _bankService.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// تحديث بيانات بنك موجود
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBankDto dto)
    {
        var result = await _bankService.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف بنك موجود
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bankService.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// تفعيل/تعطيل بنك محدد
    /// </summary>
    [HttpPatch("{id:int}/toggle-active")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _bankService.ToggleActiveAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}
