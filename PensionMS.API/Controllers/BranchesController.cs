using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Branches;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    /// <summary>
    /// جلب قائمة فروع صندوق التقاعد
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _branchService.GetAllAsync();
        return ApiResponse<List<BranchDto>>.Success(result).ToActionResult();
    }

    /// <summary>
    /// جلب بيانات فرع محدد
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);
        if (branch == null) return ApiResponse<object>.Fail("الفرع غير موجود").ToActionResult();
        return ApiResponse<BranchDto>.Success(branch).ToActionResult();
    }

    /// <summary>
    /// إضافة فرع جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
    {
        var result = await _branchService.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// تحديث بيانات فرع موجود
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto)
    {
        var result = await _branchService.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف فرع موجود
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _branchService.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// تفعيل/تعطيل فرع محدد
    /// </summary>
    [HttpPatch("{id:int}/toggle-active")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _branchService.ToggleActiveAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}


