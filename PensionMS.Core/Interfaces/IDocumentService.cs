using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;

namespace PensionMS.Core.Interfaces;

public interface IDocumentService
{
    Task<ApiResponse<PagedResult<DocumentDto>>> GetAllAsync(
        int? ownerType,
        int? documentTypeId,
        DateTime? dateFrom,
        DateTime? dateTo,
        bool? verified,
        string? searchTerm,
        int? branchId,
        int? reviewStatus,
        int page,
        int pageSize);

    Task<ApiResponse<ArchiveStatsDto>> GetArchiveStatsAsync();

    Task<ApiResponse<DocumentDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<DocumentDto>>> GetByFileIdAsync(int fileId);
    Task<ApiResponse<List<DocumentDto>>> GetByRetireeIdAsync(int retireeId);
    
    // Investment Module
    Task<ApiResponse<List<DocumentDto>>> GetByInvestmentPortfolioIdAsync(int portfolioId);
    Task<ApiResponse<List<DocumentDto>>> GetByInvestmentProjectIdAsync(int projectId);
    Task<ApiResponse<List<DocumentDto>>> GetByInvestmentPropertyIdAsync(int propertyId);
    Task<ApiResponse<List<DocumentDto>>> GetByPropertyUnitIdAsync(int unitId);
    Task<ApiResponse<List<DocumentDto>>> GetByLeaseContractIdAsync(int contractId);
    Task<ApiResponse<List<DocumentDto>>> GetByTenantIdAsync(int tenantId);
    Task<ApiResponse<List<DocumentDto>>> GetByAssetValuationIdAsync(int valuationId);
    Task<ApiResponse<List<DocumentDto>>> GetByMaintenanceRequestIdAsync(int requestId);
    Task<ApiResponse<List<DocumentDto>>> GetByWorkOrderIdAsync(int workOrderId);
    Task<ApiResponse<List<DocumentDto>>> GetByDepreciationRecordIdAsync(int recordId);
    Task<ApiResponse<List<DocumentDto>>> GetByPaymentScheduleIdAsync(int scheduleId);
    
    // Project Documents
    Task<ApiResponse<List<DocumentDto>>> GetByInvestmentPlanIdAsync(int planId);
    Task<ApiResponse<List<DocumentDto>>> GetByRiskAssessmentIdAsync(int riskId);
    
    // Operational Budget Module
    Task<ApiResponse<List<DocumentDto>>> GetByOperationalExpenseIdAsync(int expenseId);

    // HR Module
    Task<ApiResponse<List<DocumentDto>>> GetByEmployeeIdAsync(int employeeId);
    Task<ApiResponse<List<DocumentDto>>> GetByCorrespondenceIdAsync(int correspondenceId);

    Task<ApiResponse<DocumentDto>> UploadAsync(UploadDocumentDto dto, int uploadedByUserId, string? requestId = null);
    Task<ApiResponse<DocumentDto>> UpdateAsync(int id, UpdateDocumentDto dto, int updatedByUserId);
    Task<ApiResponse<bool>> ReviewAsync(int id, ReviewDocumentDto dto, int reviewedByUserId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId);

    
}
