using System;

namespace PensionMS.Core.DTOs.Reports;

// ═══════════════════════════════════════
// INV-01: Portfolio Performance
// ═══════════════════════════════════════
public class PortfolioPerformanceRow
{
    public int PortfolioID { get; set; }
    public string PortfolioName { get; set; } = "";
    public decimal TotalInvested { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal ProfitLoss => CurrentValue - TotalInvested;
    public double ROI => TotalInvested > 0 ? (double)((CurrentValue - TotalInvested) / TotalInvested * 100) : 0;
    public int ActiveProjects { get; set; }
    public int TotalProjects { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
}

// ═══════════════════════════════════════
// INV-02: Occupancy & Vacancy
// ═══════════════════════════════════════
public class OccupancyRow
{
    public int PropertyID { get; set; }
    public string PropertyName { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public int VacantUnits { get; set; }
    public int UnderMaintenance { get; set; }
    public double OccupancyRate { get; set; } // %
    public decimal EstimatedLostRevenue { get; set; }
}

// ═══════════════════════════════════════
// INV-03: Rent Collection & Receivables
// ═══════════════════════════════════════
public class RentCollectionRow
{
    public int ContractID { get; set; }
    public string TenantName { get; set; } = "";
    public string PropertyName { get; set; } = "";
    public string UnitNumber { get; set; } = "";
    public decimal DueAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount => DueAmount - PaidAmount;
    public int OverdueDays { get; set; }
    public string PaymentStatus { get; set; } = ""; // Paid, Partial, Overdue
}

// ═══════════════════════════════════════
// INV-04: Active & Expiring Contracts
// ═══════════════════════════════════════
public class ContractStatusRow
{
    public int ContractID { get; set; }
    public string TenantName { get; set; } = "";
    public string PropertyName { get; set; } = "";
    public string UnitNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal AnnualRent { get; set; }
    public int DaysToExpiry { get; set; }
}

// ═══════════════════════════════════════
// INV-05: Risk Exposure
// ═══════════════════════════════════════
public class RiskExposureRow
{
    public int RiskID { get; set; }
    public string ProjectName { get; set; } = "";
    public string RiskType { get; set; } = "";
    public string Severity { get; set; } = ""; // Critical, High, Medium, Low
    public string Status { get; set; } = "";
    public string MitigationPlan { get; set; } = "";
    public DateTime? IdentifiedDate { get; set; }
}

// ═══════════════════════════════════════
// INV-06: Maintenance & Work Orders
// ═══════════════════════════════════════
public class MaintenanceSummaryRow
{
    public int RequestID { get; set; }
    public string PropertyName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public int ResponseDays { get; set; }
    public DateTime ReportedDate { get; set; }
}

// ═══════════════════════════════════════
// INV-07: Budget vs Actual
// ═══════════════════════════════════════
public class BudgetVsActualRow
{
    public int PlanID { get; set; }
    public string ProjectName { get; set; } = "";
    public int FiscalYear { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal ActualSpent { get; set; }
    public decimal Variance => PlannedAmount - ActualSpent;
    public double VariancePct => PlannedAmount > 0 ? (double)(Variance / PlannedAmount * 100) : 0;
}

// ═══════════════════════════════════════
// RET-01: File Status
// ═══════════════════════════════════════
public class FileStatusRow
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = "";
    public string RetireeName { get; set; } = "";
    public string RankName { get; set; } = "";
    public string Status { get; set; } = "";
    public string BranchName { get; set; } = "";
    public int DaysPending { get; set; }
    public int MissingDocsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ═══════════════════════════════════════
// RET-02: Active Retirees
// ═══════════════════════════════════════
<<<<<<< HEAD
public class ActiveRetireesSummaryDto
{
    public int TotalCount { get; set; }
    public decimal TotalPension { get; set; }
    public decimal AvgPension { get; set; }
    public int GovernorateCount { get; set; }
}

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
public class ActiveRetireeRow
{
    public int RetireeID { get; set; }
    public string FullName { get; set; } = "";
    public string NationalID { get; set; } = "";
<<<<<<< HEAD
    public string MilitaryNumber { get; set; } = "";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string RankName { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal MonthlyPension { get; set; }
    public string Status { get; set; } = "";
    public string Governorate { get; set; } = "";
}

// ═══════════════════════════════════════
// RET-03: Suspensions
// ═══════════════════════════════════════
public class SuspensionRow
{
    public int SuspensionID { get; set; }
    public string RetireeName { get; set; } = "";
    public string FileNumber { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateTime SuspendedDate { get; set; }
    public DateTime? LiftedDate { get; set; }
    public int DaysSuspended { get; set; }
    public bool IsActive { get; set; }
}

// ═══════════════════════════════════════
// FIN-01: Salary Batches
// ═══════════════════════════════════════
public class SalaryBatchRow
{
    public int BatchID { get; set; }
    public string Period { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public int RecipientCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

// ═══════════════════════════════════════
// FIN-02: Settlements
// ═══════════════════════════════════════
public class SettlementRow
{
    public int SettlementID { get; set; }
<<<<<<< HEAD
    public int DecisionID { get; set; }
    public string DecisionNumber { get; set; } = "";
    public string RetireeName { get; set; } = "";
    public string MilitaryNumber { get; set; } = "";
    public string PreviousRank { get; set; } = "";
    public string ProcessedRank { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime DecisionDate { get; set; }
}

public class SettlementSummaryDto
{
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public int DecisionCount { get; set; }
    public decimal AverageAmount { get; set; }
=======
    public string DecisionNumber { get; set; } = "";
    public string RetireeName { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}

// ═══════════════════════════════════════
// FIN-03: Fiscal Period
// ═══════════════════════════════════════
public class FiscalPeriodRow
{
    public string ChapterName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string Period { get; set; } = "";
    public decimal Allocated { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => Allocated - Spent;
    public double SpentPct => Allocated > 0 ? (double)(Spent / Allocated * 100) : 0;
    public decimal Carryover { get; set; }
}

// ═══════════════════════════════════════
// DOC-01: Missing Documents (grouped by file)
// ═══════════════════════════════════════
public class FileMissingDocsRow
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = "";
    public string RetireeName { get; set; } = "";
    public string? MilitaryNumber { get; set; }
    public string? Phone { get; set; }
    public string? AgentPhone { get; set; }
    public int MissingCount { get; set; }
    public string MissingDocsList { get; set; } = "";
    public int MaxDaysMissing { get; set; }
}

// ═══════════════════════════════════════
// DOC-01: Missing Documents per person (beneficiaries/heirs)
// ═══════════════════════════════════════
public class MissingDocsByPersonRow
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = "";
    public string RetireeName { get; set; } = "";
    public string? MilitaryNumber { get; set; }
    public string PersonType { get; set; } = "";  // متقاعد / مستحق / وصي / وكيل
    public string PersonName { get; set; } = "";
    public string? Relationship { get; set; } = "";  // صلة القرابة
    public string? PersonPhone { get; set; }
    public int MissingCount { get; set; }
    public string MissingDocsList { get; set; } = "";
    public int MaxDaysMissing { get; set; }
}

// Backward-compatible flat row (used internally)
public class MissingDocRow
{
    public int FileID { get; set; }
    public string EntityType { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string DocTypeName { get; set; } = "";
    public bool IsRequired { get; set; }
    public int DaysMissing { get; set; }
    public string FileNumber { get; set; } = "";
    public string RetireeName { get; set; } = "";
    public string? MilitaryNumber { get; set; }
    public string? Phone { get; set; }
    public string? AgentPhone { get; set; }
}

// ═══════════════════════════════════════
// DOC-02: Expired Documents
// ═══════════════════════════════════════
public class ExpiredDocRow
{
    public int DocumentID { get; set; }
    public string DocTypeName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string OwnerType { get; set; } = "";
<<<<<<< HEAD
    public DateOnly? ExpiryDate { get; set; }
=======
    public DateTime? ExpiryDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public int DaysExpired { get; set; }
    public bool IsVerified { get; set; }
}

// ═══════════════════════════════════════
// DEP-01: Beneficiaries & Guardians
// ═══════════════════════════════════════
public class BeneficiaryRow
{
    public int FileID { get; set; }
    public int PersonID { get; set; }
    public string FullName { get; set; } = "";
    public string NationalID { get; set; } = "";
    public string Relationship { get; set; } = "";
    public string GuardianName { get; set; } = "";
    public string FileNumber { get; set; } = "";
    public decimal MonthlyAmount { get; set; }
    public string? RetireeName { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? PersonPhone { get; set; }
    public string PersonType { get; set; } = "";  // مستحق / وصي
}

// ═══════════════════════════════════════
<<<<<<< HEAD
// KASHF: Kashf Report (كشوفات الإحالة)
// ═══════════════════════════════════════
public class KashfReportRow
{
    public int ApprovalItemID { get; set; }
    public int? FileID { get; set; }
    public string MilitaryNumber { get; set; } = "";
    public string FullName { get; set; } = "";
    public string RankName { get; set; } = "";
    public string UnitName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string Gender { get; set; } = "";
    public decimal? Salary { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public string Status { get; set; } = "";
    public string FileNumber { get; set; } = "";
}

// ═══════════════════════════════════════
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
// COM-01: Ministry Batches
// ═══════════════════════════════════════
public class MinistryBatchRow
{
    public int BatchID { get; set; }
    public string BatchNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public int ItemCount { get; set; }
    public int TotalCount { get; set; }
    public int ProcessedCount { get; set; }
    public DateTime SubmittedDate { get; set; }
    public DateTime? BatchDate { get; set; }
}

// ═══════════════════════════════════════
// COM-02: Pending Approvals
// ═══════════════════════════════════════
public class PendingApprovalRow
{
    public int RequestID { get; set; }
    public string RequestType { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string RequestedBy { get; set; } = "";
    public decimal? Amount { get; set; }
    public int DaysPending { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ═══════════════════════════════════════
// ADM-01: Audit Log
// ═══════════════════════════════════════
public class AuditLogRow
{
    public long LogID { get; set; }
    public string UserName { get; set; } = "";
    public string Action { get; set; } = "";
    public string TableName { get; set; } = "";
    public int? RecordID { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IPAddress { get; set; }
}

// ═══════════════════════════════════════
// ADM-02: User Activity
// ═══════════════════════════════════════
public class UserActivityRow
{
    public int UserID { get; set; }
    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string BranchName { get; set; } = "";
    public int LoginCount { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
}
