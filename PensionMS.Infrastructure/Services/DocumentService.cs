using System.IO;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Helpers;

namespace PensionMS.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IComplianceEngine _complianceEngine;
    private readonly IEntitlementEngine _entitlementEngine;
    private readonly IFileReviewService _reviewService;

    public DocumentService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, IComplianceEngine complianceEngine, IEntitlementEngine entitlementEngine, IFileReviewService reviewService)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _complianceEngine = complianceEngine;
        _entitlementEngine = entitlementEngine;
        _reviewService = reviewService;
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
        var query = _unitOfWork.Documents.Query()
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
        var docQuery = _unitOfWork.Documents.Query().Where(d => !d.IsDeleted);

        var totalDocuments = await docQuery.CountAsync();
        var verifiedDocuments = await docQuery.CountAsync(d => d.Verified);
        var pendingReview = await docQuery.CountAsync(d => d.PendingReviewStatus == PendingReviewStatus.Pending);
        var expiredDocuments = await docQuery.CountAsync(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < DateTime.UtcNow);

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
        var doc = await _unitOfWork.Documents.Query()
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.DocumentID == id);

        if (doc == null)
            return ApiResponse<DocumentDto>.Fail("الوثيقة غير موجودة");

        return ApiResponse<DocumentDto>.Success(MapToDto(doc));
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetByFileIdAsync(int fileId)
    {
        var docs = await _unitOfWork.Documents.Query()
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

    public async Task<ApiResponse<List<DocumentDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var docs = await _unitOfWork.Documents.Query()
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
        var docs = await _unitOfWork.Documents.Query()
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedByUser)
            .Where(d => d.CorrespondenceID == correspondenceId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var dtos = docs.Select(d => MapToDto(d)).ToList();
        return ApiResponse<List<DocumentDto>>.Success(dtos);
    }

    public async Task<ApiResponse<DocumentDto>> UploadAsync(UploadDocumentDto dto, int uploadedByUserId, string? requestId = null)
    {
        var file = dto.File;
        if (file == null || file.Length == 0)
            return ApiResponse<DocumentDto>.Fail("لم يتم اختيار ملف");

        var docType = await _unitOfWork.DocumentTypes.GetByIdAsync(dto.DocumentTypeID);
        if (docType == null) return ApiResponse<DocumentDto>.Fail("نوع الوثيقة غير صالح");

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

        int? fileId = dto.RetirementFileID;

        if (!fileId.HasValue && dto.AgentID.HasValue)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(dto.AgentID.Value);
            if (agent != null) fileId = agent.FileID;
        }
        else if (!fileId.HasValue && dto.EntitledPersonID.HasValue)
        {
            var entitled = await _unitOfWork.EntitledPersons.GetByIdAsync(dto.EntitledPersonID.Value);
            if (entitled != null) fileId = entitled.FileID;
        }
        else if (!fileId.HasValue && dto.GuardianID.HasValue)
        {
            var guardian = await _unitOfWork.Guardians.GetByIdAsync(dto.GuardianID.Value);
            if (guardian != null) fileId = guardian.FileID;
        }
        else if (!fileId.HasValue && dto.PledgeID.HasValue)
        {
            var pledge = await _unitOfWork.DocumentPledges.GetByIdAsync(dto.PledgeID.Value);
            if (pledge != null) fileId = pledge.FileID;
        }
        else if (!fileId.HasValue && dto.ExemptionID.HasValue)
        {
            var exemption = await _unitOfWork.DocumentExemptions.GetByIdAsync(dto.ExemptionID.Value);
            if (exemption != null) fileId = exemption.FileID;
        }
        else if (!fileId.HasValue && dto.RetireeID.HasValue)
        {
            var retFile = await _unitOfWork.RetirementFiles.Query()
                .Where(f => f.RetireeID == dto.RetireeID.Value && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .FirstOrDefaultAsync();
            if (retFile != null) fileId = retFile.FileID;
        }

        string relativePath;
        long fileSize;
        string contentType;
        string fileName = file.FileName;

        try
        {
            if (dto.DocumentTypeID == PensionMS.Core.Constants.DocumentTypeConstants.NationalId && (dto.FileBack != null))
            {
                var (stream, mergedContentType, extension) = await ImageMergeHelper.MergeImagesToPdfAsync(dto.File, dto.FileBack);
                if (stream != Stream.Null)
                {
                    fileName = $"NationalID_Unified_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
                    relativePath = await _fileStorage.SaveFileAsync(stream, fileName, "documents");
                    fileSize = stream.Length;
                    contentType = mergedContentType;
                }
                else
                {
                    using var originalStream = file.OpenReadStream();
                    relativePath = await _fileStorage.SaveFileAsync(originalStream, file.FileName, "documents");
                    fileSize = file.Length;
                    contentType = file.ContentType;
                }
            }
            else
            {
                using var stream = file.OpenReadStream();
                relativePath = await _fileStorage.SaveFileAsync(stream, file.FileName, "documents");
                fileSize = file.Length;
                contentType = file.ContentType;
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<DocumentDto>.Fail("فشل في حفظ الملف. يرجى المحاولة لاحقاً");
        }

        // ─── منع التكرار: فحص وثيقة مماثلة محفوظة مسبقاً ───
        if (fileId.HasValue)
        {
            var duplicate = await _unitOfWork.Documents.Query()
                .Where(d => !d.IsDeleted
                    && d.FileID == fileId.Value
                    && d.DocumentTypeID == dto.DocumentTypeID
                    && d.FileName == fileName
                    && d.FileSize == fileSize)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync();

            if (duplicate != null)
            {
                // حذف الملف المحفوظ على القرص لأنه مكرر
                try { await _fileStorage.DeleteFileAsync(relativePath); } catch { /* تجاهل */ }
                throw new InvalidOperationException("DUPLICATE_DOCUMENT");
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

        await _unitOfWork.Documents.AddAsync(doc);
        var uploadTime = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // ─── المعالجة اللاحقة داخل try/catch منفصل ───
        // الوثيقة محفوظة بالفعل. فشل المعالجة اللاحقة لا يلغي الحفظ.
        try
        {
            // Update DocumentCount on parent entity if it's an operational expense
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

            if (fileId.HasValue)
            {
                var query = _unitOfWork.MissingDocuments.Query()
                    .Where(m => m.FileID == fileId!.Value
                              && m.DocumentTypeID == dto.DocumentTypeID
                              && (m.Status == MissingDocumentStatusConstants.Pending || m.Status == MissingDocumentStatusConstants.Pledged));

                if (dto.AgentID.HasValue)
                    query = query.Where(m => m.AgentID == dto.AgentID.Value);
                else if (dto.GuardianID.HasValue)
                    query = query.Where(m => m.GuardianID == dto.GuardianID.Value);
                else if (dto.EntitledPersonID.HasValue)
                    query = query.Where(m => m.EntitledID == dto.EntitledPersonID.Value);

                var pendingMissing = await query.ToListAsync();

                var affectedPledgeIds = pendingMissing
                    .Where(m => m.PledgeID.HasValue)
                    .Select(m => m.PledgeID.Value)
                    .Distinct()
                    .ToList();

                foreach (var m in pendingMissing)
                    m.Status = MissingDocumentStatusConstants.Fulfilled;

                if (pendingMissing.Count > 0)
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
            }

            if (fileId.HasValue)
            {
                await _complianceEngine.RefreshComplianceCacheAsync(fileId.Value);

                // إذا كانت الوثيقة تخص مستحق -> إعادة تقييم حالته تلقائياً
                if (dto.EntitledPersonID.HasValue)
                {
                    await _entitlementEngine.ReEvaluateEntitlementStatusAsync(dto.EntitledPersonID.Value);
                }
            }

            if (fileId.HasValue)
                await _reviewService.RequestReviewAsync(fileId.Value, uploadedByUserId, uploadTime);
        }
        catch (Exception ex)
        {
            // المعالجة اللاحقة فشلت لكن الوثيقة محفوظة بالفعل
            // نسجل الخطأ في السجلات ولا نرمي Exception
            System.Diagnostics.Debug.WriteLine(
                $"[DocumentService] Post-save processing failed for doc {doc.DocumentID} (requestId={requestId}): {ex.Message}");
        }

        await _unitOfWork.SaveChangesAsync(); // حفظ أي تعديلات متبقية من المعالجة اللاحقة

        return ApiResponse<DocumentDto>.Success(MapToDto(doc), "تم رفع الوثيقة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(id);
        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة غير موجودة");

        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        doc.DeletedByUserID = deletedByUserId;

        _unitOfWork.Documents.Update(doc);
        var deleteTime = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

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
            await _complianceEngine.RefreshComplianceCacheAsync(doc.FileID.Value);
            await _reviewService.RequestReviewAsync(doc.FileID.Value, deletedByUserId, deleteTime);
        }

        return ApiResponse<bool>.Success(true, "تم حذف الوثيقة");
    }

    public async Task<ApiResponse<DocumentDto>> UpdateAsync(int id, UpdateDocumentDto dto, int updatedByUserId)
    {
        var doc = await _unitOfWork.Documents.Query()
            .Include(d => d.DocumentType)
            .Include(d => d.Retiree)
            .Include(d => d.Agent)
            .Include(d => d.Guardian)
            .Include(d => d.EntitledPerson)
            .Include(d => d.RetirementFile)
            .FirstOrDefaultAsync(d => d.DocumentID == id && !d.IsDeleted);

        if (doc == null)
            return ApiResponse<DocumentDto>.Fail("الوثيقة غير موجودة");

        if (doc.Verified)
            return ApiResponse<DocumentDto>.Fail("لا يمكن تعديل وثيقة موثقة. يُرجى إلغاء التوثيق أولاً");

        var docType = await _unitOfWork.DocumentTypes.GetByIdAsync(dto.DocumentTypeID);
        if (docType == null)
            return ApiResponse<DocumentDto>.Fail("نوع الوثيقة غير صالح");

        var oldTypeId = doc.DocumentTypeID;
        var fileId = doc.FileID;

        doc.DocumentTypeID = dto.DocumentTypeID;
        doc.Notes = dto.Notes;
        doc.DocumentNumber = dto.DocumentNumber;
        doc.IssueDate = dto.IssueDate;
        doc.ExpiryDate = dto.ExpiryDate;
        doc.UpdatedAt = DateTime.UtcNow;

        doc.RetireeID = null;
        doc.EntitledID = null;
        doc.AgentID = null;
        doc.GuardianID = null;

        if (dto.RetireeID.HasValue)
            doc.RetireeID = dto.RetireeID;
        else if (dto.AgentID.HasValue)
        {
            doc.AgentID = dto.AgentID;
            if (!fileId.HasValue)
            {
                var agent = await _unitOfWork.Agents.GetByIdAsync(dto.AgentID.Value);
                if (agent != null) fileId = agent.FileID;
            }
        }
        else if (dto.GuardianID.HasValue)
        {
            doc.GuardianID = dto.GuardianID;
            if (!fileId.HasValue)
            {
                var guardian = await _unitOfWork.Guardians.GetByIdAsync(dto.GuardianID.Value);
                if (guardian != null) fileId = guardian.FileID;
            }
        }
        else if (dto.EntitledPersonID.HasValue)
        {
            doc.EntitledID = dto.EntitledPersonID;
            if (!fileId.HasValue)
            {
                var entitled = await _unitOfWork.EntitledPersons.GetByIdAsync(dto.EntitledPersonID.Value);
                if (entitled != null) fileId = entitled.FileID;
            }
        }

        doc.FileID = fileId;
        doc.OwnerType = doc.AgentID.HasValue ? DocumentOwnerType.Agent
                       : doc.GuardianID.HasValue ? DocumentOwnerType.Guardian
                       : doc.EntitledID.HasValue ? DocumentOwnerType.EntitledPerson
                       : doc.RetireeID.HasValue ? DocumentOwnerType.Retiree
                       : DocumentOwnerType.RetirementFile;

        _unitOfWork.Documents.Update(doc);
        var updateTime = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        if (oldTypeId != dto.DocumentTypeID && fileId.HasValue)
        {
            var oldMissing = await _unitOfWork.MissingDocuments.Query()
                .Where(m => m.FileID == fileId.Value
                          && m.DocumentTypeID == oldTypeId
                          && m.Status == MissingDocumentStatusConstants.Fulfilled)
                .ToListAsync();

            foreach (var m in oldMissing)
                m.Status = MissingDocumentStatusConstants.Pending;

            var newMissing = await _unitOfWork.MissingDocuments.Query()
                .Where(m => m.FileID == fileId.Value
                          && m.DocumentTypeID == dto.DocumentTypeID
                          && m.Status == MissingDocumentStatusConstants.Pending)
                .ToListAsync();

            foreach (var m in newMissing)
                m.Status = MissingDocumentStatusConstants.Fulfilled;

            if (oldMissing.Count > 0 || newMissing.Count > 0)
                await _unitOfWork.SaveChangesAsync();
        }

        if (fileId.HasValue)
        {
            await _complianceEngine.RefreshComplianceCacheAsync(fileId.Value);
            await _reviewService.RequestReviewAsync(fileId.Value, updatedByUserId, updateTime);
        }

        return ApiResponse<DocumentDto>.Success(MapToDto(doc), "تم تحديث الوثيقة بنجاح");
    }

    public async Task<ApiResponse<bool>> ReviewAsync(int id, ReviewDocumentDto dto, int reviewedByUserId)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(id);
        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة غير موجودة");

        doc.PendingReviewStatus = (PendingReviewStatus)dto.PendingReviewStatus;
        doc.ReviewNotes = dto.Notes;
        doc.RejectionReason = dto.RejectionReason;
        doc.ReviewedByUserID = reviewedByUserId;
        doc.ReviewedAt = DateTime.UtcNow;
        doc.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Documents.Update(doc);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حفظ نتيجة الفحص بنجاح");
    }

    private async Task<ApiResponse<List<DocumentDto>>> GetByPredicateAsync(System.Linq.Expressions.Expression<Func<Document, bool>> predicate)
    {
        var docs = await _unitOfWork.Documents.Query()
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

        var docs = await _unitOfWork.Documents.Query()
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
