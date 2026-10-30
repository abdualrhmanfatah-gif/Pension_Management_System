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
<<<<<<< HEAD
│  │  Domain Services (99 C# source files)                │   │
│  │  ┌────────────────┐ ┌────────────────┐ ┌──────────┐  │   │
│  │  │RetirementFile  │ │EntitledPerson  │ │Settlement│  │   │
│  │  │Commands       │ │Service        │ │Service   │  │   │
│  │  └────────────────┘ └────────────────┘ └──────────┘  │   │
│  │  ┌────────────────┐ ┌────────────────┐ ┌──────────┐  │   │
│  │  │FileReview      │ │SalaryPayment   │ │Financial │  │   │
│  │  │Service        │ │BatchService    │ │Entitlement│  │   │
=======
│  │  Business Logic Services (~50 implementations)       │   │
│  │  ┌────────────────┐ ┌────────────────┐ ┌──────────┐  │   │
│  │  │RetirementFile  │ │EntitledPerson  │ │Settlement│  │   │
│  │  │Service (2609L) │ │Service (760L)  │ │Service   │  │   │
│  │  └────────────────┘ └────────────────┘ └──────────┘  │   │
│  │  ┌────────────────┐ ┌────────────────┐ ┌──────────┐  │   │
│  │  │FileReview      │ │SalaryPayment   │ │Financial │  │   │
│  │  │Service (1065L) │ │BatchService    │ │Entitlement│  │   │
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
| DI Configuration | 18 files | Explicit domain registrations in Infrastructure/DependencyInjection, invoked from Program.cs |
=======
| DI Configuration | 1 | Service registration in `Program.cs` |
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
| Service Source Files | 99 | Domain directories, partial report files and internal helpers |
| Interface Definition Files | 97 | Contracts in Core/Interfaces |
| Background Jobs | 2 | Expired pledges, outbox processor |
=======
| Service Implementations | ~50 | Business logic execution |
| Interface Definitions | ~90 | Contracts (in Core) |
| Background Jobs | 2 | Expired pledges, outbox processor |
| SoD Guards | 3 | Separation of duties enforcement |
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
| Engines | 3 | Compliance, entitlement calculation, workflow |
| ScopedPermissionResolver | 1 | Runtime permission resolution |

**Key Responsibilities:**
- Business rule enforcement
- Transaction management
- Audit trail creation
- File review workflow orchestration
- Financial calculation engines
- Background task processing
<<<<<<< HEAD
- Action permission and business-state validation

#### تنظيم الخدمات حسب المجال

توجد خدمات التقاعد في `Services/Pension/RetirementFile` و`Beneficiaries` و`Entitlements`، وخدمات صرف المعاش في `Services/Payroll`، ودفاتر الصرف في مجلد مستقل `Services/PaymentBooks` بتسجيل `AddPaymentBooksServices()`، والتسويات في `Services/Settlements`. تبقى المحاسبة وHR والاستثمار والمخزون والأصول وبوابة المواطن في مجلداتها المتخصصة.

تجتمع الخدمات المشتركة في `Documents` و`Security/SoD` و`Audit` و`Notifications` و`Administration`. جذر `Services` لا يحتوي ملفات تنفيذ مباشرة، والـ namespaces تطابق مسارات المجلدات.

| المسؤولية | التنفيذ | العقد |
|---|---|---|
| تعديل ملفات التقاعد والوفاة وتصحيح الحالة الحياتية | `Pension/RetirementFile/RetirementFileService` | `IRetirementFileService` |
| قراءة الملفات والبحث والإحصاءات وسجلات التغيير | `Pension/RetirementFile/FileQueryService` | `IFileQueryService` |
| تغيير الحالة وعرض الانتقالات ومعالجة التعهدات | `Pension/RetirementFile/FileStatusService` | `IFileStatusService` |
| بيانات تقرير ملفات التقاعد وملخصه | `Reporting/RetirementFileReportService` | `IRetirementFileReportService` |

تشترك القراءة والتعديل في مساعدي `RetirementFileDtoMapper` و`RetirementFileCompletenessEvaluator` الداخليين، دون اعتماد خدمة القراءة على خدمة التعديل. خدمات بيانات التقارير داخل `Services/Reporting`؛ أدوات Excel/PDF داخل `Reporting/Exporters` وتعريفات التقارير داخل `Reporting/ReportRegistry.cs`.

جميع عقود الخدمات في Core، بما فيها عقود بيانات التقارير والتصدير المحاسبي والمعاملات البنكية وعميل الإشعارات. `NotificationHub` داخل `Services/Notifications`، وربطه على `/hubs/notifications` يبقى في API.

يستدعي Program.cs الدالة `AddPensionInfrastructureServices()` التي تجمع التسجيلات اليدوية للمجالات. إعدادات DbContext وJWT والسياسات وMiddleware وSignalR وValidators تبقى في API؛ لا يوجد تسجيل خدمات عبر assembly scanning. Registry وأدوات التصدير Singleton وخدمات البيانات والأعمال Scoped، وقواعد SoD المتعددة تحافظ على ترتيبها.

داخل `Jobs` يوجد معالجا الخلفية المسجلان `ExpiredPledgesJob` و`NotificationOutboxProcessor`. `PledgeExpiryCheckerService` موجود لكنه غير مسجل. خدمات المواطن الأربع غير المفعلة تبقى غير مسجلة؛ المسجل هو SMS وOTP.

`FileStatusService` تطبق مصفوفة دورة الحياة عند تغيير الحالة وعرض الانتقالات، وتمنع الانتقال من Closed. أثناء المراجعة المعلقة تقتصر قائمة الانتقالات على UnderReview إن كان انتقالًا مسموحًا من الحالة الحالية. شروط الاستحقاق المالي ودفتر الصرف والمستلم والتعهدات وSoD تظل مفحوصة عند التنفيذ؛ قائمة الانتقالات ليست ضمانًا لاستيفاء هذه الشروط.

خريطة النقل وسجل التنفيذ في [خطة تنظيم الخدمات](../plan/services-reorganization-plan.md).
=======
- Separation of duties validation
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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

<<<<<<< HEAD
#### حفظ تغييرات ملفات التقاعد ومراجعتها

يمتلك `FileReviewService` بدء المراجعة وبنود فروق البيانات وتدقيقها وإيقاف المراجعة، ويستخدم الطلب الصريح والحفظ التلقائي العمليات الداخلية نفسها. يلتقط `Infrastructure/Interceptors/FileReviewChangeInterceptor.cs` تغييرات EF وروابط الملفات والقيم السابقة، ثم يفوض آثار المراجعة إلى المالك؛ يفوض حماية بنود الصرف إلى العمليات الداخلية في `SalaryPaymentBatchService` التي تعيد استخدام `PensionPaymentStatusRules` في Core. لا يحتاج هذا الربط بناء خدمات التطبيق من داخل إنشاء السياق أو إضافة خدمات وسيطة.

| المسؤولية | المصدر السابق | الإجراء | السبب |
| --- | --- | --- | --- |
| بدء المراجعة وبنودها وإيقافها | `FileReviewService` والمنطق داخل جزء السياق المحذوف | تمديد المالك الحالي | تنفيذ واحد للطلب الصريح والتغييرات المكتشفة |
| تتبع تغييرات EF والربط بالحفظ | `PensionDbContext.RetirementLifecycle.cs` | استبدال بمعترض واحد | إبقاء التفاصيل التقنية خارج مالك قرارات المراجعة، دون زيادة عدد ملفات الإنتاج |
| حماية الحوالات | جزء السياق المحذوف و`SalaryPaymentBatchService` | تمديد خدمة الصرف وقاعدتها الحالية | حماية الحفظ المباشر دون نقل مسؤولية الصرف إلى المراجعة |
| النسخة المعتمدة | `ApprovedPensionSnapshot` | إعادة استخدام وتمديد الحفظ المتزامن | الحفاظ على مصدر حساب المعاش وشكل النسخة المخزنة دون حظر async |
| المعاملة وقفل الملف | غلاف حفظ السياق و`UnitOfWork` | إعادة استخدام القفل وتبسيط غلاف الحفظ | مالك واحد للتثبيت والتراجع، مع استمرار تعويضات الموارد الخارجية |

يسجل `PensionDbContext.OnConfiguring` معترضًا خاصًا بكل سياق، ليشمل أيضًا السياقات المنشأة مباشرة. يقتصر غلاف `SaveChanges` في السياق على تنسيق مراحل الحفظ والمعاملة وإعادة الدخول؛ تبقى الأقفال حتى اكتمال حفظ البيانات وبنود المراجعة والتدقيق والإيقاف. المعاملة الموجودة تبقى ملكًا للمستدعي، وأقفال مراجعة الملفات يعاد استخدامها من `UnitOfWork`. استُبدل ملف `PensionDbContext.RetirementLifecycle.cs` بهذا التوزيع دون إضافة ملفات Workflow أو Coordinator أو تغيير عقود HTTP.

تبقى `ApprovedPensionSnapshot` تمثيل النسخة المعتمدة الذي تستخدمه المراجعة وتوليد المعاشات؛ لا تحل فروق التغييرات محلها، ولا تستبدلها التعديلات المعلقة. أمثلة حالات الملف وفصل المهام القديمة في هذا المستند ليست مصدر السياسة الحالية؛ تُراجع ثوابت الحالات والخدمات والقواعد والاختبارات الحالية وفق `AGENTS.md` عند تغيير سلوك العمل.

قيد سابق رُصد أثناء التحقق: تصحيح مفاتيح الإنشاء في `AuditSaveChangesInterceptor` يدعم `int` فقط، بينما `FileReviewItem.ItemID` من نوع `long`. لا يعالج نقل المسؤوليات هذا القيد في التدقيق العام؛ تدقيق `ReviewDataChange` المخصص يستمر في حفظ روابط الملف والكيان والقيم السابقة والجديدة. أُعيد إنتاج القيد باستخدام السياق السابق، وتختبر حالات النقل تصحيح مفاتيح الإيقافات المدعومة وسلامة روابط تدقيق المراجعة والتراجع عن كليهما.

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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

<<<<<<< HEAD
خدمات الأعمال تستخدم `IUnitOfWork` و`IRepository<T>` للوصول إلى البيانات؛ ينفذ Infrastructure المستودعات باستخدام EF Core. بعض الخدمات التقنية مثل Workflow وSnapshot تستخدم DbContext مباشرة داخل Infrastructure، وتبقى هذه الاستثناءات ظاهرة دون تغيير سلوكها أثناء تنظيم المجلدات.
=======
EF Core DbContext acts as the repository. No explicit repository interfaces — services interact with DbContext directly for query composition, while business operations go through service interfaces.
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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

<<<<<<< HEAD
### 3.4 صلاحية الإجراء

منذ قرار 2026-10-20، لا يُشترط اختلاف منفذ الاعتماد عن منشئ السجل أو محرره. تحمي سياسات API كل إجراء بصلاحيته، ويجوز للمستخدم تنفيذ الإدخال والاعتماد إذا امتلك الصلاحيتين. أُزيلت خدمات وقواعد فصل المهام وتسجيلاتها من DI. تبقى فحوص حالة البيانات ونطاق الفرع وإيقافات الصرف والتدقيق مستقلة عن هذا القرار؛ انظر [Security.md](Security.md).
=======
### 3.4 Separation of Duties (SoD) Guards

Three guards enforce dual-control on sensitive operations:

| Guard | Scope |
|-------|-------|
| `SettlementSoDGuard` | Settlement creation vs approval |
| `SalaryBatchSoDGuard` | Salary batch creation vs approval |
| `EntitlementSoDGuard` | Entitlement calculation vs disbursement |

**Rule:** The person who creates a record cannot approve it.
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
<<<<<<< HEAD
- Permissions and their branch scopes are resolved per operation by `ScopedPermissionResolver` from active explicit role assignments; personal revocation overrides grants.
- `ICurrentUserContext` supplies authenticated identity only. `IAccessiblePensionRepository` binds that identity and the operation permission to an immutable `IPensionDataQuery`; its ownership predicates run in SQL before paging and aggregation.
- A user's read and write scopes may differ. A branch request filter narrows the allowed scope. Home branch and JWT role claims cannot grant central access.
- The retiree identity registry remains shared; associated pension files, documents and financial rows use the permission's allowed branches. Shared retiree edits require coverage of every linked file.
- Global administration and unpartitioned settlement/payroll workflows require an explicit central scope. Unsupported ownership is denied for branch-restricted queries.
- Legacy `PensionFileAccess`, `BranchScopeProvider` and `ReportingQueries` were removed. Expired pledges use the dedicated `IExpiredPledgeProcessor` background capability.
- See [Security.md](Security.md) and [the rollout plan](../plan/branch-access-unification-plan.md) for assignment semantics, transfer rules and the SQL preparation required before deployment.
=======
- Permissions resolved via `ScopedPermissionResolver`
- Branch-scoped data access (users only see their branch's data)
- Permission tiers: Branch → Regional → National
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
<<<<<<< HEAD
User → API Controller → Service Interface → IUnitOfWork / Repository → DbContext → Database
=======
User → API Controller → RetirementFileService → DbContext → Database
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
