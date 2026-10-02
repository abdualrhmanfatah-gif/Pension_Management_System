using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.OperationalBudget;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Entities.Assets;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace PensionMS.Infrastructure.Repositories;

/// <summary>
/// تنفيذ وحدة العمل — تنسيق المعاملات عبر مستودعات متعددة
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PensionDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(PensionDbContext context)
    {
        _context = context;
    }

    // ═══════════════════════════════════════
    // CORE ENTITIES
    // ═══════════════════════════════════════
    private IRepository<Retiree>? _retirees;
    public IRepository<Retiree> Retirees => _retirees ??= new Repository<Retiree>(_context);

    private IRepository<RetirementFile>? _retirementFiles;
    public IRepository<RetirementFile> RetirementFiles => _retirementFiles ??= new Repository<RetirementFile>(_context);

    private IRepository<EntitledPerson>? _entitledPersons;
    public IRepository<EntitledPerson> EntitledPersons => _entitledPersons ??= new Repository<EntitledPerson>(_context);

    private IRepository<Agent>? _agents;
    public IRepository<Agent> Agents => _agents ??= new Repository<Agent>(_context);

    private IRepository<Guardian>? _guardians;
    public IRepository<Guardian> Guardians => _guardians ??= new Repository<Guardian>(_context);

    private IRepository<GuardianEntitledPerson>? _guardianEntitledPersons;
    public IRepository<GuardianEntitledPerson> GuardianEntitledPersons => _guardianEntitledPersons ??= new Repository<GuardianEntitledPerson>(_context);

    // ═══════════════════════════════════════
    // CITIZEN PORTAL
    // ═══════════════════════════════════════
    private IRepository<CitizenAccount>? _citizenAccounts;
    public IRepository<CitizenAccount> CitizenAccounts => _citizenAccounts ??= new Repository<CitizenAccount>(_context);

    private IRepository<OtpVerification>? _otpVerifications;
    public IRepository<OtpVerification> OtpVerifications => _otpVerifications ??= new Repository<OtpVerification>(_context);

    private IRepository<ServiceRequest>? _serviceRequests;
    public IRepository<ServiceRequest> ServiceRequests => _serviceRequests ??= new Repository<ServiceRequest>(_context);

    private IRepository<SupportTicket>? _supportTickets;
    public IRepository<SupportTicket> SupportTickets => _supportTickets ??= new Repository<SupportTicket>(_context);

    private IRepository<TicketMessage>? _ticketMessages;
    public IRepository<TicketMessage> TicketMessages => _ticketMessages ??= new Repository<TicketMessage>(_context);

    private IRepository<CitizenNotification>? _citizenNotifications;
    public IRepository<CitizenNotification> CitizenNotifications => _citizenNotifications ??= new Repository<CitizenNotification>(_context);

    // ═══════════════════════════════════════
    // DOCUMENTS
    // ═══════════════════════════════════════
    private IRepository<Document>? _documents;
    public IRepository<Document> Documents => _documents ??= new Repository<Document>(_context);

    private IRepository<DocumentPledge>? _documentPledges;
    public IRepository<DocumentPledge> DocumentPledges => _documentPledges ??= new Repository<DocumentPledge>(_context);

    private IRepository<MissingDocument>? _missingDocuments;
    public IRepository<MissingDocument> MissingDocuments => _missingDocuments ??= new Repository<MissingDocument>(_context);

    private IRepository<DocumentExemption>? _documentExemptions;
    public IRepository<DocumentExemption> DocumentExemptions => _documentExemptions ??= new Repository<DocumentExemption>(_context);

    private IRepository<DocumentRequirementRule>? _documentRequirementRules;
    public IRepository<DocumentRequirementRule> DocumentRequirementRules => _documentRequirementRules ??= new Repository<DocumentRequirementRule>(_context);

    // ═══════════════════════════════════════
    // FINANCIAL
    // ═══════════════════════════════════════
    private IRepository<FinancialEntitlement>? _financialEntitlements;
    public IRepository<FinancialEntitlement> FinancialEntitlements => _financialEntitlements ??= new Repository<FinancialEntitlement>(_context);

    private IRepository<SalaryRecipient>? _salaryRecipients;
    public IRepository<SalaryRecipient> SalaryRecipients => _salaryRecipients ??= new Repository<SalaryRecipient>(_context);

    private IRepository<SalaryPaymentBatch>? _salaryPaymentBatches;
    public IRepository<SalaryPaymentBatch> SalaryPaymentBatches => _salaryPaymentBatches ??= new Repository<SalaryPaymentBatch>(_context);

    private IRepository<SalaryPaymentItem>? _salaryPaymentItems;
    public IRepository<SalaryPaymentItem> SalaryPaymentItems => _salaryPaymentItems ??= new Repository<SalaryPaymentItem>(_context);

    // ═══════════════════════════════════════
    // SETTLEMENTS
    // ═══════════════════════════════════════
    private IRepository<SettlementDecision>? _settlementDecisions;
    public IRepository<SettlementDecision> SettlementDecisions => _settlementDecisions ??= new Repository<SettlementDecision>(_context);

    private IRepository<SettlementsBatch>? _settlementsBatches;
    public IRepository<SettlementsBatch> SettlementsBatches => _settlementsBatches ??= new Repository<SettlementsBatch>(_context);

    private IRepository<SettlementItem>? _settlementItems;
    public IRepository<SettlementItem> SettlementItems => _settlementItems ??= new Repository<SettlementItem>(_context);

    // ═══════════════════════════════════════
    // MINISTRY APPROVALS
    // ═══════════════════════════════════════
    private IRepository<MinistryApprovalBatch>? _ministryApprovalBatches;
    public IRepository<MinistryApprovalBatch> MinistryApprovalBatches => _ministryApprovalBatches ??= new Repository<MinistryApprovalBatch>(_context);

    private IRepository<MinistryApprovalItem>? _ministryApprovalItems;
    public IRepository<MinistryApprovalItem> MinistryApprovalItems => _ministryApprovalItems ??= new Repository<MinistryApprovalItem>(_context);

    // ═══════════════════════════════════════
    // NOTES & SUSPENSIONS
    // ═══════════════════════════════════════
    private IRepository<FileNote>? _fileNotes;
    public IRepository<FileNote> FileNotes => _fileNotes ??= new Repository<FileNote>(_context);

    private IRepository<PensionSuspension>? _pensionSuspensions;
    public IRepository<PensionSuspension> PensionSuspensions => _pensionSuspensions ??= new Repository<PensionSuspension>(_context);

    // ═══════════════════════════════════════
    // TRACKING (ACHIEVEMENTS & OBSTACLES)
    // ═══════════════════════════════════════
    private IRepository<AchievementObstacle>? _achievementObstacles;
    public IRepository<AchievementObstacle> AchievementObstacles => _achievementObstacles ??= new Repository<AchievementObstacle>(_context);

    private IRepository<TrackingLink>? _trackingLinks;
    public IRepository<TrackingLink> TrackingLinks => _trackingLinks ??= new Repository<TrackingLink>(_context);

    private IRepository<TrackingComment>? _trackingComments;
    public IRepository<TrackingComment> TrackingComments => _trackingComments ??= new Repository<TrackingComment>(_context);

    // ═══════════════════════════════════════
    // OPERATIONAL BUDGET
    // ═══════════════════════════════════════
    private IRepository<OperationalExpense>? _operationalExpenses;
    public IRepository<OperationalExpense> OperationalExpenses => _operationalExpenses ??= new Repository<OperationalExpense>(_context);

    private IRepository<FiscalYear>? _fiscalYears;
    public IRepository<FiscalYear> FiscalYears => _fiscalYears ??= new Repository<FiscalYear>(_context);

    // ═══════════════════════════════════════
    // SECURITY
    // ═══════════════════════════════════════
    private IRepository<User>? _users;
    public IRepository<User> Users => _users ??= new Repository<User>(_context);

    private IRepository<Role>? _roles;
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);

    private IRepository<Permission>? _permissions;
    public IRepository<Permission> Permissions => _permissions ??= new Repository<Permission>(_context);

    private IRepository<RolePermission>? _rolePermissions;
    public IRepository<RolePermission> RolePermissions => _rolePermissions ??= new Repository<RolePermission>(_context);

    private IRepository<UserPermission>? _userPermissions;
    public IRepository<UserPermission> UserPermissions => _userPermissions ??= new Repository<UserPermission>(_context);

    private IRepository<UserRoleAssignment>? _userRoleAssignments;
    public IRepository<UserRoleAssignment> UserRoleAssignments => _userRoleAssignments ??= new Repository<UserRoleAssignment>(_context);

    private IRepository<TablePermission>? _tablePermissions;
    public IRepository<TablePermission> TablePermissions => _tablePermissions ??= new Repository<TablePermission>(_context);

    private IRepository<LoginHistory>? _loginHistories;
    public IRepository<LoginHistory> LoginHistories => _loginHistories ??= new Repository<LoginHistory>(_context);

    private IRepository<RefreshToken>? _refreshTokens;
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

    // ═══════════════════════════════════════
    // SYSTEM
    // ═══════════════════════════════════════
    private IRepository<Notification>? _notifications;
    public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);

    private IRepository<NotificationAttachment>? _notificationAttachments;
    public IRepository<NotificationAttachment> NotificationAttachments => _notificationAttachments ??= new Repository<NotificationAttachment>(_context);

    private IRepository<NotificationOutbox>? _notificationOutbox;
    public IRepository<NotificationOutbox> NotificationOutbox => _notificationOutbox ??= new Repository<NotificationOutbox>(_context);

    private IRepository<AuditLog>? _auditLogs;
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

    private IRepository<ImmutableAuditLog>? _immutableAuditLogs;
    public IRepository<ImmutableAuditLog> ImmutableAuditLogs => _immutableAuditLogs ??= new Repository<ImmutableAuditLog>(_context);

    private IRepository<SyncQueue>? _syncQueues;
    public IRepository<SyncQueue> SyncQueues => _syncQueues ??= new Repository<SyncQueue>(_context);

    private IRepository<DeviceRegistration>? _deviceRegistrations;
    public IRepository<DeviceRegistration> DeviceRegistrations => _deviceRegistrations ??= new Repository<DeviceRegistration>(_context);

    private IRepository<ReportTemplate>? _reportTemplates;
    public IRepository<ReportTemplate> ReportTemplates => _reportTemplates ??= new Repository<ReportTemplate>(_context);

    private IRepository<SavedReport>? _savedReports;
    public IRepository<SavedReport> SavedReports => _savedReports ??= new Repository<SavedReport>(_context);

    private IRepository<StatusTransitionLog>? _statusTransitionLogs;
    public IRepository<StatusTransitionLog> StatusTransitionLogs => _statusTransitionLogs ??= new Repository<StatusTransitionLog>(_context);

    private IRepository<SystemSetting>? _systemSettings;
    public IRepository<SystemSetting> SystemSettings => _systemSettings ??= new Repository<SystemSetting>(_context);

    private IRepository<FileSnapshot>? _fileSnapshots;
    public IRepository<FileSnapshot> FileSnapshots => _fileSnapshots ??= new Repository<FileSnapshot>(_context);

    private IRepository<FileReviewItem>? _fileReviewItems;
    public IRepository<FileReviewItem> FileReviewItems => _fileReviewItems ??= new Repository<FileReviewItem>(_context);

    private IRepository<PendingTask>? _pendingTasks;
    public IRepository<PendingTask> PendingTasks => _pendingTasks ??= new Repository<PendingTask>(_context);

    private IRepository<RiskEvent>? _riskEvents;
    public IRepository<RiskEvent> RiskEvents => _riskEvents ??= new Repository<RiskEvent>(_context);

    // ═══════════════════════════════════════
    // LOOKUPS
    // ═══════════════════════════════════════
    private IRepository<Rank>? _ranks;
    public IRepository<Rank> Ranks => _ranks ??= new Repository<Rank>(_context);

    private IRepository<Bank>? _banks;
    public IRepository<Bank> Banks => _banks ??= new Repository<Bank>(_context);

    private IRepository<Relationship>? _relationships;
    public IRepository<Relationship> Relationships => _relationships ??= new Repository<Relationship>(_context);

    private IRepository<DocumentType>? _documentTypes;
    public IRepository<DocumentType> DocumentTypes => _documentTypes ??= new Repository<DocumentType>(_context);

    private IRepository<FileStatus>? _fileStatuses;
    public IRepository<FileStatus> FileStatuses => _fileStatuses ??= new Repository<FileStatus>(_context);

    private IRepository<SocialStatus>? _socialStatuses;
    public IRepository<SocialStatus> SocialStatuses => _socialStatuses ??= new Repository<SocialStatus>(_context);

    private IRepository<HealthStatus>? _healthStatuses;
    public IRepository<HealthStatus> HealthStatuses => _healthStatuses ??= new Repository<HealthStatus>(_context);

    private IRepository<TransactionStatus>? _transactionStatuses;
    public IRepository<TransactionStatus> TransactionStatuses => _transactionStatuses ??= new Repository<TransactionStatus>(_context);

    private IRepository<PensionFundBranch>? _pensionFundBranches;
    public IRepository<PensionFundBranch> PensionFundBranches => _pensionFundBranches ??= new Repository<PensionFundBranch>(_context);

    private IRepository<SystemTable>? _systemTables;
    public IRepository<SystemTable> SystemTables => _systemTables ??= new Repository<SystemTable>(_context);

    private IRepository<RetireeCategory>? _retireeCategories;
    public IRepository<RetireeCategory> RetireeCategories => _retireeCategories ??= new Repository<RetireeCategory>(_context);

    private IRepository<PaymentBook>? _paymentBooks;
    public IRepository<PaymentBook> PaymentBooks => _paymentBooks ??= new Repository<PaymentBook>(_context);

    // ═══════════════════════════════════════
    // INVESTMENT
    // ═══════════════════════════════════════
    private IRepository<InvestmentType>? _investmentTypes;
    public IRepository<InvestmentType> InvestmentTypes => _investmentTypes ??= new Repository<InvestmentType>(_context);

    private IRepository<PropertyType>? _propertyTypes;
    public IRepository<PropertyType> PropertyTypes => _propertyTypes ??= new Repository<PropertyType>(_context);

    private IRepository<UnitType>? _unitTypes;
    public IRepository<UnitType> UnitTypes => _unitTypes ??= new Repository<UnitType>(_context);

    private IRepository<InvestmentPortfolio>? _investmentPortfolios;
    public IRepository<InvestmentPortfolio> InvestmentPortfolios => _investmentPortfolios ??= new Repository<InvestmentPortfolio>(_context);

    private IRepository<InvestmentProject>? _investmentProjects;
    public IRepository<InvestmentProject> InvestmentProjects => _investmentProjects ??= new Repository<InvestmentProject>(_context);

    private IRepository<InvestmentPlan>? _investmentPlans;
    public IRepository<InvestmentPlan> InvestmentPlans => _investmentPlans ??= new Repository<InvestmentPlan>(_context);

    private IRepository<InvestmentProperty>? _investmentProperties;
    public IRepository<InvestmentProperty> InvestmentProperties => _investmentProperties ??= new Repository<InvestmentProperty>(_context);

    private IRepository<PropertyUnit>? _propertyUnits;
    public IRepository<PropertyUnit> PropertyUnits => _propertyUnits ??= new Repository<PropertyUnit>(_context);

    private IRepository<AssetValuation>? _assetValuations;
    public IRepository<AssetValuation> AssetValuations => _assetValuations ??= new Repository<AssetValuation>(_context);

    private IRepository<DepreciationRecord>? _depreciationRecords;
    public IRepository<DepreciationRecord> DepreciationRecords => _depreciationRecords ??= new Repository<DepreciationRecord>(_context);

    private IRepository<Tenant>? _tenants;
    public IRepository<Tenant> Tenants => _tenants ??= new Repository<Tenant>(_context);

    private IRepository<LeaseContract>? _leaseContracts;
    public IRepository<LeaseContract> LeaseContracts => _leaseContracts ??= new Repository<LeaseContract>(_context);

    private IRepository<PaymentSchedule>? _paymentSchedules;
    public IRepository<PaymentSchedule> PaymentSchedules => _paymentSchedules ??= new Repository<PaymentSchedule>(_context);

    private IRepository<MaintenanceRequest>? _maintenanceRequests;
    public IRepository<MaintenanceRequest> MaintenanceRequests => _maintenanceRequests ??= new Repository<MaintenanceRequest>(_context);

    private IRepository<WorkOrder>? _workOrders;
    public IRepository<WorkOrder> WorkOrders => _workOrders ??= new Repository<WorkOrder>(_context);

    private IRepository<InvestmentRiskAssessment>? _investmentRiskAssessments;
    public IRepository<InvestmentRiskAssessment> InvestmentRiskAssessments => _investmentRiskAssessments ??= new Repository<InvestmentRiskAssessment>(_context);

    private IRepository<InvestmentApprovalRequest>? _investmentApprovalRequests;
    public IRepository<InvestmentApprovalRequest> InvestmentApprovalRequests => _investmentApprovalRequests ??= new Repository<InvestmentApprovalRequest>(_context);

    // ═══════════════════════════════════════
    // FINANCIAL SYSTEM (NEW)
    // ═══════════════════════════════════════
    private IRepository<Quarter>? _quarters;
    public IRepository<Quarter> Quarters => _quarters ??= new Repository<Quarter>(_context);

    private IRepository<Voucher>? _vouchers;
    public IRepository<Voucher> Vouchers => _vouchers ??= new Repository<Voucher>(_context);

    // ═══════════════════════════════════════
    // HR SYSTEM
    // ═══════════════════════════════════════
    private IRepository<Employee>? _employees;
    public IRepository<Employee> Employees => _employees ??= new Repository<Employee>(_context);

    private IRepository<Department>? _departments;
    public IRepository<Department> Departments => _departments ??= new Repository<Department>(_context);

    private IRepository<Section>? _sections;
    public IRepository<Section> Sections => _sections ??= new Repository<Section>(_context);

    private IRepository<JobTitle>? _jobTitles;
    public IRepository<JobTitle> JobTitles => _jobTitles ??= new Repository<JobTitle>(_context);

    private IRepository<EmploymentStatus>? _employmentStatuses;
    public IRepository<EmploymentStatus> EmploymentStatuses => _employmentStatuses ??= new Repository<EmploymentStatus>(_context);

    private IRepository<AttendanceEvent>? _attendanceEvents;
    public IRepository<AttendanceEvent> AttendanceEvents => _attendanceEvents ??= new Repository<AttendanceEvent>(_context);

    private IRepository<LeaveRequest>? _leaveRequests;
    public IRepository<LeaveRequest> LeaveRequests => _leaveRequests ??= new Repository<LeaveRequest>(_context);

    private IRepository<LeaveType>? _leaveTypes;
    public IRepository<LeaveType> LeaveTypes => _leaveTypes ??= new Repository<LeaveType>(_context);

    private IRepository<ServiceHistoryEvent>? _serviceHistoryEvents;
    public IRepository<ServiceHistoryEvent> ServiceHistoryEvents => _serviceHistoryEvents ??= new Repository<ServiceHistoryEvent>(_context);

    private IRepository<AdministrativeOrder>? _administrativeOrders;
    public IRepository<AdministrativeOrder> AdministrativeOrders => _administrativeOrders ??= new Repository<AdministrativeOrder>(_context);

    private IRepository<AdministrativeOrderType>? _administrativeOrderTypes;
    public IRepository<AdministrativeOrderType> AdministrativeOrderTypes => _administrativeOrderTypes ??= new Repository<AdministrativeOrderType>(_context);

    private IRepository<CorrespondenceType>? _correspondenceTypes;
    public IRepository<CorrespondenceType> CorrespondenceTypes => _correspondenceTypes ??= new Repository<CorrespondenceType>(_context);

    private IRepository<ExternalParty>? _externalParties;
    public IRepository<ExternalParty> ExternalParties => _externalParties ??= new Repository<ExternalParty>(_context);

    private IRepository<Correspondence>? _correspondences;
    public IRepository<Correspondence> Correspondences => _correspondences ??= new Repository<Correspondence>(_context);

    // ═══════════════════════════════════════
    // INVENTORY
    // ═══════════════════════════════════════
    private IRepository<InventoryItem>? _inventoryItems;
    public IRepository<InventoryItem> InventoryItems => _inventoryItems ??= new Repository<InventoryItem>(_context);

    private IRepository<InventoryCustody>? _inventoryCustodies;
    public IRepository<InventoryCustody> InventoryCustodies => _inventoryCustodies ??= new Repository<InventoryCustody>(_context);

    private IRepository<InventoryTransaction>? _inventoryTransactions;
    public IRepository<InventoryTransaction> InventoryTransactions => _inventoryTransactions ??= new Repository<InventoryTransaction>(_context);

    private IRepository<SimpleStocktake>? _simpleStocktakes;
    public IRepository<SimpleStocktake> SimpleStocktakes => _simpleStocktakes ??= new Repository<SimpleStocktake>(_context);

    private IRepository<SimpleStocktakeLine>? _simpleStocktakeLines;
    public IRepository<SimpleStocktakeLine> SimpleStocktakeLines => _simpleStocktakeLines ??= new Repository<SimpleStocktakeLine>(_context);

    // ═══════════════════════════════════════
    // FIXED ASSETS
    // ═══════════════════════════════════════
    private IRepository<FixedAsset>? _fixedAssets;
    public IRepository<FixedAsset> FixedAssets => _fixedAssets ??= new Repository<FixedAsset>(_context);

    private IRepository<AssetStocktake>? _assetStocktakes;
    public IRepository<AssetStocktake> AssetStocktakes => _assetStocktakes ??= new Repository<AssetStocktake>(_context);

    private IRepository<AssetStocktakeLine>? _assetStocktakeLines;
    public IRepository<AssetStocktakeLine> AssetStocktakeLines => _assetStocktakeLines ??= new Repository<AssetStocktakeLine>(_context);

    // ═══════════════════════════════════════
    // ACCOUNTING
    // ═══════════════════════════════════════
    private IRepository<ChartOfAccount>? _chartOfAccounts;
    public IRepository<ChartOfAccount> ChartOfAccounts => _chartOfAccounts ??= new Repository<ChartOfAccount>(_context);

    private IRepository<JournalEntry>? _journalEntries;
    public IRepository<JournalEntry> JournalEntries => _journalEntries ??= new Repository<JournalEntry>(_context);

    private IRepository<JournalEntryType>? _journalEntryTypes;
    public IRepository<JournalEntryType> JournalEntryTypes => _journalEntryTypes ??= new Repository<JournalEntryType>(_context);

    private IRepository<JournalEntryLine>? _journalEntryLines;
    public IRepository<JournalEntryLine> JournalEntryLines => _journalEntryLines ??= new Repository<JournalEntryLine>(_context);

    private IRepository<CostCenter>? _costCenters;
    public IRepository<CostCenter> CostCenters => _costCenters ??= new Repository<CostCenter>(_context);

    private IRepository<FiscalPeriod>? _fiscalPeriods;
    public IRepository<FiscalPeriod> FiscalPeriods => _fiscalPeriods ??= new Repository<FiscalPeriod>(_context);

    private IRepository<AccountBalance>? _accountBalances;
    public IRepository<AccountBalance> AccountBalances => _accountBalances ??= new Repository<AccountBalance>(_context);

    private IRepository<BudgetItem>? _budgetItems;
    public IRepository<BudgetItem> BudgetItems => _budgetItems ??= new Repository<BudgetItem>(_context);

    private IRepository<BankTransaction>? _bankTransactions;
    public IRepository<BankTransaction> BankTransactions => _bankTransactions ??= new Repository<BankTransaction>(_context);

    // ═══════════════════════════════════════
    // TRANSACTION MANAGEMENT
    // ═══════════════════════════════════════
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public bool HasActiveTransaction => _context.Database.CurrentTransaction != null;

    public async Task LockFileReviewAsync(int fileId)
    {
        if (!HasActiveTransaction)
            throw new InvalidOperationException("يتطلب قفل المراجعة معاملة نشطة");

        // A database row lock coordinates all API instances, including review decisions.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT [FileID] FROM [RetirementFiles] WITH (UPDLOCK, HOLDLOCK) WHERE [FileID] = {fileId}");
    }

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // ═══════════════════════════════════════
    // DISPOSE
    // ═══════════════════════════════════════
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
