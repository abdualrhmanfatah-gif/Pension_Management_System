/**
 * Centralized query key registry — single source of truth for all TanStack Query keys.
 * Import from here instead of using raw string literals.
 *
 * Usage:
 *   import { queryKeys } from "@/constants/queryKeys";
 *   useQuery({ queryKey: queryKeys.retirement.files.list(filters), ... })
 *   queryClient.invalidateQueries({ queryKey: queryKeys.retirement.files.all })
 */

export const queryKeys = {
  // ─── Retirement Files ───────────────────────────────────
  retirement: {
    files: {
      all: ["retirement-files"] as const,
      list: (filters: Record<string, unknown>) => ["retirement-files", filters] as const,
      detail: (id: number) => ["retirement-files", id] as const,
    },
    fileDetail: {
      all: ["file-detail"] as const,
      byId: (id: number) => ["file-detail", id] as const,
    },
    entitledPersons: {
      all: ["entitled-persons"] as const,
      byFile: (fileId: number) => ["entitled-persons", fileId] as const,
    },
    agents: {
      all: ["agents"] as const,
      byFile: (fileId: number) => ["agents", fileId] as const,
    },
    guardians: {
      all: ["guardians"] as const,
      byFile: (fileId: number) => ["guardians", fileId] as const,
    },
    documents: {
      all: ["documents"] as const,
      byFile: (fileId: number) => ["documents", fileId] as const,
    },
    missingDocuments: {
      all: ["missing-documents"] as const,
      byFile: (fileId: number) => ["missing-documents", fileId] as const,
    },
    reviewItems: {
      all: ["review-items"] as const,
      byFile: (fileId: number) => ["review-items", fileId] as const,
    },
    reviewDiff: {
      all: ["review-diff"] as const,
      byFile: (fileId: number) => ["review-diff", fileId] as const,
    },
    statusTransitions: {
      all: ["status-transitions"] as const,
      byFile: (fileId: number) => ["status-transitions", fileId] as const,
    },
    pledges: {
      all: ["pledges"] as const,
      byFile: (fileId: number) => ["pledges", fileId] as const,
    },
    paymentBooks: {
      all: ["payment-books"] as const,
      byFile: (fileId: number) => ["payment-books", fileId] as const,
    },
    recipients: {
      all: ["recipients"] as const,
      byFile: (fileId: number) => ["recipients", fileId] as const,
    },
    history: {
      all: ["history"] as const,
      byFile: (fileId: number) => ["history", fileId] as const,
    },
    printData: {
      all: ["print-data"] as const,
      byFile: (fileId: number) => ["print-data", fileId] as const,
    },
  },

  // ─── Retirees ───────────────────────────────────────────
  retirees: {
    all: ["retirees"] as const,
    list: (filters: Record<string, unknown>) => ["retirees", filters] as const,
    detail: (id: number) => ["retirees", id] as const,
    search: (term: string) => ["retirees-search", term] as const,
  },

  // ─── Pension ────────────────────────────────────────────
  pension: {
    statistics: ["pension-statistics"] as const,
    socialResearch: {
      all: ["social-research"] as const,
      detail: (id: number) => ["social-research", id] as const,
    },
    ministryReferrals: {
      all: ["ministry-referrals"] as const,
      list: (filters: Record<string, unknown>) => ["ministry-referrals", filters] as const,
    },
    paymentCards: {
      all: ["payment-cards"] as const,
      detail: (id: number) => ["payment-cards", id] as const,
    },
    annualBookReview: {
      all: ["annual-book-review"] as const,
    },
    salaryReconciliation: {
      all: ["salary-reconciliation"] as const,
    },
  },

  // ─── Accounting ─────────────────────────────────────────
  accounting: {
    all: ["accounting"] as const,
    accounts: {
      all: ["accounts"] as const,
      postable: ["accounts", "postable"] as const,
      allList: ["accounts", "all"] as const,
    },
    journalEntries: {
      all: ["journal-entries"] as const,
      detail: (id: number) => ["journal-entries", id] as const,
    },
    journalEntryTypes: ["journal-entry-types"] as const,
    fiscalYears: ["fiscal-years"] as const,
    periods: {
      all: ["periods"] as const,
      byYear: (yearId: number) => ["periods", yearId] as const,
      active: (yearId: number) => ["periods", "active", yearId] as const,
    },
    quarters: {
      all: ["quarters"] as const,
      byYear: (yearId: number) => ["quarters", yearId] as const,
    },
    vouchers: {
      all: ["vouchers"] as const,
      detail: (id: number) => ["voucher", id] as const,
    },
    reports: {
      trialBalance: (yearId: number, periodId?: number, fromDate?: string, toDate?: string) =>
        ["reports", "trial-balance", yearId, periodId, fromDate, toDate] as const,
      balanceSheet: (toDate: string, fiscalYearId?: number) =>
        ["reports", "balance-sheet", toDate, fiscalYearId] as const,
      incomeStatement: (yearId: number, quarterId?: number) =>
        ["reports", "income-statement", yearId, quarterId] as const,
      generalLedger: (accountId: number, fromDate?: string, toDate?: string) =>
        ["reports", "general-ledger", accountId, fromDate, toDate] as const,
      subledger: (accountId: number, fromDate?: string, toDate?: string) =>
        ["reports", "subledger", accountId, fromDate, toDate] as const,
      journalRegister: (yearId: number, fromDate?: string, toDate?: string) =>
        ["reports", "journal-register", yearId, fromDate, toDate] as const,
    },
  },

  // ─── HR ─────────────────────────────────────────────────
  hr: {
    departments: ["hr", "departments"] as const,
    sections: {
      all: ["hr", "sections"] as const,
      byDepartment: (deptId: number) => ["hr", "sections", deptId] as const,
    },
    jobTitles: ["hr", "jobTitles"] as const,
    employmentStatuses: ["hr", "employmentStatuses"] as const,
    ranks: ["hr", "ranks"] as const,
    employees: {
      all: ["hr", "employees"] as const,
      minimal: ["hr", "employees-minimal"] as const,
      simple: ["hr", "employees-simple"] as const,
      listSimple: ["hr", "employees-list-simple"] as const,
      lookup: ["hr", "employees-lookup"] as const,
      detail: (id: number) => ["hr", "employee", id] as const,
    },
    leaveTypes: ["hr", "leave-types"] as const,
  },

  // ─── Salaries ───────────────────────────────────────────
  salaries: {
    batches: {
      all: ["salary-batches"] as const,
      list: (page: number, status?: number) => ["salary-batches", page, status] as const,
      detail: (id: number) => ["salary-batch", id] as const,
    },
    batchItems: {
      all: ["salary-batch-items"] as const,
      byBatch: (batchId: number, page: number) => ["salary-batch-items", batchId, page] as const,
    },
    recipients: {
      all: ["salary-recipients"] as const,
      byFile: (fileId: number) => ["salary-recipients", fileId] as const,
    },
    entitlements: {
      all: ["financial-entitlements"] as const,
      byRetiree: (retireeId: number) => ["financial-entitlements", retireeId] as const,
    },
  },

  // ─── Settlements ────────────────────────────────────────
  settlements: {
    decisions: {
      all: ["settlement-decisions"] as const,
      list: (filters: Record<string, unknown>) => ["settlement-decisions", filters] as const,
    },
    decision: {
      all: ["settlement-decision"] as const,
      detail: (id: number) => ["settlement-decision", id] as const,
    },
    decisionItems: {
      all: ["settlement-decision-items"] as const,
      byDecision: (id: number, filters: Record<string, unknown>) =>
        ["settlement-decision-items", id, filters] as const,
    },
  },

  // ─── Suspensions ────────────────────────────────────────
  suspensions: {
    all: ["all-suspensions"] as const,
    list: (filters: Record<string, unknown>) => ["all-suspensions", filters] as const,
    search: (term: string) => ["file-search-suspension", term] as const,
  },

  // ─── Correspondence ─────────────────────────────────────
  correspondence: {
    all: ["correspondence"] as const,
    list: (filters: Record<string, unknown>) => ["correspondence", "list", filters] as const,
    types: ["correspondence", "types"] as const,
    externalParties: ["correspondence", "external-parties"] as const,
  },

  // ─── Director ───────────────────────────────────────────
  director: {
    outgoing: ["director", "outgoing"] as const,
    incoming: ["director", "incoming"] as const,
  },

  // ─── Investment ─────────────────────────────────────────
  investment: {
    portfolios: ["investment-portfolios"] as const,
    projects: ["investment-projects"] as const,
    properties: ["investment-properties"] as const,
    tenants: ["investment-tenants"] as const,
    transactions: ["investment-transactions"] as const,
  },

  // ─── Inventory ──────────────────────────────────────────
  inventory: {
    items: ["inventory-items"] as const,
    categories: ["inventory-categories"] as const,
    stocktakes: ["stocktakes"] as const,
    transactions: ["inventory-transactions"] as const,
    custody: ["inventory-custody"] as const,
  },

  // ─── Fixed Assets ───────────────────────────────────────
  fixedAssets: {
    all: ["fixed-assets"] as const,
    simple: ["fixed-assets-simple"] as const,
    detail: (id: number) => ["fixed-asset", id] as const,
    types: ["fixed-asset-types"] as const,
    categories: {
      all: ["fixed-asset-categories"] as const,
      byType: (typeId: number) => ["fixed-asset-categories", typeId] as const,
    },
  },

  // ─── Documents ──────────────────────────────────────────
  documents: {
    types: ["document-types"] as const,
    typesByOwner: (ownerType: number) => ["document-types", ownerType] as const,
  },

  // ─── Lookups ────────────────────────────────────────────
  lookups: {
    branches: ["lookups", "branches"] as const,
    banks: ["lookups", "banks"] as const,
    socialStatuses: ["social-statuses"] as const,
    healthStatuses: ["health-statuses"] as const,
  },

  // ─── Admin ──────────────────────────────────────────────
  admin: {
    users: {
      all: ["users"] as const,
      list: (filters: Record<string, unknown>) => ["users", filters] as const,
      detail: (id: number) => ["users", id] as const,
      permissions: (id: number) => ["users", id, "individual-permissions"] as const,
    },
    roles: ["roles"] as const,
    rolePermissions: (roleId: number) => ["roles", roleId, "permissions"] as const,
    permissions: ["permissions"] as const,
    notifications: {
      all: ["admin-notifications"] as const,
    },
    usersAll: ["admin", "users", "all"] as const,
    adminRoles: ["admin", "roles"] as const,
  },

  // ─── Archive ────────────────────────────────────────────
  archive: {
    all: ["archive"] as const,
    reviewDocuments: ["archive", "review-documents"] as const,
  },

  // ─── Reports ────────────────────────────────────────────
  reports: {
    data: (reportId: string, filters: Record<string, unknown>) => ["report-data", reportId, filters] as const,
  },

  // ─── Discrepancy ────────────────────────────────────────
  discrepancy: {
    report: (filter: Record<string, unknown>) => ["discrepancy-report", filter] as const,
  },

  // ─── Notifications ──────────────────────────────────────
  notifications: {
    all: ["notifications"] as const,
    // userID يعزل استجابات الحسابات المختلفة في الذاكرة المؤقتة (NF2)
    byUser: (userId: number) => ["notifications", "user", userId] as const,
    list: (userId: number, page: number, pageSize: number, category?: string) =>
      ["notifications", "user", userId, "list", page, pageSize, category ?? ""] as const,
    unread: (userId: number) => ["notifications", "user", userId, "unread"] as const,
  },

  // ─── Achievements ───────────────────────────────────────
  achievements: {
    obstacles: {
      all: ["achievement-obstacles"] as const,
      detail: (id: number) => ["achievement-obstacles", id] as const,
    },
  },
} as const;
