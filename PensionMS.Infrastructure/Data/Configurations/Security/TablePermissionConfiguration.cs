using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Security;

public class TablePermissionConfiguration : IEntityTypeConfiguration<TablePermission>
{
    public void Configure(EntityTypeBuilder<TablePermission> builder)
    {
        builder.HasKey(e => e.PermissionID);

        builder.HasOne(tp => tp.SystemTable)
            .WithMany(st => st.TablePermissions)
            .HasForeignKey(tp => tp.TableID);

        var permissions = new List<TablePermission>();
        int id = 1;
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        void Add(int roleId, int tableId, bool c, bool r, bool u, bool d)
        {
            permissions.Add(new TablePermission
            {
                PermissionID = id++,
                RoleID = roleId,
                TableID = tableId,
                CanCreate = c,
                CanRead = r,
                CanUpdate = u,
                CanDelete = d,
                CreatedAt = seedDate
            });
        }

        // Role 1: Admin — CRUD on everything (1-43)
        for (int t = 1; t <= 43; t++)
            Add(1, t, true, true, true, true);

        // Role 2: PensionManager
        foreach (var t in new[] { 1, 2, 3, 4, 5 }) Add(2, t, true, true, true, false);
        foreach (var t in new[] { 6, 7, 8, 9 }) Add(2, t, true, true, true, false);
        foreach (var t in new[] { 10, 11 }) Add(2, t, true, true, true, false);
        foreach (var t in new[] { 12, 13, 14, 15 }) Add(2, t, false, true, false, false);
        foreach (var t in new[] { 16, 17 }) Add(2, t, true, true, true, false);
        foreach (var t in new[] { 18, 19, 20 }) Add(2, t, false, true, false, false);
        foreach (var t in new[] { 21, 22, 23, 24, 25, 26 }) Add(2, t, false, true, false, false);
        for (int t = 29; t <= 43; t++) Add(2, t, false, true, false, false);

        // Role 3: PensionClerk
        foreach (var t in new[] { 1, 2, 3, 4, 5 }) Add(3, t, true, true, true, false);
        foreach (var t in new[] { 6, 7, 8, 9 }) Add(3, t, true, true, true, false);
        foreach (var t in new[] { 10, 11 }) Add(3, t, true, true, true, false);
        foreach (var t in new[] { 12, 13, 14, 15 }) Add(3, t, false, true, false, false);
        foreach (var t in new[] { 16, 17 }) Add(3, t, false, true, false, false);
        foreach (var t in new[] { 18, 19, 20 }) Add(3, t, false, true, false, false);
        for (int t = 29; t <= 43; t++) Add(3, t, false, true, false, false);

        // Role 4: FinanceOfficer
        foreach (var t in new[] { 1, 2, 3, 4, 5 }) Add(4, t, false, true, false, false);
        foreach (var t in new[] { 6, 7, 8, 9 }) Add(4, t, false, true, false, false);
        Add(4, 10, false, true, false, false);
        Add(4, 11, true, true, true, false);
        foreach (var t in new[] { 12, 13, 14, 15 }) Add(4, t, true, true, true, false);
        foreach (var t in new[] { 16, 17 }) Add(4, t, false, true, false, false);
        foreach (var t in new[] { 18, 19, 20 }) Add(4, t, true, true, true, false);
        for (int t = 29; t <= 43; t++) Add(4, t, false, true, false, false);

        // Role 6: Archivist
        foreach (var t in new[] { 1, 2, 3, 4, 5 }) Add(6, t, false, true, false, false);
        foreach (var t in new[] { 6, 7, 8, 9 }) Add(6, t, true, true, true, false);

        builder.HasData(permissions);
    }
}
