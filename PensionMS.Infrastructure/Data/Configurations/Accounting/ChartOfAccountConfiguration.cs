using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class ChartOfAccountConfiguration : IEntityTypeConfiguration<ChartOfAccount>
{
    public void Configure(EntityTypeBuilder<ChartOfAccount> builder)
    {
        builder.ToTable("ChartOfAccounts");
        builder.HasKey(e => e.AccountID);

        builder.Property(e => e.AccountCode).HasMaxLength(10).IsRequired();
        builder.Property(e => e.AccountName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.AccountNature).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => e.AccountCode).IsUnique();

        // Self-referencing hierarchy
        builder.HasOne(e => e.ParentAccount)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentAccountID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
