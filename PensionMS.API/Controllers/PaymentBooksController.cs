using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.PaymentBooks;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/payment-books")]
[Authorize]
public class PaymentBooksController : ControllerBase
{
    private readonly IPaymentBookService _paymentBookService;

    public PaymentBooksController(IPaymentBookService paymentBookService)
    {
        _paymentBookService = paymentBookService;
    }

    /// <summary>
    /// جميع دفاتر الصرف لملف معين
    /// </summary>
    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "PaymentBook.View")]
    public async Task<IActionResult> GetByFileId(int fileId)
    {
        var result = await _paymentBookService.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    /// <summary>
    /// دفتر صرف بالمعرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "PaymentBook.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _paymentBookService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// الدفتر الفعال لملف معين
    /// </summary>
    [HttpGet("active/{fileId:int}")]
    [Authorize(Policy = "PaymentBook.View")]
    public async Task<IActionResult> GetActive(int fileId)
    {
        var result = await _paymentBookService.GetActiveByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    /// <summary>
    /// التحقق من وجود رقم الدفتر
    /// </summary>
    [HttpGet("check-number")]
    [Authorize(Policy = "PaymentBook.View")]
    public async Task<IActionResult> CheckBookNumber([FromQuery] string bookNumber)
    {
        var result = await _paymentBookService.CheckBookNumberExistsAsync(bookNumber);
        return result.ToActionResult();
    }

    /// <summary>
    /// إصدار دفتر صرف جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "PaymentBook.Add")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreatePaymentBookDto dto)
    {
        var userId = this.GetUserId();
        var result = await _paymentBookService.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    /// <summary>
    /// تعديل دفتر صرف
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "PaymentBook.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentBookDto dto)
    {
        var userId = this.GetUserId();
        var result = await _paymentBookService.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف دفتر صرف
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "PaymentBook.Manage")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _paymentBookService.DeleteAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// مراجعة دفاتر الصرف (للقائمة العام)
    /// </summary>
    [HttpGet("review")]
    [Authorize(Policy = "PaymentBook.View")]
    public async Task<IActionResult> GetReview([FromQuery] PaymentBookReviewQueryDto query)
    {
        var result = await _paymentBookService.GetReviewAsync(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// إحصائيات دفاتر الصرف
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Policy = "PaymentBook.View")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _paymentBookService.GetStatsAsync();
        return result.ToActionResult();
    }
}


