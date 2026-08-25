using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingPermissionsManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── 1. Insert Accounting permissions (safe: only if not exist) ───
            // PermissionId 140 = Accounting.View
            // PermissionId 141 = Accounting.Manage
            // PermissionId 142 = Accounting.Reports
            // Note: Permissions.PermissionId is Identity; must use IDENTITY_INSERT
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Permissions ON;

                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionId = 140)
                    INSERT INTO Permissions (PermissionId, Category, Code, Description)
                    VALUES (140, N'Accounting', N'Accounting.View', N'عرض بيانات المحاسبة');

                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionId = 141)
                    INSERT INTO Permissions (PermissionId, Category, Code, Description)
                    VALUES (141, N'Accounting', N'Accounting.Manage', N'إدارة المحاسبة');

                IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionId = 142)
                    INSERT INTO Permissions (PermissionId, Category, Code, Description)
                    VALUES (142, N'Accounting', N'Accounting.Reports', N'عرض التقارير المحاسبية');

                SET IDENTITY_INSERT Permissions OFF;
            ");

            // ─── 2. Role 1 (Admin) — add permissions 128-142 if missing ───
            //     128-139 = inventory permissions (were supposed to be seeded
            //     but may have been missed); 140-142 = accounting permissions.
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT RolePermissions ON;

                DECLARE @nextId INT = (SELECT ISNULL(MAX(RolePermissionId), 999) + 1 FROM RolePermissions);

                DECLARE @rp TABLE (PermissionId INT NOT NULL);
                INSERT INTO @rp VALUES (128),(129),(130),(131),(132),(133),
                                       (134),(135),(136),(137),(138),(139),
                                       (140),(141),(142);

                INSERT INTO RolePermissions (RolePermissionId, RoleId, PermissionId, CreatedAt)
                SELECT @nextId + ROW_NUMBER() OVER (ORDER BY p.PermissionId),
                       1,
                       p.PermissionId,
                       SYSUTCDATETIME()
                FROM @rp p
                WHERE NOT EXISTS (
                    SELECT 1 FROM RolePermissions rp
                    WHERE rp.RoleId = 1 AND rp.PermissionId = p.PermissionId
                );

                SET IDENTITY_INSERT RolePermissions OFF;
            ");

            // ─── 3. Role 2 (PensionManager) — add Accounting.View + Accounting.Reports ───
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT RolePermissions ON;

                DECLARE @nextId INT = (SELECT ISNULL(MAX(RolePermissionId), 999) + 1 FROM RolePermissions);

                DECLARE @rp TABLE (PermissionId INT NOT NULL);
                INSERT INTO @rp VALUES (140),(142);

                INSERT INTO RolePermissions (RolePermissionId, RoleId, PermissionId, CreatedAt)
                SELECT @nextId + ROW_NUMBER() OVER (ORDER BY p.PermissionId),
                       2,
                       p.PermissionId,
                       SYSUTCDATETIME()
                FROM @rp p
                WHERE NOT EXISTS (
                    SELECT 1 FROM RolePermissions rp
                    WHERE rp.RoleId = 2 AND rp.PermissionId = p.PermissionId
                );

                SET IDENTITY_INSERT RolePermissions OFF;
            ");

            // ─── 4. Role 4 (FinanceOfficer) — add all 3 Accounting permissions ───
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT RolePermissions ON;

                DECLARE @nextId INT = (SELECT ISNULL(MAX(RolePermissionId), 999) + 1 FROM RolePermissions);

                DECLARE @rp TABLE (PermissionId INT NOT NULL);
                INSERT INTO @rp VALUES (140),(141),(142);

                INSERT INTO RolePermissions (RolePermissionId, RoleId, PermissionId, CreatedAt)
                SELECT @nextId + ROW_NUMBER() OVER (ORDER BY p.PermissionId),
                       4,
                       p.PermissionId,
                       SYSUTCDATETIME()
                FROM @rp p
                WHERE NOT EXISTS (
                    SELECT 1 FROM RolePermissions rp
                    WHERE rp.RoleId = 4 AND rp.PermissionId = p.PermissionId
                );

                SET IDENTITY_INSERT RolePermissions OFF;
            ");

            // ─── 5. Role 5 (Auditor) — add Accounting.View + Accounting.Reports ───
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT RolePermissions ON;

                DECLARE @nextId INT = (SELECT ISNULL(MAX(RolePermissionId), 999) + 1 FROM RolePermissions);

                DECLARE @rp TABLE (PermissionId INT NOT NULL);
                INSERT INTO @rp VALUES (140),(142);

                INSERT INTO RolePermissions (RolePermissionId, RoleId, PermissionId, CreatedAt)
                SELECT @nextId + ROW_NUMBER() OVER (ORDER BY p.PermissionId),
                       5,
                       p.PermissionId,
                       SYSUTCDATETIME()
                FROM @rp p
                WHERE NOT EXISTS (
                    SELECT 1 FROM RolePermissions rp
                    WHERE rp.RoleId = 5 AND rp.PermissionId = p.PermissionId
                );

                SET IDENTITY_INSERT RolePermissions OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove accounting RolePermission mappings
            migrationBuilder.Sql(@"
                DELETE FROM RolePermissions
                WHERE PermissionId IN (140, 141, 142)
            ");
            // Remove accounting permissions
            migrationBuilder.Sql(@"
                DELETE FROM Permissions
                WHERE PermissionId IN (140, 141, 142)
            ");
        }
    }
}
