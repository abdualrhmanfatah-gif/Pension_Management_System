using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.HasKey(e => e.BankID);

        builder.Property(e => e.BankName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.BankCode).HasMaxLength(20);

        builder.HasData(
            new Bank { BankID = 1, BankName = "البنك المركزي", BankCode = "CB", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
