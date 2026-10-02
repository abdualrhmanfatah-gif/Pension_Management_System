using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Investment;

namespace PensionMS.Infrastructure.Data.Configurations.Investment;

public class PropertyUnitConfiguration : IEntityTypeConfiguration<PropertyUnit>
{
    public void Configure(EntityTypeBuilder<PropertyUnit> builder)
    {
        builder.ToTable("PropertyUnits", t => t.HasCheckConstraint("CK_Unit_Area", "Area > 0"));
        builder.HasKey(e => e.UnitID);
        builder.HasIndex(e => new { e.PropertyID, e.UnitNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.Property(e => e.Area).HasPrecision(18, 2);
        builder.HasOne(e => e.Property).WithMany(p => p.Units).HasForeignKey(e => e.PropertyID);
        builder.HasOne(e => e.UnitType).WithMany(t => t.Units).HasForeignKey(e => e.UnitTypeID);
    }
}

public class AssetValuationConfiguration : IEntityTypeConfiguration<AssetValuation>
{
    public void Configure(EntityTypeBuilder<AssetValuation> builder)
    {
        builder.ToTable("AssetValuations");
        builder.HasKey(e => e.ValuationID);
        builder.Property(e => e.MarketValue).HasPrecision(18, 2);
        builder.HasOne(e => e.Property).WithMany(p => p.Valuations).HasForeignKey(e => e.PropertyID);
    }
}

public class DepreciationRecordConfiguration : IEntityTypeConfiguration<DepreciationRecord>
{
    public void Configure(EntityTypeBuilder<DepreciationRecord> builder)
    {
        builder.ToTable("DepreciationRecords", t => t.HasCheckConstraint("CK_Depreciation_Rate", "DepreciationRate >= 0 AND DepreciationRate <= 100"));
        builder.HasKey(e => e.DeprecID);
        builder.HasIndex(e => new { e.PropertyID, e.FiscalYearID }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.Property(e => e.DepreciationRate).HasPrecision(18, 2);
        builder.Property(e => e.DepreciationAmount).HasPrecision(18, 2);
        builder.Property(e => e.NetBookValue).HasPrecision(18, 2);
        builder.HasOne(e => e.Property).WithMany(p => p.DepreciationRecords).HasForeignKey(e => e.PropertyID);
        builder.HasOne(e => e.FiscalYear).WithMany().HasForeignKey(e => e.FiscalYearID);
    }
}

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", t => t.HasCheckConstraint("CK_Tenant_Rating", "RatingScore >= 0 AND RatingScore <= 5"));
        builder.HasKey(e => e.TenantID);
        builder.HasIndex(e => e.NationalIDOrCR).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.Property(e => e.RatingScore).HasPrecision(18, 2);
    }
}

public class LeaseContractConfiguration : IEntityTypeConfiguration<LeaseContract>
{
    public void Configure(EntityTypeBuilder<LeaseContract> builder)
    {
        builder.ToTable("LeaseContracts", t => 
        {
            t.HasCheckConstraint("CK_Lease_Dates", "EndDate > StartDate");
            t.HasCheckConstraint("CK_Lease_Value", "TotalContractValue >= 0");
        });
        builder.HasKey(e => e.ContractID);
        builder.HasIndex(e => e.UnitID).IsUnique().HasFilter("[Status] = 'Active' AND [IsDeleted] = 0");
        builder.Property(e => e.TotalContractValue).HasPrecision(18, 2);
        builder.HasOne(e => e.Unit).WithMany(u => u.LeaseContracts).HasForeignKey(e => e.UnitID);
        builder.HasOne(e => e.Tenant).WithMany(t => t.LeaseContracts).HasForeignKey(e => e.TenantID);
    }
}

public class PaymentScheduleConfiguration : IEntityTypeConfiguration<PaymentSchedule>
{
    public void Configure(EntityTypeBuilder<PaymentSchedule> builder)
    {
        builder.ToTable("PaymentSchedules", t => t.HasCheckConstraint("CK_Payment_Amounts", "PaidAmount <= ExpectedAmount AND ExpectedAmount > 0"));
        builder.HasKey(e => e.ScheduleID);
        builder.Property(e => e.ExpectedAmount).HasPrecision(18, 2);
        builder.Property(e => e.PaidAmount).HasPrecision(18, 2);
        builder.HasOne(e => e.LeaseContract).WithMany(c => c.PaymentSchedules).HasForeignKey(e => e.ContractID);
    }
}
