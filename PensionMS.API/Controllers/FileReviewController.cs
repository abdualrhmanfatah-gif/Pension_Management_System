using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/filereview")]
[Authorize]
public class FileReviewController : ControllerBase
{
    private readonly IFileReviewService _reviewService;

    public FileReviewController(IFileReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost("entities/{entityType}/{entityId:int}/acknowledge-rejection")]
    [Authorize(Policy = "File.Edit.BasicData")]
    public async Task<IActionResult> AcknowledgeRejection(string entityType, int entityId)
    {
        var userId = this.GetUserId();
        var result = await _reviewService.AcknowledgeEntityRejectionAsync(entityType, entityId, userId);
        return result.ToActionResult();
    }
}
