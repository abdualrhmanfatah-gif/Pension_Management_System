using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.DTOs.Investment;

// ═══════════════════════════════════════
// LOOKUPS
// ═══════════════════════════════════════

public class InvestmentLookupDto
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool? IsCredit { get; set; }
}

// ═══════════════════════════════════════
// PORTFOLIO
// ═══════════════════════════════════════

public class CreatePortfolioDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int ManagerUserID { get; set; }
    public decimal TotalTargetCapital { get; set; }
}

public class UpdatePortfolioDto : CreatePortfolioDto { }

public class PortfolioDto
{
    public int PortfolioID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ManagerUserID { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public decimal TotalTargetCapital { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class PortfolioSummaryDto
{
    public int PortfolioID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public decimal TotalTargetCapital { get; set; }
    public decimal TotalPlannedBudget { get; set; }
    public decimal TotalActualSpent { get; set; }
    public decimal OccupancyRate { get; set; }
}

// ═══════════════════════════════════════
// PROJECT
// ═══════════════════════════════════════

public class CreateProjectDto
{
    public int PortfolioID { get; set; }
    public int TypeID { get; set; }
    public int PensionBranchID { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? FeasibilityStudyPath { get; set; }
    public string? BoardApprovalDocPath { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Planned";

    public decimal PlannedBudget { get; set; }
    public decimal ExpectedROI { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateProjectDto : CreateProjectDto { }

public class ProjectDto
{
    public int ProjectID { get; set; }
    public int PortfolioID { get; set; }
    public string PortfolioName { get; set; } = string.Empty;
    public int TypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FeasibilityStudyPath { get; set; }
    public string? BoardApprovalDocPath { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PlannedBudget { get; set; }
    public decimal ExpectedROI { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string? EndDate { get; set; }
}

public class ProjectSummaryDto
{
    public int ProjectID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal PlannedBudget { get; set; }
    public decimal TotalActualSpent { get; set; }
    public int PropertyCount { get; set; }
    public decimal ProgressPercentage { get; set; }
}

// ═══════════════════════════════════════
// PROPERTY
// ═══════════════════════════════════════

public class CreatePropertyDto
{
    public int? ProjectID { get; set; }
    public int PropertyTypeID { get; set; }
    public int PensionBranchID { get; set; }

    [Required]
    [MaxLength(200)]
    public string PropertyName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? LocationCity { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [MaxLength(500)]
    public string? GoogleMapsUrl { get; set; }

    public string? MainPhotoPath { get; set; }
    public string? PhotosJson { get; set; }
    public string? OwnershipDeedPath { get; set; }

    public decimal TotalArea { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public decimal InitialValue { get; set; }

    [MaxLength(100)]
    public string? TitleDeedNumber { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Ready";

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdatePropertyDto : CreatePropertyDto { }

public class PropertyDto
{
    public int PropertyID { get; set; }
    public int? ProjectID { get; set; }
    public string? ProjectName { get; set; }
    public int PropertyTypeID { get; set; }
    public string PropertyTypeName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string? LocationCity { get; set; }
    public string? MainPhotoPath { get; set; }
    public decimal TotalArea { get; set; }
    public decimal InitialValue { get; set; }
    public decimal CurrentMarketValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TitleDeedNumber { get; set; }
    public string? GoogleMapsUrl { get; set; }
}

public class PropertyDetailDto : PropertyDto
{
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? PhotosJson { get; set; }
    public string? OwnershipDeedPath { get; set; }
    public string AcquisitionDate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PropertyUnitDto> Units { get; set; } = new List<PropertyUnitDto>();
    public List<ValuationDto> Valuations { get; set; } = new List<ValuationDto>();
}

// ═══════════════════════════════════════
// PROPERTY UNIT
// ═══════════════════════════════════════

public class CreateUnitDto
{
    public int PropertyID { get; set; }
    public int UnitTypeID { get; set; }

    [Required]
    [MaxLength(50)]
    public string UnitNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Floor { get; set; }

    public decimal Area { get; set; }

    public string? MainPhotoPath { get; set; }
    public string? PhotosJson { get; set; }
    public string? LayoutPlanPath { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Available";
}

public class UpdateUnitDto : CreateUnitDto { }

public class PropertyUnitDto
{
    public int UnitID { get; set; }
    public int PropertyID { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public int UnitTypeID { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public string? Floor { get; set; }
    public decimal Area { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MainPhotoPath { get; set; }
}

// ═══════════════════════════════════════
// TENANT
// ═══════════════════════════════════════

public class CreateTenantDto
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TenantType { get; set; }

    [Required]
    [MaxLength(50)]
    public string NationalIDOrCR { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public decimal RatingScore { get; set; }
}

public class UpdateTenantDto : CreateTenantDto { }

public class TenantDto
{
    public int TenantID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? TenantType { get; set; }
    public string NationalIDOrCR { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal RatingScore { get; set; }
    public int ActiveContractsCount { get; set; }
}

// ═══════════════════════════════════════
// LEASE CONTRACT
// ═══════════════════════════════════════

public class CreateLeaseDto
{
    public int UnitID { get; set; }
    public int TenantID { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalContractValue { get; set; }

    [MaxLength(50)]
    public string? PaymentFrequency { get; set; }

    public string? RenewalTerms { get; set; }
    public string? SignedContractPath { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    /// <summary>
    /// توليد جدول الدفعات تلقائياً عند إنشاء العقد
    /// </summary>
    public bool GenerateSchedules { get; set; }
}

public class UpdateLeaseDto : CreateLeaseDto { }

public class LeaseContractDto
{
    public int ContractID { get; set; }
    public int UnitID { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public int TenantID { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; }
    public decimal TotalContractValue { get; set; }
    public string? PaymentFrequency { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SignedContractPath { get; set; }
}

// ═══════════════════════════════════════
// PAYMENT SCHEDULE
// ═══════════════════════════════════════

public class CreatePaymentScheduleDto
{
    public int ContractID { get; set; }
    public DateTime DueDate { get; set; }
    public decimal ExpectedAmount { get; set; }
}

public class PaymentScheduleDto
{
    public int ScheduleID { get; set; }
    public int ContractID { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public decimal ExpectedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdatePaymentStatusDto
{
    public decimal PaidAmount { get; set; }
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Paid";
}

// ═══════════════════════════════════════
// MAINTENANCE
// ═══════════════════════════════════════

public class CreateMaintenanceRequestDto
{
    public int PropertyID { get; set; }
    public int? UnitID { get; set; }
    public int? TenantID { get; set; }

    [MaxLength(50)]
    public string? Priority { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? DamagePhotosJson { get; set; }
    public DateTime ReportedDate { get; set; }
}

public class UpdateMaintenanceRequestDto
{
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class MaintenanceRequestDto
{
    public int RequestID { get; set; }
    public int PropertyID { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public int? UnitID { get; set; }
    public string? UnitNumber { get; set; }
    public int? TenantID { get; set; }
    public string? TenantName { get; set; }
    public string? Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReportedDate { get; set; } = string.Empty;
}

public class CreateWorkOrderDto
{
    public int RequestID { get; set; }

    [MaxLength(200)]
    public string? AssignedContractor { get; set; }

    public decimal EstimatedCost { get; set; }
}

public class UpdateWorkOrderDto
{
    public decimal ActualCost { get; set; }
    public DateTime CompletionDate { get; set; }
    public string? InvoiceFilePath { get; set; }
    public string? CompletionCertPath { get; set; }
}

public class WorkOrderDto
{
    public int WorkOrderID { get; set; }
    public int RequestID { get; set; }
    public string? AssignedContractor { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public string? CompletionDate { get; set; }
    public string? InvoiceFilePath { get; set; }
    public string? CompletionCertPath { get; set; }
}

// ═══════════════════════════════════════
// FINANCIAL TRANSACTION
// ═══════════════════════════════════════

public class CreateTransactionDto
{
    public int TxnTypeID { get; set; }
    public int? ProjectID { get; set; }

    [MaxLength(100)]
    public string? SourceType { get; set; }
    public int? SourceID { get; set; }

    public DateTime TxnDate { get; set; }
    public decimal Amount { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
}

public class TransactionDto
{
    public int TxnID { get; set; }
    public int TxnTypeID { get; set; }
    public string TxnTypeName { get; set; } = string.Empty;
    public bool IsCredit { get; set; }
    public int? ProjectID { get; set; }
    public string? ProjectName { get; set; }
    public string? SourceType { get; set; }
    public string TxnDate { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}

// ═══════════════════════════════════════
// ASSET VALUATION
// ═══════════════════════════════════════

public class CreateValuationDto
{
    public int PropertyID { get; set; }
    public DateTime ValuationDate { get; set; }
    public decimal MarketValue { get; set; }

    [MaxLength(200)]
    public string? ValuedBy { get; set; }
    public string? ValuationReportPath { get; set; }
}

public class ValuationDto
{
    public int ValuationID { get; set; }
    public int PropertyID { get; set; }
    public string ValuationDate { get; set; } = string.Empty;
    public decimal MarketValue { get; set; }
    public string? ValuedBy { get; set; }
    public string? ValuationReportPath { get; set; }
}

// ═══════════════════════════════════════
// DEPRECIATION
// ═══════════════════════════════════════

public class CreateDepreciationDto
{
    public int PropertyID { get; set; }
    public int FiscalYearID { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal NetBookValue { get; set; }
}

public class DepreciationRecordDto
{
    public int DeprecID { get; set; }
    public int PropertyID { get; set; }
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public decimal DepreciationRate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal NetBookValue { get; set; }
}

// ═══════════════════════════════════════
// CHART OF ACCOUNTS
// ═══════════════════════════════════════

public class CreateAccountDto
{
    [Required]
    [MaxLength(50)]
    public string AccountCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    public int AccountType { get; set; } // 1: Debit, 2: Credit, 3: Asset, 4: Liability
    public int? ParentAccountID { get; set; }
}

public class UpdateAccountDto : CreateAccountDto { }

public class AccountDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int AccountType { get; set; }
    public string AccountTypeName { get; set; } = string.Empty;
    public int? ParentAccountID { get; set; }
    public string? ParentAccountName { get; set; }
}

// ═══════════════════════════════════════
// JOURNAL ENTRY
// ═══════════════════════════════════════

public class CreateJournalEntryDto
{
    public int? TxnID { get; set; }
    public DateTime EntryDate { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new List<JournalEntryLineDto>();
}

public class JournalEntryLineDto
{
    public int AccountID { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class JournalEntryDto
{
    public int EntryID { get; set; }
    public int? TxnID { get; set; }
    public int AccountID { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string EntryDate { get; set; } = string.Empty;
}

// ═══════════════════════════════════════
// RISK ASSESSMENT
// ═══════════════════════════════════════

public class CreateRiskDto
{
    public int ProjectID { get; set; }

    [MaxLength(100)]
    public string? RiskType { get; set; }

    [MaxLength(500)]
    public string? RiskDescription { get; set; }

    public int RiskScore { get; set; }
    public string? MitigationPlan { get; set; }

    [MaxLength(200)]
    public string? ResponsiblePerson { get; set; }

    public DateTime AssessedDate { get; set; }
}

public class RiskAssessmentDto
{
    public int RiskID { get; set; }
    public int ProjectID { get; set; }
    public string? RiskType { get; set; }
    public string? RiskDescription { get; set; }
    public int RiskScore { get; set; }
    public string? MitigationPlan { get; set; }
    public string? ResponsiblePerson { get; set; }
    public string AssessedDate { get; set; } = string.Empty;
}

// ═══════════════════════════════════════
// INVESTMENT PLAN
// ═══════════════════════════════════════

public class CreateInvestmentPlanDto
{
    public int ProjectID { get; set; }
    public int FiscalYearID { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal ActualSpent { get; set; }
}

public class UpdateInvestmentPlanDto : CreateInvestmentPlanDto { }

public class InvestmentPlanDto
{
    public int PlanID { get; set; }
    public int ProjectID { get; set; }
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public decimal PlannedAmount { get; set; }
    public decimal ActualSpent { get; set; }
}

// ═══════════════════════════════════════
// APPROVAL REQUEST
// ═══════════════════════════════════════

public class CreateApprovalDto
{
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    public int EntityID { get; set; }

    [MaxLength(100)]
    public string? WorkflowStage { get; set; }
}

public class UpdateApprovalDto
{
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
    public string? DecisionNote { get; set; }
}

public class ApprovalRequestDto
{
    public int ApprovalID { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityID { get; set; }
    public int RequestedByUserID { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public string? WorkflowStage { get; set; }
    public string? DecisionHistory { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
