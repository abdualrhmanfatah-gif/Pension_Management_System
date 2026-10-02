# API Endpoints Reference

## Pension Management System — REST API

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Base URL** | `http://localhost:5001` (dev) / `http://192.168.1.10:5141` (prod) |
| **Swagger** | `/swagger/v1/swagger.json` |
| **Auth** | JWT Bearer (`Authorization: Bearer <token>`) |
| **Content Type** | `application/json` (or `multipart/form-data` for file uploads) |
| **Total Controllers** | 62 |
| **Total Endpoints** | ~400 |

---

## 2. Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login, returns JWT + refresh token |
| POST | `/api/auth/refresh` | Refresh expired JWT |
| POST | `/api/auth/logout` | Invalidate token |
| GET | `/api/auth/me` | Get current user info |
| GET | `/api/auth/me/permissions` | Get current user's permissions |
| POST | `/api/auth/change-password` | Change password |

**Login Request:**
```json
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin@123"
}
```

**Login Response:**
```json
{
  "token": "eyJ...",
  "refreshToken": "...",
  "expiresAt": "2026-08-05T12:00:00Z",
  "user": {
    "id": 1,
    "username": "admin",
    "fullName": "Administrator",
    "role": "Admin",
    "branchId": 1
  }
}
```

---

## 3. Retirement Files (`/api/retirement-files`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List files (paginated, filterable) |
| GET | `/{id}` | Get file by ID |
| GET | `/by-number/{fileNumber}` | Get by file number |
| GET | `/by-retiree/{retireeId}` | Get by retiree ID |
| POST | `/` | Create new file |
| PUT | `/{id}` | Update file |
| POST | `/{id}/death` | Handle retiree death |
| DELETE | `/{id}` | Delete file |
| PUT | `/{id}/assign` | Assign employee to file |
| PUT | `/{id}/status` | Change file status |
| GET | `/statuses` | Get all file statuses |
| GET | `/{id}/allowed-statuses` | Get allowed status transitions |
| GET | `/{id}/status-transitions` | Get transition history |
| GET | `/{id}/change-log` | Get change log (paginated) |
| GET | `/for-data-entry` | Files for data entry view |
| GET | `/for-document-upload` | Files for document upload view |
| GET | `/document-upload-stats` | Document upload statistics |
| GET | `/uploader-stats` | Uploader performance stats |
| GET | `/pending-review` | Files pending review |
| GET | `/{id}/review-diff` | Get review diff |
| POST | `/{id}/review/approve` | Approve file review |
| POST | `/{id}/review/reject` | Reject file review |
| POST | `/{id}/request-review` | Request review |
| POST | `/review-items/{itemId}/approve` | Approve individual review item |
| POST | `/review-items/{itemId}/reject` | Reject individual review item |
| GET | `/report` | Get file report |
| GET | `/report/summary` | Get report summary |
| GET | `/report/export` | Export report (Excel/PDF) |

**Query Parameters (GET /):**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page |
| `search` | string | — | Search by name/number |
| `statusId` | int | — | Filter by status |
| `statusIds` | string | — | Comma-separated status IDs |
| `uploadedBy` | int | — | Filter by uploader |
| `cursor` | int | — | Cursor-based pagination |

---

## 4. Retirees (`/api/retirees`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List retirees (paginated, filterable) |
| GET | `/{id}` | Get retiree by ID |
| GET | `/national-id/{nationalId}` | Get by national ID |
| GET | `/military/{militaryNumber}` | Get by military number |
| GET | `/{id}/statement` | Get retiree statement |
| PUT | `/{id}` | Update retiree |
| DELETE | `/{id}` | Delete retiree |
| GET | `/{id}/full-data` | Get full retiree data |
| POST | `/{id}/photo` | Upload photo |

**Query Parameters (GET /):**
| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | int | Page number |
| `pageSize` | int | Items per page |
| `search` | string | Search term |
| `socialStatusId` | int | Filter by social status |
| `healthStatusId` | int | Filter by health status |
| `isAlive` | bool | Filter by alive status |
| `governorate` | string | Filter by governorate |
| `city` | string | Filter by city |
| `categoryId` | int | Filter by category |
| `hasFiles` | bool | Filter by having files |
| `retirementDateFrom` | DateTime | Filter from date |
| `retirementDateTo` | DateTime | Filter to date |

---

## 5. Entitled Persons (`/api/entitled-persons`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/{id}` | Get by ID |
| GET | `/by-file/{fileId}` | Get by file ID |
| POST | `/` | Create entitled person |
| PUT | `/{id}` | Update |
| PUT | `/{id}/stop` | Stop entitlement |
| PUT | `/{id}/reactivate` | Reactivate entitlement |
| POST | `/{id}/death` | Record death |
| POST | `/{id}/manual-activate` | Manual activation |
| DELETE | `/{id}` | Delete |

---

## 6. Documents (`/api/documents`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List documents (paginated, filterable) |
| GET | `/{id}` | Get by ID |
| GET | `/archive-stats` | Archive statistics |
| GET | `/by-file/{fileId}` | Get by file |
| GET | `/by-retiree/{retireeId}` | Get by retiree |
| POST | `/` | Upload document |
| DELETE | `/{id}` | Delete document |

**Query Parameters (GET /):**
| Parameter | Type | Description |
|-----------|------|-------------|
| `ownerType` | int | Document owner type |
| `documentTypeId` | int | Document type |
| `dateFrom` | DateTime | From date |
| `dateTo` | DateTime | To date |
| `verified` | bool | Verification status |
| `searchTerm` | string | Search term |
| `page` | int | Page number |
| `pageSize` | int | Items per page |

---

## 7. Payment Books (`/api/payment-books`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/by-file/{fileId}` | Get by file |
| GET | `/{id}` | Get by ID |
| GET | `/active/{fileId}` | Get active book |
| GET | `/check-number` | Validate book number |
| POST | `/` | Create payment book |
| PUT | `/{id}` | Update |
| DELETE | `/{id}` | Delete |
| GET | `/review` | Review query |
| GET | `/stats` | Statistics |

---

## 8. Salary Payment Batches (`/api/salary-batches`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List batches (paginated) |
| GET | `/{id}` | Get batch by ID |
| POST | `/` | Create batch |
| POST | `/{id}/generate` | Generate payment items |
| PUT | `/{id}/approve` | Approve batch (SoD enforced) |
| DELETE | `/{id}` | Delete batch |
| GET | `/{id}/items` | Get batch items |
| GET | `/by-file/{fileId}` | Get file items |
| PUT | `/items/{itemId}/status` | Update item status |
| GET | `/{id}/export` | Export to Excel |
| POST | `/{id}/import` | Import results |

---

## 9. Settlements (`/api/settlements`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/decisions` | List decisions (paginated) |
| GET | `/decisions/{id}` | Get decision |
| GET | `/decisions/{id}/items` | Get decision items |
| POST | `/decisions/upload` | Create decision with Excel |
| DELETE | `/decisions/{id}` | Delete decision |
| GET | `/batches` | List batches |
| GET | `/batches/{id}` | Get batch |
| POST | `/batches` | Create batch |
| POST | `/batches/{id}/import` | Import Excel to batch |
| GET | `/batches/{id}/items` | Get batch items |
| DELETE | `/batches/{id}` | Delete batch |
| GET | `/batches/{id}/decisions` | Get batch decisions |
| POST | `/batches/{id}/decisions` | Create decision in batch |
| PUT | `/items/link` | Link item to retiree |
| GET | `/by-retiree/{retireeId}` | Get by retiree |
| GET | `/decisions/{id}/export` | Export decision |
| POST | `/decisions/{id}/export-failures` | Export import failures |

---

## 10. Financial Entitlements (`/api/financial-entitlements`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/by-retiree/{retireeId}` | Get by retiree |
| GET | `/active/{retireeId}` | Get active entitlements |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create entitlement |
| PUT | `/{id}/end` | End entitlement |

---

## 11. Pension Suspensions (`/api/suspensions`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Create suspension |
| PUT | `/{suspensionId}/end` | End suspension |
| GET | `/by-file/{fileId}` | Get by file |
| GET | `/` | List all (paginated) |
| POST | `/exemptions` | Create exemption |
| GET | `/exemptions/by-file/{fileId}` | Get exemptions by file |

---

## 12. Missing Documents (`/api/missing-documents`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/by-file/{fileId}` | Get by file |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create missing doc record |
| PUT | `/{id}/fulfill` | Mark as fulfilled |
| DELETE | `/{id}` | Delete |
| GET | `/auto-detected/by-file/{fileId}` | Auto-detected missing docs |

---

## 13. Guardians (`/api/guardians`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/{id}` | Get by ID |
| GET | `/by-entitled/{entitledId}` | Get by entitled person |
| GET | `/by-file/{fileId}` | Get by file |
| POST | `/` | Assign guardian |
| PUT | `/{id}/add-minor` | Add minor to guardian |
| PUT | `/{id}/remove-minor/{entitledId}` | Remove minor |
| PUT | `/{id}` | Update guardian |
| PUT | `/{id}/end` | End guardianship |

---

## 14. Agents (`/api/agents`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/search` | Search agents |
| GET | `/by-file/{fileId}` | Get by file |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create agent |
| PUT | `/{id}` | Update |
| PUT | `/{id}/deactivate` | Deactivate |
| DELETE | `/{id}` | Delete |

---

## 15. Ministry Approvals (`/api/ministry-approvals`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/batches` | List batches |
| GET | `/batches/{id}` | Get batch |
| POST | `/batches` | Create batch |
| POST | `/batches/{id}/import` | Import Excel |
| GET | `/batches/{id}/items` | Get items (paginated) |
| GET | `/items/{itemId}` | Get item |
| DELETE | `/batches/{id}` | Delete batch |
| POST | `/batches/{id}/items` | Add manual item |
| POST | `/items/{itemId}/process` | Process item |
| GET | `/analytics/dashboard` | Dashboard stats |
| GET | `/search` | Global search |
| GET | `/batches/{id}/reconciliation` | Reconciliation matrix |
| POST | `/bulk-process` | Bulk process |
| GET | `/{id}/audit-trail` | Audit trail |
| GET | `/{id}/export` | Export report |

---

## 16. Accounting (`/api/accounting`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/accounts` | List accounts |
| GET | `/accounts/{id}` | Get account |
| GET | `/accounts/by-code/{code}` | Get by code |
| GET | `/accounts/tree` | Account tree |
| GET | `/accounts/postable` | Postable accounts |
| POST | `/accounts` | Create account |
| PUT | `/accounts/{id}` | Update account |
| DELETE | `/accounts/{id}` | Deactivate |
| PUT | `/accounts/{id}/activate` | Reactivate |
| POST | `/accounts/reset` | Reset chart |
| POST | `/accounts/disable-unused` | Disable unused |
| GET | `/cost-centers` | List cost centers |
| POST | `/cost-centers` | Create cost center |
| PUT | `/cost-centers/{id}` | Update |
| DELETE | `/cost-centers/{id}` | Delete |
| GET | `/journal-entries` | List journal entries |
| POST | `/journal-entries` | Create journal entry |
| POST | `/journal-entries/{id}/post` | Post entry |
| POST | `/journal-entries/{id}/reverse` | Reverse entry |
| GET | `/reports/trial-balance` | Trial balance |
| GET | `/reports/balance-sheet` | Balance sheet |
| GET | `/reports/income-statement` | Income statement |
| GET | `/reports/general-ledger` | General ledger |
| GET | `/reports/account-statement` | Account statement |
| GET | `/reports/cash-flow` | Cash flow |
| GET | `/fiscal-years` | List fiscal years |
| POST | `/fiscal-years` | Create fiscal year |
| GET | `/periods` | List periods |
| POST | `/close-quarter` | Close quarter |
| POST | `/close-year` | Close fiscal year |

---

## 17. Dashboard (`/api/dashboard`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/stats` | Dashboard statistics |
| GET | `/pension-statistics` | Pension statistics |

---

## 18. Reports (`/api/reports`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/retirees` | Export retirees |
| GET | `/retirement-files` | Export files |
| GET | `/entitled-persons` | Export entitled persons |
| GET | `/users` | Export users |
| GET | `/investment/*` | Investment reports (8 types) |
| GET | `/retirement/*` | Retirement reports (3 types) |
| GET | `/financial/*` | Financial reports (3 types) |
| GET | `/documents/*` | Document reports (2 types) |
| GET | `/dependents/*` | Dependent reports |
| GET | `/compliance/*` | Compliance reports |
| GET | `/admin/*` | Admin reports |

All report endpoints support `?format=excel|pdf|csv` for export.

---

## 19. Lookups (`/api/lookups`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/ranks` | All ranks |
| GET | `/banks` | All banks |
| GET | `/relationships` | All relationships |
| GET | `/document-types` | All document types |
| GET | `/file-statuses` | All file statuses |
| GET | `/social-statuses` | All social statuses |
| GET | `/health-statuses` | All health statuses |
| GET | `/retiree-categories` | All categories |
| GET | `/branches` | All branches |
| GET | `/roles` | All roles |
| GET | `/transaction-statuses` | Transaction statuses |
| GET | `/governorates` | All governorates |

---

## 20. Notifications (`/api/notifications`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List notifications (paginated) |
| GET | `/unread-count` | Get unread count |
| PUT | `/{id}/read` | Mark as read |
| PUT | `/read-all` | Mark all as read |

---

## 21. Admin Endpoints

### Users (`/api/users`)
Full CRUD + role assignment + branch assignment.

### Audit Logs (`/api/admin/audit-logs`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List audit logs (paginated) |
| GET | `/immutable` | Immutable audit trail |
| GET | `/actions` | Distinct actions |
| GET | `/tables` | Distinct tables |
| GET | `/by-record/{tableId}/{recordId}` | Get by record |

### Login History (`/api/admin/login-history`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List login history |
| GET | `/by-ip/{ip}` | Get by IP |

### Notifications (`/api/admin/notifications`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List all |
| GET | `/stats` | Statistics |
| POST | `/send` | Send notification |
| POST | `/broadcast` | Broadcast to all |
| POST | `/send-multi` | Send with attachments |
| GET | `/outbox` | Outbox queue |
| POST | `/outbox/{id}/retry` | Retry failed |

### Settings (`/api/admin/settings`)
Full CRUD for system settings.

---

## 22. Compliance (`/api/compliance`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/dashboard` | Compliance dashboard |
| GET | `/risk-events` | Risk events |
| GET | `/overrides/pending` | Pending overrides |
| POST | `/risk-events/{id}/resolve` | Resolve risk event |
| GET | `/audit/verify-chain` | Verify audit chain integrity |

---

## 23. Investment (`/api/investment/*`)

| Module | Prefix | Endpoints |
|--------|--------|-----------|
| Portfolios | `/api/investment/portfolios` | 7 |
| Projects | `/api/investment/projects` | 9 |
| Properties | `/api/investment/properties` | 14 |
| Leasing | `/api/investment/leasing` | 12 |
| Maintenance | `/api/investment/maintenance` | 8 |
| Governance | `/api/investment/governance` | 7 |
| Documents | `/api/investment/documents` | 2 |
| Lookups | `/api/investment/lookups` | 3 |

---

## 24. HR Module (`/api/hr/*`)

| Module | Prefix | Endpoints |
|--------|--------|-----------|
| Employees | `/api/hr/HrEmployees` | 7 |
| Attendance | `/api/hr/HrAttendance` | 4 |
| Leave Requests | `/api/hr/HrLeaveRequests` | 5 |
| Service History | `/api/hr/HrServiceHistory` | 5 |
| Admin Orders | `/api/hr/HrAdministrativeOrders` | 4 |
| Documents | `/api/hr/HrDocuments` | 3 |
| Reference Data | `/api/hr/HrReferenceData` | 16 |

---

## 25. Other Controllers

| Controller | Prefix | Endpoints |
|------------|--------|-----------|
| Branches | `/api/branches` | 6 |
| Banks | `/api/banks` | 6 |
| Ranks | `/api/ranks` | 5 |
| Document Types | `/api/document-types` | 6 |
| File Notes | `/api/file-notes` | 3 |
| File Statuses | `/api/file-statuses` | 1 |
| Document Pledges | `/api/document-pledges` | 6 |
| Document Exemptions | `/api/document-exemptions` | 5 |
| Achievement Obstacles | `/api/achievement-obstacles` | 9 |
| Discrepancy | `/api/discrepancy` | 4 |
| Correspondence | `/api/correspondence` | 10 |
| Fixed Assets | `/api/fixedassets` | 13 |
| Inventory Items | `/api/inventoryitems` | 5 |
| Inventory Transactions | `/api/inventorytransactions` | 6 |
| Inventory Custody | `/api/inventorycustody` | 7 |
| Budget | `/api/budget` | 10 |
| Stocktake | `/api/stocktake` | 14 |
| Bank Transactions | `/api/banktransactions` | 3 |
| Journal Entry Types | `/api/journal-entry-types` | 6 |

---

## 26. Error Responses

All endpoints return consistent error responses:

```json
{
  "success": false,
  "message": "Human-readable error message",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

| HTTP Code | Meaning |
|-----------|---------|
| 400 | Business rule violation |
| 401 | Unauthorized (missing/invalid token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Resource not found |
| 409 | Conflict (duplicate, state conflict) |
| 422 | Validation error |
| 500 | Internal server error |

---

## 27. Pagination

All list endpoints support pagination:

```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```
