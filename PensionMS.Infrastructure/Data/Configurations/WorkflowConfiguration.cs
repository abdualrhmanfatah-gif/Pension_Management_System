using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class PendingTaskConfiguration : IEntityTypeConfiguration<PendingTask>
{
    public void Configure(EntityTypeBuilder<PendingTask> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(p => p.AssignedToUser)
            .WithMany()
            .HasForeignKey(p => p.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.AssignedToRole)
            .WithMany()
            .HasForeignKey(p => p.AssignedToRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.File)
            .WithMany()
            .HasForeignKey(p => p.FileId)
            .IsRequired(false);

        builder.HasOne(p => p.SalaryBatch)
            .WithMany()
            .HasForeignKey(p => p.SalaryBatchID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DeletedByUser).WithMany().HasForeignKey(p => p.DeletedByUserID);
    }
}

public class RiskEventConfiguration : IEntityTypeConfiguration<RiskEvent>
{
    public void Configure(EntityTypeBuilder<RiskEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RiskType)
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.Severity)
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(e => e.File)
            .WithMany()
            .HasForeignKey(e => e.FileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.ResolvedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
