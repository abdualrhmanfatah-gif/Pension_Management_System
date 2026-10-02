using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable("AccountBalances");
        builder.HasKey(e => e.BalanceID);

        builder.HasIndex(e => new { e.AccountID, e.PeriodID }).IsUnique();

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Period)
            .WithMany()
            .HasForeignKey(e => e.PeriodID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
