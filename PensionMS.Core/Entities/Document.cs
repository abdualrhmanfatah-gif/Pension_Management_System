using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Entities.OperationalBudget;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Core.Entities;

/// <summary>
/// الوثائق والمستندات — المخزن المركزي لجميع الوثائق في النظام
/// </summary>
public class Document : ISoftDeletable
{
    public int DocumentID { get; set; }

    public int? RetireeID { get; set; }
    public virtual Retiree? Retiree { get; set; }

    public int? EntitledID { get; set; }
    public virtual EntitledPerson? EntitledPerson { get; set; }

    public int? FileID { get; set; }
    public virtual RetirementFile? RetirementFile { get; set; }

    public int? AgentID { get; set; }
    public virtual Agent? Agent { get; set; }

    public int? GuardianID { get; set; }
    public virtual Guardian? Guardian { get; set; }

    public int? PledgeID { get; set; }
    public virtual DocumentPledge? Pledge { get; set; }

    public int? ExemptionID { get; set; }
    public virtual DocumentExemption? Exemption { get; set; }

    public int? NoteID { get; set; }
    public virtual FileNote? Note { get; set; }

    public int? TrackingRecordID { get; set; }
    public virtual AchievementObstacle? TrackingRecord { get; set; }

    public int? SuspensionID { get; set; }
    public virtual PensionSuspension? Suspension { get; set; }

    public int? SalaryBatchID { get; set; }
    public virtual SalaryPaymentBatch? SalaryBatch { get; set; }

    public int? MinistryBatchID { get; set; }
    public virtual MinistryApprovalBatch? MinistryBatch { get; set; }

    public int? DecisionID { get; set; }
    public virtual SettlementDecision? Decision { get; set; }

    public int? SettlementBatchID { get; set; }
    public virtual SettlementsBatch? SettlementBatch { get; set; }

    public int? ReportID { get; set; }
    public virtual SavedReport? Report { get; set; }

    public int? InvestmentProjectID { get; set; }
    public virtual InvestmentProject? InvestmentProject { get; set; }

    public int? InvestmentPropertyID { get; set; }
    public virtual InvestmentProperty? InvestmentProperty { get; set; }

    public int? PropertyUnitID { get; set; }
    public virtual PropertyUnit? PropertyUnit { get; set; }

    public int? LeaseContractID { get; set; }
    public virtual LeaseContract? LeaseContract { get; set; }

    public int? TenantID { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int? InvestmentPortfolioID { get; set; }
    public virtual InvestmentPortfolio? InvestmentPortfolio { get; set; }

    public int? MaintenanceRequestID { get; set; }
    public virtual MaintenanceRequest? MaintenanceRequest { get; set; }

    public int? WorkOrderID { get; set; }
    public virtual WorkOrder? WorkOrder { get; set; }

    public int? AssetValuationID { get; set; }
    public virtual AssetValuation? AssetValuation { get; set; }

    public int? DepreciationRecordID { get; set; }
    public virtual DepreciationRecord? DepreciationRecord { get; set; }

    public int? PaymentScheduleID { get; set; }
    public virtual PaymentSchedule? PaymentSchedule { get; set; }

    public int? OperationalExpenseID { get; set; }
    public virtual OperationalExpense? OperationalExpense { get; set; }

    // Project Documents
    public int? InvestmentPlanID { get; set; }
    public virtual InvestmentPlan? InvestmentPlan { get; set; }

    public int? RiskAssessmentID { get; set; }
    public virtual InvestmentRiskAssessment? RiskAssessment { get; set; }

    public int? EmployeeID { get; set; }
    public virtual Employee? Employee { get; set; }

    public int? CorrespondenceID { get; set; }
    public virtual Correspondence? Correspondence { get; set; }

    public int? ServiceRequestID { get; set; }
    public virtual ServiceRequest? ServiceRequest { get; set; }

    public int? TicketMessageID { get; set; }
    public virtual TicketMessage? TicketMessage { get; set; }

    public int? CitizenAccountID { get; set; }
    public virtual CitizenAccount? CitizenAccount { get; set; }

    public int DocumentTypeID { get; set; }
    public virtual DocumentType DocumentType { get; set; } = null!;

    public DocumentOwnerType OwnerType { get; set; }

    public int? VerifiedByUserID { get; set; }
    public virtual User? VerifiedByUser { get; set; }

    public int UploadedByUserID { get; set; }
    public virtual User UploadedByUser { get; set; } = null!;

    public string? DocumentNumber { get; set; }
<<<<<<< HEAD
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
=======
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? Checksum { get; set; }
    public bool Verified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Notes { get; set; }

    public PendingReviewStatus PendingReviewStatus { get; set; } = PendingReviewStatus.None;
    public string? ReviewNotes { get; set; }
    public string? RejectionReason { get; set; }
    public int? ReviewedByUserID { get; set; }
    public virtual User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
