using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/properties")]
[Authorize]
public class InvestmentPropertiesController : ControllerBase
{
    private readonly IInvestmentPropertyService _service;
    private readonly IFileStorageService _fileStorage;

    public InvestmentPropertiesController(IInvestmentPropertyService service, IFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
    }

    // ----------------------------------------------------
    // Properties
    // ----------------------------------------------------

    [HttpGet]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _service.GetAllPropertiesAsync(page, pageSize);
        return result.ToActionResult();
    }

    [HttpGet("list")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetList([FromQuery] int? projectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (projectId.HasValue)
        {
            // Note: If GetPropertiesByProjectAsync doesn't support pagination natively, it will return the whole list.
            var result = await _service.GetPropertiesByProjectAsync(projectId.Value);
            return result.ToActionResult();
        }
        else
        {
            var result = await _service.GetAllPropertiesAsync(page, pageSize);
            return result.ToActionResult();
        }
    }

    [HttpGet("project/{projectId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByProject(int projectId)
    {
        var result = await _service.GetPropertiesByProjectAsync(projectId);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetPropertyByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "Investment.Add")]
    public async Task<IActionResult> Create([FromBody] CreatePropertyDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreatePropertyAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Investment.Edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePropertyDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdatePropertyAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Investment.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeletePropertyAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/upload-photo")]
    [Authorize(Policy = "Investment.Manage")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<string>.Fail("لم يتم اختيار ملف").ToActionResult();

<<<<<<< HEAD
        var userId = this.GetUserId();
        var property = await _service.GetPropertyByIdAsync(id);
        if (!property.IsSuccess)
            return ApiResponse<bool>.Fail(property.Message ?? "العقار غير موجود").ToActionResult();

        using var stream = file.OpenReadStream();
        var filePath = await _fileStorage.SaveFileAsync(stream, file.FileName, "properties");

        ApiResponse<bool> result;
        try
        {
            result = await _service.UpdatePropertyMainPhotoPathAsync(id, filePath, userId);
        }
        catch (Exception updateError)
        {
            try { await _fileStorage.DeleteFileAsync(filePath); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("تعذر تحديث صورة العقار وتنظيف الملف الجديد", updateError, cleanupError);
            }
            throw;
        }

        if (!result.IsSuccess) await _fileStorage.DeleteFileAsync(filePath);
=======
        using var stream = file.OpenReadStream();
        var filePath = await _fileStorage.SaveFileAsync(stream, file.FileName, "properties");

        var userId = this.GetUserId();
        var result = await _service.UpdatePropertyMainPhotoPathAsync(id, filePath, userId);
        
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Units
    // ----------------------------------------------------

    [HttpGet("units/all")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAllUnits()
    {
        var result = await _service.GetAllUnitsAsync();
        return result.ToActionResult();
    }

    [HttpGet("{propertyId:int}/units")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetUnits(int propertyId)
    {
        var result = await _service.GetUnitsByPropertyAsync(propertyId);
        return result.ToActionResult();
    }

    [HttpPost("units")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateUnitAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("units/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> UpdateUnit(int id, [FromBody] UpdateUnitDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateUnitAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("units/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteUnitAsync(id, userId);
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Valuations
    // ----------------------------------------------------

    [HttpGet("{propertyId:int}/valuations")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetValuations(int propertyId)
    {
        var result = await _service.GetValuationsByPropertyAsync(propertyId);
        return result.ToActionResult();
    }

    [HttpPost("valuations")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateValuation([FromBody] CreateValuationDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateValuationAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("valuations/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> DeleteValuation(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteValuationAsync(id, userId);
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Depreciation
    // ----------------------------------------------------

    [HttpGet("{propertyId:int}/depreciation")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetDepreciation(int propertyId)
    {
        var result = await _service.GetDepreciationByPropertyAsync(propertyId);
        return result.ToActionResult();
    }

    [HttpPost("depreciation")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateDepreciation([FromBody] CreateDepreciationDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateDepreciationAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("depreciation/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> DeleteDepreciation(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteDepreciationAsync(id, userId);
        return result.ToActionResult();
    }
}


