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
public class InventoryTransactionsController : ControllerBase
{
    private readonly IInventoryTransactionService _transactionService;

    public InventoryTransactionsController(IInventoryTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    [Authorize(Policy = "Inventory.Transaction.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _transactionService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("by-item/{itemId:int}")]
    [Authorize(Policy = "Inventory.Transaction.View")]
    public async Task<IActionResult> GetByItem(int itemId)
    {
        var result = await _transactionService.GetByItemAsync(itemId);
        return result.ToActionResult();
    }

    [HttpGet("by-withdrawal-order/{orderNumber}")]
    [Authorize(Policy = "Inventory.Transaction.View")]
    public async Task<IActionResult> GetByWithdrawalOrder(string orderNumber)
    {
        var result = await _transactionService.GetByWithdrawalOrderAsync(orderNumber);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Inventory.Transaction.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _transactionService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Inventory.Transaction.Create")]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var result = await _transactionService.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("bulk")]
    [Authorize(Policy = "Inventory.Transaction.Create")]
    public async Task<IActionResult> CreateBulk([FromBody] CreateBulkTransactionDto dto)
    {
        var result = await _transactionService.CreateBulkAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }
}
