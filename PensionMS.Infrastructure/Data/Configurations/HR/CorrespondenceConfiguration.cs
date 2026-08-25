using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class CorrespondenceConfiguration : IEntityTypeConfiguration<Correspondence>
{
    public void Configure(EntityTypeBuilder<Correspondence> builder)
    {
        builder.ToTable("Correspondences");
        builder.HasKey(e => e.CorrespondenceID);

        builder.Property(e => e.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Subject).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.PageNumber).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.CorrespondenceType)
            .WithMany(t => t.Correspondences)
            .HasForeignKey(e => e.CorrespondenceTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ExternalParty)
            .WithMany(p => p.Correspondences)
            .HasForeignKey(e => e.ExternalPartyID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Employee)
            .WithMany(em => em.Correspondences)
            .HasForeignKey(e => e.EmployeeID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.Direction, e.CorrespondenceTypeID, e.CorrespondenceDate });
        
        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
