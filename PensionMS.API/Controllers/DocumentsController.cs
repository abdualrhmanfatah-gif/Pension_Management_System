using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionMS.API.Extensions;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// جلب جميع الوثائق مع فلاتر
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? ownerType,
        [FromQuery] int? documentTypeId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] bool? verified,
        [FromQuery] string? searchTerm,
        [FromQuery] int? branchId,
        [FromQuery] int? reviewStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _documentService.GetAllAsync(ownerType, documentTypeId, dateFrom, dateTo, verified, searchTerm, branchId, reviewStatus, page, pageSize);
        return result.ToActionResult();
    }

    /// <summary>
    /// تفاصيل وثيقة بالمعرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _documentService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// إحصائيات الأرشيف (بما فيها الملفات ذات الوثائق الناقصة)
    /// </summary>
    [HttpGet("archive-stats")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetArchiveStats()
    {
        var result = await _documentService.GetArchiveStatsAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// وثائق حسب ملف التقاعد
    /// </summary>
    [HttpGet("by-file/{fileId:int}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetByFileId(int fileId)
    {
        var result = await _documentService.GetByFileIdAsync(fileId);
        return result.ToActionResult();
    }

    /// <summary>
    /// وثائق حسب المتقاعد
    /// </summary>
    [HttpGet("by-retiree/{retireeId:int}")]
    [Authorize(Policy = "Retiree.View")]
    public async Task<IActionResult> GetByRetiree(int retireeId)
    {
        var result = await _documentService.GetByRetireeIdAsync(retireeId);
        return result.ToActionResult();
    }

    [HttpGet("by-investment-portfolio/{portfolioId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByInvestmentPortfolio(int portfolioId) =>
        (await _documentService.GetByInvestmentPortfolioIdAsync(portfolioId)).ToActionResult();

    [HttpGet("by-investment-project/{projectId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByInvestmentProject(int projectId) =>
        (await _documentService.GetByInvestmentProjectIdAsync(projectId)).ToActionResult();

    [HttpGet("by-investment-property/{propertyId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByInvestmentProperty(int propertyId) =>
        (await _documentService.GetByInvestmentPropertyIdAsync(propertyId)).ToActionResult();

    [HttpGet("by-property-unit/{unitId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByPropertyUnit(int unitId) =>
        (await _documentService.GetByPropertyUnitIdAsync(unitId)).ToActionResult();

    [HttpGet("by-lease-contract/{contractId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByLeaseContract(int contractId) =>
        (await _documentService.GetByLeaseContractIdAsync(contractId)).ToActionResult();

    [HttpGet("by-tenant/{tenantId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByTenant(int tenantId) =>
        (await _documentService.GetByTenantIdAsync(tenantId)).ToActionResult();

    [HttpGet("by-property-valuation/{valuationId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByPropertyValuation(int valuationId) =>
        (await _documentService.GetByAssetValuationIdAsync(valuationId)).ToActionResult();

    [HttpGet("by-maintenance-ticket/{requestId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByMaintenanceTicket(int requestId) =>
        (await _documentService.GetByMaintenanceRequestIdAsync(requestId)).ToActionResult();

    [HttpGet("by-work-order/{workOrderId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByWorkOrder(int workOrderId) =>
        (await _documentService.GetByWorkOrderIdAsync(workOrderId)).ToActionResult();

    [HttpGet("by-depreciation-record/{recordId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByDepreciationRecord(int recordId) =>
        (await _documentService.GetByDepreciationRecordIdAsync(recordId)).ToActionResult();

    [HttpGet("by-lease-payment/{scheduleId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByLeasePayment(int scheduleId) =>
        (await _documentService.GetByPaymentScheduleIdAsync(scheduleId)).ToActionResult();

    [HttpGet("by-operational-expense/{expenseId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByOperationalExpense(int expenseId) =>
        (await _documentService.GetByOperationalExpenseIdAsync(expenseId)).ToActionResult();

    [HttpGet("by-investment-plan/{planId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByInvestmentPlan(int planId) =>
        (await _documentService.GetByInvestmentPlanIdAsync(planId)).ToActionResult();

    [HttpGet("by-risk-assessment/{riskId:int}")]
    [Authorize(Policy = "Investment.View")]
    public async Task<IActionResult> GetByRiskAssessment(int riskId) =>
        (await _documentService.GetByRiskAssessmentIdAsync(riskId)).ToActionResult();

    [HttpGet("by-employee/{employeeId:int}")]
    [Authorize(Policy = "HR.Employee.View")]
    public async Task<IActionResult> GetByEmployee(int employeeId) =>
        (await _documentService.GetByEmployeeIdAsync(employeeId)).ToActionResult();

    [HttpGet("by-correspondence/{correspondenceId:int}")]
    [Authorize(Policy = "Correspondence.View")]
    public async Task<IActionResult> GetByCorrespondence(int correspondenceId) =>
        (await _documentService.GetByCorrespondenceIdAsync(correspondenceId)).ToActionResult();

    /// <summary>
    /// رفع وثيقة جديدة
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "File.Documents.Upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentDto dto,
        [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        var userId = this.GetUserId();

        if (string.IsNullOrEmpty(requestId))
            requestId = Guid.NewGuid().ToString("N");

        try
        {
            var result = await _documentService.UploadAsync(dto, userId, requestId);
            return result.ToActionResult();
        }
        catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_DOCUMENT")
        {
            return Conflict(new ApiResponse<object>
            {
                IsSuccess = false,
                Message = "الوثيقة مرفوعة مسبقاً"
            });
        }
    }

    /// <summary>
    /// تعديل بيانات وثيقة
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "File.Documents.Update")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateDocumentDto dto)
    {
        var userId = this.GetUserId();
        var result = await _documentService.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// فحص وثيقة وتحديث حالة الفحص
    /// </summary>
    [HttpPut("{id:int}/review")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewDocumentDto dto)
    {
        var userId = this.GetUserId();
        var result = await _documentService.ReviewAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف وثيقة
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "File.Delete.Soft")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _documentService.DeleteAsync(id, userId);
        return result.ToActionResult();
    }
}


