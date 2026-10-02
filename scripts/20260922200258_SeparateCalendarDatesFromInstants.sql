BEGIN TRANSACTION;
GO

DROP INDEX [IX_Agents_FileID_EntitledID] ON [Agents];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RetirementFiles]') AND [c].[name] = N'ServiceStartDate');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [RetirementFiles] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [RetirementFiles] ALTER COLUMN [ServiceStartDate] date NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RetirementFiles]') AND [c].[name] = N'ServiceEndDate');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [RetirementFiles] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [RetirementFiles] ALTER COLUMN [ServiceEndDate] date NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RetirementFiles]') AND [c].[name] = N'RetirementDate');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RetirementFiles] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [RetirementFiles] ALTER COLUMN [RetirementDate] date NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Retirees]') AND [c].[name] = N'DeathDate');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Retirees] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Retirees] ALTER COLUMN [DeathDate] date NULL;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Retirees]') AND [c].[name] = N'BirthDate');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Retirees] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Retirees] ALTER COLUMN [BirthDate] date NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentBooks]') AND [c].[name] = N'IssueDate');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [PaymentBooks] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [PaymentBooks] ALTER COLUMN [IssueDate] date NOT NULL;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentBooks]') AND [c].[name] = N'ExpiryDate');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [PaymentBooks] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [PaymentBooks] ALTER COLUMN [ExpiryDate] date NOT NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MinistryApprovalItems]') AND [c].[name] = N'RetirementDate');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [MinistryApprovalItems] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [MinistryApprovalItems] ALTER COLUMN [RetirementDate] date NULL;
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MinistryApprovalItems]') AND [c].[name] = N'EnrollmentDate');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [MinistryApprovalItems] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [MinistryApprovalItems] ALTER COLUMN [EnrollmentDate] date NULL;
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EntitledPersons]') AND [c].[name] = N'DeceasedDate');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [EntitledPersons] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [EntitledPersons] ALTER COLUMN [DeceasedDate] date NULL;
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EntitledPersons]') AND [c].[name] = N'BirthDate');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [EntitledPersons] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [EntitledPersons] ALTER COLUMN [BirthDate] date NOT NULL;
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'NationalIDIssueDate');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Employees] ALTER COLUMN [NationalIDIssueDate] date NULL;
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'NationalIDExpiryDate');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Employees] ALTER COLUMN [NationalIDExpiryDate] date NULL;
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'BirthDate');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Employees] ALTER COLUMN [BirthDate] date NULL;
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'IssueDate');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Documents] ALTER COLUMN [IssueDate] date NULL;
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'ExpiryDate');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [Documents] ALTER COLUMN [ExpiryDate] date NULL;
GO

UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = '2026-09-22T20:02:53.3510744Z'
WHERE [AdministrativeOrderTypeID] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = '2026-09-22T20:02:53.3510754Z'
WHERE [AdministrativeOrderTypeID] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = '2026-09-22T20:02:53.3510759Z'
WHERE [AdministrativeOrderTypeID] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = '2026-09-22T20:02:53.3510762Z'
WHERE [AdministrativeOrderTypeID] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = '2026-09-22T20:02:53.3510766Z'
WHERE [AdministrativeOrderTypeID] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1966-12-31', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1990-01-28', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1986-05-20', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = NULL, [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = NULL, [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1991-07-30', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1973-03-06', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 7;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1987-08-13', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 8;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1994-05-18', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 9;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = NULL, [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 10;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '1988-01-02', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 11;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = '2003-07-24', [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 12;
SELECT @@ROWCOUNT;

GO

UPDATE [Employees] SET [BirthDate] = NULL, [NationalIDExpiryDate] = NULL, [NationalIDIssueDate] = NULL
WHERE [EmployeeID] = 13;
SELECT @@ROWCOUNT;

GO

UPDATE [TransactionStatuses] SET [CreatedAt] = '2026-09-22T20:02:53.4082294Z'
WHERE [StatusID] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [TransactionStatuses] SET [CreatedAt] = '2026-09-22T20:02:53.4082307Z'
WHERE [StatusID] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [TransactionStatuses] SET [CreatedAt] = '2026-09-22T20:02:53.4082309Z'
WHERE [StatusID] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [TransactionStatuses] SET [CreatedAt] = '2026-09-22T20:02:53.4082311Z'
WHERE [StatusID] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [TransactionStatuses] SET [CreatedAt] = '2026-09-22T20:02:53.4082313Z'
WHERE [StatusID] = 5;
SELECT @@ROWCOUNT;

GO

CREATE UNIQUE INDEX [IX_Agents_FileID_EntitledID] ON [Agents] ([FileID], [EntitledID]) WHERE [IsDeleted] = 0 AND [IsActive] = 1 AND [EntitledID] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922200258_SeparateCalendarDatesFromInstants', N'8.0.11');
GO

COMMIT;
GO

