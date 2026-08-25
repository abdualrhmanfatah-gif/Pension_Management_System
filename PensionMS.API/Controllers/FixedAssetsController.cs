using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Assets;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces.Services;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FixedAssetsController : ControllerBase
{
    private readonly IFixedAssetService _assetService;

    public FixedAssetsController(IFixedAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    [Authorize(Policy = "Inventory.Item.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _assetService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Inventory.Item.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _assetService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-type/{typeId:int}")]
    [Authorize(Policy = "Inventory.Item.View")]
    public async Task<IActionResult> GetByType(int typeId)
    {
        var result = await _assetService.GetByTypeAsync(typeId);
        return result.ToActionResult();
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _assetService.GetTypesAsync();
        return result.ToActionResult();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] int? typeId)
    {
        var result = await _assetService.GetCategoriesAsync(typeId);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Inventory.Item.Create")]
    public async Task<IActionResult> Create([FromBody] CreateFixedAssetDto dto)
    {
        var result = await _assetService.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Inventory.Item.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFixedAssetDto dto)
    {
        var result = await _assetService.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Inventory.Item.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _assetService.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("{id:int}/schedule")]
    [Authorize(Policy = "Inventory.Item.View")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var result = await _assetService.GetScheduleAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("depreciation/yearly/run")]
    [Authorize(Policy = "Inventory.Item.Create")]
    public async Task<IActionResult> RunYearlyDepreciation([FromQuery] int year)
    {
        var result = await _assetService.RunYearlyDepreciationAsync(year, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("depreciation/yearly/post")]
    [Authorize(Policy = "Inventory.Item.Create")]
    public async Task<IActionResult> PostYearlyDepreciation([FromQuery] int year)
    {
        var result = await _assetService.PostYearlyDepreciationAsync(year, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("categories/reset")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> ResetCategories()
    {
        var result = await _assetService.ResetCategoriesAsync();
        return result.ToActionResult();
    }

    [HttpPost("categories")]
    [Authorize(Policy = "Inventory.Item.Create")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateAssetCategoryDto dto)
    {
        var result = await _assetService.CreateCategoryAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPut("categories/{id:int}")]
    [Authorize(Policy = "Inventory.Item.Edit")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateAssetCategoryDto dto)
    {
        var result = await _assetService.UpdateCategoryAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("categories/{id:int}")]
    [Authorize(Policy = "Inventory.Item.Delete")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _assetService.DeleteCategoryAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPatch("categories/{id:int}/toggle-active")]
    [Authorize(Policy = "Inventory.Item.Edit")]
    public async Task<IActionResult> ToggleCategoryActive(int id)
    {
        var result = await _assetService.ToggleCategoryActiveAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}
