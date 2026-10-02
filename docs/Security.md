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
  "username": "admin",
  "role": "Admin",
  "branchId": 1,
  "permissions": ["*"],
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
