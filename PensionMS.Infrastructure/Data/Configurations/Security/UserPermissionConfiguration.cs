using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Security;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.User)
            .WithMany(u => u.UserPermissions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.GrantedByUser)
            .WithMany()
            .HasForeignKey(e => e.GrantedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();
    }
}
