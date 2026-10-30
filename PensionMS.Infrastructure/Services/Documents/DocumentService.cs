using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Helpers;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Core.Rules.Documents;

namespace PensionMS.Infrastructure.Services.Documents;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IEntitlementEngine _entitlementEngine;
    private readonly IFileReviewService _reviewService;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly IAccessiblePensionRepository _access;
    private readonly IImmutableAuditService _immutableAudit;
    private readonly Microsoft.Extensions.Logging.ILogger<DocumentService> _logger;

    public DocumentService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IEntitlementEngine entitlementEngine,
        IFileReviewService reviewService,
        IAccessiblePensionRepository access,
        IImmutableAuditService immutableAudit,
        Microsoft.Extensions.Logging.ILogger<DocumentService> logger,
        IMissingDocumentService missingDocumentService)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _entitlementEngine = entitlementEngine;
        _reviewService = reviewService;
        _access = access;
        _immutableAudit = immutableAudit;
        _logger = logger;
        _missingDocumentService = missingDocumentService;
    }

    /// <summary>
    /// نطاق الفرع: يستبعد وثائق الفروع الأخرى لغير المستخدم المركزي
    /// </summary>
    private async Task<bool> HasBranchAccessAsync(int? fileId)
    {
        var access = await _access.ForCurrentOperationAsync();
        if (access.Scope.AllBranches) return true;
        return fileId.HasValue
            ? await access.Files().AnyAsync(f => f.FileID == fileId.Value)
            : access.Scope.AllBranches;
    }

    private async Task<bool> CanAccessOwnerAsync(Document owner)
        => await (await _access.ForCurrentOperationAsync()).CanAccessDocumentOwnerAsync(owner);

    public async Task<bool> CanDownloadAsync(string relativePath)
    {
        var path = relativePath.Replace('\\', '/').TrimStart('/');
        if (path.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase)) path = path[8..];
        var paths = new[] { path, "uploads/" + path, "/uploads/" + path };
        var candidates = await _unitOfWork.Documents.Query().AsNoTracking()
            .Where(d => !d.IsDeleted && paths.Contains(d.FilePath))
            .Select(d => new { d.DocumentID, d.OwnerType }).ToListAsync();
        foreach (var candidate in candidates)
        {
            var permission = candidate.OwnerType switch
            {
                DocumentOwnerType.Retiree => "Retiree.View",
                DocumentOwnerType.Employee => "HR.Employee.View",
                DocumentOwnerType.Correspondence => "Correspondence.View",
                >= DocumentOwnerType.InvestmentProject and <= DocumentOwnerType.RiskAssessment => "Investment.View",
                _ => "File.View.Sensitive"
            };
            try
            {
                var access = await _access.ForPermissionAsync(permission);
                if (await access.Query(_unitOfWork.Documents).AnyAsync(d => d.DocumentID == candidate.DocumentID)) return true;
            }
            catch (PensionMS.Core.Exceptions.BranchScopeAccessException) { }
        }
        // Legacy personal photos have no Document row; ownership still comes from their real record.
        foreach (var permission in new[] { "Retiree.View", "Agent.View", "Guardian.View" })
        {
            try
            {
                var access = await _access.ForPermissionAsync(permission);
                var allowed = permission switch
                {
                    "Retiree.View" => await _unitOfWork.Retirees.Query().AnyAsync(r => !r.IsDeleted && paths.Contains(r.PersonalPhotoPath!)),
                    "Agent.View" => await access.Query(_unitOfWork.Agents).AnyAsync(a => paths.Contains(a.PersonalPhotoPath!)),
                    _ => await access.Query(_unitOfWork.Guardians).AnyAsync(g => paths.Contains(g.PersonalPhotoPath!))
                };
                if (allowed) return true;
            }
            catch (PensionMS.Core.Exceptions.BranchScopeAccessException) { }
        }
        return false;
    }

    public async Task<ApiResponse<PagedResult<DocumentDto>>> GetAllAsync(
        int? ownerType,
        int? documentTypeId,
        DateTime? dateFrom,
        DateTime? dateTo,
        bool? verified,
        string? searchTerm,
        int? branchId,
        int? reviewStatus,
        int page,
        int pageSize)
    {
        var query = (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .Include(d => d.RetirementFile).ThenInclude(rf => rf!.Retiree)
            .Include(d => d.Agent)
            .Include(d => d.Guardian)
            .Include(d => d.EntitledPerson)
            .Include(d => d.Retiree)
            .Where(d => !d.IsDeleted)
            .AsQueryable();

        if (ownerType.HasValue)
            query = query.Where(d => d.OwnerType == (DocumentOwnerType)ownerType.Value);

        if (documentTypeId.HasValue)
            query = query.Where(d => d.DocumentTypeID == documentTypeId.Value);

        if (dateFrom.HasValue)
            query = query.Where(d => d.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(d => d.CreatedAt <= dateTo.Value);

        if (verified.HasValue)
            query = query.Where(d => d.Verified == verified.Value);

        // نطاق الفرع يُفرض من الخادم: المستخدم غير المقيّد بفرع محدد لا يتجاوزه مهما أرسل المعامل
        if (branchId.HasValue)
            query = query.Where(d => d.RetirementFile != null && d.RetirementFile.PensionBranchID == branchId.Value);

        if (reviewStatus.HasValue)
            query = query.Where(d => d.PendingReviewStatus == (PendingReviewStatus)reviewStatus.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(d =>
                d.FileName.Contains(term) ||
                (d.DocumentNumber != null && d.DocumentNumber.Contains(term)) ||
                (d.Notes != null && d.Notes.Contains(term)) ||
                (d.MimeType != null && d.MimeType.Contains(term)) ||
                (d.DocumentType != null && d.DocumentType.TypeName.Contains(term)) ||
                (d.UploadedByUser != null && d.UploadedByUser.FullName.Contains(term)) ||
                (d.Agent != null && (d.Agent.FullName.Contains(term) || d.Agent.NationalID.Contains(term) || (d.Agent.Phone != null && d.Agent.Phone.Contains(term)))) ||
                (d.Guardian != null && (d.Guardian.FullName.Contains(term) || d.Guardian.NationalID.Contains(term) || (d.Guardian.Phone != null && d.Guardian.Phone.Contains(term)))) ||
                (d.Retiree != null && (d.Retiree.FullName.Contains(term) || d.Retiree.NationalID.Contains(term) || (d.Retiree.Phone != null && d.Retiree.Phone.Contains(term)))) ||
                (d.EntitledPerson != null && (d.EntitledPerson.FullName.Contains(term) || d.EntitledPerson.NationalID.Contains(term) || (d.EntitledPerson.Phone != null && d.EntitledPerson.Phone.Contains(term)))) ||
                (d.RetirementFile != null && (d.RetirementFile.FileNumber.Contains(term) || (d.RetirementFile.Retiree != null && d.RetirementFile.Retiree.FullName.Contains(term)))));
        }

        var totalCount = await query.CountAsync();

        var docs = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = docs.Select(d => MapToDto(d)).ToList();

        var result = new PagedResult<DocumentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<DocumentDto>>.Success(result);
    }

    public async Task<ApiResponse<ArchiveStatsDto>> GetArchiveStatsAsync()
    {
        var docQuery = (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents).Where(d => !d.IsDeleted);

        var totalDocuments = await docQuery.CountAsync();
        var verifiedDocuments = await docQuery.CountAsync(d => d.Verified);
        var pendingReview = await docQuery.CountAsync(d => d.PendingReviewStatus == PendingReviewStatus.Pending);
        var expiredDocuments = await docQuery.CountAsync(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < PensionMS.Core.Time.YemenTime.Today);

        var missingQuery = _unitOfWork.MissingDocuments.Query().Where(m => !m.IsDeleted);
        var outstanding = new[] { MissingDocumentStatusConstants.Pending, MissingDocumentStatusConstants.Pledged };

        var totalMissingDocuments = await missingQuery.CountAsync(m => outstanding.Contains(m.Status));
        var pendingMissingDocuments = await missingQuery.CountAsync(m => m.Status == MissingDocumentStatusConstants.Pending);
        var pledgedMissingDocuments = await missingQuery.CountAsync(m => m.Status == MissingDocumentStatusConstants.Pledged);
        var filesWithMissingDocuments = await missingQuery
            .Where(m => outstanding.Contains(m.Status))
            .Select(m => m.FileID)
            .Distinct()
            .CountAsync();

        var stats = new ArchiveStatsDto
        {
            TotalDocuments = totalDocuments,
            VerifiedDocuments = verifiedDocuments,
            PendingReview = pendingReview,
            ExpiredDocuments = expiredDocuments,
            FilesWithMissingDocuments = filesWithMissingDocuments,
            TotalMissingDocuments = totalMissingDocuments,
            PendingMissingDocuments = pendingMissingDocuments,
            PledgedMissingDocuments = pledgedMissingDocuments
        };

        return ApiResponse<ArchiveStatsDto>.Success(stats);
    }

    public async Task<ApiResponse<DocumentDto>> GetByIdAsync(int id)
    {
        var doc = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.DocumentID == id && !d.IsDeleted);

        if (doc == null)
            return ApiResponse<DocumentDto>.Fail("الوثيقة غير موجودة");

        if (!await CanAccessOwnerAsync(doc))
            return ApiResponse<DocumentDto>.Fail("لا تملك صلاحية الوصول إلى وثيقة في فرع آخر");

        return ApiResponse<DocumentDto>.Success(MapToDto(doc));
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetByFileIdAsync(int fileId)
    {
        if (!await HasBranchAccessAsync(fileId))
            return ApiResponse<List<DocumentDto>>.Fail("لا تملك صلاحية الوصول إلى ملف في فرع آخر");

        var docs = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .Include(d => d.Agent)
            .Include(d => d.Guardian)
            .Include(d => d.EntitledPerson)
            .Include(d => d.Retiree)
            .Include(d => d.RetirementFile).ThenInclude(f => f!.Retiree)
            .Include(d => d.Pledge)
            .Include(d => d.Exemption)
            .Where(d => d.FileID == fileId && !d.IsDeleted && d.OwnerType < DocumentOwnerType.InvestmentProject)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var dtos = docs.Select(d => MapToDto(d)).ToList();
        return ApiResponse<List<DocumentDto>>.Success(dtos);
    }

    public async Task<ApiResponse<string?>> GetRetireeIdentityPathAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var file = await queries.Files().AsNoTracking().Where(f => f.FileID == fileId && !f.IsDeleted)
            .Select(f => new { f.RetireeID }).FirstOrDefaultAsync();
        if (file == null) return ApiResponse<string?>.Fail("الملف غير موجود ضمن نطاق الصلاحية");
        var documents = await queries.Query(_unitOfWork.Documents).AsNoTracking()
            .Where(d => d.FileID == fileId && d.DocumentTypeID == DocumentTypeConstants.NationalId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.DocumentID).ToListAsync();
        var owner = new RetirementDocumentRequirement(DocumentTypeConstants.NationalId, file.RetireeID,
            null, null, null, null, null);
        var today = PensionMS.Core.Time.YemenTime.Today;
        var identity = RetirementFileDocumentRules.FindValidDocument(documents, owner, today);
        return ApiResponse<string?>.Success(identity?.FilePath);
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var docs = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .Where(d => d.RetireeID == retireeId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var dtos = docs.Select(d => MapToDto(d)).ToList();
        return ApiResponse<List<DocumentDto>>.Success(dtos);
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetByCorrespondenceIdAsync(int correspondenceId)
    {
        var docs = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .Where(d => d.CorrespondenceID == correspondenceId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var dtos = docs.Select(d => MapToDto(d)).ToList();
        return ApiResponse<List<DocumentDto>>.Success(dtos);
    }

    public async Task<ApiResponse<DocumentDto>> UploadAsync(UploadDocumentDto dto, int uploadedByUserId, string? requestId = null, bool deferPostProcessing = false)
    {
        var file = dto.File;
        if (file == null || file.Length == 0)
            return ApiResponse<DocumentDto>.Fail("لم يتم اختيار ملف");

        var docType = await _unitOfWork.DocumentTypes.GetByIdAsync(dto.DocumentTypeID);
        if (docType == null) return ApiResponse<DocumentDto>.Fail("نوع الوثيقة غير صالح");
        if (!docType.IsActive)
            return ApiResponse<DocumentDto>.Fail($"نوع الوثيقة «{docType.TypeName}» غير نشط");

        if (docType.MaxFileSize.HasValue)
        {
            var sizeInMb = file.Length / 1024f / 1024f;
            if (sizeInMb > docType.MaxFileSize.Value)
                return ApiResponse<DocumentDto>.Fail($"حجم الملف تجاوز الحد المسموح ({docType.MaxFileSize} MB)");
        }
        else
        {
            if (file.Length > 10 * 1024 * 1024)
                return ApiResponse<DocumentDto>.Fail("حجم الملف يتجاوز الحد الأقصى (10 ميجابايت)");
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!string.IsNullOrEmpty(docType.AllowedExtensions))
        {
            var allowed = docType.AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim().ToLower());
            if (!allowed.Contains(ext))
                return ApiResponse<DocumentDto>.Fail("امتداد الملف غير مسموح");
        }
        else
        {
            var fallback = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            if (!fallback.Contains(ext))
                return ApiResponse<DocumentDto>.Fail("امتداد الملف غير مسموح");
        }

        // م29: الوجه الخلفي يخضع لفحص الامتداد والحجم نفسه قبل الدمج، والدمج يدعم صورًا فقط
        if (dto.FileBack != null && dto.FileBack.Length > 0)
        {
            var backExt = Path.GetExtension(dto.FileBack.FileName).ToLower();
            if (!string.IsNullOrEmpty(docType.AllowedExtensions))
            {
                var allowedBack = docType.AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim().ToLower());
                if (!allowedBack.Contains(backExt))
                    return ApiResponse<DocumentDto>.Fail("امتداد الوجه الخلفي غير مسموح");
            }
            else
            {
                var fallbackBack = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                if (!fallbackBack.Contains(backExt))
                    return ApiResponse<DocumentDto>.Fail("امتداد الوجه الخلفي غير مسموح");
            }

            if (docType.MaxFileSize.HasValue)
            {
                var backSizeMb = dto.FileBack.Length / 1024f / 1024f;
                if (backSizeMb > docType.MaxFileSize.Value)
                    return ApiResponse<DocumentDto>.Fail($"حجم الوجه الخلفي تجاوز الحد المسموح ({docType.MaxFileSize} MB)");
            }
            else if (dto.FileBack.Length > 10 * 1024 * 1024)
            {
                return ApiResponse<DocumentDto>.Fail("حجم الوجه الخلفي يتجاوز الحد الأقصى (10 ميجابايت)");
            }

            if (ext == ".pdf")
                return ApiResponse<DocumentDto>.Fail("الدمج مع الوجه الخلفي يدعم الصور فقط؛ لا يمكن إرسال ملف PDF مع وجه خلفي");

            if (backExt != ".jpg" && backExt != ".jpeg" && backExt != ".png")
                return ApiResponse<DocumentDto>.Fail("الوجه الخلفي يجب أن يكون صورة بصيغة JPG أو PNG");

            var totalCapMb = docType.MaxFileSize ?? 10;
            if ((file.Length + dto.FileBack.Length) / 1024f / 1024f > totalCapMb)
                return ApiResponse<DocumentDto>.Fail($"الحجم الإجمالي للوجهين تجاوز الحد المسموح ({totalCapMb} MB)");
        }

        // م33: قيود حقول بيانات الوثيقة قبل الحفظ
        var fieldError = ValidateDocumentFields(dto.Notes, dto.DocumentNumber, dto.IssueDate, dto.ExpiryDate);
        if (fieldError != null)
            return fieldError;

        // م30: سياسة التخزين تُشتق من نوع الوثيقة نفسها — الملف النهائي بعد الدمج يُفحص بنفس القواعد
        var policyExtensions = !string.IsNullOrEmpty(docType.AllowedExtensions)
            ? docType.AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim()).Where(e => e.Length > 0).ToArray()
            : null;
        long? policyMaxBytes = docType.MaxFileSize.HasValue
            ? (long)(docType.MaxFileSize.Value * 1024 * 1024)
            : null;

        DocumentOwnerType? ownerType =
            dto.EntitledPersonID.HasValue  ? DocumentOwnerType.EntitledPerson    :
            dto.AgentID.HasValue           ? DocumentOwnerType.Agent             :
            dto.GuardianID.HasValue        ? DocumentOwnerType.Guardian          :
            dto.RetireeID.HasValue         ? DocumentOwnerType.Retiree           :
            dto.RetirementFileID.HasValue  ? DocumentOwnerType.RetirementFile    :
            dto.PledgeID.HasValue          ? DocumentOwnerType.DocumentPledge    :
            dto.ExemptionID.HasValue       ? DocumentOwnerType.DocumentExemption :
            dto.InvestmentProjectID.HasValue ? DocumentOwnerType.InvestmentProject :
            dto.InvestmentPropertyID.HasValue ? DocumentOwnerType.InvestmentProperty :
            dto.PropertyUnitID.HasValue    ? DocumentOwnerType.PropertyUnit      :
            dto.LeaseContractID.HasValue   ? DocumentOwnerType.LeaseContract     :
            dto.TenantID.HasValue          ? DocumentOwnerType.Tenant            :
            dto.InvestmentPortfolioID.HasValue ? DocumentOwnerType.InvestmentPortfolio :
            dto.MaintenanceRequestID.HasValue ? DocumentOwnerType.MaintenanceRequest :
            dto.WorkOrderID.HasValue       ? DocumentOwnerType.WorkOrder         :
            (dto.AssetValuationID.HasValue || dto.PropertyValuationID.HasValue) ? DocumentOwnerType.PropertyValuation :
            dto.DepreciationRecordID.HasValue ? DocumentOwnerType.PropertyDepreciation :
            dto.PaymentScheduleID.HasValue ? DocumentOwnerType.LeasePayment :
            dto.OperationalExpenseID.HasValue ? DocumentOwnerType.OperationalExpense :
            dto.InvestmentPlanID.HasValue  ? DocumentOwnerType.InvestmentPlan :
            dto.RiskAssessmentID.HasValue  ? DocumentOwnerType.RiskAssessment :
            dto.EmployeeID.HasValue        ? DocumentOwnerType.Employee :
            null;

        if (ownerType is null)
        {
            return ApiResponse<DocumentDto>.Fail("يجب تحديد مالك الوثيقة");
        }

        // م19: توافق نوع الوثيقة مع المالك
        if (docType.OwnerType.HasValue && docType.OwnerType.Value != ownerType.Value)
            return ApiResponse<DocumentDto>.Fail($"نوع الوثيقة «{docType.TypeName}» لا يسمح بالرفع لهذا المالك");

        // م18: مالك منطقي واحد + تحقق تابعية الشخص/التعهد/الإعفاء للملف
        var (fileId, ownerError) = await ResolveOwnerFileAsync(
            dto.RetirementFileID, dto.RetireeID, dto.EntitledPersonID, dto.AgentID, dto.GuardianID,
            dto.PledgeID, dto.ExemptionID, currentFileId: null);
        if (ownerError != null)
            return ownerError;

        if (!await CanAccessOwnerAsync(new Document { FileID = fileId, EntitledID = dto.EntitledPersonID, AssetValuationID = dto.AssetValuationID ?? dto.PropertyValuationID, RetireeID = dto.RetireeID, AgentID = dto.AgentID, GuardianID = dto.GuardianID, PledgeID = dto.PledgeID, ExemptionID = dto.ExemptionID, InvestmentProjectID = dto.InvestmentProjectID, InvestmentPropertyID = dto.InvestmentPropertyID, PropertyUnitID = dto.PropertyUnitID, LeaseContractID = dto.LeaseContractID, TenantID = dto.TenantID, InvestmentPortfolioID = dto.InvestmentPortfolioID, MaintenanceRequestID = dto.MaintenanceRequestID, WorkOrderID = dto.WorkOrderID, DepreciationRecordID = dto.DepreciationRecordID, PaymentScheduleID = dto.PaymentScheduleID, OperationalExpenseID = dto.OperationalExpenseID, InvestmentPlanID = dto.InvestmentPlanID, RiskAssessmentID = dto.RiskAssessmentID, EmployeeID = dto.EmployeeID }))
            return ApiResponse<DocumentDto>.Fail("مالك الوثيقة غير موجود أو خارج نطاق صلاحياتك");

        string relativePath;
        long fileSize;
        string contentType;
        string contentHash;
        string fileName = file.FileName;

        try
        {
            if (dto.DocumentTypeID == PensionMS.Core.Constants.DocumentTypeConstants.NationalId && (dto.FileBack != null))
            {
                var (stream, mergedContentType, extension) = await ImageMergeHelper.MergeImagesToPdfAsync(dto.File, dto.FileBack);
                if (stream != Stream.Null)
                {
                    fileName = $"NationalID_Unified_{PensionMS.Core.Time.YemenTime.Now:yyyyMMddHHmmss}{extension}";
                    contentHash = await ComputeSha256Async(stream);
                    if (stream.CanSeek) stream.Position = 0;
                    relativePath = await _fileStorage.SaveFileAsync(stream, fileName, "documents", policyExtensions, policyMaxBytes);
                    fileSize = stream.Length;
                    contentType = mergedContentType;
                }
                else
                {
                    using var originalStream = file.OpenReadStream();
                    contentHash = await ComputeSha256Async(originalStream);
                    if (originalStream.CanSeek) originalStream.Position = 0;
                    relativePath = await _fileStorage.SaveFileAsync(originalStream, file.FileName, "documents", policyExtensions, policyMaxBytes);
                    fileSize = file.Length;
                    contentType = file.ContentType;
                }
            }
            else
            {
                using var stream = file.OpenReadStream();
                contentHash = await ComputeSha256Async(stream);
                if (stream.CanSeek) stream.Position = 0;
                relativePath = await _fileStorage.SaveFileAsync(stream, file.FileName, "documents", policyExtensions, policyMaxBytes);
                fileSize = file.Length;
                contentType = file.ContentType;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل في حفظ الملف على القرص (requestId={RequestId})", requestId);
            return ApiResponse<DocumentDto>.Fail("فشل في حفظ الملف. يرجى المحاولة لاحقاً");
        }

        // ─── منع التكرار: بصمة محتوى + نطاق مالك (م24) ───
        if (fileId.HasValue)
        {
            var candidates = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
                .Where(d => !d.IsDeleted
                    && d.FileID == fileId.Value
                    && d.DocumentTypeID == dto.DocumentTypeID
                    && d.RetireeID == dto.RetireeID
                    && d.EntitledID == dto.EntitledPersonID
                    && d.AgentID == dto.AgentID
                    && d.GuardianID == dto.GuardianID
                    && ((d.Checksum != null && d.Checksum == contentHash)
                        || (d.Checksum == null && d.FileName == fileName && d.FileSize == fileSize)))
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var duplicate = candidates.FirstOrDefault(d => RetirementFileDocumentRules.IsValid(d, PensionMS.Core.Time.YemenTime.Today));

            if (duplicate != null)
            {
                // حذف الملف المحفوظ على القرص لأنه مكرر
                try { await _fileStorage.DeleteFileAsync(relativePath); } catch { /* تجاهل */ }
                throw new PensionMS.Core.Exceptions.DuplicateDocumentException(MapToDto(duplicate));
            }
        }

        var doc = new Document
        {
            DocumentTypeID = dto.DocumentTypeID,
            OwnerType = ownerType.Value,
            FileName = fileName,
            FilePath = relativePath,
            FileSize = fileSize,
            MimeType = contentType,
            Checksum = contentHash,
            Notes = dto.Notes,
            DocumentNumber = dto.DocumentNumber,
            IssueDate = dto.IssueDate,
            ExpiryDate = dto.ExpiryDate,
            UploadedByUserID = uploadedByUserId,
            FileID = fileId,
            RetireeID = dto.RetireeID,
            EntitledID = dto.EntitledPersonID,
            AgentID = dto.AgentID,
            GuardianID = dto.GuardianID,
            PledgeID = dto.PledgeID,
            ExemptionID = dto.ExemptionID,
            InvestmentProjectID = dto.InvestmentProjectID,
            InvestmentPropertyID = dto.InvestmentPropertyID,
            PropertyUnitID = dto.PropertyUnitID,
            LeaseContractID = dto.LeaseContractID,
            TenantID = dto.TenantID,
            InvestmentPortfolioID = dto.InvestmentPortfolioID,
            MaintenanceRequestID = dto.MaintenanceRequestID,
            WorkOrderID = dto.WorkOrderID,
            AssetValuationID = dto.AssetValuationID ?? dto.PropertyValuationID,
            DepreciationRecordID = dto.DepreciationRecordID,
            PaymentScheduleID = dto.PaymentScheduleID,
            OperationalExpenseID = dto.OperationalExpenseID,
            InvestmentPlanID = dto.InvestmentPlanID,
            RiskAssessmentID = dto.RiskAssessmentID,
            EmployeeID = dto.EmployeeID,
            CreatedAt = DateTime.UtcNow
        };

        if (_unitOfWork.HasActiveTransaction)
            _unitOfWork.OnRollback(async () =>
            {
                try { await _fileStorage.DeleteFileAsync(relativePath); }
                catch (Exception ex) { _logger.LogError(ex, "تعذر تنظيف مرفق وثيقة بعد التراجع عن المعاملة: {Path}", relativePath); }
            });

        var uploadTime = DateTime.UtcNow;
        try
        {
            await _unitOfWork.Documents.AddAsync(doc);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // م32: فشل حفظ السجل لا يترك ملفًا يتيمًا على القرص
            try { await _fileStorage.DeleteFileAsync(relativePath); } catch { /* تجاهل */ }

            _logger.LogError(ex, "فشل حفظ سجل الوثيقة على قاعدة البيانات (requestId={RequestId})", requestId);
            return ApiResponse<DocumentDto>.Fail("فشل في حفظ سجل الوثيقة. يرجى المحاولة لاحقاً");
        }

        await _immutableAudit.LogAsync(uploadedByUserId, "", "UploadDocument", "Document", doc.DocumentID,
            null, System.Text.Json.JsonSerializer.Serialize(new
            {
                doc.DocumentID,
                doc.DocumentTypeID,
                doc.OwnerType,
                doc.FileName,
                doc.FileID,
                doc.RetireeID,
                doc.EntitledID,
                doc.AgentID,
                doc.GuardianID
            }));

        // ─── المعالجة اللاحقة داخل try/catch منفصل ───
        // الوثيقة محفوظة بالفعل. فشل المعالجة اللاحقة لا يلغي الحفظ.
        // وضع التأجيل للرفع الجماعي: يتخطى عمل مستوى الملف (نواقص/امتثال/مراجعة) ويبقيه للتحديث النهائي الواحد.
        try
        {
            // Update DocumentCount on parent entity if it's an operational expense
            // عدد وثائق على مستوى الوثيقة نفسها: يبقى لكل وثيقة حتى في وضع التأجيل.
            if (dto.OperationalExpenseID.HasValue)
            {
                var expense = await _unitOfWork.OperationalExpenses.GetByIdAsync(dto.OperationalExpenseID.Value);
                if (expense != null)
                {
                    expense.DocumentCount++;
                    expense.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.OperationalExpenses.Update(expense);
                }
            }

            if (fileId.HasValue && !deferPostProcessing)
                await ProcessFileLevelPostUploadAsync(fileId.Value,
                    new List<Document> { doc }, uploadedByUserId, uploadTime);
        }
        catch (Exception ex)
        {
            // م26: المعالجة اللاحقة فشلت لكن الوثيقة محفوظة — سجل أخطاء دائم (لا Debug.WriteLine)
            _logger.LogError(ex,
                "فشلت المعالجة اللاحقة بعد رفع الوثيقة {DocumentId} (requestId={RequestId})",
                doc.DocumentID, requestId);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(); // حفظ أي تعديلات متبقية من المعالجة اللاحقة
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "فشل حفظ التعديلات المتبقية بعد رفع الوثيقة {DocumentId} (requestId={RequestId})",
                doc.DocumentID, requestId);
        }

        return ApiResponse<DocumentDto>.Success(MapToDto(doc), "تم رفع الوثيقة بنجاح");
    }

    /// <summary>
    /// التحديث النهائي الواحد بعد رفع مجموعة مؤجلة: نواقص + امتثال + مراجعة مرة واحدة.
    /// آمن لإعادة التكرار: إيفاء النواقص لا يعيد فتح المنجز، والامتثال يعيد الحساب من الوثائق الفعلية،
    /// والمراجعة التفاضلية لا تكرر البنود. صفوف التدقيق الجديدة عند التكرار مقصودة (توثق كل استدعاء).
    /// </summary>
    public Task<ApiResponse<RefreshAfterUploadResult>> RefreshAfterUploadAsync(
        int fileId, List<int> documentIds, int userId, DateTime? requestedAt = null)
        => RefreshAfterUploadCoreAsync(fileId, documentIds, userId, requestedAt, true);

    public Task<ApiResponse<RefreshAfterUploadResult>> RefreshAfterLifecycleUploadAsync(
        int fileId, List<int> documentIds, int userId, DateTime requestedAt)
        => RefreshAfterUploadCoreAsync(fileId, documentIds, userId, requestedAt, false);

    private async Task<ApiResponse<RefreshAfterUploadResult>> RefreshAfterUploadCoreAsync(
        int fileId, List<int> documentIds, int userId, DateTime? requestedAt, bool reevaluate)
    {
        if (!await HasBranchAccessAsync(fileId))
            return ApiResponse<RefreshAfterUploadResult>.Fail("لا تملك صلاحية تحديث ملف في فرع آخر");

        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file == null || file.IsDeleted)
            return ApiResponse<RefreshAfterUploadResult>.Fail("الملف غير موجود");

        var distinctIds = (documentIds ?? new List<int>()).Distinct().ToList();
        var docs = distinctIds.Count == 0
            ? new List<Document>()
            : await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
                .Where(d => distinctIds.Contains(d.DocumentID) && d.FileID == fileId && !d.IsDeleted)
                .ToListAsync();

        var found = docs.Select(d => d.DocumentID).ToHashSet();
        var skipped = distinctIds.Where(id => !found.Contains(id)).ToList();
        var timestamp = requestedAt ?? DateTime.UtcNow;

        try
        {
            var fulfilled = await ProcessFileLevelPostUploadAsync(fileId, docs, userId, timestamp, reevaluate);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
            return ApiResponse<RefreshAfterUploadResult>.Success(new RefreshAfterUploadResult
            {
                ProcessedDocuments = docs.Count,
                SkippedDocumentIds = skipped,
                FulfilledMissingCount = fulfilled,
                MissingDocumentsCount = updated?.MissingDocumentsCount ?? 0,
                ReviewRequested = true
            }, "تم تحديث حالة الملف بعد الرفع");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "فشل التحديث النهائي بعد الرفع للملف {FileId}", fileId);
            return ApiResponse<RefreshAfterUploadResult>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            // آمن لإعادة المحاولة: المنجز من النواقص لا يتكرر، والمراجعة تفاضلية.
            _logger.LogError(ex, "فشل التحديث النهائي بعد الرفع للملف {FileId}", fileId);
            return ApiResponse<RefreshAfterUploadResult>.Fail("فشل التحديث النهائي بعد الرفع. يمكن إعادة المحاولة بأمان دون إعادة رفع الوثائق");
        }
    }

    /// <summary>
    /// عمل مستوى الملف بعد الرفع، مرة واحدة لمجموعة وثائق: إيفاء النواقص والتعهدات، ثم امتثال واحد، ثم مراجعة واحدة.
    /// نفس الكود يخدم الرفع الفردي (قائمة بوثيقة واحدة) والتحديث النهائي الجماعي. يعيد عدد النواقص المستوفاة.
    /// </summary>
    private async Task<int> ProcessFileLevelPostUploadAsync(
        int fileId, List<Document> docs, int userId, DateTime timestamp, bool reevaluate = true)
    {
        var fulfilledTotal = 0;
        var affectedPledgeIds = new HashSet<int>();
        var requirements = RetirementFileDocumentRules.Requirements(
            await _missingDocumentService.GetRequiredPairsAsync(fileId), []);
        var today = PensionMS.Core.Time.YemenTime.Today;

        foreach (var uploaded in docs.Where(d => d.FileID == fileId && RetirementFileDocumentRules.IsValid(d, today))
            .DistinctBy(RetirementFileDocumentRules.Owner))
        {
            var query = _unitOfWork.MissingDocuments.Query()
                .Where(m => m.FileID == fileId && !m.IsDeleted
                          && m.DocumentTypeID == uploaded.DocumentTypeID
                          && (m.Status == MissingDocumentStatusConstants.Pending || m.Status == MissingDocumentStatusConstants.Pledged));
            var owner = RetirementFileDocumentRules.Owner(uploaded);
            var pendingMissing = (await query.ToListAsync())
                .Where(m => RetirementFileDocumentRules.Owner(m, requirements) == owner).ToList();

            foreach (var m in pendingMissing)
            {
                m.Status = MissingDocumentStatusConstants.Fulfilled;
                m.SubmittedAt = timestamp;
                if (m.PledgeID.HasValue)
                    affectedPledgeIds.Add(m.PledgeID.Value);
            }

            fulfilledTotal += pendingMissing.Count;
        }

        if (fulfilledTotal > 0)
        {
            await _unitOfWork.SaveChangesAsync();

            foreach (var pid in affectedPledgeIds)
            {
                var stillPending = await _unitOfWork.MissingDocuments.Query()
                    .AnyAsync(m => m.PledgeID == pid && m.DocumentTypeID != DocumentTypeConstants.PledgeDocument && m.Status != MissingDocumentStatusConstants.Fulfilled && m.Status != MissingDocumentStatusConstants.Exempted);

                if (!stillPending)
                {
                    var pledge = await _unitOfWork.DocumentPledges.GetByIdAsync(pid);
                    if (pledge != null && pledge.Status != MissingDocumentStatusConstants.Fulfilled)
                    {
                        pledge.Status = MissingDocumentStatusConstants.Fulfilled;
                        pledge.FulfilledAt = DateTime.UtcNow;
                        _unitOfWork.DocumentPledges.Update(pledge);

                        var pledgeDirectiveDocs = await _unitOfWork.MissingDocuments.Query()
                            .Where(m => m.PledgeID == pid && m.DocumentTypeID == DocumentTypeConstants.PledgeDocument && m.Status == MissingDocumentStatusConstants.Pending)
                            .ToListAsync();

                        foreach (var pd in pledgeDirectiveDocs)
                        {
                            pd.Status = MissingDocumentStatusConstants.Fulfilled;
                            pd.Notes += " (تم الإيفاء به تلقائياً)";
                        }
                    }
                }
            }
        }

        // إعادة تقييم المستحقين مرة لكل شخص بدل مرة لكل وثيقة
        foreach (var entitledId in (reevaluate ? docs : new List<Document>())
            .Where(d => d.EntitledID.HasValue)
            .Select(d => d.EntitledID!.Value)
            .Distinct())
        {
            await _entitlementEngine.ReEvaluateEntitlementStatusAsync(entitledId);
        }

        await _missingDocumentService.RefreshComplianceCacheAsync(fileId);

        var reviewResult = await _reviewService.RequestReviewAsync(fileId, userId, timestamp);
        if (!reviewResult.IsSuccess)
            throw new InvalidOperationException(reviewResult.Message ?? "تعذر تحديث مراجعة الملف");

        return fulfilledTotal;
    }

    /// <summary>
    /// م18: فرض مالك منطقي واحد والتحقق من تابعية الشخص/التعهد/الإعفاء لملف التقاعد المُرسل أو الحالي
    /// </summary>
    private async Task<(int? FileId, ApiResponse<DocumentDto>? Error)> ResolveOwnerFileAsync(
        int? retirementFileId,
        int? retireeId,
        int? entitledPersonId,
        int? agentId,
        int? guardianId,
        int? pledgeId,
        int? exemptionId,
        int? currentFileId)
    {
        var personCount = new[] { retireeId, entitledPersonId, agentId, guardianId }.Count(id => id.HasValue);
        if (personCount > 1)
            return (null, ApiResponse<DocumentDto>.Fail("لا يمكن ربط الوثيقة بأكثر من مالك شخصي واحد"));

        if (pledgeId.HasValue && exemptionId.HasValue)
            return (null, ApiResponse<DocumentDto>.Fail("لا يمكن ربط الوثيقة بتعهد وإعفاء معاً"));

        int? personFileId = null;
        if (agentId.HasValue)
        {
            var agent = await _unitOfWork.Agents.Query().FirstOrDefaultAsync(a => a.AgentID == agentId.Value);
            personFileId = agent?.FileID;
        }
        else if (entitledPersonId.HasValue)
        {
            var entitled = await _unitOfWork.EntitledPersons.Query().FirstOrDefaultAsync(e => e.EntitledID == entitledPersonId.Value);
            personFileId = entitled?.FileID;
        }
        else if (guardianId.HasValue)
        {
            var guardian = await _unitOfWork.Guardians.Query().FirstOrDefaultAsync(g => g.GuardianID == guardianId.Value);
            personFileId = guardian?.FileID;
        }
        else if (retireeId.HasValue)
        {
            var retFile = await _unitOfWork.RetirementFiles.Query()
                .Where(f => f.RetireeID == retireeId.Value && !f.IsDeleted &&
                    (!retirementFileId.HasValue || f.FileID == retirementFileId.Value))
                .OrderByDescending(f => f.CreatedAt)
                .FirstOrDefaultAsync();
            personFileId = retFile?.FileID;
        }

        var personSent = agentId.HasValue || entitledPersonId.HasValue || guardianId.HasValue || retireeId.HasValue;
        if (personSent && !personFileId.HasValue)
            return (null, ApiResponse<DocumentDto>.Fail("الشخص المحدد غير موجود أو لا يرتبط بملف تقاعد"));

        if (retirementFileId.HasValue)
        {
            if (personFileId.HasValue && personFileId.Value != retirementFileId.Value)
                return (null, ApiResponse<DocumentDto>.Fail("الشخص المحدد لا ينتمي إلى ملف التقاعد المُرسل"));

            if (pledgeId.HasValue)
            {
                var pledge = await _unitOfWork.DocumentPledges.Query().FirstOrDefaultAsync(p => p.PledgeID == pledgeId.Value);
                if (pledge == null)
                    return (null, ApiResponse<DocumentDto>.Fail("التعهد المحدد غير موجود"));
                if (pledge.FileID != retirementFileId.Value)
                    return (null, ApiResponse<DocumentDto>.Fail("التعهد المحدد لا ينتمي إلى ملف التقاعد المُرسل"));
            }

            if (exemptionId.HasValue)
            {
                var exemption = await _unitOfWork.DocumentExemptions.Query().FirstOrDefaultAsync(e => e.ExemptionID == exemptionId.Value);
                if (exemption == null)
                    return (null, ApiResponse<DocumentDto>.Fail("الإعفاء المحدد غير موجود"));
                if (exemption.FileID != retirementFileId.Value)
                    return (null, ApiResponse<DocumentDto>.Fail("الإعفاء المحدد لا ينتمي إلى ملف التقاعد المُرسل"));
            }

            return (retirementFileId, null);
        }

        int? fileId = currentFileId;

        if (personFileId.HasValue)
        {
            if (fileId.HasValue && fileId.Value != personFileId.Value)
                return (null, ApiResponse<DocumentDto>.Fail("لا يمكن نقل الوثيقة إلى ملف تقاعد آخر عبر تغيير المالك؛ حدّث ملف التقاعد صراحةً"));
            fileId = personFileId;
        }

        if (pledgeId.HasValue)
        {
            var pledge = await _unitOfWork.DocumentPledges.Query().FirstOrDefaultAsync(p => p.PledgeID == pledgeId.Value);
            if (pledge == null)
                return (null, ApiResponse<DocumentDto>.Fail("التعهد المحدد غير موجود"));
            if (fileId.HasValue && pledge.FileID != fileId.Value)
                return (null, ApiResponse<DocumentDto>.Fail("التعهد المحدد لا ينتمي إلى ملف الوثيقة"));
            fileId ??= pledge.FileID;
        }

        if (exemptionId.HasValue)
        {
            var exemption = await _unitOfWork.DocumentExemptions.Query().FirstOrDefaultAsync(e => e.ExemptionID == exemptionId.Value);
            if (exemption == null)
                return (null, ApiResponse<DocumentDto>.Fail("الإعفاء المحدد غير موجود"));
            if (fileId.HasValue && exemption.FileID != fileId.Value)
                return (null, ApiResponse<DocumentDto>.Fail("الإعفاء المحدد لا ينتمي إلى ملف الوثيقة"));
            fileId ??= exemption.FileID;
        }

        if ((personSent || pledgeId.HasValue || exemptionId.HasValue) && !fileId.HasValue)
            return (null, ApiResponse<DocumentDto>.Fail("تعذر تحديد ملف التقاعد التابع للمالك المحدد"));

        return (fileId, null);
    }

    private static async Task<string> ComputeSha256Async(Stream stream)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// م33: قيود حقول بيانات الوثيقة — تعيد رسالة فشل عربية أو null إذا كانت الحقول صالحة
    /// الحقول null تُتخطى (تعني عدم إرسالها في التعديل)
    /// </summary>
    private static ApiResponse<DocumentDto>? ValidateDocumentFields(
        string? notes, string? documentNumber, DateOnly? issueDate, DateOnly? expiryDate)
    {
        if (notes != null && notes.Length > 2000)
            return ApiResponse<DocumentDto>.Fail("الملاحظات تتجاوز الحد الأقصى (2000 حرف)");
        if (documentNumber != null && documentNumber.Length > 50)
            return ApiResponse<DocumentDto>.Fail("رقم الوثيقة يتجاوز الحد الأقصى (50 حرف)");
        if (issueDate.HasValue && expiryDate.HasValue && expiryDate.Value < issueDate.Value)
            return ApiResponse<DocumentDto>.Fail("تاريخ الانتهاء لا يمكن أن يسبق تاريخ الإصدار");
        return null;
    }

    /// <summary>
    /// م21/م23: إعادة حساب استيفاء نواقص (ملف + نوع + نطاق مالك خماسي) من الوثائق الفعلية المتبقية
    /// المسلّمة فقط تُعاد فتحها؛ المعفاة لا تُمس؛ صفوف التعهد تعود Pledged
    /// </summary>
    private async Task RecalculateMissingAsync(int? fileId, int documentTypeId)
    {
        if (!fileId.HasValue)
            return;

        var rows = await _unitOfWork.MissingDocuments.Query()
            .Where(m => m.FileID == fileId.Value && !m.IsDeleted
                     && m.DocumentTypeID == documentTypeId
                     && (m.Status == MissingDocumentStatusConstants.Fulfilled
                         || m.Status == MissingDocumentStatusConstants.Pending
                         || m.Status == MissingDocumentStatusConstants.Pledged))
            .ToListAsync();

        if (rows.Count == 0)
            return;

        var documents = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents).AsNoTracking()
            .Where(d => !d.IsDeleted
                        && d.FileID == fileId.Value
                        && d.DocumentTypeID == documentTypeId).ToListAsync();
        var requirements = RetirementFileDocumentRules.Requirements(
            await _missingDocumentService.GetRequiredPairsAsync(fileId.Value), rows);
        var today = PensionMS.Core.Time.YemenTime.Today;
        var uploaded = documents.Where(d => RetirementFileDocumentRules.IsValid(d, today))
            .Select(RetirementFileDocumentRules.Owner).ToHashSet();

        foreach (var row in rows)
        {
            row.Status = uploaded.Contains(RetirementFileDocumentRules.Owner(row, requirements))
                ? MissingDocumentStatusConstants.Fulfilled
                : row.PledgeID.HasValue && row.DocumentTypeID != DocumentTypeConstants.PledgeDocument
                    ? MissingDocumentStatusConstants.Pledged
                    : MissingDocumentStatusConstants.Pending;
        }
    }

    public Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId)
        => ExecuteDocumentChangeAsync(id, () => DeleteCoreAsync(id, deletedByUserId));

    private async Task<ApiResponse<bool>> DeleteCoreAsync(int id, int deletedByUserId)
    {
        var doc = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents).FirstOrDefaultAsync(d => d.DocumentID == id);
        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة غير موجودة");

        if (!await CanAccessOwnerAsync(doc))
            return ApiResponse<bool>.Fail("لا تملك صلاحية تعديل وثيقة في فرع آخر");

        var oldValues = new { doc.DocumentID, doc.DocumentTypeID, doc.FileName, doc.FilePath, doc.IsDeleted };

        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        doc.DeletedByUserID = deletedByUserId;

        _unitOfWork.Documents.Update(doc);
        var deleteTime = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        await _immutableAudit.LogAsync(deletedByUserId, "", "DeleteDocument", "Document", doc.DocumentID,
            System.Text.Json.JsonSerializer.Serialize(oldValues), null);

        // Decrement DocumentCount on parent entity if it's an operational expense
        if (doc.OperationalExpenseID.HasValue)
        {
            var expense = await _unitOfWork.OperationalExpenses.GetByIdAsync(doc.OperationalExpenseID.Value);
            if (expense != null && expense.DocumentCount > 0)
            {
                expense.DocumentCount--;
                expense.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.OperationalExpenses.Update(expense);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        if (doc.FileID.HasValue)
        {
            // م23: إعادة فتح النواقص التي كانت مستوفاة بهذه الوثيقة وفق المالك والنوع والوثائق المتبقية
            await RecalculateMissingAsync(doc.FileID, doc.DocumentTypeID);
            await _unitOfWork.SaveChangesAsync();

            await RefreshAfterDocumentChangeAsync([(doc.FileID, doc.EntitledID)], deletedByUserId, deleteTime);
        }

        return ApiResponse<bool>.Success(true, "تم حذف الوثيقة");
    }

    public Task<ApiResponse<DocumentDto>> UpdateAsync(int id, UpdateDocumentDto dto, int updatedByUserId)
        => ExecuteDocumentChangeAsync(id, () => UpdateCoreAsync(id, dto, updatedByUserId));

    private async Task<ApiResponse<DocumentDto>> UpdateCoreAsync(int id, UpdateDocumentDto dto, int updatedByUserId)
    {
        var doc = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.Retiree)
            .Include(d => d.Agent)
            .Include(d => d.Guardian)
            .Include(d => d.EntitledPerson)
            .Include(d => d.RetirementFile)
            .FirstOrDefaultAsync(d => d.DocumentID == id && !d.IsDeleted);

        if (doc == null)
            return ApiResponse<DocumentDto>.Fail("الوثيقة غير موجودة");

        if (!await CanAccessOwnerAsync(doc))
            return ApiResponse<DocumentDto>.Fail("لا تملك صلاحية تعديل وثيقة في فرع آخر");

        if (doc.Verified)
            return ApiResponse<DocumentDto>.Fail("لا يمكن تعديل وثيقة موثقة. يُرجى إلغاء التوثيق أولاً");

        var docType = await _unitOfWork.DocumentTypes.GetByIdAsync(dto.DocumentTypeID);
        if (docType == null)
            return ApiResponse<DocumentDto>.Fail("نوع الوثيقة غير صالح");

        // م19: فرض نشاط النوع
        if (!docType.IsActive)
            return ApiResponse<DocumentDto>.Fail($"نوع الوثيقة «{docType.TypeName}» غير نشط");

        // المالك لا يتغير إلا بإرسال أحد حقوله — ملاحظة وحدها لا تمسح المالك
        var ownerSent = dto.RetirementFileID.HasValue
            || dto.RetireeID.HasValue
            || dto.AgentID.HasValue
            || dto.GuardianID.HasValue
            || dto.EntitledPersonID.HasValue;

        // المالك المتوقع بعد التعديل: مُشتق من الحقول المُرسلة أو مالك الوثيقة الحالي
        var newOwnerType = dto.AgentID.HasValue ? DocumentOwnerType.Agent
                         : dto.GuardianID.HasValue ? DocumentOwnerType.Guardian
                         : dto.EntitledPersonID.HasValue ? DocumentOwnerType.EntitledPerson
                         : dto.RetireeID.HasValue ? DocumentOwnerType.Retiree
                         : DocumentOwnerType.RetirementFile;
        var effectiveOwnerType = ownerSent ? newOwnerType : doc.OwnerType;

        // م19: توافق نوع الوثيقة مع المالك
        if (docType.OwnerType.HasValue && docType.OwnerType.Value != effectiveOwnerType)
            return ApiResponse<DocumentDto>.Fail($"نوع الوثيقة «{docType.TypeName}» لا يسمح بالتعديل لهذا المالك");

        // م33: قيود حقول بيانات الوثيقة قبل أي تغيير أو استبدال ملف
        var updateFieldError = ValidateDocumentFields(dto.Notes, dto.DocumentNumber, dto.IssueDate, dto.ExpiryDate);
        if (updateFieldError != null)
            return updateFieldError;

        // م18: تحقق المالك ومنع النقل المخفي بين الملفات قبل أي تغيير أو استبدال ملف
        int? fileId = doc.FileID;
        if (ownerSent)
        {
            var (resolvedFileId, ownerError) = await ResolveOwnerFileAsync(
                dto.RetirementFileID, dto.RetireeID, dto.EntitledPersonID, dto.AgentID, dto.GuardianID,
                pledgeId: null, exemptionId: null, currentFileId: doc.FileID);
            if (ownerError != null)
                return ownerError;

            if (!await HasBranchAccessAsync(resolvedFileId))
                return ApiResponse<DocumentDto>.Fail("لا تملك صلاحية تعديل وثيقة في فرع آخر");

            fileId = resolvedFileId;
        }

        var oldTypeId = doc.DocumentTypeID;
        var oldFileId = doc.FileID;
        var oldAgentId = doc.AgentID;
        var oldGuardianId = doc.GuardianID;
        var oldEntitledId = doc.EntitledID;
        var oldExpiryDate = doc.ExpiryDate;
        var oldValues = new
        {
            doc.DocumentTypeID,
            doc.Notes,
            doc.DocumentNumber,
            doc.IssueDate,
            doc.ExpiryDate,
            doc.RetireeID,
            doc.EntitledID,
            doc.AgentID,
            doc.GuardianID,
            doc.FileName,
            doc.FilePath
        };

        // استبدال ملف الوثيقة (معالجة/قص) — الحفظ والتحقق عبر خدمة التخزين نفسها
        string? oldFilePath = null;
        if (dto.File != null && dto.File.Length > 0)
        {
            string newRelativePath;
            string newFileName;
            long newFileSize;
            string newContentType;
            string newContentHash;
            try
            {
                using var newStream = dto.File.OpenReadStream();
                newContentHash = await ComputeSha256Async(newStream);
                if (newStream.CanSeek) newStream.Position = 0;
                // م30: نفس سياسة نوع الوثيقة على الملف البديل
                var updatePolicyExtensions = !string.IsNullOrEmpty(docType.AllowedExtensions)
                    ? docType.AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim()).Where(e => e.Length > 0).ToArray()
                    : null;
                long? updatePolicyMaxBytes = docType.MaxFileSize.HasValue
                    ? (long)(docType.MaxFileSize.Value * 1024 * 1024)
                    : null;
                newRelativePath = await _fileStorage.SaveFileAsync(newStream, dto.File.FileName, "documents", updatePolicyExtensions, updatePolicyMaxBytes);
                newFileSize = dto.File.Length;
                newContentType = dto.File.ContentType;
                newFileName = dto.File.FileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في حفظ الملف البديل للوثيقة {DocumentId}", id);
                return ApiResponse<DocumentDto>.Fail("فشل في حفظ الملف. يرجى المحاولة لاحقاً");
            }

            oldFilePath = doc.FilePath;
            if (_unitOfWork.HasActiveTransaction)
                _unitOfWork.OnRollback(async () =>
                {
                    try { await _fileStorage.DeleteFileAsync(newRelativePath); }
                    catch (Exception ex) { _logger.LogError(ex, "تعذر تنظيف الملف البديل بعد التراجع: {Path}", newRelativePath); }
                });
            doc.FileName = newFileName;
            doc.FilePath = newRelativePath;
            doc.FileSize = newFileSize;
            doc.MimeType = newContentType;
            doc.Checksum = newContentHash;
        }

        doc.DocumentTypeID = dto.DocumentTypeID;

        // تحديث جزئي: null يعني أن الحقل لم يُرسل فيُبقى على حاله
        if (dto.Notes != null)
            doc.Notes = dto.Notes;
        if (dto.DocumentNumber != null)
            doc.DocumentNumber = dto.DocumentNumber;
        if (dto.IssueDate.HasValue)
            doc.IssueDate = dto.IssueDate;
        if (dto.ExpiryDate.HasValue)
            doc.ExpiryDate = dto.ExpiryDate;
        doc.UpdatedAt = DateTime.UtcNow;

        if (ownerSent)
        {
            doc.RetireeID = dto.RetireeID;
            doc.EntitledID = dto.EntitledPersonID;
            doc.AgentID = dto.AgentID;
            doc.GuardianID = dto.GuardianID;
            doc.FileID = fileId;
            doc.OwnerType = newOwnerType;
        }

        _unitOfWork.Documents.Update(doc);
        var updateTime = DateTime.UtcNow;
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // م32: فشل التثبيت لا يترك الملف البديل يتيمًا (الأصل لم يُحذف بعد)
            if (oldFilePath != null)
            {
                try { await _fileStorage.DeleteFileAsync(doc.FilePath); } catch { /* تجاهل */ }
            }
            _logger.LogError(ex, "فشل تثبيت تعديل الوثيقة {DocumentId}", id);
            return ApiResponse<DocumentDto>.Fail("فشل في حفظ تعديلات الوثيقة. يرجى المحاولة لاحقاً");
        }

        await _immutableAudit.LogAsync(updatedByUserId, "", "UpdateDocument", "Document", doc.DocumentID,
            System.Text.Json.JsonSerializer.Serialize(oldValues),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                doc.DocumentTypeID,
                doc.Notes,
                doc.DocumentNumber,
                doc.IssueDate,
                doc.ExpiryDate,
                doc.RetireeID,
                doc.EntitledID,
                doc.AgentID,
                doc.GuardianID,
                doc.FileName,
                doc.FilePath
            }));

        // الاحتفاظ بالأصل المعتمد ضمن سجل المراجعة بدلاً من حذفه عند التعديل.

        // إعادة حساب نواقص النوع عند تغيير المالك أو تاريخ الصلاحية، مع إبقاء المعفاة كما هي.
        var scopeChanged = oldTypeId != doc.DocumentTypeID
            || oldFileId != doc.FileID
            || oldAgentId != doc.AgentID
            || oldGuardianId != doc.GuardianID
            || oldEntitledId != doc.EntitledID;

        if (scopeChanged || oldExpiryDate != doc.ExpiryDate)
        {
            if (oldFileId != doc.FileID || oldTypeId != doc.DocumentTypeID)
                await RecalculateMissingAsync(oldFileId, oldTypeId);
            await RecalculateMissingAsync(doc.FileID, doc.DocumentTypeID);
            await _unitOfWork.SaveChangesAsync();
        }

        await RefreshAfterDocumentChangeAsync([(oldFileId, oldEntitledId), (doc.FileID, doc.EntitledID)],
            updatedByUserId, updateTime);

        return ApiResponse<DocumentDto>.Success(MapToDto(doc), "تم تحديث الوثيقة بنجاح");
    }

    public Task<ApiResponse<bool>> ReviewAsync(int id, ReviewDocumentDto dto, int reviewedByUserId)
        => ExecuteDocumentChangeAsync(id, () => ReviewCoreAsync(id, dto, reviewedByUserId));

    private async Task<ApiResponse<bool>> ReviewCoreAsync(int id, ReviewDocumentDto dto, int reviewedByUserId)
    {
        var doc = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .FirstOrDefaultAsync(d => d.DocumentID == id && !d.IsDeleted);

        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة غير موجودة");

        if (!await CanAccessOwnerAsync(doc))
            return ApiResponse<bool>.Fail("لا تملك صلاحية فحص وثيقة في فرع آخر");

        if (!Enum.IsDefined(typeof(PendingReviewStatus), dto.PendingReviewStatus))
            return ApiResponse<bool>.Fail("حالة الفحص المرسلة غير صالحة");

        var newStatus = (PendingReviewStatus)dto.PendingReviewStatus;

        if (newStatus == PendingReviewStatus.None)
            return ApiResponse<bool>.Fail("حالة الفحص المرسلة غير صالحة");

        dto.RejectionReason = newStatus == PendingReviewStatus.Rejected ? dto.RejectionReason?.Trim() : null;

        if (newStatus == PendingReviewStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
            return ApiResponse<bool>.Fail("سبب الرفض مطلوب عند رفض الوثيقة");

        if (doc.UploadedByUserID == reviewedByUserId)
            return ApiResponse<bool>.Fail("لا يمكنك فحص وثيقة رفعتها بنفسك");

        var oldStatus = doc.PendingReviewStatus;

        doc.PendingReviewStatus = newStatus;
        doc.ReviewNotes = dto.Notes;
        doc.RejectionReason = dto.RejectionReason;
        doc.ReviewedByUserID = reviewedByUserId;
        doc.ReviewedAt = DateTime.UtcNow;
        doc.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Documents.Update(doc);
        await _unitOfWork.SaveChangesAsync();

        await _immutableAudit.LogAsync(reviewedByUserId, "", "ReviewDocument", "Document", doc.DocumentID,
            System.Text.Json.JsonSerializer.Serialize(new { PendingReviewStatus = oldStatus.ToString() }),
            System.Text.Json.JsonSerializer.Serialize(new { PendingReviewStatus = newStatus.ToString(), dto.RejectionReason }));

        await RecalculateMissingAsync(doc.FileID, doc.DocumentTypeID);
        await _unitOfWork.SaveChangesAsync();
        await RefreshAfterDocumentChangeAsync([(doc.FileID, doc.EntitledID)], reviewedByUserId, DateTime.UtcNow);

        return ApiResponse<bool>.Success(true, "تم حفظ نتيجة الفحص بنجاح");
    }

    private Task<ApiResponse<T>> ExecuteDocumentChangeAsync<T>(int id, Func<Task<ApiResponse<T>>> change)
        => UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            var scope = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
                .Where(d => d.DocumentID == id).Select(d => new { d.FileID }).FirstOrDefaultAsync();
            if (scope == null) return ApiResponse<T>.Fail("الوثيقة غير موجودة");
            if (scope.FileID.HasValue) await _unitOfWork.LockFileReviewAsync(scope.FileID.Value);
            return await change();
        });

    private async Task RefreshAfterDocumentChangeAsync(IEnumerable<(int? FileId, int? EntitledId)> affected,
        int userId, DateTime changedAt)
    {
        var scopes = affected.Where(s => s.FileId.HasValue).Distinct().ToList();
        foreach (var id in scopes.Where(s => s.EntitledId.HasValue).Select(s => s.EntitledId!.Value).Distinct())
        {
            var result = await _entitlementEngine.ReEvaluateEntitlementStatusAsync(id);
            if (!result.IsSuccess) throw new InvalidOperationException(result.Message);
        }
        foreach (var fileId in scopes.Select(s => s.FileId!.Value).Distinct())
        {
            await _missingDocumentService.RefreshComplianceCacheAsync(fileId);
            var review = await _reviewService.RequestReviewAsync(fileId, userId, changedAt);
            if (!review.IsSuccess) throw new InvalidOperationException(review.Message);
        }
    }

    private async Task<ApiResponse<List<DocumentDto>>> GetByPredicateAsync(System.Linq.Expressions.Expression<Func<Document, bool>> predicate)
    {
        var docs = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .Include(d => d.PropertyUnit)
            .Where(predicate)
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var dtos = docs.Select(d => MapToDto(d)).ToList();
        return ApiResponse<List<DocumentDto>>.Success(dtos);
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetByInvestmentPortfolioIdAsync(int portfolioId) =>
        await GetByPredicateAsync(d => d.InvestmentPortfolioID == portfolioId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByInvestmentProjectIdAsync(int projectId) =>
        await GetByPredicateAsync(d => d.InvestmentProjectID == projectId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByInvestmentPropertyIdAsync(int propertyId)
    {
        var unitIds = await _unitOfWork.PropertyUnits.Query()
            .Where(u => u.PropertyID == propertyId && !u.IsDeleted)
            .Select(u => u.UnitID)
            .ToListAsync();

        var docs = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.Documents)
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .Include(d => d.PropertyUnit)
            .Where(d => (d.InvestmentPropertyID == propertyId || (d.PropertyUnitID.HasValue && unitIds.Contains(d.PropertyUnitID.Value))) 
                        && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var dtos = docs.Select(d => MapToDto(d)).ToList();
        return ApiResponse<List<DocumentDto>>.Success(dtos);
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetByPropertyUnitIdAsync(int unitId) =>
        await GetByPredicateAsync(d => d.PropertyUnitID == unitId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByLeaseContractIdAsync(int contractId) =>
        await GetByPredicateAsync(d => d.LeaseContractID == contractId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByTenantIdAsync(int tenantId) =>
        await GetByPredicateAsync(d => d.TenantID == tenantId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByAssetValuationIdAsync(int valuationId) =>
        await GetByPredicateAsync(d => d.AssetValuationID == valuationId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByMaintenanceRequestIdAsync(int requestId) =>
        await GetByPredicateAsync(d => d.MaintenanceRequestID == requestId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByWorkOrderIdAsync(int workOrderId) =>
        await GetByPredicateAsync(d => d.WorkOrderID == workOrderId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByDepreciationRecordIdAsync(int recordId) =>
        await GetByPredicateAsync(d => d.DepreciationRecordID == recordId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByPaymentScheduleIdAsync(int scheduleId) =>
        await GetByPredicateAsync(d => d.PaymentScheduleID == scheduleId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByInvestmentPlanIdAsync(int planId) =>
        await GetByPredicateAsync(d => d.InvestmentPlanID == planId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByRiskAssessmentIdAsync(int riskId) =>
        await GetByPredicateAsync(d => d.RiskAssessmentID == riskId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByEmployeeIdAsync(int employeeId) =>
        await GetByPredicateAsync(d => d.EmployeeID == employeeId);

    public async Task<ApiResponse<List<DocumentDto>>> GetByOperationalExpenseIdAsync(int expenseId) =>
        await GetByPredicateAsync(d => d.OperationalExpenseID == expenseId);

    private static DocumentDto MapToDto(Document d)
    {
        string? entityName = null;
        string? entityType = null;
        
        switch (d.OwnerType)
        {
            case DocumentOwnerType.Agent:
                entityName = d.Agent?.FullName;
                entityType = EntityTypeConstants.Agent;
                break;
            case DocumentOwnerType.Guardian:
                entityName = d.Guardian?.FullName;
                entityType = EntityTypeConstants.Guardian;
                break;
            case DocumentOwnerType.EntitledPerson:
                entityName = d.EntitledPerson?.FullName;
                entityType = EntityTypeConstants.EntitledPerson;
                break;
            case DocumentOwnerType.Retiree:
                entityName = d.Retiree?.FullName;
                entityType = EntityTypeConstants.Retiree;
                break;
            case DocumentOwnerType.DocumentPledge:
                entityType = EntityTypeConstants.Pledge;
                entityName = d.Pledge != null ? $"تعهد #{d.PledgeID}" : null;
                break;
            case DocumentOwnerType.DocumentExemption:
                entityType = EntityTypeConstants.Exemption;
                entityName = d.Exemption != null ? $"إعفاء #{d.ExemptionID}" : null;
                break;
            case DocumentOwnerType.RetirementFile:
            default:
                entityType = EntityTypeConstants.Retiree;
                entityName = d.Retiree?.FullName 
                          ?? d.RetirementFile?.Retiree?.FullName;
                break;
        }
        
        if (entityName == null && d.Agent != null) { entityName = d.Agent.FullName; entityType = EntityTypeConstants.Agent; }
        else if (entityName == null && d.Guardian != null) { entityName = d.Guardian.FullName; entityType = EntityTypeConstants.Guardian; }
        else if (entityName == null && d.EntitledPerson != null) { entityName = d.EntitledPerson.FullName; entityType = EntityTypeConstants.EntitledPerson; }
        else if (entityName == null && d.Retiree != null) { entityName = d.Retiree.FullName; entityType = EntityTypeConstants.Retiree; }
        else if (entityName == null && d.RetirementFile?.Retiree != null) { entityName = d.RetirementFile.Retiree.FullName; entityType = EntityTypeConstants.Retiree; }

        return new()
        {
            DocumentID = d.DocumentID,
            DocumentTypeID = d.DocumentTypeID,
            DocumentTypeName = d.DocumentType?.TypeName ?? "",
            FileName = d.FileName,
            FilePath = d.FilePath,
            FileExtension = Path.GetExtension(d.FileName),
            FileSize = d.FileSize,
            Notes = d.Notes,
            UploadedByUser = d.UploadedByUser?.FullName,
            EntityName = entityName,
            EntityType = entityType,
            CreatedAt = d.CreatedAt,
            IssueDate = d.IssueDate,
            ExpiryDate = d.ExpiryDate,
            DocumentNumber = d.DocumentNumber,
            Verified = d.Verified,
            HasExpiry = d.DocumentType?.HasExpiry ?? false,
            IsRequired = d.DocumentType?.IsRequired ?? false,
            AgentID = d.AgentID,
            GuardianID = d.GuardianID,
            RetireeID = d.RetireeID,
            EntitledID = d.EntitledID,
            InvestmentProjectID = d.InvestmentProjectID,
            InvestmentPropertyID = d.InvestmentPropertyID,
            PropertyUnitID = d.PropertyUnitID,
            LeaseContractID = d.LeaseContractID,
            TenantID = d.TenantID,
            InvestmentPortfolioID = d.InvestmentPortfolioID,
            MaintenanceRequestID = d.MaintenanceRequestID,
            WorkOrderID = d.WorkOrderID,
            PropertyUnitNumber = d.PropertyUnit?.UnitNumber,
            AssetValuationID = d.AssetValuationID,
            DepreciationRecordID = d.DepreciationRecordID,
            PaymentScheduleID = d.PaymentScheduleID,
            OperationalExpenseID = d.OperationalExpenseID,
            InvestmentPlanID = d.InvestmentPlanID,
            RiskAssessmentID = d.RiskAssessmentID,
            EmployeeID = d.EmployeeID,
            CorrespondenceID = d.CorrespondenceID,
            PendingReviewStatus = (int)d.PendingReviewStatus,
            OwnerType = (int)d.OwnerType,
            MimeType = d.MimeType,
            FileID = d.FileID,
            FileNumber = d.RetirementFile?.FileNumber,
            ReviewNotes = d.ReviewNotes,
            ReviewedBy = d.ReviewedByUser?.FullName,
            ReviewedAt = d.ReviewedAt,
            RejectionReason = d.RejectionReason
        };
    }
}
