# AGENTS.md — Pension Management System (PensionMS)

Government pension/retirement fund system for Yemen's General Directorate of Security Retirement Fund.
**ASP.NET Core 8 Web API (SQL Server, EF Core 8) + React 19 SPA (Vite 7, TS 5.9, Tailwind 3.4).**

All user-facing text (errors, validation, UI labels) is **Arabic**.

---

## Commands

### Frontend — run from `ui/`
```bash
npm ci            # clean install
npm run dev       # Vite dev server; proxies /api /hubs /uploads -> http://localhost:5001
npm run lint      # ESLint (flat config)
npm run build     # tsc -b && vite build
npm test          # tsc --noEmit -p tsconfig.test.json && node --test (81 tests)
npm run preview   # serve dist/ for visual verification
```

**Verification order for UI work: `npm run lint` → `npm run build` → `npm test` → `npm run preview`.**

> **`npm run lint` does not pass and is not expected to.** Baseline is ~308 errors / 79 warnings
> (`plan/m0-baseline-lint.txt`). The gate is **no *new* problems vs. baseline**, not zero. Always
> capture lint output before and after your change rather than assuming you broke it.

`npm run build` and `npm test` must pass cleanly.

### Backend — run from the specific project dir
There is **no `.sln` and no project file at the repo root**. `dotnet build` / `dotnet test` from root
fail with MSB1003.

```bash
cd PensionMS.Tests  && dotnet test --nologo   # ~86 xUnit tests
cd PensionMS.API    && dotnet run             # Kestrel on http://0.0.0.0:5001, Swagger at /swagger
```

Tests need **no live database** — xUnit + Moq, services hand-mocked via `Mock<IUnitOfWork>` +
`Mock<IRepository<T>>`. Two tests build a `PensionDbContext` against
`Server=(localdb)\MSSQLLocalDB` but only inspect `db.Model` / `ToQueryString()`, so the SQL Server
*provider* is required, not a reachable server.

> `PensionMS.Tests` references `PensionMS.Core` + `PensionMS.Infrastructure` only — **not**
> `PensionMS.API` (it link-includes one file, `Converters/YemenDateTimeConverter.cs`). Controllers,
> `Program.cs`, `ControllerExtensions` and the middleware/JWT pipeline are therefore **not testable**.

### Database
`PensionMS.Infrastructure/Migrations/` is **empty** and there is **no `dotnet-ef` tooling**. The
documented `dotnet ef migrations add` flow will not produce a working baseline. Schema changes are
applied via **hand-written SQL** in `scripts/*.sql` and `plan/*.sql`, plus EF `HasData` seed data
baked into `PensionMS.Infrastructure/Data/Configurations/**`. `Program.cs` calls
`db.Database.MigrateAsync()` on non-Development startup, but the migration assembly is empty — it is
effectively a no-op.

### Deploy (IIS / Windows Server)
```powershell
.\deploy-api.ps1    # -> E:\inetpub\PensionAPI, site+apppool "PensionAPI"; -SkipBuild, -RestartOnly
.\deploy-ui.ps1     # -> E:\inetpub\PensionUI, site "PensionUI";        -SkipBuild, -RestartOnly
```
There is **no combined `deploy.ps1`** and no `-SkipDbReset` / `-SkipIISConfig` flags.

### Toolchain
All projects target `net8.0` (ScannerAgent is `net8.0-windows`), but **no `global.json` exists**, so
the default CLI SDK is 10.0.400 building net8.0.

---

## Layout & boundaries

```
PensionMS.Core/          → entities, DTOs, interfaces, constants   (no dependencies)
PensionMS.Infrastructure/ → EF DbContext, repositories, services, jobs
PensionMS.API/           → controllers, Program.cs (DI wiring), FluentValidation
PensionMS.Tests/         → xUnit + Moq
PensionMS.ScannerAgent/  → WinForms console exe. NOT a Windows Service/BackgroundService,
                            has ZERO project references, hardcodes http://localhost:5000
ui/src/                  → components/ pages/ hooks/ queries/ stores/ lib/ types/ routes/
docs/                    → Architecture.md, API.md, BusinessRules.md, DESIGN_SYSTEM.md, adr/, audits/
```

Interfaces belong in `PensionMS.Core/Interfaces/` (93 files). A handful leak into Infrastructure —
follow the majority, don't copy the leak.

**There is no `PensionMS.CitizenAPI`** project, despite what `docs/README.md` claims. Some
`ICitizen*Service` implementations in Infrastructure are dead code (unregistered, no controller).

### DI is manual — no assembly scanning
`AddControllers()` finds controllers by default, but **every service is registered by hand**
(~120 explicit `AddScoped` lines in `Program.cs:71-186`). Adding a service means editing
`Program.cs` or nothing resolves. `AddValidatorsFromAssemblyContaining<Program>()` is the only
assembly-scanning registration; all validators live in one file,
`PensionMS.API/Validators/Validators.cs`. Two services are registered by **fully-qualified name**
(`SmsService`, `OtpService`).

Registered hosted services: `ExpiredPledgesJob` (24h), `NotificationOutboxProcessor` (30s poll).
`PledgeExpiryCheckerService` is a `BackgroundService` that is **never added to DI**.

---

## Backend conventions

- Controllers: `[ApiController]`, `[Route("api/...")]`, `[Authorize]`. Get identity from
  `ControllerExtensions` — `this.GetUserId()`, `this.GetBranchId()`, `this.IsHQ()`
  (`PensionMS.API/Extensions/ControllerExtensions.cs`).
- Return `ApiResponse<T>` via `.ToActionResult()`. `ApiResponse<T>` lives in Core;
  `ToActionResult()` lives in the API project.
- **Never** return raw `DbContext` results. All DB work goes through services in
  `PensionMS.Infrastructure/Services/` using `IUnitOfWork` (~180 repositories).
- **Branch scoping**: filter by branch ID unless `IsHQ()` is true.
- **SoD**: validate state-changing / financial operations via `ISoDGuard`
  (`Core/Interfaces/ISoDGuard.cs`, impl `Infrastructure/Services/SoDGuardService.cs`, rules in
  `Services/SoDRules/SoDRules.cs`). Only 3 services currently consume it:
  `FileStatusService`, `RetirementFileService`, `SalaryPaymentBatchService`.
- **Audit**: emit audit logs for status changes, financial calculations and admin actions via
  `IImmutableAuditService` (SHA-256 hash chain) or the `AuditSaveChangesInterceptor`.
- **Authorization**: `[Authorize(Policy = "...")]` where **policy name == permission name**, handled
  by `PermissionHandler`. ~145 `AddPolicy` calls, ~81 constants in
  `Core/Constants/PermissionConstants.cs`. There is **no default policy** — plain `[Authorize]`
  means "any authenticated user". Examples: `File.View.Sensitive` (`RetirementFilesController`),
  `Accounting.Manage`, `HR.Employee.Edit`.
- **Soft deletion is convention only.** `ISoftDeletable` is enforced by a **global query filter**
  (`PensionDbContext.cs:286-296`, applied by reflection over the interface), which hides soft-deleted
  rows from queries. But `Repository.Delete`/`DeleteRange` (`Repositories/Repository.cs:63-67`) calls
  `_dbSet.Remove(...)` — a real hard DELETE — and there is **no `SaveChanges` override or
  interceptor** converting deletes to soft deletes. Set `IsDeleted = true` in services; never call
  `Delete` on a soft-deletable entity.

### File status lifecycle
There is **no `FileStatus` enum**. `Core/Entities/FileStatus.cs` is a *lookup entity/table*
(`StatusID`, `StatusName`, `StatusColor`). Values live in
`Core/Constants/FileStatusConstants.cs` and are mirrored in `ui/src/constants/retirement.ts`:

```
New=1  Active=2  Suspended=3  Closed=4  Incomplete=5  Pending=6  UnderReview=7  Rejected=8
```

`Closed(4)` is terminal. The authoritative transition matrix is
`PensionMS.Infrastructure/Services/FileStatusService.cs:369-380` — read it, don't guess.
`Core/Entities/CommonEnums.cs` has separate `PendingReviewStatus` / `FileReviewStatus` enums.

---

## Frontend conventions

- Server state → TanStack Query v5. Mutations → `useApiMutation` from
  `@/hooks/useApiMutation` (handles cache invalidation, toasts, Arabic error extraction).
- Client state → Zustand in `ui/src/stores/`: `authStore`, `useNotificationStore` (note the `use`
  prefix), `themeStore`, `sidebarStore`, `navStore`.
- Forms → `react-hook-form` + `zod` v4 via `zodResolver`.
- RTL-first layout; use directional Tailwind classes.
- Radix UI primitives + `class-variance-authority` + `tailwind-merge` in `@/components/ui/`.

### Date/time contract — ADR 0001 (`docs/adr/0001-date-time-contract.md`)
This is the single most-violated convention. Read the ADR before touching any date code.

- DB stores **UTC**. API serializes via `YemenDateTimeConverter` (`Converters/YemenDateTimeConverter.cs`),
  writing **Asia/Aden local time without a `Z` suffix**.
- Frontend: **never** use `toLocaleDateString("ar-YE")` directly.
- Use `@/lib/date`: `todayYemen()`, `toDateOnlyYemen()`, `formatYemenDate()`,
  `formatYemenDateTime()`, `normalizeDateInputValue()`, `ageOnDate()`, `daysBetweenDateOnly()`.
- **`toISOString().split("T")[0]` and `.slice()` / `.substring()` on a `toISOString()` result are
  ESLint `error`s** (`no-restricted-syntax`, `ui/eslint.restricted-date-patterns.js`) — they are
  *wrong*, not just stylistically banned, because they silently shift the calendar day.
- Only `CreatedAt`/`UpdatedAt`/audit logs/`Date.now()` genuinely stay UTC/ISO.
- Known trap: `RiskScoringService.cs:31` uses "Arab Standard Time" (`Asia/Baghdad`, +03:00) —
  same offset as Yemen, so no bug today, but it is not what ADR 0001 mandates.
- Historical bug class: adding `+03:00` to a string already in local Yemen time converts it to UTC
  and shifts the date back a day.

### Design tokens
Source of truth is `ui/src/index.css` (~165 custom properties). Tokens are **bare HSL channel
triples** and must be wrapped:

```css
--gov-primary-700: 215 85% 22%;          /* in index.css */
/* usage */  bg-[hsl(var(--gov-primary-700))]  text-[hsl(var(--gov-success))]
```

- Families: `--gov-*` (brand/semantic), `--transition-*`, `--font-*`, chart + KPI color tokens.
- **Hardcoded hex/`hsl()` literals in components are the bug class being actively remediated**
  (`docs/design-token-audit.md`, `plan/design-token-remediation-plan.md`). Don't add new ones.
- Charts: import from `@/lib/chartColors` (`chartColors`, `pieChartColors`) rather than inlining.
- Theming: light/dark via the token blocks in `index.css`; verify both.
- Printing is a separate surface — `ui/src/styles/print.css` has its own rules.
- Honour `prefers-reduced-motion`: transitions must collapse to ~0.01ms.
- Audit workflow: fix in waves (M0–M6 in `plan/design-token-remediation-plan.md`), each F-ID traced
  to `file:line` in `docs/design-token-audit.md`. Update `docs/design-token-audit.md` in the same
  commit.

### ESLint (`ui/eslint.config.js`, flat config)
- `simple-import-sort/imports` and `unused-imports/no-unused-imports` are **warn** (auto-fixable).
- `react-hooks/purity`, `react-hooks/preserve-manual-memoization`,
  `react-hooks/set-state-in-effect` are **off** — setState-in-effect is idiomatic here.
- `@typescript-eslint/no-explicit-any` is an **error** and a major contributor to the lint baseline.

---

## Testing gotchas

- Backend tests hand-mock service constructors. **Adding a parameter to an Infrastructure service
  constructor breaks `PensionMS.Tests` compilation** (CS7036) — update the mocks in the same change.
- There are **no Clean Architecture boundary tests**. `HrSchemaBoundaryTests` only asserts HR table
  names lack a `_` prefix and that HR entities don't FK to pension aggregates.
- UI tests live in both `ui/src/lib/*.test.ts` and `ui/tests/*.test.ts`; `npm test` globs both.
- A second lint pass exists: `npm run lint:dates` (`eslint.dates.config.js`) applies **only** the
  date restrictions in `no-restricted-syntax`. `npm run lint` already includes them.

---

## Anti-patterns

- Do NOT use Next.js, SWR, RSC, or Server Actions — this is Vite + ASP.NET Core.
- Do NOT make raw EF queries in controllers.
- Do NOT bypass `ISoDGuard` for financial/approval paths.
- Do NOT hard-delete soft-deletable entities (see the soft-deletion caveat above).
- Do NOT hardcode hex/hsl colors in components — use `index.css` tokens.
- Do NOT write raw `toISOString().split("T")[0]`, `.slice()`, or `.substring()` for calendar dates.
- Do NOT create an EF migration expecting the standard flow — hand-written SQL in `scripts/` is the
  current schema path.
- Do NOT add a service without registering it in `Program.cs`.