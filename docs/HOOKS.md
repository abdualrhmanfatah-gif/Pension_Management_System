# Custom React Hooks

## Pension Management System — Hooks Reference

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Total Hooks** | 20+ |
| **Location** | `ui/src/hooks/` |
| **Query Library** | TanStack Query 5 |
| **State Library** | Zustand 5 |

### Hook Categories

| Category | Count | Hooks |
|----------|-------|-------|
| **UI/Interaction** | 6 | useDialogKeyboard, useDialogReset, useConfirm, useKeyboardNavigation, useDebouncedSearch, useToast |
| **API/Mutation** | 3 | useApiMutation, useCrudMutations, useInvalidateQueries |
| **Auth/Permission** | 2 | useRoleBasedUI, usePermission |
| **Data/Query** | 11 | File, retiree, entitled person, document, payment, settlement, suspension, ministry, branch, HR, investment queries |
| **Provider** | 2 | useSignalR, useNotification |

---

## 2. UI/Interaction Hooks

### 2.1 useDialogKeyboard

**File:** `ui/src/hooks/useDialogKeyboard.ts`

Keyboard shortcuts for dialog forms with Radix popper awareness.

```typescript
function useDialogKeyboard({
  onSave?: () => void,
  onClose?: () => void,
  isSaving?: boolean,
  isDirty?: boolean,
  enabled?: boolean
}): void
```

**Features:**
- **Ctrl+S:** Save (with Radix popper awareness)
- **Esc:** Close (only when no Radix popper is open)
- **Textarea-aware:** Doesn't trigger save when typing in textarea

**Radix Popper Awareness:**
```typescript
// Checks if any Radix popper/dropdown is open
const hasOpenPopper = document.querySelector('[data-radix-popper-content-wrapper]');
if (hasOpenPopper) return; // Don't handle keyboard
```

**Usage:**
```tsx
function EditDialog({ open, onClose, onSave }) {
  const { formState } = useForm();

  useDialogKeyboard({
    onSave: () => form.handleSubmit(onSave)(),
    onClose,
    isSaving: false,
    isDirty: formState.isDirty,
    enabled: open,
  });

  return <Dialog open={open} onOpenChange={onClose}>...</Dialog>;
}
```

---

### 2.2 useDialogReset

**File:** `ui/src/hooks/useDialogReset.ts`

Resets form when dialog opens/closes.

```typescript
function useDialogReset(
  open: boolean,
  reset: () => void,
  options?: { resetOnClose?: boolean }
): void
```

**Behavior:**
- Resets form when `open` changes from `true` to `false`
- Optionally resets when dialog opens

**Usage:**
```tsx
const { formState, reset } = useForm();

useDialogReset(open, reset, { resetOnClose: true });
```

---

### 2.3 useConfirm

**File:** `ui/src/hooks/useConfirm.tsx`

Promise-based confirmation dialog.

```typescript
function useConfirm(): {
  confirm: (options: ConfirmOptions) => Promise<boolean>;
  ConfirmDialog: React.FC<ConfirmDialogProps>;
}
```

**ConfirmOptions:**
```typescript
interface ConfirmOptions {
  title: string;
  description: string;
  confirmText?: string;
  cancelText?: string;
  variant?: 'danger' | 'warning' | 'info' | 'success';
}
```

**Usage:**
```tsx
function DeleteButton({ onDelete }) {
  const { confirm, ConfirmDialog } = useConfirm();

  const handleDelete = async () => {
    const confirmed = await confirm({
      title: 'حذف الملف',
      description: 'هل أنت متأكد من حذف هذا الملف؟',
      variant: 'danger',
      confirmText: 'حذف',
      cancelText: 'إلغاء',
    });

    if (confirmed) {
      onDelete();
    }
  };

  return (
    <>
      <Button variant="destructive" onClick={handleDelete}>
        حذف
      </Button>
      <ConfirmDialog />
    </>
  );
}
```

---

### 2.4 useKeyboardNavigation

**File:** `ui/src/hooks/useKeyboardNavigation.ts`

Arrow key navigation for lists.

```typescript
function useKeyboardNavigation(
  itemCount: number,
  onSelect: (index: number) => void
): {
  focusedIndex: number;
  setFocusedIndex: (index: number) => void;
  handleKeyDown: (e: React.KeyboardEvent) => void;
  reset: () => void;
}
```

**Features:**
- ArrowDown/ArrowUp navigation
- Enter to select
- Escape to reset
- Auto-scroll to focused item

**Usage:**
```tsx
function List({ items, onSelect }) {
  const { focusedIndex, handleKeyDown } = useKeyboardNavigation(
    items.length,
    onSelect
  );

  return (
    <div onKeyDown={handleKeyDown}>
      {items.map((item, index) => (
        <div
          key={item.id}
          data-row-index={index}
          className={focusedIndex === index ? 'bg-accent' : ''}
        >
          {item.name}
        </div>
      ))}
    </div>
  );
}
```

---

### 2.5 useDebouncedSearch

**File:** `ui/src/hooks/useDebouncedSearch.ts`

Debounced search input.

```typescript
function useDebouncedSearch(delay?: number): {
  search: string;
  setSearch: (value: string) => void;
  debouncedSearch: string;
  page: number;
  setPage: (page: number) => void;
  reset: () => void;
}
```

**Features:**
- 500ms default delay
- Auto-reset page to 1 on search change
- Reset function to clear all

**Usage:**
```tsx
function SearchableList() {
  const { search, setSearch, debouncedSearch, page, setPage } = useDebouncedSearch();

  const { data } = useQuery({
    queryKey: ['items', debouncedSearch, page],
    queryFn: () => fetchItems(debouncedSearch, page),
  });

  return (
    <div>
      <Input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder="بحث..."
      />
      <DataTable pagination={{ page, onChange: setPage }} />
    </div>
  );
}
```

---

### 2.6 useToast

**File:** `ui/src/hooks/useToast.ts`

Toast notification hook.

```typescript
function useToast(): {
  toast: (options: ToastOptions) => void;
}
```

**Usage:**
```tsx
const { toast } = useToast();

toast({
  title: 'تم الحفظ',
  description: 'تم حفظ البيانات بنجاح',
  variant: 'success',
});
```

---

## 3. API/Mutation Hooks

### 3.1 useApiMutation

**File:** `ui/src/hooks/useApiMutation.ts`

TanStack Query mutation wrapper with toast and invalidation.

```typescript
function useApiMutation<TData, TVariables>({
  mutationFn: (variables: TVariables) => Promise<TData>,
  invalidateKeys?: QueryKey[],
  successMessage?: string | ((data: TData) => string);
  errorMessage?: string;
  onSuccess?: (data: TData) => void;
}): UseMutationResult<TData, Error, TVariables>
```

**Features:**
- Automatic toast on success/error
- Query invalidation
- Loading state
- Error handling

**Usage:**
```tsx
function CreateFileForm() {
  const createFile = useApiMutation({
    mutationFn: (data: CreateFileDto) => api.post('/retirement-files', data),
    invalidateKeys: [['files']],
    successMessage: 'تم إنشاء الملف بنجاح',
    errorMessage: 'فشل إنشاء الملف',
  });

  return (
    <form onSubmit={createFile.mutate}>
      {/* Form fields */}
      <Button type="submit" loading={createFile.isPending}>
        إنشاء
      </Button>
    </form>
  );
}
```

---

### 3.2 useCrudMutations

**File:** `ui/src/hooks/useCrudMutations.ts`

Standardized CRUD mutations.

```typescript
function useCrudMutations<T, TCreate, TUpdate>({
  entityName: string,
  apiPath: string,
  queryKey: QueryKey,
}): {
  create: UseMutationResult<T, Error, TCreate>;
  update: UseMutationResult<T, Error, { id: number; data: TUpdate }>;
  delete: UseMutationResult<void, Error, number>;
}
```

**Usage:**
```tsx
function RetireesPage() {
  const { create, update, delete: remove } = useCrudMutations({
    entityName: 'المتقاعد',
    apiPath: '/retirees',
    queryKey: ['retirees'],
  });

  return (
    <div>
      <Button onClick={() => create.mutate(newRetiree)}>إضافة</Button>
      <Button onClick={() => update.mutate({ id, data: updates })}>تعديل</Button>
      <Button onClick={() => remove.mutate(id)}>حذف</Button>
    </div>
  );
}
```

---

### 3.3 useInvalidateQueries

**File:** `ui/src/hooks/useInvalidateQueries.ts`

Query invalidation helper.

```typescript
function useInvalidateQueries(): {
  invalidate: (keys: QueryKey) => Promise<void>;
}
```

**Usage:**
```tsx
const { invalidate } = useInvalidateQueries();

await invalidate(['files', fileId]);
await invalidate(['retirees']);
```

---

## 4. Auth/Permission Hooks

### 4.1 useRoleBasedUI

**File:** `ui/src/hooks/useRoleBasedUI.ts`

Permission-derived role booleans.

```typescript
function useRoleBasedUI(): {
  isAdmin: boolean;
  isRegionalManager: boolean;
  isBranchManager: boolean;
  isReviewer: boolean;
  isOperator: boolean;
  isViewer: boolean;
  isFinanceOfficer: boolean;
  isHRManager: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canApprove: boolean;
  canReview: boolean;
}
```

**Usage:**
```tsx
function FileActions({ file }) {
  const { canEdit, canDelete, canApprove } = useRoleBasedUI();

  return (
    <div>
      {canEdit && <Button onClick={handleEdit}>تعديل</Button>}
      {canDelete && <Button variant="destructive" onClick={handleDelete}>حذف</Button>}
      {canApprove && <Button variant="success" onClick={handleApprove}>موافقة</Button>}
    </div>
  );
}
```

---

### 4.2 usePermission

**File:** `ui/src/hooks/usePermission.ts`

Check specific permission.

```typescript
function usePermission(permission: string): boolean
```

**Usage:**
```tsx
const canViewAuditLogs = usePermission('Admin.Audit');

if (canViewAuditLogs) {
  // Show audit logs
}
```

---

## 5. Data/Query Hooks

### 5.1 File Queries

**File:** `ui/src/hooks/queries/useFileDetailsQuery.ts`

```typescript
function useFileDetailsQuery(fileId: number): UseQueryResult<FileDetails>
```

**File:** `ui/src/hooks/queries/useFilesQuery.ts`

```typescript
function useFilesQuery(params: FileQueryParams): UseQueryResult<PaginatedResult<File>>
```

**File:** `ui/src/hooks/queries/useFileStatusTransitionsQuery.ts`

```typescript
function useFileStatusTransitionsQuery(fileId: number): UseQueryResult<StatusTransition[]>
```

---

### 5.2 Retiree Queries

**File:** `ui/src/hooks/queries/useRetireesQuery.ts`

```typescript
function useRetireesQuery(params: RetireeQueryParams): UseQueryResult<PaginatedResult<Retiree>>
```

**File:** `ui/src/hooks/queries/useRetireeDetailsQuery.ts`

```typescript
function useRetireeDetailsQuery(retireeId: number): UseQueryResult<RetireeDetails>
```

---

### 5.3 Entitled Person Queries

**File:** `ui/src/hooks/queries/useEntitledPersonsQuery.ts`

```typescript
function useEntitledPersonsQuery(fileId: number): UseQueryResult<EntitledPerson[]>
```

---

### 5.4 Document Queries

**File:** `ui/src/hooks/queries/useDocumentsQuery.ts`

```typescript
function useDocumentsQuery(fileId: number): UseQueryResult<Document[]>
```

---

### 5.5 Payment Queries

**File:** `ui/src/hooks/queries/usePaymentBooksQuery.ts`

```typescript
function usePaymentBooksQuery(fileId: number): UseQueryResult<PaymentBook[]>
```

**File:** `ui/src/hooks/queries/useSalaryBatchesQuery.ts`

```typescript
function useSalaryBatchesQuery(params: BatchQueryParams): UseQueryResult<PaginatedResult<SalaryBatch>>
```

---

### 5.6 Settlement Queries

**File:** `ui/src/hooks/queries/useSettlementsQuery.ts`

```typescript
function useSettlementsQuery(fileId: number): UseQueryResult<SettlementDecision[]>
```

---

### 5.7 Suspension Queries

**File:** `ui/src/hooks/queries/useSuspensionsQuery.ts`

```typescript
function useSuspensionsQuery(fileId: number): UseQueryResult<PensionSuspension[]>
```

---

### 5.8 Ministry Queries

**File:** `ui/src/hooks/queries/useMinistryApprovalsQuery.ts`

```typescript
function useMinistryApprovalsQuery(params: MinistryQueryParams): UseQueryResult<PaginatedResult<MinistryApproval>>
```

---

### 5.9 Branch Queries

**File:** `ui/src/hooks/queries/useBranchesQuery.ts`

```typescript
function useBranchesQuery(): UseQueryResult<Branch[]>
```

---

### 5.10 HR Queries

**File:** `ui/src/hooks/queries/useHrEmployeesQuery.ts`

```typescript
function useHrEmployeesQuery(params: HrQueryParams): UseQueryResult<PaginatedResult<HrEmployee>>
```

---

### 5.11 Investment Queries

**File:** `ui/src/hooks/queries/useInvestmentPortfoliosQuery.ts`

```typescript
function useInvestmentPortfoliosQuery(): UseQueryResult<Portfolio[]>
```

---

## 6. Provider Hooks

### 6.1 useSignalR

**File:** `ui/src/components/providers/useSignalR.ts`

SignalR connection hook.

```typescript
function useSignalR(): {
  connection: HubConnection | null;
  isConnected: boolean;
}
```

**Usage:**
```tsx
function NotificationListener() {
  const { connection } = useSignalR();

  useEffect(() => {
    if (connection) {
      connection.on('ReceiveNotification', (notification) => {
        // Handle notification
      });
    }
  }, [connection]);

  return null;
}
```

---

### 6.2 useNotification

**File:** `ui/src/stores/useNotificationStore.ts`

Notification state hook.

```typescript
function useNotificationStore(): {
  notifications: Notification[];
  unreadCount: number;
  markAsRead: (id: number) => void;
  setNotifications: (notifications: Notification[]) => void;
  setUnreadCount: (count: number) => void;
}
```

---

## 7. Query Key Factory

**File:** `ui/src/hooks/queries/queryKeys.ts`

```typescript
export const queryKeys = {
  files: {
    all: ['files'] as const,
    detail: (id: number) => ['files', id] as const,
    reviews: (id: number) => ['files', id, 'reviews'] as const,
    statusTransitions: (id: number) => ['files', id, 'status-transitions'] as const,
  },
  retirees: {
    all: ['retirees'] as const,
    detail: (id: number) => ['retirees', id] as const,
    statement: (id: number) => ['retirees', id, 'statement'] as const,
  },
  entitledPersons: {
    all: ['entitled-persons'] as const,
    byFile: (fileId: number) => ['entitled-persons', 'file', fileId] as const,
  },
  documents: {
    all: ['documents'] as const,
    byFile: (fileId: number) => ['documents', 'file', fileId] as const,
  },
  paymentBooks: {
    all: ['payment-books'] as const,
    byFile: (fileId: number) => ['payment-books', 'file', fileId] as const,
  },
  salaryBatches: {
    all: ['salary-batches'] as const,
    detail: (id: number) => ['salary-batches', id] as const,
  },
  settlements: {
    all: ['settlements'] as const,
    detail: (id: number) => ['settlements', id] as const,
  },
  suspensions: {
    all: ['suspensions'] as const,
    byFile: (fileId: number) => ['suspensions', 'file', fileId] as const,
  },
  ministryApprovals: {
    all: ['ministry-approvals'] as const,
    detail: (id: number) => ['ministry-approvals', id] as const,
  },
  branches: {
    all: ['branches'] as const,
  },
  hr: {
    employees: ['hr', 'employees'] as const,
    attendance: ['hr', 'attendance'] as const,
    leaveRequests: ['hr', 'leave-requests'] as const,
  },
  investment: {
    portfolios: ['investment', 'portfolios'] as const,
    projects: ['investment', 'projects'] as const,
    properties: ['investment', 'properties'] as const,
  },
  dashboard: {
    stats: ['dashboard', 'stats'] as const,
  },
  lookups: {
    all: ['lookups'] as const,
    ranks: ['lookups', 'ranks'] as const,
    banks: ['lookups', 'banks'] as const,
    relationships: ['lookups', 'relationships'] as const,
    documentTypes: ['lookups', 'document-types'] as const,
  },
};
```

---

## 8. Usage Patterns

### 8.1 Standard Page Pattern

```tsx
function RetireesPage() {
  const { search, setSearch, debouncedSearch, page, setPage } = useDebouncedSearch();
  const { canCreate, canEdit, canDelete } = useRoleBasedUI();
  const { confirm, ConfirmDialog } = useConfirm();

  const { data, isLoading } = useRetireesQuery({
    search: debouncedSearch,
    page,
  });

  const { create, update, delete: remove } = useCrudMutations({
    entityName: 'المتقاعد',
    apiPath: '/retirees',
    queryKey: queryKeys.retirees.all,
  });

  const handleDelete = async (id: number) => {
    const confirmed = await confirm({
      title: 'حذف المتقاعد',
      description: 'هل أنت متأكد؟',
      variant: 'danger',
    });

    if (confirmed) {
      remove.mutate(id);
    }
  };

  return (
    <div>
      <Input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder="بحث..."
      />

      {canCreate && (
        <Button onClick={() => create.mutate(newRetiree)}>إضافة</Button>
      )}

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        loading={isLoading}
        pagination={{ page, onChange: setPage, total: data?.totalCount ?? 0 }}
      />

      <ConfirmDialog />
    </div>
  );
}
```

### 8.2 Form Dialog Pattern

```tsx
function EditRetireeDialog({ open, onClose, retireeId }) {
  const { data: retiree } = useRetireeDetailsQuery(retireeId);
  const { reset } = useForm();

  useDialogReset(open, reset);

  const updateMutation = useApiMutation({
    mutationFn: (data) => api.put(`/retirees/${retireeId}`, data),
    invalidateKeys: [queryKeys.retirees.all],
    successMessage: 'تم التحديث بنجاح',
  });

  useDialogKeyboard({
    onSave: () => form.handleSubmit(updateMutation.mutate)(),
    onClose,
    isSaving: updateMutation.isPending,
    enabled: open,
  });

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent>
        <form onSubmit={updateMutation.mutate}>
          {/* Form fields */}
          <FormDialogFooter
            onSave={() => form.handleSubmit(updateMutation.mutate)()}
            onCancel={onClose}
            isSaving={updateMutation.isPending}
          />
        </form>
      </DialogContent>
    </Dialog>
  );
}
```

---

## 9. File Structure

```
ui/src/hooks/
├── queries/
│   ├── queryKeys.ts                    # Query key factory
│   ├── useFileDetailsQuery.ts          # File details
│   ├── useFilesQuery.ts                # File list
│   ├── useFileStatusTransitionsQuery.ts # Status transitions
│   ├── useRetireesQuery.ts             # Retiree list
│   ├── useRetireeDetailsQuery.ts       # Retiree details
│   ├── useEntitledPersonsQuery.ts      # Entitled persons
│   ├── useDocumentsQuery.ts            # Documents
│   ├── usePaymentBooksQuery.ts         # Payment books
│   ├── useSalaryBatchesQuery.ts        # Salary batches
│   ├── useSettlementsQuery.ts          # Settlements
│   ├── useSuspensionsQuery.ts          # Suspensions
│   ├── useMinistryApprovalsQuery.ts    # Ministry approvals
│   ├── useBranchesQuery.ts             # Branches
│   ├── useHrEmployeesQuery.ts          # HR employees
│   └── useInvestmentPortfoliosQuery.ts # Investment portfolios
├── useDialogKeyboard.ts                # Dialog keyboard shortcuts
├── useDialogReset.ts                   # Dialog form reset
├── useConfirm.tsx                      # Confirmation dialog
├── useKeyboardNavigation.ts            # Arrow key navigation
├── useDebouncedSearch.ts               # Debounced search
├── useToast.ts                         # Toast notifications
├── useApiMutation.ts                   # API mutation wrapper
├── useCrudMutations.ts                 # CRUD mutations
├── useInvalidateQueries.ts             # Query invalidation
├── useRoleBasedUI.ts                   # Role-based UI
└── usePermission.ts                    # Permission check
```
