<<<<<<< HEAD
# AGENTS.md — Pension Management System (PensionMS)

Yemen government pension/retirement fund system. ASP.NET Core Web API, EF Core/SQL Server,
and React/Vite/TypeScript SPA. All user-facing messages and UI text are **Arabic**.
Framework/package versions come from project files and `ui/package.json`; preserve their targets.

## Working agreement

- Keep changes within the task scope and preserve unrelated tracked/untracked work.
- Before substantial edits, briefly identify the existing owner, intended change, and verification.
  Proceed with authorized work; routine implementation choices need no extra approval.
- Preserve public contracts and business behavior unless the task changes them. Report policy
  conflicts explicitly; do not silently choose new business rules.
- Check relevant callers and tests before changing shared behavior. Existing code can contain
  violations; choose examples that satisfy the rules below.
- Keep credentials and real personal data out of code, logs, fixtures, and reports. Deployment
  and writes to existing development/production databases require explicit authorization.

## Discover and reuse existing implementations

Applies to services, rules, components, hooks, utilities, repositories, DTOs, interfaces,
constants, validators, authorization, auditing, caching, and transaction workflows.

- Before adding/replacing behavior, use `rg`/`rg --files` to search by responsibility and behavior.
  Read candidate implementations, callers, and tests; filename matching alone is insufficient.
- Reuse the existing owner when its contract fits; otherwise extend that owner within scope.
  Check authorization, transaction ownership, side effects, failure handling, and lifecycle first.
- Keep one authoritative implementation per business responsibility. Orchestration delegates
  owned operations: opening a file should reuse the existing agent operation when adding an agent,
  rather than rebuild its validation, persistence, attachments, and audit.
- Sharing a Core predicate alone does not justify duplicating its surrounding workflow.
  Avoid parallel definitions/implementations for an existing concept.
- New implementations need evidence of a missing responsibility or materially different contract.
  Explain why the closest existing candidates are unsuitable. Similar syntax alone does not justify reuse.
- Preserve layer direction and avoid circular dependencies. Report out-of-scope dependencies
  and continue unaffected work; do not silently build a substitute.
- For extraction/replacement, update affected callers and remove the superseded implementation
  within scope. Review inline logic as well as new files for duplication.
- When introducing/replacing production responsibilities, report:
  **Responsibility | Existing path/symbol | Reused/extended/new | Reason**.
  Routine edits introducing no new responsibility do not need a separate inventory.

## Architecture and ownership

Layered monolith; application services currently live in Infrastructure.
The required dependency direction is toward Core. Existing violations are not templates.

| Location | Responsibility |
| --- | --- |
| `PensionMS.Core/` | Entities, DTOs, contracts, constants, independent business decisions |
| `PensionMS.Infrastructure/` | Application orchestration, EF/repositories, transactions, storage, integrations, jobs |
| `PensionMS.API/` | HTTP contracts, input validation, authentication/authorization pipeline, composition root |
| `PensionMS.Tests/` | Core/Infrastructure unit and structural tests; selected API code is link-included |
| `PensionMS.IntegrationTests/` | SQL Server and HTTP/security integration tests; references API |
| `ui/src/` | React presentation, forms, client state, typed API consumers |

- Contracts belong in `PensionMS.Core/Interfaces/`. Core must not reference API, Infrastructure,
  EF Core, or ASP.NET Core.
- Policies in `PensionMS.Core/Rules/` accept explicit facts/evaluation dates and return decisions.
  Keep I/O and application of decisions in Infrastructure.
- Distinguish business policy from transport validation, informational completeness, queries,
  and presentation. A conditional is not automatically a domain rule.
- Reuse shared rules; context-specific decisions stay with their owner. File approval consumes
  individual eligibility results rather than reimplementing person eligibility.
- Preserve existing command/query/report separation. Mappers transform data, not eligibility,
  activation, or payment permission. Add abstractions for concrete needs, not for every predicate.

## Code conventions and composition

- Follow local C# style: PascalCase types/methods, camelCase parameters, `_camelCase` private fields,
  and `Async` suffixes. Preserve nullable contracts and handle absent data explicitly.
- Avoid silent business defaults, swallowed failures, blocking async calls, and fire-and-forget writes.
  Forward cancellation where the contract supports it. Use `decimal` for money and existing rounding rules.
- Controllers delegate to application services and return `ApiResponse<T>` through `.ToActionResult()`.
  Keep database work and tracked entities out of controller responses.
- Use existing repository/UnitOfWork and scoped queries. Project reads, use `AsNoTracking`
  where appropriate, preserve server-side filtering, and avoid N+1 queries.
- Use constructor injection. Explicit registrations live in
  `PensionMS.Infrastructure/DependencyInjection/*ServiceCollectionExtensions.cs`;
  `PensionMS.API/Program.cs` calls `AddPensionInfrastructureServices()`.
  Update the owning registration, lifetimes, and affected constructor mocks.
- `PensionMS.Tests/DependencyInjectionTests.cs` checks DI and selected structural contracts,
  not full Clean Architecture compliance. Keep refactors, package changes, and upgrades within scope.

## Authorization, persistence, and side effects

- Use explicit permission policies. Plain `[Authorize]` authenticates but does not establish
  operation permission or branch scope. See `PensionMS.API/Security/PermissionHandler.cs` and `Program.cs`.
- Use existing identity helpers and `IAccessiblePensionRepository` for operation-specific pension scope.
  HQ status or client-supplied branch/user IDs do not establish unrestricted permission.
- Scope reads, mutations, exports, and owner lookups. Raw UnitOfWork queries require previously
  verified scoped aggregate access and constraints retaining that scope. Trace callers before
  declaring an authorization defect or assuming an internal query is safe.
- Preserve applicable approval/segregation-of-duties policies through current implementations;
  do not restore retired guards from old documentation.
- For `ISoftDeletable`, update deletion fields through its owner. Global filters hide deleted rows;
  `Repository.Delete/DeleteRange` reject these entities. Neither implements branch authorization.
  Inspect the model before `IgnoreQueryFilters`, which disables global filters.
- Composite mutations have one transaction owner. Reuse
  `PensionMS.Infrastructure/Data/UnitOfWorkTransaction.cs` where applicable:
  nested operations join and propagate failures; only the owner commits/rolls back.
- Preserve database locks/concurrency checks and evaluate mutation decisions under the relevant lock.
  In-process locking or check-then-insert alone does not prove cross-instance safety/idempotency.
- Preserve audit/review effects using existing `IImmutableAuditService`,
  `PensionMS.API/Interceptors/AuditSaveChangesInterceptor.cs`, and notification/outbox paths.
- Propagate required upload failures and reuse storage cleanup/UnitOfWork rollback compensation.
  Database rollback does not undo filesystem changes.

## Pension decision sources

Consult these sources for the affected decision; avoid copying their business-policy tables here.

| Behavior | Starting points |
| --- | --- |
| File statuses | `PensionMS.Core/Constants/FileStatusConstants.cs`, `ui/src/constants/retirement.ts` |
| Review, approval, transitions | `PensionMS.Infrastructure/Services/Pension/RetirementFile/` |
| Person eligibility/lifecycle | `PensionMS.Core/Rules/Entitlements/`, `PensionMS.Infrastructure/Services/Pension/Beneficiaries/`, `PensionMS.Infrastructure/Services/Pension/Entitlements/` |
| Document requirements/satisfaction | `PensionMS.Core/Rules/Documents/`, `PensionMS.Infrastructure/Services/Documents/` |
| Financial periods | `PensionMS.Core/Rules/Entitlements/PensionEntitlementRules.cs`, `PensionMS.Infrastructure/Services/Pension/Entitlements/FinancialEntitlementService.cs` |

`FileStatus` is a lookup entity, not an enum; distinguish current constants from
`LegacyFileStatusConstants`. Read `FileStatusService`, `FileReviewService`, and
`RetirementFileActivationValidator` together for approval/transitions. Eligibility, active state,
file review, financial entitlement, and payment permission are separate decisions.

## Dates and time

For date, age, expiry, or serialization changes, read `docs/adr/0001-date-time-contract.md`
alongside `PensionMS.Core/Time/YemenTime.cs`, `PensionMS.API/Converters/YemenDateTimeConverter.cs`,
`PensionMS.Tests/DateTimeContractTests.cs`, and relevant `ui/src/lib/date.ts` helpers.

**Known conflict:** ADR 0001 describes legacy unqualified Yemen wall-time output; the current
converter and contract tests use UTC instants with `Z`. Identify this mismatch before changing
the contract; preserve current behavior unless the task explicitly changes it.

- Store instants in UTC; keep calendar dates as calendar dates. Use `YemenTime` for Asia/Aden
  conversion and explicit evaluation dates inside Core rules.
- Use the shared UI date helpers. Avoid direct `toLocaleDateString("ar-YE")` and ISO slicing
  for calendar dates; see `ui/eslint.restricted-date-patterns.js`.
  Determine whether a value is a date, wall time, or instant before adding/converting an offset.

## Frontend conventions

- Keep React/Vite patterns: TanStack Query for server state, Zustand for client state,
  react-hook-form + Zod for forms. Reuse `ui/src/hooks/useApiMutation.ts`, shared domain components,
  hooks, schemas, and typed API functions.
- Use `ui/src/api/axios.ts`. It defaults to JSON; reuse the multipart convention for files
  and verify file binding. Preserve types instead of masking missing contracts with `any` or double casts.
- Reuse `@/` imports and Radix/CVA/tailwind-merge primitives in `ui/src/components/ui/`.
  Keep Arabic content, RTL layout, and directional spacing.
- Use `ui/src/index.css` color tokens as `hsl(var(--...))` and `ui/src/lib/chartColors.ts`.
  Avoid hardcoded component colors. Verify light/dark modes, reduced motion, and relevant printing
  in `ui/src/styles/print.css`.
- For token remediation, read `docs/design-token-audit.md` and
  `plan/design-token-remediation-plan.md`; update affected audit entries.
  Read `ui/eslint.config.js` for current lint rules.

## Verification and commands

Use checks appropriate to the change; commands do not authorize deployment or existing database writes.
Backend commands below use explicit project paths from the repository root.

| Check | Command / location |
| --- | --- |
| API build | `dotnet build .\PensionMS.API\PensionMS.API.csproj --nologo` |
| Unit/structural tests | `dotnet test .\PensionMS.Tests\PensionMS.Tests.csproj --nologo` |
| SQL/HTTP integration | Read `PensionMS.IntegrationTests/README.md`, set `PENSIONMS_TEST_SQLSERVER` for an isolated test server, then `dotnet test .\PensionMS.IntegrationTests\PensionMS.IntegrationTests.csproj --nologo`; filter to affected tests when appropriate |
| Frontend, from `ui/` | `npm run lint` → `npm run build` → `npm test` → `npm run preview` |
| Local API host | `dotnet run --project .\PensionMS.API\PensionMS.API.csproj`; inspect configuration first |
| Whitespace | `git diff --check` |

- In PowerShell use `npm.cmd` if `npm.ps1` is blocked. Install from the lockfile with `npm ci`
  when needed, not on every task.
- Unit tests need no live database; some inspect EF metadata/query translation.
  Integration tests use disposable `PensionMS_Test_<GUID>` databases with synthetic data;
  follow their README, not application database credentials/configuration.
  Their host does not prove production `Program.cs`, jobs, or seed correctness.
- UI lint acceptance is no new problems: capture before/after output rather than demand zero
  existing errors. Build/tests must pass; preview affected screens when possible.
- Test changed policies directly and affected callers through observable behavior.
  Use SQL tests to substantiate transaction/concurrency/constraint claims; mocks alone cannot.
  Update tests with shared contract/constructor changes.
- Report actual commands/results, baseline failures, and gaps. Do not weaken assertions,
  hide relevant failures, or fix unrelated code to report success.
- Documentation-only changes need reference/consistency checks, not application builds/tests.

## Database, operations, and guidance maintenance

- Schema changes currently use SQL under `scripts/` and `plan/`, plus model/seed configuration
  in `PensionMS.Infrastructure/Data/Configurations/`. Do not assume EF migrations supply a baseline
  or that startup migration updates the schema.
- Deployment entry points are `deploy-api.ps1` and `deploy-ui.ps1`. Inspect their current parameters
  and targets when deployment is requested; building/testing is not deployment authorization.
- Keep this file for durable agreements and non-obvious constraints. Refer to sources for
  changing facts; avoid counts, line-number inventories, task plans, and repeated policy tables.
- Architecture/business documents can lag code. Identify discrepancies and distinguish required
  standards from observed gaps. Update affected guidance within scope; add local instructions
  only for distinct directory needs, without copying root rules.
=======
# AI Agent Guidelines — Pension Management System

**Version 2.0.0**  
**Architecture**: ASP.NET Core 8 Web API + React 19 SPA (Vite)  
**Domain**: Iraqi National Pension Management System (نظام إدارة المعاشات التقاعدية)

---

## 1. System Architecture Overview

The system follows **Clean Architecture** principles divided into backend layers and a modern frontend SPA:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      CLIENT LAYER (React 19 SPA)                        │
│   Vite 7 | TypeScript 5 | Tailwind CSS 3.4 | TanStack Query v5 | Zustand  │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │ HTTP / REST / SignalR
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                 PRESENTATION LAYER (PensionMS.API)                      │
│   ASP.NET Core 8 | Controllers | JWT Auth Middleware | SignalR Hubs     │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│               INFRASTRUCTURE LAYER (PensionMS.Infrastructure)           │
│   EF Core 8 DbContext | Services Implementation | Background Jobs       │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    CORE DOMAIN LAYER (PensionMS.Core)                   │
│   Entities | DTOs | Domain Interfaces | Enums | Business Exceptions     │
└─────────────────────────────────────────────────────────────────────────┘
```

### Core Projects & Subsystems
- `PensionMS.Core`: Domain models (`RetirementFile`, `Retiree`, `EntitledPerson`), DTOs, Interfaces (`IUnitOfWork`, `IRepository<T>`).
- `PensionMS.Infrastructure`: EF Core 8 SQL Server context, domain services (`RetirementFileService`, `SettlementService`), PDF/Excel report generators.
- `PensionMS.API`: REST Web API controllers, JWT authentication, authorization policies, SignalR hub (`NotificationHub`).
- `PensionMS.CitizenAPI`: Public portal backend for citizen service requests & tracking.
- `PensionMS.ScannerAgent`: Windows background service for physical document scanner integration.
- `ui`: Main admin & operational web application built with Vite + React 19.

---

## 2. Backend Coding Standards (C# / .NET 8)

### 2.1 Controller Conventions
- Controllers must reside in `PensionMS.API/Controllers/` and inherit from `ControllerBase`.
- Always decorate controllers with `[ApiController]`, `[Route("api/[controller]")]` (or kebab-case explicit route), and `[Authorize]`.
- Return standardized `ApiResponse<T>` or `PagedResult<T>` via the `ControllerExtensions` helper `result.ToActionResult()`.

```csharp
[ApiController]
[Route("api/retirement-files")]
[Authorize]
public class RetirementFilesController : ControllerBase
{
    private readonly IFileQueryService _queryService;

    public RetirementFilesController(IFileQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    [Authorize(Policy = "File.View.Sensitive")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        int userId = this.GetUserId();
        var result = await _queryService.GetAllAsync(page, pageSize, search, userId);
        return result.ToActionResult(); // Maps ApiResponse<T> to HTTP 200, 400, or 404
    }
}
```

### 2.2 User & Branch Context Extraction
Use `ControllerExtensions` methods inside controllers:
- `this.GetUserId()`: Extracts user ID from JWT `NameIdentifier` claim. Throws `UnauthorizedAccessException` if missing.
- `this.GetBranchId()`: Extracts user's branch identifier. Returns `0` if national HQ.
- `this.IsHQ()`: Returns `true` if the user has national HQ privileges.

### 2.3 Business Logic & UnitOfWork
- Controller actions MUST NOT execute raw database queries or direct `DbContext` calls. Delegate work to domain services in `PensionMS.Infrastructure/Services/`.
- Services use `IUnitOfWork` to orchestrate repositories and persist atomic transactions.

```csharp
// Standard response format from Services
public async Task<ApiResponse<RetirementFileDto>> GetByIdAsync(int id)
{
    var file = await _unitOfWork.RetirementFiles.GetByIdAsync(id);
    if (file == null)
        return ApiResponse<RetirementFileDto>.Fail("ملف التقاعد غير موجود");

    var dto = _mapper.Map<RetirementFileDto>(file);
    return ApiResponse<RetirementFileDto>.Success(dto);
}
```

### 2.4 Separation of Duties (SoD) & Branch Scoping
- Before executing state changes (e.g. status transition, payment approval), validate rules using `ISoDGuard`.
- Scope database queries by branch ID unless `IsHQ()` is true. Use `IScopedPermissionResolver` or `BranchScopeProvider`.

---

## 3. Frontend Coding Standards (React 19 / TypeScript)

### 3.1 Data Fetching & Mutations (TanStack Query v5)
- Use custom hook `useApiMutation` from `@/hooks/useApiMutation` for POST/PUT/DELETE operations.
- `useApiMutation` automatically handles:
  1. Cache invalidation via `invalidateKeys`.
  2. Automatic toast alerts (`sonner`) with Arabic error message extraction via `normalizeApiError`.
  3. Optional success callbacks (`onSuccessCallback`).

```tsx
import { useApiMutation } from "@/hooks/useApiMutation";
import { updateFileStatus } from "@/api/retirement";

export function useUpdateFileStatus() {
  return useApiMutation(
    (data: { id: number; statusId: number; reason?: string }) => updateFileStatus(data.id, data),
    {
      invalidateKeys: [["retirement-files"], ["file-details"]],
      successMessage: "تم تحديث حالة الملفات بنجاح",
      errorTitle: "خطأ في تحديث الحالة",
    }
  );
}
```

### 3.2 State Management
- **Server State**: Managed exclusively with TanStack Query v5 (`useQuery`, `useMutation`).
- **Global Client State**: Managed with Zustand stores:
  - `authStore.ts`: Authentication token, decoded permissions, user identity, cross-tab synchronization via `BroadcastChannel`.
  - `useNotificationStore.ts`: SignalR realtime notifications.
  - `themeStore.ts` & `sidebarStore.ts`: Layout and theme state.

### 3.3 Form Handling & Schema Validation
- All forms must use `react-hook-form` paired with `zod` schema resolvers (`@hookform/resolvers/zod`).
- Zod schemas must include user-friendly Arabic validation messages.

```tsx
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

const retireeSchema = z.object({
  fullName: z.string().min(3, "الاسم الكامل مطلوب (3 حروف على الأقل)"),
  nationalId: z.string().length(12, "رقم البطاقة الوطنية يجب أن يكون 12 رقم"),
  birthDate: z.string().refine((val) => !isNaN(Date.parse(val)), "تاريخ الميلاد غير صحيح"),
});

type RetireeFormValues = z.infer<typeof retireeSchema>;

export function RetireeForm() {
  const { register, handleSubmit, formState: { errors } } = useForm<RetireeFormValues>({
    resolver: zodResolver(retireeSchema),
  });
  // ...
}
```

### 3.4 Role-Based UI Rendering
Use `useRoleBasedUI()` hook to conditionally render UI components based on permissions:

```tsx
import { useRoleBasedUI } from "@/hooks/useRoleBasedUI";

export function ActionPanel() {
  const { isReviewer, isFinancialOfficer, hasPermission } = useRoleBasedUI();

  return (
    <div className="flex gap-2">
      {isReviewer && <Button onClick={handleApprove}>اعتماد الملف</Button>}
      {isFinancialOfficer && <Button onClick={handleCalculateSettlement}>احتساب المستحقات</Button>}
      {hasPermission("File.Delete") && <Button variant="destructive">حذف</Button>}
    </div>
  );
}
```

### 3.5 Styling & Localization (RTL First)
- Layout is **RTL (Right-to-Left)** by default. Ensure Tailwind CSS directional classes consider RTL (`mr-` vs `ml-`, `space-x-reverse`, `text-right`).
- Use Radix UI primitives with `@/components/ui/` design tokens (built with `class-variance-authority` & `tailwind-merge`).

---

## 4. Database & Entity Framework Core Standards

### 4.1 Central Entity: `RetirementFile`
The central domain entity is `RetirementFile`. Key relationships:
- **1:1** with `Retiree`
- **1:N** with `EntitledPerson` (المستحقين)
- **1:N** with `Document` (الوثائق المرفقة)
- **1:N** with `PaymentBook` (دفتر الصرف)
- **1:N** with `SalaryRecipient` (مستلمي الراتب)
- **1:N** with `SettlementItem` (بنود التسوية)

### 4.2 Soft Deletion & Audit Fields
- Entities implementing `ISoftDeletable` (`IsDeleted`, `DeletedAt`, `DeletedBy`) must NEVER be hard deleted from the database.
- Intercept deletes in `PensionDbContext` or repository level to set `IsDeleted = true`.

### 4.3 Database Migrations
- Migration commands must be run targeting `PensionMS.Infrastructure`:
```powershell
dotnet ef migrations add <MigrationName> --project PensionMS.Infrastructure --startup-project PensionMS.API
dotnet ef database update --project PensionMS.Infrastructure --startup-project PensionMS.API
```

---

## 5. Domain Business Rules & State Machines

### 5.1 Retirement File Lifecycle (حالة الملفات التقاعدي)
A file transitions through strict states represented by `FileStatus`:
1. `New (1)`: Created, awaiting document attachment.
2. `PendingReview (2)`: All mandatory documents uploaded, queued for review.
3. `UnderReview (3)`: Locked by assigned reviewer.
4. `Approved (4)`: Review passed, ready for financial processing.
5. `Rejected (5)`: Returned with rejection reasons.
6. `Resubmitted (6)`: Corrections attached by operator, ready for re-review.
7. `Suspended (7)`: Pension payment suspended.
8. `Completed (8)`: Settlement completed & payment active.
9. `Archived (9)`: Closed & archived.

### 5.2 Immutable Audit Trail
- All file status changes, financial calculations, and administrative actions must emit audit log records via `IAuditLogService` or `IImmutableAuditService`.
- Audit logs contain `UserId`, `Action`, `EntityName`, `EntityId`, `OldValues`, `NewValues`, `IpAddress`, `Timestamp`.

---

## 6. Deployment & Environment Rules

- **API Port**: `5141` (Local IIS / Kestrel)
- **UI Port**: `80` (Local IIS)
- Automated deployment is executed via `deploy.ps1` in the project root.
- Environment variables:
  - `VITE_API_URL`: Points to API endpoint (e.g. `http://localhost:5141/api`).
  - `ConnectionStrings:DefaultConnection`: SQL Server connection string in `appsettings.json`.

---

## 7. Anti-Patterns & Prohibited Code Choices

❌ **DO NOT** use Next.js, SWR, React Server Components (RSC), or Server Actions (This is a Vite + ASP.NET Core project).  
❌ **DO NOT** make raw Entity Framework queries inside Web API Controllers.  
❌ **DO NOT** hardcode English error messages for end-user APIs (All user-facing messages must be in Arabic).  
❌ **DO NOT** bypass Separation of Duties (SoD) checks when writing financial or file approval code.  
❌ **DO NOT** perform direct hard deletes (`DELETE FROM ...`) on entities implementing `ISoftDeletable`.
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
