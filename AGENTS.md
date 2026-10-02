# AI Agent Guidelines — Pension Management System

**Architecture**: ASP.NET Core 8 Web API + React 19 SPA (Vite)
**Domain**: Yemeni National Pension Management System (نظام إدارة المعاشات التقاعدية)

---

## Project Structure

```
PensionMS.API/          → ASP.NET Core 8 Web API (controllers, JWT auth, SignalR hub)
PensionMS.Core/         → Domain layer (entities, DTOs, interfaces, enums, constants)
PensionMS.Infrastructure/ → EF Core 8 services, repositories, SQL Server DbContext, jobs
PensionMS.Tests/        → xUnit tests (architecture boundary + service tests)
PensionMS.ScannerAgent/ → Windows background service for document scanner integration
ui/                     → React 19 SPA (Vite 7, TypeScript 5, Tailwind CSS 3.4, Zustand)
```

## Essential Commands

### Frontend (`ui/`)
```bash
cd ui
npm ci              # install deps (clean install)
npm run dev         # dev server on port 5173, proxies /api → localhost:5001
npm run build       # tsc -b && vite build → dist/
npm run lint        # eslint .
```

### Backend (`PensionMS.API/`)
```bash
dotnet run          # starts Kestrel dev server (Swagger at /swagger)
dotnet test         # runs PensionMS.Tests (xUnit)
dotnet build        # build entire solution
```

### Database Migrations (EF Core)
```powershell
dotnet ef migrations add <Name> --project PensionMS.Infrastructure --startup-project PensionMS.API
dotnet ef database update --project PensionMS.Infrastructure --startup-project PensionMS.API
```

### Deployment
```powershell
.\deploy.ps1       # full deployment: build API → build UI → deploy to IIS
.\deploy.ps1 -SkipDbReset -SkipIISConfig   # deploy without DB migration or IIS config
```

## Port Map

| Context | API Port | UI Port | Notes |
|---------|----------|---------|-------|
| **Dev (Vite)** | 5001 | 5173 | Vite proxies `/api` → `localhost:5001` |
| **Deploy (IIS)** | 5141 | 80 | `deploy.ps1` targets `E:\inetpub\` |

`.env` has `VITE_API_URL=http://localhost:5001/api` — this is the dev URL only.

## Backend Conventions

### Controllers
- Inherit from `ControllerBase`, decorated with `[ApiController]`, `[Route("api/...")]`, `[Authorize]`.
- Use `ControllerExtensions` for JWT context: `this.GetUserId()`, `this.GetBranchId()`, `this.IsHQ()`.
- Return `ApiResponse<T>` via `.ToActionResult()`. Never return raw `DbContext` results.
- Delegate all DB work to services in `PensionMS.Infrastructure/Services/` using `IUnitOfWork`.

### Branch Scoping
- Queries must be scoped by branch ID unless `IsHQ()` returns true.
- Validate SoD via `ISoDGuard` before state changes (status transitions, payment approvals).

### Authorization
- Policy-based: `[Authorize(Policy = "File.View.Sensitive")]`.
- Permission names are hierarchical: `File.View`, `Accounting.Manage`, `HR.Employee.Edit`, etc.

### Audit Trail
- All status changes, financial calculations, and admin actions must emit audit logs via `IImmutableAuditService`.

## Frontend Conventions

### Data Fetching
- Server state: TanStack Query v5 (`useQuery`, `useMutation`).
- Mutations: use `useApiMutation` from `@/hooks/useApiMutation` — handles cache invalidation, toast alerts, Arabic error extraction.

### Forms
- `react-hook-form` + `zod` schemas with Arabic validation messages.
- Use `zodResolver` from `@hookform/resolvers/zod`.

### State
- Server state → TanStack Query.
- Global client state → Zustand (`authStore`, `useNotificationStore`, `themeStore`, `sidebarStore`).

### ESLint Rules
- `simple-import-sort/imports`: warn (auto-sort imports).
- `unused-imports/no-unused-imports`: warn (auto-remove unused).
- React Hooks purity/set-state-in-effect rules are **off**.

### Styling
- RTL-first layout. Tailwind directional classes must consider RTL (`mr-` vs `ml-`, `space-x-reverse`).
- Radix UI primitives with `class-variance-authority` + `tailwind-merge` via `@/components/ui/`.

## Business Rules

### Retirement File Lifecycle (FileStatus)
`New (1)` → `PendingReview (2)` → `UnderReview (3)` → `Approved (4)` → `Completed (8)` → `Archived (9)`
Branch: `Rejected (5)` from `UnderReview`, can be `Resubmitted (6)` → back to `PendingReview`.
`Suspended (7)` can branch from any active state.

### Soft Deletion
Entities implementing `ISoftDeletable` must NEVER be hard-deleted. Set `IsDeleted = true` at repository/interceptor level.

### All User-Facing Messages in Arabic
Error messages, validation messages, and UI text must be in Arabic. Do not hardcode English strings for end-user APIs.

## Anti-Patterns

- Do NOT use Next.js, SWR, React Server Components, or Server Actions (this is Vite + ASP.NET Core).
- Do NOT make raw EF queries inside controllers.
- Do NOT bypass SoD checks for financial/approval code.
- Do NOT hard-delete soft-deletable entities.
- Do NOT assume a `CitizenAPI` project exists — it is not present in the current codebase.

## Test Framework

- **xUnit** + **Moq** in `PensionMS.Tests/`.
- Run with `dotnet test` from the solution root.
- Architecture boundary tests verify Clean Architecture constraints.
