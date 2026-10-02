using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Security;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(e => e.RolePermissionId);

        builder.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        var rps = new List<RolePermission>();
        int id = 1000;
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        void Map(int roleId, params int[] permissionIds)
        {
            foreach (var pid in permissionIds)
                rps.Add(new RolePermission { RolePermissionId = id++, RoleId = roleId, PermissionId = pid, CreatedAt = seedDate });
        }

        // Role 1: Admin — all permissions 1-142 (excluding 117-118 Treasury)
        Map(1, Enumerable.Range(1, 116).Concat(Enumerable.Range(119, 142 - 119 + 1)).ToArray());

        // Role 2: PensionManager
        // Added 112 (History), 125-127 (File Review), 140-142 (Accounting View)
        Map(2, 1, 2, 4, 5, 6, 7, 9, 13, 17, 18, 19, 20, 21, 22, 23, 49, 54, 77, 24, 25, 26, 27, 28, 50, 55, 78, 29, 30, 60, 84, 85, 31, 32, 52, 57, 80, 33, 34, 53, 56, 79, 35, 36, 61, 86, 87, 37, 38, 51, 58, 81, 39, 40, 62, 90, 91, 41, 42, 63, 65, 83, 88, 89, 46, 82, 66, 43, 112, 121, 122, 123, 125, 126, 127, 140, 142);

        // Role 3: PensionClerk
        Map(3, 1, 2, 4, 5, 17, 18, 19, 22, 23, 49, 54, 24, 25, 27, 28, 50, 55, 29, 30, 60, 84, 31, 32, 52, 57, 33, 34, 53, 56, 35, 36, 61, 86, 37, 38, 51, 58, 46, 47, 48, 59, 43, 121, 122);

        // Role 4: FinanceOfficer
        // Added 119-120 (Budget Structure), 140-142 (Accounting)
        Map(4, 3, 4, 11, 12, 17, 22, 49, 54, 24, 27, 50, 55, 29, 31, 52, 57, 33, 53, 56, 35, 37, 38, 51, 58, 39, 41, 42, 63, 65, 43, 44, 45, 64, 88, 46, 47, 48, 59, 66, 67, 68, 69, 70, 71, 72, 92, 93, 73, 75, 95, 96, 98, 99, 113, 114, 115, 116, 117, 118, 119, 120, 121, 140, 141, 142);

        // Role 5: Auditor — added 125-127 (File Review), 140, 142 (Accounting View + Reports)
        Map(5, 4, 13, 17, 22, 24, 27, 29, 31, 33, 35, 37, 39, 41, 43, 44, 46, 66, 71, 73, 113, 117, 121, 125, 126, 127, 140, 142);

        // Role 6: Archivist
        Map(6, 5, 17, 22, 24, 27, 29, 30, 60, 84, 31, 32, 52, 57, 33, 34, 53, 56, 35, 36, 61, 86, 37, 43, 121, 122, 123);

        // Role 7: Technician
        Map(7, 73, 74, 95, 96, 43);

        
        builder.HasData(rps);
    }
}
