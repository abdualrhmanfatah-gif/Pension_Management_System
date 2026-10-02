using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class MinistryApprovalBatchConfiguration : IEntityTypeConfiguration<MinistryApprovalBatch>
{
    public void Configure(EntityTypeBuilder<MinistryApprovalBatch> builder)
    {
        builder.HasKey(e => e.BatchID);
    }
}

public class MinistryApprovalItemConfiguration : IEntityTypeConfiguration<MinistryApprovalItem>
{
    public void Configure(EntityTypeBuilder<MinistryApprovalItem> builder)
    {
        builder.HasKey(e => e.ApprovalItemID);
        builder.Property(e => e.Salary).HasPrecision(18, 2);
    }
}

public class SettlementDecisionConfiguration : IEntityTypeConfiguration<SettlementDecision>
{
    public void Configure(EntityTypeBuilder<SettlementDecision> builder)
    {
        builder.HasKey(e => e.DecisionID);
    }
}

public class SettlementsBatchConfiguration : IEntityTypeConfiguration<SettlementsBatch>
{
    public void Configure(EntityTypeBuilder<SettlementsBatch> builder)
    {
        builder.HasKey(e => e.BatchID);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
    }
}

public class SettlementItemConfiguration : IEntityTypeConfiguration<SettlementItem>
{
    public void Configure(EntityTypeBuilder<SettlementItem> builder)
    {
        builder.HasKey(e => e.SettlementItemID);

        builder.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Deductions).HasPrecision(18, 2);
        builder.Property(e => e.Notes).HasMaxLength(500);

        // Composite unique index to allow one settlement per person PER BATCH
        builder.HasIndex(e => new { e.ApprovalItemID, e.BatchID })
               .IsUnique()
               .HasFilter("[ApprovalItemID] IS NOT NULL AND [IsDeleted] = 0");

        // Relationship changed to WithMany since one approval item can have multiple settlements (in different batches)
        builder.HasOne(s => s.ApprovalItem)
               .WithMany(a => a.SettlementItems)
               .HasForeignKey(s => s.ApprovalItemID)
               .IsRequired(false);

        builder.HasOne(s => s.Retiree)
               .WithMany(r => r.SettlementItems)
               .HasForeignKey(s => s.RetireeID)
               .IsRequired(false);

        // Direct fields for when no ApprovalItem is linked
        builder.Property(s => s.MilitaryNumber).HasMaxLength(50);
        builder.Property(s => s.FullName).HasMaxLength(200);

        builder.HasOne(s => s.CurrentRank).WithMany(r => r.CurrentRankSettlements).HasForeignKey(s => s.CurrentRankID);
        builder.HasOne(s => s.ProcessedRank).WithMany(r => r.ProcessedRankSettlements).HasForeignKey(s => s.ProcessedRankID);

        builder.HasOne(s => s.CreatedByUser).WithMany().HasForeignKey(s => s.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.UpdatedByUser).WithMany().HasForeignKey(s => s.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
