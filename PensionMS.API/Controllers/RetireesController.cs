using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.API.Extensions;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RetireesController : ControllerBase
{
    private readonly IRetireeService _retireeService;
    private readonly IMemoryCache _cache;

    private const string RetireeCachePrefix = "retiree_";
    private static readonly TimeSpan RetireeCacheDuration = TimeSpan.FromMinutes(5);

    public RetireesController(IRetireeService retireeService, IMemoryCache cache)
    {
        _retireeService = retireeService;
        _cache = cache;
    }

    /// <summary>
    /// قائمة المتقاعدين مع البحث والصفحات
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PermissionConstants.RetireeView)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? socialStatusId = null,
        [FromQuery] int? healthStatusId = null,
        [FromQuery] bool? isAlive = null,
        [FromQuery] string? governorate = null,
        [FromQuery] string? city = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? rankId = null,
        [FromQuery] byte? gender = null,
        [FromQuery] bool? hasFiles = null,
        [FromQuery] bool? isProcessed = null,
        [FromQuery] bool? hasSettlements = null,
        [FromQuery] string? unitName = null,
        [FromQuery] DateOnly? retirementDateFrom = null,
        [FromQuery] DateOnly? retirementDateTo = null)
    {
        var filters = new SocialResearchQueryDto
        {
            SocialStatusId = socialStatusId,
            HealthStatusId = healthStatusId,
            IsAlive = isAlive,
            Governorate = governorate,
            City = city,
            CategoryId = categoryId,
            RankId = rankId,
            Gender = gender,
            HasFiles = hasFiles,
            IsProcessed = isProcessed,
            HasSettlements = hasSettlements,
            UnitName = unitName,
            RetirementDateFrom = retirementDateFrom,
            RetirementDateTo = retirementDateTo
        };
        var result = await _retireeService.GetAllAsync(page, pageSize, search, filters);
        return Ok(result);
    }

    /// <summary>
    /// تفاصيل متقاعد بالمعرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionConstants.RetireeView)]
    public async Task<IActionResult> GetById(int id)
    {
        var cacheKey = RetireeCachePrefix + id;
        if (_cache.TryGetValue(cacheKey, out object? cached))
            return Ok(cached);

        var result = await _retireeService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        _cache.Set(cacheKey, result, RetireeCacheDuration);

        return Ok(result);
    }

    /// <summary>
    /// بحث بالهوية الوطنية
    /// </summary>
    [HttpGet("national-id/{nationalId}")]
    [Authorize(Policy = PermissionConstants.RetireeView)]
    public async Task<IActionResult> GetByNationalID(string nationalId)
    {
        var result = await _retireeService.GetByNationalIDAsync(nationalId);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// بحث بالرقم العسكري
    /// </summary>
    [HttpGet("military/{militaryNumber}")]
    [Authorize(Policy = PermissionConstants.RetireeView)]
    public async Task<IActionResult> GetByMilitaryNumber(string militaryNumber)
    {
        var result = await _retireeService.GetByMilitaryNumberAsync(militaryNumber);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// المباينة — جمع كل بيانات المتقاعد في تقرير شامل
    /// </summary>
    [HttpGet("{id:int}/statement")]
    [Authorize(Policy = PermissionConstants.RetireeView)]
    public async Task<IActionResult> GetStatement(int id)
    {
        var result = await _retireeService.GetStatementAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// تحديث بيانات متقاعد
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionConstants.RetireeEdit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRetireeDto dto)
    {
        var result = await _retireeService.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف متقاعد (ناعم)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionConstants.RetireeDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _retireeService.DeleteAsync(id, userId);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// جلب البيانات الكاملة للمتقاعد (للملء التلقائي عند إنشاء ملف)
    /// </summary>
    [HttpGet("{id:int}/full-data")]
    [Authorize(Policy = PermissionConstants.RetireeView)]
    public async Task<IActionResult> GetFullData(int id)
    {
        var result = await _retireeService.GetFullDataAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// رفع صورة شخصية للمتقاعد
    /// </summary>
    [HttpPost("{id:int}/photo")]
    [Authorize(Policy = PermissionConstants.RetireeEdit)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(int id, IRemoteFile photo)
    {
        var result = await _retireeService.UploadPhotoAsync(id, photo);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
