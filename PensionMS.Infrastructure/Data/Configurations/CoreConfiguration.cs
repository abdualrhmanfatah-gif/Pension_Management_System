using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class RetireeConfiguration : IEntityTypeConfiguration<Retiree>
{
    public void Configure(EntityTypeBuilder<Retiree> builder)
    {
        builder.HasKey(e => e.RetireeID);
        
        builder.Property(e => e.NationalID).HasMaxLength(20).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Governorate).HasMaxLength(100);
        builder.Property(e => e.MilitaryNumber).HasMaxLength(30);
        builder.Property(e => e.LinkNumber).HasMaxLength(50);

        builder.Property(e => e.PendingReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(PendingReviewStatus.None);

        builder.HasIndex(e => e.NationalID)
               .IsUnique()
               .HasFilter("[NationalID] <> '' AND [NationalID] IS NOT NULL AND [IsDeleted] = 0");

        builder.HasOne(r => r.ApprovalItem)
               .WithOne(m => m.Retiree)
               .HasForeignKey<Retiree>(r => r.ApprovalItemID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CreatedByUser).WithMany().HasForeignKey(r => r.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.UpdatedByUser).WithMany().HasForeignKey(r => r.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RetirementFileConfiguration : IEntityTypeConfiguration<RetirementFile>
{
    public void Configure(EntityTypeBuilder<RetirementFile> builder)
    {
        builder.HasKey(e => e.FileID);

        builder.Property(e => e.FileNumber).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.StatusReason).HasMaxLength(500);

        builder.Property(e => e.ReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(FileReviewStatus.None);
        
        builder.HasIndex(e => e.FileNumber)
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");

        builder.HasOne(f => f.CreatedByUser).WithMany().HasForeignKey(f => f.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(f => f.UpdatedByUser).WithMany().HasForeignKey(f => f.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
