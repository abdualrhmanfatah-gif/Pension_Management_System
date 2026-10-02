# Pension Management System (PensionMS)

## نظام إدارة ملفات التقاعد — مديرية أمن التأمينات

Enterprise full-stack application for managing retirement files, entitled persons, financial settlements, and salary disbursements for Yemen's General Directorate of Security Retirement Fund.

---

## 1. Project Overview

| Aspect | Detail |
|--------|--------|
| **Domain** | Government pension & retirement fund management |
| **Organization** | General Directorate of Security Retirement Fund (مديرية أمن التأمينات) |
| **Architecture** | Monolith with clean architecture (4-layer) |
| **Backend** | .NET 8 / C# / ASP.NET Core Web API |
| **Frontend** | React 19 / TypeScript / Vite / Tailwind CSS |
| **Database** | SQL Server Express (local) / SQL Server (production) |
| **Auth** | JWT Bearer tokens + role-based access control |
| **Deployment** | IIS on Windows Server (ports 5141 API / 80 UI) |

---

## 2. Solution Structure

```
D:\Pension_Management_System\
├── PensionMS.API/                  # ASP.NET Core Web API (entry point)
├── PensionMS.Core/                 # Domain layer — entities, enums, interfaces
├── PensionMS.Infrastructure/       # Data access, services, background jobs
├── PensionMS.CitizenAPI/           # Public-facing citizen portal API
├── PensionMS.ScannerAgent/         # Background scanner agent
├── PensionMS.Tests/                # Unit tests
├── ui/                             # React SPA frontend
├── docs/                           # This documentation folder
├── deploy.ps1                      # Full deployment script (API + UI + IIS)
├── deploy-api.ps1                  # API-only deployment
├── deploy-ui.ps1                   # UI-only deployment
└── IMPLEMENTATION_PLAN_REDESIGN.md # Legacy design plan
```

### 2.1 Backend Projects

| Project | Purpose |
|---------|---------|
| **PensionMS.API** | Web API controllers, middleware, DI configuration, startup |
| **PensionMS.Core** | Domain entities (~150), enums, interfaces (~90), shared constants |
| **PensionMS.Infrastructure** | Service implementations (~50), EF Core DbContext, background jobs, guards |
| **PensionMS.CitizenAPI** | Separate API for public citizen-facing features |
| **PensionMS.ScannerAgent** | Background agent for document scanning processing |
| **PensionMS.Tests** | Unit tests for core business logic |

### 2.2 Frontend Structure

```
ui/src/
├── components/          # Reusable UI components (Radix UI + Tailwind)
├── pages/               # Page-level components (~100+ pages)
│   ├── retirement-files/ # 23 tabbed sub-pages for file management
│   ├── retirees/         # Retiree management pages
│   ├── dashboard/        # Dashboard & analytics
│   └── ...
├── hooks/               # React hooks
│   ├── queries/          # TanStack Query hooks (data fetching)
│   └── mutations/        # TanStack Query mutations (data writing)
├── stores/              # Zustand state stores (auth, UI state)
├── types/               # TypeScript type definitions
├── lib/                 # Utility functions, API client setup
└── routes/              # Route definitions
```

---

## 3. Core Business Domain

### 3.1 Primary Entities

| Entity | Description |
|--------|-------------|
| **RetirementFile** | Central entity — a complete retirement case file |
| **Retiree** | The retired person (police officer / security personnel) |
| **EntitledPerson** | Dependents eligible for pension (wife, children, etc.) |
| **Agent** | Legal representative handling the file |
| **Guardian** | Guardian of minor entitled persons |
| **Document** | Attached document (certificate, ID, court ruling, etc.) |
| **MissingDocument** | Tracked missing document with status workflow |
| **PaymentBook** | Monthly payment book/booklet for pension disbursement |
| **SalaryRecipient** | Person receiving salary payments |
| **SalaryPaymentBatch** | Monthly batch of salary payments |
| **SalaryPaymentItem** | Individual line item in a payment batch |
| **FinancialEntitlement** | Calculated financial entitlement |
| **SettlementDecision** | Financial settlement decision record |
| **SettlementsBatch** | Batch of settlement decisions |
| **MinistryApprovalItem** | Ministry approval for file |
| **PensionSuspension** | Active pension suspension record |
| **FileReviewItem** | Review checklist item for file |
| **FileStatus** | Status change log for file |
| **FileNote** | Notes/comments on a file |
| **FileSnapshot** | Immutable snapshot of file state |
| **DocumentPledge** | Document pledge with expiry tracking |
| **DocumentExemption** | Document exemption record |

### 3.2 Key Workflows

1. **File Lifecycle**: New File → Pending Review → Under Review → Approved / Rejected
2. **Document Processing**: Upload → Owner Classification → Validation → Approval
3. **Payment Generation**: Monthly Batch → Item Generation → Approval → Disbursement
4. **Settlement**: Entitlement Calculation → Decision → Batch → Ministry Approval → Payment
5. **Suspension**: Detection → Suspension → Notification → Reactivation

---

## 4. Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- SQL Server Express (local) or SQL Server
- IIS with ASP.NET Core Module (production)

### Development

**Backend:**
```bash
cd PensionMS.API
dotnet run
# API starts on http://0.0.0.0:5001
```

**Frontend:**
```bash
cd ui
npm install
npm run dev
# Frontend starts on http://localhost:5173
```

**Default Credentials:**
- Username: `admin`
- Password: `Admin@123`

### Production Deployment
```powershell
.\deploy.ps1                          # Full deploy (API + UI)
.\deploy-api.ps1                      # API only
.\deploy-ui.ps1                       # UI only
.\deploy.ps1 -SkipDbReset             # Deploy without DB migration
.\deploy.ps1 -SkipIISConfig           # Deploy without IIS site setup
```

---

## 5. Key Configuration

| Config | Location | Description |
|--------|----------|-------------|
| Connection String | `PensionMS.API/appsettings.json` | SQL Server connection |
| JWT Secret | `appsettings.json` → `JwtSettings:SecretKey` | Token signing key |
| Encryption Key | `appsettings.json` → `EncryptionKey` | Data encryption key |
| File Storage | `appsettings.json` → `FileStorage:BasePath` | Document upload path |
| API URL | `ui/.env` | `VITE_API_URL=http://localhost:5001/api` |
| Production URL | `ui/.env.production` | Production API endpoint |

---

## 6. Technology Stack

### Backend
| Package | Version | Purpose |
|---------|---------|---------|
| Entity Framework Core | 8.0.11 | ORM / Data Access |
| BCrypt.Net-Next | 4.0.3 | Password hashing |
| EPPlus | 7.5.0 | Excel export/import |
| QuestPDF | 2022.12.1 | PDF generation |
| System.IdentityModel.Tokens.Jwt | 8.0.0 | JWT authentication |
| FluentValidation | 11.x | Input validation |
| ASP.NET Core SignalR | 8.x | Real-time notifications |

### Frontend
| Package | Version | Purpose |
|---------|---------|---------|
| React | 19.x | UI framework |
| TypeScript | 5.x | Type safety |
| Vite | 6.x | Build tool & dev server |
| Tailwind CSS | 4.x | Utility-first styling |
| Radix UI | latest | Accessible UI primitives |
| TanStack Query | 5.x | Server state management |
| Zustand | 5.x | Client state management |
| React Router | 7.x | Client-side routing |
| Axios | 1.x | HTTP client |

---

## 7. Documentation Index

### Core Documentation

| Document | Description |
|----------|-------------|
| [Architecture.md](./Architecture.md) | System architecture, layers, patterns |
| [Database.md](./Database.md) | Entity relationships, schema, migrations |
| [API.md](./API.md) | REST API endpoints reference |
| [BusinessRules.md](./BusinessRules.md) | Business logic, workflows, state machines |
| [Deployment.md](./Deployment.md) | IIS deployment, configuration, troubleshooting |
| [Frontend.md](./Frontend.md) | React app structure, components, routing |
| [Security.md](./Security.md) | Authentication, authorization, permissions |

### Component & Design Documentation

| Document | Description |
|----------|-------------|
| [COMPONENTS.md](./COMPONENTS.md) | Complete UI component library (54 components) |
| [DESIGN_SYSTEM.md](./DESIGN_SYSTEM.md) | Colors, typography, spacing, animations, RTL patterns |
| [HOOKS.md](./HOOKS.md) | Custom React hooks (20+ hooks) |
| [CONSTANTS.md](./CONSTANTS.md) | Domain constants reference (50+ constants) |
| [PRINT_SYSTEM.md](./PRINT_SYSTEM.md) | Print components and patterns |

---

## 8. Quick Reference

### Project Statistics

| Metric | Value |
|--------|-------|
| **Total Entities** | 77 |
| **Total Services** | 50+ |
| **Total Controllers** | 62 |
| **Total Endpoints** | ~400 |
| **Total UI Components** | 54 |
| **Total Custom Hooks** | 20+ |
| **Total Permissions** | 120+ |
| **Total Constants** | 50+ |

### Key Files

| File | Purpose |
|------|---------|
| `PensionMS.API/Program.cs` | Backend entry point, DI configuration |
| `PensionMS.Infrastructure/Data/PensionDbContext.cs` | EF Core DbContext (50+ DbSets) |
| `ui/src/App.tsx` | Frontend routing (583 lines) |
| `ui/src/stores/authStore.ts` | Authentication state (610 lines) |
| `ui/src/index.css` | CSS variables, themes, print styles |
| `ui/src/components/ui/index.ts` | UI component barrel exports |

### Default Credentials

| Field | Value |
|-------|-------|
| **Username** | `admin` |
| **Password** | `Admin@123` |

### Ports

| Service | Development | Production |
|---------|-------------|------------|
| **API** | `http://localhost:5001` | `http://192.168.1.10:5141` |
| **UI** | `http://localhost:5173` | `http://192.168.1.10` |

---

## 9. License

Internal government system — not for public distribution.
