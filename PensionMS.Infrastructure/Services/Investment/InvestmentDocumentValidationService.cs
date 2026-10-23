using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentDocumentValidationService : IInvestmentDocumentValidationService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvestmentDocumentValidationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DocumentReadinessDto>> GetDocumentReadinessAsync(DocumentOwnerType ownerType, int entityId)
    {
        // 1. Get all document types for this owner type
        var requiredTypes = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.OwnerType == ownerType && dt.IsActive)
            .OrderBy(dt => dt.SortOrder)
            .ToListAsync();

        if (!requiredTypes.Any())
            return ApiResponse<DocumentReadinessDto>.Success(new DocumentReadinessDto(), "لا توجد أنواع وثائق مسجلة لهذا الكيان.");

        // 2. Get uploaded documents for this entity
        var uploadedDocs = await BuildEntityQuery(ownerType, entityId)
            .Select(d => new { d.DocumentTypeID, d.ExpiryDate, d.CreatedAt })
            .ToListAsync();

        var uploadedByType = uploadedDocs
            .GroupBy(d => d.DocumentTypeID)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First());

        // 3. Build readiness items
        var items = requiredTypes.Select(dt =>
        {
            var hasUpload = uploadedByType.TryGetValue(dt.DocumentTypeID, out var doc);
            var isExpired = hasUpload && dt.HasExpiry && doc!.ExpiryDate.HasValue && doc.ExpiryDate.Value < PensionMS.Core.Time.YemenTime.Today;

            return new DocumentReadinessItemDto
            {
                DocumentTypeID = dt.DocumentTypeID,
                TypeName = dt.TypeName,
                TypeNameEn = dt.TypeNameEn,
                SystemCode = dt.SystemCode,
                IsRequired = dt.IsRequired,
                HasExpiry = dt.HasExpiry,
                Status = !hasUpload ? "Missing" : isExpired ? "Expired" : "Provided",
                ExpiryDate = hasUpload ? doc!.ExpiryDate : null,
                UploadedAt = hasUpload ? doc!.CreatedAt : null
            };
        }).ToList();

        var totalRequired = items.Count(i => i.IsRequired);
        var totalProvided = items.Count(i => i.IsRequired && i.Status == DocumentStatusConstants.Provided);
        var totalMissing = items.Count(i => i.IsRequired && i.Status == DocumentStatusConstants.Missing);
        var totalExpired = items.Count(i => i.IsRequired && i.Status == DocumentStatusConstants.Expired);
        var result = new DocumentReadinessDto
        {
            TotalRequired = totalRequired,
            TotalProvided = totalProvided,
            TotalMissing = totalMissing,
            TotalExpired = totalExpired,
            CompletionPercentage = totalRequired > 0 ? Math.Round((decimal)totalProvided / totalRequired * 100, 1) : 100,
            Items = items
        };

        return ApiResponse<DocumentReadinessDto>.Success(result);
    }

    public async Task<ApiResponse<DocumentValidationResultDto>> ValidateReadinessForStatusChangeAsync(DocumentOwnerType ownerType, int entityId)
    {
        var readinessResult = await GetDocumentReadinessAsync(ownerType, entityId);
        if (!readinessResult.IsSuccess)
            return ApiResponse<DocumentValidationResultDto>.Fail(readinessResult.Message ?? "فشل التحقق من جاهزية الوثائق");

        var readiness = readinessResult.Data!;
        var missingDocs = readiness.Items.Where(i => i.IsRequired && i.Status == DocumentStatusConstants.Missing).ToList();
        var expiredDocs = readiness.Items.Where(i => i.IsRequired && i.Status == DocumentStatusConstants.Expired).ToList();
        var isReady = !missingDocs.Any() && !expiredDocs.Any();
        var message = isReady
            ? "جميع الوثائق الإلزامية مكتملة وصالحة. يمكن تغيير الحالة."
            : $"لا يمكن تغيير الحالة: {missingDocs.Count} وثيقة ناقصة، {expiredDocs.Count} وثيقة منتهية الصلاحية.";

        var result = new DocumentValidationResultDto
        {
            IsReady = isReady,
            MissingCount = missingDocs.Count,
            ExpiredCount = expiredDocs.Count,
            Message = message,
            MissingDocuments = missingDocs,
            ExpiredDocuments = expiredDocs
        };

        return ApiResponse<DocumentValidationResultDto>.Success(result, message);
    }

    private IQueryable<Document> BuildEntityQuery(DocumentOwnerType ownerType, int entityId)
    {
        var query = _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => d.OwnerType == ownerType && !d.IsDeleted);

        return ownerType switch
        {
            DocumentOwnerType.InvestmentProject => query.Where(d => d.InvestmentProjectID == entityId),
            DocumentOwnerType.InvestmentProperty => query.Where(d => d.InvestmentPropertyID == entityId),
            DocumentOwnerType.PropertyUnit => query.Where(d => d.PropertyUnitID == entityId),
            DocumentOwnerType.LeaseContract => query.Where(d => d.LeaseContractID == entityId),
            DocumentOwnerType.Tenant => query.Where(d => d.TenantID == entityId),
            DocumentOwnerType.InvestmentPortfolio => query.Where(d => d.InvestmentPortfolioID == entityId),
            DocumentOwnerType.MaintenanceRequest => query.Where(d => d.MaintenanceRequestID == entityId),
            DocumentOwnerType.WorkOrder => query.Where(d => d.WorkOrderID == entityId),
            _ => query.Where(d => false) // Unsupported type — returns empty
        };
    }
}
