using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Interfaces;

using PensionMS.API.Extensions;
namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/lookups")]
[Authorize]
public class InvestmentLookupsController : ControllerBase
{
    private readonly IInvestmentLookupService _service;

    public InvestmentLookupsController(IInvestmentLookupService service)
    {
        _service = service;
    }

    [HttpGet("investment-types")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetInvestmentTypes()
    {
        var result = await _service.GetInvestmentTypesAsync();
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Property Types
    // ----------------------------------------------------

    [HttpGet("property-types")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetPropertyTypes()
    {
        var result = await _service.GetPropertyTypesAsync();
        return result.ToActionResult();
    }

    // ----------------------------------------------------
    // Unit Types
    // ----------------------------------------------------

    [HttpGet("unit-types")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetUnitTypes()
    {
        var result = await _service.GetUnitTypesAsync();
        return result.ToActionResult();
    }
}


