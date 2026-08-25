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
