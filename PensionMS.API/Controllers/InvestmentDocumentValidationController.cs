using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

using PensionMS.API.Extensions;
namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/investment/documents")]
[Authorize]
public class InvestmentDocumentValidationController : ControllerBase
{
    private readonly IInvestmentDocumentValidationService _validationService;

    public InvestmentDocumentValidationController(IInvestmentDocumentValidationService validationService)
    {
        _validationService = validationService;
    }

    /// <summary>
    /// استرجاع حالة جاهزية الوثائق لكيان استثماري (مع نسبة الإنجاز)
    /// </summary>
    [HttpGet("readiness/{ownerType:int}/{entityId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetReadiness(int ownerType, int entityId)
    {
        if (!Enum.IsDefined(typeof(DocumentOwnerType), ownerType))
            return ApiResponse<object>.Fail("نوع المالك غير صالح").ToActionResult();

        var result = await _validationService.GetDocumentReadinessAsync((DocumentOwnerType)ownerType, entityId);
        return result.ToActionResult();
    }

    /// <summary>
    /// التحقق من جاهزية الوثائق لتغيير حالة الكيان (إلزامي قبل التفعيل)
    /// </summary>
    [HttpGet("validate-status/{ownerType:int}/{entityId:int}")]
    [Authorize(Policy = "Investment.Manage")]
    public async Task<IActionResult> ValidateForStatusChange(int ownerType, int entityId)
    {
        if (!Enum.IsDefined(typeof(DocumentOwnerType), ownerType))
            return ApiResponse<object>.Fail("نوع المالك غير صالح").ToActionResult();

        var result = await _validationService.ValidateReadinessForStatusChangeAsync((DocumentOwnerType)ownerType, entityId);
        return result.ToActionResult();
    }
}


