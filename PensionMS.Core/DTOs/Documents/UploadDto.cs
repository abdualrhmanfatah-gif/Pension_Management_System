using PensionMS.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.DTOs.Documents;

public class UploadDocumentDto
{
    [Required]
    public IRemoteFile File { get; set; } = null!;

    public IRemoteFile? FileBack { get; set; }

    [Required]
    public int DocumentTypeID { get; set; }

    public string? Notes { get; set; }
    public string? DocumentNumber { get; set; }

    public DateTime? IssueDate { get; set; }    public DateTime? ExpiryDate { get; set; }

    // Polymorphic parent (one is required)
    public int? RetirementFileID { get; set; }
    public int? RetireeID { get; set; }
    public int? EntitledPersonID { get; set; }
    public int? AgentID { get; set; }
    public int? GuardianID { get; set; }
    public int? PledgeID { get; set; }
    public int? ExemptionID { get; set; }

    // Investment Module
    public int? InvestmentProjectID { get; set; }
    public int? InvestmentPropertyID { get; set; }
    public int? PropertyUnitID { get; set; }
    public int? LeaseContractID { get; set; }
    public int? TenantID { get; set; }
    public int? InvestmentPortfolioID { get; set; }
    public int? MaintenanceRequestID { get; set; }
    public int? WorkOrderID { get; set; }
    public int? AssetValuationID { get; set; }
    public int? PropertyValuationID { get; set; } // Alias for AssetValuationID
    public int? DepreciationRecordID { get; set; }
    public int? PaymentScheduleID { get; set; }

    // Operational Budget Module
    public int? OperationalExpenseID { get; set; }

    // Project Documents
    public int? InvestmentPlanID { get; set; }
    public int? InvestmentTxnID { get; set; }
    public int? RiskAssessmentID { get; set; }

    // HR Module
    public int? EmployeeID { get; set; }
    public int? CorrespondenceID { get; set; }
}
