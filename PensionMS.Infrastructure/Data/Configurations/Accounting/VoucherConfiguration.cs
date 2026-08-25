using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Infrastructure.Data.Configurations.Accounting;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers");
        builder.HasKey(e => e.VoucherID);

        builder.Property(e => e.VoucherNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.Property(e => e.PayeeName).HasMaxLength(200);
        builder.Property(e => e.PayerName).HasMaxLength(200);
        builder.Property(e => e.ChequeNumber).HasMaxLength(100);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
        builder.Property(e => e.ReferralSource).HasMaxLength(200);
        builder.Property(e => e.JournalEntryNumber).HasMaxLength(30);

        builder.HasIndex(e => e.VoucherNumber).IsUnique();

        builder.HasOne(e => e.DebitAccount)
            .WithMany()
            .HasForeignKey(e => e.DebitAccountID)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.CreditAccount)
            .WithMany()
            .HasForeignKey(e => e.CreditAccountID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
