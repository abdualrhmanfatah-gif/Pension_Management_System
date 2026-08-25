using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryItemsController : ControllerBase
{
    private readonly IInventoryItemService _itemService;

    public InventoryItemsController(IInventoryItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    [Authorize(Policy = "Inventory.Item.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _itemService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Inventory.Item.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _itemService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Inventory.Item.Create")]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        var result = await _itemService.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Inventory.Item.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemDto dto)
    {
        var result = await _itemService.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Inventory.Item.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _itemService.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}
