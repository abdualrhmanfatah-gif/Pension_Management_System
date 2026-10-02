# Database Schema

## Pension Management System — Database Reference

---

## 1. Database Overview

| Property | Value |
|----------|-------|
| **Engine** | SQL Server Express (dev) / SQL Server (prod) |
| **Database Name** | `PensionManagementDB` |
| **Connection** | `Server=.\SQLEXPRESS;Database=PensionManagementDB;...` |
| **ORM** | Entity Framework Core 8 |
| **Migration Strategy** | Code-first with EF migrations |
| **Tables** | ~50 |
| **Seeded Data** | Roles, Permissions, Admin user |

---

## 2. Entity Relationship Diagram (Text)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          RETIREMENT FILE                                │
│  (Central entity — all other entities relate to or reference this)      │
└──────────────────────────┬──────────────────────────────────────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ▼                 ▼                 ▼
┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐
│    Retiree      │ │  Entitled    │ │   Agent          │
│  (1:1 with File)│ │  Person      │ │  (N:1 with File) │
│                 │ │  (N:1 File)  │ │                  │
└────────┬────────┘ └──────┬───────┘ └──────────────────┘
         │                 │
         │                 ├──────────────────┐
         │                 │                  │
         ▼                 ▼                  ▼
┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐
│  Guardian       │ │  Document    │ │  Missing         │
│  (1:1 with EP)  │ │  (N:1 File)  │ │  Document        │
│                 │ │              │ │  (N:1 File)      │
└─────────────────┘ └──────────────┘ └──────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        PAYMENT SYSTEM                                   │
└──────────────────────────┬──────────────────────────────────────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ▼                 ▼                 ▼
┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐
│  PaymentBook    │ │  Salary      │ │  Financial       │
│  (N:1 with File)│ │  Recipient   │ │  Entitlement     │
│                 │ │  (N:1 File)  │ │  (N:1 File)      │
└─────────────────┘ └──────┬───────┘ └──────────────────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │  SalaryPayment   │
                  │  Batch           │
                  └────────┬─────────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │  SalaryPayment   │
                  │  Item            │
                  └──────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        SETTLEMENT SYSTEM                                │
└──────────────────────────┬──────────────────────────────────────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ▼                 ▼                 ▼
┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐
│  Settlement     │ │  Settlements │ │  Ministry        │
│  Decision       │ │  Batch       │ │  Approval Item   │
│  (N:1 File)     │ │  (N:1 Batch) │ │  (N:1 File)      │
└─────────────────┘ └──────────────┘ └──────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        AUDIT & TRACKING                                 │
└──────────────────────────┬──────────────────────────────────────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ▼                 ▼                 ▼
┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐
│  FileStatus     │ │  FileSnapshot│ │  FileReviewItem  │
│  (audit log)    │ │  (immutable) │ │  (checklist)     │
└─────────────────┘ └──────────────┘ └──────────────────┘
```

---

## 3. Core Entity Definitions

### 3.1 RetirementFile (Central Entity)

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `Id` | `int` | No | Primary key (auto-increment) |
| `FileNumber` | `string` | No | Unique file reference number |
| `RetireeId` | `int` | Yes | FK to Retiree |
| `Status` | `enum` | No | Current file status |
| `BranchId` | `int` | No | FK to PensionFundBranch |
| `CreatedById` | `int` | No | FK to User (creator) |
| `CreatedAt` | `DateTime` | No | Creation timestamp |
| `UpdatedAt` | `DateTime` | No | Last modification timestamp |
| `Notes` | `string` | Yes | General notes |

**Status Enum Values:**
```
New, PendingReview, UnderReview, Approved, Rejected,
Resubmitted, Suspended, Completed, Archived
```

### 3.2 Retiree

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `Id` | `int` | No | Primary key |
| `FullName` | `string` | No | Full legal name |
| `NationalId` | `string` | No | National ID number |
| `RankId` | `int` | No | FK to Rank |
| `SocialStatusId` | `int` | No | FK to SocialStatus |
| `HealthStatusId` | `int` | No | FK to HealthStatus |
| `RetireeCategoryId` | `int` | No | FK to RetireeCategory |
| `Phone` | `string` | Yes | Contact phone |
| `Address` | `string` | Yes | Home address |
| `PhotoPath` | `string` | Yes | Photo file path |
| `CreatedAt` | `DateTime` | No | Creation timestamp |

### 3.3 EntitledPerson

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `Id` | `int` | No | Primary key |
| `RetirementFileId` | `int` | No | FK to RetirementFile |
| `FullName` | `string` | No | Full name |
| `NationalId` | `string` | No | National ID |
| `RelationshipId` | `int` | No | FK to Relationship (wife, child, etc.) |
| `DateOfBirth` | `DateTime` | No | Date of birth |
| `Percentage` | `decimal` | No | Entitlement percentage |
| `IsMinor` | `bool` | No | Whether person is a minor |
| `GuardianId` | `int` | Yes | FK to Guardian (if minor) |
| `IsActive` | `bool` | No | Whether entitlement is active |

### 3.4 Document

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `Id` | `int` | No | Primary key |
| `RetirementFileId` | `int` | No | FK to RetirementFile |
| `DocumentTypeId` | `int` | No | FK to DocumentType |
| `OwnerTypeId` | `int` | No | FK to DocumentOwnerType |
| `OwnerId` | `int` | Yes | FK to specific owner (Retiree/EP/Agent) |
| `FilePath` | `string` | No | File storage path |
| `FileName` | `string` | No | Original filename |
| `UploadedById` | `int` | No | FK to User |
| `UploadedAt` | `DateTime` | No | Upload timestamp |
| `IsValidated` | `bool` | No | Whether document is validated |
| `ValidatedById` | `int` | Yes | FK to User (validator) |

### 3.5 MissingDocument

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `Id` | `int` | No | Primary key |
| `RetirementFileId` | `int` | No | FK to RetirementFile |
| `DocumentTypeId` | `int` | No | FK to DocumentType |
| `OwnerTypeId` | `int` | No | FK to DocumentOwnerType |
| `Status` | `enum` | No | Pending / Obtained / Exempted |
| `Deadline` | `DateTime` | Yes | Expected completion date |
| `Notes` | `string` | Yes | Additional notes |

---

## 4. Reference/Configuration Tables

| Table | Purpose | Seed Data |
|-------|---------|-----------|
| `Ranks` | Military/police ranks | ~20 ranks |
| `SocialStatuses` | Marital/social status | Single, Married, Divorced, Widowed |
| `HealthStatuses` | Health condition | Healthy, Disabled, CriticallyIll |
| `RetireeCategories` | Retiree classification | Normal, Special, Martyr |
| `Relationships` | Entitled person relationship | Wife, Son, Daughter, Parent |
| `DocumentTypes` | Types of documents | ~30 document types |
| `DocumentOwnerTypes` | Who the document belongs to | Retiree, EntitledPerson, Agent |
| `PensionFundBranches` | Geographic branches | Regional branches |
| `Roles` | System roles | Admin, Operator, Reviewer, etc. |
| `Permissions` | Granular permissions | 56 permissions |

---

## 5. Payment Tables

### 5.1 PaymentBook

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `RetirementFileId` | `int` | FK to RetirementFile |
| `BookNumber` | `string` | Unique book number |
| `StartDate` | `DateTime` | Payment start date |
| `EndDate` | `DateTime` | Payment end date |
| `Amount` | `decimal` | Monthly payment amount |
| `IsActive` | `bool` | Whether book is active |

### 5.2 SalaryPaymentBatch

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `BatchNumber` | `string` | Unique batch reference |
| `Month` | `int` | Payment month |
| `Year` | `int` | Payment year |
| `Status` | `enum` | Draft / PendingApproval / Approved / Disbursed |
| `CreatedById` | `int` | FK to User |
| `ApprovedById` | `int` | FK to User (SoD: different from creator) |
| `TotalAmount` | `decimal` | Sum of all items |

### 5.3 SalaryPaymentItem

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `BatchId` | `int` | FK to SalaryPaymentBatch |
| `SalaryRecipientId` | `int` | FK to SalaryRecipient |
| `Amount` | `decimal` | Payment amount |
| `Status` | `enum` | Pending / Paid / Reversed |

---

## 6. Settlement Tables

### 6.1 SettlementDecision

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `RetirementFileId` | `int` | FK to RetirementFile |
| `DecisionNumber` | `string` | Official decision number |
| `DecisionDate` | `DateTime` | Date of decision |
| `Amount` | `decimal` | Settlement amount |
| `Status` | `enum` | Draft / PendingApproval / Approved / Rejected |
| `CreatedById` | `int` | FK to User |
| `ApprovedById` | `int` | FK to User (SoD enforced) |

### 6.2 SettlementsBatch

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `BatchNumber` | `string` | Batch reference |
| `Status` | `enum` | Draft / Submitted / Approved / Disbursed |
| `TotalAmount` | `decimal` | Sum of decisions |

---

## 7. Audit Tables

### 7.1 FileStatus

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `RetirementFileId` | `int` | FK to RetirementFile |
| `FromStatus` | `enum` | Previous status |
| `ToStatus` | `enum` | New status |
| `ChangedById` | `int` | FK to User |
| `ChangedAt` | `DateTime` | Timestamp |
| `Reason` | `string` | Reason for change |

### 7.2 FileSnapshot

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `RetirementFileId` | `int` | FK to RetirementFile |
| `SnapshotData` | `string (JSON)` | Full file state as JSON |
| `Hash` | `string` | SHA-256 hash for integrity |
| `CreatedAt` | `DateTime` | Timestamp |
| `CreatedById` | `int` | FK to User |

### 7.3 FileReviewItem

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary key |
| `RetirementFileId` | `int` | FK to RetirementFile |
| `ChecklistItem` | `string` | Review item description |
| `IsCompleted` | `bool` | Whether item is checked |
| `CompletedById` | `int` | FK to User |
| `CompletedAt` | `DateTime` | Completion timestamp |
| `Notes` | `string` | Reviewer notes |

---

## 8. Other Tables

| Table | Purpose |
|-------|---------|
| `Agents` | Legal representatives |
| `Guardians` | Guardians of minor entitled persons |
| `PensionSuspensions` | Active pension suspensions |
| `FinancialEntitlements` | Calculated entitlements |
| `MinistryApprovalItems` | Ministry approval records |
| `DocumentPledges` | Document pledge with expiry |
| `DocumentExemptions` | Document exemption records |
| `StatusTransitionLogs` | Status transition audit |
| `GuardianEntitledPersons` | Guardian-EP relationships |
| `AchievementObstacles` | Achievement tracking obstacles |
| `Users` | System users |
| `Branches` | Organizational branches |
| `AuditLogs` | General audit log |

---

## 9. Key Relationships

| Relationship | Type | Description |
|-------------|------|-------------|
| RetirementFile → Retiree | 1:1 | Each file has one retiree |
| RetirementFile → EntitledPerson | 1:N | File can have multiple entitled persons |
| RetirementFile → Document | 1:N | File can have multiple documents |
| RetirementFile → PaymentBook | 1:N | File can have multiple payment books |
| RetirementFile → SettlementDecision | 1:N | File can have multiple settlements |
| RetirementFile → FileStatus | 1:N | File has status change history |
| RetirementFile → FileSnapshot | 1:N | File has immutable snapshots |
| EntitledPerson → Guardian | N:1 | Minor EP has one guardian |
| SalaryPaymentBatch → SalaryPaymentItem | 1:N | Batch has multiple line items |
| SettlementsBatch → SettlementDecision | 1:N | Batch groups multiple decisions |
| User → RetirementFile (Created) | 1:N | User creates many files |
| User → RetirementFile (Approved) | 1:N | User approves many files |
| Branch → RetirementFile | 1:N | Branch owns many files |

---

## 10. Indexes

### Primary Keys
All tables use auto-incrementing `int` primary keys (`Id`).

### Recommended Indexes
| Table | Column(s) | Type | Reason |
|-------|-----------|------|--------|
| `RetirementFiles` | `FileNumber` | Unique | Fast lookup by file number |
| `RetirementFiles` | `Status` | Non-unique | Filter by status |
| `RetirementFiles` | `BranchId` | Non-unique | Branch-scoped queries |
| `Retirees` | `NationalId` | Unique | Fast lookup by national ID |
| `EntitledPersons` | `RetirementFileId` | Non-unique | FK join performance |
| `Documents` | `RetirementFileId` | Non-unique | FK join performance |
| `FileStatuses` | `RetirementFileId` | Non-unique | Audit trail queries |
| `SalaryPaymentItems` | `BatchId` | Non-unique | FK join performance |
| `Users` | `Username` | Unique | Auth lookup |

---

## 11. Migrations

```bash
# Generate migration
dotnet ef migrations add <MigrationName> --project PensionMS.Infrastructure

# Apply migrations
dotnet ef database update --project PensionMS.API

# Generate SQL script (for production)
dotnet ef migrations script --idempotent -o deploy_script.sql --project PensionMS.Infrastructure

# Apply via sqlcmd
sqlcmd -S .\SQLEXPRESS -i deploy_script.sql
```

**Migration Naming Convention:**
```
<Description>_<Timestamp>
Example: AddDocumentPledgeTable_20240115
```
