using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using PensionMS.Core.Interfaces.Accounting;
=======
using Microsoft.EntityFrameworkCore;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Assets;
using PensionMS.Core.DTOs.Inventory;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Assets;
using PensionMS.Core.Interfaces.Inventory;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StocktakeController : ControllerBase
{
    private readonly IAssetStocktakeService _assetStocktakeService;
    private readonly ISimpleStocktakeService _simpleStocktakeService;
<<<<<<< HEAD
    private readonly IFiscalPeriodService _fiscalService;
=======
    private readonly IUnitOfWork _unitOfWork;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    private readonly ILogger<StocktakeController> _logger;

    public StocktakeController(
        IAssetStocktakeService assetStocktakeService,
        ISimpleStocktakeService simpleStocktakeService,
<<<<<<< HEAD
        IFiscalPeriodService fiscalService,
=======
        IUnitOfWork unitOfWork,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        ILogger<StocktakeController> logger)
    {
        _assetStocktakeService = assetStocktakeService;
        _simpleStocktakeService = simpleStocktakeService;
<<<<<<< HEAD
        _fiscalService = fiscalService;
=======
        _unitOfWork = unitOfWork;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        _logger = logger;
    }

    // ═══════════════════════════════════════
    // السنة المالية
    // ═══════════════════════════════════════

    [HttpGet("fiscal-years")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetFiscalYears()
    {
<<<<<<< HEAD
        var result = await _fiscalService.GetAllFiscalYearsAsync();
        if (!result.IsSuccess) return result.ToActionResult();
        var years = result.Data!.OrderByDescending(y => y.StartDate)
            .Select(y => new { y.FiscalYearID, y.YearName, y.StartDate, y.EndDate, y.IsClosed });
=======
        var years = await _unitOfWork.FiscalYears.Query()
            .Where(y => !y.IsDeleted)
            .OrderByDescending(y => y.StartDate)
            .Select(y => new { y.FiscalYearID, y.YearName, y.StartDate, y.EndDate, y.IsClosed })
            .ToListAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return Ok(years);
    }

    // ═══════════════════════════════════════
    // جرد الأصول الثابتة
    // ═══════════════════════════════════════

    [HttpPost("asset-stocktake")]
    [Authorize(Policy = "Inventory.Stocktake")]
    public async Task<IActionResult> CreateAssetStocktake([FromBody] CreateAssetStocktakeDto dto)
    {
        var result = await _assetStocktakeService.CreateStocktakeAsync(dto.FiscalYearID, this.GetUserId());
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("asset-stocktake/current")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetAssetStocktake()
    {
        var result = await _assetStocktakeService.GetLatestStocktakeAsync();
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("asset-stocktake/year/{fiscalYearId}")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetAssetStocktakeByFiscalYear(int fiscalYearId)
    {
        var result = await _assetStocktakeService.GetStocktakeByFiscalYearAsync(fiscalYearId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("asset-stocktake/{stocktakeId}/details")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetAssetStocktakeDetails(int stocktakeId)
    {
        var result = await _assetStocktakeService.GetStocktakeAsync(stocktakeId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("asset-stocktake/all/{fiscalYearId}")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetAllAssetStocktakes(int fiscalYearId)
    {
        var result = await _assetStocktakeService.GetAllStocktakesAsync(fiscalYearId);
        return Ok(result);
    }

    [HttpPost("asset-stocktake/{stocktakeId}/record")]
    [Authorize(Policy = "Inventory.Stocktake")]
    public async Task<IActionResult> RecordAssetStocktake(int stocktakeId, [FromBody] RecordAssetStocktakeDto dto)
    {
        var result = await _assetStocktakeService.RecordActualCountAsync(stocktakeId, dto.Lines, this.GetUserId());
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("asset-stocktake/{stocktakeId}/complete")]
    [Authorize(Policy = "Inventory.Stocktake")]
    public async Task<IActionResult> CompleteAssetStocktake(int stocktakeId)
    {
        var result = await _assetStocktakeService.CompleteStocktakeAsync(stocktakeId, this.GetUserId());
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // ═══════════════════════════════════════
    // جرد المخزون البسيط
    // ═══════════════════════════════════════

    [HttpPost("simple-stocktake")]
    [Authorize(Policy = "Inventory.Stocktake")]
    public async Task<IActionResult> CreateSimpleStocktake()
    {
        var result = await _simpleStocktakeService.CreateStocktakeAsync(this.GetUserId());
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("simple-stocktake/current")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetSimpleStocktake()
    {
        var result = await _simpleStocktakeService.GetLatestStocktakeAsync();
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("simple-stocktake/year/{fiscalYearId}")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetSimpleStocktakeByFiscalYear(int fiscalYearId)
    {
        var result = await _simpleStocktakeService.GetStocktakeByFiscalYearAsync(fiscalYearId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("simple-stocktake/{stocktakeId}/details")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetSimpleStocktakeDetails(int stocktakeId)
    {
        var result = await _simpleStocktakeService.GetStocktakeAsync(stocktakeId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("simple-stocktake/all/{fiscalYearId}")]
    [Authorize(Policy = "Inventory.View")]
    public async Task<IActionResult> GetAllSimpleStocktakes(int fiscalYearId)
    {
        var result = await _simpleStocktakeService.GetAllStocktakesAsync(fiscalYearId);
        return Ok(result);
    }

    [HttpPost("simple-stocktake/{stocktakeId}/record")]
    [Authorize(Policy = "Inventory.Stocktake")]
    public async Task<IActionResult> RecordSimpleStocktake(int stocktakeId, [FromBody] RecordSimpleStocktakeDto dto)
    {
        var result = await _simpleStocktakeService.RecordPhysicalCountAsync(stocktakeId, dto.ActualCounts, this.GetUserId());
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("simple-stocktake/{stocktakeId}/adjust")]
    [Authorize(Policy = "Inventory.Stocktake")]
    public async Task<IActionResult> AdjustSimpleStocktake(int stocktakeId)
    {
        var result = await _simpleStocktakeService.AdjustStocktakeAsync(stocktakeId, this.GetUserId());
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
