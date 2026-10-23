using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Exceptions;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PensionMS.Infrastructure.Repositories;

/// <summary>Scopes report sources before projection, aggregation and paging.</summary>
internal sealed class PensionDataQuery(IUnitOfWork unitOfWork, PermissionScope scope, bool readOnly = true, bool historicalOwnership = false) : IPensionDataQuery
{
    public PermissionScope Scope => scope;

    public int? ResolveBranch(int? requestedBranch = null)
    {
        if (requestedBranch is <= 0) throw new ArgumentException("معرف الفرع غير صالح");
        if (scope.NarrowTo(requestedBranch).IsDenied) throw new BranchScopeAccessException();
        return requestedBranch;
    }

    public bool IsRestricted(int? requestedBranch = null)
    {
        ResolveBranch(requestedBranch);
        return !scope.AllBranches || requestedBranch.HasValue;
    }

    public IQueryable<RetirementFile> Files(int? requestedBranch = null)
    {
        ResolveBranch(requestedBranch);
        var files = unitOfWork.RetirementFiles.Query();
        if (!historicalOwnership) files = files.Where(f => !f.IsDeleted);
        if (!IsRestricted(requestedBranch)) return files;
        var branches = scope.NarrowTo(requestedBranch).BranchIds.ToArray();
        return files.Where(f => branches.Contains(f.PensionBranchID));
    }

    public IQueryable<int> FileIds(int? requestedBranch = null)
        => Files(requestedBranch).Select(f => f.FileID);

    public IQueryable<T> Query<T>(IRepository<T> repository, int? requestedBranch = null) where T : class
    {
        var branch = ResolveBranch(requestedBranch);
        var query = repository.Query();
        if (!IsRestricted(branch)) return query;
        var branches = scope.NarrowTo(branch).BranchIds.ToArray();
        if (query is IQueryable<RetirementFile> fileQuery)
            return (IQueryable<T>)fileQuery.Where(f => (historicalOwnership || !f.IsDeleted) && branches.Contains(f.PensionBranchID));
        if (query is IQueryable<PensionFundBranch> branchQuery)
            return (IQueryable<T>)branchQuery.Where(b => branches.Contains(b.PensionBranchID));
        var fileIds = FileIds(branch);
        var retireeIds = unitOfWork.RetirementFiles.Query()
            .Where(f => branches.Contains(f.PensionBranchID)).Select(f => f.RetireeID);
        var approvalIds = query is IQueryable<SettlementItem> or IQueryable<SettlementDecision> or IQueryable<SettlementsBatch>
            or IQueryable<MinistryApprovalItem> or IQueryable<MinistryApprovalBatch>
            ? unitOfWork.Retirees.Query().Where(r => retireeIds.Contains(r.RetireeID)).Select(r => r.ApprovalItemID)
            : Array.Empty<int>().AsQueryable();

        object result = query switch
        {
            IQueryable<RetirementFile> q => q.Where(f => branches.Contains(f.PensionBranchID)),
            IQueryable<Retiree> q => q.Where(r => retireeIds.Contains(r.RetireeID)),
            IQueryable<EntitledPerson> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<Agent> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<GuardianEntitledPerson> q => q.Where(g => fileIds.Contains(g.Guardian.FileID) && fileIds.Contains(g.EntitledPerson.FileID)),
            IQueryable<FileReviewItem> q => q.Where(i => fileIds.Contains(i.RetirementFileID)),
            IQueryable<Guardian> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<FinancialEntitlement> q => q.Where(e => retireeIds.Contains(e.RetireeID)),
            IQueryable<PensionSuspension> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<MissingDocument> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<PaymentBook> q => q.Where(b => fileIds.Contains(b.FileID)),
            IQueryable<SalaryRecipient> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<DocumentExemption> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<FileNote> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<DocumentPledge> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<SalaryPaymentItem> q => q.Where(e => fileIds.Contains(e.FileID)),
            IQueryable<SalaryPaymentBatch> q => q.Where(b => b.SalaryPaymentItems.Any(i => fileIds.Contains(i.FileID))
                && (readOnly || b.SalaryPaymentItems.All(i => fileIds.Contains(i.FileID)))),
            IQueryable<SettlementItem> q => q.Where(i => i.RetireeID.HasValue && retireeIds.Contains(i.RetireeID.Value)
                || i.ApprovalItemID.HasValue && approvalIds.Contains(i.ApprovalItemID.Value)),
            IQueryable<SettlementDecision> q => q.Where(d => d.SettlementItems.Any(i =>
                i.RetireeID.HasValue && retireeIds.Contains(i.RetireeID.Value)
                || i.ApprovalItemID.HasValue && approvalIds.Contains(i.ApprovalItemID.Value))),
            IQueryable<SettlementsBatch> q => q.Where(b => b.SettlementItems.Any(i =>
                i.RetireeID.HasValue && retireeIds.Contains(i.RetireeID.Value)
                || i.ApprovalItemID.HasValue && approvalIds.Contains(i.ApprovalItemID.Value))),
            IQueryable<MinistryApprovalItem> q => q.Where(i => approvalIds.Contains(i.ApprovalItemID)),
            IQueryable<MinistryApprovalBatch> q => q.Where(b => b.MinistryApprovalItems.Any(i => approvalIds.Contains(i.ApprovalItemID))),
            IQueryable<User> q => q.Where(u => (u.BranchID.HasValue && branches.Contains(u.BranchID.Value))),
            IQueryable<PensionFundBranch> q => q.Where(b => branches.Contains(b.PensionBranchID)),
            IQueryable<InvestmentProject> q => q.Where(p => branches.Contains(p.PensionBranchID)),
            IQueryable<InvestmentProperty> q => q.Where(p => branches.Contains(p.PensionBranchID)),
            IQueryable<PropertyUnit> q => q.Where(u => branches.Contains(u.Property.PensionBranchID)),
            IQueryable<LeaseContract> q => q.Where(c => branches.Contains(c.Unit.Property.PensionBranchID)),
            IQueryable<PaymentSchedule> q => q.Where(p => branches.Contains(p.LeaseContract.Unit.Property.PensionBranchID)),
            IQueryable<MaintenanceRequest> q => q.Where(r => branches.Contains(r.Property.PensionBranchID)),
            IQueryable<WorkOrder> q => q.Where(w => branches.Contains(w.MaintenanceRequest.Property.PensionBranchID)),
            IQueryable<AssetValuation> q => q.Where(v => branches.Contains(v.Property.PensionBranchID)),
            IQueryable<DepreciationRecord> q => q.Where(v => branches.Contains(v.Property.PensionBranchID)),
            IQueryable<InvestmentPlan> q => q.Where(p => branches.Contains(p.Project.PensionBranchID)),
            IQueryable<InvestmentRiskAssessment> q => q.Where(r => branches.Contains(r.Project.PensionBranchID)),
            IQueryable<InvestmentApprovalRequest> q => q.Where(r => (r.RequestedBy.BranchID.HasValue && branches.Contains(r.RequestedBy.BranchID.Value))),
            // Portfolio capital is stored globally: mixed portfolios require HQ access.
            IQueryable<Tenant> q => q.Where(t => t.LeaseContracts.Any() && t.LeaseContracts.All(c => branches.Contains(c.Unit.Property.PensionBranchID))),
            IQueryable<InvestmentPortfolio> q => q.Where(p => p.Projects.Any()
                && p.Projects.All(project => branches.Contains(project.PensionBranchID))),
            IQueryable<AuditLog> q => ScopeAudit(q, branch),
            IQueryable<Document> q => q.Where(d =>
                (d.FileID.HasValue || d.RetireeID.HasValue || d.EntitledID.HasValue || d.AgentID.HasValue || d.GuardianID.HasValue || d.PledgeID.HasValue || d.ExemptionID.HasValue || d.NoteID.HasValue || d.SuspensionID.HasValue || d.InvestmentProjectID.HasValue || d.InvestmentPropertyID.HasValue || d.PropertyUnitID.HasValue || d.LeaseContractID.HasValue || d.MaintenanceRequestID.HasValue || d.WorkOrderID.HasValue || d.AssetValuationID.HasValue || d.DepreciationRecordID.HasValue || d.PaymentScheduleID.HasValue || d.InvestmentPlanID.HasValue || d.RiskAssessmentID.HasValue || d.InvestmentPortfolioID.HasValue || d.TenantID.HasValue || d.SalaryBatchID.HasValue)
                && (!d.FileID.HasValue || (fileIds.Contains(d.FileID.Value)))
                && (!d.RetireeID.HasValue || (retireeIds.Contains(d.RetireeID.Value)))
                && (!d.EntitledID.HasValue || (d.EntitledPerson != null && fileIds.Contains(d.EntitledPerson.FileID)))
                && (!d.AgentID.HasValue || (d.Agent != null && fileIds.Contains(d.Agent.FileID)))
                && (!d.GuardianID.HasValue || (d.Guardian != null && fileIds.Contains(d.Guardian.FileID)))
                && (!d.PledgeID.HasValue || (d.Pledge != null && fileIds.Contains(d.Pledge.FileID)))
                && (!d.ExemptionID.HasValue || (d.Exemption != null && fileIds.Contains(d.Exemption.FileID)))
                && (!d.NoteID.HasValue || (d.Note != null && fileIds.Contains(d.Note.FileID)))
                && (!d.SuspensionID.HasValue || (d.Suspension != null && fileIds.Contains(d.Suspension.FileID)))
                && (!d.InvestmentProjectID.HasValue || (d.InvestmentProject != null && branches.Contains(d.InvestmentProject.PensionBranchID)))
                && (!d.InvestmentPropertyID.HasValue || (d.InvestmentProperty != null && branches.Contains(d.InvestmentProperty.PensionBranchID)))
                && (!d.PropertyUnitID.HasValue || (d.PropertyUnit != null && branches.Contains(d.PropertyUnit.Property.PensionBranchID)))
                && (!d.LeaseContractID.HasValue || (d.LeaseContract != null && branches.Contains(d.LeaseContract.Unit.Property.PensionBranchID)))
                && (!d.MaintenanceRequestID.HasValue || (d.MaintenanceRequest != null && branches.Contains(d.MaintenanceRequest.Property.PensionBranchID)))
                && (!d.WorkOrderID.HasValue || (d.WorkOrder != null && branches.Contains(d.WorkOrder.MaintenanceRequest.Property.PensionBranchID)))
                && (!d.AssetValuationID.HasValue || (d.AssetValuation != null && branches.Contains(d.AssetValuation.Property.PensionBranchID)))
                && (!d.DepreciationRecordID.HasValue || (d.DepreciationRecord != null && branches.Contains(d.DepreciationRecord.Property.PensionBranchID)))
                && (!d.PaymentScheduleID.HasValue || (d.PaymentSchedule != null && branches.Contains(d.PaymentSchedule.LeaseContract.Unit.Property.PensionBranchID)))
                && (!d.InvestmentPlanID.HasValue || (d.InvestmentPlan != null && branches.Contains(d.InvestmentPlan.Project.PensionBranchID)))
                && (!d.RiskAssessmentID.HasValue || (d.RiskAssessment != null && branches.Contains(d.RiskAssessment.Project.PensionBranchID)))
                && (!d.InvestmentPortfolioID.HasValue || (d.InvestmentPortfolio != null && d.InvestmentPortfolio.Projects.Any() && d.InvestmentPortfolio.Projects.All(p => branches.Contains(p.PensionBranchID))))
                && (!d.TenantID.HasValue || (d.Tenant != null && d.Tenant.LeaseContracts.Any() && d.Tenant.LeaseContracts.All(c => branches.Contains(c.Unit.Property.PensionBranchID))))
                && (!d.SalaryBatchID.HasValue || (d.SalaryBatch != null && d.SalaryBatch.SalaryPaymentItems.Any() && d.SalaryBatch.SalaryPaymentItems.All(i => fileIds.Contains(i.FileID))))
                && !d.TrackingRecordID.HasValue && !d.MinistryBatchID.HasValue && !d.DecisionID.HasValue && !d.SettlementBatchID.HasValue && !d.ReportID.HasValue && !d.EmployeeID.HasValue && !d.OperationalExpenseID.HasValue && !d.CorrespondenceID.HasValue && !d.ServiceRequestID.HasValue && !d.TicketMessageID.HasValue && !d.CitizenAccountID.HasValue),
            IQueryable<SocialStatus> q => q,
            IQueryable<HealthStatus> q => q,
            IQueryable<Relationship> q => q,
            IQueryable<Rank> q => q,
            IQueryable<FileStatus> q => q,
            // Global operational/accounting data needs a central permission scope.
            _ => query.Where(_ => false)
        };
        return (IQueryable<T>)result;
    }

    public async Task<bool> CanAccessDocumentOwnerAsync(Document owner)
    {
        ResolveBranch();
        if (scope.AllBranches) return true;
        if (owner.TrackingRecordID.HasValue || owner.MinistryBatchID.HasValue || owner.DecisionID.HasValue || owner.SettlementBatchID.HasValue || owner.ReportID.HasValue || owner.EmployeeID.HasValue || owner.OperationalExpenseID.HasValue || owner.CorrespondenceID.HasValue || owner.ServiceRequestID.HasValue || owner.TicketMessageID.HasValue || owner.CitizenAccountID.HasValue) return false;
        var hasOwner = false;
        if (owner.FileID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.RetirementFiles).AnyAsync(e => e.FileID == owner.FileID.Value)) return false;
        }
        if (owner.RetireeID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.Retirees).AnyAsync(e => e.RetireeID == owner.RetireeID.Value)) return false;
        }
        if (owner.EntitledID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.EntitledPersons).AnyAsync(e => e.EntitledID == owner.EntitledID.Value)) return false;
        }
        if (owner.AgentID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.Agents).AnyAsync(e => e.AgentID == owner.AgentID.Value)) return false;
        }
        if (owner.GuardianID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.Guardians).AnyAsync(e => e.GuardianID == owner.GuardianID.Value)) return false;
        }
        if (owner.PledgeID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.DocumentPledges).AnyAsync(e => e.PledgeID == owner.PledgeID.Value)) return false;
        }
        if (owner.ExemptionID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.DocumentExemptions).AnyAsync(e => e.ExemptionID == owner.ExemptionID.Value)) return false;
        }
        if (owner.NoteID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.FileNotes).AnyAsync(e => e.NoteID == owner.NoteID.Value)) return false;
        }
        if (owner.SuspensionID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.PensionSuspensions).AnyAsync(e => e.SuspensionID == owner.SuspensionID.Value)) return false;
        }
        if (owner.SalaryBatchID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.SalaryPaymentBatches).AnyAsync(e => e.BatchID == owner.SalaryBatchID.Value)) return false;
        }
        if (owner.MinistryBatchID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.MinistryApprovalBatches).AnyAsync(e => e.BatchID == owner.MinistryBatchID.Value)) return false;
        }
        if (owner.DecisionID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.SettlementDecisions).AnyAsync(e => e.DecisionID == owner.DecisionID.Value)) return false;
        }
        if (owner.SettlementBatchID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.SettlementsBatches).AnyAsync(e => e.BatchID == owner.SettlementBatchID.Value)) return false;
        }
        if (owner.InvestmentProjectID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.InvestmentProjects).AnyAsync(e => e.ProjectID == owner.InvestmentProjectID.Value)) return false;
        }
        if (owner.InvestmentPropertyID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.InvestmentProperties).AnyAsync(e => e.PropertyID == owner.InvestmentPropertyID.Value)) return false;
        }
        if (owner.PropertyUnitID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.PropertyUnits).AnyAsync(e => e.UnitID == owner.PropertyUnitID.Value)) return false;
        }
        if (owner.LeaseContractID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.LeaseContracts).AnyAsync(e => e.ContractID == owner.LeaseContractID.Value)) return false;
        }
        if (owner.InvestmentPortfolioID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.InvestmentPortfolios).AnyAsync(e => e.PortfolioID == owner.InvestmentPortfolioID.Value)) return false;
        }
        if (owner.MaintenanceRequestID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.MaintenanceRequests).AnyAsync(e => e.RequestID == owner.MaintenanceRequestID.Value)) return false;
        }
        if (owner.WorkOrderID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.WorkOrders).AnyAsync(e => e.WorkOrderID == owner.WorkOrderID.Value)) return false;
        }
        if (owner.AssetValuationID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.AssetValuations).AnyAsync(e => e.ValuationID == owner.AssetValuationID.Value)) return false;
        }
        if (owner.DepreciationRecordID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.DepreciationRecords).AnyAsync(e => e.DeprecID == owner.DepreciationRecordID.Value)) return false;
        }
        if (owner.PaymentScheduleID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.PaymentSchedules).AnyAsync(e => e.ScheduleID == owner.PaymentScheduleID.Value)) return false;
        }
        if (owner.InvestmentPlanID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.InvestmentPlans).AnyAsync(e => e.PlanID == owner.InvestmentPlanID.Value)) return false;
        }
        if (owner.RiskAssessmentID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.InvestmentRiskAssessments).AnyAsync(e => e.RiskID == owner.RiskAssessmentID.Value)) return false;
        }
        if (owner.TenantID.HasValue)
        {
            hasOwner = true;
            if (!await Query(unitOfWork.Tenants).AnyAsync(t => t.TenantID == owner.TenantID)) return false;
        }
        return hasOwner;
    }

    public async Task<bool> CanEditRetireeAsync(int retireeId)
    {
        ResolveBranch();
        if (scope.AllBranches) return true;
        var branches = scope.BranchIds.ToArray();
        return !await unitOfWork.RetirementFiles.Query().AnyAsync(f => !f.IsDeleted &&
            f.RetireeID == retireeId && !branches.Contains(f.PensionBranchID));
    }

    private IQueryable<AuditLog> ScopeAudit(IQueryable<AuditLog> query, int? branch)
    {
        // Audit payloads contain whole aggregate snapshots. A mixed-branch batch
        // may be listed with scoped totals, but its unredacted audit is HQ-only.
        var historical = new PensionDataQuery(unitOfWork, scope, readOnly, historicalOwnership: true);
        var files = historical.FileIds(branch);
        var approvals = historical.Query(unitOfWork.MinistryApprovalItems, branch).Select(i => i.ApprovalItemID);
        var settlements = historical.Query(unitOfWork.SettlementItems, branch).Select(i => i.SettlementItemID);
        // Include soft-deleted sources only to establish historical ownership, never to expose their rows.
        var keys = new Dictionary<int, IQueryable<int>>
        {
            [1] = historical.Query(unitOfWork.Retirees, branch).IgnoreQueryFilters().Select(e => e.RetireeID),
            [2] = historical.Query(unitOfWork.RetirementFiles, branch).IgnoreQueryFilters().Select(e => e.FileID),
            [3] = historical.Query(unitOfWork.EntitledPersons, branch).IgnoreQueryFilters().Select(e => e.EntitledID),
            [4] = historical.Query(unitOfWork.Agents, branch).IgnoreQueryFilters().Select(e => e.AgentID),
            [5] = historical.Query(unitOfWork.Guardians, branch).IgnoreQueryFilters().Select(e => e.GuardianID),
            [6] = historical.Query(unitOfWork.Documents, branch).IgnoreQueryFilters().Select(e => e.DocumentID),
            [7] = historical.Query(unitOfWork.DocumentPledges, branch).IgnoreQueryFilters().Select(e => e.PledgeID),
            [8] = historical.Query(unitOfWork.MissingDocuments, branch).IgnoreQueryFilters().Select(e => e.MissingDocumentID),
            [9] = historical.Query(unitOfWork.DocumentExemptions, branch).IgnoreQueryFilters().Select(e => e.ExemptionID),
            [10] = historical.Query(unitOfWork.FileNotes, branch).IgnoreQueryFilters().Select(e => e.NoteID),
            [11] = historical.Query(unitOfWork.PensionSuspensions, branch).IgnoreQueryFilters().Select(e => e.SuspensionID),
            [12] = historical.Query(unitOfWork.SalaryRecipients, branch).IgnoreQueryFilters().Select(e => e.RecipientID),
            [13] = historical.Query(unitOfWork.FinancialEntitlements, branch).IgnoreQueryFilters().Select(e => e.EntitlementID),
            [14] = historical.Query(unitOfWork.SalaryPaymentBatches, branch)
                .Where(b => b.SalaryPaymentItems.All(i => files.Contains(i.FileID)))
                .IgnoreQueryFilters().Select(e => e.BatchID),
            [15] = historical.Query(unitOfWork.SalaryPaymentItems, branch).IgnoreQueryFilters().Select(e => e.PaymentItemID),
            [16] = historical.Query(unitOfWork.MinistryApprovalBatches, branch)
                .Where(b => b.MinistryApprovalItems.All(i => approvals.Contains(i.ApprovalItemID)))
                .IgnoreQueryFilters().Select(e => e.BatchID),
            [17] = historical.Query(unitOfWork.MinistryApprovalItems, branch).IgnoreQueryFilters().Select(e => e.ApprovalItemID),
            [18] = historical.Query(unitOfWork.SettlementDecisions, branch)
                .Where(b => b.SettlementItems.All(i => settlements.Contains(i.SettlementItemID)))
                .IgnoreQueryFilters().Select(e => e.DecisionID),
            [19] = historical.Query(unitOfWork.SettlementsBatches, branch)
                .Where(b => b.SettlementItems.All(i => settlements.Contains(i.SettlementItemID)))
                .IgnoreQueryFilters().Select(e => e.BatchID),
            [20] = historical.Query(unitOfWork.SettlementItems, branch).IgnoreQueryFilters().Select(e => e.SettlementItemID),
            [21] = historical.Query(unitOfWork.Users, branch).IgnoreQueryFilters().Select(e => e.UserID)
        };
        Expression<Func<AuditLog, bool>> fallback = a => false;
        var parameter = fallback.Parameters[0];
        Expression body = fallback.Body;
        var record = Expression.Property(parameter, nameof(AuditLog.RecordID));
        foreach (var (table, ids) in keys)
        {
            var clause = Expression.AndAlso(
                Expression.Equal(Expression.Property(parameter, nameof(AuditLog.TableID)), Expression.Constant((int?)table, typeof(int?))),
                Expression.AndAlso(Expression.Property(record, "HasValue"),
                    Expression.Call(typeof(Queryable), nameof(Queryable.Contains), [typeof(int)],
                        ids.Expression, Expression.Property(record, "Value"))));
            body = Expression.OrElse(body, clause);
        }
        return query.Where(Expression.Lambda<Func<AuditLog, bool>>(body, parameter));
    }
}
