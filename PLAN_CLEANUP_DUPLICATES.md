# خطة تنظيف الكود المكرر والميت — شاملة

**التاريخ:** 2026-08-20
**الحالة:** جاهزة للتنفيذ
**المدة المقدرة:** 12-16 ساعة عمل
**عدد الملفات المحذوفة:** 12 ملف ميت
**عدد الملفات المعدّلة:** 90+ ملف

---

## فهرس المحتويات

1. [ملخص الإيجادات](#1-ملخص-الإيجادات)
2. [المرحلة 1: حذف الكود الميت (12 ملف)](#2-المرحلة-1-حذف-الكود-الميت-12-ملف)
3. [المرحلة 2: توحيد Types المركزية](#3-المرحلة-2-توحيد-types-المركزية)
4. [المرحلة 3: إصلاح API Modules المكررة](#4-المرحلة-3-إصلاح-api-modules-المكررة)
5. [المرحلة 4: دمج المكونات المكررة](#5-المرحلة-4-دمج-المكونات-المكررة)
6. [المرحلة 5: تنظيف Raw API Calls](#6-المرحلة-5-تنظيم-raw-api-calls)
7. [المرحلة 6: تنظيف Hooks المتراكبة](#7-المرحلة-6-تنظيم-hooks-المتراكبة)
8. [ملخص جميع الملفات](#8-ملخص-جميع-الملفات)
9. [خطة الاختبار](#9-خطة-الاختبار)
10. [خطة التنفيذ المقترحة](#10-خطة-التنفيذ-المقترحة)

---

## 1. ملخص الإيجادات

| الفئة | عدد الإيجادات | الخطورة |
|--------|---------------|---------|
| مكونات React ميتة (dead code) | 9 ملفات | 🔴HIGH |
| API modules مكررة (نفس endpoints) | 14 مجال في pension.ts | 🔴CRITICAL |
| Types مكررة (ApiResponse, PagedResult) | 25 تعريف → المفروض 2 | 🔴HIGH |
| Raw API calls خارج api/ | ~35 site | 🟡MEDIUM |
| Hooks متراكبة | 5 مجموعات | 🟡MEDIUM |
| مكونات مكررة (same domain) | 3 مجموعات | 🟡MEDIUM |
| Backend controllers متراكبة | 3 | 🟡MEDIUM |

---

## 2. المرحلة 1: حذف الكود الميت (12 ملف)

### 2.1 مكونات React ميتة (لا أحد يستخدمها)

| # | الملف | السبب | الكود البديل |
|---|-------|-------|-------------|
| 1 | `ui/src/components/domain/retirement/UpdateStatusDialog.tsx` | DEAD — فقط export من index.ts، لا imports | `StatusUpdateDialog.tsx` |
| 2 | `ui/src/components/domain/retirement/CreateRetirementFileDialog.tsx` | DEAD — أُسقط بـ RetirementFileDialog | `RetirementFileDialog.tsx` (mode="create") |
| 3 | `ui/src/components/domain/retirement/SuspensionDialog.tsx` | DEAD — لا imports | `CreateSuspensionDialog.tsx` |
| 4 | `ui/src/components/domain/retirement/FinancialDialog.tsx` | DEAD — لا imports | `AddFinancialEntitlementDialog.tsx` |
| 5 | `ui/src/components/ui/QuickStats.tsx` | DEAD — لا استخدام في JSX | لا يوجد |
| 6 | `ui/src/components/ui/CardToolbar.tsx` | DEAD — لا استخدام في JSX | لا يوجد |

### 2.2 صفحات ميتة (dead pages)

| # | الملف | السبب | الكود البديل |
|---|-------|-------|-------------|
| 7 | `ui/src/pages/inventory/SimpleStocktakePage.tsx` | DEAD — Route يشير لـ StocktakePage | `StocktakePage.tsx` |
| 8 | `ui/src/pages/inventory/AssetStocktakePage.tsx` | DEAD — لا imports أبداً | `StocktakePage.tsx` |
| 9 | `ui/src/pages/finance/FinanceDashboardPage.tsx` | DEAD — لا imports أبداً | `AccountingDashboardPage.tsx` |

### 2.3 Types ميتة

| # | الملف | السبب |
|---|-------|-------|
| 10 | `ui/src/types/api/common.ts` | DEAD — لا أحد يستورده (66 سطر). الـ canonical: `types/api.ts` |

### 2.4 إجراءات التنفيذ لكل ملف

**الملف 1: UpdateStatusDialog.tsx**
```
1. حذف الملف: ui/src/components/domain/retirement/UpdateStatusDialog.tsx
2. حذف export من: ui/src/components/domain/retirement/index.ts (سطر 33)
3. tsc --noEmit للتأكد
```

**الملف 2: CreateRetirementFileDialog.tsx**
```
1. حذف الملف
2. حذف export من index.ts (سطر 6)
3. tsc --noEmit
```

**الملف 3: SuspensionDialog.tsx**
```
1. حذف الملف
2. حذف export من index.ts (سطر 32)
3. tsc --noEmit
```

**الملف 4: FinancialDialog.tsx**
```
1. حذف الملف
2. حذف export من index.ts (سطر 18)
3. tsc --noEmit
```

**الملف 5: QuickStats.tsx**
```
1. حذف الملف: ui/src/components/ui/QuickStats.tsx
2. حذف export من ui/src/components/ui/index.ts (سطر 52)
3. tsc --noEmit
```

**الملف 6: CardToolbar.tsx**
```
1. حذف الملف: ui/src/components/ui/CardToolbar.tsx
2. حذف export من ui/src/components/ui/index.ts (سطر 15)
3. tsc --noEmit
```

**الملف 7-9: صفحات ميتة**
```
1. حذف: ui/src/pages/inventory/SimpleStocktakePage.tsx
2. حذف: ui/src/pages/inventory/AssetStocktakePage.tsx
3. حذف: ui/src/pages/finance/FinanceDashboardPage.tsx
4. tsc --noEmit
```

**الملف 10: types/api/common.ts**
```
1. حذف الملف: ui/src/types/api/common.ts
2. tsc --noEmit (تأكد لا أحد يستورده)
```

**الملف 11: DocumentProcessDialog.tsx** (invoices بعد الدمج — المرحلة 4)
```
→ يُدمج في DocumentProcessPage.tsx ثم يُحذف (المرحلة 4)
```

**الملف 12: SuspensionsPage.tsx** أو **PensionSuspensionsPage.tsx** (invoices بعد الدمج — المرحلة 4)
```
→ أحدهما يُدمج ثم يُحذف (المرحلة 4)
```

---

## 3. المرحلة 2: توحيد Types المركزية

### 3.1 المشكلة

**25 تعريف مكرر** لنفس الأنواع الأساسية:

| النوع | المكان الصحيح | عدد النسخ المكررة |
|--------|---------------|-------------------|
| `ApiResponse<T>` | `types/api.ts:6` | 14 نسخة مكررة في api/*.ts |
| `PagedResult<T>` | `types/api.ts:12` | 9 نسخات مكررة في api/*.ts |
| `ApiError` | `types/api.ts:22` | مكرر في types/api/common.ts |

**ملاحظة مهمة:** `types/api/common.ts` و `types/api.ts` يختلفان في الحقول:
```typescript
// types/api.ts (الصحيح — الأكثر استخداماً)
interface ApiResponse<T> { isSuccess: boolean; data: T; message?: string; }
interface PagedResult<T> { items: T[]; totalCount: number; page: number; pageSize: number; ... }

// types/api/common.ts ( DEAD — لا أحد يستورده)
interface ApiResponse<T> { success: boolean; data: T; message?: string; }  // مختلف!
interface PagedResult<T> { items: T[]; totalPages: number; }  // أبسط
```

### 3.2 خطة التنفيذ

**الخطوة 1: حذف types/api/common.ts** (المرحلة 1 أعلاه — ميت بالفعل)

**الخطوة 2: حذف التعريفات المكررة من كل API file**

الملفات المطلوب تعديلها (حذف local ApiResponse/PagedResult واستيراد من types/api):

```
ui/src/api/documents.ts      — حذف سطر 8-15 (ApiResponse تعريف محلي)
ui/src/api/salaries.ts       — حذف سطر 10-17 (ApiResponse تعريف محلي)
ui/src/api/notifications.ts  — حذف سطر 5-8 (ApiResponse تعريف محلي)
ui/src/api/ministry.ts       — حذف سطر 3-8 + سطر 9-15 (ApiResponse + PagedResult)
ui/src/api/hr.ts             — حذف سطر 3-8 + سطر 9-15 (ApiResponse + PagedResult)
ui/src/api/investment.ts     — حذف سطر 7-12 (ApiResponse تعريف محلي)
ui/src/api/fixedAssets.ts    — حذف سطر 167-172 (ApiResponse تعريف محلي)
ui/src/api/reports.ts        — حذف سطر 8-15 + سطر 33-40 (ApiResponse + PagedResult)
ui/src/api/vouchers.ts       — حذف سطر 61-68 + سطر 68-73 (PagedResult + ApiResponse)
ui/src/api/discrepancy.ts    — حذف سطر 44-49 + سطر 52-57 (PagedResult + ApiResponse)
ui/src/api/archive.ts        — حذف سطر 44-50 + سطر 50-55 (ApiResponse + PagedResult) — non-exported
ui/src/api/correspondence.ts — حذف سطر 72-78 (ApiResponse) — non-exported
ui/src/api/retirees.ts       — حذف سطر 16-22 (PagedResult تعريف محلي)
ui/src/api/settlements.ts    — حذف سطر 3-8 (PagedResult تعريف محلي)
```

**الخطوة 3: إضافة import في كل ملف**
```typescript
import type { ApiResponse, PagedResult } from "@/types/api";
```

**الخطوة 4: tsc --noEmit للتأكد من عدم وجود أخطاء**

### 3.3 ملخص التعديلات

| الملف | الإجراء | الحقول المتأثرة |
|--------|---------|-----------------|
| `api/documents.ts` | حذف 8 سطور + إضافة import | ApiResponse |
| `api/salaries.ts` | حذف 8 سطور + إضافة import | ApiResponse |
| `api/notifications.ts` | حذف 4 سطور + إضافة import | ApiResponse |
| `api/ministry.ts` | حذف 13 سطر + إضافة import | ApiResponse + PagedResult |
| `api/hr.ts` | حذف 13 سطر + إضافة import | ApiResponse + PagedResult |
| `api/investment.ts` | حذف 6 سطور + إضافة import | ApiResponse |
| `api/fixedAssets.ts` | حذف 6 سطور + إضافة import | ApiResponse |
| `api/reports.ts` | حذف 15 سطر + إضافة import | ApiResponse + PagedResult |
| `api/vouchers.ts` | حذف 12 سطر + إضافة import | PagedResult + ApiResponse |
| `api/discrepancy.ts` | حذف 12 سطر + إضافة import | PagedResult + ApiResponse |
| `api/archive.ts` | حذف 12 سطر + إضافة import | ApiResponse + PagedResult |
| `api/correspondence.ts` | حذف 7 سطور + إضافة import | ApiResponse |
| `api/retirees.ts` | حذف 7 سطور + إضافة import | PagedResult |
| `api/settlements.ts` | حذف 6 سطور + إضافة import | PagedResult |

**المجموع: 14 ملف، ~130 سطر محذوف**

---

## 4. المرحلة 3: إصلاح API Modules المكررة

### 4.1 المشكلة الأساسية

**`api/pension.ts` (572 سطر)** يحتوي على نسخ كاملة من:

| الدالة في pension.ts | الملف الأصلي | الـ Endpoint المكرر |
|----------------------|-------------|---------------------|
| `salaryBatchApi.*` | `api/salaries.ts` | GET/PUT /salary-batches/* |
| `entitledPersonApi.*` | `api/retirement.ts` | GET/POST/PUT/DELETE /entitled-persons/* |
| `suspensionApi.*` | `api/retirement.ts` | GET/POST/PUT /suspensions/* |
| `pledgeApi.*` | `api/retirement.ts` | GET/POST/PUT/DELETE /document-pledges/* |
| `exemptionApi.*` | `api/retirement.ts` | GET/POST/PUT/DELETE /document-exemptions/* |
| `financialEntitlementApi.*` | `api/financial-entitlements.ts` | GET/POST/PUT /financial-entitlements/* |
| `settlementApi.*` | `api/settlements.ts` | GET /settlements/decisions/* |
| `ministryReferralApi.*` | `api/ministry.ts` | GET/POST /ministry-approvals/* |
| `documentReviewApi.*` | `api/documents.ts` | GET/PUT /missing-documents/* |
| `statusTransitionApi.*` | `api/retirement.ts` | GET /retirement-files/:id/status-transitions |
| `lookupApi.getDocumentTypes` | `api/documents.ts` | GET /lookups/document-types |
| `lookupApi.getBranches` | `api/investment.ts` | GET /lookups/branches |
| `lookupApi.getSocialStatuses` | `api/retirement.ts` | GET /lookups/social-statuses |
| `lookupApi.getHealthStatuses` | `api/retirement.ts` | GET /lookups/health-statuses |

### 4.2 تكرارات إضافية

| الملف A | الملف B | التكرار |
|---------|---------|---------|
| `notifications.ts::admin` (7 methods) | `adminNotifications.ts` (7 methods) | نفس الـ endpoints |
| `archive.ts::reviewDocument` | `documents.ts::review` | PUT /documents/:id/review |
| `archive.ts::getDocuments` | `documents.ts::getArchiveDocuments` | GET /documents |
| `archive.ts::getStats` | `documents.ts::getArchiveStats` | GET /documents/archive-stats |
| `archive.ts::getDocumentTypes` | `pension.ts::lookupApi.getDocumentTypes` | GET /document-types |
| `retirement.ts::getRecipients` | `salary-recipients.ts::getByFile` | GET /salary-recipients/by-file/:id |
| `retirement.ts::createRecipient` | `salary-recipients.ts::create` | POST /salary-recipients |
| `retirement.ts::endRecipient` | `salary-recipients.ts::end` | PUT /salary-recipients/:id/end |

### 4.3 خطة التنفيذ

**الخطوة 1: تحويل pension.ts للمرور عبر الملفات الصحيحة**

```typescript
// BEFORE (pension.ts)
export const entitledPersonApi = {
  getByFileId: (fileId: number) =>
    api.get<ApiResponse<EntitledPersonDetail[]>>(`/entitled-persons/by-file/${fileId}`),
  // ... 8 methods
};

// AFTER (pension.ts)
import { retirementApi } from "./retirement";
export const entitledPersonApi = {
  getByFileId: (fileId: number) => retirementApi.getEntitledPersons(fileId),
  getById: (id: number) => api.get<ApiResponse<...>>(`/entitled-persons/${id}`),
  create: (dto: CreateEntitledPersonDto) => retirementApi.createEntitledPerson(dto),
  update: (id: number, dto: UpdateEntitledPersonDto) => retirementApi.updateEntitledPerson(id, dto),
  stop: (id: number, dto: StopEntitledPersonDto) => retirementApi.stopEntitledPerson(id, dto),
  reactivate: (id: number, dto: ReactivateEntitledPersonDto) => retirementApi.reactivateEntitledPerson(id, dto),
  delete: (id: number) => retirementApi.deleteEntitledPerson(id),
  recordDeath: (id: number, dto: RecordDeathDto) =>
    api.post<ApiResponse<boolean>>(`/entitled-persons/${id}/death`, dto),
};
```

**الخطوة 2: دمج admin notifications**

```typescript
// BEFORE: two separate files
// notifications.ts::admin (7 methods)
// adminNotifications.ts (7 methods)

// AFTER: notifications.ts يحتوي على admin namespace
// adminNotifications.ts يُحذف ويمر عبر notificationsApi.admin
```

**الخطوة 3: تنظيف archive.ts**

```typescript
// BEFORE: archive.ts يعرّف endpoints مكررة
// AFTER: archive.ts يمر عبر documentsApi
import { documentsApi } from "./documents";
export const archiveApi = {
  getDocuments: (filters: ArchiveFilters) => documentsApi.getArchiveDocuments(filters),
  getStats: () => documentsApi.getArchiveStats(),
  reviewDocument: (id: number, dto: ReviewDocumentDto) => documentsApi.review(id, dto),
  // keep investment-specific methods that don't exist in documentsApi
  getDocumentReadiness: (entityType: string, entityId: number) =>
    api.get(`/investment/documents/readiness/${entityType}/${entityId}`),
};
```

**الخطوة 4: تنظيف retirement.ts** (إزالة التكرار مع salary-recipients.ts)

```typescript
// retirement.ts::getRecipients → delegation to salaryRecipientApi
import { salaryRecipientApi } from "./salary-recipients";
export const retirementApi = {
  getRecipients: (fileId: number) => salaryRecipientApi.getByFile(fileId),
  createRecipient: (dto: CreateRecipientDto) => salaryRecipientApi.create(dto),
  endRecipient: (id: number, dto: EndRecipientDto) => salaryRecipientApi.end(id, dto),
  // ... rest unchanged
};
```

### 4.4 ملخص التعديلات

| الملف | الإجراء | عدد السطور المتأثرة |
|--------|---------|---------------------|
| `api/pension.ts` | تحويل 14 دالة للمرور عبر ملفات أصلية | ~200 سطر |
| `api/adminNotifications.ts` | حذف + تحويل لـ notificationsApi.admin | ~40 سطر |
| `api/archive.ts` | تحويل 4 methods لـ documentsApi | ~30 سطر |
| `api/retirement.ts` | تحويل 3 methods لـ salaryRecipientApi | ~10 سطور |

---

## 5. المرحلة 4: دمج المكونات المكررة

### 5.1 DocumentProcessPage.tsx + DocumentProcessDialog.tsx

**المشكلة:** نسخة طبق الأصل (846 سطر كل واحد). الصفحة والـ Dialog يفعلان نفس الشيء.

**الحل:** الاحتفاظ بالـ Page فقط. تحويل Dialog لمرور عبر Page كـ embeddable component.

```
1. قراءة DocumentProcessDialog.tsx بالكامل
2. استخراج الـ shared logic (ImageSettings, DEFAULT_SETTINGS, canvas pool, loadImage)
3. إنشاء shared hook: ui/src/hooks/useDocumentProcessor.ts
4. DocumentProcessPage.tsx يستخدم الـ hook
5. DocumentProcessDialog.tsx يتحول لـ wrapper يستدعي Page في dialog
6. حذف التكرار من Dialog
7. tsc --noEmit
```

**الملفات المتأثرة:**
```
ui/src/hooks/useDocumentProcessor.ts          — NEW (shared logic)
ui/src/pages/retirement-files/DocumentProcessPage.tsx — تعديل
ui/src/components/domain/retirement/DocumentProcessDialog.tsx — تعديل
```

### 5.2 SuspensionsPage.tsx vs PensionSuspensionsPage.tsx

**المشكلة:** صفحتان لنفس المجال بـ UI مختلف.

**الحل:** الدمج في `SuspensionsPage.tsx` مع دعم كلا النمطين.

```
1. قراءة كلا الملفين
2. استخراج ما يميز كل واحد
   - SuspensionsPage: CreateSuspensionDialog + DataTable
   - PensionSuspensionsPage: CrudPageLayout + inline BaseFormDialog
3. دمج في SuspensionsPage مع tab/prop للتبديل
4. حذف PensionSuspensionsPage.tsx
5. تعديل App.tsx: route /pension/suspensions → SuspensionsPage
6. tsc --noEmit
```

**الملفات المتأثرة:**
```
ui/src/pages/suspensions/SuspensionsPage.tsx      — تعديل
ui/src/pages/pension/PensionSuspensionsPage.tsx    — حذف
ui/src/App.tsx                                     — تعديل route
```

### 5.3 FileReviewPage.tsx vs FileReviewDetailPage.tsx

**المشكلة:** صفحتان لمراجعة نفس الملف بـ entry points مختلفة.

**الحل:** الاحتفاظ بكل واحدة (مهمة مختلفة):

```
- FileReviewPage: section-based review (sidebar nav)
- FileReviewDetailPage: diff-based field-change review

→ ليسا مكررين真正اً — كل واحد UI مختلف لنفس العملية.
→ الإجراء: تسمية أوضح فقط (لا دمج)
```

**الإجراء:** لا يتطلب تعديل. تسمية مختلفة ل_angles مختلفة من same review process.

---

## 6. المرحلة 5: تنظيف Raw API Calls

### 6.1 قائمة الملفات المطلوب تعديلها (~35 site)

**المجموعة A: تعديلات بسيطة (use API module بدلاً من raw api.get)**

| # | الملف | السطر | raw call | البديل |
|---|-------|-------|----------|--------|
| 1 | `sections/FinancialSection.tsx` | 206 | `api.get(/financial-entitlements/by-retiree/${id})` | `retirementApi.getFinancialEntitlements(id)` |
| 2 | `sections/FinancialSection.tsx` | 215 | `api.get(/payment-books/by-file/${id})` | `retirementApi.getPaymentBooks(id)` |
| 3 | `sections/FinancialSection.tsx` | 236 | `api.get(/salary-recipients/by-file/${id})` | `retirementApi.getRecipients(id)` |
| 4 | `sections/FinancialSection.tsx` | 255 | `api.put(/payment-books/${id})` | `retirementApi.updatePaymentBook(id, dto)` |
| 5 | `sections/FinancialSection.tsx` | 270 | `api.put(/salary-recipients/${id}/end)` | `retirementApi.endRecipient(id, dto)` |
| 6 | `sections/FinancialSection.tsx` | 280-321 | `api.get` لـ agents/guardians/entitled | `retirementApi.getAgents/getGuardians/getEntitledPersons` |
| 7 | `sections/FinancialSection.tsx` | 335 | `api.post(/salary-recipients)` | `retirementApi.createRecipient(dto)` |
| 8 | `sections/PeopleSection.tsx` | 204-208 | `api.put` لـ entitled/agents/guardians | `retirementApi.updateEntitledPerson/updateAgent/updateGuardian` |
| 9 | `sections/PeopleSection.tsx` | 239 | `api.post(/entitled-persons/${id}/death)` | `entitledPersonApi.recordDeath(id, dto)` |
| 10 | `sections/HistorySection.tsx` | 17 | `api.get(/retirement-files/${id}/change-log)` | `retirementApi.getChangeLog(id)` |
| 11 | `ReviewPanel.tsx` | 22-52 | `api.post` لـ review approve/reject | `retirementApi.approveReview/approveReviewItem/rejectReview/rejectReviewItem` |
| 12 | `UpdateStatusDialog.tsx` | 74 | `api.put(/retirement-files/${id}/status)` | `retirementApi.updateFileStatus(id, dto)` |
| 13 | `CreateFileWizard.tsx` | 268-270 | `api.put/api.post لـ /retirement-files` | `retirementApi.createFile/updateFile` |
| 14 | `EntitledPersonFormDialog.tsx` | 247-270 | `api.post/api.put لـ /entitled-persons` | `entitledPersonApi.create/update` |
| 15 | `AgentDialog.tsx` | 93-95 | `api.post/api.put لـ /agents` | `retirementApi.createAgent/updateAgent` |
| 16 | `GuardianDialog.tsx` | 85-87 | `api.post/api.put لـ /guardians` | `retirementApi.createGuardian/updateGuardian` |
| 17 | `CreateSuspensionDialog.tsx` | 67-82 | `api.get api.post` | `retirementApi.getFiles/createSuspension` |
| 18 | `HistorySection.tsx` | 17 | `api.get(/retirement-files/${id}/change-log)` | `retirementApi.getChangeLog(id)` |
| 19 | `InvestmentDocumentsManager.tsx` | 95 | `api.get(/documents/by-${entityType}/${entityId})` | Investment-specific — يبقى (لا يوجد بديل) |

**المجموعة B: صفحات طباعة (print pages)**

| # | الملف | السطر | raw call | البديل |
|---|-------|-------|----------|--------|
| 20 | `RetirementFilePrintPage.tsx` | 23-87 | 6 raw calls | `retirementApi.getFile/getRetiree/getEntitledPersons/getAgents/getGuardians/getPaymentBooks` |
| 21 | `print.utils.ts` | 75-79 | 5 raw calls | `retirementApi` methods |
| 22 | `MissingDocumentsPrintPage.tsx` | 26 | `api.get(/retirement-files/${id})` | `retirementApi.getFile(id)` |

**المجموعة C: Admin/Settings (no centralized API)**

| # | الملف | السطر | raw call | الإجراء |
|---|-------|-------|----------|---------|
| 23 | `UsersDirectoryPage.tsx` | 123,132 | `api.delete/api.post` | إنشاء `api/admin/users.ts` أو إضافة لـ `api/admin.ts` |
| 24 | `UserDialog.tsx` | 107-108 | `api.put/api.post` | نفس أعلاه |
| 25 | `UserPermissionsDialog.tsx` | 85 | `api.put` | نفس أعلاه |
| 26 | `RolePermissionsDialog.tsx` | 69 | `api.put` | إنشاء `api/admin/roles.ts` |
| 27 | `RankFormDialog.tsx` | 56-57 | `api.put/api.post` | إنشاء `api/admin/ranks.ts` |
| 28 | `DocumentTypeDialog.tsx` | 87-88 | `api.put/api.post` | إنشاء `api/admin/documentTypes.ts` |
| 29 | `BranchDialog.tsx` | 62-63 | `api.put/api.post` | إنشاء `api/admin/branches.ts` |
| 30 | `SystemSettingsPage.tsx` | 57-81 | `api.put/api.post/api.delete` | إنشاء `api/admin/settings.ts` |
| 31 | `RanksListPage.tsx` | 35 | `api.delete` | يمر عبر ranks API |
| 32 | `DocumentTypesPage.tsx` | 57 | `api.delete` | يمر عبر documentTypes API |
| 33 | `BranchesListPage.tsx` | 40,49 | `api.delete/api.patch` | يمر عبر branches API |
| 34 | `MinistryBatchesPage.tsx` | 55 | `api.delete` | `ministryApi.deleteBatch` |
| 35 | `ComplianceDashboardPage.tsx` | 68-86 | `api.get/api.post` | إنشاء `api/compliance.ts` |
| 36 | `SocialResearchPage.tsx` | 121 | `api.put` | إنشاء `api/socialResearch.ts` |
| 37 | `SettlementBatchDetailsPage.tsx` | 119 | `api.get` | `settlementApi.exportFailureReport` |
| 38 | `CreateSettlementDecisionDialog.tsx` | 63 | `api.post` | إضافة لـ `settlements.ts` |
| 39 | `CreateForceBatchDialog.tsx` | 19 | `api.post` | `ministryApi.createBatch` |

### 6.2 خطة التنفيذ — 4 ملفات Admin API جديدة

**ملف جديد: `api/admin.ts`**
```typescript
import api from "./axios";
import type { ApiResponse } from "@/types/api";

export const adminUsersApi = {
  getAll: (filters?: UserFilters) => api.get<ApiResponse<PagedResult<UserDto>>>('/users', { params: filters }),
  getById: (id: number) => api.get<ApiResponse<UserDto>>(`/users/${id}`),
  create: (dto: CreateUserDto) => api.post<ApiResponse<UserDto>>('/users', dto),
  update: (id: number, dto: UpdateUserDto) => api.put<ApiResponse<boolean>>(`/users/${id}`, dto),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/users/${id}`),
  resetPassword: (id: number) => api.post<ApiResponse<boolean>>(`/users/${id}/reset-password`),
  updatePermissions: (id: number, dto: UpdatePermissionsDto) =>
    api.put<ApiResponse<boolean>>(`/users/${id}/permissions`, dto),
};

export const adminRolesApi = {
  getAll: () => api.get<ApiResponse<RoleDto[]>>('/roles'),
  updatePermissions: (id: number, dto: UpdateRolePermissionsDto) =>
    api.put<ApiResponse<boolean>>(`/roles/${id}/permissions`, dto),
};

export const adminSettingsApi = {
  getAll: () => api.get<ApiResponse<SettingDto[]>>('/admin/settings'),
  create: (dto: CreateSettingDto) => api.post<ApiResponse<SettingDto>>('/admin/settings', dto),
  update: (id: number, dto: UpdateSettingDto) => api.put<ApiResponse<boolean>>(`/admin/settings/${id}`, dto),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/admin/settings/${id}`),
};

export const adminRanksApi = {
  getAll: () => api.get<ApiResponse<RankDto[]>>('/ranks'),
  create: (dto: CreateRankDto) => api.post<ApiResponse<RankDto>>('/ranks', dto),
  update: (id: number, dto: UpdateRankDto) => api.put<ApiResponse<boolean>>(`/ranks/${id}`, dto),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/ranks/${id}`),
};

export const adminBranchesApi = {
  getAll: () => api.get<ApiResponse<BranchDto[]>>('/branches'),
  create: (dto: CreateBranchDto) => api.post<ApiResponse<BranchDto>>('/branches', dto),
  update: (id: number, dto: UpdateBranchDto) => api.put<ApiResponse<boolean>>(`/branches/${id}`, dto),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/branches/${id}`),
  toggleActive: (id: number) => api.patch<ApiResponse<boolean>>(`/branches/${id}/toggle-active`),
};

export const adminDocumentTypesApi = {
  getAll: () => api.get<ApiResponse<DocumentTypeDto[]>>('/document-types'),
  create: (dto: CreateDocumentTypeDto) => api.post<ApiResponse<DocumentTypeDto>>('/document-types', dto),
  update: (id: number, dto: UpdateDocumentTypeDto) => api.put<ApiResponse<boolean>>(`/document-types/${id}`, dto),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/document-types/${id}`),
};

export const complianceApi = {
  getDashboard: () => api.get<ApiResponse<ComplianceDashboard>>('/compliance/dashboard'),
  getAuditVerifyChain: (filters: AuditFilters) =>
    api.get<ApiResponse<AuditChainResult[]>>('/compliance/audit/verify-chain', { params: filters }),
  resolveEvent: (id: number, dto: ResolveEventDto) =>
    api.post<ApiResponse<boolean>>(`/compliance/risk-events/${id}/resolve`, dto),
};
```

**ملف جديد: `api/socialResearch.ts`**
```typescript
import api from "./axios";
import type { ApiResponse } from "@/types/api";

export const socialResearchApi = {
  update: (id: number, dto: UpdateSocialResearchDto) =>
    api.put<ApiResponse<boolean>>(`/social-research/${id}`, dto),
};
```

### 6.3 ملخص التعديلات

| الملف | الإجراء | عدد raw calls |
|--------|---------|---------------|
| `sections/FinancialSection.tsx` | تحويل 7 raw calls لـ API modules | 7 |
| `sections/PeopleSection.tsx` | تحويل 3 raw calls لـ API modules | 3 |
| `sections/HistorySection.tsx` | تحويل 1 raw call | 1 |
| `ReviewPanel.tsx` | تحويل 4 raw calls | 4 |
| `UpdateStatusDialog.tsx` | تحويل 1 raw call | 1 |
| `CreateFileWizard.tsx` | تحويل 2 raw calls | 2 |
| `EntitledPersonFormDialog.tsx` | تحويل 2 raw calls | 2 |
| `AgentDialog.tsx` | تحويل 2 raw calls | 2 |
| `GuardianDialog.tsx` | تحويل 2 raw calls | 2 |
| `CreateSuspensionDialog.tsx` | تحويل 2 raw calls | 2 |
| `RetirementFilePrintPage.tsx` | تحويل 6 raw calls | 6 |
| `print.utils.ts` | تحويل 5 raw calls | 5 |
| `MissingDocumentsPrintPage.tsx` | تحويل 1 raw call | 1 |
| `UsersDirectoryPage.tsx` | تحويل 2 raw calls | 2 |
| `UserDialog.tsx` | تحويل 2 raw calls | 2 |
| `UserPermissionsDialog.tsx` | تحويل 1 raw call | 1 |
| `RolePermissionsDialog.tsx` | تحويل 1 raw call | 1 |
| `RankFormDialog.tsx` | تحويل 2 raw calls | 2 |
| `DocumentTypeDialog.tsx` | تحويل 2 raw calls | 2 |
| `BranchDialog.tsx` | تحويل 2 raw calls | 2 |
| `SystemSettingsPage.tsx` | تحويل 3 raw calls | 3 |
| `RanksListPage.tsx` | تحويل 1 raw call | 1 |
| `DocumentTypesPage.tsx` | تحويل 1 raw call | 1 |
| `BranchesListPage.tsx` | تحويل 2 raw calls | 2 |
| `MinistryBatchesPage.tsx` | تحويل 1 raw call | 1 |
| `ComplianceDashboardPage.tsx` | تحويل 3 raw calls | 3 |
| `SocialResearchPage.tsx` | تحويل 1 raw call | 1 |
| `SettlementBatchDetailsPage.tsx` | تحويل 1 raw call | 1 |
| `CreateSettlementDecisionDialog.tsx` | تحويل 1 raw call | 1 |
| `CreateForceBatchDialog.tsx` | تحويل 1 raw call | 1 |
| **NEW: `api/admin.ts`** | ملف جديد — 6 API namespaces | — |
| **NEW: `api/socialResearch.ts`** | ملف جديد — 1 API namespace | — |

**المجموع: 31 ملف معدّل + 2 ملف جديد**

---

## 7. المرحلة 6: تنظيف Hooks المتراكبة

### 7.1 قائمة التكرار

| Hook A | Hook B | التكرار |
|--------|--------|---------|
| `useDocumentsQuery.ts` | `useDocumentsByFileQuery.ts` + `useMissingDocumentsQuery.ts` | يستوردان نفس البيانات بطرق مختلفة |
| `useFileEntitiesQuery.ts` | `useEntitledPersonsQuery.ts` + `useAgentsQuery.ts` + `useGuardiansQuery.ts` | useFileEntitiesQuery يجمعها الثلاثة |
| `useDebounce.ts` | `useDebouncedSearch.ts` | useDebouncedSearch يعيد تدوين debounce logic |
| `hooks/useDocumentsQuery.ts` | `hooks/queries/useDocumentsByFileQuery.ts` | نفس الـ queryKey: ["documents", fileId] |

### 7.2 خطة التنفيذ

**المسألة 1: useDocumentsQuery vs useDocumentsByFileQuery**

```
hooks/useDocumentsQuery.ts — يستورد documentsApi ويجلب docs + missing + autoDetected + types
hooks/queries/useDocumentsByFileQuery.ts — يستورد documentsApi ويجلب docs فقط

→ الحل: useDocumentsByFileQuery يصبح alias لـ useDocumentsQuery (أو يُحذف ويستبدل)
→但如果 المكونات تحتاج docs فقط بدون missing، تبقى useDocumentsByFileQuery
→ الأفضل: حذف useDocumentsQuery واستبدالها باستخدام useDocumentsByFileQuery + useMissingDocumentsQuery
```

**المسألة 2: useFileEntitiesQuery vs useEntitledPersonsQuery/Agents/Guardians**

```
useFileEntitiesQuery — يستورد retirementApi ويجلب entitled + agents + guardians معاً
useEntitledPersonsQuery — يستورد api directly ويجلب entitled فقط
useAgentsQuery — يستورد api directly ويجلب agents فقط
useGuardiansQuery — يستورد api directly ويجلب guardians فقط

→ الحل: useFileEntitiesQuery يبقى (يجمعها)
→ useEntitledPersonsQuery/Agents/Guardians يمرّون عبر retirementApi بدلاً من raw api
→ لا يُحذفون لأن المكونات تحتاجهم بشكل منفصل أحياناً
```

**المسألة 3: useDebounce vs useDebouncedSearch**

```
useDebounce<T> — generic debounce (value → debounced value)
useDebouncedSearch — يحتوي debounce logic داخلياً (لا يستخدم useDebounce)

→ الحل: useDebouncedSearch يستخدم useDebounce بدلاً من إعادة تدوين المنطق
→ تعديل useDebouncedSearch: import + استخدام useDebounce
```

### 7.3 ملخص التعديلات

| الملف | الإجراء |
|--------|---------|
| `hooks/queries/useEntitledPersonsQuery.ts` | تحويل raw api.get لـ retirementApi.getEntitledPersons |
| `hooks/queries/useAgentsQuery.ts` | تحويل raw api.get لـ retirementApi.getAgents |
| `hooks/queries/useGuardiansQuery.ts` | تحويل raw api.get لـ retirementApi.getGuardians |
| `hooks/queries/useFileDetailsQuery.ts` | تحويل raw api.get لـ retirementApi.getFile + retireeApi.getById |
| `hooks/queries/useReviewItemsQuery.ts` | تحويل raw api.get لـ retirementApi.getReviewDiff |
| `hooks/queries/usePersonDocumentsQuery.ts` | تحويل raw api.get لـ pledgeApi.getByFileId |
| `hooks/useDebouncedSearch.ts` | استخدام useDebounce بدلاً من إعادة تدوين المنطق |

---

## 8. ملخص جميع الملفات

### Phase 1 — حذف ميت (12 ملف)
```
DELETE: ui/src/components/domain/retirement/UpdateStatusDialog.tsx
DELETE: ui/src/components/domain/retirement/CreateRetirementFileDialog.tsx
DELETE: ui/src/components/domain/retirement/SuspensionDialog.tsx
DELETE: ui/src/components/domain/retirement/FinancialDialog.tsx
DELETE: ui/src/components/ui/QuickStats.tsx
DELETE: ui/src/components/ui/CardToolbar.tsx
DELETE: ui/src/pages/inventory/SimpleStocktakePage.tsx
DELETE: ui/src/pages/inventory/AssetStocktakePage.tsx
DELETE: ui/src/pages/finance/FinanceDashboardPage.tsx
DELETE: ui/src/types/api/common.ts
EDIT: ui/src/components/domain/retirement/index.ts — حذف 4 exports
EDIT: ui/src/components/ui/index.ts — حذف 2 exports
```

### Phase 2 — توحيد Types (14 ملف)
```
EDIT: ui/src/api/documents.ts — حذف ApiResponse تعريف محلي + إضافة import
EDIT: ui/src/api/salaries.ts — حذف ApiResponse تعريف محلي + إضافة import
EDIT: ui/src/api/notifications.ts — حذف ApiResponse تعريف محلي + إضافة import
EDIT: ui/src/api/ministry.ts — حذف ApiResponse + PagedResult + إضافة import
EDIT: ui/src/api/hr.ts — حذف ApiResponse + PagedResult + إضافة import
EDIT: ui/src/api/investment.ts — حذف ApiResponse تعريف محلي + إضافة import
EDIT: ui/src/api/fixedAssets.ts — حذف ApiResponse تعريف محلي + إضافة import
EDIT: ui/src/api/reports.ts — حذف ApiResponse + PagedResult + إضافة import
EDIT: ui/src/api/vouchers.ts — حذف PagedResult + ApiResponse + إضافة import
EDIT: ui/src/api/discrepancy.ts — حذف PagedResult + ApiResponse + إضافة import
EDIT: ui/src/api/archive.ts — حذف ApiResponse + PagedResult + إضافة import
EDIT: ui/src/api/correspondence.ts — حذف ApiResponse + إضافة import
EDIT: ui/src/api/retirees.ts — حذف PagedResult تعريف محلي + إضافة import
EDIT: ui/src/api/settlements.ts — حذف PagedResult تعريف محلي + إضافة import
```

### Phase 3 — إصلاح API Modules (4 ملفات)
```
EDIT: ui/src/api/pension.ts — تحويل 14 دالة لتمر عبر ملفات أصلية (~200 سطر)
DELETE: ui/src/api/adminNotifications.ts — تحويل لـ notificationsApi.admin
EDIT: ui/src/api/archive.ts — تحويل 4 methods لـ documentsApi
EDIT: ui/src/api/retirement.ts — تحويل 3 methods لـ salaryRecipientApi
```

### Phase 4 — دمج مكونات (5 ملفات)
```
NEW:  ui/src/hooks/useDocumentProcessor.ts — shared logic
EDIT: ui/src/pages/retirement-files/DocumentProcessPage.tsx — استخدام hook
EDIT: ui/src/components/domain/retirement/DocumentProcessDialog.tsx — wrapper
EDIT: ui/src/pages/suspensions/SuspensionsPage.tsx — دمج
DELETE: ui/src/pages/pension/PensionSuspensionsPage.tsx
EDIT: ui/src/App.tsx — تعديل route
```

### Phase 5 — تنظيف Raw API Calls (33 ملف + 2 جديد)
```
EDIT: 31 ملف components/pages — تحويل ~50 raw call لتمر عبر API modules
NEW:  ui/src/api/admin.ts — 6 API namespaces
NEW:  ui/src/api/socialResearch.ts — 1 API namespace
```

### Phase 6 — تنظيف Hooks (7 ملفات)
```
EDIT: ui/src/hooks/queries/useEntitledPersonsQuery.ts — استخدام retirementApi
EDIT: ui/src/hooks/queries/useAgentsQuery.ts — استخدام retirementApi
EDIT: ui/src/hooks/queries/useGuardiansQuery.ts — استخدام retirementApi
EDIT: ui/src/hooks/queries/useFileDetailsQuery.ts — استخدام retirementApi + retireeApi
EDIT: ui/src/hooks/queries/useReviewItemsQuery.ts — استخدام retirementApi
EDIT: ui/src/hooks/queries/usePersonDocumentsQuery.ts — استخدام pledgeApi
EDIT: ui/src/hooks/useDebouncedSearch.ts — استخدام useDebounce
```

### Backend (3 ملفات)
```
EDIT: PensionMS.API/Controllers/DiscrepancyController.cs — دمجatedRoute المكرر
EDIT: PensionMS.API/Controllers/FileStatusesController.cs — تسمية clearer أو حذف
EDIT: PensionMS.API/Controllers/RetirementFilesController.cs — تسمية أوضح لـ report
```

---

## 9. خطة الاختبار

### اختبارات كل مرحلة

**Phase 1 (حذف ميت):**
```
npx tsc --noEmit          — تأكد لا أحد يستورد الملفات المحذوفة
grep -r "UpdateStatusDialog\|CreateRetirementFileDialog\|SuspensionDialog\|FinancialDialog" src/
grep -r "QuickStats\|CardToolbar" src/
grep -r "SimpleStocktakePage\|AssetStocktakePage\|FinanceDashboardPage" src/
```

**Phase 2 (توحيد Types):**
```
npx tsc --noEmit          — تأكد لا آخطاء نوعية
grep -rn "interface ApiResponse" src/api/  — يجب أن يُرجع 0
grep -rn "interface PagedResult" src/api/  — يجب أن يُرجع 0
grep -rn "from.*types/api/common" src/     — يجب أن يُرجع 0
```

**Phase 3 (إصلاح API):**
```
npx tsc --noEmit
grep -rn "pension\.ts.*entitledPerson\|pension\.ts.*suspension" src/ — لا مزيد من التكرار
```

**Phase 4 (دمج مكونات):**
```
npx tsc --noEmit
# اختبار يدوي: فتح كل route تأكد يعمل
```

**Phase 5 (تنظيف Raw API):**
```
npx tsc --noEmit
# البحث عن أي raw api.getremaining
grep -rn "api\.\(get\|post\|put\|delete\).*\`/" src/components/ src/pages/ src/hooks/ | grep -v "/api/"
# يجب أن يُرجع 0 (أو أقلها 10 — لا يوجد بديل مركزي)
```

**Phase 6 (تنظيف Hooks):**
```
npx tsc --noEmit
```

### اختبار شامل نهائي
```
npx tsc --noEmit
# تشغيل الـ dev server و فتح أبرز الصفحات:
# - /dashboard
# - /files → تفاصيل ملف → وثائق → مراجعة
# - /pension/entitled-persons
# - /pension/suspensions
# - /archive
# - /accounting
# - /investment
```

---

## 10. خطة التنفيذ المقترحة

### الجدول الزمني

| اليوم | المرحلة | عدد الملفات | المدة |
|-------|---------|-------------|-------|
| اليوم 1 | Phase 1: حذف ميت | 12 ملف | 1-2 ساعة |
| اليوم 1 | Phase 2: توحيد Types | 14 ملف | 2-3 ساعات |
| اليوم 2 | Phase 3: إصلاح API Modules | 4 ملفات | 3-4 ساعات |
| اليوم 3 | Phase 4: دمج مكونات | 5 ملفات | 2-3 ساعات |
| اليوم 4 | Phase 5: تنظيف Raw API | 33 ملف + 2 جديد | 4-5 ساعات |
| اليوم 5 | Phase 6: تنظيف Hooks | 7 ملفات | 1-2 ساعة |
| اليوم 5 | Backend adjustments | 3 ملفات | 1 ساعة |
| اليوم 5 | اختبار شامل | — | 2-3 ساعات |
| **المجموع** | | **~70 ملف** | **16-23 ساعة** |

### الترتيب المطلوب (dependencies)

```
Phase 1 (حذف ميت) ← لا يوجد تبعيات
Phase 2 (توحيد Types) ← لا يوجد تبعيات
Phase 3 (إصلاح API) ← يفضل做完 Phase 2 أولاً
Phase 4 (دمج مكونات) ← يفضل做完 Phase 3 أولاً
Phase 5 (تنظيم Raw API) ← يفضل做完 Phase 3 أولاً
Phase 6 (تنظيم Hooks) ← يفضل做完 Phase 3 و Phase 5 أولاً
Backend ← مستقل عن Frontend
```

### مبدأ التنفيذ

1. **كل مرحلة**: تعديل → `tsc --noEmit` → تأكد → commit
2. **كل ملف**: قراءة → تعديل أصغر diff → re-read → تأكد
3. **لا مفاجآت**: لا تدمج مكونات除非 마 Ensure Tests Pass
4. **Fallback**: إذا تعقد الدمج → تخطي và عودة لاحقاً
