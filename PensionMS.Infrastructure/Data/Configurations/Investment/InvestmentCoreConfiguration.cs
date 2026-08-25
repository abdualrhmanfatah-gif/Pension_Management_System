using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;

namespace PensionMS.Infrastructure.Data.Configurations.Investment;

public class InvestmentPortfolioConfiguration : IEntityTypeConfiguration<InvestmentPortfolio>
{
    public void Configure(EntityTypeBuilder<InvestmentPortfolio> builder)
    {
        builder.ToTable("InvestmentPortfolios", t => t.HasCheckConstraint("CK_Portfolio_TargetCapital", "TotalTargetCapital >= 0"));
        builder.HasKey(e => e.PortfolioID);
        builder.HasIndex(e => e.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.Property(e => e.TotalTargetCapital).HasPrecision(18, 2);
        builder.HasOne(e => e.Manager).WithMany().HasForeignKey(e => e.ManagerUserID);
    }
}

public class InvestmentProjectConfiguration : IEntityTypeConfiguration<InvestmentProject>
{
    public void Configure(EntityTypeBuilder<InvestmentProject> builder)
    {
        builder.ToTable("InvestmentProjects", t => 
        {
            t.HasCheckConstraint("CK_Project_Budget", "PlannedBudget >= 0");
            t.HasCheckConstraint("CK_Project_Dates", "EndDate IS NULL OR EndDate > StartDate");
        });
        builder.HasKey(e => e.ProjectID);
        builder.HasIndex(e => new { e.PortfolioID, e.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.Property(e => e.PlannedBudget).HasPrecision(18, 2);
        builder.Property(e => e.ExpectedROI).HasPrecision(18, 2);
        builder.HasOne(e => e.Portfolio).WithMany(p => p.Projects).HasForeignKey(e => e.PortfolioID);
        builder.HasOne(e => e.InvestmentType).WithMany(t => t.Projects).HasForeignKey(e => e.TypeID);
    }
}

public class InvestmentPlanConfiguration : IEntityTypeConfiguration<InvestmentPlan>
{
    public void Configure(EntityTypeBuilder<InvestmentPlan> builder)
    {
        builder.ToTable("InvestmentPlans", t => t.HasCheckConstraint("CK_Plan_Amounts", "PlannedAmount >= 0 AND ActualSpent >= 0"));
        builder.HasKey(e => e.PlanID);
        builder.HasIndex(e => new { e.ProjectID, e.FiscalYearID }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.Property(e => e.PlannedAmount).HasPrecision(18, 2);
        builder.Property(e => e.ActualSpent).HasPrecision(18, 2);
        builder.HasOne(e => e.Project).WithMany(p => p.Plans).HasForeignKey(e => e.ProjectID);
        builder.HasOne(e => e.FiscalYear).WithMany().HasForeignKey(e => e.FiscalYearID);
    }
}

public class InvestmentPropertyConfiguration : IEntityTypeConfiguration<InvestmentProperty>
{
    public void Configure(EntityTypeBuilder<InvestmentProperty> builder)
    {
        builder.ToTable("InvestmentProperties", t => t.HasCheckConstraint("CK_Property_AreaValue", "TotalArea > 0 AND InitialValue >= 0"));
        builder.HasKey(e => e.PropertyID);
        builder.Property(e => e.Latitude).HasPrecision(10, 7);
        builder.Property(e => e.Longitude).HasPrecision(10, 7);
        builder.Property(e => e.TotalArea).HasPrecision(18, 2);
        builder.Property(e => e.InitialValue).HasPrecision(18, 2);
        builder.HasOne(e => e.Project).WithMany(p => p.Properties).HasForeignKey(e => e.ProjectID).IsRequired(false);
        builder.HasOne(e => e.PropertyType).WithMany(t => t.Properties).HasForeignKey(e => e.PropertyTypeID);
    }
}
