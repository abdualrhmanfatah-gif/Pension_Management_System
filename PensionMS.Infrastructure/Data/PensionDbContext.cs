using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Entities.OperationalBudget;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Entities.Assets;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;
using System.Linq.Expressions;

namespace PensionMS.Infrastructure.Data;

using PensionMS.Infrastructure.Data.Configurations;

public partial class PensionDbContext : DbContext
{
    public PensionDbContext(DbContextOptions<PensionDbContext> options, ICurrentUserContext? lifecycleScope = null) : base(options)
    {
        _lifecycleScope = lifecycleScope;
    }

    #region DbSets — Core
    public DbSet<Retiree> Retirees => Set<Retiree>();
    public DbSet<RetirementFile> RetirementFiles => Set<RetirementFile>();
    public DbSet<EntitledPerson> EntitledPersons => Set<EntitledPerson>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<GuardianEntitledPerson> GuardianEntitledPersons => Set<GuardianEntitledPerson>();
    #endregion

    #region DbSets — Citizen Portal
    public DbSet<CitizenAccount> CitizenAccounts => Set<CitizenAccount>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<CitizenNotification> CitizenNotifications => Set<CitizenNotification>();
    #endregion

    #region DbSets — Documents
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentPledge> DocumentPledges => Set<DocumentPledge>();
    public DbSet<MissingDocument> MissingDocuments => Set<MissingDocument>();
    public DbSet<DocumentExemption> DocumentExemptions => Set<DocumentExemption>();
    #endregion

    #region DbSets — Notes & Suspensions
    public DbSet<FileNote> FileNotes => Set<FileNote>();
    public DbSet<PensionSuspension> PensionSuspensions => Set<PensionSuspension>();
    #endregion

    #region DbSets — Tracking (Achievements & Obstacles)
    public DbSet<AchievementObstacle> AchievementObstacles => Set<AchievementObstacle>();
    public DbSet<TrackingLink> TrackingLinks => Set<TrackingLink>();
    public DbSet<TrackingComment> TrackingComments => Set<TrackingComment>();
    #endregion

    #region DbSets — Financial & Salary
    public DbSet<SalaryRecipient> SalaryRecipients => Set<SalaryRecipient>();
    public DbSet<FinancialEntitlement> FinancialEntitlements => Set<FinancialEntitlement>();
    public DbSet<SalaryPaymentBatch> SalaryPaymentBatches => Set<SalaryPaymentBatch>();
    public DbSet<SalaryPaymentItem> SalaryPaymentItems => Set<SalaryPaymentItem>();
    #endregion

    #region DbSets — Ministry & Settlements
    public DbSet<MinistryApprovalBatch> MinistryApprovalBatches => Set<MinistryApprovalBatch>();
    public DbSet<MinistryApprovalItem> MinistryApprovalItems => Set<MinistryApprovalItem>();
    public DbSet<SettlementDecision> SettlementDecisions => Set<SettlementDecision>();
    public DbSet<SettlementsBatch> SettlementsBatches => Set<SettlementsBatch>();
    public DbSet<SettlementItem> SettlementItems => Set<SettlementItem>();
    #endregion

    #region DbSets — Users & Security
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<TablePermission> TablePermissions => Set<TablePermission>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    #endregion

    #region DbSets — Operational Budget

    public DbSet<OperationalExpense> OperationalExpenses => Set<OperationalExpense>();
    #endregion

    #region DbSets — Financial System (New)
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<Quarter> Quarters => Set<Quarter>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    #endregion

    #region DbSets — Notifications & Audit
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationAttachment> NotificationAttachments => Set<NotificationAttachment>();
    public DbSet<NotificationOutbox> NotificationOutbox => Set<NotificationOutbox>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ImmutableAuditLog> ImmutableAuditLogs => Set<ImmutableAuditLog>();
    public DbSet<FileSnapshot> FileSnapshots => Set<FileSnapshot>();
    public DbSet<FileReviewItem> FileReviewItems => Set<FileReviewItem>();
    #endregion

    #region DbSets — Sync
    public DbSet<SyncQueue> SyncQueues => Set<SyncQueue>();
    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();
    #endregion

    #region DbSets — Workflow & Fraud
    public DbSet<PendingTask> PendingTasks => Set<PendingTask>();
    public DbSet<RiskEvent> RiskEvents => Set<RiskEvent>();
    #endregion

    #region DbSets — Lookups
    public DbSet<PaymentBook> PaymentBooks => Set<PaymentBook>();
    public DbSet<Rank> Ranks => Set<Rank>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<DocumentRequirementRule> DocumentRequirementRules => Set<DocumentRequirementRule>();
    public DbSet<FileStatus> FileStatuses => Set<FileStatus>();
    public DbSet<SocialStatus> SocialStatuses => Set<SocialStatus>();
    public DbSet<HealthStatus> HealthStatuses => Set<HealthStatus>();
    public DbSet<TransactionStatus> TransactionStatuses => Set<TransactionStatus>();
    public DbSet<PensionFundBranch> PensionFundBranches => Set<PensionFundBranch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<JobTitle> JobTitles => Set<JobTitle>();
    public DbSet<EmploymentStatus> EmploymentStatuses => Set<EmploymentStatus>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<CorrespondenceType> CorrespondenceTypes => Set<CorrespondenceType>();
    public DbSet<ExternalParty> ExternalParties => Set<ExternalParty>();
    public DbSet<Correspondence> Correspondences => Set<Correspondence>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<AttendanceEvent> AttendanceEvents => Set<AttendanceEvent>();
    public DbSet<ServiceHistoryEvent> ServiceHistoryEvents => Set<ServiceHistoryEvent>();
    public DbSet<AdministrativeOrder> AdministrativeOrders => Set<AdministrativeOrder>();
    public DbSet<AdministrativeOrderType> AdministrativeOrderTypes => Set<AdministrativeOrderType>();
    #endregion

    #region DbSets — System
    public DbSet<SystemTable> SystemTables => Set<SystemTable>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();
    public DbSet<RetireeCategory> RetireeCategories => Set<RetireeCategory>();
    public DbSet<StatusTransitionLog> StatusTransitionLogs => Set<StatusTransitionLog>();
    #endregion

    #region DbSets — Investment
    public DbSet<InvestmentType> InvestmentTypes => Set<InvestmentType>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<UnitType> UnitTypes => Set<UnitType>();
    public DbSet<InvestmentPortfolio> InvestmentPortfolios => Set<InvestmentPortfolio>();
    public DbSet<InvestmentProject> InvestmentProjects => Set<InvestmentProject>();
    public DbSet<InvestmentPlan> InvestmentPlans => Set<InvestmentPlan>();
    public DbSet<InvestmentProperty> InvestmentProperties => Set<InvestmentProperty>();
    public DbSet<PropertyUnit> PropertyUnits => Set<PropertyUnit>();
    public DbSet<AssetValuation> AssetValuations => Set<AssetValuation>();
    public DbSet<DepreciationRecord> DepreciationRecords => Set<DepreciationRecord>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<LeaseContract> LeaseContracts => Set<LeaseContract>();
    public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<InvestmentRiskAssessment> InvestmentRiskAssessments => Set<InvestmentRiskAssessment>();
    public DbSet<InvestmentApprovalRequest> InvestmentApprovalRequests => Set<InvestmentApprovalRequest>();
    #endregion

    #region DbSets — Inventory
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryCustody> InventoryCustodies => Set<InventoryCustody>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<SimpleStocktake> SimpleStocktakes => Set<SimpleStocktake>();
    public DbSet<SimpleStocktakeLine> SimpleStocktakeLines => Set<SimpleStocktakeLine>();
    #endregion

    #region DbSets — Fixed Assets
    public DbSet<FixedAssetType> FixedAssetTypes => Set<FixedAssetType>();
    public DbSet<FixedAssetCategory> FixedAssetCategories => Set<FixedAssetCategory>();
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();
    public DbSet<AssetITDetail> AssetITDetails => Set<AssetITDetail>();
    public DbSet<AssetVehicleDetail> AssetVehicleDetails => Set<AssetVehicleDetail>();
    public DbSet<AssetBuildingDetail> AssetBuildingDetails => Set<AssetBuildingDetail>();
    public DbSet<AssetLandDetail> AssetLandDetails => Set<AssetLandDetail>();
    public DbSet<AssetFurnitureDetail> AssetFurnitureDetails => Set<AssetFurnitureDetail>();
    public DbSet<DepreciationSchedule> DepreciationSchedules => Set<DepreciationSchedule>();
    public DbSet<AssetStocktake> AssetStocktakes => Set<AssetStocktake>();
    public DbSet<AssetStocktakeLine> AssetStocktakeLines => Set<AssetStocktakeLine>();
    #endregion

    #region DbSets — Accounting
    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryType> JournalEntryTypes => Set<JournalEntryType>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PensionDbContext).Assembly);

        // ═══════════════════════════════════════
        // SEEDERS
        // ═══════════════════════════════════════
        // Seeding will be handled by InitialFinancialSystem migration or separate seeder calls

        // ═══════════════════════════════════════
        // GLOBAL: RowVersion concurrency token
        // ═══════════════════════════════════════
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var rowVersionProp = clrType.GetProperty("RowVersion");
            if (rowVersionProp != null && rowVersionProp.PropertyType == typeof(byte[]))
            {
                modelBuilder.Entity(clrType).Property("RowVersion").IsRowVersion();
            }
        }

        // ═══════════════════════════════════════
        // CHECK CONSTRAINTS
        // ═══════════════════════════════════════
        modelBuilder.Entity<SalaryRecipient>(entity =>
        {
            // السماح بحالتين:
            // 1) نوع واحد فقط (متقاعد / وكيل / مستحق / وصي)
            // 2) حالة خاصة: وصي يستلم عن مستحق محدد (GuardianID + EntitledID)
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_SalaryRecipient_TypeOrGuardianWithEntitled",
                @"
                (
                    (CASE WHEN RetireeID IS NOT NULL THEN 1 ELSE 0 END +
                     CASE WHEN AgentID IS NOT NULL THEN 1 ELSE 0 END +
                     CASE WHEN EntitledID IS NOT NULL THEN 1 ELSE 0 END +
                     CASE WHEN GuardianID IS NOT NULL THEN 1 ELSE 0 END) = 1
                )
                OR
                (
                    GuardianID IS NOT NULL
                    AND EntitledID IS NOT NULL
                    AND RetireeID IS NULL
                    AND AgentID IS NULL
                )"
            ));
        });

        // ═══════════════════════════════════════
        // INDEXES
        // ═══════════════════════════════════════
        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.HasIndex(e => e.AccountCode).IsUnique();
            entity.HasIndex(e => e.AccountCategory);
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasIndex(e => e.EntryNumber).IsUnique();
            entity.HasIndex(e => e.PeriodID);
        });

        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasIndex(e => new { e.JournalEntryID, e.LineNumber });
            entity.HasIndex(e => e.AccountID);
        });

        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasIndex(e => new { e.BankID, e.TransactionDate });
            entity.HasIndex(e => e.IsReconciled);
        });

        // ═══════════════════════════════════════
        // GLOBAL: ISoftDeletable query filter
        // ═══════════════════════════════════════
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // ═══════════════════════════════════════
        // GLOBAL: ISoftDeletable.DeletedByUserID FK → User (SetNull)
        // ═══════════════════════════════════════
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // User والكيانات ذات خاصية التنقل DeletedByUser تستخدم إعداداتها الفردية
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType) 
                && entityType.ClrType != typeof(User)
                && entityType.ClrType.GetProperty("DeletedByUser", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) == null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(User))
                    .WithMany()
                    .HasForeignKey("DeletedByUserID")
                    .IsRequired(false);
            }
        }

        // ═══════════════════════════════════════
        // GLOBAL: DeleteBehavior — Restrict بشكل افتراضي مع استثناءات
        // ═══════════════════════════════════════
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            var fkProps = relationship.Properties.Select(p => p.Name).ToList();

            if (fkProps.Contains("DeletedByUserID") && relationship.PrincipalEntityType != relationship.DeclaringEntityType)
                // حقول المراجعة: SetNull عند حذف المستخدم المسؤول (عدا العلاقات ذاتية المرجع)
                relationship.DeleteBehavior = DeleteBehavior.SetNull;

            else
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

    }
}
