using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PensionMS.API.Extensions;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/retirement-files")]
[Authorize]
public class RetirementFilesController : ControllerBase
{
    private readonly IRetirementFileService _fileService;
    private readonly IFileQueryService _queryService;
    private readonly IFileStatusService _statusService;
    private readonly IFileReviewService _reviewService;
    private readonly IDocumentService _documentService;
    private readonly IMemoryCache _cache;

    private const string FileCachePrefix = "retirement_file_";
    private static readonly TimeSpan FileCacheDuration = TimeSpan.FromMinutes(5);

    public RetirementFilesController(
        IRetirementFileService fileService,
        IFileQueryService queryService,
        IFileStatusService statusService,
        IFileReviewService reviewService,
        IDocumentService documentService,
        IMemoryCache cache)
    {
        _fileService = fileService;
        _queryService = queryService;
        _statusService = statusService;
        _reviewService = reviewService;
        _documentService = documentService;
        _cache = cache;
    }

    // ═══════════════════════════════════════
    // QUERY ENDPOINTS (IFileQueryService)
    // ═══════════════════════════════════════

    /// <summary>
    /// قائمة الملفات مع البحث والفلترة والصفحات
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] int? statusId = null, [FromQuery] string? statusIds = null,
        [FromQuery] int? uploadedBy = null, [FromQuery] int? cursor = null,
        [FromQuery] string? period = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null,
        [FromQuery] int? categoryId = null, [FromQuery] int? rankId = null,
        [FromQuery] string? governorate = null, [FromQuery] byte? gender = null,
        [FromQuery] string? unitName = null, [FromQuery] bool? hasSettlements = null)
    {
        var userId = this.GetUserId();
        var result = await _queryService.GetAllAsync(page, pageSize, search, statusId, userId, statusIds, uploadedBy, cursor, period, fromDate, toDate, categoryId, rankId, governorate, gender, unitName, hasSettlements);
        return result.ToActionResult();
    }

    /// <summary>
    /// تفاصيل ملف بالمعرف
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetById(int id)
    {
        var cacheKey = FileCachePrefix + id;
        if (_cache.TryGetValue(cacheKey, out object? cached))
            return Ok(cached);

        var result = await _queryService.GetByIdAsync(id);
        _cache.Set(cacheKey, result, FileCacheDuration);

        return result.ToActionResult();
    }

    /// <summary>
    /// بحث برقم الملف
    /// </summary>
    [HttpGet("by-number/{fileNumber}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetByFileNumber(string fileNumber)
    {
        var result = await _queryService.GetByFileNumberAsync(fileNumber);
        return result.ToActionResult();
    }

    /// <summary>
    /// ملفات متقاعد معين
    /// </summary>
    [HttpGet("by-retiree/{retireeId:int}")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetByRetireeId(int retireeId)
    {
        var result = await _queryService.GetByRetireeIdAsync(retireeId);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // MUTATION ENDPOINTS (IRetirementFileService)
    // ═══════════════════════════════════════

    /// <summary>
    /// إنشاء ملف تقاعد جديد
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "File.Create")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateRetirementFileDto dto)
    {
        var userId = this.GetUserId();
        var result = await _fileService.CreateAsync(dto, userId);
        if (!result.IsSuccess) return result.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.FileID }, result);
    }

    /// <summary>
    /// تحديث ملف
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "File.Edit.BasicData")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateRetirementFileDto dto)
    {
        var userId = this.GetUserId();
        var result = await _fileService.UpdateAsync(id, dto, userId);
        if (result.IsSuccess) _cache.Remove(FileCachePrefix + id);
        return result.ToActionResult();
    }

    /// <summary>
    /// تسجيل وفاة المتقاعد
    /// </summary>
    [HttpPost("{id:int}/death")]
    [Authorize(Policy = "File.ChangeStatus.Approve")]
    public async Task<IActionResult> HandleDeath(int id, [FromQuery] DateOnly deathDate)
    {
        var userId = this.GetUserId();
        var result = await _fileService.HandleRetireeDeathAsync(id, deathDate, userId);
        if (result.IsSuccess) _cache.Remove(FileCachePrefix + id);
        return result.ToActionResult();
    }

    /// <summary>تصحيح حالة وفاة مسجلة خطأً وإعادة المتقاعد إلى حي مع إبقاء الصرف موقوفاً للمراجعة</summary>
    [HttpPost("{id:int}/correct-life-status")]
    [Authorize(Policy = "File.ChangeStatus.Approve")]
    public async Task<IActionResult> CorrectLifeStatus(int id, [FromBody] CorrectRetireeLifeStatusDto dto)
    {
        var result = await _fileService.CorrectRetireeLifeStatusAsync(id, dto, this.GetUserId());
        if (result.IsSuccess) _cache.Remove(FileCachePrefix + id);
        return result.ToActionResult();
    }

    /// <summary>
    /// حذف ملف (ناعم)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "File.Delete.Soft")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.GetUserId();
        var result = await _fileService.DeleteAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// إسناد الملف لموظف
    /// </summary>
    [HttpPut("{id:int}/assign")]
    [Authorize(Policy = "File.Edit.BasicData")]
    public async Task<IActionResult> AssignEmployee(int id, [FromQuery] int employeeId)
    {
        var userId = this.GetUserId();
        var result = await _fileService.AssignEmployeeAsync(id, employeeId, userId);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // STATUS ENDPOINTS (IFileStatusService)
    // ═══════════════════════════════════════

    /// <summary>
    /// تغيير حالة الملفات
    /// </summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "File.ChangeStatus.Approve")]
    public async Task<IActionResult> ChangeStatus(int id, [FromQuery] int newStatusId, [FromQuery] string? reason = null)
    {
        var userId = this.GetUserId();
        var result = await _statusService.ChangeStatusAsync(id, newStatusId, reason, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// حالات الملفات
    /// </summary>
    [HttpGet("statuses")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetFileStatuses()
    {
        var result = await _statusService.GetFileStatusesAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// الحالات المسموحة للانتقال من الحالة الحالية
    /// </summary>
    [HttpGet("{id:int}/allowed-statuses")]
    [Authorize(Policy = "File.ChangeStatus.Approve")]
    public async Task<IActionResult> GetAllowedStatuses(int id)
    {
        var result = await _statusService.GetAllowedStatusTransitionsAsync(id);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // AUDIT & HISTORY (IFileQueryService)
    // ═══════════════════════════════════════

    /// <summary>
    /// سجل تغييرات حالة الملفات
    /// </summary>
    [HttpGet("{id:int}/status-transitions")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetStatusTransitions(int id)
    {
        var result = await _queryService.GetStatusTransitionsAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// سجل جميع تغييرات الملف (حالة + عمليات)
    /// </summary>
    [HttpGet("{id:int}/change-log")]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetFileChangeLog(int id, int page = 1, int pageSize = 200)
    {
        var result = await _queryService.GetFileChangeLogAsync(id, page, pageSize);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // ROLE-BASED QUERY ENDPOINTS (IFileQueryService)
    // ═══════════════════════════════════════

    /// <summary>
    /// ملفات لمدخل البيانات
    /// </summary>
    [HttpGet("for-data-entry")]
    [Authorize(Policy = "File.Edit.BasicData")]
    public async Task<IActionResult> GetForDataEntry([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _queryService.GetForDataEntryAsync(page, pageSize, search);
        return result.ToActionResult();
    }

    /// <summary>
    /// ملفات لرافع الوثائق (ناقصة مستندات)
    /// </summary>
    [HttpGet("for-document-upload")]
    [Authorize(Policy = "File.Documents.Upload")]
    public async Task<IActionResult> GetForDocumentUpload()
    {
        var result = await _queryService.GetForDocumentUploadAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// إحصائيات رفع الوثائق (لجميع الملفات)
    /// </summary>
    [HttpGet("document-upload-stats")]
    [Authorize(Policy = "File.Documents.Upload")]
    public async Task<IActionResult> GetDocumentUploadStats()
    {
        var result = await _queryService.GetDocumentUploadStatsAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// إحصائيات المدخلين (كل مدخل كم وثيقة أدخل)
    /// </summary>
    [HttpGet("uploader-stats")]
    [Authorize(Policy = "File.Documents.Upload")]
    public async Task<IActionResult> GetUploaderStats([FromQuery] UploaderStatsFilterDto filter)
    {
        var result = await _queryService.GetUploaderStatsAsync(filter);
        return result.ToActionResult();
    }

    /// <summary>
    /// إحصائيات مدخلي البيانات (كل مدخل كم ملف أنشأ)
    /// </summary>
    [HttpGet("data-entry-stats")]
    [Authorize(Policy = "File.Documents.Upload")]
    public async Task<IActionResult> GetDataEntryStats([FromQuery] UploaderStatsFilterDto filter)
    {
        var result = await _queryService.GetDataEntryStatsAsync(filter);
        return result.ToActionResult();
    }

    /// <summary>
    /// ملفات أنشأها مستخدم محدد (من سجل التدقيق)
    /// </summary>
    [HttpGet("data-entry-files/{userId:int}")]
    [Authorize(Policy = "File.Documents.Upload")]
    public async Task<IActionResult> GetDataEntryFiles(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var result = await _queryService.GetDataEntryFilesByUserAsync(userId, page, pageSize, fromDate, toDate);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // FILE REVIEW ENDPOINTS (IFileReviewService)
    // ═══════════════════════════════════════

    /// <summary>
    /// قائمة الملفات التي تحتاج مراجعة
    /// </summary>
    [HttpGet("pending-review")]
    [Authorize(Policy = "File.Review.View")]
    public async Task<IActionResult> GetPendingReviews()
    {
        var result = await _reviewService.GetPendingReviewsAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// عرض الفروقات (Diff) منذ آخر مراجعة للملف
    /// </summary>
    [HttpGet("{id:int}/review-diff")]
    [Authorize(Policy = "File.Review.View")]
    public async Task<IActionResult> GetReviewDiff(int id)
    {
        var result = await _reviewService.GetReviewDiffAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// اعتماد مراجعة الملف
    /// </summary>
    [HttpPost("{id:int}/review/approve")]
    [Authorize(Policy = "File.Review.Approve")]
    public async Task<IActionResult> ApproveReview(int id, [FromQuery] string? notes = null)
    {
        var userId = this.GetUserId();
        var result = await _reviewService.ApproveReviewAsync(id, userId, notes);
        return result.ToActionResult();
    }

    /// <summary>
    /// رفض مراجعة الملف مع سبب
    /// </summary>
    [HttpPost("{id:int}/review/reject")]
    [Authorize(Policy = "File.Review.Reject")]
    public async Task<IActionResult> RejectReview(int id, [FromQuery] string reason)
    {
        var userId = this.GetUserId();
        var result = await _reviewService.RejectReviewAsync(id, userId, reason);
        return result.ToActionResult();
    }

    /// <summary>
    /// طلب مراجعة الملف يدوياً
    /// </summary>
    [HttpPost("{id:int}/request-review")]
    [Authorize(Policy = "File.Review.View")]
    public async Task<IActionResult> RequestReview(int id)
    {
        var userId = this.GetUserId();
        var result = await _reviewService.RequestReviewAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// التحديث النهائي الواحد بعد رفع مجموعة مؤجلة: نواقص + امتثال + مراجعة مرة واحدة.
    /// آمن لإعادة التكرار. يتطلب صلاحية رفع الوثائق نفسها.
    /// </summary>
    [HttpPost("{id:int}/refresh-after-upload")]
    [Authorize(Policy = "File.Documents.Upload")]
    public async Task<IActionResult> RefreshAfterUpload(
        int id,
        [FromBody] PensionMS.Core.DTOs.Documents.RefreshAfterUploadRequest request)
    {
        var userId = this.GetUserId();
        var result = await _documentService.RefreshAfterUploadAsync(
            id, request?.DocumentIds ?? new List<int>(), userId);
        return result.ToActionResult();
    }

    // PER-ITEM REVIEW ENDPOINTS
    // ═══════════════════════════════════════

    /// <summary>
    /// اعتماد بند مراجعة فردي (يمثل تغيير كيان واحد)
    /// </summary>
    [HttpPost("review-items/{itemId:long}/approve")]
    [Authorize(Policy = "File.Review.Approve")]
    public async Task<IActionResult> ApproveReviewItem(long itemId, [FromQuery] string? notes = null)
    {
        var userId = this.GetUserId();
        var result = await _reviewService.ApproveItemAsync(itemId, userId, notes);
        return result.ToActionResult();
    }

    /// <summary>
    /// رفض بند مراجعة فردي (يمثل تغيير كيان واحد)
    /// </summary>
    [HttpPost("review-items/{itemId:long}/reject")]
    [Authorize(Policy = "File.Review.Reject")]
    public async Task<IActionResult> RejectReviewItem(long itemId, [FromQuery] string reason)
    {
        var userId = this.GetUserId();
        var result = await _reviewService.RejectItemAsync(itemId, userId, reason);
        return result.ToActionResult();
    }

    // ═══════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════

    /// <summary>
    /// تقرير تفصيلي لملفات التقاعد
    /// </summary>
    [HttpGet("report")]
    [Authorize(Policy = "File.View")]
    public async Task<IActionResult> GetReport([FromQuery] RetirementFileReportFilter filter)
    {
        var result = await _fileService.GetRetirementFileReportAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// ملخص تقارير ملفات التقاعد
    /// </summary>
    [HttpGet("report/summary")]
    [Authorize(Policy = "File.View")]
    public async Task<IActionResult> GetReportSummary([FromQuery] RetirementFileReportFilter filter)
    {
        var result = await _fileService.GetRetirementFileReportSummaryAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// تصدير تقرير ملفات التقاعد
    /// </summary>
    [HttpGet("report/export")]
    [Authorize(Policy = "File.View")]
    public async Task<IActionResult> ExportReport([FromQuery] RetirementFileReportFilter filter, [FromQuery] string format = "excel")
    {
        var bytes = await _fileService.ExportRetirementFileReportAsync(filter, format);
        if (bytes == null || bytes.Length == 0) return NotFound();

        var fileName = $"RetirementFiles_Report_{PensionMS.Core.Time.YemenTime.Now:yyyyMMdd}";
        if (format.ToLower() == ExportFormatConstants.Pdf)
        {
            return File(bytes, "application/pdf", $"{fileName}.pdf");
        }

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }
}
