IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Banks] (
        [BankID] int NOT NULL IDENTITY,
        [BankName] nvarchar(100) NOT NULL,
        [BankCode] nvarchar(20) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Banks] PRIMARY KEY ([BankID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [FileStatuses] (
        [StatusID] int NOT NULL IDENTITY,
        [StatusName] nvarchar(100) NOT NULL,
        [StatusColor] nvarchar(50) NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FileStatuses] PRIMARY KEY ([StatusID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [HealthStatuses] (
        [HealthStatusID] int NOT NULL IDENTITY,
        [HealthStatusName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_HealthStatuses] PRIMARY KEY ([HealthStatusID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [ImmutableAuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [ActorId] int NOT NULL,
        [ActorName] nvarchar(256) NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityType] nvarchar(256) NOT NULL,
        [EntityId] int NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [BranchId] int NULL,
        [IpAddress] nvarchar(max) NULL,
        [Hash] nvarchar(max) NOT NULL,
        [PreviousHash] nvarchar(max) NOT NULL,
        [CorrelationId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ImmutableAuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [PensionFundBranches] (
        [PensionBranchID] int NOT NULL IDENTITY,
        [ParentBranchId] int NULL,
        [BranchName] nvarchar(200) NOT NULL,
        [Governorate] nvarchar(100) NULL,
        [Address] nvarchar(500) NULL,
        [Phone] nvarchar(20) NULL,
        [ManagerName] nvarchar(200) NULL,
        [ManagerPhone] nvarchar(20) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PensionFundBranches] PRIMARY KEY ([PensionBranchID]),
        CONSTRAINT [FK_PensionFundBranches_PensionFundBranches_ParentBranchId] FOREIGN KEY ([ParentBranchId]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Permissions] (
        [PermissionId] int NOT NULL IDENTITY,
        [Code] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Category] nvarchar(50) NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([PermissionId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Ranks] (
        [RankID] int NOT NULL IDENTITY,
        [RankName] nvarchar(100) NOT NULL,
        [RankLevel] int NOT NULL,
        [BaseSalary] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Ranks] PRIMARY KEY ([RankID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Relationships] (
        [RelationshipID] int NOT NULL IDENTITY,
        [RelationshipName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Relationships] PRIMARY KEY ([RelationshipID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [RetireeCategories] (
        [CategoryID] int NOT NULL IDENTITY,
        [CategoryName] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_RetireeCategories] PRIMARY KEY ([CategoryID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Roles] (
        [RoleID] int NOT NULL IDENTITY,
        [RoleName] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SocialStatuses] (
        [SocialStatusID] int NOT NULL IDENTITY,
        [StatusName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SocialStatuses] PRIMARY KEY ([SocialStatusID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SystemTables] (
        [TableID] int NOT NULL IDENTITY,
        [TableNameEn] nvarchar(100) NOT NULL,
        [TableNameAr] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SystemTables] PRIMARY KEY ([TableID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [TransactionStatuses] (
        [StatusID] int NOT NULL IDENTITY,
        [StatusName] nvarchar(100) NOT NULL,
        [StatusColor] nvarchar(50) NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TransactionStatuses] PRIMARY KEY ([StatusID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [RolePermissionId] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [PermissionId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RolePermissionId]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([PermissionId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Users] (
        [UserID] int NOT NULL IDENTITY,
        [RoleID] int NULL,
        [BranchID] int NULL,
        [CreatedByUserID] int NULL,
        [FullName] nvarchar(200) NOT NULL,
        [Username] nvarchar(50) NOT NULL,
        [Email] nvarchar(254) NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [LastLogin] datetime2 NULL,
        [FailedLoginAttempts] int NOT NULL,
        [IsLocked] bit NOT NULL,
        [LockedUntil] datetime2 NULL,
        [PasswordChangedAt] datetime2 NULL,
        [MustChangePassword] bit NOT NULL,
        [RefreshToken] nvarchar(500) NULL,
        [RefreshTokenExpiryTime] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserID]),
        CONSTRAINT [FK_Users_PensionFundBranches_BranchID] FOREIGN KEY ([BranchID]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Users_Roles_RoleID] FOREIGN KEY ([RoleID]) REFERENCES [Roles] ([RoleID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Users_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Users_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [DocumentTypes] (
        [DocumentTypeID] int NOT NULL IDENTITY,
        [OwnerTableID] int NULL,
        [OwnerType] int NULL,
        [TypeName] nvarchar(200) NOT NULL,
        [SystemCode] nvarchar(50) NOT NULL,
        [TypeNameEn] nvarchar(200) NULL,
        [IsRequired] bit NOT NULL,
        [HasExpiry] bit NOT NULL,
        [MaxFileSize] int NULL,
        [AllowedExtensions] nvarchar(500) NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DocumentTypes] PRIMARY KEY ([DocumentTypeID]),
        CONSTRAINT [FK_DocumentTypes_SystemTables_OwnerTableID] FOREIGN KEY ([OwnerTableID]) REFERENCES [SystemTables] ([TableID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [TablePermissions] (
        [PermissionID] int NOT NULL IDENTITY,
        [RoleID] int NOT NULL,
        [TableID] int NOT NULL,
        [CanCreate] bit NOT NULL,
        [CanRead] bit NOT NULL,
        [CanUpdate] bit NOT NULL,
        [CanDelete] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_TablePermissions] PRIMARY KEY ([PermissionID]),
        CONSTRAINT [FK_TablePermissions_Roles_RoleID] FOREIGN KEY ([RoleID]) REFERENCES [Roles] ([RoleID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TablePermissions_SystemTables_TableID] FOREIGN KEY ([TableID]) REFERENCES [SystemTables] ([TableID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [AdministrativeOrderTypes] (
        [AdministrativeOrderTypeID] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [GeneratesServiceHistory] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_AdministrativeOrderTypes] PRIMARY KEY ([AdministrativeOrderTypeID]),
        CONSTRAINT [FK_AdministrativeOrderTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [ApprovalRequests] (
        [ApprovalID] int NOT NULL IDENTITY,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityID] int NOT NULL,
        [RequestedByUserID] int NOT NULL,
        [WorkflowStage] nvarchar(100) NULL,
        [DecisionHistory] nvarchar(max) NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_ApprovalRequests] PRIMARY KEY ([ApprovalID]),
        CONSTRAINT [FK_ApprovalRequests_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_ApprovalRequests_Users_RequestedByUserID] FOREIGN KEY ([RequestedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [LogID] bigint NOT NULL IDENTITY,
        [UserID] int NULL,
        [TableID] int NULL,
        [Action] nvarchar(100) NOT NULL,
        [RecordID] int NULL,
        [EntityKey] int NULL,
        [EntityDisplayName] nvarchar(max) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IPAddress] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [CorrelationId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([LogID]),
        CONSTRAINT [FK_AuditLogs_SystemTables_TableID] FOREIGN KEY ([TableID]) REFERENCES [SystemTables] ([TableID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AuditLogs_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [BudgetChapters] (
        [ChapterID] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_BudgetChapters] PRIMARY KEY ([ChapterID]),
        CONSTRAINT [FK_BudgetChapters_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [CorrespondenceTypes] (
        [CorrespondenceTypeID] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_CorrespondenceTypes] PRIMARY KEY ([CorrespondenceTypeID]),
        CONSTRAINT [FK_CorrespondenceTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Departments] (
        [DepartmentID] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([DepartmentID]),
        CONSTRAINT [FK_Departments_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [DeviceRegistrations] (
        [DeviceID] nvarchar(450) NOT NULL,
        [UserID] int NOT NULL,
        [DeviceName] nvarchar(max) NULL,
        [DeviceType] nvarchar(max) NULL,
        [LastKnownIP] nvarchar(max) NULL,
        [LastSyncAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DeviceRegistrations] PRIMARY KEY ([DeviceID]),
        CONSTRAINT [FK_DeviceRegistrations_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [EmploymentStatuses] (
        [EmploymentStatusID] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [IsInService] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_EmploymentStatuses] PRIMARY KEY ([EmploymentStatusID]),
        CONSTRAINT [FK_EmploymentStatuses_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [ExternalParties] (
        [ExternalPartyID] int NOT NULL IDENTITY,
        [Name] nvarchar(300) NOT NULL,
        [PartyType] nvarchar(100) NOT NULL,
        [ContactInfo] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_ExternalParties] PRIMARY KEY ([ExternalPartyID]),
        CONSTRAINT [FK_ExternalParties_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [FiscalYears] (
        [FiscalYearID] int NOT NULL IDENTITY,
        [YearName] nvarchar(50) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [TotalBudget] decimal(18,2) NOT NULL,
        [IsClosed] bit NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_FiscalYears] PRIMARY KEY ([FiscalYearID]),
        CONSTRAINT [FK_FiscalYears_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InventoryCategories] (
        [InventoryCategoryID] int NOT NULL IDENTITY,
        [CategoryName] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_InventoryCategories] PRIMARY KEY ([InventoryCategoryID]),
        CONSTRAINT [FK_InventoryCategories_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryCategories_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InvestmentPortfolios] (
        [PortfolioID] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ManagerUserID] int NOT NULL,
        [TotalTargetCapital] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_InvestmentPortfolios] PRIMARY KEY ([PortfolioID]),
        CONSTRAINT [CK_Portfolio_TargetCapital] CHECK (TotalTargetCapital >= 0),
        CONSTRAINT [FK_InvestmentPortfolios_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_InvestmentPortfolios_Users_ManagerUserID] FOREIGN KEY ([ManagerUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InvestmentTypes] (
        [TypeID] int NOT NULL IDENTITY,
        [TypeName] nvarchar(100) NOT NULL,
        [Code] nvarchar(20) NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_InvestmentTypes] PRIMARY KEY ([TypeID]),
        CONSTRAINT [FK_InvestmentTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [JobTitles] (
        [JobTitleID] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_JobTitles] PRIMARY KEY ([JobTitleID]),
        CONSTRAINT [FK_JobTitles_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [LeaveTypes] (
        [LeaveTypeID] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [IsPaid] bit NOT NULL,
        [CountsTowardsService] bit NOT NULL,
        [MaxDaysPerRequest] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_LeaveTypes] PRIMARY KEY ([LeaveTypeID]),
        CONSTRAINT [FK_LeaveTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [LoginHistories] (
        [LoginID] bigint NOT NULL IDENTITY,
        [UserID] int NULL,
        [AttemptedUsername] nvarchar(max) NOT NULL,
        [LoginTime] datetime2 NOT NULL,
        [IPAddress] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [IsSuccessful] bit NOT NULL,
        [FailureReason] nvarchar(max) NULL,
        [DeviceID] nvarchar(max) NULL,
        CONSTRAINT [PK_LoginHistories] PRIMARY KEY ([LoginID]),
        CONSTRAINT [FK_LoginHistories_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [MinistryApprovalBatches] (
        [BatchID] int NOT NULL IDENTITY,
        [ImportedByUserID] int NOT NULL,
        [BatchNumber] nvarchar(max) NULL,
        [BatchDate] datetime2 NOT NULL,
        [ReceivedDate] datetime2 NOT NULL,
        [TotalCount] int NOT NULL,
        [ProcessedCount] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_MinistryApprovalBatches] PRIMARY KEY ([BatchID]),
        CONSTRAINT [FK_MinistryApprovalBatches_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_MinistryApprovalBatches_Users_ImportedByUserID] FOREIGN KEY ([ImportedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationID] bigint NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [RelatedTableID] int NULL,
        [Title] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [Type] nvarchar(100) NULL,
        [Category] nvarchar(100) NULL,
        [Priority] tinyint NOT NULL,
        [ActionUrl] nvarchar(max) NULL,
        [ActionType] nvarchar(max) NULL,
        [Payload] nvarchar(max) NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [RelatedRecordID] int NULL,
        [ActorID] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationID]),
        CONSTRAINT [FK_Notifications_SystemTables_RelatedTableID] FOREIGN KEY ([RelatedTableID]) REFERENCES [SystemTables] ([TableID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Notifications_Users_ActorID] FOREIGN KEY ([ActorID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Notifications_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [PropertyTypes] (
        [TypeID] int NOT NULL IDENTITY,
        [TypeName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_PropertyTypes] PRIMARY KEY ([TypeID]),
        CONSTRAINT [FK_PropertyTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsRevoked] bit NOT NULL,
        [RevokedAt] datetime2 NULL,
        [ReplacedByToken] nvarchar(max) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [ReportTemplates] (
        [TemplateID] int NOT NULL IDENTITY,
        [CreatedByUserID] int NOT NULL,
        [TemplateName] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Category] nvarchar(100) NULL,
        [Template] nvarchar(max) NOT NULL,
        [QueryTemplate] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ReportTemplates] PRIMARY KEY ([TemplateID]),
        CONSTRAINT [FK_ReportTemplates_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SalaryPaymentBatches] (
        [BatchID] int NOT NULL IDENTITY,
        [BankID] int NOT NULL,
        [ImportedByUserID] int NOT NULL,
        [ApprovedByUserID] int NULL,
        [BatchNumber] nvarchar(max) NULL,
        [BatchMonth] datetime2 NOT NULL,
        [ReceivedDate] datetime2 NOT NULL,
        [TotalCount] int NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [ApprovedAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_SalaryPaymentBatches] PRIMARY KEY ([BatchID]),
        CONSTRAINT [FK_SalaryPaymentBatches_Banks_BankID] FOREIGN KEY ([BankID]) REFERENCES [Banks] ([BankID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentBatches_Users_ApprovedByUserID] FOREIGN KEY ([ApprovedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentBatches_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_SalaryPaymentBatches_Users_ImportedByUserID] FOREIGN KEY ([ImportedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SettlementDecisions] (
        [DecisionID] int NOT NULL IDENTITY,
        [CreatedByUserID] int NOT NULL,
        [DecisionNumber] nvarchar(max) NOT NULL,
        [DecisionDate] datetime2 NOT NULL,
        [DecisionType] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_SettlementDecisions] PRIMARY KEY ([DecisionID]),
        CONSTRAINT [FK_SettlementDecisions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementDecisions_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SettlementsBatches] (
        [BatchID] int NOT NULL IDENTITY,
        [CreatedByUserID] int NOT NULL,
        [BatchNumber] nvarchar(max) NULL,
        [ReceivedDate] datetime2 NOT NULL,
        [TotalCount] int NOT NULL,
        [SourceRowsCount] int NOT NULL,
        [SkippedCount] int NOT NULL,
        [FailureReportJson] nvarchar(max) NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_SettlementsBatches] PRIMARY KEY ([BatchID]),
        CONSTRAINT [FK_SettlementsBatches_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementsBatches_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [StatusTransitionLogs] (
        [TransitionID] bigint NOT NULL IDENTITY,
        [TableID] int NOT NULL,
        [ChangedByUserID] int NULL,
        [RecordID] int NOT NULL,
        [OldStatusID] int NULL,
        [NewStatusID] int NOT NULL,
        [Reason] nvarchar(max) NULL,
        [TransitionDate] datetime2 NOT NULL,
        CONSTRAINT [PK_StatusTransitionLogs] PRIMARY KEY ([TransitionID]),
        CONSTRAINT [FK_StatusTransitionLogs_SystemTables_TableID] FOREIGN KEY ([TableID]) REFERENCES [SystemTables] ([TableID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StatusTransitionLogs_Users_ChangedByUserID] FOREIGN KEY ([ChangedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [SettingID] int NOT NULL IDENTITY,
        [UpdatedByUserID] int NULL,
        [SettingKey] nvarchar(100) NOT NULL,
        [SettingValue] nvarchar(500) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Category] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([SettingID]),
        CONSTRAINT [FK_SystemSettings_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Tenants] (
        [TenantID] int NOT NULL IDENTITY,
        [FullName] nvarchar(200) NOT NULL,
        [TenantType] nvarchar(50) NULL,
        [NationalIDOrCR] nvarchar(50) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [RatingScore] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([TenantID]),
        CONSTRAINT [CK_Tenant_Rating] CHECK (RatingScore >= 0 AND RatingScore <= 5),
        CONSTRAINT [FK_Tenants_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Treasuries] (
        [TreasuryID] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [BankBalance] decimal(18,2) NOT NULL,
        [CashBalance] decimal(18,2) NOT NULL,
        [TotalBalance] decimal(18,2) NOT NULL,
        [Currency] nvarchar(10) NOT NULL,
        [LastUpdated] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_Treasuries] PRIMARY KEY ([TreasuryID]),
        CONSTRAINT [FK_Treasuries_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [UnitTypes] (
        [TypeID] int NOT NULL IDENTITY,
        [TypeName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_UnitTypes] PRIMARY KEY ([TypeID]),
        CONSTRAINT [FK_UnitTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [UserPermissions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [PermissionId] int NOT NULL,
        [IsGranted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [GrantedByUserId] int NULL,
        CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([PermissionId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserPermissions_Users_GrantedByUserId] FOREIGN KEY ([GrantedByUserId]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserPermissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [UserRoleAssignments] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        [BranchId] int NULL,
        [RegionId] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserRoleAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserRoleAssignments_PensionFundBranches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserRoleAssignments_PensionFundBranches_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserRoleAssignments_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserRoleAssignments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [DocumentRequirementRules] (
        [RuleID] int NOT NULL IDENTITY,
        [DocumentTypeID] int NOT NULL,
        [IsGlobal] bit NOT NULL,
        [TargetEntity] nvarchar(50) NULL,
        [ConditionField] nvarchar(100) NULL,
        [ConditionValue] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_DocumentRequirementRules] PRIMARY KEY ([RuleID]),
        CONSTRAINT [FK_DocumentRequirementRules_DocumentTypes_DocumentTypeID] FOREIGN KEY ([DocumentTypeID]) REFERENCES [DocumentTypes] ([DocumentTypeID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [BudgetSections] (
        [SectionID] int NOT NULL IDENTITY,
        [ChapterID] int NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_BudgetSections] PRIMARY KEY ([SectionID]),
        CONSTRAINT [FK_BudgetSections_BudgetChapters_ChapterID] FOREIGN KEY ([ChapterID]) REFERENCES [BudgetChapters] ([ChapterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetSections_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Sections] (
        [SectionID] int NOT NULL IDENTITY,
        [DepartmentID] int NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Sections] PRIMARY KEY ([SectionID]),
        CONSTRAINT [FK_Sections_Departments_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [Departments] ([DepartmentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sections_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SyncQueues] (
        [SyncID] bigint NOT NULL IDENTITY,
        [DeviceID] nvarchar(450) NOT NULL,
        [UserID] int NOT NULL,
        [TableID] int NOT NULL,
        [RecordID] int NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [Data] nvarchar(max) NOT NULL,
        [LocalTimestamp] datetime2 NOT NULL,
        [Status] nvarchar(100) NOT NULL,
        [SyncedAt] datetime2 NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [RetryCount] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SyncQueues] PRIMARY KEY ([SyncID]),
        CONSTRAINT [FK_SyncQueues_DeviceRegistrations_DeviceID] FOREIGN KEY ([DeviceID]) REFERENCES [DeviceRegistrations] ([DeviceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SyncQueues_SystemTables_TableID] FOREIGN KEY ([TableID]) REFERENCES [SystemTables] ([TableID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SyncQueues_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Quarters] (
        [QuarterID] int NOT NULL IDENTITY,
        [FiscalYearID] int NOT NULL,
        [QuarterNumber] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [TreasuryAllocated] decimal(18,2) NOT NULL,
        [IsClosed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_Quarters] PRIMARY KEY ([QuarterID]),
        CONSTRAINT [FK_Quarters_FiscalYears_FiscalYearID] FOREIGN KEY ([FiscalYearID]) REFERENCES [FiscalYears] ([FiscalYearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Quarters_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InventoryItems] (
        [InventoryItemID] int NOT NULL IDENTITY,
        [CategoryID] int NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [ItemCode] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Unit] nvarchar(50) NOT NULL,
        [Quantity] int NOT NULL,
        [MinimumQuantity] int NOT NULL,
        [Location] nvarchar(200) NULL,
        [Condition] int NOT NULL DEFAULT 0,
        [Status] int NOT NULL DEFAULT 0,
        [SerialNumber] nvarchar(100) NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_InventoryItems] PRIMARY KEY ([InventoryItemID]),
        CONSTRAINT [FK_InventoryItems_InventoryCategories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [InventoryCategories] ([InventoryCategoryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryItems_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryItems_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_InventoryItems_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InvestmentProjects] (
        [ProjectID] int NOT NULL IDENTITY,
        [PortfolioID] int NOT NULL,
        [PensionBranchID] int NOT NULL,
        [TypeID] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [FeasibilityStudyPath] nvarchar(max) NULL,
        [BoardApprovalDocPath] nvarchar(max) NULL,
        [Status] nvarchar(50) NOT NULL,
        [PlannedBudget] decimal(18,2) NOT NULL,
        [ExpectedROI] decimal(18,2) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_InvestmentProjects] PRIMARY KEY ([ProjectID]),
        CONSTRAINT [CK_Project_Budget] CHECK (PlannedBudget >= 0),
        CONSTRAINT [CK_Project_Dates] CHECK (EndDate IS NULL OR EndDate > StartDate),
        CONSTRAINT [FK_InvestmentProjects_InvestmentPortfolios_PortfolioID] FOREIGN KEY ([PortfolioID]) REFERENCES [InvestmentPortfolios] ([PortfolioID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentProjects_InvestmentTypes_TypeID] FOREIGN KEY ([TypeID]) REFERENCES [InvestmentTypes] ([TypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentProjects_PensionFundBranches_PensionBranchID] FOREIGN KEY ([PensionBranchID]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentProjects_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [MinistryApprovalItems] (
        [ApprovalItemID] int NOT NULL IDENTITY,
        [BatchID] int NOT NULL,
        [RankID] int NULL,
        [ProcessedByUserID] int NULL,
        [SerialNumber] int NOT NULL,
        [MilitaryNumber] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Gender] tinyint NULL,
        [UnitName] nvarchar(max) NULL,
        [EnrollmentDate] datetime2 NULL,
        [RetirementDate] datetime2 NULL,
        [Salary] decimal(18,2) NULL,
        [CategoryID] int NULL,
        [IsProcessed] bit NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [LinkNumber] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_MinistryApprovalItems] PRIMARY KEY ([ApprovalItemID]),
        CONSTRAINT [FK_MinistryApprovalItems_MinistryApprovalBatches_BatchID] FOREIGN KEY ([BatchID]) REFERENCES [MinistryApprovalBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MinistryApprovalItems_Ranks_RankID] FOREIGN KEY ([RankID]) REFERENCES [Ranks] ([RankID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MinistryApprovalItems_RetireeCategories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [RetireeCategories] ([CategoryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MinistryApprovalItems_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_MinistryApprovalItems_Users_ProcessedByUserID] FOREIGN KEY ([ProcessedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [NotificationAttachments] (
        [AttachmentID] int NOT NULL IDENTITY,
        [NotificationID] bigint NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [MimeType] nvarchar(max) NOT NULL,
        [FileSize] bigint NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_NotificationAttachments] PRIMARY KEY ([AttachmentID]),
        CONSTRAINT [FK_NotificationAttachments_Notifications_NotificationID] FOREIGN KEY ([NotificationID]) REFERENCES [Notifications] ([NotificationID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotificationAttachments_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [NotificationOutbox] (
        [OutboxId] bigint NOT NULL IDENTITY,
        [NotificationId] bigint NOT NULL,
        [Channel] tinyint NOT NULL,
        [Status] tinyint NOT NULL,
        [RetryCount] tinyint NOT NULL,
        [MaxRetries] tinyint NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastAttemptAt] datetime2 NULL,
        [ErrorMessage] nvarchar(500) NULL,
        CONSTRAINT [PK_NotificationOutbox] PRIMARY KEY ([OutboxId]),
        CONSTRAINT [FK_NotificationOutbox_Notifications_NotificationId] FOREIGN KEY ([NotificationId]) REFERENCES [Notifications] ([NotificationID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SavedReports] (
        [ReportID] int NOT NULL IDENTITY,
        [TemplateID] int NOT NULL,
        [GeneratedByUserID] int NOT NULL,
        [ReportName] nvarchar(200) NOT NULL,
        [Parameters] nvarchar(max) NULL,
        [GeneratedAt] datetime2 NOT NULL,
        [ReportTemplateTemplateID] int NULL,
        CONSTRAINT [PK_SavedReports] PRIMARY KEY ([ReportID]),
        CONSTRAINT [FK_SavedReports_ReportTemplates_ReportTemplateTemplateID] FOREIGN KEY ([ReportTemplateTemplateID]) REFERENCES [ReportTemplates] ([TemplateID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SavedReports_ReportTemplates_TemplateID] FOREIGN KEY ([TemplateID]) REFERENCES [ReportTemplates] ([TemplateID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SavedReports_Users_GeneratedByUserID] FOREIGN KEY ([GeneratedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [TreasuryTransactions] (
        [TransactionID] int NOT NULL IDENTITY,
        [TreasuryID] int NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [ReferenceType] nvarchar(100) NULL,
        [ReferenceID] int NULL,
        [TransactionDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_TreasuryTransactions] PRIMARY KEY ([TransactionID]),
        CONSTRAINT [FK_TreasuryTransactions_Treasuries_TreasuryID] FOREIGN KEY ([TreasuryID]) REFERENCES [Treasuries] ([TreasuryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TreasuryTransactions_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [BudgetItems] (
        [ItemID] int NOT NULL IDENTITY,
        [SectionID] int NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_BudgetItems] PRIMARY KEY ([ItemID]),
        CONSTRAINT [FK_BudgetItems_BudgetSections_SectionID] FOREIGN KEY ([SectionID]) REFERENCES [BudgetSections] ([SectionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetItems_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Employees] (
        [EmployeeID] int NOT NULL IDENTITY,
        [DepartmentID] int NOT NULL,
        [SectionID] int NULL,
        [JobTitleID] int NOT NULL,
        [EmploymentStatusID] int NOT NULL,
        [EmployeeNumber] nvarchar(50) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [NationalID] nvarchar(50) NOT NULL,
        [BirthPlace] nvarchar(200) NULL,
        [MaritalStatus] nvarchar(50) NULL,
        [MilitaryNumber] nvarchar(50) NULL,
        [RankID] int NULL,
        [MilitaryServiceDate] datetime2 NULL,
        [JobPositions] nvarchar(2000) NULL,
        [AcademicQualifications] nvarchar(2000) NULL,
        [Phone] nvarchar(50) NULL,
        [Address] nvarchar(500) NULL,
        [Notes] nvarchar(2000) NULL,
        [ProfilePhotoPath] nvarchar(500) NULL,
        [BloodGroup] nvarchar(10) NULL,
        [NationalIDIssuePlace] nvarchar(200) NULL,
        [NationalIDIssueDate] datetime2 NULL,
        [NationalIDExpiryDate] datetime2 NULL,
        [EmergencyContactName] nvarchar(200) NULL,
        [EmergencyContactPhone] nvarchar(20) NULL,
        [Gender] tinyint NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [BirthDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([EmployeeID]),
        CONSTRAINT [FK_Employees_Departments_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [Departments] ([DepartmentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_EmploymentStatuses_EmploymentStatusID] FOREIGN KEY ([EmploymentStatusID]) REFERENCES [EmploymentStatuses] ([EmploymentStatusID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_JobTitles_JobTitleID] FOREIGN KEY ([JobTitleID]) REFERENCES [JobTitles] ([JobTitleID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Ranks_RankID] FOREIGN KEY ([RankID]) REFERENCES [Ranks] ([RankID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Sections_SectionID] FOREIGN KEY ([SectionID]) REFERENCES [Sections] ([SectionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [TreasuryIncomes] (
        [IncomeID] int NOT NULL IDENTITY,
        [TreasuryID] int NOT NULL,
        [QuarterID] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [IncomeSource] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IncomeDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_TreasuryIncomes] PRIMARY KEY ([IncomeID]),
        CONSTRAINT [FK_TreasuryIncomes_Quarters_QuarterID] FOREIGN KEY ([QuarterID]) REFERENCES [Quarters] ([QuarterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TreasuryIncomes_Treasuries_TreasuryID] FOREIGN KEY ([TreasuryID]) REFERENCES [Treasuries] ([TreasuryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TreasuryIncomes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InventoryTransactions] (
        [InventoryTransactionID] int NOT NULL IDENTITY,
        [InventoryItemID] int NOT NULL,
        [TransactionType] int NOT NULL,
        [Quantity] int NOT NULL,
        [PreviousQuantity] int NOT NULL,
        [NewQuantity] int NOT NULL,
        [InvoiceNumber] nvarchar(100) NULL,
        [ReferenceNumber] nvarchar(100) NULL,
        [Reason] nvarchar(500) NULL,
        [TransactionDate] datetime2 NOT NULL,
        [UnitCost] decimal(18,2) NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'SAR',
        [WithdrawalOrderNumber] nvarchar(100) NULL,
        [PerformedByUserID] int NULL,
        [BatchID] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY ([InventoryTransactionID]),
        CONSTRAINT [FK_InventoryTransactions_InventoryItems_InventoryItemID] FOREIGN KEY ([InventoryItemID]) REFERENCES [InventoryItems] ([InventoryItemID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryTransactions_Users_PerformedByUserID] FOREIGN KEY ([PerformedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InvestmentPlans] (
        [PlanID] int NOT NULL IDENTITY,
        [ProjectID] int NOT NULL,
        [FiscalYearID] int NOT NULL,
        [PlannedAmount] decimal(18,2) NOT NULL,
        [ActualSpent] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_InvestmentPlans] PRIMARY KEY ([PlanID]),
        CONSTRAINT [CK_Plan_Amounts] CHECK (PlannedAmount >= 0 AND ActualSpent >= 0),
        CONSTRAINT [FK_InvestmentPlans_FiscalYears_FiscalYearID] FOREIGN KEY ([FiscalYearID]) REFERENCES [FiscalYears] ([FiscalYearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentPlans_InvestmentProjects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [InvestmentProjects] ([ProjectID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentPlans_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InvestmentProperties] (
        [PropertyID] int NOT NULL IDENTITY,
        [ProjectID] int NULL,
        [PensionBranchID] int NOT NULL,
        [PropertyTypeID] int NOT NULL,
        [PropertyName] nvarchar(200) NOT NULL,
        [Address] nvarchar(500) NULL,
        [LocationCity] nvarchar(100) NULL,
        [Latitude] decimal(10,7) NULL,
        [Longitude] decimal(10,7) NULL,
        [GoogleMapsUrl] nvarchar(500) NULL,
        [MainPhotoPath] nvarchar(max) NULL,
        [PhotosJson] nvarchar(max) NULL,
        [OwnershipDeedPath] nvarchar(max) NULL,
        [TotalArea] decimal(18,2) NOT NULL,
        [AcquisitionDate] datetime2 NOT NULL,
        [InitialValue] decimal(18,2) NOT NULL,
        [TitleDeedNumber] nvarchar(100) NULL,
        [Status] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_InvestmentProperties] PRIMARY KEY ([PropertyID]),
        CONSTRAINT [CK_Property_AreaValue] CHECK (TotalArea > 0 AND InitialValue >= 0),
        CONSTRAINT [FK_InvestmentProperties_InvestmentProjects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [InvestmentProjects] ([ProjectID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentProperties_PensionFundBranches_PensionBranchID] FOREIGN KEY ([PensionBranchID]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentProperties_PropertyTypes_PropertyTypeID] FOREIGN KEY ([PropertyTypeID]) REFERENCES [PropertyTypes] ([TypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvestmentProperties_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [RiskAssessments] (
        [RiskID] int NOT NULL IDENTITY,
        [ProjectID] int NOT NULL,
        [RiskType] nvarchar(100) NULL,
        [RiskDescription] nvarchar(500) NULL,
        [RiskScore] int NOT NULL,
        [MitigationPlan] nvarchar(max) NULL,
        [ResponsiblePerson] nvarchar(200) NULL,
        [AssessedDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_RiskAssessments] PRIMARY KEY ([RiskID]),
        CONSTRAINT [FK_RiskAssessments_InvestmentProjects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [InvestmentProjects] ([ProjectID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RiskAssessments_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Retirees] (
        [RetireeID] int NOT NULL IDENTITY,
        [SocialStatusID] int NULL,
        [HealthStatusID] int NULL,
        [ApprovalItemID] int NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [NationalID] nvarchar(20) NOT NULL,
        [PersonalPhotoPath] nvarchar(max) NULL,
        [MilitaryNumber] nvarchar(30) NULL,
        [Gender] tinyint NOT NULL,
        [Phone] nvarchar(20) NULL,
        [Address] nvarchar(500) NULL,
        [City] nvarchar(100) NULL,
        [Governorate] nvarchar(100) NULL,
        [LinkNumber] nvarchar(50) NULL,
        [IsAlive] bit NOT NULL,
        [BirthDate] datetime2 NULL,
        [DeathDate] datetime2 NULL,
        [PendingReviewStatus] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Retirees] PRIMARY KEY ([RetireeID]),
        CONSTRAINT [FK_Retirees_HealthStatuses_HealthStatusID] FOREIGN KEY ([HealthStatusID]) REFERENCES [HealthStatuses] ([HealthStatusID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Retirees_MinistryApprovalItems_ApprovalItemID] FOREIGN KEY ([ApprovalItemID]) REFERENCES [MinistryApprovalItems] ([ApprovalItemID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Retirees_SocialStatuses_SocialStatusID] FOREIGN KEY ([SocialStatusID]) REFERENCES [SocialStatuses] ([SocialStatusID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Retirees_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Retirees_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Retirees_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [BudgetAllocations] (
        [AllocationID] int NOT NULL IDENTITY,
        [QuarterID] int NOT NULL,
        [ItemID] int NOT NULL,
        [AmountAllocated] decimal(18,2) NOT NULL,
        [RemainingBalance] decimal(18,2) NOT NULL,
        [UsedAmount] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_BudgetAllocations] PRIMARY KEY ([AllocationID]),
        CONSTRAINT [FK_BudgetAllocations_BudgetItems_ItemID] FOREIGN KEY ([ItemID]) REFERENCES [BudgetItems] ([ItemID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetAllocations_Quarters_QuarterID] FOREIGN KEY ([QuarterID]) REFERENCES [Quarters] ([QuarterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetAllocations_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [AdministrativeOrders] (
        [AdministrativeOrderID] int NOT NULL IDENTITY,
        [EmployeeID] int NOT NULL,
        [AdministrativeOrderTypeID] int NOT NULL,
        [ApprovedByUserID] int NULL,
        [OrderNumber] nvarchar(50) NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [EffectiveDate] datetime2 NULL,
        [Subject] nvarchar(200) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT N'Draft',
        [ApprovedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_AdministrativeOrders] PRIMARY KEY ([AdministrativeOrderID]),
        CONSTRAINT [FK_AdministrativeOrders_AdministrativeOrderTypes_AdministrativeOrderTypeID] FOREIGN KEY ([AdministrativeOrderTypeID]) REFERENCES [AdministrativeOrderTypes] ([AdministrativeOrderTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdministrativeOrders_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdministrativeOrders_Users_ApprovedByUserID] FOREIGN KEY ([ApprovedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AdministrativeOrders_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [AttendanceEvents] (
        [AttendanceEventID] int NOT NULL IDENTITY,
        [EmployeeID] int NOT NULL,
        [EventDate] datetime2 NOT NULL,
        [EventType] nvarchar(50) NOT NULL,
        [EventTime] time NULL,
        [Notes] nvarchar(1000) NULL,
        [Location] nvarchar(max) NULL,
        [DeviceInfo] nvarchar(max) NULL,
        [IsManualEntry] bit NOT NULL,
        [RecordedByUserID] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AttendanceEvents] PRIMARY KEY ([AttendanceEventID]),
        CONSTRAINT [FK_AttendanceEvents_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AttendanceEvents_Users_RecordedByUserID] FOREIGN KEY ([RecordedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Correspondences] (
        [CorrespondenceID] int NOT NULL IDENTITY,
        [Direction] tinyint NOT NULL,
        [CorrespondenceTypeID] int NOT NULL,
        [SequentialNumber] int NOT NULL,
        [ReferenceNumber] nvarchar(50) NOT NULL,
        [CorrespondenceDate] datetime2 NOT NULL,
        [Subject] nvarchar(1000) NOT NULL,
        [PageNumber] nvarchar(50) NULL,
        [ExternalPartyID] int NULL,
        [EmployeeID] int NULL,
        [CreatedByUserID] int NOT NULL,
        [Notes] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Correspondences] PRIMARY KEY ([CorrespondenceID]),
        CONSTRAINT [FK_Correspondences_CorrespondenceTypes_CorrespondenceTypeID] FOREIGN KEY ([CorrespondenceTypeID]) REFERENCES [CorrespondenceTypes] ([CorrespondenceTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Correspondences_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Correspondences_ExternalParties_ExternalPartyID] FOREIGN KEY ([ExternalPartyID]) REFERENCES [ExternalParties] ([ExternalPartyID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Correspondences_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Correspondences_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [InventoryCustodies] (
        [InventoryCustodyID] int NOT NULL IDENTITY,
        [InventoryItemID] int NOT NULL,
        [EmployeeID] int NOT NULL,
        [Quantity] int NOT NULL,
        [AssignedDate] datetime2 NOT NULL,
        [ReturnedDate] datetime2 NULL,
        [Status] int NOT NULL DEFAULT 0,
        [AssignedByUserID] int NULL,
        [ReturnedToUserID] int NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_InventoryCustodies] PRIMARY KEY ([InventoryCustodyID]),
        CONSTRAINT [FK_InventoryCustodies_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryCustodies_InventoryItems_InventoryItemID] FOREIGN KEY ([InventoryItemID]) REFERENCES [InventoryItems] ([InventoryItemID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryCustodies_Users_AssignedByUserID] FOREIGN KEY ([AssignedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryCustodies_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryCustodies_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_InventoryCustodies_Users_ReturnedToUserID] FOREIGN KEY ([ReturnedToUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryCustodies_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [LeaveRequests] (
        [LeaveRequestID] int NOT NULL IDENTITY,
        [EmployeeID] int NOT NULL,
        [LeaveTypeID] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [RequestedDays] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Reason] nvarchar(1000) NULL,
        [Notes] nvarchar(2000) NULL,
        [ApprovedByUserID] int NULL,
        [ApprovedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([LeaveRequestID]),
        CONSTRAINT [CK_LeaveRequests_DateRange] CHECK ([EndDate] >= [StartDate]),
        CONSTRAINT [CK_LeaveRequests_Days] CHECK ([RequestedDays] >= 0),
        CONSTRAINT [FK_LeaveRequests_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LeaveRequests_LeaveTypes_LeaveTypeID] FOREIGN KEY ([LeaveTypeID]) REFERENCES [LeaveTypes] ([LeaveTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LeaveRequests_Users_ApprovedByUserID] FOREIGN KEY ([ApprovedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LeaveRequests_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [ServiceHistoryEvents] (
        [ServiceHistoryEventID] int NOT NULL IDENTITY,
        [EmployeeID] int NOT NULL,
        [EventType] nvarchar(50) NOT NULL,
        [EventDate] datetime2 NOT NULL,
        [Summary] nvarchar(1000) NOT NULL,
        [SourceEntityType] nvarchar(100) NULL,
        [SourceEntityId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ServiceHistoryEvents] PRIMARY KEY ([ServiceHistoryEventID]),
        CONSTRAINT [FK_ServiceHistoryEvents_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [AssetValuations] (
        [ValuationID] int NOT NULL IDENTITY,
        [PropertyID] int NOT NULL,
        [ValuationDate] datetime2 NOT NULL,
        [MarketValue] decimal(18,2) NOT NULL,
        [ValuedBy] nvarchar(200) NULL,
        [ValuationReportPath] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_AssetValuations] PRIMARY KEY ([ValuationID]),
        CONSTRAINT [FK_AssetValuations_InvestmentProperties_PropertyID] FOREIGN KEY ([PropertyID]) REFERENCES [InvestmentProperties] ([PropertyID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AssetValuations_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [DepreciationRecords] (
        [DeprecID] int NOT NULL IDENTITY,
        [PropertyID] int NOT NULL,
        [FiscalYearID] int NOT NULL,
        [DepreciationRate] decimal(18,2) NOT NULL,
        [DepreciationAmount] decimal(18,2) NOT NULL,
        [NetBookValue] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_DepreciationRecords] PRIMARY KEY ([DeprecID]),
        CONSTRAINT [CK_Depreciation_Rate] CHECK (DepreciationRate >= 0 AND DepreciationRate <= 100),
        CONSTRAINT [FK_DepreciationRecords_FiscalYears_FiscalYearID] FOREIGN KEY ([FiscalYearID]) REFERENCES [FiscalYears] ([FiscalYearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DepreciationRecords_InvestmentProperties_PropertyID] FOREIGN KEY ([PropertyID]) REFERENCES [InvestmentProperties] ([PropertyID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DepreciationRecords_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [PropertyUnits] (
        [UnitID] int NOT NULL IDENTITY,
        [PropertyID] int NOT NULL,
        [UnitTypeID] int NOT NULL,
        [UnitNumber] nvarchar(50) NOT NULL,
        [Floor] nvarchar(50) NULL,
        [Area] decimal(18,2) NOT NULL,
        [MainPhotoPath] nvarchar(max) NULL,
        [PhotosJson] nvarchar(max) NULL,
        [LayoutPlanPath] nvarchar(max) NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_PropertyUnits] PRIMARY KEY ([UnitID]),
        CONSTRAINT [CK_Unit_Area] CHECK (Area > 0),
        CONSTRAINT [FK_PropertyUnits_InvestmentProperties_PropertyID] FOREIGN KEY ([PropertyID]) REFERENCES [InvestmentProperties] ([PropertyID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PropertyUnits_UnitTypes_UnitTypeID] FOREIGN KEY ([UnitTypeID]) REFERENCES [UnitTypes] ([TypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PropertyUnits_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [CitizenAccounts] (
        [CitizenAccountID] int NOT NULL IDENTITY,
        [RetireeID] int NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [NationalID] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsVerified] bit NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [FailedOtpAttempts] int NOT NULL,
        [LockedUntil] datetime2 NULL,
        [RefreshToken] nvarchar(500) NULL,
        [RefreshTokenExpiry] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_CitizenAccounts] PRIMARY KEY ([CitizenAccountID]),
        CONSTRAINT [FK_CitizenAccounts_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CitizenAccounts_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [CitizenNotifications] (
        [NotificationID] int NOT NULL IDENTITY,
        [RetireeID] int NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CitizenNotifications] PRIMARY KEY ([NotificationID]),
        CONSTRAINT [FK_CitizenNotifications_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [FinancialEntitlements] (
        [EntitlementID] int NOT NULL IDENTITY,
        [RetireeID] int NOT NULL,
        [BatchID] int NULL,
        [Amount] decimal(18,2) NOT NULL,
        [EffectiveDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_FinancialEntitlements] PRIMARY KEY ([EntitlementID]),
        CONSTRAINT [FK_FinancialEntitlements_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FinancialEntitlements_SettlementsBatches_BatchID] FOREIGN KEY ([BatchID]) REFERENCES [SettlementsBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FinancialEntitlements_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [RetirementFiles] (
        [FileID] int NOT NULL IDENTITY,
        [RetireeID] int NOT NULL,
        [PensionBranchID] int NOT NULL,
        [StatusID] int NULL,
        [PreviousStatusID] int NULL,
        [RankID] int NULL,
        [AssignedEmployeeID] int NULL,
        [FileNumber] nvarchar(50) NOT NULL,
        [MissingDocumentsCount] int NOT NULL,
        [StatusReason] nvarchar(500) NULL,
        [FileOpenDate] datetime2 NOT NULL,
        [ClosedDate] datetime2 NULL,
        [ServiceStartDate] datetime2 NULL,
        [ServiceEndDate] datetime2 NULL,
        [RetirementDate] datetime2 NULL,
        [Notes] nvarchar(2000) NULL,
        [ReviewStatus] int NOT NULL DEFAULT 0,
        [ReviewRequestedAt] datetime2 NULL,
        [ReviewCompletedAt] datetime2 NULL,
        [ReviewNotes] nvarchar(max) NULL,
        [LastReviewedFileState] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_RetirementFiles] PRIMARY KEY ([FileID]),
        CONSTRAINT [FK_RetirementFiles_FileStatuses_StatusID] FOREIGN KEY ([StatusID]) REFERENCES [FileStatuses] ([StatusID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RetirementFiles_PensionFundBranches_PensionBranchID] FOREIGN KEY ([PensionBranchID]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RetirementFiles_Ranks_RankID] FOREIGN KEY ([RankID]) REFERENCES [Ranks] ([RankID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RetirementFiles_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RetirementFiles_Users_AssignedEmployeeID] FOREIGN KEY ([AssignedEmployeeID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RetirementFiles_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SettlementItems] (
        [SettlementItemID] int NOT NULL IDENTITY,
        [BatchID] int NOT NULL,
        [ApprovalItemID] int NULL,
        [MilitaryNumber] nvarchar(50) NULL,
        [FullName] nvarchar(200) NULL,
        [RetireeID] int NULL,
        [DecisionID] int NULL,
        [CurrentRankID] int NOT NULL,
        [ProcessedRankID] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Deductions] decimal(18,2) NULL,
        [SerialNumber] int NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_SettlementItems] PRIMARY KEY ([SettlementItemID]),
        CONSTRAINT [FK_SettlementItems_MinistryApprovalItems_ApprovalItemID] FOREIGN KEY ([ApprovalItemID]) REFERENCES [MinistryApprovalItems] ([ApprovalItemID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementItems_Ranks_CurrentRankID] FOREIGN KEY ([CurrentRankID]) REFERENCES [Ranks] ([RankID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementItems_Ranks_ProcessedRankID] FOREIGN KEY ([ProcessedRankID]) REFERENCES [Ranks] ([RankID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementItems_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementItems_SettlementDecisions_DecisionID] FOREIGN KEY ([DecisionID]) REFERENCES [SettlementDecisions] ([DecisionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementItems_SettlementsBatches_BatchID] FOREIGN KEY ([BatchID]) REFERENCES [SettlementsBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementItems_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [OperationalExpenses] (
        [ExpenseID] int NOT NULL IDENTITY,
        [BudgetAllocationID] int NOT NULL,
        [ExpenseDate] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [NetAmount] decimal(18,2) NOT NULL,
        [ExpenseType] nvarchar(200) NOT NULL,
        [RecipientName] nvarchar(500) NOT NULL,
        [RecipientWorkplace] nvarchar(500) NULL,
        [DocumentCount] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [InternalChequeNumber] nvarchar(100) NULL,
        [DirectiveSource] nvarchar(500) NULL,
        [ExpenseStatement] nvarchar(1000) NULL,
        [Status] nvarchar(100) NOT NULL,
        [SignedOrderPath] nvarchar(max) NULL,
        [SupportingDocsPath] nvarchar(max) NULL,
        [AttachmentPath] nvarchar(max) NULL,
        [CreatedByUserID] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_OperationalExpenses] PRIMARY KEY ([ExpenseID]),
        CONSTRAINT [FK_OperationalExpenses_BudgetAllocations_BudgetAllocationID] FOREIGN KEY ([BudgetAllocationID]) REFERENCES [BudgetAllocations] ([AllocationID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperationalExpenses_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperationalExpenses_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [LeaseContracts] (
        [ContractID] int NOT NULL IDENTITY,
        [UnitID] int NOT NULL,
        [TenantID] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [TotalContractValue] decimal(18,2) NOT NULL,
        [PaymentFrequency] nvarchar(50) NULL,
        [RenewalTerms] nvarchar(max) NULL,
        [SignedContractPath] nvarchar(max) NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_LeaseContracts] PRIMARY KEY ([ContractID]),
        CONSTRAINT [CK_Lease_Dates] CHECK (EndDate > StartDate),
        CONSTRAINT [CK_Lease_Value] CHECK (TotalContractValue >= 0),
        CONSTRAINT [FK_LeaseContracts_PropertyUnits_UnitID] FOREIGN KEY ([UnitID]) REFERENCES [PropertyUnits] ([UnitID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LeaseContracts_Tenants_TenantID] FOREIGN KEY ([TenantID]) REFERENCES [Tenants] ([TenantID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LeaseContracts_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [MaintenanceRequests] (
        [RequestID] int NOT NULL IDENTITY,
        [PropertyID] int NOT NULL,
        [UnitID] int NULL,
        [TenantID] int NULL,
        [Priority] nvarchar(50) NULL,
        [Description] nvarchar(max) NOT NULL,
        [DamagePhotosJson] nvarchar(max) NULL,
        [Status] nvarchar(50) NOT NULL,
        [ReportedDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_MaintenanceRequests] PRIMARY KEY ([RequestID]),
        CONSTRAINT [FK_MaintenanceRequests_InvestmentProperties_PropertyID] FOREIGN KEY ([PropertyID]) REFERENCES [InvestmentProperties] ([PropertyID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MaintenanceRequests_PropertyUnits_UnitID] FOREIGN KEY ([UnitID]) REFERENCES [PropertyUnits] ([UnitID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MaintenanceRequests_Tenants_TenantID] FOREIGN KEY ([TenantID]) REFERENCES [Tenants] ([TenantID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MaintenanceRequests_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [OtpVerifications] (
        [OtpVerificationID] int NOT NULL IDENTITY,
        [CitizenAccountID] int NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [OtpHash] nvarchar(200) NOT NULL,
        [Purpose] nvarchar(50) NOT NULL,
        [IsUsed] bit NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_OtpVerifications] PRIMARY KEY ([OtpVerificationID]),
        CONSTRAINT [FK_OtpVerifications_CitizenAccounts_CitizenAccountID] FOREIGN KEY ([CitizenAccountID]) REFERENCES [CitizenAccounts] ([CitizenAccountID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SupportTickets] (
        [SupportTicketID] int NOT NULL IDENTITY,
        [CitizenAccountID] int NOT NULL,
        [AssignedToUserID] int NULL,
        [TicketNumber] nvarchar(20) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [Subject] nvarchar(200) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ClosedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([SupportTicketID]),
        CONSTRAINT [FK_SupportTickets_CitizenAccounts_CitizenAccountID] FOREIGN KEY ([CitizenAccountID]) REFERENCES [CitizenAccounts] ([CitizenAccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupportTickets_Users_AssignedToUserID] FOREIGN KEY ([AssignedToUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupportTickets_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Agents] (
        [AgentID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [NationalID] nvarchar(20) NOT NULL,
        [PersonalPhotoPath] nvarchar(max) NULL,
        [Phone] nvarchar(20) NULL,
        [AgencyNumber] nvarchar(100) NULL,
        [AgencyDate] datetime2 NULL,
        [AgencyEndDate] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [PendingReviewStatus] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Agents] PRIMARY KEY ([AgentID]),
        CONSTRAINT [FK_Agents_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Agents_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Agents_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Agents_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [DocumentExemptions] (
        [ExemptionID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [CreatedByUserID] int NOT NULL,
        [DirectiveSource] nvarchar(max) NOT NULL,
        [ExemptionDate] datetime2 NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_DocumentExemptions] PRIMARY KEY ([ExemptionID]),
        CONSTRAINT [FK_DocumentExemptions_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DocumentExemptions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DocumentExemptions_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [EntitledPersons] (
        [EntitledID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [RelationshipID] int NOT NULL,
        [SocialStatusID] int NULL,
        [HealthStatusID] int NULL,
        [FullName] nvarchar(200) NOT NULL,
        [NationalID] nvarchar(20) NOT NULL,
        [BirthDate] datetime2 NOT NULL,
        [Phone] nvarchar(20) NULL,
        [PersonalPhotoPath] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [StopDate] datetime2 NULL,
        [StopReason] nvarchar(max) NULL,
        [StopCode] nvarchar(50) NULL,
        [IsDeceased] bit NOT NULL,
        [DeceasedDate] datetime2 NULL,
        [HasDisabilityException] bit NOT NULL,
        [PendingReviewStatus] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_EntitledPersons] PRIMARY KEY ([EntitledID]),
        CONSTRAINT [FK_EntitledPersons_HealthStatuses_HealthStatusID] FOREIGN KEY ([HealthStatusID]) REFERENCES [HealthStatuses] ([HealthStatusID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntitledPersons_Relationships_RelationshipID] FOREIGN KEY ([RelationshipID]) REFERENCES [Relationships] ([RelationshipID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntitledPersons_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntitledPersons_SocialStatuses_SocialStatusID] FOREIGN KEY ([SocialStatusID]) REFERENCES [SocialStatuses] ([SocialStatusID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntitledPersons_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntitledPersons_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_EntitledPersons_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [FileNotes] (
        [NoteID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [CreatedByUserID] int NOT NULL,
        [ResolvedByUserID] int NULL,
        [NoteText] nvarchar(max) NOT NULL,
        [NoteType] nvarchar(max) NOT NULL,
        [Priority] tinyint NOT NULL,
        [IsResolved] bit NOT NULL,
        [ResolvedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_FileNotes] PRIMARY KEY ([NoteID]),
        CONSTRAINT [FK_FileNotes_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FileNotes_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FileNotes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_FileNotes_Users_ResolvedByUserID] FOREIGN KEY ([ResolvedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [FileReviewItems] (
        [ItemID] bigint NOT NULL IDENTITY,
        [RetirementFileID] int NOT NULL,
        [AuditLogID] bigint NULL,
        [EntityType] int NULL,
        [EntityID] int NULL,
        [FieldName] nvarchar(max) NOT NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [SectionName] nvarchar(max) NULL,
        [SectionType] nvarchar(max) NULL,
        [EntityDisplayName] nvarchar(max) NULL,
        [FieldChangesJson] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [ReviewedByUserID] int NULL,
        [ReviewedAt] datetime2 NULL,
        [ReviewNotes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FileReviewItems] PRIMARY KEY ([ItemID]),
        CONSTRAINT [FK_FileReviewItems_AuditLogs_AuditLogID] FOREIGN KEY ([AuditLogID]) REFERENCES [AuditLogs] ([LogID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FileReviewItems_RetirementFiles_RetirementFileID] FOREIGN KEY ([RetirementFileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FileReviewItems_Users_ReviewedByUserID] FOREIGN KEY ([ReviewedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [FileSnapshots] (
        [Id] bigint NOT NULL IDENTITY,
        [FileId] int NOT NULL,
        [Version] int NOT NULL,
        [SnapshotType] nvarchar(max) NOT NULL,
        [FullState] nvarchar(max) NOT NULL,
        [JsonDiff] nvarchar(max) NULL,
        [CreatedByUserId] int NOT NULL,
        [Reason] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FileSnapshots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FileSnapshots_RetirementFiles_FileId] FOREIGN KEY ([FileId]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [PaymentBooks] (
        [PaymentBookID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [BookNumber] nvarchar(50) NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [BookType] nvarchar(20) NOT NULL,
        [LegacyColor] nvarchar(20) NULL,
        [ChangeReason] nvarchar(100) NULL,
        [RecipientName] nvarchar(200) NULL,
        [RecipientType] nvarchar(20) NULL,
        [Notes] nvarchar(500) NULL,
        [PendingReviewStatus] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_PaymentBooks] PRIMARY KEY ([PaymentBookID]),
        CONSTRAINT [FK_PaymentBooks_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PaymentBooks_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [PendingTasks] (
        [Id] bigint NOT NULL IDENTITY,
        [FileId] int NULL,
        [SalaryBatchID] int NULL,
        [TaskType] nvarchar(max) NOT NULL,
        [AssignedToUserId] int NULL,
        [AssignedToRoleId] int NULL,
        [Status] nvarchar(max) NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [CompletedByUserId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_PendingTasks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PendingTasks_RetirementFiles_FileId] FOREIGN KEY ([FileId]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PendingTasks_Roles_AssignedToRoleId] FOREIGN KEY ([AssignedToRoleId]) REFERENCES [Roles] ([RoleID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PendingTasks_SalaryPaymentBatches_SalaryBatchID] FOREIGN KEY ([SalaryBatchID]) REFERENCES [SalaryPaymentBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PendingTasks_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PendingTasks_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [PensionSuspensions] (
        [SuspensionID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [CreatedByUserID] int NOT NULL,
        [EndedByUserID] int NULL,
        [SuspensionDate] datetime2 NOT NULL,
        [SuspensionReason] nvarchar(max) NOT NULL,
        [SuspensionSource] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [EndDate] datetime2 NULL,
        [EndReason] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_PensionSuspensions] PRIMARY KEY ([SuspensionID]),
        CONSTRAINT [FK_PensionSuspensions_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PensionSuspensions_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PensionSuspensions_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_PensionSuspensions_Users_EndedByUserID] FOREIGN KEY ([EndedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [RiskEvents] (
        [Id] bigint NOT NULL IDENTITY,
        [FileId] int NOT NULL,
        [RiskType] nvarchar(50) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Severity] nvarchar(20) NOT NULL,
        [RiskScore] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RaisedAt] datetime2 NOT NULL,
        [ResolvedAt] datetime2 NULL,
        [ResolvedByUserId] int NULL,
        [ResolutionNotes] nvarchar(max) NULL,
        CONSTRAINT [PK_RiskEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RiskEvents_RetirementFiles_FileId] FOREIGN KEY ([FileId]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RiskEvents_Users_ResolvedByUserId] FOREIGN KEY ([ResolvedByUserId]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [ServiceRequests] (
        [RequestID] int NOT NULL IDENTITY,
        [CitizenAccountID] int NOT NULL,
        [FileID] int NOT NULL,
        [ProcessedByUserID] int NULL,
        [RequestType] nvarchar(50) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Description] nvarchar(max) NULL,
        [AdminNotes] nvarchar(max) NULL,
        [CitizenNotes] nvarchar(max) NULL,
        [RequestDataJson] nvarchar(max) NULL,
        [ProcessedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([RequestID]),
        CONSTRAINT [FK_ServiceRequests_CitizenAccounts_CitizenAccountID] FOREIGN KEY ([CitizenAccountID]) REFERENCES [CitizenAccounts] ([CitizenAccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ServiceRequests_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ServiceRequests_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_ServiceRequests_Users_ProcessedByUserID] FOREIGN KEY ([ProcessedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [PaymentSchedules] (
        [ScheduleID] int NOT NULL IDENTITY,
        [ContractID] int NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [ExpectedAmount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_PaymentSchedules] PRIMARY KEY ([ScheduleID]),
        CONSTRAINT [CK_Payment_Amounts] CHECK (PaidAmount <= ExpectedAmount AND ExpectedAmount > 0),
        CONSTRAINT [FK_PaymentSchedules_LeaseContracts_ContractID] FOREIGN KEY ([ContractID]) REFERENCES [LeaseContracts] ([ContractID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PaymentSchedules_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [WorkOrders] (
        [WorkOrderID] int NOT NULL IDENTITY,
        [RequestID] int NOT NULL,
        [AssignedContractor] nvarchar(200) NULL,
        [EstimatedCost] decimal(18,2) NOT NULL,
        [ActualCost] decimal(18,2) NOT NULL,
        [CompletionDate] datetime2 NULL,
        [InvoiceFilePath] nvarchar(max) NULL,
        [CompletionCertPath] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_WorkOrders] PRIMARY KEY ([WorkOrderID]),
        CONSTRAINT [CK_WorkOrder_Costs] CHECK (EstimatedCost >= 0 AND ActualCost >= 0),
        CONSTRAINT [FK_WorkOrders_MaintenanceRequests_RequestID] FOREIGN KEY ([RequestID]) REFERENCES [MaintenanceRequests] ([RequestID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WorkOrders_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [TicketMessages] (
        [TicketMessageID] int NOT NULL IDENTITY,
        [TicketID] int NOT NULL,
        [SenderType] nvarchar(20) NOT NULL,
        [SenderUserID] int NULL,
        [Content] nvarchar(4000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_TicketMessages] PRIMARY KEY ([TicketMessageID]),
        CONSTRAINT [FK_TicketMessages_SupportTickets_TicketID] FOREIGN KEY ([TicketID]) REFERENCES [SupportTickets] ([SupportTicketID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketMessages_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Guardians] (
        [GuardianID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [NationalID] nvarchar(20) NOT NULL,
        [PersonalPhotoPath] nvarchar(max) NULL,
        [Phone] nvarchar(20) NULL,
        [GuardianshipDocNumber] nvarchar(100) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [PendingReviewStatus] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        [EntitledPersonEntitledID] int NULL,
        CONSTRAINT [PK_Guardians] PRIMARY KEY ([GuardianID]),
        CONSTRAINT [FK_Guardians_EntitledPersons_EntitledPersonEntitledID] FOREIGN KEY ([EntitledPersonEntitledID]) REFERENCES [EntitledPersons] ([EntitledID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Guardians_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Guardians_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Guardians_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Guardians_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [DocumentPledges] (
        [PledgeID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [SuspensionID] int NULL,
        [CreatedByUserID] int NOT NULL,
        [ExtendedByUserID] int NULL,
        [PledgeNumber] nvarchar(max) NULL,
        [PledgeDate] datetime2 NOT NULL,
        [Deadline] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [FulfilledAt] datetime2 NULL,
        [PledgedByType] nvarchar(max) NOT NULL,
        [PledgedByName] nvarchar(max) NULL,
        [OriginalDeadline] datetime2 NULL,
        [IsExtended] bit NOT NULL,
        [ExtendedAt] datetime2 NULL,
        [ExtensionReason] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_DocumentPledges] PRIMARY KEY ([PledgeID]),
        CONSTRAINT [FK_DocumentPledges_PensionSuspensions_SuspensionID] FOREIGN KEY ([SuspensionID]) REFERENCES [PensionSuspensions] ([SuspensionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DocumentPledges_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DocumentPledges_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DocumentPledges_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_DocumentPledges_Users_ExtendedByUserID] FOREIGN KEY ([ExtendedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [GuardianEntitledPersons] (
        [GuardianID] int NOT NULL,
        [EntitledID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_GuardianEntitledPersons] PRIMARY KEY ([GuardianID], [EntitledID]),
        CONSTRAINT [FK_GuardianEntitledPersons_EntitledPersons_EntitledID] FOREIGN KEY ([EntitledID]) REFERENCES [EntitledPersons] ([EntitledID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GuardianEntitledPersons_Guardians_GuardianID] FOREIGN KEY ([GuardianID]) REFERENCES [Guardians] ([GuardianID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GuardianEntitledPersons_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SalaryRecipients] (
        [RecipientID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [RetireeID] int NULL,
        [AgentID] int NULL,
        [EntitledID] int NULL,
        [GuardianID] int NULL,
        [RecipientType] nvarchar(20) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(500) NULL,
        [PendingReviewStatus] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_SalaryRecipients] PRIMARY KEY ([RecipientID]),
        CONSTRAINT [CK_SalaryRecipient_TypeOrGuardianWithEntitled] CHECK (
                    (
                        (CASE WHEN RetireeID IS NOT NULL THEN 1 ELSE 0 END +
                         CASE WHEN AgentID IS NOT NULL THEN 1 ELSE 0 END +
                         CASE WHEN EntitledID IS NOT NULL THEN 1 ELSE 0 END +
                         CASE WHEN GuardianID IS NOT NULL THEN 1 ELSE 0 END) = 1
                    )
                    OR
                    (
                        GuardianID IS NOT NULL
                        AND EntitledID IS NOT NULL
                        AND RetireeID IS NULL
                        AND AgentID IS NULL
                    )),
        CONSTRAINT [FK_SalaryRecipients_Agents_AgentID] FOREIGN KEY ([AgentID]) REFERENCES [Agents] ([AgentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryRecipients_EntitledPersons_EntitledID] FOREIGN KEY ([EntitledID]) REFERENCES [EntitledPersons] ([EntitledID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryRecipients_Guardians_GuardianID] FOREIGN KEY ([GuardianID]) REFERENCES [Guardians] ([GuardianID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryRecipients_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryRecipients_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryRecipients_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [Documents] (
        [DocumentID] int NOT NULL IDENTITY,
        [RetireeID] int NULL,
        [EntitledID] int NULL,
        [FileID] int NULL,
        [AgentID] int NULL,
        [GuardianID] int NULL,
        [PledgeID] int NULL,
        [ExemptionID] int NULL,
        [NoteID] int NULL,
        [SuspensionID] int NULL,
        [SalaryBatchID] int NULL,
        [MinistryBatchID] int NULL,
        [DecisionID] int NULL,
        [SettlementBatchID] int NULL,
        [ReportID] int NULL,
        [InvestmentProjectID] int NULL,
        [InvestmentPropertyID] int NULL,
        [PropertyUnitID] int NULL,
        [LeaseContractID] int NULL,
        [TenantID] int NULL,
        [InvestmentPortfolioID] int NULL,
        [MaintenanceRequestID] int NULL,
        [WorkOrderID] int NULL,
        [AssetValuationID] int NULL,
        [DepreciationRecordID] int NULL,
        [PaymentScheduleID] int NULL,
        [OperationalExpenseID] int NULL,
        [InvestmentPlanID] int NULL,
        [RiskAssessmentID] int NULL,
        [EmployeeID] int NULL,
        [CorrespondenceID] int NULL,
        [ServiceRequestID] int NULL,
        [TicketMessageID] int NULL,
        [CitizenAccountID] int NULL,
        [DocumentTypeID] int NOT NULL,
        [OwnerType] int NOT NULL,
        [VerifiedByUserID] int NULL,
        [UploadedByUserID] int NOT NULL,
        [DocumentNumber] nvarchar(50) NULL,
        [IssueDate] datetime2 NULL,
        [ExpiryDate] datetime2 NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [FileSize] bigint NULL,
        [MimeType] nvarchar(100) NULL,
        [Checksum] nvarchar(64) NULL,
        [Verified] bit NOT NULL,
        [VerifiedAt] datetime2 NULL,
        [Notes] nvarchar(2000) NULL,
        [PendingReviewStatus] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentID]),
        CONSTRAINT [CK_Document_OwnerType] CHECK ((OwnerType = 1  AND FileID IS NOT NULL)           OR
                  (OwnerType = 2  AND RetireeID IS NOT NULL)         OR
                  (OwnerType = 3  AND EntitledID IS NOT NULL)        OR
                  (OwnerType = 4  AND AgentID IS NOT NULL)           OR
                  (OwnerType = 5  AND GuardianID IS NOT NULL)        OR
                  (OwnerType = 6  AND PledgeID IS NOT NULL)          OR
                  (OwnerType = 7  AND ExemptionID IS NOT NULL)       OR
                  (OwnerType = 8  AND NoteID IS NOT NULL)            OR
                  (OwnerType = 9  AND SuspensionID IS NOT NULL)      OR
                  (OwnerType = 10 AND SalaryBatchID IS NOT NULL)     OR
                  (OwnerType = 11 AND MinistryBatchID IS NOT NULL)   OR
                  (OwnerType = 12 AND DecisionID IS NOT NULL)        OR
                  (OwnerType = 13 AND SettlementBatchID IS NOT NULL) OR
                  (OwnerType = 14 AND ReportID IS NOT NULL)          OR
                  (OwnerType = 15 AND InvestmentProjectID IS NOT NULL)  OR
                  (OwnerType = 16 AND InvestmentPropertyID IS NOT NULL) OR
                  (OwnerType = 17 AND PropertyUnitID IS NOT NULL)       OR
                  (OwnerType = 18 AND LeaseContractID IS NOT NULL)      OR
                  (OwnerType = 19 AND TenantID IS NOT NULL)             OR
                  (OwnerType = 20 AND InvestmentPortfolioID IS NOT NULL) OR
                  (OwnerType = 21 AND MaintenanceRequestID IS NOT NULL)  OR
                  (OwnerType = 22 AND WorkOrderID IS NOT NULL)           OR
                  (OwnerType = 25 AND AssetValuationID IS NOT NULL)      OR
                  (OwnerType = 27 AND DepreciationRecordID IS NOT NULL)  OR
                  (OwnerType = 28 AND PaymentScheduleID IS NOT NULL)     OR
                  (OwnerType = 30 AND OperationalExpenseID IS NOT NULL)  OR
                  (OwnerType = 31 AND InvestmentPlanID IS NOT NULL)      OR
                  (OwnerType = 33 AND RiskAssessmentID IS NOT NULL)      OR
                  (OwnerType = 40 AND EmployeeID IS NOT NULL)             OR
                  (OwnerType = 41 AND CorrespondenceID IS NOT NULL)       OR
                  (OwnerType = 50 AND ServiceRequestID IS NOT NULL)       OR
                  (OwnerType = 51 AND TicketMessageID IS NOT NULL)        OR
                  (OwnerType = 52 AND CitizenAccountID IS NOT NULL)),
        CONSTRAINT [FK_Documents_Agents_AgentID] FOREIGN KEY ([AgentID]) REFERENCES [Agents] ([AgentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_AssetValuations_AssetValuationID] FOREIGN KEY ([AssetValuationID]) REFERENCES [AssetValuations] ([ValuationID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_CitizenAccounts_CitizenAccountID] FOREIGN KEY ([CitizenAccountID]) REFERENCES [CitizenAccounts] ([CitizenAccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Correspondences_CorrespondenceID] FOREIGN KEY ([CorrespondenceID]) REFERENCES [Correspondences] ([CorrespondenceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_DepreciationRecords_DepreciationRecordID] FOREIGN KEY ([DepreciationRecordID]) REFERENCES [DepreciationRecords] ([DeprecID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_DocumentExemptions_ExemptionID] FOREIGN KEY ([ExemptionID]) REFERENCES [DocumentExemptions] ([ExemptionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_DocumentPledges_PledgeID] FOREIGN KEY ([PledgeID]) REFERENCES [DocumentPledges] ([PledgeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_DocumentTypes_DocumentTypeID] FOREIGN KEY ([DocumentTypeID]) REFERENCES [DocumentTypes] ([DocumentTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_EntitledPersons_EntitledID] FOREIGN KEY ([EntitledID]) REFERENCES [EntitledPersons] ([EntitledID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_FileNotes_NoteID] FOREIGN KEY ([NoteID]) REFERENCES [FileNotes] ([NoteID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Guardians_GuardianID] FOREIGN KEY ([GuardianID]) REFERENCES [Guardians] ([GuardianID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_InvestmentPlans_InvestmentPlanID] FOREIGN KEY ([InvestmentPlanID]) REFERENCES [InvestmentPlans] ([PlanID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_InvestmentPortfolios_InvestmentPortfolioID] FOREIGN KEY ([InvestmentPortfolioID]) REFERENCES [InvestmentPortfolios] ([PortfolioID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_InvestmentProjects_InvestmentProjectID] FOREIGN KEY ([InvestmentProjectID]) REFERENCES [InvestmentProjects] ([ProjectID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_InvestmentProperties_InvestmentPropertyID] FOREIGN KEY ([InvestmentPropertyID]) REFERENCES [InvestmentProperties] ([PropertyID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_LeaseContracts_LeaseContractID] FOREIGN KEY ([LeaseContractID]) REFERENCES [LeaseContracts] ([ContractID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_MaintenanceRequests_MaintenanceRequestID] FOREIGN KEY ([MaintenanceRequestID]) REFERENCES [MaintenanceRequests] ([RequestID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_MinistryApprovalBatches_MinistryBatchID] FOREIGN KEY ([MinistryBatchID]) REFERENCES [MinistryApprovalBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_OperationalExpenses_OperationalExpenseID] FOREIGN KEY ([OperationalExpenseID]) REFERENCES [OperationalExpenses] ([ExpenseID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_PaymentSchedules_PaymentScheduleID] FOREIGN KEY ([PaymentScheduleID]) REFERENCES [PaymentSchedules] ([ScheduleID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_PensionSuspensions_SuspensionID] FOREIGN KEY ([SuspensionID]) REFERENCES [PensionSuspensions] ([SuspensionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_PropertyUnits_PropertyUnitID] FOREIGN KEY ([PropertyUnitID]) REFERENCES [PropertyUnits] ([UnitID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_RiskAssessments_RiskAssessmentID] FOREIGN KEY ([RiskAssessmentID]) REFERENCES [RiskAssessments] ([RiskID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_SalaryPaymentBatches_SalaryBatchID] FOREIGN KEY ([SalaryBatchID]) REFERENCES [SalaryPaymentBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_SavedReports_ReportID] FOREIGN KEY ([ReportID]) REFERENCES [SavedReports] ([ReportID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_ServiceRequests_ServiceRequestID] FOREIGN KEY ([ServiceRequestID]) REFERENCES [ServiceRequests] ([RequestID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_SettlementDecisions_DecisionID] FOREIGN KEY ([DecisionID]) REFERENCES [SettlementDecisions] ([DecisionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_SettlementsBatches_SettlementBatchID] FOREIGN KEY ([SettlementBatchID]) REFERENCES [SettlementsBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Tenants_TenantID] FOREIGN KEY ([TenantID]) REFERENCES [Tenants] ([TenantID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_TicketMessages_TicketMessageID] FOREIGN KEY ([TicketMessageID]) REFERENCES [TicketMessages] ([TicketMessageID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Documents_Users_UploadedByUserID] FOREIGN KEY ([UploadedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Users_VerifiedByUserID] FOREIGN KEY ([VerifiedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_WorkOrders_WorkOrderID] FOREIGN KEY ([WorkOrderID]) REFERENCES [WorkOrders] ([WorkOrderID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [MissingDocuments] (
        [MissingDocumentID] int NOT NULL IDENTITY,
        [FileID] int NOT NULL,
        [DocumentTypeID] int NOT NULL,
        [PledgeID] int NULL,
        [ExemptionID] int NULL,
        [AgentID] int NULL,
        [GuardianID] int NULL,
        [EntitledID] int NULL,
        [Status] nvarchar(max) NOT NULL,
        [SubmittedAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_MissingDocuments] PRIMARY KEY ([MissingDocumentID]),
        CONSTRAINT [FK_MissingDocuments_Agents_AgentID] FOREIGN KEY ([AgentID]) REFERENCES [Agents] ([AgentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingDocuments_DocumentExemptions_ExemptionID] FOREIGN KEY ([ExemptionID]) REFERENCES [DocumentExemptions] ([ExemptionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingDocuments_DocumentPledges_PledgeID] FOREIGN KEY ([PledgeID]) REFERENCES [DocumentPledges] ([PledgeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingDocuments_DocumentTypes_DocumentTypeID] FOREIGN KEY ([DocumentTypeID]) REFERENCES [DocumentTypes] ([DocumentTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingDocuments_EntitledPersons_EntitledID] FOREIGN KEY ([EntitledID]) REFERENCES [EntitledPersons] ([EntitledID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingDocuments_Guardians_GuardianID] FOREIGN KEY ([GuardianID]) REFERENCES [Guardians] ([GuardianID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingDocuments_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MissingDocuments_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE TABLE [SalaryPaymentItems] (
        [PaymentItemID] int NOT NULL IDENTITY,
        [BatchID] int NOT NULL,
        [FileID] int NOT NULL,
        [RetireeID] int NOT NULL,
        [StatusID] int NULL,
        [CreatedByUserID] int NOT NULL,
        [PaymentBookID] int NULL,
        [RecipientID] int NULL,
        [TransactionNumber] nvarchar(max) NULL,
        [FinancialNumber] nvarchar(max) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [SendDate] datetime2 NULL,
        [ReceiveDate] datetime2 NULL,
        [FailureReason] nvarchar(max) NULL,
        [SenderBranch] nvarchar(max) NULL,
        [ReceiverBranch] nvarchar(max) NULL,
        [Currency] nvarchar(max) NULL,
        [Sender] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_SalaryPaymentItems] PRIMARY KEY ([PaymentItemID]),
        CONSTRAINT [FK_SalaryPaymentItems_PaymentBooks_PaymentBookID] FOREIGN KEY ([PaymentBookID]) REFERENCES [PaymentBooks] ([PaymentBookID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentItems_Retirees_RetireeID] FOREIGN KEY ([RetireeID]) REFERENCES [Retirees] ([RetireeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentItems_RetirementFiles_FileID] FOREIGN KEY ([FileID]) REFERENCES [RetirementFiles] ([FileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentItems_SalaryPaymentBatches_BatchID] FOREIGN KEY ([BatchID]) REFERENCES [SalaryPaymentBatches] ([BatchID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentItems_SalaryRecipients_RecipientID] FOREIGN KEY ([RecipientID]) REFERENCES [SalaryRecipients] ([RecipientID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentItems_TransactionStatuses_StatusID] FOREIGN KEY ([StatusID]) REFERENCES [TransactionStatuses] ([StatusID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentItems_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalaryPaymentItems_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AdministrativeOrderTypeID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'GeneratesServiceHistory', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[AdministrativeOrderTypes]'))
        SET IDENTITY_INSERT [AdministrativeOrderTypes] ON;
    EXEC(N'INSERT INTO [AdministrativeOrderTypes] ([AdministrativeOrderTypeID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [GeneratesServiceHistory], [IsActive], [IsDeleted], [Name], [UpdatedAt])
    VALUES (1, N''ORD_APP'', ''2026-06-25T21:49:41.2795057Z'', NULL, NULL, N''قرار تعيين موظف جديد'', CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), N''قرار تعيين'', NULL),
    (2, N''ORD_PRO'', ''2026-06-25T21:49:41.2795063Z'', NULL, NULL, N''قرار ترقية رتبة أو درجة وظيفية'', CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), N''قرار ترقية'', NULL),
    (3, N''ORD_TRA'', ''2026-06-25T21:49:41.2795066Z'', NULL, NULL, N''قرار نقل بين الإدارات أو الأقسام'', CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), N''قرار نقل'', NULL),
    (4, N''ORD_DIS'', ''2026-06-25T21:49:41.2795068Z'', NULL, NULL, N''قرار تأديبي أو لفت نظر'', CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), N''قرار تأديبي'', NULL),
    (5, N''ORD_TER'', ''2026-06-25T21:49:41.2795070Z'', NULL, NULL, N''قرار إنهاء خدمة موظف'', CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), N''قرار إنهاء خدمة'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AdministrativeOrderTypeID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'GeneratesServiceHistory', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[AdministrativeOrderTypes]'))
        SET IDENTITY_INSERT [AdministrativeOrderTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BankID', N'BankCode', N'BankName', N'CreatedAt', N'IsActive', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Banks]'))
        SET IDENTITY_INSERT [Banks] ON;
    EXEC(N'INSERT INTO [Banks] ([BankID], [BankCode], [BankName], [CreatedAt], [IsActive], [UpdatedAt])
    VALUES (1, N''CB'', N''البنك المركزي'', ''2026-01-01T00:00:00.0000000Z'', CAST(1 AS bit), NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BankID', N'BankCode', N'BankName', N'CreatedAt', N'IsActive', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Banks]'))
        SET IDENTITY_INSERT [Banks] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ChapterID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[BudgetChapters]'))
        SET IDENTITY_INSERT [BudgetChapters] ON;
    EXEC(N'INSERT INTO [BudgetChapters] ([ChapterID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsActive], [IsDeleted], [Name], [UpdatedAt])
    VALUES (1, N''31'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الباب الأول: المعاشات والأجور'', NULL),
    (2, N''32'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الباب الثاني: المستلزمات السلعية'', NULL),
    (3, N''33'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الباب الثالث: المصروفات الخدمية'', NULL),
    (4, N''34'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الباب الرابع: المصروفات التحويلية والإعانات'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ChapterID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[BudgetChapters]'))
        SET IDENTITY_INSERT [BudgetChapters] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CorrespondenceTypeID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[CorrespondenceTypes]'))
        SET IDENTITY_INSERT [CorrespondenceTypes] ON;
    EXEC(N'INSERT INTO [CorrespondenceTypes] ([CorrespondenceTypeID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [IsActive], [IsDeleted], [Name], [UpdatedAt])
    VALUES (1, N''INTERNAL'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مراسلات الوزارة (الداخلي)'', NULL),
    (2, N''EXTERNAL'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مراسلات (الخارجي)'', NULL),
    (3, N''ADMIN_ORDER'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''أوامر إدارية'', NULL),
    (4, N''INTERNAL_DIRECTIVE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''توجيهات وتعاميم داخلية'', NULL),
    (5, N''EXTERNAL_DIRECTIVE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''توجيهات وتعاميم خارجية'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CorrespondenceTypeID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[CorrespondenceTypes]'))
        SET IDENTITY_INSERT [CorrespondenceTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DepartmentID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Departments]'))
        SET IDENTITY_INSERT [Departments] ON;
    EXEC(N'INSERT INTO [Departments] ([DepartmentID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsActive], [IsDeleted], [Name], [UpdatedAt])
    VALUES (1, N''PENSIONS'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''إدارة المعاشات '', NULL),
    (2, N''ADMIN'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''إدارة الشئون الإدارية'', NULL),
    (3, N''INVEST'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''إدارة الاستثمار'', NULL),
    (4, N''FINANCE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''إدارة الشئون المالية'', NULL),
    (5, N''LEGAL'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الإدارة القانونية'', NULL),
    (6, N''DG_OFFICE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مكتب المدير العام'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DepartmentID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Departments]'))
        SET IDENTITY_INSERT [Departments] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DocumentTypeID', N'AllowedExtensions', N'CreatedAt', N'HasExpiry', N'IsActive', N'IsRequired', N'MaxFileSize', N'OwnerTableID', N'OwnerType', N'SortOrder', N'SystemCode', N'TypeName', N'TypeNameEn', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[DocumentTypes]'))
        SET IDENTITY_INSERT [DocumentTypes] ON;
    EXEC(N'INSERT INTO [DocumentTypes] ([DocumentTypeID], [AllowedExtensions], [CreatedAt], [HasExpiry], [IsActive], [IsRequired], [MaxFileSize], [OwnerTableID], [OwnerType], [SortOrder], [SystemCode], [TypeName], [TypeNameEn], [UpdatedAt])
    VALUES (1, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), NULL, NULL, NULL, 1, N'''', N''الهوية الوطنية'', N''National ID'', NULL),
    (2, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 2, N'''', N''شهادة الوفاة'', N''Death Certificate'', NULL),
    (3, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 3, N'''', N''وثيقة الوكالة'', N''Agency Document'', NULL),
    (4, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 4, N'''', N''وثيقة الوصاية'', N''Guardianship Document'', NULL),
    (5, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 5, N'''', N''اثبات عزوبية'', N''Single Status Certificate'', NULL),
    (6, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 6, N'''', N''عقد زواج'', N''Marriage Contract'', NULL),
    (7, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 7, N'''', N''حصر ورثة'', N''Inheritance Certificate'', NULL),
    (8, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 8, N'''', N''أوليات الملف'', N''File Originals'', NULL),
    (9, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 9, N'''', N''وثيقة تعهد'', N''Pledge Document'', NULL),
    (10, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 10, N'''', N''توجيه بإعفاء من النواقص'', N''Missing Documents Exemption Directive'', NULL),
    (11, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 11, N'''', N''أمر إيقاف المعاش'', N''Salary Suspension Order'', NULL),
    (12, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 12, N'''', N''مرفق ملاحظة'', N''Note Attachment'', NULL),
    (13, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 13, N'''', N''صورة دفتر الصرف'', N''Payment Book Image'', NULL),
    (14, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 14, N'''', N''وثيقة طلاق'', N''Divorce Certificate'', NULL),
    (15, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 15, N'''', N''إثبات الخدمة المدنية'', N''Civil Service Proof'', NULL),
    (16, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 16, N'''', N''تقرير طبي'', N''Medical Report'', NULL),
    (17, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 17, N'''', N''حكم إعالة'', N''Maintenance Judgment'', NULL),
    (99, NULL, ''2026-01-01T00:00:00.0000000Z'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), NULL, NULL, NULL, 99, N'''', N''الهوية الوطنية (ظهر)'', N''National ID (Back)'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DocumentTypeID', N'AllowedExtensions', N'CreatedAt', N'HasExpiry', N'IsActive', N'IsRequired', N'MaxFileSize', N'OwnerTableID', N'OwnerType', N'SortOrder', N'SystemCode', N'TypeName', N'TypeNameEn', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[DocumentTypes]'))
        SET IDENTITY_INSERT [DocumentTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmploymentStatusID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'IsInService', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[EmploymentStatuses]'))
        SET IDENTITY_INSERT [EmploymentStatuses] ON;
    EXEC(N'INSERT INTO [EmploymentStatuses] ([EmploymentStatusID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsActive], [IsDeleted], [IsInService], [Name], [UpdatedAt])
    VALUES (1, N''ACTIVE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''مستمر بالعمل'', NULL),
    (2, N''UNPAID_LEAVE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''إجازة بدون معاش'', NULL),
    (3, N''SECONDED'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''منتدب / مُعار'', NULL),
    (4, N''SUSPENDED'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''موقوف عن العمل'', NULL),
    (5, N''ABSENT'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''منقطع عن العمل'', NULL),
    (6, N''RESIGNED'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''مستقيل'', NULL),
    (7, N''TERMINATED'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''مفصول'', NULL),
    (8, N''RETIRED_HR'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''مُحال للتقاعد (إدارياً)'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmploymentStatusID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'IsInService', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[EmploymentStatuses]'))
        SET IDENTITY_INSERT [EmploymentStatuses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ExternalPartyID', N'ContactInfo', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsActive', N'IsDeleted', N'Name', N'PartyType', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[ExternalParties]'))
        SET IDENTITY_INSERT [ExternalParties] ON;
    EXEC(N'INSERT INTO [ExternalParties] ([ExternalPartyID], [ContactInfo], [CreatedAt], [DeletedAt], [DeletedByUserID], [IsActive], [IsDeleted], [Name], [PartyType], [UpdatedAt])
    VALUES (1, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بنك القطيبي'', N''بنك'', NULL),
    (2, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الإدارة العامة لمكافحة المخدرات'', N''إدارة أمنية'', NULL),
    (3, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''وكيل الخدمة المدنية'', N''وزارة'', NULL),
    (4, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الهيئة العامة للتأمينات - تعز'', N''هيئة حكومية'', NULL),
    (5, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مدير أمن محافظة عدن'', N''إدارة أمنية'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ExternalPartyID', N'ContactInfo', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsActive', N'IsDeleted', N'Name', N'PartyType', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[ExternalParties]'))
        SET IDENTITY_INSERT [ExternalParties] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StatusID', N'CreatedAt', N'Description', N'StatusColor', N'StatusName') AND [object_id] = OBJECT_ID(N'[FileStatuses]'))
        SET IDENTITY_INSERT [FileStatuses] ON;
    EXEC(N'INSERT INTO [FileStatuses] ([StatusID], [CreatedAt], [Description], [StatusColor], [StatusName])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', NULL, N''outline'', N''جديد''),
    (2, ''2026-01-01T00:00:00.0000000Z'', NULL, N''success'', N''نشط''),
    (3, ''2026-01-01T00:00:00.0000000Z'', NULL, N''destructive'', N''موقوف''),
    (4, ''2026-01-01T00:00:00.0000000Z'', NULL, N''secondary'', N''مغلق''),
    (5, ''2026-01-01T00:00:00.0000000Z'', NULL, N''warning'', N''ناقص''),
    (6, ''2026-01-01T00:00:00.0000000Z'', NULL, N''warning'', N''معلق''),
    (7, ''2026-01-01T00:00:00.0000000Z'', NULL, N''info'', N''تحت المراجعة''),
    (8, ''2026-01-01T00:00:00.0000000Z'', NULL, N''destructive'', N''مرفوض'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StatusID', N'CreatedAt', N'Description', N'StatusColor', N'StatusName') AND [object_id] = OBJECT_ID(N'[FileStatuses]'))
        SET IDENTITY_INSERT [FileStatuses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FiscalYearID', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'EndDate', N'IsClosed', N'IsDeleted', N'Notes', N'StartDate', N'TotalBudget', N'UpdatedAt', N'YearName') AND [object_id] = OBJECT_ID(N'[FiscalYears]'))
        SET IDENTITY_INSERT [FiscalYears] ON;
    EXEC(N'INSERT INTO [FiscalYears] ([FiscalYearID], [CreatedAt], [DeletedAt], [DeletedByUserID], [EndDate], [IsClosed], [IsDeleted], [Notes], [StartDate], [TotalBudget], [UpdatedAt], [YearName])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, ''2026-12-31T23:59:59.0000000Z'', CAST(0 AS bit), CAST(0 AS bit), NULL, ''2026-01-01T00:00:00.0000000Z'', 1000000000.0, NULL, N''2026'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FiscalYearID', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'EndDate', N'IsClosed', N'IsDeleted', N'Notes', N'StartDate', N'TotalBudget', N'UpdatedAt', N'YearName') AND [object_id] = OBJECT_ID(N'[FiscalYears]'))
        SET IDENTITY_INSERT [FiscalYears] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'HealthStatusID', N'CreatedAt', N'HealthStatusName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[HealthStatuses]'))
        SET IDENTITY_INSERT [HealthStatuses] ON;
    EXEC(N'INSERT INTO [HealthStatuses] ([HealthStatusID], [CreatedAt], [HealthStatusName], [UpdatedAt])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', N''سليم'', NULL),
    (2, ''2026-01-01T00:00:00.0000000Z'', N''عاجز جزئياً'', NULL),
    (3, ''2026-01-01T00:00:00.0000000Z'', N''عاجز كلياً'', NULL),
    (4, ''2026-01-01T00:00:00.0000000Z'', N''متوفى'', NULL),
    (5, ''2026-01-01T00:00:00.0000000Z'', N''شهيد'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'HealthStatusID', N'CreatedAt', N'HealthStatusName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[HealthStatuses]'))
        SET IDENTITY_INSERT [HealthStatuses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TypeID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsDeleted', N'TypeName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[InvestmentTypes]'))
        SET IDENTITY_INSERT [InvestmentTypes] ON;
    EXEC(N'INSERT INTO [InvestmentTypes] ([TypeID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsDeleted], [TypeName], [UpdatedAt])
    VALUES (1, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), N''عقاري'', NULL),
    (2, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), N''أسهم'', NULL),
    (3, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), N''سندات'', NULL),
    (4, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), N''وديعة بنكية'', NULL),
    (5, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(0 AS bit), N''عملات'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TypeID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsDeleted', N'TypeName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[InvestmentTypes]'))
        SET IDENTITY_INSERT [InvestmentTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'JobTitleID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[JobTitles]'))
        SET IDENTITY_INSERT [JobTitles] ON;
    EXEC(N'INSERT INTO [JobTitles] ([JobTitleID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsActive], [IsDeleted], [Name], [UpdatedAt])
    VALUES (1, N''DIR_GEN'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''قيادي'', CAST(1 AS bit), CAST(0 AS bit), N''المدير العام'', NULL),
    (2, N''DEP_DIR_GEN'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''قيادي'', CAST(1 AS bit), CAST(0 AS bit), N''نائب المدير العام'', NULL),
    (3, N''DEPT_MGR'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''إشرافي'', CAST(1 AS bit), CAST(0 AS bit), N''مدير إدارة'', NULL),
    (4, N''DG_OFFICE_MGR'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''إشرافي'', CAST(1 AS bit), CAST(0 AS bit), N''مدير مكتب المدير العام'', NULL),
    (5, N''SEC_HEAD'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''إشرافي'', CAST(1 AS bit), CAST(0 AS bit), N''رئيس قسم'', NULL),
    (6, N''EMPLOYEE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''تخصصي وإداري'', CAST(1 AS bit), CAST(0 AS bit), N''موظف'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'JobTitleID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[JobTitles]'))
        SET IDENTITY_INSERT [JobTitles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'LeaveTypeID', N'Code', N'CountsTowardsService', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'IsPaid', N'MaxDaysPerRequest', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[LeaveTypes]'))
        SET IDENTITY_INSERT [LeaveTypes] ON;
    EXEC(N'INSERT INTO [LeaveTypes] ([LeaveTypeID], [Code], [CountsTowardsService], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsActive], [IsDeleted], [IsPaid], [MaxDaysPerRequest], [Name], [UpdatedAt])
    VALUES (1, N''ANNUAL'', CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''إجازة اعتيادية منتظمة.'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 30, N''إجازة اعتيادية'', NULL),
    (2, N''SICK'', CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''إجازة مرضية معتمدة.'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 30, N''إجازة مرضية'', NULL),
    (3, N''UNPAID'', CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''إجازة بدون معاش.'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), 90, N''إجازة بدون معاش'', NULL),
    (4, N''STUDY'', CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, N''إجازة دراسية معتمدة.'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 365, N''إجازة دراسية'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'LeaveTypeID', N'Code', N'CountsTowardsService', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'IsPaid', N'MaxDaysPerRequest', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[LeaveTypes]'))
        SET IDENTITY_INSERT [LeaveTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PensionBranchID', N'Address', N'BranchName', N'CreatedAt', N'Governorate', N'IsActive', N'ManagerName', N'ManagerPhone', N'ParentBranchId', N'Phone', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PensionFundBranches]'))
        SET IDENTITY_INSERT [PensionFundBranches] ON;
    EXEC(N'INSERT INTO [PensionFundBranches] ([PensionBranchID], [Address], [BranchName], [CreatedAt], [Governorate], [IsActive], [ManagerName], [ManagerPhone], [ParentBranchId], [Phone], [UpdatedAt])
    VALUES (1, NULL, N''الادارة العامة'', ''2026-01-01T00:00:00.0000000Z'', N''عدن'', CAST(1 AS bit), NULL, NULL, NULL, NULL, NULL),
    (4, NULL, N''فرع حضرموت'', ''2026-01-01T00:00:00.0000000Z'', N''حضرموت'', CAST(1 AS bit), NULL, NULL, NULL, NULL, NULL),
    (5, NULL, N''فرع الحديدة'', ''2026-01-01T00:00:00.0000000Z'', N''الحديدة'', CAST(1 AS bit), NULL, NULL, NULL, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PensionBranchID', N'Address', N'BranchName', N'CreatedAt', N'Governorate', N'IsActive', N'ManagerName', N'ManagerPhone', N'ParentBranchId', N'Phone', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PensionFundBranches]'))
        SET IDENTITY_INSERT [PensionFundBranches] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Category', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([PermissionId], [Category], [Code], [Description])
    VALUES (1, N''File'', N''File.Create'', N''إنشاء ملف تقاعد جديد''),
    (2, N''File'', N''File.Edit.BasicData'', N''تعديل البيانات الأساسية''),
    (3, N''Financial'', N''File.Edit.FinancialData'', N''تعديل البيانات المالية''),
    (4, N''File'', N''File.View.Sensitive'', N''رؤية الحقول الحساسة''),
    (5, N''File'', N''File.Documents.Upload'', N''رفع وإدارة الوثائق''),
    (6, N''File'', N''File.ChangeStatus.Return'', N''إعادة الملف لوجود نواقص''),
    (7, N''File'', N''File.ChangeStatus.Approve'', N''اعتماد الملف''),
    (8, N''System'', N''File.Override'', N''تجاوز قواعد النظام''),
    (9, N''File'', N''File.Delete.Soft'', N''حذف منطقي للملف''),
    (10, N''Export'', N''Export.Sensitive'', N''تصدير بيانات حساسة''),
    (11, N''Export'', N''Export.Financial'', N''تصدير بيانات مالية''),
    (12, N''Financial'', N''File.View.BankData'', N''رؤية بيانات الحساب البنكي''),
    (13, N''System'', N''Audit.ViewAll'', N''رؤية سجلات التدقيق''),
    (14, N''System'', N''System.ManageUsers'', N''إدارة المستخدمين والأدوار''),
    (15, N''System'', N''System.Settings'', N''إعدادات النظام''),
    (16, N''System'', N''System.Admin'', N''مدير النظام — صلاحية إدارية شاملة''),
    (17, N''EntitledPerson'', N''EntitledPerson.View'', N''عرض بيانات المستحقين''),
    (18, N''EntitledPerson'', N''EntitledPerson.Create'', N''إضافة مستحق جديد''),
    (19, N''EntitledPerson'', N''EntitledPerson.Edit'', N''تعديل بيانات مستحق''),
    (20, N''EntitledPerson'', N''EntitledPerson.Delete'', N''حذف مستحق''),
    (21, N''EntitledPerson'', N''EntitledPerson.Lifecycle'', N''إيقاف/إعادة تفعيل استحقاق''),
    (22, N''Guardian'', N''Guardian.View'', N''عرض بيانات الأوصياء''),
    (23, N''Guardian'', N''Guardian.Manage'', N''إدارة الوصايا''),
    (24, N''Retiree'', N''Retiree.View'', N''عرض بيانات المتقاعدين''),
    (25, N''Retiree'', N''Retiree.Edit'', N''تعديل بيانات المتقاعد''),
    (26, N''Retiree'', N''Retiree.Delete'', N''حذف متقاعد''),
    (27, N''Agent'', N''Agent.View'', N''عرض بيانات الوكلاء''),
    (28, N''Agent'', N''Agent.Manage'', N''إدارة الوكلاء''),
    (29, N''FileNote'', N''FileNote.View'', N''عرض ملاحظات الملفات''),
    (30, N''FileNote'', N''FileNote.Manage'', N''إدارة ملاحظات الملفات''),
    (31, N''DocumentExemption'', N''DocumentExemption.View'', N''عرض إعفاءات الوثائق''),
    (32, N''DocumentExemption'', N''DocumentExemption.Manage'', N''إدارة إعفاءات الوثائق''),
    (33, N''DocumentPledge'', N''DocumentPledge.View'', N''عرض التعهدات''),
    (34, N''DocumentPledge'', N''DocumentPledge.Manage'', N''إدارة التعهدات''),
    (35, N''MissingDocument'', N''MissingDocument.View'', N''عرض الوثائق الناقصة''),
    (36, N''MissingDocument'', N''MissingDocument.Manage'', N''إدارة الوثائق الناقصة''),
    (37, N''PaymentBook'', N''PaymentBook.View'', N''عرض دفاتر الصرف''),
    (38, N''PaymentBook'', N''PaymentBook.Manage'', N''إدارة دفاتر الصرف''),
    (39, N''Ministry'', N''Ministry.View'', N''عرض كشوفات إحالة إلى التقاعد''),
    (40, N''Ministry'', N''Ministry.Manage'', N''إدارة كشوفات إحالة إلى التقاعد''),
    (41, N''Suspension'', N''Suspension.View'', N''عرض إيقافات المعاش''),
    (42, N''Suspension'', N''Suspension.Manage'', N''إدارة إيقافات المعاش'');
    INSERT INTO [Permissions] ([PermissionId], [Category], [Code], [Description])
    VALUES (43, N''Dashboard'', N''Dashboard.View'', N''عرض لوحة التحكم''),
    (44, N''Settlement'', N''Settlement.View'', N''عرض التسويات''),
    (45, N''Settlement'', N''Settlement.Manage'', N''إدارة التسويات''),
    (46, N''SalaryRecipient'', N''SalaryRecipient.View'', N''عرض مستلمي المعاشات''),
    (47, N''SalaryRecipient'', N''SalaryRecipient.Manage'', N''إدارة مستلمي المعاشات''),
    (48, N''SalaryRecipient'', N''SalaryRecipient.Add'', N''إضافة مستلم معاش''),
    (49, N''Guardian'', N''Guardian.Add'', N''إضافة وصي جديد''),
    (50, N''Agent'', N''Agent.Add'', N''إضافة وكيل جديد''),
    (51, N''PaymentBook'', N''PaymentBook.Add'', N''إصدار دفتر صرف جديد''),
    (52, N''DocumentExemption'', N''DocumentExemption.Add'', N''إضافة إعفاء جديد''),
    (53, N''DocumentPledge'', N''DocumentPledge.Add'', N''إضافة تعهد جديد''),
    (54, N''Guardian'', N''Guardian.Edit'', N''تعديل بيانات وصي''),
    (55, N''Agent'', N''Agent.Edit'', N''تعديل بيانات وكيل''),
    (56, N''DocumentPledge'', N''DocumentPledge.Edit'', N''تعديل تعهد''),
    (57, N''DocumentExemption'', N''DocumentExemption.Edit'', N''تعديل إعفاء''),
    (58, N''PaymentBook'', N''PaymentBook.Edit'', N''تعديل دفتر صرف''),
    (59, N''SalaryRecipient'', N''SalaryRecipient.Edit'', N''تعديل مستلم معاش''),
    (60, N''FileNote'', N''FileNote.Edit'', N''تعديل ملاحظة ملف''),
    (61, N''MissingDocument'', N''MissingDocument.Edit'', N''تعديل وثيقة ناقصة''),
    (62, N''Ministry'', N''Ministry.Edit'', N''تعديل كشف إحالة إلى التقاعد''),
    (63, N''Suspension'', N''Suspension.Edit'', N''تعديل إيقاف معاش''),
    (64, N''Settlement'', N''Settlement.Edit'', N''تعديل تسوية''),
    (65, N''Suspension'', N''Suspension.Add'', N''إضافة إيقاف معاش''),
    (66, N''SalaryBatch'', N''SalaryBatch.View'', N''عرض كشوفات صرف المعاشات''),
    (67, N''SalaryBatch'', N''SalaryBatch.Manage'', N''إدارة كشوفات صرف المعاشات''),
    (68, N''SalaryBatch'', N''SalaryBatch.Approve'', N''اعتماد كشوفات صرف المعاشات''),
    (69, N''SalaryBatch'', N''SalaryBatch.Export'', N''تصدير كشوفات صرف المعاشات''),
    (70, N''SalaryBatch'', N''SalaryBatch.ImportResults'', N''استيراد نتائج الحوالات لكشوف المعاشات''),
    (71, N''OperationalBudget'', N''OperationalBudget.View'', N''عرض الميزانية التشغيلية''),
    (72, N''OperationalBudget'', N''OperationalBudget.Manage'', N''إدارة الميزانية التشغيلية''),
    (73, N''Investment'', N''Investment.View'', N''عرض الاستثمارات''),
    (74, N''Investment'', N''Investment.Manage'', N''إدارة الاستثمارات''),
    (75, N''Investment'', N''Investment.Finance'', N''المالية الاستثمارية''),
    (76, N''Investment'', N''Investment.Approve'', N''اعتماد الاستثمارات''),
    (77, N''Guardian'', N''Guardian.Delete'', N''حذف وصي''),
    (78, N''Agent'', N''Agent.Delete'', N''حذف وكيل''),
    (79, N''DocumentPledge'', N''DocumentPledge.Delete'', N''حذف تعهد''),
    (80, N''DocumentExemption'', N''DocumentExemption.Delete'', N''حذف إعفاء''),
    (81, N''PaymentBook'', N''PaymentBook.Delete'', N''حذف دفتر صرف''),
    (82, N''SalaryRecipient'', N''SalaryRecipient.Delete'', N''حذف مستلم معاش''),
    (83, N''Suspension'', N''Suspension.Delete'', N''حذف إيقاف معاش''),
    (84, N''FileNote'', N''FileNote.Add'', N''إضافة ملاحظة ملف'');
    INSERT INTO [Permissions] ([PermissionId], [Category], [Code], [Description])
    VALUES (85, N''FileNote'', N''FileNote.Delete'', N''حذف ملاحظة ملف''),
    (86, N''MissingDocument'', N''MissingDocument.Add'', N''إضافة وثيقة ناقصة''),
    (87, N''MissingDocument'', N''MissingDocument.Delete'', N''حذف وثيقة ناقصة''),
    (88, N''Settlement'', N''Settlement.Add'', N''إضافة تسوية''),
    (89, N''Settlement'', N''Settlement.Delete'', N''حذف تسوية''),
    (90, N''Ministry'', N''Ministry.Add'', N''إضافة كشف إحالة إلى التقاعد''),
    (91, N''Ministry'', N''Ministry.Delete'', N''حذف كشف إحالة إلى التقاعد''),
    (92, N''OperationalBudget'', N''OperationalBudget.Add'', N''إضافة ميزانية تشغيلية''),
    (93, N''OperationalBudget'', N''OperationalBudget.Edit'', N''تعديل ميزانية تشغيلية''),
    (94, N''OperationalBudget'', N''OperationalBudget.Delete'', N''حذف ميزانية تشغيلية''),
    (95, N''Investment'', N''Investment.Add'', N''إضافة استثمار''),
    (96, N''Investment'', N''Investment.Edit'', N''تعديل استثمار''),
    (97, N''Investment'', N''Investment.Delete'', N''حذف استثمار''),
    (98, N''SalaryBatch'', N''Payroll.ExcludeBeneficiary'', N''استبعاد مستفيد من كشف صرف''),
    (99, N''SalaryBatch'', N''Payroll.ManageDeductions'', N''إدارة الاستقطاعات المالية''),
    (100, N''System'', N''System.Settings.Banks'', N''إدارة بيانات البنوك''),
    (101, N''System'', N''System.Settings.DocumentTypes'', N''إدارة أنواع الوثائق''),
    (102, N''System'', N''System.Settings.JobTitles'', N''إدارة المسميات الوظيفية''),
    (103, N''System'', N''System.Settings.InvestmentTypes'', N''إدارة أنواع الاستثمارات''),
    (104, N''HR'', N''HR.Dashboard.View'', N''عرض لوحة الشؤون الإدارية''),
    (105, N''HR'', N''HR.Reference.View'', N''عرض البيانات المرجعية للشؤون الإدارية''),
    (106, N''HR'', N''HR.Reference.Manage'', N''إدارة البيانات المرجعية للشؤون الإدارية''),
    (107, N''HR'', N''HR.Employee.View'', N''عرض الموظفين''),
    (108, N''HR'', N''HR.Employee.Create'', N''إضافة موظف''),
    (109, N''HR'', N''HR.Employee.Edit'', N''تعديل موظف''),
    (110, N''HR'', N''HR.Employee.Delete'', N''حذف موظف''),
    (111, N''HR'', N''HR.ServiceHistory.View'', N''عرض سجل الخدمة''),
    (112, N''File'', N''File.View.History'', N''عرض سجل العمليات وتاريخ الملف''),
    (113, N''Financial'', N''Financial.Reports.View'', N''عرض التقارير المالية المتقدمة''),
    (114, N''Financial'', N''Financial.Bank.Statement.View'', N''عرض كشوفات الحساب البنكي''),
    (115, N''OperationalBudget'', N''OperationalBudget.Reports.View'', N''عرض تقارير الميزانية التشغيلية''),
    (116, N''System'', N''System.Audit.Export'', N''تصدير سجلات التدقيق والعمليات''),
    (117, N''Treasury'', N''Treasury.View'', N''عرض الخزينة والحركات المالية''),
    (118, N''Treasury'', N''Treasury.Manage'', N''إدارة عمليات الخزينة (إيداع/سحب)''),
    (119, N''OperationalBudget'', N''BudgetStructure.View'', N''عرض هيكل الميزانية التعريفي''),
    (120, N''OperationalBudget'', N''BudgetStructure.Manage'', N''إدارة هيكل الميزانية التعريفي''),
    (121, N''Correspondence'', N''Correspondence.View'', N''عرض المراسلات''),
    (122, N''Correspondence'', N''Correspondence.Create'', N''إضافة قيد مراسلة''),
    (123, N''Correspondence'', N''Correspondence.Edit'', N''تعديل قيد مراسلة''),
    (124, N''Correspondence'', N''Correspondence.Delete'', N''حذف قيد مراسلة''),
    (125, N''File'', N''File.Review.View'', N''عرض لوحة مراجعة الملف''),
    (126, N''File'', N''File.Review.Approve'', N''اعتماد مراجعة الملف'');
    INSERT INTO [Permissions] ([PermissionId], [Category], [Code], [Description])
    VALUES (127, N''File'', N''File.Review.Reject'', N''رفض مراجعة الملف''),
    (128, N''Inventory'', N''Inventory.Category.View'', N''عرض تصنيفات المخزون''),
    (129, N''Inventory'', N''Inventory.Category.Manage'', N''إدارة تصنيفات المخزون''),
    (130, N''Inventory'', N''Inventory.Item.View'', N''عرض أصناف المخزون''),
    (131, N''Inventory'', N''Inventory.Item.Create'', N''إضافة صنف مخزون جديد''),
    (132, N''Inventory'', N''Inventory.Item.Edit'', N''تعديل صنف مخزون''),
    (133, N''Inventory'', N''Inventory.Item.Delete'', N''حذف صنف مخزون''),
    (134, N''Inventory'', N''Inventory.Custody.View'', N''عرض العهد الشخصية''),
    (135, N''Inventory'', N''Inventory.Custody.Create'', N''تسليم عهدة جديدة''),
    (136, N''Inventory'', N''Inventory.Custody.Return'', N''إرجاع عهدة''),
    (137, N''Inventory'', N''Inventory.Transaction.View'', N''عرض حركات المخزون''),
    (138, N''Inventory'', N''Inventory.Transaction.Create'', N''تسجيل حركة مخزون''),
    (139, N''Inventory'', N''Inventory.Stocktake'', N''تسوية الجرد'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Category', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TypeID', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsDeleted', N'TypeName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PropertyTypes]'))
        SET IDENTITY_INSERT [PropertyTypes] ON;
    EXEC(N'INSERT INTO [PropertyTypes] ([TypeID], [CreatedAt], [DeletedAt], [DeletedByUserID], [IsDeleted], [TypeName], [UpdatedAt])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''أرض'', NULL),
    (2, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''مبنى سكني'', NULL),
    (3, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''مجمع تجاري'', NULL),
    (4, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''مستودع'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TypeID', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsDeleted', N'TypeName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PropertyTypes]'))
        SET IDENTITY_INSERT [PropertyTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RankID', N'BaseSalary', N'CreatedAt', N'RankLevel', N'RankName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Ranks]'))
        SET IDENTITY_INSERT [Ranks] ON;
    EXEC(N'INSERT INTO [Ranks] ([RankID], [BaseSalary], [CreatedAt], [RankLevel], [RankName], [UpdatedAt])
    VALUES (1, 120000.0, ''2024-01-01T00:00:00.0000000'', 1, N''ملازم'', NULL),
    (2, 135000.0, ''2024-01-01T00:00:00.0000000'', 2, N''ملازم أول'', NULL),
    (3, 150000.0, ''2024-01-01T00:00:00.0000000'', 3, N''نقيب'', NULL),
    (4, 165000.0, ''2024-01-01T00:00:00.0000000'', 4, N''رائد'', NULL),
    (5, 180000.0, ''2024-01-01T00:00:00.0000000'', 5, N''مقدم'', NULL),
    (6, 200000.0, ''2024-01-01T00:00:00.0000000'', 6, N''عقيد'', NULL),
    (7, 225000.0, ''2024-01-01T00:00:00.0000000'', 7, N''عميد'', NULL),
    (8, 250000.0, ''2024-01-01T00:00:00.0000000'', 8, N''لواء'', NULL),
    (9, 280000.0, ''2024-01-01T00:00:00.0000000'', 9, N''فريق'', NULL),
    (10, 320000.0, ''2024-01-01T00:00:00.0000000'', 10, N''فريق أول'', NULL),
    (11, 65000.0, ''2024-01-01T00:00:00.0000000'', 11, N''جندي'', NULL),
    (12, 75000.0, ''2024-01-01T00:00:00.0000000'', 12, N''عريف'', NULL),
    (13, 85000.0, ''2024-01-01T00:00:00.0000000'', 13, N''رقيب ثاني'', NULL),
    (14, 95000.0, ''2024-01-01T00:00:00.0000000'', 14, N''رقيب اول'', NULL),
    (15, 105000.0, ''2024-01-01T00:00:00.0000000'', 15, N''مساعد'', NULL),
    (16, 110000.0, ''2024-01-01T00:00:00.0000000'', 16, N''مساعد ثاني'', NULL),
    (17, 115000.0, ''2024-01-01T00:00:00.0000000'', 17, N''مساعد اول'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RankID', N'BaseSalary', N'CreatedAt', N'RankLevel', N'RankName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Ranks]'))
        SET IDENTITY_INSERT [Ranks] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RelationshipID', N'CreatedAt', N'RelationshipName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Relationships]'))
        SET IDENTITY_INSERT [Relationships] ON;
    EXEC(N'INSERT INTO [Relationships] ([RelationshipID], [CreatedAt], [RelationshipName], [UpdatedAt])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', N''زوجة'', NULL),
    (2, ''2026-01-01T00:00:00.0000000Z'', N''ابن'', NULL),
    (3, ''2026-01-01T00:00:00.0000000Z'', N''بنت'', NULL),
    (4, ''2026-01-01T00:00:00.0000000Z'', N''أم'', NULL),
    (5, ''2026-01-01T00:00:00.0000000Z'', N''أب'', NULL),
    (6, ''2026-01-01T00:00:00.0000000Z'', N''أخت'', NULL),
    (7, ''2026-01-01T00:00:00.0000000Z'', N''أخ'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RelationshipID', N'CreatedAt', N'RelationshipName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Relationships]'))
        SET IDENTITY_INSERT [Relationships] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleID', N'CreatedAt', N'Description', N'IsActive', N'RoleName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] ON;
    EXEC(N'INSERT INTO [Roles] ([RoleID], [CreatedAt], [Description], [IsActive], [RoleName], [UpdatedAt])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', N''مدير النظام — الحاسب الآلي وصلاحيات كاملة'', CAST(1 AS bit), N''Admin'', NULL),
    (2, ''2026-01-01T00:00:00.0000000Z'', N''مدير إدارة المعاشات — مراجعة واعتماد الملفات'', CAST(1 AS bit), N''PensionManager'', NULL),
    (3, ''2026-01-01T00:00:00.0000000Z'', N''موظف المعاشات — إنشاء واستكمال الملفات'', CAST(1 AS bit), N''PensionClerk'', NULL),
    (4, ''2026-01-01T00:00:00.0000000Z'', N''الموظف المالي — إدارة المعاشات والتسويات المالية'', CAST(1 AS bit), N''FinanceOfficer'', NULL),
    (5, ''2026-01-01T00:00:00.0000000Z'', N''المدقق — قراءة ومراجعة فقط لجميع البيانات'', CAST(1 AS bit), N''Auditor'', NULL),
    (6, ''2026-01-01T00:00:00.0000000Z'', N''موظف الأرشيف — رفع وإدارة الوثائق فقط'', CAST(1 AS bit), N''Archivist'', NULL),
    (7, ''2026-01-01T00:00:00.0000000Z'', N''فني استثمار — عرض بيانات الصيانة والوثائق'', CAST(1 AS bit), N''Technician'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleID', N'CreatedAt', N'Description', N'IsActive', N'RoleName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SocialStatusID', N'CreatedAt', N'StatusName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[SocialStatuses]'))
        SET IDENTITY_INSERT [SocialStatuses] ON;
    EXEC(N'INSERT INTO [SocialStatuses] ([SocialStatusID], [CreatedAt], [StatusName], [UpdatedAt])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', N''أعزب'', NULL),
    (2, ''2026-01-01T00:00:00.0000000Z'', N''متزوج'', NULL),
    (3, ''2026-01-01T00:00:00.0000000Z'', N''أرمل'', NULL),
    (4, ''2026-01-01T00:00:00.0000000Z'', N''مطلق'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SocialStatusID', N'CreatedAt', N'StatusName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[SocialStatuses]'))
        SET IDENTITY_INSERT [SocialStatuses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SettingID', N'Category', N'CreatedAt', N'Description', N'SettingKey', N'SettingValue', N'UpdatedAt', N'UpdatedByUserID') AND [object_id] = OBJECT_ID(N'[SystemSettings]'))
        SET IDENTITY_INSERT [SystemSettings] ON;
    EXEC(N'INSERT INTO [SystemSettings] ([SettingID], [Category], [CreatedAt], [Description], [SettingKey], [SettingValue], [UpdatedAt], [UpdatedByUserID])
    VALUES (1, N''General'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''OrganizationName'', N''هيئة التقاعد الوطنية'', NULL, NULL),
    (2, N''Pension'', ''2026-01-01T00:00:00.0000000Z'', N''السن القانوني للتقاعد'', N''LegalRetirementAge'', N''63'', NULL, NULL),
    (3, N''Pension'', ''2026-01-01T00:00:00.0000000Z'', N''الحد الأدنى لسنوات الخدمة لاستحقاق المعاش'', N''MinimumServiceYears'', N''15'', NULL, NULL),
    (4, N''Finance'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Currency'', N''د.ع'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SettingID', N'Category', N'CreatedAt', N'Description', N'SettingKey', N'SettingValue', N'UpdatedAt', N'UpdatedByUserID') AND [object_id] = OBJECT_ID(N'[SystemSettings]'))
        SET IDENTITY_INSERT [SystemSettings] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TableID', N'CreatedAt', N'Description', N'TableNameAr', N'TableNameEn') AND [object_id] = OBJECT_ID(N'[SystemTables]'))
        SET IDENTITY_INSERT [SystemTables] ON;
    EXEC(N'INSERT INTO [SystemTables] ([TableID], [CreatedAt], [Description], [TableNameAr], [TableNameEn])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', NULL, N''المتقاعدون'', N''Retirees''),
    (2, ''2026-01-01T00:00:00.0000000Z'', NULL, N''ملفات التقاعد'', N''RetirementFiles''),
    (3, ''2026-01-01T00:00:00.0000000Z'', NULL, N''المستحقون'', N''EntitledPersons''),
    (4, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الوكلاء'', N''Agents''),
    (5, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الأوصياء'', N''Guardians''),
    (6, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الوثائق'', N''Documents''),
    (7, ''2026-01-01T00:00:00.0000000Z'', NULL, N''التعهدات'', N''DocumentPledges''),
    (8, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الوثائق الناقصة'', N''MissingDocuments''),
    (9, ''2026-01-01T00:00:00.0000000Z'', NULL, N''إعفاءات الوثائق'', N''DocumentExemptions''),
    (10, ''2026-01-01T00:00:00.0000000Z'', NULL, N''ملاحظات الملفات'', N''FileNotes''),
    (11, ''2026-01-01T00:00:00.0000000Z'', NULL, N''إيقاف المعاش'', N''PensionSuspensions''),
    (12, ''2026-01-01T00:00:00.0000000Z'', NULL, N''مستلمو المعاشات'', N''SalaryRecipients''),
    (13, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الاستحقاقات المالية'', N''FinancialEntitlements''),
    (14, ''2026-01-01T00:00:00.0000000Z'', NULL, N''كشوفات المعاشات'', N''SalaryPaymentBatches''),
    (15, ''2026-01-01T00:00:00.0000000Z'', NULL, N''تفاصيل المعاشات'', N''SalaryPaymentItems''),
    (16, ''2026-01-01T00:00:00.0000000Z'', NULL, N''كشوفات اعتماد القوة'', N''MinistryApprovalBatches''),
    (17, ''2026-01-01T00:00:00.0000000Z'', NULL, N''تفاصيل اعتماد القوة'', N''MinistryApprovalItems''),
    (18, ''2026-01-01T00:00:00.0000000Z'', NULL, N''قرارات التسوية'', N''SettlementDecisions''),
    (19, ''2026-01-01T00:00:00.0000000Z'', NULL, N''كشوفات التسويات'', N''SettlementsBatches''),
    (20, ''2026-01-01T00:00:00.0000000Z'', NULL, N''تفاصيل التسويات'', N''SettlementItems''),
    (21, ''2026-01-01T00:00:00.0000000Z'', NULL, N''المستخدمون'', N''Users''),
    (22, ''2026-01-01T00:00:00.0000000Z'', NULL, N''سجل الدخول'', N''LoginHistories''),
    (23, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الأدوار'', N''Roles''),
    (24, ''2026-01-01T00:00:00.0000000Z'', NULL, N''صلاحيات الجداول'', N''TablePermissions''),
    (25, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الإشعارات'', N''Notifications''),
    (26, ''2026-01-01T00:00:00.0000000Z'', NULL, N''سجل العمليات'', N''AuditLogs''),
    (27, ''2026-01-01T00:00:00.0000000Z'', NULL, N''قائمة المزامنة'', N''SyncQueues''),
    (28, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الأجهزة المسجلة'', N''DeviceRegistrations''),
    (29, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الرتب العسكرية'', N''Ranks''),
    (30, ''2026-01-01T00:00:00.0000000Z'', NULL, N''البنوك'', N''Banks''),
    (31, ''2026-01-01T00:00:00.0000000Z'', NULL, N''صلات القرابة'', N''Relationships''),
    (32, ''2026-01-01T00:00:00.0000000Z'', NULL, N''أنواع الوثائق'', N''DocumentTypes''),
    (33, ''2026-01-01T00:00:00.0000000Z'', NULL, N''حالات الملفات'', N''FileStatuses''),
    (34, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الحالات الاجتماعية'', N''SocialStatuses''),
    (35, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الحالات الصحية'', N''HealthStatuses''),
    (36, ''2026-01-01T00:00:00.0000000Z'', NULL, N''حالات المعاملات'', N''TransactionStatuses''),
    (37, ''2026-01-01T00:00:00.0000000Z'', NULL, N''فروع الصندوق'', N''PensionFundBranches''),
    (38, ''2026-01-01T00:00:00.0000000Z'', NULL, N''جداول النظام'', N''SystemTables''),
    (39, ''2026-01-01T00:00:00.0000000Z'', NULL, N''إعدادات النظام'', N''SystemSettings''),
    (40, ''2026-01-01T00:00:00.0000000Z'', NULL, N''قوالب التقارير'', N''ReportTemplates''),
    (41, ''2026-01-01T00:00:00.0000000Z'', NULL, N''التقارير المحفوظة'', N''SavedReports''),
    (42, ''2026-01-01T00:00:00.0000000Z'', NULL, N''أنواع التقاعد'', N''RetirementTypes'');
    INSERT INTO [SystemTables] ([TableID], [CreatedAt], [Description], [TableNameAr], [TableNameEn])
    VALUES (43, ''2026-01-01T00:00:00.0000000Z'', NULL, N''سجل تغيير الحالات'', N''StatusTransitionLogs''),
    (44, ''2026-01-01T00:00:00.0000000Z'', NULL, N''الخزائن المالية'', N''Treasuries''),
    (45, ''2026-01-01T00:00:00.0000000Z'', NULL, N''حركات الخزينة'', N''TreasuryTransactions''),
    (46, ''2026-01-01T00:00:00.0000000Z'', NULL, N''تخصيصات الميزانية'', N''BudgetAllocations'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TableID', N'CreatedAt', N'Description', N'TableNameAr', N'TableNameEn') AND [object_id] = OBJECT_ID(N'[SystemTables]'))
        SET IDENTITY_INSERT [SystemTables] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StatusID', N'CreatedAt', N'Description', N'StatusColor', N'StatusName') AND [object_id] = OBJECT_ID(N'[TransactionStatuses]'))
        SET IDENTITY_INSERT [TransactionStatuses] ON;
    EXEC(N'INSERT INTO [TransactionStatuses] ([StatusID], [CreatedAt], [Description], [StatusColor], [StatusName])
    VALUES (1, ''2026-06-25T21:49:41.3041618Z'', N''تم إنشاء الحوالة ولم ترسل بعد'', N''warning'', N''قيد الإرسال''),
    (2, ''2026-06-25T21:49:41.3041624Z'', N''تم إرسال الحوالة للبنك'', N''info'', N''تم الإرسال''),
    (3, ''2026-06-25T21:49:41.3041626Z'', N''تم استلام الحوالة من المستفيد'', N''success'', N''تم الاستلام''),
    (4, ''2026-06-25T21:49:41.3041627Z'', N''فشلت الحوالة'', N''destructive'', N''فشل''),
    (5, ''2026-06-25T21:49:41.3041628Z'', N''تم إلغاء الحوالة'', N''secondary'', N''ملغي'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StatusID', N'CreatedAt', N'Description', N'StatusColor', N'StatusName') AND [object_id] = OBJECT_ID(N'[TransactionStatuses]'))
        SET IDENTITY_INSERT [TransactionStatuses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TypeID', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsDeleted', N'TypeName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[UnitTypes]'))
        SET IDENTITY_INSERT [UnitTypes] ON;
    EXEC(N'INSERT INTO [UnitTypes] ([TypeID], [CreatedAt], [DeletedAt], [DeletedByUserID], [IsDeleted], [TypeName], [UpdatedAt])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''شقة'', NULL),
    (2, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''مكتب'', NULL),
    (3, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''معرض تجاري'', NULL),
    (4, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, CAST(0 AS bit), N''مستودع فرعي'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TypeID', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'IsDeleted', N'TypeName', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[UnitTypes]'))
        SET IDENTITY_INSERT [UnitTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SectionID', N'ChapterID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[BudgetSections]'))
        SET IDENTITY_INSERT [BudgetSections] ON;
    EXEC(N'INSERT INTO [BudgetSections] ([SectionID], [ChapterID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsActive], [IsDeleted], [Name], [UpdatedAt])
    VALUES (1, 1, N''311'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''معاشات وأجور نقدية'', NULL),
    (2, 1, N''312'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''البدلات والتعويضات الأخرى'', NULL),
    (3, 1, N''313'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''المزايا العينية'', NULL),
    (4, 1, N''314'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''المكافآت'', NULL),
    (5, 1, N''316'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''التأمينات والتقاعد'', NULL),
    (6, 2, N''322'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''الوقود والزيوت والقوى المحركة والمياه'', NULL),
    (7, 2, N''323'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قطع الغيار واللوازم'', NULL),
    (8, 2, N''325'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''القرطاسية والمطبوعات'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SectionID', N'ChapterID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[BudgetSections]'))
        SET IDENTITY_INSERT [BudgetSections] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RuleID', N'ConditionField', N'ConditionValue', N'Description', N'DocumentTypeID', N'IsActive', N'IsGlobal', N'TargetEntity') AND [object_id] = OBJECT_ID(N'[DocumentRequirementRules]'))
        SET IDENTITY_INSERT [DocumentRequirementRules] ON;
    EXEC(N'INSERT INTO [DocumentRequirementRules] ([RuleID], [ConditionField], [ConditionValue], [Description], [DocumentTypeID], [IsActive], [IsGlobal], [TargetEntity])
    VALUES (1, NULL, NULL, N''الهوية الوطنية (عالمي)'', 1, CAST(1 AS bit), CAST(1 AS bit), NULL),
    (2, NULL, NULL, N''أوليات الملف (عالمي)'', 8, CAST(1 AS bit), CAST(1 AS bit), NULL),
    (3, N''IsAlive'', N''False'', N''شهادة الوفاة للمتوفين'', 2, CAST(1 AS bit), CAST(0 AS bit), N''Retiree''),
    (4, N''Exists'', N''True'', N''وثيقة الوكالة عند وجود وكيل'', 3, CAST(1 AS bit), CAST(0 AS bit), N''Agent'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RuleID', N'ConditionField', N'ConditionValue', N'Description', N'DocumentTypeID', N'IsActive', N'IsGlobal', N'TargetEntity') AND [object_id] = OBJECT_ID(N'[DocumentRequirementRules]'))
        SET IDENTITY_INSERT [DocumentRequirementRules] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmployeeID', N'AcademicQualifications', N'Address', N'BirthDate', N'BirthPlace', N'BloodGroup', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'DepartmentID', N'EmergencyContactName', N'EmergencyContactPhone', N'EmployeeNumber', N'EmploymentStatusID', N'FullName', N'Gender', N'HireDate', N'IsDeleted', N'JobPositions', N'JobTitleID', N'MaritalStatus', N'MilitaryNumber', N'MilitaryServiceDate', N'NationalID', N'NationalIDExpiryDate', N'NationalIDIssueDate', N'NationalIDIssuePlace', N'Notes', N'Phone', N'ProfilePhotoPath', N'RankID', N'SectionID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] ON;
    EXEC(N'INSERT INTO [Employees] ([EmployeeID], [AcademicQualifications], [Address], [BirthDate], [BirthPlace], [BloodGroup], [CreatedAt], [DeletedAt], [DeletedByUserID], [DepartmentID], [EmergencyContactName], [EmergencyContactPhone], [EmployeeNumber], [EmploymentStatusID], [FullName], [Gender], [HireDate], [IsDeleted], [JobPositions], [JobTitleID], [MaritalStatus], [MilitaryNumber], [MilitaryServiceDate], [NationalID], [NationalIDExpiryDate], [NationalIDIssueDate], [NationalIDIssuePlace], [Notes], [Phone], [ProfilePhotoPath], [RankID], [SectionID], [UpdatedAt])
    VALUES (1, N''جامعي'', N''المنصورة'', ''1966-12-31T00:00:00.0000000Z'', NULL, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 6, NULL, NULL, N''EMP-001'', 1, N''جمال فضل عبدالكريم القطيبي'', CAST(1 AS tinyint), ''1984-08-04T00:00:00.0000000Z'', CAST(0 AS bit), N''المدير العام'', 1, N''متزوج'', N''2361'', ''1984-08-04T00:00:00.0000000Z'', N'''', NULL, NULL, NULL, NULL, NULL, NULL, 7, NULL, NULL),
    (2, N''دكتوراه إدارة أعمال'', N''المنصورة'', ''1990-01-28T00:00:00.0000000Z'', N''بيت الفقيه/الحديدة'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 6, NULL, NULL, N''EMP-002'', 1, N''شامخ عبدالرحمن أحمد فتاح'', CAST(1 AS tinyint), ''2009-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''نائب المدير العام المدير المالي'', 2, N''متزوج'', N''24244'', ''2009-01-01T00:00:00.0000000Z'', N'''', NULL, NULL, NULL, NULL, N''777618680'', NULL, 6, NULL, NULL),
    (3, N''بكالوريوس قانون'', N''خورمكسر/العريش'', ''1986-05-20T00:00:00.0000000Z'', N''عدن'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 6, NULL, NULL, N''EMP-003'', 1, N''نائف أحمد محمد الشاجري'', CAST(1 AS tinyint), ''2010-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''مساعد المدير العام'', 6, N''متزوج'', N''18789'', ''2010-01-01T00:00:00.0000000Z'', N'''', NULL, NULL, NULL, NULL, NULL, NULL, 6, NULL, NULL),
    (4, N''بكالوريوس'', N''لحج'', NULL, N''لحج'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 6, NULL, NULL, N''EMP-004'', 1, N''صالح علي مقبل حيدرة'', CAST(1 AS tinyint), ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''مستشار المدير العام'', 6, N''متزوج'', NULL, NULL, N'''', NULL, NULL, NULL, NULL, N''777355188'', NULL, 6, NULL, NULL),
    (5, NULL, NULL, NULL, NULL, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, NULL, N''EMP-005'', 1, N''منصور عبدالله اليماني'', CAST(1 AS tinyint), ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''مدير إدارة المعاشات'', 3, NULL, N''7443'', NULL, N'''', NULL, NULL, NULL, NULL, N''770601789'', NULL, 6, NULL, NULL),
    (6, N''ماجستير نظم معلومات'', N''المنصورة'', ''1991-07-30T00:00:00.0000000Z'', N''بيت الفقيه/الحديدة'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 6, NULL, NULL, N''EMP-006'', 1, N''كفاح عبدالرحمن أحمد فتاح'', CAST(1 AS tinyint), ''2014-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''مدير التخطيط'', 6, N''متزوج'', N''33068'', ''2014-01-01T00:00:00.0000000Z'', N'''', NULL, NULL, NULL, NULL, N''771985009'', NULL, 4, NULL, NULL),
    (13, NULL, N''الصولبان غازي علوان'', NULL, NULL, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 2, NULL, NULL, N''EMP-013'', 1, N''محمد عبده علي'', CAST(1 AS tinyint), ''2025-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''مختص النظافة'', 6, N''متزوج'', NULL, ''2025-01-01T00:00:00.0000000Z'', N'''', NULL, NULL, NULL, NULL, NULL, NULL, 11, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmployeeID', N'AcademicQualifications', N'Address', N'BirthDate', N'BirthPlace', N'BloodGroup', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'DepartmentID', N'EmergencyContactName', N'EmergencyContactPhone', N'EmployeeNumber', N'EmploymentStatusID', N'FullName', N'Gender', N'HireDate', N'IsDeleted', N'JobPositions', N'JobTitleID', N'MaritalStatus', N'MilitaryNumber', N'MilitaryServiceDate', N'NationalID', N'NationalIDExpiryDate', N'NationalIDIssueDate', N'NationalIDIssuePlace', N'Notes', N'Phone', N'ProfilePhotoPath', N'RankID', N'SectionID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PensionBranchID', N'Address', N'BranchName', N'CreatedAt', N'Governorate', N'IsActive', N'ManagerName', N'ManagerPhone', N'ParentBranchId', N'Phone', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PensionFundBranches]'))
        SET IDENTITY_INSERT [PensionFundBranches] ON;
    EXEC(N'INSERT INTO [PensionFundBranches] ([PensionBranchID], [Address], [BranchName], [CreatedAt], [Governorate], [IsActive], [ManagerName], [ManagerPhone], [ParentBranchId], [Phone], [UpdatedAt])
    VALUES (2, NULL, N''فرع صنعاء'', ''2026-01-01T00:00:00.0000000Z'', N''صنعاء'', CAST(1 AS bit), NULL, NULL, 1, NULL, NULL),
    (3, NULL, N''فرع تعز'', ''2026-01-01T00:00:00.0000000Z'', N''تعز'', CAST(1 AS bit), NULL, NULL, 1, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PensionBranchID', N'Address', N'BranchName', N'CreatedAt', N'Governorate', N'IsActive', N'ManagerName', N'ManagerPhone', N'ParentBranchId', N'Phone', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PensionFundBranches]'))
        SET IDENTITY_INSERT [PensionFundBranches] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RolePermissionId', N'CreatedAt', N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
        SET IDENTITY_INSERT [RolePermissions] ON;
    EXEC(N'INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1000, ''2026-01-01T00:00:00.0000000Z'', 1, 1),
    (1001, ''2026-01-01T00:00:00.0000000Z'', 2, 1),
    (1002, ''2026-01-01T00:00:00.0000000Z'', 3, 1),
    (1003, ''2026-01-01T00:00:00.0000000Z'', 4, 1),
    (1004, ''2026-01-01T00:00:00.0000000Z'', 5, 1),
    (1005, ''2026-01-01T00:00:00.0000000Z'', 6, 1),
    (1006, ''2026-01-01T00:00:00.0000000Z'', 7, 1),
    (1007, ''2026-01-01T00:00:00.0000000Z'', 8, 1),
    (1008, ''2026-01-01T00:00:00.0000000Z'', 9, 1),
    (1009, ''2026-01-01T00:00:00.0000000Z'', 10, 1),
    (1010, ''2026-01-01T00:00:00.0000000Z'', 11, 1),
    (1011, ''2026-01-01T00:00:00.0000000Z'', 12, 1),
    (1012, ''2026-01-01T00:00:00.0000000Z'', 13, 1),
    (1013, ''2026-01-01T00:00:00.0000000Z'', 14, 1),
    (1014, ''2026-01-01T00:00:00.0000000Z'', 15, 1),
    (1015, ''2026-01-01T00:00:00.0000000Z'', 16, 1),
    (1016, ''2026-01-01T00:00:00.0000000Z'', 17, 1),
    (1017, ''2026-01-01T00:00:00.0000000Z'', 18, 1),
    (1018, ''2026-01-01T00:00:00.0000000Z'', 19, 1),
    (1019, ''2026-01-01T00:00:00.0000000Z'', 20, 1),
    (1020, ''2026-01-01T00:00:00.0000000Z'', 21, 1),
    (1021, ''2026-01-01T00:00:00.0000000Z'', 22, 1),
    (1022, ''2026-01-01T00:00:00.0000000Z'', 23, 1),
    (1023, ''2026-01-01T00:00:00.0000000Z'', 24, 1),
    (1024, ''2026-01-01T00:00:00.0000000Z'', 25, 1),
    (1025, ''2026-01-01T00:00:00.0000000Z'', 26, 1),
    (1026, ''2026-01-01T00:00:00.0000000Z'', 27, 1),
    (1027, ''2026-01-01T00:00:00.0000000Z'', 28, 1),
    (1028, ''2026-01-01T00:00:00.0000000Z'', 29, 1),
    (1029, ''2026-01-01T00:00:00.0000000Z'', 30, 1),
    (1030, ''2026-01-01T00:00:00.0000000Z'', 31, 1),
    (1031, ''2026-01-01T00:00:00.0000000Z'', 32, 1),
    (1032, ''2026-01-01T00:00:00.0000000Z'', 33, 1),
    (1033, ''2026-01-01T00:00:00.0000000Z'', 34, 1),
    (1034, ''2026-01-01T00:00:00.0000000Z'', 35, 1),
    (1035, ''2026-01-01T00:00:00.0000000Z'', 36, 1),
    (1036, ''2026-01-01T00:00:00.0000000Z'', 37, 1),
    (1037, ''2026-01-01T00:00:00.0000000Z'', 38, 1),
    (1038, ''2026-01-01T00:00:00.0000000Z'', 39, 1),
    (1039, ''2026-01-01T00:00:00.0000000Z'', 40, 1),
    (1040, ''2026-01-01T00:00:00.0000000Z'', 41, 1),
    (1041, ''2026-01-01T00:00:00.0000000Z'', 42, 1);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1042, ''2026-01-01T00:00:00.0000000Z'', 43, 1),
    (1043, ''2026-01-01T00:00:00.0000000Z'', 44, 1),
    (1044, ''2026-01-01T00:00:00.0000000Z'', 45, 1),
    (1045, ''2026-01-01T00:00:00.0000000Z'', 46, 1),
    (1046, ''2026-01-01T00:00:00.0000000Z'', 47, 1),
    (1047, ''2026-01-01T00:00:00.0000000Z'', 48, 1),
    (1048, ''2026-01-01T00:00:00.0000000Z'', 49, 1),
    (1049, ''2026-01-01T00:00:00.0000000Z'', 50, 1),
    (1050, ''2026-01-01T00:00:00.0000000Z'', 51, 1),
    (1051, ''2026-01-01T00:00:00.0000000Z'', 52, 1),
    (1052, ''2026-01-01T00:00:00.0000000Z'', 53, 1),
    (1053, ''2026-01-01T00:00:00.0000000Z'', 54, 1),
    (1054, ''2026-01-01T00:00:00.0000000Z'', 55, 1),
    (1055, ''2026-01-01T00:00:00.0000000Z'', 56, 1),
    (1056, ''2026-01-01T00:00:00.0000000Z'', 57, 1),
    (1057, ''2026-01-01T00:00:00.0000000Z'', 58, 1),
    (1058, ''2026-01-01T00:00:00.0000000Z'', 59, 1),
    (1059, ''2026-01-01T00:00:00.0000000Z'', 60, 1),
    (1060, ''2026-01-01T00:00:00.0000000Z'', 61, 1),
    (1061, ''2026-01-01T00:00:00.0000000Z'', 62, 1),
    (1062, ''2026-01-01T00:00:00.0000000Z'', 63, 1),
    (1063, ''2026-01-01T00:00:00.0000000Z'', 64, 1),
    (1064, ''2026-01-01T00:00:00.0000000Z'', 65, 1),
    (1065, ''2026-01-01T00:00:00.0000000Z'', 66, 1),
    (1066, ''2026-01-01T00:00:00.0000000Z'', 67, 1),
    (1067, ''2026-01-01T00:00:00.0000000Z'', 68, 1),
    (1068, ''2026-01-01T00:00:00.0000000Z'', 69, 1),
    (1069, ''2026-01-01T00:00:00.0000000Z'', 70, 1),
    (1070, ''2026-01-01T00:00:00.0000000Z'', 71, 1),
    (1071, ''2026-01-01T00:00:00.0000000Z'', 72, 1),
    (1072, ''2026-01-01T00:00:00.0000000Z'', 73, 1),
    (1073, ''2026-01-01T00:00:00.0000000Z'', 74, 1),
    (1074, ''2026-01-01T00:00:00.0000000Z'', 75, 1),
    (1075, ''2026-01-01T00:00:00.0000000Z'', 76, 1),
    (1076, ''2026-01-01T00:00:00.0000000Z'', 77, 1),
    (1077, ''2026-01-01T00:00:00.0000000Z'', 78, 1),
    (1078, ''2026-01-01T00:00:00.0000000Z'', 79, 1),
    (1079, ''2026-01-01T00:00:00.0000000Z'', 80, 1),
    (1080, ''2026-01-01T00:00:00.0000000Z'', 81, 1),
    (1081, ''2026-01-01T00:00:00.0000000Z'', 82, 1),
    (1082, ''2026-01-01T00:00:00.0000000Z'', 83, 1),
    (1083, ''2026-01-01T00:00:00.0000000Z'', 84, 1);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1084, ''2026-01-01T00:00:00.0000000Z'', 85, 1),
    (1085, ''2026-01-01T00:00:00.0000000Z'', 86, 1),
    (1086, ''2026-01-01T00:00:00.0000000Z'', 87, 1),
    (1087, ''2026-01-01T00:00:00.0000000Z'', 88, 1),
    (1088, ''2026-01-01T00:00:00.0000000Z'', 89, 1),
    (1089, ''2026-01-01T00:00:00.0000000Z'', 90, 1),
    (1090, ''2026-01-01T00:00:00.0000000Z'', 91, 1),
    (1091, ''2026-01-01T00:00:00.0000000Z'', 92, 1),
    (1092, ''2026-01-01T00:00:00.0000000Z'', 93, 1),
    (1093, ''2026-01-01T00:00:00.0000000Z'', 94, 1),
    (1094, ''2026-01-01T00:00:00.0000000Z'', 95, 1),
    (1095, ''2026-01-01T00:00:00.0000000Z'', 96, 1),
    (1096, ''2026-01-01T00:00:00.0000000Z'', 97, 1),
    (1097, ''2026-01-01T00:00:00.0000000Z'', 98, 1),
    (1098, ''2026-01-01T00:00:00.0000000Z'', 99, 1),
    (1099, ''2026-01-01T00:00:00.0000000Z'', 100, 1),
    (1100, ''2026-01-01T00:00:00.0000000Z'', 101, 1),
    (1101, ''2026-01-01T00:00:00.0000000Z'', 102, 1),
    (1102, ''2026-01-01T00:00:00.0000000Z'', 103, 1),
    (1103, ''2026-01-01T00:00:00.0000000Z'', 104, 1),
    (1104, ''2026-01-01T00:00:00.0000000Z'', 105, 1),
    (1105, ''2026-01-01T00:00:00.0000000Z'', 106, 1),
    (1106, ''2026-01-01T00:00:00.0000000Z'', 107, 1),
    (1107, ''2026-01-01T00:00:00.0000000Z'', 108, 1),
    (1108, ''2026-01-01T00:00:00.0000000Z'', 109, 1),
    (1109, ''2026-01-01T00:00:00.0000000Z'', 110, 1),
    (1110, ''2026-01-01T00:00:00.0000000Z'', 111, 1),
    (1111, ''2026-01-01T00:00:00.0000000Z'', 112, 1),
    (1112, ''2026-01-01T00:00:00.0000000Z'', 113, 1),
    (1113, ''2026-01-01T00:00:00.0000000Z'', 114, 1),
    (1114, ''2026-01-01T00:00:00.0000000Z'', 115, 1),
    (1115, ''2026-01-01T00:00:00.0000000Z'', 116, 1),
    (1116, ''2026-01-01T00:00:00.0000000Z'', 117, 1),
    (1117, ''2026-01-01T00:00:00.0000000Z'', 118, 1),
    (1118, ''2026-01-01T00:00:00.0000000Z'', 119, 1),
    (1119, ''2026-01-01T00:00:00.0000000Z'', 120, 1),
    (1120, ''2026-01-01T00:00:00.0000000Z'', 121, 1),
    (1121, ''2026-01-01T00:00:00.0000000Z'', 122, 1),
    (1122, ''2026-01-01T00:00:00.0000000Z'', 123, 1),
    (1123, ''2026-01-01T00:00:00.0000000Z'', 124, 1),
    (1124, ''2026-01-01T00:00:00.0000000Z'', 125, 1),
    (1125, ''2026-01-01T00:00:00.0000000Z'', 126, 1);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1126, ''2026-01-01T00:00:00.0000000Z'', 127, 1),
    (1127, ''2026-01-01T00:00:00.0000000Z'', 1, 2),
    (1128, ''2026-01-01T00:00:00.0000000Z'', 2, 2),
    (1129, ''2026-01-01T00:00:00.0000000Z'', 4, 2),
    (1130, ''2026-01-01T00:00:00.0000000Z'', 5, 2),
    (1131, ''2026-01-01T00:00:00.0000000Z'', 6, 2),
    (1132, ''2026-01-01T00:00:00.0000000Z'', 7, 2),
    (1133, ''2026-01-01T00:00:00.0000000Z'', 9, 2),
    (1134, ''2026-01-01T00:00:00.0000000Z'', 13, 2),
    (1135, ''2026-01-01T00:00:00.0000000Z'', 17, 2),
    (1136, ''2026-01-01T00:00:00.0000000Z'', 18, 2),
    (1137, ''2026-01-01T00:00:00.0000000Z'', 19, 2),
    (1138, ''2026-01-01T00:00:00.0000000Z'', 20, 2),
    (1139, ''2026-01-01T00:00:00.0000000Z'', 21, 2),
    (1140, ''2026-01-01T00:00:00.0000000Z'', 22, 2),
    (1141, ''2026-01-01T00:00:00.0000000Z'', 23, 2),
    (1142, ''2026-01-01T00:00:00.0000000Z'', 49, 2),
    (1143, ''2026-01-01T00:00:00.0000000Z'', 54, 2),
    (1144, ''2026-01-01T00:00:00.0000000Z'', 77, 2),
    (1145, ''2026-01-01T00:00:00.0000000Z'', 24, 2),
    (1146, ''2026-01-01T00:00:00.0000000Z'', 25, 2),
    (1147, ''2026-01-01T00:00:00.0000000Z'', 26, 2),
    (1148, ''2026-01-01T00:00:00.0000000Z'', 27, 2),
    (1149, ''2026-01-01T00:00:00.0000000Z'', 28, 2),
    (1150, ''2026-01-01T00:00:00.0000000Z'', 50, 2),
    (1151, ''2026-01-01T00:00:00.0000000Z'', 55, 2),
    (1152, ''2026-01-01T00:00:00.0000000Z'', 78, 2),
    (1153, ''2026-01-01T00:00:00.0000000Z'', 29, 2),
    (1154, ''2026-01-01T00:00:00.0000000Z'', 30, 2),
    (1155, ''2026-01-01T00:00:00.0000000Z'', 60, 2),
    (1156, ''2026-01-01T00:00:00.0000000Z'', 84, 2),
    (1157, ''2026-01-01T00:00:00.0000000Z'', 85, 2),
    (1158, ''2026-01-01T00:00:00.0000000Z'', 31, 2),
    (1159, ''2026-01-01T00:00:00.0000000Z'', 32, 2),
    (1160, ''2026-01-01T00:00:00.0000000Z'', 52, 2),
    (1161, ''2026-01-01T00:00:00.0000000Z'', 57, 2),
    (1162, ''2026-01-01T00:00:00.0000000Z'', 80, 2),
    (1163, ''2026-01-01T00:00:00.0000000Z'', 33, 2),
    (1164, ''2026-01-01T00:00:00.0000000Z'', 34, 2),
    (1165, ''2026-01-01T00:00:00.0000000Z'', 53, 2),
    (1166, ''2026-01-01T00:00:00.0000000Z'', 56, 2),
    (1167, ''2026-01-01T00:00:00.0000000Z'', 79, 2);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1168, ''2026-01-01T00:00:00.0000000Z'', 35, 2),
    (1169, ''2026-01-01T00:00:00.0000000Z'', 36, 2),
    (1170, ''2026-01-01T00:00:00.0000000Z'', 61, 2),
    (1171, ''2026-01-01T00:00:00.0000000Z'', 86, 2),
    (1172, ''2026-01-01T00:00:00.0000000Z'', 87, 2),
    (1173, ''2026-01-01T00:00:00.0000000Z'', 37, 2),
    (1174, ''2026-01-01T00:00:00.0000000Z'', 38, 2),
    (1175, ''2026-01-01T00:00:00.0000000Z'', 51, 2),
    (1176, ''2026-01-01T00:00:00.0000000Z'', 58, 2),
    (1177, ''2026-01-01T00:00:00.0000000Z'', 81, 2),
    (1178, ''2026-01-01T00:00:00.0000000Z'', 39, 2),
    (1179, ''2026-01-01T00:00:00.0000000Z'', 40, 2),
    (1180, ''2026-01-01T00:00:00.0000000Z'', 62, 2),
    (1181, ''2026-01-01T00:00:00.0000000Z'', 90, 2),
    (1182, ''2026-01-01T00:00:00.0000000Z'', 91, 2),
    (1183, ''2026-01-01T00:00:00.0000000Z'', 41, 2),
    (1184, ''2026-01-01T00:00:00.0000000Z'', 42, 2),
    (1185, ''2026-01-01T00:00:00.0000000Z'', 63, 2),
    (1186, ''2026-01-01T00:00:00.0000000Z'', 65, 2),
    (1187, ''2026-01-01T00:00:00.0000000Z'', 83, 2),
    (1188, ''2026-01-01T00:00:00.0000000Z'', 88, 2),
    (1189, ''2026-01-01T00:00:00.0000000Z'', 89, 2),
    (1190, ''2026-01-01T00:00:00.0000000Z'', 46, 2),
    (1191, ''2026-01-01T00:00:00.0000000Z'', 82, 2),
    (1192, ''2026-01-01T00:00:00.0000000Z'', 66, 2),
    (1193, ''2026-01-01T00:00:00.0000000Z'', 43, 2),
    (1194, ''2026-01-01T00:00:00.0000000Z'', 112, 2),
    (1195, ''2026-01-01T00:00:00.0000000Z'', 117, 2),
    (1196, ''2026-01-01T00:00:00.0000000Z'', 121, 2),
    (1197, ''2026-01-01T00:00:00.0000000Z'', 122, 2),
    (1198, ''2026-01-01T00:00:00.0000000Z'', 123, 2),
    (1199, ''2026-01-01T00:00:00.0000000Z'', 125, 2),
    (1200, ''2026-01-01T00:00:00.0000000Z'', 126, 2),
    (1201, ''2026-01-01T00:00:00.0000000Z'', 127, 2),
    (1202, ''2026-01-01T00:00:00.0000000Z'', 1, 3),
    (1203, ''2026-01-01T00:00:00.0000000Z'', 2, 3),
    (1204, ''2026-01-01T00:00:00.0000000Z'', 4, 3),
    (1205, ''2026-01-01T00:00:00.0000000Z'', 5, 3),
    (1206, ''2026-01-01T00:00:00.0000000Z'', 17, 3),
    (1207, ''2026-01-01T00:00:00.0000000Z'', 18, 3),
    (1208, ''2026-01-01T00:00:00.0000000Z'', 19, 3),
    (1209, ''2026-01-01T00:00:00.0000000Z'', 22, 3);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1210, ''2026-01-01T00:00:00.0000000Z'', 23, 3),
    (1211, ''2026-01-01T00:00:00.0000000Z'', 49, 3),
    (1212, ''2026-01-01T00:00:00.0000000Z'', 54, 3),
    (1213, ''2026-01-01T00:00:00.0000000Z'', 24, 3),
    (1214, ''2026-01-01T00:00:00.0000000Z'', 25, 3),
    (1215, ''2026-01-01T00:00:00.0000000Z'', 27, 3),
    (1216, ''2026-01-01T00:00:00.0000000Z'', 28, 3),
    (1217, ''2026-01-01T00:00:00.0000000Z'', 50, 3),
    (1218, ''2026-01-01T00:00:00.0000000Z'', 55, 3),
    (1219, ''2026-01-01T00:00:00.0000000Z'', 29, 3),
    (1220, ''2026-01-01T00:00:00.0000000Z'', 30, 3),
    (1221, ''2026-01-01T00:00:00.0000000Z'', 60, 3),
    (1222, ''2026-01-01T00:00:00.0000000Z'', 84, 3),
    (1223, ''2026-01-01T00:00:00.0000000Z'', 31, 3),
    (1224, ''2026-01-01T00:00:00.0000000Z'', 32, 3),
    (1225, ''2026-01-01T00:00:00.0000000Z'', 52, 3),
    (1226, ''2026-01-01T00:00:00.0000000Z'', 57, 3),
    (1227, ''2026-01-01T00:00:00.0000000Z'', 33, 3),
    (1228, ''2026-01-01T00:00:00.0000000Z'', 34, 3),
    (1229, ''2026-01-01T00:00:00.0000000Z'', 53, 3),
    (1230, ''2026-01-01T00:00:00.0000000Z'', 56, 3),
    (1231, ''2026-01-01T00:00:00.0000000Z'', 35, 3),
    (1232, ''2026-01-01T00:00:00.0000000Z'', 36, 3),
    (1233, ''2026-01-01T00:00:00.0000000Z'', 61, 3),
    (1234, ''2026-01-01T00:00:00.0000000Z'', 86, 3),
    (1235, ''2026-01-01T00:00:00.0000000Z'', 37, 3),
    (1236, ''2026-01-01T00:00:00.0000000Z'', 38, 3),
    (1237, ''2026-01-01T00:00:00.0000000Z'', 51, 3),
    (1238, ''2026-01-01T00:00:00.0000000Z'', 58, 3),
    (1239, ''2026-01-01T00:00:00.0000000Z'', 46, 3),
    (1240, ''2026-01-01T00:00:00.0000000Z'', 47, 3),
    (1241, ''2026-01-01T00:00:00.0000000Z'', 48, 3),
    (1242, ''2026-01-01T00:00:00.0000000Z'', 59, 3),
    (1243, ''2026-01-01T00:00:00.0000000Z'', 43, 3),
    (1244, ''2026-01-01T00:00:00.0000000Z'', 121, 3),
    (1245, ''2026-01-01T00:00:00.0000000Z'', 122, 3),
    (1246, ''2026-01-01T00:00:00.0000000Z'', 3, 4),
    (1247, ''2026-01-01T00:00:00.0000000Z'', 4, 4),
    (1248, ''2026-01-01T00:00:00.0000000Z'', 11, 4),
    (1249, ''2026-01-01T00:00:00.0000000Z'', 12, 4),
    (1250, ''2026-01-01T00:00:00.0000000Z'', 17, 4),
    (1251, ''2026-01-01T00:00:00.0000000Z'', 22, 4);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1252, ''2026-01-01T00:00:00.0000000Z'', 49, 4),
    (1253, ''2026-01-01T00:00:00.0000000Z'', 54, 4),
    (1254, ''2026-01-01T00:00:00.0000000Z'', 24, 4),
    (1255, ''2026-01-01T00:00:00.0000000Z'', 27, 4),
    (1256, ''2026-01-01T00:00:00.0000000Z'', 50, 4),
    (1257, ''2026-01-01T00:00:00.0000000Z'', 55, 4),
    (1258, ''2026-01-01T00:00:00.0000000Z'', 29, 4),
    (1259, ''2026-01-01T00:00:00.0000000Z'', 31, 4),
    (1260, ''2026-01-01T00:00:00.0000000Z'', 52, 4),
    (1261, ''2026-01-01T00:00:00.0000000Z'', 57, 4),
    (1262, ''2026-01-01T00:00:00.0000000Z'', 33, 4),
    (1263, ''2026-01-01T00:00:00.0000000Z'', 53, 4),
    (1264, ''2026-01-01T00:00:00.0000000Z'', 56, 4),
    (1265, ''2026-01-01T00:00:00.0000000Z'', 35, 4),
    (1266, ''2026-01-01T00:00:00.0000000Z'', 37, 4),
    (1267, ''2026-01-01T00:00:00.0000000Z'', 38, 4),
    (1268, ''2026-01-01T00:00:00.0000000Z'', 51, 4),
    (1269, ''2026-01-01T00:00:00.0000000Z'', 58, 4),
    (1270, ''2026-01-01T00:00:00.0000000Z'', 39, 4),
    (1271, ''2026-01-01T00:00:00.0000000Z'', 41, 4),
    (1272, ''2026-01-01T00:00:00.0000000Z'', 42, 4),
    (1273, ''2026-01-01T00:00:00.0000000Z'', 63, 4),
    (1274, ''2026-01-01T00:00:00.0000000Z'', 65, 4),
    (1275, ''2026-01-01T00:00:00.0000000Z'', 43, 4),
    (1276, ''2026-01-01T00:00:00.0000000Z'', 44, 4),
    (1277, ''2026-01-01T00:00:00.0000000Z'', 45, 4),
    (1278, ''2026-01-01T00:00:00.0000000Z'', 64, 4),
    (1279, ''2026-01-01T00:00:00.0000000Z'', 88, 4),
    (1280, ''2026-01-01T00:00:00.0000000Z'', 46, 4),
    (1281, ''2026-01-01T00:00:00.0000000Z'', 47, 4),
    (1282, ''2026-01-01T00:00:00.0000000Z'', 48, 4),
    (1283, ''2026-01-01T00:00:00.0000000Z'', 59, 4),
    (1284, ''2026-01-01T00:00:00.0000000Z'', 66, 4),
    (1285, ''2026-01-01T00:00:00.0000000Z'', 67, 4),
    (1286, ''2026-01-01T00:00:00.0000000Z'', 68, 4),
    (1287, ''2026-01-01T00:00:00.0000000Z'', 69, 4),
    (1288, ''2026-01-01T00:00:00.0000000Z'', 70, 4),
    (1289, ''2026-01-01T00:00:00.0000000Z'', 71, 4),
    (1290, ''2026-01-01T00:00:00.0000000Z'', 72, 4),
    (1291, ''2026-01-01T00:00:00.0000000Z'', 92, 4),
    (1292, ''2026-01-01T00:00:00.0000000Z'', 93, 4),
    (1293, ''2026-01-01T00:00:00.0000000Z'', 73, 4);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1294, ''2026-01-01T00:00:00.0000000Z'', 75, 4),
    (1295, ''2026-01-01T00:00:00.0000000Z'', 95, 4),
    (1296, ''2026-01-01T00:00:00.0000000Z'', 96, 4),
    (1297, ''2026-01-01T00:00:00.0000000Z'', 98, 4),
    (1298, ''2026-01-01T00:00:00.0000000Z'', 99, 4),
    (1299, ''2026-01-01T00:00:00.0000000Z'', 113, 4),
    (1300, ''2026-01-01T00:00:00.0000000Z'', 114, 4),
    (1301, ''2026-01-01T00:00:00.0000000Z'', 115, 4),
    (1302, ''2026-01-01T00:00:00.0000000Z'', 116, 4),
    (1303, ''2026-01-01T00:00:00.0000000Z'', 117, 4),
    (1304, ''2026-01-01T00:00:00.0000000Z'', 118, 4),
    (1305, ''2026-01-01T00:00:00.0000000Z'', 119, 4),
    (1306, ''2026-01-01T00:00:00.0000000Z'', 120, 4),
    (1307, ''2026-01-01T00:00:00.0000000Z'', 121, 4),
    (1308, ''2026-01-01T00:00:00.0000000Z'', 4, 5),
    (1309, ''2026-01-01T00:00:00.0000000Z'', 13, 5),
    (1310, ''2026-01-01T00:00:00.0000000Z'', 17, 5),
    (1311, ''2026-01-01T00:00:00.0000000Z'', 22, 5),
    (1312, ''2026-01-01T00:00:00.0000000Z'', 24, 5),
    (1313, ''2026-01-01T00:00:00.0000000Z'', 27, 5),
    (1314, ''2026-01-01T00:00:00.0000000Z'', 29, 5),
    (1315, ''2026-01-01T00:00:00.0000000Z'', 31, 5),
    (1316, ''2026-01-01T00:00:00.0000000Z'', 33, 5),
    (1317, ''2026-01-01T00:00:00.0000000Z'', 35, 5),
    (1318, ''2026-01-01T00:00:00.0000000Z'', 37, 5),
    (1319, ''2026-01-01T00:00:00.0000000Z'', 39, 5),
    (1320, ''2026-01-01T00:00:00.0000000Z'', 41, 5),
    (1321, ''2026-01-01T00:00:00.0000000Z'', 43, 5),
    (1322, ''2026-01-01T00:00:00.0000000Z'', 44, 5),
    (1323, ''2026-01-01T00:00:00.0000000Z'', 46, 5),
    (1324, ''2026-01-01T00:00:00.0000000Z'', 66, 5),
    (1325, ''2026-01-01T00:00:00.0000000Z'', 71, 5),
    (1326, ''2026-01-01T00:00:00.0000000Z'', 73, 5),
    (1327, ''2026-01-01T00:00:00.0000000Z'', 113, 5),
    (1328, ''2026-01-01T00:00:00.0000000Z'', 117, 5),
    (1329, ''2026-01-01T00:00:00.0000000Z'', 121, 5),
    (1330, ''2026-01-01T00:00:00.0000000Z'', 125, 5),
    (1331, ''2026-01-01T00:00:00.0000000Z'', 126, 5),
    (1332, ''2026-01-01T00:00:00.0000000Z'', 127, 5),
    (1333, ''2026-01-01T00:00:00.0000000Z'', 5, 6),
    (1334, ''2026-01-01T00:00:00.0000000Z'', 17, 6),
    (1335, ''2026-01-01T00:00:00.0000000Z'', 22, 6);
    INSERT INTO [RolePermissions] ([RolePermissionId], [CreatedAt], [PermissionId], [RoleId])
    VALUES (1336, ''2026-01-01T00:00:00.0000000Z'', 24, 6),
    (1337, ''2026-01-01T00:00:00.0000000Z'', 27, 6),
    (1338, ''2026-01-01T00:00:00.0000000Z'', 29, 6),
    (1339, ''2026-01-01T00:00:00.0000000Z'', 30, 6),
    (1340, ''2026-01-01T00:00:00.0000000Z'', 60, 6),
    (1341, ''2026-01-01T00:00:00.0000000Z'', 84, 6),
    (1342, ''2026-01-01T00:00:00.0000000Z'', 31, 6),
    (1343, ''2026-01-01T00:00:00.0000000Z'', 32, 6),
    (1344, ''2026-01-01T00:00:00.0000000Z'', 52, 6),
    (1345, ''2026-01-01T00:00:00.0000000Z'', 57, 6),
    (1346, ''2026-01-01T00:00:00.0000000Z'', 33, 6),
    (1347, ''2026-01-01T00:00:00.0000000Z'', 34, 6),
    (1348, ''2026-01-01T00:00:00.0000000Z'', 53, 6),
    (1349, ''2026-01-01T00:00:00.0000000Z'', 56, 6),
    (1350, ''2026-01-01T00:00:00.0000000Z'', 35, 6),
    (1351, ''2026-01-01T00:00:00.0000000Z'', 36, 6),
    (1352, ''2026-01-01T00:00:00.0000000Z'', 61, 6),
    (1353, ''2026-01-01T00:00:00.0000000Z'', 86, 6),
    (1354, ''2026-01-01T00:00:00.0000000Z'', 37, 6),
    (1355, ''2026-01-01T00:00:00.0000000Z'', 43, 6),
    (1356, ''2026-01-01T00:00:00.0000000Z'', 121, 6),
    (1357, ''2026-01-01T00:00:00.0000000Z'', 122, 6),
    (1358, ''2026-01-01T00:00:00.0000000Z'', 123, 6),
    (1359, ''2026-01-01T00:00:00.0000000Z'', 73, 7),
    (1360, ''2026-01-01T00:00:00.0000000Z'', 74, 7),
    (1361, ''2026-01-01T00:00:00.0000000Z'', 95, 7),
    (1362, ''2026-01-01T00:00:00.0000000Z'', 96, 7),
    (1363, ''2026-01-01T00:00:00.0000000Z'', 43, 7)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RolePermissionId', N'CreatedAt', N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
        SET IDENTITY_INSERT [RolePermissions] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SectionID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'DepartmentID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Sections]'))
        SET IDENTITY_INSERT [Sections] ON;
    EXEC(N'INSERT INTO [Sections] ([SectionID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [DepartmentID], [Description], [IsActive], [IsDeleted], [Name], [UpdatedAt])
    VALUES (1, N''PEN_RESEARCH'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم البحث الاجتماعي والفحص'', NULL),
    (2, N''PEN_SETTLE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم التسوية والربط'', NULL),
    (3, N''PEN_ENTITLE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الاستحقاقات'', NULL),
    (4, N''PEN_RECORDS'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم السجلات والأحصاء'', NULL),
    (5, N''PEN_IT'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الحاسب الآلي'', NULL),
    (6, N''PEN_DOCS'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم التوثيق'', NULL),
    (7, N''ADM_SECRET'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 2, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم السكرتارية'', NULL),
    (8, N''ADM_PRINT'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 2, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الطباعة والنسخ'', NULL),
    (9, N''ADM_FINANCE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 2, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الشئون المالية والإدارية'', NULL),
    (10, N''INV_STUDIES'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 3, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الدراسات والجدوى الاقتصادي'', NULL),
    (11, N''INV_DESIGN'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 3, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم التصاميم'', NULL),
    (12, N''INV_TRADE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 3, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم العلاقات التجارية'', NULL),
    (13, N''INV_EXEC'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 3, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم التنفيذ والمتابعة'', NULL),
    (14, N''FIN_BUDGET'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الميزانية والحسابات'', NULL),
    (15, N''FIN_REVENUE'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الإيرادات'', NULL),
    (16, N''FIN_EXPENSES'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم المصروفات والمدفوعات'', NULL),
    (17, N''FIN_AUDIT'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم المراجعة والرقابة المالية'', NULL),
    (18, N''FIN_PURCH'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم المشتروات والمخازن'', NULL),
    (19, N''FIN_TREASURY'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الخزينة'', NULL),
    (20, N''LEG_CONSULT'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 5, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم الاستشارات القانونية'', NULL),
    (21, N''LEG_INVEST'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 5, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قسم التحقيقات والمرافعات'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SectionID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'DepartmentID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Sections]'))
        SET IDENTITY_INSERT [Sections] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionID', N'CanCreate', N'CanDelete', N'CanRead', N'CanUpdate', N'CreatedAt', N'RoleID', N'TableID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[TablePermissions]'))
        SET IDENTITY_INSERT [TablePermissions] ON;
    EXEC(N'INSERT INTO [TablePermissions] ([PermissionID], [CanCreate], [CanDelete], [CanRead], [CanUpdate], [CreatedAt], [RoleID], [TableID], [UpdatedAt])
    VALUES (1, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 1, NULL),
    (2, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 2, NULL),
    (3, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 3, NULL),
    (4, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 4, NULL),
    (5, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 5, NULL),
    (6, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 6, NULL),
    (7, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 7, NULL),
    (8, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 8, NULL),
    (9, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 9, NULL),
    (10, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 10, NULL),
    (11, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 11, NULL),
    (12, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 12, NULL),
    (13, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 13, NULL),
    (14, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 14, NULL),
    (15, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 15, NULL),
    (16, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 16, NULL),
    (17, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 17, NULL),
    (18, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 18, NULL),
    (19, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 19, NULL),
    (20, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 20, NULL),
    (21, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 21, NULL),
    (22, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 22, NULL),
    (23, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 23, NULL),
    (24, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 24, NULL),
    (25, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 25, NULL),
    (26, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 26, NULL),
    (27, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 27, NULL),
    (28, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 28, NULL),
    (29, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 29, NULL),
    (30, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 30, NULL),
    (31, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 31, NULL),
    (32, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 32, NULL),
    (33, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 33, NULL),
    (34, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 34, NULL),
    (35, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 35, NULL),
    (36, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 36, NULL),
    (37, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 37, NULL),
    (38, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 38, NULL),
    (39, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 39, NULL),
    (40, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 40, NULL),
    (41, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 41, NULL),
    (42, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 42, NULL);
    INSERT INTO [TablePermissions] ([PermissionID], [CanCreate], [CanDelete], [CanRead], [CanUpdate], [CreatedAt], [RoleID], [TableID], [UpdatedAt])
    VALUES (43, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 1, 43, NULL),
    (44, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 1, NULL),
    (45, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 2, NULL),
    (46, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 3, NULL),
    (47, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 4, NULL),
    (48, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 5, NULL),
    (49, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 6, NULL),
    (50, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 7, NULL),
    (51, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 8, NULL),
    (52, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 9, NULL),
    (53, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 10, NULL),
    (54, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 11, NULL),
    (55, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 12, NULL),
    (56, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 13, NULL),
    (57, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 14, NULL),
    (58, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 15, NULL),
    (59, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 16, NULL),
    (60, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 17, NULL),
    (61, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 18, NULL),
    (62, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 19, NULL),
    (63, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 20, NULL),
    (64, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 21, NULL),
    (65, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 22, NULL),
    (66, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 23, NULL),
    (67, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 24, NULL),
    (68, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 25, NULL),
    (69, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 26, NULL),
    (70, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 29, NULL),
    (71, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 30, NULL),
    (72, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 31, NULL),
    (73, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 32, NULL),
    (74, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 33, NULL),
    (75, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 34, NULL),
    (76, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 35, NULL),
    (77, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 36, NULL),
    (78, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 37, NULL),
    (79, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 38, NULL),
    (80, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 39, NULL),
    (81, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 40, NULL),
    (82, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 41, NULL),
    (83, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 42, NULL),
    (84, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 2, 43, NULL);
    INSERT INTO [TablePermissions] ([PermissionID], [CanCreate], [CanDelete], [CanRead], [CanUpdate], [CreatedAt], [RoleID], [TableID], [UpdatedAt])
    VALUES (85, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 1, NULL),
    (86, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 2, NULL),
    (87, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 3, NULL),
    (88, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 4, NULL),
    (89, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 5, NULL),
    (90, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 6, NULL),
    (91, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 7, NULL),
    (92, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 8, NULL),
    (93, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 9, NULL),
    (94, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 10, NULL),
    (95, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 11, NULL),
    (96, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 12, NULL),
    (97, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 13, NULL),
    (98, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 14, NULL),
    (99, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 15, NULL),
    (100, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 16, NULL),
    (101, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 17, NULL),
    (102, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 18, NULL),
    (103, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 19, NULL),
    (104, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 20, NULL),
    (105, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 29, NULL),
    (106, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 30, NULL),
    (107, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 31, NULL),
    (108, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 32, NULL),
    (109, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 33, NULL),
    (110, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 34, NULL),
    (111, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 35, NULL),
    (112, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 36, NULL),
    (113, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 37, NULL),
    (114, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 38, NULL),
    (115, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 39, NULL),
    (116, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 40, NULL),
    (117, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 41, NULL),
    (118, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 42, NULL),
    (119, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 3, 43, NULL),
    (120, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 1, NULL),
    (121, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 2, NULL),
    (122, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 3, NULL),
    (123, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 4, NULL),
    (124, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 5, NULL),
    (125, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 6, NULL),
    (126, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 7, NULL);
    INSERT INTO [TablePermissions] ([PermissionID], [CanCreate], [CanDelete], [CanRead], [CanUpdate], [CreatedAt], [RoleID], [TableID], [UpdatedAt])
    VALUES (127, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 8, NULL),
    (128, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 9, NULL),
    (129, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 10, NULL),
    (130, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 11, NULL),
    (131, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 12, NULL),
    (132, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 13, NULL),
    (133, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 14, NULL),
    (134, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 15, NULL),
    (135, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 16, NULL),
    (136, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 17, NULL),
    (137, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 18, NULL),
    (138, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 19, NULL),
    (139, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 20, NULL),
    (140, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 29, NULL),
    (141, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 30, NULL),
    (142, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 31, NULL),
    (143, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 32, NULL),
    (144, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 33, NULL),
    (145, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 34, NULL),
    (146, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 35, NULL),
    (147, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 36, NULL),
    (148, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 37, NULL),
    (149, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 38, NULL),
    (150, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 39, NULL),
    (151, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 40, NULL),
    (152, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 41, NULL),
    (153, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 42, NULL),
    (154, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 4, 43, NULL),
    (155, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 1, NULL),
    (156, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 2, NULL),
    (157, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 3, NULL),
    (158, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 4, NULL),
    (159, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 5, NULL),
    (160, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 6, NULL),
    (161, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 7, NULL),
    (162, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 8, NULL),
    (163, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), ''2026-01-01T00:00:00.0000000Z'', 6, 9, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionID', N'CanCreate', N'CanDelete', N'CanRead', N'CanUpdate', N'CreatedAt', N'RoleID', N'TableID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[TablePermissions]'))
        SET IDENTITY_INSERT [TablePermissions] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ItemID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'SectionID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[BudgetItems]'))
        SET IDENTITY_INSERT [BudgetItems] ON;
    EXEC(N'INSERT INTO [BudgetItems] ([ItemID], [Code], [CreatedAt], [DeletedAt], [DeletedByUserID], [Description], [IsActive], [IsDeleted], [Name], [SectionID], [UpdatedAt])
    VALUES (1, N''3111'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''معاشات وأجور دائمة'', 1, NULL),
    (2, N''3112'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''معاشات وأجور تعاقدية'', 1, NULL),
    (3, N''3121'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل تمثيل + مظهر'', 2, NULL),
    (4, N''3122'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل مناطق حرة (كادر) + الأمن'', 2, NULL),
    (5, N''31233'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل حضور جلسات'', 2, NULL),
    (6, N''3124'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل تخرج'', 2, NULL),
    (7, N''3125'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل مسؤولية'', 2, NULL),
    (8, N''3126'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل تعويض السكن'', 2, NULL),
    (9, N''3127'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل تعويض العمل الإضافي'', 2, NULL),
    (10, N''3128'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''بدل انتقال'', 2, NULL),
    (11, N''3131'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''تكاليف أغذية العاملين'', 3, NULL),
    (12, N''3132'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''تكاليف ملابس العاملين'', 3, NULL),
    (13, N''3133'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''خدمات ثقافية واجتماعية للعاملين'', 3, NULL),
    (14, N''3134'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''تكاليف علاج العاملين'', 3, NULL),
    (15, N''3141'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''المكافآت التشجيعية'', 4, NULL),
    (16, N''3142'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''إكرامية رمضان'', 4, NULL),
    (17, N''3161'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''حصة الحكومة (قسط التقاعد + مساهمة الموظف)'', 5, NULL),
    (18, N''3162'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''حصة صندوق التأمين إصابة العمل'', 5, NULL),
    (19, N''3221'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مواد بترولية'', 6, NULL),
    (20, N''3222'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''كهرباء'', 6, NULL),
    (21, N''3223'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مياه'', 6, NULL),
    (22, N''3224'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''زيوت وشحوم'', 6, NULL),
    (23, N''3225'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''غاز'', 6, NULL),
    (24, N''3231'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قطع غيار الآلات والسيارات'', 7, NULL),
    (25, N''3232'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''لوازم ومهمات الصيانة'', 7, NULL),
    (26, N''3233'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''قطع غيار التجهيزات الإدارية'', 7, NULL),
    (27, N''3251'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''القرطاسية واللوازم المكتبية'', 8, NULL),
    (28, N''3252'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مستلزمات سلعية متنوعة'', 8, NULL),
    (29, N''3253'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''مطبوعات دورية'', 8, NULL),
    (30, N''3254'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), N''شتول وأغراس'', 8, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ItemID', N'Code', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'Description', N'IsActive', N'IsDeleted', N'Name', N'SectionID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[BudgetItems]'))
        SET IDENTITY_INSERT [BudgetItems] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmployeeID', N'AcademicQualifications', N'Address', N'BirthDate', N'BirthPlace', N'BloodGroup', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'DepartmentID', N'EmergencyContactName', N'EmergencyContactPhone', N'EmployeeNumber', N'EmploymentStatusID', N'FullName', N'Gender', N'HireDate', N'IsDeleted', N'JobPositions', N'JobTitleID', N'MaritalStatus', N'MilitaryNumber', N'MilitaryServiceDate', N'NationalID', N'NationalIDExpiryDate', N'NationalIDIssueDate', N'NationalIDIssuePlace', N'Notes', N'Phone', N'ProfilePhotoPath', N'RankID', N'SectionID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] ON;
    EXEC(N'INSERT INTO [Employees] ([EmployeeID], [AcademicQualifications], [Address], [BirthDate], [BirthPlace], [BloodGroup], [CreatedAt], [DeletedAt], [DeletedByUserID], [DepartmentID], [EmergencyContactName], [EmergencyContactPhone], [EmployeeNumber], [EmploymentStatusID], [FullName], [Gender], [HireDate], [IsDeleted], [JobPositions], [JobTitleID], [MaritalStatus], [MilitaryNumber], [MilitaryServiceDate], [NationalID], [NationalIDExpiryDate], [NationalIDIssueDate], [NationalIDIssuePlace], [Notes], [Phone], [ProfilePhotoPath], [RankID], [SectionID], [UpdatedAt])
    VALUES (7, N''دبلوم + 7 دورات كمبيوتر'', N''مديرية الشيخ عثمان/الممدارة'', ''1973-03-06T00:00:00.0000000Z'', N''عدن/صيرة'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, NULL, N''EMP-007'', 1, N''خالد صالح بن صالح'', CAST(1 AS tinyint), ''1991-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''نائب المدير المالي رئيس الحاسب الآلي والحسابات'', 5, N''متزوج'', N''27940'', ''1991-01-01T00:00:00.0000000Z'', N''291268142893'', NULL, NULL, NULL, NULL, N''778306198'', NULL, 3, 14, NULL),
    (8, N''ثانوية عامة'', N''عدن/دار سعد/الميدان'', ''1987-08-13T00:00:00.0000000Z'', N''عدن/دار سعد'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, NULL, N''EMP-008'', 1, N''آمنة علي عياش سالم'', CAST(0 AS tinyint), ''2008-01-05T00:00:00.0000000Z'', CAST(0 AS bit), N''رئيس قسم الأرشيف'', 5, N''متزوجة'', N''185650'', ''2008-01-05T00:00:00.0000000Z'', N''108121943248'', NULL, NULL, NULL, NULL, N''783421792'', NULL, 17, 6, NULL),
    (9, N''بكالوريوس قسم حاسوب'', N''خورمكسر/العريش'', ''1994-05-18T00:00:00.0000000Z'', N''عدن/صيرة'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, NULL, N''EMP-009'', 1, N''رنا علي هربي يوسف'', CAST(0 AS tinyint), ''2015-12-01T00:00:00.0000000Z'', CAST(0 AS bit), N''مختص البحث الاجتماعي'', 6, N''عازبة'', N''305561'', ''2015-12-01T00:00:00.0000000Z'', N''495335740730'', NULL, NULL, NULL, NULL, N''772373136'', NULL, 12, 1, NULL),
    (10, NULL, NULL, NULL, NULL, NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, NULL, N''EMP-010'', 1, N''أحمد الخضر أحمد'', CAST(1 AS tinyint), ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''مختص الأرشيف'', 6, NULL, NULL, NULL, N'''', NULL, NULL, NULL, NULL, N''776859111'', NULL, 11, 6, NULL),
    (11, N''ثانوية'', N''الثقنية'', ''1988-01-02T00:00:00.0000000Z'', N''الحديدة'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, NULL, N''EMP-011'', 1, N''ماهر هبه علي هبه'', CAST(1 AS tinyint), ''2025-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''رئيس قسم المخازن'', 5, N''عازب'', N''374013'', ''2025-01-01T00:00:00.0000000Z'', N'''', NULL, NULL, NULL, NULL, N''784881417'', NULL, 11, 18, NULL),
    (12, N''بكالوريوس هندسة مدنية'', N''عدن/الشيخ عثمان/الممدارة'', ''2003-07-24T00:00:00.0000000Z'', N''عدن'', NULL, ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, 4, NULL, NULL, N''EMP-012'', 1, N''عمرو خالد صالح'', CAST(1 AS tinyint), ''2026-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''رئيس قسم المراجعة'', 5, N''عازب'', NULL, NULL, N'''', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 17, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmployeeID', N'AcademicQualifications', N'Address', N'BirthDate', N'BirthPlace', N'BloodGroup', N'CreatedAt', N'DeletedAt', N'DeletedByUserID', N'DepartmentID', N'EmergencyContactName', N'EmergencyContactPhone', N'EmployeeNumber', N'EmploymentStatusID', N'FullName', N'Gender', N'HireDate', N'IsDeleted', N'JobPositions', N'JobTitleID', N'MaritalStatus', N'MilitaryNumber', N'MilitaryServiceDate', N'NationalID', N'NationalIDExpiryDate', N'NationalIDIssueDate', N'NationalIDIssuePlace', N'Notes', N'Phone', N'ProfilePhotoPath', N'RankID', N'SectionID', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Employees]'))
        SET IDENTITY_INSERT [Employees] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AdministrativeOrders_AdministrativeOrderTypeID] ON [AdministrativeOrders] ([AdministrativeOrderTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AdministrativeOrders_ApprovedByUserID] ON [AdministrativeOrders] ([ApprovedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AdministrativeOrders_DeletedByUserID] ON [AdministrativeOrders] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AdministrativeOrders_EmployeeID] ON [AdministrativeOrders] ([EmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AdministrativeOrders_OrderNumber] ON [AdministrativeOrders] ([OrderNumber]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AdministrativeOrders_Status] ON [AdministrativeOrders] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AdministrativeOrderTypes_Code] ON [AdministrativeOrderTypes] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AdministrativeOrderTypes_DeletedByUserID] ON [AdministrativeOrderTypes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Agents_CreatedByUserID] ON [Agents] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Agents_DeletedByUserID] ON [Agents] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Agents_FileID] ON [Agents] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Agents_UpdatedByUserID] ON [Agents] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ApprovalRequests_DeletedByUserID] ON [ApprovalRequests] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ApprovalRequests_RequestedByUserID] ON [ApprovalRequests] ([RequestedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AssetValuations_DeletedByUserID] ON [AssetValuations] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AssetValuations_PropertyID] ON [AssetValuations] ([PropertyID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AttendanceEvents_EmployeeID_EventDate_EventType] ON [AttendanceEvents] ([EmployeeID], [EventDate], [EventType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AttendanceEvents_RecordedByUserID] ON [AttendanceEvents] ([RecordedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_TableID] ON [AuditLogs] ([TableID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserID] ON [AuditLogs] ([UserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_BudgetAllocations_DeletedByUserID] ON [BudgetAllocations] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_BudgetAllocations_ItemID] ON [BudgetAllocations] ([ItemID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BudgetAllocations_QuarterID_ItemID] ON [BudgetAllocations] ([QuarterID], [ItemID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BudgetChapters_Code] ON [BudgetChapters] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_BudgetChapters_DeletedByUserID] ON [BudgetChapters] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BudgetItems_Code] ON [BudgetItems] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_BudgetItems_DeletedByUserID] ON [BudgetItems] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_BudgetItems_SectionID] ON [BudgetItems] ([SectionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_BudgetSections_ChapterID] ON [BudgetSections] ([ChapterID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BudgetSections_Code] ON [BudgetSections] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_BudgetSections_DeletedByUserID] ON [BudgetSections] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_CitizenAccounts_DeletedByUserID] ON [CitizenAccounts] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CitizenAccounts_NationalID] ON [CitizenAccounts] ([NationalID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CitizenAccounts_RetireeID] ON [CitizenAccounts] ([RetireeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_CitizenNotifications_RetireeID] ON [CitizenNotifications] ([RetireeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Correspondences_CorrespondenceTypeID] ON [Correspondences] ([CorrespondenceTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Correspondences_CreatedByUserID] ON [Correspondences] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Correspondences_DeletedByUserID] ON [Correspondences] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Correspondences_Direction_CorrespondenceTypeID_CorrespondenceDate] ON [Correspondences] ([Direction], [CorrespondenceTypeID], [CorrespondenceDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Correspondences_EmployeeID] ON [Correspondences] ([EmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Correspondences_ExternalPartyID] ON [Correspondences] ([ExternalPartyID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_CorrespondenceTypes_DeletedByUserID] ON [CorrespondenceTypes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Departments_Code] ON [Departments] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Departments_DeletedByUserID] ON [Departments] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Departments_Name] ON [Departments] ([Name]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DepreciationRecords_DeletedByUserID] ON [DepreciationRecords] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DepreciationRecords_FiscalYearID] ON [DepreciationRecords] ([FiscalYearID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DepreciationRecords_PropertyID_FiscalYearID] ON [DepreciationRecords] ([PropertyID], [FiscalYearID]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DeviceRegistrations_UserID] ON [DeviceRegistrations] ([UserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentExemptions_CreatedByUserID] ON [DocumentExemptions] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentExemptions_DeletedByUserID] ON [DocumentExemptions] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentExemptions_FileID] ON [DocumentExemptions] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentPledges_CreatedByUserID] ON [DocumentPledges] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentPledges_DeletedByUserID] ON [DocumentPledges] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentPledges_ExtendedByUserID] ON [DocumentPledges] ([ExtendedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentPledges_FileID] ON [DocumentPledges] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentPledges_SuspensionID] ON [DocumentPledges] ([SuspensionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentRequirementRules_DocumentTypeID] ON [DocumentRequirementRules] ([DocumentTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_AgentID] ON [Documents] ([AgentID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_AssetValuationID] ON [Documents] ([AssetValuationID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_CitizenAccountID] ON [Documents] ([CitizenAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_CorrespondenceID] ON [Documents] ([CorrespondenceID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_DecisionID] ON [Documents] ([DecisionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_DeletedByUserID] ON [Documents] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_DepreciationRecordID] ON [Documents] ([DepreciationRecordID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_DocumentTypeID] ON [Documents] ([DocumentTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_EmployeeID] ON [Documents] ([EmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_EntitledID] ON [Documents] ([EntitledID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_ExemptionID] ON [Documents] ([ExemptionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_FileID] ON [Documents] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_GuardianID] ON [Documents] ([GuardianID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_InvestmentPlanID] ON [Documents] ([InvestmentPlanID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_InvestmentPortfolioID] ON [Documents] ([InvestmentPortfolioID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_InvestmentProjectID] ON [Documents] ([InvestmentProjectID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_InvestmentPropertyID] ON [Documents] ([InvestmentPropertyID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_LeaseContractID] ON [Documents] ([LeaseContractID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_MaintenanceRequestID] ON [Documents] ([MaintenanceRequestID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_MinistryBatchID] ON [Documents] ([MinistryBatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_NoteID] ON [Documents] ([NoteID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_OperationalExpenseID] ON [Documents] ([OperationalExpenseID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_PaymentScheduleID] ON [Documents] ([PaymentScheduleID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_PledgeID] ON [Documents] ([PledgeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_PropertyUnitID] ON [Documents] ([PropertyUnitID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_ReportID] ON [Documents] ([ReportID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_RetireeID] ON [Documents] ([RetireeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_RiskAssessmentID] ON [Documents] ([RiskAssessmentID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_SalaryBatchID] ON [Documents] ([SalaryBatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_ServiceRequestID] ON [Documents] ([ServiceRequestID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_SettlementBatchID] ON [Documents] ([SettlementBatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_SuspensionID] ON [Documents] ([SuspensionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_TenantID] ON [Documents] ([TenantID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_TicketMessageID] ON [Documents] ([TicketMessageID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_UploadedByUserID] ON [Documents] ([UploadedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_VerifiedByUserID] ON [Documents] ([VerifiedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Documents_WorkOrderID] ON [Documents] ([WorkOrderID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_DocumentTypes_OwnerTableID] ON [DocumentTypes] ([OwnerTableID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Employees_DeletedByUserID] ON [Employees] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Employees_DepartmentID_SectionID] ON [Employees] ([DepartmentID], [SectionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Employees_EmployeeNumber] ON [Employees] ([EmployeeNumber]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Employees_EmploymentStatusID] ON [Employees] ([EmploymentStatusID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Employees_JobTitleID] ON [Employees] ([JobTitleID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Employees_NationalID] ON [Employees] ([NationalID]) WHERE [NationalID] <> '''' AND [NationalID] IS NOT NULL AND [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Employees_RankID] ON [Employees] ([RankID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Employees_SectionID] ON [Employees] ([SectionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EmploymentStatuses_Code] ON [EmploymentStatuses] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_EmploymentStatuses_DeletedByUserID] ON [EmploymentStatuses] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EmploymentStatuses_Name] ON [EmploymentStatuses] ([Name]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_EntitledPersons_CreatedByUserID] ON [EntitledPersons] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_EntitledPersons_DeletedByUserID] ON [EntitledPersons] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EntitledPersons_FileID_NationalID] ON [EntitledPersons] ([FileID], [NationalID]) WHERE [IsDeleted] = 0 AND [NationalID] <> '''' AND [NationalID] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_EntitledPersons_HealthStatusID] ON [EntitledPersons] ([HealthStatusID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_EntitledPersons_RelationshipID] ON [EntitledPersons] ([RelationshipID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_EntitledPersons_SocialStatusID] ON [EntitledPersons] ([SocialStatusID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_EntitledPersons_UpdatedByUserID] ON [EntitledPersons] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ExternalParties_DeletedByUserID] ON [ExternalParties] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FileNotes_CreatedByUserID] ON [FileNotes] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FileNotes_DeletedByUserID] ON [FileNotes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FileNotes_FileID] ON [FileNotes] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FileNotes_ResolvedByUserID] ON [FileNotes] ([ResolvedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FileReviewItems_AuditLogID] ON [FileReviewItems] ([AuditLogID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FileReviewItems_RetirementFileID] ON [FileReviewItems] ([RetirementFileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FileReviewItems_ReviewedByUserID] ON [FileReviewItems] ([ReviewedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FileSnapshots_FileId_Version] ON [FileSnapshots] ([FileId], [Version]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FinancialEntitlements_BatchID] ON [FinancialEntitlements] ([BatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FinancialEntitlements_DeletedByUserID] ON [FinancialEntitlements] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FinancialEntitlements_RetireeID_EffectiveDate_EndDate_IsDeleted] ON [FinancialEntitlements] ([RetireeID], [EffectiveDate], [EndDate], [IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_FiscalYears_DeletedByUserID] ON [FiscalYears] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_GuardianEntitledPersons_DeletedByUserID] ON [GuardianEntitledPersons] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_GuardianEntitledPersons_EntitledID] ON [GuardianEntitledPersons] ([EntitledID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Guardians_CreatedByUserID] ON [Guardians] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Guardians_DeletedByUserID] ON [Guardians] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Guardians_EntitledPersonEntitledID] ON [Guardians] ([EntitledPersonEntitledID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Guardians_FileID] ON [Guardians] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Guardians_UpdatedByUserID] ON [Guardians] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryCategories_CategoryName] ON [InventoryCategories] ([CategoryName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCategories_CreatedByUserID] ON [InventoryCategories] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCategories_UpdatedByUserID] ON [InventoryCategories] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCustodies_AssignedByUserID] ON [InventoryCustodies] ([AssignedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCustodies_CreatedByUserID] ON [InventoryCustodies] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCustodies_DeletedByUserID] ON [InventoryCustodies] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCustodies_EmployeeID] ON [InventoryCustodies] ([EmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCustodies_InventoryItemID] ON [InventoryCustodies] ([InventoryItemID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCustodies_ReturnedToUserID] ON [InventoryCustodies] ([ReturnedToUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryCustodies_UpdatedByUserID] ON [InventoryCustodies] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_CategoryID] ON [InventoryItems] ([CategoryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_CreatedByUserID] ON [InventoryItems] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_DeletedByUserID] ON [InventoryItems] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InventoryItems_ItemCode] ON [InventoryItems] ([ItemCode]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_UpdatedByUserID] ON [InventoryItems] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_BatchID] ON [InventoryTransactions] ([BatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_InventoryItemID] ON [InventoryTransactions] ([InventoryItemID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_PerformedByUserID] ON [InventoryTransactions] ([PerformedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentPlans_DeletedByUserID] ON [InvestmentPlans] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentPlans_FiscalYearID] ON [InvestmentPlans] ([FiscalYearID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InvestmentPlans_ProjectID_FiscalYearID] ON [InvestmentPlans] ([ProjectID], [FiscalYearID]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentPortfolios_DeletedByUserID] ON [InvestmentPortfolios] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentPortfolios_ManagerUserID] ON [InvestmentPortfolios] ([ManagerUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InvestmentPortfolios_Name] ON [InvestmentPortfolios] ([Name]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentProjects_DeletedByUserID] ON [InvestmentProjects] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentProjects_PensionBranchID] ON [InvestmentProjects] ([PensionBranchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InvestmentProjects_PortfolioID_Name] ON [InvestmentProjects] ([PortfolioID], [Name]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentProjects_TypeID] ON [InvestmentProjects] ([TypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentProperties_DeletedByUserID] ON [InvestmentProperties] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentProperties_PensionBranchID] ON [InvestmentProperties] ([PensionBranchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentProperties_ProjectID] ON [InvestmentProperties] ([ProjectID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentProperties_PropertyTypeID] ON [InvestmentProperties] ([PropertyTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_InvestmentTypes_DeletedByUserID] ON [InvestmentTypes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InvestmentTypes_TypeName_Code] ON [InvestmentTypes] ([TypeName], [Code]) WHERE [Code] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_JobTitles_Code] ON [JobTitles] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_JobTitles_DeletedByUserID] ON [JobTitles] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_JobTitles_Name] ON [JobTitles] ([Name]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LeaseContracts_DeletedByUserID] ON [LeaseContracts] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LeaseContracts_TenantID] ON [LeaseContracts] ([TenantID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LeaseContracts_UnitID] ON [LeaseContracts] ([UnitID]) WHERE [Status] = ''Active'' AND [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_ApprovedByUserID] ON [LeaveRequests] ([ApprovedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_DeletedByUserID] ON [LeaveRequests] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_EmployeeID_StartDate_EndDate] ON [LeaveRequests] ([EmployeeID], [StartDate], [EndDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_LeaveTypeID] ON [LeaveRequests] ([LeaveTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LeaveTypes_Code] ON [LeaveTypes] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LeaveTypes_DeletedByUserID] ON [LeaveTypes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LeaveTypes_Name] ON [LeaveTypes] ([Name]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_LoginHistories_UserID] ON [LoginHistories] ([UserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRequests_DeletedByUserID] ON [MaintenanceRequests] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRequests_PropertyID] ON [MaintenanceRequests] ([PropertyID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRequests_TenantID] ON [MaintenanceRequests] ([TenantID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRequests_UnitID] ON [MaintenanceRequests] ([UnitID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MinistryApprovalBatches_DeletedByUserID] ON [MinistryApprovalBatches] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MinistryApprovalBatches_ImportedByUserID] ON [MinistryApprovalBatches] ([ImportedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MinistryApprovalItems_BatchID] ON [MinistryApprovalItems] ([BatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MinistryApprovalItems_CategoryID] ON [MinistryApprovalItems] ([CategoryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MinistryApprovalItems_DeletedByUserID] ON [MinistryApprovalItems] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MinistryApprovalItems_ProcessedByUserID] ON [MinistryApprovalItems] ([ProcessedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MinistryApprovalItems_RankID] ON [MinistryApprovalItems] ([RankID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_AgentID] ON [MissingDocuments] ([AgentID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_DeletedByUserID] ON [MissingDocuments] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_DocumentTypeID] ON [MissingDocuments] ([DocumentTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_EntitledID] ON [MissingDocuments] ([EntitledID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_ExemptionID] ON [MissingDocuments] ([ExemptionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_FileID] ON [MissingDocuments] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_GuardianID] ON [MissingDocuments] ([GuardianID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_MissingDocuments_PledgeID] ON [MissingDocuments] ([PledgeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_NotificationAttachments_DeletedByUserID] ON [NotificationAttachments] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_NotificationAttachments_NotificationID] ON [NotificationAttachments] ([NotificationID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_NotificationOutbox_NotificationId] ON [NotificationOutbox] ([NotificationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_NotificationOutbox_Status_RetryCount] ON [NotificationOutbox] ([Status], [RetryCount]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Notifications_ActorID] ON [Notifications] ([ActorID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Notifications_RelatedTableID] ON [Notifications] ([RelatedTableID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserID] ON [Notifications] ([UserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_OperationalExpenses_BudgetAllocationID] ON [OperationalExpenses] ([BudgetAllocationID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_OperationalExpenses_CreatedByUserID] ON [OperationalExpenses] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_OperationalExpenses_DeletedByUserID] ON [OperationalExpenses] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_OtpVerifications_CitizenAccountID] ON [OtpVerifications] ([CitizenAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PaymentBooks_DeletedByUserID] ON [PaymentBooks] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PaymentBooks_FileID] ON [PaymentBooks] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PaymentSchedules_ContractID] ON [PaymentSchedules] ([ContractID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PaymentSchedules_DeletedByUserID] ON [PaymentSchedules] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PendingTasks_AssignedToRoleId] ON [PendingTasks] ([AssignedToRoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PendingTasks_AssignedToUserId] ON [PendingTasks] ([AssignedToUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PendingTasks_DeletedByUserID] ON [PendingTasks] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PendingTasks_FileId] ON [PendingTasks] ([FileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PendingTasks_SalaryBatchID] ON [PendingTasks] ([SalaryBatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PensionFundBranches_ParentBranchId] ON [PensionFundBranches] ([ParentBranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PensionSuspensions_CreatedByUserID] ON [PensionSuspensions] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PensionSuspensions_DeletedByUserID] ON [PensionSuspensions] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PensionSuspensions_EndedByUserID] ON [PensionSuspensions] ([EndedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PensionSuspensions_FileID] ON [PensionSuspensions] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PropertyTypes_DeletedByUserID] ON [PropertyTypes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PropertyTypes_TypeName] ON [PropertyTypes] ([TypeName]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PropertyUnits_DeletedByUserID] ON [PropertyUnits] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PropertyUnits_PropertyID_UnitNumber] ON [PropertyUnits] ([PropertyID], [UnitNumber]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_PropertyUnits_UnitTypeID] ON [PropertyUnits] ([UnitTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Quarters_DeletedByUserID] ON [Quarters] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Quarters_FiscalYearID] ON [Quarters] ([FiscalYearID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ReportTemplates_CreatedByUserID] ON [ReportTemplates] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Retirees_ApprovalItemID] ON [Retirees] ([ApprovalItemID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Retirees_CreatedByUserID] ON [Retirees] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Retirees_DeletedByUserID] ON [Retirees] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Retirees_HealthStatusID] ON [Retirees] ([HealthStatusID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Retirees_NationalID] ON [Retirees] ([NationalID]) WHERE [NationalID] <> '''' AND [NationalID] IS NOT NULL AND [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Retirees_SocialStatusID] ON [Retirees] ([SocialStatusID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Retirees_UpdatedByUserID] ON [Retirees] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_AssignedEmployeeID] ON [RetirementFiles] ([AssignedEmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_DeletedByUserID] ON [RetirementFiles] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RetirementFiles_FileNumber] ON [RetirementFiles] ([FileNumber]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_PensionBranchID] ON [RetirementFiles] ([PensionBranchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_RankID] ON [RetirementFiles] ([RankID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_RetireeID] ON [RetirementFiles] ([RetireeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_StatusID] ON [RetirementFiles] ([StatusID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RiskAssessments_DeletedByUserID] ON [RiskAssessments] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RiskAssessments_ProjectID] ON [RiskAssessments] ([ProjectID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RiskEvents_FileId] ON [RiskEvents] ([FileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RiskEvents_ResolvedByUserId] ON [RiskEvents] ([ResolvedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [RolePermissions] ([RoleId], [PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentBatches_ApprovedByUserID] ON [SalaryPaymentBatches] ([ApprovedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentBatches_BankID] ON [SalaryPaymentBatches] ([BankID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentBatches_DeletedByUserID] ON [SalaryPaymentBatches] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentBatches_ImportedByUserID] ON [SalaryPaymentBatches] ([ImportedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentItems_BatchID] ON [SalaryPaymentItems] ([BatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentItems_CreatedByUserID] ON [SalaryPaymentItems] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentItems_DeletedByUserID] ON [SalaryPaymentItems] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SalaryPaymentItems_FileID_BatchID] ON [SalaryPaymentItems] ([FileID], [BatchID]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentItems_PaymentBookID] ON [SalaryPaymentItems] ([PaymentBookID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentItems_RecipientID] ON [SalaryPaymentItems] ([RecipientID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentItems_RetireeID] ON [SalaryPaymentItems] ([RetireeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryPaymentItems_StatusID] ON [SalaryPaymentItems] ([StatusID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_AgentID] ON [SalaryRecipients] ([AgentID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_DeletedByUserID] ON [SalaryRecipients] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_EntitledID] ON [SalaryRecipients] ([EntitledID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_FileID_IsActive] ON [SalaryRecipients] ([FileID], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_GuardianID] ON [SalaryRecipients] ([GuardianID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_RetireeID] ON [SalaryRecipients] ([RetireeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SavedReports_GeneratedByUserID] ON [SavedReports] ([GeneratedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SavedReports_ReportTemplateTemplateID] ON [SavedReports] ([ReportTemplateTemplateID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SavedReports_TemplateID] ON [SavedReports] ([TemplateID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Sections_Code] ON [Sections] ([Code]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Sections_DeletedByUserID] ON [Sections] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Sections_DepartmentID_Name] ON [Sections] ([DepartmentID], [Name]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ServiceHistoryEvents_EmployeeID_EventDate] ON [ServiceHistoryEvents] ([EmployeeID], [EventDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ServiceRequests_CitizenAccountID] ON [ServiceRequests] ([CitizenAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ServiceRequests_DeletedByUserID] ON [ServiceRequests] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ServiceRequests_FileID] ON [ServiceRequests] ([FileID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_ServiceRequests_ProcessedByUserID] ON [ServiceRequests] ([ProcessedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementDecisions_CreatedByUserID] ON [SettlementDecisions] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementDecisions_DeletedByUserID] ON [SettlementDecisions] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SettlementItems_ApprovalItemID_BatchID] ON [SettlementItems] ([ApprovalItemID], [BatchID]) WHERE [ApprovalItemID] IS NOT NULL AND [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_BatchID] ON [SettlementItems] ([BatchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_CurrentRankID] ON [SettlementItems] ([CurrentRankID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_DecisionID] ON [SettlementItems] ([DecisionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_DeletedByUserID] ON [SettlementItems] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_ProcessedRankID] ON [SettlementItems] ([ProcessedRankID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_RetireeID] ON [SettlementItems] ([RetireeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementsBatches_CreatedByUserID] ON [SettlementsBatches] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SettlementsBatches_DeletedByUserID] ON [SettlementsBatches] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_StatusTransitionLogs_ChangedByUserID] ON [StatusTransitionLogs] ([ChangedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_StatusTransitionLogs_TableID] ON [StatusTransitionLogs] ([TableID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_AssignedToUserID] ON [SupportTickets] ([AssignedToUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_CitizenAccountID] ON [SupportTickets] ([CitizenAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_DeletedByUserID] ON [SupportTickets] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SupportTickets_TicketNumber] ON [SupportTickets] ([TicketNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SyncQueues_DeviceID] ON [SyncQueues] ([DeviceID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SyncQueues_TableID] ON [SyncQueues] ([TableID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SyncQueues_UserID] ON [SyncQueues] ([UserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_SystemSettings_UpdatedByUserID] ON [SystemSettings] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TablePermissions_RoleID] ON [TablePermissions] ([RoleID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TablePermissions_TableID] ON [TablePermissions] ([TableID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Tenants_DeletedByUserID] ON [Tenants] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Tenants_NationalIDOrCR] ON [Tenants] ([NationalIDOrCR]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TicketMessages_DeletedByUserID] ON [TicketMessages] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TicketMessages_TicketID] ON [TicketMessages] ([TicketID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Treasuries_DeletedByUserID] ON [Treasuries] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TreasuryIncomes_DeletedByUserID] ON [TreasuryIncomes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TreasuryIncomes_QuarterID] ON [TreasuryIncomes] ([QuarterID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TreasuryIncomes_TreasuryID] ON [TreasuryIncomes] ([TreasuryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TreasuryTransactions_DeletedByUserID] ON [TreasuryTransactions] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_TreasuryTransactions_TreasuryID] ON [TreasuryTransactions] ([TreasuryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_UnitTypes_DeletedByUserID] ON [UnitTypes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UnitTypes_TypeName] ON [UnitTypes] ([TypeName]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_UserPermissions_GrantedByUserId] ON [UserPermissions] ([GrantedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_UserPermissions_PermissionId] ON [UserPermissions] ([PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserPermissions_UserId_PermissionId] ON [UserPermissions] ([UserId], [PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_UserRoleAssignments_BranchId] ON [UserRoleAssignments] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_UserRoleAssignments_RegionId] ON [UserRoleAssignments] ([RegionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_UserRoleAssignments_RoleId] ON [UserRoleAssignments] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UserRoleAssignments_UserId_RoleId_BranchId] ON [UserRoleAssignments] ([UserId], [RoleId], [BranchId]) WHERE [IsActive] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Users_BranchID] ON [Users] ([BranchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Users_CreatedByUserID] ON [Users] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Users_DeletedByUserID] ON [Users] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_Users_RoleID] ON [Users] ([RoleID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_DeletedByUserID] ON [WorkOrders] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_RequestID] ON [WorkOrders] ([RequestID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625214943_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260625214943_Init', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OperationalExpenses]') AND [c].[name] = N'BudgetAllocationID');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [OperationalExpenses] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [OperationalExpenses] ALTER COLUMN [BudgetAllocationID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    ALTER TABLE [OperationalExpenses] ADD [ClassifiedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    ALTER TABLE [OperationalExpenses] ADD [ClassifiedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    ALTER TABLE [OperationalExpenses] ADD [IsProvisional] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-29T22:17:48.0795672Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-29T22:17:48.0795715Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-29T22:17:48.0795717Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-29T22:17:48.0795719Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-29T22:17:48.0795721Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-29T22:17:48.1076912Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-29T22:17:48.1076936Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-29T22:17:48.1076938Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-29T22:17:48.1076939Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-29T22:17:48.1076940Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260629221751_AddProvisionalExpenseSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260629221751_AddProvisionalExpenseSupport', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    ALTER TABLE [BudgetAllocations] ADD [ApprovalRemarks] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    ALTER TABLE [BudgetAllocations] ADD [ApprovalStatus] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    ALTER TABLE [BudgetAllocations] ADD [ApprovedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    ALTER TABLE [BudgetAllocations] ADD [ApprovedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:23:30.7376805Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:23:30.7376816Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:23:30.7376818Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:23:30.7376820Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:23:30.7376821Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:23:30.7651842Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:23:30.7651848Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:23:30.7651850Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:23:30.7651851Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:23:30.7651852Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    CREATE INDEX [IX_BudgetAllocations_ApprovedByUserID] ON [BudgetAllocations] ([ApprovedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    ALTER TABLE [BudgetAllocations] ADD CONSTRAINT [FK_BudgetAllocations_Users_ApprovedByUserID] FOREIGN KEY ([ApprovedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630192333_AddBudgetAllocationApprovalStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260630192333_AddBudgetAllocationApprovalStatus', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    ALTER TABLE [TreasuryTransactions] ADD [ChequeImagePath] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    ALTER TABLE [TreasuryTransactions] ADD [ChequeNumber] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:57:07.9409296Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:57:07.9409324Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:57:07.9409326Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:57:07.9409328Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-06-30T19:57:07.9409329Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:57:07.9667263Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:57:07.9667283Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:57:07.9667285Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:57:07.9667286Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-06-30T19:57:07.9667287Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630195710_AddChequeFieldsToTreasuryTransactions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260630195710_AddChequeFieldsToTreasuryTransactions', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    ALTER TABLE [OperationalExpenses] ADD [FiscalYearID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    ALTER TABLE [OperationalExpenses] ADD [QuarterNumber] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-01T21:03:14.4255604Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-01T21:03:14.4255637Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-01T21:03:14.4255638Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-01T21:03:14.4255653Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-01T21:03:14.4255654Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-01T21:03:14.4525253Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-01T21:03:14.4525279Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-01T21:03:14.4525280Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-01T21:03:14.4525282Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-01T21:03:14.4525283Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701210317_AddProvisionalQuarterFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260701210317_AddProvisionalQuarterFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE TABLE [ChartOfAccounts] (
        [AccountID] int NOT NULL IDENTITY,
        [AccountCode] nvarchar(10) NOT NULL,
        [AccountName] nvarchar(200) NOT NULL,
        [AccountNature] nvarchar(10) NOT NULL,
        [AccountLevel] int NOT NULL,
        [ParentAccountID] int NULL,
        [BookNumber] int NOT NULL,
        [IsPostable] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_ChartOfAccounts] PRIMARY KEY ([AccountID]),
        CONSTRAINT [FK_ChartOfAccounts_ChartOfAccounts_ParentAccountID] FOREIGN KEY ([ParentAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChartOfAccounts_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE TABLE [CostCenters] (
        [CostCenterID] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [ParentCostCenterID] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_CostCenters] PRIMARY KEY ([CostCenterID]),
        CONSTRAINT [FK_CostCenters_CostCenters_ParentCostCenterID] FOREIGN KEY ([ParentCostCenterID]) REFERENCES [CostCenters] ([CostCenterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CostCenters_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE TABLE [FiscalPeriods] (
        [PeriodID] int NOT NULL IDENTITY,
        [FiscalYearID] int NOT NULL,
        [PeriodName] nvarchar(50) NOT NULL,
        [PeriodNumber] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [IsClosed] bit NOT NULL,
        [ClosedByUserID] int NULL,
        [ClosedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_FiscalPeriods] PRIMARY KEY ([PeriodID]),
        CONSTRAINT [FK_FiscalPeriods_FiscalYears_FiscalYearID] FOREIGN KEY ([FiscalYearID]) REFERENCES [FiscalYears] ([FiscalYearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FiscalPeriods_Users_ClosedByUserID] FOREIGN KEY ([ClosedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FiscalPeriods_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE TABLE [AccountBalances] (
        [BalanceID] int NOT NULL IDENTITY,
        [AccountID] int NOT NULL,
        [PeriodID] int NOT NULL,
        [OpeningDebit] decimal(18,2) NOT NULL,
        [OpeningCredit] decimal(18,2) NOT NULL,
        [PeriodDebit] decimal(18,2) NOT NULL,
        [PeriodCredit] decimal(18,2) NOT NULL,
        [ClosingDebit] decimal(18,2) NOT NULL,
        [ClosingCredit] decimal(18,2) NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_AccountBalances] PRIMARY KEY ([BalanceID]),
        CONSTRAINT [FK_AccountBalances_ChartOfAccounts_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccountBalances_FiscalPeriods_PeriodID] FOREIGN KEY ([PeriodID]) REFERENCES [FiscalPeriods] ([PeriodID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccountBalances_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE TABLE [JournalEntries] (
        [JournalEntryID] int NOT NULL IDENTITY,
        [EntryNumber] nvarchar(50) NOT NULL,
        [EntryDate] datetime2 NOT NULL,
        [PeriodID] int NOT NULL,
        [SourceModule] nvarchar(50) NOT NULL,
        [SourceReferenceID] int NULL,
        [Description] nvarchar(500) NOT NULL,
        [TotalDebit] decimal(18,2) NOT NULL,
        [TotalCredit] decimal(18,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [PostedByUserID] int NULL,
        [PostedAt] datetime2 NULL,
        [ReversedEntryID] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([JournalEntryID]),
        CONSTRAINT [FK_JournalEntries_FiscalPeriods_PeriodID] FOREIGN KEY ([PeriodID]) REFERENCES [FiscalPeriods] ([PeriodID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JournalEntries_JournalEntries_ReversedEntryID] FOREIGN KEY ([ReversedEntryID]) REFERENCES [JournalEntries] ([JournalEntryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JournalEntries_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_JournalEntries_Users_PostedByUserID] FOREIGN KEY ([PostedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE TABLE [JournalEntryLines] (
        [LineID] int NOT NULL IDENTITY,
        [JournalEntryID] int NOT NULL,
        [AccountID] int NOT NULL,
        [CostCenterID] int NULL,
        [Debit] decimal(18,2) NOT NULL,
        [Credit] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY ([LineID]),
        CONSTRAINT [FK_JournalEntryLines_ChartOfAccounts_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JournalEntryLines_CostCenters_CostCenterID] FOREIGN KEY ([CostCenterID]) REFERENCES [CostCenters] ([CostCenterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JournalEntryLines_JournalEntries_JournalEntryID] FOREIGN KEY ([JournalEntryID]) REFERENCES [JournalEntries] ([JournalEntryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JournalEntryLines_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T17:07:59.9077353Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T17:07:59.9077388Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T17:07:59.9077390Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T17:07:59.9077394Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T17:07:59.9077395Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T17:07:59.9355241Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T17:07:59.9355263Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T17:07:59.9355264Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T17:07:59.9355266Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T17:07:59.9355267Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AccountBalances_AccountID_PeriodID] ON [AccountBalances] ([AccountID], [PeriodID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_AccountBalances_DeletedByUserID] ON [AccountBalances] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_AccountBalances_PeriodID] ON [AccountBalances] ([PeriodID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ChartOfAccounts_AccountCode] ON [ChartOfAccounts] ([AccountCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_ChartOfAccounts_DeletedByUserID] ON [ChartOfAccounts] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_ChartOfAccounts_ParentAccountID] ON [ChartOfAccounts] ([ParentAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CostCenters_Code] ON [CostCenters] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_CostCenters_DeletedByUserID] ON [CostCenters] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_CostCenters_ParentCostCenterID] ON [CostCenters] ([ParentCostCenterID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_FiscalPeriods_ClosedByUserID] ON [FiscalPeriods] ([ClosedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_FiscalPeriods_DeletedByUserID] ON [FiscalPeriods] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FiscalPeriods_FiscalYearID_PeriodNumber] ON [FiscalPeriods] ([FiscalYearID], [PeriodNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_DeletedByUserID] ON [JournalEntries] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE UNIQUE INDEX [IX_JournalEntries_EntryNumber] ON [JournalEntries] ([EntryNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_PeriodID_Status] ON [JournalEntries] ([PeriodID], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_PostedByUserID] ON [JournalEntries] ([PostedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_ReversedEntryID] ON [JournalEntries] ([ReversedEntryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_SourceModule_SourceReferenceID] ON [JournalEntries] ([SourceModule], [SourceReferenceID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_AccountID] ON [JournalEntryLines] ([AccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_CostCenterID] ON [JournalEntryLines] ([CostCenterID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_DeletedByUserID] ON [JournalEntryLines] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_JournalEntryID] ON [JournalEntryLines] ([JournalEntryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703170803_AddAccounting'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703170803_AddAccounting', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703173740_AddAccountingPermissionsManual'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703173740_AddAccountingPermissionsManual'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703173740_AddAccountingPermissionsManual'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703173740_AddAccountingPermissionsManual'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703173740_AddAccountingPermissionsManual'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703173740_AddAccountingPermissionsManual'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703173740_AddAccountingPermissionsManual', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703183540_AddQuarterToFiscalPeriod'
)
BEGIN
    ALTER TABLE [FiscalPeriods] ADD [QuarterID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703183540_AddQuarterToFiscalPeriod'
)
BEGIN
    CREATE INDEX [IX_FiscalPeriods_QuarterID] ON [FiscalPeriods] ([QuarterID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703183540_AddQuarterToFiscalPeriod'
)
BEGIN
    ALTER TABLE [FiscalPeriods] ADD CONSTRAINT [FK_FiscalPeriods_Quarters_QuarterID] FOREIGN KEY ([QuarterID]) REFERENCES [Quarters] ([QuarterID]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703183540_AddQuarterToFiscalPeriod'
)
BEGIN

                    UPDATE fp
                    SET fp.QuarterID = q.QuarterID
                    FROM FiscalPeriods fp
                    INNER JOIN Quarters q 
                        ON q.FiscalYearID = fp.FiscalYearID
                        AND q.QuarterNumber = CASE
                            WHEN fp.PeriodNumber BETWEEN 1 AND 3 THEN 1
                            WHEN fp.PeriodNumber BETWEEN 4 AND 6 THEN 2
                            WHEN fp.PeriodNumber BETWEEN 7 AND 9 THEN 3
                            WHEN fp.PeriodNumber BETWEEN 10 AND 12 THEN 4
                        END
                    WHERE fp.QuarterID IS NULL;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703183540_AddQuarterToFiscalPeriod'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703183540_AddQuarterToFiscalPeriod', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703192314_AddAccount356OperationalAllocations'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = N'356')
                    BEGIN
                        DECLARE @ParentID INT = (SELECT AccountID FROM ChartOfAccounts WHERE AccountCode = N'35');

                        INSERT INTO ChartOfAccounts (
                            AccountCode,
                            AccountName,
                            AccountNature,
                            AccountLevel,
                            BookNumber,
                            ParentAccountID,
                            IsPostable,
                            IsActive,
                            IsDeleted,
                            Description,
                            CreatedAt
                        ) VALUES (
                            N'356',
                            N'مخصصات تشغيلية',
                            N'Debit',
                            3,
                            3,
                            @ParentID,
                            1,
                            1,
                            0,
                            N'تخصيصات للميزانية التشغيلية',
                            SYSUTCDATETIME()
                        );
                    END
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703192314_AddAccount356OperationalAllocations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703192314_AddAccount356OperationalAllocations', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T19:54:55.5496564Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T19:54:55.5496601Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T19:54:55.5496603Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T19:54:55.5496604Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T19:54:55.5496606Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T19:54:55.5858868Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T19:54:55.5858906Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T19:54:55.5858908Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T19:54:55.5858910Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T19:54:55.5858913Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703195459_UpdateCOAYemenOfficial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703195459_UpdateCOAYemenOfficial', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    DROP TABLE [TreasuryIncomes];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    DROP TABLE [TreasuryTransactions];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    DROP TABLE [Treasuries];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 117;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 118;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'DELETE FROM [RolePermissions]
    WHERE [RolePermissionId] = 1383;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'DELETE FROM [RolePermissions]
    WHERE [RolePermissionId] = 1384;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'DELETE FROM [RolePermissions]
    WHERE [RolePermissionId] = 1385;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'DELETE FROM [SystemTables]
    WHERE [TableID] = 44;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'DELETE FROM [SystemTables]
    WHERE [TableID] = 45;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Quarters]') AND [c].[name] = N'TreasuryAllocated');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Quarters] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Quarters] DROP COLUMN [TreasuryAllocated];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T21:34:00.9530321Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T21:34:00.9530357Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T21:34:00.9530359Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T21:34:00.9530361Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T21:34:00.9530362Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 119
    WHERE [RolePermissionId] = 1116;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 120
    WHERE [RolePermissionId] = 1117;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 121
    WHERE [RolePermissionId] = 1118;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 122
    WHERE [RolePermissionId] = 1119;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 123
    WHERE [RolePermissionId] = 1120;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 124
    WHERE [RolePermissionId] = 1121;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 125
    WHERE [RolePermissionId] = 1122;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 126
    WHERE [RolePermissionId] = 1123;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 127
    WHERE [RolePermissionId] = 1124;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 128
    WHERE [RolePermissionId] = 1125;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 129
    WHERE [RolePermissionId] = 1126;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 130
    WHERE [RolePermissionId] = 1127;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 131
    WHERE [RolePermissionId] = 1128;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 132
    WHERE [RolePermissionId] = 1129;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 133
    WHERE [RolePermissionId] = 1130;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 134
    WHERE [RolePermissionId] = 1131;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 135
    WHERE [RolePermissionId] = 1132;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 136
    WHERE [RolePermissionId] = 1133;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 137
    WHERE [RolePermissionId] = 1134;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 138
    WHERE [RolePermissionId] = 1135;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 139
    WHERE [RolePermissionId] = 1136;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 140
    WHERE [RolePermissionId] = 1137;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 141
    WHERE [RolePermissionId] = 1138;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 142
    WHERE [RolePermissionId] = 1139;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 1, [RoleId] = 2
    WHERE [RolePermissionId] = 1140;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 2, [RoleId] = 2
    WHERE [RolePermissionId] = 1141;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 4
    WHERE [RolePermissionId] = 1142;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 5
    WHERE [RolePermissionId] = 1143;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 6
    WHERE [RolePermissionId] = 1144;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 7
    WHERE [RolePermissionId] = 1145;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 9
    WHERE [RolePermissionId] = 1146;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 13
    WHERE [RolePermissionId] = 1147;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 17
    WHERE [RolePermissionId] = 1148;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 18
    WHERE [RolePermissionId] = 1149;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 19
    WHERE [RolePermissionId] = 1150;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 20
    WHERE [RolePermissionId] = 1151;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 21
    WHERE [RolePermissionId] = 1152;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 22
    WHERE [RolePermissionId] = 1153;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 23
    WHERE [RolePermissionId] = 1154;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 49
    WHERE [RolePermissionId] = 1155;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 54
    WHERE [RolePermissionId] = 1156;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 77
    WHERE [RolePermissionId] = 1157;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 24
    WHERE [RolePermissionId] = 1158;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 25
    WHERE [RolePermissionId] = 1159;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 26
    WHERE [RolePermissionId] = 1160;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 27
    WHERE [RolePermissionId] = 1161;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 28
    WHERE [RolePermissionId] = 1162;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 50
    WHERE [RolePermissionId] = 1163;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 55
    WHERE [RolePermissionId] = 1164;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 78
    WHERE [RolePermissionId] = 1165;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 29
    WHERE [RolePermissionId] = 1166;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 30
    WHERE [RolePermissionId] = 1167;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 60
    WHERE [RolePermissionId] = 1168;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 84
    WHERE [RolePermissionId] = 1169;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 85
    WHERE [RolePermissionId] = 1170;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 31
    WHERE [RolePermissionId] = 1171;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 32
    WHERE [RolePermissionId] = 1172;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 52
    WHERE [RolePermissionId] = 1173;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 57
    WHERE [RolePermissionId] = 1174;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 80
    WHERE [RolePermissionId] = 1175;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 33
    WHERE [RolePermissionId] = 1176;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 34
    WHERE [RolePermissionId] = 1177;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 53
    WHERE [RolePermissionId] = 1178;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 56
    WHERE [RolePermissionId] = 1179;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 79
    WHERE [RolePermissionId] = 1180;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 35
    WHERE [RolePermissionId] = 1181;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 36
    WHERE [RolePermissionId] = 1182;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 61
    WHERE [RolePermissionId] = 1183;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 86
    WHERE [RolePermissionId] = 1184;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 87
    WHERE [RolePermissionId] = 1185;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 37
    WHERE [RolePermissionId] = 1186;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 38
    WHERE [RolePermissionId] = 1187;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 51
    WHERE [RolePermissionId] = 1188;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 58
    WHERE [RolePermissionId] = 1189;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 81
    WHERE [RolePermissionId] = 1190;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 39
    WHERE [RolePermissionId] = 1191;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 40
    WHERE [RolePermissionId] = 1192;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 62
    WHERE [RolePermissionId] = 1193;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 90
    WHERE [RolePermissionId] = 1194;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 91
    WHERE [RolePermissionId] = 1195;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 41
    WHERE [RolePermissionId] = 1196;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 42
    WHERE [RolePermissionId] = 1197;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 63
    WHERE [RolePermissionId] = 1198;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 65
    WHERE [RolePermissionId] = 1199;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 83
    WHERE [RolePermissionId] = 1200;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 88
    WHERE [RolePermissionId] = 1201;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 89
    WHERE [RolePermissionId] = 1202;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 46
    WHERE [RolePermissionId] = 1203;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 82
    WHERE [RolePermissionId] = 1204;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 66
    WHERE [RolePermissionId] = 1205;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 43
    WHERE [RolePermissionId] = 1206;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 112
    WHERE [RolePermissionId] = 1207;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 121
    WHERE [RolePermissionId] = 1208;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 122
    WHERE [RolePermissionId] = 1209;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 123
    WHERE [RolePermissionId] = 1210;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 125
    WHERE [RolePermissionId] = 1211;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 126
    WHERE [RolePermissionId] = 1212;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 127
    WHERE [RolePermissionId] = 1213;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 140
    WHERE [RolePermissionId] = 1214;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 142
    WHERE [RolePermissionId] = 1215;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 1, [RoleId] = 3
    WHERE [RolePermissionId] = 1216;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 2, [RoleId] = 3
    WHERE [RolePermissionId] = 1217;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 4, [RoleId] = 3
    WHERE [RolePermissionId] = 1218;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 5
    WHERE [RolePermissionId] = 1219;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 17
    WHERE [RolePermissionId] = 1220;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 18
    WHERE [RolePermissionId] = 1221;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 19
    WHERE [RolePermissionId] = 1222;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 22
    WHERE [RolePermissionId] = 1223;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 23
    WHERE [RolePermissionId] = 1224;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 49
    WHERE [RolePermissionId] = 1225;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 54
    WHERE [RolePermissionId] = 1226;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 24
    WHERE [RolePermissionId] = 1227;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 25
    WHERE [RolePermissionId] = 1228;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 27
    WHERE [RolePermissionId] = 1229;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 28
    WHERE [RolePermissionId] = 1230;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 50
    WHERE [RolePermissionId] = 1231;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 55
    WHERE [RolePermissionId] = 1232;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 29
    WHERE [RolePermissionId] = 1233;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 30
    WHERE [RolePermissionId] = 1234;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 60
    WHERE [RolePermissionId] = 1235;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 84
    WHERE [RolePermissionId] = 1236;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 31
    WHERE [RolePermissionId] = 1237;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 32
    WHERE [RolePermissionId] = 1238;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 52
    WHERE [RolePermissionId] = 1239;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 57
    WHERE [RolePermissionId] = 1240;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 33
    WHERE [RolePermissionId] = 1241;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 34
    WHERE [RolePermissionId] = 1242;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 53
    WHERE [RolePermissionId] = 1243;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 56
    WHERE [RolePermissionId] = 1244;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 35
    WHERE [RolePermissionId] = 1245;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 36
    WHERE [RolePermissionId] = 1246;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 61
    WHERE [RolePermissionId] = 1247;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 86
    WHERE [RolePermissionId] = 1248;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 37
    WHERE [RolePermissionId] = 1249;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 38
    WHERE [RolePermissionId] = 1250;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 51
    WHERE [RolePermissionId] = 1251;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 58
    WHERE [RolePermissionId] = 1252;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 46
    WHERE [RolePermissionId] = 1253;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 47
    WHERE [RolePermissionId] = 1254;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 48
    WHERE [RolePermissionId] = 1255;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 59
    WHERE [RolePermissionId] = 1256;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 43
    WHERE [RolePermissionId] = 1257;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 121
    WHERE [RolePermissionId] = 1258;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 122
    WHERE [RolePermissionId] = 1259;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 3, [RoleId] = 4
    WHERE [RolePermissionId] = 1260;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 4, [RoleId] = 4
    WHERE [RolePermissionId] = 1261;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 11, [RoleId] = 4
    WHERE [RolePermissionId] = 1262;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 12
    WHERE [RolePermissionId] = 1263;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 17
    WHERE [RolePermissionId] = 1264;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 22
    WHERE [RolePermissionId] = 1265;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 49
    WHERE [RolePermissionId] = 1266;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 54
    WHERE [RolePermissionId] = 1267;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 24
    WHERE [RolePermissionId] = 1268;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 27
    WHERE [RolePermissionId] = 1269;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 50
    WHERE [RolePermissionId] = 1270;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 55
    WHERE [RolePermissionId] = 1271;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 29
    WHERE [RolePermissionId] = 1272;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 31
    WHERE [RolePermissionId] = 1273;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 52
    WHERE [RolePermissionId] = 1274;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 57
    WHERE [RolePermissionId] = 1275;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 33
    WHERE [RolePermissionId] = 1276;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 53
    WHERE [RolePermissionId] = 1277;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 56
    WHERE [RolePermissionId] = 1278;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 35
    WHERE [RolePermissionId] = 1279;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 37
    WHERE [RolePermissionId] = 1280;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 38
    WHERE [RolePermissionId] = 1281;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 51
    WHERE [RolePermissionId] = 1282;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 58
    WHERE [RolePermissionId] = 1283;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 39
    WHERE [RolePermissionId] = 1284;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 41
    WHERE [RolePermissionId] = 1285;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 42
    WHERE [RolePermissionId] = 1286;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 63
    WHERE [RolePermissionId] = 1287;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 65
    WHERE [RolePermissionId] = 1288;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 43
    WHERE [RolePermissionId] = 1289;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 44
    WHERE [RolePermissionId] = 1290;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 45
    WHERE [RolePermissionId] = 1291;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 64
    WHERE [RolePermissionId] = 1292;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 88
    WHERE [RolePermissionId] = 1293;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 46
    WHERE [RolePermissionId] = 1294;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 47
    WHERE [RolePermissionId] = 1295;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 48
    WHERE [RolePermissionId] = 1296;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 59
    WHERE [RolePermissionId] = 1297;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 66
    WHERE [RolePermissionId] = 1298;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 67
    WHERE [RolePermissionId] = 1299;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 68
    WHERE [RolePermissionId] = 1300;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 69
    WHERE [RolePermissionId] = 1301;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 70
    WHERE [RolePermissionId] = 1302;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 71
    WHERE [RolePermissionId] = 1303;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 72
    WHERE [RolePermissionId] = 1304;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 92
    WHERE [RolePermissionId] = 1305;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 93
    WHERE [RolePermissionId] = 1306;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 73
    WHERE [RolePermissionId] = 1307;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 75
    WHERE [RolePermissionId] = 1308;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 95
    WHERE [RolePermissionId] = 1309;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 96
    WHERE [RolePermissionId] = 1310;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 98
    WHERE [RolePermissionId] = 1311;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 99
    WHERE [RolePermissionId] = 1312;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 113
    WHERE [RolePermissionId] = 1313;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 114
    WHERE [RolePermissionId] = 1314;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 115
    WHERE [RolePermissionId] = 1315;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 116
    WHERE [RolePermissionId] = 1316;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 117
    WHERE [RolePermissionId] = 1317;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 118
    WHERE [RolePermissionId] = 1318;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 119
    WHERE [RolePermissionId] = 1319;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 120
    WHERE [RolePermissionId] = 1320;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 121
    WHERE [RolePermissionId] = 1321;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 140
    WHERE [RolePermissionId] = 1322;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 141
    WHERE [RolePermissionId] = 1323;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 142
    WHERE [RolePermissionId] = 1324;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 4, [RoleId] = 5
    WHERE [RolePermissionId] = 1325;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 13, [RoleId] = 5
    WHERE [RolePermissionId] = 1326;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 17, [RoleId] = 5
    WHERE [RolePermissionId] = 1327;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 22
    WHERE [RolePermissionId] = 1328;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 24
    WHERE [RolePermissionId] = 1329;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 27
    WHERE [RolePermissionId] = 1330;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 29
    WHERE [RolePermissionId] = 1331;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 31
    WHERE [RolePermissionId] = 1332;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 33
    WHERE [RolePermissionId] = 1333;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 35
    WHERE [RolePermissionId] = 1334;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 37
    WHERE [RolePermissionId] = 1335;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 39
    WHERE [RolePermissionId] = 1336;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 41
    WHERE [RolePermissionId] = 1337;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 43
    WHERE [RolePermissionId] = 1338;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 44
    WHERE [RolePermissionId] = 1339;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 46
    WHERE [RolePermissionId] = 1340;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 66
    WHERE [RolePermissionId] = 1341;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 71
    WHERE [RolePermissionId] = 1342;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 73
    WHERE [RolePermissionId] = 1343;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 113
    WHERE [RolePermissionId] = 1344;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 117
    WHERE [RolePermissionId] = 1345;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 121
    WHERE [RolePermissionId] = 1346;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 125
    WHERE [RolePermissionId] = 1347;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 126
    WHERE [RolePermissionId] = 1348;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 127
    WHERE [RolePermissionId] = 1349;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 140
    WHERE [RolePermissionId] = 1350;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 142
    WHERE [RolePermissionId] = 1351;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 5, [RoleId] = 6
    WHERE [RolePermissionId] = 1352;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 17, [RoleId] = 6
    WHERE [RolePermissionId] = 1353;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 22, [RoleId] = 6
    WHERE [RolePermissionId] = 1354;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 24
    WHERE [RolePermissionId] = 1355;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 27
    WHERE [RolePermissionId] = 1356;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 29
    WHERE [RolePermissionId] = 1357;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 30
    WHERE [RolePermissionId] = 1358;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 60
    WHERE [RolePermissionId] = 1359;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 84
    WHERE [RolePermissionId] = 1360;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 31
    WHERE [RolePermissionId] = 1361;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 32
    WHERE [RolePermissionId] = 1362;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 52
    WHERE [RolePermissionId] = 1363;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 57
    WHERE [RolePermissionId] = 1364;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 33
    WHERE [RolePermissionId] = 1365;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 34
    WHERE [RolePermissionId] = 1366;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 53
    WHERE [RolePermissionId] = 1367;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 56
    WHERE [RolePermissionId] = 1368;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 35
    WHERE [RolePermissionId] = 1369;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 36
    WHERE [RolePermissionId] = 1370;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 61
    WHERE [RolePermissionId] = 1371;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 86
    WHERE [RolePermissionId] = 1372;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 37
    WHERE [RolePermissionId] = 1373;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 43
    WHERE [RolePermissionId] = 1374;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 121
    WHERE [RolePermissionId] = 1375;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 122
    WHERE [RolePermissionId] = 1376;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 123
    WHERE [RolePermissionId] = 1377;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 73, [RoleId] = 7
    WHERE [RolePermissionId] = 1378;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 74, [RoleId] = 7
    WHERE [RolePermissionId] = 1379;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 95, [RoleId] = 7
    WHERE [RolePermissionId] = 1380;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 96
    WHERE [RolePermissionId] = 1381;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [RolePermissions] SET [PermissionId] = 43
    WHERE [RolePermissionId] = 1382;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T21:34:00.9829323Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T21:34:00.9829620Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T21:34:00.9829623Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T21:34:00.9829625Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T21:34:00.9829628Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703213404_RemoveTreasuryTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703213404_RemoveTreasuryTables', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    ALTER TABLE [OperationalExpenses] DROP CONSTRAINT [FK_OperationalExpenses_BudgetAllocations_BudgetAllocationID];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    DROP TABLE [BudgetAllocations];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    DROP TABLE [BudgetItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    DROP TABLE [BudgetSections];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    DROP TABLE [BudgetChapters];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    DROP INDEX [IX_OperationalExpenses_BudgetAllocationID] ON [OperationalExpenses];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 71;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 72;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 92;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 93;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 94;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 115;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 119;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 120;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OperationalExpenses]') AND [c].[name] = N'BudgetAllocationID');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [OperationalExpenses] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [OperationalExpenses] DROP COLUMN [BudgetAllocationID];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:18:07.6181636Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:18:07.6181677Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:18:07.6181679Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:18:07.6181680Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:18:07.6181681Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:18:07.6449996Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:18:07.6450020Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:18:07.6450022Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:18:07.6450023Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:18:07.6450025Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703221810_RemoveOperationalBudget'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703221810_RemoveOperationalBudget', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    CREATE TABLE [Vouchers] (
        [VoucherID] int NOT NULL IDENTITY,
        [VoucherNumber] nvarchar(30) NOT NULL,
        [VoucherType] int NOT NULL,
        [Status] int NOT NULL,
        [VoucherDate] datetime2 NOT NULL,
        [PayeeName] nvarchar(200) NULL,
        [PayerName] nvarchar(200) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [DebitAccountID] int NOT NULL,
        [CreditAccountID] int NOT NULL,
        [ChequeNumber] nvarchar(100) NULL,
        [ReferenceNumber] nvarchar(100) NULL,
        [JournalEntryID] int NULL,
        [JournalEntryNumber] nvarchar(30) NULL,
        [CreatedByUserID] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_Vouchers] PRIMARY KEY ([VoucherID]),
        CONSTRAINT [FK_Vouchers_ChartOfAccounts_CreditAccountID] FOREIGN KEY ([CreditAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Vouchers_ChartOfAccounts_DebitAccountID] FOREIGN KEY ([DebitAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Vouchers_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Vouchers_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:54:18.1655937Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:54:18.1655978Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:54:18.1655979Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:54:18.1655981Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-03T22:54:18.1655982Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:54:18.1923547Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:54:18.1923581Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:54:18.1923583Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:54:18.1923584Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-03T22:54:18.1923585Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    CREATE INDEX [IX_Vouchers_CreatedByUserID] ON [Vouchers] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    CREATE INDEX [IX_Vouchers_CreditAccountID] ON [Vouchers] ([CreditAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    CREATE INDEX [IX_Vouchers_DebitAccountID] ON [Vouchers] ([DebitAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    CREATE INDEX [IX_Vouchers_DeletedByUserID] ON [Vouchers] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vouchers_VoucherNumber] ON [Vouchers] ([VoucherNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703225422_AddVouchers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703225422_AddVouchers', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-04T21:54:30.0910784Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-04T21:54:30.0910819Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-04T21:54:30.0910821Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-04T21:54:30.0910823Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-04T21:54:30.0910824Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-04T21:54:30.1180815Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-04T21:54:30.1180843Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-04T21:54:30.1180844Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-04T21:54:30.1180845Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-04T21:54:30.1180847Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704215433_AddReferralSourceToVoucher'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704215433_AddReferralSourceToVoucher', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705191141_AddCreatedByUserIDToJournalEntry'
)
BEGIN
    ALTER TABLE [JournalEntries] ADD [CreatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705191141_AddCreatedByUserIDToJournalEntry'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_CreatedByUserID] ON [JournalEntries] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705191141_AddCreatedByUserIDToJournalEntry'
)
BEGIN
    ALTER TABLE [JournalEntries] ADD CONSTRAINT [FK_JournalEntries_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705191141_AddCreatedByUserIDToJournalEntry'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705191141_AddCreatedByUserIDToJournalEntry', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    ALTER TABLE [FiscalYears] ADD [ClosedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    ALTER TABLE [FiscalYears] ADD [ClosedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-05T19:20:48.0940130Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-05T19:20:48.0940136Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-05T19:20:48.0940137Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-05T19:20:48.0940138Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-05T19:20:48.0940142Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-05T19:20:48.1232126Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-05T19:20:48.1232132Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-05T19:20:48.1232134Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-05T19:20:48.1232135Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-05T19:20:48.1232137Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    CREATE INDEX [IX_FiscalYears_ClosedByUserID] ON [FiscalYears] ([ClosedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    ALTER TABLE [FiscalYears] ADD CONSTRAINT [FK_FiscalYears_Users_ClosedByUserID] FOREIGN KEY ([ClosedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705192051_AddClosedAtClosedByUserIDToFiscalYear', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    ALTER TABLE [InventoryCategories] ADD [AccountID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    ALTER TABLE [InventoryCategories] ADD [CategoryCode] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T21:35:20.3721233Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T21:35:20.3721242Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T21:35:20.3721244Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T21:35:20.3721245Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T21:35:20.3721246Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T21:35:20.4032741Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T21:35:20.4032748Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T21:35:20.4032750Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T21:35:20.4032751Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T21:35:20.4032752Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    CREATE INDEX [IX_InventoryCategories_AccountID] ON [InventoryCategories] ([AccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InventoryCategories_CategoryCode] ON [InventoryCategories] ([CategoryCode]) WHERE [CategoryCode] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    ALTER TABLE [InventoryCategories] ADD CONSTRAINT [FK_InventoryCategories_ChartOfAccounts_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706213524_AddAccountLinkToInventoryCategory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706213524_AddAccountLinkToInventoryCategory', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [FixedAssetTypes] (
        [AssetTypeID] int NOT NULL IDENTITY,
        [TypeName] nvarchar(50) NOT NULL,
        [TypeCode] nvarchar(20) NOT NULL,
        [Description] nvarchar(200) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FixedAssetTypes] PRIMARY KEY ([AssetTypeID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [FixedAssetCategories] (
        [CategoryID] int NOT NULL IDENTITY,
        [CategoryName] nvarchar(100) NOT NULL,
        [CategoryCode] nvarchar(20) NULL,
        [Description] nvarchar(500) NULL,
        [AssetTypeID] int NOT NULL,
        [UsefulLifeMonths] int NOT NULL,
        [SalvagePercent] decimal(5,2) NOT NULL,
        [AssetAccountID] int NULL,
        [AccumDeprAccountID] int NULL,
        [DeprExpenseAccountID] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        CONSTRAINT [PK_FixedAssetCategories] PRIMARY KEY ([CategoryID]),
        CONSTRAINT [FK_FixedAssetCategories_ChartOfAccounts_AccumDeprAccountID] FOREIGN KEY ([AccumDeprAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssetCategories_ChartOfAccounts_AssetAccountID] FOREIGN KEY ([AssetAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssetCategories_ChartOfAccounts_DeprExpenseAccountID] FOREIGN KEY ([DeprExpenseAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssetCategories_FixedAssetTypes_AssetTypeID] FOREIGN KEY ([AssetTypeID]) REFERENCES [FixedAssetTypes] ([AssetTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssetCategories_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssetCategories_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [FixedAssets] (
        [AssetID] int NOT NULL IDENTITY,
        [AssetNumber] nvarchar(50) NOT NULL,
        [AssetName] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CategoryID] int NOT NULL,
        [AssetTypeID] int NOT NULL,
        [SerialNumber] nvarchar(100) NULL,
        [PurchaseDate] datetime2 NOT NULL,
        [InServiceDate] datetime2 NOT NULL,
        [AcquiredCost] decimal(18,2) NOT NULL,
        [SalvageValue] decimal(18,2) NOT NULL,
        [CurrencyCode] nvarchar(3) NOT NULL,
        [UsefulLifeMonths] int NOT NULL,
        [DepreciationMethod] int NOT NULL,
        [AccumulatedDepreciation] decimal(18,2) NOT NULL,
        [NetBookValue] decimal(18,2) NOT NULL,
        [LastDepreciationDate] datetime2 NULL,
        [Location] nvarchar(200) NULL,
        [CustodianEmployeeID] int NULL,
        [AssetAccountID] int NOT NULL,
        [AccumDeprAccountID] int NOT NULL,
        [DeprExpenseAccountID] int NOT NULL,
        [Status] int NOT NULL,
        [DisposalDate] datetime2 NULL,
        [DisposalReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedByUserID] int NULL,
        [UpdatedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_FixedAssets] PRIMARY KEY ([AssetID]),
        CONSTRAINT [FK_FixedAssets_ChartOfAccounts_AccumDeprAccountID] FOREIGN KEY ([AccumDeprAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssets_ChartOfAccounts_AssetAccountID] FOREIGN KEY ([AssetAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssets_ChartOfAccounts_DeprExpenseAccountID] FOREIGN KEY ([DeprExpenseAccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssets_Employees_CustodianEmployeeID] FOREIGN KEY ([CustodianEmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssets_FixedAssetCategories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [FixedAssetCategories] ([CategoryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssets_FixedAssetTypes_AssetTypeID] FOREIGN KEY ([AssetTypeID]) REFERENCES [FixedAssetTypes] ([AssetTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssets_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FixedAssets_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_FixedAssets_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [AssetBuildingDetails] (
        [AssetID] int NOT NULL,
        [AreaSqm] decimal(10,2) NULL,
        [BuiltAreaSqm] decimal(10,2) NULL,
        [FloorCount] int NOT NULL,
        [OwnershipDocNumber] nvarchar(100) NULL,
        [LicenseNumber] nvarchar(100) NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_AssetBuildingDetails] PRIMARY KEY ([AssetID]),
        CONSTRAINT [FK_AssetBuildingDetails_FixedAssets_AssetID] FOREIGN KEY ([AssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [AssetFurnitureDetails] (
        [AssetID] int NOT NULL,
        [Material] nvarchar(50) NULL,
        [Color] nvarchar(50) NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_AssetFurnitureDetails] PRIMARY KEY ([AssetID]),
        CONSTRAINT [FK_AssetFurnitureDetails_FixedAssets_AssetID] FOREIGN KEY ([AssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [AssetITDetails] (
        [AssetID] int NOT NULL,
        [Model] nvarchar(100) NULL,
        [Manufacturer] nvarchar(100) NULL,
        [ProcessorType] nvarchar(100) NULL,
        [ProcessorSpeed] nvarchar(50) NULL,
        [RamSize] nvarchar(50) NULL,
        [HardDiskType] nvarchar(50) NULL,
        [HardDiskSize] nvarchar(50) NULL,
        [GpuModel] nvarchar(100) NULL,
        [ScreenSize] nvarchar(50) NULL,
        [ScreenResolution] nvarchar(50) NULL,
        [OperatingSystem] nvarchar(50) NULL,
        [HasWebcam] bit NOT NULL,
        [HasWifi] bit NOT NULL,
        [HasBluetooth] bit NOT NULL,
        [IpAddress] nvarchar(50) NULL,
        [MacAddress] nvarchar(50) NULL,
        [WarrantyExpiry] datetime2 NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_AssetITDetails] PRIMARY KEY ([AssetID]),
        CONSTRAINT [FK_AssetITDetails_FixedAssets_AssetID] FOREIGN KEY ([AssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [AssetLandDetails] (
        [AssetID] int NOT NULL,
        [PlotNumber] nvarchar(100) NULL,
        [LandAreaSqm] decimal(12,2) NULL,
        [OwnershipDocNumber] nvarchar(100) NULL,
        [OwnershipDocDate] datetime2 NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_AssetLandDetails] PRIMARY KEY ([AssetID]),
        CONSTRAINT [FK_AssetLandDetails_FixedAssets_AssetID] FOREIGN KEY ([AssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [AssetVehicleDetails] (
        [AssetID] int NOT NULL,
        [PlateNumber] nvarchar(50) NULL,
        [PlateLetter1] nvarchar(10) NULL,
        [PlateLetter2] nvarchar(10) NULL,
        [PlateLetter3] nvarchar(10) NULL,
        [EngineNumber] nvarchar(100) NULL,
        [ChassisNumber] nvarchar(100) NULL,
        [TransmissionType] nvarchar(50) NULL,
        [FuelType] nvarchar(50) NULL,
        [CylinderCount] int NULL,
        [EngineCapacity] nvarchar(50) NULL,
        [HorsePower] int NULL,
        [WheelCount] int NOT NULL,
        [TireSize] nvarchar(50) NULL,
        [Color] nvarchar(50) NULL,
        [ModelYear] int NULL,
        [RegistrationExpiry] datetime2 NULL,
        [InsuranceExpiry] datetime2 NULL,
        [Mileage] int NOT NULL,
        [HasGps] bit NOT NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_AssetVehicleDetails] PRIMARY KEY ([AssetID]),
        CONSTRAINT [FK_AssetVehicleDetails_FixedAssets_AssetID] FOREIGN KEY ([AssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE TABLE [DepreciationSchedules] (
        [ScheduleID] int NOT NULL IDENTITY,
        [AssetID] int NOT NULL,
        [PeriodYear] int NOT NULL,
        [PeriodMonth] int NOT NULL,
        [PeriodStartDate] datetime2 NOT NULL,
        [PeriodEndDate] datetime2 NOT NULL,
        [DepreciationAmount] decimal(18,2) NOT NULL,
        [AccumulatedDepreciation] decimal(18,2) NOT NULL,
        [NetBookValue] decimal(18,2) NOT NULL,
        [IsPosted] bit NOT NULL,
        [PostedDate] datetime2 NULL,
        [JournalEntryID] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_DepreciationSchedules] PRIMARY KEY ([ScheduleID]),
        CONSTRAINT [FK_DepreciationSchedules_FixedAssets_AssetID] FOREIGN KEY ([AssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DepreciationSchedules_JournalEntries_JournalEntryID] FOREIGN KEY ([JournalEntryID]) REFERENCES [JournalEntries] ([JournalEntryID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T23:11:59.0881811Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T23:11:59.0881819Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T23:11:59.0881821Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T23:11:59.0881823Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-06T23:11:59.0881824Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T23:11:59.1176303Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T23:11:59.1176309Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T23:11:59.1176311Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T23:11:59.1176312Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-06T23:11:59.1176313Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DepreciationSchedules_AssetID_PeriodYear_PeriodMonth] ON [DepreciationSchedules] ([AssetID], [PeriodYear], [PeriodMonth]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_DepreciationSchedules_JournalEntryID] ON [DepreciationSchedules] ([JournalEntryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssetCategories_AccumDeprAccountID] ON [FixedAssetCategories] ([AccumDeprAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssetCategories_AssetAccountID] ON [FixedAssetCategories] ([AssetAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssetCategories_AssetTypeID] ON [FixedAssetCategories] ([AssetTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_FixedAssetCategories_CategoryCode] ON [FixedAssetCategories] ([CategoryCode]) WHERE [CategoryCode] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FixedAssetCategories_CategoryName] ON [FixedAssetCategories] ([CategoryName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssetCategories_CreatedByUserID] ON [FixedAssetCategories] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssetCategories_DeprExpenseAccountID] ON [FixedAssetCategories] ([DeprExpenseAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssetCategories_UpdatedByUserID] ON [FixedAssetCategories] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_AccumDeprAccountID] ON [FixedAssets] ([AccumDeprAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_AssetAccountID] ON [FixedAssets] ([AssetAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FixedAssets_AssetNumber] ON [FixedAssets] ([AssetNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_AssetTypeID] ON [FixedAssets] ([AssetTypeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_CategoryID] ON [FixedAssets] ([CategoryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_CreatedByUserID] ON [FixedAssets] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_CustodianEmployeeID] ON [FixedAssets] ([CustodianEmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_DeletedByUserID] ON [FixedAssets] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_DeprExpenseAccountID] ON [FixedAssets] ([DeprExpenseAccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_FixedAssets_SerialNumber] ON [FixedAssets] ([SerialNumber]) WHERE [SerialNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE INDEX [IX_FixedAssets_UpdatedByUserID] ON [FixedAssets] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FixedAssetTypes_TypeCode] ON [FixedAssetTypes] ([TypeCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FixedAssetTypes_TypeName] ON [FixedAssetTypes] ([TypeName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706231201_AddFixedAssetTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706231201_AddFixedAssetTables', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    ALTER TABLE [InventoryCustodies] DROP CONSTRAINT [FK_InventoryCustodies_InventoryItems_InventoryItemID];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC sp_rename N'[InventoryCustodies].[InventoryItemID]', N'AssetID', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC sp_rename N'[InventoryCustodies].[IX_InventoryCustodies_InventoryItemID]', N'IX_InventoryCustodies_AssetID', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-07T20:42:18.8387092Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-07T20:42:18.8387101Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-07T20:42:18.8387103Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-07T20:42:18.8387104Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-07T20:42:18.8387105Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-07T20:42:18.8613717Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-07T20:42:18.8613729Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-07T20:42:18.8613730Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-07T20:42:18.8613731Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-07T20:42:18.8613732Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    ALTER TABLE [InventoryCustodies] ADD CONSTRAINT [FK_InventoryCustodies_FixedAssets_AssetID] FOREIGN KEY ([AssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707204221_LinkCustodyToFixedAssets'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707204221_LinkCustodyToFixedAssets', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    ALTER TABLE [FixedAssets] ADD [Quantity] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T02:41:25.5765710Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T02:41:25.5765716Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T02:41:25.5765717Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T02:41:25.5765718Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T02:41:25.5765720Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T02:41:25.5986101Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T02:41:25.5986106Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T02:41:25.5986108Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T02:41:25.5986123Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T02:41:25.5986124Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708024127_AddAssetQuantity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708024127_AddAssetQuantity', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    ALTER TABLE [InventoryItems] DROP CONSTRAINT [FK_InventoryItems_InventoryCategories_CategoryID];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    DROP TABLE [InventoryCategories];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    DROP INDEX [IX_InventoryItems_CategoryID] ON [InventoryItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'DELETE FROM [UserPermissions]
    WHERE [PermissionId] = 128;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'DELETE FROM [UserPermissions]
    WHERE [PermissionId] = 129;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'DELETE FROM [RolePermissions]
    WHERE [PermissionId] = 128;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'DELETE FROM [RolePermissions]
    WHERE [PermissionId] = 129;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 128;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [PermissionId] = 129;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryItems]') AND [c].[name] = N'CategoryID');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [InventoryItems] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [InventoryItems] DROP COLUMN [CategoryID];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T19:18:42.0934232Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T19:18:42.0934237Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T19:18:42.0934239Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T19:18:42.0934241Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T19:18:42.0934245Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T19:18:42.1187500Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T19:18:42.1187505Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T19:18:42.1187507Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T19:18:42.1187508Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T19:18:42.1187509Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708191845_RemoveInventoryCategories'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708191845_RemoveInventoryCategories', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    ALTER TABLE [FixedAssets] ADD [UnitPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    ALTER TABLE [FixedAssets] ADD [ValuationMethod] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    ALTER TABLE [FixedAssets] ADD [ValuationNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T20:16:58.6052721Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T20:16:58.6052725Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T20:16:58.6052727Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T20:16:58.6052729Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T20:16:58.6052731Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T20:16:58.6296281Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T20:16:58.6296288Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T20:16:58.6296289Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T20:16:58.6296291Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T20:16:58.6296292Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708201701_AddAssetValuationFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708201701_AddAssetValuationFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FixedAssets]') AND [c].[name] = N'DepreciationMethod');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [FixedAssets] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [FixedAssets] DROP COLUMN [DepreciationMethod];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FixedAssets]') AND [c].[name] = N'UsefulLifeMonths');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [FixedAssets] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [FixedAssets] DROP COLUMN [UsefulLifeMonths];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FixedAssetCategories]') AND [c].[name] = N'UsefulLifeMonths');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [FixedAssetCategories] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [FixedAssetCategories] DROP COLUMN [UsefulLifeMonths];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    ALTER TABLE [FixedAssets] ADD [DepreciationRate] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    ALTER TABLE [FixedAssetCategories] ADD [DepreciationRate] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T22:45:08.3202161Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T22:45:08.3202168Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T22:45:08.3202170Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T22:45:08.3202171Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-08T22:45:08.3202172Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T22:45:08.3464341Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T22:45:08.3464349Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T22:45:08.3464350Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T22:45:08.3464352Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-08T22:45:08.3464353Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708224515_AnnualDepreciationRate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708224515_AnnualDepreciationRate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    CREATE TABLE [BudgetItems] (
        [BudgetItemID] int NOT NULL IDENTITY,
        [FiscalYearID] int NOT NULL,
        [QuarterID] int NULL,
        [PeriodID] int NULL,
        [AccountID] int NOT NULL,
        [PlannedAmount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_BudgetItems] PRIMARY KEY ([BudgetItemID]),
        CONSTRAINT [FK_BudgetItems_ChartOfAccounts_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetItems_FiscalPeriods_PeriodID] FOREIGN KEY ([PeriodID]) REFERENCES [FiscalPeriods] ([PeriodID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetItems_FiscalYears_FiscalYearID] FOREIGN KEY ([FiscalYearID]) REFERENCES [FiscalYears] ([FiscalYearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetItems_Quarters_QuarterID] FOREIGN KEY ([QuarterID]) REFERENCES [Quarters] ([QuarterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BudgetItems_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-09T19:27:20.8145235Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-09T19:27:20.8145242Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-09T19:27:20.8145244Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-09T19:27:20.8145263Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-09T19:27:20.8145264Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-09T19:27:20.8349553Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-09T19:27:20.8349563Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-09T19:27:20.8349565Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-09T19:27:20.8349566Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-09T19:27:20.8349567Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    CREATE INDEX [IX_BudgetItems_AccountID] ON [BudgetItems] ([AccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    CREATE INDEX [IX_BudgetItems_DeletedByUserID] ON [BudgetItems] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    CREATE INDEX [IX_BudgetItems_FiscalYearID] ON [BudgetItems] ([FiscalYearID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    CREATE INDEX [IX_BudgetItems_PeriodID] ON [BudgetItems] ([PeriodID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    CREATE INDEX [IX_BudgetItems_QuarterID] ON [BudgetItems] ([QuarterID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260709192724_AddBudgetItem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260709192724_AddBudgetItem', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE TABLE [AssetStocktakes] (
        [AssetStocktakeID] int NOT NULL IDENTITY,
        [FiscalYearID] int NOT NULL,
        [StocktakeDate] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedByUserID] int NOT NULL,
        [VerifiedByUserID] int NULL,
        [VerifiedAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_AssetStocktakes] PRIMARY KEY ([AssetStocktakeID]),
        CONSTRAINT [FK_AssetStocktakes_FiscalYears_FiscalYearID] FOREIGN KEY ([FiscalYearID]) REFERENCES [FiscalYears] ([FiscalYearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AssetStocktakes_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AssetStocktakes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_AssetStocktakes_Users_VerifiedByUserID] FOREIGN KEY ([VerifiedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE TABLE [SimpleStocktakes] (
        [SimpleStocktakeID] int NOT NULL IDENTITY,
        [FiscalYearID] int NOT NULL,
        [StocktakeDate] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedByUserID] int NOT NULL,
        [CompletedByUserID] int NULL,
        [CompletedAt] datetime2 NULL,
        [TotalItems] int NOT NULL,
        [ItemsWithVariance] int NOT NULL,
        [TotalVarianceValue] decimal(18,2) NOT NULL,
        [JournalEntryID] int NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_SimpleStocktakes] PRIMARY KEY ([SimpleStocktakeID]),
        CONSTRAINT [FK_SimpleStocktakes_FiscalYears_FiscalYearID] FOREIGN KEY ([FiscalYearID]) REFERENCES [FiscalYears] ([FiscalYearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SimpleStocktakes_JournalEntries_JournalEntryID] FOREIGN KEY ([JournalEntryID]) REFERENCES [JournalEntries] ([JournalEntryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SimpleStocktakes_Users_CompletedByUserID] FOREIGN KEY ([CompletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SimpleStocktakes_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SimpleStocktakes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE TABLE [AssetStocktakeLines] (
        [StocktakeLineID] int NOT NULL IDENTITY,
        [AssetStocktakeID] int NOT NULL,
        [FixedAssetID] int NOT NULL,
        [AssetNumber] nvarchar(max) NOT NULL,
        [AssetName] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NULL,
        [NetBookValue] decimal(18,2) NOT NULL,
        [ActualLocation] nvarchar(max) NULL,
        [ActualCondition] nvarchar(max) NOT NULL,
        [IsPresent] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_AssetStocktakeLines] PRIMARY KEY ([StocktakeLineID]),
        CONSTRAINT [FK_AssetStocktakeLines_AssetStocktakes_AssetStocktakeID] FOREIGN KEY ([AssetStocktakeID]) REFERENCES [AssetStocktakes] ([AssetStocktakeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AssetStocktakeLines_FixedAssets_FixedAssetID] FOREIGN KEY ([FixedAssetID]) REFERENCES [FixedAssets] ([AssetID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE TABLE [SimpleStocktakeLines] (
        [LineID] int NOT NULL IDENTITY,
        [SimpleStocktakeID] int NOT NULL,
        [InventoryItemID] int NOT NULL,
        [ItemName] nvarchar(max) NOT NULL,
        [ItemCode] nvarchar(max) NOT NULL,
        [SystemQuantity] int NOT NULL,
        [PhysicalQuantity] int NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_SimpleStocktakeLines] PRIMARY KEY ([LineID]),
        CONSTRAINT [FK_SimpleStocktakeLines_InventoryItems_InventoryItemID] FOREIGN KEY ([InventoryItemID]) REFERENCES [InventoryItems] ([InventoryItemID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SimpleStocktakeLines_SimpleStocktakes_SimpleStocktakeID] FOREIGN KEY ([SimpleStocktakeID]) REFERENCES [SimpleStocktakes] ([SimpleStocktakeID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-10T22:44:23.4136285Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-10T22:44:23.4136295Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-10T22:44:23.4136297Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-10T22:44:23.4136299Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-10T22:44:23.4136301Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-10T22:44:23.4327780Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-10T22:44:23.4327786Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-10T22:44:23.4327788Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-10T22:44:23.4327789Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-10T22:44:23.4327790Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_AssetStocktakeLines_AssetStocktakeID] ON [AssetStocktakeLines] ([AssetStocktakeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_AssetStocktakeLines_FixedAssetID] ON [AssetStocktakeLines] ([FixedAssetID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_AssetStocktakes_CreatedByUserID] ON [AssetStocktakes] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_AssetStocktakes_DeletedByUserID] ON [AssetStocktakes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_AssetStocktakes_FiscalYearID] ON [AssetStocktakes] ([FiscalYearID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_AssetStocktakes_VerifiedByUserID] ON [AssetStocktakes] ([VerifiedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_SimpleStocktakeLines_InventoryItemID] ON [SimpleStocktakeLines] ([InventoryItemID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_SimpleStocktakeLines_SimpleStocktakeID] ON [SimpleStocktakeLines] ([SimpleStocktakeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_SimpleStocktakes_CompletedByUserID] ON [SimpleStocktakes] ([CompletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_SimpleStocktakes_CreatedByUserID] ON [SimpleStocktakes] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_SimpleStocktakes_DeletedByUserID] ON [SimpleStocktakes] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_SimpleStocktakes_FiscalYearID] ON [SimpleStocktakes] ([FiscalYearID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    CREATE INDEX [IX_SimpleStocktakes_JournalEntryID] ON [SimpleStocktakes] ([JournalEntryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710224426_AddStocktakeTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710224426_AddStocktakeTables', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    ALTER TABLE [AssetStocktakeLines] ADD [RecordedQuantity] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    ALTER TABLE [AssetStocktakeLines] ADD [SystemQuantity] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-11T00:24:56.7306960Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-11T00:24:56.7306967Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-11T00:24:56.7306968Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-11T00:24:56.7306970Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-11T00:24:56.7306971Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-11T00:24:56.7519205Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-11T00:24:56.7519211Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-11T00:24:56.7519212Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-11T00:24:56.7519214Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-11T00:24:56.7519215Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711002500_AddStocktakeQuantity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260711002500_AddStocktakeQuantity', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    ALTER TABLE [Documents] ADD [TrackingRecordID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE TABLE [AchievementObstacles] (
        [ID] int NOT NULL IDENTITY,
        [Number] nvarchar(max) NOT NULL,
        [Type] int NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [Priority] int NOT NULL,
        [Progress] tinyint NOT NULL,
        [OwnerUserID] int NOT NULL,
        [DepartmentID] int NULL,
        [DueDate] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedByUserID] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedByUserID] int NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_AchievementObstacles] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_AchievementObstacles_Departments_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [Departments] ([DepartmentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AchievementObstacles_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AchievementObstacles_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL,
        CONSTRAINT [FK_AchievementObstacles_Users_OwnerUserID] FOREIGN KEY ([OwnerUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AchievementObstacles_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE TABLE [TrackingComments] (
        [ID] int NOT NULL IDENTITY,
        [TrackingRecordId] int NOT NULL,
        [RecordID] int NOT NULL,
        [Comment] nvarchar(max) NOT NULL,
        [CreatedByUserID] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_TrackingComments] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_TrackingComments_AchievementObstacles_RecordID] FOREIGN KEY ([RecordID]) REFERENCES [AchievementObstacles] ([ID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TrackingComments_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TrackingComments_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE TABLE [TrackingLinks] (
        [ID] int NOT NULL IDENTITY,
        [TrackingRecordId] int NOT NULL,
        [RecordID] int NOT NULL,
        [EntityType] int NOT NULL,
        [EntityId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TrackingLinks] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_TrackingLinks_AchievementObstacles_RecordID] FOREIGN KEY ([RecordID]) REFERENCES [AchievementObstacles] ([ID]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T18:32:10.1477367Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T18:32:10.1477374Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T18:32:10.1477375Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T18:32:10.1477376Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T18:32:10.1477380Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Category', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([PermissionId], [Category], [Code], [Description])
    VALUES (143, N''File'', N''File.View'', N''عرض ملفات التقاعد'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Category', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T18:32:10.1727121Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T18:32:10.1727130Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T18:32:10.1727131Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T18:32:10.1727133Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T18:32:10.1727134Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_Documents_TrackingRecordID] ON [Documents] ([TrackingRecordID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_AchievementObstacles_CreatedByUserID] ON [AchievementObstacles] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_AchievementObstacles_DeletedByUserID] ON [AchievementObstacles] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_AchievementObstacles_DepartmentID] ON [AchievementObstacles] ([DepartmentID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_AchievementObstacles_OwnerUserID] ON [AchievementObstacles] ([OwnerUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_AchievementObstacles_UpdatedByUserID] ON [AchievementObstacles] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_TrackingComments_CreatedByUserID] ON [TrackingComments] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_TrackingComments_DeletedByUserID] ON [TrackingComments] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_TrackingComments_RecordID] ON [TrackingComments] ([RecordID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    CREATE INDEX [IX_TrackingLinks_RecordID] ON [TrackingLinks] ([RecordID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AchievementObstacles_TrackingRecordID] FOREIGN KEY ([TrackingRecordID]) REFERENCES [AchievementObstacles] ([ID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183214_AddFileViewPermission'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717183214_AddFileViewPermission', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [Tenants] ADD [AccountID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [CheckDueDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [CheckNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [IsReconciled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [LineNumber] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [ReconciledAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [ReconciledByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [SubledgerEntityID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD [SubledgerEntityType] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [ChartOfAccounts] ADD [AccountCategory] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [ChartOfAccounts] ADD [HasSubledger] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [ChartOfAccounts] ADD [SubledgerType] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [Banks] ADD [AccountID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [Banks] ADD [BankAccountNo] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [Banks] ADD [SwiftCode] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE TABLE [BankTransactions] (
        [TransactionID] int NOT NULL IDENTITY,
        [BankID] int NOT NULL,
        [JournalEntryID] int NOT NULL,
        [LineID] int NOT NULL,
        [TransactionDate] datetime2 NOT NULL,
        [TransactionType] nvarchar(20) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NULL,
        [ReferenceNo] nvarchar(100) NULL,
        [IsReconciled] bit NOT NULL,
        [ReconciledAt] datetime2 NULL,
        [BankStatementRef] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedByUserID] int NULL,
        CONSTRAINT [PK_BankTransactions] PRIMARY KEY ([TransactionID]),
        CONSTRAINT [FK_BankTransactions_Banks_BankID] FOREIGN KEY ([BankID]) REFERENCES [Banks] ([BankID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BankTransactions_JournalEntries_JournalEntryID] FOREIGN KEY ([JournalEntryID]) REFERENCES [JournalEntries] ([JournalEntryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BankTransactions_JournalEntryLines_LineID] FOREIGN KEY ([LineID]) REFERENCES [JournalEntryLines] ([LineID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BankTransactions_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T21:34:19.6407471Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T21:34:19.6407479Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T21:34:19.6407481Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T21:34:19.6407483Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-17T21:34:19.6407485Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [Banks] SET [AccountID] = NULL, [BankAccountNo] = NULL, [SwiftCode] = NULL
    WHERE [BankID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T21:34:19.6641640Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T21:34:19.6641646Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T21:34:19.6641648Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T21:34:19.6641649Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-17T21:34:19.6641650Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_Tenants_AccountID] ON [Tenants] ([AccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_JournalEntryID_LineNumber] ON [JournalEntryLines] ([JournalEntryID], [LineNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_ReconciledByUserID] ON [JournalEntryLines] ([ReconciledByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_SubledgerEntityType] ON [JournalEntryLines] ([SubledgerEntityType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_PeriodID] ON [JournalEntries] ([PeriodID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_ChartOfAccounts_AccountCategory] ON [ChartOfAccounts] ([AccountCategory]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_Banks_AccountID] ON [Banks] ([AccountID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_BankTransactions_BankID_TransactionDate] ON [BankTransactions] ([BankID], [TransactionDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_BankTransactions_DeletedByUserID] ON [BankTransactions] ([DeletedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_BankTransactions_IsReconciled] ON [BankTransactions] ([IsReconciled]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_BankTransactions_JournalEntryID] ON [BankTransactions] ([JournalEntryID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    CREATE INDEX [IX_BankTransactions_LineID] ON [BankTransactions] ([LineID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [Banks] ADD CONSTRAINT [FK_Banks_ChartOfAccounts_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [JournalEntryLines] ADD CONSTRAINT [FK_JournalEntryLines_Users_ReconciledByUserID] FOREIGN KEY ([ReconciledByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    ALTER TABLE [Tenants] ADD CONSTRAINT [FK_Tenants_ChartOfAccounts_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [ChartOfAccounts] ([AccountID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717213422_AddSubledgerAndBankTransaction'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717213422_AddSubledgerAndBankTransaction', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718013139_AddPaymentBookIssuingBranch'
)
BEGIN
    ALTER TABLE [PaymentBooks] ADD [IssuingBranchID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718013139_AddPaymentBookIssuingBranch'
)
BEGIN
    CREATE INDEX [IX_PaymentBooks_IssuingBranchID] ON [PaymentBooks] ([IssuingBranchID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718013139_AddPaymentBookIssuingBranch'
)
BEGIN
    ALTER TABLE [PaymentBooks] ADD CONSTRAINT [FK_PaymentBooks_PensionFundBranches_IssuingBranchID] FOREIGN KEY ([IssuingBranchID]) REFERENCES [PensionFundBranches] ([PensionBranchID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718013139_AddPaymentBookIssuingBranch'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718013139_AddPaymentBookIssuingBranch', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719003405_AddBankIDToVouchers'
)
BEGIN
    ALTER TABLE [Vouchers] ADD [BankID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719003405_AddBankIDToVouchers'
)
BEGIN
    CREATE INDEX [IX_Vouchers_BankID] ON [Vouchers] ([BankID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719003405_AddBankIDToVouchers'
)
BEGIN
    ALTER TABLE [Vouchers] ADD CONSTRAINT [FK_Vouchers_Banks_BankID] FOREIGN KEY ([BankID]) REFERENCES [Banks] ([BankID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719003405_AddBankIDToVouchers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260719003405_AddBankIDToVouchers', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'SourceModule')
                    BEGIN
                        IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'IX_JournalEntries_SourceModule_SourceReferenceID')
                            DROP INDEX [IX_JournalEntries_SourceModule_SourceReferenceID] ON [JournalEntries];
                        ALTER TABLE [JournalEntries] DROP COLUMN [SourceModule];
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'JournalEntryTypeID')
                        ALTER TABLE [JournalEntries] ADD [JournalEntryTypeID] int NOT NULL DEFAULT 0
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JournalEntryTypes')
                    BEGIN
                        CREATE TABLE [JournalEntryTypes] (
                            [JournalEntryTypeID] int NOT NULL IDENTITY,
                            [TypeName] nvarchar(50) NOT NULL,
                            [TypeNameEn] nvarchar(50) NOT NULL,
                            [Code] nvarchar(20) NULL,
                            [Color] nvarchar(7) NULL,
                            [Description] nvarchar(500) NULL,
                            [IsActive] bit NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [UpdatedAt] datetime2 NULL,
                            [IsDeleted] bit NOT NULL DEFAULT 0,
                            [DeletedAt] datetime2 NULL,
                            [DeletedByUserID] int NULL,
                            [RowVersion] rowversion NULL,
                            CONSTRAINT [PK_JournalEntryTypes] PRIMARY KEY ([JournalEntryTypeID]),
                            CONSTRAINT [FK_JournalEntryTypes_Users_DeletedByUserID] FOREIGN KEY ([DeletedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE SET NULL
                        );
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM [JournalEntryTypes])
                    BEGIN
                        SET IDENTITY_INSERT [JournalEntryTypes] ON;
                        INSERT INTO [JournalEntryTypes] ([TypeName], [TypeNameEn], [Code], [Color], [Description], [IsActive], [CreatedAt], [IsDeleted])
                        VALUES
                        (N'أرصدة افتتاحية', 'Opening Balance', 'OPN', '#7F8C8D', N'قيود بداية السنة المالية والأرصدة المرحلة', 1, GETUTCDATE(), 0),
                        (N'قيد عام', 'General Journal', 'GEN', '#34495E', N'القيود اليدوية والتسويات المحاسبية', 1, GETUTCDATE(), 0),
                        (N'مشتريات', 'Purchases', 'PUR', '#3498DB', N'قيود شراء الأصول والمخزون والخدمات', 1, GETUTCDATE(), 0),
                        (N'رواتب وأجور', 'Payroll', 'SAL', '#2980B9', N'قيود رواتب الموظفين والاستحقاقات', 1, GETUTCDATE(), 0),
                        (N'معاشات تقاعدية', 'Pensions', 'PEN', '#2ECC71', N'قيود صرف المعاشات والمستحقات التقاعدية', 1, GETUTCDATE(), 0),
                        (N'مصروفات', 'Expenses', 'EXP', '#E74C3C', N'المصروفات التشغيلية والإدارية', 1, GETUTCDATE(), 0),
                        (N'إيرادات', 'Revenues', 'REV', '#27AE60', N'الإيرادات والاشتراكات والعوائد', 1, GETUTCDATE(), 0),
                        (N'سند قبض', 'Receipt Voucher', 'REC', '#16A085', N'تحصيل النقدية والبنوك', 1, GETUTCDATE(), 0),
                        (N'سند صرف', 'Payment Voucher', 'PAY', '#8E44AD', N'عمليات الدفع والصرف', 1, GETUTCDATE(), 0),
                        (N'الأصول الثابتة', 'Fixed Assets', 'AST', '#1ABC9C', N'شراء وإضافة ونقل واستبعاد الأصول', 1, GETUTCDATE(), 0),
                        (N'إهلاك الأصول', 'Depreciation', 'DEP', '#E67E22', N'قيود الإهلاك الدوري للأصول', 1, GETUTCDATE(), 0),
                        (N'المخزون', 'Inventory', 'INV', '#16A085', N'حركات وتسويات المخزون', 1, GETUTCDATE(), 0),
                        (N'تسويات محاسبية', 'Adjustments', 'ADJ', '#F39C12', N'قيود التسويات والتصحيح', 1, GETUTCDATE(), 0),
                        (N'إقفال السنة المالية', 'Year Closing', 'CLS', '#9B59B6', N'إقفال الحسابات وترحيل النتائج', 1, GETUTCDATE(), 0),
                        (N'قيود آلية', 'System Generated', 'SYS', '#95A5A6', N'قيود ينشئها النظام تلقائياً', 1, GETUTCDATE(), 0);
                        SET IDENTITY_INSERT [JournalEntryTypes] OFF;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    UPDATE [JournalEntries] SET [JournalEntryTypeID] = (SELECT [JournalEntryTypeID] FROM [JournalEntryTypes] WHERE [Code] = 'OPN')
                    WHERE [JournalEntryTypeID] = 0
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntries') AND name = 'IX_JournalEntries_JournalEntryTypeID')
                        CREATE INDEX [IX_JournalEntries_JournalEntryTypeID] ON [JournalEntries] ([JournalEntryTypeID])
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntryTypes') AND name = 'IX_JournalEntryTypes_Code')
                        CREATE UNIQUE INDEX [IX_JournalEntryTypes_Code] ON [JournalEntryTypes] ([Code]) WHERE [Code] IS NOT NULL
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('JournalEntryTypes') AND name = 'IX_JournalEntryTypes_TypeName')
                        CREATE UNIQUE INDEX [IX_JournalEntryTypes_TypeName] ON [JournalEntryTypes] ([TypeName])
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_JournalEntries_JournalEntryTypes_JournalEntryTypeID')
                        ALTER TABLE [JournalEntries] ADD CONSTRAINT [FK_JournalEntries_JournalEntryTypes_JournalEntryTypeID] FOREIGN KEY ([JournalEntryTypeID]) REFERENCES [JournalEntryTypes] ([JournalEntryTypeID])
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260721001138_FixJournalEntryTypeID'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260721001138_FixJournalEntryTypeID', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChartOfAccounts]') AND [c].[name] = N'HasSubledger');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [ChartOfAccounts] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [ChartOfAccounts] DROP COLUMN [HasSubledger];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChartOfAccounts]') AND [c].[name] = N'SubledgerType');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ChartOfAccounts] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [ChartOfAccounts] DROP COLUMN [SubledgerType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-22T21:24:48.0243243Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-22T21:24:48.0243251Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-22T21:24:48.0243253Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-22T21:24:48.0243254Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-07-22T21:24:48.0243255Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-22T21:24:48.0452851Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-22T21:24:48.0452856Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-22T21:24:48.0452858Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-22T21:24:48.0452859Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-07-22T21:24:48.0452860Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722212450_SyncSubledgerChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260722212450_SyncSubledgerChanges', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TrackingLinks]') AND [c].[name] = N'EntityId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [TrackingLinks] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [TrackingLinks] DROP COLUMN [EntityId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AchievementObstacles]') AND [c].[name] = N'RowVersion');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [AchievementObstacles] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [AchievementObstacles] DROP COLUMN [RowVersion];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    ALTER TABLE [TrackingLinks] ADD [DepartmentID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    ALTER TABLE [TrackingLinks] ADD [EmployeeID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    ALTER TABLE [AchievementObstacles] ADD [EmployeeID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    CREATE INDEX [IX_TrackingLinks_DepartmentID] ON [TrackingLinks] ([DepartmentID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    CREATE INDEX [IX_TrackingLinks_EmployeeID] ON [TrackingLinks] ([EmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    CREATE INDEX [IX_AchievementObstacles_EmployeeID] ON [AchievementObstacles] ([EmployeeID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    ALTER TABLE [AchievementObstacles] ADD CONSTRAINT [FK_AchievementObstacles_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    ALTER TABLE [TrackingLinks] ADD CONSTRAINT [FK_TrackingLinks_Departments_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [Departments] ([DepartmentID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    ALTER TABLE [TrackingLinks] ADD CONSTRAINT [FK_TrackingLinks_Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803001721_AddRealAchievementObstacleRelations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260803001721_AddRealAchievementObstacleRelations', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    ALTER TABLE [Documents] ADD [RejectionReason] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    ALTER TABLE [Documents] ADD [ReviewNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    ALTER TABLE [Documents] ADD [ReviewedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    ALTER TABLE [Documents] ADD [ReviewedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T20:54:53.7078822Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T20:54:53.7078829Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T20:54:53.7078831Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T20:54:53.7078833Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T20:54:53.7078836Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T20:54:53.7288771Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T20:54:53.7288778Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T20:54:53.7288779Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T20:54:53.7288780Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T20:54:53.7288781Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    CREATE INDEX [IX_Documents_ReviewedByUserID] ON [Documents] ([ReviewedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_Users_ReviewedByUserID] FOREIGN KEY ([ReviewedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806205456_AddDocumentReviewFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806205456_AddDocumentReviewFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AchievementObstacles]') AND [c].[name] = N'CompletedAt');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [AchievementObstacles] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [AchievementObstacles] DROP COLUMN [CompletedAt];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AchievementObstacles]') AND [c].[name] = N'DueDate');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [AchievementObstacles] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [AchievementObstacles] DROP COLUMN [DueDate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AchievementObstacles]') AND [c].[name] = N'Priority');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [AchievementObstacles] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [AchievementObstacles] DROP COLUMN [Priority];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:27:49.3920950Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:27:49.3920956Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:27:49.3920958Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:27:49.3920959Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:27:49.3920960Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:27:49.4147960Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:27:49.4147966Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:27:49.4147968Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:27:49.4147969Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:27:49.4147971Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806222752_RemoveAchievementPriorityDueDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806222752_RemoveAchievementPriorityDueDate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AchievementObstacles]') AND [c].[name] = N'Progress');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [AchievementObstacles] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [AchievementObstacles] DROP COLUMN [Progress];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:55:52.3673229Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:55:52.3673237Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:55:52.3673239Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:55:52.3673240Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-06T22:55:52.3673241Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:55:52.3885502Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:55:52.3885526Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:55:52.3885527Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:55:52.3885529Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-06T22:55:52.3885532Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806225555_RemoveAchievementProgress'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806225555_RemoveAchievementProgress', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SettlementItems] ADD [CreatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SettlementItems] ADD [UpdatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SalaryRecipients] ADD [CreatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SalaryRecipients] ADD [UpdatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [RetirementFiles] ADD [CreatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [RetirementFiles] ADD [UpdatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [PensionSuspensions] ADD [UpdatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [PaymentBooks] ADD [CreatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [PaymentBooks] ADD [UpdatedByUserID] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-12T01:41:46.6708069Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-12T01:41:46.6708082Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-12T01:41:46.6708085Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-12T01:41:46.6708088Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-12T01:41:46.6708090Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-12T01:41:46.7188568Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-12T01:41:46.7188574Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-12T01:41:46.7188576Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-12T01:41:46.7188579Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-12T01:41:46.7188585Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_CreatedByUserID] ON [SettlementItems] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_SettlementItems_UpdatedByUserID] ON [SettlementItems] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_CreatedByUserID] ON [SalaryRecipients] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_SalaryRecipients_UpdatedByUserID] ON [SalaryRecipients] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_CreatedByUserID] ON [RetirementFiles] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_RetirementFiles_UpdatedByUserID] ON [RetirementFiles] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_PensionSuspensions_UpdatedByUserID] ON [PensionSuspensions] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_PaymentBooks_CreatedByUserID] ON [PaymentBooks] ([CreatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    CREATE INDEX [IX_PaymentBooks_UpdatedByUserID] ON [PaymentBooks] ([UpdatedByUserID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [PaymentBooks] ADD CONSTRAINT [FK_PaymentBooks_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [PaymentBooks] ADD CONSTRAINT [FK_PaymentBooks_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [PensionSuspensions] ADD CONSTRAINT [FK_PensionSuspensions_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [RetirementFiles] ADD CONSTRAINT [FK_RetirementFiles_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [RetirementFiles] ADD CONSTRAINT [FK_RetirementFiles_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SalaryRecipients] ADD CONSTRAINT [FK_SalaryRecipients_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SalaryRecipients] ADD CONSTRAINT [FK_SalaryRecipients_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SettlementItems] ADD CONSTRAINT [FK_SettlementItems_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    ALTER TABLE [SettlementItems] ADD CONSTRAINT [FK_SettlementItems_Users_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812014151_AddAuditUserColumnsToCoreTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812014151_AddAuditUserColumnsToCoreTables', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-16T01:33:05.5630918Z''
    WHERE [AdministrativeOrderTypeID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-16T01:33:05.5630931Z''
    WHERE [AdministrativeOrderTypeID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-16T01:33:05.5630934Z''
    WHERE [AdministrativeOrderTypeID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-16T01:33:05.5630936Z''
    WHERE [AdministrativeOrderTypeID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [AdministrativeOrderTypes] SET [CreatedAt] = ''2026-08-16T01:33:05.5630938Z''
    WHERE [AdministrativeOrderTypeID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Category', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([PermissionId], [Category], [Code], [Description])
    VALUES (145, N''Report'', N''Report.Pension.View'', N''عرض تقارير المعاشات''),
    (146, N''Report'', N''Report.Financial.View'', N''عرض التقارير المالية''),
    (147, N''Report'', N''Report.Entitlements.View'', N''عرض تقارير الاستحقاقات''),
    (148, N''Report'', N''Report.Documents.View'', N''عرض تقارير الوثائق''),
    (149, N''Report'', N''Report.Export'', N''تصدير التقارير''),
    (150, N''Report'', N''Report.Admin.View'', N''عرض التقارير الإدارية'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Category', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-16T01:33:05.6149981Z''
    WHERE [StatusID] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-16T01:33:05.6149991Z''
    WHERE [StatusID] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-16T01:33:05.6149993Z''
    WHERE [StatusID] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-16T01:33:05.6149995Z''
    WHERE [StatusID] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    EXEC(N'UPDATE [TransactionStatuses] SET [CreatedAt] = ''2026-08-16T01:33:05.6149996Z''
    WHERE [StatusID] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816013310_AddReportPermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260816013310_AddReportPermissions', N'8.0.11');
END;
GO

COMMIT;
GO

