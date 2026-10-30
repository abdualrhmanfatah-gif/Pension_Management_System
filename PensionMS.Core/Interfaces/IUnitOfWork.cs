using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.OperationalBudget;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Entities.Assets;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Entities.Accounting;
namespace PensionMS.Core.Interfaces;

/// <summary>
/// وحدة العمل — تنسيق العمليات عبر مستودعات متعددة ضمن معاملة واحدة
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ═══════════════════════════════════════
    // CORE ENTITIES
    // ═══════════════════════════════════════
    IRepository<Retiree> Retirees { get; }
    IRepository<RetirementFile> RetirementFiles { get; }
    IRepository<EntitledPerson> EntitledPersons { get; }
    IRepository<Agent> Agents { get; }
    IRepository<Guardian> Guardians { get; }
    IRepository<GuardianEntitledPerson> GuardianEntitledPersons { get; }

    // ═══════════════════════════════════════
    // CITIZEN PORTAL
    // ═══════════════════════════════════════
    IRepository<CitizenAccount> CitizenAccounts { get; }
    IRepository<OtpVerification> OtpVerifications { get; }
    IRepository<ServiceRequest> ServiceRequests { get; }
    IRepository<SupportTicket> SupportTickets { get; }
    IRepository<TicketMessage> TicketMessages { get; }
    IRepository<CitizenNotification> CitizenNotifications { get; }

    // ═══════════════════════════════════════
    // DOCUMENTS
    // ═══════════════════════════════════════
    IRepository<Document> Documents { get; }
    IRepository<DocumentPledge> DocumentPledges { get; }
    IRepository<MissingDocument> MissingDocuments { get; }
    IRepository<DocumentExemption> DocumentExemptions { get; }
    IRepository<DocumentRequirementRule> DocumentRequirementRules { get; }

    // ═══════════════════════════════════════
    // FINANCIAL
    // ═══════════════════════════════════════
    IRepository<FinancialEntitlement> FinancialEntitlements { get; }
    IRepository<SalaryRecipient> SalaryRecipients { get; }
    IRepository<SalaryPaymentBatch> SalaryPaymentBatches { get; }
    IRepository<SalaryPaymentItem> SalaryPaymentItems { get; }

    // ═══════════════════════════════════════
    // SETTLEMENTS
    // ═══════════════════════════════════════
    IRepository<SettlementDecision> SettlementDecisions { get; }
    IRepository<SettlementsBatch> SettlementsBatches { get; }
    IRepository<SettlementItem> SettlementItems { get; }

    // ═══════════════════════════════════════
    // MINISTRY APPROVALS
    // ═══════════════════════════════════════
    IRepository<MinistryApprovalBatch> MinistryApprovalBatches { get; }
    IRepository<MinistryApprovalItem> MinistryApprovalItems { get; }

    // ═══════════════════════════════════════
    // NOTES & SUSPENSIONS
    // ═══════════════════════════════════════
    IRepository<FileNote> FileNotes { get; }
    IRepository<PensionSuspension> PensionSuspensions { get; }

    // ═══════════════════════════════════════
    // TRACKING (ACHIEVEMENTS & OBSTACLES)
    // ═══════════════════════════════════════
    IRepository<AchievementObstacle> AchievementObstacles { get; }
    IRepository<TrackingLink> TrackingLinks { get; }
    IRepository<TrackingComment> TrackingComments { get; }

    // ═══════════════════════════════════════
    // OPERATIONAL BUDGET
    // ═══════════════════════════════════════
    IRepository<OperationalExpense> OperationalExpenses { get; }

    IRepository<FiscalYear> FiscalYears { get; }

    // ═══════════════════════════════════════
    // SECURITY
    // ═══════════════════════════════════════
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<UserPermission> UserPermissions { get; }
    IRepository<UserRoleAssignment> UserRoleAssignments { get; }
    IRepository<TablePermission> TablePermissions { get; }
    IRepository<LoginHistory> LoginHistories { get; }
    IRepository<RefreshToken> RefreshTokens { get; }

    // ═══════════════════════════════════════
    // SYSTEM
    // ═══════════════════════════════════════
    IRepository<Notification> Notifications { get; }
    IRepository<NotificationAttachment> NotificationAttachments { get; }
    IRepository<NotificationOutbox> NotificationOutbox { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<ImmutableAuditLog> ImmutableAuditLogs { get; }
    IRepository<SyncQueue> SyncQueues { get; }
    IRepository<DeviceRegistration> DeviceRegistrations { get; }
    IRepository<ReportTemplate> ReportTemplates { get; }
    IRepository<SavedReport> SavedReports { get; }
    IRepository<StatusTransitionLog> StatusTransitionLogs { get; }
    IRepository<SystemSetting> SystemSettings { get; }
    IRepository<FileSnapshot> FileSnapshots { get; }
    IRepository<FileReviewItem> FileReviewItems { get; }
    IRepository<PendingTask> PendingTasks { get; }
    IRepository<RiskEvent> RiskEvents { get; }

    // ═══════════════════════════════════════
    // LOOKUPS
    // ═══════════════════════════════════════
    IRepository<Rank> Ranks { get; }
    IRepository<Bank> Banks { get; }
    IRepository<Relationship> Relationships { get; }
    IRepository<DocumentType> DocumentTypes { get; }
    IRepository<FileStatus> FileStatuses { get; }
    IRepository<SocialStatus> SocialStatuses { get; }
    IRepository<HealthStatus> HealthStatuses { get; }
    IRepository<TransactionStatus> TransactionStatuses { get; }
    IRepository<PensionFundBranch> PensionFundBranches { get; }
    IRepository<SystemTable> SystemTables { get; }
    IRepository<RetireeCategory> RetireeCategories { get; }
    IRepository<PaymentBook> PaymentBooks { get; }

    // ═══════════════════════════════════════
    // INVESTMENT
    // ═══════════════════════════════════════
    IRepository<InvestmentType> InvestmentTypes { get; }
    IRepository<PropertyType> PropertyTypes { get; }
    IRepository<UnitType> UnitTypes { get; }
    IRepository<InvestmentPortfolio> InvestmentPortfolios { get; }
    IRepository<InvestmentProject> InvestmentProjects { get; }
    IRepository<InvestmentPlan> InvestmentPlans { get; }
    IRepository<InvestmentProperty> InvestmentProperties { get; }
    IRepository<PropertyUnit> PropertyUnits { get; }
    IRepository<AssetValuation> AssetValuations { get; }
    IRepository<DepreciationRecord> DepreciationRecords { get; }
    IRepository<Tenant> Tenants { get; }
    IRepository<LeaseContract> LeaseContracts { get; }
    IRepository<PaymentSchedule> PaymentSchedules { get; }
    IRepository<MaintenanceRequest> MaintenanceRequests { get; }
    IRepository<WorkOrder> WorkOrders { get; }
    IRepository<InvestmentRiskAssessment> InvestmentRiskAssessments { get; }
    IRepository<InvestmentApprovalRequest> InvestmentApprovalRequests { get; }

    // ═══════════════════════════════════════
    // FINANCIAL SYSTEM (NEW)
    // ═══════════════════════════════════════
    IRepository<Quarter> Quarters { get; }
    IRepository<Voucher> Vouchers { get; }

    // ═══════════════════════════════════════
    // HR SYSTEM
    // ═══════════════════════════════════════
    IRepository<Employee> Employees { get; }
    IRepository<Department> Departments { get; }
    IRepository<Section> Sections { get; }
    IRepository<JobTitle> JobTitles { get; }
    IRepository<EmploymentStatus> EmploymentStatuses { get; }
    IRepository<AttendanceEvent> AttendanceEvents { get; }
    IRepository<LeaveRequest> LeaveRequests { get; }
    IRepository<LeaveType> LeaveTypes { get; }
    IRepository<ServiceHistoryEvent> ServiceHistoryEvents { get; }

    IRepository<AdministrativeOrder> AdministrativeOrders { get; }
    IRepository<AdministrativeOrderType> AdministrativeOrderTypes { get; }

    IRepository<CorrespondenceType> CorrespondenceTypes { get; }
    IRepository<ExternalParty> ExternalParties { get; }
    IRepository<Correspondence> Correspondences { get; }


    // ═══════════════════════════════════════
    // INVENTORY
    // ═══════════════════════════════════════
    IRepository<InventoryItem> InventoryItems { get; }
    IRepository<InventoryCustody> InventoryCustodies { get; }
    IRepository<InventoryTransaction> InventoryTransactions { get; }
    IRepository<SimpleStocktake> SimpleStocktakes { get; }
    IRepository<SimpleStocktakeLine> SimpleStocktakeLines { get; }

    // ═══════════════════════════════════════
    // FIXED ASSETS
    // ═══════════════════════════════════════
    IRepository<FixedAsset> FixedAssets { get; }
    IRepository<AssetStocktake> AssetStocktakes { get; }
    IRepository<AssetStocktakeLine> AssetStocktakeLines { get; }

    // ═══════════════════════════════════════
    // ACCOUNTING
    // ═══════════════════════════════════════
    IRepository<ChartOfAccount> ChartOfAccounts { get; }
    IRepository<JournalEntry> JournalEntries { get; }
    IRepository<JournalEntryType> JournalEntryTypes { get; }
    IRepository<JournalEntryLine> JournalEntryLines { get; }
    IRepository<CostCenter> CostCenters { get; }
    IRepository<FiscalPeriod> FiscalPeriods { get; }
    IRepository<AccountBalance> AccountBalances { get; }
    IRepository<BudgetItem> BudgetItems { get; }
    IRepository<BankTransaction> BankTransactions { get; }

    // ═══════════════════════════════════════
    // TRANSACTION MANAGEMENT
    // ═══════════════════════════════════════
    /// <summary>Flushes tracked changes. Inside a transaction it does not commit that transaction.</summary>
    Task<int> SaveChangesAsync();
    bool HasActiveTransaction { get; }
    Task BeginTransactionAsync();
    /// <summary>Registers an external-resource cleanup for rollback by this transaction owner.</summary>
    void OnRollback(Func<Task> compensation);
    /// <summary>Serializes review changes for one file until the active transaction ends.</summary>
    Task LockFileReviewAsync(int fileId);
    /// <summary>Serializes changes to a salary batch until the active transaction ends.</summary>
    Task LockSalaryBatchAsync(int batchId);
    /// <summary>Prevents two batches accruing the same monthly pension concurrently.</summary>
    Task LockPayrollPeriodAsync(DateTime month);
    /// <summary>يحجز الاستحقاقات المالية للمتقاعد داخل معاملة نشطة لمنع تكرار الإنشاء المتزامن.</summary>
    Task LockRetireeEntitlementsAsync(int retireeId);
    /// <summary>Only the transaction owner commits. Does not implicitly call SaveChanges.</summary>
    Task CommitAsync();
    /// <summary>Only the transaction owner rolls back. Discard the operation context after failure; tracked values are not restored.</summary>
    Task RollbackAsync();
}
