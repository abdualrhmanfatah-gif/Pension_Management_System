using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    BankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.BankID);
                });

            migrationBuilder.CreateTable(
                name: "FileStatuses",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatusColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "HealthStatuses",
                columns: table => new
                {
                    HealthStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthStatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthStatuses", x => x.HealthStatusID);
                });

            migrationBuilder.CreateTable(
                name: "ImmutableAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    ActorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImmutableAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PensionFundBranches",
                columns: table => new
                {
                    PensionBranchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentBranchId = table.Column<int>(type: "int", nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ManagerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionFundBranches", x => x.PensionBranchID);
                    table.ForeignKey(
                        name: "FK_PensionFundBranches_PensionFundBranches_ParentBranchId",
                        column: x => x.ParentBranchId,
                        principalTable: "PensionFundBranches",
                        principalColumn: "PensionBranchID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "Ranks",
                columns: table => new
                {
                    RankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RankLevel = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ranks", x => x.RankID);
                });

            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    RelationshipID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelationshipName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => x.RelationshipID);
                });

            migrationBuilder.CreateTable(
                name: "RetireeCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetireeCategories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "SocialStatuses",
                columns: table => new
                {
                    SocialStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialStatuses", x => x.SocialStatusID);
                });

            migrationBuilder.CreateTable(
                name: "SystemTables",
                columns: table => new
                {
                    TableID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemTables", x => x.TableID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatuses",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatusColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RolePermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_PensionFundBranches_BranchID",
                        column: x => x.BranchID,
                        principalTable: "PensionFundBranches",
                        principalColumn: "PensionBranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerTableID = table.Column<int>(type: "int", nullable: true),
                    OwnerType = table.Column<int>(type: "int", nullable: true),
                    TypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    HasExpiry = table.Column<bool>(type: "bit", nullable: false),
                    MaxFileSize = table.Column<int>(type: "int", nullable: true),
                    AllowedExtensions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeID);
                    table.ForeignKey(
                        name: "FK_DocumentTypes_SystemTables_OwnerTableID",
                        column: x => x.OwnerTableID,
                        principalTable: "SystemTables",
                        principalColumn: "TableID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TablePermissions",
                columns: table => new
                {
                    PermissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    TableID = table.Column<int>(type: "int", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TablePermissions", x => x.PermissionID);
                    table.ForeignKey(
                        name: "FK_TablePermissions_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TablePermissions_SystemTables_TableID",
                        column: x => x.TableID,
                        principalTable: "SystemTables",
                        principalColumn: "TableID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdministrativeOrderTypes",
                columns: table => new
                {
                    AdministrativeOrderTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    GeneratesServiceHistory = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeOrderTypes", x => x.AdministrativeOrderTypeID);
                    table.ForeignKey(
                        name: "FK_AdministrativeOrderTypes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalRequests",
                columns: table => new
                {
                    ApprovalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityID = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserID = table.Column<int>(type: "int", nullable: false),
                    WorkflowStage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DecisionHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRequests", x => x.ApprovalID);
                    table.ForeignKey(
                        name: "FK_ApprovalRequests_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ApprovalRequests_Users_RequestedByUserID",
                        column: x => x.RequestedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    TableID = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordID = table.Column<int>(type: "int", nullable: true),
                    EntityKey = table.Column<int>(type: "int", nullable: true),
                    EntityDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_AuditLogs_SystemTables_TableID",
                        column: x => x.TableID,
                        principalTable: "SystemTables",
                        principalColumn: "TableID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetChapters",
                columns: table => new
                {
                    ChapterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetChapters", x => x.ChapterID);
                    table.ForeignKey(
                        name: "FK_BudgetChapters_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CorrespondenceTypes",
                columns: table => new
                {
                    CorrespondenceTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrespondenceTypes", x => x.CorrespondenceTypeID);
                    table.ForeignKey(
                        name: "FK_CorrespondenceTypes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK_Departments_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeviceRegistrations",
                columns: table => new
                {
                    DeviceID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastKnownIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceRegistrations", x => x.DeviceID);
                    table.ForeignKey(
                        name: "FK_DeviceRegistrations_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentStatuses",
                columns: table => new
                {
                    EmploymentStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsInService = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentStatuses", x => x.EmploymentStatusID);
                    table.ForeignKey(
                        name: "FK_EmploymentStatuses_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExternalParties",
                columns: table => new
                {
                    ExternalPartyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PartyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalParties", x => x.ExternalPartyID);
                    table.ForeignKey(
                        name: "FK_ExternalParties_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FiscalYears",
                columns: table => new
                {
                    FiscalYearID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYears", x => x.FiscalYearID);
                    table.ForeignKey(
                        name: "FK_FiscalYears_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCategories",
                columns: table => new
                {
                    InventoryCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCategories", x => x.InventoryCategoryID);
                    table.ForeignKey(
                        name: "FK_InventoryCategories_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCategories_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentPortfolios",
                columns: table => new
                {
                    PortfolioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ManagerUserID = table.Column<int>(type: "int", nullable: false),
                    TotalTargetCapital = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentPortfolios", x => x.PortfolioID);
                    table.CheckConstraint("CK_Portfolio_TargetCapital", "TotalTargetCapital >= 0");
                    table.ForeignKey(
                        name: "FK_InvestmentPortfolios_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InvestmentPortfolios_Users_ManagerUserID",
                        column: x => x.ManagerUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentTypes", x => x.TypeID);
                    table.ForeignKey(
                        name: "FK_InvestmentTypes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JobTitles",
                columns: table => new
                {
                    JobTitleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTitles", x => x.JobTitleID);
                    table.ForeignKey(
                        name: "FK_JobTitles_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    LeaveTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    CountsTowardsService = table.Column<bool>(type: "bit", nullable: false),
                    MaxDaysPerRequest = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.LeaveTypeID);
                    table.ForeignKey(
                        name: "FK_LeaveTypes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LoginHistories",
                columns: table => new
                {
                    LoginID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    AttemptedUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginHistories", x => x.LoginID);
                    table.ForeignKey(
                        name: "FK_LoginHistories_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MinistryApprovalBatches",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportedByUserID = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryApprovalBatches", x => x.BatchID);
                    table.ForeignKey(
                        name: "FK_MinistryApprovalBatches_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MinistryApprovalBatches_Users_ImportedByUserID",
                        column: x => x.ImportedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RelatedTableID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedRecordID = table.Column<int>(type: "int", nullable: true),
                    ActorID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_SystemTables_RelatedTableID",
                        column: x => x.RelatedTableID,
                        principalTable: "SystemTables",
                        principalColumn: "TableID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_ActorID",
                        column: x => x.ActorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyTypes", x => x.TypeID);
                    table.ForeignKey(
                        name: "FK_PropertyTypes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueryTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK_ReportTemplates_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryPaymentBatches",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankID = table.Column<int>(type: "int", nullable: false),
                    ImportedByUserID = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserID = table.Column<int>(type: "int", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryPaymentBatches", x => x.BatchID);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentBatches_Banks_BankID",
                        column: x => x.BankID,
                        principalTable: "Banks",
                        principalColumn: "BankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentBatches_Users_ApprovedByUserID",
                        column: x => x.ApprovedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentBatches_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentBatches_Users_ImportedByUserID",
                        column: x => x.ImportedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementDecisions",
                columns: table => new
                {
                    DecisionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    DecisionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementDecisions", x => x.DecisionID);
                    table.ForeignKey(
                        name: "FK_SettlementDecisions_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementDecisions_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SettlementsBatches",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    SourceRowsCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    FailureReportJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementsBatches", x => x.BatchID);
                    table.ForeignKey(
                        name: "FK_SettlementsBatches_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementsBatches_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StatusTransitionLogs",
                columns: table => new
                {
                    TransitionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableID = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserID = table.Column<int>(type: "int", nullable: true),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    OldStatusID = table.Column<int>(type: "int", nullable: true),
                    NewStatusID = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransitionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusTransitionLogs", x => x.TransitionID);
                    table.ForeignKey(
                        name: "FK_StatusTransitionLogs_SystemTables_TableID",
                        column: x => x.TableID,
                        principalTable: "SystemTables",
                        principalColumn: "TableID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StatusTransitionLogs_Users_ChangedByUserID",
                        column: x => x.ChangedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SettingID);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenantType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NationalIDOrCR = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RatingScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantID);
                    table.CheckConstraint("CK_Tenant_Rating", "RatingScore >= 0 AND RatingScore <= 5");
                    table.ForeignKey(
                        name: "FK_Tenants_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Treasuries",
                columns: table => new
                {
                    TreasuryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BankBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treasuries", x => x.TreasuryID);
                    table.ForeignKey(
                        name: "FK_Treasuries_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UnitTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTypes", x => x.TypeID);
                    table.ForeignKey(
                        name: "FK_UnitTypes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_PensionFundBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "PensionFundBranches",
                        principalColumn: "PensionBranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_PensionFundBranches_RegionId",
                        column: x => x.RegionId,
                        principalTable: "PensionFundBranches",
                        principalColumn: "PensionBranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentRequirementRules",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false),
                    TargetEntity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConditionField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConditionValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRequirementRules", x => x.RuleID);
                    table.ForeignKey(
                        name: "FK_DocumentRequirementRules_DocumentTypes_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetSections",
                columns: table => new
                {
                    SectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChapterID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetSections", x => x.SectionID);
                    table.ForeignKey(
                        name: "FK_BudgetSections_BudgetChapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "BudgetChapters",
                        principalColumn: "ChapterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetSections_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionID);
                    table.ForeignKey(
                        name: "FK_Sections_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sections_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SyncQueues",
                columns: table => new
                {
                    SyncID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TableID = table.Column<int>(type: "int", nullable: false),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncQueues", x => x.SyncID);
                    table.ForeignKey(
                        name: "FK_SyncQueues_DeviceRegistrations_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "DeviceRegistrations",
                        principalColumn: "DeviceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncQueues_SystemTables_TableID",
                        column: x => x.TableID,
                        principalTable: "SystemTables",
                        principalColumn: "TableID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncQueues_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quarters",
                columns: table => new
                {
                    QuarterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearID = table.Column<int>(type: "int", nullable: false),
                    QuarterNumber = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TreasuryAllocated = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quarters", x => x.QuarterID);
                    table.ForeignKey(
                        name: "FK_Quarters_FiscalYears_FiscalYearID",
                        column: x => x.FiscalYearID,
                        principalTable: "FiscalYears",
                        principalColumn: "FiscalYearID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quarters_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    InventoryItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MinimumQuantity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Condition = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.InventoryItemID);
                    table.ForeignKey(
                        name: "FK_InventoryItems_InventoryCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "InventoryCategories",
                        principalColumn: "InventoryCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentProjects",
                columns: table => new
                {
                    ProjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioID = table.Column<int>(type: "int", nullable: false),
                    PensionBranchID = table.Column<int>(type: "int", nullable: false),
                    TypeID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FeasibilityStudyPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoardApprovalDocPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlannedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedROI = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentProjects", x => x.ProjectID);
                    table.CheckConstraint("CK_Project_Budget", "PlannedBudget >= 0");
                    table.CheckConstraint("CK_Project_Dates", "EndDate IS NULL OR EndDate > StartDate");
                    table.ForeignKey(
                        name: "FK_InvestmentProjects_InvestmentPortfolios_PortfolioID",
                        column: x => x.PortfolioID,
                        principalTable: "InvestmentPortfolios",
                        principalColumn: "PortfolioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentProjects_InvestmentTypes_TypeID",
                        column: x => x.TypeID,
                        principalTable: "InvestmentTypes",
                        principalColumn: "TypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentProjects_PensionFundBranches_PensionBranchID",
                        column: x => x.PensionBranchID,
                        principalTable: "PensionFundBranches",
                        principalColumn: "PensionBranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentProjects_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MinistryApprovalItems",
                columns: table => new
                {
                    ApprovalItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchID = table.Column<int>(type: "int", nullable: false),
                    RankID = table.Column<int>(type: "int", nullable: true),
                    ProcessedByUserID = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<int>(type: "int", nullable: false),
                    MilitaryNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    UnitName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetirementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: true),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryApprovalItems", x => x.ApprovalItemID);
                    table.ForeignKey(
                        name: "FK_MinistryApprovalItems_MinistryApprovalBatches_BatchID",
                        column: x => x.BatchID,
                        principalTable: "MinistryApprovalBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MinistryApprovalItems_Ranks_RankID",
                        column: x => x.RankID,
                        principalTable: "Ranks",
                        principalColumn: "RankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MinistryApprovalItems_RetireeCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "RetireeCategories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MinistryApprovalItems_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MinistryApprovalItems_Users_ProcessedByUserID",
                        column: x => x.ProcessedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationAttachments",
                columns: table => new
                {
                    AttachmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationID = table.Column<long>(type: "bigint", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationAttachments", x => x.AttachmentID);
                    table.ForeignKey(
                        name: "FK_NotificationAttachments_Notifications_NotificationID",
                        column: x => x.NotificationID,
                        principalTable: "Notifications",
                        principalColumn: "NotificationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationAttachments_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NotificationOutbox",
                columns: table => new
                {
                    OutboxId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    Channel = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RetryCount = table.Column<byte>(type: "tinyint", nullable: false),
                    MaxRetries = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationOutbox", x => x.OutboxId);
                    table.ForeignKey(
                        name: "FK_NotificationOutbox_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SavedReports",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    GeneratedByUserID = table.Column<int>(type: "int", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportTemplateTemplateID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedReports", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK_SavedReports_ReportTemplates_ReportTemplateTemplateID",
                        column: x => x.ReportTemplateTemplateID,
                        principalTable: "ReportTemplates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavedReports_ReportTemplates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "ReportTemplates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavedReports_Users_GeneratedByUserID",
                        column: x => x.GeneratedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreasuryID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceID = table.Column<int>(type: "int", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryTransactions", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_Treasuries_TreasuryID",
                        column: x => x.TreasuryID,
                        principalTable: "Treasuries",
                        principalColumn: "TreasuryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BudgetItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_BudgetItems_BudgetSections_SectionID",
                        column: x => x.SectionID,
                        principalTable: "BudgetSections",
                        principalColumn: "SectionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetItems_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    SectionID = table.Column<int>(type: "int", nullable: true),
                    JobTitleID = table.Column<int>(type: "int", nullable: false),
                    EmploymentStatusID = table.Column<int>(type: "int", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthPlace = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MilitaryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RankID = table.Column<int>(type: "int", nullable: true),
                    MilitaryServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JobPositions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AcademicQualifications = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProfilePhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BloodGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NationalIDIssuePlace = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NationalIDIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NationalIDExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_EmploymentStatuses_EmploymentStatusID",
                        column: x => x.EmploymentStatusID,
                        principalTable: "EmploymentStatuses",
                        principalColumn: "EmploymentStatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_JobTitles_JobTitleID",
                        column: x => x.JobTitleID,
                        principalTable: "JobTitles",
                        principalColumn: "JobTitleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Ranks_RankID",
                        column: x => x.RankID,
                        principalTable: "Ranks",
                        principalColumn: "RankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Sections_SectionID",
                        column: x => x.SectionID,
                        principalTable: "Sections",
                        principalColumn: "SectionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryIncomes",
                columns: table => new
                {
                    IncomeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreasuryID = table.Column<int>(type: "int", nullable: false),
                    QuarterID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncomeSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncomeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryIncomes", x => x.IncomeID);
                    table.ForeignKey(
                        name: "FK_TreasuryIncomes_Quarters_QuarterID",
                        column: x => x.QuarterID,
                        principalTable: "Quarters",
                        principalColumn: "QuarterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreasuryIncomes_Treasuries_TreasuryID",
                        column: x => x.TreasuryID,
                        principalTable: "Treasuries",
                        principalColumn: "TreasuryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreasuryIncomes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    InventoryTransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemID = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PreviousQuantity = table.Column<int>(type: "int", nullable: false),
                    NewQuantity = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "SAR"),
                    WithdrawalOrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PerformedByUserID = table.Column<int>(type: "int", nullable: true),
                    BatchID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.InventoryTransactionID);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_InventoryItems_InventoryItemID",
                        column: x => x.InventoryItemID,
                        principalTable: "InventoryItems",
                        principalColumn: "InventoryItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Users_PerformedByUserID",
                        column: x => x.PerformedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentPlans",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    FiscalYearID = table.Column<int>(type: "int", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualSpent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentPlans", x => x.PlanID);
                    table.CheckConstraint("CK_Plan_Amounts", "PlannedAmount >= 0 AND ActualSpent >= 0");
                    table.ForeignKey(
                        name: "FK_InvestmentPlans_FiscalYears_FiscalYearID",
                        column: x => x.FiscalYearID,
                        principalTable: "FiscalYears",
                        principalColumn: "FiscalYearID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentPlans_InvestmentProjects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "InvestmentProjects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentPlans_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentProperties",
                columns: table => new
                {
                    PropertyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: true),
                    PensionBranchID = table.Column<int>(type: "int", nullable: false),
                    PropertyTypeID = table.Column<int>(type: "int", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LocationCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    GoogleMapsUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnershipDeedPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalArea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TitleDeedNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentProperties", x => x.PropertyID);
                    table.CheckConstraint("CK_Property_AreaValue", "TotalArea > 0 AND InitialValue >= 0");
                    table.ForeignKey(
                        name: "FK_InvestmentProperties_InvestmentProjects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "InvestmentProjects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentProperties_PensionFundBranches_PensionBranchID",
                        column: x => x.PensionBranchID,
                        principalTable: "PensionFundBranches",
                        principalColumn: "PensionBranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentProperties_PropertyTypes_PropertyTypeID",
                        column: x => x.PropertyTypeID,
                        principalTable: "PropertyTypes",
                        principalColumn: "TypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentProperties_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessments",
                columns: table => new
                {
                    RiskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    RiskType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RiskDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    MitigationPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AssessedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessments", x => x.RiskID);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_InvestmentProjects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "InvestmentProjects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Retirees",
                columns: table => new
                {
                    RetireeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SocialStatusID = table.Column<int>(type: "int", nullable: true),
                    HealthStatusID = table.Column<int>(type: "int", nullable: true),
                    ApprovalItemID = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PersonalPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MilitaryNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LinkNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PendingReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retirees", x => x.RetireeID);
                    table.ForeignKey(
                        name: "FK_Retirees_HealthStatuses_HealthStatusID",
                        column: x => x.HealthStatusID,
                        principalTable: "HealthStatuses",
                        principalColumn: "HealthStatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Retirees_MinistryApprovalItems_ApprovalItemID",
                        column: x => x.ApprovalItemID,
                        principalTable: "MinistryApprovalItems",
                        principalColumn: "ApprovalItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Retirees_SocialStatuses_SocialStatusID",
                        column: x => x.SocialStatusID,
                        principalTable: "SocialStatuses",
                        principalColumn: "SocialStatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Retirees_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Retirees_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Retirees_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetAllocations",
                columns: table => new
                {
                    AllocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuarterID = table.Column<int>(type: "int", nullable: false),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    AmountAllocated = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetAllocations", x => x.AllocationID);
                    table.ForeignKey(
                        name: "FK_BudgetAllocations_BudgetItems_ItemID",
                        column: x => x.ItemID,
                        principalTable: "BudgetItems",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocations_Quarters_QuarterID",
                        column: x => x.QuarterID,
                        principalTable: "Quarters",
                        principalColumn: "QuarterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocations_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AdministrativeOrders",
                columns: table => new
                {
                    AdministrativeOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    AdministrativeOrderTypeID = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserID = table.Column<int>(type: "int", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeOrders", x => x.AdministrativeOrderID);
                    table.ForeignKey(
                        name: "FK_AdministrativeOrders_AdministrativeOrderTypes_AdministrativeOrderTypeID",
                        column: x => x.AdministrativeOrderTypeID,
                        principalTable: "AdministrativeOrderTypes",
                        principalColumn: "AdministrativeOrderTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdministrativeOrders_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdministrativeOrders_Users_ApprovedByUserID",
                        column: x => x.ApprovedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdministrativeOrders_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceEvents",
                columns: table => new
                {
                    AttendanceEventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsManualEntry = table.Column<bool>(type: "bit", nullable: false),
                    RecordedByUserID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceEvents", x => x.AttendanceEventID);
                    table.ForeignKey(
                        name: "FK_AttendanceEvents_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceEvents_Users_RecordedByUserID",
                        column: x => x.RecordedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Correspondences",
                columns: table => new
                {
                    CorrespondenceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    CorrespondenceTypeID = table.Column<int>(type: "int", nullable: false),
                    SequentialNumber = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CorrespondenceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PageNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalPartyID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correspondences", x => x.CorrespondenceID);
                    table.ForeignKey(
                        name: "FK_Correspondences_CorrespondenceTypes_CorrespondenceTypeID",
                        column: x => x.CorrespondenceTypeID,
                        principalTable: "CorrespondenceTypes",
                        principalColumn: "CorrespondenceTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Correspondences_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Correspondences_ExternalParties_ExternalPartyID",
                        column: x => x.ExternalPartyID,
                        principalTable: "ExternalParties",
                        principalColumn: "ExternalPartyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Correspondences_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Correspondences_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCustodies",
                columns: table => new
                {
                    InventoryCustodyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AssignedByUserID = table.Column<int>(type: "int", nullable: true),
                    ReturnedToUserID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCustodies", x => x.InventoryCustodyID);
                    table.ForeignKey(
                        name: "FK_InventoryCustodies_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCustodies_InventoryItems_InventoryItemID",
                        column: x => x.InventoryItemID,
                        principalTable: "InventoryItems",
                        principalColumn: "InventoryItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCustodies_Users_AssignedByUserID",
                        column: x => x.AssignedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCustodies_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCustodies_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryCustodies_Users_ReturnedToUserID",
                        column: x => x.ReturnedToUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCustodies_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    LeaveRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovedByUserID = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.LeaveRequestID);
                    table.CheckConstraint("CK_LeaveRequests_DateRange", "[EndDate] >= [StartDate]");
                    table.CheckConstraint("CK_LeaveRequests_Days", "[RequestedDays] >= 0");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeID",
                        column: x => x.LeaveTypeID,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_ApprovedByUserID",
                        column: x => x.ApprovedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ServiceHistoryEvents",
                columns: table => new
                {
                    ServiceHistoryEventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SourceEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceEntityId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHistoryEvents", x => x.ServiceHistoryEventID);
                    table.ForeignKey(
                        name: "FK_ServiceHistoryEvents_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetValuations",
                columns: table => new
                {
                    ValuationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyID = table.Column<int>(type: "int", nullable: false),
                    ValuationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MarketValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValuedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValuationReportPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetValuations", x => x.ValuationID);
                    table.ForeignKey(
                        name: "FK_AssetValuations_InvestmentProperties_PropertyID",
                        column: x => x.PropertyID,
                        principalTable: "InvestmentProperties",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetValuations_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DepreciationRecords",
                columns: table => new
                {
                    DeprecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyID = table.Column<int>(type: "int", nullable: false),
                    FiscalYearID = table.Column<int>(type: "int", nullable: false),
                    DepreciationRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepreciationAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetBookValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepreciationRecords", x => x.DeprecID);
                    table.CheckConstraint("CK_Depreciation_Rate", "DepreciationRate >= 0 AND DepreciationRate <= 100");
                    table.ForeignKey(
                        name: "FK_DepreciationRecords_FiscalYears_FiscalYearID",
                        column: x => x.FiscalYearID,
                        principalTable: "FiscalYears",
                        principalColumn: "FiscalYearID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepreciationRecords_InvestmentProperties_PropertyID",
                        column: x => x.PropertyID,
                        principalTable: "InvestmentProperties",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepreciationRecords_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PropertyUnits",
                columns: table => new
                {
                    UnitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyID = table.Column<int>(type: "int", nullable: false),
                    UnitTypeID = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Area = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MainPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayoutPlanPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyUnits", x => x.UnitID);
                    table.CheckConstraint("CK_Unit_Area", "Area > 0");
                    table.ForeignKey(
                        name: "FK_PropertyUnits_InvestmentProperties_PropertyID",
                        column: x => x.PropertyID,
                        principalTable: "InvestmentProperties",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyUnits_UnitTypes_UnitTypeID",
                        column: x => x.UnitTypeID,
                        principalTable: "UnitTypes",
                        principalColumn: "TypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyUnits_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CitizenAccounts",
                columns: table => new
                {
                    CitizenAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetireeID = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedOtpAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitizenAccounts", x => x.CitizenAccountID);
                    table.ForeignKey(
                        name: "FK_CitizenAccounts_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CitizenAccounts_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CitizenNotifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetireeID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitizenNotifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_CitizenNotifications_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialEntitlements",
                columns: table => new
                {
                    EntitlementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetireeID = table.Column<int>(type: "int", nullable: false),
                    BatchID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialEntitlements", x => x.EntitlementID);
                    table.ForeignKey(
                        name: "FK_FinancialEntitlements_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialEntitlements_SettlementsBatches_BatchID",
                        column: x => x.BatchID,
                        principalTable: "SettlementsBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialEntitlements_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RetirementFiles",
                columns: table => new
                {
                    FileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetireeID = table.Column<int>(type: "int", nullable: false),
                    PensionBranchID = table.Column<int>(type: "int", nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: true),
                    PreviousStatusID = table.Column<int>(type: "int", nullable: true),
                    RankID = table.Column<int>(type: "int", nullable: true),
                    AssignedEmployeeID = table.Column<int>(type: "int", nullable: true),
                    FileNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MissingDocumentsCount = table.Column<int>(type: "int", nullable: false),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileOpenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetirementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReviewRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastReviewedFileState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetirementFiles", x => x.FileID);
                    table.ForeignKey(
                        name: "FK_RetirementFiles_FileStatuses_StatusID",
                        column: x => x.StatusID,
                        principalTable: "FileStatuses",
                        principalColumn: "StatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetirementFiles_PensionFundBranches_PensionBranchID",
                        column: x => x.PensionBranchID,
                        principalTable: "PensionFundBranches",
                        principalColumn: "PensionBranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetirementFiles_Ranks_RankID",
                        column: x => x.RankID,
                        principalTable: "Ranks",
                        principalColumn: "RankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetirementFiles_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetirementFiles_Users_AssignedEmployeeID",
                        column: x => x.AssignedEmployeeID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetirementFiles_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SettlementItems",
                columns: table => new
                {
                    SettlementItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchID = table.Column<int>(type: "int", nullable: false),
                    ApprovalItemID = table.Column<int>(type: "int", nullable: true),
                    MilitaryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RetireeID = table.Column<int>(type: "int", nullable: true),
                    DecisionID = table.Column<int>(type: "int", nullable: true),
                    CurrentRankID = table.Column<int>(type: "int", nullable: false),
                    ProcessedRankID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SerialNumber = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementItems", x => x.SettlementItemID);
                    table.ForeignKey(
                        name: "FK_SettlementItems_MinistryApprovalItems_ApprovalItemID",
                        column: x => x.ApprovalItemID,
                        principalTable: "MinistryApprovalItems",
                        principalColumn: "ApprovalItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementItems_Ranks_CurrentRankID",
                        column: x => x.CurrentRankID,
                        principalTable: "Ranks",
                        principalColumn: "RankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementItems_Ranks_ProcessedRankID",
                        column: x => x.ProcessedRankID,
                        principalTable: "Ranks",
                        principalColumn: "RankID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementItems_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementItems_SettlementDecisions_DecisionID",
                        column: x => x.DecisionID,
                        principalTable: "SettlementDecisions",
                        principalColumn: "DecisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementItems_SettlementsBatches_BatchID",
                        column: x => x.BatchID,
                        principalTable: "SettlementsBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementItems_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OperationalExpenses",
                columns: table => new
                {
                    ExpenseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetAllocationID = table.Column<int>(type: "int", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RecipientWorkplace = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DocumentCount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalChequeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DirectiveSource = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpenseStatement = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SignedOrderPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportingDocsPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalExpenses", x => x.ExpenseID);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_BudgetAllocations_BudgetAllocationID",
                        column: x => x.BudgetAllocationID,
                        principalTable: "BudgetAllocations",
                        principalColumn: "AllocationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LeaseContracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitID = table.Column<int>(type: "int", nullable: false),
                    TenantID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalContractValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RenewalTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignedContractPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseContracts", x => x.ContractID);
                    table.CheckConstraint("CK_Lease_Dates", "EndDate > StartDate");
                    table.CheckConstraint("CK_Lease_Value", "TotalContractValue >= 0");
                    table.ForeignKey(
                        name: "FK_LeaseContracts_PropertyUnits_UnitID",
                        column: x => x.UnitID,
                        principalTable: "PropertyUnits",
                        principalColumn: "UnitID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseContracts_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseContracts_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRequests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyID = table.Column<int>(type: "int", nullable: false),
                    UnitID = table.Column<int>(type: "int", nullable: true),
                    TenantID = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DamagePhotosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReportedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_InvestmentProperties_PropertyID",
                        column: x => x.PropertyID,
                        principalTable: "InvestmentProperties",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_PropertyUnits_UnitID",
                        column: x => x.UnitID,
                        principalTable: "PropertyUnits",
                        principalColumn: "UnitID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OtpVerifications",
                columns: table => new
                {
                    OtpVerificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenAccountID = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OtpHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerifications", x => x.OtpVerificationID);
                    table.ForeignKey(
                        name: "FK_OtpVerifications_CitizenAccounts_CitizenAccountID",
                        column: x => x.CitizenAccountID,
                        principalTable: "CitizenAccounts",
                        principalColumn: "CitizenAccountID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    SupportTicketID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenAccountID = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserID = table.Column<int>(type: "int", nullable: true),
                    TicketNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.SupportTicketID);
                    table.ForeignKey(
                        name: "FK_SupportTickets_CitizenAccounts_CitizenAccountID",
                        column: x => x.CitizenAccountID,
                        principalTable: "CitizenAccounts",
                        principalColumn: "CitizenAccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_AssignedToUserID",
                        column: x => x.AssignedToUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    AgentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PersonalPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AgencyNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AgencyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgencyEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PendingReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.AgentID);
                    table.ForeignKey(
                        name: "FK_Agents_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agents_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agents_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Agents_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentExemptions",
                columns: table => new
                {
                    ExemptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    DirectiveSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExemptionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentExemptions", x => x.ExemptionID);
                    table.ForeignKey(
                        name: "FK_DocumentExemptions_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentExemptions_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentExemptions_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EntitledPersons",
                columns: table => new
                {
                    EntitledID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    RelationshipID = table.Column<int>(type: "int", nullable: false),
                    SocialStatusID = table.Column<int>(type: "int", nullable: true),
                    HealthStatusID = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PersonalPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StopDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StopReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StopCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeceased = table.Column<bool>(type: "bit", nullable: false),
                    DeceasedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasDisabilityException = table.Column<bool>(type: "bit", nullable: false),
                    PendingReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntitledPersons", x => x.EntitledID);
                    table.ForeignKey(
                        name: "FK_EntitledPersons_HealthStatuses_HealthStatusID",
                        column: x => x.HealthStatusID,
                        principalTable: "HealthStatuses",
                        principalColumn: "HealthStatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntitledPersons_Relationships_RelationshipID",
                        column: x => x.RelationshipID,
                        principalTable: "Relationships",
                        principalColumn: "RelationshipID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntitledPersons_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntitledPersons_SocialStatuses_SocialStatusID",
                        column: x => x.SocialStatusID,
                        principalTable: "SocialStatuses",
                        principalColumn: "SocialStatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntitledPersons_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntitledPersons_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EntitledPersons_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileNotes",
                columns: table => new
                {
                    NoteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    ResolvedByUserID = table.Column<int>(type: "int", nullable: true),
                    NoteText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileNotes", x => x.NoteID);
                    table.ForeignKey(
                        name: "FK_FileNotes_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileNotes_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileNotes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FileNotes_Users_ResolvedByUserID",
                        column: x => x.ResolvedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileReviewItems",
                columns: table => new
                {
                    ItemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetirementFileID = table.Column<int>(type: "int", nullable: false),
                    AuditLogID = table.Column<long>(type: "bigint", nullable: true),
                    EntityType = table.Column<int>(type: "int", nullable: true),
                    EntityID = table.Column<int>(type: "int", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByUserID = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileReviewItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_FileReviewItems_AuditLogs_AuditLogID",
                        column: x => x.AuditLogID,
                        principalTable: "AuditLogs",
                        principalColumn: "LogID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileReviewItems_RetirementFiles_RetirementFileID",
                        column: x => x.RetirementFileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileReviewItems_Users_ReviewedByUserID",
                        column: x => x.ReviewedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SnapshotType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonDiff = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileSnapshots_RetirementFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentBooks",
                columns: table => new
                {
                    PaymentBookID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    BookNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BookType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LegacyColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecipientType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PendingReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentBooks", x => x.PaymentBookID);
                    table.ForeignKey(
                        name: "FK_PaymentBooks_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentBooks_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PendingTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: true),
                    SalaryBatchID = table.Column<int>(type: "int", nullable: true),
                    TaskType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedToRoleId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingTasks_RetirementFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingTasks_Roles_AssignedToRoleId",
                        column: x => x.AssignedToRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingTasks_SalaryPaymentBatches_SalaryBatchID",
                        column: x => x.SalaryBatchID,
                        principalTable: "SalaryPaymentBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingTasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingTasks_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PensionSuspensions",
                columns: table => new
                {
                    SuspensionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    EndedByUserID = table.Column<int>(type: "int", nullable: true),
                    SuspensionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SuspensionReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuspensionSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PensionSuspensions", x => x.SuspensionID);
                    table.ForeignKey(
                        name: "FK_PensionSuspensions_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PensionSuspensions_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PensionSuspensions_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PensionSuspensions_Users_EndedByUserID",
                        column: x => x.EndedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    RiskType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<int>(type: "int", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskEvents_RetirementFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskEvents_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenAccountID = table.Column<int>(type: "int", nullable: false),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    ProcessedByUserID = table.Column<int>(type: "int", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CitizenNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_CitizenAccounts_CitizenAccountID",
                        column: x => x.CitizenAccountID,
                        principalTable: "CitizenAccounts",
                        principalColumn: "CitizenAccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_ProcessedByUserID",
                        column: x => x.ProcessedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractID = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSchedules", x => x.ScheduleID);
                    table.CheckConstraint("CK_Payment_Amounts", "PaidAmount <= ExpectedAmount AND ExpectedAmount > 0");
                    table.ForeignKey(
                        name: "FK_PaymentSchedules_LeaseContracts_ContractID",
                        column: x => x.ContractID,
                        principalTable: "LeaseContracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentSchedules_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    WorkOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestID = table.Column<int>(type: "int", nullable: false),
                    AssignedContractor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionCertPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.WorkOrderID);
                    table.CheckConstraint("CK_WorkOrder_Costs", "EstimatedCost >= 0 AND ActualCost >= 0");
                    table.ForeignKey(
                        name: "FK_WorkOrders_MaintenanceRequests_RequestID",
                        column: x => x.RequestID,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "RequestID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    TicketMessageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketID = table.Column<int>(type: "int", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SenderUserID = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.TicketMessageID);
                    table.ForeignKey(
                        name: "FK_TicketMessages_SupportTickets_TicketID",
                        column: x => x.TicketID,
                        principalTable: "SupportTickets",
                        principalColumn: "SupportTicketID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketMessages_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Guardians",
                columns: table => new
                {
                    GuardianID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NationalID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PersonalPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GuardianshipDocNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PendingReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    EntitledPersonEntitledID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guardians", x => x.GuardianID);
                    table.ForeignKey(
                        name: "FK_Guardians_EntitledPersons_EntitledPersonEntitledID",
                        column: x => x.EntitledPersonEntitledID,
                        principalTable: "EntitledPersons",
                        principalColumn: "EntitledID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guardians_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guardians_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guardians_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Guardians_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPledges",
                columns: table => new
                {
                    PledgeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    SuspensionID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    ExtendedByUserID = table.Column<int>(type: "int", nullable: true),
                    PledgeNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PledgeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FulfilledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PledgedByType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PledgedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExtended = table.Column<bool>(type: "bit", nullable: false),
                    ExtendedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtensionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPledges", x => x.PledgeID);
                    table.ForeignKey(
                        name: "FK_DocumentPledges_PensionSuspensions_SuspensionID",
                        column: x => x.SuspensionID,
                        principalTable: "PensionSuspensions",
                        principalColumn: "SuspensionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentPledges_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentPledges_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentPledges_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumentPledges_Users_ExtendedByUserID",
                        column: x => x.ExtendedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuardianEntitledPersons",
                columns: table => new
                {
                    GuardianID = table.Column<int>(type: "int", nullable: false),
                    EntitledID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuardianEntitledPersons", x => new { x.GuardianID, x.EntitledID });
                    table.ForeignKey(
                        name: "FK_GuardianEntitledPersons_EntitledPersons_EntitledID",
                        column: x => x.EntitledID,
                        principalTable: "EntitledPersons",
                        principalColumn: "EntitledID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuardianEntitledPersons_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuardianEntitledPersons_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SalaryRecipients",
                columns: table => new
                {
                    RecipientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    RetireeID = table.Column<int>(type: "int", nullable: true),
                    AgentID = table.Column<int>(type: "int", nullable: true),
                    EntitledID = table.Column<int>(type: "int", nullable: true),
                    GuardianID = table.Column<int>(type: "int", nullable: true),
                    RecipientType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PendingReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryRecipients", x => x.RecipientID);
                    table.CheckConstraint("CK_SalaryRecipient_TypeOrGuardianWithEntitled", "\r\n                (\r\n                    (CASE WHEN RetireeID IS NOT NULL THEN 1 ELSE 0 END +\r\n                     CASE WHEN AgentID IS NOT NULL THEN 1 ELSE 0 END +\r\n                     CASE WHEN EntitledID IS NOT NULL THEN 1 ELSE 0 END +\r\n                     CASE WHEN GuardianID IS NOT NULL THEN 1 ELSE 0 END) = 1\r\n                )\r\n                OR\r\n                (\r\n                    GuardianID IS NOT NULL\r\n                    AND EntitledID IS NOT NULL\r\n                    AND RetireeID IS NULL\r\n                    AND AgentID IS NULL\r\n                )");
                    table.ForeignKey(
                        name: "FK_SalaryRecipients_Agents_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agents",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryRecipients_EntitledPersons_EntitledID",
                        column: x => x.EntitledID,
                        principalTable: "EntitledPersons",
                        principalColumn: "EntitledID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryRecipients_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryRecipients_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryRecipients_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryRecipients_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetireeID = table.Column<int>(type: "int", nullable: true),
                    EntitledID = table.Column<int>(type: "int", nullable: true),
                    FileID = table.Column<int>(type: "int", nullable: true),
                    AgentID = table.Column<int>(type: "int", nullable: true),
                    GuardianID = table.Column<int>(type: "int", nullable: true),
                    PledgeID = table.Column<int>(type: "int", nullable: true),
                    ExemptionID = table.Column<int>(type: "int", nullable: true),
                    NoteID = table.Column<int>(type: "int", nullable: true),
                    SuspensionID = table.Column<int>(type: "int", nullable: true),
                    SalaryBatchID = table.Column<int>(type: "int", nullable: true),
                    MinistryBatchID = table.Column<int>(type: "int", nullable: true),
                    DecisionID = table.Column<int>(type: "int", nullable: true),
                    SettlementBatchID = table.Column<int>(type: "int", nullable: true),
                    ReportID = table.Column<int>(type: "int", nullable: true),
                    InvestmentProjectID = table.Column<int>(type: "int", nullable: true),
                    InvestmentPropertyID = table.Column<int>(type: "int", nullable: true),
                    PropertyUnitID = table.Column<int>(type: "int", nullable: true),
                    LeaseContractID = table.Column<int>(type: "int", nullable: true),
                    TenantID = table.Column<int>(type: "int", nullable: true),
                    InvestmentPortfolioID = table.Column<int>(type: "int", nullable: true),
                    MaintenanceRequestID = table.Column<int>(type: "int", nullable: true),
                    WorkOrderID = table.Column<int>(type: "int", nullable: true),
                    AssetValuationID = table.Column<int>(type: "int", nullable: true),
                    DepreciationRecordID = table.Column<int>(type: "int", nullable: true),
                    PaymentScheduleID = table.Column<int>(type: "int", nullable: true),
                    OperationalExpenseID = table.Column<int>(type: "int", nullable: true),
                    InvestmentPlanID = table.Column<int>(type: "int", nullable: true),
                    RiskAssessmentID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    CorrespondenceID = table.Column<int>(type: "int", nullable: true),
                    ServiceRequestID = table.Column<int>(type: "int", nullable: true),
                    TicketMessageID = table.Column<int>(type: "int", nullable: true),
                    CitizenAccountID = table.Column<int>(type: "int", nullable: true),
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false),
                    OwnerType = table.Column<int>(type: "int", nullable: false),
                    VerifiedByUserID = table.Column<int>(type: "int", nullable: true),
                    UploadedByUserID = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Checksum = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PendingReviewStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentID);
                    table.CheckConstraint("CK_Document_OwnerType", "(OwnerType = 1  AND FileID IS NOT NULL)           OR\r\n              (OwnerType = 2  AND RetireeID IS NOT NULL)         OR\r\n              (OwnerType = 3  AND EntitledID IS NOT NULL)        OR\r\n              (OwnerType = 4  AND AgentID IS NOT NULL)           OR\r\n              (OwnerType = 5  AND GuardianID IS NOT NULL)        OR\r\n              (OwnerType = 6  AND PledgeID IS NOT NULL)          OR\r\n              (OwnerType = 7  AND ExemptionID IS NOT NULL)       OR\r\n              (OwnerType = 8  AND NoteID IS NOT NULL)            OR\r\n              (OwnerType = 9  AND SuspensionID IS NOT NULL)      OR\r\n              (OwnerType = 10 AND SalaryBatchID IS NOT NULL)     OR\r\n              (OwnerType = 11 AND MinistryBatchID IS NOT NULL)   OR\r\n              (OwnerType = 12 AND DecisionID IS NOT NULL)        OR\r\n              (OwnerType = 13 AND SettlementBatchID IS NOT NULL) OR\r\n              (OwnerType = 14 AND ReportID IS NOT NULL)          OR\r\n              (OwnerType = 15 AND InvestmentProjectID IS NOT NULL)  OR\r\n              (OwnerType = 16 AND InvestmentPropertyID IS NOT NULL) OR\r\n              (OwnerType = 17 AND PropertyUnitID IS NOT NULL)       OR\r\n              (OwnerType = 18 AND LeaseContractID IS NOT NULL)      OR\r\n              (OwnerType = 19 AND TenantID IS NOT NULL)             OR\r\n              (OwnerType = 20 AND InvestmentPortfolioID IS NOT NULL) OR\r\n              (OwnerType = 21 AND MaintenanceRequestID IS NOT NULL)  OR\r\n              (OwnerType = 22 AND WorkOrderID IS NOT NULL)           OR\r\n              (OwnerType = 25 AND AssetValuationID IS NOT NULL)      OR\r\n              (OwnerType = 27 AND DepreciationRecordID IS NOT NULL)  OR\r\n              (OwnerType = 28 AND PaymentScheduleID IS NOT NULL)     OR\r\n              (OwnerType = 30 AND OperationalExpenseID IS NOT NULL)  OR\r\n              (OwnerType = 31 AND InvestmentPlanID IS NOT NULL)      OR\r\n              (OwnerType = 33 AND RiskAssessmentID IS NOT NULL)      OR\r\n              (OwnerType = 40 AND EmployeeID IS NOT NULL)             OR\r\n              (OwnerType = 41 AND CorrespondenceID IS NOT NULL)       OR\r\n              (OwnerType = 50 AND ServiceRequestID IS NOT NULL)       OR\r\n              (OwnerType = 51 AND TicketMessageID IS NOT NULL)        OR\r\n              (OwnerType = 52 AND CitizenAccountID IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Documents_Agents_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agents",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_AssetValuations_AssetValuationID",
                        column: x => x.AssetValuationID,
                        principalTable: "AssetValuations",
                        principalColumn: "ValuationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_CitizenAccounts_CitizenAccountID",
                        column: x => x.CitizenAccountID,
                        principalTable: "CitizenAccounts",
                        principalColumn: "CitizenAccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Correspondences_CorrespondenceID",
                        column: x => x.CorrespondenceID,
                        principalTable: "Correspondences",
                        principalColumn: "CorrespondenceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_DepreciationRecords_DepreciationRecordID",
                        column: x => x.DepreciationRecordID,
                        principalTable: "DepreciationRecords",
                        principalColumn: "DeprecID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentExemptions_ExemptionID",
                        column: x => x.ExemptionID,
                        principalTable: "DocumentExemptions",
                        principalColumn: "ExemptionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentPledges_PledgeID",
                        column: x => x.PledgeID,
                        principalTable: "DocumentPledges",
                        principalColumn: "PledgeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentTypes_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_EntitledPersons_EntitledID",
                        column: x => x.EntitledID,
                        principalTable: "EntitledPersons",
                        principalColumn: "EntitledID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_FileNotes_NoteID",
                        column: x => x.NoteID,
                        principalTable: "FileNotes",
                        principalColumn: "NoteID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_InvestmentPlans_InvestmentPlanID",
                        column: x => x.InvestmentPlanID,
                        principalTable: "InvestmentPlans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_InvestmentPortfolios_InvestmentPortfolioID",
                        column: x => x.InvestmentPortfolioID,
                        principalTable: "InvestmentPortfolios",
                        principalColumn: "PortfolioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_InvestmentProjects_InvestmentProjectID",
                        column: x => x.InvestmentProjectID,
                        principalTable: "InvestmentProjects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_InvestmentProperties_InvestmentPropertyID",
                        column: x => x.InvestmentPropertyID,
                        principalTable: "InvestmentProperties",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_LeaseContracts_LeaseContractID",
                        column: x => x.LeaseContractID,
                        principalTable: "LeaseContracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_MaintenanceRequests_MaintenanceRequestID",
                        column: x => x.MaintenanceRequestID,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "RequestID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_MinistryApprovalBatches_MinistryBatchID",
                        column: x => x.MinistryBatchID,
                        principalTable: "MinistryApprovalBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_OperationalExpenses_OperationalExpenseID",
                        column: x => x.OperationalExpenseID,
                        principalTable: "OperationalExpenses",
                        principalColumn: "ExpenseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_PaymentSchedules_PaymentScheduleID",
                        column: x => x.PaymentScheduleID,
                        principalTable: "PaymentSchedules",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_PensionSuspensions_SuspensionID",
                        column: x => x.SuspensionID,
                        principalTable: "PensionSuspensions",
                        principalColumn: "SuspensionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_PropertyUnits_PropertyUnitID",
                        column: x => x.PropertyUnitID,
                        principalTable: "PropertyUnits",
                        principalColumn: "UnitID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_RiskAssessments_RiskAssessmentID",
                        column: x => x.RiskAssessmentID,
                        principalTable: "RiskAssessments",
                        principalColumn: "RiskID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_SalaryPaymentBatches_SalaryBatchID",
                        column: x => x.SalaryBatchID,
                        principalTable: "SalaryPaymentBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_SavedReports_ReportID",
                        column: x => x.ReportID,
                        principalTable: "SavedReports",
                        principalColumn: "ReportID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_ServiceRequests_ServiceRequestID",
                        column: x => x.ServiceRequestID,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_SettlementDecisions_DecisionID",
                        column: x => x.DecisionID,
                        principalTable: "SettlementDecisions",
                        principalColumn: "DecisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_SettlementsBatches_SettlementBatchID",
                        column: x => x.SettlementBatchID,
                        principalTable: "SettlementsBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_TicketMessages_TicketMessageID",
                        column: x => x.TicketMessageID,
                        principalTable: "TicketMessages",
                        principalColumn: "TicketMessageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Users_UploadedByUserID",
                        column: x => x.UploadedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Users_VerifiedByUserID",
                        column: x => x.VerifiedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_WorkOrders_WorkOrderID",
                        column: x => x.WorkOrderID,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissingDocuments",
                columns: table => new
                {
                    MissingDocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false),
                    PledgeID = table.Column<int>(type: "int", nullable: true),
                    ExemptionID = table.Column<int>(type: "int", nullable: true),
                    AgentID = table.Column<int>(type: "int", nullable: true),
                    GuardianID = table.Column<int>(type: "int", nullable: true),
                    EntitledID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissingDocuments", x => x.MissingDocumentID);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_Agents_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agents",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_DocumentExemptions_ExemptionID",
                        column: x => x.ExemptionID,
                        principalTable: "DocumentExemptions",
                        principalColumn: "ExemptionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_DocumentPledges_PledgeID",
                        column: x => x.PledgeID,
                        principalTable: "DocumentPledges",
                        principalColumn: "PledgeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_DocumentTypes_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_EntitledPersons_EntitledID",
                        column: x => x.EntitledID,
                        principalTable: "EntitledPersons",
                        principalColumn: "EntitledID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissingDocuments_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SalaryPaymentItems",
                columns: table => new
                {
                    PaymentItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchID = table.Column<int>(type: "int", nullable: false),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    RetireeID = table.Column<int>(type: "int", nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    PaymentBookID = table.Column<int>(type: "int", nullable: true),
                    RecipientID = table.Column<int>(type: "int", nullable: true),
                    TransactionNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderBranch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverBranch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryPaymentItems", x => x.PaymentItemID);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_PaymentBooks_PaymentBookID",
                        column: x => x.PaymentBookID,
                        principalTable: "PaymentBooks",
                        principalColumn: "PaymentBookID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_Retirees_RetireeID",
                        column: x => x.RetireeID,
                        principalTable: "Retirees",
                        principalColumn: "RetireeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_RetirementFiles_FileID",
                        column: x => x.FileID,
                        principalTable: "RetirementFiles",
                        principalColumn: "FileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_SalaryPaymentBatches_BatchID",
                        column: x => x.BatchID,
                        principalTable: "SalaryPaymentBatches",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_SalaryRecipients_RecipientID",
                        column: x => x.RecipientID,
                        principalTable: "SalaryRecipients",
                        principalColumn: "RecipientID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_TransactionStatuses_StatusID",
                        column: x => x.StatusID,
                        principalTable: "TransactionStatuses",
                        principalColumn: "StatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryPaymentItems_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "AdministrativeOrderTypes",
                columns: new[] { "AdministrativeOrderTypeID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "GeneratesServiceHistory", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "ORD_APP", new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5057), null, null, "قرار تعيين موظف جديد", true, true, false, "قرار تعيين", null },
                    { 2, "ORD_PRO", new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5063), null, null, "قرار ترقية رتبة أو درجة وظيفية", true, true, false, "قرار ترقية", null },
                    { 3, "ORD_TRA", new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5066), null, null, "قرار نقل بين الإدارات أو الأقسام", true, true, false, "قرار نقل", null },
                    { 4, "ORD_DIS", new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5068), null, null, "قرار تأديبي أو لفت نظر", true, true, false, "قرار تأديبي", null },
                    { 5, "ORD_TER", new DateTime(2026, 6, 25, 21, 49, 41, 279, DateTimeKind.Utc).AddTicks(5070), null, null, "قرار إنهاء خدمة موظف", true, true, false, "قرار إنهاء خدمة", null }
                });

            migrationBuilder.InsertData(
                table: "Banks",
                columns: new[] { "BankID", "BankCode", "BankName", "CreatedAt", "IsActive", "UpdatedAt" },
                values: new object[] { 1, "CB", "البنك المركزي", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null });

            migrationBuilder.InsertData(
                table: "BudgetChapters",
                columns: new[] { "ChapterID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "31", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الأول: المعاشات والأجور", null },
                    { 2, "32", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الثاني: المستلزمات السلعية", null },
                    { 3, "33", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الثالث: المصروفات الخدمية", null },
                    { 4, "34", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الباب الرابع: المصروفات التحويلية والإعانات", null }
                });

            migrationBuilder.InsertData(
                table: "CorrespondenceTypes",
                columns: new[] { "CorrespondenceTypeID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "INTERNAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "مراسلات الوزارة (الداخلي)", null },
                    { 2, "EXTERNAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "مراسلات (الخارجي)", null },
                    { 3, "ADMIN_ORDER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "أوامر إدارية", null },
                    { 4, "INTERNAL_DIRECTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "توجيهات وتعاميم داخلية", null },
                    { 5, "EXTERNAL_DIRECTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "توجيهات وتعاميم خارجية", null }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "PENSIONS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "إدارة المعاشات ", null },
                    { 2, "ADMIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "إدارة الشئون الإدارية", null },
                    { 3, "INVEST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "إدارة الاستثمار", null },
                    { 4, "FINANCE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "إدارة الشئون المالية", null },
                    { 5, "LEGAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الإدارة القانونية", null },
                    { 6, "DG_OFFICE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مكتب المدير العام", null }
                });

            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "DocumentTypeID", "AllowedExtensions", "CreatedAt", "HasExpiry", "IsActive", "IsRequired", "MaxFileSize", "OwnerTableID", "OwnerType", "SortOrder", "SystemCode", "TypeName", "TypeNameEn", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, true, null, null, null, 1, "", "الهوية الوطنية", "National ID", null },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 2, "", "شهادة الوفاة", "Death Certificate", null },
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, false, null, null, null, 3, "", "وثيقة الوكالة", "Agency Document", null },
                    { 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, false, null, null, null, 4, "", "وثيقة الوصاية", "Guardianship Document", null },
                    { 5, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 5, "", "اثبات عزوبية", "Single Status Certificate", null },
                    { 6, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 6, "", "عقد زواج", "Marriage Contract", null },
                    { 7, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 7, "", "حصر ورثة", "Inheritance Certificate", null },
                    { 8, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 8, "", "أوليات الملف", "File Originals", null },
                    { 9, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 9, "", "وثيقة تعهد", "Pledge Document", null },
                    { 10, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 10, "", "توجيه بإعفاء من النواقص", "Missing Documents Exemption Directive", null },
                    { 11, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 11, "", "أمر إيقاف المعاش", "Salary Suspension Order", null },
                    { 12, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 12, "", "مرفق ملاحظة", "Note Attachment", null },
                    { 13, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 13, "", "صورة دفتر الصرف", "Payment Book Image", null },
                    { 14, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 14, "", "وثيقة طلاق", "Divorce Certificate", null },
                    { 15, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 15, "", "إثبات الخدمة المدنية", "Civil Service Proof", null },
                    { 16, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 16, "", "تقرير طبي", "Medical Report", null },
                    { 17, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, null, null, null, 17, "", "حكم إعالة", "Maintenance Judgment", null },
                    { 99, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, false, null, null, null, 99, "", "الهوية الوطنية (ظهر)", "National ID (Back)", null }
                });

            migrationBuilder.InsertData(
                table: "EmploymentStatuses",
                columns: new[] { "EmploymentStatusID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "IsInService", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, true, "مستمر بالعمل", null },
                    { 2, "UNPAID_LEAVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, false, "إجازة بدون معاش", null },
                    { 3, "SECONDED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, false, "منتدب / مُعار", null },
                    { 4, "SUSPENDED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, false, "موقوف عن العمل", null },
                    { 5, "ABSENT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, false, "منقطع عن العمل", null },
                    { 6, "RESIGNED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, false, "مستقيل", null },
                    { 7, "TERMINATED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, false, "مفصول", null },
                    { 8, "RETIRED_HR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, false, "مُحال للتقاعد (إدارياً)", null }
                });

            migrationBuilder.InsertData(
                table: "ExternalParties",
                columns: new[] { "ExternalPartyID", "ContactInfo", "CreatedAt", "DeletedAt", "DeletedByUserID", "IsActive", "IsDeleted", "Name", "PartyType", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "بنك القطيبي", "بنك", null },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "الإدارة العامة لمكافحة المخدرات", "إدارة أمنية", null },
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "وكيل الخدمة المدنية", "وزارة", null },
                    { 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "الهيئة العامة للتأمينات - تعز", "هيئة حكومية", null },
                    { 5, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "مدير أمن محافظة عدن", "إدارة أمنية", null }
                });

            migrationBuilder.InsertData(
                table: "FileStatuses",
                columns: new[] { "StatusID", "CreatedAt", "Description", "StatusColor", "StatusName" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "outline", "جديد" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "success", "نشط" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "destructive", "موقوف" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "secondary", "مغلق" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "warning", "ناقص" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "warning", "معلق" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "info", "تحت المراجعة" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "destructive", "مرفوض" }
                });

            migrationBuilder.InsertData(
                table: "FiscalYears",
                columns: new[] { "FiscalYearID", "CreatedAt", "DeletedAt", "DeletedByUserID", "EndDate", "IsClosed", "IsDeleted", "Notes", "StartDate", "TotalBudget", "UpdatedAt", "YearName" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), false, false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1000000000m, null, "2026" });

            migrationBuilder.InsertData(
                table: "HealthStatuses",
                columns: new[] { "HealthStatusID", "CreatedAt", "HealthStatusName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "سليم", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "عاجز جزئياً", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "عاجز كلياً", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "متوفى", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "شهيد", null }
                });

            migrationBuilder.InsertData(
                table: "InvestmentTypes",
                columns: new[] { "TypeID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsDeleted", "TypeName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "عقاري", null },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "أسهم", null },
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "سندات", null },
                    { 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "وديعة بنكية", null },
                    { 5, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "عملات", null }
                });

            migrationBuilder.InsertData(
                table: "JobTitles",
                columns: new[] { "JobTitleID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "DIR_GEN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "قيادي", true, false, "المدير العام", null },
                    { 2, "DEP_DIR_GEN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "قيادي", true, false, "نائب المدير العام", null },
                    { 3, "DEPT_MGR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "إشرافي", true, false, "مدير إدارة", null },
                    { 4, "DG_OFFICE_MGR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "إشرافي", true, false, "مدير مكتب المدير العام", null },
                    { 5, "SEC_HEAD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "إشرافي", true, false, "رئيس قسم", null },
                    { 6, "EMPLOYEE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "تخصصي وإداري", true, false, "موظف", null }
                });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "LeaveTypeID", "Code", "CountsTowardsService", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "IsPaid", "MaxDaysPerRequest", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "ANNUAL", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "إجازة اعتيادية منتظمة.", true, false, true, 30, "إجازة اعتيادية", null },
                    { 2, "SICK", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "إجازة مرضية معتمدة.", true, false, true, 30, "إجازة مرضية", null },
                    { 3, "UNPAID", false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "إجازة بدون معاش.", true, false, false, 90, "إجازة بدون معاش", null },
                    { 4, "STUDY", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "إجازة دراسية معتمدة.", true, false, true, 365, "إجازة دراسية", null }
                });

            migrationBuilder.InsertData(
                table: "PensionFundBranches",
                columns: new[] { "PensionBranchID", "Address", "BranchName", "CreatedAt", "Governorate", "IsActive", "ManagerName", "ManagerPhone", "ParentBranchId", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "الادارة العامة", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "عدن", true, null, null, null, null, null },
                    { 4, null, "فرع حضرموت", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "حضرموت", true, null, null, null, null, null },
                    { 5, null, "فرع الحديدة", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "الحديدة", true, null, null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "Code", "Description" },
                values: new object[,]
                {
                    { 1, "File", "File.Create", "إنشاء ملف تقاعد جديد" },
                    { 2, "File", "File.Edit.BasicData", "تعديل البيانات الأساسية" },
                    { 3, "Financial", "File.Edit.FinancialData", "تعديل البيانات المالية" },
                    { 4, "File", "File.View.Sensitive", "رؤية الحقول الحساسة" },
                    { 5, "File", "File.Documents.Upload", "رفع وإدارة الوثائق" },
                    { 6, "File", "File.ChangeStatus.Return", "إعادة الملف لوجود نواقص" },
                    { 7, "File", "File.ChangeStatus.Approve", "اعتماد الملف" },
                    { 8, "System", "File.Override", "تجاوز قواعد النظام" },
                    { 9, "File", "File.Delete.Soft", "حذف منطقي للملف" },
                    { 10, "Export", "Export.Sensitive", "تصدير بيانات حساسة" },
                    { 11, "Export", "Export.Financial", "تصدير بيانات مالية" },
                    { 12, "Financial", "File.View.BankData", "رؤية بيانات الحساب البنكي" },
                    { 13, "System", "Audit.ViewAll", "رؤية سجلات التدقيق" },
                    { 14, "System", "System.ManageUsers", "إدارة المستخدمين والأدوار" },
                    { 15, "System", "System.Settings", "إعدادات النظام" },
                    { 16, "System", "System.Admin", "مدير النظام — صلاحية إدارية شاملة" },
                    { 17, "EntitledPerson", "EntitledPerson.View", "عرض بيانات المستحقين" },
                    { 18, "EntitledPerson", "EntitledPerson.Create", "إضافة مستحق جديد" },
                    { 19, "EntitledPerson", "EntitledPerson.Edit", "تعديل بيانات مستحق" },
                    { 20, "EntitledPerson", "EntitledPerson.Delete", "حذف مستحق" },
                    { 21, "EntitledPerson", "EntitledPerson.Lifecycle", "إيقاف/إعادة تفعيل استحقاق" },
                    { 22, "Guardian", "Guardian.View", "عرض بيانات الأوصياء" },
                    { 23, "Guardian", "Guardian.Manage", "إدارة الوصايا" },
                    { 24, "Retiree", "Retiree.View", "عرض بيانات المتقاعدين" },
                    { 25, "Retiree", "Retiree.Edit", "تعديل بيانات المتقاعد" },
                    { 26, "Retiree", "Retiree.Delete", "حذف متقاعد" },
                    { 27, "Agent", "Agent.View", "عرض بيانات الوكلاء" },
                    { 28, "Agent", "Agent.Manage", "إدارة الوكلاء" },
                    { 29, "FileNote", "FileNote.View", "عرض ملاحظات الملفات" },
                    { 30, "FileNote", "FileNote.Manage", "إدارة ملاحظات الملفات" },
                    { 31, "DocumentExemption", "DocumentExemption.View", "عرض إعفاءات الوثائق" },
                    { 32, "DocumentExemption", "DocumentExemption.Manage", "إدارة إعفاءات الوثائق" },
                    { 33, "DocumentPledge", "DocumentPledge.View", "عرض التعهدات" },
                    { 34, "DocumentPledge", "DocumentPledge.Manage", "إدارة التعهدات" },
                    { 35, "MissingDocument", "MissingDocument.View", "عرض الوثائق الناقصة" },
                    { 36, "MissingDocument", "MissingDocument.Manage", "إدارة الوثائق الناقصة" },
                    { 37, "PaymentBook", "PaymentBook.View", "عرض دفاتر الصرف" },
                    { 38, "PaymentBook", "PaymentBook.Manage", "إدارة دفاتر الصرف" },
                    { 39, "Ministry", "Ministry.View", "عرض كشوفات إحالة إلى التقاعد" },
                    { 40, "Ministry", "Ministry.Manage", "إدارة كشوفات إحالة إلى التقاعد" },
                    { 41, "Suspension", "Suspension.View", "عرض إيقافات المعاش" },
                    { 42, "Suspension", "Suspension.Manage", "إدارة إيقافات المعاش" },
                    { 43, "Dashboard", "Dashboard.View", "عرض لوحة التحكم" },
                    { 44, "Settlement", "Settlement.View", "عرض التسويات" },
                    { 45, "Settlement", "Settlement.Manage", "إدارة التسويات" },
                    { 46, "SalaryRecipient", "SalaryRecipient.View", "عرض مستلمي المعاشات" },
                    { 47, "SalaryRecipient", "SalaryRecipient.Manage", "إدارة مستلمي المعاشات" },
                    { 48, "SalaryRecipient", "SalaryRecipient.Add", "إضافة مستلم معاش" },
                    { 49, "Guardian", "Guardian.Add", "إضافة وصي جديد" },
                    { 50, "Agent", "Agent.Add", "إضافة وكيل جديد" },
                    { 51, "PaymentBook", "PaymentBook.Add", "إصدار دفتر صرف جديد" },
                    { 52, "DocumentExemption", "DocumentExemption.Add", "إضافة إعفاء جديد" },
                    { 53, "DocumentPledge", "DocumentPledge.Add", "إضافة تعهد جديد" },
                    { 54, "Guardian", "Guardian.Edit", "تعديل بيانات وصي" },
                    { 55, "Agent", "Agent.Edit", "تعديل بيانات وكيل" },
                    { 56, "DocumentPledge", "DocumentPledge.Edit", "تعديل تعهد" },
                    { 57, "DocumentExemption", "DocumentExemption.Edit", "تعديل إعفاء" },
                    { 58, "PaymentBook", "PaymentBook.Edit", "تعديل دفتر صرف" },
                    { 59, "SalaryRecipient", "SalaryRecipient.Edit", "تعديل مستلم معاش" },
                    { 60, "FileNote", "FileNote.Edit", "تعديل ملاحظة ملف" },
                    { 61, "MissingDocument", "MissingDocument.Edit", "تعديل وثيقة ناقصة" },
                    { 62, "Ministry", "Ministry.Edit", "تعديل كشف إحالة إلى التقاعد" },
                    { 63, "Suspension", "Suspension.Edit", "تعديل إيقاف معاش" },
                    { 64, "Settlement", "Settlement.Edit", "تعديل تسوية" },
                    { 65, "Suspension", "Suspension.Add", "إضافة إيقاف معاش" },
                    { 66, "SalaryBatch", "SalaryBatch.View", "عرض كشوفات صرف المعاشات" },
                    { 67, "SalaryBatch", "SalaryBatch.Manage", "إدارة كشوفات صرف المعاشات" },
                    { 68, "SalaryBatch", "SalaryBatch.Approve", "اعتماد كشوفات صرف المعاشات" },
                    { 69, "SalaryBatch", "SalaryBatch.Export", "تصدير كشوفات صرف المعاشات" },
                    { 70, "SalaryBatch", "SalaryBatch.ImportResults", "استيراد نتائج الحوالات لكشوف المعاشات" },
                    { 71, "OperationalBudget", "OperationalBudget.View", "عرض الميزانية التشغيلية" },
                    { 72, "OperationalBudget", "OperationalBudget.Manage", "إدارة الميزانية التشغيلية" },
                    { 73, "Investment", "Investment.View", "عرض الاستثمارات" },
                    { 74, "Investment", "Investment.Manage", "إدارة الاستثمارات" },
                    { 75, "Investment", "Investment.Finance", "المالية الاستثمارية" },
                    { 76, "Investment", "Investment.Approve", "اعتماد الاستثمارات" },
                    { 77, "Guardian", "Guardian.Delete", "حذف وصي" },
                    { 78, "Agent", "Agent.Delete", "حذف وكيل" },
                    { 79, "DocumentPledge", "DocumentPledge.Delete", "حذف تعهد" },
                    { 80, "DocumentExemption", "DocumentExemption.Delete", "حذف إعفاء" },
                    { 81, "PaymentBook", "PaymentBook.Delete", "حذف دفتر صرف" },
                    { 82, "SalaryRecipient", "SalaryRecipient.Delete", "حذف مستلم معاش" },
                    { 83, "Suspension", "Suspension.Delete", "حذف إيقاف معاش" },
                    { 84, "FileNote", "FileNote.Add", "إضافة ملاحظة ملف" },
                    { 85, "FileNote", "FileNote.Delete", "حذف ملاحظة ملف" },
                    { 86, "MissingDocument", "MissingDocument.Add", "إضافة وثيقة ناقصة" },
                    { 87, "MissingDocument", "MissingDocument.Delete", "حذف وثيقة ناقصة" },
                    { 88, "Settlement", "Settlement.Add", "إضافة تسوية" },
                    { 89, "Settlement", "Settlement.Delete", "حذف تسوية" },
                    { 90, "Ministry", "Ministry.Add", "إضافة كشف إحالة إلى التقاعد" },
                    { 91, "Ministry", "Ministry.Delete", "حذف كشف إحالة إلى التقاعد" },
                    { 92, "OperationalBudget", "OperationalBudget.Add", "إضافة ميزانية تشغيلية" },
                    { 93, "OperationalBudget", "OperationalBudget.Edit", "تعديل ميزانية تشغيلية" },
                    { 94, "OperationalBudget", "OperationalBudget.Delete", "حذف ميزانية تشغيلية" },
                    { 95, "Investment", "Investment.Add", "إضافة استثمار" },
                    { 96, "Investment", "Investment.Edit", "تعديل استثمار" },
                    { 97, "Investment", "Investment.Delete", "حذف استثمار" },
                    { 98, "SalaryBatch", "Payroll.ExcludeBeneficiary", "استبعاد مستفيد من كشف صرف" },
                    { 99, "SalaryBatch", "Payroll.ManageDeductions", "إدارة الاستقطاعات المالية" },
                    { 100, "System", "System.Settings.Banks", "إدارة بيانات البنوك" },
                    { 101, "System", "System.Settings.DocumentTypes", "إدارة أنواع الوثائق" },
                    { 102, "System", "System.Settings.JobTitles", "إدارة المسميات الوظيفية" },
                    { 103, "System", "System.Settings.InvestmentTypes", "إدارة أنواع الاستثمارات" },
                    { 104, "HR", "HR.Dashboard.View", "عرض لوحة الشؤون الإدارية" },
                    { 105, "HR", "HR.Reference.View", "عرض البيانات المرجعية للشؤون الإدارية" },
                    { 106, "HR", "HR.Reference.Manage", "إدارة البيانات المرجعية للشؤون الإدارية" },
                    { 107, "HR", "HR.Employee.View", "عرض الموظفين" },
                    { 108, "HR", "HR.Employee.Create", "إضافة موظف" },
                    { 109, "HR", "HR.Employee.Edit", "تعديل موظف" },
                    { 110, "HR", "HR.Employee.Delete", "حذف موظف" },
                    { 111, "HR", "HR.ServiceHistory.View", "عرض سجل الخدمة" },
                    { 112, "File", "File.View.History", "عرض سجل العمليات وتاريخ الملف" },
                    { 113, "Financial", "Financial.Reports.View", "عرض التقارير المالية المتقدمة" },
                    { 114, "Financial", "Financial.Bank.Statement.View", "عرض كشوفات الحساب البنكي" },
                    { 115, "OperationalBudget", "OperationalBudget.Reports.View", "عرض تقارير الميزانية التشغيلية" },
                    { 116, "System", "System.Audit.Export", "تصدير سجلات التدقيق والعمليات" },
                    { 117, "Treasury", "Treasury.View", "عرض الخزينة والحركات المالية" },
                    { 118, "Treasury", "Treasury.Manage", "إدارة عمليات الخزينة (إيداع/سحب)" },
                    { 119, "OperationalBudget", "BudgetStructure.View", "عرض هيكل الميزانية التعريفي" },
                    { 120, "OperationalBudget", "BudgetStructure.Manage", "إدارة هيكل الميزانية التعريفي" },
                    { 121, "Correspondence", "Correspondence.View", "عرض المراسلات" },
                    { 122, "Correspondence", "Correspondence.Create", "إضافة قيد مراسلة" },
                    { 123, "Correspondence", "Correspondence.Edit", "تعديل قيد مراسلة" },
                    { 124, "Correspondence", "Correspondence.Delete", "حذف قيد مراسلة" },
                    { 125, "File", "File.Review.View", "عرض لوحة مراجعة الملف" },
                    { 126, "File", "File.Review.Approve", "اعتماد مراجعة الملف" },
                    { 127, "File", "File.Review.Reject", "رفض مراجعة الملف" },
                    { 128, "Inventory", "Inventory.Category.View", "عرض تصنيفات المخزون" },
                    { 129, "Inventory", "Inventory.Category.Manage", "إدارة تصنيفات المخزون" },
                    { 130, "Inventory", "Inventory.Item.View", "عرض أصناف المخزون" },
                    { 131, "Inventory", "Inventory.Item.Create", "إضافة صنف مخزون جديد" },
                    { 132, "Inventory", "Inventory.Item.Edit", "تعديل صنف مخزون" },
                    { 133, "Inventory", "Inventory.Item.Delete", "حذف صنف مخزون" },
                    { 134, "Inventory", "Inventory.Custody.View", "عرض العهد الشخصية" },
                    { 135, "Inventory", "Inventory.Custody.Create", "تسليم عهدة جديدة" },
                    { 136, "Inventory", "Inventory.Custody.Return", "إرجاع عهدة" },
                    { 137, "Inventory", "Inventory.Transaction.View", "عرض حركات المخزون" },
                    { 138, "Inventory", "Inventory.Transaction.Create", "تسجيل حركة مخزون" },
                    { 139, "Inventory", "Inventory.Stocktake", "تسوية الجرد" }
                });

            migrationBuilder.InsertData(
                table: "PropertyTypes",
                columns: new[] { "TypeID", "CreatedAt", "DeletedAt", "DeletedByUserID", "IsDeleted", "TypeName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "أرض", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "مبنى سكني", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "مجمع تجاري", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "مستودع", null }
                });

            migrationBuilder.InsertData(
                table: "Ranks",
                columns: new[] { "RankID", "BaseSalary", "CreatedAt", "RankLevel", "RankName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 120000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "ملازم", null },
                    { 2, 135000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ملازم أول", null },
                    { 3, 150000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "نقيب", null },
                    { 4, 165000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "رائد", null },
                    { 5, 180000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "مقدم", null },
                    { 6, 200000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, "عقيد", null },
                    { 7, 225000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, "عميد", null },
                    { 8, 250000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, "لواء", null },
                    { 9, 280000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, "فريق", null },
                    { 10, 320000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, "فريق أول", null },
                    { 11, 65000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 11, "جندي", null },
                    { 12, 75000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, "عريف", null },
                    { 13, 85000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, "رقيب ثاني", null },
                    { 14, 95000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, "رقيب اول", null },
                    { 15, 105000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, "مساعد", null },
                    { 16, 110000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 16, "مساعد ثاني", null },
                    { 17, 115000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 17, "مساعد اول", null }
                });

            migrationBuilder.InsertData(
                table: "Relationships",
                columns: new[] { "RelationshipID", "CreatedAt", "RelationshipName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "زوجة", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ابن", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "بنت", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "أم", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "أب", null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "أخت", null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "أخ", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleID", "CreatedAt", "Description", "IsActive", "RoleName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "مدير النظام — الحاسب الآلي وصلاحيات كاملة", true, "Admin", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "مدير إدارة المعاشات — مراجعة واعتماد الملفات", true, "PensionManager", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "موظف المعاشات — إنشاء واستكمال الملفات", true, "PensionClerk", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "الموظف المالي — إدارة المعاشات والتسويات المالية", true, "FinanceOfficer", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "المدقق — قراءة ومراجعة فقط لجميع البيانات", true, "Auditor", null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "موظف الأرشيف — رفع وإدارة الوثائق فقط", true, "Archivist", null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "فني استثمار — عرض بيانات الصيانة والوثائق", true, "Technician", null }
                });

            migrationBuilder.InsertData(
                table: "SocialStatuses",
                columns: new[] { "SocialStatusID", "CreatedAt", "StatusName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "أعزب", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "متزوج", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "أرمل", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "مطلق", null }
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "SettingID", "Category", "CreatedAt", "Description", "SettingKey", "SettingValue", "UpdatedAt", "UpdatedByUserID" },
                values: new object[,]
                {
                    { 1, "General", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "OrganizationName", "هيئة التقاعد الوطنية", null, null },
                    { 2, "Pension", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "السن القانوني للتقاعد", "LegalRetirementAge", "63", null, null },
                    { 3, "Pension", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "الحد الأدنى لسنوات الخدمة لاستحقاق المعاش", "MinimumServiceYears", "15", null, null },
                    { 4, "Finance", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Currency", "د.ع", null, null }
                });

            migrationBuilder.InsertData(
                table: "SystemTables",
                columns: new[] { "TableID", "CreatedAt", "Description", "TableNameAr", "TableNameEn" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "المتقاعدون", "Retirees" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ملفات التقاعد", "RetirementFiles" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "المستحقون", "EntitledPersons" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الوكلاء", "Agents" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الأوصياء", "Guardians" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الوثائق", "Documents" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "التعهدات", "DocumentPledges" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الوثائق الناقصة", "MissingDocuments" },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "إعفاءات الوثائق", "DocumentExemptions" },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ملاحظات الملفات", "FileNotes" },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "إيقاف المعاش", "PensionSuspensions" },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "مستلمو المعاشات", "SalaryRecipients" },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الاستحقاقات المالية", "FinancialEntitlements" },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "كشوفات المعاشات", "SalaryPaymentBatches" },
                    { 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "تفاصيل المعاشات", "SalaryPaymentItems" },
                    { 16, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "كشوفات اعتماد القوة", "MinistryApprovalBatches" },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "تفاصيل اعتماد القوة", "MinistryApprovalItems" },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "قرارات التسوية", "SettlementDecisions" },
                    { 19, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "كشوفات التسويات", "SettlementsBatches" },
                    { 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "تفاصيل التسويات", "SettlementItems" },
                    { 21, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "المستخدمون", "Users" },
                    { 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "سجل الدخول", "LoginHistories" },
                    { 23, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الأدوار", "Roles" },
                    { 24, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "صلاحيات الجداول", "TablePermissions" },
                    { 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الإشعارات", "Notifications" },
                    { 26, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "سجل العمليات", "AuditLogs" },
                    { 27, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "قائمة المزامنة", "SyncQueues" },
                    { 28, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الأجهزة المسجلة", "DeviceRegistrations" },
                    { 29, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الرتب العسكرية", "Ranks" },
                    { 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "البنوك", "Banks" },
                    { 31, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "صلات القرابة", "Relationships" },
                    { 32, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "أنواع الوثائق", "DocumentTypes" },
                    { 33, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "حالات الملفات", "FileStatuses" },
                    { 34, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الحالات الاجتماعية", "SocialStatuses" },
                    { 35, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الحالات الصحية", "HealthStatuses" },
                    { 36, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "حالات المعاملات", "TransactionStatuses" },
                    { 37, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "فروع الصندوق", "PensionFundBranches" },
                    { 38, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "جداول النظام", "SystemTables" },
                    { 39, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "إعدادات النظام", "SystemSettings" },
                    { 40, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "قوالب التقارير", "ReportTemplates" },
                    { 41, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "التقارير المحفوظة", "SavedReports" },
                    { 42, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "أنواع التقاعد", "RetirementTypes" },
                    { 43, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "سجل تغيير الحالات", "StatusTransitionLogs" },
                    { 44, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الخزائن المالية", "Treasuries" },
                    { 45, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "حركات الخزينة", "TreasuryTransactions" },
                    { 46, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "تخصيصات الميزانية", "BudgetAllocations" }
                });

            migrationBuilder.InsertData(
                table: "TransactionStatuses",
                columns: new[] { "StatusID", "CreatedAt", "Description", "StatusColor", "StatusName" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1618), "تم إنشاء الحوالة ولم ترسل بعد", "warning", "قيد الإرسال" },
                    { 2, new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1624), "تم إرسال الحوالة للبنك", "info", "تم الإرسال" },
                    { 3, new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1626), "تم استلام الحوالة من المستفيد", "success", "تم الاستلام" },
                    { 4, new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1627), "فشلت الحوالة", "destructive", "فشل" },
                    { 5, new DateTime(2026, 6, 25, 21, 49, 41, 304, DateTimeKind.Utc).AddTicks(1628), "تم إلغاء الحوالة", "secondary", "ملغي" }
                });

            migrationBuilder.InsertData(
                table: "UnitTypes",
                columns: new[] { "TypeID", "CreatedAt", "DeletedAt", "DeletedByUserID", "IsDeleted", "TypeName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "شقة", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "مكتب", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "معرض تجاري", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, "مستودع فرعي", null }
                });

            migrationBuilder.InsertData(
                table: "BudgetSections",
                columns: new[] { "SectionID", "ChapterID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "311", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "معاشات وأجور نقدية", null },
                    { 2, 1, "312", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "البدلات والتعويضات الأخرى", null },
                    { 3, 1, "313", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "المزايا العينية", null },
                    { 4, 1, "314", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "المكافآت", null },
                    { 5, 1, "316", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "التأمينات والتقاعد", null },
                    { 6, 2, "322", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "الوقود والزيوت والقوى المحركة والمياه", null },
                    { 7, 2, "323", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "قطع الغيار واللوازم", null },
                    { 8, 2, "325", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "القرطاسية والمطبوعات", null }
                });

            migrationBuilder.InsertData(
                table: "DocumentRequirementRules",
                columns: new[] { "RuleID", "ConditionField", "ConditionValue", "Description", "DocumentTypeID", "IsActive", "IsGlobal", "TargetEntity" },
                values: new object[,]
                {
                    { 1, null, null, "الهوية الوطنية (عالمي)", 1, true, true, null },
                    { 2, null, null, "أوليات الملف (عالمي)", 8, true, true, null },
                    { 3, "IsAlive", "False", "شهادة الوفاة للمتوفين", 2, true, false, "Retiree" },
                    { 4, "Exists", "True", "وثيقة الوكالة عند وجود وكيل", 3, true, false, "Agent" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeID", "AcademicQualifications", "Address", "BirthDate", "BirthPlace", "BloodGroup", "CreatedAt", "DeletedAt", "DeletedByUserID", "DepartmentID", "EmergencyContactName", "EmergencyContactPhone", "EmployeeNumber", "EmploymentStatusID", "FullName", "Gender", "HireDate", "IsDeleted", "JobPositions", "JobTitleID", "MaritalStatus", "MilitaryNumber", "MilitaryServiceDate", "NationalID", "NationalIDExpiryDate", "NationalIDIssueDate", "NationalIDIssuePlace", "Notes", "Phone", "ProfilePhotoPath", "RankID", "SectionID", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "جامعي", "المنصورة", new DateTime(1966, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, null, null, "EMP-001", 1, "جمال فضل عبدالكريم القطيبي", (byte)1, new DateTime(1984, 8, 4, 0, 0, 0, 0, DateTimeKind.Utc), false, "المدير العام", 1, "متزوج", "2361", new DateTime(1984, 8, 4, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, null, null, null, null, 7, null, null },
                    { 2, "دكتوراه إدارة أعمال", "المنصورة", new DateTime(1990, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), "بيت الفقيه/الحديدة", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, null, null, "EMP-002", 1, "شامخ عبدالرحمن أحمد فتاح", (byte)1, new DateTime(2009, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "نائب المدير العام المدير المالي", 2, "متزوج", "24244", new DateTime(2009, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, null, null, "777618680", null, 6, null, null },
                    { 3, "بكالوريوس قانون", "خورمكسر/العريش", new DateTime(1986, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "عدن", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, null, null, "EMP-003", 1, "نائف أحمد محمد الشاجري", (byte)1, new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "مساعد المدير العام", 6, "متزوج", "18789", new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, null, null, null, null, 6, null, null },
                    { 4, "بكالوريوس", "لحج", null, "لحج", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, null, null, "EMP-004", 1, "صالح علي مقبل حيدرة", (byte)1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "مستشار المدير العام", 6, "متزوج", null, null, "", null, null, null, null, "777355188", null, 6, null, null },
                    { 5, null, null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, null, "EMP-005", 1, "منصور عبدالله اليماني", (byte)1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "مدير إدارة المعاشات", 3, null, "7443", null, "", null, null, null, null, "770601789", null, 6, null, null },
                    { 6, "ماجستير نظم معلومات", "المنصورة", new DateTime(1991, 7, 30, 0, 0, 0, 0, DateTimeKind.Utc), "بيت الفقيه/الحديدة", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, null, null, "EMP-006", 1, "كفاح عبدالرحمن أحمد فتاح", (byte)1, new DateTime(2014, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "مدير التخطيط", 6, "متزوج", "33068", new DateTime(2014, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, null, null, "771985009", null, 4, null, null },
                    { 13, null, "الصولبان غازي علوان", null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, null, null, "EMP-013", 1, "محمد عبده علي", (byte)1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "مختص النظافة", 6, "متزوج", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, null, null, null, null, 11, null, null }
                });

            migrationBuilder.InsertData(
                table: "PensionFundBranches",
                columns: new[] { "PensionBranchID", "Address", "BranchName", "CreatedAt", "Governorate", "IsActive", "ManagerName", "ManagerPhone", "ParentBranchId", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 2, null, "فرع صنعاء", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "صنعاء", true, null, null, 1, null, null },
                    { 3, null, "فرع تعز", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "تعز", true, null, null, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RolePermissionId", "CreatedAt", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1000, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 },
                    { 1001, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1 },
                    { 1002, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1 },
                    { 1003, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1 },
                    { 1004, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 1 },
                    { 1005, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 1 },
                    { 1006, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, 1 },
                    { 1007, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, 1 },
                    { 1008, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, 1 },
                    { 1009, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, 1 },
                    { 1010, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, 1 },
                    { 1011, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, 1 },
                    { 1012, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, 1 },
                    { 1013, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, 1 },
                    { 1014, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, 1 },
                    { 1015, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, 1 },
                    { 1016, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, 1 },
                    { 1017, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, 1 },
                    { 1018, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, 1 },
                    { 1019, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, 1 },
                    { 1020, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, 1 },
                    { 1021, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, 1 },
                    { 1022, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, 1 },
                    { 1023, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, 1 },
                    { 1024, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, 1 },
                    { 1025, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, 1 },
                    { 1026, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, 1 },
                    { 1027, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, 1 },
                    { 1028, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, 1 },
                    { 1029, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, 1 },
                    { 1030, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, 1 },
                    { 1031, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, 1 },
                    { 1032, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, 1 },
                    { 1033, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, 1 },
                    { 1034, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, 1 },
                    { 1035, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, 1 },
                    { 1036, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, 1 },
                    { 1037, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, 1 },
                    { 1038, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, 1 },
                    { 1039, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, 1 },
                    { 1040, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, 1 },
                    { 1041, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, 1 },
                    { 1042, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 1 },
                    { 1043, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, 1 },
                    { 1044, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, 1 },
                    { 1045, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 46, 1 },
                    { 1046, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 47, 1 },
                    { 1047, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 48, 1 },
                    { 1048, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 49, 1 },
                    { 1049, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 50, 1 },
                    { 1050, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 51, 1 },
                    { 1051, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 52, 1 },
                    { 1052, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 53, 1 },
                    { 1053, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 54, 1 },
                    { 1054, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 55, 1 },
                    { 1055, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 56, 1 },
                    { 1056, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 57, 1 },
                    { 1057, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 58, 1 },
                    { 1058, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 59, 1 },
                    { 1059, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 60, 1 },
                    { 1060, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 61, 1 },
                    { 1061, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 62, 1 },
                    { 1062, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 63, 1 },
                    { 1063, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 64, 1 },
                    { 1064, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 65, 1 },
                    { 1065, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 66, 1 },
                    { 1066, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 67, 1 },
                    { 1067, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 68, 1 },
                    { 1068, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 69, 1 },
                    { 1069, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 70, 1 },
                    { 1070, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 71, 1 },
                    { 1071, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 72, 1 },
                    { 1072, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 73, 1 },
                    { 1073, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 74, 1 },
                    { 1074, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 75, 1 },
                    { 1075, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 76, 1 },
                    { 1076, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 77, 1 },
                    { 1077, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 78, 1 },
                    { 1078, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 79, 1 },
                    { 1079, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 80, 1 },
                    { 1080, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 81, 1 },
                    { 1081, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 82, 1 },
                    { 1082, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 83, 1 },
                    { 1083, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 84, 1 },
                    { 1084, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 85, 1 },
                    { 1085, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 86, 1 },
                    { 1086, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 87, 1 },
                    { 1087, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 88, 1 },
                    { 1088, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 89, 1 },
                    { 1089, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90, 1 },
                    { 1090, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 91, 1 },
                    { 1091, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 92, 1 },
                    { 1092, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 93, 1 },
                    { 1093, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 94, 1 },
                    { 1094, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 95, 1 },
                    { 1095, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 96, 1 },
                    { 1096, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 97, 1 },
                    { 1097, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 98, 1 },
                    { 1098, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 99, 1 },
                    { 1099, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, 1 },
                    { 1100, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 101, 1 },
                    { 1101, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 102, 1 },
                    { 1102, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 103, 1 },
                    { 1103, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 104, 1 },
                    { 1104, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 105, 1 },
                    { 1105, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 106, 1 },
                    { 1106, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 107, 1 },
                    { 1107, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 108, 1 },
                    { 1108, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 109, 1 },
                    { 1109, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 110, 1 },
                    { 1110, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 111, 1 },
                    { 1111, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 112, 1 },
                    { 1112, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 113, 1 },
                    { 1113, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 114, 1 },
                    { 1114, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 115, 1 },
                    { 1115, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 116, 1 },
                    { 1116, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 117, 1 },
                    { 1117, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 118, 1 },
                    { 1118, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 119, 1 },
                    { 1119, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 120, 1 },
                    { 1120, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 121, 1 },
                    { 1121, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 122, 1 },
                    { 1122, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 123, 1 },
                    { 1123, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 124, 1 },
                    { 1124, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 125, 1 },
                    { 1125, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 126, 1 },
                    { 1126, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 127, 1 },
                    { 1127, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2 },
                    { 1128, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2 },
                    { 1129, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 2 },
                    { 1130, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 2 },
                    { 1131, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 2 },
                    { 1132, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, 2 },
                    { 1133, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, 2 },
                    { 1134, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, 2 },
                    { 1135, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, 2 },
                    { 1136, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, 2 },
                    { 1137, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, 2 },
                    { 1138, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, 2 },
                    { 1139, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, 2 },
                    { 1140, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, 2 },
                    { 1141, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, 2 },
                    { 1142, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 49, 2 },
                    { 1143, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 54, 2 },
                    { 1144, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 77, 2 },
                    { 1145, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, 2 },
                    { 1146, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, 2 },
                    { 1147, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, 2 },
                    { 1148, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, 2 },
                    { 1149, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, 2 },
                    { 1150, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 50, 2 },
                    { 1151, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 55, 2 },
                    { 1152, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 78, 2 },
                    { 1153, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, 2 },
                    { 1154, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, 2 },
                    { 1155, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 60, 2 },
                    { 1156, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 84, 2 },
                    { 1157, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 85, 2 },
                    { 1158, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, 2 },
                    { 1159, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, 2 },
                    { 1160, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 52, 2 },
                    { 1161, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 57, 2 },
                    { 1162, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 80, 2 },
                    { 1163, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, 2 },
                    { 1164, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, 2 },
                    { 1165, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 53, 2 },
                    { 1166, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 56, 2 },
                    { 1167, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 79, 2 },
                    { 1168, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, 2 },
                    { 1169, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, 2 },
                    { 1170, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 61, 2 },
                    { 1171, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 86, 2 },
                    { 1172, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 87, 2 },
                    { 1173, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, 2 },
                    { 1174, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, 2 },
                    { 1175, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 51, 2 },
                    { 1176, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 58, 2 },
                    { 1177, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 81, 2 },
                    { 1178, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, 2 },
                    { 1179, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, 2 },
                    { 1180, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 62, 2 },
                    { 1181, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90, 2 },
                    { 1182, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 91, 2 },
                    { 1183, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, 2 },
                    { 1184, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, 2 },
                    { 1185, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 63, 2 },
                    { 1186, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 65, 2 },
                    { 1187, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 83, 2 },
                    { 1188, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 88, 2 },
                    { 1189, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 89, 2 },
                    { 1190, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 46, 2 },
                    { 1191, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 82, 2 },
                    { 1192, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 66, 2 },
                    { 1193, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 2 },
                    { 1194, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 112, 2 },
                    { 1195, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 117, 2 },
                    { 1196, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 121, 2 },
                    { 1197, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 122, 2 },
                    { 1198, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 123, 2 },
                    { 1199, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 125, 2 },
                    { 1200, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 126, 2 },
                    { 1201, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 127, 2 },
                    { 1202, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3 },
                    { 1203, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 3 },
                    { 1204, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 3 },
                    { 1205, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 3 },
                    { 1206, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, 3 },
                    { 1207, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, 3 },
                    { 1208, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, 3 },
                    { 1209, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, 3 },
                    { 1210, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, 3 },
                    { 1211, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 49, 3 },
                    { 1212, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 54, 3 },
                    { 1213, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, 3 },
                    { 1214, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, 3 },
                    { 1215, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, 3 },
                    { 1216, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, 3 },
                    { 1217, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 50, 3 },
                    { 1218, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 55, 3 },
                    { 1219, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, 3 },
                    { 1220, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, 3 },
                    { 1221, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 60, 3 },
                    { 1222, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 84, 3 },
                    { 1223, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, 3 },
                    { 1224, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, 3 },
                    { 1225, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 52, 3 },
                    { 1226, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 57, 3 },
                    { 1227, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, 3 },
                    { 1228, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, 3 },
                    { 1229, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 53, 3 },
                    { 1230, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 56, 3 },
                    { 1231, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, 3 },
                    { 1232, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, 3 },
                    { 1233, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 61, 3 },
                    { 1234, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 86, 3 },
                    { 1235, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, 3 },
                    { 1236, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, 3 },
                    { 1237, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 51, 3 },
                    { 1238, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 58, 3 },
                    { 1239, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 46, 3 },
                    { 1240, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 47, 3 },
                    { 1241, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 48, 3 },
                    { 1242, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 59, 3 },
                    { 1243, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 3 },
                    { 1244, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 121, 3 },
                    { 1245, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 122, 3 },
                    { 1246, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 4 },
                    { 1247, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 4 },
                    { 1248, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, 4 },
                    { 1249, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, 4 },
                    { 1250, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, 4 },
                    { 1251, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, 4 },
                    { 1252, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 49, 4 },
                    { 1253, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 54, 4 },
                    { 1254, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, 4 },
                    { 1255, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, 4 },
                    { 1256, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 50, 4 },
                    { 1257, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 55, 4 },
                    { 1258, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, 4 },
                    { 1259, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, 4 },
                    { 1260, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 52, 4 },
                    { 1261, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 57, 4 },
                    { 1262, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, 4 },
                    { 1263, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 53, 4 },
                    { 1264, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 56, 4 },
                    { 1265, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, 4 },
                    { 1266, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, 4 },
                    { 1267, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, 4 },
                    { 1268, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 51, 4 },
                    { 1269, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 58, 4 },
                    { 1270, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, 4 },
                    { 1271, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, 4 },
                    { 1272, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, 4 },
                    { 1273, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 63, 4 },
                    { 1274, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 65, 4 },
                    { 1275, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 4 },
                    { 1276, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, 4 },
                    { 1277, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, 4 },
                    { 1278, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 64, 4 },
                    { 1279, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 88, 4 },
                    { 1280, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 46, 4 },
                    { 1281, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 47, 4 },
                    { 1282, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 48, 4 },
                    { 1283, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 59, 4 },
                    { 1284, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 66, 4 },
                    { 1285, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 67, 4 },
                    { 1286, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 68, 4 },
                    { 1287, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 69, 4 },
                    { 1288, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 70, 4 },
                    { 1289, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 71, 4 },
                    { 1290, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 72, 4 },
                    { 1291, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 92, 4 },
                    { 1292, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 93, 4 },
                    { 1293, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 73, 4 },
                    { 1294, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 75, 4 },
                    { 1295, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 95, 4 },
                    { 1296, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 96, 4 },
                    { 1297, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 98, 4 },
                    { 1298, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 99, 4 },
                    { 1299, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 113, 4 },
                    { 1300, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 114, 4 },
                    { 1301, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 115, 4 },
                    { 1302, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 116, 4 },
                    { 1303, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 117, 4 },
                    { 1304, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 118, 4 },
                    { 1305, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 119, 4 },
                    { 1306, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 120, 4 },
                    { 1307, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 121, 4 },
                    { 1308, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 5 },
                    { 1309, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, 5 },
                    { 1310, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, 5 },
                    { 1311, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, 5 },
                    { 1312, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, 5 },
                    { 1313, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, 5 },
                    { 1314, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, 5 },
                    { 1315, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, 5 },
                    { 1316, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, 5 },
                    { 1317, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, 5 },
                    { 1318, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, 5 },
                    { 1319, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, 5 },
                    { 1320, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, 5 },
                    { 1321, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 5 },
                    { 1322, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, 5 },
                    { 1323, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 46, 5 },
                    { 1324, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 66, 5 },
                    { 1325, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 71, 5 },
                    { 1326, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 73, 5 },
                    { 1327, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 113, 5 },
                    { 1328, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 117, 5 },
                    { 1329, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 121, 5 },
                    { 1330, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 125, 5 },
                    { 1331, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 126, 5 },
                    { 1332, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 127, 5 },
                    { 1333, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 6 },
                    { 1334, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, 6 },
                    { 1335, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, 6 },
                    { 1336, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, 6 },
                    { 1337, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, 6 },
                    { 1338, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, 6 },
                    { 1339, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, 6 },
                    { 1340, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 60, 6 },
                    { 1341, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 84, 6 },
                    { 1342, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, 6 },
                    { 1343, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, 6 },
                    { 1344, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 52, 6 },
                    { 1345, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 57, 6 },
                    { 1346, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, 6 },
                    { 1347, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, 6 },
                    { 1348, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 53, 6 },
                    { 1349, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 56, 6 },
                    { 1350, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, 6 },
                    { 1351, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, 6 },
                    { 1352, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 61, 6 },
                    { 1353, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 86, 6 },
                    { 1354, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, 6 },
                    { 1355, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 6 },
                    { 1356, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 121, 6 },
                    { 1357, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 122, 6 },
                    { 1358, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 123, 6 },
                    { 1359, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 73, 7 },
                    { 1360, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 74, 7 },
                    { 1361, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 95, 7 },
                    { 1362, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 96, 7 },
                    { 1363, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 7 }
                });

            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "SectionID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "DepartmentID", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "PEN_RESEARCH", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, true, false, "قسم البحث الاجتماعي والفحص", null },
                    { 2, "PEN_SETTLE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, true, false, "قسم التسوية والربط", null },
                    { 3, "PEN_ENTITLE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, true, false, "قسم الاستحقاقات", null },
                    { 4, "PEN_RECORDS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, true, false, "قسم السجلات والأحصاء", null },
                    { 5, "PEN_IT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, true, false, "قسم الحاسب الآلي", null },
                    { 6, "PEN_DOCS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, true, false, "قسم التوثيق", null },
                    { 7, "ADM_SECRET", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, null, true, false, "قسم السكرتارية", null },
                    { 8, "ADM_PRINT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, null, true, false, "قسم الطباعة والنسخ", null },
                    { 9, "ADM_FINANCE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, null, true, false, "قسم الشئون المالية والإدارية", null },
                    { 10, "INV_STUDIES", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, null, true, false, "قسم الدراسات والجدوى الاقتصادي", null },
                    { 11, "INV_DESIGN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, null, true, false, "قسم التصاميم", null },
                    { 12, "INV_TRADE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, null, true, false, "قسم العلاقات التجارية", null },
                    { 13, "INV_EXEC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, null, true, false, "قسم التنفيذ والمتابعة", null },
                    { 14, "FIN_BUDGET", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, true, false, "قسم الميزانية والحسابات", null },
                    { 15, "FIN_REVENUE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, true, false, "قسم الإيرادات", null },
                    { 16, "FIN_EXPENSES", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, true, false, "قسم المصروفات والمدفوعات", null },
                    { 17, "FIN_AUDIT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, true, false, "قسم المراجعة والرقابة المالية", null },
                    { 18, "FIN_PURCH", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, true, false, "قسم المشتروات والمخازن", null },
                    { 19, "FIN_TREASURY", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, true, false, "قسم الخزينة", null },
                    { 20, "LEG_CONSULT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, null, true, false, "قسم الاستشارات القانونية", null },
                    { 21, "LEG_INVEST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, null, true, false, "قسم التحقيقات والمرافعات", null }
                });

            migrationBuilder.InsertData(
                table: "TablePermissions",
                columns: new[] { "PermissionID", "CanCreate", "CanDelete", "CanRead", "CanUpdate", "CreatedAt", "RoleID", "TableID", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, null },
                    { 2, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2, null },
                    { 3, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3, null },
                    { 4, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, null },
                    { 5, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5, null },
                    { 6, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 6, null },
                    { 7, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 7, null },
                    { 8, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8, null },
                    { 9, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 9, null },
                    { 10, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 10, null },
                    { 11, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 11, null },
                    { 12, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 12, null },
                    { 13, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 13, null },
                    { 14, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 14, null },
                    { 15, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 15, null },
                    { 16, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 16, null },
                    { 17, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 17, null },
                    { 18, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 18, null },
                    { 19, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 19, null },
                    { 20, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 20, null },
                    { 21, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 21, null },
                    { 22, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 22, null },
                    { 23, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 23, null },
                    { 24, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 24, null },
                    { 25, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 25, null },
                    { 26, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 26, null },
                    { 27, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 27, null },
                    { 28, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 28, null },
                    { 29, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 29, null },
                    { 30, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 30, null },
                    { 31, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 31, null },
                    { 32, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 32, null },
                    { 33, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 33, null },
                    { 34, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 34, null },
                    { 35, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 35, null },
                    { 36, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 36, null },
                    { 37, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 37, null },
                    { 38, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 38, null },
                    { 39, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 39, null },
                    { 40, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 40, null },
                    { 41, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 41, null },
                    { 42, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 42, null },
                    { 43, true, true, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 43, null },
                    { 44, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, null },
                    { 45, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2, null },
                    { 46, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 3, null },
                    { 47, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 4, null },
                    { 48, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 5, null },
                    { 49, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 6, null },
                    { 50, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 7, null },
                    { 51, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 8, null },
                    { 52, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 9, null },
                    { 53, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 10, null },
                    { 54, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 11, null },
                    { 55, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 12, null },
                    { 56, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 13, null },
                    { 57, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 14, null },
                    { 58, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 15, null },
                    { 59, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 16, null },
                    { 60, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 17, null },
                    { 61, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 18, null },
                    { 62, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 19, null },
                    { 63, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 20, null },
                    { 64, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 21, null },
                    { 65, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 22, null },
                    { 66, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 23, null },
                    { 67, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 24, null },
                    { 68, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 25, null },
                    { 69, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 26, null },
                    { 70, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 29, null },
                    { 71, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 30, null },
                    { 72, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 31, null },
                    { 73, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 32, null },
                    { 74, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 33, null },
                    { 75, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 34, null },
                    { 76, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 35, null },
                    { 77, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 36, null },
                    { 78, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 37, null },
                    { 79, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 38, null },
                    { 80, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 39, null },
                    { 81, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 40, null },
                    { 82, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 41, null },
                    { 83, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 42, null },
                    { 84, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 43, null },
                    { 85, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, null },
                    { 86, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 2, null },
                    { 87, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 3, null },
                    { 88, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 4, null },
                    { 89, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 5, null },
                    { 90, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 6, null },
                    { 91, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 7, null },
                    { 92, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 8, null },
                    { 93, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 9, null },
                    { 94, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 10, null },
                    { 95, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 11, null },
                    { 96, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 12, null },
                    { 97, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 13, null },
                    { 98, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 14, null },
                    { 99, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 15, null },
                    { 100, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 16, null },
                    { 101, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 17, null },
                    { 102, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 18, null },
                    { 103, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 19, null },
                    { 104, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 20, null },
                    { 105, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 29, null },
                    { 106, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 30, null },
                    { 107, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 31, null },
                    { 108, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 32, null },
                    { 109, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 33, null },
                    { 110, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 34, null },
                    { 111, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 35, null },
                    { 112, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 36, null },
                    { 113, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 37, null },
                    { 114, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 38, null },
                    { 115, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 39, null },
                    { 116, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 40, null },
                    { 117, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 41, null },
                    { 118, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 42, null },
                    { 119, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 43, null },
                    { 120, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, null },
                    { 121, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 2, null },
                    { 122, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 3, null },
                    { 123, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 4, null },
                    { 124, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 5, null },
                    { 125, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 6, null },
                    { 126, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 7, null },
                    { 127, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 8, null },
                    { 128, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 9, null },
                    { 129, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 10, null },
                    { 130, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 11, null },
                    { 131, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 12, null },
                    { 132, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 13, null },
                    { 133, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 14, null },
                    { 134, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 15, null },
                    { 135, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 16, null },
                    { 136, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 17, null },
                    { 137, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 18, null },
                    { 138, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 19, null },
                    { 139, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 20, null },
                    { 140, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 29, null },
                    { 141, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 30, null },
                    { 142, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 31, null },
                    { 143, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 32, null },
                    { 144, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 33, null },
                    { 145, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 34, null },
                    { 146, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 35, null },
                    { 147, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 36, null },
                    { 148, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 37, null },
                    { 149, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 38, null },
                    { 150, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 39, null },
                    { 151, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 40, null },
                    { 152, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 41, null },
                    { 153, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 42, null },
                    { 154, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 43, null },
                    { 155, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 1, null },
                    { 156, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 2, null },
                    { 157, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 3, null },
                    { 158, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 4, null },
                    { 159, false, false, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 5, null },
                    { 160, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 6, null },
                    { 161, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 7, null },
                    { 162, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 8, null },
                    { 163, true, false, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 9, null }
                });

            migrationBuilder.InsertData(
                table: "BudgetItems",
                columns: new[] { "ItemID", "Code", "CreatedAt", "DeletedAt", "DeletedByUserID", "Description", "IsActive", "IsDeleted", "Name", "SectionID", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "3111", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "معاشات وأجور دائمة", 1, null },
                    { 2, "3112", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "معاشات وأجور تعاقدية", 1, null },
                    { 3, "3121", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تمثيل + مظهر", 2, null },
                    { 4, "3122", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل مناطق حرة (كادر) + الأمن", 2, null },
                    { 5, "31233", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل حضور جلسات", 2, null },
                    { 6, "3124", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تخرج", 2, null },
                    { 7, "3125", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل مسؤولية", 2, null },
                    { 8, "3126", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تعويض السكن", 2, null },
                    { 9, "3127", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل تعويض العمل الإضافي", 2, null },
                    { 10, "3128", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "بدل انتقال", 2, null },
                    { 11, "3131", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "تكاليف أغذية العاملين", 3, null },
                    { 12, "3132", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "تكاليف ملابس العاملين", 3, null },
                    { 13, "3133", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "خدمات ثقافية واجتماعية للعاملين", 3, null },
                    { 14, "3134", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "تكاليف علاج العاملين", 3, null },
                    { 15, "3141", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "المكافآت التشجيعية", 4, null },
                    { 16, "3142", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "إكرامية رمضان", 4, null },
                    { 17, "3161", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "حصة الحكومة (قسط التقاعد + مساهمة الموظف)", 5, null },
                    { 18, "3162", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "حصة صندوق التأمين إصابة العمل", 5, null },
                    { 19, "3221", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مواد بترولية", 6, null },
                    { 20, "3222", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "كهرباء", 6, null },
                    { 21, "3223", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مياه", 6, null },
                    { 22, "3224", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "زيوت وشحوم", 6, null },
                    { 23, "3225", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "غاز", 6, null },
                    { 24, "3231", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "قطع غيار الآلات والسيارات", 7, null },
                    { 25, "3232", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "لوازم ومهمات الصيانة", 7, null },
                    { 26, "3233", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "قطع غيار التجهيزات الإدارية", 7, null },
                    { 27, "3251", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "القرطاسية واللوازم المكتبية", 8, null },
                    { 28, "3252", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مستلزمات سلعية متنوعة", 8, null },
                    { 29, "3253", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "مطبوعات دورية", 8, null },
                    { 30, "3254", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "شتول وأغراس", 8, null }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeID", "AcademicQualifications", "Address", "BirthDate", "BirthPlace", "BloodGroup", "CreatedAt", "DeletedAt", "DeletedByUserID", "DepartmentID", "EmergencyContactName", "EmergencyContactPhone", "EmployeeNumber", "EmploymentStatusID", "FullName", "Gender", "HireDate", "IsDeleted", "JobPositions", "JobTitleID", "MaritalStatus", "MilitaryNumber", "MilitaryServiceDate", "NationalID", "NationalIDExpiryDate", "NationalIDIssueDate", "NationalIDIssuePlace", "Notes", "Phone", "ProfilePhotoPath", "RankID", "SectionID", "UpdatedAt" },
                values: new object[,]
                {
                    { 7, "دبلوم + 7 دورات كمبيوتر", "مديرية الشيخ عثمان/الممدارة", new DateTime(1973, 3, 6, 0, 0, 0, 0, DateTimeKind.Utc), "عدن/صيرة", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, null, "EMP-007", 1, "خالد صالح بن صالح", (byte)1, new DateTime(1991, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "نائب المدير المالي رئيس الحاسب الآلي والحسابات", 5, "متزوج", "27940", new DateTime(1991, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "291268142893", null, null, null, null, "778306198", null, 3, 14, null },
                    { 8, "ثانوية عامة", "عدن/دار سعد/الميدان", new DateTime(1987, 8, 13, 0, 0, 0, 0, DateTimeKind.Utc), "عدن/دار سعد", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, null, "EMP-008", 1, "آمنة علي عياش سالم", (byte)0, new DateTime(2008, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), false, "رئيس قسم الأرشيف", 5, "متزوجة", "185650", new DateTime(2008, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "108121943248", null, null, null, null, "783421792", null, 17, 6, null },
                    { 9, "بكالوريوس قسم حاسوب", "خورمكسر/العريش", new DateTime(1994, 5, 18, 0, 0, 0, 0, DateTimeKind.Utc), "عدن/صيرة", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, null, "EMP-009", 1, "رنا علي هربي يوسف", (byte)0, new DateTime(2015, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "مختص البحث الاجتماعي", 6, "عازبة", "305561", new DateTime(2015, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "495335740730", null, null, null, null, "772373136", null, 12, 1, null },
                    { 10, null, null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, null, "EMP-010", 1, "أحمد الخضر أحمد", (byte)1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "مختص الأرشيف", 6, null, null, null, "", null, null, null, null, "776859111", null, 11, 6, null },
                    { 11, "ثانوية", "الثقنية", new DateTime(1988, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), "الحديدة", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, null, "EMP-011", 1, "ماهر هبه علي هبه", (byte)1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "رئيس قسم المخازن", 5, "عازب", "374013", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, null, null, "784881417", null, 11, 18, null },
                    { 12, "بكالوريوس هندسة مدنية", "عدن/الشيخ عثمان/الممدارة", new DateTime(2003, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "عدن", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, null, null, "EMP-012", 1, "عمرو خالد صالح", (byte)1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "رئيس قسم المراجعة", 5, "عازب", null, null, "", null, null, null, null, null, null, null, 17, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrders_AdministrativeOrderTypeID",
                table: "AdministrativeOrders",
                column: "AdministrativeOrderTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrders_ApprovedByUserID",
                table: "AdministrativeOrders",
                column: "ApprovedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrders_DeletedByUserID",
                table: "AdministrativeOrders",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrders_EmployeeID",
                table: "AdministrativeOrders",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrders_OrderNumber",
                table: "AdministrativeOrders",
                column: "OrderNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrders_Status",
                table: "AdministrativeOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrderTypes_Code",
                table: "AdministrativeOrderTypes",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeOrderTypes_DeletedByUserID",
                table: "AdministrativeOrderTypes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_CreatedByUserID",
                table: "Agents",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_DeletedByUserID",
                table: "Agents",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_FileID",
                table: "Agents",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_UpdatedByUserID",
                table: "Agents",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_DeletedByUserID",
                table: "ApprovalRequests",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_RequestedByUserID",
                table: "ApprovalRequests",
                column: "RequestedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetValuations_DeletedByUserID",
                table: "AssetValuations",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetValuations_PropertyID",
                table: "AssetValuations",
                column: "PropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEvents_EmployeeID_EventDate_EventType",
                table: "AttendanceEvents",
                columns: new[] { "EmployeeID", "EventDate", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEvents_RecordedByUserID",
                table: "AttendanceEvents",
                column: "RecordedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableID",
                table: "AuditLogs",
                column: "TableID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserID",
                table: "AuditLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_DeletedByUserID",
                table: "BudgetAllocations",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_ItemID",
                table: "BudgetAllocations",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocations_QuarterID_ItemID",
                table: "BudgetAllocations",
                columns: new[] { "QuarterID", "ItemID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetChapters_Code",
                table: "BudgetChapters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetChapters_DeletedByUserID",
                table: "BudgetChapters",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_Code",
                table: "BudgetItems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_DeletedByUserID",
                table: "BudgetItems",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_SectionID",
                table: "BudgetItems",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetSections_ChapterID",
                table: "BudgetSections",
                column: "ChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetSections_Code",
                table: "BudgetSections",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetSections_DeletedByUserID",
                table: "BudgetSections",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenAccounts_DeletedByUserID",
                table: "CitizenAccounts",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenAccounts_NationalID",
                table: "CitizenAccounts",
                column: "NationalID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitizenAccounts_RetireeID",
                table: "CitizenAccounts",
                column: "RetireeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitizenNotifications_RetireeID",
                table: "CitizenNotifications",
                column: "RetireeID");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondences_CorrespondenceTypeID",
                table: "Correspondences",
                column: "CorrespondenceTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondences_CreatedByUserID",
                table: "Correspondences",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondences_DeletedByUserID",
                table: "Correspondences",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondences_Direction_CorrespondenceTypeID_CorrespondenceDate",
                table: "Correspondences",
                columns: new[] { "Direction", "CorrespondenceTypeID", "CorrespondenceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Correspondences_EmployeeID",
                table: "Correspondences",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondences_ExternalPartyID",
                table: "Correspondences",
                column: "ExternalPartyID");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenceTypes_DeletedByUserID",
                table: "CorrespondenceTypes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeletedByUserID",
                table: "Departments",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationRecords_DeletedByUserID",
                table: "DepreciationRecords",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationRecords_FiscalYearID",
                table: "DepreciationRecords",
                column: "FiscalYearID");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationRecords_PropertyID_FiscalYearID",
                table: "DepreciationRecords",
                columns: new[] { "PropertyID", "FiscalYearID" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRegistrations_UserID",
                table: "DeviceRegistrations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExemptions_CreatedByUserID",
                table: "DocumentExemptions",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExemptions_DeletedByUserID",
                table: "DocumentExemptions",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExemptions_FileID",
                table: "DocumentExemptions",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPledges_CreatedByUserID",
                table: "DocumentPledges",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPledges_DeletedByUserID",
                table: "DocumentPledges",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPledges_ExtendedByUserID",
                table: "DocumentPledges",
                column: "ExtendedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPledges_FileID",
                table: "DocumentPledges",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPledges_SuspensionID",
                table: "DocumentPledges",
                column: "SuspensionID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRequirementRules_DocumentTypeID",
                table: "DocumentRequirementRules",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AgentID",
                table: "Documents",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AssetValuationID",
                table: "Documents",
                column: "AssetValuationID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CitizenAccountID",
                table: "Documents",
                column: "CitizenAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CorrespondenceID",
                table: "Documents",
                column: "CorrespondenceID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DecisionID",
                table: "Documents",
                column: "DecisionID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DeletedByUserID",
                table: "Documents",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DepreciationRecordID",
                table: "Documents",
                column: "DepreciationRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeID",
                table: "Documents",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EmployeeID",
                table: "Documents",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EntitledID",
                table: "Documents",
                column: "EntitledID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ExemptionID",
                table: "Documents",
                column: "ExemptionID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FileID",
                table: "Documents",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_GuardianID",
                table: "Documents",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InvestmentPlanID",
                table: "Documents",
                column: "InvestmentPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InvestmentPortfolioID",
                table: "Documents",
                column: "InvestmentPortfolioID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InvestmentProjectID",
                table: "Documents",
                column: "InvestmentProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InvestmentPropertyID",
                table: "Documents",
                column: "InvestmentPropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_LeaseContractID",
                table: "Documents",
                column: "LeaseContractID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_MaintenanceRequestID",
                table: "Documents",
                column: "MaintenanceRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_MinistryBatchID",
                table: "Documents",
                column: "MinistryBatchID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_NoteID",
                table: "Documents",
                column: "NoteID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OperationalExpenseID",
                table: "Documents",
                column: "OperationalExpenseID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PaymentScheduleID",
                table: "Documents",
                column: "PaymentScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PledgeID",
                table: "Documents",
                column: "PledgeID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PropertyUnitID",
                table: "Documents",
                column: "PropertyUnitID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ReportID",
                table: "Documents",
                column: "ReportID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_RetireeID",
                table: "Documents",
                column: "RetireeID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_RiskAssessmentID",
                table: "Documents",
                column: "RiskAssessmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SalaryBatchID",
                table: "Documents",
                column: "SalaryBatchID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ServiceRequestID",
                table: "Documents",
                column: "ServiceRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SettlementBatchID",
                table: "Documents",
                column: "SettlementBatchID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SuspensionID",
                table: "Documents",
                column: "SuspensionID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TenantID",
                table: "Documents",
                column: "TenantID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TicketMessageID",
                table: "Documents",
                column: "TicketMessageID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedByUserID",
                table: "Documents",
                column: "UploadedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VerifiedByUserID",
                table: "Documents",
                column: "VerifiedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_WorkOrderID",
                table: "Documents",
                column: "WorkOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_OwnerTableID",
                table: "DocumentTypes",
                column: "OwnerTableID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DeletedByUserID",
                table: "Employees",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentID_SectionID",
                table: "Employees",
                columns: new[] { "DepartmentID", "SectionID" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmploymentStatusID",
                table: "Employees",
                column: "EmploymentStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_JobTitleID",
                table: "Employees",
                column: "JobTitleID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NationalID",
                table: "Employees",
                column: "NationalID",
                unique: true,
                filter: "[NationalID] <> '' AND [NationalID] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_RankID",
                table: "Employees",
                column: "RankID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SectionID",
                table: "Employees",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentStatuses_Code",
                table: "EmploymentStatuses",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentStatuses_DeletedByUserID",
                table: "EmploymentStatuses",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentStatuses_Name",
                table: "EmploymentStatuses",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EntitledPersons_CreatedByUserID",
                table: "EntitledPersons",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_EntitledPersons_DeletedByUserID",
                table: "EntitledPersons",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_EntitledPersons_FileID_NationalID",
                table: "EntitledPersons",
                columns: new[] { "FileID", "NationalID" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [NationalID] <> '' AND [NationalID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EntitledPersons_HealthStatusID",
                table: "EntitledPersons",
                column: "HealthStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_EntitledPersons_RelationshipID",
                table: "EntitledPersons",
                column: "RelationshipID");

            migrationBuilder.CreateIndex(
                name: "IX_EntitledPersons_SocialStatusID",
                table: "EntitledPersons",
                column: "SocialStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_EntitledPersons_UpdatedByUserID",
                table: "EntitledPersons",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalParties_DeletedByUserID",
                table: "ExternalParties",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FileNotes_CreatedByUserID",
                table: "FileNotes",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FileNotes_DeletedByUserID",
                table: "FileNotes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FileNotes_FileID",
                table: "FileNotes",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_FileNotes_ResolvedByUserID",
                table: "FileNotes",
                column: "ResolvedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FileReviewItems_AuditLogID",
                table: "FileReviewItems",
                column: "AuditLogID");

            migrationBuilder.CreateIndex(
                name: "IX_FileReviewItems_RetirementFileID",
                table: "FileReviewItems",
                column: "RetirementFileID");

            migrationBuilder.CreateIndex(
                name: "IX_FileReviewItems_ReviewedByUserID",
                table: "FileReviewItems",
                column: "ReviewedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FileSnapshots_FileId_Version",
                table: "FileSnapshots",
                columns: new[] { "FileId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialEntitlements_BatchID",
                table: "FinancialEntitlements",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialEntitlements_DeletedByUserID",
                table: "FinancialEntitlements",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialEntitlements_RetireeID_EffectiveDate_EndDate_IsDeleted",
                table: "FinancialEntitlements",
                columns: new[] { "RetireeID", "EffectiveDate", "EndDate", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_DeletedByUserID",
                table: "FiscalYears",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_GuardianEntitledPersons_DeletedByUserID",
                table: "GuardianEntitledPersons",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_GuardianEntitledPersons_EntitledID",
                table: "GuardianEntitledPersons",
                column: "EntitledID");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_CreatedByUserID",
                table: "Guardians",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_DeletedByUserID",
                table: "Guardians",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_EntitledPersonEntitledID",
                table: "Guardians",
                column: "EntitledPersonEntitledID");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_FileID",
                table: "Guardians",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_UpdatedByUserID",
                table: "Guardians",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_CategoryName",
                table: "InventoryCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_CreatedByUserID",
                table: "InventoryCategories",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_UpdatedByUserID",
                table: "InventoryCategories",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustodies_AssignedByUserID",
                table: "InventoryCustodies",
                column: "AssignedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustodies_CreatedByUserID",
                table: "InventoryCustodies",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustodies_DeletedByUserID",
                table: "InventoryCustodies",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustodies_EmployeeID",
                table: "InventoryCustodies",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustodies_InventoryItemID",
                table: "InventoryCustodies",
                column: "InventoryItemID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustodies_ReturnedToUserID",
                table: "InventoryCustodies",
                column: "ReturnedToUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustodies_UpdatedByUserID",
                table: "InventoryCustodies",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CategoryID",
                table: "InventoryItems",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CreatedByUserID",
                table: "InventoryItems",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_DeletedByUserID",
                table: "InventoryItems",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemCode",
                table: "InventoryItems",
                column: "ItemCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_UpdatedByUserID",
                table: "InventoryItems",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_BatchID",
                table: "InventoryTransactions",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_InventoryItemID",
                table: "InventoryTransactions",
                column: "InventoryItemID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_PerformedByUserID",
                table: "InventoryTransactions",
                column: "PerformedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPlans_DeletedByUserID",
                table: "InvestmentPlans",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPlans_FiscalYearID",
                table: "InvestmentPlans",
                column: "FiscalYearID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPlans_ProjectID_FiscalYearID",
                table: "InvestmentPlans",
                columns: new[] { "ProjectID", "FiscalYearID" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPortfolios_DeletedByUserID",
                table: "InvestmentPortfolios",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPortfolios_ManagerUserID",
                table: "InvestmentPortfolios",
                column: "ManagerUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPortfolios_Name",
                table: "InvestmentPortfolios",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProjects_DeletedByUserID",
                table: "InvestmentProjects",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProjects_PensionBranchID",
                table: "InvestmentProjects",
                column: "PensionBranchID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProjects_PortfolioID_Name",
                table: "InvestmentProjects",
                columns: new[] { "PortfolioID", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProjects_TypeID",
                table: "InvestmentProjects",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProperties_DeletedByUserID",
                table: "InvestmentProperties",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProperties_PensionBranchID",
                table: "InvestmentProperties",
                column: "PensionBranchID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProperties_ProjectID",
                table: "InvestmentProperties",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProperties_PropertyTypeID",
                table: "InvestmentProperties",
                column: "PropertyTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTypes_DeletedByUserID",
                table: "InvestmentTypes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTypes_TypeName_Code",
                table: "InvestmentTypes",
                columns: new[] { "TypeName", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_Code",
                table: "JobTitles",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_DeletedByUserID",
                table: "JobTitles",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_Name",
                table: "JobTitles",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseContracts_DeletedByUserID",
                table: "LeaseContracts",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseContracts_TenantID",
                table: "LeaseContracts",
                column: "TenantID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseContracts_UnitID",
                table: "LeaseContracts",
                column: "UnitID",
                unique: true,
                filter: "[Status] = 'Active' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApprovedByUserID",
                table: "LeaveRequests",
                column: "ApprovedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_DeletedByUserID",
                table: "LeaveRequests",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeID_StartDate_EndDate",
                table: "LeaveRequests",
                columns: new[] { "EmployeeID", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeID",
                table: "LeaveRequests",
                column: "LeaveTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Code",
                table: "LeaveTypes",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_DeletedByUserID",
                table: "LeaveTypes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserID",
                table: "LoginHistories",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_DeletedByUserID",
                table: "MaintenanceRequests",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_PropertyID",
                table: "MaintenanceRequests",
                column: "PropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_TenantID",
                table: "MaintenanceRequests",
                column: "TenantID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_UnitID",
                table: "MaintenanceRequests",
                column: "UnitID");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryApprovalBatches_DeletedByUserID",
                table: "MinistryApprovalBatches",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryApprovalBatches_ImportedByUserID",
                table: "MinistryApprovalBatches",
                column: "ImportedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryApprovalItems_BatchID",
                table: "MinistryApprovalItems",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryApprovalItems_CategoryID",
                table: "MinistryApprovalItems",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryApprovalItems_DeletedByUserID",
                table: "MinistryApprovalItems",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryApprovalItems_ProcessedByUserID",
                table: "MinistryApprovalItems",
                column: "ProcessedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryApprovalItems_RankID",
                table: "MinistryApprovalItems",
                column: "RankID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_AgentID",
                table: "MissingDocuments",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_DeletedByUserID",
                table: "MissingDocuments",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_DocumentTypeID",
                table: "MissingDocuments",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_EntitledID",
                table: "MissingDocuments",
                column: "EntitledID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_ExemptionID",
                table: "MissingDocuments",
                column: "ExemptionID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_FileID",
                table: "MissingDocuments",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_GuardianID",
                table: "MissingDocuments",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_MissingDocuments_PledgeID",
                table: "MissingDocuments",
                column: "PledgeID");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttachments_DeletedByUserID",
                table: "NotificationAttachments",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttachments_NotificationID",
                table: "NotificationAttachments",
                column: "NotificationID");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_NotificationId",
                table: "NotificationOutbox",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_Status_RetryCount",
                table: "NotificationOutbox",
                columns: new[] { "Status", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorID",
                table: "Notifications",
                column: "ActorID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedTableID",
                table: "Notifications",
                column: "RelatedTableID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_BudgetAllocationID",
                table: "OperationalExpenses",
                column: "BudgetAllocationID");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_CreatedByUserID",
                table: "OperationalExpenses",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_DeletedByUserID",
                table: "OperationalExpenses",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_CitizenAccountID",
                table: "OtpVerifications",
                column: "CitizenAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBooks_DeletedByUserID",
                table: "PaymentBooks",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBooks_FileID",
                table: "PaymentBooks",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedules_ContractID",
                table: "PaymentSchedules",
                column: "ContractID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedules_DeletedByUserID",
                table: "PaymentSchedules",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTasks_AssignedToRoleId",
                table: "PendingTasks",
                column: "AssignedToRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTasks_AssignedToUserId",
                table: "PendingTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTasks_DeletedByUserID",
                table: "PendingTasks",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTasks_FileId",
                table: "PendingTasks",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTasks_SalaryBatchID",
                table: "PendingTasks",
                column: "SalaryBatchID");

            migrationBuilder.CreateIndex(
                name: "IX_PensionFundBranches_ParentBranchId",
                table: "PensionFundBranches",
                column: "ParentBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PensionSuspensions_CreatedByUserID",
                table: "PensionSuspensions",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PensionSuspensions_DeletedByUserID",
                table: "PensionSuspensions",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PensionSuspensions_EndedByUserID",
                table: "PensionSuspensions",
                column: "EndedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PensionSuspensions_FileID",
                table: "PensionSuspensions",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyTypes_DeletedByUserID",
                table: "PropertyTypes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyTypes_TypeName",
                table: "PropertyTypes",
                column: "TypeName",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_DeletedByUserID",
                table: "PropertyUnits",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_PropertyID_UnitNumber",
                table: "PropertyUnits",
                columns: new[] { "PropertyID", "UnitNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_UnitTypeID",
                table: "PropertyUnits",
                column: "UnitTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Quarters_DeletedByUserID",
                table: "Quarters",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Quarters_FiscalYearID",
                table: "Quarters",
                column: "FiscalYearID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_CreatedByUserID",
                table: "ReportTemplates",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Retirees_ApprovalItemID",
                table: "Retirees",
                column: "ApprovalItemID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Retirees_CreatedByUserID",
                table: "Retirees",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Retirees_DeletedByUserID",
                table: "Retirees",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Retirees_HealthStatusID",
                table: "Retirees",
                column: "HealthStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Retirees_NationalID",
                table: "Retirees",
                column: "NationalID",
                unique: true,
                filter: "[NationalID] <> '' AND [NationalID] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Retirees_SocialStatusID",
                table: "Retirees",
                column: "SocialStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Retirees_UpdatedByUserID",
                table: "Retirees",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_AssignedEmployeeID",
                table: "RetirementFiles",
                column: "AssignedEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_DeletedByUserID",
                table: "RetirementFiles",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_FileNumber",
                table: "RetirementFiles",
                column: "FileNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_PensionBranchID",
                table: "RetirementFiles",
                column: "PensionBranchID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_RankID",
                table: "RetirementFiles",
                column: "RankID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_RetireeID",
                table: "RetirementFiles",
                column: "RetireeID");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementFiles_StatusID",
                table: "RetirementFiles",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_DeletedByUserID",
                table: "RiskAssessments",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_ProjectID",
                table: "RiskAssessments",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_RiskEvents_FileId",
                table: "RiskEvents",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskEvents_ResolvedByUserId",
                table: "RiskEvents",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentBatches_ApprovedByUserID",
                table: "SalaryPaymentBatches",
                column: "ApprovedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentBatches_BankID",
                table: "SalaryPaymentBatches",
                column: "BankID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentBatches_DeletedByUserID",
                table: "SalaryPaymentBatches",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentBatches_ImportedByUserID",
                table: "SalaryPaymentBatches",
                column: "ImportedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_BatchID",
                table: "SalaryPaymentItems",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_CreatedByUserID",
                table: "SalaryPaymentItems",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_DeletedByUserID",
                table: "SalaryPaymentItems",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_FileID_BatchID",
                table: "SalaryPaymentItems",
                columns: new[] { "FileID", "BatchID" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_PaymentBookID",
                table: "SalaryPaymentItems",
                column: "PaymentBookID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_RecipientID",
                table: "SalaryPaymentItems",
                column: "RecipientID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_RetireeID",
                table: "SalaryPaymentItems",
                column: "RetireeID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPaymentItems_StatusID",
                table: "SalaryPaymentItems",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_AgentID",
                table: "SalaryRecipients",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_DeletedByUserID",
                table: "SalaryRecipients",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_EntitledID",
                table: "SalaryRecipients",
                column: "EntitledID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_FileID_IsActive",
                table: "SalaryRecipients",
                columns: new[] { "FileID", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_GuardianID",
                table: "SalaryRecipients",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRecipients_RetireeID",
                table: "SalaryRecipients",
                column: "RetireeID");

            migrationBuilder.CreateIndex(
                name: "IX_SavedReports_GeneratedByUserID",
                table: "SavedReports",
                column: "GeneratedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SavedReports_ReportTemplateTemplateID",
                table: "SavedReports",
                column: "ReportTemplateTemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_SavedReports_TemplateID",
                table: "SavedReports",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_Code",
                table: "Sections",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DeletedByUserID",
                table: "Sections",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentID_Name",
                table: "Sections",
                columns: new[] { "DepartmentID", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHistoryEvents_EmployeeID_EventDate",
                table: "ServiceHistoryEvents",
                columns: new[] { "EmployeeID", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CitizenAccountID",
                table: "ServiceRequests",
                column: "CitizenAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_DeletedByUserID",
                table: "ServiceRequests",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_FileID",
                table: "ServiceRequests",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ProcessedByUserID",
                table: "ServiceRequests",
                column: "ProcessedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDecisions_CreatedByUserID",
                table: "SettlementDecisions",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementDecisions_DeletedByUserID",
                table: "SettlementDecisions",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_ApprovalItemID_BatchID",
                table: "SettlementItems",
                columns: new[] { "ApprovalItemID", "BatchID" },
                unique: true,
                filter: "[ApprovalItemID] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_BatchID",
                table: "SettlementItems",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_CurrentRankID",
                table: "SettlementItems",
                column: "CurrentRankID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_DecisionID",
                table: "SettlementItems",
                column: "DecisionID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_DeletedByUserID",
                table: "SettlementItems",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_ProcessedRankID",
                table: "SettlementItems",
                column: "ProcessedRankID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_RetireeID",
                table: "SettlementItems",
                column: "RetireeID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementsBatches_CreatedByUserID",
                table: "SettlementsBatches",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementsBatches_DeletedByUserID",
                table: "SettlementsBatches",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_StatusTransitionLogs_ChangedByUserID",
                table: "StatusTransitionLogs",
                column: "ChangedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_StatusTransitionLogs_TableID",
                table: "StatusTransitionLogs",
                column: "TableID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedToUserID",
                table: "SupportTickets",
                column: "AssignedToUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CitizenAccountID",
                table: "SupportTickets",
                column: "CitizenAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_DeletedByUserID",
                table: "SupportTickets",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TicketNumber",
                table: "SupportTickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueues_DeviceID",
                table: "SyncQueues",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueues_TableID",
                table: "SyncQueues",
                column: "TableID");

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueues_UserID",
                table: "SyncQueues",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedByUserID",
                table: "SystemSettings",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TablePermissions_RoleID",
                table: "TablePermissions",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_TablePermissions_TableID",
                table: "TablePermissions",
                column: "TableID");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DeletedByUserID",
                table: "Tenants",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_NationalIDOrCR",
                table: "Tenants",
                column: "NationalIDOrCR",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_DeletedByUserID",
                table: "TicketMessages",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketID",
                table: "TicketMessages",
                column: "TicketID");

            migrationBuilder.CreateIndex(
                name: "IX_Treasuries_DeletedByUserID",
                table: "Treasuries",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryIncomes_DeletedByUserID",
                table: "TreasuryIncomes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryIncomes_QuarterID",
                table: "TreasuryIncomes",
                column: "QuarterID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryIncomes_TreasuryID",
                table: "TreasuryIncomes",
                column: "TreasuryID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_DeletedByUserID",
                table: "TreasuryTransactions",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TreasuryID",
                table: "TreasuryTransactions",
                column: "TreasuryID");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_DeletedByUserID",
                table: "UnitTypes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_TypeName",
                table: "UnitTypes",
                column: "TypeName",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_GrantedByUserId",
                table: "UserPermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId_PermissionId",
                table: "UserPermissions",
                columns: new[] { "UserId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_BranchId",
                table: "UserRoleAssignments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_RegionId",
                table: "UserRoleAssignments",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_RoleId",
                table: "UserRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId_RoleId_BranchId",
                table: "UserRoleAssignments",
                columns: new[] { "UserId", "RoleId", "BranchId" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchID",
                table: "Users",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedByUserID",
                table: "Users",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedByUserID",
                table: "Users",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_DeletedByUserID",
                table: "WorkOrders",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_RequestID",
                table: "WorkOrders",
                column: "RequestID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministrativeOrders");

            migrationBuilder.DropTable(
                name: "ApprovalRequests");

            migrationBuilder.DropTable(
                name: "AttendanceEvents");

            migrationBuilder.DropTable(
                name: "CitizenNotifications");

            migrationBuilder.DropTable(
                name: "DocumentRequirementRules");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "FileReviewItems");

            migrationBuilder.DropTable(
                name: "FileSnapshots");

            migrationBuilder.DropTable(
                name: "FinancialEntitlements");

            migrationBuilder.DropTable(
                name: "GuardianEntitledPersons");

            migrationBuilder.DropTable(
                name: "ImmutableAuditLogs");

            migrationBuilder.DropTable(
                name: "InventoryCustodies");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "LoginHistories");

            migrationBuilder.DropTable(
                name: "MissingDocuments");

            migrationBuilder.DropTable(
                name: "NotificationAttachments");

            migrationBuilder.DropTable(
                name: "NotificationOutbox");

            migrationBuilder.DropTable(
                name: "OtpVerifications");

            migrationBuilder.DropTable(
                name: "PendingTasks");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RiskEvents");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SalaryPaymentItems");

            migrationBuilder.DropTable(
                name: "ServiceHistoryEvents");

            migrationBuilder.DropTable(
                name: "SettlementItems");

            migrationBuilder.DropTable(
                name: "StatusTransitionLogs");

            migrationBuilder.DropTable(
                name: "SyncQueues");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TablePermissions");

            migrationBuilder.DropTable(
                name: "TreasuryIncomes");

            migrationBuilder.DropTable(
                name: "TreasuryTransactions");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "UserRoleAssignments");

            migrationBuilder.DropTable(
                name: "AdministrativeOrderTypes");

            migrationBuilder.DropTable(
                name: "AssetValuations");

            migrationBuilder.DropTable(
                name: "Correspondences");

            migrationBuilder.DropTable(
                name: "DepreciationRecords");

            migrationBuilder.DropTable(
                name: "FileNotes");

            migrationBuilder.DropTable(
                name: "InvestmentPlans");

            migrationBuilder.DropTable(
                name: "OperationalExpenses");

            migrationBuilder.DropTable(
                name: "PaymentSchedules");

            migrationBuilder.DropTable(
                name: "RiskAssessments");

            migrationBuilder.DropTable(
                name: "SavedReports");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "DocumentExemptions");

            migrationBuilder.DropTable(
                name: "DocumentPledges");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PaymentBooks");

            migrationBuilder.DropTable(
                name: "SalaryPaymentBatches");

            migrationBuilder.DropTable(
                name: "SalaryRecipients");

            migrationBuilder.DropTable(
                name: "TransactionStatuses");

            migrationBuilder.DropTable(
                name: "SettlementDecisions");

            migrationBuilder.DropTable(
                name: "SettlementsBatches");

            migrationBuilder.DropTable(
                name: "DeviceRegistrations");

            migrationBuilder.DropTable(
                name: "Treasuries");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "CorrespondenceTypes");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "ExternalParties");

            migrationBuilder.DropTable(
                name: "BudgetAllocations");

            migrationBuilder.DropTable(
                name: "LeaseContracts");

            migrationBuilder.DropTable(
                name: "ReportTemplates");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "MaintenanceRequests");

            migrationBuilder.DropTable(
                name: "InventoryCategories");

            migrationBuilder.DropTable(
                name: "PensionSuspensions");

            migrationBuilder.DropTable(
                name: "SystemTables");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Guardians");

            migrationBuilder.DropTable(
                name: "EmploymentStatuses");

            migrationBuilder.DropTable(
                name: "JobTitles");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "BudgetItems");

            migrationBuilder.DropTable(
                name: "Quarters");

            migrationBuilder.DropTable(
                name: "CitizenAccounts");

            migrationBuilder.DropTable(
                name: "PropertyUnits");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "EntitledPersons");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "BudgetSections");

            migrationBuilder.DropTable(
                name: "FiscalYears");

            migrationBuilder.DropTable(
                name: "InvestmentProperties");

            migrationBuilder.DropTable(
                name: "UnitTypes");

            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropTable(
                name: "RetirementFiles");

            migrationBuilder.DropTable(
                name: "BudgetChapters");

            migrationBuilder.DropTable(
                name: "InvestmentProjects");

            migrationBuilder.DropTable(
                name: "PropertyTypes");

            migrationBuilder.DropTable(
                name: "FileStatuses");

            migrationBuilder.DropTable(
                name: "Retirees");

            migrationBuilder.DropTable(
                name: "InvestmentPortfolios");

            migrationBuilder.DropTable(
                name: "InvestmentTypes");

            migrationBuilder.DropTable(
                name: "HealthStatuses");

            migrationBuilder.DropTable(
                name: "MinistryApprovalItems");

            migrationBuilder.DropTable(
                name: "SocialStatuses");

            migrationBuilder.DropTable(
                name: "MinistryApprovalBatches");

            migrationBuilder.DropTable(
                name: "Ranks");

            migrationBuilder.DropTable(
                name: "RetireeCategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PensionFundBranches");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
