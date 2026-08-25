# Domain Constants

## Pension Management System — Constants Reference

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Total Constants** | 50+ |
| **Location** | `ui/src/constants/index.ts` |
| **Backend Location** | `PensionMS.Core/Constants/` |

---

## 2. File Status Constants

### 2.1 File Status

| Constant | Value | Arabic | Description |
|----------|-------|--------|-------------|
| `NEW` | 1 | جديد | Newly created |
| `PENDING_REVIEW` | 2 | قيد الانتظار | Submitted for review |
| `UNDER_REVIEW` | 3 | قيد المراجعة | Actively being reviewed |
| `APPROVED` | 4 | مقبول | Review passed |
| `REJECTED` | 5 | مرفوض | Review failed |
| `RESUBMITTED` | 6 | أعيد الإرسال | Rejected file resubmitted |
| `SUSPENDED` | 7 | معلق | Pension suspended |
| `COMPLETED` | 8 | مكتمل | File processing complete |
| `ARCHIVED` | 9 | أرشيف | File archived |

### 2.2 Status Transitions

```typescript
FILE_STATUS_TRANSITIONS = {
  NEW: ['PENDING_REVIEW'],
  PENDING_REVIEW: ['UNDER_REVIEW'],
  UNDER_REVIEW: ['APPROVED', 'REJECTED'],
  REJECTED: ['RESUBMITTED'],
  RESUBMITTED: ['UNDER_REVIEW'],
  APPROVED: ['COMPLETED'],
  SUSPENDED: ['UNDER_REVIEW'],
  COMPLETED: ['ARCHIVED'],
};
```

---

## 3. Batch Status Constants

### 3.1 Batch Status

| Constant | Arabic | Usage |
|----------|--------|-------|
| `DRAFT` | مسودة | Initial draft state |
| `APPROVED` | معتمد | Approved by authority |
| `NEW` | جديد | Newly created |
| `IMPORTED` | تم الاستيراد | Data imported |
| `PROCESSING` | قيد المعالجة | Being processed |
| `COMPLETED` | مكتمل | Processing complete |

---

## 4. Financial Constants

### 4.1 Transaction Type

| Constant | Value | Usage |
|----------|-------|-------|
| `INCOME` | "Income" | Income transaction |
| `EXPENSE` | "Expense" | Expense transaction |

### 4.2 Treasury Source

| Constant | Value | Arabic | Usage |
|----------|-------|--------|-------|
| `BANK` | "Bank" | بنك | Bank transaction |
| `CASH` | "Cash" | نقدي | Cash transaction |

### 4.3 Account Nature

| Constant | Value | Usage |
|----------|-------|-------|
| `DEBIT` | "Debit" | Debit account |
| `CREDIT` | "Credit" | Credit account |

### 4.4 Account Levels

| Level | Value | Name | Description |
|-------|-------|------|-------------|
| `BOOK` | 1 | دفتر | Top-level book |
| `MAIN` | 2 | رئيسي | Main account |
| `SUB` | 3 | فرعي | Sub account |
| `DETAIL` | 4 | تفصيلي | Detail account |
| `POSTABLE` | 5 | قابل للقيد | Postable account |

### 4.5 Journal Status

| Constant | Value | Usage |
|----------|-------|-------|
| `DRAFT` | "Draft" | Draft journal entry |
| `POSTED` | "Posted" | Posted to ledger |
| `REVERSED` | "Reversed" | Reversed entry |

### 4.6 Source Module

| Constant | Value | Usage |
|----------|-------|-------|
| `MANUAL` | "Manual" | Manual entry |
| `SALARY` | "Salary" | Salary batch entry |
| `SETTLEMENT` | "Settlement" | Settlement entry |

---

## 5. Recipient Constants

### 5.1 Recipient Type

| Constant | Value | Arabic | Description |
|----------|-------|--------|-------------|
| `RETIREE` | "Retiree" | متقاعد | The retired person |
| `AGENT` | "Agent" | وكيل | Legal representative |
| `ENTITLED` | "Entitled" | مستحق | Entitled person |
| `GUARDIAN` | "Guardian" | وصي | Guardian of minor |

### 5.2 Recipient Type Arabic

```typescript
RECIPIENT_TYPE_AR = {
  RETIREE: "متقاعد",
  AGENT: "وكيل",
  ENTITLED: "مستحق",
  GUARDIAN: "وصي",
};
```

---

## 6. View Mode Constants

| Constant | Value | Arabic | Description |
|----------|-------|--------|-------------|
| `DATA_ENTRY` | "dataEntry" | إدخال بيانات | Data entry mode |
| `DOCUMENT_UPLOADER` | "documentUploader" | رفع وثائق | Document upload mode |
| `REVIEWER` | "reviewer" | مراجع | Review mode |
| `FINANCIAL_OFFICER` | "financialOfficer" | مالي | Financial officer mode |

---

## 7. Review Constants

### 7.1 Review Action

| Constant | Value | Arabic | Description |
|----------|-------|--------|-------------|
| `RETURN` | "return" | إرجاع | Return to previous state |
| `REJECT` | "reject" | رفض | Reject submission |
| `APPROVE` | "approve" | قبول | Approve submission |

### 7.2 Review Sections

| Constant | Value | Arabic |
|----------|-------|--------|
| `BASIC` | "basic" | البيانات الأساسية |
| `PEOPLE` | "people" | الأشخاص |
| `DOCUMENTS` | "documents" | الوثائق |
| `FINANCIAL` | "financial" | المالية |
| `HISTORY` | "history" | التاريخ |

### 7.3 Pending Review Status

| Constant | Value | Description |
|----------|-------|-------------|
| `NONE` | 0 | No review needed |
| `PENDING_REVIEW` | 1 | Pending review |
| `APPROVED` | 2 | Review approved |
| `REJECTED` | 3 | Review rejected |

---

## 8. Salary Constants

### 8.1 Salary Status

| Constant | Value | Arabic | Description |
|----------|-------|--------|-------------|
| `ACTIVE` | "نشط" | نشط | Active salary |
| `SUSPENDED` | "موقوف" | موقوف | Suspended salary |

### 8.2 Suspension Status

| Constant | Value | Arabic | Description |
|----------|-------|--------|-------------|
| `ACTIVE` | "فعّال" | فعّال | Active suspension |

---

## 9. Document Constants

### 9.1 Book Types

| Value | Label | Arabic |
|-------|-------|--------|
| `إلكتروني` | إلكتروني (بطاقة صرف) | Electronic payment card |
| `يدوي` | يدوي (دفتر قديم) | Manual old booklet |

### 9.2 Legacy Book Colors

```typescript
LEGACY_BOOK_COLORS = ["أبيض", "أخضر", "أزرق"];
// White, Green, Blue
```

### 9.3 Document Status Colors

| Status | Color | Usage |
|--------|-------|-------|
| `uploaded` | `text-green-600` | Uploaded documents |
| `missing` | `text-red-600` | Missing documents |
| `pledge` | `text-amber-600` | Pledged documents |
| `exempted` | `text-purple-600` | Exempted documents |

---

## 10. Person Constants

### 10.1 Person Type

| Constant | Value | Arabic |
|----------|-------|--------|
| `ENTITLED` | "entitled" | مستحق |
| `AGENT` | "agent" | وكيل |
| `GUARDIAN` | "guardian" | وصي |

### 10.2 Person Document Filter

| Constant | Value | Arabic |
|----------|-------|--------|
| `ALL` | "all" | الكل |
| `UPLOADED` | "uploaded" | مرفوعة |
| `MISSING` | "missing" | ناقصة |

---

## 11. HR Constants

### 11.1 Gender

| Constant | Value | Arabic |
|----------|-------|--------|
| `MALE` | 1 | ذكر |
| `FEMALE` | 2 | أنثى |

### 11.2 Attendance Type

| Constant | Value | Arabic |
|----------|-------|--------|
| `CHECK_IN` | "CheckIn" | حضور |
| `CHECK_OUT` | "CheckOut" | انصراف |

### 11.3 Leave Status

| Constant | Value | Arabic |
|----------|-------|--------|
| `PENDING` | "Pending" | قيد الانتظار |
| `APPROVED` | "Approved" | مقبول |
| `REJECTED` | "Rejected" | مرفوض |

### 11.4 Approval Status

| Constant | Value | Arabic |
|----------|-------|--------|
| `PENDING` | "Pending" | قيد الانتظار |
| `APPROVED` | "Approved" | مقبول |
| `REJECTED` | "Rejected" | مرفوض |

---

## 12. Investment Constants

### 12.1 Project Status

| Constant | Value | Arabic |
|----------|-------|--------|
| `ACTIVE` | "Active" | نشط |
| `PLANNED` | "Planned" | مخطط |
| `COMPLETED` | "Completed" | مكتمل |
| `ON_HOLD` | "OnHold" | معلق |

---

## 13. Expense Constants

### 13.1 Expense Status

| Constant | Value | Arabic |
|----------|-------|--------|
| `NEW` | "جديد" | جديد |
| `UNDER_REVIEW` | "تحت المراجعة" | تحت المراجعة |
| `APPROVED` | "معتمد" | معتمد |
| `DISBURSED` | "تم الصرف" | تم الصرف |
| `CANCELLED` | "ملغي" | ملغي |
| `PENDING_CLASSIFICATION` | "بانتظار التصنيف" | بانتظار التصنيف |

---

## 14. UI Constants

### 14.1 Badge Variant

| Constant | Value | Usage |
|----------|-------|-------|
| `SUCCESS` | "success" | Positive status |
| `WARNING` | "warning" | Caution status |
| `DANGER` | "danger" | Negative status |
| `INFO` | "info" | Informational |
| `GHOST` | "ghost" | Subtle badge |
| `OUTLINE` | "outline" | Outlined badge |

### 14.2 Column Type

| Constant | Value | Usage |
|----------|-------|-------|
| `CURRENCY` | "currency" | Currency formatting |
| `PERCENT` | "percent" | Percentage formatting |
| `DATE` | "date" | Date formatting |
| `NUMBER` | "number" | Number formatting |

### 14.3 General Status

| Constant | Value | Arabic |
|----------|-------|--------|
| `ACTIVE` | "Active" | نشط |
| `INACTIVE` | "Inactive" | غير نشط |

---

## 15. Miscellaneous Constants

### 15.1 Gap Status

| Constant | Value | Arabic | Usage |
|----------|-------|--------|-------|
| `MATCHED` | "مطابق" | مطابق | Records match |
| `NOT_IN_SYSTEM` | "غير موجود في النظام" | غير موجود في النظام | Not found |

### 15.2 Priority

| Constant | Value | Arabic |
|----------|-------|--------|
| `URGENT` | "Urgent" | عاجل |
| `HIGH` | "High" | مرتفع |
| `NORMAL` | "Normal" | عادي |
| `LOW` | "Low" | منخفض |

### 15.3 Audit Change Type

| Constant | Value | Arabic |
|----------|-------|--------|
| `CREATE` | "Create" | إنشاء |
| `DELETE` | "Delete" | حذف |
| `STATUS_CHANGE` | "StatusChange" | تغيير حالة |

### 15.4 Boolean String

| Constant | Value |
|----------|-------|
| `TRUE` | "true" |
| `FALSE` | "false" |

### 15.5 Tenant Type

| Constant | Value | Arabic |
|----------|-------|--------|
| `COMPANY` | "Company" | شركة |
| `INDIVIDUAL` | "Individual" | فرد |
| `MERCHANT` | "Merchant" | تاجر |

### 15.6 Retiree Tab

| Constant | Value | Arabic |
|----------|-------|--------|
| `APPROVAL` | "approval" | الاعتماد |
| `FILES` | "files" | الملفات |
| `SETTLEMENTS` | "settlements" | التسويات |
| `ENTITLEMENTS` | "entitlements" | المستحقات |

### 15.7 Pledge Tab

| Constant | Value | Arabic |
|----------|-------|--------|
| `PLEDGES` | "pledges" | الرهون |
| `EXEMPTIONS` | "exemptions" | الإعفاءات |

---

## 16. Backend Constants

### 16.1 Permission Constants (120+)

```typescript
// File permissions
Files.View, Files.Create, Files.Edit, Files.Delete, Files.Review, Files.Approve

// Retiree permissions
Retirees.View, Retirees.Create, Retirees.Edit, Retirees.Delete

// Entitled Person permissions
EntitledPersons.View, EntitledPersons.Create, EntitledPersons.Edit, EntitledPersons.Delete

// Document permissions
Documents.View, Documents.Upload, Documents.Delete, Documents.Validate

// Payment permissions
Payments.View, Payments.Create, Payments.Approve, Payments.Execute

// Settlement permissions
Settlements.View, Settlements.Create, Settlements.Approve, Settlements.Execute

// Suspension permissions
Suspensions.View, Suspensions.Create, Suspensions.End

// Report permissions
Reports.View, Reports.Export

// Admin permissions
Admin.Users, Admin.Roles, Admin.Audit, Admin.Settings, Admin.Branches

// Investment permissions
Investment.View, Investment.Create, Investment.Edit, Investment.Delete

// HR permissions
HR.View, HR.Create, HR.Edit, HR.Delete
```

### 16.2 Role Constants

| Role | Access Level | Arabic |
|------|-------------|--------|
| `Admin` | Full access (national) | مدير |
| `RegionalManager` | Regional branch access | مدير إقليمي |
| `BranchManager` | Branch access | مدير فرع |
| `Reviewer` | File review permissions | مراجع |
| `Operator` | Data entry permissions | مشغّل |
| `Viewer` | Read-only access | مشاهد |
| `FinanceOfficer` | Financial operations | موظف مالي |
| `HRManager` | HR module access | مدير الموارد البشرية |

### 16.3 Document Owner Types

| Value | Name | Arabic |
|-------|------|--------|
| 1 | Retiree | متقاعد |
| 2 | EntitledPerson | مستحق |
| 3 | Agent | وكيل |
| 4 | Guardian | وصي |
| 5 | General | عام |

### 16.4 Risk Types

| Type | Weight | Description |
|------|--------|-------------|
| File Age | 25% | Older files = higher risk |
| Missing Docs | 25% | More missing = higher risk |
| Pending Review | 25% | Longer pending = higher risk |
| Suspension History | 15% | More suspensions = higher risk |
| Financial Anomalies | 10% | Unusual patterns = higher risk |

### 16.5 Currency & Thresholds

| Constant | Value | Usage |
|----------|-------|-------|
| Currency | YER | Yemeni Rial |
| Risk Threshold | 1,500,000 | RiskScoringService |
| Timezone | GMT+3 | Arabic Standard Time |
| Default Password | Admin@123 | Default admin |
| JWT Expiry | 60 minutes | Token expiry |
| Refresh Token Expiry | 1 day | Refresh token |
| Rate Limit | 10 req/min | Sensitive endpoints |

---

## 17. File Structure

```
ui/src/constants/
├── index.ts                    # Main constants file
├── assets.ts                   # Asset constants
├── hr-event-types.ts           # HR event types
└── inventory.ts                # Inventory constants

PensionMS.Core/Constants/
├── PermissionConstants.cs      # Backend permissions
├── RoleConstants.cs            # Backend roles
├── StatusConstants.cs          # Status values
└── DocumentConstants.cs        # Document types
```

---

## 18. Usage Patterns

### 18.1 Status Badge

```tsx
import { Badge } from "@/components/ui";
import { BATCH_STATUS } from "@/constants";

function StatusBadge({ status }: { status: string }) {
  const variant = status === BATCH_STATUS.APPROVED ? 'success' :
                  status === BATCH_STATUS.DRAFT ? 'warning' :
                  status === BATCH_STATUS.COMPLETED ? 'info' : 'secondary';

  return <Badge variant={variant}>{status}</Badge>;
}
```

### 18.2 Conditional Rendering

```tsx
import { useRoleBasedUI } from "@/hooks/useRoleBasedUI";

function FileActions() {
  const { canEdit, canDelete, canApprove } = useRoleBasedUI();

  return (
    <div>
      {canEdit && <Button>تعديل</Button>}
      {canDelete && <Button variant="destructive">حذف</Button>}
      {canApprove && <Button variant="success">موافقة</Button>}
    </div>
  );
}
```

### 18.3 Column Formatting

```tsx
import { COLUMN_TYPE } from "@/constants";

const columns = [
  {
    accessorKey: 'amount',
    type: COLUMN_TYPE.CURRENCY, // Formats as YER
  },
  {
    accessorKey: 'percentage',
    type: COLUMN_TYPE.PERCENT, // Formats as percentage
  },
  {
    accessorKey: 'date',
    type: COLUMN_TYPE.DATE, // Formats as DD/MM/YYYY
  },
];
```
