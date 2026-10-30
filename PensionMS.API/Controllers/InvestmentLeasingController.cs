using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/leasing")]
[Authorize]
public class InvestmentLeasingController : ControllerBase
{
    private readonly IInvestmentLeasingService _service;
    private readonly IFileStorageService _fileStorage;

    public InvestmentLeasingController(IInvestmentLeasingService service, IFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
    }

    // ----------------------------------------------------
    // Tenants
    // ----------------------------------------------------

    [HttpGet("tenants")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAllTenants()
    {
        var result = await _service.GetAllTenantsAsync();
        return result.ToActionResult();
    }

    [HttpGet("tenants/{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetTenantById(int id)
    {
        var result = await _service.GetTenantByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("tenants/{tenantId:int}/properties")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetPropertiesByTenant(int tenantId)
    {
        var result = await _service.GetPropertiesByTenantAsync(tenantId);
        return result.ToActionResult();
    }

    [HttpPost("tenants")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateTenantAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("tenants/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> UpdateTenant(int id, [FromBody] UpdateTenantDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateTenantAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("tenants/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> DeleteTenant(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteTenantAsync(id, userId);
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Lease Contracts
    // ----------------------------------------------------

    [HttpGet("properties/{propertyId:int}/contracts")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetContractsByProperty(int propertyId)
    {
        var result = await _service.GetLeasesByPropertyAsync(propertyId);
        return result.ToActionResult();
    }

    [HttpGet("contracts")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAllContracts()
    {
        var result = await _service.GetAllLeasesAsync();
        return result.ToActionResult();
    }

    [HttpGet("contracts/{id:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetContractById(int id)
    {
        var result = await _service.GetLeaseContractByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("contracts")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateContract([FromBody] CreateLeaseDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreateLeaseContractAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("contracts/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> UpdateContract(int id, [FromBody] UpdateLeaseDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdateLeaseContractAsync(id, dto, userId);
        return result.ToActionResult();
    }

    [HttpDelete("contracts/{id:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var userId = this.GetUserId();
        var result = await _service.DeleteLeaseContractAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpPost("contracts/{id:int}/upload-signed-contract")]
    [Authorize(Policy = "Investment.Manage")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadSignedContract(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<string>.Fail("لم يتم اختيار ملف").ToActionResult();

<<<<<<< HEAD
        var userId = this.GetUserId();
        var contract = await _service.GetLeaseContractByIdAsync(id);
        if (!contract.IsSuccess)
            return ApiResponse<bool>.Fail(contract.Message ?? "العقد غير موجود").ToActionResult();

        using var stream = file.OpenReadStream();
        var filePath = await _fileStorage.SaveFileAsync(stream, file.FileName, "contracts");

        ApiResponse<bool> result;
        try
        {
            result = await _service.UpdateLeaseContractFilePathAsync(id, filePath, userId);
        }
        catch (Exception updateError)
        {
            try { await _fileStorage.DeleteFileAsync(filePath); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("تعذر تحديث العقد الموقع وتنظيف الملف الجديد", updateError, cleanupError);
            }
            throw;
        }

        if (!result.IsSuccess) await _fileStorage.DeleteFileAsync(filePath);
=======
        using var stream = file.OpenReadStream();
        var filePath = await _fileStorage.SaveFileAsync(stream, file.FileName, "contracts");
        
        var userId = this.GetUserId();
        var result = await _service.UpdateLeaseContractFilePathAsync(id, filePath, userId);
        
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Payment Schedules
    // ----------------------------------------------------

    [HttpGet("contracts/{contractId:int}/schedules")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetSchedules(int contractId)
    {
        var result = await _service.GetSchedulesByContractAsync(contractId);
        return result.ToActionResult();
    }

    [HttpGet("schedules")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetAllSchedules()
    {
        var result = await _service.GetAllSchedulesAsync();
        return result.ToActionResult();
    }

    [HttpPost("schedules")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreatePaymentScheduleDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.CreatePaymentScheduleAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpPut("schedules/{id:int}/status")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> UpdateScheduleStatus(int id, [FromBody] UpdatePaymentStatusDto dto)
    {
        var userId = this.GetUserId();
        var result = await _service.UpdatePaymentStatusAsync(id, dto, userId);
        return result.ToActionResult();
    }
}


