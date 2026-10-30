# Security & Permissions

## Pension Management System — Authentication, Authorization & Audit

---

## 1. Authentication

| Property | Value |
|----------|-------|
| **Type** | JWT Bearer |
| **Algorithm** | HMAC-SHA256 |
| **Expiration** | 60 minutes |
| **Storage** | Browser localStorage |
| **Refresh** | Supported via refresh token |
| **Password Hash** | BCrypt (12 rounds) |

### Login Flow

```
1. User submits username + password
2. API validates credentials (BCrypt hash)
3. API generates JWT with user claims
4. API returns JWT + refresh token
5. Frontend stores in localStorage
6. Frontend attaches JWT to all requests
```

### Token Contents

```json
{
  "sub": "1",
<<<<<<< HEAD
  "role": "Admin",
  "BranchId": 1,
=======
  "username": "admin",
  "role": "Admin",
  "branchId": 1,
  "permissions": ["*"],
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  "exp": 1691238167
}
```

### Password Policies

| Rule | Requirement |
|------|-------------|
| Minimum length | 8 characters |
| Uppercase | At least 1 |
| Lowercase | At least 1 |
| Number | At least 1 |
| Special char | At least 1 |

---

## 2. Authorization

### Roles

| Role | Access Level |
|------|-------------|
| `Admin` | Full access (national) |
| `RegionalManager` | Regional branch access |
| `BranchManager` | Branch access |
| `Reviewer` | File review permissions |
| `Operator` | Data entry permissions |
| `Viewer` | Read-only access |
| `FinanceOfficer` | Financial operations |
| `HRManager` | HR module access |

### 56 Granular Permissions

| Module | Permissions |
|--------|-------------|
| **Files** | `Files.View`, `Files.Create`, `Files.Edit`, `Files.Delete`, `Files.Review`, `Files.Approve` |
| **Retirees** | `Retirees.View`, `Retirees.Create`, `Retirees.Edit`, `Retirees.Delete` |
| **Entitled Persons** | `EntitledPersons.View`, `EntitledPersons.Create`, `EntitledPersons.Edit`, `EntitledPersons.Delete` |
| **Documents** | `Documents.View`, `Documents.Upload`, `Documents.Delete`, `Documents.Validate` |
| **Payments** | `Payments.View`, `Payments.Create`, `Payments.Approve`, `Payments.Execute` |
| **Settlements** | `Settlements.View`, `Settlements.Create`, `Settlements.Approve`, `Settlements.Execute` |
| **Suspensions** | `Suspensions.View`, `Suspensions.Create`, `Suspensions.End` |
| **Reports** | `Reports.View`, `Reports.Export` |
| **Admin** | `Admin.Users`, `Admin.Roles`, `Admin.Audit`, `Admin.Settings`, `Admin.Branches` |
| **Investment** | `Investment.View`, `Investment.Create`, `Investment.Edit`, `Investment.Delete` |
| **HR** | `HR.View`, `HR.Create`, `HR.Edit`, `HR.Delete` |

<<<<<<< HEAD
### قرار الوصول إلى بيانات التقاعد — 2026-10-22

المصدر المعتمد هو `IScopedPermissionResolver.GetAllowedScopeAsync(userId, permissionCode)`.
يتحقق من المستخدم الفعال غير المحذوف، ثم تعيينات `UserRoleAssignments` النشطة التي يمنح دورها **الصلاحية المطلوبة نفسها**.
النتيجة `PermissionScope`: منع، فروع محددة، أو جميع الفروع. تعيين الفرع يحدد فرعاً؛ تعيين الإقليم يشمل أبناءه؛ التعيين المركزي الصريح يترك `BranchId` و`RegionId` فارغين.
رقم الفرع 1 واسم الدور و`User.RoleID` وادعاءات JWT لا تمنح مركزية. `Users.BranchID` فرع إداري، وليس بديلاً عن تعيين الدور ونطاقه.

السحب الفردي يمنع الصلاحية ولو منحها الدور. المنحة الفردية لا توسع نطاقاً تمنحه الأدوار بالفعل.
إذا لم يمنح أي تعيين العملية، فالمنحة الفردية القديمة تتيح فرع الحساب فقط، بشرط أن يغطيه تعيين وظيفي نشط وصريح؛ غياب هذا الفرع أو التعيين يعني المنع. المنحة الفردية لا تمنح مركزية.

`PermissionHandler` يتحقق من صلاحية العملية. الخدمات تستخدم `IAccessiblePensionRepository` للحصول على `IPensionDataQuery` ثابت للنطاق، ثم تقيد الاستعلام بفرع السجل الفعلي قبل العد والتجميع والتصفح. `ForCurrentOperationAsync` يقرأ السياسات من بيانات endpoint على الخادم، ويأخذ تقاطع نطاقاتها؛ غيابها يعني المنع. لا يرسل العميل رمز الصلاحية.
يمكن للمستخدم نفسه العرض مركزياً والتعديل في فرع واحد فقط. فلتر الفرع يضيّق نطاق العملية ولا يوسعه.

وفق قرار المستخدم، يبقى سجل المتقاعدين التعريفي عاماً لحامل `Retiree.View`، مع تقييد الملفات والوثائق والاستحقاقات والتسويات التابعة بالفروع. تعديل بيانات متقاعد مشترك يتطلب تغطية جميع ملفاته غير المحذوفة.
تحميل الملفات عبر `/uploads` و`/api/files` يتحقق من سجل الوثيقة ومالكها؛ المصادقة وحدها لا تكفي. الصورة التعريفية القديمة للمتقاعد تتبع السجل العام.

العمليات الإدارية `System.*` تتطلب نطاقاً مركزياً صريحاً. واجهات التسوية المشتركة غير المجزأة، وإنشاء كشوف المعاشات وتوليد بنودها واستيراد النتائج، تتطلب كذلك نطاقاً مركزياً للعملية. التقارير تقيد بنود الدفعات ومجاميعها بالفروع المرئية؛ تعديل دفعة مشتركة يتطلب تغطية كل بنودها.
وثيقة ذات عدة مراجع ملكية تتطلب السماح بكل المراجع. المالك الذي لا توجد له علاقة فرعية معرّفة لا يُسمح به للمستخدم المقيد بالفروع.

تُدار التعيينات عبر `PUT /api/Users/{id}/role-assignments` بصلاحية `System.ManageUsers` المركزية، مع سجل تدقيق. تغيير فرع الحساب لا ينقل نطاقات الأدوار تلقائياً.
نقل الملف إجراء مستقل `File.Transfer` يتحقق من فرعي المصدر والهدف، و`RowVersion`، والمراجعة، ويسجل السبب. لا يغيّر التعديل العادي فرع الملف.

حُذفت تطبيقات النطاق القديمة: `PensionFileAccess` و`BranchScopeProvider` و`ReportingQueries`. فحص الهوية فقط في `ICurrentUserContext`؛ معالجة التعهدات المنتهية في مهمة مستقلة لا تتجاوز فحص طلبات HTTP.

قبل نشر هذا التغيير، راجع ثم طبّق `scripts/branch-access-rollout.sql` على قاعدة التطبيق المقصودة.
الوضع الافتراضي معاينة `@Apply = 0`. حدد الحسابات المركزية المعتمدة في `@CentralUsers` صراحةً، ثم استخدم `@Apply = 1` بعد المراجعة والنسخ الاحتياطي. الحسابات المحلية التي لم يسبق لها تعيين فقط يمكن تهيئتها من الدور والفرع القديمين؛ التعيينات المسحوبة لا تُعاد. لا تُمنح `File.Transfer` لأي دور تلقائياً. لم يُنفّذ السكربت على قاعدة التطبيق أثناء التطوير.

---

## 3. صلاحيات تنفيذ الإجراءات

بقرار معتمد بتاريخ 2026-10-20، أُلغيت قيود فصل المهام. يستطيع صاحب صلاحية الإجراء تنفيذه حتى لو أنشأ السجل أو عدّل بياناته أو رفع وثائقه أو أنشأ كشف المعاشات نفسه. يشمل ذلك اعتماد مراجعة الملف كاملة أو بند منها واعتماد كشف المعاشات.

تُطبَّق صلاحيات الإجراءات بواسطة سياسات التفويض في API؛ مثل `File.Review.Approve` و`SalaryBatch.Approve` و`File.ChangeStatus.Approve`. تبقى حدود الفرع وشروط انتقال الحالات وصحة البيانات وإيقافات المعاش وسجل التدقيق نافذة. إلغاء فصل المهام لا يمنح المستخدم صلاحية جديدة، ولا يتيح صرف ملف معلق أو معاش موقوف.
=======
### Branch-Scoped Permissions

| Tier | Scope | Example Roles |
|------|-------|---------------|
| **Branch** | Own branch only | Operator, Reviewer |
| **Regional** | Multiple branches | RegionalManager |
| **National** | All branches | Admin |

---

## 3. Separation of Duties (SoD)

Three guards enforce dual-control:

| Guard | Rule |
|-------|------|
| `SettlementSoDGuard` | Settlement creator cannot approve it |
| `SalaryBatchSoDGuard` | Batch creator cannot approve it |
| `EntitlementSoDGuard` | Calculator cannot disburse |

```
Settlement:  Creator (Operator) → Approver (Reviewer) → Executor (Finance)
Salary Batch: Creator (Finance) → Approver (Admin) → Executor (Finance)
```
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

---

## 4. Data Security

### Encryption

| Data | Method |
|------|--------|
| Passwords | BCrypt (12 rounds) |
| Sensitive fields | AES-256 |
| JWT tokens | HMAC-SHA256 |

### Sensitive Fields

| Entity | Encrypted Fields |
|--------|-----------------|
| Retiree | National ID, Bank Account |
| Entitled Person | National ID |
| User | Password |

---

## 5. Audit Trail

### Records Created

| Record | Purpose |
|--------|---------|
| `FileStatus` | Status change log |
| `FileSnapshot` | Immutable state snapshot (hash chain) |
| `AuditLog` | General audit entry |

### AuditLog Fields

| Field | Description |
|-------|-------------|
| `Action` | CREATE, UPDATE, DELETE |
| `TableName` | Affected table |
| `RecordId` | Affected record |
| `UserId` | Actor |
| `Timestamp` | UTC timestamp |
| `OldValues` | Previous state (JSON) |
| `NewValues` | New state (JSON) |
| `IpAddress` | Client IP |

### Hash Chain Integrity

```csharp
var hash = SHA256.HashData(Encoding.UTF8.GetBytes(snapshotData + previousHash));
```

Verify: `GET /api/compliance/audit/verify-chain`

---

## 6. Compliance & Risk

### Compliance Monitoring

| Check | Threshold | Action |
|-------|-----------|--------|
| Files pending review | > 30 days | Risk event |
| Missing documents | Approaching deadline | Notification |
| Expired pledges | Past expiry | Auto-release |
| SoD violations | Attempted | Blocked + logged |
| Suspensions | > 12 months | Review required |

### Risk Scoring

| Factor | Weight |
|--------|--------|
| File age | 25% |
| Missing docs | 25% |
| Pending review duration | 25% |
| Suspension history | 15% |
| Financial anomalies | 10% |

---

## 7. API Security

- **CORS**: Restricted to known origins
- **Validation**: FluentValidation on all inputs
- **SQL Injection**: EF Core parameterized queries
- **XSS**: Output encoding
- **Rate Limiting**: 100 req/min per user, 5 login attempts/min
- **HTTPS**: Recommended for production

---

## 8. Frontend Security

- JWT auto-attached via Axios interceptor
- Auto-logout on 401 response
- Route protection via `ProtectedRoute` component
- Permission-based route access via `RequirePermission`
- Input validation before submission
- File type validation on upload

---

## 9. Security Checklist

### Deployment
- [ ] Change default admin password
- [ ] Use strong JWT secret (32+ chars)
- [ ] Use strong encryption key (32 chars)
- [ ] Enable HTTPS
- [ ] Restrict CORS to production domain
- [ ] Remove Swagger from production

### Database
- [ ] Use dedicated SQL account (not sa)
- [ ] Restrict network access
- [ ] Enable encryption at rest
- [ ] Regular backups

### Monitoring
- [ ] Enable request logging
- [ ] Monitor failed logins
- [ ] Review audit logs regularly
- [ ] Monitor compliance risk events
