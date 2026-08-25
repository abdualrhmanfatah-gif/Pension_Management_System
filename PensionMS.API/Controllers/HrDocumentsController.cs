using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/hr/HrDocuments")]
[Authorize]
public class HrDocumentsController : ControllerBase
{
    private readonly IHrDocumentService _service;

    public HrDocumentsController(IHrDocumentService service)
    {
        _service = service;
    }

    [HttpGet("{employeeId:int}")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId)
    {
        var result = await _service.GetEmployeeDocumentsAsync(employeeId);
        return result.ToActionResult();
    }

    [HttpPost("upload")]
    [Authorize(Policy = "HR.Employee.Edit")]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
    {
        var result = await _service.UploadEmployeeDocumentAsync(dto, this.GetUserId());
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "HR.Employee.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteDocumentAsync(id, this.GetUserId());
        return result.ToActionResult();
    }
}


