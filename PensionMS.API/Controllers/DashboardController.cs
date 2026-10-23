using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

using PensionMS.API.Extensions;
namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService service, ILogger<DashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// إحصائيات لوحة التحكم
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Policy = "Dashboard.View")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// إحصائيات شاملة للمعاشات
    /// </summary>
    [HttpGet("pension-statistics")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetPensionStatistics()
    {
        return (await _service.GetPensionStatisticsAsync()).ToActionResult();
    }

    /// <summary>
    /// إحصائيات الأداء الشهري
    /// </summary>
    [HttpGet("performance-statistics")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetPerformanceStatistics()
    {
        return (await _service.GetPerformanceStatisticsAsync()).ToActionResult();
    }

    /// <summary>
    /// آخر عمليات النظام
    /// </summary>
    [HttpGet("recent-activities")]
    [Authorize(Policy = "Dashboard.View")]
    public async Task<IActionResult> GetRecentActivities()
    {
        return (await _service.GetRecentActivitiesAsync()).ToActionResult();
    }
}


