using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/hr/HrReferenceData")]
[Authorize]
public class HrReferenceDataController : ControllerBase
{
    private readonly IHrReferenceDataService _service;

    public HrReferenceDataController(IHrReferenceDataService service)
    {
        _service = service;
    }

    [HttpGet("departments")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetDepartments()
    {
        var result = await _service.GetDepartmentsAsync();
        return result.ToActionResult();
    }

    [HttpGet("departments/{id:int}")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetDepartmentById(int id)
    {
        var result = await _service.GetDepartmentByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("departments")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        var result = await _service.CreateDepartmentAsync(dto, this.GetUserId());
        if (!result.IsSuccess) return result.ToActionResult();
        return result.ToActionResult();
    }

    [HttpPut("departments/{id:int}")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var result = await _service.UpdateDepartmentAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("departments/{id:int}")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var result = await _service.DeleteDepartmentAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("sections")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetSections([FromQuery] int? departmentId = null)
    {
        var result = await _service.GetSectionsAsync(departmentId);
        return result.ToActionResult();
    }

    [HttpGet("sections/{id:int}")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetSectionById(int id)
    {
        var result = await _service.GetSectionByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("sections")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto dto)
    {
        var result = await _service.CreateSectionAsync(dto, this.GetUserId());
        if (!result.IsSuccess) return result.ToActionResult();
        return result.ToActionResult();
    }

    [HttpPut("sections/{id:int}")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateSectionDto dto)
    {
        var result = await _service.UpdateSectionAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("sections/{id:int}")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var result = await _service.DeleteSectionAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("job-titles")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetJobTitles()
    {
        var result = await _service.GetJobTitlesAsync();
        return result.ToActionResult();
    }

    [HttpGet("job-titles/{id:int}")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetJobTitleById(int id)
    {
        var result = await _service.GetJobTitleByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("job-titles")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> CreateJobTitle([FromBody] CreateJobTitleDto dto)
    {
        var result = await _service.CreateJobTitleAsync(dto, this.GetUserId());
        if (!result.IsSuccess) return result.ToActionResult();
        return result.ToActionResult();
    }

    [HttpPut("job-titles/{id:int}")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> UpdateJobTitle(int id, [FromBody] UpdateJobTitleDto dto)
    {
        var result = await _service.UpdateJobTitleAsync(id, dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("job-titles/{id:int}")]
    [Authorize(Policy = "HR.Reference.Manage")]
    public async Task<IActionResult> DeleteJobTitle(int id)
    {
        var result = await _service.DeleteJobTitleAsync(id, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpGet("employment-statuses")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetEmploymentStatuses()
    {
        var result = await _service.GetEmploymentStatusesAsync();
        return result.ToActionResult();
    }

    [HttpGet("leave-types")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var result = await _service.GetLeaveTypesAsync();
        return result.ToActionResult();
    }

    [HttpGet("ranks")]
    [Authorize(Policy = "HR.Reference.View")]
    public async Task<IActionResult> GetRanks()
    {
        var result = await _service.GetRanksAsync();
        return result.ToActionResult();
    }
}


