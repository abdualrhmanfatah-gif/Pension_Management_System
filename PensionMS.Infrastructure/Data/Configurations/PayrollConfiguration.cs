using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class SalaryRecipientConfiguration : IEntityTypeConfiguration<SalaryRecipient>
{
    public void Configure(EntityTypeBuilder<SalaryRecipient> builder)
    {
        builder.HasKey(e => e.RecipientID);

        builder.Property(e => e.RecipientType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.SharePercent).HasPrecision(5, 2);

        builder.Property(e => e.PendingReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(PendingReviewStatus.None);

        builder.HasOne(s => s.RetirementFile).WithMany(f => f.SalaryRecipients).HasForeignKey(s => s.FileID);
        builder.HasOne(s => s.Retiree).WithMany(r => r.SalaryRecipients).HasForeignKey(s => s.RetireeID);
        builder.HasOne(s => s.Agent).WithMany(a => a.SalaryRecipients).HasForeignKey(s => s.AgentID);
        builder.HasOne(s => s.EntitledPerson).WithMany(e => e.SalaryRecipients).HasForeignKey(s => s.EntitledID);
        builder.HasOne(s => s.Guardian).WithMany(g => g.SalaryRecipients).HasForeignKey(s => s.GuardianID);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SalaryRecipient_TypeOrGuardianWithEntitled",
            @"( (CASE WHEN RetireeID IS NOT NULL THEN 1 ELSE 0 END +
                 CASE WHEN AgentID IS NOT NULL THEN 1 ELSE 0 END +
                 CASE WHEN EntitledID IS NOT NULL THEN 1 ELSE 0 END +
                 CASE WHEN GuardianID IS NOT NULL THEN 1 ELSE 0 END) = 1 )
              OR
              ( GuardianID IS NOT NULL AND EntitledID IS NOT NULL AND RetireeID IS NULL AND AgentID IS NULL )"
        ));

        builder.HasIndex(e => new { e.FileID, e.IsActive });
        builder.HasOne(s => s.DeletedByUser).WithMany().HasForeignKey(s => s.DeletedByUserID);

        builder.HasOne(s => s.CreatedByUser).WithMany().HasForeignKey(s => s.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.UpdatedByUser).WithMany().HasForeignKey(s => s.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FinancialEntitlementConfiguration : IEntityTypeConfiguration<FinancialEntitlement>
{
    public void Configure(EntityTypeBuilder<FinancialEntitlement> builder)
    {
        builder.HasKey(e => e.EntitlementID);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.HasOne(fe => fe.Retiree).WithMany(r => r.FinancialEntitlements).HasForeignKey(fe => fe.RetireeID);
        builder.HasIndex(e => new { e.RetireeID, e.EffectiveDate, e.EndDate, e.IsDeleted });
    }
}

public class SalaryPaymentBatchConfiguration : IEntityTypeConfiguration<SalaryPaymentBatch>
{
    public void Configure(EntityTypeBuilder<SalaryPaymentBatch> builder)
    {
        builder.HasKey(e => e.BatchID);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.HasOne(b => b.ImportedByUser).WithMany().HasForeignKey(b => b.ImportedByUserID);
        builder.HasOne(b => b.ApprovedByUser).WithMany().HasForeignKey(b => b.ApprovedByUserID);
    }
}

public class SalaryPaymentItemConfiguration : IEntityTypeConfiguration<SalaryPaymentItem>
{
    public void Configure(EntityTypeBuilder<SalaryPaymentItem> builder)
    {
        builder.HasKey(e => e.PaymentItemID);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.HasOne(s => s.RetirementFile).WithMany(f => f.SalaryPaymentItems).HasForeignKey(s => s.FileID);
        builder.HasOne(s => s.PaymentBook).WithMany(pb => pb.SalaryPaymentItems).HasForeignKey(s => s.PaymentBookID);
        builder.HasOne(s => s.Recipient).WithMany(sr => sr.SalaryPaymentItems).HasForeignKey(s => s.RecipientID);
        builder.HasIndex(e => e.BatchID);
        builder.HasIndex(e => new { e.FileID, e.BatchID, e.RecipientID }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
