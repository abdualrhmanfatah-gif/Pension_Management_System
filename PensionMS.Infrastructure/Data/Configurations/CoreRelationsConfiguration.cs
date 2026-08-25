using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class FileNoteConfiguration : IEntityTypeConfiguration<FileNote>
{
    public void Configure(EntityTypeBuilder<FileNote> builder)
    {
        builder.HasKey(e => e.NoteID);
        builder.HasOne(f => f.CreatedByUser).WithMany().HasForeignKey(f => f.CreatedByUserID);
        builder.HasOne(f => f.ResolvedByUser).WithMany().HasForeignKey(f => f.ResolvedByUserID);
        builder.HasOne(fn => fn.RetirementFile).WithMany(f => f.FileNotes).HasForeignKey(fn => fn.FileID);
    }
}

public class PensionSuspensionConfiguration : IEntityTypeConfiguration<PensionSuspension>
{
    public void Configure(EntityTypeBuilder<PensionSuspension> builder)
    {
        builder.HasKey(e => e.SuspensionID);
        builder.HasOne(p => p.CreatedByUser).WithMany().HasForeignKey(p => p.CreatedByUserID);
        builder.HasOne(p => p.EndedByUser).WithMany().HasForeignKey(p => p.EndedByUserID);
        builder.HasOne(p => p.UpdatedByUser).WithMany().HasForeignKey(p => p.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.RetirementFile).WithMany(f => f.PensionSuspensions).HasForeignKey(p => p.FileID);
    }
}

public class EntitledPersonConfiguration : IEntityTypeConfiguration<EntitledPerson>
{
    public void Configure(EntityTypeBuilder<EntitledPerson> builder)
    {
        builder.HasKey(e => e.EntitledID);

        builder.Property(e => e.FullName).HasMaxLength(200);
        builder.Property(e => e.NationalID).HasMaxLength(20);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.StopCode).HasMaxLength(50);

        builder.Property(e => e.PendingReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(PendingReviewStatus.None);

        builder.HasIndex(e => new { e.FileID, e.NationalID })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0 AND [NationalID] <> '' AND [NationalID] IS NOT NULL");
        builder.HasOne(e => e.RetirementFile).WithMany(f => f.EntitledPersons).HasForeignKey(e => e.FileID);

        builder.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        builder.HasKey(e => e.GuardianID);

        builder.Property(e => e.FullName).HasMaxLength(200);
        builder.Property(e => e.NationalID).HasMaxLength(20);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.GuardianshipDocNumber).HasMaxLength(100);

        builder.Property(e => e.PendingReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(PendingReviewStatus.None);

        builder.HasOne(g => g.RetirementFile).WithMany(f => f.Guardians).HasForeignKey(g => g.FileID);

        builder.HasOne(g => g.CreatedByUser).WithMany().HasForeignKey(g => g.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(g => g.UpdatedByUser).WithMany().HasForeignKey(g => g.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GuardianEntitledPersonConfiguration : IEntityTypeConfiguration<GuardianEntitledPerson>
{
    public void Configure(EntityTypeBuilder<GuardianEntitledPerson> builder)
    {
        builder.HasKey(ge => new { ge.GuardianID, ge.EntitledID });

        builder.HasOne(ge => ge.Guardian)
            .WithMany(g => g.GuardianEntitledPersons)
            .HasForeignKey(ge => ge.GuardianID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ge => ge.EntitledPerson)
            .WithMany(e => e.GuardianEntitledPersons)
            .HasForeignKey(ge => ge.EntitledID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MissingDocumentConfiguration : IEntityTypeConfiguration<MissingDocument>
{
    public void Configure(EntityTypeBuilder<MissingDocument> builder)
    {
        builder.HasKey(e => e.MissingDocumentID);
        builder.HasOne(m => m.RetirementFile).WithMany(f => f.MissingDocuments).HasForeignKey(m => m.FileID);
        builder.HasOne(m => m.DocumentType).WithMany(dt => dt.MissingDocuments).HasForeignKey(m => m.DocumentTypeID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Agent).WithMany().HasForeignKey(m => m.AgentID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Guardian).WithMany().HasForeignKey(m => m.GuardianID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.EntitledPerson).WithMany().HasForeignKey(m => m.EntitledID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.DeletedByUser).WithMany().HasForeignKey(m => m.DeletedByUserID);
    }
}

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasKey(e => e.AgentID);

        builder.Property(e => e.FullName).HasMaxLength(200);
        builder.Property(e => e.NationalID).HasMaxLength(20);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.AgencyNumber).HasMaxLength(100).IsRequired(false);

        builder.Property(e => e.PendingReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(PendingReviewStatus.None);

        builder.HasOne(a => a.RetirementFile).WithMany(f => f.Agents).HasForeignKey(a => a.FileID);

        builder.HasOne(a => a.CreatedByUser).WithMany().HasForeignKey(a => a.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.UpdatedByUser).WithMany().HasForeignKey(a => a.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
