import { z } from "zod";

// ═════════════════════════════════════════════
// ENUMS / CONSTANTS (mirrored from backend)
// ═════════════════════════════════════════════

export const ACCOUNT_NATURE = {
  DEBIT: "Debit",
  CREDIT: "Credit",
} as const;
export type AccountNature = (typeof ACCOUNT_NATURE)[keyof typeof ACCOUNT_NATURE];

export const JOURNAL_STATUS = {
  DRAFT: "Draft",
  POSTED: "Posted",
  REVERSED: "Reversed",
} as const;
export type JournalStatus = (typeof JOURNAL_STATUS)[keyof typeof JOURNAL_STATUS];

export const SOURCE_MODULE = {
  MANUAL: "Manual",
  SALARY: "Salary",
  SETTLEMENT: "Settlement",
} as const;
export type SourceModule = (typeof SOURCE_MODULE)[keyof typeof SOURCE_MODULE];

export const PERIOD_STATUS = {
  OPEN: "Open",
  CLOSED: "Closed",
} as const;

export const ACCOUNT_CATEGORY = {
  ASSET: "Asset",
  LIABILITY: "Liability",
  EQUITY: "Equity",
  REVENUE: "Revenue",
  EXPENSE: "Expense",
} as const;
export type AccountCategory = (typeof ACCOUNT_CATEGORY)[keyof typeof ACCOUNT_CATEGORY];

export const ACCOUNT_CATEGORY_AR: Record<AccountCategory, string> = {
  Asset: "أصول",
  Liability: "مطلوبات",
  Equity: "حقوق ملكية",
  Revenue: "إيرادات",
  Expense: "مصروفات",
};
export type PeriodStatus = (typeof PERIOD_STATUS)[keyof typeof PERIOD_STATUS];

// ═════════════════════════════════════════════
// CHART OF ACCOUNTS
// ═════════════════════════════════════════════

export interface ChartOfAccountDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountNature: AccountNature;
  accountLevel: number;
  parentAccountID: number | null;
  parentAccountName: string | null;
  isPostable: boolean;
  isActive: boolean;
  accountCategory: AccountCategory;
  description: string | null;
  subAccountsCount: number;
  hasChildren: boolean;
  createdAt: string;
}

export interface AccountTreeNode extends ChartOfAccountDto {
  children: AccountTreeNode[];
}

export interface CreateChartOfAccountDto {
  accountCode: string;
  accountName: string;
  accountNature: AccountNature;
  parentAccountID: number | null;
  isPostable: boolean;
  accountCategory: AccountCategory;
  description: string | null;
}

export interface UpdateChartOfAccountDto extends CreateChartOfAccountDto {
  isActive: boolean;
}

export const createAccountSchema = z.object({
  accountCode: z.string().min(1, "كود الحساب مطلوب"),
  accountName: z.string().min(1, "اسم الحساب مطلوب"),
  accountNature: z.enum(["Debit", "Credit"]),
  parentAccountID: z.number().nullable().optional(),
  isPostable: z.boolean().default(false),
  description: z.string().nullable().optional(),
});

export type CreateAccountForm = z.infer<typeof createAccountSchema>;

// ═════════════════════════════════════════════
// COST CENTERS
// ═════════════════════════════════════════════

export interface CostCenterDto {
  costCenterID: number;
  code: string;
  name: string;
  parentCostCenterID: number | null;
  parentCostCenterName: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCostCenterDto {
  code: string;
  name: string;
  parentCostCenterID: number | null;
}

export interface UpdateCostCenterDto extends CreateCostCenterDto {
  isActive: boolean;
}

export const createCostCenterSchema = z.object({
  code: z.string().min(1, "كود مركز التكلفة مطلوب"),
  name: z.string().min(1, "اسم مركز التكلفة مطلوب"),
  parentCostCenterID: z.number().nullable().optional(),
});

export type CreateCostCenterForm = z.infer<typeof createCostCenterSchema>;

// ═════════════════════════════════════════════
// FISCAL YEARS & PERIODS
// ═════════════════════════════════════════════

export interface FiscalYearSummaryDto {
  fiscalYearID: number;
  yearName: string;
  startDate: string;
  endDate: string;
  totalBudget: number;
  isClosed: boolean;
}

export interface CreateFiscalYearDto {
  yearName: string;
  startDate: string;
  endDate: string;
  totalBudget: number;
}

export interface FiscalPeriodDto {
  periodID: number;
  fiscalYearID: number;
  fiscalYearName: string | null;
  quarterID: number | null;
  quarterName: string | null;
  periodName: string;
  periodNumber: number;
  startDate: string;
  endDate: string;
  isClosed: boolean;
  closedByUserName: string | null;
  closedAt: string | null;
  createdAt: string;
}

// ═════════════════════════════════════════════
// QUARTERS (الفصول المالية)
// ═════════════════════════════════════════════

export interface QuarterDto {
  quarterID: number;
  fiscalYearID: number;
  quarterNumber: number;
  quarterName: string;
  startDate: string;
  endDate: string;
  isClosed: boolean;
  periods: FiscalPeriodDto[];
  totalRevenue: number;
  totalExpenses: number;
  netResult: number;
}

export interface CloseQuarterRequestDto {
  quarterID: number;
}

// ═════════════════════════════════════════════
// QUARTER DETAILS
// ═════════════════════════════════════════════

export interface PeriodBreakdownDto {
  periodID: number;
  periodName: string;
  periodNumber: number;
  openingBalance: number;
  withdrawn: number;
  spent: number;
  closingBalance: number;
}

export interface ExpenseBreakdownDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  amount: number;
  entryCount: number;
}

export interface QuarterDetailsDto {
  quarterID: number;
  fiscalYearID: number;
  quarterNumber: number;
  quarterName: string;
  startDate: string;
  endDate: string;
  isClosed: boolean;
  totalWithdrawn: number;
  totalSpent: number;
  remainingBalance: number;
  periods: PeriodBreakdownDto[];
  expenseBreakdown: ExpenseBreakdownDto[];
}

// ═════════════════════════════════════════════
// JOURNAL ENTRY TYPES (أنواع القيود)
// ═════════════════════════════════════════════

export interface JournalEntryTypeDto {
  journalEntryTypeID: number;
  typeName: string;
  typeNameEn: string;
  code: string | null;
  color: string | null;
  description: string | null;
  isActive: boolean;
  createdAt: string;
}

// ═════════════════════════════════════════════
// JOURNAL ENTRIES
// ═════════════════════════════════════════════

export interface JournalEntryLineDto {
  lineID: number;
  journalEntryID: number;
  lineNumber: number;
  accountID: number;
  accountCode: string | null;
  accountName: string | null;
  costCenterID: number | null;
  costCenterName: string | null;
  debit: number;
  credit: number;
  description: string | null;
  checkNo: string | null;
  checkDueDate: string | null;
  isReconciled: boolean;
  reconciledAt: string | null;
  reconciledByUserName: string | null;
}

export interface JournalEntryDto {
  journalEntryID: number;
  entryNumber: string;
  entryDate: string;
  periodID: number;
  periodName: string | null;
  sourceModule: string;
  sourceReferenceID: number | null;
  description: string;
  totalDebit: number;
  totalCredit: number;
  status: JournalStatus;
  postedByUserName: string | null;
  postedAt: string | null;
  reversedEntryID: number | null;
  createdByUserName: string | null;
  createdAt: string;
  lines: JournalEntryLineDto[];
}

export interface JournalEntryFilterDto {
  fromDate?: string;
  toDate?: string;
  sourceModule?: string;
  status?: string;
  periodID?: number;
  accountID?: number;
  page?: number;
  pageSize?: number;
}

export interface CreateJournalEntryLineDto {
  accountID: number;
  costCenterID?: number | null;
  debit: number;
  credit: number;
  description?: string | null;
  checkNo?: string | null;
  checkDueDate?: string | null;
}

export interface CreateJournalEntryDto {
  entryDate: string;
  periodID: number;
  journalEntryTypeID: number;
  description: string;
  lines: CreateJournalEntryLineDto[];
}

export const createJournalEntrySchema = z.object({
  entryDate: z.string().min(1, "تاريخ القيد مطلوب"),
  periodID: z.number().min(1, "الفترة المحاسبية مطلوبة"),
  description: z.string().min(1, "بيان القيد مطلوب"),
  lines: z.array(z.object({
    accountID: z.number().min(1, "الحساب مطلوب"),
    costCenterID: z.number().nullable().optional(),
    debit: z.number().min(0, "لا يمكن أن يكون سالباً"),
    credit: z.number().min(0, "لا يمكن أن يكون سالباً"),
    description: z.string().nullable().optional(),
  })).min(2, "يجب إضافة بندين على الأقل")
    .refine((lines) => {
      const totalDebit = lines.reduce((s, l) => s + l.debit, 0);
      const totalCredit = lines.reduce((s, l) => s + l.credit, 0);
      return Math.abs(totalDebit - totalCredit) < 0.01;
    }, "يجب أن يتساوى إجمالي المدين مع إجمالي الدائن"),
});

export type CreateJournalEntryForm = z.infer<typeof createJournalEntrySchema>;

// ═════════════════════════════════════════════
// ACCOUNT BALANCES
// ═════════════════════════════════════════════

export interface AccountBalanceDto {
  balanceID: number;
  accountID: number;
  accountCode: string;
  accountName: string;
  accountNature: string;
  periodID: number;
  periodName: string | null;
  openingDebit: number;
  openingCredit: number;
  periodDebit: number;
  periodCredit: number;
  closingDebit: number;
  closingCredit: number;
  netBalance: number;
}

// ═════════════════════════════════════════════
// REPORT DTOS
// ═════════════════════════════════════════════

export interface TrialBalanceItemDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountNature: string;
  accountLevel: number;
  parentAccountID: number | null;
  isPostable: boolean;
  // الأرصدة الافتتاحية
  openingDebit: number;
  openingCredit: number;
  // حركة الفترة
  movementDebit: number;
  movementCredit: number;
  // الأرصدة الختامية
  closingDebit: number;
  closingCredit: number;
  // المجاميع الإجمالية
  totalDebit: number;
  totalCredit: number;
  balance: number;
  balanceType: string; // "Debit" أو "Credit"
}

export interface AccountStatementItemDto {
  entryDate: string;
  entryNumber: string;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

export interface AccountStatementResultDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountNature: string;
  fromDate: string;
  toDate: string;
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  closingBalance: number;
  items: AccountStatementItemDto[];
}

export interface BalanceSheetSectionDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountLevel: number;
  isPostable: boolean;
  balance: number;
  children: BalanceSheetSectionDto[];
}

export interface BalanceSheetDto {
  asOfDate: string;
  assets: BalanceSheetSectionDto[];
  liabilities: BalanceSheetSectionDto[];
  equity: BalanceSheetSectionDto[];
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
  isBalanced: boolean;
}

export interface IncomeStatementLineDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountLevel: number;
  amount: number;
  children: IncomeStatementLineDto[];
}

export interface IncomeStatementDto {
  fromDate: string;
  toDate: string;
  revenues: IncomeStatementLineDto[];
  expenses: IncomeStatementLineDto[];
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;
}

export interface GeneralLedgerLineDto {
  entryDate: string;
  entryNumber: string;
  description: string;
  costCenterName: string | null;
  debit: number;
  credit: number;
  runningBalance: number;
  accountCode: string | null;
  accountName: string | null;
}

export interface GeneralLedgerDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountNature: string;
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  closingBalance: number;
  entries: GeneralLedgerLineDto[];
}

// ═════════════════════════════════════════════
// SUBLEDGER REPORT (تقرير الأستاذ المساعد)
// ═════════════════════════════════════════════

export interface SubledgerReportLineDto {
  entryDate: string;
  entryNumber: string;
  accountCode: string | null;
  accountName: string | null;
  description: string | null;
  debit: number;
  credit: number;
  runningBalance: number;
}

export interface SubledgerReportResultDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  closingBalance: number;
  entries: SubledgerReportLineDto[];
}

export interface JournalRegisterItemDto {
  journalEntryID: number;
  entryNumber: string;
  entryDate: string;
  description: string;
  sourceModule: string;
  status: number;
  createdByUserName: string;
  totalDebit: number;
  totalCredit: number;
  lines: JournalEntryLineDto[];
}

export interface JournalRegisterDto {
  fromDate: string;
  toDate: string;
  totalEntries: number;
  grandTotalDebit: number;
  grandTotalCredit: number;
  entries: JournalRegisterItemDto[];
}

// ═════════════════════════════════════════════
// DASHBOARD
// ═════════════════════════════════════════════

export interface MonthlyFinancialSummaryDto {
  month: string;
  revenue: number;
  expenses: number;
  netIncome: number;
}

export interface RecentJournalEntryDto {
  journalEntryID: number;
  entryNumber: string;
  entryDate: string;
  description: string;
  journalEntryTypeID: number;
  journalEntryTypeName: string | null;
  status: string;
  totalDebit: number;
}

export interface CostCenterSummaryDto {
  costCenterName: string;
  totalDebit: number;
  totalCredit: number;
}

export interface AccountingDashboardDto {
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;
  totalJournalEntries: number;
  draftEntries: number;
  postedEntries: number;
  reversedEntries: number;
  openPeriods: number;
  closedPeriods: number;
  monthlySummary: MonthlyFinancialSummaryDto[];
  recentEntries: RecentJournalEntryDto[];
  costCenterBreakdown: CostCenterSummaryDto[];
}

// ═════════════════════════════════════════════
// CLOSING & OPENING BALANCES
// ═════════════════════════════════════════════

export interface YearEndCloseDto {
  fiscalYearID: number;
  targetAccountID: number;
}

export interface OpeningBalanceDto {
  newFiscalYearID: number;
  sourceFiscalYearID: number;
}

export interface ClosingResultDto {
  journalEntryID: number;
  entryNumber: string;
  totalRevenuesClosed: number;
  totalExpensesClosed: number;
  netResult: number;
  resultType: string;
}

export interface OpeningBalanceResultDto {
  journalEntryID: number;
  entryNumber: string;
  accountsCarriedForward: number;
  totalDebits: number;
  totalCredits: number;
}

export interface SeedInitialDefaultsResultDto {
  fiscalYearID: number;
  periodID: number;
  balanceCount: number;
  assetCount: number;
  centralBankBalance: number;
  assets: SeedInitialAssetDto[];
  balances: SeedInitialBalanceLineDto[];
}

export interface SeedInitialAssetDto {
  assetNumber: string;
  assetName: string;
  accountID: number;
  accountCode: string;
  accountName: string;
  acquiredCost: number;
}

export interface SeedInitialBalanceLineDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountNature: string;
  balance: number;
}

// ═══════════════════════════════════════════════
// INITIAL OPENING BALANCES (الأرصدة الافتتاحية الأولية)
// ═══════════════════════════════════════════════

export interface InitialOpeningBalanceLineDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  accountNature: string;
  balance: number;
}

export interface InitialOpeningBalanceRequestDto {
  fiscalYearID: number;
  asOfDate: string;
  description: string;
  lines: InitialOpeningBalanceLineDto[];
  assets?: OpeningBalanceAssetDto[];
}

export interface OpeningBalanceAssetDto {
  assetName: string;
  assetNumber?: string;
  acquiredCost: number;
  assetAccountID: number;
  categoryID: number;
  assetTypeID: number;
  usefulLifeMonths: number;
  salvageValue: number;
  quantity: number;
  purchaseDate: string;
  inServiceDate: string;
}

export interface InitialOpeningBalanceResponseDto {
  fiscalYearID: number;
  yearName: string;
  asOfDate: string;
  description?: string;
  journalEntryID: number;
  entryNumber?: string;
  lines: InitialOpeningBalanceLineDto[];
}

// ═══════════════════════════════════════
// CASH FLOW STATEMENT
// ═══════════════════════════════════════

export interface CashFlowItemDto {
  accountID: number;
  accountCode: string;
  accountName: string;
  amount: number;
  type: string;
}

export interface CashFlowStatementDto {
  fiscalYearID: number;
  yearName: string;
  fromDate: string;
  toDate: string;
  operatingCashInflow: number;
  operatingCashOutflow: number;
  netOperatingCashFlow: number;
  investingCashInflow: number;
  investingCashOutflow: number;
  netInvestingCashFlow: number;
  financingCashInflow: number;
  financingCashOutflow: number;
  netFinancingCashFlow: number;
  netCashFlow: number;
  beginningCashBalance: number;
  endingCashBalance: number;
  operatingItems: CashFlowItemDto[];
  investingItems: CashFlowItemDto[];
  financingItems: CashFlowItemDto[];
}

// ═══════════════════════════════════════════════
// BANK TRANSACTIONS (حركات البنك)
// ═══════════════════════════════════════════════

export const BANK_TRANSACTION_TYPE = {
  DEPOSIT: "Deposit",
  WITHDRAWAL: "Withdrawal",
  TRANSFER: "Transfer",
  FEE: "Fee",
} as const;
export type BankTransactionType =
  (typeof BANK_TRANSACTION_TYPE)[keyof typeof BANK_TRANSACTION_TYPE];

export const BANK_TRANSACTION_TYPE_AR: Record<BankTransactionType, string> = {
  Deposit: "إيداع",
  Withdrawal: "سحب",
  Transfer: "تحويل",
  Fee: "رسوم بنكية",
};

export interface BankTransactionDto {
  transactionID: number;
  bankID: number;
  bankName: string | null;
  journalEntryID: number;
  entryNumber: string | null;
  lineID: number;
  lineNumber: number;
  transactionDate: string;
  transactionType: BankTransactionType;
  amount: number;
  description: string | null;
  referenceNo: string | null;
  isReconciled: boolean;
  reconciledAt: string | null;
  bankStatementRef: string | null;
  createdAt: string;
}

export interface BankTransactionFilterDto {
  bankID?: number;
  fromDate?: string;
  toDate?: string;
  isReconciled?: boolean;
}

// ═══════════════════════════════════════════════
// REPORT DTOS — Financial Reports (التقارير المالية)
// ═══════════════════════════════════════════════

/** Quarterly report — التقرير الفصلي */
export interface QuarterlyReportQuarterDto {
  quarterNumber: number;
  totalAllocated: number;
  totalSpent: number;
  remainingBalance: number;
}

export interface QuarterlyReportDto {
  quarters: QuarterlyReportQuarterDto[];
  totalAllocated: number;
  totalSpent: number;
  totalRemaining: number;
}

/** Budget report — تقرير الموازنة */
export interface BudgetReportItemDto {
  accountCode: string;
  accountName: string;
  budgetAmount: number;
  actualAmount: number;
  variance: number;
  variancePercent?: number;
}

export interface BudgetReportResultDto {
  items: BudgetReportItemDto[];
  totalBudget: number;
  totalActual: number;
  totalVariance: number;
}

/** Current operations — العمليات الجارية */
export interface CurrentOperationsItemDto {
  accountCode: string;
  accountName: string;
  openingBalance: number;
  debit: number;
  credit: number;
  closingBalance: number;
}

export interface CurrentOperationsReportDto {
  items: CurrentOperationsItemDto[];
  totalOpening: number;
  totalDebit: number;
  totalCredit: number;
  totalClosing: number;
}

/** Liabilities — الديون والالتزامات */
export interface LiabilitiesItemDto {
  accountCode: string;
  accountName: string;
  dueDate?: string;
  amount: number;
  status?: string;
}

export interface LiabilitiesReportDto {
  items: LiabilitiesItemDto[];
  totalAmount: number;
}

/** Year-over-year comparison — المقارنة السنوية */
export interface YearOverYearItemDto {
  accountName: string;
  year1Amount: number;
  year2Amount: number;
  change: number;
  changePercent?: number;
}

export interface YearOverYearReportDto {
  items: YearOverYearItemDto[];
  totalYear1: number;
  totalYear2: number;
  totalChange: number;
}

/** Cumulative P&L — الأرباح والخسائر المتراكم */
export interface CumulativePnLItemDto {
  accountCode: string;
  accountName: string;
  amount: number;
}

export interface CumulativePnLReportDto {
  items: CumulativePnLItemDto[];
  netIncome: number;
}

/** Cost centers report — مراكز التكلفة */
export interface CostCenterReportItemDto {
  costCenterCode: string;
  costCenterName: string;
  budget: number;
  actual: number;
  variance: number;
}

export interface CostCenterReportDto {
  items: CostCenterReportItemDto[];
  totalBudget: number;
  totalActual: number;
  totalVariance: number;
}

/** Provisions report — المخصصات والاحتياطيات */
export interface ProvisionsItemDto {
  accountCode: string;
  accountName: string;
  openingBalance: number;
  additions: number;
  utilizations: number;
  closingBalance: number;
}

export interface ProvisionsReportDto {
  items: ProvisionsItemDto[];
  totalOpening: number;
  totalAdditions: number;
  totalUtilizations: number;
  totalClosing: number;
}

/** Depreciation report — تقرير الإهلاك */
export interface DepreciationReportItemDto {
  assetNumber: string;
  assetName: string;
  acquiredCost: number;
  depreciationRate: number;
  accumulatedDepreciation: number;
  currentDepreciation: number;
  netBookValue: number;
}

export interface DepreciationReportDto {
  items: DepreciationReportItemDto[];
  totalCost: number;
  totalAccumulated: number;
  totalCurrent: number;
  totalNetBookValue: number;
}

/** Balance check result */
export interface BalanceCheckResult {
  isBalanced: boolean;
  difference: number;
  debitTotal?: number;
  creditTotal?: number;
}
