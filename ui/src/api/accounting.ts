import type {
  AccountBalanceDto,
  AccountingDashboardDto,
  AccountStatementResultDto,
  AccountTreeNode,
  BalanceSheetDto,
  BankTransactionDto,
  BankTransactionFilterDto,
  BudgetReportResultDto,
  CashFlowStatementDto,
  ChartOfAccountDto,
  CloseQuarterRequestDto,
  ClosingResultDto,
  CostCenterDto,
  CostCenterReportDto,
  CreateChartOfAccountDto,
  CreateCostCenterDto,
  CreateFiscalYearDto,
  CreateJournalEntryDto,
  CumulativePnLReportDto,
  CurrentOperationsReportDto,
  DepreciationReportDto,
  FiscalPeriodDto,
  FiscalYearSummaryDto,
  GeneralLedgerDto,
  IncomeStatementDto,
  InitialOpeningBalanceRequestDto,
  InitialOpeningBalanceResponseDto,
  JournalEntryDto,
  JournalEntryFilterDto,
  JournalEntryTypeDto,
  JournalRegisterDto,
  LiabilitiesReportDto,
  OpeningBalanceDto,
  OpeningBalanceResultDto,
  ProvisionsReportDto,
  QuarterDetailsDto,
  QuarterDto,
  QuarterlyReportDto,
  SeedInitialDefaultsResultDto,
  SubledgerReportResultDto,
  TrialBalanceItemDto,
  UpdateChartOfAccountDto,
  UpdateCostCenterDto,
  YearEndCloseDto,
  YearOverYearReportDto,
} from "@/types/accounting";
import type { ApiResponse } from "@/types/api";

import api from "./axios";

// ═══════════════════════════════════════════════════
// CHART OF ACCOUNTS (دليل الحسابات)
// ═══════════════════════════════════════════════════

export const accountsApi = {
  getAll: (level?: number) =>
    api.get<ApiResponse<ChartOfAccountDto[]>>("/accounting/accounts", { params: { level } })
      .then((r) => r.data.data),

  getById: (id: number) =>
    api.get<ApiResponse<ChartOfAccountDto>>(`/accounting/accounts/${id}`)
      .then((r) => r.data.data),

  getByCode: (code: string) =>
    api.get<ApiResponse<ChartOfAccountDto>>(`/accounting/accounts/by-code/${code}`)
      .then((r) => r.data.data),

  getTree: (parentId?: number) =>
    api.get<ApiResponse<AccountTreeNode[]>>("/accounting/accounts/tree", { params: { parentId } })
      .then((r) => r.data.data),

  getPostable: () =>
    api.get<ApiResponse<ChartOfAccountDto[]>>("/accounting/accounts/postable")
      .then((r) => r.data.data),

  create: (data: CreateChartOfAccountDto) =>
    api.post<ApiResponse<ChartOfAccountDto>>("/accounting/accounts", data)
      .then((r) => r.data.data),

  update: (id: number, data: UpdateChartOfAccountDto) =>
    api.put<ApiResponse<ChartOfAccountDto>>(`/accounting/accounts/${id}`, data)
      .then((r) => r.data.data),

  deactivate: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/accounting/accounts/${id}`)
      .then((r) => r.data.data),

  activate: (id: number) =>
    api.put<ApiResponse<boolean>>(`/accounting/accounts/${id}/activate`)
      .then((r) => r.data.data),

  reset: () =>
    api.post<ApiResponse<boolean>>("/accounting/accounts/reset")
      .then((r) => r.data),

  disableUnused: () =>
    api.post<ApiResponse<number>>("/accounting/accounts/disable-unused")
      .then((r) => r.data),
};

// ═══════════════════════════════════════════════════
// COST CENTERS (مراكز التكلفة)
// ═══════════════════════════════════════════════════

export const costCentersApi = {
  getAll: () =>
    api.get<ApiResponse<CostCenterDto[]>>("/accounting/cost-centers")
      .then((r) => r.data.data),

  getById: (id: number) =>
    api.get<ApiResponse<CostCenterDto>>(`/accounting/cost-centers/${id}`)
      .then((r) => r.data.data),

  create: (data: CreateCostCenterDto) =>
    api.post<ApiResponse<CostCenterDto>>("/accounting/cost-centers", data)
      .then((r) => r.data.data),

  update: (id: number, data: UpdateCostCenterDto) =>
    api.put<ApiResponse<CostCenterDto>>(`/accounting/cost-centers/${id}`, data)
      .then((r) => r.data.data),

  deactivate: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/accounting/cost-centers/${id}`)
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// FISCAL YEARS (السنوات المالية)
// ═══════════════════════════════════════════════════

export const fiscalYearsApi = {
  getAll: () =>
    api.get<ApiResponse<FiscalYearSummaryDto[]>>("/accounting/fiscal-years")
      .then((r) => r.data.data),

  create: (dto: CreateFiscalYearDto) =>
    api.post<ApiResponse<FiscalYearSummaryDto>>("/accounting/fiscal-years", dto)
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// FISCAL PERIODS (الفترات المحاسبية)
// ═══════════════════════════════════════════════════

export const periodsApi = {
  getAll: (fiscalYearId: number) =>
    api.get<ApiResponse<FiscalPeriodDto[]>>("/accounting/periods", { params: { fiscalYearId } })
      .then((r) => r.data.data),

  getActive: (fiscalYearId: number) =>
    api.get<ApiResponse<FiscalPeriodDto>>("/accounting/periods/active", { params: { fiscalYearId } })
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// QUARTERS (الفصول المالية)
// ═══════════════════════════════════════════════════

export const quartersApi = {
  getAll: (fiscalYearId: number) =>
    api.get<ApiResponse<QuarterDto[]>>("/accounting/quarters", { params: { fiscalYearId } })
      .then((r) => r.data.data),

  getDetails: (quarterId: number) =>
    api.get<ApiResponse<QuarterDetailsDto>>(`/accounting/quarters/${quarterId}/details`)
      .then((r) => r.data.data),

  close: (data: CloseQuarterRequestDto) =>
    api.post<ApiResponse<QuarterDto>>("/accounting/close-quarter", data)
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// JOURNAL ENTRY TYPES (أنواع القيود)
// ═══════════════════════════════════════════════════

export const journalEntryTypesApi = {
  getAll: () =>
    api.get<ApiResponse<JournalEntryTypeDto[]>>("/journal-entry-types")
      .then((r) => r.data.data),

  getById: (id: number) =>
    api.get<ApiResponse<JournalEntryTypeDto>>(`/journal-entry-types/${id}`)
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// JOURNAL ENTRIES (القيود اليومية)
// ═══════════════════════════════════════════════════

export const journalEntriesApi = {
  getAll: (filter: JournalEntryFilterDto) =>
    api.get<ApiResponse<JournalEntryDto[]>>("/accounting/journal-entries", { params: filter })
      .then((r) => r.data),

  getById: (id: number) =>
    api.get<ApiResponse<JournalEntryDto>>(`/accounting/journal-entries/${id}`)
      .then((r) => r.data.data),

  create: (data: CreateJournalEntryDto) =>
    api.post<ApiResponse<JournalEntryDto>>("/accounting/journal-entries", data)
      .then((r) => r.data.data),

  post: (id: number) =>
    api.post<ApiResponse<JournalEntryDto>>(`/accounting/journal-entries/${id}/post`)
      .then((r) => r.data.data),

  reverse: (id: number, reason: string) =>
    api.post<ApiResponse<JournalEntryDto>>(`/accounting/journal-entries/${id}/reverse`, { reason })
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// ACCOUNT BALANCES (الأرصدة)
// ═══════════════════════════════════════════════════

export const balancesApi = {
  get: (accountId: number, periodId: number) =>
    api.get<ApiResponse<AccountBalanceDto>>("/accounting/balances", { params: { accountId, periodId } })
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// REPORTS (التقارير المالية)
// ═══════════════════════════════════════════════════

export const reportsApi = {
  trialBalance: (params: { fiscalYearId?: number; periodId?: number; fromDate?: string; toDate?: string }) =>
    api.get<ApiResponse<TrialBalanceItemDto[]>>("/accounting/reports/trial-balance", { params })
      .then((r) => r.data.data),

  balanceSheet: (asOfDate: string, fiscalYearId?: number) =>
    api.get<ApiResponse<BalanceSheetDto>>("/accounting/reports/balance-sheet", { params: { asOfDate, fiscalYearId } })
      .then((r) => r.data.data),

  incomeStatement: (fiscalYearId: number, quarterId?: number) =>
    api.get<ApiResponse<IncomeStatementDto>>("/accounting/reports/income-statement", { params: { fiscalYearId, quarterId } })
      .then((r) => r.data.data),

  generalLedger: (params: { accountId: number; fiscalYearId?: number; periodId?: number; fromDate?: string; toDate?: string }) =>
    api.get<ApiResponse<GeneralLedgerDto>>("/accounting/reports/general-ledger", { params })
      .then((r) => r.data.data),

  accountStatement: (params: { accountId: number; fiscalYearId?: number; fromDate: string; toDate: string }) =>
    api.get<ApiResponse<AccountStatementResultDto>>("/accounting/reports/account-statement", { params })
      .then((r) => r.data.data),

  subledger: (params: { accountId: number; fromDate?: string; toDate?: string }) =>
    api.get<ApiResponse<SubledgerReportResultDto>>("/accounting/reports/subledger", { params })
      .then((r) => r.data.data),

  journalRegister: (params: { fiscalYearId?: number; fromDate: string; toDate: string }) =>
    api.get<ApiResponse<JournalRegisterDto>>("/accounting/reports/journal-register", { params })
      .then((r) => r.data.data),

  cashFlow: (fiscalYearId: number) =>
    api.get<ApiResponse<CashFlowStatementDto>>("/accounting/reports/cash-flow", { params: { fiscalYearId } })
      .then((r) => r.data.data),

  // ── Export Functions ──
  exportTrialBalance: (params: { periodId?: number; fromDate?: string; toDate?: string; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/trial-balance/export", { params, responseType: "blob" }),

  exportBalanceSheet: (params: { asOfDate: string; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/balance-sheet/export", { params, responseType: "blob" }),

  exportIncomeStatement: (params: { fiscalYearId: number; quarterId?: number; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/income-statement/export", { params, responseType: "blob" }),

  exportGeneralLedger: (params: { accountId: number; periodId?: number; fromDate?: string; toDate?: string; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/general-ledger/export", { params, responseType: "blob" }),

  exportAccountStatement: (params: { accountId: number; fromDate: string; toDate: string; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/account-statement/export", { params, responseType: "blob" }),

  exportSubledger: (params: { accountId: number; fromDate?: string; toDate?: string; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/subledger/export", { params, responseType: "blob" }),

  exportJournalRegister: (params: { fromDate: string; toDate: string; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/journal-register/export", { params, responseType: "blob" }),

  exportCashFlow: (params: { fiscalYearId: number; format?: "excel" | "pdf" }) =>
    api.get("/accounting/reports/cash-flow/export", { params, responseType: "blob" }),

  // ── التقارير المفقودة ──
  quarterlyReport: (params: { fiscalYearId?: number; quarterId?: number }) =>
    api.get<ApiResponse<QuarterlyReportDto>>("/accounting/reports/quarterly", { params })
      .then((r) => r.data.data),

  budgetReport: (params: { fiscalYearId?: number }) =>
    api.get<ApiResponse<BudgetReportResultDto>>("/accounting/reports/budget", { params })
      .then((r) => r.data.data),

  currentOperations: (params: { fiscalYearId?: number }) =>
    api.get<ApiResponse<CurrentOperationsReportDto>>("/accounting/reports/current-operations", { params })
      .then((r) => r.data.data),

  liabilitiesReport: (params: { fiscalYearId?: number }) =>
    api.get<ApiResponse<LiabilitiesReportDto>>("/accounting/reports/liabilities", { params })
      .then((r) => r.data.data),

  yearOverYear: (params: { year1Id?: number; year2Id?: number }) =>
    api.get<ApiResponse<YearOverYearReportDto>>("/accounting/reports/year-over-year", { params })
      .then((r) => r.data.data),

  cumulativePnL: (params: { fiscalYearId?: number }) =>
    api.get<ApiResponse<CumulativePnLReportDto>>("/accounting/reports/cumulative-pnl", { params })
      .then((r) => r.data.data),

  costCentersReport: (params: { fiscalYearId?: number }) =>
    api.get<ApiResponse<CostCenterReportDto>>("/accounting/reports/cost-centers", { params })
      .then((r) => r.data.data),

  provisionsReport: (params: { fiscalYearId?: number }) =>
    api.get<ApiResponse<ProvisionsReportDto>>("/accounting/reports/provisions", { params })
      .then((r) => r.data.data),

  depreciationReport: (params: { fiscalYearId?: number }) =>
    api.get<ApiResponse<DepreciationReportDto>>("/accounting/reports/depreciation", { params })
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// DASHBOARD (لوحة التحكم)
// ═══════════════════════════════════════════════════

export const accountingApi = {
  getDashboard: (fiscalYearId?: number) =>
    api.get<ApiResponse<AccountingDashboardDto>>("/accounting/dashboard", { params: { fiscalYearId } })
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// YEAR-END CLOSING (الإقفال السنوي)
// ═══════════════════════════════════════════════════

export const closingApi = {
  closeYear: (data: YearEndCloseDto) =>
    api.post<ApiResponse<ClosingResultDto>>("/accounting/close-year", data)
      .then((r) => r.data.data),

  createOpeningBalances: (data: OpeningBalanceDto) =>
    api.post<ApiResponse<OpeningBalanceResultDto>>("/accounting/opening-balances", data)
      .then((r) => r.data.data),

  createInitialOpeningBalances: (data: InitialOpeningBalanceRequestDto) =>
    api.post<ApiResponse<OpeningBalanceResultDto>>("/accounting/initial-opening-balances", data)
      .then((r) => r.data.data),

  getInitialOpeningBalances: (fiscalYearId: number) =>
    api.get<ApiResponse<InitialOpeningBalanceResponseDto>>(`/accounting/initial-opening-balances/${fiscalYearId}`)
      .then((r) => r.data.data),

  seedInitialDefaults: (fiscalYearID: number) =>
    api.post<ApiResponse<SeedInitialDefaultsResultDto>>("/accounting/initial-opening-balances/seed", { fiscalYearID })
      .then((r) => r.data.data),

  resetAll: () =>
    api.post<ApiResponse<boolean>>("/accounting/reset-all")
      .then((r) => r.data.data),
};

// ═══════════════════════════════════════════════════
// BANK TRANSACTIONS (حركات البنك)
// ═══════════════════════════════════════════════════

export const bankTransactionsApi = {
  getAll: (filter?: BankTransactionFilterDto) =>
    api.get<ApiResponse<BankTransactionDto[]>>("/accounting/bank-transactions", { params: filter })
      .then((r) => r.data.data),

  reconcile: (transactionId: number) =>
    api.post<ApiResponse<BankTransactionDto>>(`/accounting/bank-transactions/${transactionId}/reconcile`)
      .then((r) => r.data.data),
};
