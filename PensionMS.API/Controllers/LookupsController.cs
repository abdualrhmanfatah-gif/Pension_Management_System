using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.Interfaces;

using PensionMS.API.Extensions;
namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("ranks")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetRanks()
        => Ok(await _lookupService.GetRanksAsync());

    [HttpGet("banks")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetBanks()
        => Ok(await _lookupService.GetBanksAsync());

    [HttpGet("relationships")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetRelationships()
        => Ok(await _lookupService.GetRelationshipsAsync());

    [HttpGet("document-types")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetDocumentTypes()
        => Ok(await _lookupService.GetDocumentTypesAsync());

    [HttpGet("file-statuses")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetFileStatuses()
        => Ok(await _lookupService.GetFileStatusesAsync());

    [HttpGet("social-statuses")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetSocialStatuses()
        => Ok(await _lookupService.GetSocialStatusesAsync());

    [HttpGet("health-statuses")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetHealthStatuses()
        => Ok(await _lookupService.GetHealthStatusesAsync());

    [HttpGet("retiree-categories")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetRetireeCategories()
        => Ok(await _lookupService.GetRetireeCategoriesAsync());

    [HttpGet("branches")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetBranches()
        => Ok(await _lookupService.GetBranchesAsync());

    [HttpGet("roles")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetRoles()
        => Ok(await _lookupService.GetRolesAsync());

    [HttpGet("transaction-statuses")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetTransactionStatuses()
        => Ok(await _lookupService.GetTransactionStatusesAsync());

    [HttpGet("governorates")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetGovernorates()
        => Ok(await _lookupService.GetGovernoratesAsync());

    [HttpGet("units")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetUnits()
        => Ok(await _lookupService.GetUnitsAsync());
}


