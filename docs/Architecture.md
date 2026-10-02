# Architecture

## System Architecture — Pension Management System

---

## 1. High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      CLIENT LAYER                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │  React SPA   │  │ Citizen API  │  │  Scanner Agent   │  │
│  │  (Port 80)   │  │  (Separate)  │  │  (Background)    │  │
│  └──────┬───────┘  └──────┬───────┘  └────────┬─────────┘  │
└─────────┼─────────────────┼───────────────────┼─────────────┘
          │                 │                   │
          ▼                 ▼                   ▼
┌─────────────────────────────────────────────────────────────┐
│                      API LAYER                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │            ASP.NET Core Web API (Port 5141)          │   │
│  │  ┌──────────┐ ┌──────────┐ ┌───────────────────┐    │   │
│  │  │Controllers│ │Middleware│ │ SignalR Hub        │    │   │
│  │  │ (80+)    │ │  (JWT)   │ │ (Notifications)   │    │   │
│  │  └──────────┘ └──────────┘ └───────────────────┘    │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                   SERVICE LAYER                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Business Logic Services (~50 implementations)       │   │
│  │  ┌────────────────┐ ┌────────────────┐ ┌──────────┐  │   │
│  │  │RetirementFile  │ │EntitledPerson  │ │Settlement│  │   │
│  │  │Service (2609L) │ │Service (760L)  │ │Service   │  │   │
│  │  └────────────────┘ └────────────────┘ └──────────┘  │   │
│  │  ┌────────────────┐ ┌────────────────┐ ┌──────────┐  │   │
│  │  │FileReview      │ │SalaryPayment   │ │Financial │  │   │
│  │  │Service (1065L) │ │BatchService    │ │Entitlement│  │   │
│  │  └────────────────┘ └────────────────┘ └──────────┘  │   │
│  │  ┌────────────────────────────────────────────────┐   │   │
│  │  │  Engines: Compliance | Entitlement | Workflow  │   │   │
│  │  │  Guards: SoD | Branch | Permission            │   │   │
│  │  └────────────────────────────────────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  DATA ACCESS LAYER                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Entity Framework Core 8 (DbContext)                 │   │
│  │  ┌──────────┐ ┌──────────┐ ┌───────────────────┐    │   │
│  │  │ DbSets   │ │ Migrations│ │ Configurations    │    │   │
│  │  │ (50+)    │ │ (EF Core)│ │ (Fluent API)      │    │   │
│  │  └──────────┘ └──────────┘ └───────────────────┘    │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    DATABASE                                 │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  SQL Server Express (Dev) / SQL Server (Prod)        │   │
│  │  Database: PensionManagementDB                       │   │
│  │  Tables: 50+ | Stored Procedures: 0 | Views: 0      │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Layer Responsibilities

### 2.1 Presentation Layer (PensionMS.API)

**Project:** `PensionMS.API`

| Component | Count | Purpose |
|-----------|-------|---------|
| Controllers | ~80 | REST API endpoints, HTTP method routing |
| Middleware | 4 | JWT auth, request logging, error handling, CORS |
| DI Configuration | 1 | Service registration in `Program.cs` |
| SignalR Hub | 1 | Real-time notifications to connected clients |

**Key Responsibilities:**
- HTTP request/response handling
- Input validation (FluentValidation)
- JWT token validation and user context extraction
- Response formatting (standardized API responses)
- Swagger/OpenAPI documentation

**Middleware Pipeline (order matters):**
1. CORS Policy
2. Request Logging
3. JWT Authentication
4. Authorization
5. Custom Error Handling
6. Rate Limiting
7. Response Caching

### 2.2 Service Layer (PensionMS.Infrastructure)

**Project:** `PensionMS.Infrastructure`

| Component | Count | Purpose |
|-----------|-------|---------|
| Service Implementations | ~50 | Business logic execution |
| Interface Definitions | ~90 | Contracts (in Core) |
| Background Jobs | 2 | Expired pledges, outbox processor |
| SoD Guards | 3 | Separation of duties enforcement |
| Engines | 3 | Compliance, entitlement calculation, workflow |
| ScopedPermissionResolver | 1 | Runtime permission resolution |

**Key Responsibilities:**
- Business rule enforcement
- Transaction management
- Audit trail creation
- File review workflow orchestration
- Financial calculation engines
- Background task processing
- Separation of duties validation

### 2.3 Domain Layer (PensionMS.Core)

**Project:** `PensionMS.Core`

| Component | Count | Purpose |
|-----------|-------|---------|
| Entities | ~150 | Domain models and value objects |
| Enums | ~25 | Type-safe constants |
| Interfaces | ~90 | Service contracts |
| Constants | 3 | Permission, role, status constants |

**Key Responsibilities:**
- Domain model definitions
- Business invariant specifications
- Service contract definitions
- Shared enums and constants

### 2.4 Data Access Layer (PensionMS.Infrastructure)

| Component | Count | Purpose |
|-----------|-------|---------|
| DbContext | 1 | `PensionDbContext` — 50+ DbSets |
| Entity Configurations | ~50 | Fluent API mappings |
| Migrations | ~20 | Schema evolution |
| Seed Data | 1 | Default roles, permissions, admin user |

---

## 3. Design Patterns

### 3.1 Clean Architecture (Onion)

```
Core (Domain) ← Infrastructure ← API (Presentation)
     ↑               ↑
     └─── Interfaces ─┘
```

- **Dependency Rule:** Dependencies point inward. Core has zero external dependencies.
- **Interface Segregation:** Core defines interfaces; Infrastructure implements them.
- **Testability:** Core and Infrastructure can be tested without API layer.

### 3.2 Repository Pattern (via EF Core)

EF Core DbContext acts as the repository. No explicit repository interfaces — services interact with DbContext directly for query composition, while business operations go through service interfaces.

### 3.3 State Machine Pattern

RetirementFile follows explicit state transitions:

```
New → PendingReview → UnderReview → Approved
                         ↓              ↓
                    Rejected        Completed
                         ↓
                    Resubmitted → UnderReview
```

Implemented via `FileStatusService` with guard conditions.

### 3.4 Separation of Duties (SoD) Guards

Three guards enforce dual-control on sensitive operations:

| Guard | Scope |
|-------|-------|
| `SettlementSoDGuard` | Settlement creation vs approval |
| `SalaryBatchSoDGuard` | Salary batch creation vs approval |
| `EntitlementSoDGuard` | Entitlement calculation vs disbursement |

**Rule:** The person who creates a record cannot approve it.

### 3.5 Outbox Pattern

For reliable event publishing:

```
1. Business transaction writes to outbox table
2. Background job (NotificationOutboxProcessor) polls outbox
3. Job publishes events (SignalR, email, etc.)
4. Job marks outbox entry as processed
```

### 3.6 Immutable Audit Trail

Every state-changing operation creates a `FileSnapshot`:
- Point-in-time representation of file state
- Tamper-evident (hash chain)
- Enables historical comparison
- Compliance with government audit requirements

---

## 4. Cross-Cutting Concerns

### 4.1 Authentication & Authorization

```
Request → JWT Validation → Role Extraction → Permission Resolution → Controller
```

- JWT tokens contain user ID, username, role, branch ID
- Permissions resolved via `ScopedPermissionResolver`
- Branch-scoped data access (users only see their branch's data)
- Permission tiers: Branch → Regional → National

### 4.2 Audit Trail

Every mutation creates:
1. `FileStatus` record (status change log)
2. `FileSnapshot` (immutable state snapshot)
3. `AuditLog` entry (who, what, when, where)

### 4.3 Error Handling

Global exception handler catches:
- `BusinessRuleException` → 400 (business validation)
- `UnauthorizedAccessException` → 403 (permission denied)
- `EntityNotFoundException` → 404 (not found)
- `ValidationException` → 422 (input validation)
- Generic exceptions → 500 (logged, sanitized)

### 4.4 Real-Time Notifications

SignalR hub for:
- File status changes
- New document uploads
- Settlement approvals
- Payment batch completions
- System alerts

---

## 5. Data Flow Diagrams

### 5.1 File Creation Flow

```
User → API Controller → RetirementFileService → DbContext → Database
  │                                                │
  │                                                ↓
  │                                          FileSnapshot (audit)
  │                                          FileStatus (log)
  │
  ↓
Response → User
```

### 5.2 Payment Generation Flow

```
Admin → SalaryPaymentBatchService → EntitlementEngine → Calculate amounts
  │                                                         │
  │                                                         ↓
  │                                                   SalaryPaymentItem[]
  │
  ↓
Approval → SoD Guard → Approve → MinistryApprovalItem → Ministry
```

### 5.3 Document Processing Flow

```
User uploads → DocumentService → FileStorage (disk)
  │                                    │
  │                                    ↓
  │                              Document record (DB)
  │
  ↓
Owner classification → MissingDocument check → ComplianceEngine
```

---

## 6. Background Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| `ExpiredPledgesJob` | Daily | Auto-release expired document pledges |
| `NotificationOutboxProcessor` | Every 30s | Process pending notification events |

---

## 7. External Integrations

| System | Protocol | Purpose |
|--------|----------|---------|
| SQL Server | TCP/IP | Database access |
| IIS | HTTP | Web server hosting |
| File System | Disk I/O | Document/photo storage |
| SignalR | WebSocket | Real-time client push |

---

## 8. Scalability Considerations

| Aspect | Current | Notes |
|--------|---------|-------|
| Deployment | Single server | IIS on Windows Server |
| Database | Single instance | SQL Server Express (dev) / Standard (prod) |
| Caching | In-memory | Response caching middleware |
| Background Jobs | In-process | Hosted services (not distributed) |
| Load Balancing | None | Single-server deployment |

**Future Scaling Options:**
- Horizontal: Add more API instances behind load balancer
- Database: Read replicas for reporting
- Caching: Redis for distributed cache
- Jobs: Hangfire for distributed background processing
