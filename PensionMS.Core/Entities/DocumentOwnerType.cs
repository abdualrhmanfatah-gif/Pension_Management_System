namespace PensionMS.Core.Entities;

/// <summary>
/// أنواع مالكي الوثائق — يُستخدم لتحديد السياق الذي تنتمي إليه الوثيقة
/// </summary>
public enum DocumentOwnerType
{
    RetirementFile    = 1,
    Retiree           = 2,
    EntitledPerson    = 3,
    Agent             = 4,
    Guardian          = 5,
    DocumentPledge    = 6,
    DocumentExemption = 7,
    FileNote          = 8,
    PensionSuspension = 9,
    SalaryPaymentBatch    = 10,
    MinistryApprovalBatch = 11,
    SettlementDecision    = 12,
    SettlementsBatch      = 13,
    SavedReport           = 14,
    InvestmentProject     = 15,
    InvestmentProperty    = 16,
    PropertyUnit          = 17,
    LeaseContract         = 18,
    Tenant                = 19,
    InvestmentPortfolio   = 20,
    MaintenanceRequest    = 21,
    WorkOrder             = 22,
    InvestmentFinance     = 23,
    InvestmentGovernance  = 24,
    PropertyValuation     = 25,
    FeasibilityStudy      = 26,
    PropertyDepreciation  = 27,
    LeasePayment          = 28,
    OperationalExpense    = 30,

    // Project Documents
    InvestmentPlan        = 31,
    InvestmentTxn         = 32,
    RiskAssessment        = 33,
    Employee              = 40,
    Correspondence        = 41,

    // Citizen Portal
    ServiceRequest        = 50,
    TicketMessage         = 51,
    CitizenAccount        = 52
}
