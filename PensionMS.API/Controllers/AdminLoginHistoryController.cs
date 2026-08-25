using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.Core.Interfaces;

using PensionMS.API.Extensions;
namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/admin/login-history")]
[Authorize]
public class AdminLoginHistoryController : ControllerBase
{
    private readonly ILoginHistoryService _service;

    public AdminLoginHistoryController(ILoginHistoryService service)
    {
        _service = service;
    }

    /// <summary>
    /// جلب سجلات تسجيل الدخول مع فلترة وترقيم
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? userId = null,
        [FromQuery] bool? isSuccessful = null,
        [FromQuery] string? ipAddress = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, userId, isSuccessful, ipAddress, from, to, search);
        return result.ToActionResult();
    }

    /// <summary>
    /// جلب محاولات الدخول لعنوان IP محدد
    /// </summary>
    [HttpGet("by-ip/{ip}")]
    [Authorize(Policy = "Audit.ViewAll")]
    public async Task<IActionResult> GetByIp(string ip)
    {
        var result = await _service.GetByIpAsync(ip);
        return result.ToActionResult();
    }
}


