/*
  تسوية تعيينات الأدوار قبل تشغيل قرار الوصول الموحد.
  الوضع الافتراضي معاينة فقط. شغّل على قاعدة التطبيق المقصودة بعد مراجعة النتائج.
  أضف معرّفات الحسابات المركزية المعتمدة إلى @CentralUsers صراحةً.
  لا يحوّل رقم الفرع 1 أو الفرع الفارغ إلى مركزية تلقائياً.
  لا يعيد تفعيل تعيينات سُحبت سابقاً، ولا يمنح صلاحية النقل تلقائياً لأي دور.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Apply bit = 0;
DECLARE @RunId uniqueidentifier = NEWID();
DECLARE @CentralUsers TABLE (UserId int PRIMARY KEY);
-- مثال بعد التحقق من هوية الحساب ودوره: INSERT @CentralUsers VALUES (123);

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL OR OBJECT_ID(N'dbo.UserRoleAssignments', N'U') IS NULL
    THROW 51000, N'قاعدة البيانات لا تحتوي جداول المستخدمين والتعيينات المطلوبة', 1;

SELECT u.UserID, u.Username, u.RoleID, u.BranchID,
    CASE WHEN c.UserId IS NOT NULL THEN N'مركزي معتمد صراحةً'
         WHEN u.BranchID > 0 THEN N'فرع محدد'
         ELSE N'يحتاج إلى تحديد نطاق؛ لن يمنح وصولاً تلقائياً' END AS ProposedScope,
    (SELECT COUNT(*) FROM dbo.UserRoleAssignments a WHERE a.UserId = u.UserID) AS AssignmentHistoryCount
FROM dbo.Users u LEFT JOIN @CentralUsers c ON c.UserId = u.UserID
WHERE u.IsActive = 1 AND u.IsDeleted = 0;

SELECT a.Id, a.UserId, a.RoleId, a.BranchId, a.RegionId, a.IsActive
FROM dbo.UserRoleAssignments a WHERE a.IsActive = 1;

-- Personal grants have no independent scope. Review accounts without a home branch.
SELECT u.UserID, u.Username, u.BranchID, p.Code, up.IsGranted
FROM dbo.UserPermissions up JOIN dbo.Users u ON u.UserID = up.UserId
JOIN dbo.Permissions p ON p.PermissionId = up.PermissionId
WHERE u.IsActive = 1 AND u.IsDeleted = 0;

IF @Apply = 0 RETURN;

BEGIN TRY
BEGIN TRANSACTION;
IF EXISTS(SELECT 1 FROM dbo.UserRoleAssignments a
    WHERE a.IsActive = 1 AND a.BranchId IS NULL AND a.RegionId IS NULL
        AND NOT EXISTS(SELECT 1 FROM @CentralUsers c WHERE c.UserId = a.UserId))
    THROW 51003, N'راجع كل تعيين مركزي قائم واعتمد حسابه صراحةً في قائمة الحسابات المركزية قبل التسوية', 1;
IF EXISTS(SELECT 1 FROM dbo.UserRoleAssignments WHERE IsActive = 1 AND BranchId IS NOT NULL AND RegionId IS NOT NULL)
    THROW 51002, N'توجد تعيينات تجمع فرعاً وإقليماً؛ صحح نطاقها صراحةً قبل التسوية', 1;

IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.UserRoleAssignments') AND name = N'IX_UserRoleAssignments_UserId_RoleId_BranchId')
    DROP INDEX IX_UserRoleAssignments_UserId_RoleId_BranchId ON dbo.UserRoleAssignments;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.UserRoleAssignments') AND name = N'IX_UserRoleAssignments_UserId_RoleId_BranchId_RegionId')
    CREATE UNIQUE INDEX IX_UserRoleAssignments_UserId_RoleId_BranchId_RegionId
    ON dbo.UserRoleAssignments(UserId, RoleId, BranchId, RegionId) WHERE IsActive = 1;
IF OBJECT_ID(N'dbo.BranchAccessRolloutBackup', N'U') IS NULL
    CREATE TABLE dbo.BranchAccessRolloutBackup
    (
        RunId uniqueidentifier NOT NULL,
        AssignmentId int NOT NULL,
        ChangeType varchar(10) NOT NULL,
        WasActive bit NULL,
        CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_BranchAccessRolloutBackup PRIMARY KEY (RunId, AssignmentId, ChangeType)
    );

-- Bootstrap only accounts which never had assignments. Revoked histories stay revoked.
INSERT dbo.UserRoleAssignments(UserId, RoleId, BranchId, RegionId, IsActive, CreatedAt)
OUTPUT @RunId, inserted.Id, 'Insert', NULL INTO dbo.BranchAccessRolloutBackup(RunId, AssignmentId, ChangeType, WasActive)
SELECT u.UserID, u.RoleID, CASE WHEN c.UserId IS NULL THEN u.BranchID ELSE NULL END, NULL, 1, SYSUTCDATETIME()
FROM dbo.Users u LEFT JOIN @CentralUsers c ON c.UserId = u.UserID
WHERE u.IsActive = 1 AND u.IsDeleted = 0 AND u.RoleID IS NOT NULL
    AND (u.BranchID > 0 OR c.UserId IS NOT NULL)
    AND NOT EXISTS(SELECT 1 FROM dbo.UserRoleAssignments a WHERE a.UserId = u.UserID);

-- Replace only the explicitly selected account's primary-role local assignment.
UPDATE a SET IsActive = 0
OUTPUT @RunId, inserted.Id, 'Update', deleted.IsActive INTO dbo.BranchAccessRolloutBackup(RunId, AssignmentId, ChangeType, WasActive)
FROM dbo.UserRoleAssignments a
JOIN dbo.Users u ON u.UserID = a.UserId AND u.RoleID = a.RoleId
JOIN @CentralUsers c ON c.UserId = u.UserID
WHERE a.IsActive = 1 AND (a.BranchId IS NOT NULL OR a.RegionId IS NOT NULL);

INSERT dbo.UserRoleAssignments(UserId, RoleId, BranchId, RegionId, IsActive, CreatedAt)
OUTPUT @RunId, inserted.Id, 'Insert', NULL INTO dbo.BranchAccessRolloutBackup(RunId, AssignmentId, ChangeType, WasActive)
SELECT u.UserID, u.RoleID, NULL, NULL, 1, SYSUTCDATETIME()
FROM dbo.Users u JOIN @CentralUsers c ON c.UserId = u.UserID
WHERE u.IsActive = 1 AND u.IsDeleted = 0 AND u.RoleID IS NOT NULL
    AND NOT EXISTS(SELECT 1 FROM dbo.UserRoleAssignments a WHERE a.UserId = u.UserID AND a.RoleId = u.RoleID
        AND a.IsActive = 1 AND a.BranchId IS NULL AND a.RegionId IS NULL);

IF NOT EXISTS
(
    SELECT 1 FROM dbo.UserRoleAssignments a
    JOIN dbo.Users u ON u.UserID = a.UserId AND u.IsActive = 1 AND u.IsDeleted = 0
    JOIN dbo.RolePermissions rp ON rp.RoleId = a.RoleId
    JOIN dbo.Permissions p ON p.PermissionId = rp.PermissionId AND p.Code = N'System.ManageUsers'
    WHERE a.IsActive = 1 AND a.BranchId IS NULL AND a.RegionId IS NULL
        AND NOT EXISTS(SELECT 1 FROM dbo.UserPermissions up
            WHERE up.UserId = u.UserID AND up.PermissionId = p.PermissionId AND up.IsGranted = 0)
)
    THROW 51001, N'يجب اعتماد حساب مركزي واحد على الأقل يملك إدارة المستخدمين قبل تفعيل النسخة الجديدة', 1;

IF NOT EXISTS(SELECT 1 FROM dbo.Permissions WHERE Code = N'File.Transfer')
    INSERT dbo.Permissions(Code, Description, Category)
    VALUES(N'File.Transfer', N'نقل ملف تقاعد بين الفروع', N'File');

COMMIT;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    THROW;
END CATCH;
SELECT @RunId AS RolloutRunId;
/*
  احتفظ بـ RolloutRunId. الرجوع عن التسوية يحتاج مراجعة التغييرات الإدارية اللاحقة.
  لا تحذف التعيينات ولا تستخدم تراجعاً عاماً دون مراجعة النسخة الاحتياطية لهذا التشغيل.
  سجلات التعيينات الأصلية محفوظة؛ التعديلات أعلاه تقتصر على IsActive وإضافة صفوف.
*/
