using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/document-types")]
[Authorize]
public class DocumentTypesController : ControllerBase
{
    private readonly IDocumentTypeService _service;

    public DocumentTypesController(IDocumentTypeService service)
    {
        _service = service;
    }

    /// <summary>
    /// جلب جميع أنواع الوثائق
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "System.Settings.DocumentTypes")]
    public async Task<IActionResult> GetAll([FromQuery] int? ownerType = null)
    {
        var result = await _service.GetAllAsync(ownerType);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب أنواع الوثائق لمالك محدد — متاح لأي مستخدم مصادق عليه
    /// </summary>
    [HttpGet("lookup/by-owner/{ownerType:int}")]
    [Authorize]
    public async Task<IActionResult> LookupByOwnerType(int ownerType)
    {
        var result = await _service.GetByOwnerTypeAsync(ownerType);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب جميع أنواع الوثائق — متاح لأي مستخدم مصادق عليه
    /// </summary>
    [HttpGet("lookup")]
    [Authorize]
    public async Task<IActionResult> LookupAll()
    {
        var result = await _service.GetAllAsync(null);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب أنواع الوثائق لمالك محدد (متقاعد، مستحق، وكيل...)
    /// </summary>
    [HttpGet("by-owner/{ownerType:int}")]
    [Authorize(Policy = "System.Settings.DocumentTypes")]
    public async Task<IActionResult> GetByOwnerType(int ownerType)
    {
        var result = await _service.GetByOwnerTypeAsync(ownerType);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب نوع وثيقة محدد
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "System.Settings.DocumentTypes")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// إضافة نوع وثيقة جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDocumentTypeDto dto)
    {
        var result = await _service.CreateAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// تحديث بيانات نوع وثيقة
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentTypeDto dto)
    {
        var result = await _service.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف نوع وثيقة
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "System.Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}


