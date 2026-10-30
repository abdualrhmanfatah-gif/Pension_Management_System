using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/entitled-persons")]
[Authorize]
public class EntitledPersonsController : ControllerBase
{
    private readonly IEntitledPersonService _service;

    public EntitledPersonsController(IEntitledPersonService service)
    {
        _service = service;
    }

    // ─── READ ─────────────────────────────────────────────────────────

    /// <summary>بحث شامل عن المستحقين</summary>
    [HttpGet("search")]
    [Authorize(Policy = "EntitledPerson.View")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
    {
        var result = await _service.SearchAsync(q, limit);
        return result.ToActionResult();
    }

    /// <summary>تفاصيل مستحق بالمعرف</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "EntitledPerson.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>مستحقون حسب ملف التقاعد</summary>
    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "EntitledPerson.View")]
    public async Task<IActionResult> GetByFileId(int fileId)
    {
        var result = await _service.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    // ─── CREATE ───────────────────────────────────────────────────────

    /// <summary>إضافة مستحق جديد</summary>
    [HttpPost]
    [Authorize(Policy = "EntitledPerson.Create")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateEntitledPersonDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(result);
        return result.ToActionResult();
    }

    // ─── UPDATE ───────────────────────────────────────────────────────

    /// <summary>تحديث بيانات مستحق</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "EntitledPerson.Edit")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateEntitledPersonDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    // ─── LIFECYCLE ────────────────────────────────────────────────────

    /// <summary>
    /// إيقاف الاستحقاق — StopCode: AGE_LIMIT | DAUGHTER_MARRIED | WIDOW_REMARRIED | DECEASED | ADMIN_DECISION
    /// يقبل multipart/form-data لرفع الوثيقة المطلوبة
    /// </summary>
    [HttpPut("{id:int}/stop")]
    [Authorize(Policy = "EntitledPerson.Lifecycle")]
<<<<<<< HEAD
    [Consumes("multipart/form-data")]
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public async Task<IActionResult> Stop(int id, [FromForm] StopEntitledPersonDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.StopAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// إعادة تفعيل الاستحقاق — ReactivationCode: DIVORCED | HUSBAND_DECEASED | DISABILITY_REPORT | ADMIN_DECISION
    /// يقبل multipart/form-data لرفع الوثائق المطلوبة
    /// </summary>
    [HttpPut("{id:int}/reactivate")]
    [Authorize(Policy = "EntitledPerson.Lifecycle")]
<<<<<<< HEAD
    [Consumes("multipart/form-data")]
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public async Task<IActionResult> Reactivate(int id, [FromForm] ReactivateEntitledPersonDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.ReactivateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// تسجيل وفاة المستحق — يُوقف الصرف تلقائياً
    /// يقبل multipart/form-data لرفع شهادة الوفاة
    /// </summary>
    [HttpPost("{id:int}/death")]
    [Authorize(Policy = "EntitledPerson.Lifecycle")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RecordDeath(int id, [FromForm] RecordDeathDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.RecordDeathAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>تفعيل المستحق يدوياً مع وجود نواقص</summary>
    [HttpPost("{id:int}/manual-activate")]
    [Authorize(Policy = "EntitledPerson.Lifecycle")]
    public async Task<IActionResult> ManualActivate(int id, [FromBody] ManualActivateEntitledPersonDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.ManualActivateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    // ─── DELETE ───────────────────────────────────────────────────────

    /// <summary>حذف مستحق (ناعم)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "EntitledPerson.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteAsync(id, userId);
        return result.ToActionResult();
    }
}


