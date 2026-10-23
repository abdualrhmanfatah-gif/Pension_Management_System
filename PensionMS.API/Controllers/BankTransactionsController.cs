using PensionMS.Core.Interfaces.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Infrastructure.Services.Accounting;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankTransactionsController : ControllerBase
{
    private readonly IBankTransactionService _bankTransactionService;

    public BankTransactionsController(IBankTransactionService bankTransactionService)
    {
        _bankTransactionService = bankTransactionService;
    }

    /// <summary>
    /// جلب معاملات بنك محدد
    /// </summary>
    [HttpGet("bank/{bankId:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetByBank(int bankId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _bankTransactionService.GetByBankAsync(bankId, fromDate, toDate);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب معاملات حساب بنكي
    /// </summary>
    [HttpGet("account/{accountId:int}")]
    [Authorize(Policy = "Accounting.View")]
    public async Task<IActionResult> GetByAccount(int accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _bankTransactionService.GetByAccountAsync(accountId, fromDate, toDate);
        return result.ToActionResult();
    }

    /// <summary>
    /// تسجيل معاملة بنكية جديدة
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Accounting.Manage")]
    public async Task<IActionResult> Create([FromBody] CreateBankTransactionDto dto)
    {
        var result = await _bankTransactionService.CreateAsync(dto);
        return result.ToActionResult();
    }
}
