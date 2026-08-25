using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;

namespace PensionMS.Infrastructure.Data.Configurations.Investment;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.ToTable("MaintenanceRequests");
        builder.HasKey(e => e.RequestID);
        builder.HasOne(e => e.Property).WithMany(p => p.MaintenanceRequests).HasForeignKey(e => e.PropertyID);
        builder.HasOne(e => e.Unit).WithMany(u => u.MaintenanceRequests).HasForeignKey(e => e.UnitID).IsRequired(false);
        builder.HasOne(e => e.Tenant).WithMany(t => t.MaintenanceRequests).HasForeignKey(e => e.TenantID).IsRequired(false);
    }
}

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders", t => t.HasCheckConstraint("CK_WorkOrder_Costs", "EstimatedCost >= 0 AND ActualCost >= 0"));
        builder.HasKey(e => e.WorkOrderID);
        builder.Property(e => e.EstimatedCost).HasPrecision(18, 2);
        builder.Property(e => e.ActualCost).HasPrecision(18, 2);
        builder.HasOne(e => e.MaintenanceRequest).WithMany(r => r.WorkOrders).HasForeignKey(e => e.RequestID);
    }
}

public class InvestmentRiskAssessmentConfiguration : IEntityTypeConfiguration<InvestmentRiskAssessment>
{
    public void Configure(EntityTypeBuilder<InvestmentRiskAssessment> builder)
    {
        builder.ToTable("RiskAssessments");
        builder.HasKey(e => e.RiskID);
        builder.HasOne(e => e.Project).WithMany(p => p.RiskAssessments).HasForeignKey(e => e.ProjectID);
    }
}

public class InvestmentApprovalRequestConfiguration : IEntityTypeConfiguration<InvestmentApprovalRequest>
{
    public void Configure(EntityTypeBuilder<InvestmentApprovalRequest> builder)
    {
        builder.ToTable("ApprovalRequests");
        builder.HasKey(e => e.ApprovalID);
        builder.HasOne(e => e.RequestedBy).WithMany().HasForeignKey(e => e.RequestedByUserID);
    }
}
