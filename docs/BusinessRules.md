# Business Rules

## Pension Management System — Business Logic & Workflows

<<<<<<< HEAD
> **مرجع القواعد المالية لهذه المهمة:** [القواعد المعتمدة للاستحقاقات والتسويات والمعالجة والصرف](./pension-financial-rules.md)، المثبتة من نص صاحب المشروع بتاريخ 2026-10-30. الوصف المالي القديم في القسمين 4 و5 أدناه لا يتقدم على هذه القواعد عند الاختلاف. المرجع يميز بين القواعد المعتمدة، والأسئلة غير المحسومة، والسلوك الحالي الذي لم يُحدّث بعد؛ ولا يغيّر السياسات الأخرى خارج نطاق النص المرفق.

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
---

## 1. File Lifecycle State Machine

### 1.1 Status Values

| Status | Code | Description |
|--------|------|-------------|
| `New` | 1 | Newly created, not yet reviewed |
| `PendingReview` | 2 | Submitted, waiting for review |
| `UnderReview` | 3 | Actively being reviewed |
| `Approved` | 4 | Review passed |
| `Rejected` | 5 | Review failed |
| `Resubmitted` | 6 | Rejected file resubmitted |
| `Suspended` | 7 | Pension suspended |
| `Completed` | 8 | File processing complete |
| `Archived` | 9 | File archived |

### 1.2 Valid Transitions

```
New → PendingReview
PendingReview → UnderReview
UnderReview → Approved
UnderReview → Rejected
Rejected → Resubmitted
Resubmitted → UnderReview
Approved → Completed
Any → Suspended (emergency)
Suspended → UnderReview (reactivation)
Completed → Archived
```

### 1.3 Transition Rules

| From | To | Allowed Roles | Conditions |
|------|----|---------------|------------|
| New | PendingReview | Operator, Admin | All required docs attached |
| PendingReview | UnderReview | Reviewer, Admin | File assigned to reviewer |
| UnderReview | Approved | Reviewer, Admin | All checklist items pass |
| UnderReview | Rejected | Reviewer, Admin | Reason provided |
| Rejected | Resubmitted | Operator, Admin | Corrections made |
| Approved | Completed | Admin | Financial settlement done |
| Any | Suspended | Admin | Suspension reason required |
| Suspended | UnderReview | Admin | Reactivation approved |

---

## 2. Document Processing Rules

### 2.1 Document Owner Types

| Owner Type | Code | Description |
|------------|------|-------------|
| `Retiree` | 1 | Document belongs to retiree |
| `EntitledPerson` | 2 | Document belongs to entitled person |
| `Agent` | 3 | Document belongs to agent |
| `Guardian` | 4 | Document belongs to guardian |
| `General` | 5 | General file document |

### 2.2 Required Documents by File Stage

**For File Submission:**
- Retiree national ID copy
- Retirement decision
- Service record
- Bank account details
- Social status certificate

**For Entitled Person Addition:**
- Entitled person national ID
- Birth certificate (minors)
- Marriage certificate (wife)
- Death certificate (death claims)

**For Financial Settlement:**
- All pending documents resolved
- Ministry approval obtained
- No active suspensions

### 2.3 Document Validation Flow

```
Upload → Owner Classification → Type Validation → Content Check → Approval
```

1. **Upload**: File stored to disk, record created
2. **Owner Classification**: Linked to specific entity (retiree/EP/agent)
3. **Type Validation**: Document type matches owner type
4. **Content Check**: Manual or automated verification
5. **Approval**: Reviewer marks as validated

### 2.4 Missing Document Detection

Automatic detection compares required documents against uploaded documents:

```csharp
// Pseudo-code
var required = GetRequiredDocuments(file);
var uploaded = GetUploadedDocuments(file);
var missing = required.Except(uploaded, by document type);
```

Missing documents are tracked with:
- Deadline for submission
- Status (Pending → Obtained / Exempted)
- Auto-notifications on deadline approach

---

## 3. Entitled Person Rules

### 3.1 Eligibility Criteria

| Relationship | Eligibility | Percentage |
|-------------|-------------|------------|
| Wife | Age < 60, not remarried | 50% of pension |
| Son | Age < 25, student | 25% per son |
| Son (disabled) | Any age, disabled | 25% |
| Daughter | Unmarried, any age | 25% per daughter |
| Parent | Dependent, age > 60 | 25% each |

### 3.2 Lifecycle Events

| Event | Action | Required Documents |
|-------|--------|-------------------|
| Marriage | Add wife as EP | Marriage certificate |
| Birth | Add child as EP | Birth certificate |
| Death | Remove EP, adjust % | Death certificate |
| Marriage (daughter) | Stop entitlement | New marriage certificate |
| Age 25 (son) | Stop entitlement | Automatic |
| Disability | Extend entitlement | Medical certificate |
| Divorce | Stop wife entitlement | Divorce certificate |

### 3.3 Percentage Calculation

Total entitlement percentage across all active entitled persons cannot exceed 100% of the base pension.

```
Wife: 50%
Son 1: 25%
Son 2: 25%
Total: 100% (maximum)
```

If adding a new EP would exceed 100%, existing percentages must be adjusted or the addition blocked.

### 3.4 Minor Entitled Persons

Minors (age < 18) require:
- Guardian assignment
- Guardian approval for all transactions
- Periodic review of guardianship status
- Auto-release when reaching age 18

---

## 4. Financial Settlement Rules

### 4.1 Settlement Calculation

```
Settlement Amount = (Monthly Pension × Months Owed) + Allowances - Deductions
```

**Components:**
- **Monthly Pension**: Based on rank, years of service, and category
- **Months Owed**: Unpaid months from retirement to settlement date
- **Allowances**: Housing, transport, special duty
- **Deductions**: Loans, advances, overpayments

### 4.2 Settlement Workflow

```
1. Create Settlement Decision
   ↓
2. Import/Enter Items (Excel or manual)
   ↓
3. Link Items to Retirees
   ↓
4. Add to Batch
   ↓
5. Submit for Ministry Approval
   ↓
6. Ministry Reviews & Approves
   ↓
7. Generate Payment
```

### 4.3 SoD Enforcement on Settlements

**Creation vs Approval Separation:**
- Person who creates settlement decision CANNOT approve it
- Person who approves CANNOT be the same as creator
- System enforces via `SettlementSoDGuard`

```csharp
// SettlementSoDGuard
if (decision.CreatedById == currentUser.Id)
    throw new BusinessRuleException("Creator cannot approve own decision");
```

### 4.4 Batch Processing

Settlements are processed in batches:
- Batch groups multiple decisions
- Batch submitted to ministry as one package
- Partial batch processing allowed
- Batch status: Draft → Submitted → Approved → Disbursed

---

## 5. Salary Payment Rules

### 5.1 Monthly Payment Cycle

```
1. Admin creates SalaryPaymentBatch (Month/Year)
   ↓
2. System generates items from active pension recipients
   ↓
3. Calculate amount for each recipient
   ↓
4. Submit for approval (SoD enforced)
   ↓
5. Approve batch
   ↓
6. Export to bank/payment system
   ↓
7. Import payment results
   ↓
8. Mark items as Paid/Failed
```

### 5.2 Payment Calculation

```
Payment Amount = Base Pension × (Entitlement Percentage / 100) - Suspensions
```

**Factors:**
- Base pension from rank and service
- Entitled person's percentage
- Active suspensions reduce payment
- Dead entitled persons excluded

### 5.3 SoD on Salary Batches

```csharp
// SalaryBatchSoDGuard
if (batch.CreatedById == currentUser.Id)
    throw new BusinessRuleException("Creator cannot approve salary batch");
```

### 5.4 Batch States

```
Draft → PendingApproval → Approved → Disbursed
                              ↓
                         Rejected (back to Draft)
```

---

## 6. Suspension Rules

### 6.1 Suspension Types

| Type | Duration | Impact |
|------|----------|--------|
| Temporary | 1-12 months | Payments paused |
| Indefinite | Until lifted | Payments paused |
| Permanent | Never lifted | Payments stopped |

### 6.2 Suspension Effects

1. **Payment Pause**: No payments during suspension
2. **Entitlement Freeze**: EP percentages frozen
3. **Notification**: Retiree and EP notified
4. **Audit Trail**: Full suspension record maintained

### 6.3 Exemption Rules

Exemptions allow continued payments during suspension:
- Medical emergency
- Court order
- Administrative error
- Special circumstances (board approval)

---

## 7. Document Pledge Rules

### 7.1 Pledge Lifecycle

```
Create Pledge → Active → Expiry Check
                          ↓
                    Auto-Release (if expired)
                          ↓
                    Mark as Released
```

### 7.2 Expiry Handling

- Background job (`ExpiredPledgesJob`) runs daily
- Checks all active pledges against expiry date
- Auto-releases expired pledges
- Sends notification on release
- Logs release action for audit

### 7.3 Manual Fulfillment

- Pledge can be fulfilled before expiry
- Fulfillment requires document submission
- System validates document type matches pledge

---

## 8. Branch Scoping Rules

### 8.1 Data Isolation

Each user sees only data from their assigned branch:

```csharp
// Branch-scoped query
var files = context.RetirementFiles
    .Where(f => f.BranchId == currentUser.BranchId)
    .ToList();
```

### 8.2 Permission Tiers

| Tier | Scope | Access |
|------|-------|--------|
| Branch | Own branch only | Operator, Reviewer |
| Regional | Multiple branches | Regional Manager |
| National | All branches | Admin |

### 8.3 Cross-Branch Operations

- Admin can view/modify any branch's data
- Regional manager can view regional branches
- Regular users cannot access other branches
- All cross-branch access logged for audit

---

## 9. Compliance Rules

### 9.1 Compliance Engine

Continuous monitoring for:
- Files pending review > 30 days
- Missing documents approaching deadline
- Expired document pledges
- Unusual financial patterns
- SoD violations attempted
- Suspensions without end date > 12 months

### 9.2 Risk Scoring

Each file gets a risk score based on:
- Age of file (older = higher risk)
- Number of missing documents
- Pending review duration
- Financial anomalies
- Suspension history

### 9.3 Audit Chain Verification

Immutable audit trail with hash chain:
```
Entry N → Hash(N) = SHA256(Data(N) + Hash(N-1))
```

Verification ensures:
- No tampering with audit records
- Complete chain from first to last entry
- Any break indicates potential fraud

---

## 10. Notification Rules

### 10.1 Event-Driven Notifications

| Event | Recipients | Channel |
|-------|------------|---------|
| File created | Admin, Branch Operator | In-app |
| Review requested | Assigned Reviewer | In-app, SignalR |
| Review completed | File Creator, Admin | In-app, SignalR |
| Document uploaded | File Owner | In-app |
| Missing doc deadline | File Creator | In-app |
| Payment batch ready | Finance Team | In-app |
| Suspension created | Retiree, EP | In-app |
| Pledge expired | File Creator | In-app |

### 10.2 Outbox Pattern

Notifications are processed reliably:
1. Event written to outbox table (in same transaction)
2. Background processor polls outbox
3. Sends notifications (SignalR, future: email/SMS)
4. Marks as processed
5. Retries on failure with exponential backoff

---

## 11. Report Generation Rules

### 11.1 Report Types

| Category | Reports |
|----------|---------|
| Retirement | File status, Active retirees, Suspensions |
| Financial | Salary batches, Settlements, Fiscal period |
| Documents | Missing, Expired |
| Dependents | Beneficiaries |
| Compliance | Ministry batches, Pending approvals |
| Admin | Audit log, User activity |
| Investment | Portfolio, Occupancy, Rent, Contracts, Risk, Maintenance, Budget |

### 11.2 Export Formats

- **Excel** (.xlsx): Full data with formatting
- **PDF**: Formatted report with headers/footers
- **CSV**: Raw data export

### 11.3 Report Filters

All reports support:
- Date range filters
- Branch filters
- Status filters
- Search terms
- Pagination for large datasets

---

## 12. Input Validation Rules

### 12.1 National ID

- Must be exactly 9 digits
- Must not already exist in system
- Must match valid Yemeni national ID format

### 12.2 File Number

- Auto-generated on creation
- Format: `{BranchCode}-{Year}-{SequentialNumber}`
- Example: `SA-2026-00001`

### 12.3 Financial Amounts

- Must be positive
- Maximum 2 decimal places
- Maximum value: 999,999,999.99
- Currency: Yemeni Rial (YER)

### 12.4 Dates

- Retirement date must be in the past
- Death date must be after birth date
- Settlement period must not overlap existing settlements
- Payment dates must be within fiscal period

---

## 13. Concurrency Rules

### 13.1 Optimistic Concurrency

- Entity `RowVersion` column for conflict detection
- Update checks version before saving
- Conflict returns 409 Conflict

### 13.2 Pessimistic Locking

Used for:
- Batch approval (prevents double-approve)
- Settlement processing
- Payment generation

```csharp
// Pessimistic lock example
using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
// ... process
await transaction.CommitAsync();
```
