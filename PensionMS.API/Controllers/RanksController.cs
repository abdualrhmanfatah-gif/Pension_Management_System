using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Ranks;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RanksController : ControllerBase
{
    private readonly IRankService _rankService;

    public RanksController(IRankService rankService)
    {
        _rankService = rankService;
    }

    /// <summary>
    /// جلب قائمة الرتب العسكرية والمعاشات الأساسية المرتبطة بها
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _rankService.GetAllAsync();
        return ApiResponse<List<RankDto>>.Success(result).ToActionResult();
    }

    /// <summary>
    /// جلب بيانات رتبة محددة
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var rank = await _rankService.GetByIdAsync(id);
        if (rank == null) return ApiResponse<object>.Fail("الرتبة غير موجودة").ToActionResult();
        return ApiResponse<RankDto>.Success(rank).ToActionResult();
    }

    /// <summary>
    /// إضافة رتبة عسكرية جديدة
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRankDto dto)
    {
        var result = await _rankService.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// تحديث بيانات رتبة موجودة
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRankDto dto)
    {
        var result = await _rankService.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف رتبة عسكرية
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _rankService.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}


