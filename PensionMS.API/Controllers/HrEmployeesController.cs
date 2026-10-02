using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/hr/HrEmployees")]
[Authorize]
public class HrEmployeesController : ControllerBase
{
    private readonly IHrEmployeeService _service;

    public HrEmployeesController(IHrEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] int? cursor = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, search, cursor);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "HR.Employee.Create")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var result = await _service.CreateAsync(dto, this.GetUserId());
        if (!result.IsSuccess) return result.ToActionResult();
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await _service.UpdateAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "HR.Employee.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/photo")]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("الملف غير صالح");
        
        using var stream = file.OpenReadStream();
        var result = await _service.UploadPhotoAsync(id, stream, file.FileName, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("stats")]
    [Authorize(Policy = "HR.Dashboard.View")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return result.ToActionResult();
    }

    [HttpGet("documents/global")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetGlobalDocuments([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _service.GetGlobalEmployeeDocumentsAsync(page, pageSize, search);
        return result.ToActionResult();
    }
}


