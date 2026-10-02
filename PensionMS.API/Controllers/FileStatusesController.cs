using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.Interfaces;

using PensionMS.API.Extensions;
namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/file-statuses")]
[Authorize]
public class FileStatusesController : ControllerBase
{
    private readonly IFileStatusService _statusService;

    public FileStatusesController(IFileStatusService statusService)
    {
        _statusService = statusService;
    }

    [HttpGet]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _statusService.GetFileStatusesAsync();
        return result.ToActionResult();
    }
}


