using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(e => e.DocumentID);

        builder.Property(e => e.DocumentNumber).HasMaxLength(50);
        builder.Property(e => e.FileName).HasMaxLength(255);
        builder.Property(e => e.FilePath).HasMaxLength(500);
        builder.Property(e => e.MimeType).HasMaxLength(100);
        builder.Property(e => e.Checksum).HasMaxLength(64);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.Property(e => e.PendingReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(PendingReviewStatus.None);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Document_OwnerType",
            @"(OwnerType = 1  AND FileID IS NOT NULL)           OR
              (OwnerType = 2  AND RetireeID IS NOT NULL)         OR
              (OwnerType = 3  AND EntitledID IS NOT NULL)        OR
              (OwnerType = 4  AND AgentID IS NOT NULL)           OR
              (OwnerType = 5  AND GuardianID IS NOT NULL)        OR
              (OwnerType = 6  AND PledgeID IS NOT NULL)          OR
              (OwnerType = 7  AND ExemptionID IS NOT NULL)       OR
              (OwnerType = 8  AND NoteID IS NOT NULL)            OR
              (OwnerType = 9  AND SuspensionID IS NOT NULL)      OR
              (OwnerType = 10 AND SalaryBatchID IS NOT NULL)     OR
              (OwnerType = 11 AND MinistryBatchID IS NOT NULL)   OR
              (OwnerType = 12 AND DecisionID IS NOT NULL)        OR
              (OwnerType = 13 AND SettlementBatchID IS NOT NULL) OR
              (OwnerType = 14 AND ReportID IS NOT NULL)          OR
              (OwnerType = 15 AND InvestmentProjectID IS NOT NULL)  OR
              (OwnerType = 16 AND InvestmentPropertyID IS NOT NULL) OR
              (OwnerType = 17 AND PropertyUnitID IS NOT NULL)       OR
              (OwnerType = 18 AND LeaseContractID IS NOT NULL)      OR
              (OwnerType = 19 AND TenantID IS NOT NULL)             OR
              (OwnerType = 20 AND InvestmentPortfolioID IS NOT NULL) OR
              (OwnerType = 21 AND MaintenanceRequestID IS NOT NULL)  OR
              (OwnerType = 22 AND WorkOrderID IS NOT NULL)           OR
              (OwnerType = 25 AND AssetValuationID IS NOT NULL)      OR
              (OwnerType = 27 AND DepreciationRecordID IS NOT NULL)  OR
              (OwnerType = 28 AND PaymentScheduleID IS NOT NULL)     OR
              (OwnerType = 30 AND OperationalExpenseID IS NOT NULL)  OR
              (OwnerType = 31 AND InvestmentPlanID IS NOT NULL)      OR
              (OwnerType = 33 AND RiskAssessmentID IS NOT NULL)      OR
              (OwnerType = 40 AND EmployeeID IS NOT NULL)             OR
              (OwnerType = 41 AND CorrespondenceID IS NOT NULL)       OR
              (OwnerType = 50 AND ServiceRequestID IS NOT NULL)       OR
              (OwnerType = 51 AND TicketMessageID IS NOT NULL)        OR
              (OwnerType = 52 AND CitizenAccountID IS NOT NULL)"));

        builder.HasOne(d => d.Retiree).WithMany(r => r.Documents).HasForeignKey(d => d.RetireeID);
        builder.HasOne(d => d.EntitledPerson).WithMany(e => e.Documents).HasForeignKey(d => d.EntitledID);
        builder.HasOne(d => d.RetirementFile).WithMany(f => f.Documents).HasForeignKey(d => d.FileID);
        builder.HasOne(d => d.Agent).WithMany(a => a.Documents).HasForeignKey(d => d.AgentID);
        builder.HasOne(d => d.Guardian).WithMany(g => g.Documents).HasForeignKey(d => d.GuardianID);
        builder.HasOne(d => d.Pledge).WithMany(p => p.Documents).HasForeignKey(d => d.PledgeID);
        builder.HasOne(d => d.Exemption).WithMany(e => e.Documents).HasForeignKey(d => d.ExemptionID);
        builder.HasOne(d => d.Note).WithMany(n => n.Documents).HasForeignKey(d => d.NoteID);
        builder.HasOne(d => d.Suspension).WithMany(s => s.Documents).HasForeignKey(d => d.SuspensionID);
        builder.HasOne(d => d.SalaryBatch).WithMany(b => b.Documents).HasForeignKey(d => d.SalaryBatchID);
        builder.HasOne(d => d.MinistryBatch).WithMany(b => b.Documents).HasForeignKey(d => d.MinistryBatchID);
        builder.HasOne(d => d.Decision).WithMany(d => d.Documents).HasForeignKey(d => d.DecisionID);
        builder.HasOne(d => d.SettlementBatch).WithMany(b => b.Documents).HasForeignKey(d => d.SettlementBatchID);
        builder.HasOne(d => d.Report).WithMany(r => r.Documents).HasForeignKey(d => d.ReportID);
        builder.HasOne(d => d.InvestmentProject).WithMany(p => p.Documents).HasForeignKey(d => d.InvestmentProjectID);
        builder.HasOne(d => d.InvestmentProperty).WithMany(p => p.Documents).HasForeignKey(d => d.InvestmentPropertyID);
        builder.HasOne(d => d.PropertyUnit).WithMany(u => u.Documents).HasForeignKey(d => d.PropertyUnitID);
        builder.HasOne(d => d.LeaseContract).WithMany(c => c.Documents).HasForeignKey(d => d.LeaseContractID);
        builder.HasOne(d => d.Tenant).WithMany(t => t.Documents).HasForeignKey(d => d.TenantID);
        builder.HasOne(d => d.InvestmentPortfolio).WithMany().HasForeignKey(d => d.InvestmentPortfolioID);
        builder.HasOne(d => d.MaintenanceRequest).WithMany().HasForeignKey(d => d.MaintenanceRequestID);
        builder.HasOne(d => d.WorkOrder).WithMany().HasForeignKey(d => d.WorkOrderID);
        builder.HasOne(d => d.AssetValuation).WithMany(v => v.Documents).HasForeignKey(d => d.AssetValuationID);
        builder.HasOne(d => d.DepreciationRecord).WithMany().HasForeignKey(d => d.DepreciationRecordID);
        builder.HasOne(d => d.PaymentSchedule).WithMany().HasForeignKey(d => d.PaymentScheduleID);
        builder.HasOne(d => d.OperationalExpense).WithMany(e => e.Documents).HasForeignKey(d => d.OperationalExpenseID);
        builder.HasOne(d => d.InvestmentPlan).WithMany().HasForeignKey(d => d.InvestmentPlanID);
        builder.HasOne(d => d.RiskAssessment).WithMany().HasForeignKey(d => d.RiskAssessmentID);
        builder.HasOne(d => d.Employee).WithMany(e => e.Documents).HasForeignKey(d => d.EmployeeID);
        builder.HasOne(d => d.Correspondence).WithMany(c => c.Documents).HasForeignKey(d => d.CorrespondenceID).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(d => d.ServiceRequest).WithMany(s => s.Documents).HasForeignKey(d => d.ServiceRequestID);
        builder.HasOne(d => d.TicketMessage).WithMany(t => t.Documents).HasForeignKey(d => d.TicketMessageID);
        builder.HasOne(d => d.CitizenAccount).WithMany().HasForeignKey(d => d.CitizenAccountID);
        builder.HasOne(d => d.DocumentType).WithMany(t => t.Documents).HasForeignKey(d => d.DocumentTypeID);            
        builder.HasOne(d => d.VerifiedByUser).WithMany().HasForeignKey(d => d.VerifiedByUserID);
        builder.HasOne(d => d.UploadedByUser).WithMany().HasForeignKey(d => d.UploadedByUserID);
    }
}

public class DocumentPledgeConfiguration : IEntityTypeConfiguration<DocumentPledge>
{
    public void Configure(EntityTypeBuilder<DocumentPledge> builder)
    {
        builder.HasKey(e => e.PledgeID);
        builder.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedByUserID);
        builder.HasOne(d => d.ExtendedByUser).WithMany().HasForeignKey(d => d.ExtendedByUserID);
        builder.HasOne(d => d.RetirementFile).WithMany(f => f.DocumentPledges).HasForeignKey(d => d.FileID);
    }
}

public class DocumentExemptionConfiguration : IEntityTypeConfiguration<DocumentExemption>
{
    public void Configure(EntityTypeBuilder<DocumentExemption> builder)
    {
        builder.HasKey(e => e.ExemptionID);
        builder.HasOne(d => d.RetirementFile).WithMany(f => f.DocumentExemptions).HasForeignKey(d => d.FileID);
        builder.HasOne(d => d.DeletedByUser).WithMany().HasForeignKey(d => d.DeletedByUserID);
    }
}
