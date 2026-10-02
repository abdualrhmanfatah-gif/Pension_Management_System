using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Security;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.UserID);

        builder.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Username).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(254);
        builder.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(e => e.RefreshToken).HasMaxLength(500);

        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasOne(u => u.CreatedByUser).WithMany(u => u.CreatedUsers).HasForeignKey(u => u.CreatedByUserID);
        builder.HasOne(u => u.DeletedByUser).WithMany(u => u.DeletedUsers).HasForeignKey(u => u.DeletedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.HasKey(e => e.LoginID);
    }
}
