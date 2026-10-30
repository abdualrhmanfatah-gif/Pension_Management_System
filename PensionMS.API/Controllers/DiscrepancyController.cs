using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscrepancyController : ControllerBase
{
    private readonly IDiscrepancyService _service;

    public DiscrepancyController(IDiscrepancyService service)
    {
        _service = service;
    }

    [HttpGet("report")]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetReport([FromQuery] DiscrepancyFilterDto filter)
    {
        var result = await _service.GetDiscrepancyReportAsync(filter);
        return Ok(result);
    }

    [HttpGet("kashf-report")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetKashfReport([FromQuery] DiscrepancyFilterDto filter)
    {
        var result = await _service.GetDiscrepancyReportAsync(filter);
        return Ok(result);
    }

    [HttpGet("kashf-report/summary")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> GetKashfReportSummary([FromQuery] DiscrepancyFilterDto filter)
    {
        var result = await _service.GetKashfReportSummaryAsync(filter);
        return Ok(result);
    }

    [HttpGet("kashf-report/export")]
    [Authorize(Policy = "Ministry.View")]
    public async Task<IActionResult> ExportKashfReport([FromQuery] DiscrepancyFilterDto filter, [FromQuery] string format = "excel")
    {
        var bytes = await _service.ExportKashfReportAsync(filter, format);
        if (bytes == null || bytes.Length == 0) return NotFound();

<<<<<<< HEAD
        var fileName = $"Kashf_Report_{PensionMS.Core.Time.YemenTime.Now:yyyyMMdd}";
=======
        var fileName = $"Kashf_Report_{DateTime.Now:yyyyMMdd}";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (format.ToLower() == ExportFormatConstants.Pdf)
        {
            return File(bytes, "application/pdf", $"{fileName}.pdf");
        }

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }
}
