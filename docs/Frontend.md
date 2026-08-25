# Frontend Documentation

## Pension Management System — React SPA

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Framework** | React 19 |
| **Language** | TypeScript 5 |
| **Build Tool** | Vite 6 |
| **Styling** | Tailwind CSS 4 |
| **UI Primitives** | Radix UI |
| **Server State** | TanStack Query 5 |
| **Client State** | Zustand 5 |
| **Routing** | React Router 7 |
| **HTTP Client** | Axios |
| **Charts** | Recharts |
| **Tables** | TanStack Table |

---

## 2. Project Structure

```
ui/
├── public/                    # Static assets
├── src/
│   ├── components/           # Reusable UI components
│   │   ├── ui/               # Base UI components (buttons, inputs, etc.)
│   │   ├── layout/           # Layout components (sidebar, header, etc.)
│   │   ├── forms/            # Form components
│   │   └── shared/           # Shared domain components
│   ├── pages/                # Page components (~100+)
│   │   ├── dashboard/        # Dashboard pages
│   │   ├── retirement-files/ # File management (23 tabbed pages)
│   │   ├── retirees/         # Retiree management
│   │   ├── entitled-persons/ # Entitled person management
│   │   ├── documents/        # Document management
│   │   ├── payments/         # Payment management
│   │   ├── settlements/      # Settlement management
│   │   ├── reports/          # Report pages
│   │   ├── admin/            # Admin pages (users, settings, audit)
│   │   ├── investment/       # Investment module
│   │   ├── hr/               # HR module
│   │   └── auth/             # Login, password change
│   ├── hooks/                # Custom React hooks
│   │   ├── queries/          # TanStack Query hooks (data fetching)
│   │   ├── mutations/        # TanStack Query mutations (data writing)
│   │   └── shared/           # Shared hooks
│   ├── stores/               # Zustand stores
│   │   ├── authStore.ts      # Authentication state (610 lines)
│   │   └── uiStore.ts        # UI state (sidebar, theme)
│   ├── types/                # TypeScript type definitions
│   │   ├── api.ts            # API response types
│   │   ├── entities.ts       # Domain entity types
│   │   └── index.ts          # Type exports
│   ├── lib/                  # Utilities
│   │   ├── api.ts            # Axios instance setup
│   │   ├── utils.ts          # General utilities
│   │   └── constants.ts      # Frontend constants
│   ├── routes/               # Route definitions
│   │   └── index.tsx         # Route config
│   ├── App.tsx               # Root component with router (583 lines)
│   └── main.tsx              # Entry point
├── .env                      # Development environment
├── .env.production           # Production environment
├── index.html                # HTML template
├── package.json              # Dependencies
├── tailwind.config.js        # Tailwind configuration
├── tsconfig.json             # TypeScript configuration
├── vite.config.ts            # Vite configuration
└── web.config                # IIS SPA routing config
```

---

## 3. Environment Configuration

### 3.1 Development (.env)

```
VITE_API_URL=http://localhost:5001/api
```

### 3.2 Production (.env.production)

```
VITE_API_URL=http://192.168.1.10:5141/api
```

### 3.3 Accessing Environment Variables

```typescript
const apiUrl = import.meta.env.VITE_API_URL;
```

---

## 4. Authentication Store (`authStore.ts`)

Zustand store managing authentication state (610 lines).

### 4.1 State

```typescript
interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  permissions: Permission[];
  isAuthenticated: boolean;
  isLoading: boolean;
}
```

### 4.2 Actions

| Action | Description |
|--------|-------------|
| `login(username, password)` | Authenticate user, store tokens |
| `logout()` | Clear tokens, redirect to login |
| `refreshToken()` | Refresh expired JWT |
| `loadUser()` | Load user from storage on app start |
| `hasPermission(permission)` | Check if user has permission |
| `hasAnyPermission(permissions)` | Check if user has any of permissions |
| `changePassword(oldPass, newPass)` | Change password |

### 4.3 Token Persistence

- Tokens stored in `localStorage`
- Auto-refresh on token expiry
- Redirect to `/login` on auth failure

---

## 5. Routing (`App.tsx`)

583 lines defining all application routes.

### 5.1 Route Structure

```typescript
/                       → Dashboard
/login                  → Login page
/change-password        → Password change

/retirement-files       → File list
/retirement-files/new   → Create file
/retirement-files/:id   → File details (tabbed view)
  /overview             → Overview tab
  /documents            → Documents tab
  /entitled-persons     → Entitled persons tab
  /payments             → Payments tab
  /settlements          → Settlements tab
  /suspensions          → Suspensions tab
  /review               → Review tab
  /history              → History tab
  /notes                → Notes tab

/retirees               → Retiree list
/retirees/:id           → Retiree details

/entitled-persons       → Entitled person list
/entitled-persons/:id   → Entitled person details

/documents              → Document management
/payments               → Payment management
/settlements            → Settlement management
/suspensions            → Suspension management

/reports                → Reports hub
/reports/:type          → Specific report

/admin                  → Admin hub
/admin/users            → User management
/admin/roles            → Role management
/admin/audit-logs       → Audit logs
/admin/settings         → System settings
/admin/branches         → Branch management

/investment             → Investment module
/hr                     → HR module
```

### 5.2 Protected Routes

All routes except `/login` require authentication:

```typescript
<ProtectedRoute>
  <DashboardPage />
</ProtectedRoute>
```

### 5.3 Role-Based Routes

```typescript
<Route element={<RequirePermission permission="Admin.Users.View" />}>
  <Route path="/admin/users" element={<UsersPage />} />
</Route>
```

---

## 6. Key Pages

### 6.1 Retirement File Details (23 Tabs)

`/retirement-files/:id` renders `FileWorkflowManager.tsx` which manages 23 tabbed sub-pages:

| Tab | Component | Description |
|-----|-----------|-------------|
| Overview | `FileOverviewTab` | Summary, status, basic info |
| Documents | `DocumentProcessPage` | Document upload/validation (890 lines) |
| Entitled Persons | `EntitledPersonsTab` | EP management |
| Payment Books | `PaymentBooksTab` | Payment book management |
| Salary Recipients | `SalaryRecipientsTab` | Salary recipient management |
| Settlements | `SettlementsTab` | Settlement decisions |
| Suspensions | `SuspensionsTab` | Pension suspensions |
| Financial | `FinancialTab` | Financial entitlements |
| Ministry | `MinistryTab` | Ministry approvals |
| Review | `ReviewTab` | File review checklist |
| Status | `StatusTab` | Status history |
| Notes | `NotesTab` | File notes |
| History | `HistoryTab` | Change history |
| Snapshot | `SnapshotTab` | File snapshots |
| Agent | `AgentTab` | Agent management |
| Guardian | `GuardianTab` | Guardian management |
| Pledges | `PledgesTab` | Document pledges |
| Exemptions | `ExemptionsTab` | Document exemptions |
| Missing | `MissingDocsTab` | Missing documents |
| Death | `DeathHandlingTab` | Death handling workflow |
| Reports | `FileReportsTab` | File-specific reports |
| Audit | `AuditTab` | Audit trail |
| Compliance | `ComplianceTab` | Compliance status |

### 6.2 Document Processing Page

`DocumentProcessPage.tsx` (890 lines) handles:
- Document upload with drag-and-drop
- Owner type classification (Retiree/EP/Agent)
- Document type selection
- Validation workflow
- Missing document detection
- Document preview

### 6.3 Dashboard

`DashboardPage.tsx` displays:
- File statistics (by status)
- Retiree counts
- Financial summaries
- Recent activity
- Charts (Recharts)
- Quick actions

---

## 7. Data Fetching

### 7.1 TanStack Query Hooks

Located in `hooks/queries/`:

```typescript
// useFileDetailsQuery.ts
export function useFileDetailsQuery(fileId: number) {
  return useQuery({
    queryKey: ['file', fileId],
    queryFn: () => api.get(`/retirement-files/${fileId}`),
    staleTime: 30_000,
  });
}
```

### 7.2 Query Keys Pattern

```typescript
// Consistent query key factory
export const queryKeys = {
  files: {
    all: ['files'] as const,
    detail: (id: number) => ['files', id] as const,
    reviews: (id: number) => ['files', id, 'reviews'] as const,
  },
  retirees: {
    all: ['retirees'] as const,
    detail: (id: number) => ['retirees', id] as const,
  },
};
```

### 7.3 Mutations

Located in `hooks/mutations/`:

```typescript
// useCreateFileMutation.ts
export function useCreateFileMutation() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateFileDto) => api.post('/retirement-files', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.files.all });
    },
  });
}
```

---

## 8. UI Components

### 8.1 Base Components (Radix UI)

| Component | Source | Usage |
|-----------|--------|-------|
| Button | Radix | All actions |
| Dialog | Radix | Modals |
| Dropdown Menu | Radix | Actions menus |
| Select | Radix | Dropdowns |
| Tabs | Radix | File detail tabs |
| Table | TanStack Table | Data tables |
| Form | React Hook Form | All forms |
| Toast | Radix | Notifications |

### 8.2 Layout Components

| Component | Description |
|-----------|-------------|
| `AppLayout` | Main layout with sidebar |
| `Sidebar` | Navigation sidebar |
| `Header` | Top header with user info |
| `Breadcrumb` | Navigation breadcrumbs |
| `PageHeader` | Page title + actions |

### 8.3 Form Components

| Component | Description |
|-----------|-------------|
| `FormField` | Label + input + error |
| `DateInput` | Date picker |
| `FileUpload` | Drag-and-drop file upload |
| `SearchInput` | Search with debounce |
| `FilterBar` | Filter controls |

---

## 9. State Management

### 9.1 Server State (TanStack Query)

- Automatic caching
- Background refetching
- Optimistic updates
- Error handling

### 9.2 Client State (Zustand)

| Store | Purpose |
|-------|---------|
| `authStore` | User, tokens, permissions |
| `uiStore` | Sidebar open, theme, locale |

### 9.3 URL State

Some pages use URL params for filters:

```typescript
const [searchParams, setSearchParams] = useSearchParams();
const page = parseInt(searchParams.get('page') || '1');
```

---

## 10. API Integration

### 10.1 Axios Setup

```typescript
// lib/api.ts
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor: attach JWT
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Response interceptor: handle 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

### 10.2 File Upload

```typescript
const formData = new FormData();
formData.append('file', file);
formData.append('ownerType', '1');
formData.append('documentTypeId', '5');

await api.post('/documents', formData, {
  headers: { 'Content-Type': 'multipart/form-data' },
});
```

---

## 11. Building & Deployment

### 11.1 Development

```bash
cd ui
npm install
npm run dev     # Vite dev server on :5173
```

### 11.2 Production Build

```bash
npm run build   # Output to dist/
```

### 11.3 Build Output

```
dist/
├── index.html
├── assets/
│   ├── index-[hash].js       # Main bundle
│   ├── index-[hash].css      # Main styles
│   └── vendor-[hash].js      # Vendor bundle
└── favicon.ico
```

### 11.4 Deploy to IIS

```powershell
.\deploy-ui.ps1
# Copies dist/ to E:\inetpub\PensionUI
# Ensures web.config for SPA routing
# Restarts IIS site
```

---

## 12. Performance

| Metric | Target |
|--------|--------|
| First Contentful Paint | < 1.5s |
| Largest Contentful Paint | < 2.5s |
| Time to Interactive | < 3s |
| Bundle Size (gzip) | < 200KB |

### 12.1 Optimizations

- Code splitting by route
- Lazy loading of heavy components
- TanStack Query caching
- Image optimization
- Vite tree shaking

---

## 13. Accessibility

- Radix UI primitives (WCAG 2.1 AA)
- Keyboard navigation
- Screen reader support
- Focus management
- Color contrast compliance
