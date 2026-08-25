using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryTypeConfiguration : IEntityTypeConfiguration<JournalEntryType>
{
    public void Configure(EntityTypeBuilder<JournalEntryType> builder)
    {
        builder.ToTable("JournalEntryTypes");
        builder.HasKey(e => e.JournalEntryTypeID);

        builder.Property(e => e.TypeName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.TypeNameEn).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(20);
        builder.Property(e => e.Color).HasMaxLength(7);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.TypeName).IsUnique();

        // ISoftDeletable
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
